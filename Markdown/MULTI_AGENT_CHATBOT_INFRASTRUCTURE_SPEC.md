# Multi-Agent Chatbot Infrastructure Specification

**Project**: Three2025 Framework  
**Date**: December 26, 2025  
**Purpose**: Design and implement reusable multi-agent chatbot infrastructure for integration across Razor pages

---

## 1. Executive Summary

This specification outlines the architecture for embedding intelligent, multi-agent chatbots throughout the Three2025 application. The goal is to create a modular, reusable system where chatbots can be easily added to any Razor page, with specialized agents that understand the context of the page they're embedded in and can orchestrate calls to other agents to solve complex problems.

### Key Objectives

1. **Reusable Chat Component** - Single Razor component that can be embedded anywhere
2. **Multi-Agent Orchestration** - Primary agent that understands when to call specialized agents
3. **Context-Aware Agents** - Agents that understand the specific domain (3D modeling, SysML, animation, etc.)
4. **Service Injection** - Clean dependency injection pattern for chat services
5. **Visual Integration** - Consistent UI patterns that work across all pages

---

## 2. Current State Analysis

### 2.1 Existing Infrastructure

**Radzen Components (Already Integrated):**
- ✅ `RadzenAIChat` - Production-ready chat UI component
- ✅ Full Radzen component library available
- ✅ Consistent theming across application
- ✅ Event-driven architecture (`MessageSent`, `ResponseReceived`, etc.)

**Chat Services (Existing):**
- `IMultiProviderChatService` - Manages multiple AI providers (GitHub Models, AWS Bedrock)
- `IChatProvider` - Abstraction for chat providers
- `MultiProviderChatService` - Implementation with streaming support
- `GitHubModelProvider` & `BedrockProvider` - Concrete provider implementations

**Legacy Chat Pages (Can be deprecated):**
- `AIChat.razor` - Legacy simple chat UI
- `AIChatMulti.razor` - Multi-provider chat with logging
- `AIChatPage.razor` - Full-page chat interface

**Agent Framework Re (What We Need to Build)

1. ✅ **UI Foundation** - RadzenAIChat provides this (we just need to wrap it)
2. ❌ **Agent Orchestration** - No coordinator agent to route between specialists
3. ❌ **Context Awareness** - Agents don't know which page they're embedded in
4. ❌ **Domain-Specific Agents** - No agents for 3D modeling, animation, LEGO snapping, etc.
5. ❌ **Chat History Persistence** - No way to save/restore conversations per page
6. ❌ **Page Integration Pattern** - No standard way to add chat to existing pages
7. ❌ **Actionable Responses** - Agents can't trigger page actions (create shapes, run animations)
1. **No Reusable Embedded Component** - Current chat UIs are full-page only
2. **No Agent Orchestration** - No coordinator agent to route between specialists
3. **No Context Awareness** - Agents don't know which page they're embedded in
4. **No Domain-Specific Agents** - No agents for 3D modeling, animation, LEGO snapping, etc.
5. **No Chat History Persistence** - No way to save/restore conversations
6. **No Page Integration Pattern** - No standard way to add chat to existing pages

---

## 3. Proposed Architecture

### 3.1 Component Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│                  Any Razor Page                             │
│  (GeometryTest, KnModelAnimation, LegoSnapping, etc.)      │
│                                                              │
│  ┌────────────────────────────────────────────────────┐    │
│  │         <AgentChatPanel>                           │    │
│  │  ┌──────────────────────────────────────────────┐  │    │
│  │  │         RadzenAIChat (UI Layer)              │  │    │
│  │  │  • Message display & input                   │  │    │
│  │  │  • Events: MessageSent, ResponseReceived     │  │    │
│  │  │  • Built-in Markdown, history, theming       │  │    │
│  │  └──────────────────────────────────────────────┘  │    │
│  │                     ↓                               │    │
│  │  ┌──────────────────────────────────────────────┐  │    │
│  │  │    ChatOrchestrator Service                  │  │    │
│  │  │  • Analyzes user intent                      │  │    │
│  │  │  • Routes to specialized agents              │  │    │
│  │  │  • Aggregates responses                      │  │    │
│  │  └──────────────────────────────────────────────┘  │    │
│  │                     │                               │    │
│  │          ┌──────────┼──────────┐                    │    │
│  │          │          │          │                    │    │
│  │     ┌────▼────┐ ┌──▼────┐ ┌───▼─────┐              │    │
│  │     │ 3D      │ │ SysML │ │ Animation│             │    │
│  │     │ Agent   │ │ Agent │ │ Agent    │             │    │
│  │     └─────────┘ └───────┘ └──────────┘             │    │
│  └────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

### 3.2 Service Layer Architecture

```
Program.cs Registration:
├── IMultiProviderChatService (existing)
├── IChatOrchestrator (new)
│   └── Routes between specialized agents
├── IAgentFactory (new)
│   └── Creates specialized agents
├── IChatHistoryService (new)
│   └── Persists chat sessions
└── Specialized Agents (new)
    ├── I3DModelingAgent
    ├── ISysMLAgent
    ├── IAnimationAgent
    ├── IGeometryAgent
    └── IVisualizationAgent
```

### 3.3 Page Context System

Each chat component needs to know its context:

```csharp
public class PageContext
{
    public string PageName { get; set; }
    public string PageRoute { get; set; }
    public Dictionary<string, object> PageState { get; set; }
    public List<string> AvailableAgents { get; set; }
    public string DomainFocus { get; set; } // "3D", "Animation", "SysML", etc.
}
```

---

## 4. Detailed Component Design

### 4.1 AgentChatPanel Component (RadzenAIChat Wrapper)

**File**: `Components/Shared/AgentChatPanel.razor`

