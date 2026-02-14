# AI Tool Integration Guide for Microsoft.Extensions.AI

**Version:** 1.0  
**Date:** December 26, 2025  
**Framework:** Microsoft.Extensions.AI Agent Framework  
**Purpose:** Complete guide for integrating custom tools into AI agents using reflection-based discovery

---

## Overview

This guide documents the proven pattern for integrating custom business logic as AI-accessible tools in the Microsoft.Extensions.AI framework. The pattern uses reflection-based tool discovery, dependency injection, and the `[Description]` attribute to automatically expose C# methods to LLMs.

**Key Benefits:**
- ✅ Automatic tool discovery via reflection
- ✅ Type-safe method signatures
- ✅ Leverages existing DI infrastructure
- ✅ Zero boilerplate per tool
- ✅ Works with any ITechnician implementation
- ✅ Tools are shared across all agents

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Startup                       │
└────────────────────────────────┬────────────────────────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │   Dependency Injection   │
                    │   (Program.cs)           │
                    │                          │
                    │ • IGeometryTech          │
                    │ • ITechnicianToolProvider│
                    │ • IChatOrchestrator      │
                    │ • IAgentFactory          │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │  ChatOrchestrator        │
                    │  Constructor             │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │  TechnicianToolProvider  │
                    │  .DiscoverAllTools()     │
                    │                          │
                    │  1. Find ITechnician     │
                    │     interfaces           │
                    │  2. Resolve from DI      │
                    │  3. Scan [Description]   │
                    │  4. Create AIFunctions   │
                    └────────────┬─────────────┘
                                 │
                        12 AIFunction objects
                                 │
                    ┌────────────▼─────────────┐
                    │     Agent Factory        │
                    │  Creates agents with     │
                    │  tools collection        │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │   Specialized Agents     │
                    │  (Geometry, Animation,   │
                    │   3D Modeling, etc.)     │
                    │                          │
                    │  Each has: _tools list   │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │  User Message Arrives    │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │  Agent.ProcessAsync()    │
                    │                          │
                    │  _chatService.SendAsync( │
                    │    messages,             │
                    │    _tools  ← passed here │
                    │  )                       │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │  LLM Execution           │
                    │                          │
                    │  Sees: AddShape,         │
                    │        RotateShape,      │
                    │        GetShapes, etc.   │
                    │                          │
                    │  Decides to call tool    │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │  AIFunction.InvokeAsync()│
                    │                          │
                    │  Calls actual C# method: │
                    │  GeometryTech.AddShape() │
                    └────────────┬─────────────┘
                                 │
                    ┌────────────▼─────────────┐
                    │  Business Logic Executes │
                    │                          │
                    │  • Creates shape         │
                    │  • Adds to stage         │
                    │  • Refreshes UI          │
                    │  • Returns state         │
                    └──────────────────────────┘
```

---

## Step-by-Step Integration

### Step 1: Define the Technician Interface

Create an interface that inherits from `ITechnician`. This marks it for automatic discovery.

**File:** `Apprentice/IGeometryTech.cs`

```csharp
namespace Three2025.Apprentice;

public interface IGeometryTech : ITechnician
{
    void SetStage(FoStage3D stage);
    
    List<ShapeInfo> GetShapes();
    
    List<ShapeInfo> AddShape(string name, bool isOn, string color, string shapeType);
    
    List<ShapeInfo> RepositionShape(string name, double x, double y, double z);
    
    List<ShapeInfo> ChangeColor(string name, string color);
    
    // ... more methods
}
```

**Key Points:**
- Must inherit from `ITechnician` marker interface
- Define clean method signatures
- Use descriptive parameter names
- Return useful types (DTOs, lists, primitives)

### Step 2: Implement the Technician

Implement the interface and add `[Description]` attributes to methods you want exposed as tools.

**File:** `Apprentice/GeometryTech.cs`

```csharp
public class GeometryTech : IGeometryTech
{
    private readonly IFoundryService _foundryService;
    private FoStage3D? _stage;
    
