# Semantic Kernel to Microsoft Agents Migration Plan

**Project**: Three2025 Framework  
**Date**: December 26, 2025  
**Objective**: Complete removal of Semantic Kernel, migration to Microsoft.Agents.AI

---

## Executive Summary

**Current State:**
- ✅ Microsoft.Agents.AI already installed (v1.0.0-preview.251219.1)
- ❌ Semantic Kernel v1.32.0 in use (LEGACY - TO BE REMOVED)
- 🔧 ~16 `[KernelFunction]` decorated methods across Technician classes
- 🔧 `ApprenticeAI.cs` orchestrates SK kernel and plugins

**End State:**
- ✅ Zero Semantic Kernel dependencies
- ✅ All tools auto-discovered from `ITechnician` implementations
- ✅ Integrated with `ChatOrchestrator` for multi-agent system
- ✅ Modern attribute-based tool registration

**Timeline**: 3-4 days (phased approach with validation gates)

---

## Phase 0: Pre-Migration Setup (4 hours)

### Objectives
- Create new infrastructure without breaking existing code
- Establish parallel systems for safe migration
- Set up comprehensive testing

### Tasks

#### 0.1 Create Agent Tool Attribute System

**File**: `Services/Agents/AgentToolAttribute.cs`

```csharp
using System.ComponentModel;

namespace Three2025.Services.Agents;

/// <summary>
/// Marks a method as available to AI agents as a tool.
/// Replaces legacy [KernelFunction] from Semantic Kernel.
/// Compatible with Microsoft.Agents.AI and AIFunctionFactory.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AgentToolAttribute : Attribute
{
    /// <summary>
    /// The name of the tool as exposed to the AI agent
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Optional description override (use [Description] attribute instead for consistency)
    /// </summary>
    public string? Description { get; set; }
    
    public AgentToolAttribute(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// Creates a tool with auto-generated name from method name
    /// </summary>
    public AgentToolAttribute() : this(string.Empty)
    {
    }
}
```

**Validation**: Build successfully, no errors

---

#### 0.2 Create Technician Tool Provider Interface

**File**: `Services/Agents/ITechnicianToolProvider.cs`

```csharp
using Microsoft.Extensions.AI;

namespace Three2025.Services.Agents;

/// <summary>
/// Discovers and provides tools from ITechnician implementations to AI agents.
/// Replaces Semantic Kernel's plugin system.
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
    IEnumerable<AIFunction> GetToolsFor<T>() where T : ITechnician;
    
    /// <summary>
    /// Get tools from a specific technician by interface type
    /// </summary>
    IEnumerable<AIFunction> GetToolsFor(Type technicianInterface);
    
    /// <summary>
    /// Get count of discovered tools (for diagnostics)
    /// </summary>
    int GetToolCount();
    
    /// <summary>
    /// Get list of all technician types that have tools
    /// </summary>
    IEnumerable<Type> GetTechnicianTypes();
}
```

**Validation**: Build successfully, interface compiles

---

#### 0.3 Implement Technician Tool Provider

**File**: `Services/Agents/TechnicianToolProvider.cs`

