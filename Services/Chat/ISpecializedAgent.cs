using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

/// <summary>
/// Base interface for all specialized agents in the multi-agent system
/// </summary>
public interface ISpecializedAgent
{
    /// <summary>
    /// Unique name of this agent (e.g., "3D Modeling Agent")
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Brief description of what this agent does
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Check if this agent is relevant for the given page context
    /// </summary>
    bool IsRelevantForContext(PageContext context);
    
    /// <summary>
    /// Process a user message and return a response
    /// </summary>
    Task<string> ProcessAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process a user message with streaming, yielding response chunks as they arrive
    /// </summary>
    IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);
}
