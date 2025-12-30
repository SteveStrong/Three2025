#nullable enable

namespace Three2025.Models.Chat;

/// <summary>
/// Represents a chat message for display in the UI
/// </summary>
public class ChatDisplayMessage
{
    /// <summary>
    /// True if message is from user, false if from assistant
    /// </summary>
    public bool IsUser { get; set; }
    
    /// <summary>
    /// The message text content (can contain HTML)
    /// </summary>
    public string Text { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional agent name for assistant messages
    /// </summary>
    public string? AgentName { get; set; }
    
    /// <summary>
    /// When the message was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
