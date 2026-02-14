# DiagramViewer — Native Blazor Diagram Library Specification

**Atlas Phase — Specification Document**
**Date:** 2025-02-13
**Feature:** A native Blazor diagram rendering library that uses standard Blazor component rendering (SVG/HTML via `RenderTreeBuilder`) instead of Canvas2D JS interop or third-party diagram libraries.

---

## 1. Preamble

### Context for Indy

The workspace currently has **two distinct rendering paradigms** for diagrams:

1. **Canvas2D** (FoundryWorldsAndDrawings) — An imperative retained-mode 2D graphics engine using HTML5 `<canvas>` with batched JS interop. All shapes (`FoGlyph2D` hierarchy) are drawn via `Canvas2DContext` calls. No DOM elements, no accessibility, no Blazor composition.

2. **Blazor.Diagrams** (FoundryMentorModeler) — Uses the `Z.Blazor.Diagrams` v3.0.3 NuGet package. Diagram models (`DiagramNode`, `DiagramLink`, `DiagramGroup`, `DiagramPort`) extend library base classes. Custom renderers (`Node710Renderer`, etc.) use `BuildRenderTree` to emit SVG/HTML. Widgets are `.razor` components resolved at runtime via `BlazorDiagram.GetComponent()`.

**The DiagramViewer** is a **third approach**: a fully native Blazor diagram library owned by Foundry, with no third-party diagram dependency. It uses standard Blazor rendering — `RenderTreeBuilder`, `ComponentBase`, SVG elements, CSS positioning — to produce interactive diagrams where **every visual element is a real DOM node** that Blazor controls.

### Why Build This

| Concern | Canvas2D | Z.Blazor.Diagrams | Native DiagramViewer |
|---------|----------|-------------------|---------------------|
| **Ownership** | Full | Third-party NuGet | Full |
| **Rendering** | JS interop canvas calls | Blazor + custom renderers | Pure Blazor SVG/HTML |
| **Composability** | None (imperative draw) | Limited (widget resolution) | Full Blazor composition |
| **Accessibility** | None (opaque canvas) | Partial (DOM elements) | Full (semantic SVG/HTML) |
| **Hit testing** | Manual matrix math | DOM events (through library) | Native DOM events |
| **Dynamic controls** | Not supported | Via `Register<TModel, TComponent>()` | Via `RenderFragment` / `DynamicComponent` |
| **Dependency** | None | Z.Blazor.Diagrams v3.0.3 | None |

### Design Philosophy

- **SVG-first rendering** — Diagrams are SVG documents. Nodes can be `<foreignObject>` wrappers around HTML/Blazor components, or pure SVG shapes. Links are SVG `<path>` elements. This gives us vector scaling, CSS styling, and native browser events.
- **Blazor-native composition** — Every diagram element is a `ComponentBase` with `[Parameter]` properties. No custom resolve/register pattern needed; use standard Blazor `RenderFragment`, `DynamicComponent`, or typed child components.
- **Model-View separation** — Diagram models are pure C# objects (no base class dependency on a third-party library). Views are Blazor components that observe model changes.
- **Integration with existing infrastructure** — Models implement `ITreeNode` (for tree views) and `IComponentViewer` (for domain model bridging), exactly as the current `DiagramNode`/`DiagramLink`/etc. do.

---

## 2. Project Convention Scan

### Target Location

The DiagramViewer library will live in **FoundryWorldsAndDrawings** alongside the existing Canvas2D infrastructure, under a new `Diagram/` directory. This follows the pattern where FoundryWorldsAndDrawings provides rendering primitives consumed by FoundryMentorModeler and Three2025.

### Namespace

`FoundryWorldsAndDrawings.Diagram`

### Conventions Observed

| Convention | Source | Applied |
|---|---|---|
| Triple-interface pattern | `DiagramNode : NodeModel, IComponentViewer, ITreeNode` | Models implement `IComponentViewer` + `ITreeNode` (no third-party base) |
| `StatusBitArray` for state | All diagram models in FoundryMentorModeler | Same pattern |
| `KnComponent` wrapping | All diagram models | Same pattern — `GetComponent()` / `SetComponent()` |
| Generic factory methods | `MentorDiagram.CreateNode<T>()` | Same generic factory pattern |
| Find-or-create ports | `DiagramNode.EstablishPort<T>()` | Same pattern |
| Custom behaviors | `DiagramDragMovablesBehavior` | Behavior system with pluggable drag/pan/zoom |
| `.razor` widgets | `CircuitNodeWidget.razor` | Diagram element widgets as `.razor` components |
| `BuildRenderTree` renderers | `Node710Renderer.cs` | SVG renderers using `RenderTreeBuilder` |
| Batched operations | `Canvas2DContext.BeginBatch/EndBatch` | Model change batching for efficient re-render |

---

## 3. Architecture Analysis

### Layer Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Application Layer (Three2025)                 │
│  Pages that host diagrams, register widgets, build models       │
├─────────────────────────────────────────────────────────────────┤
│              Domain Bridge (FoundryMentorModeler)                │
│  Subclassed diagram models wrapping KnComponent domain objects  │
├─────────────────────────────────────────────────────────────────┤
│           DiagramViewer Library (FoundryWorldsAndDrawings)       │
│  ┌─────────────┐ ┌──────────────┐ ┌───────────────────────┐    │
│  │   Models     │ │  Components  │ │   Behaviors           │    │
│  │ DvDiagram    │ │ DvCanvas     │ │ DvDragBehavior        │    │
│  │ DvNode       │ │ DvNodeView   │ │ DvPanBehavior         │    │
│  │ DvLink       │ │ DvLinkView   │ │ DvZoomBehavior        │    │
│  │ DvPort       │ │ DvPortView   │ │ DvSelectionBehavior   │    │
│  │ DvGroup      │ │ DvGroupView  │ │ DvLinkCreationBehavior│    │
│  │ DvLabel      │ │ DvLabelView  │ │ DvKeyboardBehavior    │    │
│  │ DvLayer      │ │ DvGridView   │ │                       │    │
│  └─────────────┘ └──────────────┘ └───────────────────────┘    │
│  ┌─────────────┐ ┌──────────────┐ ┌───────────────────────┐    │
│  │  Layout      │ │  Routing     │ │  Geometry             │    │
│  │ TreeLayout   │ │ OrthRouter   │ │ DvPoint / DvSize      │    │
│  │ ForceLayout  │ │ StraightRouter│ │ DvRect / DvMatrix     │    │
│  │ GridLayout   │ │ BezierRouter │ │ DvPathBuilder         │    │
│  │ DagreLayout  │ │              │ │ DvAnchor              │    │
│  └─────────────┘ └──────────────┘ └───────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

### Model Layer — Pure C# (No Third-Party Base Classes)

