#nullable enable
namespace Three2025.Models.Apprentice;

/// <summary>
/// Information about a component in the knowledge model
/// </summary>
public class ComponentInfo
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required string Type { get; init; }
    public string? ParentPath { get; init; }
    public int ChildCount { get; init; }
    public List<ParameterInfo> Parameters { get; init; } = new();
}
