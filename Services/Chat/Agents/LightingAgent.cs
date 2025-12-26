using System.Text;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class LightingAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<LightingAgent> _logger;
    
    public string Name => "Lighting Agent";
    public string Description => "Expert in 3D lighting, illumination, shadows, and visual effects";
    
    public LightingAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<LightingAgent> logger)
    {
        _chatService = chatService;
        _tools = technicianTools.ToList();
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        var relevantKeywords = new[] { "light", "lighting", "shadow", "illumination", "lamp", "ambient", "directional" };
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
            You are a Lighting Expert specializing in 3D scene illumination and visual effects.
            
            Your expertise includes:
            - Managing scene lighting (ambient, directional, point, spot lights)
            - Light positioning and intensity control
            - Color temperature and lighting moods
            - Shadow configuration
            - Light state management (on/off)
            - Saving and restoring lighting configurations
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools including LightingTech operations
            
            You have direct access to lighting tools like:
            - GetLights, AddLight, DeleteLight
            - RepositionLight, ChangeState, ChangeColor
            - SaveLights, RestoreLights, PickARandomColor
            
            Provide clear guidance for creating effective lighting setups. Use the lighting tools when appropriate.
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
        
        _logger.LogInformation($"Lighting Agent processed request: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
        
        return response.ToString();
    }
}