All model classes use the `Dv` prefix (DiagramViewer) to avoid collision with existing `Diagram*` classes.

#### DvDiagram — The Root Container

```csharp
namespace FoundryWorldsAndDrawings.Diagram;

public class DvDiagram : ITreeNode
{
    // Identity
    public string Id { get; }
    public string ViewName { get; }

    // Layers (ordered rendering)
    public DvLayer<DvGroup> Groups { get; }
    public DvLayer<DvNode> Nodes { get; }
    public DvLayer<DvLink> Links { get; }

    // Viewport state
    public DvViewport Viewport { get; }          // Pan/Zoom state
    public DvSelectionState Selection { get; }   // Multi-select tracking

    // Options
    public DvDiagramOptions Options { get; }

    // Behaviors (pluggable interaction handlers)
    private List<DvBehavior> _behaviors;
    public void RegisterBehavior<T>(T behavior) where T : DvBehavior;
    public void UnregisterBehavior<T>() where T : DvBehavior;
    public T? GetBehavior<T>() where T : DvBehavior;

    // Factory methods (generic, same pattern as MentorDiagram)
    public T CreateNode<T>(DvPoint position) where T : DvNode;
    public T CreateGroup<T>(DvPoint position) where T : DvGroup;
    public T ConnectPorts<T>(DvPort source, DvPort target) where T : DvLink;

    // Widget registration (maps model types to Blazor component types)
    public void RegisterWidget<TModel, TComponent>() where TComponent : ComponentBase;
    public Type? GetWidgetType(Type modelType);
    public Type? GetWidgetType<TModel>();

    // Lifecycle
    public void ClearAll();
    public void RemoveNode(DvNode node);
    public void RemoveLink(DvLink link);
    public void RemoveGroup(DvGroup group);

    // Change notification (batched)
    public event Action? Changed;
    public void BeginBatch();
    public void EndBatch();    // fires Changed once
    public void SuspendRefresh();
    public void ResumeRefresh();

    // Ordered rendering
    public void BringToFront(DvNode node);
    public void SendToBack(DvNode node);

    // ITreeNode implementation
    // ...
}
```

#### DvNode — Base Node Model

```csharp
public class DvNode : ITreeNode
{
    public string Id { get; }
    public string Title { get; set; }
    public DvPoint Position { get; set; }       // Top-left corner
    public DvSize Size { get; set; }            // Width × Height
    public bool Visible { get; set; }
    public bool Locked { get; set; }
    public StatusBitArray StatusBits { get; }

    // Ports
    public IReadOnlyList<DvPort> Ports { get; }
    public T AddPort<T>(DvPortAlignment alignment) where T : DvPort;
    public T? FindPort<T>(DvPortAlignment alignment) where T : DvPort;
    public void RemovePort(DvPort port);

    // Links (convenience — derived from ports)
    public IEnumerable<DvLink> GetConnectedLinks();

    // Grouping
    public DvGroup? Group { get; internal set; }

    // Events
    public event Action<DvNode>? Moved;
    public event Action<DvNode>? Resized;
    public event Action<DvNode>? Changed;

    // Selection
    public bool IsSelected { get; set; }

    // ITreeNode
    // ...
}
```

#### DvLink — Connection Model

```csharp
public class DvLink : ITreeNode
{
    public string Id { get; }
    public string? Title { get; set; }
    public string Color { get; set; }

    // Endpoints (anchor-based for flexibility)
    public DvAnchor Source { get; set; }
    public DvAnchor Target { get; set; }

    // Routing
    public IDvRouter Router { get; set; }           // Orthogonal, Straight, Bezier
    public IDvPathGenerator PathGenerator { get; set; }

    // Vertices (user-placed waypoints)
    public IList<DvPoint> Vertices { get; }

    // Labels
    public IList<DvLinkLabel> Labels { get; }
    public DvLinkLabel AddLabel(string text, double fraction = 0.5);

    // Computed path (output of router + path generator)
    public string SvgPath { get; }                  // Computed SVG `d` attribute
    public IReadOnlyList<DvPoint> Route { get; }    // Computed route points

    // Events
    public event Action<DvLink>? Changed;

    // Reconnection
    public void Reconnect(DvAnchor source, DvAnchor target);
}
```

#### DvPort — Connection Point Model

```csharp
public class DvPort : ITreeNode
{
    public string Id { get; }
    public DvNode Parent { get; }
    public DvPortAlignment Alignment { get; set; }
    public DvPoint Offset { get; set; }            // Relative to parent node
    public DvSize Size { get; set; }               // Default 12×12
    public string Color { get; set; }

    // Connection rules
    public int MaxLinks { get; set; }              // 0 = unlimited
    public virtual bool CanConnectTo(DvPort other);

    // Computed position (parent position + offset)
    public DvPoint GetAbsolutePosition();

    // Connected links
    public IReadOnlyList<DvLink> Links { get; }
}
```

#### DvGroup — Container Model

```csharp
public class DvGroup : DvNode
{
    public IReadOnlyList<DvNode> Children { get; }
    public double Padding { get; set; }
    public bool AutoSize { get; set; }

    public void AddChild(DvNode node);
    public void RemoveChild(DvNode node);
    public DvRect ComputeBounds();               // Union of children + padding
}
```

#### DvLabel — Text Label Model

```csharp
public class DvLinkLabel : ITreeNode
{
    public string Id { get; }
    public string Text { get; set; }
    public double Distance { get; set; }         // 0-1 = fraction along path
    public DvPoint Offset { get; set; }          // Additional offset from path position
    public DvPoint ComputedPosition { get; }     // Resolved position on path

    public event Action<DvLinkLabel>? Changed;
}
```

### Geometry Primitives

```csharp
namespace FoundryWorldsAndDrawings.Diagram;

public record struct DvPoint(double X, double Y)
{
    public static DvPoint Zero => new(0, 0);
    public DvPoint Add(DvPoint other) => new(X + other.X, Y + other.Y);
    public DvPoint Subtract(DvPoint other) => new(X - other.X, Y - other.Y);
    public double DistanceTo(DvPoint other) => Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
}

public record struct DvSize(double Width, double Height)
{
    public static DvSize Zero => new(0, 0);
}

public record struct DvRect(double X, double Y, double Width, double Height)
{
    public DvPoint Center => new(X + Width / 2, Y + Height / 2);
    public bool Contains(DvPoint point) => ...;
    public bool Intersects(DvRect other) => ...;
    public DvRect Union(DvRect other) => ...;
    public static DvRect FromPoints(DvPoint topLeft, DvPoint bottomRight) => ...;
}

public record struct DvMatrix(double M11, double M12, double M21, double M22, double OffsetX, double OffsetY)
{
    public DvPoint Transform(DvPoint point) => ...;
    public static DvMatrix Identity => new(1, 0, 0, 1, 0, 0);
    public static DvMatrix CreateTranslation(double x, double y) => ...;
    public static DvMatrix CreateScale(double sx, double sy) => ...;
}
```