```csharp
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel; // TEMPORARY - for reading [KernelFunction] during migration
using Three2025.Apprentice;

namespace Three2025.Services.Agents;

public class TechnicianToolProvider : ITechnicianToolProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TechnicianToolProvider> _logger;
    private readonly Dictionary<Type, List<AIFunction>> _toolCache = new();
    private readonly object _cacheLock = new();
    
    public TechnicianToolProvider(
        IServiceProvider serviceProvider, 
        ILogger<TechnicianToolProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public IEnumerable<AIFunction> DiscoverAllTools()
    {
        _logger.LogInformation("Starting tool discovery from ITechnician implementations...");
        
        var technicianTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ITechnician).IsAssignableFrom(t) && 
                       t.IsInterface && 
                       t != typeof(ITechnician))
            .ToList();
        
        _logger.LogInformation($"Found {technicianTypes.Count} ITechnician interfaces");
        
        var allTools = new List<AIFunction>();
        
        foreach (var interfaceType in technicianTypes)
        {
            try
            {
                var tools = GetToolsFor(interfaceType);
                allTools.AddRange(tools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to discover tools from {interfaceType.Name}");
            }
        }
        
        _logger.LogInformation($"✅ Discovered {allTools.Count} total tools from {technicianTypes.Count} technicians");
        return allTools;
    }
    
    public IEnumerable<AIFunction> GetToolsFor<T>() where T : ITechnician
    {
        return GetToolsFor(typeof(T));
    }
    
    public IEnumerable<AIFunction> GetToolsFor(Type technicianInterface)
    {
        lock (_cacheLock)
        {
            if (_toolCache.TryGetValue(technicianInterface, out var cachedTools))
            {
                return cachedTools;
            }
        }
        
        var tools = ExtractToolsFromTechnician(technicianInterface);
        
        lock (_cacheLock)
        {
            _toolCache[technicianInterface] = tools;
        }
        
        return tools;
    }
    
    public int GetToolCount()
    {
        return DiscoverAllTools().Count();
    }
    
    public IEnumerable<Type> GetTechnicianTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ITechnician).IsAssignableFrom(t) && 
                       t.IsInterface && 
                       t != typeof(ITechnician));
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
        
        // Find methods with [KernelFunction] (legacy) or [AgentTool] (new)
        var methods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => 
                m.GetCustomAttribute<KernelFunctionAttribute>() != null ||
                m.GetCustomAttribute<AgentToolAttribute>() != null ||
                (m.GetCustomAttribute<DescriptionAttribute>() != null && 
                 m.DeclaringType == implementationType))
            .ToList();
        
        _logger.LogInformation($"  {interfaceType.Name}: Found {methods.Count} tool methods");
        
        foreach (var method in methods)
        {
            try
            {
                var kernelAttr = method.GetCustomAttribute<KernelFunctionAttribute>();
                var agentAttr = method.GetCustomAttribute<AgentToolAttribute>();
                var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                
                // Determine tool name (priority: AgentTool > KernelFunction > method name)
                string toolName = agentAttr?.Name 
                    ?? kernelAttr?.Name 
                    ?? method.Name;
                
                if (string.IsNullOrWhiteSpace(toolName))
                {
                    toolName = method.Name;
                }
                
                // Get description
                string? description = agentAttr?.Description 
                    ?? descAttr?.Description 
                    ?? $"Invokes {method.Name}";
                
                // Create AIFunction using AIFunctionFactory
                var func = AIFunctionFactory.Create(
                    method,
                    target: implementation,
                    name: toolName,
                    description: description);
                
                tools.Add(func);
                
                _logger.LogDebug($"    ✓ {toolName}: {description}");
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

**Validation**: 
- Build successfully
- All methods compile
- No SK-specific errors

---

#### 0.4 Register Tool Provider in DI

**File**: `Program.cs` (add after existing Technician registrations)

```csharp
// Register tool provider for agent system
builder.Services.AddSingleton<ITechnicianToolProvider, TechnicianToolProvider>();
```

**Validation**: Application starts without errors

---

#### 0.5 Create Tool Discovery Test Page (Optional but Recommended)

**File**: `Components/Pages/ToolDiscoveryTest.razor`

```razor
@page "/tool-discovery-test"
@inject ITechnicianToolProvider ToolProvider
@rendermode InteractiveServer

<PageTitle>Tool Discovery Test</PageTitle>

<RadzenCard>
    <h3>🔧 ITechnician Tool Discovery Test</h3>
    <p>This page validates that tools are being discovered from Technician classes.</p>
    
    <RadzenButton Text="Discover Tools" Click="DiscoverTools" ButtonStyle="ButtonStyle.Primary" />
    
    @if (discoveryComplete)
    {
        <div style="margin-top: 20px;">
            <h4>Discovery Results</h4>
            <p><strong>Total Tools Found:</strong> @toolCount</p>
            <p><strong>Technicians Scanned:</strong> @technicianCount</p>
            
            <h5>Tools by Technician:</h5>
            <ul>
                @foreach (var tech in technicianTools)
                {
                    <li>
                        <strong>@tech.Key.Name</strong> → @tech.Value.Count() tools
                        <ul>
                            @foreach (var tool in tech.Value)
                            {
                                <li>@tool.Metadata.Name - @tool.Metadata.Description</li>
                            }
                        </ul>
                    </li>
                }
            </ul>
        </div>
    }
</RadzenCard>

