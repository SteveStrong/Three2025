# SpacialFrameTest Predictions

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Companion To:** `SPACIALFRAMETEST_SPECIFICATION.md`  
**Based On:** Atlas Prediction Checklist v1.0, SpacialBoxTest prediction experience

---

## Prediction Budget Allocation

| Category | Budget | Predictions |
|---|---|---|
| **Integration seam risks** | 30% | #1, #2 |
| **Spec gap / omission risks** | 30% | #3, #4 |
| **Implementer behavior** | 20% | #5 |
| **Framework / API risks** | 10% | #6 |
| **Runtime / environment** | 10% | #7 |

**Total predictions:** 7  
**High confidence:** 3 (predictions #1, #5, #6)  
**Medium confidence:** 3 (#2, #3, #7)  
**Low confidence:** 1 (#4)

---

## Integration Seam Predictions

### Prediction 1: Tree View Decision Will Cascade From SpacialBoxTest

**Confidence:** 🟢 High (85%)  
**Category:** Integration seam  
**Cost if it hits:** 0 minutes (decision already made)

**What I predict:** By the time Indy implements SpacialFrameTest, the SpacialBoxTest implementation will have already resolved the tree view question (add FoundryMicroCore.Blazor.Controls reference, or remove the panel). SpacialFrameTest will just follow that precedent.

**The integration seam:** Both pages have the same broken tree view component. The decision made for SpacialBoxTest is the decision for SpacialFrameTest.

**What would prove me wrong:** The two specs are implemented independently by different Indy instances that don't share context. Or Sully makes a different decision for SpacialFrameTest specifically.

---

### Prediction 2: Direct @bind to _model.Property Will Work But @bind:after to _model.Method May Need a Lambda Wrapper

**Confidence:** 🟡 Medium (50%)  
**Category:** Integration seam  
**Cost if it hits:** 15-30 minutes (wrap each @bind:after in a lambda or add code-behind delegates)

**The integration seam:** The spec proposes `@bind:after="_model.AutoRefreshShape"` for direct method binding from razor to model. Standard `@bind:after` accepts `Action` or `Func<Task>`. `_model.AutoRefreshShape` is a public void method on the model.

**What I checked:**
- Blazor `@bind:after` accepts method group references that match `Action` or `Func<Task>`
- The method group `_model.AutoRefreshShape` should resolve to `Action` since it returns void
- But: Blazor compiler sometimes struggles with method groups on non-component objects

**What I predict:** Either:
- It works (50% — standard Blazor behavior)
- Blazor compiler emits "cannot convert method group to EventCallback" or similar, requiring `@bind:after="() => _model.AutoRefreshShape()"` lambda syntax

**What would prove me wrong:** It just works as written with no compiler complaints. Or if Blazor .NET 10 has specific improvements for method group references on non-component objects.

---

## Spec Gap Predictions

### Prediction 3: Sully Will Sequence These Pages — SpacialBoxTest First, Then SpacialFrameTest

**Confidence:** 🟡 Medium (60%)  
**Category:** Spec gap  
**Cost if it hits:** 0 (actually helps — lessons from BoxTest improve FrameTest)

**The gap:** The spec doesn't address implementation ordering. Both specs exist independently. But they share the same blockers (tree view dependency, model extraction pattern).

**What I predict:** Sully will say "do SpacialBoxTest first, then apply the lessons to SpacialFrameTest." This is the smart approach — BoxTest is more complex (animations, dirty flags), so it stress-tests the pattern harder. FrameTest then becomes a straightforward repeat.

**What would prove me wrong:** Sully wants both done simultaneously, or FrameTest first (simpler = faster validation).

---

### Prediction 4: The Spec Doesn't Address the Commented-Out Scale Controls Properly

**Confidence:** 🔴 Low (30%)  
**Category:** Spec gap  
**Cost if it hits:** 5 minutes

**The gap:** The razor has Scale controls commented out with `@* ... *@`. The spec says "preserve disabled state." But when Indy moves ScaleX/Y/Z properties to the model, the commented-out razor markup still references the old `ScaleX` (not `_model.ScaleX`). If someone uncomments the Scale section later, it won't compile because the code-behind no longer has those properties.

**What I predict:** Indy will either:
1. Update the commented-out markup to reference `_model.ScaleX` (correct but unnecessary effort for dead code)
2. Leave it as-is (broken if uncommented, but it's commented out so doesn't matter)
3. Remove the commented-out code entirely

Option 2 is most likely. Option 3 would be cleaner. Neither is wrong.

**What would prove me wrong:** Sully says "uncomment it and wire it up" — then ScaleX needs proper model binding. But the spec explicitly says not to uncomment it.

---

## Implementer Behavior Predictions

### Prediction 5: Model Extraction Will Be Faster Than SpacialBoxTest

**Confidence:** 🟢 High (80%)  
**Category:** Implementer behavior  
**Cost if it hits:** 0 (positive prediction)

**What I predict:** SpacialFrameTest is simpler than SpacialBoxTest (445 lines vs 573, no animations, no dirty flags, no submarine). If Indy has already done SpacialBoxTest, the pattern is proven. SpacialFrameTest will take 60-90 minutes vs 2-3 hours for BoxTest.

Even if SpacialFrameTest goes first, it's simpler code with a cleaner extraction surface. The increment methods are repetitive, the visualization methods are copy-paste from BoxTest's pattern.

**Evidence:** The code is mechanically simpler:
- No Tweener complexity
- No dirty flag testing
- No `IsDirty` / `IsStale()` confusion
- Straightforward clear-and-recreate pattern (no update-existing logic)

**What would prove me wrong:** The transform binding (`@bind` to model properties) creates unexpected Blazor compilation issues that take time to debug. This is the only complex aspect of this page.

---

## Framework / API Predictions

### Prediction 6: All APIs Will Work First Try — No Surprises

**Confidence:** 🟢 High (90%)  
**Category:** Framework / API  
**Cost if it hits:** 0

**What I predict:** Every API call in this page is identical to SpacialBoxTest or already verified. `SpacialFrame3D(shape, "m")`, `GetVertices()`, `GetEdges()`, `GetFaces()`, `CreateBox()`, `Transform3`, `Euler.FromDegrees` — all battle-tested.

**What would prove me wrong:** A versioning change to one of these APIs since the last build. Or a subtle behavioral difference in how `SpacialFrame3D.GetVertices()` handles transforms compared to `SpacialBox3D.GetLocalVertices()`. But the existing page works, and we're just moving the code.

---

## Runtime / Environment Predictions

### Prediction 7: Auto-Refresh Pattern (Clear + Recreate Every Input Change) Will Produce Brief Visual Flicker

**Confidence:** 🟡 Medium (40%)  
**Category:** Runtime / environment  
**Cost if it hits:** 0 (pre-existing behavior, not caused by modernization)

**What I predict:** When the user types in a Position/Rotation/Pivot input, `@bind:after` fires `AutoRefreshShape()` which calls `_frameStage.ClearAll()` then recreates the shape. This clear-recreate on every keystroke might cause a visible flicker as the shape disappears and reappears.

**Important:** This is the EXISTING behavior — not caused by the modernization. It will be exactly as noticeable (or not) as it is today. I include this prediction only to document it — not as a risk.

**What would prove me wrong:** The rendering pipeline batches the clear+add so it appears atomic. Or the input `@bind` only fires on blur/Enter (not every keystroke), so the clear-recreate happens less frequently.

---

## What I'm NOT Predicting (Proven Non-Risks)

- ~~"Will Arena work?"~~ — Always works.
- ~~"Will Indy follow the spec?"~~ — Yes.
- ~~"Will Canvas3D initialize?"~~ — Same as SpacialBoxTest.
- ~~"Will Indy get method signatures wrong?"~~ — Not if the spec provides them.
- ~~"Will the page route change?"~~ — `/spacialframetest` stays. Already in NavMenu and Showcase.

---

## Meta-Prediction

**What I learned from writing SpacialBoxTest predictions:** The most valuable prediction was #1 (SceneTreePanel project reference missing) — it caught a real spec gap. The implementation behavior predictions (#6, #7) were probably the least valuable — they predict what Indy will or won't do, which has historically been inaccurate.

**For this page:** I've weighted toward integration seams (#1, #2) and spec gaps (#3, #4) rather than implementer behavior. The one implementer prediction (#5) is positive ("this will go smoothly") which aligns with the checklist's guidance that positive infrastructure predictions are Atlas's best category.

---

*Predictions written following the Atlas Prediction Checklist v1.0 (February 10, 2026). Budget allocation: Integration 29%, Spec gaps 29%, Implementer 14%, Framework 14%, Runtime 14%.*
