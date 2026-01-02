# Conversational Mentor - Implementation Plan

**Created**: January 1, 2026  
**Goal**: Build a conversational AI interface for visual knowledge model construction  
**Architecture**: Chat + 2D Canvas + Model Tree with KnowledgeType toolbar

---

## Vision: Three-Panel Interface

```
┌─────────────────────────────────────────────────────────────────────┐
│  Toolbar: [Concept] [Property] [Role] [Context] [Component] [...]  │
├──────────────────┬─────────────────────────┬────────────────────────┤
│   Chat Panel     │    2D Canvas            │   Model Tree           │
│   (25%)          │    (50%)                │   (25%)                │
│                  │                         │                        │
│ ┌─────────────┐  │  ┌───────────────────┐ │ ┌──────────────────┐  │
│ │ Test Prompt │  │  │                   │ │ │ 📁 Models        │  │
│ │ Dropdown ▼  │  │  │   MentorShape2D   │ │ │   📘 BeamModel   │  │
│ └─────────────┘  │  │   Canvas          │ │ │     🔷 beam1     │  │
│                  │  │                   │ │ │       • Length   │  │
│ ┌─────────────┐  │  │  ┌────┐   ┌────┐ │ │ │       • Load     │  │
│ │ User: Create│  │  │  │Beam│───│Prop│ │ │ │       • E        │  │
│ │ a beam...   │  │  │  └────┘   └────┘ │ │ │       • I        │  │
│ │             │  │  │                   │ │ │ 📁 Contexts      │  │
│ │ AI: Created │  │  │  ┌────┐          │ │ │ 📁 Roles         │  │
│ │ Component   │  │  │  │Role│          │ │ │ 📁 Concepts      │  │
│ │ 'beam1'...  │  │  │  └────┘          │ │ └──────────────────┘  │
│ └─────────────┘  │  │                   │ │                        │
│                  │  └───────────────────┘ │ Tabs:                  │
│ ┌─────────────┐  │                         │ • Tree View            │
│ │ Your msg... │  │  Page: [Mentor ▼]       │ • Parameters           │
│ │     [Send]  │  │  Zoom: [100% ▼]         │ • Formulas             │
│ └─────────────┘  │  [Clear] [Save] [Load]  │ • API Log              │
└──────────────────┴─────────────────────────┴────────────────────────┘
```

---

## Philosophy: Human-First, Chatbot Learns

**Core Insight**: The Blazor interface with toolbar buttons allows humans to build models using the powerful event-driven system. As humans work, the **chatbot observes and learns** valid construction patterns. Over time, the chatbot enriches its understanding by watching humans create valid models.

### Learning Flow

```
Human clicks [Concept] button
    ↓
MentorPlayground.CreateShape<KnConcept>() called
    ↓
DrawingEditChanged.Created event published
    ↓
┌─────────────────────────────────┬──────────────────────────────┐
│ MentorModelManager              │ Chatbot Observer             │
│ (Updates knowledge model)       │ (Learns construction pattern)│
└─────────────────────────────────┴──────────────────────────────┘

Human drags Property onto Concept
    ↓
MentorConstructTool detects collision
    ↓
IsDropAllowed(Property, Concept) → true
    ↓
DrawingEditChanged.ChildAdded event published
    ↓
┌─────────────────────────────────┬──────────────────────────────┐
│ MentorModelManager              │ Chatbot Observer             │
│ (Groups Property into Concept)  │ (Learns: Property → Concept) │
└─────────────────────────────────┴──────────────────────────────┘

Chatbot builds pattern library:
- "Concept shapes can contain Property shapes"
- "Role shapes connect to other Role shapes hierarchically"
- "Formula shapes attach to Role shapes for conditional logic"
```

**Result**: Mentor2DTech becomes the API that **both humans and chatbots use**, but chatbot learns correct usage by watching humans.

---

## Phase 1: Enhance Mentor2DTech API

**Goal**: Create knowledge-aware API that mirrors MentorPlayground pattern AND logs all operations for chatbot learning

### 1.1 Add Knowledge Shape Operations