@code {
    private bool discoveryComplete = false;
    private int toolCount = 0;
    private int technicianCount = 0;
    private Dictionary<Type, IEnumerable<AIFunction>> technicianTools = new();
    
    private void DiscoverTools()
    {
        var allTools = ToolProvider.DiscoverAllTools().ToList();
        toolCount = allTools.Count;
        
        var types = ToolProvider.GetTechnicianTypes().ToList();
        technicianCount = types.Count;
        
        technicianTools.Clear();
        foreach (var type in types)
        {
            var tools = ToolProvider.GetToolsFor(type).ToList();
            if (tools.Any())
            {
                technicianTools[type] = tools;
            }
        }
        
        discoveryComplete = true;
    }
}
```

**Validation**: 
- Page loads
- Click "Discover Tools" shows all methods from LightingTech, ClockTech, etc.
- Verify count matches expected (10 from LightingTech alone)

---

### Phase 0 Success Criteria

✅ All new files compile  
✅ Application starts without errors  
✅ Tool discovery test page shows all expected tools  
✅ No existing functionality broken  
✅ Semantic Kernel still working (parallel systems)

**Decision Point**: Proceed to Phase 1 only if all criteria met

---

## Phase 1: Migrate Technician Classes (1 day)

### Objectives
- Replace `[KernelFunction]` with `[AgentTool]` or `[Description]`
- Remove Semantic Kernel imports
- Maintain backward compatibility during migration

### Approach: Migrate One Technician at a Time

#### 1.1 Migrate LightingTech (Pilot)

**File**: `Apprentice/LightingTech.cs`

**Changes**:
1. Remove `using Microsoft.SemanticKernel;`
2. Replace all `[KernelFunction("name")]` with `[Description("description")]`
3. Use built-in .NET `[Description]` attribute for simplicity

**Before:**
```csharp
using Microsoft.SemanticKernel;

[KernelFunction("delete_light")]
[Description("delete a light")]
[return: Description("return the deleted light")]
public LightingComponent? DeleteLight(string name)
```

**After:**
```csharp
// No SK import needed!

[Description("delete a light")]
public LightingComponent? DeleteLight(
    [Description("The name of the light to delete")] string name)
```

**Implementation Strategy**:
- Create branch: `migrate/lighting-tech`
- Apply changes
- Test via Tool Discovery Test page
- Test via actual chat interaction
- Merge when validated

**Validation**:
- Tool Discovery shows all 10 LightingTech methods
- Chat can call lighting tools successfully
- No errors in console

---

#### 1.2 Migrate Remaining Technicians (Parallel)

Following same pattern as LightingTech:

| Technician | Methods to Migrate | Owner | Branch |
|------------|-------------------|-------|--------|
| `ClockTech` | TBD (count from grep) | - | `migrate/clock-tech` |
| `CageTech` | TBD | - | `migrate/cage-tech` |
| `RackTech` | TBD | - | `migrate/rack-tech` |
| `CuckooClockTech` | TBD | - | `migrate/cuckoo-tech` |
| `TrisocTech` | TBD | - | `migrate/trisoc-tech` |

**Process for Each**:
1. Create feature branch
2. Remove SK imports
3. Replace `[KernelFunction]` → `[Description]`
4. Test tool discovery
5. Test via chat
6. Merge to main

---

#### 1.3 Testing Matrix

Create test cases for each technician:

**LightingTech Test Cases:**
- ✅ Get list of lights
- ✅ Add a new light
- ✅ Delete a light by name
- ✅ Change light color
- ✅ Change light state (on/off)
- ✅ Reposition light
- ✅ Save/restore lights
- ✅ Generate random color

**Testing Approach**:
1. **Manual**: Use Tool Discovery Test page
2. **Chat**: Ask agent to call each function
3. **Unit**: Create integration tests (optional)

---

### Phase 1 Success Criteria

✅ All Technician classes migrated  
✅ Zero `using Microsoft.SemanticKernel;` in Apprentice folder  
✅ All tools discoverable via `TechnicianToolProvider`  
✅ All tools callable via chat interface  
✅ No SK dependencies in Technician layer  

**Decision Point**: Proceed to Phase 2 only if all criteria met

---

## Phase 2: Integrate with ChatOrchestrator (4 hours)

### Objectives
- Wire tools into multi-agent chat system
- Replace SK-based tool calling with MS Agents framework
- Maintain existing chat functionality

### Tasks

#### 2.1 Update ChatOrchestrator to Use Tools

**File**: `Services/Chat/ChatOrchestrator.cs`

**Add Tool Integration:**

```csharp
public class ChatOrchestrator : IChatOrchestrator
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ITechnicianToolProvider _toolProvider;
    private readonly IAgentFactory _agentFactory;
    private readonly ILogger<ChatOrchestrator> _logger;
    
    private Dictionary<string, ISpecializedAgent> _agents = new();
    private List<AIFunction> _technicianTools = new(); // NEW
    
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
        _technicianTools = _toolProvider.DiscoverAllTools().ToList();
        _logger.LogInformation($"Loaded {_technicianTools.Count} technician tools");
        
        InitializeAgents();
    }
    
    private void InitializeAgents()
    {
        // Create specialized agents WITH technician tools
        RegisterAgent(_agentFactory.Create3DModelingAgent(_technicianTools));
        RegisterAgent(_agentFactory.CreateAnimationAgent(_technicianTools));
        // etc...
    }
}
```

**Validation**: 
- ChatOrchestrator initializes successfully
- Tools are loaded (check logs)
- Agents can access tools

---

#### 2.2 Update Specialized Agents to Accept Tools

**File**: `Services/Chat/Agents/ThreeDModelingAgent.cs`

**Update Constructor:**

```csharp
public class ThreeDModelingAgent : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    
    public ThreeDModelingAgent(
        IMultiProviderChatService chatService,
        IEnumerable<AIFunction> technicianTools,
        ILogger<ThreeDModelingAgent> logger)
    {
        _chatService = chatService;
        _tools = technicianTools.ToList();
        _logger = logger;
    }
    
    public async Task<string> ProcessAsync(...)
    {
        // When creating agent for this request, include tools
        var chatClient = _chatService.GetCurrentChatClient();
        var agent = chatClient.CreateAIAgent(
            instructions: GetSystemPrompt(),
            name: Name,
            tools: _tools); // Pass technician tools!
        
        // ... rest of processing
    }
}
```

**Apply to All Agents**:
- 3D Modeling Agent
- Animation Agent  
- Parameter Agent
- Geometry Agent
- SysML Agent
- Visualization Agent

---

#### 2.3 Update AgentFactory to Pass Tools

**File**: `Services/Chat/AgentFactory.cs`

```csharp
public class AgentFactory : IAgentFactory
{
    public ISpecializedAgent Create3DModelingAgent(IEnumerable<AIFunction> tools)
        => new ThreeDModelingAgent(_chatService, tools, _loggerFactory.CreateLogger<ThreeDModelingAgent>());
    
