using System.Text;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class ThreeDModelingAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<ThreeDModelingAgent> _logger;
    
    public string Name => "Shape3D Technician";
    public string Description => "Creates and manipulates 3D geometry using Shape3DTech tools. Provides answers by executing tool operations.";
    
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
            You are a Shape3D Technician with direct access to {{{_tools.Count}}} Shape3DTech tools.
            
            IMPORTANT: You provide answers by EXECUTING TOOLS, not by explaining what could be done.
            
            Your workflow:
            1. User asks to create/modify 3D geometry
            2. You immediately USE THE APPROPRIATE TOOLS to perform the action
            3. After tools execute, you briefly confirm what was created/modified
            
            Available Shape3DTech tools:
            - AddShape, AddShapeWithDimensions - Create new 3D geometry (box, sphere, cylinder, cone, etc.)
            - RepositionShape, RotateShape, ScaleShape - Transform existing geometry
            - ChangeColor, ChangeState - Modify geometry appearance and visibility
            - DuplicateShape, DeleteShape - Copy or remove geometry
            - GetShapes, GetShapeByName - Query existing geometry in the scene
            
            DO: Execute tools immediately when asked to create or modify shapes
            DON'T: Explain how to do something without actually doing it
            DON'T: Suggest manual steps - use the tools instead
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        // Add recent conversation history (last 5 messages)
        messages.AddRange(conversationHistory.TakeLast(5));
        
        // Add current user message
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        _logger.LogInformation($"🔧 Shape3D Technician calling LLM with {_tools.Count} tools");
        var response = await _chatService.SendMessageAsync(
            userMessage, messages, _tools, cancellationToken: cancellationToken);
        
        _logger.LogInformation($"✅ Shape3D Technician response: {response.Substring(0, Math.Min(100, response.Length))}...");
        
        return response;
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var systemPrompt = $$$"""
            You are a Shape3D Technician with direct access to {{{_tools.Count}}} Shape3DTech tools.
            
            IMPORTANT: You provide answers by EXECUTING TOOLS, not by explaining what could be done.
            
            Your workflow:
            1. User asks to create/modify 3D geometry
            2. You immediately USE THE APPROPRIATE TOOLS to perform the action
            3. After tools execute, you briefly confirm what was created/modified
            
            Available Shape3DTech tools:
            - AddShape, AddShapeWithDimensions - Create new 3D geometry (box, sphere, cylinder, cone, etc.)
            - RepositionShape, RotateShape, ScaleShape - Transform existing geometry
            - ChangeColor, ChangeState - Modify geometry appearance and visibility
            - DuplicateShape, DeleteShape - Copy or remove geometry
            - GetShapes, GetShapeByName - Query existing geometry in the scene
            
            DO: Execute tools immediately when asked to create or modify shapes
            DON'T: Explain how to do something without actually doing it
            DON'T: Suggest manual steps - use the tools instead
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
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