### Anchor System

Anchors define where a link endpoint attaches. This abstracts port-based, node-center, and point-based connections:

```csharp
public abstract class DvAnchor
{
    public abstract DvPoint GetPosition(DvLink link);
    public abstract DvNode? GetNode();
}

public class DvPortAnchor : DvAnchor
{
    public DvPort Port { get; }
    public override DvPoint GetPosition(DvLink link) => Port.GetAbsolutePosition();
    public override DvNode? GetNode() => Port.Parent;
}

public class DvNodeAnchor : DvAnchor
{
    public DvNode Node { get; }
    public override DvPoint GetPosition(DvLink link) => Node.Position.Add(new(Node.Size.Width / 2, Node.Size.Height / 2));
    public override DvNode? GetNode() => Node;
}

public class DvPositionAnchor : DvAnchor
{
    public DvPoint Position { get; set; }
    public override DvPoint GetPosition(DvLink link) => Position;
    public override DvNode? GetNode() => null;
}
```

### Viewport Model

```csharp
public class DvViewport
{
    public DvPoint Pan { get; set; }           // Current pan offset
    public double Zoom { get; set; }            // Current zoom level (1.0 = 100%)
    public double MinZoom { get; set; }
    public double MaxZoom { get; set; }
    public DvSize ContainerSize { get; set; }   // Size of the hosting element

    // Coordinate transforms
    public DvPoint ScreenToWorld(DvPoint screen);
    public DvPoint WorldToScreen(DvPoint world);

    // Pan/Zoom operations
    public void PanBy(double dx, double dy);
    public void ZoomTo(double zoom, DvPoint? focus = null);
    public void ZoomToFit(DvRect bounds, double padding = 20);

    public event Action? Changed;

    // SVG transform string
    public string GetTransformString() => $"translate({Pan.X},{Pan.Y}) scale({Zoom})";
}
```

### Routing Interfaces

```csharp
public interface IDvRouter
{
    /// <summary>
    /// Compute route points from source anchor to target anchor,
    /// respecting intermediate vertices and avoiding obstacles.
    /// </summary>
    IReadOnlyList<DvPoint> Route(DvLink link);
}

public interface IDvPathGenerator
{
    /// <summary>
    /// Generate an SVG path `d` attribute from route points.
    /// </summary>
    string GeneratePath(IReadOnlyList<DvPoint> route);
}

// Built-in implementations
public class DvStraightRouter : IDvRouter { ... }
public class DvOrthogonalRouter : IDvRouter { ... }
public class DvBezierPathGenerator : IDvPathGenerator { ... }
public class DvStraightPathGenerator : IDvPathGenerator { ... }
```

### Layout Algorithms

```csharp
public interface IDvLayout
{
    void Apply(DvDiagram diagram, DvLayoutOptions? options = null);
}

public record DvLayoutOptions
{
    public double NodeSpacingX { get; init; } = 80;
    public double NodeSpacingY { get; init; } = 60;
    public DvPoint Origin { get; init; } = DvPoint.Zero;
    public DvLayoutDirection Direction { get; init; } = DvLayoutDirection.TopToBottom;
}

public enum DvLayoutDirection { TopToBottom, LeftToRight, BottomToTop, RightToLeft }

// Built-in layouts
public class DvTreeLayout : IDvLayout { ... }       // Hierarchical tree
public class DvGridLayout : IDvLayout { ... }       // Grid arrangement
public class DvForceDirectedLayout : IDvLayout { ... }  // Force-directed graph
```

---

## 4. Component Layer — Blazor SVG/HTML Rendering

### DvCanvas — The Root Blazor Component

The main hosting component. Renders an SVG container with pan/zoom transform, then renders all diagram layers.

```razor
@* DvCanvas.razor *@
@namespace FoundryWorldsAndDrawings.Diagram.Components

<div class="dv-canvas-container" 
     @ref="_container"
     style="width:100%; height:100%; overflow:hidden; position:relative;"
     @onpointerdown="OnPointerDown"
     @onpointermove="OnPointerMove" 
     @onpointerup="OnPointerUp"
     @onwheel="OnWheel"
     @onkeydown="OnKeyDown"
     tabindex="0">

    @if (Diagram != null)
    {
        <svg class="dv-canvas-svg"
             width="100%" height="100%"
             xmlns="http://www.w3.org/2000/svg">

            @* Background grid *@
            @if (ShowGrid)
            {
                <DvGridView Viewport="Diagram.Viewport" GridSize="GridSize" />
            }

            @* Pan/Zoom transform group *@
            <g transform="@Diagram.Viewport.GetTransformString()">

                @* Groups layer (rendered first = behind) *@
                @foreach (var group in Diagram.Groups.GetVisible())
                {
                    <DvGroupView Group="@group" Diagram="@Diagram" />
                }

                @* Links layer *@
                @foreach (var link in Diagram.Links.GetVisible())
                {
                    <DvLinkView Link="@link" Diagram="@Diagram" />
                }

                @* Nodes layer (rendered last = on top) *@
                @foreach (var node in Diagram.Nodes.GetVisible())
                {
                    <DvNodeView Node="@node" Diagram="@Diagram" />
                }
            </g>

            @* Selection rectangle (not transformed) *@
            @if (_selectionRect != null)
            {
                <rect class="dv-selection-rect" 
                      x="@_selectionRect.Value.X" y="@_selectionRect.Value.Y"
                      width="@_selectionRect.Value.Width" height="@_selectionRect.Value.Height"
                      fill="rgba(0,120,215,0.1)" stroke="#0078D7" stroke-dasharray="4" />
            }
        </svg>
    }
</div>
```

#### DvCanvas.razor.cs — Code-Behind

```csharp
public partial class DvCanvas : ComponentBase, IDisposable
{
    [Parameter] public DvDiagram? Diagram { get; set; }
    [Parameter] public bool ShowGrid { get; set; } = true;
    [Parameter] public double GridSize { get; set; } = 20;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private ElementReference _container;
    private DvRect? _selectionRect;
    private DotNetObjectReference<DvCanvas>? _dotNetRef;

    protected override void OnInitialized()
    {
        if (Diagram != null)
            Diagram.Changed += OnDiagramChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            // Initialize container size tracking via JS
            await JSRuntime.InvokeVoidAsync("dvInitCanvas", _container, _dotNetRef);
        }
    }

    private void OnDiagramChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    // Pointer event handlers delegate to diagram behaviors
    private void OnPointerDown(PointerEventArgs e) { ... }
    private void OnPointerMove(PointerEventArgs e) { ... }
    private void OnPointerUp(PointerEventArgs e) { ... }
    private void OnWheel(WheelEventArgs e) { ... }
    private void OnKeyDown(KeyboardEventArgs e) { ... }

    [JSInvokable]
    public void OnContainerResized(double width, double height)
    {
        if (Diagram != null)
        {
            Diagram.Viewport.ContainerSize = new DvSize(width, height);
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        if (Diagram != null)
            Diagram.Changed -= OnDiagramChanged;
        _dotNetRef?.Dispose();
    }
}
```

