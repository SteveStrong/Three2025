namespace Three2025.Models.Chat;

/// <summary>
/// Metadata describing a test sequence
/// </summary>
public record TestSequenceMetadata
{
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Category { get; init; } = "General";
    public string[] Prompts { get; init; } = Array.Empty<string>();
    public int PromptCount => Prompts?.Length ?? 0;
}

/// <summary>
/// Attribute for marking test sequence properties with metadata
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TestSequenceAttribute : Attribute
{
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
}