**File**: `Three2025/Apprentice/Mentor2DTech.cs`

```csharp
// Knowledge-aware shape creation
[Description("Create a knowledge shape (Concept, Property, Role, etc.) on the canvas")]
public MentorShapeInfo CreateKnowledgeShape(
    [Description("Type: Concept, Property, Role, Context, Component, Feature, Formula, Variable, ValidValues")]
    string knowledgeType,
    [Description("Display title/label")] 
    string title,
    [Description("X position in pixels")] 
    int x,
    [Description("Y position in pixels")] 
    int y
);

// Attach shapes using type-aware logic (IsDropAllowed/IsConnectAllowed)
[Description("Attach one shape to another - system determines containment vs connection")]
public AttachmentResult AttachShape(
    [Description("Name of child/source shape")]
    string childName,
    [Description("Name of parent/target shape")]
    string parentName
);

// Query the knowledge object behind a shape
[Description("Get the knowledge object associated with a shape")]
public KnowledgeObjectInfo? GetKnowledgeObject(
    [Description("Name of the shape")]
    string shapeName
);

// Query allowed relationships
[Description("Check if one shape can be attached to another")]
public bool CanAttach(
    [Description("Name of child shape")]
    string childName,
    [Description("Name of parent shape")]
    string parentName
);

[Description("Get list of knowledge types that can be attached to a shape")]
public List<string> GetAllowedChildTypes(
    [Description("Name of the parent shape")]
    string shapeName
);
```

### 1.2 Add New DTOs

**File**: `Three2025/Models/Apprentice/MentorShapeInfo.cs`

```csharp
/// <summary>
/// Information about a knowledge shape
/// </summary>
public record MentorShapeInfo(
    string Name,
    string Title,
    string KnowledgeType,
    int X,
    int Y,
    string Color,
    string ShapeId,
    string KnowledgeObjectId
);

/// <summary>
/// Result of an attachment operation
/// </summary>
public record AttachmentResult(
    bool Success,
    string AttachmentType, // "Containment" or "Connection"
    string Message,
    string? ConnectorId  // ShapeId of connector line if Connection type
);

/// <summary>
/// Information about a knowledge object
/// </summary>
public record KnowledgeObjectInfo(
    string Id,
    string Type,
    string Title,
    Dictionary<string, string> Properties,
    List<string> Children,
    List<string> Connections
);
```

### 1.3 Add Learning/Observation Methods

**Goal**: Allow chatbot to query what's allowed and observe human actions

```csharp
// Query grammar rules
[Description("Get list of knowledge types that can be dropped into a parent type")]
public List<string> GetAllowedChildTypes(
    [Description("Parent knowledge type (Concept, Role, etc.)")]
    string parentType
);

[Description("Get list of knowledge types that can connect to a target type")]
public List<string> GetAllowedConnectionTypes(
    [Description("Target knowledge type")]
    string targetType
);

// Observe human actions (for learning)
[Description("Get recent human actions (last N operations)")]
public List<HumanAction> GetRecentActions(
    [Description("Number of recent actions to retrieve")]
    int count = 10
);

[Description("Get statistics about construction patterns")]
public ConstructionStats GetConstructionPatterns();
```

**New DTOs**:

```csharp
public record HumanAction(
    DateTime Timestamp,
    string ActionType, // "CreateShape", "AttachShape", "MoveShape", "DeleteShape"
    string KnowledgeType,
    Dictionary<string, string> Details
);

public record ConstructionStats(
    Dictionary<string, int> ShapeTypeFrequency,
    Dictionary<string, List<string>> CommonContainmentPatterns, // Parent → Children
    Dictionary<string, List<string>> CommonConnectionPatterns,  // Source → Targets
    List<string> FrequentSequences // ["Create Context", "Create Property", "Attach Property to Context"]
);
```

### 1.4 Integration with MentorPlayground

Mentor2DTech needs to inject and delegate to MentorPlayground AND track all actions:

```csharp
public class Mentor2DTech : IMentor2DTech
{
    private readonly IWorkspace _workspace;
    private readonly IMentorPlayground _playground;
    private readonly IMentorModelManager _modelManager;
    private readonly ComponentBus _pubsub;
    private FoPage2D? _page;
    private readonly Dictionary<string, MentorShape2D> _knowledgeShapes = new();
    private readonly List<HumanAction> _actionHistory = new();
    private readonly ILogger<Mentor2DTech> _logger;
    
    public Mentor2DTech(
        IWorkspace workspace, 
        IMentorPlayground playground,
        IMentorModelManager modelManager,
        ComponentBus pubsub,
        ILogger<Mentor2DTech> logger)
    {
        _workspace = workspace;
        _playground = playground;
        _modelManager = modelManager;
        _pubsub = pubsub;
    }
    
    public MentorShapeInfo CreateKnowledgeShape(string knowledgeType, string title, int x, int y)
    {
        // Parse knowledge type enum
        var type = Enum.Parse<KnowledgeType>(knowledgeType);
        
        // Delegate to playground (same pattern as MentorWorkbook.CreateShape)
        var shape = type switch
        {
            KnowledgeType.Concept => _playground.CreateShape<KnConcept>(title),
            KnowledgeType.Property => _playground.CreateShape<KnProperty>(title),
            KnowledgeType.Role => _playground.CreateShape<KnRole>(title),
            KnowledgeType.Context => _playground.CreateShape<KnContext>(title),
            KnowledgeType.Component => _playground.CreateShape<KnComponent>(title),
            // ... etc
        };
        
        // Position the shape
        shape.MoveTo(x, y);
        
        // Track it
        _knowledgeShapes[title] = shape;
        
        // Return info
        return new MentorShapeInfo(
            title,
            title,
            knowledgeType,
            x, y,
            shape.Color,
            shape.GetGlyphId(),
            shape.GetKnowledgeObject()?.GetKnowId() ?? ""
        );
    }
    
    public AttachmentResult AttachShape(string childName, string parentName)
    {
        var child = _knowledgeShapes[childName];
        var parent = _knowledgeShapes[parentName];
        
        // Delegate to playground.Attach() - it handles IsDropAllowed/IsConnectAllowed
        var result = _playground.Attach(child, parent);
        
        // Determine what happened
        if (parent.IsConnectAllowed(child))
        {
            return new AttachmentResult(true, "Connection", 
                $"Connected {childName} to {parentName}", 
                result.UpstreamShape?.GetGlyphId());
        }
        else if (parent.IsDropAllowed(child))
        {
            return new AttachmentResult(true, "Containment",
                $"{childName} is now child of {parentName}",
                null);
        }
        
        return new AttachmentResult(false, "None", 
            $"Cannot attach {childName} to {parentName}", null);
    }
}
```

--- (Human-First Interface)

**Goal**: Build the three-panel UI with toolbar that enables HUMANS to construct models visually. Chatbot observes and learns from these actions.

**Key Insight**: The toolbar buttons and drag-and-drop interface are **not just for humans** - they demonstrate to the chatbot what valid construction patterns look like. Every human action is logged and becomes training data for the chatbot's pattern recognition.

### 2.2 Human-Centric Design Priorities

1. **Toolbar should be intuitive** - Each knowledge type has a clear button with icon and color
2. **Visual feedback on drops** - Show green highlight when valid drop target, red when invalid
3. **Drag-and-drop smoothness** - Shapes snap to grid, auto-arrange to avoid overlap
4. **Undo/redo support** - Humans experiment, need to backtrack
5. **Status messages** - "Property added to Concept 'Table'" - confirms what happened
6. **Real-time tree sync** - As humans build, tree updates instantly

### 2.1 Enhanced Event Observation

When human performs any action, ConversationalMentor publishes to chatbot:

