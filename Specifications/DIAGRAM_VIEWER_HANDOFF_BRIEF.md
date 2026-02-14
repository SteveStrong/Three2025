# DiagramViewer — Atlas Handoff Brief

**Date:** 2025-02-13
**Feature:** Native Blazor Diagram Rendering Library (DiagramViewer)

---

## 1. Verification Inventory

### Files Atlas Actually Opened and Read

| File | Status | What Was Extracted |
|------|--------|-------------------|
| `FoundryMentorModeler/Diagram/MentorDiagram.cs` | ✅ READ | Full class: factory methods, behavior registration, generic patterns |
| `FoundryMentorModeler/Diagram/DiagramNode.cs` | ✅ READ | Full class: constructor, ports, IComponentViewer, ITreeNode |
| `FoundryMentorModeler/Diagram/DiagramLink.cs` | ✅ READ | Full class: port/node constructors, reconnect, labels |
| `FoundryMentorModeler/Diagram/DiagramGroup.cs` | ✅ READ | Full class: padding, autosize, children |
| `FoundryMentorModeler/Diagram/DiagramPort.cs` | ✅ READ | Full class: size 30×30, CanAttachTo, alignment |
| `FoundryMentorModeler/Diagram/DiagramLinkLabel.cs` | ✅ READ | Full class wrapping KnComponent |
| `FoundryMentorModeler/Diagram/DiagramDragMovablesBehavior.cs` | ✅ READ | Drag behavior with grid snapping |
| `FoundryMentorModeler/Diagram/DiagramLayer.cs` | ✅ READ | Custom layer management |
| `FoundryMentorModeler/Diagram/DiagramPortRenderer.cs` | ✅ READ | Port rendering with color |
| `FoundryMentorModeler/Diagram/KnEditor2DParameter.cs` | ✅ READ | Cache bridge between KnParameter and IComponentViewer |
| `FoundryMentorModeler/Diagram/Renderers/Node710Renderer.cs` | ✅ READ | BuildRenderTree: div/g, foreignObject, resize observer |
| `FoundryMentorModeler/Diagram/Renderers/Link710Renderer.cs` | ✅ READ | BuildRenderTree: g element, component resolution |
| `FoundryMentorModeler/Diagram/Renderers/Port710Renderer.cs` | ✅ READ | BuildRenderTree: port positioning, BoundingClientRect |
| `FoundryMentorModeler/Diagram/Renderers/Group710Renderer.cs` | ✅ READ | BuildRenderTree: div/g, transparent rect hit testing |
| `FoundryMentorModeler/Diagram/Renderers/LinkVertex710Renderer.cs` | ✅ READ | Vertex circle rendering, double-click removal |
| `FoundryMentorModeler/Diagram/Renderers/LinkLabel710Renderer.cs` | ✅ READ | foreignObject positioning along path |
| `FoundryMentorModeler/Diagram/WindowDiagram/UIDiagram.cs` | ✅ READ | Window management variant with pub/sub |
| `FoundryMentorModeler/Diagram/WindowDiagram/WindowNode.cs` | ✅ READ | WindowNode extends DiagramGroup with SetSize |
| `FoundryMentorModeler/Diagram/WindowDiagram/WindowWidgetBase.cs` | ✅ READ | ComponentBase with IWidgetView |
| `FoundryMentorModeler/Mentor/IRender.cs` | ✅ READ | IComponentViewer, IWidgetView, IModelRenderable, IRenderController |
| `FoundryMicroCore/Core/ITreeNode.cs` | ✅ READ | Full ITreeNode interface with default implementations |
| `FoundryWorldsAndDrawings/Canvas/BECanvas.razor` | ✅ READ | Canvas element component |
| `FoundryWorldsAndDrawings/Canvas/BECanvasComponent.cs` | ✅ READ | ComponentBase with ElementReference |
| `FoundryWorldsAndDrawings/Canvas/RenderingContext.cs` | ✅ READ | Batched JS interop base class |
| `FoundryWorldsAndDrawings/Canvas/Canvas2D/Canvas2DContext.cs` | ✅ READ | Full Canvas 2D API wrapper |
| `FoundryWorldsAndDrawings/Shapes2D/FoGlyph2D.cs` | ✅ READ | 1164-line shape base class |
| `FoundryWorldsAndDrawings/Shapes2D/FoPage2D.cs` | ✅ READ | IPage2D interface and page container |
| `FoundryWorldsAndDrawings/Solutions/FoWorkspace.cs` | ✅ READ | IDrawing/IArena dual mode |
| `FoundryWorldsAndDrawings.csproj` | ✅ READ | Dependencies: BlazorComponentBus, Radzen, SkiaSharp |
| `FoundryMentorModeler.csproj` | ✅ READ | Z.Blazor.Diagrams v3.0.3 dependency |
| `Three2025/Components/DiagramWidgets/*.razor.disabled` | ✅ READ | All 3 disabled widget examples |

