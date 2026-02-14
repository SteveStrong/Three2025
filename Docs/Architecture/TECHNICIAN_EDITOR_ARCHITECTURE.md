# Technician-Editor Architecture
## LLM-Driven Tool Orchestration Pattern

**Date:** January 3, 2026  
**Status:** Established Pattern  
**Purpose:** Define how LLM agents interact with domain models through a three-layer architecture

---

## Core Architecture

### Three-Layer Pattern

```
┌─────────────────────────────────────────┐
│         MentorServices (Scoped)         │
│   Single Source of Truth for Session   │
│  - CurrentModel, CurrentComponent       │
│  - Global session state                 │
└─────────────────────────────────────────┘
                    ▲
                    │ reads/writes
                    │
┌─────────────────────────────────────────┐
│        Technician (Scoped)              │
│      LLM Orchestrator Layer             │
│  - Exposes tools to LLM                 │
│  - Maintains conversation context       │
│  - Creates and manages Editor(s)        │
└─────────────────────────────────────────┘
                    │
                    │ creates/uses
                    ▼
┌─────────────────────────────────────────┐
│      Editor (Not in DI)                 │
│      Sculptor/Worker Layer              │
│  - Direct domain manipulation           │
│  - Maintains working state              │
│  - Multiple instances possible          │
└─────────────────────────────────────────┘
```

---

## Component Roles

### 1. MentorServices (Services Layer)
**Registration:** `builder.Services.AddScoped<IMentorServices, MentorServices>()`

**Purpose:** Session-wide shared state - "The Display Window"

**Responsibilities:**
- Hold `CurrentModel` and `CurrentComponent` (what user is viewing/observing)
- Single source of truth for UI rendering
- Event bus for model change notifications (`ModelEditChanged`)
- **Never modified directly by UI** - only by orchestration layer

**Key Principle:** UI always reads from MentorServices, never caches locally

**Lifecycle:** Per-circuit (one instance per user session/SignalR connection)

---

### 2. Technician (Orchestration Layer)
**Registration:** `builder.Services.AddScoped<IModelTech, ModelTech>()`

**Examples:** `ModelTech`, `Shape2DTech`, `Shape3DTech`, `Mentor2DTech`

**Purpose:** LLM-driven orchestrator - "The Conductor"

**Responsibilities:**
- Expose tools/functions to LLM via `[Description]` attributes
- Translate high-level LLM requests into domain operations
- Create and manage Editor instance(s) dynamically
- Update MentorServices when state changes
- Maintain conversation context across multiple tool calls
- "Bookmark" working state for ongoing sessions

**Key Pattern:**
```csharp
public class ModelTech : IModelTech
{
    private readonly IMentorServices _mentorServices;
    private readonly IModelEditor _editor;  // Created, not injected!
    
    public ModelTech(IMentorServices mentorServices)
    {
        _mentorServices = mentorServices;
        _editor = new ModelEditor(mentorServices);  // Dynamic creation
    }
    
    public KnModel? CurrentModel 
    { 
        get => _mentorServices.CurrentModel;  // Read from authority
        private set => _mentorServices.CurrentModel = value;  // Write to authority
    }
}
```

**Lifecycle:** Per-circuit (persists across all tool calls in conversation)

---

### 3. Editor (Worker Layer)
**Registration:** **NOT in DI** - created dynamically by Technician

**Examples:** `ModelEditor`, `Shape2DEditor`, `Shape3DEditor`, `Mentor2DEditor`

**Purpose:** Stateful sculptor - "The Craftsman"

**Responsibilities:**
- Direct manipulation of domain objects (models, components, shapes)
- Maintain working context (which page, which stage, created objects)
- Support multiple instances for parallel work
- Publish change events via PubSub
- **Does not know about MentorServices** - just works on what it's given

