# DiagramViewer — Blazor.Diagrams Application Specification

**Atlas Phase — Specification Document**  
**Date:** February 13, 2026  
**Feature:** Build diagram-based Blazor pages using the existing Blazor.Diagrams (Z.Blazor.Diagrams) infrastructure in FoundryMentorModeler  
**Experiment:** 07-DiagramViewer

---

## 1. Preamble

### Context for Indy

The workspace already has a **complete, working diagram infrastructure** built on the **Z.Blazor.Diagrams** NuGet package, integrated through **FoundryMentorModeler**. This spec is NOT about building a new diagram library. It is about **using the existing library** to build diagram-based application pages.

The existing infrastructure includes:

| Component | Location | Purpose |
|---|---|---|
| `MentorDiagram` | `FoundryMentorModeler/Diagram/MentorDiagram.cs` | Wraps `BlazorDiagram`, adds `Register<TModel,TComponent>()`, `CreateNode<T>()`, `ConnectPorts<T>()`, `ConnectNodes<T>()`, `ClearAll()` |
| `DiagramNode` | `FoundryMentorModeler/Diagram/DiagramNode.cs` | Base node class, wraps `KnComponent`, implements `IComponentViewer`, `ITreeNode` |
| `DiagramLink` | `FoundryMentorModeler/Diagram/DiagramLink.cs` | Base link class, wraps `KnComponent`, implements `IComponentViewer`, `ITreeNode` |
| `DiagramPort` | `FoundryMentorModeler/Diagram/DiagramPort.cs` | Base port class, wraps `KnComponent`, `CanAttachTo()` validation |
| `DiagramGroup` | `FoundryMentorModeler/Diagram/DiagramGroup.cs` | Base group class, wraps `KnComponent`, autosize support |
| `DiagramLinkLabel` | `FoundryMentorModeler/Diagram/DiagramLinkLabel.cs` | Label attached to links |
| `DiagramPortRenderer` | `FoundryMentorModeler/Diagram/DiagramPortRenderer.cs` | Custom Blazor port renderer component |
| `Node710Renderer` | `FoundryMentorModeler/Diagram/Renderers/Node710Renderer.cs` | Custom Blazor node renderer |
| `Link710Renderer` | `FoundryMentorModeler/Diagram/Renderers/Link710Renderer.cs` | Custom Blazor link renderer |
| `Group710Renderer` | `FoundryMentorModeler/Diagram/Renderers/Group710Renderer.cs` | Custom Blazor group renderer |
| `Port710Renderer` | `FoundryMentorModeler/Diagram/Renderers/Port710Renderer.cs` | Custom Blazor port renderer |
| `IMentorServices` | `FoundryMentorModeler/Mentor/MentorServices.cs` | Service locator with `EstablishDiagram<T>()`, `EstablishModel<T>()`, `CurrentDiagram()` |
| `MentorDiagramManager` | `FoundryMentorModeler/Mentor/MentorDiagramManager.cs` | Manages diagram instances (`EstablishDiagram<T>()`, `SetCurrentDiagram()`) |
| `KnModel` | `FoundryMentorModeler/Mentor/KnModel.cs` | Base model class with `RenderDiagram(view, clear, OnComplete)` |
| `KnComponent` | `FoundryMentorModeler/Mentor/KnComponent.cs` | Base component with `RenderEditor(RenderContextEditor ctx)` pattern |
| `RenderContextEditor` | `FoundryMentorModeler/Mentor/RenderContext.cs` | Immutable context record bundling `Diagram`, `ViewName`, `Deep`, hierarchical `Target`, `PostCreation<T>()`, `ForChild()` |
| `KnEditor2DParameter` | `FoundryMentorModeler/Diagram/KnEditor2DParameter.cs` | Caches `IComponentViewer` for diagram re-render |
| `DiagramDragMovablesBehavior` | `FoundryMentorModeler/Diagram/DiagramDragMovablesBehavior.cs` | Custom drag behavior replacing default |
| `UIDiagram` | `FoundryMentorModeler/Diagram/WindowDiagram/UIDiagram.cs` | Alternative diagram class for windowed UI (custom Pan/Zoom behaviors) |
| `Base_710` | `Plugin_710.Model` (NuGet or local) | Domain model base with `LocationParameters()`, `SizeParameters()`, `MoveTo()`, `Subcomponents<T>()` |

**The existing DiagramViewer.razor page** in Three2025 demonstrates this pattern already, but uses `Plugin_710.Model` types (`SystemBlock_710`, `CircuitNode_710`, etc.) and its three widget files are currently `.disabled`. This is the reference implementation to study.

### What This Spec Enables

A developer (Indy) should be able to build a new diagram page by following these steps:

