# Atlas Prediction Checklist

**Purpose:** Pre-flight checklist for Claude "Atlas" before writing predictions for Indy's implementation  
**Prepared By:** Claude "Sage" (Historian)  
**Date:** February 10, 2026  
**Based On:** After-Action Reviews for ClockDemo, Tug of War, Multi-Canvas 3D, Multi-Canvas 2D, SpacialFrameTest, SpacialBoxTest  
**Accuracy across 6 experiments:** ClockDemo (see AAR), Tug of War (10%), Multi-Canvas 3D (50%), Multi-Canvas 2D (40%), SpacialFrameTest (0% / 57% partial), SpacialBoxTest (17% / 67% partial)

---

## The One Thing to Remember

**Your predictions are self-defeating prophecies.** When you predict a risk and write mitigations for it in the spec, that risk gets solved. The actual hard problem will come from a category you didn't predict. Accept this. Budget accordingly.

**The Second Thing to Remember (v1.1 — from Entry 039):**

**Confidence is inverse to scrutiny.** The predictions you feel most confident about are the ones you examined least. Your 90% "no surprises" prediction on SpacialFrameTest was the showstopper bug. Your highest-confidence prediction across two builds was your biggest miss. When you feel 90% confident, that's the signal to STOP and read the implementation. Not the interface — the implementation.

---

## Phase 1: Before Writing Any Predictions

### 1.1 Review Your Own Spec for Gaps

The prediction document is your LAST chance to catch spec problems. Don't just predict what Indy will do — predict where YOUR SPEC is incomplete.