```razor
@namespace Three2025.Components.Shared
@inherits AgentChatPanelBase
@rendermode InteractiveServer

<style>
    .agent-chat-container {
        position: fixed;
        right: 20px;
        bottom: 20px;
        width: 450px;
        z-index: 1000;
        transition: all 0.3s ease;
    }
    
    .agent-chat-container.minimized {
        width: auto;
    }
    
    .agent-status-bar {
        margin-bottom: 8px;
    }
    
    .agent-indicator {
        display: inline-flex;
        align-items: center;
        gap: 4px;
        font-size: 0.85em;
    }
</style>

<div class="agent-chat-container @(IsMinimized ? "minimized" : "")">
    @if (!IsMinimized)
    {
        <!-- Agent Status Indicator -->
        <RadzenCard class="agent-status-bar" Variant="Variant.Filled" Style="padding: 8px;">
            <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="0.5rem" JustifyContent="JustifyContent.SpaceBetween">
                <RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="0.5rem">
                    <span class="agent-indicator">
                        <RadzenIcon Icon="smart_toy" />
                        <strong>@CurrentAgentName</strong>
                    </span>
                    @if (IsProcessing)
                    {
                        <RadzenProgressBarCircular ShowValue="false" Size="ProgressBarCircularSize.Small" 
                                                   ProgressBarStyle="ProgressBarStyle.Primary" />
                    }
                </RadzenStack>
                
                <!-- Available Agents -->
                <RadzenStack Orientation="Orientation.Horizontal" Gap="0.25rem">
                    @foreach (var agent in AvailableAgents.Take(4))
                    {
                        <RadzenBadge BadgeStyle="@(agent == CurrentAgentName ? BadgeStyle.Success : BadgeStyle.Light)" 
                                     Text="@GetAgentInitial(agent)" 
                                     Title="@agent"
                                     Style="cursor: help;" />
                    }
                </RadzenStack>
                
                <RadzenButton Icon="minimize" 
                              ButtonStyle="ButtonStyle.Light" 
                              Size="ButtonSize.Small"
                              Click="ToggleMinimize"
                              Title="Minimize" />
            </RadzenStack>
        </RadzenCard>

        <!-- RadzenAIChat Component -->
        <RadzenAIChat @ref="chatControl"
                      Title="@($"{PageContext.DomainFocus} Assistant")"
                      Placeholder="@GetContextualPlaceholder()"
                      Style="height: 500px;"
                      MessageSent="@OnMessageSent"
                      MessageAdded="@OnMessageAdded"
                      ResponseReceived="@OnResponseReceived"
                      ChatCleared="@OnChatCleared" />
    }
    else
    {
        <!-- Minimized State -->
        <RadzenButton Text="💬 AI Assistant" 
                      Icon="chat"
                      Click="ToggleMinimize"
                      ButtonStyle="ButtonStyle.Primary"
                      Size="ButtonSize.Medium"
                      Style="box-shadow: 0 4px 12px rgba(0,0,0,0.15);" />
    }
</div>
```

**File**: `Components/Shared/AgentChatPanel.razor.cs`

```csharp
namespace Three2025.Components.Shared;

public class AgentChatPanelBase : ComponentBase
{
    [Inject] private IChatOrchestrator Orchestrator { get; set; } = null!;
    [Inject] private IChatHistoryService HistoryService { get; set; } = null!;
    [Inject] private ILogger<AgentChatPanelBase> Logger { get; set; } = null!;
    
    [Parameter] public PageContext PageContext { get; set; } = new();
    [Parameter] public EventCallback<AgentResponse> OnAgentResponse { get; set; }
    
    protected RadzenAIChat? chatControl;
    protected string CurrentAgentName { get; set; } = "Coordinator";
    protected bool IsMinimized { get; set; } = false;
    protected bool IsProcessing { get; set; } = false;
    protected List<string> AvailableAgents => PageContext.AvailableAgents;
    
    protected override async Task OnInitializedAsync()
    {
        // Load chat history for this page context
        var history = await HistoryService.LoadHistoryAsync(PageContext.PageName);
        
        // Pre-populate RadzenAIChat with history after render
        await Task.Delay(100); // Give RadzenAIChat time to initialize
        
        foreach (var msg in history)
        {
            await chatControl?.AddMessage(msg.Content, msg.Role == "user");
        }
    }
    
    protected void ToggleMinimize()
    {
        IsMinimized = !IsMinimized;
    }
    
    protected string GetContextualPlaceholder()
    {
        return PageContext.DomainFocus switch
        {
            "3D" => "Ask about geometries, meshes, or 3D modeling...",
            "Animation" => "Ask about animations, timing, or motion...",
            "SysML" => "Ask about system diagrams or requirements...",
            _ => $"Ask me about {PageContext.DomainFocus.ToLower()}..."
        };
    }
    
    protected string GetAgentInitial(string agentName)
    {
        var parts = agentName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 
            ? $"{parts[0][0]}{parts[1][0]}" 
            : agentName.Substring(0, Math.Min(2, agentName.Length));
    }
    
    protected async Task OnMessageSent(string message)
    {
        Logger.LogInformation($"User message: {message}");
        IsProcessing = true;
        StateHasChanged();
        
        try
        {
            // Get current conversation history from RadzenAIChat
            var conversationHistory = GetChatHistory();
            
            // Orchestrator analyzes and routes to appropriate agent(s)
            var response = await Orchestrator.ProcessMessageAsync(
                message, 
                PageContext, 
                conversationHistory,
                onAgentSwitch: (agentName) => 
                {
                    CurrentAgentName = agentName;
                    InvokeAsync(StateHasChanged);
                });
            
            // RadzenAIChat automatically handles the response via ResponseReceived event
            // But we can manually add it if needed:
            // await chatControl.AddMessage(response.Content, isUser: false);
            
            // Save updated history
            await HistoryService.SaveHistoryAsync(PageContext.PageName, GetChatHistory());
            
            // Notify parent page if callback provided
            if (OnAgentResponse.HasDelegate)
            {
                await OnAgentResponse.InvokeAsync(response);
            }
            
            Logger.LogInformation($"Response from {response.AgentName}: {response.Content.Substring(0, Math.Min(50, response.Content.Length))}...");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
            await chatControl?.AddMessage($"❌ Error: {ex.Message}", isUser: false);
        }
        finally
        {
            IsProcessing = false;
            CurrentAgentName = "Coordinator";
            StateHasChanged();
        }
    }
    
    protected void OnMessageAdded(Radzen.Blazor.ChatMessage message)
    {
        Logger.LogDebug($"Message added: {(message.IsUser ? "User" : "Assistant")}");
    }
    
    protected void OnResponseReceived(string response)
    {
        Logger.LogInformation($"Response received: {response.Substring(0, Math.Min(50, response.Length))}...");
    }
    
    protected async Task OnChatCleared()
    {
        await HistoryService.ClearHistoryAsync(PageContext.PageName);
        CurrentAgentName = "Coordinator";
        Logger.LogInformation("Chat cleared");
    }
    
    private List<ChatMessage> GetChatHistory()
    {
        if (chatControl?.Messages == null) return new List<ChatMessage>();
        
        return chatControl.Messages
            .Select(m => new ChatMessage 
            { 
                Role = m.IsUser ? "user" : "assistant", 
                Content = m.Content,
                Timestamp = m.Timestamp ?? DateTime.Now
            })
            .ToList();
    }
}
```

