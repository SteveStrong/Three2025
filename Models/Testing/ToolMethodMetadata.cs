using System.Reflection;

namespace Three2025.Models.Testing;

#nullable enable

/// <summary>
/// Metadata about a tool method extracted from [Description] attributes
/// </summary>
public class ToolMethodMetadata
{
    public string MethodName { get; set; } = "";
    public string Description { get; set; } = "";
    public List<ParameterMetadata> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = "";
    public MethodInfo MethodInfo { get; set; } = null!;
    public string Category { get; set; } = "General";
}