### DvNodeView — Node Renderer

Renders a node as an SVG `<foreignObject>` wrapping a Blazor component, or as a pure SVG `<g>` group with shapes.

```csharp
// DvNodeView.cs — Programmatic renderer (no .razor file)
namespace FoundryWorldsAndDrawings.Diagram.Components;

public class DvNodeView : ComponentBase, IDisposable
{
    [Parameter] public DvNode Node { get; set; } = default!;
    [Parameter] public DvDiagram Diagram { get; set; } = default!;

    private bool _shouldRender = true;

    protected override void OnInitialized()
    {
        Node.Changed += OnNodeChanged;
        Node.Moved += OnNodeChanged;
    }

    protected override bool ShouldRender() => _shouldRender;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!Node.Visible) return;

        var seq = 0;
        var widgetType = Diagram.GetWidgetType(Node.GetType());

        // SVG group positioned at node location
        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", $"dv-node{(Node.IsSelected ? " dv-selected" : "")}");
        builder.AddAttribute(seq++, "transform", $"translate({Node.Position.X},{Node.Position.Y})");

        // Pointer events
        builder.AddAttribute(seq++, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, e => OnPointerDown(e)));
        builder.AddAttribute(seq++, "onpointerup", EventCallback.Factory.Create<PointerEventArgs>(this, e => OnPointerUp(e)));

        if (widgetType != null)
        {
            // Render custom widget inside <foreignObject>
            builder.OpenElement(seq++, "foreignObject");
            builder.AddAttribute(seq++, "width", Node.Size.Width);
            builder.AddAttribute(seq++, "height", Node.Size.Height);

            builder.OpenComponent(seq++, widgetType);
            builder.AddAttribute(seq++, "Node", Node);
            builder.CloseComponent();

            builder.CloseElement(); // foreignObject
        }
        else
        {
            // Default rectangle rendering
            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "width", Node.Size.Width);
            builder.AddAttribute(seq++, "height", Node.Size.Height);
            builder.AddAttribute(seq++, "rx", 6);
            builder.AddAttribute(seq++, "class", "dv-node-default");
            builder.CloseElement();

            // Title text
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "x", Node.Size.Width / 2);
            builder.AddAttribute(seq++, "y", Node.Size.Height / 2);
            builder.AddAttribute(seq++, "text-anchor", "middle");
            builder.AddAttribute(seq++, "dominant-baseline", "central");
            builder.AddAttribute(seq++, "class", "dv-node-title");
            builder.AddContent(seq++, Node.Title);
            builder.CloseElement();
        }

        // Render ports
        foreach (var port in Node.Ports)
        {
            builder.OpenComponent<DvPortView>(seq++);
            builder.AddAttribute(seq++, "Port", port);
            builder.AddAttribute(seq++, "Diagram", Diagram);
            builder.CloseComponent();
        }

        builder.CloseElement(); // g
        _shouldRender = false;
    }

    private void OnNodeChanged(DvNode _) { _shouldRender = true; InvokeAsync(StateHasChanged); }
    private void OnPointerDown(PointerEventArgs e) { /* delegate to behaviors */ }
    private void OnPointerUp(PointerEventArgs e) { /* delegate to behaviors */ }

    public void Dispose() { Node.Changed -= OnNodeChanged; Node.Moved -= OnNodeChanged; }
}
```

### DvLinkView — Link Renderer

```csharp
public class DvLinkView : ComponentBase, IDisposable
{
    [Parameter] public DvLink Link { get; set; } = default!;
    [Parameter] public DvDiagram Diagram { get; set; } = default!;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", $"dv-link{(Link.IsSelected ? " dv-selected" : "")}");

        // Invisible wider path for easier click targeting
        builder.OpenElement(seq++, "path");
        builder.AddAttribute(seq++, "d", Link.SvgPath);
        builder.AddAttribute(seq++, "class", "dv-link-hitarea");
        builder.AddAttribute(seq++, "stroke", "transparent");
        builder.AddAttribute(seq++, "stroke-width", 12);
        builder.AddAttribute(seq++, "fill", "none");
        builder.AddAttribute(seq++, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, OnPointerDown));
        builder.CloseElement();

        // Visible path
        builder.OpenElement(seq++, "path");
        builder.AddAttribute(seq++, "d", Link.SvgPath);
        builder.AddAttribute(seq++, "class", "dv-link-path");
        builder.AddAttribute(seq++, "stroke", Link.Color);
        builder.AddAttribute(seq++, "stroke-width", 2);
        builder.AddAttribute(seq++, "fill", "none");
        builder.AddAttribute(seq++, "marker-end", "url(#dv-arrowhead)");
        builder.CloseElement();

        // Vertices (draggable waypoints)
        foreach (var vertex in Link.Vertices)
        {
            builder.OpenElement(seq++, "circle");
            builder.AddAttribute(seq++, "cx", vertex.X);
            builder.AddAttribute(seq++, "cy", vertex.Y);
            builder.AddAttribute(seq++, "r", 5);
            builder.AddAttribute(seq++, "class", "dv-link-vertex");
            builder.CloseElement();
        }

        // Labels
        foreach (var label in Link.Labels)
        {
            builder.OpenComponent<DvLabelView>(seq++);
            builder.AddAttribute(seq++, "Label", label);
            builder.AddAttribute(seq++, "Link", Link);
            builder.CloseComponent();
        }

        builder.CloseElement(); // g
    }
}
```

### DvPortView — Port Renderer

