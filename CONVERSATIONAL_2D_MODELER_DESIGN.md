# Conversational 2D Modeler - Design Specification

**Author**: GitHub Copilot  
**Date**: January 1, 2026  
**Status**: Design Phase  
**Version**: 1.0

---

## Executive Summary

The **Conversational 2D Modeler** integrates AI-driven natural language interaction with visual 2D diagram construction, creating a bidirectional system where:
- **Chatbot → Drawing**: AI generates shapes and connections through conversation
- **Drawing → Chatbot**: Humans manually add shapes, and AI learns from the model
- **Drawing → Model**: Shapes automatically assemble knowledge models via event-driven architecture
- **Model → Drawing**: Existing models can be visualized as interactive diagrams

This creates a unique **"watch your model draw itself"** experience where knowledge models emerge from natural conversation or visual manipulation.

---

## Architecture Overview

### Three-Layer Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                   USER INTERACTION LAYER                     │
│  ┌──────────────────────┐    ┌────────────────────────────┐│
│  │   Chat Interface     │    │   2D Drawing Canvas        ││
│  │  - Message history   │    │  - MentorShape2D nodes     ││
│  │  - Input prompt      │    │  - Drag/drop/connect       ││
│  │  - Test prompts      │    │  - Manual shape tools      ││
│  └──────────────────────┘    └────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
              ↓                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    API LAYER (Technicians)                   │
│  ┌──────────────────────┐    ┌────────────────────────────┐│
│  │  Mentor2DTech        │    │  MentorPlayground          ││
│  │  - CreateShape()     │    │  - Attach()                ││
│  │  - ConnectShapes()   │    │  - CreateNodeShape()       ││
│  │  - MoveShape()       │    │  - Manual interaction      ││
│  └──────────────────────┘    └────────────────────────────┘│
└─────────────────────────────────────────────────────────────┘
              ↓                              ↓
┌─────────────────────────────────────────────────────────────┐
│                EVENT-DRIVEN MODEL LAYER                      │
│  ┌──────────────────────────────────────────────────────────┐
│  │           MentorModelManager (Lookup Table)              │
│  │  - Lookup: ShapeId → KnBase object                       │
│  │  - Connection handlers: Shape links → Knowledge relations│
│  │  - Added handlers: Shape containment → Knowledge grouping│
│  └──────────────────────────────────────────────────────────┘
│  ┌──────────────────────────────────────────────────────────┐
│  │              Knowledge Model (KnBase)                     │
│  │  KnConcept, KnRole, KnComponent, KnProperty, etc.        │
│  └──────────────────────────────────────────────────────────┘
└─────────────────────────────────────────────────────────────┘
```

---

## Core Components

### 1. Conversational2DModeler (UI Component)

**Location**: `Three2025/Components/Conversational2DModeler.razor`

**Layout**: Split-panel design with resizable splitter

```
┌────────────────────────────────────────────────────────────┐
│                     Conversational 2D Modeler              │
├──────────────────────┬────────────────────────────────────┤
│  Chat Panel (40%)    │  Drawing Panel (60%)               │
│                      │                                     │
│ ┌─────────────────┐  │  ┌──────────────────────────────┐ │
│ │ Test Prompts ▼  │  │  │                              │ │
│ └─────────────────┘  │  │                              │ │
│                      │  │      2D Canvas                │ │
│ ┌─────────────────┐  │  │   (MentorShape2D)            │ │
│ │ User: Create... │  │  │                              │ │
│ │ AI: Created...  │  │  │  ┌────┐    ┌─────┐          │ │
│ │ User: Add prop..│  │  │  │Beam│────│Prop │          │ │
│ │ AI: Added...    │  │  │  └────┘    └─────┘          │ │
│ └─────────────────┘  │  │                              │ │
│                      │  └──────────────────────────────┘ │
│ ┌─────────────────┐  │  ┌──────────────────────────────┐ │
│ │ Your message... │  │  │ Shape Tools:                 │ │
│ │         [Send]  │  │  │ [Concept] [Property] [Role]  │ │
│ └─────────────────┘  │  └──────────────────────────────┘ │
│                      │                                     │
│ ┌─────────────────┐  │  ┌──────────────────────────────┐ │
│ │ API Call Log ▼  │  │  │ Model Tree View              │ │
│ └─────────────────┘  │  └──────────────────────────────┘ │
└──────────────────────┴────────────────────────────────────┘
```

**Features**:
- **Chat Panel**:
  - Test prompt dropdown (structural beam, cantilever, thermal analysis, etc.)
  - Message history with role-based styling (user/assistant)
  - Input field with send button
  - Collapsible API call log

- **Drawing Panel**:
  - Embedded 2D canvas (`FoPage2D` integration)
  - Shape creation toolbar (Concept, Property, Role, Component, Feature, etc.)
  - Real-time shape rendering as chatbot responds
  - Manual drag/drop/connect tools
  - Collapsible model tree view

- **Resizable Splitter**: Drag to adjust chat/drawing ratio

---

### 2. Mentor2DTech (AI-Facing API)

**Location**: `Three2025/Apprentice/Mentor2DTech.cs`  
**Interface**: `IMentor2DTech`  
**Purpose**: Clean API for LLM function calling to create/manipulate 2D diagrams

**Current Capabilities**:
```csharp
// Canvas setup
FoPage2D EstablishCanvas2D(string? pageName = null);