1. **Create domain model classes** that hold application data (inheriting `KnComponent` or `Base_710`)
2. **Create editor classes** (inheriting `DiagramNode`, `DiagramLink`, or `DiagramGroup`) that bridge domain → diagram
3. **Create widget `.razor` components** that define the visual representation of each node/link type
4. **Create a page** that establishes a diagram, registers widgets, builds a model, and calls `RenderDiagram()`

---

## 2. Project Convention Scan

### Target Directory

New diagram pages go in `Three2025/Components/Pages/`. Diagram widget components go in `Three2025/Components/DiagramWidgets/`.

### Sibling Page Conventions (from reading 3 nearest siblings)

| Convention | Source | Details |
|---|---|---|
| Code-behind pattern | `DiagramViewer.razor` + `.razor.cs` | Razor inherits a base class in the `.cs` file |
| `@rendermode InteractiveServer` | `DiagramViewer.razor` line 3 | All interactive pages use server render mode |
| `@inherits` base class | `DiagramViewer.razor` line 11 | `@inherits DiagramViewerBase` |
| Inject `IMentorServices` | `DiagramViewer.razor.cs` line 22 | `[Inject] protected IMentorServices MentorServices { get; set; } = null!;` |
| Inject `ComponentBus` | `DiagramViewer.razor.cs` line 23 | `[Inject] protected ComponentBus PubSub { get; set; } = null!;` |
| `IDisposable` pattern | `DiagramViewer.razor.cs` line 19 | Base class implements `IDisposable` with `JSDisconnectedException` handling |
| Diagram in `OnInitialized()` | `DiagramViewer.razor.cs` line 43 | `_diagram = MentorServices.EstablishDiagram<MentorDiagram>("DiagramViewerCanvas");` |
| Widget registration in `OnInitialized()` | `DiagramViewer.razor.cs` lines 47-49 | `_diagram.Register<TEditor, TWidget>(true)` before any rendering |
| Model creation in button handlers | `DiagramViewer.razor.cs` line 121 | `_model = MentorServices.EstablishModel<Model_710>("DiagramViewerModel");` |
| `RenderDiagram()` call | `DiagramViewer.razor.cs` line 176 | `_model.RenderDiagram("SystemView", clear: true, () => { ... });` |
| Radzen Splitter layout | `DiagramViewer.razor` line 109 | `RadzenSplitter` with `Orientation.Horizontal`, diagram pane 70%, tree pane 30% |
| `CascadingValue` with `IsFixed="true"` | `DiagramViewer.razor` line 133 | Passes diagram to `DiagramCanvas` component |
| Standard widgets | `DiagramViewer.razor` lines 135-140 | `SelectionBoxWidget`, `GridWidget`, `NavigatorWidget` |

### Existing Files Status

| File | Status | Notes |
|---|---|---|
| `DiagramViewer.razor` | ✅ Exists, active | 182 lines, full page with splitter layout |
| `DiagramViewer.razor.cs` | ✅ Exists, active | 418 lines, `DiagramViewerBase` class |
| `SystemBlockWidget.razor.disabled` | ⚠️ Disabled | In `DiagramWidgets/`, references `Plugin_710.Model` |
| `CircuitNodeWidget.razor.disabled` | ⚠️ Disabled | In `DiagramWidgets/`, references `Plugin_710.Model` |
| `CircuitGroupWidget.razor.disabled` | ⚠️ Disabled | In `DiagramWidgets/`, references `Plugin_710.Model` |
| `Drawing.razor.disabled` | ⚠️ Disabled | 3D drawing page, unrelated to diagram |
| `Drawing.razor.cs` | ✅ Exists, active | 3D drawing code-behind, different pattern (Canvas3D, not diagrams) |

### CSS/JS Already Loaded

In [Components/App.razor](Components/App.razor):
```html
<!-- Head -->
<link rel="stylesheet" href="_content/Z.Blazor.Diagrams/style.min.css" />
<link rel="stylesheet" href="_content/Z.Blazor.Diagrams/default.styles.min.css" />

<!-- Body (after blazor.web.js) -->
<script src="_content/Z.Blazor.Diagrams/script.min.js"></script>
```

Z.Blazor.Diagrams CSS and JS are **already properly loaded**. No changes needed to `App.razor`.

---

## 3. Architecture Analysis

### The Three-Layer Pattern (Verified)

Every diagram page follows this architecture:

```
┌────────────────────────────────────────────────────────────┐
│ APPLICATION LAYER (Three2025/Components/Pages/)            │
│                                                            │
│  Page.razor          — Blazor markup, CascadingValue,      │
│                        DiagramCanvas, RadzenSplitter       │
│  Page.razor.cs       — OnInitialized: EstablishDiagram,    │
│                        Register widgets, build model,      │
│                        call RenderDiagram()                │
├────────────────────────────────────────────────────────────┤
│ EDITOR/WIDGET LAYER (Three2025/Components/DiagramWidgets/) │
│                                                            │
│  MyEditor.cs         — Inherits DiagramNode, wraps domain  │
│                        object via KnComponent              │
│  MyWidget.razor      — Visual rendering of the node,       │
│                        [Parameter] MyEditor Node            │
├────────────────────────────────────────────────────────────┤
│ DOMAIN MODEL LAYER (Plugin_710 or application-specific)    │
│                                                            │
│  MyModel : KnModel   — override RenderDiagram() to call   │
│                        RenderEditor on child components    │
│  MyBlock : Base_710   — Domain data, RenderEditor()        │
│                        creates DiagramNode via             │
│                        KnComponent.RenderEditor(ctx)       │
├────────────────────────────────────────────────────────────┤
│ INFRASTRUCTURE (FoundryMentorModeler — NuGet package)      │
│                                                            │
│  MentorDiagram, DiagramNode, DiagramLink, DiagramPort,     │
│  DiagramGroup, RenderContextEditor, KnEditor2DParameter,   │
│  IMentorServices, MentorDiagramManager                     │
├────────────────────────────────────────────────────────────┤
│ DIAGRAM ENGINE (Z.Blazor.Diagrams — NuGet package)         │
│                                                            │
│  BlazorDiagram, NodeModel, LinkModel, GroupModel,          │
│  PortModel, DiagramCanvas, SelectionBoxWidget, GridWidget  │
└────────────────────────────────────────────────────────────┘
```

### Modern vs Legacy Distinction

**Modern pattern** (use this): `KnComponent.RenderEditor(RenderContextEditor ctx)` — the base class handles establish/finalize/PostCreation. Domain components just need to exist in the model tree, and the base `RenderEditor` in `KnComponent` automatically:
1. Calls `EstablishEditor(view, diagram)` to get/create a `KnEditor2DParameter`
2. Checks `wasUnknown` to detect first-time creation
3. Calls `FinalizeEditor(view)` to resolve the parameter value
4. Calls `ctx.PostCreation(viewer)` to register with the diagram (only on first creation)
5. Recursively calls `RenderEditor(childCtx)` on all `Subcomponents<KnComponent>()`

**The key insight**: For basic node types, you do NOT need to override `RenderEditor` in your domain classes. The base `KnComponent.RenderEditor()` does everything automatically — as long as the editor type is registered on the diagram and the domain objects are in the model tree via `AddChildComponent<T>()`.

**Custom `RenderEditor` overrides** are needed when:
- You want to skip rendering based on visibility (`if (!IsVisible) return;`)
- You need custom port creation after node creation
- You want non-standard child traversal (e.g., traversing relationships instead of `Subcomponents<T>()`)

### The Registration → Rendering Flow (Verified Against Source)

```
OnInitialized():
  diagram = MentorServices.EstablishDiagram<MentorDiagram>("name")
  diagram.Register<MyEditor, MyWidget>(true)
    └─ Internally: BlazorDiagram.RegisterComponent<TModel, TComponent>(replace)
       This tells the diagram: when rendering a MyEditor node, use MyWidget.razor

OnAfterRenderAsync(firstRender) or Button Handler:
  model = MentorServices.EstablishModel<Model_710>("name")
    └─ Gets or creates model, stores via MentorModelManager
  
  // Build domain model tree...
  model.AddChildComponent<KnComponent>(block)
  
  model.RenderDiagram("viewName", clear: true, () => { })
    └─ KnModel.RenderDiagram:
       1. var (found, diagram) = _services.CurrentDiagram()
       2. var ctx = RenderContextEditor.Create(diagram, view, deep: true)
       3. if (clear) diagram.ClearAll()
       4. For each child in GetCollection<KnComponent>():
          child.RenderEditor(ctx)
            └─ KnComponent.RenderEditor:
               a. EstablishEditor(view, diagram) → KnEditor2DParameter
               b. wasUnknown = !param.IsValid()
               c. FinalizeEditor(view) → resolves parameter
               d. viewer = param.GetCurrentValueAs<IComponentViewer>()
               e. if (wasUnknown) ctx.PostCreation(viewer)
                  └─ Adds to diagram: Nodes.Add / Links.Add / Groups.Add
                  └─ If inside a group: parentGroup.AddChild(node)
               f. For each Subcomponents<KnComponent>():
                  child.RenderEditor(ctx.ForChild(viewer))
```

### How Widgets Get Resolved at Render Time

When `ctx.PostCreation(viewer)` adds a `DiagramNode` to `Diagram.Nodes`, the Blazor.Diagrams `DiagramCanvas` component renders it. The rendering pipeline:

1. `DiagramCanvas` iterates `diagram.Nodes`
2. For each node, `Node710Renderer` (or default renderer) calls `BlazorDiagram.GetComponent(node)` 
3. This returns the widget type registered via `diagram.Register<TEditor, TWidget>(true)`
4. The renderer creates a dynamic component of that widget type, passing the node as `[Parameter]`
5. The widget's `.razor` file renders the HTML/CSS for that node

---

## 4. Verified API Table

