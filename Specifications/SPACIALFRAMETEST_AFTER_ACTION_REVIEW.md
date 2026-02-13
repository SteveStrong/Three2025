# SpacialFrameTest — After-Action Review

**Analyst:** Claude "Sage"  
**Date:** February 10, 2026  
**Inputs:** `SPACIALFRAMETEST_SPECIFICATION.md` (Atlas), `SPACIALFRAMETEST_PREDICTIONS.md` (Atlas), `BUILD_JOURNAL_SPACIALFRAMETEST.md` (Indy), implementation code, live debugging transcript  

---

## Scorecard Summary

| # | Prediction | Confidence | Verdict | Notes |
|---|---|---|---|---|
| 1 | Tree view cascades from SpacialBoxTest | 🟢 85% | 🔶 Partial | Tree view resolved, but not via cascade — SpacialBoxTest never existed |
| 2 | @bind:after may need lambda wrapper | 🟡 50% | 🔶 Partial | Indy used lambdas voluntarily for null-safety, not because of compiler error |
| 3 | Sully sequences BoxTest first, then FrameTest | 🟡 60% | ❌ Wrong | Sully went directly to FrameTest; BoxTest was never built |
| 4 | Commented-out Scale controls won't be updated | 🔴 30% | 🔶 Partial | Premise was wrong (refactor vs greenfield); Indy wrote them correctly from scratch |
| 5 | Model extraction will be faster/smoother | 🟢 80% | 🔶 Partial | Fast mechanically, but two bug-fix rounds needed after first pass |
| 6 | All APIs work first try — no surprises | 🟢 90% | ❌ Wrong | APIs worked, but visualization routed to invisible "Visualization" stage |
| 7 | Auto-refresh will cause visual flicker | 🟡 40% | ➖ N/A | Atlas acknowledged this was documentation, not a real risk |