// Shape creation (generic boxes for now)
BoxInfo AddBox(string name, string label, int x, int y, int width, int height, string color);
BoxInfo AddStateBox(string name, string label, int x, int y, string color);
BoxInfo AddDecisionBox(string name, string label, int x, int y);

// Link creation
LinkInfo AddDirectedLink(string sourceName, string targetName, string label);

// Queries
BoxInfo? FindBox(string name);
List<BoxInfo> GetAllBoxes();
List<LinkInfo> GetAllLinks();

// Modifications
void MoveBox(string name, int x, int y);
void UpdateBoxLabel(string name, string newLabel);
void DeleteBox(string name);
```

**Needed Enhancements**:
```csharp
// Knowledge-aware shape creation
MentorShapeInfo CreateKnowledgeShape(KnowledgeType type, string title, int x, int y);

// Automatic containment (drop one shape into another)
void AddSubshape(string childName, string parentName);

// Connection with semantic meaning
void ConnectShapes(string sourceName, string targetName, ConnectType type);

// Query knowledge objects
KnBase? GetKnowledgeObject(string shapeName);
List<KnowledgeShapeInfo> GetAllKnowledgeShapes();
```

---

### 3. MentorPlayground (Human-Facing API)

**Location**: `FoundryMentorModeler/Mentor/MentorPlayground.cs`  
**Interface**: `IMentorPlayground`  
**Purpose**: Supports manual shape creation and manipulation by humans

**Current API**:
```csharp
// Create shape from knowledge type
MentorShape2D CreateShape<T>(string title = "") where T : KnBase;

// Attach shapes (automatic connection or containment based on types)
MentorShape2D Attach(MentorShape2D shape, MentorShape2D target);

// Create shape from existing knowledge object
V CreateNodeShape<V>(KnBase model) where V : MentorShape2D;
```

**Shape Attachment Logic**:
- If `target.IsConnectAllowed(shape)` → Creates connector line (`MentorShape1D`)
- If `target.IsDropAllowed(shape)` → Adds as subshape (containment)
- Publishes `DrawingEditChanged` events to trigger model assembly

---

### 4. MentorModelManager (Event-Driven Orchestrator)

**Location**: `FoundryMentorModeler/Mentor/MentorModelManager.cs`  
**Interface**: `IMentorModelManager`  
**Purpose**: Maintains bidirectional mapping between shapes and knowledge objects

**Core Data Structures**:
```csharp
// Primary lookup table: ShapeId → KnBase object
protected Dictionary<string, KnBase> Lookup = new();

