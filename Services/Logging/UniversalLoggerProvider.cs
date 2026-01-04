#nullable enable

namespace Three2025.Services.Logging;

/// <summary>
/// Provider that creates UniversalLogger instances.
/// Register this in Program.cs to use custom logging throughout the application.
/// </summary>
public class UniversalLoggerProvider : ILoggerProvider
{
    private readonly Func<string, LogLevel, bool> _filter;

    /// <summary>
    /// Create a new UniversalLoggerProvider with optional filtering.
    /// </summary>
    /// <param name="filter">Filter function to control which logs are shown. 
    /// If null, defaults to Information level and above.</param>
    public UniversalLoggerProvider(Func<string, LogLevel, bool>? filter = null)
    {
        _filter = filter ?? DefaultFilter;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new UniversalLogger(categoryName, _filter);
    }

    public void Dispose()
    {
        // No cleanup needed
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // MODIFY THIS METHOD TO CHANGE DEFAULT FILTERING
    // ═══════════════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Default filter: Show Information and above for most categories.
    /// CUSTOMIZE this method to change which logs are shown by default.
    /// </summary>
    private static bool DefaultFilter(string category, LogLevel logLevel)
    {
        // IMPORTANT: Allow startup messages (Now listening on..., Application started...)
        if (category.Contains("Microsoft.Hosting.Lifetime"))
        {
            return logLevel >= LogLevel.Information;
        }

        // Suppress noisy Microsoft/ASP.NET Core logs - only show warnings and above
        if (category.Contains("Microsoft.AspNetCore") || 
            category.Contains("Microsoft.Hosting") ||
            category.Contains("Microsoft.Extensions"))
        {
            return logLevel >= LogLevel.Warning;
        }

        // Suppress Entity Framework unless Error
        if (category.Contains("Microsoft.EntityFrameworkCore"))
        {
            return logLevel >= LogLevel.Error;
        }

        // Show Debug and above for Three2025 application logs
        if (category.Contains("Three2025"))
        {
            return logLevel >= LogLevel.Debug;
        }

        // Default: Information and above
        return logLevel >= LogLevel.Information;
    }
}
