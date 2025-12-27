#nullable enable

using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;

namespace Three2025.Components.Shared.Chat;

/// <summary>
/// Main chat panel component that orchestrates the entire chat UI
/// </summary>
public partial class ChatPanel
{
    #region Message Display Parameters

    /// <summary>
    /// List of messages to display in the chat
    /// </summary>
    [Parameter, EditorRequired]
    public List<ChatDisplayMessage> Messages { get; set; } = new();

    /// <summary>
    /// Current streaming text being generated
    /// </summary>
    [Parameter]
    public string? StreamingText { get; set; }

    /// <summary>
    /// Whether a message is currently being processed
    /// </summary>
    [Parameter]
    public bool IsProcessing { get; set; }

    /// <summary>
    /// Name of the current agent responding
    /// </summary>
    [Parameter]
    public string CurrentAgent { get; set; } = "Assistant";

    #endregion

    #region Input Parameters

    /// <summary>
    /// Current input value
    /// </summary>
    [Parameter]
    public string InputValue { get; set; } = string.Empty;

    /// <summary>
    /// Event callback when input value changes
    /// </summary>
    [Parameter]
    public EventCallback<string> InputValueChanged { get; set; }

    /// <summary>
    /// Placeholder text for input field
    /// </summary>
    [Parameter]
    public string InputPlaceholder { get; set; } = "Type a message...";

    /// <summary>
    /// Event callback when send button is clicked
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback OnSendMessage { get; set; }

    #endregion

    #region Activity Log Parameters

    /// <summary>
    /// Whether to show the activity log panel
    /// </summary>
    [Parameter]
    public bool ShowActivityLog { get; set; } = false;

    /// <summary>
    /// List of activity log entries
    /// </summary>
    [Parameter]
    public List<Models.Chat.ActivityLogEntry> ActivityLogs { get; set; } = new();

    /// <summary>
    /// Whether activity log should auto-scroll
    /// </summary>
    [Parameter]
    public bool AutoScrollLogs { get; set; } = true;

    /// <summary>
    /// Event callback when auto-scroll toggle is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnToggleAutoScroll { get; set; }

    /// <summary>
    /// Event callback when clear logs button is clicked
    /// </summary>
    [Parameter]
    public EventCallback OnClearLogs { get; set; }

    #endregion

    #region Layout Parameters

    /// <summary>
    /// Flex ratio for chat messages area (default: 3)
    /// </summary>
    [Parameter]
    public string ChatFlexRatio { get; set; } = "3";

    /// <summary>
    /// Flex ratio for activity log area (default: 2)
    /// </summary>
    [Parameter]
    public string LogFlexRatio { get; set; } = "2";

    /// <summary>
    /// Custom content for the header area
    /// </summary>
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    /// <summary>
    /// Custom footer actions (e.g., quick test buttons)
    /// </summary>
    [Parameter]
    public RenderFragment? FooterActions { get; set; }

    #endregion

    private async Task OnInputValueChanged()
    {
        await InputValueChanged.InvokeAsync(InputValue);
    }
}