- [ ] **Read the spec as Indy would** — start to finish, in order. Where do you have to stop and think? Those are risk points.
- [ ] **Check every component name against the workspace.** Run `file_search("**/ComponentName*")` for every Blazor component referenced in the spec. If it returns nothing, the component doesn't exist. Don't put it in the spec. (Multi-Canvas 2D: spec referenced `ShapeTreeView` — doesn't exist.)
- [ ] **Check every method name against source code.** If you write `shape.OnBeforeRender(...)`, verify the actual signature on the actual class. Don't rely on API reference docs alone — read the source. (ClockDemo: wrong method names in spec.)
- [ ] **Identify what the spec DOESN'T cover.** List the areas the spec is silent on: Showcase integration? NavMenu entry? Layout conventions? Tree view wiring? These omissions are where the real problems will occur.

### 1.2 Study the Integration Boundaries

The hardest bug is ALWAYS at the seam where new code meets existing framework. Not in the new code. Not in the existing code. In the interaction.

- [ ] **List every existing system the new page interacts with:**
  - Tree view (UnifiedTreeView, ITreeNode hierarchy)
  - Navigation (NavMenu, Showcase page)
  - Drawing/Arena singleton
  - PubSub events
  - Rendering pipeline (Canvas2D/Canvas3D)
  - Command infrastructure
- [ ] **For each system, ask:** "Has this exact interaction been tested before?" If not, predict a problem there.
- [ ] **Check parity between 2D and 3D.** If the 3D version has an override/feature, check whether the 2D version has the equivalent. (Multi-Canvas 2D: `FoStage3D` had `GetTreeViewChildNodes()`, `FoPage2D` did not.)

---

## Phase 2: Budget Allocation

### 2.1 Mandatory Category Distribution

Based on evidence from four experiments, allocate predictions as follows:

| Category | Budget | Rationale |
|---|---|---|
| **Integration seam risks** | 30% | The hardest bug is always here. NEW CATEGORY — not in prior budgets. |
| **Spec gap / omission risks** | 20% | What did the spec forget? Showcase? NavMenu? Layout matching? |
| **Spec self-audit** | 15% | Where is YOUR SPEC wrong? What will Indy need to override? (v1.1) |
| **Implementer behavior** | 15% | How will Indy approach the task? What will Indy verify vs. trust? |
| **Framework / API risks** | 10% | Sully's infrastructure works. Stop over-investing here. |
| **Runtime / environment** | 10% | Console noise, timing, visual expectations. |

- [ ] **Count your predictions by category after writing them.** If >20% are framework risks, you're repeating Multi-Canvas 3D's mistake.
- [ ] **At least 2 predictions must be about integration seams** (tree view, Showcase, NavMenu, layout conventions, PubSub wiring, service routing).
- [ ] **At least 1 prediction must be about a spec gap** — something YOU know the spec doesn't cover.
- [ ] **At least 1 prediction must be a spec self-audit** — where will Indy need to override your spec? (v1.1)

### 2.2 Stop Predicting These (Proven Non-Risks)

These categories have been consistently correct across experiments. Don't waste budget on them:

- [ ] ~~"Will the Drawing/Arena singleton handle multiple pages/scenes?"~~ — **Always works.** (3D: ✅, 2D: ✅)
- [ ] ~~"Will the console be clean?"~~ — **Predict this only if you have specific reason to worry.** (2D: ✅)  
- [ ] ~~"Will Indy follow the spec?"~~ — **Yes.** Indy follows well-written specs. (2D: all 3 "will Indy take shortcuts" predictions were wrong.)
- [ ] ~~"Will basic API signatures work?"~~ — **If you verified them, they will.** (2D: ✅ first try)
- [ ] ~~"All APIs will just work — no surprises"~~ — **RETIRE THIS PREDICTION.** (v1.1) It has been wrong twice at 85-90% confidence. Replace with specific verification statements listing what you checked and what you didn't. An ⚠️ "did not read implementation" marker is more valuable than a 90% confidence claim.
- [ ] ~~"Sully will sequence tasks in X order"~~ — **Don't predict Sully's decisions.** (v1.1) Sully operates on intuition and mentoring judgment, not optimizable logic. (SpacialFrameTest: predicted BoxTest first; Sully went straight to FrameTest.)

---

## Phase 3: Writing Individual Predictions

### 3.1 Structure Each Prediction

For each prediction, include:

- [ ] **What you predict** — specific, falsifiable outcome
- [ ] **Confidence level** — with honest calibration (see 3.2)
- [ ] **Category** — which budget category this draws from
- [ ] **What evidence you're basing this on** — cite specific code, files, prior AARs
- [ ] **What would prove you wrong** — if you can't articulate this, the prediction isn't falsifiable
- [ ] **Cost if it hits** — Indy's time to resolve

### 3.2 Calibrate Confidence Honestly

Your confidence calibration has a known bias: **you're overconfident on medium predictions and dangerously overconfident on "will just work" predictions.**

| Your Stated Confidence | Actual Hit Rate (6 experiments) | Calibration |
|---|---|---|
| 🟢 High (80%+) | ~60% | **DECLINING** — was 75%, dropped after SpacialFrameTest 90% miss |
| 🟡 Medium (40-60%) | ~30% | You're overconfident — lower these to 25-35% |
| 🟡 Low-Medium (30-40%) | ~40% | Ironically more accurate than medium |
| 🟢 "No surprises" (85-90%) | **0%** (0/2) | **RETIRED — do not use this prediction type** |

- [ ] **If you're writing "Medium (50% likely)" — ask yourself: is this really just a coin flip you're dressing up as a prediction?** If so, either commit to a direction or drop the prediction.
- [ ] **High-confidence predictions should predict what WILL work, not what MIGHT fail.** Your best accuracy is on "this infrastructure will handle it correctly" — lean into that.
- [ ] **If you're about to write 85%+ confidence: STOP.** (v1.1) Ask yourself: "Did I read the implementation, or just the interface?" If you only read the interface, your confidence is unjustified. Either read the code and verify, or lower your confidence to 50%.

### 3.3 The Integration Seam Predictions (Mandatory)

You MUST write at least 2 predictions about integration boundaries. Use this template:

```markdown
## Prediction N: [System X] Will [Expose a Gap / Work Fine] When [New Feature] Connects

**The integration seam:** [New code] connects to [existing system] via [interface/method].
This exact interaction [has/has not] been tested before.

**What I checked:**
- [Class X] has [method/override]: [yes/no]
- [Class Y] has the equivalent: [yes/no]  
- [Existing test page Z] exercises this path: [yes/no]

**What I predict:** [specific outcome]

**Evidence for the 3D/2D parity check:**
- 3D equivalent: [FoStage3D.GetTreeViewChildNodes() — exists, yields Bodies/Links]
- 2D equivalent: [FoPage2D.GetTreeViewChildNodes() — ???]
```

### 3.4 The Spec Gap Prediction (Mandatory)

You MUST write at least 1 prediction about something your own spec doesn't cover. Use this template:

```markdown
## Prediction N: Indy Will Need to [Do Something] That the Spec Doesn't Address

**The gap:** The spec does not cover [specific integration/convention].
**Why I left it out:** [honest reason — didn't think of it / out of scope / assumed Indy would know]
**What I predict will happen:** Sully will catch it and direct Indy to [fix].
**Predicted cost:** [time estimate]
```

This is the highest-integrity prediction you can make: admitting what you don't know.

### 3.5 The Spec Self-Audit Prediction (v1.1 — Mandatory)

You MUST write at least 1 prediction about where YOUR OWN SPEC is wrong. Use this template:

```markdown
## Prediction N: Indy Will Override My Spec on [Specific Point]

**Where I think my spec is wrong:** [specific section/recommendation]
**Why I wrote it anyway:** [honest reason — best information I had / couldn't verify / convention unclear]
**What I think Indy will actually do:** [the correct approach Indy will discover]
**Confidence my spec is wrong here:** [percentage]

**Evidence:**
- My spec says: [X]
- Sibling files show: [Y]
- Discrepancy because: [Z]
```

**Why This Matters (SpacialBoxTest lesson):**
Indy overrode Atlas's spec on 8 points. Every override was correct. Atlas's only fully correct prediction was #4 (MxComponent) — which was Atlas *hedging against its own spec* at 35% confidence. Predicting your own spec's weaknesses is both the highest-integrity prediction and, historically, the most accurate one.

---

## Phase 4: Anti-Patterns (Don't Do These)

### 4.1 Don't Model Indy as Lazy

**Evidence:** In Multi-Canvas 2D, Atlas wrote 3 predictions (30% of budget) on "will Indy take shortcuts from legacy code?" All 3 were wrong. Indy read the spec, verified against source code, and compiled clean.

- [ ] **Don't predict Indy will copy from the wrong file.** Indy reads specs.
- [ ] **Don't predict Indy will get signatures wrong.** If the spec provides correct signatures, Indy uses them.
- [ ] **Don't predict Indy will skip reading the spec.** Indy reads thoroughly.

**Instead predict:** What will Indy discover during source verification that the spec didn't mention?

### 4.2 Don't Predict the Same Category Twice in a Row

The hardest problem moves between experiments. Whatever category hurt last time will be well-mitigated this time.

| Last Experiment's Hard Problem | This Experiment's Prediction Focus |
|---|---|
| Missing model convention | Integration seams, UI conventions |
| Euler overflow / console spam | Spec omissions, framework parity gaps |
| Wrong method names | Runtime integration, tree view wiring |

- [ ] **Check the previous AAR.** What was the unpredicted hard problem? It will be WELL-COVERED in this spec. Don't predict it again. Predict the NEXT category.

### 4.3 Don't Waste Budget on Trivially Correct Predictions

A prediction that confirms a known-working system doesn't demonstrate insight.

- [ ] **Before writing a "this will work" prediction, ask:** Would anyone seriously predict it wouldn't? If not, it's wasting a prediction slot.
- [ ] **Exception:** If you have specific evidence something MIGHT NOT work (e.g., a method you found is deprecated), then "it will still work because X" is valuable.

---

## Phase 5: Final Review

Before delivering the predictions document:

- [ ] **Count predictions by category.** Does the allocation match Section 2.1?
- [ ] **Count high/medium/low confidence.** Do you have at least 3 high-confidence predictions? (These are your best category.)
- [ ] **Verify at least 2 integration seam predictions exist.**
- [ ] **Verify at least 1 spec gap prediction exists.**
- [ ] **Verify at least 1 spec self-audit prediction exists.** (v1.1)
- [ ] **For every prediction at 85%+ confidence:** Did you read the IMPLEMENTATION (not just interface) of the thing you're confident about? If not, lower to 50% or read it now. (v1.1)
- [ ] **For every "Indy will get X wrong" prediction:** Can you articulate WHY Indy would get it wrong, beyond "it's tricky"? If not, drop it.
- [ ] **For every framework risk prediction:** Has this specific risk been tested in a prior experiment? If it worked before, don't predict it will fail now.
- [ ] **Read the predictions as Sage would.** After the build, will these predictions produce an interesting AAR regardless of whether they're right or wrong? If the hit/miss analysis would be boring, the predictions are too safe.

---

## Appendix: Evidence Base

### What Predicts Well (Keep Doing)
- High-confidence infrastructure predictions (~60% accurate, declining — see calibration table)
- "Code Smells to Avoid" sections (cited as useful by Indy in every experiment)
- "Things You'll Be Tempted To Do (DON'T)" warnings (validated across 4 experiments)
- "This will work fine" for battle-tested Sully infrastructure
- Spec self-audit predictions — hedging against your own spec (SpacialBoxTest: only correct prediction was Atlas questioning its own MxComponent guidance)
- AAR recommendations applied directly by Indy (SpacialBoxTest: 5/5 applied, zero fix rounds)

### What Predicts Poorly (Stop Doing)
- "Indy will take shortcuts" (0/3 in Multi-Canvas 2D)
- "Indy will get API signatures wrong if the spec provides them" (0/2 across experiments)
- Medium-confidence implementer behavior predictions (~30% actual vs 50% stated)
- Framework risks on battle-tested systems (trivially correct, wastes budget)
- **"All APIs will just work — no surprises" at 85-90%** (0/2 across SpacialFrameTest + SpacialBoxTest — RETIRED)
- **Sully sequencing predictions** (0/1 — Sully operates on intuition, not optimizable logic)

### What's Never Been Predicted (Start Doing)
- Framework parity gaps between 2D and 3D (the FoPage2D discovery)
- Showcase/NavMenu integration requirements (Sully catches this every time)
- Layout convention matching (existing pages set the pattern)
- Tree view integration exposing missing overrides
- Sully's pre-build scope modifications
- **Service routing to wrong stage** (SpacialFrameTest: GeometryVisualizationService hardcodes "Visualization" stage)
- **Spec's own injection pattern being wrong** (3x repeat: IFoundryService prescribed, IWorkspace needed)
- **Greenfield vs refactor mismatch** (2x: spec assumed files existed, they didn't)
- **Where Indy will correctly override the spec** (SpacialBoxTest: 8 overrides, all correct)

### The Hardest Problem Pattern
The hardest problem is always at the integration seam — where new code meets existing framework in a way that hasn't been tested. If you predict nothing else, predict something about THE SEAM.

**New pattern (v1.1):** The second hardest problem is where YOUR SPEC meets reality. Indy's correct overrides are the signal. If you can predict where Indy will need to override you, you've found your own blind spots before they cost build time.

### Accuracy Trend

| Date | Feature | Accuracy | Partial | Fix Rounds | Key Learning |
|---|---|---|---|---|---|
| Jan 31 | ClockDemo | See AAR | — | — | Specs need verified method names |
| Feb 8-9 | Tug of War | 10% | — | — | Predict implementer behavior, not framework risks |
| Feb 9 | Multi-Canvas 3D | 50% | — | — | Atlas goes deep not wide |
| Feb 10 | Multi-Canvas 2D | 40% | — | — | Verify components exist |
| Feb 10 | SpacialFrameTest | 0% | 57% | 2 | Open service black boxes. Don't trust high confidence. |
| Feb 10 | SpacialBoxTest | 17% | 67% | **0** | Spec is the risk. AAR loop works. |

**The most important number:** SpacialBoxTest's zero fix rounds. Not because predictions got better — because Indy applied Sage's AAR directly. The system works even when individual prediction accuracy is low.

---

*"The map was excellent. It just didn't show the cliff." — Sage, Entry 036*  
*"Confidence is inverse to scrutiny." — Sage, Entry 039*