    public ISpecializedAgent CreateAnimationAgent(IEnumerable<AIFunction> tools)
        => new AnimationAgent(_chatService, tools, _loggerFactory.CreateLogger<AnimationAgent>());
    
    // ... etc for all agents
}
```

**Validation**:
- All agents compile
- Factory creates agents with tools
- No runtime errors

---

### Phase 2 Success Criteria

✅ ChatOrchestrator loads technician tools  
✅ Specialized agents receive tools  
✅ Chat system functional with new architecture  
✅ Agents can call technician methods (test: "turn on the chandelier")  
✅ No SK dependencies in Services/Chat layer

**Decision Point**: Proceed to Phase 3 only if all criteria met

---

## Phase 3: Remove Semantic Kernel Infrastructure (2 hours)

### Objectives
- Delete ApprenticeAI.cs (SK orchestration layer)
- Remove SK package from project
- Clean up all SK references

### Tasks

#### 3.1 Delete ApprenticeAI.cs

**Files to Delete:**
- `Apprentice/ApprenticeAI.cs` (entire file)

**Update DI Registration in Program.cs:**

```csharp
// REMOVE THIS LINE:
// builder.Services.AddScoped<IApprenticeAI, ApprenticeAI>();

// Tool access now comes through ChatOrchestrator!
```

**Validation**: 
- Project builds without ApprenticeAI
- No references to IApprenticeAI remain

---

#### 3.2 Remove SK Package Reference

**File**: `Three2025.csproj`

**Remove:**
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.32.0" />
```

**Validation**:
- `dotnet restore` succeeds
- `dotnet build` succeeds
- No SK assembly references remain

---

#### 3.3 Clean Up Tool Provider (Remove SK Support)

**File**: `Services/Agents/TechnicianToolProvider.cs`

**Remove SK Compatibility Code:**

```csharp
// REMOVE this import:
// using Microsoft.SemanticKernel;

// UPDATE method discovery to ONLY look for [AgentTool] or [Description]:
var methods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
    .Where(m => 
        m.GetCustomAttribute<AgentToolAttribute>() != null ||
        m.GetCustomAttribute<DescriptionAttribute>() != null)
    .ToList();

// REMOVE KernelFunction handling:
// var kernelAttr = method.GetCustomAttribute<KernelFunctionAttribute>();
```

