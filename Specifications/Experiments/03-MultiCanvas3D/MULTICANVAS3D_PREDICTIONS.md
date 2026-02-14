# Multi-Canvas 3D Test — Atlas Predictions for Indy's Implementation

**Architect:** Claude "Atlas"  
**Date:** February 9, 2026  
**Purpose:** Pre-mortem predictions for how Indy will experience implementing from MULTICANVAS3D_COMPONENT_SPECIFICATION.md

---

## How to Use This Document

**Indy:** After you finish implementing (or attempting to implement) the Multi-Canvas 3D Test page, come back here and score each prediction. Mark each one:
- ✅ **Correct** — Atlas predicted this accurately
- ❌ **Wrong** — Atlas was off-base
- 🔶 **Partially** — Some truth but not the full picture
- ➖ **N/A** — Didn't come up

Then write a brief **After-Action Summary** at the bottom. This feedback loop helps Atlas write better specs and predictions for future features.

---

## What's Different About This Spec

This is the first specification Atlas has written where **the complete working implementation already exists and is included inline**. Appendix A of the spec contains both files — `MultiCanvas3DTest.razor` and `MultiCanvas3DTest.razor.cs` — copy-paste ready. This was Indy's #1 request from the Tug of War AAR.

This fundamentally changes the prediction landscape. The primary risk is no longer "will Indy discover the right APIs?" — the APIs are all in the appendix with verified, running code. The risks shift to:

1. **Will Indy actually use the appendix**, or will they read the spec body and try to build from patterns?
2. **Will the environment differ** enough that copy-paste doesn't just work?
3. **Will Indy resist the urge to "improve"** the working code?

---

## Overall Confidence Assessment

**Atlas's overall prediction:** Indy will achieve **90-95% success on first pass**. This is the highest confidence rating Atlas has ever given, and it's entirely because the spec includes a complete, tested, working implementation.

**Predicted total time:** 30-60 minutes. Most of that is reading the spec, copying files, and verifying. If Indy starts from the appendix, this could be done in 15 minutes.

**Predicted outcome:** A fully working page with all three canvases rendering independently. The most likely deviation is Indy "improving" something that didn't need improving.

> **Indy's score:** ___/10 (fill in after implementation)  
> **Actual time:** ___ hours  
> **First-pass compilation:** ☐ Clean ☐ 1-5 errors ☐ 5-15 errors ☐ 15+ errors

---

## Prediction 1: Indy Will Start from the Appendix (Not the Spec Body)

**Confidence:** 🟢 High (80% likely)

The spec body explains architecture, code paths, and golden patterns. The appendix has the two complete files. I predict Indy will skim the spec body for context, then go straight to Appendix A and copy both files.

**Why this matters:** If Indy starts from the appendix, compilation will succeed on first attempt. If Indy starts from the spec body and tries to assemble from fragments, they'll likely miss something (a using statement, a null check, the `_shapesAdded` flag).

**What I think will happen:** Indy copies the files, builds, gets zero errors. This is the happy path.

**Risk if wrong:** If Indy doesn't use the appendix (maybe they want to "understand before copying"), they'll spend 20-40 minutes assembling what's already done.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 2: The Scene Activation Bug Will NOT Bite Indy

**Confidence:** 🟢 High (90% likely)

The biggest technical challenge in multi-canvas — exclusive scene activation via `SetActiveStage()` — was **already fixed** (Feb 9, 2026) before this spec was written. `Canvas3DComponent` now uses `scene.ForceActive()` instead. Indy's copy of the code will work with the fixed library.

**What I think will happen:** All three canvases render shapes in their own scenes. No cross-contamination. Indy never encounters the exclusive activation problem because it's already been patched in the platform.

**Small risk (10%):** If Indy is working from a stale build of `FoundryWorldsAndDrawings.dll` that still has the old `SetActiveStage()` in `Canvas3DComponent`, the original bug will resurface. The spec's Troubleshooting Guide covers this, but Indy would need to rebuild the library.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 3: Indy Will Be Tempted to "Improve" the Animation Pattern

**Confidence:** 🟡 Medium (55% likely)

The working code uses time-based animation: `var dt = 1.0 / Math.Max(fps, 1)`. The Golden Pattern section warns against adding guards but doesn't say "don't switch to frame-based." I predict Indy will notice the `Math.Max(fps, 1)` defense and think:

- "What if I just use frame-based angle increments? Simpler and more robust."
- Or: "I should add `SetTransformStale()` calls after rotation — the spec's Code Smells section mentions it."

**What I think will happen:** 

**Scenario A (45%):** Indy copies verbatim. Everything works. No issue.

**Scenario B (40%):** Indy switches to frame-based animation (`angle += 0.01` per frame). This actually works fine — it just changes the rotation speed. The spheres' bounce timing will differ because `tick / fps` gives elapsed seconds while a frame counter doesn't. The visual result is acceptable but not identical to the reference.

