#nullable enable

using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;

namespace Three2025.Components.Shared.Chat;

public partial class ActivityLog
{
    /// <summary>
    /// The list of log entries to display
    /// </summary>
    [Parameter, EditorRequired]
    public List<Models.Chat.ActivityLogEntry> Logs { get; set; } = new();

    /// <summary>
    /// Whether to auto-scroll to bottom on new entries
    /// </summary>
    [Parameter]
    public bool AutoScroll { get; set; } = true;

    /// <summary>
    /// Event callback when auto-scroll toggle is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnToggleAutoScroll { get; set; }

    /// <summary>
    /// Event callback when clear button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnClear { get; set; }

    private ElementReference containerRef;
    private ElementReference scrollAnchor;

    private async Task HandleToggleAutoScroll()
    {
        await OnToggleAutoScroll.InvokeAsync();
    }

    private async Task HandleClear()
    {
        await OnClear.InvokeAsync();
    }
}
