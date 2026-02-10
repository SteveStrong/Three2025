# Multi-Canvas 2D Test — Atlas Predictions for Indy's Implementation

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Spec:** `MULTICANVAS2DTEST_SPECIFICATION.md`  
**Purpose:** Pre-mortem predictions focused on whether Indy can locate the right information and use it correctly

---

## How to Use This Document

**Indy:** After you finish implementing (or attempting to implement) the Multi-Canvas 2D Test page, come back here and score each prediction. Mark each one:
- ✅ **Correct** — Atlas predicted this accurately
- ❌ **Wrong** — Atlas was off-base
- 🔶 **Partially** — Some truth but not the full picture
- ➖ **N/A** — Didn't come up

Then write a brief **After-Action Summary** at the bottom.

> **Indy's score:** ___/10 (fill in after implementation)  
> **First-pass compilation:** ☐ Clean ☐ 1-5 errors ☐ 5-15 errors ☐ 15+ errors

---

## What's Different About This Spec

Unlike the MultiCanvas3D spec which included a complete appendix, this spec does NOT include copy-paste-ready files. Instead, it provides:

1. **A clear modern reference** (ClockDemo) and explains how to adapt it for 2D
2. **Code fragments** for each page's setup method (verified signatures)
3. **A legacy implementation** (the existing MultiCanvas2DTest) that shows WHAT but not HOW

This means Indy must **assemble** the implementation from the spec's fragments, guided by the ClockDemo pattern. The primary risk isn't "will the APIs work" — it's "will Indy correctly translate a 3D reference (ClockDemo) into a 2D implementation, and will Indy follow the spec rather than the legacy code that's sitting right there."

---

## Prediction Budget (Per Checklist Guidance)

| Category | Budget | Predictions |
|---|---|---|
| Framework risks | 10% | #8 |
| Implementer behavior | 40% | #3, #4, #5, #7 |
| Information location accuracy | 30% | #1, #2, #6 |
| Runtime behavior / environment | 10% | #9 |
| Meta-outcomes | 10% | #10 |

---

## Prediction 1: Indy Will Use the Legacy Code-Behind as a Starting Point (Not ClockDemo)

**Confidence:** 🟡 Medium (60% likely)

**The information location problem:** Two competing examples exist:
- The **legacy** `MultiCanvas2DTest.razor.cs` — sitting right there, already working, 2D, same feature
- The **modern** `ClockDemo.razor.cs` + `ClockDemoModel.cs` — correct pattern, but it's 3D

Indy will face a fork: start from the legacy code that already does the right THING, or start from the modern code that does things the right WAY. Both are in the workspace. The spec says "use ClockDemo" but the path of least resistance is "fix what already exists."

**What I think will happen:**

**Scenario A (60%):** Indy opens the legacy `MultiCanvas2DTest.razor.cs`, sees working 2D code, and starts refactoring it into the Model-behind pattern. This works but means Indy is fighting legacy habits — `Task.Run`, shared mutable state, `public required` injection — and must actively replace them.

**Scenario B (40%):** Indy starts fresh from ClockDemo, copies the code-behind and model, and adapts for 2D. This is cleaner but requires translating 3D concepts (arena/stage) to 2D concepts (drawing/page).

**Why this matters:** Scenario A produces a working page faster but may carry legacy anti-patterns. Scenario B follows the spec but requires more cognitive translation.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 2: Indy Will Get the 2D OnBeforeRender Signature Wrong on First Try

**Confidence:** 🟡 Medium (50% likely)

**The information location problem:** The spec says the 2D signature is `Action<FoGlyph2D, int>` — two parameters `(shape, tick)`. The ClockDemo reference (which Indy is told to copy) uses the 3D signature `Action<FoGlyph3D, int, double>` — three parameters `(shape, tick, fps)`.

If Indy copies ClockDemo and changes `FoGlyph3D` to `FoGlyph2D` mechanically, the lambda will have three parameters where only two are expected. This is a compile error — easy to fix — but it reveals whether Indy read the spec's "Known Gotchas" section or just did a type-rename.

**What I think will happen:**

