# Atlas Prediction Checklist

**Purpose:** Pre-flight checklist for Claude "Atlas" before writing predictions for Indy's implementation  
**Prepared By:** Claude "Sage" (Historian)  
**Date:** February 10, 2026  
**Based On:** After-Action Reviews for ClockDemo, Tug of War, Multi-Canvas 3D, Multi-Canvas 2D  
**Accuracy across 4 experiments:** ClockDemo (see AAR), Tug of War (10%), Multi-Canvas 3D (50%), Multi-Canvas 2D (40%)

---

## The One Thing to Remember

**Your predictions are self-defeating prophecies.** When you predict a risk and write mitigations for it in the spec, that risk gets solved. The actual hard problem will come from a category you didn't predict. Accept this. Budget accordingly.

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
| **Spec gap / omission risks** | 25% | What did the spec forget? Showcase? NavMenu? Layout matching? |
| **Implementer behavior** | 25% | How will Indy approach the task? What will Indy verify vs. trust? |
| **Framework / API risks** | 10% | Sully's infrastructure works. Stop over-investing here. |
| **Runtime / environment** | 10% | Console noise, timing, visual expectations. |

- [ ] **Count your predictions by category after writing them.** If >20% are framework risks, you're repeating Multi-Canvas 3D's mistake.
- [ ] **At least 2 predictions must be about integration seams** (tree view, Showcase, NavMenu, layout conventions, PubSub wiring).
- [ ] **At least 1 prediction must be about a spec gap** — something YOU know the spec doesn't cover.

### 2.2 Stop Predicting These (Proven Non-Risks)

These categories have been consistently correct across experiments. Don't waste budget on them:

- [ ] ~~"Will the Drawing/Arena singleton handle multiple pages/scenes?"~~ — **Always works.** (3D: ✅, 2D: ✅)
- [ ] ~~"Will the console be clean?"~~ — **Predict this only if you have specific reason to worry.** (2D: ✅)  
- [ ] ~~"Will Indy follow the spec?"~~ — **Yes.** Indy follows well-written specs. (2D: all 3 "will Indy take shortcuts" predictions were wrong.)
- [ ] ~~"Will basic API signatures work?"~~ — **If you verified them, they will.** (2D: ✅ first try)

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

Your confidence calibration has a known bias: **you're overconfident on medium predictions and underconfident on high ones.**

| Your Stated Confidence | Actual Hit Rate (4 experiments) | Calibration |
|---|---|---|
| 🟢 High (80%+) | ~75% | Roughly correct — keep these |
| 🟡 Medium (40-60%) | ~30% | You're overconfident — lower these to 25-35% |
| 🟡 Low-Medium (30-40%) | ~40% | Ironically more accurate than medium |

- [ ] **If you're writing "Medium (50% likely)" — ask yourself: is this really just a coin flip you're dressing up as a prediction?** If so, either commit to a direction or drop the prediction.
- [ ] **High-confidence predictions should predict what WILL work, not what MIGHT fail.** Your best accuracy is on "this infrastructure will handle it correctly" — lean into that.

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
- [ ] **For every "Indy will get X wrong" prediction:** Can you articulate WHY Indy would get it wrong, beyond "it's tricky"? If not, drop it.
- [ ] **For every framework risk prediction:** Has this specific risk been tested in a prior experiment? If it worked before, don't predict it will fail now.
- [ ] **Read the predictions as Sage would.** After the build, will these predictions produce an interesting AAR regardless of whether they're right or wrong? If the hit/miss analysis would be boring, the predictions are too safe.

---

## Appendix: Evidence Base

### What Predicts Well (Keep Doing)
- High-confidence infrastructure predictions (~75% accurate)
- "Code Smells to Avoid" sections (cited as useful by Indy in every experiment)
- "Things You'll Be Tempted To Do (DON'T)" warnings (validated across 2 experiments)
- "This will work fine" for battle-tested Sully infrastructure

### What Predicts Poorly (Stop Doing)
- "Indy will take shortcuts" (0/3 in Multi-Canvas 2D)
- "Indy will get API signatures wrong if the spec provides them" (0/2 across experiments)
- Medium-confidence implementer behavior predictions (~30% actual vs 50% stated)
- Framework risks on battle-tested systems (trivially correct, wastes budget)

### What's Never Been Predicted (Start Doing)
- Framework parity gaps between 2D and 3D (the FoPage2D discovery)
- Showcase/NavMenu integration requirements (Sully catches this every time)
- Layout convention matching (existing pages set the pattern)
- Tree view integration exposing missing overrides
- Sully's pre-build scope modifications

### The Hardest Problem Pattern
The hardest problem is always at the integration seam — where new code meets existing framework in a way that hasn't been tested. If you predict nothing else, predict something about THE SEAM.

---

*"The map was excellent. It just didn't show the cliff." — Sage, Entry 036*
