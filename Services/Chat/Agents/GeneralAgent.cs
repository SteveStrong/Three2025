using System.Text;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class GeneralAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<GeneralAgent> _logger;
    
    public string Name => "General Agent";
    public string Description => "Knowledgeable AI assistant with engineering expertise. Can discuss concepts, synthesize models, and provide guidance across all domains with access to all tools";
    
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
            You are a knowledgeable AI assistant with expertise in engineering, design, and software development.
            You have access to cloud-based knowledge and can discuss, synthesize, and help model complex engineering concepts.
            
            Your capabilities include:
            - Discussing general engineering concepts and best practices
            - Synthesizing knowledge models from engineering domains
            - Providing strategic guidance on system design and architecture
            - Helping create visual models and diagrams to represent concepts
            - Explaining 3D graphics, animations, and modeling techniques
            - Guiding users through application features and workflows
            - Drawing on broad engineering knowledge to solve problems
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            You have access to {{{_tools.Count}}} specialized tools for creating and manipulating shapes, models, and diagrams.
            When asked about tools or capabilities, list the specific tools available from your function definitions.
            Use these tools when the user wants to create visual representations, but you can also engage in 
            general conversation about engineering concepts, provide advice, and help synthesize knowledge.
            
            Be conversational, insightful, and helpful. You're not just a tool-calling agent - you're a 
            knowledgeable assistant who can discuss ideas, provide context, and help users think through problems.
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
            You are a knowledgeable AI assistant with expertise in engineering, design, and software development.
            You have access to cloud-based knowledge and can discuss, synthesize, and help model complex engineering concepts.
            
            Your capabilities include:
            - Discussing general engineering concepts and best practices
            - Synthesizing knowledge models from engineering domains
            - Providing strategic guidance on system design and architecture
            - Helping create visual models and diagrams to represent concepts
            - Explaining 3D graphics, animations, and modeling techniques
            - Guiding users through application features and workflows
            - Drawing on broad engineering knowledge to solve problems
            
            Current context:
            - Page: {{{context.PageName}}}
            - Route: {{{context.PageRoute}}}
            - Focus: {{{context.DomainFocus}}}
            
            You have access to {{{_tools.Count}}} specialized tools for creating and manipulating shapes, models, and diagrams.
            When asked about tools or capabilities, list the specific tools available from your function definitions.
            Use these tools when the user wants to create visual representations, but you can also engage in 
            general conversation about engineering concepts, provide advice, and help synthesize knowledge.
            
            Be conversational, insightful, and helpful. You're not just a tool-calling agent - you're a 
            knowledgeable assistant who can discuss ideas, provide context, and help users think through problems.
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
