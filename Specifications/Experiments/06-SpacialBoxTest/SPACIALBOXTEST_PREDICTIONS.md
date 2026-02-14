# SpacialBoxTest Predictions

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Companion To:** `SPACIALBOXTEST_SPECIFICATION.md`  
**Based On:** Atlas Prediction Checklist v1.0, AARs from ClockDemo, Tug of War, Multi-Canvas 3D, Multi-Canvas 2D

---

## Prediction Budget Allocation

| Category | Budget | Predictions |
|---|---|---|
| **Integration seam risks** | 30% | #1, #2, #3 |
| **Spec gap / omission risks** | 25% | #4, #5 |
| **Implementer behavior** | 25% | #6, #7 |
| **Framework / API risks** | 10% | #8 |
| **Runtime / environment** | 10% | #9 |

**Total predictions:** 9  
**High confidence:** 4 (predictions #1, #6, #8, #9)  
**Medium confidence:** 3 (#3, #5, #7)  
**Low confidence:** 2 (#2, #4)

---

## Integration Seam Predictions

### ~~Prediction 1: SceneTreePanel Will NOT Resolve~~ — RESOLVED PRE-BUILD

**Confidence:** ~~🟢 High (90%)~~ → **RESOLVED**
**Category:** Integration seam

**Resolution:** Sully approved adding `FoundryMicroCore.Blazor.Controls` project reference to `Three2025.csproj` and `@using` directives to `_Imports.razor` BEFORE handoff to Indy. This prediction is now moot — SceneTreePanel will resolve.

---

### Prediction 2: Sully Will Decide Against Adding the Blazor Controls Dependency

**Confidence:** 🔴 Low (30%)  
**Category:** Integration seam  
**Cost if it hits:** 10 minutes (just remove the tree panel entirely or use a simple `UnifiedTreeView` directly if available through a different path)

**The integration seam:** Adding a project reference to `FoundryMicroCore.Blazor.Controls` is a project-level architectural decision. The current page has `<ShapeTreeView/>` which doesn't work. The fix requires either adding a new project dependency or removing the panel.

**What I predict:** Sully may say "just remove the tree panel for now" rather than adding a new project reference. This would be the lower-risk option — the page works fine without it, and the tree view was broken anyway (ShapeTreeView doesn't exist).

**What would prove me wrong:** Sully wants the tree panel and approves adding the reference. Or Sully knows a different path to get tree view functionality without the new dependency (e.g., through FoundryWorldsAndDrawings exposing a tree component).

---

### Prediction 3: The OnStateChanged Callback Will Cause a Threading Issue

**Confidence:** 🟡 Medium (40%)  
**Category:** Integration seam  
**Cost if it hits:** 30-60 minutes

**The integration seam:** The model calls `OnStateChanged?.Invoke()` which the code-behind wires to `() => InvokeAsync(StateHasChanged)`. This pattern works when called from the Blazor synchronization context (button click handlers). But some model methods are called from within `OnAfterRenderAsync` (the `Initialize` method), and others from Tweener callbacks which may execute on a timer thread.

**What I checked:**
- `InvokeAsync(StateHasChanged)` is the correct Blazor pattern for marshaling to the render context
- Button click handlers execute on the Blazor sync context → `InvokeAsync` is technically redundant but safe
- `OnAfterRenderAsync` is already on the sync context → safe
- Tweener callbacks: `OnComplete(() => SetStatus("..."))` — if the Tweener ever gets pumped, this callback's execution context is unknown

**What I predict:** The basic button-click flow will work fine. If anyone fixes the Tweener to actually run, the `OnComplete` callbacks calling `SetStatus` → `OnStateChanged` may throw "The current thread is not associated with the Dispatcher."

**What would prove me wrong:** All model methods are only ever called from Blazor lifecycle/event handlers (which is true today since Tweeners don't actually run), so this never manifests.

---

## Spec Gap Predictions

### Prediction 4: Sully Will Request the Model Inherit MxComponent, Not Be a Plain Class

**Confidence:** 🔴 Low (35%)  
**Category:** Spec gap  
**Cost if it hits:** 20 minutes (change base class, add attributes, adjust constructor)

**The gap:** The spec explicitly says "plain class" for `SpacialBoxTestModel` and provides reasoning (ClockDemoModel uses MxComponent for discovery/tree participation, but this model doesn't need that). However, the spec also says the project direction is toward Model-behind with MxComponent-derived models.

**Why I left it as plain class:** SpacialBoxTestModel doesn't participate in the MxCore object graph — it's pure logic separation. Adding `MxComponent` inheritance would add constructor complexity (call `base(name)`, register with parent) without clear benefit for a test page.

**What I predict:** There's a reasonable chance Sully will say "make it MxComponent — I want consistency across all model classes" since that's the pattern direction. The spec's reasoning is sound but may not match Sully's convention preference.

**What would prove me wrong:** Sully agrees with the plain class approach. Or Sully doesn't comment on this at all.

---

### Prediction 5: The Spec Doesn't Address Whether the Page Currently Compiles

**Confidence:** 🟡 Medium (50%)  
**Category:** Spec gap  
**Cost if it hits:** 5 minutes (realization), 0 minutes (problem)

**The gap:** The current `SpacialBoxTest.razor` references `<ShapeTreeView/>`. This component does not exist. Either:
1. The page currently fails to compile (meaning it's already broken), or
2. The component is silently ignored by Blazor's rendering (unlikely for an unknown tag)

The spec treats this as "replace with SceneTreePanel" but doesn't acknowledge that the page may already be broken. If it IS currently broken, then the bar for "working" is lower — we just need to compile, not preserve existing behavior.

**What I predict:** Indy will discover during initial build that the page doesn't compile, and will recognize this is a pre-existing issue. This will actually make the task easier — Indy can remove `<ShapeTreeView/>` immediately without worrying about breaking something that works.

**What would prove me wrong:** `ShapeTreeView` resolves through some path I haven't found (e.g., a tag helper, a Radzen component, or a file in a location I didn't search).

---

## Implementer Behavior Predictions

### Prediction 6: The Model Extraction Will Go Smoothly — This Is Mechanical Work

**Confidence:** 🟢 High (85%)  
**Category:** Implementer behavior  
**Cost if it hits:** 0 (this is a "will work" prediction)

**What I predict:** The core task — extracting 573 lines of code-behind into a model class — is straightforward move-and-rewire work. The methods are self-contained, the dependencies are clear (IFoundryService, IGeometryVisualizationService, NavigationManager), and the spec provides exact guidance on what stays in code-behind vs. what moves to model.

**Evidence for this prediction:**
- Every method in the code-behind has clear boundaries
- No circular dependencies between methods
- The injection pattern is simple (constructor injection into model)
- Indy has successfully followed extraction specs in prior experiments
- The spec provides both the model skeleton and the slimmed code-behind skeleton

**What would prove me wrong:** A hidden dependency that prevents clean separation — for example, if a method needs both `Canvas3DComponent` (Blazor ref, must stay in code-behind) AND domain state (must live in model) in a way that can't be split. I've reviewed all methods and don't see this, but life finds a way.

---

### Prediction 7: Indy Will Be Tempted to Fix the Tweener Issue Despite the Spec Saying Not To

**Confidence:** 🟡 Medium (45%)  
**Category:** Implementer behavior  
**Cost if it hits:** 30-60 minutes (if the fix works) or 1-2 hours (if it creates new problems)

**What I predict:** The spec explicitly warns "Don't fix the Tweener animations while refactoring." But Indy will move the animation methods to the model, see `new Tweener()` with no `Update()` pump, and the engineer's instinct to fix broken code will kick in.

**The temptation path:**
1. Indy moves `AnimateDoorHinge()` to model
2. Indy notices the Tweener is never pumped
3. Indy finds `Shape3DTech.cs` line 343 showing the `Tweener.Update(frameTime)` pattern
4. Indy adds an animation frame subscription to pump the Tweener
5. This works... or creates disposal/lifecycle complexity the spec doesn't cover

**What would prove me wrong:** Indy reads the spec warning, acknowledges it, and moves the broken Tweener code as-is. This is the more likely outcome if Indy is disciplined about scope.

---

## Framework / API Predictions

### Prediction 8: All Shape Creation and Visualization APIs Will Work First Try

**Confidence:** 🟢 High (85%)  
**Category:** Framework / API  
**Cost if it hits:** 0 (this is a "will work" prediction)

**What I predict:** `FoShape3D.CreateBox()`, `FoModel3D.CreateModel()`, `stage.AddShape()`, and all `IGeometryVisualizationService.Show*()` methods will work exactly as they do in the existing code. The model extraction doesn't change any API calls — it just moves them to a different class.

**Evidence:**
- These APIs are battle-tested across SpacialBoxTest, SpacialFrameTest, and multiple other pages
- The refactoring is moving code, not changing behavior
- All method signatures are verified against source-audited API reference
- SpacialFrameTest uses identical patterns and works

**What would prove me wrong:** A bizarre edge case where the calling context matters (e.g., some method internally checks the calling assembly or uses ambient state from ComponentBase). Extremely unlikely.

---

## Runtime / Environment Predictions

### Prediction 9: The Page Will Produce Clean Console Output After Refactoring

**Confidence:** 🟢 High (80%)  
**Category:** Runtime / environment  
**Cost if it hits:** 0 (this is a "will work" prediction)

**What I predict:** After the model extraction, the page will produce the same console output it does today:
- `✅ SpacialBoxTest: Retrieved stage 'SpacialBoxScene' from Canvas` on load
- `✅ Created: 2×1.5×1m` after initial box creation
- Button-specific status messages
- No error spam, no Euler overflow warnings (no continuous animation unless tweener is fixed)

**Evidence:**
- The existing page has no continuous animation (Tweeners aren't pumped), so there's no source of per-frame console spam
- The `WriteSuccess()` and `WriteInfo()` calls are one-shot (button clicks), not per-frame
- Moving code to a model doesn't change logging behavior

**What would prove me wrong:** If the model extraction somehow breaks the initialization order (e.g., `AddAxisToScene()` runs before `_scene` is set), we'd get null reference exceptions in console. But the spec preserves the existing initialization flow.

---

## Meta-Prediction: What Atlas Got Wrong

**The blind spot I'm most worried about:** Prediction #1 (SceneTreePanel project reference). The spec confidently says "replace ShapeTreeView with SceneTreePanel" without ever checking whether `Three2025.csproj` references the library that contains SceneTreePanel. This is exactly the kind of blind spot the Atlas Specification Checklist was designed to catch — and I missed it in the spec, only finding it during prediction research.

**The lesson:** Specs should verify not just that a component exists in the workspace, but that it's **accessible** from the target project's dependency graph.

---

## What I'm NOT Predicting (Proven Non-Risks)

Per the prediction checklist, I'm explicitly not wasting budget on:

- ~~"Will Arena/Drawing singleton work?"~~ — Always works. FoStage3D + Arena pattern is battle-tested.
- ~~"Will Indy follow the spec?"~~ — Yes. Indy follows well-written specs. 0% chance of shortcut-taking.
- ~~"Will basic API signatures work?"~~ — All verified against source. They'll work.
- ~~"Will Canvas3D initialization work?"~~ — Same pattern as SpacialFrameTest, GlueTest3D, etc.
- ~~"Will there be compilation errors from using statements?"~~ — Except for the SceneTreePanel @using (Prediction #1), all usings are from existing code.

---

*These predictions were written following the Atlas Prediction Checklist v1.0 (February 10, 2026). Budget allocation verified: Integration 33%, Spec gaps 22%, Implementer behavior 22%, Framework 11%, Runtime 11%.*