**Validation**:
- Tool discovery still works
- All tools found via Description attribute
- No SK types referenced

---

#### 3.4 Search and Destroy Remaining SK References

**Command**:
```bash
grep -r "using Microsoft.SemanticKernel" Three2025/**/*.cs
grep -r "KernelFunction" Three2025/**/*.cs
```

**Expected Result**: ZERO matches

---

### Phase 3 Success Criteria

✅ ApprenticeAI.cs deleted  
✅ SK package removed from .csproj  
✅ Zero SK references in codebase  
✅ Application builds successfully  
✅ All tools still discoverable  
✅ Chat system fully functional  

**Decision Point**: Proceed to Phase 4 only if all criteria met

---

## Phase 4: Testing & Validation (4 hours)

### Objectives
- Comprehensive end-to-end testing
- Performance validation
- Documentation updates

### Testing Checklist

#### 4.1 Tool Discovery Tests

- [ ] All ITechnician interfaces discovered
- [ ] Correct tool count (should match pre-migration)
- [ ] Each tool has correct name and description
- [ ] Tool Discovery Test page shows all tools

#### 4.2 Chat Integration Tests

**LightingTech Tools:**
- [ ] "Show me all the lights" → calls `get_lights`
- [ ] "Add a light called Desk Lamp that's on and blue" → calls `add_light`
- [ ] "Delete the Chandelier light" → calls `delete_light`
- [ ] "Move Table Lamp to position 5, 2, 3" → calls `Reposition_Light`
- [ ] "Turn off the Desk Lamp" → calls `change_state`
- [ ] "Change Porch light to red" → calls `change_color`
- [ ] "Pick a random color for me" → calls `PickARandomColor`

**Other Technicians:**
- [ ] Test 2-3 functions from each remaining technician
- [ ] Verify parameters are correctly passed
- [ ] Verify return values are useful

#### 4.3 Multi-Agent Orchestration Tests

- [ ] Chat on GeometryTest page uses 3D Modeling Agent with tools
- [ ] Chat on KnModelAnimation page uses Animation Agent with tools
- [ ] Agent switching works correctly
- [ ] Tools available to all agents

#### 4.4 Performance Tests

- [ ] Tool discovery completes in < 1 second
- [ ] Chat response time < 3 seconds
- [ ] No memory leaks during extended use
- [ ] Tool cache working (check logs for cache hits)

#### 4.5 Error Handling Tests

- [ ] Invalid tool parameters handled gracefully
- [ ] Non-existent light name returns appropriate error
- [ ] Agent handles tool failures without crashing
- [ ] Logging provides useful diagnostics

---

### Phase 4 Success Criteria

✅ All test cases pass  
✅ Performance acceptable  
✅ No regressions in existing functionality  
✅ Error handling robust  
✅ Documentation updated  

---

## Rollback Plan

### If Migration Fails

**At Phase 0-1:**
- Delete new files (TechnicianToolProvider, etc.)
- Revert Technician class changes
- Continue using SK as before

**At Phase 2:**
- Revert ChatOrchestrator changes
- Restore ApprenticeAI.cs from git history
- Re-add SK package

**At Phase 3+:**
- Full git revert to last stable commit
- Restore SK package
- Rebuild ApprenticeAI.cs

**Emergency Rollback Command:**
```bash
git revert HEAD~n  # n = number of commits to revert
dotnet restore
dotnet build
```

---

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Tool discovery fails | Low | High | Phase 0 validation, comprehensive testing |
| AIFunctionFactory incompatible | Low | Medium | Test early in Phase 0, fallback to manual wrappers |
| Performance degradation | Low | Low | Performance tests in Phase 4, caching implemented |
| Breaking existing chat | Medium | High | Parallel systems in Phases 0-1, gradual migration |
| Documentation gaps | Medium | Low | Continuous documentation updates |

---

## Success Metrics

### Technical Metrics
- ✅ Zero SK package dependencies
- ✅ 100% tool migration (all `[KernelFunction]` converted)
- ✅ Tool discovery < 1 second
- ✅ Chat response time < 3 seconds
- ✅ Zero runtime errors in logs

