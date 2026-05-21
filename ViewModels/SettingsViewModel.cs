using System.Text.Json;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChatApp.ViewModels;

public sealed partial class SettingsViewModel : ViewModelBase
{
    private static readonly string SavePath = Path.Combine(AppContext.BaseDirectory, "settings.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _endpoint = "https://api.deepseek.com/v1";

    [ObservableProperty]
    private int _themeIndex;

    public string[] Themes { get; } = ["跟随系统", "浅色", "深色"];

    public SettingsViewModel()
    {
        Load();
        ThemeIndex = SettingsService.ThemeIndex;
    }

    partial void OnThemeIndexChanged(int value)
    {
        ApplyTheme(value);
        Save();
    }

    [RelayCommand]
    private void Save()
    {
        var settings = new SettingsData
        {
            ApiKey = ApiKey,
            Endpoint = Endpoint,
            ThemeIndex = ThemeIndex,
        };

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SavePath, json);
    }

    private void Load()
    {
        if (!File.Exists(SavePath)) return;

        try
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonSerializer.Deserialize<SettingsData>(json, JsonOptions);

            if (data != null)
            {
                ApiKey = data.ApiKey ?? string.Empty;
                Endpoint = data.Endpoint ?? "https://api.deepseek.com/v1";
            }
        }
        catch
        {
        }
    }

    private void ApplyTheme(int index)
    {
        SettingsService.ApplyTheme(index);
    }
}

public class SettingsData
{
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    public int ThemeIndex { get; set; }
}

public static class SettingsService
{
    private static readonly string SavePath = Path.Combine(AppContext.BaseDirectory, "settings.json");

    public static int ThemeIndex { get; private set; }

    public static void LoadAndApplyTheme()
    {
        if (!File.Exists(SavePath)) return;

        try
        {
            var json = File.ReadAllText(SavePath);
            var data = System.Text.Json.JsonSerializer.Deserialize<SettingsData>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (data != null)
            {
                ThemeIndex = data.ThemeIndex;
                ApplyTheme(ThemeIndex);
            }
        }
        catch
        {
        }
    }

    public static void ApplyTheme(int index)
    {
        ThemeIndex = index;
        var app = Avalonia.Application.Current;
        if (app == null) return;

        var theme = index switch
        {
            0 => ThemeVariant.Default,
            1 => ThemeVariant.Light,
            2 => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
        app.RequestedThemeVariant = theme;
    }
}
