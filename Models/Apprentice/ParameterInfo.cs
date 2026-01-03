#nullable enable
namespace Three2025.Models.Apprentice;

/// <summary>
/// Information about a parameter in a component
/// </summary>
public class ParameterInfo
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public string? Unit { get; set; }
    public bool IsFormula { get; set; }
    
    // UI-friendly properties
    public bool IsCalculated => IsFormula;
    public string DisplayValue => Unit != null ? $"{Value} {Unit}" : Value;
    public string Formula => IsFormula ? Value : "";
}
