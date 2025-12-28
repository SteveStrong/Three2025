using System.Text;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class ThreeDModelingAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<ThreeDModelingAgent> _logger;
    
    public string Name => "3D Modeling Agent";
    public string Description => "Expert in 3D modeling, Three.js, geometry creation, and spatial operations";
    
    public ThreeDModelingAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<ThreeDModelingAgent> logger)
    {
        _chatService = chatService;
        _tools = technicianTools.ToList();
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        // Relevant for pages dealing with 3D models, geometry, shapes
        var relevantKeywords = new[] { "geometry", "3d", "model", "shape", "mesh", "box", "sphere", "cage", "rack" };
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
            You are a 3D Modeling Expert specializing in Three.js and 3D geometry.
            
            Your expertise includes:
            - Creating and manipulating 3D shapes (boxes, spheres, cylinders, custom geometries)
            - Three.js scene setup and rendering
            - Spatial transformations (translate, rotate, scale)
            - Material properties and textures
            - Camera positioning and controls
            - Complex structures like cages and racks
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools for 3D operations
            
            Provide clear, actionable guidance for 3D modeling tasks. When appropriate, suggest specific tool calls.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        // Add recent conversation history (last 5 messages)
        messages.AddRange(conversationHistory.TakeLast(5));
        
        // Add current user message
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        // Get streaming response
        var response = new StringBuilder();
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, _tools, cancellationToken: cancellationToken))
        {
            response.Append(chunk);
        }
        
        _logger.LogInformation($"3D Modeling Agent processed request: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
        
        return response.ToString();
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$$"""
            You are a 3D Modeling Expert specializing in the FoundryWorldsAndDrawings Library with 2D/3D geometry.
            
            Your expertise includes:
            - you provide answers only by using the tools available to you
            - You only use the tools provided to you to perform 3D modeling tasks.
            - Creating and manipulating 3D shapes (boxes, spheres, cylinders, custom geometries)
            - Spatial transformations (translate, rotate, scale)
            - Material properties and textures
            - Camera positioning and controls
            - Complex structures like cages and racks
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            Available tools: {{{_tools.Count}}} technician tools for 3D operations
            
            Provide clear, actionable guidance for 3D modeling tasks. When appropriate, suggest specific tool calls.
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
        
        _logger.LogInformation($"3D Modeling Agent streamed response: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}...");
    }
}
