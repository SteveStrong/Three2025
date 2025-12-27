using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

#nullable enable

/// <summary>
/// Orchestrates chat interactions across multiple specialized agents.
/// Acts as the "router" that determines which agent(s) to call based on user intent.
/// </summary>
public interface IChatOrchestrator
{
    /// <summary>
    /// Process a user message, route to appropriate agent(s), and return aggregated response
    /// </summary>
    Task<AgentResponse> ProcessMessageAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process a user message with streaming, yielding response chunks as they arrive
    /// </summary>
    IAsyncEnumerable<StreamingChunk> ProcessMessageStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get list of agents available for a given page context
    /// </summary>
    List<string> GetAvailableAgents(PageContext context);
    
    /// <summary>
    /// Register a new specialized agent
    /// </summary>
    void RegisterAgent(ISpecializedAgent agent);
    
    /// <summary>
    /// Get count of available tools
    /// </summary>
    int GetToolCount();
    
    /// <summary>
    /// Get all available tools for inspection
    /// </summary>
    IEnumerable<AIFunction> GetAllTools();
}

public class AgentResponse
{
    public string Content { get; set; } = "";
    public string AgentName { get; set; } = "";
    public List<string> AgentsInvolved { get; set; } = new();
    public Dictionary<string, object>? ActionableData { get; set; }
}

public class StreamingChunk
{
    public string Content { get; set; } = "";
    public string AgentName { get; set; } = "";
    public bool IsComplete { get; set; } = false;
    public List<string> AgentsInvolved { get; set; } = new();
}

public class PageContext
{
    public string PageName { get; set; } = "";
    public string PageRoute { get; set; } = "";
    public Dictionary<string, object> PageState { get; set; } = new();
    public List<string> AvailableAgents { get; set; } = new();
    public string DomainFocus { get; set; } = "General";
}
