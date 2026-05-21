using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace ChatApp.Services;

public sealed class ChatContext
{
    public required AIAgent Agent { get; init; }
    public required Role Role { get; init; }
    public AgentSession Session { get; set; } = null!;
}

public sealed class AgentFactory
{
    private readonly string _apiKey;
    private readonly string _endpoint;

    private static readonly Dictionary<string, Func<AIFunction>> _toolRegistry = new()
    {
        ["WeatherTool"] = () => AIFunctionFactory.Create(Tools.WeatherTool.GetWeather),
        ["TimeTool"] = () => AIFunctionFactory.Create(Tools.TimeTool.GetCurrentTime),
        ["CalculatorTool"] = () => AIFunctionFactory.Create(Tools.CalculatorTool.Calculate)
    };

    public AgentFactory(string apiKey, string endpoint)
    {
        _apiKey = apiKey;
        _endpoint = endpoint;
    }

    public async Task<ChatContext> CreateAsync(Role role, string model)
    {
        var openAiClient = new OpenAIClient(
            new ApiKeyCredential(_apiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri(_endpoint)
            });

        var tools = role.Tools
            .Where(_toolRegistry.ContainsKey)
            .Select(t => _toolRegistry[t]())
            .ToArray();

        var agent = openAiClient
            .GetChatClient(model)
            .AsIChatClient()
            .AsAIAgent(
                instructions: role.Instructions,
                name: role.Name,
                tools: tools);

        var session = await agent.CreateSessionAsync();

        return new ChatContext
        {
            Agent = agent,
            Role = role,
            Session = session
        };
    }
}