    public GeometryTech(IFoundryService foundryService)
    {
        _foundryService = foundryService;
    }
    
    public void SetStage(FoStage3D stage)
    {
        _stage = stage; // Not exposed to AI - no [Description]
    }
    
    [Description("Create and add a 3D shape to the geometry stage")]
    public List<ShapeInfo> AddShape(
        [Description("The name of the shape to create")] string name,
        [Description("Whether the shape should be visible/active")] bool isOn,
        [Description("The color of the shape")] string color,
        [Description("The type of shape: box, sphere, cylinder, cone, torus")] 
        string shapeType = "box")
    {
        if (_stage == null)
            throw new InvalidOperationException("Stage not initialized");
            
        var shape = new GeometryShape(name, shapeType)
        {
            IsOn = isOn,
            Color = color
        };
        
        _stage.AddShape(shape);
        RefreshUI();
        
        return GetShapes();
    }
    
    [Description("Gets a list of all shapes and their current state")]
    public List<ShapeInfo> GetShapes()
    {
        if (_stage == null) return new List<ShapeInfo>();
        
        return _stage.Members<GeometryShape>()
            .Select(s => ConvertToShapeInfo(s))
            .ToList();
    }
    
    // Helper method - not exposed (no [Description])
    private void RefreshUI()
    {
        _foundryService?.PubSub?.Publish(RefreshRenderMessage.ClearAllSelected());
    }
}

// Data Transfer Object
public class ShapeInfo
{
    public string Name { get; set; } = "";
    public string GeomType { get; set; } = "";
    public string Color { get; set; } = "";
    public bool IsVisible { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    // ... more properties
}
```

**Key Points:**
- `[Description]` on METHOD = exposes as tool
- `[Description]` on PARAMETER = guides LLM on usage
- Methods WITHOUT `[Description]` are NOT exposed
- Return useful data structures
- Use default parameters for optional values

### Step 3: Register in Dependency Injection

**CRITICAL:** Service lifetime must match dependencies!

**File:** `Program.cs`

```csharp
// ❌ WRONG - Singleton cannot resolve Scoped services
services.AddSingleton<ITechnicianToolProvider, TechnicianToolProvider>();
services.AddScoped<IGeometryTech, GeometryTech>();

// ✅ CORRECT - Match lifetimes
services.AddScoped<ITechnicianToolProvider, TechnicianToolProvider>();
services.AddScoped<IGeometryTech, GeometryTech>();

// Register other components
services.AddScoped<IMultiProviderChatService, MultiProviderChatService>();
services.AddScoped<IChatOrchestrator, ChatOrchestrator>();
services.AddScoped<IAgentFactory, AgentFactory>();
```

**Service Lifetime Rules:**
- If technician depends on `IFoundryService`, `IFoCollection`, or any Scoped service → use `Scoped`
- Tool provider must match or be more transient than technicians
- Chat services should be `Scoped` for per-request state

### Step 4: Implement Tool Discovery

Create a provider that scans for `ITechnician` interfaces and converts methods to `AIFunction` objects.

**File:** `Services/Agents/TechnicianToolProvider.cs`

```csharp
public class TechnicianToolProvider : ITechnicianToolProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TechnicianToolProvider> _logger;
    
    public TechnicianToolProvider(
        IServiceProvider serviceProvider,
        ILogger<TechnicianToolProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public IEnumerable<AIFunction> DiscoverAllTools()
    {
        _logger.LogInformation("🔍 Starting tool discovery...");
        
        // Find all ITechnician interfaces
        var technicianTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ITechnician).IsAssignableFrom(t) && 
                       t.IsInterface && 
                       t != typeof(ITechnician))
            .ToList();
        
        var allTools = new List<AIFunction>();
        
        foreach (var interfaceType in technicianTypes)
        {
            var tools = ExtractToolsFromTechnician(interfaceType);
            allTools.AddRange(tools);
        }
        
