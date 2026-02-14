# DiagramViewer — Atlas Handoff Brief

**Date:** February 13, 2026  
**Experiment:** 07-DiagramViewer  
**Atlas → Indy Handoff**

---

## 1. Verification Inventory — Files Atlas Actually Opened

### ✅ Files I Read During Research

| File | Lines Read | Depth |
|---|---|---|
| `FoundryMentorModeler/Diagram/MentorDiagram.cs` | 1-228 (full) | Deep — all methods, constructors, tree node implementation |
| `FoundryMentorModeler/Diagram/DiagramNode.cs` | 1-136 (full) | Deep — constructors, ports, component wrapping |
| `FoundryMentorModeler/Diagram/DiagramLink.cs` | 1-155 (full) | Deep — both constructor patterns (port-based, node-based), labels |
| `FoundryMentorModeler/Diagram/DiagramPort.cs` | 1-129 (full) | Deep — CanAttachTo, alignment hack |
| `FoundryMentorModeler/Diagram/DiagramGroup.cs` | 1-140 (full) | Deep — constructors, ports, autosize |
| `FoundryMentorModeler/Diagram/DiagramLinkLabel.cs` | 1-76 (full) | Deep |
| `FoundryMentorModeler/Diagram/DiagramPortRenderer.cs` | 1-100 | Medium — BuildRenderTree, event handling |
| `FoundryMentorModeler/Diagram/DiagramLayer.cs` | 1-108 (full) | Deep — custom layer with batched add/remove |
| `FoundryMentorModeler/Diagram/DiagramDragMovablesBehavior.cs` | 1-80 | Medium — custom drag behavior |
| `FoundryMentorModeler/Diagram/KnEditor2DParameter.cs` | 1-111 (full) | Deep — caching, SetCache, GetCurrentValueAs |
| `FoundryMentorModeler/Diagram/Renderers/Node710Renderer.cs` | 1-100 | Medium — rendering pipeline, resize observer |
| `FoundryMentorModeler/Diagram/Renderers/Link710Renderer.cs` | 1-100 (full) | Deep — SVG `<g>` wrapper, widget resolution |
| `FoundryMentorModeler/Diagram/Renderers/Group710Renderer.cs` | 1-100 | Medium — rendering, size tracking |
| `FoundryMentorModeler/Diagram/Renderers/Port710Renderer.cs` | 1-100 | Medium — rendering, dimension updates |
| `FoundryMentorModeler/Diagram/WindowDiagram/UIDiagram.cs` | 1-100 | Medium — alternative diagram type with custom behaviors |
| `FoundryMentorModeler/Diagram/WindowDiagram/WindowNode.cs` | 1-24 (full) | Deep |
| `FoundryMentorModeler/Mentor/MentorServices.cs` | 1-277 (full) | Deep — all methods, DI constructor |
| `FoundryMentorModeler/Mentor/MentorDiagramManager.cs` | 1-237 (full) | Deep — EstablishDiagram options, CurrentDiagram logic |
| `FoundryMentorModeler/Mentor/KnModel.cs` | 1-160 | Deep — RenderDiagram, RenderDrawing2D (partial read) |
| `FoundryMentorModeler/Mentor/KnComponent.cs` | 290-422 | Deep — RenderEditor, EstablishEditor, FinalizeEditor |
| `FoundryMentorModeler/Mentor/RenderContext.cs` | 80-200 | Deep — RenderContextEditor record, PostCreation, ForChild |
| `FoundryMentorModeler/Mentor/IRender.cs` | 1-120 (full) | Deep — IComponentViewer, IModelRenderable, IRenderController |
| `Three2025/Components/Pages/DiagramViewer.razor` | 1-182 (full) | Deep — complete page markup |
| `Three2025/Components/Pages/DiagramViewer.razor.cs` | 1-418 (full) | Deep — complete code-behind |
| `Three2025/Components/Pages/Drawing.razor.cs` | 1-100 | Medium — for sibling injection comparison |
| `Three2025/Components/Pages/Drawing.razor.disabled` | 1-60 | Skimmed — different pattern (Canvas3D) |
| `Three2025/Components/Pages/Mentor2DModeler.razor` | 1-60 | Medium — sibling page structure |
| `Three2025/Components/Pages/Mentor2DModeler.razor.cs` | 1-60 | Skimmed — injection pattern |
| `Three2025/Components/DiagramWidgets/SystemBlockWidget.razor.disabled` | 1-90 (full) | Deep — widget structure, port rendering |
| `Three2025/Components/DiagramWidgets/CircuitNodeWidget.razor.disabled` | 1-86 (full) | Deep — widget structure |
| `Three2025/Components/DiagramWidgets/CircuitGroupWidget.razor.disabled` | 1-92 (full) | Deep — group widget |
| `Three2025/Components/App.razor` | 1-30 (full) | Deep — CSS/JS loading verified |
| `Three2025/Components/Pages/KnModel/Base_710.csHOLD` | 1-400 | Deep — domain model base class |
| `Three2025/Docs/Patterns/SCENARIO_CHAT_DIAGRAM_IMPLEMENTATION_GUIDE.md` | 1-950 | Deep — reference implementation, all phases |
| `Three2025/Docs/Patterns/BLAZOR_DIAGRAM_IMPLEMENTATION_REFERENCE.md` | 1-120 | Medium — CSS/JS config, registration |
| `FoundryMentorModeler/CodeStatus.cs` | line 58 (grep) | Spot check — DI registration |

