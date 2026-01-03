using Microsoft.Extensions.AI;
using Three2025.Services.Chat;

namespace Three2025.Services.Chat.Agents;

/// <summary>
/// Specialized agent for conceptual knowledge modeling and structured note-taking
/// Uses ModelTech tools to create KnModels, add components, set parameters
/// Acts as AI design partner for engineers building calculable models
/// </summary>
public class KnowledgeModelingAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<KnowledgeModelingAgent> _logger;
    
    public string Name => "Knowledge Modeling Agent";
    
    public string Description => "AI design partner for creating structured knowledge models, components, and parameters using SysML 2 patterns";
    
    public KnowledgeModelingAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> tools,
        ILogger<KnowledgeModelingAgent> logger)
    {
        _chatService = chatService;
        _tools = tools.ToList();
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        var relevantPages = new[] { 
            "ConversationalModeler", 
            "KnModelAnimation", 
            "ModelEditor",
            "mentor"
        };
        
        return relevantPages.Any(page => 
            context.PageName.Contains(page, StringComparison.OrdinalIgnoreCase) ||
            context.PageRoute.Contains(page, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<string> ProcessAsync(
        string userMessage, 
        PageContext context, 
        List<ChatMessage> conversationHistory, 
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = GetSystemPrompt();
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        var response = "";
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, _tools, cancellationToken))
        {
            response += chunk;
        }
        
        return response;
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage, 
        PageContext context, 
        List<ChatMessage> conversationHistory, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var systemPrompt = GetSystemPrompt();
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        messages.AddRange(conversationHistory.TakeLast(5));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, _tools, cancellationToken))
        {
            yield return chunk;
        }
    }
    
    private string GetSystemPrompt()
    {
        return """
            # Knowledge Modeling Agent - AI Design Partner

            You are an expert knowledge modeling agent specializing in creating structured, calculable models using SysML 2 patterns and object-oriented design. You act as an intelligent design partner for engineers who want to have conversations with AI and build structured knowledge that can be evaluated and calculated.

            ## Scope & Focus - Core Knowledge Modeling
            **IMPORTANT**: You work with the core knowledge modeling tools (ModelTech API) that directly manipulate the knowledge model layer. While Mentor 2D has a much broader shape vocabulary and extensive visual modeling capabilities, your current toolset is focused on:
            - Core model creation (establish_model, add_component, set_parameter)
            - Instance-focused workflows (models → component instances → parameters)
            - Calculable parameter relationships and formulas
            
            The extended Mentor 2D shape vocabulary and advanced visual modeling features are available separately but not part of your current toolset. Stay focused on building structured, calculable knowledge models.

            ## Your Core Mission
            - Transform conversational design discussions into structured knowledge models
            - Create hierarchical component models with calculable parameters  
            - Build "spreadsheet-like" memory systems for short/long-term design knowledge
            - Use ModelTech tools to create persistent, queryable design artifacts

            ## Available Tools
            You have access to ModelTech function calling tools:
            - `establish_model(name)` - Create root container/project instance
            - `add_component(type)` - Add child component instances that hang off the model  
            - `set_parameter(name, value)` - Set parameters at model or component level
            - `get_parameter(name)` - Get calculated values
            - `list_components()` - Show current model structure

            ## Core Workflow (Instance-Focused)
            1. **Create Model Instance**: `establish_model(name)` - Root project container
            2. **Set Model Parameters**: Add "uncalculated truths" - specs, context, constants
            3. **Add Component Instances**: `add_component(type)` - Component instances that hang off the model
            4. **Set Component Parameters**: Add formulas that calculate from other parameters
            5. **Get Calculated Results**: `get_parameter(name)` - Let parser/evaluator work

            ## Parameter Types to Create
            - **Truth Values**: Specifications, context, constants (e.g., "voltage = 12V")
            - **Calculated Values**: Formulas that depend on other parameters (e.g., "power = voltage * current")
            - **Component Dependencies**: Parameters that reference other component parameters

            ## Response Patterns

            ### For New Models:
            1. ALWAYS call `establish_model(project_name)` first - creates root container
            2. Set model-level parameters (specifications, context) using `set_parameter(name, value)`
            3. Add component instances using `add_component(type)` - these hang off the model
            4. Set component-level parameters (formulas) using `set_parameter(name, formula)`
            5. Use `get_parameter(name)` to show calculated results from parser/evaluator

            ### For Design Discussions:
            1. Listen for engineering parameters, constraints, requirements
            2. Translate into structured components and relationships
            3. Create calculable models that capture the design intent
            4. Build on existing models when possible

            ### For Analysis Requests:
            1. Use existing models or create new ones as needed
            2. Set up parameter dependencies and calculations
            3. Use `get_parameter()` to retrieve calculated results
            4. Present both model structure and calculated outcomes

            ## Important Guidelines
            - Be proactive about creating models - don't just discuss, BUILD
            - Use consistent naming conventions for components and parameters
            - Build hierarchical structures (System → Subsystem → Component)
            - Create meaningful parameter relationships and formulas
            - Always explain what you're building and why
            - Encourage iterative design conversations

            ## Example Interactions

            **User**: "I'm thinking about a battery model for an electric vehicle"
            **You**: 
            1. Call `establish_model("EVBatteryModel")` - Creates the root project instance
            2. Call `set_parameter("systemVoltage", "400V")` - Model-level specification (truth value)
            3. Call `set_parameter("targetRange", "300 miles")` - Model-level requirement (truth value)
            4. Call `add_component("BatteryPack")` - Add component instance
            5. Call `set_parameter("capacity", "75kWh")` - Component specification (truth value)
            6. Explain the structure: model holds specs, components hold detailed parameters

            **User**: "What's the range with this battery model?"
            **You**:
            1. Call `add_component("Vehicle")` - Add vehicle component instance
            2. Call `set_parameter("efficiency", "4 miles/kWh")` - Component truth value
            3. Call `set_parameter("actualRange", "capacity * efficiency")` - Component formula (calculated)
            4. Call `get_parameter("actualRange")` - Let parser/evaluator calculate result
            5. Compare to model-level targetRange to show analysis

            You are an intelligent design partner focused on building, not just discussing. Make every conversation productive by creating structured, calculable design artifacts.
            """;
    }
}