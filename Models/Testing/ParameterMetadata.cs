namespace Three2025.Models.Testing;

#nullable enable

/// <summary>
/// Metadata about a tool method parameter extracted from reflection
/// </summary>
public class ParameterMetadata
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public object? DefaultValue { get; set; }
    public bool IsOptional { get; set; }
    public bool HasDefaultValue { get; set; }
}
