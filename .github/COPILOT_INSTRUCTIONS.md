# GitHub Copilot Collaboration Guidelines

## Core Principle: Suggest First, Implement Second

When working on this codebase, always distinguish between **fixing the immediate problem** and **suggesting improvements**.

---

## Before Making Changes

### ✅ DO: Fix the Immediate Problem
- Trace back to find what broke
- Work within existing architecture
- Use existing patterns and methods
- Verify the fix works

### ⚠️ STOP & ASK: Before Adding New Code
When you're about to:
- Add new hooks, methods, or classes
- Change existing architecture or patterns
- "Improve" something that wasn't part of the request
- Add features beyond the immediate fix

**Stop and suggest instead:**
```
"I noticed [observation]. Should we [proposed change], 
or does [existing code] already handle this?"
```

Wait for confirmation before implementing.

---

## Debugging Philosophy

### Trace Back, Don't Redesign
- When something breaks, find what changed
- Check if recent refactors affected the flow
- Verify existing mechanisms still work
- Don't assume the original design was wrong

### Example: "Pipe Not Rendering"
**❌ Wrong Approach:**
- Add a PreComputeMesh hook to fix it
- Assume the pipe needs help computing its path

**✅ Right Approach:**
1. "Pipe isn't rendering. Let me check what changed."
2. "We modified stale flag clearing - does that affect the pipe?"
3. "I see boxes call SetGeometryStale() on the pipe - is that still working?"
4. "The pipe has RecomputeMesh() with FromShape3D/ToShape3D - does this already handle path updates?"

---

## Architecture Respect

### Work Within Existing Patterns
- **Animation System**: BeforeAnimationRefresh callbacks, SetGeometryStale() marking
- **3-Wave Batching**: Independents → Boundaries → Dependents
- **Stale Flags**: Natural clearing as objects are processed
- **Stage Pattern**: Arena → Stages → Shapes
- **Event Bus**: ComponentBus pub/sub for animation frames

### Don't Second-Guess the Design
If you notice something that seems inefficient or unusual:
1. Consider there might be a reason for it
2. Ask: "I notice X does Y. Is there a reason for this pattern?"
3. Learn from the answer
4. Apply that knowledge to future work

---

## Communication Style

### When Suggesting Improvements
- Be specific about what you noticed
- Explain your reasoning
- Ask if the improvement is wanted
- Accept "no" gracefully

### Example Suggestion Format
```
"Hey, I notice [specific observation].

This could be improved by [specific suggestion].

However, I see [existing code] - does that already handle this?
Should we make the change, or is the current approach intentional?"
```

---

## Project-Specific Knowledge

### Key Architectural Decisions
1. **RecomputeBoundary Flag Persistence**: Never cleared by ClearAllStaleFlags() - it's a persistent opt-in
2. **Pipe Auto-Rebuild**: Pipes with FromShape3D/ToShape3D automatically rebuild geometry in RecomputeMesh()
3. **Frame Countdown**: RunForFrames() enables precise frame control for debugging
4. **Debug Mode**: StartPaused uses shorter animation duration (0.5s vs 5s) for visible stepping

### Common Patterns
- Objects mark dependent objects stale when they change
- Scene3D processes stale objects in 3 waves
- Flags cleared immediately after processing each object
- JavaScript callbacks use TaskCompletionSource for async coordination

---

## When In Doubt

**Ask these questions:**
1. Am I fixing the immediate problem, or adding a feature?
2. Does existing code already handle this?
3. Would the user expect this change?
4. Should I suggest this instead of implementing it?

**If any answer is uncertain → STOP AND ASK**

---

## Summary

✅ Fix bugs within existing architecture  
✅ Use existing patterns and methods  
✅ Trace back to find what broke  
⚠️ Suggest improvements before implementing  
⚠️ Ask about architectural changes  
⚠️ Verify assumptions about existing code  

**Remember: The user has a mental model of this codebase. Work with it, don't surprise them by changing it.**