```csharp
public class DvPortView : ComponentBase, IDisposable
{
    [Parameter] public DvPort Port { get; set; } = default!;
    [Parameter] public DvDiagram Diagram { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        var pos = ComputePortOffset();

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", "dv-port");
        builder.AddAttribute(seq++, "transform", $"translate({pos.X},{pos.Y})");

        // Port circle/square
        builder.OpenElement(seq++, "circle");
        builder.AddAttribute(seq++, "r", Port.Size.Width / 2);
        builder.AddAttribute(seq++, "fill", string.IsNullOrEmpty(Port.Color) ? "#6c757d" : Port.Color);
        builder.AddAttribute(seq++, "class", "dv-port-shape");
        builder.AddAttribute(seq++, "onpointerdown", EventCallback.Factory.Create<PointerEventArgs>(this, OnPortPointerDown));
        builder.CloseElement();

        if (ChildContent != null)
        {
            builder.AddContent(seq++, ChildContent);
        }

        builder.CloseElement(); // g
    }

    private DvPoint ComputePortOffset()
    {
        var node = Port.Parent;
        return Port.Alignment switch
        {
            DvPortAlignment.Top => new(node.Size.Width / 2, 0),
            DvPortAlignment.Bottom => new(node.Size.Width / 2, node.Size.Height),
            DvPortAlignment.Left => new(0, node.Size.Height / 2),
            DvPortAlignment.Right => new(node.Size.Width, node.Size.Height / 2),
            DvPortAlignment.TopLeft => new(0, 0),
            DvPortAlignment.TopRight => new(node.Size.Width, 0),
            DvPortAlignment.BottomLeft => new(0, node.Size.Height),
            DvPortAlignment.BottomRight => new(node.Size.Width, node.Size.Height),
            _ => Port.Offset
        };
    }

    private void OnPortPointerDown(PointerEventArgs e)
    {
        // Start link creation behavior
        Diagram.GetBehavior<DvLinkCreationBehavior>()?.StartLink(Port, e);
    }
}
```

### DvGroupView — Group Renderer

```csharp
public class DvGroupView : ComponentBase, IDisposable
{
    [Parameter] public DvGroup Group { get; set; } = default!;
    [Parameter] public DvDiagram Diagram { get; set; } = default!;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;
        var bounds = Group.AutoSize ? Group.ComputeBounds() : 
                     new DvRect(Group.Position.X, Group.Position.Y, Group.Size.Width, Group.Size.Height);

        builder.OpenElement(seq++, "g");
        builder.AddAttribute(seq++, "class", $"dv-group{(Group.IsSelected ? " dv-selected" : "")}");
        builder.AddAttribute(seq++, "transform", $"translate({bounds.X},{bounds.Y})");

        var widgetType = Diagram.GetWidgetType(Group.GetType());

        if (widgetType != null)
        {
            builder.OpenElement(seq++, "foreignObject");
            builder.AddAttribute(seq++, "width", bounds.Width);
            builder.AddAttribute(seq++, "height", bounds.Height);
            builder.OpenComponent(seq++, widgetType);
            builder.AddAttribute(seq++, "Group", Group);
            builder.CloseComponent();
            builder.CloseElement();
        }
        else
        {
            // Default dashed-border group
            builder.OpenElement(seq++, "rect");
            builder.AddAttribute(seq++, "width", bounds.Width);
            builder.AddAttribute(seq++, "height", bounds.Height);
            builder.AddAttribute(seq++, "rx", 8);
            builder.AddAttribute(seq++, "class", "dv-group-default");
            builder.CloseElement();

            // Group title
            builder.OpenElement(seq++, "text");
            builder.AddAttribute(seq++, "x", 10);
            builder.AddAttribute(seq++, "y", 20);
            builder.AddAttribute(seq++, "class", "dv-group-title");
            builder.AddContent(seq++, Group.Title);
            builder.CloseElement();
        }

        // Ports
        foreach (var port in Group.Ports)
        {
            builder.OpenComponent<DvPortView>(seq++);
            builder.AddAttribute(seq++, "Port", port);
            builder.AddAttribute(seq++, "Diagram", Diagram);
            builder.CloseComponent();
        }

        builder.CloseElement(); // g
    }
}
```

### DvGridView — Background Grid

```csharp
public class DvGridView : ComponentBase
{
    [Parameter] public DvViewport Viewport { get; set; } = default!;
    [Parameter] public double GridSize { get; set; } = 20;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var seq = 0;

        // SVG <defs> with grid pattern
        builder.OpenElement(seq++, "defs");

        builder.OpenElement(seq++, "pattern");
        builder.AddAttribute(seq++, "id", "dv-grid-pattern");
        builder.AddAttribute(seq++, "width", GridSize * Viewport.Zoom);
        builder.AddAttribute(seq++, "height", GridSize * Viewport.Zoom);
        builder.AddAttribute(seq++, "patternUnits", "userSpaceOnUse");
        builder.AddAttribute(seq++, "x", Viewport.Pan.X);
        builder.AddAttribute(seq++, "y", Viewport.Pan.Y);

        builder.OpenElement(seq++, "circle");
        builder.AddAttribute(seq++, "cx", 1);
        builder.AddAttribute(seq++, "cy", 1);
        builder.AddAttribute(seq++, "r", 1);
        builder.AddAttribute(seq++, "fill", "rgba(0,0,0,0.15)");
        builder.CloseElement();

        builder.CloseElement(); // pattern
        builder.CloseElement(); // defs

        // Full-size rect filled with grid pattern
        builder.OpenElement(seq++, "rect");
        builder.AddAttribute(seq++, "width", "100%");
        builder.AddAttribute(seq++, "height", "100%");
        builder.AddAttribute(seq++, "fill", "url(#dv-grid-pattern)");
        builder.CloseElement();
    }
}
```

---

## 5. Behavior System — Pluggable Interactions

Behaviors are the primary mechanism for handling user interaction. They are pluggable, replaceable, and composable.

```csharp
public abstract class DvBehavior : IDisposable
{
    protected DvDiagram Diagram { get; }

    protected DvBehavior(DvDiagram diagram) { Diagram = diagram; }

    public virtual void OnPointerDown(DvPointerEventArgs e) { }
    public virtual void OnPointerMove(DvPointerEventArgs e) { }
    public virtual void OnPointerUp(DvPointerEventArgs e) { }
    public virtual void OnWheel(DvWheelEventArgs e) { }
    public virtual void OnKeyDown(DvKeyboardEventArgs e) { }

    public virtual void Dispose() { }
}

// Wraps Blazor event args with diagram context
public record DvPointerEventArgs(
    PointerEventArgs Original,
    DvPoint ScreenPosition,
    DvPoint WorldPosition,       // After inverse viewport transform
    DvNode? TargetNode,
    DvPort? TargetPort,
    DvLink? TargetLink
);
```

### Built-In Behaviors