        _logger.LogInformation($"✅ Discovered {allTools.Count} tools");
        return allTools;
    }
    
    private List<AIFunction> ExtractToolsFromTechnician(Type interfaceType)
    {
        var tools = new List<AIFunction>();
        
        // CRITICAL: Resolve implementation from DI
        var implementation = _serviceProvider.GetService(interfaceType);
        if (implementation == null)
        {
            _logger.LogWarning($"⚠️ No implementation for {interfaceType.Name}");
            return tools;
        }
        
        var implementationType = implementation.GetType();
        
        // Find methods with [Description] attribute
        var methods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null &&
                       m.DeclaringType == implementationType)
            .ToList();
        
        foreach (var method in methods)
        {
            try
            {
                var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description 
                    ?? $"Invokes {method.Name}";
                
                // Convert to AIFunction using framework factory
                var func = AIFunctionFactory.Create(
                    method,
                    target: implementation,
                    name: method.Name,
                    description: description);
                
                tools.Add(func);
                
                _logger.LogDebug($"  ✓ {method.Name}: {description}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to create tool from {method.Name}");
            }
        }
        
        return tools;
    }
}
```

**Key Points:**
- Use `_serviceProvider.GetService()` to resolve implementations
- Only process methods with `[Description]` attribute
- Use `AIFunctionFactory.Create()` to convert methods
- Include detailed logging for debugging

### Step 5: Wire into Chat Orchestrator

The orchestrator discovers tools once and passes them to all agents.

**File:** `Services/Chat/ChatOrchestrator.cs`

```csharp
public class ChatOrchestrator : IChatOrchestrator
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ITechnicianToolProvider _toolProvider;
    private readonly IAgentFactory _agentFactory;
    private readonly Dictionary<string, ISpecializedAgent> _agents = new();
    private readonly List<AIFunction> _technicianTools = new();
    
    public ChatOrchestrator(
        IMultiProviderChatService chatService,
        ITechnicianToolProvider toolProvider,
        IAgentFactory agentFactory)
    {
        _chatService = chatService;
        _toolProvider = toolProvider;
        _agentFactory = agentFactory;
        
        // Discover tools ONCE at startup
        _technicianTools.AddRange(_toolProvider.DiscoverAllTools());
        
        InitializeAgents();
    }
    
    private void InitializeAgents()
    {
        // Pass tools to ALL agents
        RegisterAgent(_agentFactory.CreateGeometryAgent(_technicianTools));
        RegisterAgent(_agentFactory.CreateAnimationAgent(_technicianTools));
        RegisterAgent(_agentFactory.CreateClockAgent(_technicianTools));
        RegisterAgent(_agentFactory.CreateGeneralAgent(_technicianTools));
    }
    
    public int GetToolCount() => _technicianTools.Count;
    
    public IEnumerable<AIFunction> GetAllTools() => _technicianTools;
}
```

**Key Points:**
- Discover tools in constructor (happens once per scope)
- Store tools in field for reuse
- Pass same tool collection to all agents
- Provide methods to query tool count for diagnostics

### Step 6: Create Agents with Tools

Agent factory creates specialized agents and injects tools.

**File:** `Services/Chat/AgentFactory.cs`

```csharp
public class AgentFactory : IAgentFactory
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ILoggerFactory _loggerFactory;
    
    public ISpecializedAgent CreateGeometryAgent(IEnumerable<AIFunction> tools)
        => new GeometryAgent(
            _chatService, 
            tools,  // Tools passed to agent
            _loggerFactory.CreateLogger<GeometryAgent>());
    
    // ... more agent creation methods
}
```

### Step 7: Agent Uses Tools

Each agent receives tools and passes them to the LLM.

**File:** `Services/Chat/Agents/GeometryAgent.cs`

```csharp
public class GeometryAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    
    public string Name => "Geometry Agent";
    
    public GeometryAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<GeometryAgent> logger)
    {
        _chatService = chatService;
        _tools = technicianTools.ToList(); // Store tools
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt = """
            You are a Geometry Expert specializing in 3D shapes and spatial manipulation.
            
            You have direct access to geometry tools like:
            - AddShape, AddShapeWithDimensions
            - RepositionShape, RotateShape, ScaleShape
            - ChangeColor, ChangeState
            - GetShapes, GetShapeByName
            - DuplicateShape, DeleteShape
            
            Use these tools to help users create and manipulate 3D scenes.
            """;
        
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        messages.AddRange(conversationHistory);
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        // CRITICAL: Pass tools to LLM
        await foreach (var chunk in _chatService.SendMessageStreamingAsync(
            userMessage,
            messages,
            _tools,  // <-- Tools available to LLM here!
            cancellationToken: cancellationToken))
        {
            yield return chunk;
        }
    }
}
```

**Key Points:**
- Store tools in agent field
- Include tool capabilities in system prompt
- Pass `_tools` to `SendMessageStreamingAsync()`
- LLM can now see and call all tools

### Step 8: Connect Stage in Page

The page connects the technician to its actual runtime context.

**File:** `Components/Pages/AgentCanvasIntegration.razor.cs`

```csharp
public partial class AgentCanvasIntegration : ComponentBase
{
    [Inject] protected IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] protected IGeometryTech GeometryTech { get; set; } = default!;
    
    protected Canvas3DComponent Canvas3DReference;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(100); // Let canvas initialize
            
            if (Canvas3DReference?.Stage != null)
            {
                // CRITICAL: Connect technician to canvas stage
                GeometryTech.SetStage(Canvas3DReference.Stage);
                Logger.LogInformation("✅ Connected GeometryTech to stage");
            }
        }
    }
    
    protected async Task SendMessage()
    {
        // Orchestrator routes to appropriate agent with tools
        await foreach (var chunk in ChatOrchestrator.ProcessMessageStreamingAsync(
            userInput,
            pageContext,
            conversationHistory))
        {
            streamingResponse += chunk;
            StateHasChanged();
        }
    }
}
```

**Key Points:**
- Inject both orchestrator and technician
- Connect technician to runtime context in `OnAfterRenderAsync`
- Tools execute against connected context (stage, canvas, etc.)

---

## Critical Patterns & Best Practices

### Pattern 1: Service Lifetime Matching

```csharp
// Rule: Service must match or be more transient than dependencies