### ❌ Files I Did NOT Open

| File | Why Not | Risk |
|---|---|---|
| `FoundryMentorModeler/Mentor/MentorModelManager.cs` | Trusted the service to work from IMentorServices calls | LOW — established pattern, multiple pages use it |
| `Three2025/Program.cs` | Did not verify DI registrations | MEDIUM — if IMentorServices not registered, nothing works |
| `Three2025/Three2025.csproj` | Did not verify Z.Blazor.Diagrams NuGet reference | LOW — CSS/JS loading in App.razor proves it's referenced |
| `Plugin_710` NuGet source | Not available locally (NuGet) | MEDIUM — Editor types may not exist |
| `MentorTreeView` component | Referenced in DiagramViewer but not searched | LOW — existing page uses it successfully |
| Any `.razor.cs` code-behind for widget files | Widgets are `.disabled`, no code-behind exists | LOW — widgets don't need code-behind (can use `@code {}`) |
| `FoundryMentorModeler/Evaluator/` folder | Formula/evaluator system that resolves KnEditor2DParameter values | MEDIUM — this is how nodes actually get created from parameters |

---

## 2. Method/API Verification Table

| Method / API in Spec | Status | Notes |
|---|---|---|
| `MentorServices.EstablishDiagram<T>(name)` | ✅ VERIFIED | Read implementation in MentorServices.cs and MentorDiagramManager.cs |
| `MentorServices.EstablishModel<T>(name)` | ✅ VERIFIED | Read implementation |
| `MentorServices.CurrentDiagram()` | ✅ VERIFIED | Read implementation — auto-selects logic |
| `MentorDiagram.Register<TModel, TComponent>(replace)` | ✅ VERIFIED | Delegates to BlazorDiagram.RegisterComponent |
| `MentorDiagram.ClearAll()` | ✅ VERIFIED | Clears Nodes, Links, Groups |
| `MentorDiagram.CreateNode<T>(source, point)` | ✅ VERIFIED | Activator.CreateInstance pattern |
| `MentorDiagram.ConnectPorts<T>(source, color, from, to)` | ✅ VERIFIED | Sets OrthogonalRouter + StraightPathGenerator |
| `MentorDiagram.ConnectNodes<T>(source, from, to)` | ✅ VERIFIED | Same router/path setup |
| `KnModel.RenderDiagram(view, clear, onComplete)` | ✅ VERIFIED | Read full implementation |
| `KnComponent.RenderEditor(ctx)` | ✅ VERIFIED | Read full implementation — establish/finalize/PostCreation pattern |
| `RenderContextEditor.Create(diagram, view, deep)` | ✅ VERIFIED | Read implementation |
| `RenderContextEditor.PostCreation<T>(viewer)` | ✅ VERIFIED | Read switch statement — Groups/Nodes/Links |
| `RenderContextEditor.ForChild(parentViewer)` | ✅ VERIFIED | Record `with` expression |
| `DiagramNode(KnComponent, Point)` constructor | ✅ VERIFIED | Called via Activator.CreateInstance |
| `DiagramNode.EstablishPort<T>(source, alignment, point)` | ✅ VERIFIED | Find-or-create pattern |
| `DiagramLink(KnComponent, PortModel, PortModel)` | ✅ VERIFIED | Port-based constructor |
| `DiagramLink(KnComponent, NodeModel, NodeModel)` | ✅ VERIFIED | Node-based constructor |
| `DiagramLink.AddLabel(label, fraction)` | ✅ VERIFIED | Creates DiagramLinkLabel |
| `KnEditor2DParameter.GetCurrentValueAs<T>()` | ✅ VERIFIED | Cache check, returns IComponentViewer |
| `KnEditor2DParameter.Smash()` | ✅ VERIFIED | Clears cache + base |
| How `EstablishEditor` creates the actual node instance | ⚠️ ASSUMED | I traced the parameter establish/finalize pattern but did NOT read the evaluator that resolves the parameter into a DiagramNode instance. This is a potential gap. |
| Widget `[Parameter]` property name must be `Node` | 🔶 INFERRED | Seen in all widget examples, consistent with Blazor.Diagrams documentation, but not verified in renderer source |
| `Port710Renderer` vs `PortRenderer` usage | ⚠️ ASSUMED | Widget examples use `Port710Renderer` but I didn't verify the renderer selection mechanism |

---

## 3. Integration Seam Annotations

