namespace Three2025.Models.Chat;

/// <summary>
/// Represents an entry in the activity/debug log
/// </summary>
public class ActivityLogEntry
{
    /// <summary>
    /// The type/category of log entry
    /// </summary>
    public string Type { get; set; } = "";
    
    /// <summary>
    /// The main log message
    /// </summary>
    public string Message { get; set; } = "";
    
    /// <summary>
    /// Optional additional details
    /// </summary>
    public string Details { get; set; } = "";
    
    /// <summary>
    /// When the log entry was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
