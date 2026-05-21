using System.Collections.ObjectModel;
using System.Linq;
using ChatApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatApp.ViewModels;

public sealed partial class SessionItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _roleId = string.Empty;

    [ObservableProperty]
    private string _modelId = string.Empty;

    [ObservableProperty]
    private DateTime _updatedAt;
}

public sealed partial class SessionGroup : ObservableObject
{
    [ObservableProperty]
    private string _label = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SessionItem> _items = [];
}

public sealed partial class ModelItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    public override string ToString() => $"{Name} - {Description}";
}

public sealed partial class RoleItem : ObservableObject
{
    [ObservableProperty]
    private string _id = string.Empty;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _custom;
}

public sealed partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentPage;

    [ObservableProperty]
    private ObservableCollection<SessionGroup> _sessionGroups = [];

    [ObservableProperty]
    private ObservableCollection<ModelItem> _models = [];

    [ObservableProperty]
    private ModelItem? _selectedModel;

    [ObservableProperty]
    private bool _sidebarOpen = true;

    [ObservableProperty]
    private string _currentSessionId = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    private void DismissError()
    {
        ErrorMessage = string.Empty;
    }

    public MainWindowViewModel()
    {
        Models.Add(new ModelItem { Id = "deepseek-v4-flash", Name = "DeepSeek V4 Flash", Description = "极速响应" });
        Models.Add(new ModelItem { Id = "deepseek-v4-pro", Name = "DeepSeek V4 Pro", Description = "旗舰模型" });
        Models.Add(new ModelItem { Id = "deepseek-chat", Name = "DeepSeek Chat", Description = "即将弃用" });
        Models.Add(new ModelItem { Id = "deepseek-reasoner", Name = "DeepSeek Reasoner", Description = "即将弃用" });
        SelectedModel = Models[0];

        LoadExistingSessions();

        CurrentPage = new WelcomeViewModel(this);
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        SidebarOpen = !SidebarOpen;
    }

    [RelayCommand]
    private void NewChat()
    {
        CurrentSessionId = string.Empty;
        CurrentPage = new WelcomeViewModel(this);
    }

    [RelayCommand]
    private void OpenSettings()
    {
        CurrentPage = new SettingsViewModel();
    }

    [RelayCommand]
    private void SelectSession(string sessionId)
    {
        CurrentSessionId = sessionId;
        CurrentPage = new ChatViewModel(this, sessionId);
    }

    [RelayCommand]
    private void DeleteSession(string sessionId)
    {
        AppServices.ChatSessionService.DeleteSession(sessionId);

        foreach (var group in SessionGroups)
        {
            var item = group.Items.FirstOrDefault(i => i.Id == sessionId);
            if (item != null)
            {
                group.Items.Remove(item);
                break;
            }
        }

        for (int i = SessionGroups.Count - 1; i >= 0; i--)
        {
            if (SessionGroups[i].Items.Count == 0)
                SessionGroups.RemoveAt(i);
        }

        if (CurrentSessionId == sessionId)
        {
            CurrentSessionId = string.Empty;
            CurrentPage = new WelcomeViewModel(this);
        }
    }

    internal void HandleSessionNotFound(string sessionId)
    {
        DeleteSessionCommand.Execute(sessionId);
        ErrorMessage = "对话不存在，可能已被删除。";
    }

    [RelayCommand]
    private async Task StartChat(string roleId)
    {
        try
        {
            var modelId = SelectedModel?.Id ?? "deepseek-v4-flash";
            var session = AppServices.ChatSessionService.CreateSession(roleId, modelId);

            var item = new SessionItem
            {
                Id = session.Id,
                Title = session.Title,
                RoleId = roleId,
                ModelId = session.ModelId,
                UpdatedAt = DateTime.Now,
            };

            var todayGroup = SessionGroups.FirstOrDefault(g => g.Label == "今天");
            if (todayGroup == null)
            {
                todayGroup = new SessionGroup { Label = "今天" };
                SessionGroups.Insert(0, todayGroup);
            }
            todayGroup.Items.Insert(0, item);

            CurrentSessionId = session.Id;
            CurrentPage = new ChatViewModel(this, session.Id);
        }
        catch (ArgumentException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    internal void UpdateSessionTitle(string sessionId, string title)
    {
        foreach (var group in SessionGroups)
        {
            var item = group.Items.FirstOrDefault(i => i.Id == sessionId);
            if (item != null)
            {
                item.Title = title;
                break;
            }
        }
    }

    private void LoadExistingSessions()
    {
        var sessions = AppServices.ChatSessionService.GetAllSessions();
        if (sessions.Count == 0) return;

        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);

        foreach (var session in sessions)
        {
            var date = session.UpdatedAt.LocalDateTime.Date;
            var label = date == today ? "今天"
                : date == yesterday ? "昨天"
                : "更早";

            var group = SessionGroups.FirstOrDefault(g => g.Label == label);
            if (group == null)
            {
                group = new SessionGroup { Label = label };
                SessionGroups.Add(group);
            }

            group.Items.Add(new SessionItem
            {
                Id = session.Id,
                Title = session.Title,
                RoleId = session.RoleId,
                ModelId = session.ModelId,
                UpdatedAt = session.UpdatedAt.LocalDateTime,
            });
        }
    }
}