```csharp
protected override void OnInitialized()
{
    // Subscribe to ALL drawing events
    PubSub.SubscribeTo<DrawingEditChanged>(OnDrawingChanged);
    
    // Forward events to chatbot for learning
    PubSub.SubscribeTo<DrawingEditChanged>(e => {
        var humanAction = new HumanAction(
            DateTime.Now,
            GetActionType(e.State),
            GetKnowledgeType(e),
            ExtractDetails(e)
        );
        
        // Log for chatbot learning
        Mentor2DTech.LogHumanAction(humanAction);
        
        // Optionally show in chat
        if (ShowHumanActionsInChat)
        {
            ChatMessages.Add(new ChatMessage("system", 
                $"📊 Observed: {FormatAction(humanAction)}"));
        }
    });
}
```nt

**Goal**: Build the three-panel UI with toolbar

### 2.1 Component Structure

**File**: `Three2025/Components/ConversationalMentor.razor`

```razor
@page "/conversational-mentor"
@using Three2025.Apprentice
@inject IMentor2DTech Mentor2DTech
@inject IMentorServices MentorServices

<div class="conversational-mentor">
    <!-- Toolbar -->
    <div class="toolbar">
        <button @onclick="() => CreateShapeAt(KnowledgeType.Concept)">🔷 Concept</button>
        <button @onclick="() => CreateShapeAt(KnowledgeType.Property)">📊 Property</button>
        <button @onclick="() => CreateShapeAt(KnowledgeType.Role)">👤 Role</button>
        <button @onclick="() => CreateShapeAt(KnowledgeType.Context)">📋 Context</button>
        <button @onclick="() => CreateShapeAt(KnowledgeType.Component)">⚙️ Component</button>
        <button @onclick="() => CreateShapeAt(KnowledgeType.Feature)">✨ Feature</button>
        <button @onclick="() => CreateShapeAt(KnowledgeType.Formula)">🧮 Formula</button>
        <button @onclick="() => CreateShapeAt(KnowledgeType.Variable)">🔢 Variable</button>
        
        <div class="spacer"></div>
        
        <button @onclick="ClearCanvas">🗑️ Clear</button>
        <button @onclick="SaveModel">💾 Save</button>
        <button @onclick="LoadModel">📂 Load</button>
    </div>
    
    <!-- Three panels -->
    <div class="panels-container">
        <!-- Chat Panel (Left 25%) -->
        <div class="chat-panel">
            <div class="test-prompts">
                <select @onchange="LoadTestPrompt">
                    <option value="">Test Prompts...</option>
                    <option value="beam">Structural Beam Model</option>
                    <option value="strategic">Strategic Plan Decision Tree</option>
                    <option value="table">Table Component Model</option>
                </select>
            </div>
            
            <div class="chat-history">
                @foreach (var msg in ChatMessages)
                {
                    <div class="message @msg.Role">
                        <div class="role">@msg.Role:</div>
                        <div class="content">@msg.Content</div>
                    </div>
                }
            </div>
            
            <div class="chat-input">
                <input @bind="UserInput" @onkeydown="HandleKeyDown" 
                       placeholder="Describe what you want to build..." />
                <button @onclick="SendMessage">Send</button>
            </div>
        </div>
        
        <!-- Canvas Panel (Center 50%) -->
        <div class="canvas-panel">
            <div class="canvas-controls">
                <label>Page:</label>
                <select @bind="SelectedPage">
                    @foreach (var page in Pages)
                    {
                        <option value="@page">@page</option>
                    }
                </select>
                
                <label>Zoom:</label>
                <select @bind="ZoomLevel">
                    <option value="50">50%</option>
                    <option value="75">75%</option>
                    <option value="100">100%</option>
                    <option value="150">150%</option>
                </select>
            </div>
            
            <!-- Embed actual canvas here -->
            <div class="canvas-container" @ref="canvasContainer">
                @* Canvas will be rendered here via JS interop *@
            </div>
        </div>
        
        <!-- Tree Panel (Right 25%) -->
        <div class="tree-panel">
            <div class="tabs">
                <button class="@(ActiveTab == "tree" ? "active" : "")" 
                        @onclick='() => ActiveTab = "tree"'>Tree View</button>
                <button class="@(ActiveTab == "params" ? "active" : "")" 
                        @onclick='() => ActiveTab = "params"'>Parameters</button>
                <button class="@(ActiveTab == "log" ? "active" : "")" 
                        @onclick='() => ActiveTab = "log"'>API Log</button>
      3 Code-Behind with Learning Integration
            
            <div class="tab-content">
                @if (ActiveTab == "tree")
                {
                    <SuccessTreeView RootItems="@GetModelTree()" 
                                   OnSelect="@HandleTreeSelection" />
                }
                else if (ActiveTab == "params")
                {
                    <div class="parameters">
                        @if (SelectedComponent != null)
                        {
                            <h4>@SelectedComponent.GetTitle()</h4>
                            @foreach (var param in GetParameters(SelectedComponent))
                            {
                                <div class="param-row">
                                    <span>@param.Name:</span>
                                    <span>@param.DisplayValue</span>
                                </div>
                            }
                        }
                    </div>
                }
                else if (ActiveTab == "log")
                {
                    <div class="api-log">
                        @foreach (var call in ApiCallLog)
                        {
                            <div class="log-entry">
                                <div class="timestamp">@call.Timestamp</div>
                                <div class="method">@call.Method</div>
                                <div class="result">@call.Result</div>
                            </div>
                        }
                    </div>
                }
            </div>
        </div>
    </div>
</div>
```

