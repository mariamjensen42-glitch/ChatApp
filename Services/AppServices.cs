using Microsoft.Extensions.Configuration;

namespace ChatApp.Services;

public static class AppServices
{
    public static CustomRoleService CustomRoleService { get; } = new();
    public static AgentFactory AgentFactory { get; private set; } = null!;
    public static ChatSessionService ChatSessionService { get; private set; } = null!;

    public static void Initialize()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var apiKey = config["DeepSeek:ApiKey"]
            ?? Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
            ?? throw new InvalidOperationException("请在 appsettings.json 中设置 DeepSeek:ApiKey 或设置 DEEPSEEK_API_KEY 环境变量");

        var endpoint = config["DeepSeek:Endpoint"] ?? "https://api.deepseek.com/v1";

        AgentFactory = new AgentFactory(apiKey, endpoint);
        ChatSessionService = new ChatSessionService(AgentFactory, CustomRoleService);
    }
}
