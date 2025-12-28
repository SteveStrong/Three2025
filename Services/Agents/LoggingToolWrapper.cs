#nullable enable

using Microsoft.Extensions.AI;

namespace Three2025.Services.Agents;

/// <summary>
/// Tool wrapper utilities for logging and monitoring
/// Note: Individual tool methods (Shape3DTech, etc.) already include logging via WriteSuccess/WriteWarning
/// Those logs appear in console/server logs
/// </summary>
public static class LoggingToolWrapper
{
    public static List<AIFunction> WrapAllWithLogging(IEnumerable<AIFunction> tools, ILogger logger)
    {
        // Tools already have built-in logging via WriteSuccess/WriteWarning extensions
        // which output to console - those can be monitored server-side
        logger.LogInformation($"Registered {tools.Count()} tools with built-in logging");
        return tools.ToList();
    }
}

