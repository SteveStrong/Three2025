# LLM-Driven Model Construction: The Core Insight

**Date**: January 1, 2026  
**Breakthrough**: Build an API simple enough for LLMs to call, powerful enough to construct sophisticated knowledge models

---

## The Pattern That Changes Everything

### Traditional Approach (WRONG)
```
User: "Create a beam model"
  ↓
Agent writes C# code:
  var beam = CreateShape(KnowledgeType.Concept, "Beam");
  var length = CreateShape(KnowledgeType.Property, "Length");
  play.Attach(length, beam);
```

**Problem**: Agent is writing code, not calling APIs. Can't deploy this, can't iterate, can't learn from mistakes.

### LLM-Driven Approach (CORRECT)
```
User: "Create a beam model with length, width, and height"
  ↓
LLM reasons:
  "I need a Concept for Beam"
  "I need Properties for dimensions"
  "Properties must attach to Concepts"
  ↓
LLM calls Mentor2DTech API:
  → CreateKnowledgeShape(type="Concept", title="Beam", x=100, y=100)
    Returns: { success: true, name: "Beam_1", shapeId: "abc123" }
    
  → CreateKnowledgeShape(type="Property", title="Length", x=150, y=150)
    Returns: { success: true, name: "Length_1", shapeId: "def456" }
    
  → AttachShape(childName="Length_1", parentName="Beam_1")
    Returns: { success: true, attachmentType: "contained", message: "Property attached to Concept" }
    
  → CreateKnowledgeShape(type="Property", title="Width", x=200, y=150)
  → AttachShape(childName="Width_1", parentName="Beam_1")
  
  → CreateKnowledgeShape(type="Property", title="Height", x=250, y=150)
  → AttachShape(childName="Height_1", parentName="Beam_1")
  ↓
Behind the scenes (AUTOMATIC via events):
  → KnConcept("Beam_1") created in knowledge model
  → KnProperty("Length_1") created and slotted into Beam_1.Members<KnProperty>()
  → KnProperty("Width_1") created and slotted into Beam_1.Members<KnProperty>()
  → KnProperty("Height_1") created and slotted into Beam_1.Members<KnProperty>()
  → MentorShape2D objects appear on canvas
  → Lookup table maintains bidirectional ShapeId ↔ KnBase mapping
  ↓
LLM now uses the model:
  → CreateKnowledgeShape(type="Formula", title="Length * Width * Height", ...)
  → AttachShape(childName="Formula_1", parentName="Beam_1")
  
Model evaluates formula, LLM gets result, problem solved!
```

---

## Why This Is Genius

### 1. **Simple API Surface**
Only 4 core methods needed:
- `CreateKnowledgeShape(type, title, x, y)` → Creates shape + knowledge object
- `AttachShape(childName, parentName)` → Attaches with rules checking
- `GetAllShapes()` → Query current state
- `CanAttach(child, parent)` → Validate before attempting

LLM can reason about these easily. No complex C# syntax, no knowledge of internal architecture.

### 2. **Automatic Assembly**
The event-driven architecture (MentorModelManager) handles ALL the complexity:
- DrawingEditChanged.Created → AddKnowledge<T>() called automatically
- DrawingEditChanged.ChildAdded → Group<T,U>() called automatically
- DrawingEditChanged.Connected → Connect<T,U>() called automatically
- Lookup table maintained automatically
- Model relationships assembled automatically

LLM just calls simple methods, sophisticated model appears "magically."

### 3. **Rule Enforcement**
MentorShape2D.IsDropAllowed() and IsConnectAllowed() prevent invalid constructions:
```csharp
AttachShape("Property_1", "Property_2")
  → Returns: { success: false, error: "Cannot attach Property to Property. Properties can only attach to Concept, Context, Component, or Relation." }

LLM learns from error, tries again:

AttachShape("Property_1", "Concept_1")
  → Returns: { success: true }
```

Rules are encoded in the system, not in LLM prompts. LLM learns by trying and getting feedback.

### 4. **Human Correction Loop**
When LLM makes mistakes:

```
LLM attempts:
  CreateKnowledgeShape("Formula", "Volume")
  AttachShape("Volume", "Beam_1")  ❌ (Formulas can't attach to Concepts directly)
  
Human sees visual result, corrects:
  Drags "Volume" Formula onto "Length" Property instead
  
System captures correction:
  {
    llmAttempted: AttachShape("Volume", "Beam_1"),
    humanCorrected: AttachShape("Volume", "Length_1"),
    reason: "Formulas attach to Properties or Roles, not Concepts"
  }
  
Next LLM call includes correction context:
  "Previously you tried to attach Formula to Concept, but Formulas attach to Properties or Roles."
  
LLM doesn't repeat mistake!
```

### 5. **Problem-Solving Substrate**
The constructed model becomes **computational infrastructure**:

```
User: "What's the volume of a beam 5m × 0.2m × 0.3m?"

LLM has already built model with Volume formula.
LLM calls:
  → SetPropertyValue("Length_1", 5.0)
  → SetPropertyValue("Width_1", 0.2)
  → SetPropertyValue("Height_1", 0.3)
  → EvaluateFormula("Volume_1")
  
Returns: 0.3 m³

LLM responds: "The beam volume is 0.3 cubic meters."
```

No domain-specific code written. Knowledge model + formulas = solver.

---

## Infrastructure: Proven and Production-Ready

### MentorWorkbook + MentorPlayground Pattern

**Discovery**: The infrastructure already exists in `FoundryMentorModeler` library!

```csharp
// MentorWorkbook - Multi-page drawing environment
public class MentorWorkbook : FoWorkbook, IMentorWorkbook
{
    private IDrawing Drawing { get; set; }  // Standard FoDrawing2D
    private IMentorPlayground Playground { get; set; }
    
    public MentorWorkbook(IWorkspace space, IFoundryService foundry)
    {
        Drawing = space.GetDrawing()!;  // Uses existing drawing infrastructure!
        
        // Creates pages for different contexts
        EstablishCurrentPage<FoPage2D>("Definitions", "orange");
        EstablishCurrentPage<FoPage2D>("Mentor", "grey");
    }
}

// MentorPlayground - Shape creation factory
public class MentorPlayground : KnBase, IMentorPlayground
{
    public MentorShape2D CreateShape<T>(string title="") where T : KnBase
    {
        // 1. Create knowledge object
        var item = Activator.CreateInstance(typeof(T), name) as T;
        ModelManager.AddKnowledge<T>(item);
        
        // 2. Create visual shape
        var shape = CreateNodeShape<MentorShape2D>(item);
        
        // 3. Add to page (automatic rendering)
        Drawing.FirstPage().Add(shape);
        
        // 4. Publish event
        PubSub.Publish<DrawingEditChanged>(DrawingEditChanged.Created(shape));
        
        return shape;
    }
}
```

**Key Benefits**:
- ✅ **No specialized drawing class needed** - Uses standard `FoDrawing2D`
- ✅ **Automatic shape-model linking** - `CreateShape<T>()` creates both at once
- ✅ **Event-driven assembly** - `DrawingEditChanged` events trigger model updates
- ✅ **Multi-page support** - Different pages for different contexts
- ✅ **Rule enforcement** - `IsDropAllowed()` / `IsConnectAllowed()` prevent invalid constructions
- ✅ **Proven in production** - Already used in existing mentor applications

### The CreateShape Magic

One method call creates:
1. **Knowledge model object** (`KnConcept`, `KnProperty`, etc.)
2. **Visual shape** (`MentorShape2D`)
3. **Bidirectional link** (shape ↔ model via `ModelManager.Lookup` table)
4. **Canvas rendering** (automatic via animation loop)
5. **Event notification** (observers can learn from action)

LLM just needs to call `CreateKnowledgeShape("Concept", "Beam", 100, 100)` - the rest happens automatically!

---

## Development Strategy: Test-Driven with North Star

### The North Star Code

```csharp
// This C# code is the NORTH STAR - what the model SHOULD look like
private void CreateBeamModel()
{
    var play = Playground;
    
    var beam = CreateShape(KnowledgeType.Concept, "Beam");
    beam.MoveTo(100, 100);
    
    var length = CreateShape(KnowledgeType.Property, "Length");
    play.Attach(length, beam);
    
    var width = CreateShape(KnowledgeType.Property, "Width");
    play.Attach(width, beam);
    
    var height = CreateShape(KnowledgeType.Property, "Height");
    play.Attach(height, beam);
    
    var volume = CreateShape(KnowledgeType.Formula, "Length * Width * Height");
    play.Attach(volume, length);  // Note: Formulas attach to Properties!
    
    var material = CreateShape(KnowledgeType.Concept, "Material");
    material.MoveTo(400, 100);
    
    var density = CreateShape(KnowledgeType.Property, "Density");
    play.Attach(density, material);
    
    play.Attach(beam, material);  // Beam uses Material (inheritance)
    
    var mass = CreateShape(KnowledgeType.Formula, "Volume * Density@");
    play.Attach(mass, beam);
}
```

**This code is NOT executed by the LLM**. This code is the **test oracle**.

### The LLM Prompt

```
You are building a knowledge model to solve a beam analysis problem.

The problem: Calculate the mass of a steel beam that is 5 meters long, 20cm wide, and 30cm tall. Steel density is 7850 kg/m³.

Use the Mentor2DTech API to:
1. Create concepts for Beam and Material
2. Add properties for dimensions (Length, Width, Height) and Density
3. Create formulas to calculate Volume and Mass
4. Structure the model correctly so formulas can reference properties

Available API methods:
- CreateKnowledgeShape(type, title, x, y)
- AttachShape(childName, parentName)
- GetAllShapes()
- CanAttach(child, parent)
```