// ❌ BREAKS - Singleton cannot resolve Scoped
services.AddSingleton<ITechnicianToolProvider>()
services.AddScoped<IGeometryTech>()

// ✅ WORKS - Both Scoped
services.AddScoped<ITechnicianToolProvider>()
services.AddScoped<IGeometryTech>()

// ✅ WORKS - Transient can resolve Scoped
services.AddTransient<ITechnicianToolProvider>()
services.AddScoped<IGeometryTech>()
```

**Why it matters:** At startup, if TechnicianToolProvider is Singleton, it tries to resolve GeometryTech immediately, but GeometryTech is Scoped (per-request). This fails with 0 tools discovered.

### Pattern 2: Selective Exposure

```csharp
// ✅ Exposed to AI
[Description("Create a new shape")]
public List<ShapeInfo> AddShape(string name, string color) { }

// ❌ NOT exposed to AI (no [Description])
public void SetStage(FoStage3D stage) { }
private void RefreshUI() { }
```

**Why it matters:** Not all methods should be AI-accessible. Use `[Description]` as a gate.

### Pattern 3: Rich Parameter Descriptions

```csharp
[Description("Rotates a shape around X, Y, Z axes in degrees")]
public List<ShapeInfo> RotateShape(
    [Description("The name of the shape to rotate")] 
    string name,
    [Description("Rotation around X axis in degrees (0-360)")] 
    double xDegrees,
    [Description("Rotation around Y axis in degrees (0-360)")] 
    double yDegrees,
    [Description("Rotation around Z axis in degrees (0-360)")] 
    double zDegrees)
```

**Why it matters:** LLM uses descriptions to understand how to call methods correctly.

### Pattern 4: Return Useful State

```csharp
// ❌ Void return - agent gets no feedback
public void AddShape(string name, string color) { }