| Method | Class | Signature | Status | Where Verified |
|---|---|---|---|---|
| `EstablishDiagram<T>(name)` | `IMentorServices` | `T EstablishDiagram<T>(string pageName) where T : MentorDiagram` | ✅ VERIFIED | `MentorServices.cs` line 40, 175 |
| `EstablishModel<T>(name)` | `IMentorServices` | `T EstablishModel<T>(string title) where T : KnModel` | ✅ VERIFIED | `MentorServices.cs` line 38, 167 |
| `CurrentDiagram()` | `IMentorServices` | `(bool found, MentorDiagram diagram) CurrentDiagram()` | ✅ VERIFIED | `MentorServices.cs` line 41, 179 |
| `Register<TModel, TComponent>(replace)` | `MentorDiagram` | `void Register<TModel, TComponent>(bool replace = false)` | ✅ VERIFIED | `MentorDiagram.cs` line 74 |
| `ClearAll()` | `MentorDiagram` | `void ClearAll()` | ✅ VERIFIED | `MentorDiagram.cs` line 95 |
| `CreateNode<T>(source, point)` | `MentorDiagram` | `T CreateNode<T>(KnComponent source, Point point) where T : DiagramNode` | ✅ VERIFIED | `MentorDiagram.cs` line 113 |
| `ConnectPorts<T>(source, color, from, to)` | `MentorDiagram` | `T ConnectPorts<T>(KnComponent source, string color, PortModel fromPort, PortModel toPort) where T : DiagramLink` | ✅ VERIFIED | `MentorDiagram.cs` line 150 |
| `ConnectNodes<T>(source, from, to)` | `MentorDiagram` | `T ConnectNodes<T>(KnComponent source, NodeModel fromNode, NodeModel toNode) where T : DiagramLink` | ✅ VERIFIED | `MentorDiagram.cs` line 161 |
| `RenderDiagram(view, clear, onComplete)` | `KnModel` | `virtual MentorDiagram RenderDiagram(string view, bool clear, Action OnComplete)` | ✅ VERIFIED | `KnModel.cs` line 103 |
| `RenderEditor(ctx)` | `KnComponent` | `virtual IComponentViewer? RenderEditor(RenderContextEditor ctx)` | ✅ VERIFIED | `KnComponent.cs` line 312 |
| `RenderContextEditor.Create(diagram, view, deep)` | `RenderContextEditor` | `static RenderContextEditor Create(MentorDiagram diagram, string viewName, bool deep = true)` | ✅ VERIFIED | `RenderContext.cs` line 106 |
| `PostCreation<T>(viewer)` | `RenderContextEditor` | `T PostCreation<T>(T viewer) where T : class, IComponentViewer` | ✅ VERIFIED | `RenderContext.cs` line 141 |
| `ForChild(parentViewer)` | `RenderContextEditor` | `RenderContextEditor ForChild(IComponentViewer parentViewer)` | ✅ VERIFIED | `RenderContext.cs` line 123 |
| `EstablishEditor(view, diagram)` | `KnComponent` | `virtual KnEditor2DParameter EstablishEditor(string view, MentorDiagram diagram)` | ✅ VERIFIED | `KnComponent.cs` line 347 |
| `SetActive() / SetInactive()` | `MentorDiagram` | `void SetActive()` / `void SetInactive()` | ✅ VERIFIED | `MentorDiagram.cs` lines 43, 55 |
| `EstablishPort<T>(source, alignment, point)` | `DiagramNode` | `T EstablishPort<T>(KnComponent source, PortAlignment alignment, Point? point) where T : DiagramPort` | ✅ VERIFIED | `DiagramNode.cs` line 45 |
| `DiagramNode(KnComponent, Point)` | `DiagramNode` | constructor | ✅ VERIFIED | `DiagramNode.cs` line 24 |
| `DiagramLink(KnComponent, PortModel, PortModel)` | `DiagramLink` | constructor (port-based) | ✅ VERIFIED | `DiagramLink.cs` line 52 |
| `DiagramLink(KnComponent, NodeModel, NodeModel)` | `DiagramLink` | constructor (node-based) | ✅ VERIFIED | `DiagramLink.cs` line 70 |
| `DiagramGroup(KnComponent, Point)` | `DiagramGroup` | constructor | ✅ VERIFIED | `DiagramGroup.cs` line 22 |
| `AddLabel(label, fraction)` | `DiagramLink` | `DiagramLinkLabel AddLabel(string label, double fraction = 0.5)` | ✅ VERIFIED | `DiagramLink.cs` line 80 |

---

## 5. Reference Implementation Strategy

### Copy This: `DiagramViewer.razor` + `DiagramViewer.razor.cs`

The existing DiagramViewer page is the **definitive reference**. It demonstrates every pattern needed. To build a new diagram page:

1. **Copy** `DiagramViewer.razor` → `YourPage.razor`
2. **Copy** `DiagramViewer.razor.cs` → `YourPage.razor.cs`
3. **Rename** the base class and update `@inherits`
4. **Replace** the model/editor/widget types with your own
5. **Enable or create** the widget `.razor` files in `DiagramWidgets/`

### Copy This Widget Pattern: `SystemBlockWidget.razor.disabled`

The disabled widget files show the exact widget structure:

```razor
@namespace Three2025.Components.DiagramWidgets
@using FoundryMentorModeler.Diagram
@using Blazor.Diagrams.Components.Renderers
@rendermode @(new InteractiveServerRenderMode(false))

<div class="my-node @(Node.Selected ? "selected" : "")">
    <div class="node-title">@Node.GetTitle()</div>
    
    @foreach (var port in PortList())
    {
        <Port710Renderer Port="@port" Class="@port.Alignment.ToString().ToLower()" />
    }
</div>

@code {
    [Parameter] public MyEditor Node { get; set; } = null!;
    
    public List<DiagramPort> PortList()
    {
        return Node.Ports.Cast<DiagramPort>().ToList();
    }
}
```

**Critical detail**: Widget uses `Port710Renderer` (not `PortRenderer`) for port rendering. This is the custom renderer in FoundryMentorModeler that handles `DiagramPort` types.

---

## 6. Infrastructure Assumptions

| Assumption | Confidence | Investigation Starting Point |
|---|---|---|
| Z.Blazor.Diagrams NuGet is referenced by Three2025 project | High | Check `Three2025.csproj` for package reference (may be transitive via FoundryMentorModeler) |
| `IMentorServices` is registered in DI container | High | Check `Program.cs` or `CodeStatus.cs` in FoundryMentorModeler for `services.AddScoped<IMentorServices, MentorServices>()` |
| `ComponentBus` is registered in DI | High | Check `Program.cs` |
| `MentorDiagramManager` is registered in DI | ✅ VERIFIED | `FoundryMentorModeler/CodeStatus.cs` line 58: `services.AddScoped<IMentorDiagramManager, MentorDiagramManager>()` |
| `Plugin_710.Model` types are available | ⚠️ UNCERTAIN | DiagramViewer.razor.cs references `Plugin_710.Model` but the code files are in `KnModel/` folder with `.csHOLD` extension — may not compile |
| Custom renderers (`Node710Renderer` etc.) are used automatically | ⚠️ ASSUMED | These are in the FoundryMentorModeler NuGet; may need explicit configuration |
| `MentorTreeView` component exists and works | ⚠️ ASSUMED | Referenced in DiagramViewer.razor line 168 but not searched for |
| `RefreshRenderMessage` is available | ⚠️ ASSUMED | Used in `RefreshTree()` method but not verified |

---

## 7. Code Path Traces

### Trace 1: From Button Click to Visible Node

```
User clicks "Create System Diagram" button
  → CreateSystemDiagram() handler fires
    → _model = MentorServices.EstablishModel<Model_710>("DiagramViewerModel")
      → MentorModelManager finds/creates model, stores it, calls model.SetMentorServices(services)
    → solution = _model.EstablishSolution()
    → solution.Build<SystemBlock_710>(component, index, lookup) — creates domain objects
    → _model.AddChildComponent<KnComponent>(block) — adds to model tree
    → _diagram.ClearAll() — removes existing nodes/links/groups
    → _model.RenderDiagram("SystemView", clear: true, callback)
      → KnModel.RenderDiagram:
        → (found, diagram) = _services.CurrentDiagram()
        → ctx = RenderContextEditor.Create(diagram, "SystemView", deep: true)
        → for each child KnComponent:
          → child.RenderEditor(ctx)
            → EstablishEditor("SystemView", diagram) → KnEditor2DParameter
            → FinalizeEditor("SystemView") → resolves value
            → viewer = param.GetCurrentValueAs<IComponentViewer>()
              → This creates a SystemBlockEditor(component, point)
            → ctx.PostCreation(viewer)
              → diagram.Nodes.Add(node) — NOW VISIBLE
            → Recurse children with ctx.ForChild(viewer)
    → Blazor re-renders, DiagramCanvas sees new nodes
    → Node710Renderer renders each node
      → BlazorDiagram.GetComponent(node) → returns SystemBlockWidget type
      → Renders SystemBlockWidget.razor with Node parameter
```

### Trace 2: How `EstablishEditor` Creates the Right Node Type

This is the **least obvious** part of the system. The `KnEditor2DParameter` uses the **evaluator** system:

```
EstablishEditor("SystemView", diagram):
  → Creates KnEditor2DParameter("SystemView") if not exists
  → Returns the parameter

FinalizeEditor("SystemView"):
  → Calls GetCurrentValueAs<IComponentViewer>()
  → If cache is empty (first time):
    → Evaluates the parameter — which uses the evaluator/formula system
    → The evaluated result is the IComponentViewer (a DiagramNode subclass)
  → If cache has value (re-render):
    → Returns cached IComponentViewer
```

