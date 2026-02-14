# Atlas Handoff Brief Checklist

**Purpose:** Guide for Atlas to write an honest handoff brief after completing a specification  
**Replaces:** `ATLAS_PREDICTION_CHECKLIST.md` (predictions retired — see rationale below)  
**Created:** February 13, 2026  
**Based On:** 6 experiments where prediction accuracy (10-50%) didn't matter but spec thoroughness did

---

## Why Handoff Briefs Replace Predictions

Predictions asked: *"What will go wrong?"* — a forecast that was wrong 50-90% of the time.

Handoff Briefs ask: *"What did I actually verify?"* — a statement of fact that's always truthful.

The evidence from 6 Atlas/Indy/Sage experiments:
- Prediction accuracy never exceeded 50%, yet build quality improved steadily
- The improvements came from *mitigations Atlas put into the spec* while thinking about predictions, not from the predictions themselves
- The highest-value predictions were "spec self-audits" — Atlas confessing where the spec was weak
- Indy's 100% override accuracy showed that Indy doesn't need forecasts; Indy needs an honest reliability map of the spec

The Handoff Brief extracts the part that worked (forcing Atlas to annotate spec reliability) and drops the part that didn't (forecasting what would go wrong).

---

## What a Handoff Brief Is

A section-by-section annotation of the specification's own reliability. For every significant claim, method reference, architecture decision, or integration point in the spec, Atlas states one of:

| Annotation | Meaning | Symbol |
|---|---|---|
| **VERIFIED** | I read the source code and confirmed this | ✅ |
| **ASSUMED** | I believe this is correct but did not read the source | ⚠️ |
| **INFERRED** | I extrapolated from similar patterns; may be wrong | 🔶 |
| **UNREAD** | I did not open this file/service/implementation | ❌ |

The Handoff Brief is Atlas's honest answer to: *"If this spec is wrong somewhere, where is it most likely wrong?"*

---

## Phase 1: Before Writing the Handoff Brief

### 1.1 Reread Your Own Spec as Indy

Read the spec start to finish, in order, as if you're about to build from it.

- [ ] **Where did you stop and think?** Those spots need annotations.
- [ ] **Where did you write a method name from memory?** Mark as ASSUMED or VERIFIED.
- [ ] **Where did you reference a component or service?** Did you open it? Mark honestly.
- [ ] **Where did you describe behavior?** Did you trace the code path, or describe what you *think* it does?

### 1.2 Inventory What You Actually Opened

Be specific. List the files you read during spec research.

```markdown
## Files I Actually Opened During Research
- ✅ Services/Chat/MultiProviderChatService.cs — read lines 1-340
- ✅ Services/Chat/ChatOrchestrator.cs — read full file
- ⚠️ Services/Chat/Agents/GeneralAgent.cs — skimmed, didn't trace all paths
- ❌ Services/Agents/TechnicianToolProvider.cs — described from interface only
- ❌ Apprentice/Shape3DTech.cs — assumed from ITechnician pattern
```

This list is the single most valuable artifact in the Handoff Brief. When something breaks during Indy's build, this list tells Indy exactly where to look: at the files Atlas didn't open.

---

## Phase 2: Write the Handoff Brief

### 2.1 Required Sections

Every Handoff Brief must include:

1. **Verification Inventory** — files Atlas opened vs. didn't
2. **Method/API Verification Table** — every method referenced in the spec, marked ✅/⚠️/🔶/❌
3. **Integration Seam Annotations** — where new code meets existing systems, with honest assessment
4. **Service Implementation Status** — for every service the spec delegates to: did Atlas read the implementation or just the interface?
5. **Spec Weakness Confessions** — where Atlas knows the spec is probably wrong or incomplete
6. **What Indy Should Check First** — Atlas's honest recommendation for where to start verifying

### 2.2 Method/API Verification Table

For every method, class, or component referenced in the spec:

```markdown
## Verification Table

| Reference | Status | Evidence |
|---|---|---|
| `MultiProviderChatService.SendMessageStreamingAsync()` | ✅ VERIFIED | Read full implementation, lines 89-180 |
| `ChatOrchestrator.AnalyzeIntentAsync()` | ✅ VERIFIED | Read implementation, traced JSON parsing |
| `TechnicianToolProvider.DiscoverAllTools()` | ⚠️ ASSUMED | Read interface only; described behavior from method name |
| `AIFunctionFactory.Create()` | 🔶 INFERRED | Microsoft.Extensions.AI method; described from documentation, not source |
| `OPResult.AsToolResult()` | ⚠️ ASSUMED | Used in working code; didn't read implementation |
| `ChatClientAgent` | 🔶 INFERRED | Microsoft.Agents.AI preview; behavior inferred from usage patterns |
```

- [ ] **Every method in the spec appears in this table**
- [ ] **No VERIFIED claims without citing the file you read**
- [ ] **ASSUMED entries include why you didn't verify** (not critical? out of scope? couldn't find source?)

### 2.3 Integration Seam Annotations

For every point where the specified system connects to existing infrastructure:

```markdown
## Integration Seams

### Seam: Tool Discovery ↔ DI Container
- **Connection:** TechnicianToolProvider calls IServiceProvider.GetService() to resolve ITechnician implementations at runtime
- **Atlas's knowledge:** ⚠️ ASSUMED — described DI resolution from interface; did not read TechnicianToolProvider implementation
- **What could go wrong:** If technicians aren't registered as Scoped, resolution may fail silently or throw
- **Indy should check:** Open TechnicianToolProvider.cs and verify how it handles unresolvable technicians

### Seam: Streaming Response ↔ Blazor UI Thread
- **Connection:** async IAsyncEnumerable from chat service consumed on Blazor render thread
- **Atlas's knowledge:** ✅ VERIFIED — read the page's ProcessSingleMessage method
- **What could go wrong:** StateHasChanged() during streaming may be throttled
```

