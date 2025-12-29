using System.Text;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class ClockAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<ClockAgent> _logger;
    
    public string Name => "Clock Agent";
    public string Description => "Expert in clock mechanisms, cuckoo clocks, and time-based animations";
    
    public ClockAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<ClockAgent> logger)
    {
        _chatService = chatService;
        _tools = technicianTools.ToList();
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        var relevantKeywords = new[] { "clock", "cuckoo", "time", "pendulum", "gear", "mechanism" };
        return relevantKeywords.Any(k => 
            context.PageName.Contains(k, StringComparison.OrdinalIgnoreCase) ||
            context.DomainFocus.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<string> ProcessAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$$"""
            You are a Clock Mechanism Expert specializing in clock design and time-based animations.
            
            Your expertise includes:
            - Clock face design and hand movements
            - Cuckoo clock mechanisms and animations
            - Pendulum motion and timing
            - Gear systems and mechanical movements
            - Time synchronization and display
            - Coordinated multi-part animations
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools including ClockTech and CuckooClockTech operations
            
            Provide clear guidance for creating clock mechanisms and time-based animations.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        _logger.LogInformation($"⏰ Clock Agent calling LLM with {_tools.Count} tools");
        var response = await _chatService.SendMessageAsync(
            userMessage, messages, _tools, cancellationToken: cancellationToken);
        
        _logger.LogInformation($"✅ Clock Agent response: {response.Substring(0, Math.Min(100, response.Length))}...");
        
        return response;
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$$"""
            You are a Clock Mechanism Expert specializing in clock design and time-based animations.
            
            Your expertise includes:
            - Clock face design and hand movements
            - Cuckoo clock mechanisms and animations
            - Pendulum motion and timing
            - Gear systems and mechanical movements
            - Time synchronization and display
            - Coordinated multi-part animations
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools including ClockTech and CuckooClockTech operations
            
            Provide clear guidance for creating clock mechanisms and time-based animations.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        _logger.LogInformation($"⏰ Clock Agent starting streaming for: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
        
        var chunkCount = 0;
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, _tools, cancellationToken: cancellationToken))
        {
            chunkCount++;
            yield return chunk;
        }
        
        _logger.LogInformation($"⏰ Clock Agent completed streaming: {chunkCount} chunks for '{userMessage.Substring(0, Math.Min(50, userMessage.Length))}...'");
    }
}
