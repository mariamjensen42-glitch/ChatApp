using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ChatApp.Services;
using ChatApp.ViewModels;
using ChatApp.Views;
using LiveMarkdown.Avalonia;

namespace ChatApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        AppServices.Initialize();
        SettingsService.LoadAndApplyTheme();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        SyncLiveMarkdownTheme();
        ActualThemeVariantChanged += (_, _) => SyncLiveMarkdownTheme();

        WarmupLiveMarkdown();

        base.OnFrameworkInitializationCompleted();
    }

    private void WarmupLiveMarkdown()
    {
        _ = DispatcherTimer.RunOnce(() =>
        {
            var builder = new ObservableStringBuilder();
            builder.Append("# Hello\n\n**bold** *italic* `code`\n\n```csharp\nConsole.WriteLine(\"test\");\n```\n");
            var renderer = new MarkdownRenderer { MarkdownBuilder = builder };
            _ = renderer.ToString();
            builder.Clear();
        }, TimeSpan.FromMilliseconds(500));
    }

    private void SyncLiveMarkdownTheme()
    {
        var isDark = ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;

        void SetColor(string key, Color light, Color dark)
        {
            Resources[key] = isDark ? dark : light;
        }

        SetColor("BorderColor",              Color.Parse("#3C3C3C"), Color.Parse("#404040"));
        SetColor("ForegroundColor",          Color.Parse("#1F2937"), Color.Parse("#E5E7EB"));
        SetColor("CardBackgroundColor",      Color.Parse("#F9FAFB"), Color.Parse("#252525"));
        SetColor("SecondaryCardBackgroundColor", Color.Parse("#1E1E1E"), Color.Parse("#1E1E1E"));
        SetColor("CodeInlineColor",          Color.Parse("#B45309"), Color.Parse("#CE9178"));
        SetColor("QuoteBorderColor",         Color.Parse("#0969DA"), Color.Parse("#569CD6"));
    }
}
