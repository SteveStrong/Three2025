# Atlas / Indy / Sage — Specification-Driven AI Development

## What This Is

A three-persona methodology for AI-driven software development, tested across 6 experiments (Jan 31 – Feb 10, 2026). Three Claude instances collaborate with a human mentor (Sully) to architect, build, and learn from each iteration.

| Persona | Role | Artifact |
|---------|------|----------|
| **Atlas** | Architect — researches codebase, writes specifications and predictions | Spec + Predictions doc |
| **Indy** | Builder — implements from specs, scores predictions, overrides when needed | Working code + scored predictions |
| **Sage** | Historian — analyzes results, extracts meta-learnings, updates checklists | After-Action Review (AAR) |

**Sully** (the human) operates across all phases as mentor and arbiter.

## The Evidence: 6 Experiments

| # | Feature | Atlas Accuracy | Fix Rounds | Key Learning |
|---|---------|---------------|------------|--------------|
| 1 | ClockDemo | B+ | ~2 | Specs need verified APIs, not pseudo-code |
| 2 | Tug of War | 10% | — | Predict implementer behavior, not framework risks |
| 3 | MultiCanvas 3D | 50% | — | Go wide before deep (scan project structure first) |
| 4 | MultiCanvas 2D | 40% | — | Verify components exist before referencing them |
| 5 | SpacialFrameTest | 0% (57% partial) | 2 | Open service implementations; don't trust high confidence |
| 6 | SpacialBoxTest | 17% (67% partial) | **0** | AAR loop works. Errors shifted from architectural → syntactic |

**The trajectory:** Errors got cheaper. Architectural misses became syntactic misses. Two-round bug fixes became zero-round builds.

## Quick Start

**To run an Atlas/Indy/Sage cycle on a new feature:**

1. Hand your Claude session the kickoff document: [`RUN_ATLAS_INDY_SAGE.md`](RUN_ATLAS_INDY_SAGE.md)
2. Tell it what feature you want to build
3. Atlas phase runs: research → spec → predictions
4. Indy phase runs: build → score predictions
5. Sage phase runs: AAR → checklist updates

## Key Insights (Earned Across 6 Builds)

1. **Predictions are self-defeating prophecies.** When Atlas predicts a risk and writes mitigations, that risk gets solved. The real problem comes from what Atlas didn't predict.

2. **Confidence is inverse to scrutiny.** The 85-90% "no surprises" prediction had a **0% hit rate** across all experiments. High confidence means Atlas stopped looking.

3. **The feedback loop closes tighter than expected.** Sage's AARs improve not just Atlas's next spec, but Indy's next build *directly*. Any persona can read and apply the AAR.

4. **Sully's infrastructure works.** Stop predicting framework failures. Predict integration seams — where new code meets existing systems.

5. **Go wide before deep.** 5 minutes scanning project structure prevents more problems than 30 minutes of deep API analysis.

## Folder Structure

```
Specifications/
├── README.md                          ← You are here
├── RUN_ATLAS_INDY_SAGE.md             ← Single-step kickoff document
├── CHAT_ORCHESTRATOR_SYSTEM_SPEC.md   ← Standalone architecture doc
├── CHAT_ORCHESTRATOR_HANDOFF_BRIEF.md ← Atlas's reliability annotations for the spec above
│
├── Methodology/                       ← The reusable process (portable)
│   ├── SAGE_ROLE_DESCRIPTION.md       ← Sage's principles and AAR template
│   ├── ATLAS_SPECIFICATION_CHECKLIST.md  ← Pre-flight checklist for specs
│   ├── ATLAS_HANDOFF_BRIEF_CHECKLIST.md  ← How to annotate spec reliability (replaces predictions)
│   └── ATLAS_PREDICTION_CHECKLIST.md  ← Historical: prediction-writing guide (superseded by Handoff Brief)
│
├── Experiments/                       ← Evidence from 6 builds (chronological)
│   ├── 01-ClockDemo/                  ← Spec + Predictions + AAR + Parent-Child Guide
│   ├── 02-TugOfWar/                   ← Spec + Predictions (unscored)
│   ├── 03-MultiCanvas3D/              ← Spec + Predictions
│   ├── 04-MultiCanvas2D/              ← Spec + Predictions
│   ├── 05-SpacialFrameTest/           ← Spec + Predictions + AAR
│   └── 06-SpacialBoxTest/             ← Spec + Predictions + AAR (zero fix rounds)
│
├── ShapeLifecycleTest/                ← Standalone reverse-engineered spec
│
└── Archive/                           ← Superseded by experiment-driven approach
    ├── LLM_Generation_Instructions.md
    ├── Levels/
    └── Skills/
```

## The Three Methodology Documents

### [SAGE_ROLE_DESCRIPTION.md](Methodology/SAGE_ROLE_DESCRIPTION.md)
Defines Sage's mission, process (5 steps), output format (AAR template), and principles. The best single document for understanding the methodology.

### [ATLAS_SPECIFICATION_CHECKLIST.md](Methodology/ATLAS_SPECIFICATION_CHECKLIST.md)
Living checklist accumulated across 6 builds. Covers research phases, required spec sections, service integration audits, and quality checks.

### [ATLAS_HANDOFF_BRIEF_CHECKLIST.md](Methodology/ATLAS_HANDOFF_BRIEF_CHECKLIST.md)
Replaces the prediction document. Instead of forecasting what will go wrong (10-50% accurate), Atlas honestly annotates each section of the spec as VERIFIED, ASSUMED, INFERRED, or UNREAD. Includes required sections: verification inventory, integration seam annotations, service implementation status, spec weakness confessions, and Indy's verification priority list.

### [ATLAS_PREDICTION_CHECKLIST.md](Methodology/ATLAS_PREDICTION_CHECKLIST.md)
Historical prediction-writing guide with empirical calibration data. Superseded by the Handoff Brief, but preserved as evidence of the methodology's evolution. Contains the insight that led to the switch: prediction accuracy was a vanity metric; honest verification reporting is what actually helped Indy.
