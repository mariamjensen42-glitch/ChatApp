using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Interactivity;
using ChatApp.ViewModels;
using LiveMarkdown.Avalonia;

namespace ChatApp.Views;

public partial class ChatView : UserControl
{
    private bool _isUserAtBottom = true;

    public ChatView()
    {
        InitializeComponent();
        InputBox.AddHandler(KeyDownEvent, OnInputKeyDown, RoutingStrategies.Tunnel);
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var scroll = this.GetVisualDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault(s => s.Name == "MessagesScroll");
        if (scroll != null)
            scroll.ScrollChanged += OnScrollChanged;

        if (DataContext is not ChatViewModel vm) return;

        var renderers = this.GetVisualDescendants()
            .OfType<MarkdownRenderer>()
            .ToList();

        foreach (var renderer in renderers)
        {
            if (renderer.DataContext is not ChatMessage msg || msg.Role != "assistant") continue;

            var builder = new ObservableStringBuilder();
            builder.Append(msg.Text ?? string.Empty);
            renderer.MarkdownBuilder = builder;

            msg.PropertyChanged += (_, propertyArgs) =>
            {
                if (propertyArgs.PropertyName == nameof(ChatMessage.Text))
                {
                    builder.Clear();
                    builder.Append(msg.Text ?? string.Empty);
                    TryAutoScroll();
                }
            };
        }

        _isUserAtBottom = true;
        TryAutoScroll();
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var scroll = (ScrollViewer)sender!;
        var extentHeight = scroll.Extent.Height;
        var viewportHeight = scroll.Viewport.Height;
        var offsetY = scroll.Offset.Y;
        _isUserAtBottom = extentHeight - viewportHeight - offsetY < 40;
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            if (DataContext is ChatViewModel vm)
                vm.SendCommand.Execute(null);
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is not ChatViewModel vm) return;

        vm.Messages.CollectionChanged += OnMessagesChanged;
    }

    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems == null) return;

        Dispatcher.UIThread.Post(() =>
        {
            foreach (ChatMessage msg in e.NewItems)
            {
                if (msg.Role != "assistant") continue;

                var renderer = FindMarkdownRendererForMessage(msg);
                if (renderer == null) continue;

                var builder = new ObservableStringBuilder();
                builder.Append(msg.Text ?? string.Empty);
                renderer.MarkdownBuilder = builder;

                msg.PropertyChanged += (_, propertyArgs) =>
                {
                    if (propertyArgs.PropertyName == nameof(ChatMessage.Text))
                    {
                        builder.Clear();
                        builder.Append(msg.Text ?? string.Empty);
                        TryAutoScroll();
                    }
                };
            }

            _isUserAtBottom = true;
            TryAutoScroll();
        });
    }

    private void TryAutoScroll()
    {
        if (!_isUserAtBottom) return;

        Dispatcher.UIThread.Post(() =>
        {
            var scroll = this.GetVisualDescendants()
                .OfType<ScrollViewer>()
                .FirstOrDefault(s => s.Name == "MessagesScroll");
            scroll?.ScrollToEnd();
        });
    }

    private MarkdownRenderer? FindMarkdownRendererForMessage(ChatMessage msg)
    {
        if (msg == null) return null;

        return this.GetVisualDescendants()
            .OfType<MarkdownRenderer>()
            .FirstOrDefault(r => r.DataContext == msg);
    }
}
