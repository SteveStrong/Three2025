# Mentor 2D Modeler — Atlas Handoff Brief

**Spec:** `MENTOR_2D_MODELER_SPEC.md`  
**Atlas:** Claude (Architect)  
**Date:** February 13, 2026  
**Purpose:** Honest annotation of spec reliability — what I verified vs. what I assumed

---

## Files I Actually Opened During Research

| File | Read? | How Much |
|---|---|---|
| `Components/Pages/Mentor2DModeler.razor` | ✅ | Full file (587 lines) — all markup, toolbar, tabs, CSS |
| `Components/Pages/Mentor2DModeler.razor.cs` | ✅ | Full file (722 lines) — lifecycle, shape creation, chat, logging |
| `Components/Pages/ConversationalModeler.razor` | ✅ | Full file — studied as sibling page |
| `Components/Pages/ConversationalModeler.razor.cs` | ✅ | Full file — compared DI, lifecycle, chat integration |
| `Apprentice/IShape2DTech.cs` | ✅ | Full interface — all method signatures |
| `Apprentice/Shape2DTech.cs` | ✅ | Full file — constructor, editor creation, method delegation |
| `Apprentice/ModelTech.cs` | ✅ | Full file — interface + implementation, SetPageContext, visual methods |
| `FoundryMentorModeler/Mentor/MentorStudio.cs` | ✅ | Full file — CreateShape, Attach, CreateNodeShape, Menu, constructor |
| `FoundryWorldsAndDrawings/Shared/Canvas2DComponent.razor` | ✅ | Template — BECanvas, CanvasInputWrapper |
| `FoundryWorldsAndDrawings/Shared/Canvas2DComponent.razor.cs` | ✅ | Full file — Page property, EstablishPage, render loop |
| `FoundryMentorModeler/Shared/MentorTreeView.razor` | ✅ | Full file — UnifiedTreeView usage |
| `FoundryMentorModeler/Shared/MentorTreeView.razor.cs` | ✅ | Full file — GetAllNodes, event subscriptions |
| `FoundryMentorModeler/CodeStatus.cs` | ✅ | AddFoundryMentorModelerServices() — DI registrations |
| `Three2025/Program.cs` | ⚠️ Partial | Read DI registration section (lines ~86-107) |
| `ShapeTreeView` component | ❌ NOT FOUND | Searched workspace — no .razor file exists matching this name |
| `FoundryMentorModeler/Model/KnConcept.cs` | ❌ NOT READ | Described KnowledgeType from page usage, not source |
| `FoundryMentorModeler/Model/KnProperty.cs` | ❌ NOT READ | Same — inferred from Playground.CreateShape<T> pattern |
| `MentorShape2D` class | ❌ NOT READ | Described from usage in MentorStudio and page |
| `Shape2DEditor` class | ❌ NOT READ | Described as delegation target in Shape2DTech |
| `ModelEditor` class | ❌ NOT READ | Described as delegation target in ModelTech |
| `ChatOrchestrator.ProcessMessageAsync()` | ✅ | Read in prior spec work — verified non-streaming path |

---

## Verification Table