// Event handler tables
protected Dictionary<string, Func<DrawingEditChanged, bool>> Connection = new();
protected Dictionary<string, Func<DrawingEditChanged, bool>> Disconnection = new();
protected Dictionary<string, Func<DrawingEditChanged, bool>> Added = new();
protected Dictionary<string, Func<DrawingEditChanged, bool>> Removed = new();
```

**Event Subscriptions**:
```csharp
// Subscribe to drawing events
PubSub.SubscribeTo<DrawingEditChanged>(OnEditorChanged);
PubSub.SubscribeTo<ModelEditChanged>(OnModelChanged);
PubSub.SubscribeTo<SelectionChanged>(OnSelectionChanged);
```

**Event → Action Mappings**:

**Connection Handlers** (shape connected via line):
```csharp
Connection.Add(Key("Context"), (x) => Connect<KnContext>(x, HasSubcontext));
Connection.Add(Key("Role"), (x) => Connect<KnRole>(x, HasSubcomponent));
Connection.Add(Key("Component"), (x) => Connect<KnComponent>(x, HasSubcomponent));
Connection.Add(Key("Concept"), (x) => Connect<KnConcept>(x, HasSubclass));
```

**Grouping Handlers** (shape dropped into another):
```csharp
Added.Add(Key("Role", "Concept"), Group<KnRole, KnConcept>);
Added.Add(Key("Context", "Property"), Group<KnContext, KnProperty>);
Added.Add(Key("Concept", "Property"), Group<KnConcept, KnProperty>);
Added.Add(Key("Component", "Property"), Group<KnComponent, KnProperty>);
Added.Add(Key("Concept", "Variable"), Group<KnConcept, KnVariable>);
```

**The Magic**: When a shape is created/connected/dropped, the appropriate knowledge relationship is automatically established in the model.

---

### 5. MentorShape2D (Visual Knowledge Object)

**Location**: `FoundryMentorModeler/WorkBooks/MentorShape2D.cs`  
**Purpose**: Visual representation of knowledge objects with interaction rules

**Key Methods**:

**Type Rules**:
```csharp
bool IsDropAllowed(IKnowledgeShape? shape);
bool IsConnectAllowed(IKnowledgeShape? shape);
```

**Allowed Containment Relationships** (child, parent):
- Property → Context
- Property → Concept
- Property → Relation
- Property → Component
- Concept → Role
- Concept → Feature
- Variable → Concept
- Formula → Role
- Trait → Concept
- ValidValues → Property

**Allowed Connection Relationships** (source, target):
- Role → Role (hierarchy)
- Component → Component (assembly)
- Context → Context (subcontexts)
- Concept → Concept (inheritance)
- Feature → Concept (realization)
- Feature → Feature (composition)
- Resource → Role (example)

---

## Bidirectional Workflows

### Workflow 1: Chatbot Creates Diagram

**User types**: "Create a structural beam model"

**Sequence**:
1. LLM function call: `Mentor2DTech.CreateKnowledgeShape(KnowledgeType.Component, "Beam", 100, 100)`
2. Mentor2DTech creates `MentorShape2D` with knowledge object attached
3. Adds shape to canvas → renders visually
4. Publishes `DrawingEditChanged.Created` event
5. `MentorModelManager` catches event → adds to `Lookup` table
6. User sees shape appear on canvas

**User types**: "Add a Length property to the beam"

**Sequence**:
1. LLM function call: `Mentor2DTech.CreateKnowledgeShape(KnowledgeType.Property, "Length", 120, 150)`
2. LLM function call: `Mentor2DTech.AddSubshape("Length", "Beam")`
3. Property shape added as subshape of Beam shape
4. Publishes `DrawingEditChanged.ChildAdded(Beam, Length)` event
5. `MentorModelManager` catches event → calls `Group<KnComponent, KnProperty>`
6. **Knowledge model updated**: `beamComponent.Add<KnProperty>(lengthProperty)`
7. User sees property shape nested inside beam shape

---

### Workflow 2: Human Creates Diagram

**User clicks**: "Concept" button in shape toolbar

**Sequence**:
1. `MentorPlayground.CreateShape<KnConcept>("Beam")` called
2. Creates `new KnConcept("Beam")` knowledge object
3. `MentorModelManager.AddKnowledge<KnConcept>(concept)` called
4. Adds to `Lookup` table: `shapeId → concept`
5. Creates `MentorShape2D` linked to concept
6. Adds shape to canvas at default location
7. Publishes `DrawingEditChanged.Created` event
8. User sees concept shape on canvas

**User drags**: Property shape onto Concept shape

**Sequence**:
1. `MentorConstructTool` detects drag operation
2. Checks `concept.IsDropAllowed(property)` → returns `true`
3. Calls `concept.AddSubshape<MentorShape2D>(property, page)`
4. Publishes `DrawingEditChanged.ChildAdded(concept, property)` event
5. `MentorModelManager` calls `Group<KnConcept, KnProperty>`
6. **Knowledge model updated**: `conceptObject.Add<KnProperty>(propertyObject)`
7. Chatbot receives event notification → learns about the new property

---

### Workflow 3: Chatbot Learns from Human Actions

**Human manually adds**: Several shapes and connections

**Sequence**:
1. All actions publish `DrawingEditChanged` events
2. Chatbot has subscribed to event stream (via `ComponentBus`)
3. Chatbot receives event log:
   - `Created(ConceptShape "Table")`
   - `Created(PropertyShape "Width")`
   - `ChildAdded(Table, Width)`
   - `Created(PropertyShape "Height")`
   - `ChildAdded(Table, Height)`
4. Chatbot updates its understanding: "User is modeling a table with Width and Height properties"
5. Chatbot suggests: "Would you like me to add a calculated Volume property?"

---

## Knowledge Type System

### Core Types and Their Visual Representations

| Knowledge Type | Visual Style | Can Contain | Can Connect To |
|---------------|-------------|-------------|----------------|
| **KnConcept** | Blue box, 200px | Property, Variable, Trait, Feature | Concept (inheritance) |
| **KnComponent** | Green box, 200px | Property | Component (assembly) |
| **KnRole** | Purple box, 200px | Concept, Formula | Role (hierarchy) |
| **KnContext** | Orange box, 400px | Property, DefaultValue | Context (subcontext) |
| **KnFeature** | Teal box, 200px | Concept | Feature, Concept |
| **KnProperty** | Yellow box, 200px | Formula, ValidValues | - |
| **KnVariable** | Pink box, 100px | - | - |
| **KnFormula** | Gray box, 100px | - | - |
| **KnTrait** | Cyan box, 100px | - | - |

---

## Event-Driven Message System

### DrawingEditChanged Events

**Published when**: Shapes are created, connected, dropped, deleted, or modified

**Event Types**:
```csharp
enum EditState {
    Created,      // New shape added
    Destroyed,    // Shape deleted
    ChildAdded,   // Shape dropped into another (containment)
    ChildRemoved, // Subshape removed
    Connected,    // Two shapes connected via line
    Disconnected, // Connection removed
    ValueChanged  // Shape text/title edited
}
```

**Message Structure**:
```csharp
class DrawingEditChanged {
    EditState State;
    (FoGlyph2D from, FoGlyph2D to, FoGlyph2D? connector, object? data) Selections;
    ChangeValue? Values; // For title changes
    bool SkipInverse;    // Prevent duplicate inverse relationship updates
}
```

### ModelEditChanged Events

**Published when**: Knowledge model is modified programmatically (not via drawing)

**Event Types**:
```csharp
enum ModelEditState {
    Created,          // New knowledge object
    Destroyed,        // Knowledge object deleted
    ChildAdded,       // Relationship added
    ChildRemoved,     // Relationship removed
    ParameterChanged, // Parameter value updated
    TitleChanged      // Object renamed
}
```

**Usage**: Allows external systems (like `ModelTech`) to update models, and the drawing canvas automatically reflects changes.

---

## Integration with LLM (Function Calling)

### Tool Definitions for LLM

**Mentor2DTech Tools** (AI creates diagrams):

```typescript
{
  name: "create_knowledge_shape",
  description: "Create a typed knowledge shape (Concept, Property, Role, etc.) at specified position",
  parameters: {
    type: { enum: ["Concept", "Component", "Property", "Role", "Context", "Feature", "Variable", "Formula"] },
    title: { type: "string", description: "Display name" },
    x: { type: "integer", description: "X position in pixels" },
    y: { type: "integer", description: "Y position in pixels" }
  }
}