### Files Atlas Did NOT Open

| File/Area | Why Not | Risk |
|-----------|---------|------|
| `FoundryMentorModeler/Mentor/RenderContext.cs` | Summarized from search, not line-by-line | LOW — well documented in search results |
| `FoundryWorldsAndDrawings/Interaction/` directory | Interactions for Canvas2D, not diagram | LOW — different paradigm |
| `FoundryWorldsAndDrawings/Shapes2D/FoShape2D.cs` | Canvas2D shape, not relevant to SVG approach | LOW |
| `Three2025/Components/Pages/DiagramViewer.razor.cs` | File exists but is `<Compile Remove>` in csproj | MEDIUM — shows prior consumption pattern |
| `Three2025/Components/Layout/` directory | Not examined | LOW — layout shell, not diagram rendering |
| `FoundryMentorModeler/Evaluator/` directory | Domain evaluator layer | LOW — not needed for diagram rendering |

---

## 2. Method/API Verification Table

| Method/API | Status | Source |
|---|---|---|
| `ITreeNode.GetUniqueId()` | ✅ VERIFIED | `FoundryMicroCore/Core/ITreeNode.cs` — read full interface |
| `ITreeNode.GetTreeViewNodeTitle()` | ✅ VERIFIED | Same file — confirmed all 7 methods + 4 default implementations |
| `IComponentViewer.GetComponent()` | ✅ VERIFIED | `FoundryMentorModeler/Mentor/IRender.cs` — read full interface |
| `IComponentViewer.SetVisible()` | ✅ VERIFIED | Same file |
| `StatusBitArray` usage | ✅ VERIFIED | All 5 diagram model classes — used identically in each |
| `BlazorDiagram.GetComponent()` | ✅ VERIFIED | `Node710Renderer.cs` — confirmed usage pattern `BlazorDiagram.GetComponent(Node)` |
| `NodeModel` base class | ✅ VERIFIED | `DiagramNode : NodeModel` — from Z.Blazor.Diagrams via csproj |
| `RenderTreeBuilder` pattern in renderers | ✅ VERIFIED | All 6 renderer files — read complete `BuildRenderTree` implementations |
| `ComponentBase` lifecycle (`OnInitialized`, `ShouldRender`) | ✅ VERIFIED | Standard Blazor — confirmed usage in all renderers |
| `DvPoint`, `DvSize`, `DvRect` structs | ⚠️ ASSUMED | New types — designed from Blazor.Diagrams `Point`/`Size` equivalents |
| `DvViewport.GetTransformString()` | ⚠️ ASSUMED | New — SVG transform syntax is standard, but untested |
| `DvBehavior` abstract class | ⚠️ ASSUMED | New design — pattern modeled after `DiagramDragMovablesBehavior` |
| `IDvRouter` / `IDvPathGenerator` | ⚠️ ASSUMED | New — interfaces modeled after Z.Blazor.Diagrams `IRouter`/`IPathGenerator` |
| `DvDiagram.RegisterWidget<TModel, TComponent>()` | 🔶 INFERRED | Modeled after `MentorDiagram.Register<TModel, TComponent>()` — verified source pattern |
| `DvDiagram.CreateNode<T>()` | 🔶 INFERRED | Modeled after `MentorDiagram.CreateNode<T>()` — verified source uses `Activator.CreateInstance` |
| `BuildRenderTree` sequence numbers | ✅ VERIFIED | All 6 renderers use incrementing `seq` counter — Blazor convention confirmed |
| `foreignObject` for custom widgets | ✅ VERIFIED | `Node710Renderer.cs` and `LinkLabel710Renderer.cs` both use `foreignObject` |
| `EventCallback.Factory.Create<T>()` | ✅ VERIFIED | Standard Blazor API — confirmed in renderer files |
| `DvLayer<T>` | 🔶 INFERRED | Modeled after `DiagramLayer.cs` — verified source but new generic design |
| `DvTreeLayout` / `DvGridLayout` | ⚠️ ASSUMED | New — no existing layout implementation was found in the codebase |
| SVG `<pattern>` for grid | ⚠️ ASSUMED | Standard SVG — not verified in codebase (no existing SVG grid) |
| `ResizeObserver` JS interop | ✅ VERIFIED | `Node710Renderer.cs` uses `JsRuntime.ObserveResizes` — same pattern proposed |
| `InvokeAsync(StateHasChanged)` | ✅ VERIFIED | All renderers use this pattern for off-thread model change events |