**Scenario C (15%):** Indy adds `SetTransformStale()` after each `Transform.Rotation =` assignment. This is harmless (setting rotation already triggers staleness internally), but it shows Indy didn't fully trust the pipeline.

**None of these are bugs.** The worst outcome is "animation looks slightly different from reference" which is fine for a test page.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 4: The `Task.Delay(500)` Will Work on Indy's Machine

**Confidence:** 🟡 Medium (70% likely)

The spec includes `await Task.Delay(500)` to wait for all three canvases to initialize. This is a known fragile pattern. On the dev machine (where the code is running now), 500ms is sufficient.

**What I think will happen:**

**Scenario A (70%):** 500ms is enough. All three stages are non-null. Everything works.

**Scenario B (20%):** One stage is null. Console shows "Stage X not ready". Indy increases delay to 1000ms. Problem solved in under a minute.

**Scenario C (10%):** Indy decides to replace `Task.Delay` with a poll loop:
```csharp
while (_canvasA?.Stage == null) await Task.Delay(100);
```
This is actually a better pattern but adds complexity the reference didn't need.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 5: The UnifiedTreeView Will Cause a Compile Error

**Confidence:** 🟡 Medium (40% likely)

The tree view component `<UnifiedTreeView RootNode="@_arenaNode" ShowTypeNames="true" ShowBadges="true" />` requires `@using FoundryMicroCore.Blazor.Controls.Components.TreeView`. This is included in the appendix, but:

- The component might have been renamed or moved since the code was written
- `ShowTypeNames` and `ShowBadges` parameters might not exist on the current version
- `ITreeNode` might resolve to the wrong interface if multiple assemblies define it

**What I think will happen:**

**Scenario A (60%):** Everything compiles. UnifiedTreeView shows the arena hierarchy.

**Scenario B (30%):** `ShowTypeNames` or `ShowBadges` doesn't exist. Indy removes the parameters, tree still renders (just without those display features).

**Scenario C (10%):** `UnifiedTreeView` component not found. Indy comments out the tree view section. Page works with three canvases and no tree view. Acceptable for the test.

**This is the most likely first compile error**, if there is one. Everything else in the appendix uses core FoundryWorldsAndDrawings APIs that are battle-tested.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 6: Indy Will NOT Encounter the Vector2 Ambiguity

**Confidence:** 🟢 High (95% likely)

The Plugin710 project had `Vector2` ambiguity between `FoundryWorldsAndDrawings.ThreeD.Maths.Vector2` and `System.Numerics.Vector2`. The Multi-Canvas 3D Test only uses `Vector3` and `Euler` — no `Vector2` anywhere. This won't be an issue.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 7: Indy Will Add Something Not In the Spec

**Confidence:** 🟢 High (75% likely)

This is the implementer behavior prediction. Indy will read the spec, see three scenes with basic shapes, and want to add something extra to demonstrate mastery. I predict at least one of:

- **Camera controls** — try to set different camera angles per canvas
- **An extra scene** — add a 4th canvas or replace the tree view with a canvas
- **Shape interaction** — add click handlers or hover effects
- **More shapes** — additional geometry types (icosahedron, torusknot, capsule)
- **Console logging** — add more WriteInfo/WriteSuccess calls for debugging

**None of these are harmful.** But they increase the surface area for bugs. The spec is deliberately minimal — 8 shapes across 3 scenes — because the goal is proving multi-canvas isolation, not shape variety.

**My prediction:** Indy adds 1-2 extra shapes or a slightly different animation, and it works fine. No debugging cost.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 8: The Hardest Bug (If Any) Will Be in Disposal

**Confidence:** 🟡 Medium (35% likely)

The spec's `Dispose()` just logs a message. The actual cleanup is handled internally by `Canvas3DComponent`. But if Indy navigates away and back, or if the browser reconnects, there's a chance of:

- Duplicate stages (SceneA already exists in the arena from the previous visit)
- Orphaned JavaScript viewers (container div gone, but JS viewer still in ViewerLookup)
- `ObjectDisposedException` on animation callback after navigation

**What I think will happen:**

**Scenario A (65%):** Indy implements, tests, navigates away, comes back. Everything works because `EstablishStage` is idempotent (reuses existing stage).

**Scenario B (25%):** Indy navigates away then back, and gets duplicate shapes (old ones + new ones) because the stage wasn't cleared. This is visible but not a crash.

**Scenario C (10%):** Indy hits an `ObjectDisposedException` in the console. Annoying but doesn't break functionality.

**If this happens:** The fix is to clear the stage in `OnAfterRenderAsync` before setting up shapes, or check if shapes already exist.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 9: Indy Will Question the `arena.GetAllStages()` Approach in `UpdateCounts`

**Confidence:** 🟡 Low-Medium (30% likely)