// ✅ Return current state - agent can verify
public List<ShapeInfo> AddShape(string name, string color)
{
    // ... create shape
    return GetShapes(); // Return full scene state
}
```

**Why it matters:** LLM needs feedback to verify operations and chain commands.

### Pattern 5: Default Parameters

```csharp
[Description("Create a new shape")]
public List<ShapeInfo> AddShape(
    string name,
    bool isOn = true,           // Default visible
    string color = "gray",      // Default color
    string shapeType = "box")   // Default shape
{
    // LLM can call: AddShape("Box1") or AddShape("Box1", true, "red", "sphere")
}
```

**Why it matters:** Simpler common-case calls, flexibility for complex cases.

---

## Troubleshooting Guide

### Problem: "0 tools discovered"

**Symptoms:**
```
🔍 Starting tool discovery...
✅ Discovered 0 total tools
```

**Causes & Solutions:**

1. **Service Lifetime Mismatch**
   ```csharp
   // Check Program.cs registration
   // Solution: Match lifetimes
   services.AddScoped<ITechnicianToolProvider>()
   services.AddScoped<IGeometryTech>()
   ```

2. **Interface Not Inheriting ITechnician**
   ```csharp
   // ❌ Wrong
   public interface IGeometryTech { }
   
   // ✅ Correct
   public interface IGeometryTech : ITechnician { }
   ```

3. **No [Description] Attributes**
   ```csharp
   // Add [Description] to methods you want exposed
   [Description("Create a shape")]
   public List<ShapeInfo> AddShape(...) { }
   ```

4. **Not Registered in DI**
   ```csharp
   // Add to Program.cs
   services.AddScoped<IGeometryTech, GeometryTech>();
   ```

### Problem: "LLM doesn't call tools"

**Causes & Solutions:**

1. **Tools Not Passed to LLM**
   ```csharp
   // Check agent code
   await _chatService.SendMessageStreamingAsync(
       message,
       messages,
       _tools,  // <-- Must pass tools!
       cancellationToken);
   ```

2. **Vague Descriptions**
   ```csharp
   // ❌ Unclear
   [Description("Does something")]
   
   // ✅ Clear
   [Description("Creates a 3D box shape with specified color and position")]
   ```

3. **System Prompt Doesn't Mention Tools**
   ```csharp
   var systemPrompt = """
       You are a Geometry Expert.
       You have access to shape manipulation tools:
       - AddShape, RepositionShape, RotateShape, etc.
       Use these tools to help users.
       """;
   ```

### Problem: "Stage not initialized" Error

**Cause:** Technician not connected to runtime context.

**Solution:**
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(100); // Wait for canvas
        
        if (Canvas3DReference?.Stage != null)
        {
            GeometryTech.SetStage(Canvas3DReference.Stage);
        }
    }
}
```

### Problem: Tools Execute But Nothing Happens

**Causes:**

1. **No UI Refresh**
   ```csharp
   public List<ShapeInfo> AddShape(...)
   {
       _stage.AddShape(shape);
       RefreshUI(); // <-- Call this!
       return GetShapes();
   }
   ```

2. **Stage Not Connected**
   ```csharp
   // Verify in logs
   if (_stage == null)
       throw new InvalidOperationException("Stage not initialized");
   ```

---

## Advanced Patterns

### Pattern: Runtime Capability Discovery

Allow agents to discover tool capabilities at runtime:

```csharp
[Description("Gets comprehensive information about all available tool capabilities")]
public ToolCapabilities GetToolCapabilities()
{
    return new ToolCapabilities
    {
        ToolName = "GeometryTech",
        SupportedShapeTypes = new[] { "box", "sphere", "cylinder", ... },
        Categories = new[]
        {
            new CapabilityCategory
            {
                CategoryName = "Shape Creation",
                Operations = new[]
                {
                    new ToolOperation
                    {
                        MethodName = "AddShape",
                        Description = "Create new shape",
                        Parameters = ...
                    }
                }
            }
        }
    };
}
```