**Scenario A (50%):** Indy writes `(shape, tick, fps)` → gets compile error → fixes in 30 seconds. The spec's gotcha section covers this, and the compiler error is clear.

**Scenario B (35%):** Indy reads the Golden Pattern section first and uses `(shape, tick)` from the start. No error.

**Scenario C (15%):** Indy copies from the legacy code (which already uses 2D signatures) and gets it right by accident, not because of the spec.

**Predicted cost if wrong:** Under 1 minute. The compiler catches this immediately.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 3: Indy Will Struggle With the `PageName` vs `SceneName` Discrepancy

**Confidence:** 🟡 Medium (45% likely)

**The information location problem:** The spec flags this as a CRITICAL DISCREPANCY — the legacy `.razor` uses `PageName="PageA"` but that parameter doesn't exist on `Canvas2DComponent`. Only `SceneName` exists.

But here's the problem: Indy has the legacy `.razor` file in the workspace AND in the spec's context. If Indy copies the razor layout from the legacy file (reasonable — it has the grid, the borders, the labels), they'll copy `PageName` too.

**What I think will happen:**

**Scenario A (45%):** Indy copies the legacy razor, includes `PageName="PageA"`, gets a compile error about unrecognized parameter. Searches spec for "PageName", finds the discrepancy warning. Fixes to `SceneName`. Cost: 2-5 minutes.

**Scenario B (40%):** Indy reads the spec's razor template (Step 6) which correctly uses `SceneName`. No error.

**Scenario C (15%):** Indy copies legacy razor verbatim. `PageName` gets silently ignored as an unmatched `[Parameter]` (Blazor allows extra attributes on components). Canvas doesn't bind to the right page. Shapes don't appear. Indy debugs for 10-20 minutes before finding the cause. **This is the worst-case scenario.**

**Why this is an information location prediction:** The answer is in the spec (Section 3, Section 8). The question is whether Indy reads those sections before hitting the problem or only after.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 4: Indy Will Over-Engineer the Build/Auto-Build Pattern

**Confidence:** 🟡 Medium (40% likely)

**The implementer behavior problem:** The spec says to call `_model.BuildAllPages()` in `OnInitialized`. Simple. But ClockDemo's model has `[ModelComponent(AutoRunAction = "BuildAndStart")]` and `[DiscoverableComponent(...)]` attributes.

If Indy studies ClockDemoModel closely (as the spec directs), they'll see these attributes and wonder: "Should MultiCanvas2DTestModel also have these?" The spec doesn't use them. But ClockDemo does. And the spec says "copy ClockDemo."

**What I think will happen:**

**Scenario A (40%):** Indy adds `[DiscoverableComponent]` and `[ModelComponent]` attributes. These won't hurt if the discovery system ignores them for 2D, but they might trigger unexpected auto-run behavior.

**Scenario B (45%):** Indy follows the spec's explicit model code and skips the attributes. Correct path.

**Scenario C (15%):** Indy adds the attributes AND tries to wire up `AutoRunAction`, leading to pages being built twice (once via auto-run, once via explicit `BuildAllPages()` in `OnInitialized`). Results in duplicate shapes.

**Predicted cost if wrong:** 5-15 minutes debugging duplicate shapes or unexpected command execution.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 5: Indy Will Add Canvas `@ref` Fields Despite the Spec Warning

**Confidence:** 🟡 Low-Medium (35% likely)

**The implementer behavior problem:** Every 3D page (including ClockDemo) stores a `Canvas3DComponent? _canvasComponent` ref. The legacy 2D code doesn't use refs because Canvas2DComponent self-manages. The spec explicitly warns: "DON'T add canvas @ref fields."

But Indy's muscle memory from studying ClockDemo says "Canvas component → store a ref." And the DualCanvas2D3DTest in the workspace stores both `Canvas2DComponent?` and `Canvas3DComponent?` refs.

**What I think will happen:**

**Scenario A (65%):** Indy follows the spec and omits canvas refs. Correct.

**Scenario B (25%):** Indy adds `@ref` fields, doesn't use them for anything, they sit harmlessly. No bug, just unnecessary code.