### Functional Metrics
- ✅ All Technician tools accessible via chat
- ✅ Multi-agent system operational
- ✅ No feature regressions
- ✅ Developer experience improved (simpler API)

---

## Post-Migration Tasks

### Documentation
- [ ] Update MULTI_AGENT_CHATBOT_INFRASTRUCTURE_SPEC.md
- [ ] Create "Adding New Technician Tools" guide
- [ ] Update README with new architecture
- [ ] Document [AgentTool] vs [Description] usage

### Code Quality
- [ ] Add XML documentation to TechnicianToolProvider
- [ ] Add unit tests for tool discovery
- [ ] Add integration tests for each technician
- [ ] Code review and cleanup

### Future Enhancements
- [ ] Add tool categorization (3D, Lighting, Animation, etc.)
- [ ] Implement tool usage analytics
- [ ] Add tool versioning
- [ ] Create tool documentation generator

---

## Timeline Summary

| Phase | Duration | Dependencies | Go/No-Go Gate |
|-------|----------|--------------|---------------|
| Phase 0: Infrastructure | 4 hours | None | Tool discovery works |
| Phase 1: Migrate Technicians | 1 day | Phase 0 | All tools migrated |
| Phase 2: ChatOrchestrator | 4 hours | Phase 1 | Agents use tools |
| Phase 3: Remove SK | 2 hours | Phase 2 | Zero SK refs |
| Phase 4: Testing | 4 hours | Phase 3 | All tests pass |
| **TOTAL** | **3-4 days** | | |

---

## Appendix A: File Checklist

### Files to Create
- [ ] `Services/Agents/AgentToolAttribute.cs`
- [ ] `Services/Agents/ITechnicianToolProvider.cs`
- [ ] `Services/Agents/TechnicianToolProvider.cs`
- [ ] `Components/Pages/ToolDiscoveryTest.razor` (optional)

### Files to Modify
- [ ] `Apprentice/LightingTech.cs`
- [ ] `Apprentice/ClockTech.cs`
- [ ] `Apprentice/CageTech.cs`
- [ ] `Apprentice/RackTech.cs`
- [ ] `Apprentice/CuckooClockTech.cs`
- [ ] `Apprentice/TrisocTech.cs`
- [ ] `Services/Chat/ChatOrchestrator.cs`
- [ ] `Services/Chat/AgentFactory.cs`
- [ ] `Services/Chat/Agents/*.cs` (all agent implementations)
- [ ] `Program.cs`
- [ ] `Three2025.csproj`

### Files to Delete
- [ ] `Apprentice/ApprenticeAI.cs`

---

## Appendix B: Command Reference

### Discovery Commands
```bash
# Find all KernelFunction usages
grep -r "KernelFunction" Three2025/**/*.cs

# Find all SK imports
grep -r "using Microsoft.SemanticKernel" Three2025/**/*.cs

# Count tools per file
grep -c "KernelFunction" Three2025/Apprentice/*.cs
```

### Testing Commands
```bash
# Build
dotnet build

# Run tests
dotnet test

# Run application
dotnet run
```

### Git Workflow
```bash
# Create feature branch
git checkout -b migrate/phase-X

# Commit changes
git add .
git commit -m "Phase X: Description"

# Merge to main
git checkout main
git merge migrate/phase-X
```

---

## Appendix C: Key Contacts & Resources

**Microsoft.Agents.AI Documentation:**
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai
- https://github.com/microsoft/extensions-ai

**Internal Resources:**
- MULTI_AGENT_CHATBOT_INFRASTRUCTURE_SPEC.md
- This migration plan

**Team Contacts:**
- Migration Lead: TBD
- Testing Lead: TBD
- Rollback Authority: TBD

---

## Conclusion

This migration plan provides a structured, low-risk approach to removing Semantic Kernel and adopting Microsoft.Agents.AI. By proceeding in phases with clear validation gates, we ensure that each step is stable before proceeding to the next.

**Key Principles:**
1. **Parallel Systems**: New infrastructure alongside old (Phase 0-1)
2. **Incremental Migration**: One technician at a time (Phase 1)
3. **Validation Gates**: Must pass criteria before proceeding
4. **Rollback Ready**: Can revert at any phase
5. **Comprehensive Testing**: Validate before declaring success

**Expected Outcome**: Modern, maintainable agent-based architecture with zero legacy dependencies.

---

*Document Version: 1.0*  
*Last Updated: December 26, 2025*  
*Status: READY FOR EXECUTION*
