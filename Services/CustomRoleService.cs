using System.Collections.Concurrent;

namespace ChatApp.Services;

public sealed class CustomRoleService
{
    private readonly ConcurrentDictionary<string, Role> _roles = new();

    public IReadOnlyList<Role> GetAll()
    {
        return _roles.Values.OrderBy(r => r.Name).ToList();
    }

    public Role? GetById(string id)
    {
        _roles.TryGetValue(id, out var role);
        return role;
    }

    public Role Create(string name, string description, string instructions, string[] tools)
    {
        var id = "custom_" + Guid.NewGuid().ToString("N")[..8];
        var role = new Role
        {
            Id = id,
            Name = name,
            Description = description,
            Instructions = instructions,
            Tools = tools,
            Custom = true
        };
        _roles[id] = role;
        return role;
    }

    public Role? Update(string id, string name, string description, string instructions, string[] tools)
    {
        if (!_roles.TryGetValue(id, out _))
            return null;

        var role = new Role
        {
            Id = id,
            Name = name,
            Description = description,
            Instructions = instructions,
            Tools = tools,
            Custom = true
        };
        _roles[id] = role;
        return role;
    }

    public bool Delete(string id)
    {
        return _roles.TryRemove(id, out _);
    }
}
