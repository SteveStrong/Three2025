# AI Tool Debugging - Lessons Learned

**Date**: January 4, 2026  
**Context**: Debugging color change failures in 3D shape manipulation via AI agent

---

## Problem Summary

User requested: "Change cube1 to red, then orange, then yellow, then green, then blue, then purple"

**Expected**: All 6 color changes execute successfully  
**Actual**: First 2 worked (red, orange), then remaining colors failed

---

## Root Causes Identified

### 1. Tool Return Type Serialization Issue

**Problem**: AI tool methods were returning raw objects (FoShape3D, List<T>) instead of OPResult.

**Impact**: Microsoft.Extensions.AI JSON-serializes tool return values for LLM consumption. When serializing complex domain objects, the LLM received meaningless internal metadata like:

```json
{
  "operatorToken": {"text": "Success", "kind": 29, ...},
  "resultType": 0
}
```

The LLM could see that *something* happened but couldn't understand whether it succeeded or failed.

**Solution**: ALL technician methods now return `OPResult` with public serialization properties:
- `ResultType` - "Success", "Error", "Shape3D", etc.
- `HasError` - boolean
- `ResultMessage` - Human-readable description

### 2. Private Fields Not Serialized

**Problem**: OPResult used private fields (`_value`, `_type`) that JSON serialization couldn't access.

**Impact**: Even when returning OPResult, the LLM couldn't see the actual results.

**Solution**: Added public properties that expose the data in LLM-friendly format:

```csharp
public string ResultType => _type.ToString();
public bool HasError => _type == ResultStatus.Error || ...;
public string ResultMessage => FormatResultForLLM();
```

### 3. Over-Cautious Agent System Prompt

**Problem**: System prompt said "verify before acting" which made LLM call `GetShapes()` before EVERY operation.

**Impact**: Instead of calling `ChangeColor('cube1', 'green')`, the LLM would call `GetShapes()` to "verify" the shape exists - even when the user explicitly named it.

**Solution**: Updated system prompt to distinguish explicit names from ambiguous references:

```markdown
## CRITICAL: Explicit Names vs Ambiguous References

**Step 0: Check if the user provided an EXPLICIT SHAPE NAME**
- If user says "Make cube1 green" → The name IS "cube1" - CALL ChangeColor('cube1', 'green') IMMEDIATELY
- DO NOT call GetShapes() when an explicit name is provided!

**Only call GetShapes() for AMBIGUOUS references:**
- "Move it" → Who is "it"? Check history or call GetShapes()
```

---

## Debugging Techniques Used

### 1. Log Analysis

Console logging at tool execution points revealed:
```
🔧 Executing tool: GetShapes     ← Expected: ChangeColor
```

This showed the LLM was calling the wrong tool.

### 2. JSON Serialization Inspection

Logging the actual serialized tool result showed the LLM was receiving cryptic metadata instead of meaningful feedback.

### 3. Prompt Engineering Iteration

Multiple iterations of the system prompt were needed to make the "explicit name = direct action" rule clear enough for the LLM to follow.

---

## Architecture Patterns Established

### 1. OPResult as Standard Return Type

```csharp
public interface ISomeTech : ITechnician
{
    // ALL methods return OPResult - no exceptions!
    OPResult DoSomething(string param);
}
```

### 2. OPResult Factory Methods

| Method | Usage |
|--------|-------|
| `OPResult.Success(msg)` | Operation completed |
| `OPResult.Error(msg)` | Operation failed |
| `OPResult.Object(val)` | Return object with success |
| `OPResult.Collection(list)` | Return list of items |

### 3. Agent Prompt Pattern

```markdown
## When to verify vs when to act

✅ Explicit name provided → Call tool directly
✅ Pronoun used ("it", "that") → Check conversation history first
✅ Ambiguous reference → Then call query tools
```

---

## Files Modified

| File | Change |
|------|--------|
| `OpResult.cs` | Added public serialization properties |
| `Shape3DTech.cs` | All methods return OPResult |
| `IShape3DTech.cs` | Interface updated for OPResult |
| `ThreeDModelingAgent.cs` | System prompts updated |
| `AI_TOOL_INTEGRATION_GUIDE.md` | Added OPResult section |
| `AI_AGENT_SHAPE_TOOL_SPECIFICATION.md` | Updated return type guidance |
| `MULTI_AGENT_CHATBOT_INFRASTRUCTURE_SPEC.md` | Added Appendix C |

---

## Key Takeaways

1. **JSON serialization matters** - Private fields are invisible to LLMs
2. **Consistent return types** - ALL tool methods must use same pattern
3. **System prompts control behavior** - Vague instructions cause unexpected behavior
4. **Explicit is better than implicit** - LLMs follow instructions literally
5. **Log everything** - Console logging was essential for diagnosis

---

## Related Documentation

- [AI_TOOL_INTEGRATION_GUIDE.md](AI_TOOL_INTEGRATION_GUIDE.md) - Complete tool integration patterns
- [AI_AGENT_SHAPE_TOOL_SPECIFICATION.md](AI_AGENT_SHAPE_TOOL_SPECIFICATION.md) - Shape tool developer guide
- [MULTI_AGENT_CHATBOT_INFRASTRUCTURE_SPEC.md](MULTI_AGENT_CHATBOT_INFRASTRUCTURE_SPEC.md) - Agent orchestration