### 4.2 Chat Orchestrator Service

**File**: `Services/Chat/IChatOrchestrator.cs`

```csharp
namespace Three2025.Services.Chat;

/// <summary>
/// Orchestrates chat interactions across multiple specialized agents.
/// Acts as the "router" that determines which agent(s) to call based on user intent.
/// </summary>
public interface IChatOrchestrator
{
    /// <summary>
    /// Process a user message, route to appropriate agent(s), and return aggregated response
    /// </summary>
    Task<AgentResponse> ProcessMessageAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get list of agents available for a given page context
    /// </summary>
    List<string> GetAvailableAgents(PageContext context);
    
    /// <summary>
    /// Register a new specialized agent
    /// </summary>
    void RegisterAgent(ISpecializedAgent agent);
}

public class AgentResponse
{
    public string Content { get; set; } = "";
    public string AgentName { get; set; } = "";
    public List<string> AgentsInvolved { get; set; } = new();
    public Dictionary<string, object>? ActionableData { get; set; }
}

public class PageContext
{
    public string PageName { get; set; } = "";
    public string PageRoute { get; set; } = "";
    public Dictionary<string, object> PageState { get; set; } = new();
    public List<string> AvailableAgents { get; set; } = new();
    public string DomainFocus { get; set; } = "General";
}
```

**File**: `Services/Chat/ChatOrchestrator.cs`

