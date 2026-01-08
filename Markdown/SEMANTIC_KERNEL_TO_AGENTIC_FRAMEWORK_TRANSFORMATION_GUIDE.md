# Semantic Kernel to Microsoft Agentic Framework Transformation Guide

**Project**: Three2025 Framework  
**Date**: January 8, 2026  
**Audience**: Future AI assistants and development teams  
**Purpose**: Complete playbook for migrating from Microsoft Semantic Kernel to Microsoft.Extensions.AI Agentic Framework

---

## Executive Summary

This document captures the complete transformation journey from a Semantic Kernel-based AI system to Microsoft's modern Agentic Framework using `Microsoft.Extensions.AI`. This migration was completed over a month-long effort and resulted in a robust multi-agent chat system with automatic tool discovery and specialized domain agents.

**What Changed:**
- ❌ **Removed**: Semantic Kernel v1.32.0 with `[KernelFunction]` decorators
- ✅ **Added**: Microsoft.Extensions.AI with reflection-based tool discovery
- 🔄 **Transformed**: Single SK kernel orchestrator → Multi-agent orchestration with specialized agents
- 🛠️ **Modernized**: Plugin system → ITechnician interface-based tool providers

**Key Achievement**: Zero-boilerplate tool registration - any method marked with `[Description]` on an `ITechnician` implementation is automatically available to ALL AI agents.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Core Concepts](#core-concepts)
3. [Migration Strategy](#migration-strategy)
4. [Step-by-Step Transformation](#step-by-step-transformation)
5. [Agent System Architecture](#agent-system-architecture)
6. [Tool Discovery Pattern](#tool-discovery-pattern)
7. [Testing & Validation](#testing-validation)
8. [Lessons Learned](#lessons-learned)
9. [Common Pitfalls](#common-pitfalls)

---

## Architecture Overview

### Before: Semantic Kernel Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    ApprenticeAI.cs                          │
│              (Semantic Kernel Orchestrator)                 │
│                                                              │
│  • Creates Kernel instance                                  │
│  • Registers Plugins (LightingTech, ClockTech, etc.)       │
│  • Manually imports [KernelFunction] methods               │
│  • Single point of coordination                             │
│                                                              │
│  ┌─────────────────────────────────────────────────────┐   │
│  │  Technician Classes                                 │   │
│  │  • LightingTech.cs                                  │   │
│  │  • ClockTech.cs                                     │   │
│  │  • CageTech.cs                                      │   │
│  │                                                      │   │
│  │  [KernelFunction("add_light")]                      │   │
│  │  public void AddLight(...)                          │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘

Problems:
- Manual plugin registration required
- Single orchestrator (no agent specialization)
- Tight coupling to SK framework
- [KernelFunction] attributes lock you into SK
```

### After: Microsoft.Extensions.AI Agentic Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Application Startup                           │
│                          (Program.cs)                                │
└─────────────────────────────┬───────────────────────────────────────┘
                              │
                  ┌───────────▼────────────┐
                  │ Dependency Injection   │
                  │ • ITechnicianToolProvider │
                  │ • IChatOrchestrator    │
                  │ • IAgentFactory        │
                  │ • IShape3DTech         │
                  │ • IModelTech           │
                  └───────────┬────────────┘
                              │
              ┌───────────────▼────────────────┐
              │  TechnicianToolProvider        │
              │  .DiscoverAllTools()           │
              │                                 │
              │  1. Find all ITechnician ifaces│
              │  2. Resolve implementations    │
              │  3. Scan for [Description]     │
              │  4. Create AIFunction wrappers │
              └───────────┬────────────────────┘
                          │
                   All Tools (AIFunction[])
                          │
              ┌───────────▼────────────────┐
              │   ChatOrchestrator         │
              │   • Receives all tools     │
              │   • Routes to agents       │
              │   • Handles conversation   │
              └───────────┬────────────────┘
                          │
        ┌─────────────────┼─────────────────┐
        │                 │                 │
   ┌────▼────┐      ┌────▼────┐      ┌────▼────┐
   │  3D     │      │ Knowledge│      │ General │
   │ Modeling│      │ Modeling │      │  Agent  │
   │  Agent  │      │  Agent   │      │         │
   └─────────┘      └──────────┘      └─────────┘
   Shape3DTech      ModelTech         All Tools
   tools only       tools only        available

Benefits:
✅ Zero-boilerplate tool registration
✅ Specialized agents with context awareness
✅ Framework-agnostic [Description] attributes
✅ Automatic discovery via reflection
✅ Tool sharing across agents
```

---

## Core Concepts

### 1. ITechnician - The Marker Interface

**Purpose**: Identifies classes that provide tools to AI agents

```csharp
namespace Three2025.Apprentice;

/// <summary>
/// Marker interface for classes that provide tools to AI agents.
/// Any interface inheriting from ITechnician will be automatically discovered.
/// </summary>
public interface ITechnician
{
    // Empty marker interface - used for discovery only
}
```

**Key Insight**: This is your "plugin system" - any interface inheriting from `ITechnician` becomes discoverable.

### 2. Tool Definition Pattern

**OLD (Semantic Kernel):**
```csharp
using Microsoft.SemanticKernel;

[KernelFunction("add_light")]
[Description("Adds a new light to the scene")]
public LightingComponent AddLight(
    string name,
    string color,
    bool isOn)
{
    // implementation
}
```

**NEW (Microsoft.Extensions.AI):**
```csharp
using System.ComponentModel;

[Description("Adds a new light to the scene")]
public LightingComponent AddLight(
    [Description("The name of the light")] string name,
    [Description("The color of the light")] string color,
    [Description("Whether the light should start on")] bool isOn)
{
    // implementation
}
```

**Changes:**
- ❌ Remove `using Microsoft.SemanticKernel`
- ❌ Remove `[KernelFunction]` attribute
- ✅ Keep `[Description]` on method
- ✅ Add `[Description]` on parameters (helps LLM understand)
- ✅ Use standard `System.ComponentModel.Description`

### 3. Automatic Tool Discovery

The magic happens in `TechnicianToolProvider`:

```csharp
public IEnumerable<AIFunction> DiscoverAllTools()
{
    // 1. Find ALL interfaces that inherit from ITechnician
    var technicianTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(ITechnician).IsAssignableFrom(t) && 
                   t.IsInterface && 
                   t != typeof(ITechnician))
        .ToList();
    
    // 2. For each interface, get implementation from DI
    foreach (var interfaceType in technicianTypes)
    {
        var implementation = _serviceProvider.GetService(interfaceType);
        
        // 3. Find methods with [Description] attribute
        var methods = implementationType.GetMethods()
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null)
            .ToList();
        
        // 4. Convert each method to AIFunction
        foreach (var method in methods)
        {
            var func = AIFunctionFactory.Create(
                method,
                target: implementation,
                name: method.Name,
                description: descAttr.Description);
            
            allTools.Add(func);
        }
    }
    
    return allTools;
}
```

**Key Insight**: This runs ONCE at startup. All discovered tools are cached and shared across all agents.

### 4. Specialized Agents

Each agent is context-aware and receives a filtered set of tools:

```csharp
// 3D Modeling Agent - Only gets Shape3DTech tools
var shape3DTools = _toolProvider.GetToolsFor<IShape3DTech>();
var modelingAgent = new ThreeDModelingAgent(_chatService, shape3DTools, logger);

// Knowledge Modeling Agent - Only gets ModelTech tools
var modelTools = _toolProvider.GetToolsFor<IModelTech>();
var knowledgeAgent = new KnowledgeModelingAgent(_chatService, modelTools, logger);

// General Agent - Gets ALL tools
var generalAgent = new GeneralAgent(_chatService, allTools, logger);
```

**Benefits:**
- Agents have focused capabilities
- System prompts can be specific
- Reduces token usage (smaller tool lists)
- Prevents tool confusion

---

## Migration Strategy

### Phase 0: Build New Infrastructure (No Breaking Changes)

**Goal**: Create new system alongside existing SK system

**Tasks:**
1. ✅ Create `AgentToolAttribute.cs` (optional, future-proofing)
2. ✅ Create `ITechnicianToolProvider.cs` interface
3. ✅ Implement `TechnicianToolProvider.cs` with SK compatibility
4. ✅ Create `ISpecializedAgent.cs` interface
5. ✅ Create `ChatOrchestrator.cs` for agent coordination
6. ✅ Create `AgentFactory.cs` for agent creation
7. ✅ Create first specialized agents:
   - `ThreeDModelingAgent.cs`
   - `KnowledgeModelingAgent.cs`
   - `GeneralAgent.cs`

**Validation**: New system compiles, old system still works

### Phase 1: Migrate Technician Classes (One at a Time)

**Goal**: Replace `[KernelFunction]` with `[Description]` attributes

**Process for Each Technician:**

1. **Identify Current State**
   ```bash
   # Find all KernelFunction decorators
   grep -r "\[KernelFunction" Apprentice/*.cs
   ```

2. **Create Migration Branch**
   ```bash
   git checkout -b migrate/shape3d-tech
   ```

3. **Edit Technician File**
   - Remove: `using Microsoft.SemanticKernel;`
   - Replace: `[KernelFunction("tool_name")]` → `[Description("tool description")]`
   - Add parameter descriptions
   - Keep method signatures identical

4. **Test Discovery**
   - Run application
   - Check logs for tool discovery count
   - Verify tool appears in Tool Discovery Test page

5. **Test Via Chat**
   - Ask agent to use the tool
   - Verify execution works
   - Check return values

6. **Merge**
   ```bash
   git merge migrate/shape3d-tech
   ```

**Example Migration:**

```csharp
// BEFORE
using Microsoft.SemanticKernel;

[KernelFunction("add_shape")]
public List<ShapeInfo> AddShape(string name, string color, string type)
{
    // ...
}

// AFTER
using System.ComponentModel;

[Description("Create and add a 3D shape to the geometry stage")]
public List<ShapeInfo> AddShape(
    [Description("The name of the shape")] string name,
    [Description("The color of the shape (red, blue, green, etc.)")] string color,
    [Description("The type of shape: box, sphere, cylinder, cone, torus")] string type)
{
    // ...
}
```

**Technicians to Migrate:**
- ✅ Shape3DTech (3D modeling)
- ✅ Shape2DTech (2D drawing)
- ✅ ModelTech (knowledge modeling)
- ✅ ClockTech (clock creation)
- ✅ CuckooClockTech (specialized clocks)
- ✅ Others as needed

### Phase 2: Integrate Agents with Tools

**Goal**: Wire up agents to use discovered tools

**Tasks:**

1. **Update ChatOrchestrator Constructor**
   ```csharp
   public ChatOrchestrator(
       IMultiProviderChatService chatService,
       ITechnicianToolProvider toolProvider,
       IAgentFactory agentFactory,
       ILogger<ChatOrchestrator> logger)
   {
       // Discover all tools at startup
       _technicianTools.AddRange(toolProvider.DiscoverAllTools());
       _logger.LogInformation($"✅ Loaded {_technicianTools.Count} tools");
       
       // Initialize agents with tools
       InitializeAgents();
   }
   ```

2. **Create Specialized Agents with Filtered Tools**
   ```csharp
   private void InitializeAgents()
   {
       // 3D Modeling Agent - Shape3DTech tools only
       var shape3DTools = _toolProvider.GetToolsFor<IShape3DTech>();
       var modelingAgent = _agentFactory.Create3DModelingAgent(shape3DTools);
       RegisterAgent(modelingAgent);
       
       // Knowledge Modeling Agent - ModelTech tools only
       var modelTools = _toolProvider.GetToolsFor<IModelTech>();
       var knowledgeAgent = _agentFactory.CreateKnowledgeModelingAgent(modelTools);
       RegisterAgent(knowledgeAgent);
       
       // General Agent - All tools
       var generalAgent = _agentFactory.CreateGeneralAgent(_technicianTools);
       RegisterAgent(generalAgent);
   }
   ```

3. **Update Each Agent to Accept and Use Tools**
   ```csharp
   public class ThreeDModelingAgent : ISpecializedAgent
   {
       private readonly IMultiProviderChatService _chatService;
       private readonly List<AIFunction> _tools; // ← Tools injected here
       
       public ThreeDModelingAgent(
           IMultiProviderChatService chatService,
           IEnumerable<AIFunction> tools, // ← Receive tools
           ILogger<ThreeDModelingAgent> logger)
       {
           _chatService = chatService;
           _tools = tools.ToList();
           _logger = logger;
       }
       
       public async Task<string> ProcessAsync(...)
       {
           // Pass tools to LLM
           await foreach (var chunk in _chatService.SendMessageStreamingAsync(
               userMessage, 
               messages, 
               _tools, // ← Tools available to LLM
               cancellationToken))
           {
               response += chunk;
           }
           
           return response;
       }
   }
   ```

### Phase 3: Remove Semantic Kernel Infrastructure

**Goal**: Delete all SK dependencies and code

**Tasks:**

1. **Delete ApprenticeAI.cs**
   - This was the SK orchestrator
   - No longer needed - ChatOrchestrator handles this now
   ```bash
   git rm Apprentice/ApprenticeAI.cs
   ```

2. **Remove Package Reference**
   ```xml
   <!-- Three2025.csproj - REMOVE THIS -->
   <PackageReference Include="Microsoft.SemanticKernel" Version="1.32.0" />
   ```

3. **Remove SK Compatibility from TechnicianToolProvider**
   ```csharp
   // REMOVE
   using Microsoft.SemanticKernel;
   
   // REMOVE SK attribute checking
   var kernelAttr = method.GetCustomAttribute<KernelFunctionAttribute>();
   ```

4. **Remove from DI Registration**
   ```csharp
   // Program.cs - REMOVE
   // builder.Services.AddScoped<IApprenticeAI, ApprenticeAI>();
   ```

5. **Search and Destroy**
   ```bash
   # Verify zero SK references
   grep -r "Microsoft.SemanticKernel" **/*.cs
   # Should return: No matches
   
   grep -r "KernelFunction" **/*.cs
   # Should return: No matches
   ```

### Phase 4: Testing & Validation

**Goal**: Ensure system works end-to-end

**Test Matrix:**

| Test Category | Test Cases | Status |
|--------------|------------|--------|
| **Tool Discovery** | All ITechnician interfaces found | ✅ |
| | Correct tool count | ✅ |
| | Tools cached properly | ✅ |
| **Agent Creation** | 3D Modeling Agent created | ✅ |
| | Knowledge Modeling Agent created | ✅ |
| | General Agent created | ✅ |
| | Agents receive correct tools | ✅ |
| **Tool Execution** | Shape3DTech: AddShape | ✅ |
| | Shape3DTech: RepositionShape | ✅ |
| | ModelTech: establish_model | ✅ |
| | ModelTech: add_component | ✅ |
| **Chat Integration** | Message routing to correct agent | ✅ |
| | Streaming responses | ✅ |
| | Tool calls execute | ✅ |
| | Multi-turn conversations | ✅ |
| **Error Handling** | Missing tool gracefully handled | ✅ |
| | Invalid parameters rejected | ✅ |
| | Tool exceptions logged | ✅ |

---

## Step-by-Step Transformation

### Step 1: Create ITechnician Marker Interface

**File**: `Apprentice/ITechnician.cs`

```csharp
namespace Three2025.Apprentice;

/// <summary>
/// Marker interface for technician classes that provide AI tools.
/// Any interface inheriting from ITechnician will be automatically discovered
/// by the TechnicianToolProvider and its methods exposed to AI agents.
/// </summary>
public interface ITechnician
{
    // Empty - used only for discovery
}
```

### Step 2: Define Specific Technician Interfaces

**File**: `Apprentice/IShape3DTech.cs`

```csharp
namespace Three2025.Apprentice;

/// <summary>
/// 3D shape creation and manipulation tools.
/// Inherits ITechnician for automatic discovery.
/// </summary>
public interface IShape3DTech : ITechnician
{
    void SetStage(FoStage3D stage);
    List<ShapeInfo> GetShapes();
    List<ShapeInfo> AddShape(string name, bool isOn, string color, string shapeType);
    List<ShapeInfo> RepositionShape(string name, double x, double y, double z);
    List<ShapeInfo> RotateShape(string name, double x, double y, double z);
    List<ShapeInfo> ScaleShape(string name, double scaleX, double scaleY, double scaleZ);
    List<ShapeInfo> ChangeColor(string name, string color);
    List<ShapeInfo> DeleteShape(string name);
    // ... more methods
}
```

**File**: `Apprentice/IModelTech.cs`

```csharp
namespace Three2025.Apprentice;

/// <summary>
/// Knowledge modeling and structured design tools.
/// Inherits ITechnician for automatic discovery.
/// </summary>
public interface IModelTech : ITechnician
{
    void SetMentor(IModelMentor mentor);
    ModelInfo establish_model(string title);
    ModelInfo add_component(string name);
    ModelInfo set_parameter(string name, string payload);
    // ... more methods
}
```

### Step 3: Implement Technician Classes

**File**: `Apprentice/Shape3DTech.cs`

```csharp
using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;

namespace Three2025.Apprentice;

public class Shape3DTech : IShape3DTech
{
    private FoStage3D? _stage;
    
    public void SetStage(FoStage3D stage)
    {
        _stage = stage;
    }
    
    [Description("Create and add a 3D shape to the geometry stage")]
    public List<ShapeInfo> AddShape(
        [Description("The name of the shape to create")] string name,
        [Description("Whether the shape should be visible/active")] bool isOn,
        [Description("The color of the shape (red, blue, green, etc.)")] string color,
        [Description("The type of shape: box, sphere, cylinder, cone, torus, tetrahedron, octahedron, dodecahedron, icosahedron")] 
        string shapeType = "box")
    {
        if (_stage == null)
            throw new InvalidOperationException("Stage not initialized. Call SetStage first.");
        
        var shape = CreateGeometryShape(name, shapeType);
        shape.IsOn = isOn;
        shape.SetColor(color);
        
        _stage.AddShape(shape);
        RefreshUI();
        
        return GetShapes();
    }
    
    [Description("Get a list of all 3D shapes currently in the scene with their properties")]
    public List<ShapeInfo> GetShapes()
    {
        if (_stage == null) return new List<ShapeInfo>();
        
        return _stage.Members<GeometryShape>()
            .Select(s => ConvertToShapeInfo(s))
            .ToList();
    }
    
    [Description("Move a shape to a new absolute position in 3D space")]
    public List<ShapeInfo> RepositionShape(
        [Description("The name of the shape to move")] string name,
        [Description("X coordinate (left-/right+)")] double x,
        [Description("Y coordinate (down-/up+)")] double y,
        [Description("Z coordinate (back-/forward+)")] double z)
    {
        var shape = FindShape(name);
        if (shape == null)
            throw new ArgumentException($"Shape '{name}' not found");
        
        shape.Position.Set(x, y, z);
        RefreshUI();
        
        return GetShapes();
    }
    
    // ... more methods with [Description] attributes
}
```

**Key Points:**
- ✅ Use `[Description]` on methods you want to expose
- ✅ Use `[Description]` on parameters to help LLM understand
- ✅ Methods without `[Description]` are NOT exposed (like `SetStage`)
- ✅ Return useful data structures (DTOs, lists)
- ✅ Throw meaningful exceptions

### Step 4: Create Tool Provider

**File**: `Services/Agents/ITechnicianToolProvider.cs`

```csharp
using Microsoft.Extensions.AI;

namespace Three2025.Services.Agents;

/// <summary>
/// Discovers and provides tools from ITechnician implementations to AI agents.
/// </summary>
public interface ITechnicianToolProvider
{
    /// <summary>
    /// Discovers all tools from all registered ITechnician services
    /// </summary>
    IEnumerable<AIFunction> DiscoverAllTools();
    
    /// <summary>
    /// Get tools from a specific technician type
    /// </summary>
    IEnumerable<AIFunction> GetToolsFor<T>() where T : class;
    
    /// <summary>
    /// Get tools from a specific technician by interface type
    /// </summary>
    IEnumerable<AIFunction> GetToolsFor(Type technicianInterface);
}
```

**File**: `Services/Agents/TechnicianToolProvider.cs`

```csharp
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;
using Three2025.Apprentice;

namespace Three2025.Services.Agents;

public class TechnicianToolProvider : ITechnicianToolProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TechnicianToolProvider> _logger;
    private readonly Dictionary<Type, List<AIFunction>> _toolCache = new();
    
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
        
        // Find all ITechnician interfaces (excluding ITechnician itself)
        var technicianTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ITechnician).IsAssignableFrom(t) && 
                       t.IsInterface && 
                       t != typeof(ITechnician))
            .ToList();
        
        _logger.LogInformation($"📋 Found {technicianTypes.Count} technician interfaces");
        
        var allTools = new List<AIFunction>();
        
        foreach (var interfaceType in technicianTypes)
        {
            try
            {
                var tools = GetToolsFor(interfaceType);
                allTools.AddRange(tools);
                _logger.LogInformation($"  ➡️  {interfaceType.Name}: {tools.Count()} tools");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to discover tools from {interfaceType.Name}");
            }
        }
        
        _logger.LogInformation($"✅ Discovered {allTools.Count} total tools");
        return allTools;
    }
    
    public IEnumerable<AIFunction> GetToolsFor<T>() where T : class
    {
        return GetToolsFor(typeof(T));
    }
    
    public IEnumerable<AIFunction> GetToolsFor(Type technicianInterface)
    {
        // Check cache first
        lock (_toolCache)
        {
            if (_toolCache.TryGetValue(technicianInterface, out var cachedTools))
                return cachedTools;
        }
        
        // Extract tools
        var tools = ExtractToolsFromTechnician(technicianInterface);
        
        // Cache for future use
        lock (_toolCache)
        {
            _toolCache[technicianInterface] = tools;
        }
        
        return tools;
    }
    
    private List<AIFunction> ExtractToolsFromTechnician(Type interfaceType)
    {
        var tools = new List<AIFunction>();
        
        // Get the concrete implementation from DI
        var implementation = _serviceProvider.GetService(interfaceType);
        if (implementation == null)
        {
            _logger.LogWarning($"⚠️  No implementation registered for {interfaceType.Name}");
            return tools;
        }
        
        var implementationType = implementation.GetType();
        
        // Find methods with [Description] attribute
        var methods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null)
            .ToList();
        
        _logger.LogInformation($"  {interfaceType.Name}: Found {methods.Count} tool methods");
        
        foreach (var method in methods)
        {
            try
            {
                var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                
                // Create AIFunction using AIFunctionFactory
                var func = AIFunctionFactory.Create(
                    method,
                    target: implementation,
                    name: method.Name,
                    description: descAttr!.Description);
                
                tools.Add(func);
                _logger.LogDebug($"    ✓ {method.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"    ✗ Failed to convert {method.Name} to AIFunction");
            }
        }
        
        return tools;
    }
}
```

### Step 5: Create Agent Interface and Base Implementation

**File**: `Services/Chat/ISpecializedAgent.cs`

```csharp
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

/// <summary>
/// Interface for specialized AI agents with context awareness
/// </summary>
public interface ISpecializedAgent
{
    /// <summary>
    /// Agent name for display and routing
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Description of agent capabilities
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Determines if this agent should handle requests in given context
    /// </summary>
    bool IsRelevantForContext(PageContext context);
    
    /// <summary>
    /// Process a user message and return complete response
    /// </summary>
    Task<string> ProcessAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process a user message and stream response chunks
    /// </summary>
    IAsyncEnumerable<string> ProcessStreamingAsync(
        string userMessage,
        PageContext context,
        List<ChatMessage> conversationHistory,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Context information about the page where chat is embedded
/// </summary>
public record PageContext(
    string PageName,
    string PageRoute,
    string DomainFocus);
```

### Step 6: Create Specialized Agents

**File**: `Services/Chat/Agents/ThreeDModelingAgent.cs`

```csharp
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat.Agents;

public class ThreeDModelingAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger<ThreeDModelingAgent> _logger;
    
    public string Name => "Shape3D Technician";
    
    public string Description => "Creates and manipulates 3D geometry using Shape3DTech tools";
    
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
        var relevantKeywords = new[] { "geometry", "3d", "model", "shape" };
        return relevantKeywords.Any(k => 
            context.PageName.Contains(k, StringComparison.OrdinalIgnoreCase));
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
        [System.Runtime.CompilerServices.EnumeratorCancellation] 
        CancellationToken cancellationToken = default)
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
        return $"""
            You are a Shape3D Technician with access to {_tools.Count} 3D modeling tools.
            
            CRITICAL: You EXECUTE tools immediately, not just describe what could be done.
            
            ## Available Tools
            - AddShape(name, isOn, color, shapeType) - Create 3D geometry
            - RepositionShape(name, x, y, z) - Move shapes
            - RotateShape(name, x, y, z) - Rotate shapes
            - ScaleShape(name, scaleX, scaleY, scaleZ) - Resize shapes
            - ChangeColor(name, color) - Update colors
            - DeleteShape(name) - Remove shapes
            - GetShapes() - List all shapes
            
            ## Coordinate System
            Right-handed: X (left-/right+), Y (down-/up+), Z (back-/forward+)
            Origin at (0,0,0)
            
            ## Workflow
            1. User asks to create/modify geometry
            2. You immediately USE THE TOOLS
            3. Confirm what was created/modified
            
            Always call GetShapes() first to understand current scene state.
            """;
    }
}
```

### Step 7: Create Agent Factory

**File**: `Services/Chat/IAgentFactory.cs`

```csharp
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

public interface IAgentFactory
{
    ISpecializedAgent Create3DModelingAgent(IEnumerable<AIFunction> tools);
    ISpecializedAgent CreateKnowledgeModelingAgent(IEnumerable<AIFunction> tools);
    ISpecializedAgent CreateGeneralAgent(IEnumerable<AIFunction> tools);
}
```

**File**: `Services/Chat/AgentFactory.cs`

```csharp
using Microsoft.Extensions.AI;
using Three2025.Services.Chat.Agents;

namespace Three2025.Services.Chat;

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
    
    public ISpecializedAgent Create3DModelingAgent(IEnumerable<AIFunction> tools)
        => new ThreeDModelingAgent(
            _chatService, 
            tools, 
            _loggerFactory.CreateLogger<ThreeDModelingAgent>());
    
    public ISpecializedAgent CreateKnowledgeModelingAgent(IEnumerable<AIFunction> tools)
        => new KnowledgeModelingAgent(
            _chatService, 
            tools, 
            _loggerFactory.CreateLogger<KnowledgeModelingAgent>());
    
    public ISpecializedAgent CreateGeneralAgent(IEnumerable<AIFunction> tools)
        => new GeneralAgent(
            _chatService, 
            tools, 
            _loggerFactory.CreateLogger<GeneralAgent>());
}
```

### Step 8: Create Chat Orchestrator

**File**: `Services/Chat/IChatOrchestrator.cs`

```csharp
namespace Three2025.Services.Chat;

public interface IChatOrchestrator
{
    Task<string> SendMessageAsync(
        string message,
        PageContext context,
        CancellationToken cancellationToken = default);
    
    IAsyncEnumerable<string> SendMessageStreamingAsync(
        string message,
        PageContext context,
        CancellationToken cancellationToken = default);
    
    void RegisterAgent(ISpecializedAgent agent);
    IEnumerable<ISpecializedAgent> GetAvailableAgents();
}
```

**File**: `Services/Chat/ChatOrchestrator.cs`

```csharp
using Microsoft.Extensions.AI;
using Three2025.Services.Agents;

namespace Three2025.Services.Chat;

public class ChatOrchestrator : IChatOrchestrator
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ITechnicianToolProvider _toolProvider;
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<ChatOrchestrator> _logger;
    
    private readonly Dictionary<string, ISpecializedAgent> _agents = new();
    private readonly List<AIFunction> _technicianTools = new();
    private readonly List<ChatMessage> _conversationHistory = new();
    
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
        
        // Discover all technician tools at startup
        _technicianTools.AddRange(_toolProvider.DiscoverAllTools());
        _logger.LogInformation($"✅ Loaded {_technicianTools.Count} technician tools");
        
        // Initialize specialized agents
        InitializeAgents();
    }
    
    private void InitializeAgents()
    {
        _logger.LogInformation("🔧 Initializing agents...");
        
        // General Agent - Has access to ALL tools
        var generalAgent = _agentFactory.CreateGeneralAgent(_technicianTools);
        RegisterAgent(generalAgent);
        
        // 3D Modeling Agent - Only Shape3DTech tools
        var shape3DTools = _toolProvider.GetToolsFor<IShape3DTech>();
        _logger.LogInformation($"  Shape3D Agent: {shape3DTools.Count()} tools");
        var modelingAgent = _agentFactory.Create3DModelingAgent(shape3DTools);
        RegisterAgent(modelingAgent);
        
        // Knowledge Modeling Agent - Only ModelTech tools
        var modelTools = _toolProvider.GetToolsFor<IModelTech>();
        _logger.LogInformation($"  Knowledge Agent: {modelTools.Count()} tools");
        var knowledgeAgent = _agentFactory.CreateKnowledgeModelingAgent(modelTools);
        RegisterAgent(knowledgeAgent);
        
        _logger.LogInformation($"✅ Initialized {_agents.Count} agents");
    }
    
    public void RegisterAgent(ISpecializedAgent agent)
    {
        if (agent?.Name != null)
        {
            _agents[agent.Name] = agent;
        }
    }
    
    public IEnumerable<ISpecializedAgent> GetAvailableAgents()
    {
        return _agents.Values;
    }
    
    public async Task<string> SendMessageAsync(
        string message,
        PageContext context,
        CancellationToken cancellationToken = default)
    {
        // Select most relevant agent based on context
        var agent = SelectAgent(context);
        _logger.LogInformation($"🎯 Routing to: {agent.Name}");
        
        // Add user message to history
        _conversationHistory.Add(new ChatMessage(ChatRole.User, message));
        
        // Process with selected agent
        var response = await agent.ProcessAsync(
            message, 
            context, 
            _conversationHistory, 
            cancellationToken);
        
        // Add response to history
        _conversationHistory.Add(new ChatMessage(ChatRole.Assistant, response));
        
        return response;
    }
    
    public async IAsyncEnumerable<string> SendMessageStreamingAsync(
        string message,
        PageContext context,
        [System.Runtime.CompilerServices.EnumeratorCancellation] 
        CancellationToken cancellationToken = default)
    {
        var agent = SelectAgent(context);
        _logger.LogInformation($"🎯 Routing to: {agent.Name}");
        
        _conversationHistory.Add(new ChatMessage(ChatRole.User, message));
        
        var fullResponse = "";
        await foreach (var chunk in agent.ProcessStreamingAsync(
            message, context, _conversationHistory, cancellationToken))
        {
            fullResponse += chunk;
            yield return chunk;
        }
        
        _conversationHistory.Add(new ChatMessage(ChatRole.Assistant, fullResponse));
    }
    
    private ISpecializedAgent SelectAgent(PageContext context)
    {
        // Find most relevant agent for this context
        foreach (var agent in _agents.Values)
        {
            if (agent.IsRelevantForContext(context))
            {
                return agent;
            }
        }
        
        // Default to General Agent
        return _agents.Values.FirstOrDefault(a => a.Name.Contains("General"))
            ?? _agents.Values.First();
    }
}
```

### Step 9: Register Services in DI

**File**: `Program.cs`

```csharp
// Tool discovery and agent infrastructure
builder.Services.AddSingleton<ITechnicianToolProvider, TechnicianToolProvider>();
builder.Services.AddSingleton<IAgentFactory, AgentFactory>();
builder.Services.AddSingleton<IChatOrchestrator, ChatOrchestrator>();

// Technician implementations (tools)
builder.Services.AddScoped<IShape3DTech, Shape3DTech>();
builder.Services.AddScoped<IShape2DTech, Shape2DTech>();
builder.Services.AddScoped<IModelTech, ModelTech>();
builder.Services.AddScoped<IClockTech, ClockTech>();
// ... other technicians

// Chat service (already existed)
builder.Services.AddScoped<IMultiProviderChatService, MultiProviderChatService>();

// REMOVE OLD SK SERVICES
// builder.Services.AddScoped<IApprenticeAI, ApprenticeAI>(); // ← DELETE THIS
```

---

## Agent System Architecture

### Agent Responsibility Matrix

| Agent | Domain | Tools | When to Use |
|-------|--------|-------|-------------|
| **General Agent** | Broad assistance, questions | ALL tools | Default, general queries, multi-domain tasks |
| **3D Modeling Agent** | 3D geometry creation | Shape3DTech only | GeometryTest page, "create a box", "add shapes" |
| **Knowledge Modeling Agent** | Conceptual design, BOMs | ModelTech only | ConversationalModeler, "create a model", "add component" |
| **Animation Agent** (future) | Motion, keyframes | Animation tools | Animation pages, "animate the box" |
| **Clock Agent** (future) | Clocks, cuckoo clocks | ClockTech tools | Clock pages, "create a clock face" |

### Agent Selection Logic

```csharp
private ISpecializedAgent SelectAgent(PageContext context)
{
    // Each agent implements IsRelevantForContext()
    // Check in priority order:
    
    // 1. Specialized agents first
    foreach (var agent in _agents.Values
        .Where(a => !a.Name.Contains("General")))
    {
        if (agent.IsRelevantForContext(context))
            return agent;
    }
    
    // 2. Fall back to General Agent
    return _agents["General Agent"];
}
```

**Example Context Matching:**

```csharp
// ThreeDModelingAgent
public bool IsRelevantForContext(PageContext context)
{
    var keywords = new[] { "geometry", "3d", "shape", "model" };
    return keywords.Any(k => 
        context.PageName.Contains(k, StringComparison.OrdinalIgnoreCase) ||
        context.PageRoute.Contains(k, StringComparison.OrdinalIgnoreCase));
}

// KnowledgeModelingAgent
public bool IsRelevantForContext(PageContext context)
{
    var keywords = new[] { "mentor", "modeler", "sysml", "component" };
    return keywords.Any(k => 
        context.PageName.Contains(k, StringComparison.OrdinalIgnoreCase));
}
```

---

## Tool Discovery Pattern

### How It Works

1. **Startup**: `ChatOrchestrator` constructor runs
2. **Discovery**: `TechnicianToolProvider.DiscoverAllTools()` scans assemblies
3. **Finding**: Locates all interfaces inheriting from `ITechnician`
4. **Resolution**: Gets implementations from DI container
5. **Scanning**: Reflects over methods looking for `[Description]`
6. **Wrapping**: Creates `AIFunction` wrappers using `AIFunctionFactory`
7. **Caching**: Stores in dictionary for fast lookup
8. **Distribution**: Filters and passes to agent constructors

### Tool Filtering

```csharp
// Get ALL tools
var allTools = _toolProvider.DiscoverAllTools();

// Get tools for specific technician
var shape3DTools = _toolProvider.GetToolsFor<IShape3DTech>();
var modelTools = _toolProvider.GetToolsFor<IModelTech>();

// Create agents with filtered tools
var modelingAgent = new ThreeDModelingAgent(_chatService, shape3DTools, logger);
var knowledgeAgent = new KnowledgeModelingAgent(_chatService, modelTools, logger);
var generalAgent = new GeneralAgent(_chatService, allTools, logger);
```

### Tool Execution Flow

```
User: "Create a red box called TestBox"
    ↓
ChatOrchestrator.SendMessageAsync()
    ↓
SelectAgent(context) → ThreeDModelingAgent
    ↓
ThreeDModelingAgent.ProcessAsync()
    ↓
_chatService.SendMessageStreamingAsync(message, history, _tools)
    ↓
LLM sees tools, decides to call AddShape
    ↓
AIFunction.InvokeAsync() calls Shape3DTech.AddShape()
    ↓
Shape3DTech creates GeometryShape, adds to stage
    ↓
Returns List<ShapeInfo> to LLM
    ↓
LLM generates response: "Created red box 'TestBox' at origin"
    ↓
Response streamed back to user
```

---

## Testing & Validation

### Unit Test Example

```csharp
[Fact]
public void TechnicianToolProvider_ShouldDiscoverAllTools()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<IShape3DTech, Shape3DTech>();
    services.AddScoped<IModelTech, ModelTech>();
    var provider = services.BuildServiceProvider();
    
    var toolProvider = new TechnicianToolProvider(
        provider, 
        Mock.Of<ILogger<TechnicianToolProvider>>());
    
    // Act
    var tools = toolProvider.DiscoverAllTools().ToList();
    
    // Assert
    Assert.True(tools.Count > 0, "Should discover at least one tool");
    Assert.Contains(tools, t => t.Name == "AddShape");
    Assert.Contains(tools, t => t.Name == "establish_model");
}
```

### Integration Test Example

```csharp
[Fact]
public async Task ChatOrchestrator_ShouldCallShape3DTechTool()
{
    // Arrange
    var orchestrator = CreateOrchestrator();
    var context = new PageContext("GeometryTest", "/geometry", "3D");
    
    // Act
    var response = await orchestrator.SendMessageAsync(
        "Create a red box called TestBox",
        context);
    
    // Assert
    Assert.Contains("TestBox", response);
    Assert.Contains("red", response, StringComparison.OrdinalIgnoreCase);
    
    // Verify tool was called
    var shapes = _shape3DTech.GetShapes();
    Assert.Contains(shapes, s => s.Name == "TestBox");
}
```

### Manual Testing Checklist

- [ ] **Tool Discovery Test Page**: Verify all tools listed
- [ ] **Chat on GeometryTest**: "Create a blue sphere" → works
- [ ] **Chat on ConversationalModeler**: "Create a model called TestModel" → works
- [ ] **Multi-turn conversation**: History preserved across turns
- [ ] **Tool parameters**: Complex parameters parsed correctly
- [ ] **Error handling**: Invalid tool calls gracefully handled
- [ ] **Streaming**: Responses stream character-by-character
- [ ] **Agent switching**: Different pages use different agents

---

## Lessons Learned

### 1. Start with Marker Interface Pattern

**Why**: Enables automatic discovery without manual registration

**Implementation**:
```csharp
public interface ITechnician { } // Marker

public interface IShape3DTech : ITechnician { } // Discoverable
```

**Benefit**: Add new technician? Just inherit from ITechnician. Tools automatically available.

### 2. Use [Description] for Everything

**Why**: Framework-agnostic, works with multiple AI libraries

**Implementation**:
```csharp
[Description("Creates a 3D shape")] // Method description
public List<ShapeInfo> AddShape(
    [Description("Name of the shape")] string name, // Parameter description
    [Description("Color as hex or name")] string color)
```

**Benefit**: LLM understands tool purpose AND parameter meaning

### 3. Return Useful Data Structures

**Why**: LLM needs context about current state

**Bad**:
```csharp
public void AddShape(string name) { } // No feedback
```

**Good**:
```csharp
public List<ShapeInfo> AddShape(string name) 
{
    // ... create shape
    return GetShapes(); // Return current state
}
```

**Benefit**: LLM can confirm action and see results

### 4. Tool Filtering is Critical

**Why**: Reduces token usage, prevents confusion

**Implementation**:
```csharp
// Give 3D agent ONLY 3D tools
var shape3DTools = _toolProvider.GetToolsFor<IShape3DTech>();
var agent = new ThreeDModelingAgent(_chatService, shape3DTools, logger);
```

**Benefit**: Agent sees 10 tools instead of 100 tools

### 5. Context-Aware Agent Selection

**Why**: Right tool for the job

**Implementation**:
```csharp
public bool IsRelevantForContext(PageContext context)
{
    return context.PageName.Contains("Geometry") ||
           context.DomainFocus == "3D";
}
```

**Benefit**: User on GeometryTest page automatically gets 3D agent

### 6. Cache Tool Discovery

**Why**: Reflection is expensive, only do it once

**Implementation**:
```csharp
private readonly Dictionary<Type, List<AIFunction>> _toolCache = new();

public IEnumerable<AIFunction> GetToolsFor(Type type)
{
    if (_toolCache.TryGetValue(type, out var cached))
        return cached;
    
    var tools = ExtractTools(type);
    _toolCache[type] = tools;
    return tools;
}
```

**Benefit**: Second call is instant

### 7. Logging is Essential

**Why**: Debugging tool discovery and execution

**Implementation**:
```csharp
_logger.LogInformation($"✅ Discovered {tools.Count} tools");
_logger.LogInformation($"🎯 Routing to: {agent.Name}");
_logger.LogDebug($"    ✓ {toolName}: {description}");
```

**Benefit**: See what's happening at startup and runtime

---

## Common Pitfalls

### ❌ Forgetting to Register Technician in DI

**Problem**: Tool provider can't find implementation

**Solution**:
```csharp
// Program.cs
builder.Services.AddScoped<IShape3DTech, Shape3DTech>();
```

**Symptom**: Log shows "No implementation registered for IShape3DTech"

### ❌ Not Using [Description] Attribute

**Problem**: Method not discovered as tool

**Wrong**:
```csharp
public void AddShape(string name) { } // Not discovered!
```

**Right**:
```csharp
[Description("Creates a shape")]
public void AddShape(string name) { } // Discovered!
```

### ❌ Forgetting to Pass Tools to Agent

**Problem**: Agent has no tools, can't do anything

**Wrong**:
```csharp
var agent = new ThreeDModelingAgent(_chatService, logger);
```

**Right**:
```csharp
var tools = _toolProvider.GetToolsFor<IShape3DTech>();
var agent = new ThreeDModelingAgent(_chatService, tools, logger);
```

### ❌ Not Calling GetShapes() Before Operating

**Problem**: LLM doesn't know current scene state

**Solution**: Always start system prompt with instruction to call GetShapes()

```csharp
"""
ALWAYS call GetShapes() first to understand current scene before creating new shapes.
"""
```

### ❌ Returning void Instead of Data

**Problem**: LLM gets no feedback

**Bad**:
```csharp
public void AddShape(string name) { } // Silent
```

**Good**:
```csharp
public List<ShapeInfo> AddShape(string name) 
{ 
    // ...
    return GetShapes(); // Feedback
}
```

### ❌ Not Handling Nulls/Missing Resources

**Problem**: NullReferenceException crashes tool

**Solution**:
```csharp
[Description("Add a shape")]
public List<ShapeInfo> AddShape(string name)
{
    if (_stage == null)
        throw new InvalidOperationException("Stage not initialized");
    
    // ... safe to use _stage
}
```

### ❌ Assuming Migration is Instant

**Problem**: Changing too much at once breaks everything

**Solution**: Phased migration (Phase 0-4), validate each phase

---

## Quick Reference

### File Checklist for New Projects

When transforming a Semantic Kernel app to Agentic Framework:

**Core Infrastructure:**
- [ ] `ITechnician.cs` - Marker interface
- [ ] `ITechnicianToolProvider.cs` - Discovery interface
- [ ] `TechnicianToolProvider.cs` - Discovery implementation
- [ ] `ISpecializedAgent.cs` - Agent interface
- [ ] `IChatOrchestrator.cs` - Orchestrator interface
- [ ] `ChatOrchestrator.cs` - Orchestrator implementation
- [ ] `IAgentFactory.cs` - Factory interface
- [ ] `AgentFactory.cs` - Factory implementation

**Technician Interfaces:**
- [ ] `IShape3DTech.cs` - 3D tools
- [ ] `IShape2DTech.cs` - 2D tools
- [ ] `IModelTech.cs` - Knowledge modeling
- [ ] ... others as needed

**Technician Implementations:**
- [ ] `Shape3DTech.cs`
- [ ] `Shape2DTech.cs`
- [ ] `ModelTech.cs`
- [ ] ... others as needed

**Specialized Agents:**
- [ ] `ThreeDModelingAgent.cs`
- [ ] `KnowledgeModelingAgent.cs`
- [ ] `GeneralAgent.cs`
- [ ] ... others as needed

**Registration:**
- [ ] Update `Program.cs` DI registration

**Removal:**
- [ ] Delete `ApprenticeAI.cs` (SK orchestrator)
- [ ] Remove SK package from `.csproj`
- [ ] Remove all `using Microsoft.SemanticKernel`
- [ ] Remove all `[KernelFunction]` attributes

### Key Namespaces

```csharp
// Core .NET
using System.ComponentModel; // For [Description]
using System.Reflection;     // For tool discovery

// Microsoft Agentic Framework
using Microsoft.Extensions.AI; // AIFunction, ChatMessage, etc.

// Your domain
using Three2025.Apprentice;        // ITechnician, IShape3DTech, etc.
using Three2025.Services.Agents;   // TechnicianToolProvider
using Three2025.Services.Chat;     // ChatOrchestrator, agents
```

### Package References

```xml
<ItemGroup>
  <!-- Microsoft.Extensions.AI - Core framework -->
  <PackageReference Include="Microsoft.Extensions.AI" Version="10.1.1" />
  
  <!-- Provider-specific packages -->
  <PackageReference Include="Microsoft.Extensions.AI.Ollama" Version="9.0.1-preview.1.24570.5" />
  <PackageReference Include="Microsoft.Extensions.AI.AzureAIInference" Version="10.0.0-preview.1.25559.3" />
  
  <!-- DO NOT INCLUDE -->
  <!-- <PackageReference Include="Microsoft.SemanticKernel" Version="1.32.0" /> -->
</ItemGroup>
```

### Migration Commands

```bash
# 1. Find all KernelFunction uses
grep -r "\[KernelFunction" **/*.cs

# 2. Find all SK imports
grep -r "using Microsoft.SemanticKernel" **/*.cs

# 3. Verify zero SK references after migration
grep -r "SemanticKernel" **/*.cs
# Should return: No matches

# 4. Test tool discovery
dotnet run
# Check logs for "Discovered X total tools"

# 5. Test agent creation
# Check logs for "Initialized X agents"
```

---

## Conclusion

This transformation from Semantic Kernel to Microsoft's Agentic Framework represents a significant architectural improvement:

1. **Reduced Boilerplate**: From manual plugin registration to automatic discovery
2. **Improved Modularity**: Specialized agents with focused capabilities
3. **Framework Independence**: Standard `[Description]` attributes instead of SK-specific ones
4. **Better Scalability**: Easy to add new technicians and agents
5. **Enhanced Testing**: Clear separation of concerns enables better testing

**The Pattern Works**: Once established, adding new capabilities is trivial:
- New technician? Create interface inheriting ITechnician, implement with [Description] methods
- New agent? Create class implementing ISpecializedAgent, register in factory
- New tool? Add [Description] method to existing technician

This document serves as a complete playbook for future AI assistants or developers undertaking similar transformations. The patterns and principles are transferable to any project moving from Semantic Kernel to modern agentic frameworks.

---

**Document Version**: 1.0  
**Last Updated**: January 8, 2026  
**Next Review**: When starting new agent transformations

