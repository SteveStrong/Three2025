using Three2025.Services.Logging;

namespace Three2025.Services.Logging;

/// <summary>
/// USAGE INSTRUCTIONS for UniversalLogger
/// 
/// To enable UniversalLogger in your application:
/// 
/// 1. In Program.cs, add this code BEFORE var app = builder.Build():
/// 
///    builder.Logging.ClearProviders();  // Remove default loggers
///    builder.Logging.AddProvider(new UniversalLoggerProvider());
/// 
/// 2. To customize filtering, pass a filter function:
/// 
///    builder.Logging.AddProvider(new UniversalLoggerProvider(
///        (category, logLevel) => 
///        {
///            // Only show errors from Microsoft libraries
///            if (category.Contains("Microsoft"))
///                return logLevel >= LogLevel.Error;
///            
///            // Show everything Debug and above from your app
///            return logLevel >= LogLevel.Debug;
///        }
///    ));
/// 
/// 3. To customize log formatting, edit UniversalLogger.cs:
///    - Modify FormatLogEntry() to change the log message format
///    - Modify ExportToConsole() to change colors or output behavior
///    - Modify GetLevelIcon() to change emoji/icons
///    - Modify GetConsoleColor() to change colors per log level
/// 
/// EXAMPLES:
/// 
/// Example 1: Minimal logging (Errors only)
/// ──────────────────────────────────────────
/// builder.Logging.ClearProviders();
/// builder.Logging.AddProvider(new UniversalLoggerProvider(
///     (_, level) => level >= LogLevel.Error
/// ));
/// 
/// Example 2: Verbose application logging
/// ──────────────────────────────────────────
/// builder.Logging.ClearProviders();
/// builder.Logging.AddProvider(new UniversalLoggerProvider(
///     (category, level) => 
///     {
///         if (category.Contains("Three2025"))
///             return level >= LogLevel.Trace;  // Everything from your app
///         return level >= LogLevel.Warning;    // Warnings+ from libraries
///     }
/// ));
/// 
/// Example 3: Focus on specific services
/// ──────────────────────────────────────────
/// builder.Logging.ClearProviders();
/// builder.Logging.AddProvider(new UniversalLoggerProvider(
///     (category, level) => 
///     {
///         // Detailed logs from chat services
///         if (category.Contains("ChatOrchestrator") || 
///             category.Contains("Agent"))
///             return level >= LogLevel.Debug;
///         
///         // Normal logs from everything else
///         return level >= LogLevel.Information;
///     }
/// ));
/// 
/// CUSTOMIZATION EXAMPLES:
/// 
/// To change log format, edit UniversalLogger.FormatLogEntry():
/// 
/// // Compact format:
/// var formattedMessage = $"{timestamp} {levelIcon} {message}";
/// 
/// // Detailed format:
/// var formattedMessage = $"[{timestamp}] [{logLevel}] [{category}]\n  {message}";
/// 
/// // JSON-like format:
/// var formattedMessage = $"{{\"time\":\"{timestamp}\",\"level\":\"{logLevel}\",\"msg\":\"{message}\"}}";
/// 
/// To change colors, edit UniversalLogger.GetConsoleColor():
/// 
/// LogLevel.Information => ConsoleColor.Cyan,    // Make info messages cyan
/// LogLevel.Warning => ConsoleColor.Magenta,     // Make warnings magenta
/// 
/// To add file logging, modify UniversalLogger.ExportToConsole():
/// 
/// private void ExportToConsole(string formattedEntry, LogLevel logLevel)
/// {
///     Console.WriteLine(formattedEntry);
///     
///     // Also write to file
///     File.AppendAllText("logs.txt", formattedEntry + Environment.NewLine);
/// }
/// 
/// </summary>
public static class UniversalLoggerUsageGuide
{
    // This class exists only for documentation purposes
}