**Scenario C (10%):** Indy adds `@ref` fields and tries to use them to manually set pages on the canvases (e.g., `_canvasA.SetManagedPage(page)`), creating a race condition with the canvas's own page discovery. Pages might render wrong content.

**Predicted cost if wrong:** 0 minutes (Scenario B) to 15 minutes (Scenario C).

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 6: Indy Will Correctly Locate the FoShape1D Glue Pattern

**Confidence:** 🟢 High (80% likely)

**The information location test:** The spec provides the exact glue pattern:
```csharp
var connector = new FoShape1D("Arrow", "cyan");
connector.Height = 6;
connector.GlueStartTo(box1, "RIGHT");
connector.GlueFinishTo(box2, "LEFT");
page.AddShape(connector);
```

This is also present in the legacy code (`_connectorC`). Two sources agree. The API reference confirms the signatures. This is the ONE pattern where legacy code and spec code align perfectly.

**What I think will happen:** Indy copies this almost verbatim. It works first time. The only variation might be `Height` value (legacy uses 50, spec uses 6).

**The subtle risk (20%):** Indy adds the connector to a DIFFERENT page than the boxes, or adds it before the boxes are added. The spec's troubleshooting guide covers this but the failure mode is "invisible connector" which is hard to debug visually.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 7: Indy Will Reintroduce the `Task.Delay` Timing Pattern

**Confidence:** 🟡 Medium (40% likely)

**The implementer behavior problem:** The spec says "create pages in OnInitialized — canvases find them later." But the legacy code uses `Task.Delay(100)` and the DualCanvas2D3DTest uses `Task.Delay(200)`. If Indy's pages don't show shapes on first load, the natural instinct is "I need to wait for the canvases."

**What I think will happen:**

**Scenario A (50%):** Pages in `OnInitialized` works perfectly. Canvases find them. No timing issue. Spec is proven correct.

**Scenario B (30%):** Pages in `OnInitialized` technically works but shapes appear a beat late (one frame delay while canvas discovers its page). Indy considers this fine.

**Scenario C (20%):** Shapes don't appear on first load. Indy adds `Task.Delay` in `OnAfterRender`, mirroring the legacy pattern. This works but defeats the spec's cleaner architecture. The spec's fallback note anticipated this: "If pages don't appear, try moving BuildAllPages to OnAfterRender."

**Why this is an information location prediction:** If Scenario C hits, the spec tells Indy what to do. But will Indy find that note before spending 15 minutes experimenting?

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 8: The Drawing Service Singleton Will Handle Multiple Pages Correctly

**Confidence:** 🟢 High (85% likely)

**The framework risk:** `IDrawing` is a singleton. Three Canvas2DComponents each call `drawing.EstablishPage()` or find their page by `SceneName`. The infrastructure assumption is that multiple pages coexist in one drawing and each canvas renders only its managed page.

**What I think will happen:** This works. The 2D multi-page architecture is older and more battle-tested than the 3D multi-stage work. `Canvas2DComponent` already uses `ManagedPage` for per-canvas rendering.

**The 15% risk:** If two canvases try to `SetActivePage()` exclusively (like the 3D exclusive activation bug), only the last canvas's page renders. But the source audit shows `Canvas2DComponent` uses `ManagedPage` directly in `RenderPage()`, bypassing the active-page concept for rendering. Only focus/interaction uses active page.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 9: Console Will Be Clean (No Spam) for 30 Seconds

**Confidence:** 🟢 High (80% likely)

**The runtime behavior prediction:** The spec uses frame-based animation with no logging inside callbacks. Unlike the MultiCanvas3D spec (which had Euler overflow warnings), 2D shape animation doesn't involve Euler angles or 3D transform stale flags.

**What I think will happen:** Initialization messages appear, then silence. The 2D render loop is simpler than 3D — no JavaScript interop per frame, no mesh regeneration, no boundary recomputation.

**The 20% risk:** If Indy adds debugging `WriteInfo()` calls inside OnBeforeRender callbacks during development and forgets to remove them, console will spam at 60 messages/second/shape = 360+ messages/second across 6 animated shapes.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 10: The Model-Behind Translation Is The Real Test