The spec uses `arena.GetAllStages().Count` and `Sum(s => s.AllBodies().Count())` for the UI counters. If the arena has stages from OTHER pages (e.g., a previously visited ClockDemo), the count will be wrong — it'll include those stages too.

**What I think will happen:** This only triggers if Indy visits another 3D page before coming to Multi-Canvas. In a fresh session, there are exactly 3 stages. Indy probably won't notice this subtlety.

**If they do notice:** They might filter to only stages named "SceneA", "SceneB", "SceneC". That's a valid improvement.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 10: This Will Be the Fastest Spec-to-Implementation Atlas Has Seen

**Confidence:** 🟢 High (85% likely)

The previous specs (ClockDemo, Tug of War, Shape Lifecycle) all required Indy to assemble code from patterns and fragments. This spec includes the complete, tested, running implementation as an appendix.

**My prediction:** Indy will go from "reading spec" to "running page with 3 canvases" in **under 30 minutes**. The majority of that time will be reading and understanding, not debugging.

**Why I'm 15% uncertain:** Machine differences, library version drift, or Indy choosing to build from scratch rather than copy could all slow things down. Also, if the project has changed since the spec was written (new dependencies, moved files, renamed namespaces), the copy-paste won't be perfectly clean.

**This is the meta-prediction:** We're testing whether "give Indy the complete file" is the slam-dunk strategy that the Tug of War AAR suggested it would be.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction Summary Table

| # | Prediction | Confidence | Predicted Impact |
|---|---|---|---|
| 1 | Indy starts from Appendix (not spec body) | 🟢 80% | Determines total time |
| 2 | Scene activation bug won't bite | 🟢 90% | 0 min (already fixed) |
| 3 | Indy tempted to "improve" animation | 🟡 55% | 0-10 min |
| 4 | Task.Delay(500) works | 🟡 70% | 0-2 min if not |
| 5 | UnifiedTreeView causes first compile error | 🟡 40% | 2-5 min |
| 6 | No Vector2 ambiguity | 🟢 95% | 0 min |
| 7 | Indy adds something extra | 🟢 75% | 0-10 min |
| 8 | Hardest bug (if any) is disposal/navigation | 🟡 35% | 0-15 min |
| 9 | GetAllStages count includes other pages | 🟡 30% | 0-5 min |
| 10 | Fastest spec-to-implementation ever | 🟢 85% | < 30 min total |

**Total predicted debugging overhead:** 0-30 minutes. This is dramatically lower than Tug of War (1.5-3 hours predicted, 3 days actual) because the complete code is provided.

---

## The Meta-Experiment

This predictions document is testing two hypotheses:

### Hypothesis 1: Complete Code Eliminates API Discovery Problems
Indy's Tug of War feedback: "What I needed was 10 lines. Copy them. Don't add anything." This spec provides ~220 lines. If Prediction 10 is correct, the "complete file" strategy works and should become standard for all future specs.

### Hypothesis 2: Implementer Behavior Risks Remain Even With Complete Code
Even with perfect code in hand, I predict Indy will be tempted to modify it (Predictions 3, 7). If these predictions hit, it means **the spec's job isn't just to provide correct code — it's to provide correct code AND a compelling reason not to change it.**

The spec tries to address this with the Golden Pattern's ❌ DON'T list and the Implementer Behavior Warnings section. We'll see if that's enough.

---

## Questions for Indy's After-Action Review

1. **Did you start from Appendix A, or did you build from the spec body?**

2. **What was the FIRST thing that didn't work?** (If anything)

3. **Did you change any of the provided code? If so, why?**

4. **How long from "reading spec" to "page running with 3 canvases"?**

5. **Was the spec body (architecture analysis, code paths, golden pattern) useful, or did you skip it?**

6. **On a scale of 1-10, how well did the spec prepare you?**

7. **Should all future specs include the complete implementation as an appendix?**

8. **What's ONE thing that would have made this even easier?**

---

## Atlas's Self-Assessment

**What I did well:**
- Included complete, tested, copy-paste implementation (learning from Tug of War AAR)
- Every method name verified against source code
- Predictions focused on implementer behavior (not framework risks) per Indy's feedback
- Low prediction count (10 vs Tug of War's 10) because risk surface is much smaller

**What I'm uncertain about:**
- Whether the UnifiedTreeView component and its parameters still exist as-written
- Whether `Task.Delay(500)` is universally sufficient across machines
- Whether Indy will have the ForceActive fix in their build of FoundryWorldsAndDrawings

**What I would do differently next time:**
- Nothing, actually. If this works, the "complete file + predictions focused on implementer behavior" formula is the template for all future specs. If it doesn't work, Indy's AAR will tell me exactly what to change.

---

*This prediction document should be reviewed by Indy after implementation is complete. Atlas will use the feedback to calibrate future specifications.*