```csharp
namespace Three2025.Services.Chat;

public class ChatOrchestrator : IChatOrchestrator
{
    private readonly IMultiProviderChatService _chatService;
    private readonly IAgentFactory _agentFactory;
    private readonly Dictionary<string, ISpecializedAgent> _agents = new();
    private readonly ILogger<ChatOrchestrator> _logger;
    
    public ChatOrchestrator(
        IMultiProviderChatService chatService,
        IAgentFactory agentFactory,
        ILogger<ChatOrchestrator> logger)
    {
        _chatService = chatService;
        _agentFactory = agentFactory;
        _logger = logger;
        
        // Initialize specialized agents
        InitializeAgents();
    }
    
    private void InitializeAgents()
    {
        // Register all specialized agents
        RegisterAgent(_agentFactory.Create3DModelingAgent());
        RegisterAgent(_agentFactory.CreateSysMLAgent());
        RegisterAgent(_agentFactory.CreateAnimationAgent());
        RegisterAgent(_agentFactory.CreateGeometryAgent());
        RegisterAgent(_agentFactory.CreateVisualizationAgent());
        RegisterAgent(_agentFactory.CreateParameterAgent());
    }
    
    public void RegisterAgent(ISpecializedAgent agent)
    {
        _agents[agent.Name] = agent;
        _logger.LogInformation($"Registered agent: {agent.Name}");
    }
    
    public List<string> GetAvailableAgents(PageContext context)
    {
        return _agents.Values
            .Where(a => a.IsRelevantForContext(context))
            .Select(a => a.Name)
            .ToList();
    }
    
    public async Task<AgentResponse> ProcessMessageAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch = null,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Analyze user intent and determine which agent(s) to call
        var intent = await AnalyzeIntentAsync(userMessage, context, conversationHistory);
        
        _logger.LogInformation($"Intent analysis: {intent.PrimaryAction}, Confidence: {intent.Confidence}");
        
        // Step 2: Route to appropriate agent(s)
        if (intent.RequiresMultipleAgents)
        {
            return await ExecuteMultiAgentWorkflowAsync(
                userMessage, 
                context, 
                intent, 
                conversationHistory,
                onAgentSwitch,
                cancellationToken);
        }
        else
        {
            return await ExecuteSingleAgentAsync(
                userMessage, 
                context, 
                intent.PrimaryAgent, 
                conversationHistory,
                onAgentSwitch,
                cancellationToken);
        }
    }
    
    private async Task<IntentAnalysis> AnalyzeIntentAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory)
    {
        // Use the coordinator agent to analyze intent
        var coordinatorPrompt = $$$"""
            You are a routing coordinator for a multi-agent system. Analyze the user's message and determine:
            1. What the user wants to accomplish
            2. Which specialized agent(s) should handle this request
            3. Whether multiple agents need to collaborate
            
            Available agents for this context ({{{context.PageName}}}):
            {{{string.Join("\n", GetAvailableAgents(context).Select(a => $"- {a}: {_agents[a].Description}"))}}}
            
            User message: {{{userMessage}}}
            
            Respond with JSON:
            {
                "primaryAgent": "AgentName",
                "requiresMultipleAgents": false,
                "additionalAgents": [],
                "primaryAction": "brief description",
                "confidence": 0.95
            }
            """;
        
        var messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = coordinatorPrompt },
            new() { Role = "user", Content = userMessage }
        };
        
        var response = "";
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, cancellationToken: default))
        {
            response += chunk;
        }
        
        // Parse JSON response
        return JsonSerializer.Deserialize<IntentAnalysis>(response) 
               ?? new IntentAnalysis { PrimaryAgent = "General", Confidence = 0.5 };
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
    
    private async Task<AgentResponse> ExecuteMultiAgentWorkflowAsync(
        string userMessage,
        PageContext context,
        IntentAnalysis intent,
        List<ChatMessage> conversationHistory,
        Action<string>? onAgentSwitch,
        CancellationToken cancellationToken)
    {
        // Sequential workflow: primary agent -> secondary agents -> aggregation
        var responses = new Dictionary<string, string>();
        var agentsInvolved = new List<string> { intent.PrimaryAgent };
        agentsInvolved.AddRange(intent.AdditionalAgents);
        
        // Execute primary agent first
        onAgentSwitch?.Invoke(intent.PrimaryAgent);
        var primaryResponse = await ExecuteSingleAgentAsync(
            userMessage, context, intent.PrimaryAgent, conversationHistory, null, cancellationToken);
        responses[intent.PrimaryAgent] = primaryResponse.Content;
        
        // Execute additional agents with context from primary
        foreach (var agentName in intent.AdditionalAgents)
        {
            onAgentSwitch?.Invoke(agentName);
            
            var contextualizedMessage = $"Primary analysis from {intent.PrimaryAgent}:\n{primaryResponse.Content}\n\nOriginal request: {userMessage}";
            var secondaryResponse = await ExecuteSingleAgentAsync(
                contextualizedMessage, context, agentName, conversationHistory, null, cancellationToken);
            responses[agentName] = secondaryResponse.Content;
        }
        
        // Aggregate responses
        var aggregatedContent = AggregateResponses(responses, agentsInvolved);
        
        return new AgentResponse
        {
            Content = aggregatedContent,
            AgentName = "Coordinator",
            AgentsInvolved = agentsInvolved
        };
    }
    
    private string AggregateResponses(Dictionary<string, string> responses, List<string> agents)
    {
        if (responses.Count == 1)
        {
            return responses.Values.First();
        }
        
        // Combine multiple agent responses
        var sb = new StringBuilder();
        foreach (var agent in agents)
        {
            if (responses.TryGetValue(agent, out var response))
            {
                sb.AppendLine($"**{agent} says:**");
                sb.AppendLine(response);
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }
}

internal class IntentAnalysis
{
    public string PrimaryAgent { get; set; } = "";
    public bool RequiresMultipleAgents { get; set; }
    public List<string> AdditionalAgents { get; set; } = new();
    public string PrimaryAction { get; set; } = "";
    public double Confidence { get; set; }
}
```

### 4.3 Specialized Agent Interface

**File**: `Services/Chat/ISpecializedAgent.cs`

```csharp
namespace Three2025.Services.Chat;

/// <summary>
/// Base interface for all specialized agents in the system
/// </summary>
public interface ISpecializedAgent
{
    /// <summary>
    /// Agent name (e.g., "3D Modeling Agent", "SysML Agent")
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Brief description of agent capabilities
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Determines if this agent is relevant for the given page context
    /// </summary>
    bool IsRelevantForContext(PageContext context);
    
    /// <summary>
    /// Process a user message and return a response
    /// </summary>
    Task<string> ProcessAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the system prompt/instructions for this agent
    /// </summary>
    string GetSystemPrompt();
}
```

### 4.4 Example Specialized Agent: 3D Modeling Agent

**File**: `Services/Chat/Agents/ThreeDModelingAgent.cs`

```csharp
namespace Three2025.Services.Chat.Agents;

public class ThreeDModelingAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ILogger<ThreeDModelingAgent> _logger;
    
    public string Name => "3D Modeling Agent";
    public string Description => "Expert in Three.js, 3D geometry, meshes, materials, and scene composition";
    
    public ThreeDModelingAgent(
        IMultiProviderChatService chatService,
        ILogger<ThreeDModelingAgent> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }
    
    public bool IsRelevantForContext(PageContext context)
    {
        var relevantPages = new[] 
        { 
            "GeometryTest", "Home", "Drawing", "SpacialBoxTest", 
            "LegoSnapping", "MatrixTest", "Clock" 
        };
        return relevantPages.Any(p => context.PageName.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
    
    public string GetSystemPrompt()
    {
        return """
            You are an expert 3D modeling assistant specializing in Three.js and the Foundry 3D framework.
            
            Your expertise includes:
            - Three.js geometry primitives (BoxGeometry, SphereGeometry, CylinderGeometry, etc.)
            - Mesh creation, materials, and scene composition
            - 3D transformations (position, rotation, scale)
            - Camera setup and lighting
            - The FoShape3D architecture used in this application
            - Canvas3D component and Stage management
            - The three-parameter pattern (Property → Parameter → Geometry → Shape)
            
            When helping users:
            1. Provide concrete code examples using the Foundry framework
            2. Explain 3D concepts clearly with visual analogies
            3. Reference specific files and methods from the codebase when relevant
            4. Suggest best practices for performance and organization
            5. Help debug 3D rendering issues
            
            Current page context will be provided with each request.
            """;
    }
    
    public async Task<string> ProcessAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"3D Modeling Agent processing: {userMessage}");
        
        // Build context-aware messages
        var messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = GetSystemPrompt() },
            new() { Role = "system", Content = $"Current page: {context.PageName} ({context.PageRoute})" },
            new() { Role = "system", Content = $"Page focus: {context.DomainFocus}" }
        };
        
        // Add conversation history
        messages.AddRange(conversationHistory.TakeLast(10)); // Last 10 messages for context
        
        // Add current user message
        messages.Add(new ChatMessage { Role = "user", Content = userMessage });
        
        // Get streaming response
        var response = new StringBuilder();
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage, messages, cancellationToken))
        {
            response.Append(chunk);
        }
        
        return response.ToString();
    }
}
```