| Reference | Status | Evidence |
|---|---|---|
| `Playground.CreateShape<KnConcept>(title, page)` | ✅ VERIFIED | Read MentorStudio.CreateShape<T> — creates KnBase, calls CreateNodeShape, adds to page |
| `Playground.Attach(shape, target)` | ✅ VERIFIED | Read MentorStudio.Attach — checks IsConnectAllowed/IsDropAllowed, creates connector or containment |
| `Shape2DTech.SetPage(canvasPage)` | ✅ VERIFIED | Read Shape2DTech — stores page reference for subsequent operations |
| `ModelTech.SetPageContext(page, studio)` | ✅ VERIFIED | Read ModelTech — stores `_currentPage` and `_mentorStudio`, required by visual methods |
| `Canvas2DReference.Page` | ✅ VERIFIED | Read Canvas2DComponentBase — `Page` returns `ManagedPage`, set in OnAfterRenderAsync from `SceneName` |
| `ChatOrchestrator.ProcessMessageAsync()` | ✅ VERIFIED | Read implementation in prior spec — intent analysis → agent routing → tool execution |
| `MentorTreeView` | ✅ VERIFIED | Read component — uses UnifiedTreeView, GetAllNodes from MentorServices |
| `ShapeTreeView` | ❌ UNREAD / NOT FOUND | Searched entire workspace — no `.razor` file found. Referenced in markup but may not compile. |
| `MentorShape2D` constructors/properties | ⚠️ ASSUMED | Used in MentorStudio.CreateNodeShape — described behavior from studio code, not MentorShape2D source |
| `KnBase` hierarchy (KnConcept, KnProperty, etc.) | ⚠️ ASSUMED | Described from usage in CreateShape<T>; did not open individual class files |
| `DrawingEditChanged` event | ⚠️ ASSUMED | Published by MentorStudio; consumed by MentorTreeView. Did not read event definition |
| `FoPage2D.AddShape()` | ⚠️ ASSUMED | Called by CreateNodeShape; did not read FoPage2D implementation |
| `IMentorServices.PubSub` | ⚠️ ASSUMED | Used in MentorStudio constructor; described from usage |
| `IMentorServices.MentorModel` | ⚠️ ASSUMED | Used in SaveModel/LoadModel; described from page usage |
| `RadzenSplitter` / `RadzenSplitterPane` | 🔶 INFERRED | Radzen Blazor component; described from markup usage, not Radzen source |
| `TestSequenceSelector` | ⚠️ ASSUMED | Present in markup; did not open component source |
| `OPResult` return from Shape2DTech methods | ⚠️ ASSUMED | Same as Chat Orchestrator spec — did not read OPResult source |

---

## Integration Seams

### Seam 1: Canvas Page ↔ Technician Connection
- **Connection:** `OnAfterRenderAsync` calls `Shape2DTech.SetPage(canvasPage)` and `ModelTech.SetPageContext(canvasPage, Playground)` to connect both technicians to the canvas-managed page
- **Atlas's knowledge:** ✅ VERIFIED — read both SetPage and SetPageContext implementations, and the page lifecycle
- **What could go wrong:** The 100ms `Task.Delay` may not be enough for canvas initialization on slow machines. If `Canvas2DReference.Page` is null, the whole initialization aborts.
- **Indy should check:** Whether the 100ms delay is reliable. Consider a retry loop or event-based notification from Canvas2DComponent.

### Seam 2: MentorStudio ↔ DI Scope
- **Connection:** Page creates `new MentorStudio(Workspace, MentorServices.PubSub, MentorServices)` — bypassing DI despite `IMentorStudio` being registered
- **Atlas's knowledge:** ✅ VERIFIED — read MentorStudio constructor and DI registration
- **What could go wrong:** If MentorServices or its dependencies have been disposed/recreated by Blazor's circuit lifecycle, the manually-created MentorStudio may hold stale references.
- **Indy should check:** Whether the DI-registered IMentorStudio could be used instead, or whether the manual creation is truly needed for page-specific canvas binding.

### Seam 3: AI Tool Calls ↔ Canvas Rendering
- **Connection:** When the AI calls `ModelTech.CreateConceptShape()`, it creates a MentorShape2D and adds it to the canvas page. But the page doesn't call `StateHasChanged()` after AI tool execution — the Blazor render loop doesn't know the canvas changed.
- **Atlas's knowledge:** ⚠️ ASSUMED — I read the chat processing flow but didn't trace what happens to the canvas after a tool call creates shapes during an AI response.
- **What could go wrong:** Shapes created by AI tool calls may not appear on canvas until the next user interaction triggers a re-render. The canvas has its own animation loop (RAF-based), so shapes may appear immediately on the canvas but the tree view may not update.
- **Indy should check:** Does `Canvas2DComponent`'s animation loop pick up new shapes automatically? Does `MentorTreeView` refresh when `DrawingEditChanged` fires from AI-created shapes?

