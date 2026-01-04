#nullable enable

namespace Three2025.Services.Logging;

/// <summary>
/// Custom logger with easily modifiable console output formatting.
/// Modify the FormatLogEntry method to change how logs appear in console.
/// </summary>
public class UniversalLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Func<string, LogLevel, bool> _filter;

    public UniversalLogger(string categoryName, Func<string, LogLevel, bool>? filter = null)
    {
        _categoryName = categoryName;
        _filter = filter ?? ((category, logLevel) => true);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null; // Scope tracking not implemented
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _filter(_categoryName, logLevel);
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var formattedEntry = FormatLogEntry(logLevel, _categoryName, message, exception);
        
        ExportToConsole(formattedEntry, logLevel);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // MODIFY THIS METHOD TO CHANGE LOG FORMAT
    // ═══════════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Format the log entry for console output.
    /// CUSTOMIZE THIS METHOD to change how logs look in the console.
    /// </summary>
    private string FormatLogEntry(LogLevel logLevel, string category, string message, Exception? exception)
    {
        // Format components
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        var levelIcon = GetLevelIcon(logLevel);
        var shortCategory = GetShortCategory(category);
        
        // CUSTOMIZE THIS FORMAT STRING
        //var formattedMessage = $"[{timestamp}] {levelIcon} {shortCategory}: {message}";

        var formattedMessage = $"[{timestamp}] {message}";        
        // Add exception if present
        if (exception != null)
        {
            formattedMessage += $"\n  ❌ Exception: {exception.GetType().Name}: {exception.Message}";
            if (exception.StackTrace != null)
            {
                var stackLines = exception.StackTrace.Split('\n').Take(3); // First 3 lines
                formattedMessage += $"\n  {string.Join("\n  ", stackLines)}";
            }
        }
        
        return formattedMessage;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // MODIFY THIS METHOD TO CHANGE CONSOLE OUTPUT BEHAVIOR
    // ═══════════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Export formatted log to console.
    /// CUSTOMIZE THIS METHOD to change console colors or output behavior.
    /// </summary>
    private void ExportToConsole(string formattedEntry, LogLevel logLevel)
    {
        // Save original color
        var originalColor = Console.ForegroundColor;
        
        // CUSTOMIZE THESE COLORS
        Console.ForegroundColor = GetConsoleColor(logLevel);
        Console.WriteLine(formattedEntry);
        
        // Restore original color
        Console.ForegroundColor = originalColor;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // HELPER METHODS - CUSTOMIZE AS NEEDED
    // ═══════════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Get emoji/icon for log level.
    /// CUSTOMIZE to change icons.
    /// </summary>
    private string GetLevelIcon(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "🔍",
        LogLevel.Debug => "🐛",
        LogLevel.Information => "ℹ️ ",
        LogLevel.Warning => "⚠️ ",
        LogLevel.Error => "❌",
        LogLevel.Critical => "💥",
        _ => "  "
    };

    /// <summary>
    /// Get console color for log level.
    /// CUSTOMIZE to change colors.
    /// </summary>
    private ConsoleColor GetConsoleColor(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => ConsoleColor.Gray,
        LogLevel.Debug => ConsoleColor.DarkGray,
        LogLevel.Information => ConsoleColor.White,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Error => ConsoleColor.Red,
        LogLevel.Critical => ConsoleColor.DarkRed,
        _ => ConsoleColor.White
    };

    /// <summary>
    /// Shorten category name for readability.
    /// CUSTOMIZE to change category display.
    /// </summary>
    private string GetShortCategory(string category)
    {
        // Extract last part of namespace: "Three2025.Services.Chat.ChatOrchestrator" -> "ChatOrchestrator"
        var parts = category.Split('.');
        return parts.Length > 0 ? parts[^1] : category;
    }
}