**Use case:** Agent asks "What can you do?" and gets structured capability information.

### Pattern: Tool Versioning

```csharp
public interface IGeometryTech : ITechnician
{
    string GetToolVersion(); // Returns "1.0"
    
    [Description("V1: Create basic shape")]
    List<ShapeInfo> AddShape(string name, string color);
    
    [Description("V2: Create shape with dimensions")]
    List<ShapeInfo> AddShapeV2(string name, string color, double width, double height, double depth);
}
```

### Pattern: Batch Operations

```csharp
[Description("Delete multiple shapes in one operation")]
public List<ShapeInfo> DeleteMultipleShapes(
    [Description("List of shape names to delete")]
    List<string> names)
{
    int deletedCount = 0;
    foreach (var name in names)
    {
        var shape = FindShape(name);
        if (shape != null)
        {
            DeleteShape(name);
            deletedCount++;
        }
    }
    return GetShapes();
}
```

---

## Testing Tools

### Manual Testing

```csharp
// Page code
protected async Task TestToolDiscovery()
{
    var toolCount = ChatOrchestrator.GetToolCount();
    var tools = ChatOrchestrator.GetAllTools();
    
    Logger.LogInformation($"Found {toolCount} tools:");
    foreach (var tool in tools)
    {
        Logger.LogInformation($"  - {tool.Metadata.Name}: {tool.Metadata.Description}");
    }
}
```

### Diagnostic Endpoint

```csharp
[Description("Shows all available tools and their capabilities")]
public ToolDiagnostics GetToolDiagnostics()
{
    return new ToolDiagnostics
    {
        TotalTools = _allTools.Count,
        ToolsByCategory = _allTools.GroupBy(t => t.Category).ToDictionary(...),
        TechnicianCount = _technicianTypes.Count
    };
}
```

---

## Performance Considerations

### Tool Discovery Caching

Tool discovery happens once per scope (per request in web apps):

```csharp
private readonly Dictionary<Type, List<AIFunction>> _toolCache = new();

public IEnumerable<AIFunction> GetToolsFor(Type technicianInterface)
{
    if (_toolCache.TryGetValue(technicianInterface, out var cached))
        return cached;
        
    var tools = ExtractTools(technicianInterface);
    _toolCache[technicianInterface] = tools;
    return tools;
}
```

### Lazy Initialization

```csharp
private List<AIFunction>? _tools;

public IEnumerable<AIFunction> DiscoverAllTools()
{
    if (_tools != null)
        return _tools;
        
    _tools = new List<AIFunction>();
    // ... discovery logic
    return _tools;
}
```

---

## Related Documentation

- **[AI_AGENT_SHAPE_TOOL_SPECIFICATION.md](AI_AGENT_SHAPE_TOOL_SPECIFICATION.md)** - Dual guide for building and using shape tools
- **[MULTI_AGENT_CHATBOT_INFRASTRUCTURE_SPEC.md](MULTI_AGENT_CHATBOT_INFRASTRUCTURE_SPEC.md)** - Overall agent architecture
- **Microsoft.Extensions.AI Documentation** - Framework reference

---

## Real Implementation Examples

### GeometryTech
Complete working implementation with 19+ tools.
- **Location**: `Three2025/Apprentice/GeometryTech.cs`
- **Tools**: Shape creation, transformation, querying, deletion
- **Interface**: `IGeometryTech`

### TechnicianToolProvider
Reflection-based tool discovery system.
- **Location**: `Three2025/Services/Agents/TechnicianToolProvider.cs`
- **Features**: Automatic scanning, caching, error handling

### ChatOrchestrator
Orchestrates tool discovery and agent creation.
- **Location**: `Three2025/Services/Chat/ChatOrchestrator.cs`
- **Pattern**: Tools discovered once, shared with all agents

### GeometryAgent
Specialized agent using geometry tools.
- **Location**: `Three2025/Services/Chat/Agents/GeometryAgent.cs`
- **Pattern**: Receives tools, passes to LLM