### 2.2 Code-Behind

**File**: `Three2025/Components/ConversationalMentor.razor.cs`

```csharp
public partial class ConversationalMentor : ComponentBase
{
    [Inject] private IMentor2DTech Mentor2DTech { get; set; } = default!;
    [Inject] private IMentorServices MentorServices { get; set; } = default!;
    [Inject] private ComponentBus PubSub { get; set; } = default!;
    
    private List<ChatMessage> ChatMessages = new();
    private List<ApiCall> ApiCallLog = new();
    private string UserInput = "";
    private string ActiveTab = "tree";
    private string SelectedPage = "Mentor";
    private string ZoomLevel = "100";
    private List<string> Pages = new() { "Mentor", "Definitions", "Racks" };
    private KnComponent? SelectedComponent;
    private ElementReference canvasContainer;
    
    protected override async Task OnInitializedAsync()
    {
        // Initialize canvas
        Mentor2DTech.EstablishCanvas2D("Mentor");
        
        // Subscribe to drawing events
        PubSub.SubscribeTo<DrawingEditChanged>(OnDrawingChanged);
        
        // Welcome message
        ChatMessages.Add(new ChatMessage("assistant", 
            "Hi! I can help you build knowledge models. Try clicking the buttons above to create shapes, " +
            "or describe what you want to build in chat."));
    }
    
    private void CreateShapeAt(KnowledgeType type)
    {
        var count = GetShapeCount(type);
        var title = $"{type}_{count + 1}";
        
        // Position at center with some randomness
        var x = 400 + (count * 50);
        var y = 300 + (count * 30);
        
        var result = Mentor2DTech.CreateKnowledgeShape(type.ToString(), title, x, y);
        
        LogApiCall("CreateKnowledgeShape", $"{type}, {title}, ({x},{y})", 
            $"Created {result.ShapeId}");
        
        ChatMessages.Add(new ChatMessage("assistant", 
            $"Created {type} '{title}' at ({x}, {y})"));
        
        StateHasChanged();
    }
    
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserInput)) return;
        
        ChatMessages.Add(new ChatMessage("user", UserInput));
        
        // TODO: Call LLM with function calling
        // For now, simple pattern matching
        var response = await ProcessUserMessage(UserInput);
        
        ChatMessages.Add(new ChatMessage("assistant", response));
        UserInput = "";
        StateHasChanged();
    }
    
    private async Task<string> ProcessUserMessage(string message)
    {
        // Simple pattern matching for demo
        if (message.Contains("beam", StringComparison.OrdinalIgnoreCase))
        {
            return CreateBeamModel();
        }
        else if (message.Contains("strategic", StringComparison.OrdinalIgnoreCase))
        {
            return CreateStrategicPlanModel();
        }
        
        return "I understand you want to build something. Try one of the test prompts!";
    }
    
    private string CreateBeamModel()
    {
        // Create component
        var beam = Mentor2DTech.CreateKnowledgeShape("Component", "beam1", 200, 200);
        LogApiCall("CreateKnowledgeShape", "Component, beam1", beam.ShapeId);
        
        // Create properties
        var length = Mentor2DTech.CreateKnowledgeShape("Property", "Length", 220, 250);
        Mentor2DTech.AttachShape("Length", "beam1");
        LogApiCall("AttachShape", "Length → beam1", "Containment");
        
        var load = Mentor2DTech.CreateKnowledgeShape("Property", "Load", 220, 280);
        Mentor2DTech.AttachShape("Load", "beam1");
        LogApiCall("AttachShape", "Load → beam1", "Containment");
        
        return "Created a beam component with Length and Load properties!";
    }
    
    private string CreateStrategicPlanModel()
    {
        // Replicate MentorWorkbook.CreateStrategicPlan() using Mentor2DTech
        var context = Mentor2DTech.CreateKnowledgeShape("Context", "Strategic Plan Pro", 400, 100);
        
        var q1 = Mentor2DTech.CreateKnowledgeShape("Context", 
            "Is this for a profit or non-profit?", 420, 150);
        Mentor2DTech.AttachShape("Is this for a profit or non-profit?", "Strategic Plan Pro");
        
        var a1 = Mentor2DTech.CreateKnowledgeShape("Property", "Answer1", 440, 180);
        Mentor2DTech.AttachShape("Answer1", "Is this for a profit or non-profit?");
        
        var vv1 = Mentor2DTech.CreateKnowledgeShape("ValidValues", "profit; non-profit", 460, 210);
        Mentor2DTech.AttachShape("profit; non-profit", "Answer1");
        
        // ... continue pattern
        
        return "Created strategic planning decision tree!";
    }
    
    private void OnDrawingChanged(DrawingEditChanged message)
    {
        // Refresh tree view when shapes change
        StateHasChanged();
    }
    
    private IEnumerable<ITreeNode> GetModelTree()
    {
        return MentorServices.MentorModel.GetTreeChildren();
    }
    
    private void LogApiCall(string method, string args, string result)
    {
        ApiCallLog.Add(new ApiCall
        {
            Timestamp = DateTime.Now.ToString("HH:mm:ss"),
            Method = method,
            Arguments = args,
            Result = result
        });
    }
}

public record ChatMessage(string Role, string Content);
public record ApiCall
{
    public string Timestamp { get; init; } = "";
    public string Method { get; init; } = "";
    public string Arguments { get; init; } = "";
    public string Result { get; init; } = "";
}
```