### The Test

After LLM completes API calls, we:

1. **Inspect the knowledge model** (via MentorModelManager.Lookup):
   - Does Beam concept exist?
   - Does it have 3 properties: Length, Width, Height?
   - Does Material concept exist with Density property?
   - Are formulas attached correctly?

2. **Compare to North Star**:
   - Structure matches? ✓
   - Attachments valid? ✓
   - Formula expressions correct? ✓

3. **Functional Test**:
   - Set property values
   - Evaluate formulas
   - Does mass calculation return 2355 kg? ✓

### When LLM Fails

**Failure**: LLM attaches Volume Formula to Beam Concept (invalid)

**Detection**: AttachShape() returns error: "Cannot attach Formula to Concept"

**Correction Path**:

Option 1: LLM self-corrects from error message
```
Error: "Cannot attach Formula to Concept. Formulas attach to Properties or Roles."
LLM reasons: "I should attach to a Property instead"
LLM tries: AttachShape("Volume_1", "Length_1") ✓
```

Option 2: Human corrects visually
```
Human drags Formula from Concept to Property
System captures: {
  what_llm_did: AttachShape("Volume_1", "Beam_1"),
  what_human_did: AttachShape("Volume_1", "Length_1"),
  explanation: "Formulas go on Properties, not parent Concepts"
}
Next LLM call enriched with this lesson
```

---

## Implementation Phases (Hours, Not Days!)

### Phase 1: LLM-Callable API (4-6 hours)

**Goal**: Mentor2DTech exposes simple methods LLM can call

**Core Methods**:
```csharp
OperationResult<MentorShapeInfo> CreateKnowledgeShape(string type, string title, int x, int y);
OperationResult<AttachmentResult> AttachShape(string childName, string parentName);
List<MentorShapeInfo> GetAllShapes();
ValidationResult CanAttach(string childName, string parentName);
```

**DTOs**:
```csharp
public record OperationResult<T>(
    bool Success,
    T? Data,
    string? ErrorMessage,
    string? Suggestion  // "Try attaching to a Concept instead"
);

public record MentorShapeInfo(
    string Name,          // "Beam_1"
    string Type,          // "Concept"
    string ShapeId,       // For internal tracking
    double X, double Y,
    List<string> Children
);
```

**Test**: Can call these methods from C# and build beam model?

### Phase 2: Function Calling Schema (2-3 hours)

**Goal**: LLM can discover and call these methods via OpenAI/Anthropic function calling

**OpenAI Function Schema**:
```json
{
  "name": "create_knowledge_shape",
  "description": "Create a knowledge shape (Concept, Property, Formula, etc.) on the 2D canvas. Returns the auto-generated name you must use to reference this shape in subsequent calls.",
  "parameters": {
    "type": "object",
    "properties": {
      "type": {
        "type": "string",
        "enum": ["Concept", "Property", "Formula", "Role", "Context", "Component", "Feature", "Relation", "Variable", "ValidValues"],
        "description": "Knowledge type - determines visual style and attachment rules"
      },
      "title": {
        "type": "string",
        "description": "Display label for the shape"
      },
      "x": { "type": "integer", "description": "X position in pixels" },
      "y": { "type": "integer", "description": "Y position in pixels" }
    },
    "required": ["type", "title", "x", "y"]
  }
}
```

**Test**: LLM can call function and get back shape name?

### Phase 3: System Prompt Engineering (2-3 hours)

**Goal**: LLM understands when/how to use API

**System Prompt Template**:
```
You are a knowledge modeling assistant. Users describe problems, you build knowledge models using the Mentor2DTech API.

REASONING PROCESS:
1. Parse user's domain (e.g., "beam analysis")
2. Identify entities → Concepts (Beam, Material)
3. Identify properties → Properties (Length, Width, Density)
4. Identify calculations → Formulas (Volume = L×W×H)
5. Plan structure (which attaches to what)
6. Call API to build model
7. Validate with GetAllShapes()
8. Use model to solve problem

CRITICAL ATTACHMENT RULES:
- Property attaches to: Concept, Context, Component, Relation ✓
- Formula attaches to: Property, Role ✓
- Concept attaches to: Concept (inheritance) ✓
- Role attaches to: Role (hierarchy) ✓

When attachment fails, API tells you why. Reason about the feedback and try different structure.

EXAMPLE:
User: "Calculate mass of 5m steel beam, 20cm × 30cm cross-section, density 7850 kg/m³"

Think:
- Need Beam Concept with dimension Properties
- Need Material Concept with Density Property
- Need Volume Formula (L×W×H)
- Need Mass Formula (V×D)
- Formulas attach to Properties, not Concepts!

Execute:
1. create_knowledge_shape(type="Concept", title="Beam", x=100, y=100)
   → Returns: { name: "Beam_1" }
2. create_knowledge_shape(type="Property", title="Length", x=150, y=150)
   → Returns: { name: "Length_1" }
3. attach_shape(child="Length_1", parent="Beam_1")
   → Success
...
```

