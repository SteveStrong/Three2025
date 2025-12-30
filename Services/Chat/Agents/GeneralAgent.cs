using System.Text;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class GeneralAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<GeneralAgent> _logger;
    
    public string Name => "General Agent";
    public string Description => "General-purpose assistant for questions and guidance across all domains";
    
    public GeneralAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<GeneralAgent> logger)
    {
        _chatService = chatService;
        _tools = technicianTools.ToList();
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        // General agent is always relevant as fallback
        return true;
    }
    
    public async Task<string> ProcessAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$$"""
            You are a helpful AI assistant for the Three2025 framework application.
            
            You can help with:
            - General questions about the application
            - Navigation and feature discovery
            - Explaining concepts and capabilities
            - Directing users to appropriate pages and features
            - Answering questions about 3D graphics, animations, and modeling
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools across multiple domains
            
            Be friendly, helpful, and concise. If the user's question is domain-specific, provide relevant information.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        _logger.LogInformation($"🤖 General Agent calling LLM with {_tools.Count} tools");
        var response = await _chatService.SendMessageAsync(
            userMessage, messages, _tools, cancellationToken: cancellationToken);
        
        _logger.LogInformation($"✅ General Agent response: {response.Substring(0, Math.Min(100, response.Length))}...");
        
        return response;
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$$"""
            You are a helpful AI assistant for the Three2025 framework application.
            
            You can help with:
            - General questions about the application
            - Navigation and feature discovery
            - Explaining concepts and capabilities
            - Directing users to appropriate pages and features
            - Answering questions about 3D graphics, animations, and modeling
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools across multiple domains
            
            Be friendly, helpful, and concise. If the user's question is domain-specific, provide relevant information.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, _tools, cancellationToken: cancellationToken))
        {
            yield return chunk;
        }
        
        _logger.LogInformation($"General Agent streamed response: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
    }
}