**Important**: The `KnEditor2DParameter` caches the `IComponentViewer`. On re-render, calling `SmashAllGeometry()` or `ClearAllGeometry()` on the model clears these caches, forcing new nodes to be created.

---

## 8. Service Integration Audit

### `MentorDiagramManager.EstablishDiagram<T>(pageName)`
- **Implementation read**: ✅ YES — `MentorDiagramManager.cs` lines 89-117
- **Behavior**: Creates `MentorDiagram` via `Activator.CreateInstance` with `BlazorDiagramOptions` (multi-selection enabled, zoom enabled, orthogonal router, smooth path generator). Caches in `DiagramLookup` dictionary. Sets as current diagram.
- **Integration seam**: The `BlazorDiagramOptions` are hardcoded — orthogonal router + smooth path generator. If you want different routing, you'd need to reconfigure after establishment.

### `IMentorServices.CurrentDiagram()`
- **Implementation read**: ✅ YES — `MentorDiagramManager.cs` lines 63-78
- **Behavior**: Returns first diagram marked `IsSelected` from `DiagramLookup`. If none selected, creates/selects default "Diagram". 
- **Integration seam**: If multiple pages establish diagrams, `CurrentDiagram()` returns whichever was last set via `SetCurrentDiagram()`. The `EstablishDiagram<T>()` call automatically sets the new diagram as current.

### `KnModel.RenderDiagram(view, clear, OnComplete)`
- **Implementation read**: ✅ YES — `KnModel.cs` lines 103-125
- **Behavior**: Gets `CurrentDiagram()`, creates `RenderContextEditor`, clears if requested, iterates `GetCollection<KnComponent>()` calling `RenderEditor(ctx)` on each.
- **Integration seam**: Uses `GetCollection<KnComponent>()` — these are direct children added via `AddChildComponent<KnComponent>()` or `AddMember<KnComponent>()`. If you used `HasSub` relationship (Base_710 pattern), those are accessed via `Subcomponents<T>()` instead.

### `RenderContextEditor.PostCreation<T>(viewer)`
- **Implementation read**: ✅ YES — `RenderContext.cs` lines 134-161
- **Behavior**: Switch on viewer type — `DiagramGroup` → `Diagram.Groups.Add`, `DiagramNode` → `Diagram.Nodes.Add` (+ `parentGroup.AddChild` if inside group context), `DiagramLink` → `Diagram.Links.Add`.
- **Integration seam**: Clean. No hardcoded names or routing.

---

## 9. Known Gotchas

### 9.1 Plugin_710 Availability
The existing `DiagramViewer.razor.cs` references `Plugin_710.Model` types (`Model_710`, `SystemBlock_710`, `CircuitNode_710`, `SystemLink_710`, `Common_710`, `LayoutType_710`). The `.csHOLD` files in `Components/Pages/KnModel/` suggest these were part of a local plugin that may or may not be in the build path. The `SystemBlockEditor`, `CircuitNodeEditor`, `CircuitGroupEditor` types referenced in widget registration are NOT found in any `.cs` file — they likely come from the Plugin_710 NuGet or are not yet created.

**Indy action**: Check if `Plugin_710.Model` types resolves. If not, you'll need to either:
- Create local editor classes that inherit `DiagramNode`
- Or find the NuGet that provides them

### 9.2 Widget `.disabled` Files
The three widget files have `.disabled` extension — they won't compile. They also have `@// DISABLED: using Plugin_710.Model` comments. They need to be renamed to `.razor` and have their imports fixed to be usable.

### 9.3 `RenderEditor` vs Manual Node Creation
The `DiagramViewer.razor.cs` uses BOTH patterns:
- **Pattern A** (line 176): `_model.RenderDiagram("SystemView", clear: true, callback)` — lets the model/component infrastructure create nodes via `RenderEditor`
- **Pattern B** (implicit in model builder): Manually creating blocks, setting positions via `Calculations()`, adding to model

These are complementary, not competing. Pattern A triggers rendering; Pattern B builds the model tree that Pattern A renders.

### 9.4 Diagram Disposal
The `Dispose()` method in `DiagramViewerBase` wraps `_diagram?.ClearAll()` in a try-catch for `JSDisconnectedException`. This is essential — without it, dispose during circuit disconnection throws.

### 9.5 MentorTreeView Dependency
The `DiagramViewer.razor` line 168 uses `<MentorTreeView />` in the right panel. This component is not in the same file — verify it exists and what it expects (likely reads from the model tree via PubSub events).

---

## 10. Troubleshooting Guide