**Key Pattern:**
```csharp
public class ModelEditor : IModelEditor
{
    private readonly IMentorServices _mentorServices;
    
    public ModelEditor(IMentorServices mentorServices)
    {
        _mentorServices = mentorServices;
    }
    
    public KnComponent AddChild(KnComponent parent, KnComponent child)
    {
        // Direct domain manipulation
        parent.AddChild(child);
        
        // Publish change notification
        _mentorServices.PubSub.Publish(new ModelEditChanged("ChildAdded"));
        
        return child;
    }
}
```

**Lifecycle:** Managed by Technician (created in constructor, lives for session)

---

## Key Architectural Decisions

### Why All Scoped (Not Singleton)?
**Scoped = Per-circuit in Blazor Server**

Each SignalR connection (user session) gets its own instances of:
- MentorServices (user's current view state)
- ModelTech (user's conversation context)
- Editors (user's working context)

**Benefits:**
- Automatic isolation between users
- State persists across conversation
- Natural cleanup on disconnect
- Supports ongoing "bookmark" pattern

---

### Why Dynamic Editor Creation (Not DI)?
**Original Pattern:** Inject editors via DI
```csharp
public ModelTech(IMentorServices services, IModelEditor editor) // ❌ Limiting
```

**New Pattern:** Create editors dynamically
```csharp
public ModelTech(IMentorServices services)
{
    _editor = new ModelEditor(services);  // ✅ Flexible
}
```

**Rationale:**

1. **Multiple Editors:** Technician can create multiple editors for parallel work
   ```csharp
   private ModelEditor _primaryEditor;
   private ModelEditor _scratchpadEditor;  // For experimenting
   ```

2. **Explicit State:** Editor state is visible in Technician's fields, not hidden in DI
   ```csharp
   // Clear what's being maintained
   private Shape2DEditor _diagramEditor;
   private Shape2DEditor _annotationEditor;
   ```

3. **Conversation Context:** Editor becomes a "bookmark" - maintains what was last created
   ```csharp
   // Editor remembers all boxes/links created during conversation
   _editor.AddBox("A", ...);
   // Later in conversation...
   _editor.AddDirectedLink("A", "B");  // Still knows about "A"
   ```

4. **Flexibility:** Can create/destroy editors as needed for complex scenarios

5. **Consistency:** Matches "sculptor" metaphor - Tech manages multiple workbenches

---

## Applied Pattern: All Four Technicians

### ModelTech → ModelEditor
```csharp
public ModelTech(IMentorServices mentorServices)
{
    _mentorServices = mentorServices;
    _editor = new ModelEditor(mentorServices);
}
```
**Operates on:** KnModel, KnComponent, KnParameter

---

### Shape2DTech → Shape2DEditor
```csharp
public Shape2DTech(IWorkspace workspace, IFoundryService foundryService, ILogger logger)
{
    _workspace = workspace;
    _foundryService = foundryService;
    _editor = new Shape2DEditor(foundryService);
    _logger = logger;
}
```
**Operates on:** FoPage2D, FoShape2D, FoShape1D

---

### Shape3DTech → Shape3DEditor
```csharp
public Shape3DTech(IWorkspace workspace, IFoundryService foundryService)
{
    Workspace = workspace;
    FoundryService = foundryService;
    ShapeEditor = new Shape3DEditor(foundryService);
}
```
**Operates on:** FoStage3D, FoGlyph3D

---

### Mentor2DTech → Mentor2DEditor
```csharp
public Mentor2DTech(IWorkspace workspace, IFoundryService foundryService, ILogger logger)
{
    _workspace = workspace;
    _foundryService = foundryService;
    _editor = new Mentor2DEditor(foundryService);
    _logger = logger;
}
```
**Operates on:** Diagram boxes, links (FoShape2D, FoShape1D)

---

## LLM Interaction Flow

### Example: Voltage Divider Creation

```
User: "Create a voltage divider for 12V to 5V at 10mA"
  │
  ↓
LLM analyzes prompt → Plans tool sequence
  │
  ↓
Tool Call: create_model("VoltageDivider")
  │
  ↓ ModelTech receives call
  │
  ↓ _editor.CreateModel("VoltageDivider", "ElectricalSystem")
  │
  ↓ ModelTech: _mentorServices.CurrentModel = newModel
  │
  ↓ ModelTech: Publish ModelEditChanged("Created")
  │
  ↓ UI reads _mentorServices.CurrentModel → Shows "VoltageDivider" in tree
  │
  ↓
Tool Call: add_component("Specifications")
  │
  ↓ ModelTech: _editor.AddComponent(parent=CurrentModel, "Specifications")
  │
  ↓ ModelTech: _mentorServices.CurrentComponent = newComponent
  │
  ↓ ModelTech: Publish ModelEditChanged("ComponentAdded")
  │
  ↓ UI refreshes → Shows "Specifications" under "VoltageDivider"
  │
  ↓
Tool Call: set_parameter("InputVoltage", "12 V")
  │
  ↓ ModelTech: _editor.SetParameter(CurrentComponent, "InputVoltage", "12 V")
  │
  ↓ ModelTech: Publish ModelEditChanged("ParameterSet")
  │
  ↓ UI refreshes → Shows parameter in Parameters tab
  │
  ↓
... (continues for all components and parameters)
  │
  ↓
LLM: "Created voltage divider with 4 components, 8 parameters"
```

**Key Points:**
1. Editor maintains state of all created components
2. MentorServices tracks current view
3. UI always reads from MentorServices
4. Events trigger UI refresh
5. Conversation context preserved across tool calls

---

## Critical Principles

### 1. Single Source of Truth
**MentorServices is the authority for current state**

✅ **Correct:**
```csharp
// UI always reads from MentorServices
private KnModel? CurrentModel => _mentorServices.CurrentModel;
```

❌ **Incorrect:**
```csharp
// Don't cache locally
private KnModel? _currentModel;  // Creates stale copies
```

---

### 2. Live Object Graph
**Pass real domain objects, not wrappers**

✅ **Correct:**
```csharp
public OPResult GetComponent(string name)
{
    var component = _editor.FindComponent(name);
    return new OPResult("component", ResultStatus.Instance, component);  // Real object
}
```

❌ **Incorrect:**
```csharp
// Don't create wrapper classes
public ComponentInfo GetComponent(string name)  // Breaks action chain
{
    var component = _editor.FindComponent(name);
    return new ComponentInfo(component.Name, component.Type);  // Copy, not reference
}
```

**Rationale:** LLM needs to manipulate the actual object, not a snapshot

---

### 3. State Persistence
**Editors maintain "bookmarks" throughout conversation**

```csharp
// First tool call
_editor.AddBox("Start", 10, 10, 100, 50, "green");

// ... several tool calls later ...

// Editor still remembers "Start" exists
_editor.AddDirectedLink("Start", "Process");  // Works because state persisted
```

---

### 4. Multiple Editors Pattern
**When complex scenarios require parallel contexts**

```csharp
public class ModelTech
{
    private readonly ModelEditor _mainEditor;
    private ModelEditor? _experimentEditor;  // For trying alternatives
    
    public void ExperimentWithAlternative()
    {
        _experimentEditor = new ModelEditor(_mentorServices);
        // Work on alternative without affecting main model
    }
    
    public void CommitExperiment()
    {
        // Copy successful experiment to main editor
        _mentorServices.CurrentModel = _experimentEditor.WorkingModel;
    }
}
```

---

## UI Integration Pattern

### ConversationalModeler Pattern
```csharp
// CORRECT: Read-only property that always reflects authority
private KnModel? CurrentModel => ModelTech?.CurrentModel;  // Routes to MentorServices

@if (CurrentModel == null)
{
    <div>No model created yet...</div>
}
else
{
    <MentorTreeView/>  // Reads from MentorServices via subscription
}
```

### Event Subscription Pattern
```csharp
protected override void OnInitialized()
{
    // Subscribe to model changes
    _mentorServices.PubSub.Subscribe<ModelEditChanged>(OnModelEditChanged);
}

private void OnModelEditChanged(ModelEditChanged message)
{
    // No need to update local cache - properties read from authority
    InvokeAsync(StateHasChanged);  // Just trigger re-render
}
```

---

## DI Registration Summary

### ✅ Registered as Scoped
```csharp
// Three2025/Program.cs

// Session state
builder.Services.AddScoped<IMentorServices, MentorServices>();

// Technicians (orchestrators)
builder.Services.AddScoped<IModelTech, ModelTech>();
builder.Services.AddScoped<IShape2DTech, Shape2DTech>();
builder.Services.AddScoped<IShape3DTech, Shape3DTech>();
builder.Services.AddScoped<IMentor2DTech, Mentor2DTech>();
```

### ❌ NOT Registered (Created Dynamically)
```csharp
// Editors are created by their technicians
// - ModelEditor (created by ModelTech)
// - Shape2DEditor (created by Shape2DTech)
// - Shape3DEditor (created by Shape3DTech)
// - Mentor2DEditor (created by Mentor2DTech)
```

---

## Benefits of This Architecture

1. **Clear Separation of Concerns**
   - MentorServices = What the user sees
   - Technician = How LLM orchestrates
   - Editor = How domain is manipulated

2. **Flexible State Management**
   - Multiple editors for complex scenarios
   - Explicit bookmarking during conversations
   - Easy to reason about what state exists where

3. **Testable**
   - Can create Technician with mock services
   - Can create Editor standalone for unit tests
   - Can test UI by just manipulating MentorServices

4. **Scalable**
   - Add new Technicians without affecting others
   - Add new Editors without changing DI
   - Multiple users completely isolated

5. **LLM-Friendly**
   - Tools expose high-level operations
   - State persists across tool calls
   - Live object graph enables action chains

---

## Future Extension Patterns

### Adding a New Technician
```csharp
// 1. Create Editor (not in DI)
public class DatabaseEditor : IDatabaseEditor
{
    private readonly IFoundryService _foundryService;
    
    public DatabaseEditor(IFoundryService foundryService)
    {
        _foundryService = foundryService;
    }
    
    // Domain operations...
}

// 2. Create Technician (register as Scoped)
public class DatabaseTech : IDatabaseTech
{
    private readonly IMentorServices _mentorServices;
    private readonly IDatabaseEditor _editor;
    
    public DatabaseTech(IMentorServices mentorServices)
    {
        _mentorServices = mentorServices;
        _editor = new DatabaseEditor(mentorServices.FoundryService);
    }
    
    [Description("Create a database table")]
    public TableInfo CreateTable(string name) 
    {
        var table = _editor.CreateTable(name);
        _mentorServices.PublishChange("TableCreated");
        return new TableInfo(table);
    }
}

// 3. Register in Program.cs
builder.Services.AddScoped<IDatabaseTech, DatabaseTech>();
```

---

## Anti-Patterns to Avoid

### ❌ Caching State Locally
```csharp
private KnModel? _cachedModel;  // DON'T DO THIS

public void OnModelChanged()
{
    _cachedModel = _mentorServices.CurrentModel;  // Creates stale copy
}
```

### ❌ Creating Wrapper Classes
```csharp
public class ComponentWrapper  // DON'T DO THIS
{
    public string Name { get; set; }
    public string Type { get; set; }
}

// Breaks action chain - LLM can't manipulate original
```

### ❌ Injecting Editors via DI
```csharp
// DON'T DO THIS - limits flexibility
public ModelTech(IMentorServices services, IModelEditor editor)
{
    _editor = editor;  // Stuck with single instance
}
```

### ❌ UI Modifying MentorServices Directly
```csharp
// DON'T DO THIS - UI should only read
private void OnButtonClick()
{
    _mentorServices.CurrentModel = newModel;  // Bypasses orchestration
}
```

---

## Summary

This three-layer architecture establishes clear patterns for LLM-driven tool orchestration:

- **MentorServices** holds session truth
- **Technicians** orchestrate LLM tools and create Editors dynamically  
- **Editors** maintain working state and manipulate domain objects
- All Scoped for per-session isolation
- UI always reads from MentorServices
- Live object graphs enable action chains
- State persists throughout conversation

**Follow this pattern for all future Technician/Editor implementations.**
