# Atlas Specification Checklist

**Purpose:** Pre-flight checklist for Claude "Atlas" (Architect) before delivering specifications to Claude "Indy" (Builder)  
**Based On:** ClockDemo After Action Review (January 31, 2026)  
**Updated:** February 1, 2026

---

## Phase 0: Preamble (ALWAYS INCLUDE THIS FIRST)

Every specification MUST begin with this preamble to set context for the implementer:

- [ ] **Copy the preamble template into spec** - Must be Section 0 or immediately after title
- [ ] **Customize feature name** - Replace `[FEATURE NAME]` placeholders
- [ ] **Customize testing objectives** - Add 2-3 feature-specific goals
- [ ] **Explain specific impact** - What this proves about persistent/interactive systems

**Preamble Template:**
````markdown
## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about building [FEATURE NAME] - it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 3D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you build [FEATURE NAME], you're testing whether AI can:
1. Successfully use 40 years of distilled programming patterns
2. [Add 2-3 feature-specific test objectives]
3. [...]

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can leverage accumulated human wisdom to create [feature-specific impact]. [Explain what this feature demonstrates about persistent/interactive capabilities].

**Now, let's see what treasures you can uncover.** 🗺️⚙️
````

**Why This Matters:**
- Sets proper context about the deeper mission
- Explains WHY we're building this, not just WHAT
- Gives Indy permission to be resourceful when specs aren't perfect
- Frames the work as part of a larger AI capability test

**CRITICAL:** This was mistakenly put in predictions document for ClockDemo. Always include in the spec itself!

---

## Phase 1: Research (Before Writing Specs)

### 1.1 Study Existing Patterns (Budget: 30 minutes)

- [ ] **Find 2-3 similar components** in the codebase
  - Search for components that implement similar features
  - Look for Test/Demo/Example files that show patterns in action
  - Document which files you examined

- [ ] **Identify the dominant architecture pattern**
  - Is it Model-first? Component-first? Tech-first?
  - What base classes are used? (ComponentBase, MxComponent, etc.)
  - How are lifecycle hooks managed? (OnInitialized, OnAfterRender, etc.)

- [ ] **Map the infrastructure layer**
  - How are stages created and managed?
  - How is animation subscription handled?
  - How are shapes added to scenes?
  - How are collections and editors used?

**Documentation Template:**
```markdown
## Architecture Analysis
Based on: [path/to/ReferenceFile1.cs], [path/to/ReferenceFile2.cs]

Current Pattern: [Pattern name, e.g., "Model-first with Editor pattern"]

Key Characteristics:
- Base class: [class name]
- Stage management: [how it works]
- Shape lifecycle: [how it works]
- Animation hookup: [how it works]

Files to study as reference:
1. [file1] - demonstrates [feature]
2. [file2] - demonstrates [feature]
```

### 1.2 Read API References (Budget: 15 minutes)

- [ ] **Locate relevant API documentation files**
  - `FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md`
  - `FOUNDRY_MENTORMODELER_API_REFERENCE.md`
  - `FOUNDRY_MICROCORE_BLAZOR_CONTROLS_API_REFERENCE.md`
  - Component-specific API docs in `/Docs` folders

