using System.Collections.ObjectModel;
using Avalonia.Threading;
using ChatApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatApp.ViewModels;

public sealed partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _role = string.Empty;

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private DateTime _timestamp;
}

public sealed partial class ChatViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private readonly string _sessionId;

    [ObservableProperty]
    private string _roleName = string.Empty;

    [ObservableProperty]
    private string _modelName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ChatMessage> _messages = [];

    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _canClear;

    [ObservableProperty]
    private bool _isHistoryLoading;

    public ChatViewModel(MainWindowViewModel mainVm, string sessionId)
    {
        _mainVm = mainVm;
        _sessionId = sessionId;
        _isHistoryLoading = true;
        _ = LoadSessionAsync();
    }

    private async Task LoadSessionAsync()
    {
        var session = AppServices.ChatSessionService.GetSession(_sessionId);
        if (session == null)
        {
            _mainVm.HandleSessionNotFound(_sessionId);
            return;
        }

        RoleName = session.Title;
        ModelName = session.ModelId switch
        {
            "deepseek-v4-flash" => "V4 Flash",
            "deepseek-v4-pro" => "V4 Pro",
            "deepseek-chat" => "Chat",
            "deepseek-reasoner" => "Reasoner",
            _ => session.ModelId,
        };

        var items = session.Messages.Select(msg => new ChatMessage
        {
            Id = msg.Id,
            Role = msg.Role,
            Text = msg.Text,
            Timestamp = msg.Timestamp.DateTime,
        }).ToList();

        const int batchSize = 5;
        for (var i = 0; i < items.Count; i += batchSize)
        {
            var batch = items.Skip(i).Take(batchSize);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var item in batch)
                    Messages.Add(item);
            }, DispatcherPriority.Normal);

            if (i + batchSize < items.Count)
                await Task.Delay(16);
        }

        CanClear = Messages.Count > 0;
        IsHistoryLoading = false;
    }

    [RelayCommand]
    private async Task Send()
    {
        var text = InputText.Trim();
        if (string.IsNullOrEmpty(text) || IsLoading) return;
        var session = AppServices.ChatSessionService.GetSession(_sessionId);
        if (session == null) return;

        var userMsg = new Services.ChatMessage
        {
            Id = Guid.NewGuid().ToString("N"),
            Role = "user",
            Text = text
        };
        AppServices.ChatSessionService.AddMessage(_sessionId, userMsg);

        Messages.Add(new ChatMessage
        {
            Id = userMsg.Id,
            Role = "user",
            Text = text,
            Timestamp = DateTime.Now,
        });

        if (Messages.Count == 1)
        {
            _mainVm.UpdateSessionTitle(_sessionId,
                text.Length > 20 ? text[..20] + "..." : text);
        }

        InputText = string.Empty;
        IsLoading = true;
        CanClear = true;

        try
        {
            var context = await AppServices.ChatSessionService.GetOrCreateAgentContextAsync(session);

            var assistantMsg = new ChatMessage
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = "assistant",
                Text = string.Empty,
                Timestamp = DateTime.Now,
            };
            Messages.Add(assistantMsg);

            var assistantText = string.Empty;
            await foreach (var update in context.Agent.RunStreamingAsync(text, context.Session))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    assistantText += update.Text;
                    assistantMsg.Text = assistantText;
                }
            }

            var savedMsg = new Services.ChatMessage
            {
                Id = assistantMsg.Id,
                Role = "assistant",
                Text = assistantText
            };
            AppServices.ChatSessionService.AddMessage(_sessionId, savedMsg);
        }
        catch
        {
            Messages.Add(new ChatMessage
            {
                Id = Guid.NewGuid().ToString("N"),
                Role = "assistant",
                Text = "出错了，请稍后重试。",
                Timestamp = DateTime.Now,
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        AppServices.ChatSessionService.ClearMessages(_sessionId);
        Messages.Clear();
        CanClear = false;
    }
}