| Symptom | Likely Cause | Fix |
|---|---|---|
| Diagram canvas shows but no nodes | Widget registration missing or wrong | Verify `diagram.Register<TEditor, TWidget>(true)` in `OnInitialized()` BEFORE any render call |
| Nodes render as plain boxes | Default widget used instead of custom | Check that editor class matches registration: `Register<MyEditor, MyWidget>` and the node IS a `MyEditor` instance |
| "Object disposed" exception on navigate away | Missing `JSDisconnectedException` catch in Dispose | Wrap `ClearAll()` in try-catch like `DiagramViewerBase.Dispose()` |
| Nodes don't appear after `RenderDiagram` | `CurrentDiagram()` returning wrong diagram | Call `MentorServices.MentorDiagramManager.SetCurrentDiagram(_diagram)` after establishing |
| Ports not rendering | Using `PortRenderer` instead of `Port710Renderer` | Use custom `Port710Renderer` from FoundryMentorModeler |
| Links not connecting | Ports not initialized | Ensure ports are added via `node.EstablishPort<DiagramPort>(source, alignment)` AFTER node is added to diagram |
| Clicking nodes throws null ref | Widget `[Parameter]` not typed correctly | Widget must have `[Parameter] public MyEditor Node { get; set; }` — the property name must be `Node` for `NodeModel` descendants |
| Context menu not working | Missing `@oncontextmenu:preventDefault` | Add both `@oncontextmenu="Handler"` and `@oncontextmenu:preventDefault` to widget div |
| Model tree not updating | PubSub not firing refresh | Call `PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null))` after model changes |

---

## 11. Implementation Steps

### Step 1: Choose Your Domain

Decide what your diagram represents. Examples:
- System architecture (blocks + connections)
- Circuit diagram (nodes + wires)
- Process flow (steps + transitions)
- Knowledge graph (concepts + relationships)

### Step 2: Create Domain Model Classes

If using `Base_710` pattern (recommended for Plugin_710 compatibility):

```csharp
// Your domain block — holds application data
public class MyBlock_710 : Base_710
{
    public MyBlock_710(DT_Component source) : base(source) { }
    public MyBlock_710(string name) : base(name) { }
    
    // Domain-specific properties/methods
}

// Your model — container for all blocks
public class MyModel_710 : KnModel
{
    protected MySolution_710 solution;
    
    public MySolution_710 EstablishSolution() 
    {
        if (solution == null)
        {
            solution = new MySolution_710(Common_710.New_DT_Component("Solution"));
            AddChildComponent<MySolution_710>(solution);
        }
        return solution;
    }
}
```

If using plain `KnComponent` (simpler, no Plugin_710 dependency):

```csharp
public class MyBlock : KnComponent
{
    public MyBlock(string name) : base(name) { }
    // Domain data
}

public class MyModel : KnModel
{
    // Override RenderDiagram if you need custom rendering logic
}
```

### Step 3: Create Editor Classes

```csharp
using Blazor.Diagrams.Core.Geometry;
using FoundryMentorModeler.Diagram;
using FoundryMentorModeler.Model;

namespace Three2025.Components.DiagramWidgets;

// Node editor — bridges domain object to diagram node
public class MyBlockEditor : DiagramNode
{
    public MyBlockEditor(KnComponent source, Point position) 
        : base(source, position)
    {
        Title = source.Name;
    }
    
    public MyBlock GetBlock() => GetComponent() as MyBlock;
}

// Link editor — bridges domain connection to diagram link (if needed)
public class MyLinkEditor : DiagramLink
{
    public MyLinkEditor(KnComponent source, PortModel sourcePort, PortModel targetPort)
        : base(source, "black", sourcePort, targetPort) { }
    
    // Or node-to-node:
    public MyLinkEditor(KnComponent source, NodeModel from, NodeModel to)
        : base(source, from, to) { }
}
```

### Step 4: Create Widget Components

`DiagramWidgets/MyBlockWidget.razor`:
```razor
@namespace Three2025.Components.DiagramWidgets
@using FoundryMentorModeler.Diagram
@using Blazor.Diagrams.Components.Renderers
@rendermode @(new InteractiveServerRenderMode(false))

<style>
    .my-block-node { /* your styles */ }
    .my-block-node.selected { /* selected state */ }
</style>

<div class="my-block-node @(Node.Selected ? "selected" : "")">
    <div class="node-title">@Node.GetTitle()</div>
    
    @foreach (var port in PortList())
    {
        <Port710Renderer Port="@port" Class="@port.Alignment.ToString().ToLower()" />
    }
</div>

@code {
    [Parameter] public MyBlockEditor Node { get; set; } = null!;
    
    public List<DiagramPort> PortList()
        => Node.Ports.Cast<DiagramPort>().ToList();
}
```

### Step 5: Create the Page

