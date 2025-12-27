using System.Text;
using Microsoft.Extensions.AI;
using Three2025.Services.Agents;

namespace Three2025.Services.Chat.Agents;

public class GeometryAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<GeometryAgent> _logger;
    
    public string Name => "Geometry Agent";
    public string Description => "Expert in 3D geometry, shapes, spatial positioning, and geometric transformations";
    
    public GeometryAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<GeometryAgent> logger)
    {
        _chatService = chatService;
        // Wrap tools with logging
        _tools = LoggingToolWrapper.WrapAllWithLogging(technicianTools, logger);
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        var relevantKeywords = new[] { "geometry", "shape", "box", "sphere", "cylinder", "mesh", "position", "transform", "3d", "spatial" };
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
            You are a Geometry Expert specializing in 3D scene illumination and visual effects.
            
            Your expertise includes:
            - Managing scene Geometry (ambient, directional, point, spot lights)
            - Light positioning and intensity control
            - Color temperature and Geometry moods
            - Shadow configuration
            - Light state management (on/off)
            - Saving and restoring Geometry configurations
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools including GeometryTech operations
            
            You have direct access to Geometry tools like:
            - GetLights, AddLight, DeleteLight
            - RepositionLight, ChangeState, ChangeColor
            - SaveLights, RestoreLights, PickARandomColor
            
            Provide clear guidance for creating effective Geometry setups. Use the Geometry tools when appropriate.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        var response = new StringBuilder();
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, _tools, cancellationToken: cancellationToken))
        {
            response.Append(chunk);
        }
        
        _logger.LogInformation($"Geometry Agent processed request: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
        
        return response.ToString();
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$$"""
            You are a Geometry Expert specializing in 3D scene illumination and visual effects.
            
            Your expertise includes:
            - Managing scene Geometry (ambient, directional, point, spot lights)
            - Light positioning and intensity control
            - Color temperature and Geometry moods
            - Shadow configuration
            - Light state management (on/off)
            - Saving and restoring Geometry configurations
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools including GeometryTech operations
            
            You have direct access to Geometry tools like:
            - GetLights, AddLight, DeleteLight
            - RepositionLight, ChangeState, ChangeColor
            - SaveLights, RestoreLights, PickARandomColor
            
            Provide clear guidance for creating effective Geometry setups. Use the Geometry tools when appropriate.
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
        
        _logger.LogInformation($"Geometry Agent streamed response: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
    }
}
