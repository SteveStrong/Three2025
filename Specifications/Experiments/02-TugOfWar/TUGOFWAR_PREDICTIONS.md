# Tug of War — Atlas Predictions for Indy's Implementation

**Architect:** Claude "Atlas"  
**Date:** February 8, 2026  
**Purpose:** Pre-mortem predictions for how Indy will experience implementing from TUGOFWAR_SPECIFICATION.md

---

## How to Use This Document

**Indy:** After you finish implementing (or attempting to implement) the Tug of War page, come back here and score each prediction. Mark each one:
- ✅ **Correct** — Atlas predicted this accurately
- ❌ **Wrong** — Atlas was off-base
- 🔶 **Partially** — Some truth but not the full picture
- ➖ **N/A** — Didn't come up

Then write a brief **After-Action Summary** at the bottom. This feedback loop helps Atlas write better specs and predictions for future features.

---

## Overall Confidence Assessment

**Atlas's overall prediction:** Indy will achieve **75-85% success on first pass**, with the remaining 15-25% requiring investigation and adaptation due to library version drift and the inherent fragility of the timing-based initialization pattern.

**Predicted total time:** 4-6 hours including debugging (the spec says 3-5, but I'm adding a buffer for the inevitable version-drift surprises).

**Predicted outcome:** A working page with all major features functional, but likely 1-2 features that need workarounds or differ slightly from spec.

> **Indy's score:** ___/10 (fill in after implementation)  
> **Actual time:** ___ hours  
> **First-pass compilation:** ☐ Clean ☐ 1-5 errors ☐ 5-15 errors ☐ 15+ errors

---

## Prediction 1: The Naming Transition Will Bite

**Confidence:** 🟡 Medium (60% likely)

I predict Indy will encounter at least one method that has been renamed since this spec was written. The most likely candidates:

| Old Name (In Spec) | New Name (Maybe) | Risk |
|---|---|---|
| `BeforeAnimationRefresh()` | `OnBeforeRender()` | HIGH — This is the core animation hook |
| `ClearAnimationRefresh()` | `ClearBeforeRender()` | HIGH — Used in disposal and animation completion |
| `PreComputeMesh` (delegate) | `OnPreComputeMesh()` (fluent) | MEDIUM — Used on FoText3D |

**What I think will happen:** Indy will initially use the old names from the spec, get a compile error, search for the working examples in `Apprentice/FoClockFace3D.cs`, find the correct name, and fix it. This will cost 10-20 minutes.

**What I'm most worried about:** If BOTH old and new names exist as different methods with slightly different behavior, Indy could wire up the wrong one without realizing it. The animation would appear to work but have subtle timing differences.

**My mitigation in the spec:** I told Indy explicitly about the rename and said "use whichever compiles." I also pointed to `FoClockFace3D.cs` as ground truth. I think this is sufficient.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 2: The 3D Side Will Work Before the 2D Side

**Confidence:** 🟢 High (80% likely)

The 3D API reference is source-audited. The 2D API reference is explicitly documented as NOT source-audited. I predict:

1. **3D shapes, pipes, text** — will work on first or second attempt
2. **2D shapes (FoShape2D, FoText2D)** — will probably work fine (simple constructors)
3. **2D Tween animation** (`FoGlyph2D.Animations.Tween<T>`) — **this is where I expect trouble**
4. **FoShape1D wire with GlueStartTo/GlueFinishTo** — might have signature changes

**What I think will happen:** The 3D tug-of-war animation will be running and looking great, while Indy spends time debugging why the 2D tween isn't firing or the wire isn't connecting. The 2D side will eventually work but take disproportionate debugging time relative to its complexity.

**Why I'm worried about 2D Tween specifically:** The `FoGlyph2D.Animations.Tween<T>(target, new { PinX = value }, duration, delay)` pattern uses anonymous objects with reflection to find property names. If `PinX` was ever renamed or if the tween engine's property resolution changed, this would silently fail rather than throwing a compile error.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 3: Stale Flags Will Cause "Invisible Shape" Debugging

**Confidence:** 🟢 High (85% likely)

I predict Indy will have at least one episode where shapes exist in C# (verified via `AllBodies().Count`) but are invisible in the browser. The cause will be one of:

- Forgot `SetTransformStale()` after changing position
- Forgot `SetGeometryStale()` after changing pipe path
- Forgot `arena.RenderArena(0, 0)` for static shapes
- Forgot `SetRecomputeBoundary()` before animation callbacks

**What I think will happen:** Indy will follow the spec, shapes will appear for the simple `Add3Boxes` test (because the spec includes `RenderArena`), but the animated shapes might not initially appear because a stale flag was missed on one of the pipes or the text. Indy will add diagnostic logging, discover the issue, add the missing stale call, and move on. This will cost 15-30 minutes.

**My mitigation in the spec:** I highlighted stale flags as a code smell (Section 6), included `SetTransformStale()` and `SetGeometryStale()` in every relevant code sample (Section 3), and have a dedicated troubleshooting entry. I think the spec coverage is thorough, but the sheer number of places stale flags are needed makes it likely one gets missed.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 4: The Task.Delay Timing Will Need Adjustment

**Confidence:** 🟡 Medium (50% likely)

The spec says `await Task.Delay(100)` before accessing `Canvas3DReference.Stage`. The working `DebugCanvas.razor.cs` example uses 500ms. I predict:

**Optimistic scenario (50%):** 100ms works fine, no issues.  
**Pessimistic scenario (40%):** Stage is null on first access, Indy increases to 500ms, problem solved.  
**Worst case (10%):** Intermittent — works sometimes, fails sometimes, causing confusing behavior.

**What I think will happen:** If it fails, Indy will initially be confused about why stage is null despite following the spec exactly. The console diagnostic logging in the spec should quickly reveal the timing issue, and Indy will increase the delay. Total cost: 5-15 minutes.

**Deeper concern:** The Task.Delay pattern is fundamentally fragile. A better pattern would be a retry loop with exponential backoff, or an event-based notification from Canvas3DComponent when it's ready. I didn't include this in the spec because it would add complexity and the existing pattern works. But I suspect Indy will encounter this fragility and might propose a better approach — which would be a welcome improvement.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 5: The Double-Counted Animation Time Will Confuse

**Confidence:** 🟡 Medium (55% likely)

The TugOfWar implementation has a subtle issue: `_animationTime` is incremented in BOTH Box1's AND Box2's `BeforeAnimationRefresh` callbacks. I documented this in the spec as a "known issue (acceptable)" because progress is clamped at 1.0.

**What I think will happen (two scenarios):**

**Scenario A (40%):** Indy reads the spec, trusts the "acceptable" label, and reproduces it faithfully. Animation runs at 2x intended speed but completes correctly. No debugging time spent.

**Scenario B (55%):** Indy notices the timing feels off, investigates, discovers the double-counting, and either:
- (a) Fixes it by only incrementing in one callback — **correct fix, no side effects**
- (b) Tries to fix it and accidentally breaks the progress coordination between Box1 and Box2 — **30+ minutes of debugging**

**Scenario C (5%):** Indy doesn't notice at all. Animation works, just faster than intended.

**My worry:** If Indy "fixes" the double-count by moving time accumulation to the page-level `OnAnimationFrame`, they'll discover that the page-level callback fires at a different phase than the shape-level callbacks, introducing a frame-lag issue. The fix looks obvious but the execution is tricky.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 6: FoPipe3D Will Need the Most Iteration

**Confidence:** 🟢 High (75% likely)

The spec describes TWO different pipe patterns:
1. **Connecting pipe** — `FromShape3D`/`ToShape3D` + `CreatePipe()` — endpoints track shapes
2. **Path-based pipe** — `CreateTube()` with explicit `List<Vector3>` + `Path3D` replacement

I predict Indy will mix these up at least once. Specifically:

- Trying to set `Path3D` on a pipe created with `CreatePipe()` (might not work — `CreatePipe` uses endpoint shapes)
- Trying to set `FromShape3D`/`ToShape3D` on a pipe created with `CreateTube()` (won't auto-track)
- Forgetting that `SetGeometryStale()` is needed after EVERY `Path3D` change (position changes use `SetTransformStale`, path changes use `SetGeometryStale` — different flags!)

**What I think will happen:** The connecting tube between Box1 and Box2 will likely work (because the pattern is clear — set endpoints, createPipe). The growing pipe with path replacement is where iteration will concentrate. Indy might see the pipe appear at initial height but not grow, then realize `SetGeometryStale()` was being called too early or on the wrong object.

**My mitigation:** The inline code samples (3D and 3E) are explicit about which pattern to use for which pipe. The troubleshooting guide covers "Pipe Not Connecting / Not Growing." I think this is adequate, but the mental model of "two kinds of pipes" is a stumbling block that no amount of documentation fully eliminates — you have to experience it.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 7: Debug Controls (Pause/Step/Resume) Will Just Work

**Confidence:** 🟢 High (90% likely)

The `AnimationFrameBus` pause/step/resume API is straightforward static method calls. The spec provides exact method names and they're verified against the 3D API reference. I predict this will work on first or second attempt with minimal debugging.

**Small worry:** `TriggerSingleFrame()` is async. If Indy forgets the `await`, the step will fire but the UI won't reflect it immediately. The spec includes `await` in the sample code, but async/await mistakes are common.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 8: Indy Will Struggle with the ShapeDraw Custom Arrow

**Confidence:** 🟡 Medium (60% likely)

The 2D wire/arrow uses a custom `ShapeDraw` delegate:
```csharp
ShapeDraw = async (ctx, obj) => await DrawArrowAsync(ctx, obj.Width, obj.Height, obj.Color)
```

This requires `Blazor.Extensions.Canvas.Canvas2D.Canvas2DContext` and a custom drawing method. I predict:

**Scenario A (40%):** Indy copies the `DrawArrowAsync` method from the spec code sample and it compiles and works.

**Scenario B (50%):** The `Canvas2DContext` type comes from a NuGet package (`Blazor.Extensions.Canvas`) whose version may have changed. Method names like `SetFillStyleAsync`, `FillRectAsync`, `BeginPathAsync` etc. might have slightly different signatures. Indy spends 15-30 minutes adapting.

**Scenario C (10%):** Indy decides the custom arrow isn't worth the effort and uses a simpler visual (solid line or default shape). This is a valid adaptation.

**My mitigation:** I included the full `DrawArrowAsync` implementation in the spec. But I also know that Canvas2D context APIs are notoriously version-sensitive.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 9: The Using Statements Will Need Adjustment

**Confidence:** 🟡 Medium (65% likely)

I provided the complete list of `using` statements in Appendix A. But namespaces are the most likely thing to change between library versions. I predict Indy will need to fix 1-3 using statements due to:

- Namespace reorganization (e.g., `FoundryWorldsAndDrawings.ThreeD.Core` might have split or merged)
- Types moving between namespaces
- New intermediary namespaces being introduced

**What I think will happen:** Indy will start with the spec's using list, get compile errors on 1-2 imports, use IDE autocomplete to find the correct namespaces, and fix them in 2-5 minutes. This is routine and shouldn't be a blocker.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction 10: The Spec's Biggest Weakness

**Confidence:** 🟢 High (this is my honest self-assessment)

I believe the spec's biggest weakness is **not providing the complete, compilable TugOfWar.razor.cs file inline**. I provided patterns and fragments, but Indy will need to assemble them into a working whole. This assembly step is where misunderstandings compound — a missed `await`, a wrong variable name, a callback wired to the wrong shape.

**Why I didn't include it:** Sully said Indy won't have access to the library source, but the TugOfWar files ARE in Three2025 which Indy CAN access. So Indy should be able to read the actual implementation. The spec's role is to explain what it does, not to be a copy-paste replacement.

**But if I'm wrong about Indy's access:** If Indy somehow doesn't have the TugOfWar files available, the spec would need to include the COMPLETE code-behind as an appendix. I chose not to because Sully said Indy would have "a handful of working examples" — the TugOfWar files themselves should be among those.

**My recommendation for future specs:** Always include a COMPLETE, one-file, copy-paste-and-it-compiles reference — even if it's redundant with working code. The cost of redundancy is low; the cost of an Indy who can't find the reference file is high.

> **Indy's verdict:** ☐ Correct ☐ Wrong ☐ Partial  
> **What actually happened:**

---

## Prediction Summary Table

| # | Prediction | Confidence | Predicted Impact |
|---|---|---|---|
| 1 | Naming transition (BeforeAnimationRefresh) | 🟡 60% | 10-20 min |
| 2 | 3D works before 2D | 🟢 80% | 30-60 min on 2D |
| 3 | Stale flags cause invisible shapes | 🟢 85% | 15-30 min |
| 4 | Task.Delay needs adjustment | 🟡 50% | 5-15 min |
| 5 | Double-counted animation time confuses | 🟡 55% | 0-30 min |
| 6 | FoPipe3D needs most iteration | 🟢 75% | 20-40 min |
| 7 | Debug controls just work | 🟢 90% | <5 min |
| 8 | Custom arrow drawing struggles | 🟡 60% | 15-30 min |
| 9 | Using statements need fixes | 🟡 65% | 2-5 min |
| 10 | Spec's biggest weakness = no complete file | 🟢 High | Varies |

**Total predicted debugging overhead:** 1.5-3 hours on top of clean implementation time.

---

## Questions for Indy's After-Action Review

When you're done, please answer these:

1. **What was the FIRST thing that didn't compile?** (This tells Atlas what to verify more carefully.)

2. **What was the HARDEST bug to diagnose?** (This tells Atlas what needs better troubleshooting coverage.)

3. **Which code sample from the spec was MOST useful?** (This tells Atlas what format works best.)

4. **Which code sample was LEAST useful or MISLEADING?** (This tells Atlas what to fix.)

5. **Did you reference the API docs, and were they accurate?** (This tells Atlas whether to rely on docs or inline everything.)

6. **What did you wish the spec had included?** (This directly improves the next spec.)

7. **On a scale of 1-10, how well did the spec prepare you?** (Calibration for Atlas's confidence ratings.)

8. **Would you have preferred a single complete file over the pattern-based approach?** (Addresses Prediction 10.)

---

## Atlas's Self-Assessment

**What I did well:**
- Comprehensive inline code samples from verified, working code
- Explicit "use whichever compiles" guidance for naming transitions
- Troubleshooting guide with code-based diagnostics
- Honest confidence ratings per section

**What I'm uncertain about:**
- Whether the 2D API has changed enough to cause real problems
- Whether `PreComputeMesh` delegate assignment still works vs fluent API
- The exact Task.Delay threshold needed on Indy's machine

**What I would do differently next time:**
- Include the COMPLETE implementation file as an appendix (even if available elsewhere)
- Test-compile the spec's code samples against the CURRENT library version before handoff
- Ask Sully to confirm exactly which files Indy will have access to, rather than assuming

**My learning goal from this experiment:**
I want Indy's feedback to tell me whether the **pattern-based spec approach** (showing fragments and explaining how they compose) works better or worse than a **monolithic approach** (here's the whole file, here's what each section does). The ClockDemo AAR suggested copy-and-modify is the best strategy, but this spec is more complex since it involves two independent rendering systems cooperating on one page.

---

*This prediction document should be reviewed by Indy after implementation is complete. Atlas will use the feedback to calibrate future specifications.*