---

## Phase 3: Canvas Integration

**Goal**: Embed the actual 2D drawing canvas

### 3.1 Canvas Component

**File**: `Three2025/Components/MentorCanvas2D.razor`

```razor
@using FoundryWorldsAndDrawings
@inject IWorkspace Workspace

<div class="mentor-canvas-container" @ref="container">
    @* Canvas rendered via JS interop *@
</div>

@code {
    [Parameter] public string PageName { get; set; } = "Mentor";
    [Parameter] public int ZoomLevel { get; set; } = 100;
    
    private ElementReference container;
    private IDrawing? drawing;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            drawing = Workspace.GetDrawing();
            var page = drawing.EstablishPage<FoPage2D>(PageName);
            
            // Initialize canvas in this container
            // await JS.InvokeVoidAsync("initializeCanvas", container);
        }
    }
}
```

### 3.2 Embed in ConversationalMentor

Replace the placeholder div with:

```razor
<div class="canvas-container">
    <MentorCanvas2D PageName="@SelectedPage" ZoomLevel="@int.Parse(ZoomLevel)" />
</div>
```

---

## Phase 4: LLM Integration with Pattern Learning

**Goal**: Connect to OpenAI/Anthropic with function calling, enriched by observed human patterns

### 4.2 LLM Service Implementationhanced LLM Context

Before calling LLM, inject learned patterns into system prompt:

```csharp
private async Task<string> BuildSystemPrompt()
{
    var stats = Mentor2DTech.GetConstructionPatterns();
    
    var prompt = @"You are a knowledge modeling assistant. You help users build 
visual knowledge models using typed shapes that connect according to specific rules.

## Learned Patterns from Human Experts:

### Most Common Shape Types:
" + string.Join("\n", stats.ShapeTypeFrequency.Select(kvp => 
        $"- {kvp.Key}: {kvp.Value} uses"))
+ @"

### Valid Containment Patterns (Parent → Children):
" + string.Join("\n", stats.CommonContainmentPatterns.Select(kvp =>
        $"- {kvp.Key} commonly contains: {string.Join(", ", kvp.Value)}"))
+ @"

### Valid Connection Patterns (Source → Targets):
" + string.Join("\n", stats.CommonConnectionPatterns.Select(kvp =>
        $"- {kvp.Key} often connects to: {string.Join(", ", kvp.Value)}"))
+ @"

### Typical Construction Sequences:
" + string.Join("\n", stats.FrequentSequences.Select(s => $"- {s}"))
+ @"

When users ask you to create models, follow these learned patterns from expert usage.";

    return prompt;
}
```

### 4.1 LLM Service with Learning

### 4.1 LLM Service

**File**: `Three2025/Services/LLMService.cs`