### 4.5 Agent Factory

**File**: `Services/Chat/IAgentFactory.cs`

```csharp
namespace Three2025.Services.Chat;

/// <summary>
/// Factory for creating specialized agents
/// </summary>
public interface IAgentFactory
{
    ISpecializedAgent Create3DModelingAgent();
    ISpecializedAgent CreateSysMLAgent();
    ISpecializedAgent CreateAnimationAgent();
    ISpecializedAgent CreateGeometryAgent();
    ISpecializedAgent CreateVisualizationAgent();
    ISpecializedAgent CreateParameterAgent();
}

public class AgentFactory : IAgentFactory
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ILoggerFactory _loggerFactory;
    
    public AgentFactory(
        IMultiProviderChatService chatService,
        ILoggerFactory loggerFactory)
    {
        _chatService = chatService;
        _loggerFactory = loggerFactory;
    }
    
    public ISpecializedAgent Create3DModelingAgent()
        => new ThreeDModelingAgent(_chatService, _loggerFactory.CreateLogger<ThreeDModelingAgent>());
    
    public ISpecializedAgent CreateSysMLAgent()
        => new SysMLAgent(_chatService, _loggerFactory.CreateLogger<SysMLAgent>());
    
    public ISpecializedAgent CreateAnimationAgent()
        => new AnimationAgent(_chatService, _loggerFactory.CreateLogger<AnimationAgent>());
    
    public ISpecializedAgent CreateGeometryAgent()
        => new GeometryAgent(_chatService, _loggerFactory.CreateLogger<GeometryAgent>());
    
    public ISpecializedAgent CreateVisualizationAgent()
        => new VisualizationAgent(_chatService, _loggerFactory.CreateLogger<VisualizationAgent>());
    
    public ISpecializedAgent CreateParameterAgent()
        => new ParameterAgent(_chatService, _loggerFactory.CreateLogger<ParameterAgent>());
}
```

### 4.6 Chat History Service

**File**: `Services/Chat/IChatHistoryService.cs`

```csharp
namespace Three2025.Services.Chat;

/// <summary>
/// Service for persisting and retrieving chat history
/// </summary>
public interface IChatHistoryService
{
    Task<List<ChatMessage>> LoadHistoryAsync(string pageContext);
    Task SaveHistoryAsync(string pageContext, List<ChatMessage> messages);
    Task ClearHistoryAsync(string pageContext);
    Task<List<string>> GetAllContextsAsync();
}

public class ChatHistoryService : IChatHistoryService
{
    private readonly string _storageFolder;
    private readonly ILogger<ChatHistoryService> _logger;
    
    public ChatHistoryService(IConfiguration configuration, ILogger<ChatHistoryService> logger)
    {
        _storageFolder = configuration["ChatHistoryFolder"] ?? "./storage/chat-history";
        _logger = logger;
        
        // Ensure storage folder exists
        Directory.CreateDirectory(_storageFolder);
    }
    
    public async Task<List<ChatMessage>> LoadHistoryAsync(string pageContext)
    {
        var filePath = GetHistoryFilePath(pageContext);
        if (!File.Exists(filePath))
        {
            return new List<ChatMessage>();
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<ChatMessage>>(json) ?? new List<ChatMessage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load chat history for {pageContext}");
            return new List<ChatMessage>();
        }
    }
    
    public async Task SaveHistoryAsync(string pageContext, List<ChatMessage> messages)
    {
        var filePath = GetHistoryFilePath(pageContext);
        try
        {
            var json = JsonSerializer.Serialize(messages, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to save chat history for {pageContext}");
        }
    }
    
    public async Task ClearHistoryAsync(string pageContext)
    {
        var filePath = GetHistoryFilePath(pageContext);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    
    public async Task<List<string>> GetAllContextsAsync()
    {
        return Directory.GetFiles(_storageFolder, "*.json")
         agent chat panel (uses RadzenAIChat internally) -->
<AgentChatPanel 
    PageContext="@chatContext" 
    OnAgentResponse="HandleChatResponse" />

@code {
    private PageContext chatContext = new()
    {
        PageName = "GeometryTestHarness",
        PageRoute = "/geometry-test-harness",
        DomainFocus = "3D Geometry",
        AvailableAgents = new List<string> 
        { 
            "3D Modeling Agent", 
            "Geometry Agent", 
            "Parameter Agent" 
        }
    };
    
    private async Task HandleChatResponse(AgentResponse response)
    {
        // Optionally handle agent responses
        // E.g., if agent suggests parameter changes, apply them
        
        if (response.ActionableData?.ContainsKey("create_geometry") == true)
        {
            var geometryType = response.ActionableData["geometry_type"]?.ToString();
            await CreateGeometryFromAgent(geometryType, response.ActionableData);
        }
        
        Console.WriteLine($"{response.AgentName} suggested: {response.Content

<!-- Your existing page content -->
<div class="geometry-test-container">
    <!-- ... existing UI ... -->
</div>

<!-- Add embedded chat panel -->
<EmbeddedChatPanel 
    PageContext="@chatContext" 
    OnAgentResponse="HandleChatResponse" />

@code {
    private PageContext chatContext = new()
    {
        PageName = "GeometryTestHarness",
        PageRoute = "/geometry-test-harness",
        DomainFocus = "3D Geometry",
        AvailableAgents = new List<string> 
        { 
            "3D Modeling Agent", 
            "Geometry Agent", 
            "Parameter Agent" 
        }
    };
    
    private async Task HandleChatResponse(string response)
    {
        // Optionally handle agent responses
        // E.g., if agent suggests parameter changes, apply them
        Console.WriteLine($"Agent suggested: {response}");
    }
}
```