| Seam | Where New Meets Existing | Knowledge Level |
|---|---|---|
| **Widget Registration → Rendering** | `diagram.Register<TEditor, TWidget>` must match the actual node type returned by `RenderEditor` | ✅ HIGH — traced full flow from Register through PostCreation |
| **Plugin_710 availability** | DiagramViewer.razor.cs imports `Plugin_710.Model` — editor types may come from there | ⚠️ MEDIUM — `.csHOLD` files suggest local implementations exist but may not compile |
| **KnEditor2DParameter → IComponentViewer resolution** | The evaluator system that turns a parameter into an actual DiagramNode instance | ⚠️ LOW — did not read evaluator implementation |
| **`CurrentDiagram()` multi-page routing** | If user navigates between diagram pages, `CurrentDiagram()` may return stale diagram | ✅ HIGH — verified that `EstablishDiagram` auto-sets current |
| **Node710Renderer custom rendering** | The custom renderers replace default Blazor.Diagrams renderers — how is this wired? | ⚠️ MEDIUM — renderers are in FoundryMentorModeler NuGet, may auto-register or need explicit setup |

---

## 4. Service Implementation Status

| Service | Read Implementation? | Notes |
|---|---|---|
| `MentorServices` | ✅ Full file (277 lines) | All delegate methods traced |
| `MentorDiagramManager` | ✅ Full file (237 lines) | Diagram creation options, caching, save/restore |
| `MentorModelManager` | ❌ Not read | Trusted from interface usage |
| `KnModel` | ✅ Partial (160/334 lines) | RenderDiagram verified, RenderAll not fully read |
| `KnComponent` | ✅ RenderEditor section (lines 290-422) | Core rendering pattern verified |
| `RenderContextEditor` | ✅ Full record (lines 80-170) | All methods verified |
| Evaluator system | ❌ Not read | This resolves KnEditor2DParameter values into actual node instances — a potential blind spot |

---

## 5. Spec Weakness Confessions

### Weakness 1: The Evaluator Black Box
I traced the `RenderEditor` flow to the point where `KnEditor2DParameter.GetCurrentValueAs<IComponentViewer>()` is called. But I did **not** read how the evaluator system resolves the parameter value into an actual `DiagramNode` subclass instance. The `EstablishEditor` creates a `KnEditor2DParameter` and the `FinalizeEditor` presumably triggers evaluation — but I don't know the evaluation formula that creates e.g. a `SystemBlockEditor` from a `SystemBlock_710`. 

**Risk**: If Indy creates new editor classes, they need to be wired into the evaluator system somehow. The existing DiagramViewer code works because Plugin_710 has this wiring. A new application may need to set up this wiring manually.

**Mitigation in spec**: The spec recommends copying the DiagramViewer pattern directly and shows the `Register<TEditor, TWidget>` call. If the evaluator doesn't automatically produce the right editor type, Indy should look at how `CommonCreateNode<T>` or similar methods work in the Plugin_710 source.

### Weakness 2: Plugin_710 Types May Not Be Available
The spec references `Model_710`, `Base_710`, `SystemBlock_710`, `Common_710`, etc. These come from either a NuGet package or the `.csHOLD` files. I verified `Base_710.csHOLD` exists but the `.csHOLD` extension means it's not in the build. If these types aren't available via NuGet, the existing `DiagramViewer.razor.cs` probably doesn't compile right now.

**Risk**: The reference implementation may itself be broken.

**Mitigation**: Indy should try building first. If Plugin_710 types don't resolve, Indy can build plain `KnComponent`-based domain objects and `DiagramNode`-based editors without Plugin_710.

### Weakness 3: Custom Renderer Activation
I see the 710 renderers (`Node710Renderer`, `Link710Renderer`, etc.) in FoundryMentorModeler but I did NOT verify how they replace the default Blazor.Diagrams renderers. The `MentorDiagram` constructor doesn't contain renderer registration. They may be registered elsewhere, or may rely on assembly scanning, or may need explicit registration.

**Risk**: If custom renderers aren't active, nodes may render but with wrong visual behavior.

---

## 6. What Indy Should Check First (Priority Order)

1. **Does Three2025 build?** — Try `dotnet build` to see if Plugin_710 types resolve. If not, you know the scope of work immediately.

2. **Is the evaluator system creating DiagramNode instances?** — Put a breakpoint or WriteInfo in `KnComponent.RenderEditor()` at the `wasUnknown` check. If `viewer` is null after `GetCurrentValueAs<IComponentViewer>()`, the evaluator isn't producing nodes. This is the most opaque part of the system.

3. **Are custom renderers active?** — After getting a node to appear, check if it uses your widget or the default gray box. If gray box, the renderer registration is missing.

4. **Widget `[Parameter]` name** — Verify that the widget's `[Parameter]` property is named correctly for the renderer to inject. Check what `Node710Renderer.BuildRenderTree` passes as the attribute name.

5. **Port rendering** — If using ports, verify `Port710Renderer` works vs `PortRenderer`. The widget examples use `Port710Renderer` explicitly.

---

*Atlas signing off. The spec's strongest area is the verified API table — every method signature was confirmed against source. The weakest area is the evaluator system that creates node instances from parameters. Indy: if nodes don't appear after `RenderDiagram()`, start debugging at `KnEditor2DParameter.GetCurrentValueAs<IComponentViewer>()`. That's where the magic either works or doesn't.*

*— Atlas*