---

## 3. Integration Seam Annotations

| Integration Seam | Knowledge Level | Notes |
|---|---|---|
| **ITreeNode compatibility** | ✅ HIGH | Interface is well-understood, all model classes can implement it identically to existing diagram models |
| **IComponentViewer bridge** | ✅ HIGH | Pattern is clear from `DiagramNode` etc. — bridge subclasses go in FoundryMentorModeler, not in the library |
| **CSS in Blazor library** | ⚠️ MEDIUM | CSS files in Razor class libraries need to be in `wwwroot/` or use CSS isolation. Existing Canvas2D doesn't use component CSS — verify the bundling mechanism |
| **JS interop file location** | ⚠️ MEDIUM | Must be in `wwwroot/` of the Razor class library. Consuming apps reference via `_content/FoundryWorldsAndDrawings/js/dv-interop.js` |
| **BuildRenderTree + child components** | ✅ HIGH | Verified pattern from 6 existing renderers — `OpenComponent<T>` with `AddAttribute("ParameterName", value)` |
| **SVG foreignObject + Blazor components** | 🔶 MEDIUM | Verified that existing renderers use this, but browser quirks around overflow/sizing are real — needs testing |
| **Event routing through SVG** | ⚠️ MEDIUM | SVG `pointer-events` behave differently than HTML — the `onpointerdown` capture-at-container approach should work but needs testing |
| **`_Imports.razor` namespace registrations** | ⚠️ LOW RISK | New `Diagram` namespace needs to be added to `_Imports.razor` in consuming projects |

---

## 4. Service Implementation Status

| Service/Interface | Implementation Read? | Notes |
|---|---|---|
| `ITreeNode` | ✅ YES — full interface | 7 methods + 4 default implementations. Clean, well-understood. |
| `IComponentViewer` | ✅ YES — full interface | 9 methods. Bridge pattern is clear. |
| `BlazorDiagram` (Z.Blazor.Diagrams) | ⚠️ INTERFACE ONLY | Read the consumption patterns but not the NuGet package source. New library replaces this entirely. |
| `Canvas2DContext` | ✅ YES — full implementation | 430 lines. Not used by DiagramViewer, but confirmed the batching pattern for reference. |
| `ComponentBus` (pub/sub) | ⚠️ INTERFACE ONLY | Used in `UIDiagram.cs` for window messages. DiagramViewer uses direct event subscriptions instead — may want pub/sub later. |

---

## 5. Spec Weakness Confessions

### Confession 1: Layout Algorithms Are Speculative

The spec includes `DvTreeLayout`, `DvGridLayout`, and `DvForceDirectedLayout` but **no existing layout implementation was found in the codebase** to reference. The `solution.LayoutDiagramTreeFromRoot()` method exists in FoundryMentorModeler but I did not read its implementation. Layout algorithm correctness is entirely assumed.

**Risk:** Layout may be the most complex part to implement correctly. Tree layout with proper spacing, edge routing, and incremental updates is non-trivial.

**Mitigation:** Start with `DvGridLayout` (trivially correct: arrange in a grid) and defer tree/force layouts to Phase 5.

### Confession 2: Orthogonal Router Is Non-Trivial