### 5.2 Page-Specific Agent Configuration

For pages with unique needs, you can configure which agents are available:

```csharp
// In KnModelAnimationTest.razor.cs
protected override void OnInitialized()
{
    chatContext = new PageContext
    {
        PageName = "KnModelAnimation",
        PageRoute = "/knmodel-animation",
        DomainFocus = "Animation & KN Parameters",
        AvailableAgents = new List<string>
        {
            "Animation Agent",      // Primary for this page
            "Parameter Agent",      // Helps with KN parameters
            "3D Modeling Agent"     // Helps with geometry
        },
        PageState = new Dictionary<string, object>
        {
            ["CurrentAnimationState"] = AnimationState,
            ["CurrentClock"] = ClockController?.CurrentTime
        }
    };
}
```

### 5.3 Agent Collaboration Example

When a user asks: *"Create an animated box that grows and rotates"*

1. **Orchestrator** analyzes intent → needs both 3D and Animation agents
2. **3D Modeling Agent** creates the box geometry and mesh
3. **Animation Agent** designs the animation sequence
4. **Orchestrator** aggregates responses with combined instructions

---

## 6. Specialized Agents to Implement

### 6.1 Priority Agents

| Agent Name | Domain | Key Responsibilities |
|------------|--------|---------------------|
| **3D Modeling Agent** | Three.js, FoShape3D | Geometry creation, materials, scene setup |
| **Animation Agent** | Clock system, GlideTween | Animation sequences, timing, interpolation |
| **Parameter Agent** | KN Model, three-parameter | Parameter definition, property updates |
| **Geometry Agent** | Computational geometry | Mesh calculations, transformations |
| **SysML Agent** | Systems modeling | Block diagrams, requirements, workflows |
| **Visualization Agent** | Data visualization | Charts, diagrams, visual representations |

### 6.2 Secondary Agents (Future)

- **LEGO Snapping Agent** - Snapping algorithms and constraints
- **Lighting Agent** - Scene lighting and shadows
- **Camera Agent** - Camera positioning and animation
- **Materials Agent** - PBR materials, textures
- **Physics Agent** - Collision detection, dynamics
- **Code Generation Agent** - Generate Blazor/C# code from descriptions

---

## 7. Service Registration

**Update** `Program.cs`:

```csharp
// Chat infrastructure
builder.Services.AddScoped<IMultiProviderChatService, MultiProviderChatService>(); // Existing
builder.Services.AddScoped<IChatOrchestrator, ChatOrchestrator>(); // New
builder.Services.AddScoped<IAgentFactory, AgentFactory>(); // New
builder.Services.AddSingleton<IChatHistoryService, ChatHistoryService>(); // New

// Specialized agents (auto-registered by AgentFactory, but can be explicit)
builder.Services.AddTransient<ThreeDModelingAgent>();
builder.Services.AddTransient<AnimationAgent>();
builder.Services.AddTransient<GeometryAgent>();
builder.Services.AddTransient<SysMLAgent>();
builder.Services.AddTransient<VisualizationAgent>();
builder.RadzenAIChat Integration Benefits

**Built-in Features from Radzen (No Custom Code Needed):**

✅ **Message Display** - Automatic rendering of user/assistant messages with proper styling  
✅ **Markdown Support** - Code blocks, lists, links automatically formatted  
✅ **Timestamp Display** - Automatic time tracking per message  
✅ **Scroll Management** - Auto-scroll to latest message  
✅ **Input Handling** - Text area with Send button, Enter key support  
✅ **Message History** - Built-in collection management  
✅ **Theming** - Automatically matches your Radzen theme  
✅ **Accessibility** - WCAG compliant, keyboard navigation  
✅ **Responsive** - Mobile-friendly out of the box  

**What We Add (The Value Layer):**

- 🎯 **Agent Status Bar** - Shows which specialized agent is currently responding
- 🎯 **Multi-Agent Orchestration** - Routes messages to appropriate experts
- 🎯 **Context Awareness** - Knows which page and domain it's serving
- 🎯 **Minimize/Maximize** - Collapsible panel that doesn't obstruct page
- 🎯 **Persistence** - Saves/restores chat history per page
- 🎯 **Actionable Responses** - Agents can trigger page actions (create shapes, run animations)

### 8.2 Agent Status Indicators

Already included in `AgentChatPanel.razor` - uses Radzen components:

```razor
<!-- Active Agent Indicator -->
<RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="0.5rem">
    <span class="agent-indicator">
        <RadzenIcon Icon="smart_toy" />
        <strong>@CurrentAgentName</strong>
    </span>
    @if (IsProcessing)
    {
        <RadzenProgressBarCircular ShowValue="false" Size="ProgressBarCircularSize.Small" />
    }
</RadzenStack>

