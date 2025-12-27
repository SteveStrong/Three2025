#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Three2025.Components.Shared.Chat;

public partial class ChatInput
{
    /// <summary>
    /// The current input value
    /// </summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Event callback for value changes
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Event callback when send button is clicked
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback OnSend { get; set; }

    /// <summary>
    /// Whether the input is disabled during processing
    /// </summary>
    [Parameter]
    public bool IsProcessing { get; set; }

    /// <summary>
    /// Placeholder text for the input field
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Type a message...";

    /// <summary>
    /// Optional quick action buttons to display below input
    /// </summary>
    [Parameter]
    public RenderFragment? QuickActions { get; set; }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await HandleSend();
        }
    }

    private async Task HandleSend()
    {
        if (!string.IsNullOrWhiteSpace(Value) && !IsProcessing)
        {
            await OnSend.InvokeAsync();
        }
    }
}
