namespace ChatApp.Services;

public sealed class Role
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Instructions { get; init; }
    public string[] Tools { get; init; } = [];
    public bool Custom { get; init; }
}
