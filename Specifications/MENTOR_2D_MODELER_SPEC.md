# Mentor 2D Modeler — Architectural Specification

**Purpose:** Document the complete design of the Mentor 2D Visual Modeler page so it can be reproduced, debugged, and extended in another Blazor application.  
**Author:** Claude "Atlas" (Architect)  
**Date:** February 13, 2026  
**Source Application:** Three2025 (Blazor Server, .NET 9.0)

---

## For Indy — Before You Start

**Read the Handoff Brief first:** [`MENTOR_2D_MODELER_HANDOFF_BRIEF.md`](MENTOR_2D_MODELER_HANDOFF_BRIEF.md)

This spec tells you *what to build*. The Handoff Brief tells you *how much to trust each part of this spec*. Every method, service, and component referenced here is annotated as VERIFIED (Atlas read the source), ASSUMED (Atlas didn't check), INFERRED (Atlas guessed from patterns), or UNREAD (Atlas never opened the file).

**Your first 15 minutes:** Open the Handoff Brief's "Verification Priority" section. Check those items before writing code. The most expensive bugs come from ASSUMED and UNREAD areas.

**When this spec disagrees with reality:** Trust reality. Note the override.

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about reproducing a visual modeler — it's about testing whether AI can successfully use **persistent memory and expression systems** to build tools where humans create knowledge models through both conversation and direct manipulation.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 2D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you reproduce the Mentor 2D Modeler, you're testing whether AI can:
1. Successfully integrate three subsystems — chat AI, 2D canvas rendering, and knowledge model management — into a cohesive visual experience
2. Understand how technicians bridge AI tool-calling to domain-specific shape creation on a canvas
3. Build a page where conversation and direct manipulation both create the same underlying model

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can take a sophisticated three-panel visual modeling environment and reproduce it — understanding the interplay between chat, canvas, and model tree.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## 1. System Overview

The Mentor 2D Modeler is a **three-panel visual knowledge modeling environment** embedded in a Blazor Server application. Users build knowledge models through two complementary interaction modes:

1. **Conversational modeling** — Chat with an AI agent that has tool-calling access to model creation and 2D shape creation APIs
2. **Direct manipulation** — Click toolbar buttons to create typed knowledge shapes (Concept, Property, Role, etc.) on a 2D canvas, then drag-drop to attach, connect, and rearrange

### Core Capabilities
- **Three-Panel Layout:** Chat (25%) + Canvas (50%) + Model Explorer (25%) using RadzenSplitter
- **Knowledge Shape Types:** Concept, Property, Role, Context, Component, Formula, Feature, Relation, Variable, ValidValues — each with a distinct color and icon
- **AI-Assisted Modeling:** LLM creates models via tool calls (establish_model, add_component, set_parameter) and visual shapes via ModelTech's visual methods
- **Direct Shape Creation:** Toolbar buttons create MentorShape2D instances on the canvas via MentorStudio
- **Shape Relationships:** Drag-drop creates parent-child containment or connector lines between shapes via MentorStudio.Attach()
- **Dual Tree Views:** Model tree (MentorTreeView — knowledge hierarchy) and Shape tree (ShapeTreeView — canvas objects) plus Activity log
- **Pre-built Examples:** "Quick Beam" creates a Concept with attached Properties; "Strategic Plan" creates a multi-level model with Contexts, Properties, ValidValues, Roles, and Formulas
- **Persistence:** Save/Load model via MentorServices.MentorModel

---

## 2. Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│  LAYER 1: PAGE (Mentor2DModeler)                                │
│  - @page "/mentor2d-modeler"                                    │
│  - Three-panel RadzenSplitter layout                            │
│  - Owns chat UI, toolbar, and tree panel                        │
│  - Creates MentorStudio in OnAfterRenderAsync                   │
│  - Wires technicians to canvas page                             │
└──────────┬──────────────────┬────────────────┬──────────────────┘
           │                  │                │
     ┌─────▼─────┐    ┌──────▼──────┐   ┌─────▼──────┐
     │  CHAT AI   │    │  2D CANVAS  │   │   TREES    │
     │ Orchestrat │    │ Canvas2D    │   │ MentorTree │
     │ → ModelTech│    │ Component   │   │ ShapeTree  │
     │ → Shape2D  │    │ (rendering) │   │ ActivityLog│
     └─────┬──────┘    └──────┬──────┘   └────────────┘
           │                  │
     ┌─────▼──────────────────▼──────────────────────────────────┐
     │  LAYER 2: TECHNICIANS (AI Tool Bridge)                     │
     │  - ModelTech: establish_model, add_component, set_parameter│
     │    + visual methods: CreateConceptShape, CreatePropertyShape│
     │  - Shape2DTech: AddRectangle, AddCircle, MoveShape, etc.   │
     │  Both connected to canvas page via SetPage/SetPageContext   │
     └──────────┬────────────────────────────────────────────────┘
                │
     ┌──────────▼────────────────────────────────────────────────┐
     │  LAYER 3: MENTOR STUDIO (IMentorStudio)                    │
     │  - CreateShape<T>() — creates KnBase + MentorShape2D       │
     │  - Attach() — containment or connection via shape rules     │
     │  - Publishes DrawingEditChanged events                      │
     │  - Manages KnowledgeType → shape mapping                    │
     └──────────┬────────────────────────────────────────────────┘
                │
     ┌──────────▼────────────────────────────────────────────────┐
     │  LAYER 4: FOUNDRY INFRASTRUCTURE                           │
     │  - IWorkspace → IDrawing → FoPage2D (canvas page)          │
     │  - MentorShape2D — shaped knowledge node on canvas          │
     │  - KnBase hierarchy — KnConcept, KnProperty, KnRole, etc.  │
     │  - IMentorServices — model manager, pub/sub, diagram mgr    │
     └───────────────────────────────────────────────────────────┘
```

---

## 3. File Inventory

### 3.1 Page Files

| File | Location | Lines | Purpose |
|------|----------|-------|---------|
| `Mentor2DModeler.razor` | `Components/Pages/` | 587 | Markup: 3-panel layout, toolbar, chat, tree tabs, CSS |
| `Mentor2DModeler.razor.cs` | `Components/Pages/` | 722 | Code-behind: lifecycle, shape creation, chat, logging |

### 3.2 Key Dependencies

| Component/Service | Location | Purpose |
|---|---|---|
| `Canvas2DComponent` | `FoundryWorldsAndDrawings/Shared/` | 2D HTML5 canvas with animation loop |
| `MentorStudio` | `FoundryMentorModeler/Mentor/MentorStudio.cs` | Shape factory + attachment logic |
| `ModelTech` | `Three2025/Apprentice/ModelTech.cs` | AI tool bridge for models + visual shapes |
| `Shape2DTech` | `Three2025/Apprentice/Shape2DTech.cs` | AI tool bridge for raw 2D shapes |
| `ChatPanel` | `Three2025/Components/Shared/Chat/` | Chat UI component |
| `MentorTreeView` | `FoundryMentorModeler/Shared/` | Knowledge model tree view |
| `ShapeTreeView` | ⚠️ **Referenced but may not exist as component** | Canvas shape tree view |
| `TestSequenceSelector` | `Three2025/Components/Shared/Testing/` | Pre-built test prompt runner |
| `ChatOrchestrator` | `Three2025/Services/Chat/` | AI routing + tool execution |

### 3.3 Knowledge Types and their Shape Colors

| KnowledgeType | Class | Emoji | Toolbar Color | Description |
|---|---|---|---|---|
| Concept | `KnConcept` | 📘 | `#3498db` (blue) | Core domain concept |
| Property | `KnProperty` | 📋 | `#f39c12` (orange) | Named attribute with value |
| Role | `KnRole` | 👤 | `#9b59b6` (purple) | Actor or responsibility |
| Context | `KnContext` | 📦 | `#95a5a6` (gray) | Scoping container |
| Component | `KnComponent` | 🔧 | `#27ae60` (green) | Structural part |
| Formula | `KnFormula` | ƒ | `#e74c3c` (red) | Calculation expression |
| Feature | `KnFeature` | ⚙️ | `#34495e` (dark gray) | Capability |
| Relation | `KnRelation` | 🔗 | `#16a085` (teal) | Connection between concepts |
| Variable | `KnVariable` | 🔢 | transparent | Numeric parameter |
| ValidValues | `KnValidValues` | ✓ | transparent | Enumerated choices |

---

## 4. DI Registration

All services required by this page:

```csharp
// In Program.cs or via AddFoundryMentorModelerServices()
builder.Services.AddFoundryWorldsAndDrawingsServices(envConfig);  // IWorkspace, IDrawing, etc.
builder.Services.AddFoundryMentorModelerServices();               // IMentorServices, IMentorStudio, etc.

// Technicians (scoped — may hold per-circuit state)
builder.Services.AddScoped<IShape2DTech, Shape2DTech>();
builder.Services.AddScoped<IModelTech, ModelTech>();

// Chat infrastructure
builder.Services.AddScoped<ITechnicianToolProvider, TechnicianToolProvider>();
builder.Services.AddScoped<IChatOrchestrator, ChatOrchestrator>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
```

**Critical:** `MentorStudio` is registered in DI via `AddFoundryMentorModelerServices()`, but the page creates its own instance manually:
```csharp
Playground = new MentorStudio(Workspace, MentorServices.PubSub, MentorServices);
```
This is intentional — the page needs a studio instance connected to its specific canvas page.

---

## 5. Page Lifecycle

### 5.1 OnInitializedAsync

```csharp
protected override async Task OnInitializedAsync()
{
    base.OnInitialized();
    await LogInfo("Mentor 2D Visual Modeler initializing...");
}
```

Minimal — real setup happens after the canvas renders.

### 5.2 OnAfterRenderAsync (firstRender only)

This is the **critical initialization sequence**. Order matters:

```
1. Wait 100ms for canvas JS interop to initialize
2. Verify Canvas2DReference.Page is not null → ABORT if null
3. Get Drawing from Workspace
4. Connect Shape2DTech to canvas page: Shape2DTech.SetPage(canvasPage)
5. Log tool count from ChatOrchestrator
6. Create MentorStudio: new MentorStudio(Workspace, PubSub, MentorServices)
7. Connect ModelTech to MentorStudio: ModelTech.SetPageContext(canvasPage, Playground)
8. Verify studio page matches canvas page
9. StateHasChanged()
```

**Why this order:** The technicians need a canvas page to target. The canvas page doesn't exist until `Canvas2DComponent.OnAfterRenderAsync` creates it from the `SceneName`. The 100ms delay gives the canvas component time to initialize. Then everything gets wired to that page.

---

## 6. Three-Panel Layout

```
┌────────────────────┬──────────────────────────────┬────────────────────┐
│   Chat Panel (25%) │     Canvas Panel (50%)        │  Explorer (25%)    │
│                    │                               │                    │
│ ┌──────────────┐   │ ┌──────────Toolbar──────────┐ │ ┌──Tab Bar──────┐ │
│ │ 🎨 AI Assist │   │ │📘📋👤📦🔧ƒ⚙️🔗🔢✓│ │ │Model│Shape│Act│ │
│ │    Header    │   │ │         ⭕🗑️🧪💾📂 │ │ └───────────────┘ │
│ ├──────────────┤   │ └────────────────────────────┘ │                    │
│ │              │   │                               │ ┌──────────────┐  │
│ │   Messages   │   │  ┌──────────────────────────┐ │ │              │  │
│ │              │   │  │                          │ │ │  MentorTree  │  │
│ │              │   │  │   Canvas2DComponent      │ │ │   View  or   │  │
│ │              │   │  │   SceneName=             │ │ │  ShapeTree   │  │
│ │              │   │  │   "Mentor2DModeler"      │ │ │   View  or   │  │
│ │              │   │  │                          │ │ │  Activity    │  │
│ │              │   │  │                          │ │ │   Log        │  │
│ │              │   │  └──────────────────────────┘ │ │              │  │
│ ├──────────────┤   │                               │ └──────────────┘  │
│ │ Quick Tests  │   │                               │                    │
│ ├──────────────┤   │                               │                    │
│ │  Chat Input  │   │                               │                    │
│ └──────────────┘   │                               │                    │
└────────────────────┴──────────────────────────────┴────────────────────┘
```

### 6.1 Left Panel — Chat

Uses `<ChatPanel>` with:
- **Custom header:** gradient banner "🎨 Mentor 2D AI Assistant" + knowledge type chips (Concepts/Properties/Roles) + `<TestSequenceSelector>`
- **Custom footer actions:** Quick test buttons (Test Concept, Test Property, Test Model, List Tools, Quick Beam, Strategic Plan, Clear)
- **Chat processing:** Calls `ChatOrchestrator.ProcessMessageAsync()` (non-streaming) with tool execution
- **PageContext:** Name="Mentor 2D Modeler", Route="/mentor2d-modeler", DomainFocus describes AI capabilities

### 6.2 Center Panel — Canvas with Toolbar

- **Toolbar:** Gradient header with 10 knowledge type buttons + test/clear/save/load buttons
- **Canvas:** `<Canvas2DComponent SceneName="Mentor2DModeler" @ref="Canvas2DReference" />`
- **Toolbar buttons** call `CreateConcept()`, `CreateProperty()`, etc. → each delegates to `Playground.CreateShape<KnType>(title, Canvas2DReference.Page)`

### 6.3 Right Panel — Model Explorer

Three tabs controlled by `_activeTreeTab`:
- **"model" tab:** `<MentorTreeView/>` — shows knowledge model hierarchy from IMentorServices
- **"shape" tab:** `<ShapeTreeView/>` — ⚠️ referenced but component existence uncertain
- **"activity" tab:** Activity log with dark terminal-style background, auto-scroll, clear button

---

## 7. Shape Creation — Two Paths

### Path 1: Toolbar Buttons (Direct Manipulation)

```
User clicks "📘 Concept" toolbar button
  → CreateConcept()
    → CreateShape(() => Playground.CreateShape<KnConcept>("", Canvas2DReference.Page), "Concept")
      → MentorStudio.CreateShape<KnConcept>(title, page)
        → Creates KnConcept instance, registers in ModelManager
        → Creates MentorShape2D, calls SetKnowledge(), adds to page
        → Publishes DrawingEditChanged.Created
      → shape.MoveTo(nextX, nextY)
      → Advance position grid (nextX += 150, wrap at 800)
      → RefreshTree()
      → StateHasChanged()
```

### Path 2: AI Chat (Conversational)

```
User types "Create a concept called Engine with properties Horsepower and Torque"
  → SendChatMessage()
    → ChatOrchestrator.ProcessMessageAsync(message, pageContext, history)
      → Intent analysis → routes to agent
      → Agent calls LLM with tools
      → LLM calls establish_model("Engine") → ModelTech
      → LLM calls add_component("Horsepower") → ModelTech
      → LLM calls set_parameter("value", "...", "Horsepower") → ModelTech
      → (Optional) ModelTech.CreateConceptShape() → MentorStudio → canvas
    → Response returned to chat
    → StateHasChanged()
```

**Note:** The AI path creates models via `ModelTech` tool calls, but visual shape creation depends on whether `ModelTech.SetPageContext()` was called (it is, in OnAfterRenderAsync). The model-only methods (establish_model, add_component, set_parameter) work regardless of canvas connection.

---

## 8. Pre-Built Examples

### 8.1 Beam Example (`CreateBeamExample()`)

Creates a simple engineering concept model:
```
Beam (KnConcept) at (200, 200)
  ├── Length (KnProperty) — attached via Playground.Attach()
  ├── Width (KnProperty) — attached
  └── Height (KnProperty) — attached
```

`Playground.Attach(property, beam)` triggers `DrawingEditChanged.ChildAdded` → property becomes a subshape inside the beam shape on canvas, and the beam auto-resizes.

### 8.2 Strategic Plan Example (`CreateStrategicPlanExample()`)

Creates a complex multi-level model demonstrating all knowledge types:
```
Strategic Plan Pro (KnContext) at (400, 100)
  ├── Q1: "Is this profit or non-profit?" (KnContext)
  │   └── Answer1 (KnProperty)
  │       └── "profit; non-profit" (KnValidValues)
  ├── Q2: "New organization or on-going concern?" (KnContext)
  │   └── Answer2 (KnProperty)
  │       └── "new; on-going" (KnValidValues)
  └── Q3: "Detailed plan for product or service?" (KnContext)
      └── Answer3 (KnProperty)
          └── "yes; no" (KnValidValues)

Strategic Plan Pro (KnRole) at (400, 500)
  └── General Plan (KnRole)
      ├── New Non-profit Plan (KnRole)
      │   ├── Formula: "Answer1@ == 'non-profit' && Answer2@ == 'new'" (KnFormula)
      │   └── New Non-profit TEMPLATE (KnConcept)
      └── On-going Non-profit Plan (KnRole)
          ├── Formula: "Answer1@ == 'non-profit' && Answer2@ == 'on-going'" (KnFormula)
          └── On-going Non-profit TEMPLATE (KnConcept)
```

Uses `CreateShapeWithType()` helper that maps `KnowledgeType` enum to `CreateShape<T>()` calls.

---

## 9. Chat Integration

### 9.1 Chat Processing (Non-Streaming)

Unlike `ChatOrchestratorTest` which bypasses the orchestrator, this page uses the **full orchestrator path**:

```csharp
var response = await ChatOrchestrator.ProcessMessageAsync(
    userMessage,
    pageContext,
    conversationHistory,
    onAgentSwitch: async (agentName) => { ... });
```

This means:
- Intent analysis occurs (LLM-based routing to specialized agents)
- Agent-specific system prompts are used
- Tool execution happens inside the orchestrator, not the page

### 9.2 Tool Categories Available

| Technician | Tools Available | Example |
|---|---|---|
| `ModelTech` | establish_model, add_component, set_parameter, get_parameter, list_components, get_component | "Create a model called Engine" |
| `Shape2DTech` | AddRectangle, AddCircle, AddText, ConnectShapes, MoveShape, SetColor, GetShapes, DeleteShape | "Draw a red circle" |
| Other registered technicians | All via TechnicianToolProvider discovery | Varies |

### 9.3 Message Queue

Test sequences load prompts into a `Queue<string>`, processed sequentially with 2000ms delays between messages.

---

## 10. Activity Logging

The page has its own logging system (separate from ChatPanel's ActivityLog):

```csharp
private class LogEntry
{
    public string Level { get; set; } = "info";     // info, success, warning, error
    public string Message { get; set; } = "";
    public DateTime Timestamp { get; set; }
}
```

Methods: `LogInfo()`, `LogSuccess()`, `LogWarning()`, `LogError()` — all append to `List<LogEntry> ActivityLog` with auto-scroll behavior.

Displayed in the "Activity" tab with a dark terminal-style UI (background: `#1e1e1e`, monospace font, colored left borders per log level).

---

## 11. Comparison with ConversationalModeler

| Aspect | Mentor 2D Modeler | ConversationalModeler |
|---|---|---|
| Route | `/mentor2d-modeler` | `/conversational-modeler` |
| Panels | 3 (Chat + Canvas + Tree) | 2 (Chat + Tree) |
| Canvas | Yes — `Canvas2DComponent` | No |
| Shape Creation | Toolbar + AI | AI only (model, not visual) |
| Technicians | `IModelTech` + `IShape2DTech` | `IModelTech` only |
| MentorStudio | Created manually, connected to canvas | Not used |
| Tree Tabs | Model + Shapes + Activity | Model + Parameters + Activity |
| Chat Processing | `ProcessMessageAsync` (non-streaming) | `ProcessMessageAsync` (non-streaming) |
| Quick Tests | Beam, Strategic Plan | Office Desk, Brick Patio, etc. |
| Focus | Visual diagramming + conversation | Pure conversational modeling |

---

## 12. Known Issues & Technical Debt

### 12.1 ShapeTreeView Component
The "Shapes" tab references `<ShapeTreeView/>` but this component may not exist as a `.razor` file in the workspace. If it compiles, it may be defined in a library not visible here, or it may be a dead reference. Indy should verify.

### 12.2 MentorStudio Created Outside DI
The page creates `new MentorStudio(...)` manually rather than injecting `IMentorStudio`. This is intentional (needs page-specific canvas reference) but means the studio instance doesn't participate in DI lifecycle.

### 12.3 Duplicate IMentorServices Registration
`IMentorServices` is registered both in `AddFoundryMentorModelerServices()` and directly in `Program.cs`. Last registration wins.

### 12.4 100ms Initialization Delay
`OnAfterRenderAsync` uses `await Task.Delay(100)` to wait for canvas JS interop. This is fragile — on slow machines, the canvas may not be ready in 100ms.

### 12.5 Position Grid Doesn't Account for Canvas Size
`nextX` wraps at 800, `nextY` wraps at 600 — hardcoded values that don't match `Canvas2DComponent`'s default size (1800x1200).

### 12.6 No Streaming Response
Chat uses `ProcessMessageAsync` (non-streaming), unlike `ChatOrchestratorTest` which has streaming infrastructure. User sees no typing indicator during AI response generation.

---

## 13. Reproducing in Another Application

### Step 1: Infrastructure
Ensure these are registered:
- `AddFoundryWorldsAndDrawingsServices()` — canvas, drawing, workspace
- `AddFoundryMentorModelerServices()` — mentor model, studio, services

### Step 2: Technicians
Register and connect:
```csharp
builder.Services.AddScoped<IShape2DTech, Shape2DTech>();
builder.Services.AddScoped<IModelTech, ModelTech>();
```
Both must be connected to the canvas page in `OnAfterRenderAsync`:
```csharp
Shape2DTech.SetPage(canvasPage);
ModelTech.SetPageContext(canvasPage, mentorStudio);
```

### Step 3: Chat System
Register the chat orchestrator system (see `CHAT_ORCHESTRATOR_SYSTEM_SPEC.md`).

### Step 4: Page Layout
Three-panel RadzenSplitter with:
- Left: `<ChatPanel>` with quick test buttons
- Center: `<Canvas2DComponent>` with knowledge type toolbar
- Right: `<MentorTreeView/>` + activity log tabs

### Step 5: Shape Creation
Wire toolbar buttons to `MentorStudio.CreateShape<KnType>(title, page)` with position management.

---

*This specification captures the complete design of the Mentor 2D Visual Modeler page as of February 13, 2026.*