**Confidence:** 🟢 High (90% likely)

**The meta-prediction:** This spec's real challenge isn't any individual API or pattern — it's the translation task. Indy must:

1. Study a **3D** model reference (ClockDemoModel) and adapt it for **2D**
2. Ignore a **working legacy** implementation that's in the same directory
3. Assemble from **fragments** rather than copy a **complete file**

I predict the overall implementation will succeed, but the path will reveal where spec fragments are insufficient vs. where a complete appendix would have saved time.

**My prediction for outcome:** Indy produces a working page with all three animated canvases in **1-2 hours**. The first 30 minutes will be reading and orienting. The next 30-60 minutes will be writing the model and code-behind. The remaining time will be CSS and troubleshooting.

**Compared to MultiCanvas3D (which had a complete appendix):** This will take 2-3x longer because Indy must assemble rather than copy. But the Model-behind pattern will be cleaner because Indy is building it fresh rather than pasting and modifying.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction Summary Table

| # | Prediction | Category | Confidence | Predicted Cost |
|---|---|---|---|---|
| 1 | Indy starts from legacy code (not ClockDemo) | Info location | 🟡 60% | 0-20 min extra |
| 2 | 2D OnBeforeRender signature wrong initially | Info location | 🟡 50% | <1 min |
| 3 | `PageName` vs `SceneName` discrepancy bites | Info location | 🟡 45% | 2-20 min |
| 4 | Over-engineers build/auto-build attributes | Implementer | 🟡 40% | 5-15 min |
| 5 | Adds unnecessary canvas `@ref` fields | Implementer | 🟡 35% | 0-15 min |
| 6 | Gets FoShape1D glue correct | Info location | 🟢 80% | 0 min |
| 7 | Reintroduces `Task.Delay` timing | Implementer | 🟡 40% | 0-15 min |
| 8 | Drawing singleton handles multi-page fine | Framework | 🟢 85% | 0 min |
| 9 | Console clean for 30 seconds | Runtime | 🟢 80% | 0-2 min |
| 10 | Model translation is main challenge; 1-2 hrs | Meta | 🟢 90% | — |

**Total predicted debugging overhead:** 15-45 minutes (assembly + translation cost)

---

## The New Prediction Focus: Information Location

Per Sully's guidance, these predictions focus on **whether Indy can find and use the right information** rather than how long individual steps take. The three information location predictions (#1, #2, #3) all test the same underlying question:

**When the spec and the workspace contain conflicting information, which does Indy follow?**

- The spec says `SceneName` → legacy code says `PageName` (#3)
- The spec says `(shape, tick)` → ClockDemo reference says `(shape, tick, fps)` (#2)
- The spec says "start from ClockDemo" → legacy code is right there and already 2D (#1)

If Indy follows the spec over workspace evidence in all three cases, every prediction resolves favorably. If Indy follows workspace evidence over the spec, predictions #1, #2, and #3 all hit their worst-case scenarios.

**The core hypothesis:** A well-written spec creates enough authority that Indy follows it even when competing evidence exists in the workspace. The MultiCanvas3D spec tested "complete file is fastest." This spec tests "clear instructions with verified fragments are sufficient."

---

## Questions for Indy's After-Action Review

1. **Where did you start?** Legacy code, ClockDemo, or the spec's code fragments?

2. **Did you read the full spec before coding, or did you start coding after the first few sections?**

3. **Which section of the spec was most useful?** (Golden Pattern? Implementation Steps? Troubleshooting?)

4. **Did the `PageName` vs `SceneName` warning save you time, or did you not encounter it?**

5. **Did you use `Task.Delay` anywhere? If so, why?**

6. **What information was MISSING that you had to discover yourself?**

7. **On a scale of 1-10, how well did the spec prepare you vs. the MultiCanvas3D spec (which had complete files)?**

8. **Should this spec have included complete files as an appendix, or were the fragments sufficient?**

---

*This prediction document should be reviewed by Indy after implementation is complete. Atlas will use the feedback to calibrate the "fragments vs. complete files" question for future specifications.*