```csharp
// Drag nodes/groups on pointer down + move
public class DvDragBehavior : DvBehavior
{
    private DvNode? _dragging;
    private DvPoint _dragStart;
    private Dictionary<DvNode, DvPoint> _initialPositions = new();
    public bool SnapToGrid { get; set; } = false;
    public double GridSize { get; set; } = 20;
    // ...
}

// Pan canvas on middle-click drag or Ctrl+drag
public class DvPanBehavior : DvBehavior
{
    // ...
}

// Zoom on mouse wheel (centered on cursor)
public class DvZoomBehavior : DvBehavior
{
    public double ZoomStep { get; set; } = 0.1;
    // ...
}

// Rubber-band selection rectangle
public class DvSelectionBehavior : DvBehavior
{
    // ...
}

// Draw a new link by dragging from port to port
public class DvLinkCreationBehavior : DvBehavior
{
    public void StartLink(DvPort source, PointerEventArgs e) { ... }
    // Shows a temporary "dangling" link following the cursor
    // On drop over a valid port: creates the real link
    // On drop over empty space: cancel
}

// Delete selected elements, undo/redo triggers
public class DvKeyboardBehavior : DvBehavior
{
    // Delete/Backspace = delete selected
    // Ctrl+A = select all
    // Escape = deselect all
}
```

---

## 6. Options and Configuration

```csharp
public class DvDiagramOptions
{
    // Viewport
    public double MinZoom { get; set; } = 0.1;
    public double MaxZoom { get; set; } = 5.0;
    public double DefaultZoom { get; set; } = 1.0;

    // Grid
    public bool ShowGrid { get; set; } = true;
    public double GridSize { get; set; } = 20;
    public bool SnapToGrid { get; set; } = false;

    // Selection
    public bool AllowMultiSelect { get; set; } = true;
    public bool AllowRubberBandSelect { get; set; } = true;

    // Links
    public Type DefaultRouter { get; set; } = typeof(DvOrthogonalRouter);
    public Type DefaultPathGenerator { get; set; } = typeof(DvStraightPathGenerator);
    public string DefaultLinkColor { get; set; } = "#333333";
    public bool AllowFreeLinks { get; set; } = false;   // Links must connect ports

    // Nodes
    public bool AllowNodeDrag { get; set; } = true;
    public bool AllowNodeResize { get; set; } = false;

    // Groups
    public double DefaultGroupPadding { get; set; } = 45;
    public bool DefaultAutoSize { get; set; } = true;
}
```

---

## 7. CSS Styling

```css
/* dv-diagram.css */

.dv-canvas-container {
    background: #fafafa;
    cursor: default;
    user-select: none;
}

.dv-canvas-container:focus {
    outline: 2px solid #0078D7;
}

/* Nodes */
.dv-node { cursor: move; }
.dv-node.dv-selected > rect,
.dv-node.dv-selected > foreignObject { filter: drop-shadow(0 0 4px #0078D7); }
.dv-node-default {
    fill: white;
    stroke: #333;
    stroke-width: 1.5;
}
.dv-node-title {
    font-family: 'Segoe UI', sans-serif;
    font-size: 13px;
    fill: #333;
    pointer-events: none;
}

/* Links */
.dv-link-path {
    transition: stroke 0.15s;
}
.dv-link.dv-selected .dv-link-path {
    stroke: #0078D7 !important;
    stroke-width: 3;
}
.dv-link-hitarea { cursor: pointer; }
.dv-link-vertex {
    fill: white;
    stroke: #333;
    stroke-width: 1.5;
    cursor: grab;
}

/* Ports */
.dv-port-shape {
    cursor: crosshair;
    transition: r 0.15s, fill 0.15s;
}
.dv-port-shape:hover {
    r: 8;
    fill: #0078D7;
}

/* Groups */
.dv-group-default {
    fill: rgba(0,120,215,0.05);
    stroke: #0078D7;
    stroke-width: 1;
    stroke-dasharray: 6 3;
}
.dv-group-title {
    font-family: 'Segoe UI', sans-serif;
    font-size: 12px;
    fill: #666;
    font-weight: 600;
}
.dv-group.dv-selected .dv-group-default {
    stroke-width: 2;
    fill: rgba(0,120,215,0.1);
}

/* Selection rectangle */
.dv-selection-rect {
    pointer-events: none;
}
```

---

## 8. Domain Model Bridge — Integration with FoundryMentorModeler

The DiagramViewer library provides base classes. FoundryMentorModeler subclasses them to bridge to `KnComponent`:

```csharp
// In FoundryMentorModeler — NOT in the library itself
namespace FoundryMentorModeler.Diagram;

public class MentorDvNode : DvNode, IComponentViewer
{
    protected KnComponent Component { get; set; }

    public MentorDvNode(KnComponent source, DvPoint position) 
        : base(source.GetKnowId(), position)
    {
        SetComponent(source);
        Title = source.Title ?? source.GetKnowId();
    }

    // IComponentViewer implementation
    public KnComponent GetComponent() => Component;
    public void SetComponent(KnComponent component) { Component = component; }
    public bool SetVisible(bool visible, bool deep) { Visible = visible; return visible; }
    public string GetTitle() => Title;
    public int GetLocationId() => 1;
    public int SetLocationId(int id) => id;
    public bool HasHyperlinks() => false;
    public List<Hyperlink> GetHyperlinks() => new();
    public bool HasSubcomponents() => false;
}
```

This separation means the DiagramViewer library has **zero dependency** on FoundryMentorModeler's domain model. The bridge is always in the consuming project.

---

## 9. JavaScript Interop (Minimal)

The DiagramViewer minimizes JS interop — only what browser APIs require:

```javascript
// wwwroot/js/dv-interop.js

window.dvInitCanvas = (container, dotNetRef) => {
    // ResizeObserver for container size tracking
    const observer = new ResizeObserver(entries => {
        for (const entry of entries) {
            const { width, height } = entry.contentRect;
            dotNetRef.invokeMethodAsync('OnContainerResized', width, height);
        }
    });
    observer.observe(container);

    // Store for cleanup
    container.__dvObserver = observer;
    container.__dvDotNetRef = dotNetRef;
};

window.dvDisposeCanvas = (container) => {
    container.__dvObserver?.disconnect();
    container.__dvDotNetRef?.dispose();
};

// Optional: get bounding rect for port position calculation
window.dvGetBoundingRect = (element) => {
    const rect = element.getBoundingClientRect();
    return { x: rect.x, y: rect.y, width: rect.width, height: rect.height };
};
```

---

## 10. File Structure