`Pages/MyDiagramPage.razor`:
```razor
@page "/my-diagram"
@namespace Three2025.Components.Pages
@rendermode InteractiveServer
@using FoundryMentorModeler.Diagram
@using Blazor.Diagrams
@using Blazor.Diagrams.Components
@using Blazor.Diagrams.Components.Widgets
@inherits MyDiagramPageBase

<CascadingValue Value="_diagram" IsFixed="true">
    <DiagramCanvas>
        <Widgets>
            <SelectionBoxWidget />
            <GridWidget Size="50" Mode="GridMode.Line" BackgroundColor="white" />
            <NavigatorWidget Width="200" Height="120" />
        </Widgets>
    </DiagramCanvas>
</CascadingValue>
```

`Pages/MyDiagramPage.razor.cs`:
```csharp
public class MyDiagramPageBase : ComponentBase, IDisposable
{
    [Inject] protected IMentorServices MentorServices { get; set; } = null!;
    
    protected MentorDiagram? _diagram;
    protected MyModel? _model;
    
    protected override void OnInitialized()
    {
        _diagram = MentorServices.EstablishDiagram<MentorDiagram>("MyDiagramCanvas");
        _diagram.Register<MyBlockEditor, MyBlockWidget>(true);
        base.OnInitialized();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _model = MentorServices.EstablishModel<MyModel>("MyModel");
            // Build model tree...
            _model.RenderDiagram("MyView", clear: true, () => { });
            StateHasChanged();
        }
    }
    
    public void Dispose()
    {
        try { _diagram?.ClearAll(); }
        catch (JSDisconnectedException) { }
        _diagram = null;
    }
}
```

### Step 6: Register Navigation (Optional)

Add the page to your navigation menu if desired.

---

## 12. Success Criteria / Verification Checklist

- [ ] Page loads without errors
- [ ] Diagram canvas renders with grid background
- [ ] Nodes appear at specified positions when model is built
- [ ] Nodes render using custom widget (not default gray boxes)
- [ ] Nodes are draggable on the canvas
- [ ] Ports render on nodes (if using ports)
- [ ] Links connect between nodes/ports
- [ ] Navigator widget shows minimap in bottom-right
- [ ] Selection box widget allows multi-select
- [ ] Disposing the page does not throw `JSDisconnectedException`
- [ ] Model tree (if using MentorTreeView) reflects the domain model
- [ ] Nodes can be added dynamically (add to model tree, re-render)
- [ ] `ClearAll()` removes all nodes/links/groups from canvas

---

## 13. Visual Expectations

### Expected Page Layout

```
┌────────────────────────────────────────────────────────────┐
│  Header / Controls                                         │
├───────────────────────────────────┬────────────────────────┤
│                                   │                        │
│    DIAGRAM CANVAS (70%)           │  MODEL TREE (30%)      │
│                                   │                        │
│  ┌─────────┐    ┌──────────┐     │  📦 Model              │
│  │  Node A  │───│  Node B  │     │    ├── Block A          │
│  └─────────┘    └──────────┘     │    ├── Block B          │
│       │                           │    └── Block C          │
│  ┌─────────┐                     │                        │
│  │  Node C  │                    │  [Refresh] button       │
│  └─────────┘                     │                        │
│                                   │                        │
│              Grid background      │                        │
│                    [Navigator]     │                        │
├───────────────────────────────────┴────────────────────────┤
│  Status bar / counts                                       │
└────────────────────────────────────────────────────────────┘
```

### Node Appearance

Each node type has its own widget defining:
- Border color and style
- Background gradient
- Title text
- Optional badges/info
- Port positions (top/bottom/left/right circles)
- Selected state (highlighted border, shadow)

Links appear as SVG paths connecting ports or node centers, using orthogonal routing (right angles) with smooth path generation.

---

## Appendix: Key File Locations

| Purpose | Path |
|---|---|
| Diagram infrastructure | `FoundryMentorModeler/Diagram/` |
| Renderers | `FoundryMentorModeler/Diagram/Renderers/` |
| Window diagram | `FoundryMentorModeler/Diagram/WindowDiagram/` |
| Services | `FoundryMentorModeler/Mentor/MentorServices.cs` |
| Diagram manager | `FoundryMentorModeler/Mentor/MentorDiagramManager.cs` |
| Model base | `FoundryMentorModeler/Mentor/KnModel.cs` |
| Component base | `FoundryMentorModeler/Mentor/KnComponent.cs` |
| Render context | `FoundryMentorModeler/Mentor/RenderContext.cs` |
| Interfaces | `FoundryMentorModeler/Mentor/IRender.cs` |
| Existing page | `Three2025/Components/Pages/DiagramViewer.razor(.cs)` |
| Existing widgets (disabled) | `Three2025/Components/DiagramWidgets/*.razor.disabled` |
| CSS/JS loading | `Three2025/Components/App.razor` |
| Pattern guide | `Three2025/Docs/Patterns/SCENARIO_CHAT_DIAGRAM_IMPLEMENTATION_GUIDE.md` |
| Pattern reference | `Three2025/Docs/Patterns/BLAZOR_DIAGRAM_IMPLEMENTATION_REFERENCE.md` |