---

## CRITICAL: OPResult Return Pattern

**All technician methods MUST return `OPResult`** - this is a fundamental architectural requirement.

### Why OPResult?

When Microsoft.Extensions.AI executes a tool, it **JSON-serializes the return value** before sending it back to the LLM. If your method returns raw objects (FoShape3D, string, etc.), the LLM receives cryptic serialized metadata instead of meaningful feedback.

### The Problem (Before)

```csharp
// ❌ WRONG - Returns raw object
[Description("Change the color of a shape")]
public FoShape3D ChangeColor(string name, string color)
{
    var shape = FindShape(name);
    shape.Color = color;
    return shape;  // LLM sees: {"operatorToken": {"text": "...", ...}}
}
```

The LLM receives meaningless JSON and can't confirm the operation succeeded.

### The Solution (After)

```csharp
// ✅ CORRECT - Returns OPResult
[Description("Change the color of a shape")]
public OPResult ChangeColor(string name, string color)
{
    var shape = FindShape(name);
    if (shape == null)
        return OPResult.Error($"Shape '{name}' not found");
    
    shape.Color = color;
    return OPResult.Success($"Changed color of '{name}' to '{color}'");
}
```

The LLM receives:
```json
{
  "ResultType": "Success",
  "HasError": false,
  "ResultMessage": "Changed color of 'cube1' to 'green'"
}
```

### OPResult Factory Methods

| Method | Usage | Example |
|--------|-------|---------|
| `OPResult.Success(message)` | Operation completed successfully | `OPResult.Success("Deleted shape 'Box1'")` |
| `OPResult.Error(message)` | Operation failed | `OPResult.Error("Shape not found")` |
| `OPResult.Object(value)` | Return an object with success | `OPResult.Object(newShape)` |
| `OPResult.Collection(list)` | Return a list of items | `OPResult.Collection(shapes)` |

### OPResult Public Properties (for JSON Serialization)

OPResult exposes these properties for LLM consumption:

```csharp
public string ResultType { get; }      // "Success", "Error", "Shape3D", "Collection"
public bool HasError { get; }          // true if error occurred
public string ResultMessage { get; }   // Human-readable description
```

### Interface Contract

Update your interface to match:

```csharp
public interface IShape3DTech : ITechnician
{
    // ALL methods return OPResult - no exceptions!
    OPResult AddShape(string name, string color, string shapeType);
    OPResult ChangeColor(string name, string color);
    OPResult DeleteShape(string name);
    OPResult GetShapes();
    // ... etc
}
```

---

## CRITICAL: Agent System Prompt Guidelines

The LLM's behavior is entirely controlled by its system prompt. Ambiguous prompts lead to incorrect tool selection.

### The Verification Problem

**Problematic Prompt:**
```
❌ DON'T: Assume shape names - call GetShapes() to confirm what exists
```

This makes the LLM call `GetShapes()` before EVERY operation, even when the user explicitly names the shape: "Make cube1 green" → LLM calls GetShapes() instead of ChangeColor().

**Fixed Prompt:**
```
✅ DO: When user provides EXPLICIT name like "cube1" → CALL THE TOOL DIRECTLY!
❌ DON'T: Call GetShapes() before ChangeColor when user says "Make cube1 green"
```

### Prompt Pattern: Explicit Names vs Ambiguous References

**ALWAYS include this in agent system prompts:**

```markdown
## CRITICAL: Explicit Names vs Ambiguous References

**Step 0: Check if the user provided an EXPLICIT SHAPE NAME**
- If user says "Make cube1 green" → The name IS "cube1" - CALL ChangeColor('cube1', 'green') IMMEDIATELY
- If user says "Move Box1 up" → The name IS "Box1" - CALL RepositionShape('Box1', 0, 5, 0) IMMEDIATELY
- DO NOT call GetShapes() when an explicit name is provided!

**Only call GetShapes() for AMBIGUOUS references:**
- "Move it" → Who is "it"? Check history or call GetShapes()
- "Make the box red" → Which box? May need GetShapes()
- "Delete that" → What is "that"? Check context

## CRITICAL EXAMPLES:
"Make cube1 green" → DIRECTLY call ChangeColor('cube1', 'green') - NO GetShapes() needed!
"Move Box1 up" → DIRECTLY call RepositionShape('Box1', 0, 5, 0) - NO GetShapes() needed!
"Make it blue" → ONLY NOW check history or call GetShapes() to find what "it" refers to
```

