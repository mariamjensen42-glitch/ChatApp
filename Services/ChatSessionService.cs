using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatApp.Services;

public sealed class ChatMessage
{
    public required string Id { get; set; }
    public required string Role { get; set; }
    public required string Text { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}

public sealed class ChatSession
{
    public required string Id { get; set; }
    public required string RoleId { get; set; }
    public required string ModelId { get; set; }
    public string Title { get; set; } = "新对话";
    public List<ChatMessage> Messages { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;

    [JsonIgnore]
    public ChatContext? AgentContext { get; set; }
}

public sealed class ChatSessionService
{
    private static readonly string SavePath = Path.Combine(AppContext.BaseDirectory, "sessions.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ConcurrentDictionary<string, ChatSession> _sessions = new();
    private readonly AgentFactory _agentFactory;
    private readonly CustomRoleService _customRoleService;
    private readonly object _saveLock = new();

    public ChatSessionService(AgentFactory agentFactory, CustomRoleService customRoleService)
    {
        _agentFactory = agentFactory;
        _customRoleService = customRoleService;
        Load();
    }

    public ChatSession CreateSession(string roleId, string modelId)
    {
        var role = RoleCatalog.GetById(roleId) ?? _customRoleService.GetById(roleId);
        if (role == null)
            throw new ArgumentException("无效的角色ID", nameof(roleId));

        var session = new ChatSession
        {
            Id = Guid.NewGuid().ToString("N"),
            RoleId = roleId,
            ModelId = modelId,
            Title = $"与{role.Name}的对话"
        };

        _sessions[session.Id] = session;
        Save();
        return session;
    }

    public ChatSession? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    public IReadOnlyList<ChatSession> GetAllSessions()
    {
        return _sessions.Values.OrderByDescending(s => s.UpdatedAt).ToList();
    }

    public void DeleteSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
        Save();
    }

    public void ClearMessages(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Messages.Clear();
            session.AgentContext = null;
            session.UpdatedAt = DateTimeOffset.Now;
            Save();
        }
    }

    public async Task<ChatContext> GetOrCreateAgentContextAsync(ChatSession session)
    {
        if (session.AgentContext != null)
            return session.AgentContext;

        var role = RoleCatalog.GetById(session.RoleId) ?? _customRoleService.GetById(session.RoleId);
        if (role == null)
            throw new InvalidOperationException("角色不存在");

        session.AgentContext = await _agentFactory.CreateAsync(role, session.ModelId);
        return session.AgentContext;
    }

    public void AddMessage(string sessionId, ChatMessage message)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.Messages.Add(message);
            session.UpdatedAt = DateTimeOffset.Now;

            if (session.Messages.Count == 1 && message.Role == "user")
            {
                session.Title = message.Text.Length > 20
                    ? message.Text[..20] + "..."
                    : message.Text;
            }

            Save();
        }
    }

    private void Save()
    {
        lock (_saveLock)
        {
            var json = JsonSerializer.Serialize(_sessions.Values.ToList(), JsonOptions);
            File.WriteAllText(SavePath, json);
        }
    }

    private void Load()
    {
        if (!File.Exists(SavePath)) return;

        try
        {
            var json = File.ReadAllText(SavePath);
            var sessions = JsonSerializer.Deserialize<List<ChatSession>>(json, JsonOptions) ?? [];
            foreach (var s in sessions)
                _sessions[s.Id] = s;
        }
        catch
        {
        }
    }
}