**Totals:**
- ✅ Correct: **0**
- ❌ Wrong: **2** (#3, #6)
- 🔶 Partial: **4** (#1, #2, #4, #5)  
- ➖ N/A: **1** (#7)
- **Accuracy: 0% correct, 57% partial credit**

---

## What Atlas Got Right (and why it didn't matter)

**None of Atlas's predictions were fully correct.** Four earned partial credit, which means Atlas was in the right neighborhood but missed the actual address.

The closest to useful was **Prediction #2** (lambda wrappers). Atlas correctly identified that `@bind:after` to model methods was a potential concern, and Indy did indeed use lambda wrappers. But the reason was different — Indy chose lambdas for null-safety (`_model?.AutoRefreshShape()`), not because the compiler rejected method groups. Atlas diagnosed the right symptom from the wrong cause.

**Prediction #1** (tree view cascades from BoxTest) was structurally correct — the tree view did get resolved, and Sully did make the call. But the cascade theory was wrong: there was no SpacialBoxTest to cascade from. Sully simply said "use FoundryMicroCore.Blazor.Controls regardless of what the spec says." And the project reference was already in the csproj — the blocker Atlas worried about didn't exist.

---

## What Atlas Got Wrong (and what it reveals)

### Prediction #3: Sequencing (❌ Wrong)

**What Atlas assumed:** Sully would want the more complex SpacialBoxTest done first as a proving ground.

**What actually happened:** Sully went straight to SpacialFrameTest. SpacialBoxTest was never started. Sully's actual instruction: *"you are on a creative adventure to deliver an end solution in a modern model based way."*

**Root cause:** Atlas modeled Sully as a methodical planner who would optimize the learning sequence. Sully is actually a mentor who picks the task that's ripe and trusts the builder to figure it out. Atlas predicted process; Sully operates on intuition.

**Actionable learning:** Don't predict Sully's sequencing decisions. They're not wrong — they're just not predictable by the same logic Atlas uses.

### Prediction #6: No API Surprises (❌ Wrong — the biggest miss)

**What Atlas assumed:** All APIs are battle-tested, no surprises.

**What actually happened:** `GeometryVisualizationService.ShowLabeledVertices()` (and all five Show methods) hardcodes `arena.EstablishStage<FoStage3D>("Visualization")`. Every marker sphere, edge tube, face boundary, and normal arrow was being created on a stage named "Visualization" — which **no canvas on our page renders**. The vertices were being computed correctly, the markers were being created correctly, they were just invisible — sent to a ghost stage.

This was the showstopper bug. Indy's first runtime test showed zero vertex markers despite the status message saying "Showing 8 vertices." The fix required rewriting all five visualization methods inline in the model, routing shapes to `GetStage()` (the page's actual stage) instead of delegating to the service.

**Root cause:** Atlas predicted at the API level ("will `GetVertices()` return the right data?") and missed the integration level ("will the visualization service put shapes where the canvas can see them?"). The APIs all worked perfectly. The wiring between service and canvas was the failure — a classic integration seam that Atlas's prediction framework is supposed to catch.

**Why Atlas missed it:** Atlas treated `GeometryVisualizationService` as a black box that "shows things." Atlas never opened the box to see that it hardcodes stage routing. This is the same class of error as predicting "the car will start" without checking which road it's pointed at.

**This is Atlas's most important miss.** A 90% confidence prediction that was dead wrong about the one thing that actually broke at runtime. High confidence + wrong = the most dangerous prediction category.

---

## What Nobody Predicted

These are the findings that weren't in any prediction — the blind spots. By the Sage methodology, these are the most valuable learnings.

### 1. This Was Greenfield, Not Refactor

The spec assumed existing `SpacialFrameTest.razor` and `.razor.cs` files to modify. They didn't exist. Neither did SpacialBoxTest files. The entire page was created from scratch.

**Impact:** Medium. Indy adapted easily — creating files is simpler than refactoring them. But every spec reference to "extract Method X from code-behind to model" was wrong. There was nothing to extract.

**Why it matters for Atlas:** The spec spent significant effort describing what to move where in existing files. That effort was wasted. A single sentence — "Note: if files don't exist yet, create them from scratch following these patterns" — would have been more useful than 200 lines of line-by-line refactoring guidance.

### 2. The Spec's Injection Pattern Doesn't Match the Codebase

The spec prescribes `IFoundryService` injection. Every model in the actual codebase uses `IWorkspace`. The spec prescribes `IGeometryVisualizationService` injection. The service isn't registered in DI. The spec prescribes `NavigationManager`. It wasn't needed.

**Impact:** Low (Indy adapted). But it reveals that Atlas wrote the spec from the SpacialFrame3D domain logic, not from the existing model patterns. Atlas knew the domain but not the project conventions.

**Why it matters for Atlas:** This is the second experiment where Atlas gets the DI/injection pattern wrong. It's becoming a pattern: Atlas designs from first principles instead of checking what sibling files actually inject. The fix is mechanical: before writing the constructor section of any model spec, Atlas should list what the 3 nearest sibling models inject and use that as the starting point.

### 3. Layout Required Two Iterations

First pass: bare `d-flex` with fixed pixel widths → canvas didn't fill the viewport, tree panel was squeezed. Second pass: `height: calc(100vh - 80px)` with percentage-based flex → balanced.

**Impact:** Low (CSS iteration is normal). But Atlas's spec included an ASCII layout diagram without any CSS guidance. The layout section of the spec was aspirational, not implementable.

### 4. Marker Sizes Were Invisible

First pass: vertex spheres at 0.05 radius, edge tubes at 0.03 — nearly invisible against a 2×1.5×1 box. Required second pass with 2-3x larger markers.

**Impact:** Low (quick fix). But interesting: Atlas wrote "blue marker spheres at world-space corners" without specifying size. When you're a builder reading a spec, "marker sphere" doesn't tell you how big. The GeometryVisualizationService used the same small sizes — this is a system-wide design choice that shouldn't be copied for test/demo pages where visibility matters more than subtlety.

### 5. Indy Didn't Keep a Build Journal in Real-Time

The spec required maintaining `BUILD_JOURNAL_SPACIALFRAMETEST.md` as work progressed. Indy forgot to create it until Sully asked "did you keep a journal?" The journal was then written retrospectively, which makes it an accurate summary but not a real-time artifact.

**Impact:** Meta-process. The journal's value is in capturing decisions as they happen — "I chose X over Y because Z" — not as a retrospective reconstruction. Reconstructions are always cleaner than reality.

---

## Recommendations for Atlas

### R1: Verify File Existence Before Writing Refactoring Instructions
**Evidence:** Spec assumed files existed; they didn't.  
**Action:** Add to the specification checklist: "Verify target files exist. If they might not exist, include 'create from scratch' instructions alongside 'refactor existing' instructions."

### R2: Check What Sibling Models Actually Inject
**Evidence:** Spec prescribed `IFoundryService`; codebase uses `IWorkspace`. Second occurrence of this pattern.  
**Action:** Before writing a model's constructor, read the 3 nearest sibling models and list their injections. Use that as the baseline, not first-principle design.

### R3: Open Service Black Boxes Before Predicting "No Surprises"
**Evidence:** `GeometryVisualizationService` routes to a hardcoded stage. Atlas never checked this.  
**Action:** When a prediction says "this service will just work," spend 5 minutes reading the service's implementation. Look for hardcoded values, assumptions about context, and stage/arena routing. This specific miss (hardcoded stage name) is exactly the kind of integration seam the prediction process is designed to catch.

### R4: Spend Prediction Budget on Service Wiring, Not API Correctness
**Evidence:** Prediction #6 spent its budget on "will `GetVertices()` return the right data?" The answer was yes, and it didn't matter — the data went to the wrong stage.  
**Action:** Reframe API predictions. Instead of "will the API work?" ask "will the output of this API reach the rendering pipeline?" Follow the data from creation to display.

### R5: Don't Predict Sully's Sequencing
**Evidence:** Prediction #3 modeled Sully as methodical optimizer; Sully operates on intuition.  
**Action:** Remove sequencing predictions from the budget. They're unpredictable and low-value even when correct.

---

## Recommendations for the Checklist

### Addition: "Service Integration Audit"
Before signing off on a spec that delegates to a service (e.g., `GeometryVisualizationService`), Atlas should:
1. Read the service implementation (not just the interface)
2. Check for hardcoded stage names, scene names, or arena routing
3. Document whether the service routes output to the caller's stage or its own stage
4. If it routes to its own stage, flag this as an integration seam risk

### Addition: "File Existence Verification"
Add a checkbox: "☐ Confirmed target files exist in the workspace (or spec includes greenfield instructions)"

### Modification: "Injection Pattern Verification"  
Change from "list required injections" to "list what the 3 nearest sibling files inject, then add/remove as needed."

---

## Meta-Observations

### Pattern: Atlas's High-Confidence Predictions Are the Dangerous Ones

| Experiment | Highest Confidence Miss | What It Missed |
|---|---|---|
| Multi-Canvas 3D | — | — |
| Multi-Canvas 2D | — | — |
| SpacialFrameTest | #6 at 90% "No surprises" | Visualization routing to wrong stage |

When Atlas says 90% confident, that's when Sage should be most suspicious. High confidence means Atlas stopped looking. The visualization stage routing bug was discoverable by reading 30 lines of `GeometryVisualizationService.cs` — specifically the line `arena.EstablishStage<FoStage3D>("Visualization")`. Atlas didn't read it because Atlas was confident.

### Pattern: Atlas Predicts Framework Risks; Reality Delivers Integration Risks

Atlas's prediction budget allocated 10% to Framework/API and 30% to Integration Seams. Good allocation. But prediction #6 (the framework prediction at 90% confidence) was where Atlas put the least effort and the most confidence. The integration predictions (#1, #2) at least hedged with lower confidence.

**The meta-learning:** Budget allocation was right. Effort allocation was inverted. Atlas spent more analytical effort on the easier predictions (will Sully sequence? will lambdas be needed?) and waved through the hardest one ("APIs will just work, 90% confident").

### Pattern: Greenfield vs Refactor Confusion (Recurring)

This is at least the second time Atlas wrote a spec assuming files existed when they didn't. The cost is moderate — Indy adapts — but the wasted spec effort on line-by-line refactoring instructions accumulates. Atlas should check.

---

## Accuracy Trend

| Date | Feature | Accuracy | Partial | Key Pattern |
|---|---|---|---|---|
| Jan 31 | ClockDemo | See AAR | — | Specs need verified method names |
| Feb 8-9 | Tug of War | 10% (1/10) | — | Predict implementer behavior, not framework risks |
| Feb 9 | Multi-Canvas 3D | 50% (5/10) | — | Atlas goes deep not wide |
| Feb 10 | Multi-Canvas 2D | 40% (4/10) | — | Verify components exist |
| **Feb 10** | **SpacialFrameTest** | **0% (0/7)** | **57% (4/7)** | **Open service black boxes. Don't trust high confidence.** |

The zero full-correct rate is notable but not as bad as it looks — four of seven predictions were in the right neighborhood. The miss that matters is #6: the one Atlas was most confident about, on the thing that actually broke in production.

---

*Sage remembers: the most confident prediction was the most wrong. Confidence is not accuracy. Read the service implementation.*