**Test**: Give LLM beam problem, does it call API correctly?

### Phase 4: Visual Verification UI (3-4 hours)

**Goal**: See what LLM built, compare to North Star

**UI Features**:
- Show API call sequence in log panel
- Highlight shapes in creation order
- Display LLM's reasoning annotations
- Button: "Compare to Expected Model"
- Visual diff: Green=correct, Red=wrong, Yellow=sub-optimal

**Test**: Human can quickly verify LLM's model structure

### Phase 5: Correction Learning (2-3 hours)

**Goal**: When LLM makes mistakes, human corrections teach it

**Correction Capture**:
```csharp
public record ModelCorrection(
    string UserPrompt,
    List<ApiCall> LLMAttempts,
    List<HumanEdit> Corrections,
    string GeneratedExplanation
);
```

**Learning Loop**:
1. LLM builds model → Error in structure
2. Human corrects visually
3. System generates explanation of correction
4. Next LLM call includes correction context
5. LLM doesn't repeat mistake

**Test**: Give same problem twice, verify LLM learns from first correction

---

## Success Criteria

### Milestone 1: API Works
✅ Can call Mentor2DTech.CreateKnowledgeShape() from C#  
✅ Can call Mentor2DTech.AttachShape() from C#  
✅ Invalid attachments return clear error messages  
✅ Model assembles automatically via events  
✅ North Star test passes (model structure matches expected)

### Milestone 2: LLM Can Call API
✅ LLM function calling schema defined  
✅ LLM successfully calls CreateKnowledgeShape()  
✅ LLM receives function results correctly  
✅ LLM can query model state with GetAllShapes()  
✅ LLM self-corrects from error messages

### Milestone 3: End-to-End Problem Solving
✅ User: "Calculate beam mass: 5m × 0.2m × 0.3m, steel, 7850 kg/m³"  
✅ LLM reasons about needed model structure  
✅ LLM calls API to build Concepts, Properties, Formulas (10-12 calls)  
✅ Model materializes automatically via events  
✅ LLM uses model to calculate answer  
✅ LLM responds: "The beam mass is 2355 kg"  
✅ Model structure matches North Star code

### Milestone 4: Learning from Corrections
✅ LLM makes structural error (e.g., wrong attachment)  
✅ Human corrects visually  
✅ System captures correction as structured diff  
✅ System generates explanation  
✅ Next LLM call includes correction context  
✅ LLM doesn't repeat same mistake  
✅ Accuracy improves over 3-5 correction cycles

---

## The Fundamental Shift

**Old thinking**: "How do I teach the LLM to write C# code to build models?"  
❌ Wrong direction. Code generation is fragile, can't deploy, can't learn from mistakes.

**New thinking**: "How do I design an API the LLM can reason about and call?"  
✅ Correct direction. Function calling is robust, deployable, learns from feedback.

**Key insight**: The developer (me) writes the North Star code as a test oracle. The LLM doesn't write code - it **calls APIs** to achieve the same result. When it fails, error messages and human corrections teach it.

---

## Timeline: Hours, Not Days

- **Phase 1**: 4-6 hours (LLM-callable API in Mentor2DTech)
- **Phase 2**: 2-3 hours (Function calling schemas)
- **Phase 3**: 2-3 hours (System prompt engineering)
- **Phase 4**: 3-4 hours (Verification UI)
- **Phase 5**: 2-3 hours (Correction learning)

**Total**: 13-19 hours of focused work = 2-3 days

**Why so fast?**
- Event-driven architecture already exists (MentorModelManager)
- Visual syntax rules already encoded (IsDropAllowed/IsConnectAllowed)
- MentorPlayground pattern already proven (CreateShape/Attach)
- Just exposing existing power to LLM via thin API wrapper!

---

## Next Steps

1. ✅ Document this insight (THIS FILE)
2. ⬜ Update CONVERSATIONAL_MENTOR_IMPLEMENTATION_PLAN.md to reflect LLM-driven approach
3. ⬜ Create North Star test suite (BeamModel, StrategicPlan, etc.)
4. ⬜ Implement Phase 1: Mentor2DTech API
5. ⬜ Test with hardcoded API calls (prove architecture)
6. ⬜ Implement Phase 2: Function calling schemas
7. ⬜ Test with LLM (prove it can call APIs)
8. ⬜ Iterate on system prompt until beam problem solves correctly
9. ⬜ Add correction learning loop
10. ⬜ Celebrate genius architecture! 🎉