### Verification Tools Should Be Rare

If your LLM is calling `GetShapes()` for every request, your prompt is too cautious. The pattern should be:

1. **User provides explicit name** → Execute directly
2. **User uses pronoun ("it", "that")** → Check conversation history first
3. **Still ambiguous** → Then call GetShapes()

---

## Editor Pattern: Tool → Editor → Model

Editors provide the implementation layer between tools and the model. They:

1. **Manage state** - Know which stage/page is active
2. **Execute operations** - Create, modify, delete shapes
3. **Return OPResult** - Consistent return type for all operations

### Editor Responsibilities

```csharp
public interface IShape3DEditor
{
    // State management
    void ConnectStage(FoStage3D stage);
    FoStage3D? GetActiveStage();
    
    // Shape operations - ALL return OPResult
    OPResult CreateShape(string name, string shapeType, string color, 
                         double width, double height, double depth);
    OPResult ChangeColor(string name, string color);
    OPResult FindShape(string name);  // Returns OPResult.Object(shape) or OPResult.Error()
    OPResult DeleteShape(string name);
    OPResult GetAllShapes();
}
```

### Tool → Editor → Model Flow

```
User: "Make cube1 green"
        │
        ▼
┌─────────────────────────┐
│  Shape3DTech (Tool)     │  ← Receives LLM tool call
│  ChangeColor(name,color)│
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│  Shape3DEditor          │  ← Executes business logic
│  - Finds shape by name  │
│  - Validates operation  │
│  - Updates color        │
│  - Returns OPResult     │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│  FoStage3D / FoShape3D  │  ← Model layer
│  (Actual shape objects) │
└─────────────────────────┘
```

---

## Debugging Checklist

When tools aren't working correctly:

### 1. Check Tool Discovery
```
✅ [14:00:12] 📋 Found 8 ITechnician interfaces
✅ [14:00:12] ✓ IShape3DTech -> Shape3DTech: 27 tools
```

### 2. Check Tool Execution
```
✅ [14:00:24] 🔧 Executing tool: ChangeColor
```

### 3. Check OPResult Serialization
```
✅ Tool result value: {
     "ResultType": "Success",
     "HasError": false,
     "ResultMessage": "Changed color of 'cube1' to 'red'"
   }
```

### 4. Check Tool Selection
If the LLM calls the **wrong tool** (e.g., GetShapes instead of ChangeColor):
- Review the system prompt for ambiguous instructions
- Add explicit examples showing when to use each tool
- Make the "explicit name = direct action" rule clearer

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 26, 2025 | Initial guide based on proven Three2025 patterns |
| 1.1 | Jan 4, 2026 | Added OPResult pattern, agent prompt guidelines, editor pattern, debugging checklist |

---

## Key Takeaways

✅ **Use Scoped services** to match Blazor/web request lifecycle  
✅ **[Description] is the gate** - only marked methods become tools  
✅ **Describe parameters** to guide LLM on proper usage  
✅ **ALL technician methods return OPResult** - enables meaningful LLM feedback  
✅ **OPResult has public properties** - ResultType, HasError, ResultMessage for JSON serialization  
✅ **Agent prompts must distinguish explicit names from ambiguous references**  
✅ **Explicit name = direct action** - don't verify what the user told you  
✅ **Connect context in page lifecycle** (SetStage, etc.)  
✅ **Tools are discovered once** and shared across agents  
✅ **Log everything** for debugging tool discovery issues  

This pattern enables rapid creation of AI-accessible APIs with minimal boilerplate and maximum type safety.