- [ ] **At least 3 integration seams identified**
- [ ] **Each seam annotated with Atlas's actual knowledge level**
- [ ] **Investigation starting point provided for each**

### 2.4 Service Implementation Status

For every service the spec delegates to — **this is the most critical section.** (SpacialFrameTest lesson: Atlas's 90% confidence prediction failed because Atlas never read 30 lines of service implementation.)

```markdown
## Service Implementation Status

| Service | Interface Read? | Implementation Read? | Routing Concerns |
|---|---|---|---|
| `MultiProviderChatService` | ✅ | ✅ Lines 1-340 | Routes to active provider's IChatClient |
| `ChatOrchestrator` | ✅ | ✅ Full file | Intent analysis hardcodes coordinator prompt |
| `TechnicianToolProvider` | ✅ | ❌ NOT READ | Unknown: Does it cache? Handle failures? Filter methods? |
| `AgentFactory` | ✅ | ⚠️ Skimmed | Appears to create agents with injected tools |
```

- [ ] **If "Implementation Read?" is ❌ or ⚠️, this IS the highest-risk area.** Say so explicitly.
- [ ] **Never write "battle-tested, no surprises" for a service you haven't read.**

### 2.5 Spec Weakness Confessions

The most honest section. Atlas lists where the spec is probably wrong or incomplete.

```markdown
## Where This Spec Is Probably Wrong

1. **OPResult dependency chain** — I described OPResult as having "deep dependencies" but didn't trace the actual dependency tree. The simplification recommendation (replace with plain strings) is my best guess, not verified.

2. **Microsoft.Agents.AI usage** — I described ChatClientAgent from usage patterns in MultiProviderChatService. This is a preview package; the API may differ from what I described.

3. **Duplication between Three2025 and FoundryMicroCore.Blazor.Controls** — I said they're "structurally identical" based on file names. I did NOT diff the implementations. They may have diverged.
```

- [ ] **At least 2 confessions** (if you can't think of any, you haven't looked hard enough)
- [ ] **Each confession includes what Indy should do to verify**
- [ ] **No defensive framing** — don't say "this might be slightly different." Say "I didn't check."

### 2.6 What Indy Should Check First

Atlas's recommendation for Indy's first 15 minutes, based on where the spec is weakest:

```markdown
## Indy's Verification Priority

1. **Open TechnicianToolProvider.cs** — I didn't read it. The tool discovery pipeline description is inferred from the interface. If tools don't appear, start here.

2. **Diff the two ChatPanel implementations** — Three2025 vs FoundryMicroCore.Blazor.Controls. I claimed they're identical. Verify before choosing which to use.

3. **Test OPResult.AsToolResult() with a plain string return** — I recommended replacing OPResult with strings but didn't verify that AIFunctionFactory handles the conversion.
```

---

## Phase 3: Quality Check

Before attaching the Handoff Brief to the spec:

- [ ] **Every ✅ VERIFIED has a file citation** — if you can't cite the file, change it to ⚠️ ASSUMED
- [ ] **The "Files I Actually Opened" list is honest** — no padding
- [ ] **At least 2 spec weakness confessions** — the most honest thing you can write
- [ ] **Service implementations are flagged if unread** — this is where the showstopper lives
- [ ] **No forecasting language** — no "I predict", no "X% likely", no confidence percentages on future outcomes
- [ ] **Indy verification priorities are concrete** — file names, not abstract concerns

---

## What Sage Evaluates (After Indy Builds)

The After-Action Review shifts from scoring predictions to evaluating honesty:

| Old (Prediction-based) | New (Handoff Brief-based) |
|---|---|
| Was the prediction correct? | Was the annotation honest? |
| What did Atlas get right? | Did VERIFIED items actually work? |
| What did Atlas get wrong? | Were the actual problems in ASSUMED/UNREAD areas? |
| What was unpredicted? | What sections had no annotations? (Gaps in the brief itself) |

The goal isn't Atlas predicting the future correctly. The goal is Atlas honestly reporting the present — what was checked, what wasn't, and where Indy should be cautious.

---

## Relationship to the Specification Checklist

The Handoff Brief is the **companion document** to the specification. The spec tells Indy *what to build*. The Handoff Brief tells Indy *how much to trust each part of the spec*.

The `ATLAS_SPECIFICATION_CHECKLIST.md` remains unchanged — it governs spec quality. The Handoff Brief Checklist governs the handoff annotation that follows spec completion.

**Workflow:**
1. Atlas writes the spec (using Specification Checklist)
2. Atlas writes the Handoff Brief (using this checklist)
3. Indy reads both — builds from the spec, uses the brief to prioritize verification
4. Sage writes the AAR — evaluates brief honesty alongside build outcomes

---

## The One Thing to Remember

**You can't reliably predict where Indy will struggle. You CAN reliably report what you actually verified.**

The shift from prediction to annotation isn't about lower ambition. It's about higher honesty. Six experiments proved that Atlas's forecasts had a 10-50% hit rate. Atlas's factual reports about what was read vs. not read would have had a 100% accuracy rate — because they're statements about the past, not the future.

Be honest about what you checked. Be honest about what you didn't. That's the handoff.