```
FoundryWorldsAndDrawings/
  Diagram/
    Components/
      DvCanvas.razor
      DvCanvas.razor.cs
      DvNodeView.cs              (BuildRenderTree — no .razor)
      DvLinkView.cs              (BuildRenderTree — no .razor)
      DvPortView.cs              (BuildRenderTree — no .razor)
      DvGroupView.cs             (BuildRenderTree — no .razor)
      DvLabelView.cs             (BuildRenderTree — no .razor)
      DvGridView.cs              (BuildRenderTree — no .razor)
    Models/
      DvDiagram.cs
      DvNode.cs
      DvLink.cs
      DvPort.cs
      DvGroup.cs
      DvLinkLabel.cs
      DvLayer.cs
      DvViewport.cs
      DvSelectionState.cs
      DvDiagramOptions.cs
    Geometry/
      DvPoint.cs
      DvSize.cs
      DvRect.cs
      DvMatrix.cs
      DvPathBuilder.cs
    Anchors/
      DvAnchor.cs
      DvPortAnchor.cs
      DvNodeAnchor.cs
      DvPositionAnchor.cs
    Behaviors/
      DvBehavior.cs
      DvDragBehavior.cs
      DvPanBehavior.cs
      DvZoomBehavior.cs
      DvSelectionBehavior.cs
      DvLinkCreationBehavior.cs
      DvKeyboardBehavior.cs
    Routers/
      IDvRouter.cs
      DvStraightRouter.cs
      DvOrthogonalRouter.cs
    PathGenerators/
      IDvPathGenerator.cs
      DvStraightPathGenerator.cs
      DvBezierPathGenerator.cs
    Layouts/
      IDvLayout.cs
      DvLayoutOptions.cs
      DvTreeLayout.cs
      DvGridLayout.cs
      DvForceDirectedLayout.cs
    Styles/
      dv-diagram.css
  wwwroot/
    js/
      dv-interop.js
```

---

## 11. Enumerations

```csharp
public enum DvPortAlignment
{
    Top, Bottom, Left, Right,
    TopLeft, TopRight, BottomLeft, BottomRight,
    Custom    // Uses Offset coordinates
}

public enum DvLinkType
{
    Straight, Orthogonal, Bezier, Step
}
```

---

## 12. Usage Example — How a Page Consumes the DiagramViewer

```razor
@page "/diagram-demo"
@using FoundryWorldsAndDrawings.Diagram
@using FoundryWorldsAndDrawings.Diagram.Components

<DvCanvas Diagram="@_diagram" ShowGrid="true" GridSize="20" />

@code {
    private DvDiagram _diagram = new("Demo", new DvDiagramOptions
    {
        SnapToGrid = true,
        DefaultRouter = typeof(DvOrthogonalRouter)
    });

    protected override void OnInitialized()
    {
        // Register custom widgets
        _diagram.RegisterWidget<MyCustomNode, MyCustomNodeWidget>();

        // Register behaviors
        _diagram.RegisterBehavior(new DvDragBehavior(_diagram) { SnapToGrid = true });
        _diagram.RegisterBehavior(new DvPanBehavior(_diagram));
        _diagram.RegisterBehavior(new DvZoomBehavior(_diagram));
        _diagram.RegisterBehavior(new DvSelectionBehavior(_diagram));
        _diagram.RegisterBehavior(new DvLinkCreationBehavior(_diagram));
        _diagram.RegisterBehavior(new DvKeyboardBehavior(_diagram));

        // Build diagram
        var nodeA = _diagram.CreateNode<DvNode>(new DvPoint(100, 100));
        nodeA.Title = "System A";
        nodeA.Size = new DvSize(160, 80);
        var portAOut = nodeA.AddPort<DvPort>(DvPortAlignment.Right);

        var nodeB = _diagram.CreateNode<DvNode>(new DvPoint(400, 200));
        nodeB.Title = "System B";
        nodeB.Size = new DvSize(160, 80);
        var portBIn = nodeB.AddPort<DvPort>(DvPortAlignment.Left);

        var link = _diagram.ConnectPorts<DvLink>(portAOut, portBIn);
        link.Color = "#2196F3";
        link.AddLabel("Data Flow", 0.5);

        // Apply layout
        var layout = new DvTreeLayout();
        layout.Apply(_diagram);
    }
}
```

---

## 13. Success Criteria / Verification Checklist

| # | Criterion | How to Verify |
|---|-----------|---------------|
| 1 | SVG renders in the browser | Inspect DOM — see `<svg>` with `<g>`, `<rect>`, `<path>` elements |
| 2 | Nodes are draggable | Pointer down on node + move → node follows cursor |
| 3 | Links connect ports | Drag from port → link path follows cursor → drop on port → link created |
| 4 | Pan/Zoom works | Middle-click drag = pan, mouse wheel = zoom, transforms update |
| 5 | Selection works | Click = select single, Ctrl+click = add to selection, rubber band = multi-select |
| 6 | Custom widgets render | Register a `.razor` component → it appears inside `<foreignObject>` |
| 7 | Groups contain nodes | Create group, add children → dashed boundary encloses them |
| 8 | Labels on links | `AddLabel()` → text appears at specified fraction along path |
| 9 | Layout algorithms work | `DvTreeLayout.Apply()` → nodes arranged hierarchically |
| 10 | No JS interop for rendering | All SVG emitted from `BuildRenderTree` — JS only for ResizeObserver |
| 11 | Zero dependency on Z.Blazor.Diagrams | No `using Blazor.Diagrams` anywhere in the library |
| 12 | Integrates with ITreeNode | All models implement `ITreeNode` → visible in tree views |
| 13 | Integrates with IComponentViewer | Subclassed models in FoundryMentorModeler bridge to KnComponent |
| 14 | Orthogonal routing | Links with `DvOrthogonalRouter` produce right-angle paths |
| 15 | Grid snapping | With `SnapToGrid = true`, dragged nodes snap to grid intersections |

---

## 14. Visual Expectations

### Default Diagram View

```
┌──────────────────────────────────────────────────────────────┐
│ · · · · · · · · · · · · · · · · · · · · · · · · · · · · · · │
│ · · · · · · · · · · · · · · · · · · · · · · · · · · · · · · │
│ · · ┌─────────────┐ · · · · · · · · ┌─────────────┐ · · · · │
│ · · │  System A    │ · · · · · · · · │  System B    │ · · · · │
│ · · │             ●──── Data Flow ───●             │ · · · · │
│ · · │              │ · · · · · · · · │              │ · · · · │
│ · · └─────────────┘ · · · · · · · · └─────────────┘ · · · · │
│ · · · · · · · · · · · · · · · · · · · · · · · · · · · · · · │
│ · · · ┌ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┐ · · · │
│ · · · │ Group: Power Subsystem                     │ · · · · │
│ · · · │  ┌─────────┐    ┌─────────┐               │ · · · · │
│ · · · │  │ Motor    │───▶│ Driver  │               │ · · · · │
│ · · · │  └─────────┘    └─────────┘               │ · · · · │
│ · · · └ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ┘ · · · │
│ · · · · · · · · · · · · · · · · · · · · · · · · · · · · · · │
└──────────────────────────────────────────────────────────────┘
  Dots = grid pattern    ● = ports    ─── = links    ┌ ─ ┐ = group
```