The `DvOrthogonalRouter` produces L-shaped or Z-shaped paths that avoid obstacles. This is algorithmically complex (visibility graph, A* pathfinding). The spec describes the interface but not the algorithm.

**Risk:** Implementing a good orthogonal router that avoids node overlap is a significant engineering challenge.

**Mitigation:** Start with `DvStraightRouter` (direct line) for V1. Add orthogonal routing as Phase 2 enhancement.

### Confession 3: Browser Compatibility of SVG foreignObject

The spec relies on `<foreignObject>` to embed Blazor components inside SVG nodes. This works in modern browsers but has known issues with styling inheritance, overflow handling, and pointer events in older browsers.

**Risk:** Custom widgets may render incorrectly in some browsers.

**Mitigation:** Test early. Fall back to HTML overlay divs positioned absolutely if `foreignObject` proves unreliable.

---

## 6. Indy's Verification Priority

**Check these first — ordered by risk:**

1. **⚠️ SVG foreignObject rendering** — Create a minimal test: SVG with `<foreignObject>` containing a Blazor component. Verify it renders, receives events, and styles correctly in the target browser.

2. **⚠️ CSS bundling in Razor class library** — Verify how CSS files are included from FoundryWorldsAndDrawings. Check if `wwwroot/` files are served correctly via `_content/` path. Look at how existing `Canvas/JsLib/` files are referenced.

3. **⚠️ Event routing** — Create a minimal SVG with nested `<g>` elements and verify that `onpointerdown` events fire correctly and provide accurate coordinates for hit-testing.

4. **🔶 DvDiagram.RegisterWidget pattern** — Verify the `Dictionary<Type, Type>` approach works with `builder.OpenComponent(seq++, widgetType)` where `widgetType` is resolved at runtime.

5. **⚠️ Layout algorithm correctness** — Skip tree layout initially. Implement grid layout first (trivially correct).

6. **🔶 BuildRenderTree sequence number patterns** — Confirm the incrementing `seq` pattern works correctly when rendering variable numbers of child components (ports, vertices, labels).

---

## 7. What's New vs. What's Ported

| Aspect | Source | Confidence |
|--------|--------|-----------|
| Model class structure (Dv*) | Ported from `DiagramNode`/`DiagramLink`/etc. — removed third-party base classes | HIGH |
| `BuildRenderTree` patterns | Ported from `*710Renderer.cs` files — same approach, different model types | HIGH |
| Widget registration pattern | Ported from `MentorDiagram.Register<T,T>()` | HIGH |
| Behavior system | **NEW** — modeled loosely after `DiagramDragMovablesBehavior` but new architecture | MEDIUM |
| Anchor abstraction | **NEW** — no equivalent in existing codebase (Z.Blazor.Diagrams has `SinglePortAnchor` but we're replacing it) | MEDIUM |
| Viewport/pan/zoom | **NEW** — existing renderers get pan/zoom from `BlazorDiagram` internals (not read) | MEDIUM |
| Router/PathGenerator | **NEW** — interfaces modeled after Z.Blazor.Diagrams but implementations are from scratch | LOW-MEDIUM |
| Layout algorithms | **NEW** — no reference implementation found | LOW |
| CSS styling | **NEW** — no existing diagram CSS (Z.Blazor.Diagrams provides its own) | MEDIUM |
| JS interop (minimal) | Ported pattern from `Node710Renderer` `ObserveResizes` | HIGH |

---

## Summary

This is a **greenfield library** with well-understood patterns borrowed from the existing `FoundryMentorModeler/Diagram/` code. The highest-risk areas are:

1. SVG `foreignObject` browser compatibility
2. Orthogonal routing algorithm
3. Layout algorithms (no reference implementation)

The lowest-risk areas are:

1. Model class design (directly modeled from existing, well-read code)
2. `BuildRenderTree` rendering (6 verified examples to follow)
3. `ITreeNode`/`IComponentViewer` integration (interfaces fully understood)

**Recommendation:** Build Phases 1-3 first (models, straight routing, SVG rendering). Get a visible, interactive diagram on screen. Then add behaviors (Phase 4) and layouts (Phase 5). Defer orthogonal routing and advanced layouts.
