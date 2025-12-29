using Microsoft.AspNetCore.Components;
using Three2025.Models.Testing;

namespace Three2025.Components.Shared.Testing;

public partial class ToolMethodButton
{
#nullable enable
    
    [Parameter] public ToolMethodMetadata Method { get; set; } = null!;
    [Parameter] public EventCallback OnExecute { get; set; }
    [Parameter] public bool IsExecuting { get; set; }

    private bool _showDetails = false;

    private void ToggleDetails()
    {
        _showDetails = !_showDetails;
    }

    private string FormatDefaultValue(object? value)
    {
        if (value == null) return "null";
        if (value is string s) return $"\"{s}\"";
        if (value is bool b) return b.ToString().ToLower();
        return value.ToString() ?? "null";
    }
}
