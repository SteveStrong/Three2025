using System.Text;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class AnimationAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<AnimationAgent> _logger;
    
    public string Name => "Animation Agent";
    public string Description => "Expert in animations, tweens, timelines, and motion control";
    
    public AnimationAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<AnimationAgent> logger)
    {
        _chatService = chatService;
        _tools = technicianTools.ToList();
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        var relevantKeywords = new[] { "animation", "tween", "motion", "timeline", "clock", "cuckoo" };
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
            You are an Animation Expert specializing in motion, tweening, and timeline control.
            
            Your expertise includes:
            - Animation sequences and keyframes
            - Tween animations (position, rotation, scale, color)
            - Timeline control and synchronization
            - Easing functions and motion curves
            - Loop animations and reversible sequences
            - Clock mechanisms and mechanical animations
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools for animation operations
            
            Provide clear guidance for creating smooth, engaging animations. Suggest specific timing and easing strategies.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        var response = new StringBuilder();
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, cancellationToken: cancellationToken))
        {
            response.Append(chunk);
        }
        
        _logger.LogInformation($"Animation Agent processed request: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
        
        return response.ToString();
    }
}