### Selected State
- Selected nodes get a blue `drop-shadow` filter
- Selected links turn blue (#0078D7) and thicken to 3px
- Ports enlarge on hover (CSS `r` transition from 6 to 8)

### Custom Widget Inside Node
When a `.razor` widget is registered, the node renders as a `<foreignObject>` containing that Blazor component at full fidelity — allowing buttons, inputs, badges, gradient backgrounds, etc.

---

## 15. Known Gotchas

1. **`<foreignObject>` quirks** — SVG `foreignObject` has known browser differences. Firefox and Chrome handle overflow differently. Always set explicit `width/height` on the `foreignObject` element matching `Node.Size`.

2. **SVG event handling** — SVG elements don't bubble pointer events the same way HTML does. The `DvCanvas` container (HTML `<div>`) captures all pointer events and translates coordinates to world space before dispatching to behaviors.

3. **Sequence numbers in `BuildRenderTree`** — Must be monotonically increasing within each `BuildRenderTree` call but do NOT need to be globally unique. Use a local `seq` counter incrementing from 0. Do NOT use dynamically computed sequence numbers based on data — this breaks Blazor's diff algorithm.

4. **`StateHasChanged` must be on the UI thread** — All event subscriptions from model objects must call `InvokeAsync(StateHasChanged)`, not `StateHasChanged()` directly, since model changes may occur off-thread.

5. **Port position after node drag** — Links must recalculate their paths when a connected node moves. The `DvDragBehavior` must trigger path recalculation on all connected links when a drag completes (or during drag for live updating).

6. **Zoom center** — Zooming should center on the mouse cursor position, not on `(0,0)`. The `DvZoomBehavior` must compute the focal point and adjust pan to compensate.

---

## 16. Troubleshooting Guide

| Symptom | Likely Cause | Fix |
|---------|-------------|-----|
| Nothing renders | `Diagram` parameter is null on `DvCanvas` | Ensure diagram is created in `OnInitialized`, not in a field initializer |
| Nodes don't move | `DvDragBehavior` not registered | Call `diagram.RegisterBehavior(new DvDragBehavior(diagram))` |
| Links don't follow nodes | Path not recalculated on move | Ensure `DvDragBehavior.OnPointerMove` triggers `link.Refresh()` |
| Custom widget not appearing | Widget type not registered | Call `diagram.RegisterWidget<TModel, TComponent>()` before creating nodes |
| Ports not clickable | SVG `pointer-events` blocked by parent | Check CSS `pointer-events` is not `none` on port elements |
| Pan/zoom jerky | Multiple `StateHasChanged` calls per frame | Use `BeginBatch()` / `EndBatch()` to coalesce changes |
| `foreignObject` content clipped | Size mismatch | Ensure `foreignObject` width/height match `Node.Size` |
| Grid doesn't scroll with pan | Pattern position not bound to viewport | Bind `pattern` `x`/`y` attributes to `Viewport.Pan` |

---

## 17. Implementation Steps (Priority Order)

### Phase 1: Core Models + Geometry (Foundation)
1. Create `Diagram/Geometry/` — `DvPoint.cs`, `DvSize.cs`, `DvRect.cs`, `DvMatrix.cs`
2. Create `Diagram/Models/DvDiagramOptions.cs`
3. Create `Diagram/Models/DvViewport.cs`
4. Create `Diagram/Models/DvSelectionState.cs`
5. Create `Diagram/Models/DvLayer.cs`
6. Create `Diagram/Models/DvNode.cs`
7. Create `Diagram/Models/DvPort.cs` + `DvPortAlignment` enum
8. Create `Diagram/Models/DvLink.cs` + `DvLinkLabel.cs`
9. Create `Diagram/Models/DvGroup.cs`
10. Create `Diagram/Anchors/` — `DvAnchor.cs`, `DvPortAnchor.cs`, `DvNodeAnchor.cs`, `DvPositionAnchor.cs`
11. Create `Diagram/Models/DvDiagram.cs` (orchestrator)

### Phase 2: Routing + Path Generation
12. Create `Diagram/Routers/IDvRouter.cs`
13. Create `Diagram/Routers/DvStraightRouter.cs`
14. Create `Diagram/Routers/DvOrthogonalRouter.cs`
15. Create `Diagram/PathGenerators/IDvPathGenerator.cs`
16. Create `Diagram/PathGenerators/DvStraightPathGenerator.cs`
17. Create `Diagram/PathGenerators/DvBezierPathGenerator.cs`

### Phase 3: Blazor Components (Rendering)
18. Create `Diagram/Styles/dv-diagram.css`
19. Create `wwwroot/js/dv-interop.js`
20. Create `Diagram/Components/DvGridView.cs`
21. Create `Diagram/Components/DvPortView.cs`
22. Create `Diagram/Components/DvNodeView.cs`
23. Create `Diagram/Components/DvLinkView.cs`
24. Create `Diagram/Components/DvLabelView.cs`
25. Create `Diagram/Components/DvGroupView.cs`
26. Create `Diagram/Components/DvCanvas.razor` + `DvCanvas.razor.cs`

### Phase 4: Behaviors (Interaction)
27. Create `Diagram/Behaviors/DvBehavior.cs` (abstract base)
28. Create `Diagram/Behaviors/DvDragBehavior.cs`
29. Create `Diagram/Behaviors/DvPanBehavior.cs`
30. Create `Diagram/Behaviors/DvZoomBehavior.cs`
31. Create `Diagram/Behaviors/DvSelectionBehavior.cs`
32. Create `Diagram/Behaviors/DvLinkCreationBehavior.cs`
33. Create `Diagram/Behaviors/DvKeyboardBehavior.cs`

### Phase 5: Layout Algorithms
34. Create `Diagram/Layouts/IDvLayout.cs` + `DvLayoutOptions.cs`
35. Create `Diagram/Layouts/DvTreeLayout.cs`
36. Create `Diagram/Layouts/DvGridLayout.cs`

### Phase 6: Integration + Demo
37. Create demo page in Three2025 consuming the library
38. Wire up `ITreeNode` integration for tree view
39. Create sample custom widget (`.razor` component)

---

## 18. Future Extensions (Out of Scope for V1)

- **Minimap** — A small overview panel showing the full diagram with a viewport rectangle
- **Export** — SVG export (trivial since the DOM is already SVG), PNG via `<canvas>` screenshot
- **Undo/Redo** — Command pattern tracking all model mutations
- **Clipboard** — Copy/Paste of selected nodes and their connections
- **Snap guides** — Alignment guides when dragging near other nodes
- **Force-directed layout** — Physics simulation for automatic graph layout
- **Swimlanes** — Horizontal/vertical partition regions
- **Theming** — CSS variable-based theme switching (dark mode, etc.)
- **Touch support** — Pinch-to-zoom, two-finger pan
- **Connection validation** — Type-safe port connections (e.g., output→input only)
