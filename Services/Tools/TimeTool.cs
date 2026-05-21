using System.ComponentModel;

namespace ChatApp.Services.Tools;

public sealed class TimeTool
{
    [Description("获取当前时间")]
    public static string GetCurrentTime()
    {
        return DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
