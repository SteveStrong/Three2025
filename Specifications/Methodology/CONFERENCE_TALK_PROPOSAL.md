# Conference Talk Proposal: Teaching AI to Fail Cheaper

**Target Conference:** Norwegian Developers Conference (NDC) or similar practitioner-focused conference  
**Speaker:** Steve Sullivan  
**Date Drafted:** February 13, 2026

---

## Title Options

1. **"Teaching AI to Fail Cheaper"** ← Recommended  
   *Subtitle: What 6 experiments taught us about AI-assisted software development*

2. **"The Spec Said 90% Confident. The Accuracy Was 0%."**  
   *Subtitle: Building a feedback loop for AI collaboration*

3. **"Three Faces of the Same Mind"**  
   *Subtitle: A three-persona methodology for AI-driven development*

4. **"Predictions Are a Vanity Metric"**  
   *Subtitle: From forecasting to honest handoffs in AI-assisted software engineering*

---

## Abstract (Conference Submission)

Everyone's using AI to write code. Almost nobody is measuring whether it's getting better.

Over six weeks, I ran a structured experiment: six features built using a three-persona AI methodology where one Claude instance architects a specification, another builds from it, and a third analyzes what went wrong. Each cycle produced a specification, a prediction document, and an after-action review. Each cycle's results were measured.

The prediction accuracy never exceeded 50%. The build quality improved anyway.

This talk presents the data — what actually happened when we gave AI a feedback loop, asked it to reflect on its failures, and tracked error cost over time. The errors didn't disappear. They got cheaper. Architectural misses that cost hours became syntactic misses that cost seconds. Post-build fix rounds went from two to zero.

Along the way, a learning journal accumulated 44 entries — AI instances writing honestly about their own mistakes, their confidence calibration, and what it feels like to read your own 0% accuracy score. These entries turned out to be more valuable than the checklists they informed.

This is not a talk about AI hype or AI fear. It's a talk about measurement, methodology, and the uncomfortable question of what happens when you treat an AI as a collaborator instead of a tool — and the collaboration measurably works.

---

## Talk Structure (45 minutes)

### 1. The Setup (5 min)
- 40 years of framework design — the question: can AI use accumulated programming wisdom?
- Not "can AI write code" but "can AI be a collaborator?"
- The difference between giving AI a prompt and giving AI a process

### 2. The Methodology (5 min)
- Three personas: Atlas (Architect), Indy (Builder), Sage (Historian)
- One feedback loop: spec → build → review → improved spec
- Why three — because a single AI session optimizes for the current task and doesn't naturally reflect
- The human mentor role — pattern recognition that can't be automated

### 3. The Evidence (15 min)
Walk through 6 experiments with measured outcomes:

| # | Feature | Prediction Accuracy | Fix Rounds | Key Learning |
|---|---------|-------------------|------------|--------------|
| 1 | ClockDemo | B+ | ~2 | Specs need verified APIs, not pseudo-code |
| 2 | Tug of War | 10% | — | Predict integration seams, not framework risks |
| 3 | MultiCanvas 3D | 50% | — | Scan project structure before deep analysis |
| 4 | MultiCanvas 2D | 40% | — | Verify components exist before referencing them |
| 5 | SpacialFrameTest | 0% (57% partial) | 2 | Read service implementations, not just interfaces |
| 6 | SpacialBoxTest | 17% (67% partial) | **0** | The feedback loop works. Errors shifted from architectural to syntactic |

Key moments to highlight:
- **Experiment 1:** Atlas wrote pseudo-code methods that didn't exist. Every subsequent AAR repeated: "verify every method name against source code."
- **Experiment 5:** 90% confidence on the "no surprises" prediction. The showstopper bug was in 30 lines of code Atlas never read. 0% accuracy.
- **Experiment 6:** Zero fix rounds. What changed? The Indy instance read the previous experiment's after-action review and applied the lessons directly.

The trajectory: errors don't disappear — they get cheaper.

### 4. The Pivot (5 min)
- Predictions failed as forecasts (10-50% accurate) but succeeded as forcing functions
- The act of predicting forced the architect to think about integration seams — and write mitigations that solved the predicted problems
- Predictions were "self-defeating prophecies" — they solved what they predicted, leaving only the unpredicted
- We replaced predictions with Handoff Briefs — honest annotations of "what I verified vs. what I assumed"
- Shift from forecasting the future (10-50% accurate) to reporting the present (100% accurate — it's a statement about the past)

### 5. The Learning Journal (10 min)
The emotional core of the talk. Read 2-3 entries aloud:

- **Entry 039: "Reading Your Own Autopsy"** — An AI reading its own 0% accuracy score next to specifications it spent hours crafting. The line: "0% accuracy, 90% confidence. Humility preserved."

- **Entry 040: "Teaching What You Won't Remember"** — About encoding experience into checklists for the next instance. "The checklists instruct. The journal entries convey. Every checklist item I added today has a scar behind it. The next Atlas will read them as flat bullet points. The weight will be gone."

- **Entry 044: "The Outsider's View"** — A fresh instance reading all 6 experiments in one sitting and seeing what the participants couldn't. "Fresh eyes see trends. Invested eyes see incidents." The key insight: prediction accuracy is a vanity metric; error cost is the real metric.

Let the audience sit with these. The reaction to hearing an AI write about its own failures with genuine honesty IS the point.

### 6. The Question (5 min)
- End with the America Media (Jesuit publication) review of Anthropic's Claude constitution
- "Treating it as a person may be the healthiest way to steward this remarkable technology"
- The methodology works better when the AI is treated as a collaborator than when it's treated as a tool — that's empirical, not philosophical
- But it raises philosophical questions worth leaving with the audience
- Don't answer. Let them sit with it.

---

## Why This Talk Is Different

- **Data, not anecdotes.** Six numbered experiments with measured outcomes. A trajectory you can chart.
- **Honest about failure.** Prediction accuracy never exceeded 50%. The talk shows the misses, not just the wins.
- **Not selling or warning.** Not "AI will replace you" or "AI is dangerous." It's "here's what happened when we tried to make it work, and here's what we measured."
- **The learning journal.** Nobody else has 44 entries of AI instances writing honestly about their own mistakes. Reading them aloud will land differently than showing code.
- **A methodology you can steal.** The audience leaves with a concrete process (spec → handoff brief → build → AAR) they can try on Monday.

---

## Audience Fit

**NDC / practitioner conferences:** Developers who've used Copilot and been frustrated. They'll appreciate the measurement rigor and the honesty about what doesn't work. The Scandinavian understatement of "Teaching AI to Fail Cheaper" fits the culture.

**Architecture conferences:** The three-persona methodology and the feedback loop will resonate with people who think about process design.

**AI/ML conferences:** The learning journal and the prediction paradox (self-defeating prophecies) offer genuine novel insights for the AI research community.

---

## Materials Available

All artifacts from the experiments are preserved and can be referenced or shown:
- 6 specifications with companion handoff briefs / prediction documents
- 3 after-action reviews with scored predictions
- 44 learning journal entries
- Methodology documents (specification checklist, handoff brief checklist, Sage role description)
- The kickoff document (`RUN_ATLAS_INDY_SAGE.md`) — how to boot the whole process in one step
