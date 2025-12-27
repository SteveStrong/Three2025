#nullable enable

using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;

namespace Three2025.Components.Shared.Chat;

public partial class ChatMessageList
{
    /// <summary>
    /// The list of messages to display
    /// </summary>
    [Parameter, EditorRequired]
    public List<ChatDisplayMessage> Messages { get; set; } = new();

    /// <summary>
    /// Current streaming text being typed
    /// </summary>
    [Parameter]
    public string? StreamingText { get; set; }

    /// <summary>
    /// Whether a message is currently being processed
    /// </summary>
    [Parameter]
    public bool IsProcessing { get; set; }

    /// <summary>
    /// The name of the current agent responding
    /// </summary>
    [Parameter]
    public string CurrentAgent { get; set; } = "Assistant";

    private ElementReference containerRef;
    private ElementReference scrollAnchor;
}