### Seam 4: ShapeTreeView Component
- **Connection:** Markup references `<ShapeTreeView/>` in the "Shapes" tab
- **Atlas's knowledge:** ❌ NOT FOUND — searched entire workspace for `ShapeTreeView.razor` with zero results
- **What could go wrong:** If this component doesn't exist, the "Shapes" tab either shows nothing, throws a compile error, or the component is defined inline somewhere I didn't find.
- **Indy should check:** Does the project compile? If yes, where is ShapeTreeView defined? If no, is this tab dead code?

---

## Service Implementation Status

| Service | Interface Read? | Implementation Read? | Risk Assessment |
|---|---|---|---|
| `MentorStudio` | ✅ | ✅ Full | Low risk — read CreateShape, Attach, constructor |
| `ModelTech` | ✅ | ✅ Full | Low risk — read all tool methods and visual methods |
| `Shape2DTech` | ✅ | ✅ Full | Low risk — delegates to Shape2DEditor |
| `Shape2DEditor` | ❌ | ❌ NOT READ | **Medium risk** — Shape2DTech delegates all work here |
| `ModelEditor` | ❌ | ❌ NOT READ | **Medium risk** — ModelTech delegates model operations here |
| `Canvas2DComponent` | ✅ | ✅ Full | Low risk — understood page creation and render loop |
| `MentorTreeView` | ✅ | ✅ Full | Low risk — understood node building and event subscription |
| `ChatOrchestrator` | ✅ | ✅ Full | Low risk — verified in prior spec work |
| `FoPage2D` | ❌ | ❌ NOT READ | Medium risk — central to everything; described only from consumers |
| `MentorShape2D` | ❌ | ❌ NOT READ | Medium risk — the visual node; described from MentorStudio usage |

---

## Where This Spec Is Probably Wrong

1. **ShapeTreeView existence.** I could not find this component anywhere in the workspace. The spec marks it with ⚠️ but doesn't resolve whether it compiles. If the project builds successfully with this reference, the component exists somewhere I didn't search (perhaps generated, or in a NuGet package). If it doesn't compile, this is dead markup.

2. **Canvas refresh after AI tool calls.** I described shape creation via AI as working because ModelTech.SetPageContext was called. But I didn't verify that shapes added to the page during an AI response actually render on the canvas in real-time. The canvas animation loop (RAF-based) should pick them up, but the tree view refresh depends on event subscriptions I marked as ASSUMED.

3. **KnowledgeType enum values.** I listed 10 knowledge types from the toolbar buttons and page code. I did not open any KnBase subclass files to verify they all exist, or that KnowledgeType has exactly these values. The switch statement in `CreateShapeWithType()` maps them, but the enum definition is unread.

4. **Position grid values.** I described nextX wrapping at 800 and nextY at 600 from reading the code. I noted these don't match canvas size (1800x1200) as a known issue. But I didn't verify whether the canvas has zoom/pan that would make 800x600 actually fill the viewport.

---

## Indy's Verification Priority

1. **Compile the project and check ShapeTreeView.** This is the most uncertain item in the entire spec. Either it exists and I missed it, or it's a reference to a component that doesn't exist. 2 minutes to resolve.

2. **Open `Shape2DEditor.cs` and `ModelEditor.cs`.** Both technicians delegate all work to their respective editors. I read the technicians but not the editors. If tool calls fail, the bug is in the editor, not the technician.

3. **Test AI shape creation on canvas.** Type "create a concept called Beam" in the chat. Does a shape appear on the canvas? Does the tree refresh? This tests the full seam from AI → ModelTech → MentorStudio → canvas page → MentorTreeView.

4. **Verify the 100ms delay is sufficient.** On a slow machine, is `Canvas2DReference.Page` always non-null after 100ms? If not, the entire page initialization silently aborts.

5. **Open one KnBase subclass** (e.g., `KnConcept.cs`) and verify it matches the description. The knowledge type hierarchy is central to the page and entirely unread.

---

*This Handoff Brief is my honest annotation of what I verified and what I assumed while writing the Mentor 2D Modeler Specification. The spec is strongest on the page's markup, code-behind, and immediately consumed services (MentorStudio, ModelTech, Shape2DTech). It's weakest on the downstream implementations (editors, FoPage2D, MentorShape2D) and the ShapeTreeView mystery.*