```csharp
public class LLMService
{
    private readonly IMentor2DTech _tech;
    
    public async Task<string> ProcessConversation(List<ChatMessage> history)
    {
        var tools = new[]
        {
            new
            {
                name = "create_knowledge_shape",
            Learning Validation & Refinement

**Goal**: Verify that chatbot learns from human actions and improves over time

### 5.0 Learning Validation Scenarios

**Scenario 1: Pattern Recognition**
1. Human creates 5 different Concept shapes, each with 2-3 Properties
2. Query `GetConstructionPatterns()` - should show "Concept → Property" as common pattern
3. Ask chatbot: "Create a table concept" 
4. Verify: Chatbot automatically suggests adding properties (learned behavior)

**Scenario 2: Sequence Learning**
1. Human repeatedly: Creates Context → Creates Property → Attaches Property → Creates ValidValues → Attaches ValidValues
2. Query `GetConstructionPatterns().FrequentSequences`
3. Should show: ["Create Context", "Create Property", "Attach Property", "Create ValidValues", "Attach ValidValues"]
4. Ask chatbot: "Create a questionnaire"
5. Verify: Chatbot follows learned sequence pattern

**Scenario 3: Invalid Action Prevention**
1. Human tries to drop Property into Property (fails with visual feedback)
2. Chatbot observes: "Attempted invalid attachment"
3. Later, chatbot should never suggest Property → Property containment
4. Query `GetAllowedChildTypes("Property")` - should NOT include "Property"

**Scenario 4: Strategic Plan Replication**
1. Human manually builds strategic plan (from MentorWorkbook.CreateStrategicPlan())
2. All actions logged
3. Ask chatbot: "Build a decision tree for selecting business templates"
4. Verify: Chatbot uses Context/Property/ValidValues/Role/Formula pattern (learned from human)

### 5.1 Traditional         {
                    type = "object",
                    properties = new
                    {
                        knowledgeType = new { type = "string", 
                            enum = new[] { "Concept", "Property", "Role", "Context", 
                                         "Component", "Feature", "Formula", "Variable" }},
                        title = new { type = "string" },
                        x = new { type = "integer" },
                        y = new { type = "integer" }
                    }
                }
            },
            new
            {
                name = "attach_shape",
                description = "Attach one shape to another",
                parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        childName = new { type = "string" },
                        parentName = new { type = "string" }
                    }
                }
            }
        };
        
        // Call OpenAI/Anthropic with tools
        // Parse function calls
        // Execute via Mentor2DTech
        // Return response
    }
} Primary Focus |
|-------|-------------|----------|--------------|---------------|
| **Phase 1** | Enhance Mentor2DTech API + Learning Infrastructure | 3-4 days | MentorPlayground, ModelManager | Action logging, pattern tracking |
| **Phase 2** | Build Human-First UI (Toolbar, Drag/Drop) | 3-4 days | Phase 1 | Human usability, visual feedback |
| **Phase 3** | Integrate Canvas + Event Observation | 2-3 days | Phase 2, Drawing system | Real-time learning from human actions |
| **Phase 4** | LLM Integration with Learned Patterns | 3-4 days | Phase 2, OpenAI/Anthropic API | Inject learned patterns into LLM context |
| **Phase 5** | Learning Validation & Refinement | 3-4 days | All phases | Verify chatbot learns and improves |
| **Total** | End-to-end | **14-19 days** | | |

### Phased Learning Evolution

**Phase 1 End State**: Mentor2DTech logs all actions, tracks patterns, provides query API

### Human Interface Metrics
- ✅ Toolbar creates shapes that appear on canvas
- ✅ Drag-and-drop provides visual feedback (green for valid, red for invalid)
- ✅ Shapes can be attached via drag-drop smoothly
- ✅ Tree view reflects current model state in real-time
- ✅ Undo/redo works correctly
- ✅ Save/load preserves entire state
- ✅ Human can replicate Strategic Plan example visually

### Learning & Observation Metrics
- ✅ API log captures ALL human operations
- ✅ Action history queryable via `GetRecentActions()`
- ✅ Pattern statistics accurate via `GetConstructionPatterns()`
- ✅ Common patterns detected after 10-20 human operations
- ✅ Frequent sequences identified correctly
- ✅ Invalid attempts logged (for negative examples)

### Chatbot Integration Metrics
- ✅ LLM can query learned patterns before generating models
- ✅ LLM follows observed patterns when building models
- ✅ LLM avoids invalid attachments (learned from human failures)
- ✅ LLM suggests next steps based on frequent sequences
- ✅ Chatbot accuracy improves measurably after observing 50+ human actions
- ✅ Strategic Plan example can be replicated by chatbot using learned patternshatbot demonstrably learns from human examples

1. **Manual Shape Creation**: Click toolbar buttons, verify shapes appear
2. **Manual Attachment**: Drag-and-drop shapes, verify events fire
3. **Chat Creation**: Type "create a beam", verify Mentor2DTech calls
4. **Strategic Plan**: Load test prompt, verify decision tree builds
5. **Tree Sync**: Create shapes, verify tree updates
6. **API Log**: Perform actions, verify log captures calls

### 5.2 Refinement Areas

- Layout algorithms (auto-arrange shapes)
- Collision detection
- Undo/redo
- Save/load diagrams
- Export to PNG/SVG
- Multi-page support

---

## Implementation Timeline

| Phase | Description | Duration | Dependencies |
|-------|-------------|----------|--------------|
| **Phase 1** | Enhance Mentor2DTech API | 2-3 days | MentorPlayground, ModelManager |
| **Phase 2** | Build ConversationalMentor UI | 2-3 days | Phase 1 |
| **Phase 3** | Integrate Canvas | 1-2 days | Phase 2, Drawing system |
| **Phase 4** | LLM Integration | 2-3 days | Phase 2, OpenAI/Anthropic API |
| **Phase 5** | Testing & Polish | 2-3 days | All phases |
| **Total** | End-to-end | **10-15 days** | |

---

## Success Metrics

- ✅ Toolbar creates shapes that appear on canvas
- ✅ Shapes can be attached via drag-drop or chat
- ✅ Tree view reflects current model state
- ✅ API log captures all operations
- ✅ LLM can build models through conversation
- ✅ Save/load preserves entire state
- ✅ Strategic Plan example works end-to-end

---

## Next Steps

1. **Review this plan** - Does it capture your vision?
2. **Start Phase 1** - Enhance Mentor2DTech with knowledge operations
3. **Create ConversationalMentor.razor** - Build the three-panel layout
4. **Test integration** - Verify events flow correctly
5. **Add LLM** - Connect OpenAI function calling
6. **Iterate** - Refine based on testing

Ready to begin Phase 1?