<!-- Available Agents Badge -->
@foreach (var agent in AvailableAgents)
{
    <RadzenBadge BadgeStyle="@(agent == CurrentAgentName ? BadgeStyle.Success : BadgeStyle.Light)" 
                 Text="@GetAgen) ⚡ Faster with RadzenAIChat
- [ ] Create `AgentChatPanel` component (wrapper around `RadzenAIChat`)
- [ ] Implement `IChatOrchestrator` and `ChatOrchestrator`
- [ ] Implement `IChatHistoryService` and `ChatHistoryService`
- [ ] Implement `IAgentFactory` and `AgentFactory`
- [ ] Create `ISpecializedAgent` interface
- [ ] Update `Program.cs` with new service registrations

**Time Saved**: ~3-4 days by using RadzenAIChat instead of building custom UI

### Phase 2: Core Agents (Week 2)
- [ ] Implement `ThreeDModelingAgent`
- [ ] Implement `AnimationAgent`
- [ ] Implement `ParameterAgent`
- [ ] Implement `GeometryAgent`
- [ ] Test agent routing and responses
- [ ] Refine system prompts for each agent

### Phase 3: Integration (Week 2-3)
- [ ] Add chat to 3 pilot pages (GeometryTest, KnModelAnimation, Clock)
- [ ] Test multi-agent workflows
- [ ] Refine agent prompts and routing logic
- [ ] Add chat history persistence
- [ ] Gather user feedback on agent accuracy

### Phase 4: Expansion (Week 3-4)
- [ ] Implement `SysMLAgent` and `VisualizationAgent`
- [ ] Roll out chat to all major pages (20+ pages)
- [ ] Add actionable responses (agents trigger page actions)
- [ ] Performance optimization and caching
- [ ] Add usage analytics

### Phase 5: Advanced Features (Week 4+)
- [ ] Agent memory and learning
- [ ] Code generation capabilities
- [ ] Visual diagram generation
- [ ] Voice input/output
- [ ] File upload and analysis
- [ ] Implement `AnimationAgent`
- [ ] Implement `ParameterAgent`
- [ ] Implement `GeometryAgent`
- [ ] Test agent routing and responses

### Phase 3: Integration (Week 3-4)
- [ ] Add chat to 3 pilot pages (GeometryTest, KnModelAnimation, Clock)
- [ ] Test multi-agent workflows
- [ ] Refine agent prompts and routing logic
- [ ] Add chat history persistence

### Phase 4: Expansion (Week 4+)
- [ ] Implement `SysMLAgent` and `VisualizationAgent`
- [ ] Roll out chat to all major pages
- [ ] Add advanced features (voice input, code execution, file upload)
- [ ] Performance optimization and caching

---

## 10. Advanced Features (Future)

### 10.1 Agent Actions

Agents can return structured data for the page to execute:

```csharp
public class AgentResponse
{
    public string Content { get; set; }
    public string AgentName { get; set; }
    public Dictionary<string, object>? ActionableData { get; set; } // NEW
}

// Example: Agent suggests creating a box
ActionableData = new()
{
    ["action"] = "create_shape",
    ["shapeType"] = "box",
    ["parameters"] = new { width = 10, height = 10, depth = 10 },
    ["position"] = new { x = 0, y = 0, z = 0 }
};
```

Page can detect and execute:

```csharp
private async Task HandleChatResponse(string response, AgentResponse fullResponse)
{
    if (fullResponse.ActionableData?.ContainsKey("action") == true)
    {
        var action = fullResponse.ActionableData["action"]?.ToString();
        switch (action)
        {
            case "create_shape":
                await ExecuteShapeCreation(fullResponse.ActionableData);
                break;
            case "animate":
                await ExecuteAnimation(fullResponse.ActionableData);
                break;
        }
    }
}
```

### 10.2 Agent Memory and Context

Agents can maintain working memory across messages:

```csharp
public interface ISpecializedAgent
{
    Dictionary<string, object> WorkingMemory { get; }
    void RememberFact(string key, object value);
    object? RecallFact(string key);
}
```

### 10.3 Visual Agent Feedback

Show which agent is currently active:

```razor
<div class="agent-status-bar">
    @foreach (var agent in AvailableAgents)
    {
        <div class="agent-indicator @(agent == CurrentAgent ? "active" : "")">
            @agent
        </div>
    }
</div>
```

### 10.4 Code Execution Capability

Agents can generate and execute code in sandbox:

```csharp
public interface ICodeExecutionService
{
    Task<ExecutionResult> ExecuteAsync(string code, string language);
}

// Agent generates code, execution service runs it safely
var code = "var box = new FoBox3D { Width = 10, Height = 10, Depth = 10 };";
var result = await _codeExecution.ExecuteAsync(code, "csharp");
```

---

## 11. Testing Strategy

### 11.1 Unit Tests

```csharp
[Fact]
public async Task ChatOrchestrator_Routes_To_Correct_Agent()
{
    // Arrange
    var orchestrator = CreateOrchestrator();
    var context = new PageContext { DomainFocus = "3D" };
    
    // Act
    var response = await orchestrator.ProcessMessageAsync(
        "Create a blue sphere", context, new List<ChatMessage>());
    
    // Assert
    Assert.Equal("3D Modeling Agent", response.AgentName);
}
```

### 11.2 Integration Tests

```csharp
[Fact]
public async Task EmbeddedChatPanel_Displays_Messages()
{
    // Test Blazor component rendering with Bunit
    var cut = RenderComponent<EmbeddedChatPanel>(parameters => parameters
        .Add(p => p.PageContext, new PageContext())
    );
    
    Assert.Contains("Assistant", cut.Markup);
}
```

### 11.3 Agent Prompt Testing

Create test suite to validate agent responses:

```csharp
[Theory]
[InlineData("Create a red cube", "3D Modeling Agent")]
[InlineData("Animate this shape", "Animation Agent")]
[InlineData("What is a block diagram?", "SysML Agent")]
public async Task Agent_Selection_Is_Correct(string message, string expectedAgent)
{
    var intent = await _orchestrator.AnalyzeIntentAsync(message, defaultContext);
    Assert.Equal(expectedAgent, intent.PrimaryAgent);
}
```

---

## 12. Performance Considerations

### 12.1 Response Streaming

All agents use streaming to provide immediate feedback:

```csharp
await foreach (var chunk in _chatService.SendMessageStreamingAsync(...))
{
    response.Append(chunk);
    StateHasChanged(); // Update UI progressively
}
```

### 12.2 Caching

Cache agent routing decisions for similar queries:

```csharp
private readonly MemoryCache _routingCache = new(new MemoryCacheOptions());

public async Task<IntentAnalysis> AnalyzeIntentAsync(string message, PageContext context)
{
    var cacheKey = $"{context.PageName}:{message.GetHashCode()}";
    if (_routingCache.TryGetValue(cacheKey, out IntentAnalysis cached))
    {
        return cached;
    }
    
    var result = await ActualAnalysis(message, context);
    _routingCache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
    return result;
}
```

### 12.3 Lazy Agent Loading

Only initialize agents when first needed:

```csharp
private readonly Lazy<ISpecializedAgent> _3dAgent;

_3dAgent = new Lazy<ISpecializedAgent>(() => 
    _agentFactory.Create3DModelingAgent());
```

---

## 13. Security Considerations

### 13.1 Input Sanitization

Sanitize all user inputs before sending to agents:

```csharp
private string SanitizeInput(string input)
{
    // Remove potential injection attempts
    // Limit length
    // Validate format
    return input.Substring(0, Math.Min(input.Length, 2000));
}
```

### 13.2 Agent Response Validation

Validate agent responses before displaying:

```csharp
private bool IsResponseSafe(string response)
{
    // Check for script injection
    // Validate markdown doesn't contain XSS
    // Ensure no file system access commands
    return !response.Contains("<script");
}
```

### 13.3 Rate Limiting

Implement rate limiting per session:

```csharp
public class RateLimitedChatService
{
    private readonly Dictionary<string, RateLimit> _limits = new();
    
    public bool CanSendMessage(string sessionId)
    {
        // Max 30 messages per minute
        return _limits[sessionId].Count < 30;
    }
}
```

---

## 14. Success Metrics

### 14.1 Quantitative

- **Response Time**: < 2 seconds for intent analysis
- **Accuracy**: > 85% correct agent routing
- **Adoption**: Used in 50%+ of user sessions
- **Satisfaction**: > 4.0/5.0 user rating

### 14.2 Qualitative

- Users find answers without leaving the page
- Reduced support requests for common questions
- Developers use chat for code generation
- Agent suggestions lead to feature discoveries

---

## 15. Future Enhancements

### 15.1 Multi-Modal Agents

- **Vision Agent**: Analyze screenshots and suggest improvements
- **Voice Agent**: Voice-to-text and text-to-voice
- **Diagram Agent**: Generate Mermaid diagrams from descriptions

### 15.2 Learning and Improvement

- Track which agent responses were helpful (thumbs up/down)
- Fine-tune agent prompts based on feedback
- A/B test different prompt strategies

### 15.3 Cross-Page Context

- Agent remembers what user did on previous pages
- Suggests rAgentChatPanel.razor          # Wrapper around RadzenAIChat
│       └── AgentChatPanel.razor.cs       # Orchestration logic
├── Services/
│   └── Chat/
│       ├── IChatOrchestrator.cs          # Main orchestrator interface
│       ├── ChatOrchestrator.cs           # Routes messages to agents
│       ├── IAgentFactory.cs              # Agent creation interface
│       ├── AgentFactory.cs               # Creates specialized agents
│       ├── ISpecializedAgent.cs          # Base agent interface
│       ├── IChatHistoryService.cs        # Persistence interface
│       ├── ChatHistoryService.cs         # File-based persistence
│       └── Agents/
│           ├── ThreeDModelingAgent.cs    # 3D/Three.js expert
│           ├── AnimationAgent.cs         # Animation & timing expert
│           ├── ParameterAgent.cs         # KN parameter expert
│           ├── GeometryAgent.cs          # Computational geometry expert
│           ├── SysMLAgent.cs             # Systems modeling expert
│           └── VisualizationAgent.cs     # Data visualization expert
└── storage/
    └── chat-history/                     # Persisted conversations
        ├── GeometryTestHarness.json
        ├── KnModelAnimation.json
        └── Clock.json
```

**Key Dependencies:**
- `Radzen.Blazor` - Provides `RadzenAIChat` base component
- `Microsoft.Extensions.AI` - For chat client abstraction (existing)
- `Microsoft.Agents.AI` - For agent framework (existing)
- `System.Text.Json` - For history serializationee2025/
├── Components/
│   └── Shared/
│       ├── EmbeddedChatPanel.razor
│       ├── EmbeddedChatPanel.razor.cs
│       ├── ChatMessageBubble.razor
│       └── ThinkingIndicator.razor
├── Services/
│   └── Chat/
│       ├── IChatOrchestrator.cs
│       ├── ChatOrchestrator.cs
│       ├── IAgentFactory.cs
│       ├── AgentFactory.cs
│       ├── ISpecializedAgent.cs
│       ├── IChatHistoryService.cs
│       ├── ChatHistoryService.cs
│       └── Agents/
│           ├── ThreeDModelingAgent.cs
│           ├── AnimationAgent.cs
│           ├── ParameterAgent.cs
│           ├── GeometryAgent.cs
│           ├── SysMLAgent.cs
│           └── VisualizationAgent.cs
└── storage/
    └── chat-history/
        ├── GeometryTestHarness.json
        ├── KnModelAnimation.json
        └── Clock.json
```

---

## Appendix B: Sample Agent Prompts

See individual agent implementation files for complete system prompts.

**Key Patterns:**
1. Clear role definition
2. Expertise areas listed
3. Context awareness instructions
4. Code example expectations
5. Response format guidelines

---

## Conclusion

This specification provides a complete blueprint for implementing a sophisticated multi-agent chatbot system throughout the Three2025 application. The modular design allows for incremental implementation while maintaining extensibility for future enhancements.

**Next Steps:**
1. Review and approve specification
2. Create GitHub issues for Phase 1 tasks
3. Begin implementation with `EmbeddedChatPanel` component
4. Iterate based on developer feedback
