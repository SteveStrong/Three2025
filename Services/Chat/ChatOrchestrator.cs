using System.Text.Json;
using Microsoft.Extensions.AI;
using Three2025.Services.Agents;

namespace Three2025.Services.Chat;

#nullable enable

public class ChatOrchestrator : IChatOrchestrator
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ITechnicianToolProvider _toolProvider;
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<ChatOrchestrator> _logger;
    
    private readonly Dictionary<string, ISpecializedAgent> _agents = new();
    private readonly List<Microsoft.Extensions.AI.AIFunction> _technicianTools = new();
    
    public ChatOrchestrator(
        IMultiProviderChatService chatService,
        ITechnicianToolProvider toolProvider,
        IAgentFactory agentFactory,
        ILogger<ChatOrchestrator> logger)
    {
        _chatService = chatService;
        _toolProvider = toolProvider;
        _agentFactory = agentFactory;
        _logger = logger;
        
        // Discover all technician tools
        _technicianTools.AddRange(_toolProvider.DiscoverAllTools());
        _logger.LogInformation($"Loaded {_technicianTools.Count} technician tools");
        
        InitializeAgents();
    }
    
    private void InitializeAgents()
    {
        _logger.LogInformation("🔧 Starting agent initialization...");
        
        // Create specialized agents WITH technician tools
        var agent1 = _agentFactory.Create3DModelingAgent(_technicianTools);
        RegisterAgent(agent1);
        
        var agent2 = _agentFactory.CreateAnimationAgent(_technicianTools);
        RegisterAgent(agent2);
        
        var agent3 = _agentFactory.CreateGeometryAgent(_technicianTools);
        RegisterAgent(agent3);
        
        var agent4 = _agentFactory.CreateClockAgent(_technicianTools);
        RegisterAgent(agent4);
        
        var agent5 = _agentFactory.CreateGeneralAgent(_technicianTools);
        RegisterAgent(agent5);
        
        _logger.LogInformation($"✅ Initialized {_agents.Count} specialized agents: {string.Join(", ", _agents.Keys)}");
    }
    
    public void RegisterAgent(ISpecializedAgent agent)
    {
        if (agent == null)
        {
            _logger.LogError("❌ Attempted to register null agent!");
            return;
        }
        
        if (string.IsNullOrEmpty(agent.Name))
        {
            _logger.LogError("❌ Attempted to register agent with null/empty name!");
            return;
        }
        
        _agents[agent.Name] = agent;
        _logger.LogInformation($"✅ Registered agent: {agent.Name} (Total: {_agents.Count})");
    }
    
    public List<string> GetAvailableAgents(PageContext context)
    {
        return _agents.Values
            .Where(a => a.IsRelevantForContext(context))
            .Select(a => a.Name)
            .ToList();
    }
    
    public int GetToolCount() => _technicianTools.Count;
    
    public IEnumerable<AIFunction> GetAllTools() => _technicianTools;
    
    public async Task<AgentResponse> ProcessMessageAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch = null,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Analyze user intent and determine which agent(s) to call
        var intent = await AnalyzeIntentAsync(userMessage, context, conversationHistory);
        
        _logger.LogInformation($"Intent analysis: {intent.PrimaryAction}, Agent: {intent.PrimaryAgent}, Confidence: {intent.Confidence}");
        
        // Step 2: Route to appropriate agent
        return await ExecuteSingleAgentAsync(
            userMessage, 
            context, 
            intent.PrimaryAgent, 
            conversationHistory,
            onAgentSwitch,
            cancellationToken);
    }
    
    public async IAsyncEnumerable<StreamingChunk> ProcessMessageStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Step 1: Analyze user intent and determine which agent(s) to call
        var intent = await AnalyzeIntentAsync(userMessage, context, conversationHistory);
        
        _logger.LogInformation($"Intent analysis: {intent.PrimaryAction}, Agent: {intent.PrimaryAgent}, Confidence: {intent.Confidence}");
        
        // Step 2: Route to appropriate agent and stream response
        if (!_agents.TryGetValue(intent.PrimaryAgent, out var agent))
        {
            agent = _agents.Values.First(); // Fallback to first agent
            _logger.LogWarning($"Agent '{intent.PrimaryAgent}' not found, using fallback: {agent.Name}");
        }
        
        onAgentSwitch?.Invoke(intent.PrimaryAgent);
        
        await foreach (var chunk in agent.ProcessStreamingAsync(
            userMessage, 
            context, 
            conversationHistory, 
            cancellationToken))
        {
            yield return new StreamingChunk
            {
                Content = chunk,
                AgentName = intent.PrimaryAgent,
                IsComplete = false,
                AgentsInvolved = new List<string> { intent.PrimaryAgent }
            };
        }
        
        // Final chunk to indicate completion
        yield return new StreamingChunk
        {
            Content = "",
            AgentName = intent.PrimaryAgent,
            IsComplete = true,
            AgentsInvolved = new List<string> { intent.PrimaryAgent }
        };
    }
    
    private async Task<IntentAnalysis> AnalyzeIntentAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory)
    {
        // Use the coordinator to analyze intent
        var availableAgents = GetAvailableAgents(context);
        var agentDescriptions = string.Join("\n", 
            availableAgents.Select(a => $"- {a}: {_agents[a].Description}"));
        
        var coordinatorPrompt = $$"""
            You are a routing coordinator for a multi-agent system. Analyze the user's message and determine:
            1. What the user wants to accomplish
            2. Which specialized agent should handle this request
            
            Available agents for this context ({{context.PageName}}):
            {{agentDescriptions}}
            
            User message: {{userMessage}}
            
            Respond with JSON only (no markdown):
            {
                "primaryAgent": "AgentName",
                "primaryAction": "brief description",
                "confidence": 0.95
            }
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, coordinatorPrompt),
            new(ChatRole.User, userMessage)
        };
        
        var response = "";
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, cancellationToken: default))
        {
            response += chunk;
        }
        
        // Parse JSON response
        try
        {
            // Clean markdown code blocks if present
            response = response.Trim();
            if (response.StartsWith("```json"))
                response = response[7..];
            if (response.StartsWith("```"))
                response = response[3..];
            if (response.EndsWith("```"))
                response = response[..^3];
            response = response.Trim();
            
            var intent = JsonSerializer.Deserialize<IntentAnalysis>(response, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            // Validate agent exists
            if (intent != null && !_agents.ContainsKey(intent.PrimaryAgent))
            {
                // Fallback to first available agent
                intent.PrimaryAgent = availableAgents.FirstOrDefault() ?? "General Agent";
            }
            
            return intent ?? new IntentAnalysis 
            { 
                PrimaryAgent = availableAgents.FirstOrDefault() ?? "General Agent", 
                Confidence = 0.5,
                PrimaryAction = "Fallback routing"
            };
        }
        catch (JsonException ex)
        {
            _logger.LogWarning($"Failed to parse intent JSON: {ex.Message}. Response: {response}");
            return new IntentAnalysis 
            { 
                PrimaryAgent = availableAgents.FirstOrDefault() ?? "General Agent", 
                Confidence = 0.3,
                PrimaryAction = "Error fallback"
            };
        }
    }
    
    private async Task<AgentResponse> ExecuteSingleAgentAsync(
        string userMessage,
        PageContext context,
        string agentName,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch,
        CancellationToken cancellationToken)
    {
        if (!_agents.TryGetValue(agentName, out var agent))
        {
            agent = _agents.Values.First(); // Fallback to first agent
            _logger.LogWarning($"Agent '{agentName}' not found, using fallback: {agent.Name}");
        }
        
        onAgentSwitch?.Invoke(agentName);
        
        var response = await agent.ProcessAsync(
            userMessage, 
            context, 
            conversationHistory, 
            cancellationToken);
        
        return new AgentResponse
        {
            Content = response,
            AgentName = agentName,
            AgentsInvolved = new List<string> { agentName }
        };
    }
}

internal class IntentAnalysis
{
    public string PrimaryAgent { get; set; } = "";
    public string PrimaryAction { get; set; } = "";
    public double Confidence { get; set; }
}