- [ ] **Verify every method you plan to use**
  - Look up exact method names (don't assume!)
  - Verify parameter signatures
  - Check return types
  - Note any async requirements

- [ ] **Document API verification**

**Documentation Template:**
```markdown
## Verified Against
- API Reference: [path/to/API_REFERENCE.md]
- Last verified: [date]
- Version: [if applicable]

Method Verification:
- ✅ `MethodName()` - verified in [API doc section]
- ✅ `AnotherMethod()` - verified in [API doc section]
```

### 1.3 Review Code Smell Documentation (Budget: 10 minutes)

- [ ] **Read relevant code smell documents**
  - `FoundryMicroCore.Library/Core/docs/CODE_SMELLS_ANALYSIS.md`
  - `FoundryWorldsAndDrawings/Docs/CODE_SMELLS.md` (if exists)
  - Any component-specific smell documentation

- [ ] **Identify smells relevant to this task**
  - Which smells apply to collection operations?
  - Which smells apply to animation/rendering?
  - Which smells apply to Blazor lifecycle?

- [ ] **Note newly discovered patterns to watch for**

---

## Phase 2: Write Specification

### 2.1 Required Sections

Every specification must include:

- [ ] **1. Architecture Analysis** (from Phase 1.1)
- [ ] **2. Verified Against** (from Phase 1.2)
- [ ] **3. Reference Implementation Strategy**
- [ ] **4. Infrastructure Assumptions**
- [ ] **5. Code Path Traces** (for non-trivial mechanisms)
- [ ] **6. Code Smells to Avoid** (from Phase 1.3)
- [ ] **7. Known Gotchas**
- [ ] **8. Troubleshooting Guide**
- [ ] **9. Implementation Steps**
- [ ] **10. Success Criteria**

### 2.2 Reference Implementation Strategy

Instead of writing abstract pseudo-code, provide a copy-and-modify strategy:

```markdown
## Reference Implementation Strategy

### Primary Reference
Copy: `[path/to/WorkingExample.cs]`
Demonstrates: [what patterns it shows]

### Modification Steps
1. Copy `[source file]` to `[new file]`
2. Rename class from `[OldName]` to `[NewName]`
3. Replace `[specific section]` with `[new behavior]`
4. Add `[specific feature]` using pattern from `[other reference file]`

### Delta from Reference
What's different for this component:
- [Difference 1]: Because [reason]
- [Difference 2]: Because [reason]
```

- [ ] **Primary reference file identified and cited**
- [ ] **Specific line numbers for key patterns** (when helpful)
- [ ] **Delta from reference clearly explained**
- [ ] **No abstract pseudo-code without verification**

### 2.3 Infrastructure Assumptions

Make your assumptions explicit so Indy knows where to investigate if things break:

```markdown
## Infrastructure Assumptions

I'm assuming these work correctly. If not, investigate:

### Assumption: Stage Management Works
- [ ] `arena.EstablishStage<FoStage3D>("Name")` creates an isolated stage
- [ ] `stage.AddShape(shape)` adds shape to stage collection
- [ ] If broken: Check [file/class/method]

### Assumption: Parent-Child Hierarchies Work
- [ ] `parent.AddShape(child)` establishes hierarchy
- [ ] Child transforms are relative to parent
- [ ] Children render when parent renders
- [ ] If broken: Check `FoGlyph3DEditor.AddShape()` and `AllSubGlyph3Ds()`

### Assumption: Animation Subscription Works
- [ ] `AnimationFrameBus.SubscribeToAnimation()` gets called every frame
- [ ] Unsubscribe in Dispose prevents leaks
- [ ] If broken: Check [file/method]
```

- [ ] **Every non-trivial assumption documented**
- [ ] **Investigation starting point provided for each**
- [ ] **Organized by subsystem (Stage, Animation, Collections, etc.)**

### 2.4 Code Path Traces

For any non-trivial mechanism, trace the full code path:

```markdown
## Code Path Traces

### When you call `parent.AddShape(child)`
1. First, `FoGlyph3D.AddShape()` is invoked (virtual method)
2. This calls `editor.Add<FoGlyph3D>(child)` on the parent's editor
3. Editor stores child in `Collection<FoGlyph3D>` (base type)
4. Child's parent reference is set via `child.Parent = this`
5. Stale flag is set on parent: `SetTransformStale()`
6. During render, `AllSubGlyph3Ds()` traverses children from base collection

**Critical:** Must use `FoGlyph3DEditor` not generic `MxComponentEditor<FoGlyph3D>` 
or children won't be stored in correct base-type collection.

### When you call `shape.RotateTo(angle)`
1. `Transform3.Rotation` property is updated
2. `SetTransformStale()` is called on the shape
3. Shape is marked for update in next collection pass
4. During `CollectChanges()`, transform is serialized
5. JavaScript receives transform update via JSInterop
6. Three.js updates the mesh's rotation matrix
```

- [ ] **Every complex mechanism traced**
- [ ] **Step-by-step flow documented**
- [ ] **File/method references included**
- [ ] **Critical gotchas highlighted**

### 2.5 Code Smells to Avoid

Document both known smells and task-specific warnings:

```markdown
## Code Smells to Avoid

### From MicroCore (CODE_SMELLS_ANALYSIS.md)

#### #10 MxComponentEditor.Find() O(N) Performance
**Don't:**
```csharp
var item = editor.AllMembers().FirstOrDefault(x => x.Name == "foo");
```
**Do:**
```csharp
var item = editor.FindByName("foo"); // O(1) dictionary lookup
```

#### #13 GetAction() LINQ Inefficiency
**Don't:**
```csharp
var action = GetAllActions().FirstOrDefault(a => a.Name == name);
```
**Do:**
```csharp
var action = GetAction(name); // Direct dictionary access
```

### Task-Specific Warnings

#### Collection Type Mismatch
**Problem:** Generic `Add<T>()` infers concrete type; base-type queries miss items
**Solution:** Use specialized editors (`FoGlyph3DEditor`) that enforce base type storage
```csharp
// Don't: Generic editor
var editor = new MxComponentEditor<FoGlyph3D>(parent);
editor.Add(child); // Stores as concrete type

// Do: Specialized editor
var editor = new FoGlyph3DEditor(parent);
editor.AddShape(child); // Stores in base FoGlyph3D collection
```

#### Missing Stale Flags in 3D
**Problem:** Transform changes without `SetTransformStale()` don't render
**Solution:** Always call stale flag after transform updates
```csharp
shape.Transform.Position = new Vector3(x, y, z);
shape.SetTransformStale(); // Required!
```
```

- [ ] **Known smells from docs included with references**
- [ ] **Task-specific smells predicted and documented**
- [ ] **Code examples show both wrong and right way**
- [ ] **Explanations include WHY, not just WHAT**

### 2.6 Known Gotchas

Document non-obvious issues that will waste Indy's time:

```markdown
## Known Gotchas

### Parent-Child Shape Hierarchies
When adding a child shape to a parent:
- ✅ Use `parent.AddShape(child)` - it's virtual and polymorphic
- ❌ DO NOT use generic `MxComponentEditor<T>` directly
- ✅ Children must be in same collection type as parent traversal expects
- ✅ Verify with: `parent.AllSubGlyph3Ds()` should return your children
- 🔍 Debug: Check collection type with `parent.GetCollection<FoGlyph3D>()`

### Rotation Pivot Points
When rotating shapes that should pivot from one end (clock hands, doors):
- ❌ Default: Shape rotates around its center
- ✅ Solution: Offset geometry OR set `Transform.Pivot` to attachment point
- 🎯 Pattern: Door hinge (pivot at edge) vs spinning top (pivot at center)

### Animation Frame Timing
When subscribing to animation:
- ✅ Subscribe in `OnAfterRenderAsync(firstRender)`
- ❌ Don't subscribe in `OnInitializedAsync` (canvas not ready)
- ✅ Always unsubscribe in `Dispose()`
- 🔍 Debug: Check subscription count in AnimationFrameBus
```

- [ ] **Each gotcha includes symptoms**
- [ ] **Verification steps provided**
- [ ] **Debug strategies included**

### 2.7 Troubleshooting Guide

Provide diagnostic steps for common failures:

```markdown
## Troubleshooting Guide

### Shapes Not Appearing

**Symptom:** Shape created but not visible in scene

**Diagnosis Steps:**
1. **Check stage has the shape:** 
   ```csharp
   var shapes = stage.GetCollection<FoShape3D>();
   $"Shape count: {shapes.Count}".WriteInfo();
   ```
2. **Check stale flags:** 
   ```csharp
   var isStale = shape.IsStale();
   $"Shape stale: {isStale}".WriteInfo();
   ```
   Should be true initially to trigger mesh generation
3. **Check mesh generation:** 
   Add breakpoint in `RecomputeMesh()` or `GetComputedMesh()`
4. **Check visibility:** 
   Verify `shape.Visible = true` and `shape.Opacity > 0`

**Common Causes:**
- Shape added to wrong stage
- Transform scale is zero
- Shape is behind camera
- Canvas3D not initialized yet

---

### Child Shapes Not Rendering

**Symptom:** Parent visible but children invisible

**Diagnosis Steps:**
1. **Check parent has children:**
   ```csharp
   var children = parent.AllSubGlyph3Ds();
   $"Child count: {children.Count()}".WriteInfo();
   ```
2. **Check collection type match:**
   ```csharp
   var collection = parent.GetCollection<FoGlyph3D>();
   $"Collection count: {collection.Count}".WriteInfo();
   ```
   Should match children count
3. **Verify mesh traversal:**
   Add logging in `GetComputedMesh()` to see if it traverses children

**Common Causes:**
- Children stored in wrong collection type (concrete vs base)
- Using wrong editor (MxComponentEditor vs FoGlyph3DEditor)
- Children not marked stale
- Parent's `GetComputedMesh()` doesn't traverse children

---

### Rotation Not Updating

**Symptom:** Shape created, rotation called, but visual doesn't change

**Diagnosis Steps:**
1. **Verify rotation is being called:**
   ```csharp
   shape.RotateTo(angle);
   $"Rotated to {angle}".WriteInfo();
   ```
2. **Check stale flag triggered:**
   ```csharp
   shape.SetTransformStale(); // Add this if missing
   var isStale = shape.IsStale();
   ```
3. **Verify shape collected in changes:**
   Check if `CollectChanges()` includes this shape
4. **Check JavaScript receives update:**
   Add breakpoint in Canvas3D JSInterop to see transform data

**Common Causes:**
- Missing `SetTransformStale()` call
- Shape not in active stage's collection
- Rotation in wrong units (degrees vs radians)
- Rotation axis incorrect
```

- [ ] **Each issue has symptom description**
- [ ] **Diagnostic steps are code-based (not vague)**
- [ ] **Common causes listed**
- [ ] **Organized by problem category**

### 2.8 Implementation Steps

Provide a clear sequence with verification points:

```markdown
## Implementation Steps

### Step 1: Create Component Files
1. Create `ClockDemo.razor` with @page route and Canvas3D
2. Create `ClockDemo.razor.cs` with ComponentBase inheritance
3. Add using statements (verified from API docs)
4. **Verify:** Files compile with no errors

### Step 2: Setup Stage and Tech
1. Inject IFoundryService and IWorkspace
2. In OnInitializedAsync, create stage with `EstablishStage<FoStage3D>`
3. Create ClockDemoTech component inheriting MxComponent
4. Add tech to stage
5. **Verify:** Page loads without exceptions, stage exists

### Step 3: Add Clock Face
1. Copy shape creation pattern from `Test3DBasicShapesModel`
2. Create clock face as FoShape3D with CircleGeometry
3. Add to stage with `_stage.AddShape(clockFace)`
4. **Verify:** Clock face visible in Canvas3D

### Step 4: Add Clock Hands
1. Create hour, minute, second hands as FoShape3D
2. Position at clock center (z slightly forward to avoid z-fighting)
3. Add as children to center post OR directly to stage
4. **Verify:** All hands visible

### Step 5: Subscribe to Animation
1. Subscribe in OnAfterRenderAsync(firstRender)
2. Implement OnAnimationFrame handler
3. Calculate angles based on DateTime
4. Call RotateTo() on each hand
5. Call SetTransformStale() after rotation
6. **Verify:** Hands rotate smoothly, FPS stable

### Step 6: Add Submarine Orbit
1. Create submarine model with FoModel3D
2. Calculate orbit path: `x = cos(angle) * radius`, `y = sin(angle) * radius`
3. Update position in OnAnimationFrame
4. Calculate heading as tangent to orbit
5. **Verify:** Submarine orbits, heading needs refinement (expected issue)

### Step 7: Disposal
1. Unsubscribe from animation in Dispose()
2. Clean up stage and tech
3. Call base.Dispose()
4. **Verify:** No console errors on navigation away
```

- [ ] **Each step is specific and actionable**
- [ ] **Verification point after each step**
- [ ] **Copy-paste points identified**
- [ ] **Expected issues noted in advance**

---

### 2.9 Step-by-Step Test Sequence (CRITICAL - Dual Purpose)

**Purpose**: This section is both SPECIFICATION (shows how feature works) and VERIFICATION (proves correct implementation)

**Why This Matters**: Under-specification causes divergence. Step-by-step test sequence with exact expected results eliminates ambiguity and provides built-in verification.

Create complete walkthrough showing:

- [ ] **Complete First-Run Test** - From page load to full feature use
  - Each step numbered (Step 1, Step 2, etc.)
  - "Action:" - What user does
  - "Expected Results:" - Bullet list of what should happen
  - "Console Output:" - Exact messages with emojis/formatting
  - "If Failed:" - Quick diagnosis pointers
  
- [ ] **Expected Console Output** - Show EXACT log messages
  - Include emojis (🔵, ✅, ❌) from actual code
  - Show sequencing of messages
  - Indicate which messages appear together
  
- [ ] **Verification Checklist** - Final checkbox list after all steps
  - One checkbox per major observable behavior
  - Quick yes/no verification points
  - Can be run through in 2-3 minutes

**Example Format:**
````markdown
#### Step 3: Render Shape

**Action:** Click "Render Shape" button

**Expected Results:**
- ✅ Console shows: "🔵 RENDER SHAPE to stage"
- ✅ Console shows: "🆕 CREATE: Building new shape geometry"
- ✅ Canvas shows green box at center
- ✅ Box dimensions 1x2x3

**Console Output:**
```text
🔵 RENDER SHAPE to stage
🆕 CREATE: Building new shape geometry
✅ Geometry created and cached (Type=Box, W=1, H=2, D=3)
```

**If Failed:**
- No shape visible → Check RenderStage() called
- Wrong size → Verify parameter values
````

---

### 2.10 Success Criteria

Define what "done" looks like:

```markdown
## Success Criteria

### Compilation
- [ ] Zero compilation errors
- [ ] Zero compilation warnings
- [ ] All using statements resolve
- [ ] All method signatures match Razor bindings

### Runtime (First Load)
- [ ] Page loads without exceptions
- [ ] Canvas3D initializes and renders
- [ ] Stage created and active
- [ ] Clock face visible

### Runtime (Functionality)
- [ ] Clock hands rotate at correct speeds (verify against real clock)
- [ ] Second hand completes rotation in 60 seconds
- [ ] Minute hand completes rotation in 60 minutes
- [ ] Hour hand completes rotation in 12 hours
- [ ] FPS counter stable (>30 fps)
- [ ] Submarine orbits smoothly

### Known Issues (Acceptable)
- [ ] Clock hands may rotate from center not base (pivot issue)
- [ ] Submarine heading may not match direction (tangent calculation)

### Disposal
- [ ] No console errors on navigation away
- [ ] Animation unsubscribed
- [ ] No memory leaks (shapes cleaned up)
```

- [ ] **Criteria are specific and testable**
- [ ] **Organized by phase (compilation, runtime, disposal)**
- [ ] **Known issues explicitly listed as acceptable**
- [ ] **Performance criteria included**

---

## Phase 3: Self-Review (Before Handoff)

### 3.1 Method Name Verification

- [ ] **Every method call verified against API docs**
- [ ] **Every class name verified against source/docs**
- [ ] **Every event/callback name verified**
- [ ] **Every property name verified**

### 3.2 Code Path Completeness

- [ ] **Parent-child hierarchies traced** (if applicable)
- [ ] **Collection operations traced** (if applicable)
- [ ] **Animation lifecycle traced** (if applicable)
- [ ] **Disposal flow traced** (if applicable)

### 3.3 Pain Point Coverage

Check that you've predicted pain points in ALL these layers:

- [ ] **JavaScript/WebGL layer** (Canvas, Three.js, rendering)
- [ ] **C#-to-JS Interop layer** (Blazor JSInterop, marshalling)
- [ ] **Framework Infrastructure layer** (Collections, editors, traversal)
- [ ] **Application Logic layer** (Timers, state, calculations)
- [ ] **Blazor Lifecycle layer** (Disposal, navigation, initialization)

### 3.4 Reference Quality

- [ ] **At least one reference implementation cited**
- [ ] **File paths are absolute and correct**
- [ ] **Line numbers provided for key patterns** (when helpful)
- [ ] **API doc references are accurate**

### 3.5 Confidence Calibration

For each major section, rate your confidence:

```markdown
## Confidence Levels

- **Architecture Pattern:** 🟢 High - Based on Test3DBasicShapesModel (verified)
- **Stage Management:** 🟢 High - Verified in API reference
- **Shape Creation:** 🟢 High - Copied from working example
- **Parent-Child Hierarchy:** 🟡 Medium - Assumes AddShape works, verify if issues
- **Rotation Animation:** 🟢 High - Pattern used in other demos
- **Pivot Point Rotation:** 🔴 Low - May need Transform.Pivot (not fully documented)
- **Submarine Heading:** 🟡 Medium - Tangent calculation may need adjustment
```

- [ ] **Every major section rated**
- [ ] **Low confidence areas flagged for Indy**
- [ ] **Investigation pointers provided for uncertain areas**

### 3.6 Completeness Check

- [ ] **All required sections present** (see 2.1)
- [ ] **No pseudo-code without verification**
- [ ] **No assumed method names**
- [ ] **Troubleshooting guide covers likely failures**
- [ ] **Success criteria are testable**

---

## Phase 4: Handoff to Indy

### 4.1 Handoff Summary

Include at the top of your specification:

```markdown
## Specification Handoff Summary

**Architect:** Claude "Atlas"
**Date:** [date]
**Estimated Implementation Time:** [X-Y hours]
**Confidence:** [Overall confidence level]

**Primary Reference:** [path/to/file.cs] - Copy this, modify as specified

**High Uncertainty Areas:**
1. [Area 1] - May require [investigation/adjustment]
2. [Area 2] - If blocked, investigate [starting point]

**Known Limitations:**
1. [Limitation 1] - Acceptable for MVP
2. [Limitation 2] - Can be improved later

**Verification Checklist:**
- [ ] Compiles without errors
- [ ] Runs without exceptions
- [ ] Clock hands rotate
- [ ] Submarine orbits
- [ ] FPS stable >30
```

- [ ] **Estimated time provided**
- [ ] **Confidence level stated**
- [ ] **High uncertainty areas called out**
- [ ] **Quick verification checklist included**

### 4.2 Final Questions

Before handoff, answer these:

- [ ] **Would Indy know where to start?** (Reference implementation clear?)
- [ ] **Would Indy know what to verify?** (Success criteria testable?)
- [ ] **Would Indy know what to debug?** (Troubleshooting guide complete?)
- [ ] **Would Indy trust the method names?** (All verified?)
- [ ] **Would Indy understand the architecture?** (Pattern explained?)

---

## Appendix: Common Mistakes to Avoid

Based on ClockDemo AAR:

### ❌ Don't: Write abstract pseudo-code
```csharp
// DON'T DO THIS
_clockStage.LinkToScene();  // Method doesn't exist!
SubscribeToAnimation();      // Wrong name!
```

### ✅ Do: Verify and cite
```csharp
// DO THIS
// Verified in FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md, section "Animation"
AnimationFrameBus.SubscribeToPreAnimation(OnAnimationFrame);
```

---

### ❌ Don't: Predict wrong pain points
"I'm worried about Canvas3D integration..." (Actually works fine)

### ✅ Do: Cover all layers
"I'm confident Canvas3D works (battle-tested). I'm less sure about parent-child collection mechanics - verify if children don't render."

---

### ❌ Don't: Assume infrastructure works
"Just call AddShape and it works."

### ✅ Do: Trace the flow
"When you call parent.AddShape(child), it invokes FoGlyph3D.AddShape() which uses FoGlyph3DEditor to store in base collection..."

---

### ❌ Don't: Propose patterns without evidence
"Let's use ComponentBase with separate Tech class..."

### ✅ Do: Follow existing patterns
"Based on Test3DBasicShapesModel, the codebase uses Model-first pattern. Copy that structure."

---

## Version History

- **v1.0** - February 1, 2026 - Initial checklist based on ClockDemo AAR
  - Incorporates all lessons from CLOCKDEMO_AFTER_ACTION_REVIEW.md
  - Four-phase structure: Research, Write, Review, Handoff
  - Comprehensive coverage of all AAR recommendations

---

*This checklist is a living document. Update as new patterns emerge.*
