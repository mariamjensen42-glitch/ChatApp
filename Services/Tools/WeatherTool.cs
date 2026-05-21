using System.ComponentModel;

namespace ChatApp.Services.Tools;

public sealed class WeatherTool
{
    private static readonly Random _random = new();
    private static readonly string[] _conditions = ["晴天", "多云", "雨天", "雪天"];

    [Description("获取指定城市的当前天气")]
    public static string GetWeather([Description("城市名称")] string city)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        var temp = _random.Next(15, 30);
        var condition = _conditions[_random.Next(_conditions.Length)];

        return $"{city} 今天{condition}，温度 {temp}°C";
    }
}