{
  name: "add_subshape",
  description: "Add one shape as a child of another (containment relationship)",
  parameters: {
    childName: { type: "string" },
    parentName: { type: "string" }
  }
}

{
  name: "connect_shapes",
  description: "Connect two shapes with a semantic relationship",
  parameters: {
    sourceName: { type: "string" },
    targetName: { type: "string" },
    relationshipType: { enum: ["HasSubcomponent", "HasSubclass", "HasExample"] }
  }
}

{
  name: "get_all_shapes",
  description: "Get list of all shapes currently on the canvas",
  parameters: {}
}
```

### Example LLM Conversation

**User**: "Create a table with width, height, and depth properties"

**LLM Response**:
```json
[
  {
    "tool": "create_knowledge_shape",
    "args": { "type": "Concept", "title": "Table", "x": 100, "y": 100 }
  },
  {
    "tool": "create_knowledge_shape",
    "args": { "type": "Property", "title": "Width", "x": 120, "y": 150 }
  },
  {
    "tool": "add_subshape",
    "args": { "childName": "Width", "parentName": "Table" }
  },
  {
    "tool": "create_knowledge_shape",
    "args": { "type": "Property", "title": "Height", "x": 120, "y": 180 }
  },
  {
    "tool": "add_subshape",
    "args": { "childName": "Height", "parentName": "Table" }
  },
  {
    "tool": "create_knowledge_shape",
    "args": { "type": "Property", "title": "Depth", "x": 120, "y": 210 }
  },
  {
    "tool": "add_subshape",
    "args": { "childName": "Depth", "parentName": "Table" }
  }
]
```

**AI Message**: "I've created a Table concept with three properties: Width, Height, and Depth. You can now set values for these properties or add formulas."

---

## Implementation Phases

### Phase 1: UI Framework (Current)
- ✅ Split-panel layout with chat and model tree
- ✅ Test prompt dropdown
- ✅ API call logging
- ⬜ **TODO**: Replace model tree panel with 2D canvas integration
- ⬜ **TODO**: Add shape creation toolbar

### Phase 2: Drawing Integration
- ⬜ Embed `FoPage2D` canvas in drawing panel
- ⬜ Wire up `MentorModelManager` event subscriptions
- ⬜ Display shapes created via `MentorPlayground`
- ⬜ Test manual shape creation/connection

### Phase 3: Mentor2DTech Enhancement
- ⬜ Add `CreateKnowledgeShape()` method
- ⬜ Add `AddSubshape()` method
- ⬜ Add `ConnectShapes()` with semantic types
- ⬜ Add `GetKnowledgeObject()` query
- ⬜ Test with manual API calls

### Phase 4: LLM Integration
- ⬜ Define OpenAI function schemas for Mentor2DTech
- ⬜ Add LLM service to ConversationalModeler
- ⬜ Implement function call handling
- ⬜ Test with live prompts

### Phase 5: Bidirectional Learning
- ⬜ Subscribe chatbot to `DrawingEditChanged` events
- ⬜ Format events as conversational updates
- ⬜ Add "What did I just do?" query capability
- ⬜ Test human → AI feedback loop

### Phase 6: Advanced Features
- ⬜ Collision detection (proximity-based auto-connect)
- ⬜ Layout algorithms (auto-arrange shapes)
- ⬜ Save/restore diagrams
- ⬜ Export to PNG/SVG
- ⬜ Undo/redo support

---

## Key Architectural Insights

### 1. **Event-Driven Architecture Eliminates Boilerplate**

Traditional approach:
```csharp
var concept = new KnConcept("Table");
var property = new KnProperty("Width");
concept.Add<KnProperty>(property);
```

Event-driven approach:
```csharp
// Just create and attach shapes - model assembles automatically!
var conceptShape = CreateShape<KnConcept>("Table");
var propertyShape = CreateShape<KnProperty>("Width");
Attach(propertyShape, conceptShape);
// Behind the scenes: MentorModelManager handles concept.Add<KnProperty>(property)
```

### 2. **Type Rules Create Guardrails**

The `IsDropAllowed()` and `IsConnectAllowed()` methods encode domain rules:
- Properties can only be added to Concepts/Components/Contexts
- Concepts can only inherit from other Concepts
- Features connect Concepts to implementations

This prevents invalid model structures automatically.

### 3. **Lookup Table Enables Bidirectional Sync**

```csharp
Dictionary<string, KnBase> Lookup = new();
```

- **Key**: Shape GlyphId (unique visual identifier)
- **Value**: Knowledge object (semantic data)
- **Enables**: Click on shape → highlight in tree, select in tree → highlight shape

### 4. **Event Bus Decouples Systems**

```csharp
PubSub.Publish<DrawingEditChanged>(event);
```

Multiple subscribers can react:
- `MentorModelManager` → updates knowledge model
- `SuccessTreeView` → refreshes tree display
- `Chatbot` → learns from user actions
- `Logger` → records history

No tight coupling between UI, model, and AI.

---

## Success Criteria

### User Experience Goals

1. **"Watch Your Model Draw Itself"**: User types natural language, sees shapes appear and connect in real-time
2. **Seamless Bidirectional Editing**: User can switch between chatting and manually editing without friction
3. **Intelligent Suggestions**: AI learns from manual edits and suggests next steps
4. **Visual Clarity**: Type-specific colors and layouts make relationships obvious
5. **Low Learning Curve**: New users can create valid models without knowing knowledge engineering

### Technical Goals

1. **Event-Driven Consistency**: All model changes (AI or human) flow through same event system
2. **Type Safety**: `IsDropAllowed()` and `IsConnectAllowed()` prevent invalid structures
3. **Performance**: Canvas handles 100+ shapes without lag
4. **Extensibility**: Easy to add new knowledge types and relationship rules
5. **Testability**: Mentor2DTech API can be tested without UI

---

## Open Questions & Future Enhancements

### 1. **Collision Detection**
You mentioned shapes "colliding with each other" triggers events. How does proximity detection work? Should we:
- Implement spatial indexing (quadtree)?
- Define collision radius per shape type?
- Trigger auto-connect when shapes overlap?

### 2. **Layout Algorithms**
Should the system auto-arrange shapes? Options:
- **Force-directed graph** (spring embedder)
- **Hierarchical layout** (tree-based)
- **Grid snapping** (align to grid)
- **Manual + suggestions** (AI suggests positions, human accepts/rejects)

### 3. **Multi-User Collaboration**
Could multiple users (or multiple AI agents) edit the same diagram simultaneously?
- WebSocket-based real-time sync
- Conflict resolution for overlapping edits
- Cursor tracking (see others' mouse pointers)

### 4. **Version Control**
How to handle save/load/history?
- Git-like branching for "what-if" scenarios
- Time-travel debugging (replay event history)
- Diff visualization (show what changed)

### 5. **Domain-Specific Constraints**
Should the system enforce physics/engineering rules?
- Example: Beam must have Length, E, and I before deflection calculation
- Validation errors shown as red highlights
- AI automatically adds missing properties

---

## Conclusion

The Conversational 2D Modeler leverages your proven **event-driven architecture** to create a unique **conversational visual programming environment**. By combining:

1. **MentorModelManager's lookup table** (shape ↔ knowledge object mapping)
2. **Event-driven relationship rules** (automatic model assembly)
3. **Type-specific interaction rules** (prevents invalid structures)
4. **LLM function calling** (AI-driven shape creation)
5. **Bidirectional feedback** (human edits → AI learns)

We create a system where **knowledge models emerge naturally from conversation or visual manipulation**, with zero boilerplate code required from the user.

The **"watch your model draw itself"** experience is not just a UI flourish—it's a **fundamental shift in how humans interact with knowledge representation systems**.

---

## Next Steps

1. **Review this document** - Does this capture your vision?
2. **Refine Mentor2DTech API** - Add missing knowledge-aware methods
3. **Integrate FoPage2D canvas** - Replace tree panel with drawing canvas
4. **Test event flow** - Manually create shapes, verify model assembly
5. **Connect LLM** - Add OpenAI function calling to chatbot
6. **Iterate** - User testing, feedback, refinement

Ready to proceed?
