#nullable enable

using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;

namespace Three2025.Components.Shared.Chat;

public partial class ChatMessage
{
    /// <summary>
    /// The message to display
    /// </summary>
    [Parameter, EditorRequired]
    public ChatDisplayMessage Message { get; set; } = default!;

    /// <summary>
    /// Optional agent name override for assistant messages
    /// </summary>
    [Parameter]
    public string? AgentName { get; set; }
}
