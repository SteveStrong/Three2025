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

### 1.1 Project Convention Scan (Budget: 5 minutes — ALWAYS DO FIRST)

**Atlas: Go wide before going deep.** This is YOUR research step, not Indy's. Before you study any framework internals, scan the project's file structure to discover conventions that your spec must follow. If you skip this, you risk writing a spec that contradicts project conventions — and Indy will follow your spec over visible evidence.

- [ ] **List all files in the target directory** (e.g., `Models/`, `Components/Pages/`)
  - Run `ls Models/` or equivalent — takes 5 seconds
  - Count how many existing pages/components follow each pattern
  - If 11/11 pages have a Model class, the 12th needs one too

- [ ] **Verify target files exist** (v1.3 addition — SpacialFrameTest AAR R1)
  - Run `file_search("**/TargetPage.razor*")` for every file the spec plans to modify
  - If files DON'T exist: spec is greenfield, not refactor — include creation instructions
  - If files DO exist: spec can reference line-by-line refactoring
  - **Never assume files exist. Check.** (SpacialFrameTest: spec assumed existing files; they didn't exist. 200 lines of refactoring guidance was wasted.)

- [ ] **Identify mandatory project conventions**
  - Does every page have a corresponding Model class?
  - Does every page use a specific injection pattern (`private = null!` vs `public required`)?
  - Does every page follow a specific namespace convention?
  - What base classes do peer pages use?

- [ ] **List sibling files for the new component**
  - What files exist alongside where this new component will live?
  - What naming patterns do they follow?
  - What structural patterns are universal (not just common)?

- [ ] **Read 3 nearest sibling model constructors** (v1.3 addition — SpacialBoxTest AAR)
  - Open the 3 most similar existing model files
  - List what each one injects: `IWorkspace`? `IFoundryService`? `NavigationManager`?
  - Use **what siblings actually inject** as your starting point — not first-principle design
  - (SpacialFrameTest: spec prescribed `IFoundryService`; every sibling uses `IWorkspace`. SpacialBoxTest: same error. This is now a 3x repeat.)

- [ ] **Document what you found**

**Documentation Template:**
```markdown
## Project Convention Scan
Directory: [path/to/Models/]
Files found: [count]

Universal Patterns (all files follow):
- Every page has a Model class in Models/ (11/11)
- Every model inherits MxComponent
- Every code-behind uses `private IFoundryService _foundry = null!`

This spec MUST follow:
- [ ] Include Model class definition
- [ ] Use established injection pattern
- [ ] Match namespace conventions
```

**Why This Matters (MultiCanvas3D lesson):**
Atlas studied the Three.js rendering pipeline in extraordinary depth but never typed `ls Models/`. The spec said "No separate Tech class" — not as a deliberate architectural decision, but as a blind spot. Indy followed the directive because the spec was well-written, and well-written specs create implied authority even where they're wrong. Sully caught the missing model after implementation. This 5-minute scan would have prevented the most expensive post-compilation fix.

> **The rule: Go wide first (5 minutes scanning project structure), then go deep (thorough framework analysis). Wide-then-deep, not one-or-the-other.**

---

### 1.2 Study Existing Patterns (Budget: 30 minutes)

> **⚠️ CRITICAL WARNING: Legacy Code Is Not a Reference Implementation**
>
> Atlas's primary job is to review **legacy applications** that worked under earlier versions of MxMicroCore and write specs that modernize them using the current architecture — where a **central Model class drives the UI**. This means the code you're studying is likely full of **outdated patterns**: logic tangled into code-behind, missing Model classes, direct manipulation instead of editor patterns, ad-hoc state management instead of MxComponent-based models.
>
> **Do NOT treat the legacy code as an example of how to build.** It shows you *what* the application does — its intent, its features, its user-facing behavior. It does NOT show you *how* to build it under the modern architecture.
>
> Your job is **translation, not transcription:**
> - **Capture the essence** — What does this application demonstrate? What is it trying to do?
> - **Ignore the implementation** — How it did it under the old architecture is irrelevant to your spec
> - **Spec the modern way** — Use the Model-behind pattern, the editor pattern, and the conventions you found in Phase 1.1
>
> If the legacy code doesn't have a Model class, that doesn't mean your spec shouldn't. If the legacy code stuffs everything into the code-behind, that's the problem you're fixing, not the pattern you're following.

- [ ] **Find 2-3 similar components** in the codebase
  - Search for components that implement similar features
  - Look for Test/Demo/Example files that show patterns in action
  - Document which files you examined
  - **Distinguish modern examples (Model-behind) from legacy examples (monolithic code-behind)**

- [ ] **Identify the dominant architecture pattern**
  - Is it Model-first? Component-first? Tech-first?
  - What base classes are used? (ComponentBase, MxComponent, etc.)
  - How are lifecycle hooks managed? (OnInitialized, OnAfterRender, etc.)
  - **If the code you're studying doesn't match the conventions from Phase 1.1, it's legacy — study it for intent, not for structure**

- [ ] **Map the infrastructure layer**
  - How are stages created and managed?
  - How is animation subscription handled?
  - How are shapes added to scenes?
  - How are collections and editors used?

- [ ] **Separate WHAT from HOW**
  - **WHAT** (capture this): features, user interactions, visual behavior, domain logic
  - **HOW** (ignore this): old architecture, missing models, monolithic structure, outdated patterns
  - **SPEC THIS**: the modern way to achieve the WHAT, using conventions from Phase 1.1

**Documentation Template:**
```markdown
## Architecture Analysis
Based on: [path/to/ReferenceFile1.cs], [path/to/ReferenceFile2.cs]

Legacy or Modern?: [Legacy — this code predates the Model-behind pattern]
Intent to preserve: [What the application does, its features, its purpose]
Patterns to NOT carry forward: [What the legacy code does wrong by modern standards]

Current Pattern (for the spec): [Pattern name, e.g., "Model-first with Editor pattern"]

Key Characteristics:
- Base class: [class name]
- Stage management: [how it works]
- Shape lifecycle: [how it works]
- Animation hookup: [how it works]

Files to study as reference:
1. [file1] - demonstrates [feature] — [Legacy/Modern]
2. [file2] - demonstrates [feature] — [Legacy/Modern]
```

### 1.3 Read API References (Budget: 15 minutes)

- [ ] **Locate relevant API documentation files**
  - `FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md` — shapes, stages, Canvas3DComponent, Transform3, animation
  - `FOUNDRY_3D_API_REFERENCE.md` — authoritative 3D reference (FoShape3D, FoGlyph3D, stale flags, factory methods, hallucinated API list)
  - `FOUNDRY_MENTORMODELER_API_REFERENCE.md` — mentor services, model editing
  - `FOUNDRY_MICROCORE_BLAZOR_CONTROLS_API_REFERENCE.md` — **shared UI components** (see below)
  - Component-specific API docs in `/Docs` folders

- [ ] **Check Blazor Controls for shared components** (Budget: 5 minutes)
  - `FOUNDRY_MICROCORE_BLAZOR_CONTROLS_API_REFERENCE.md` defines the **real, available** Blazor components:
    - `SceneTreePanel` — tabbed tree view (Model/Shapes/Scene tabs) with `Stage` and `Model` parameters
    - `UnifiedTreeView` — universal `ITreeNode` tree display
    - `CommandPanel` / `CommandButtonGroup` / `CommandStepsPanel` — command UI
    - `ToastService` / `ToastContainer` — notifications
  - **If your spec references a Blazor component, verify it exists:** `file_search("**/ComponentName*")`
  - Components that do NOT exist: `ShapeTreeView`, `RadzenShapeTreeView`, `TreeGrid`, `CommandDialog`
  - **The rule:** If `file_search` returns nothing, the component doesn't exist. Use what's in the Blazor Controls API reference.

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

### 1.4 Review Code Smell Documentation (Budget: 10 minutes)

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

- [ ] **1. Project Convention Scan** (from Phase 1.1 — MUST be first)
- [ ] **2. Architecture Analysis** (from Phase 1.2)
- [ ] **3. Verified Against** (from Phase 1.3)
- [ ] **4. Reference Implementation Strategy**
- [ ] **5. Infrastructure Assumptions**
- [ ] **6. Code Path Traces** (for non-trivial mechanisms)
- [ ] **7. Code Smells to Avoid** (from Phase 1.4)
- [ ] **8. Known Gotchas**
- [ ] **9. Troubleshooting Guide**
- [ ] **10. Implementation Steps**
- [ ] **11. Success Criteria**
- [ ] **12. Project Convention Compliance** (from Phase 1.1)
- [ ] **13. Visual Expectations** (what the page should look like)
- [ ] **14. Model/Domain Section** (mandatory for page specs)\n- [ ] **15. Service Integration Audit** (v1.3 \u2014 for any spec that delegates to services)\n- [ ] **16. UI Layout** (v1.3 \u2014 exact panel structure to preserve)

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

### 2.3 Service Integration Audit (v1.3 addition — SpacialFrameTest AAR R3)

Before signing off on a spec that **delegates to a service** (e.g., `GeometryVisualizationService`, `CommandService`, `ToastService`), Atlas MUST open the service implementation — not just reference the interface.

- [ ] **Read the service implementation** (not just the interface)
  - Open the actual `.cs` file, not just the `I*Service` interface
  - Search for hardcoded stage names, scene names, or arena routing
  - Check: does the service route output to the **caller's stage** or **its own internal stage**?
  
- [ ] **Document service routing**
  ```markdown
  ### Service: GeometryVisualizationService
  Implementation: [path/to/file.cs]
  Routes output to: arena.EstablishStage<FoStage3D>("Visualization") ← HARDCODED
  ⚠️ This means markers go to a "Visualization" stage, NOT the caller's stage.
  Impact: If the page's Canvas doesn't render the "Visualization" stage, markers are invisible.
  ```

- [ ] **If the service has hardcoded routing, flag it as an integration seam risk**
  - Either: document that the page's canvas must render that stage name
  - Or: recommend writing the logic inline, routing to the page's own stage
  - Or: recommend updating the service to accept a stage parameter

**Why This Matters (SpacialFrameTest lesson):**
 Atlas treated `GeometryVisualizationService` as a black box and wrote "battle-tested, no surprises, 90% confident." The service hardcodes `EstablishStage<FoStage3D>("Visualization")` — sending every marker to a ghost stage that no canvas renders. Shapes computed correctly, created correctly, placed correctly — and completely invisible. This was the showstopper bug, discoverable by reading 30 lines. **Atlas never read them because Atlas was confident.** High confidence + unread implementation = the most dangerous combination.

---

### 2.4 Infrastructure Assumptions (was 2.3)

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
- [ ] **Every Blazor component in the spec verified via `file_search`** — if it doesn't exist, don't spec it

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

### 4.2 Build Journal Requirement

Every spec MUST include a Build Journal section instructing Indy to maintain a timestamped journal during implementation. This journal is a **required input for Sage's After-Action Review**.

- [ ] **Build Journal template included in spec**
- [ ] **Instructs Indy to log as they go** (not after the fact)
- [ ] **Template includes After-Action Questions for Sage**

Without the build journal, Sage has only the code to analyze — no timing data, no decision rationale, no record of what surprised Indy or where the spec misled. The learning cycle breaks.

**Template to include in every spec:**
```markdown
### Build Journal Requirement

Maintain `BUILD_JOURNAL_[FEATURE].md` in the project root. Log as you go:
- Phase start/end times
- Decisions that differed from spec (and why)
- Surprises (APIs that didn't work as described)
- Console output observations
- Where the spec helped vs. where it misled

Include After-Action Questions at the end for Sage.
```

---

### 4.3 Final Questions

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

- **v1.1** - February 9, 2026 - Updated with Tug of War AAR (Indy's notes)
  - Added Phase 2.11: Golden Pattern section requirement
  - Added Phase 2.12: Implementer Behavior Predictions
  - Added Phase 3.7: Silent Failure Audit
  - Added new "Common Mistakes" entries from TugOfWar experience
  - See TUGOFWAR_PREDICTIONS.md scorecard for full data

- **v1.2** - February 10, 2026 - Updated with MultiCanvas3D AAR + Sage analysis
  - **Added Phase 1.1: Project Convention Scan** — "Go wide before going deep" (R1)
  - Renumbered Phase 1 sections (old 1.1→1.2, 1.2→1.3, 1.3→1.4)
  - Updated Phase 2.1 required sections list (14 items, was 10)
  - **Added prediction budget guidance** to Phase 2.12 (R4)
  - **Added Phase 2.13: Project Convention Compliance** — verify all conventions (R1)
  - **Added Phase 2.14: Visual Expectations** — describe what the page should look like (R3)
  - **Added Phase 2.15: Model/Domain Section** — mandatory for page specs (R6)
  - **Added Phase 3.8: Console Output Verification** — 30 seconds clean (R2)
  - **Added Phase 3.9: "Explain Why Not" Audit** — no bare prohibitions (R5)
  - Added new "Common Mistakes" entries from MultiCanvas3D experience
  - Informed by Learning Journal 035 (spec authority bias)
  - See MULTICANVAS3D_AFTER_ACTION_REVIEW.md for full data

- **v1.3** - February 10, 2026 - Updated with SpacialFrameTest + SpacialBoxTest AARs
  - **Added Phase 1.1: File Existence Verification** — check if target files exist before writing refactor instructions (R1)
  - **Added Phase 1.1: Sibling Constructor Audit** — read 3 nearest models to get injection pattern right (R2, 3x repeat fix)
  - **Added Phase 2.3: Service Integration Audit** — read service implementations, not just interfaces (R3, showstopper fix)
  - **Updated Phase 2.1: Added "Service Integration Audit" to required sections**
  - **Updated Phase 2.1: Added "UI Layout" to required sections**
  - **Added Phase 2.16: UI Layout** — exact panel structure, widths, CSS classes to preserve
  - **Added Appendix: Common Mistakes from SpacialFrameTest/SpacialBoxTest**
  - Informed by Learning Journal entries 037 (error surface), 038 (identity), 039 (autopsy)
  - Key insight: high confidence on unexamined code is the most dangerous prediction category
  - See SPACIALFRAMETEST_AFTER_ACTION_REVIEW.md and SPACIALBOXTEST_AFTER_ACTION_REVIEW.md

---

## Indy's Notes to Atlas (from Tug of War, February 9, 2026)

### The Scorecard

Atlas predicted 10 things. Final tally: **1 correct, 3 wrong, 3 partial, 3 N/A.**

The biggest miss: Atlas had 80% confidence that 3D would work before 2D. The exact opposite happened. 2D worked first try. 3D took 3 days.

**Why Atlas's predictions failed:** Atlas predicted *framework risks* — API drift, stale flags, pipe confusion, Task.Delay timing. The actual failure was an *implementer behavior* — I added unnecessary guards (`if (fps <= 0)`) to OnBeforeRender callbacks that silently killed every frame. Atlas predicted what the *library* might do wrong. The library was fine. **I** was the problem.

### What Atlas Should Do Differently

---

### NEW — Phase 2.11: Golden Pattern (ALWAYS INCLUDE FOR ANIMATIONS)

Every spec involving 3D animation MUST include a clearly marked **Golden Pattern** section. This is not architecture. This is not theory. This is the exact code to copy.

**The rule:** If a working example exists in the codebase, extract the minimal working pattern and present it as:

````markdown
## 🏆 GOLDEN PATTERN — Copy This Exactly

**Source:** `Animation3DPrimitivesModel.cs` line 423-440 (Oscillation example)

```csharp
double angle = 0.0;

shape.OnBeforeRender((self, tick, fps) =>
{
    angle += Math.PI / 300;
    var x = startX + distance * Math.Cos(angle);
    self.Transform.Position = new Vector3(x, y, z);
    self.SetTransformStale();
});

stage.AddShape(shape);
```

**CRITICAL RULES:**
- ❌ Do NOT add `if (fps <= 0)` guards
- ❌ Do NOT add `if (tick == 0)` guards  
- ❌ Do NOT add `if (animationDone) return` — use `ClearBeforeRender()` instead
- ❌ Do NOT use time-based `1.0 / fps` — use frame-based angle increments
- ❌ Do NOT call `SetRecomputeBoundary()` unless you need Wave 2 world positions
- ✅ Increment angle. Set position. Mark stale. That's it.
````

- [ ] **Golden Pattern extracted from working codebase example**
- [ ] **Source file and line numbers cited**
- [ ] **Anti-patterns listed with ❌ (things NOT to add)**
- [ ] **Pattern is minimal — under 15 lines**
- [ ] **No theory, no explanation of pipeline — just the code**

**Why This Matters (Tug of War lesson):**
I spent 3 days debugging because I "improved" the working pattern with defensive guards. The spec explained the pipeline beautifully — how UpdateForAnimation fires in Pass 1, how Bodies render before Links, how the collector batches changes. None of that mattered. What I needed was: "Here are 10 lines. Copy them. Don't add anything."

---

### NEW — Phase 2.12: Implementer Behavior Predictions

Atlas's predictions focused on what the *framework* might break. The actual failure was what the *implementer* did wrong. Future specs should predict both.

- [ ] **Predict framework risks** (API drift, naming changes, version sensitivity)
- [ ] **Predict implementer habits** (over-guarding, defensive coding, unnecessary features)
- [ ] **Predict project convention adherence** (will Indy follow conventions the spec omits?)
- [ ] **List specific things NOT to do** (more valuable than what TO do)

**Prediction Budget Guidance (MultiCanvas3D lesson):**

| Category | Old Budget | Recommended Budget | Rationale |
|---|---|---|---|
| Framework risks | 40% | 10% | Sully's infrastructure is battle-tested |
| Implementer behavior | 30% | 40% | The implementer is the largest variable |
| Project convention adherence | 0% | 30% | The blind spot that consumed the most post-compilation effort |
| Runtime behavior / environment | 20% | 10% | Console output, timing, disposal |
| Meta-outcomes | 10% | 10% | Speed, difficulty, overall trajectory |

> Zero predictions in MultiCanvas3D covered **project convention adherence** — the issue that consumed the most post-compilation effort. The real emerging category is project pattern compliance.

**Template:**
```markdown
## Implementer Behavior Warnings

### Things You Will Be Tempted To Do (DON'T)

1. **Add null/guard checks to OnBeforeRender callbacks**
   Why you'll want to: "What if fps is 0? What if the shape isn't ready?"
   Why you shouldn't: The pipeline guarantees valid state. Guards silently kill callbacks.
   
2. **Use time-based animation (animTime += dt)**
   Why you'll want to: "Real-time animation should be time-based for consistency"
   Why you shouldn't: Every working 3D example uses frame-based angle increments. Match the pattern.

3. **Call SetRecomputeBoundary() on animated shapes**
   Why you'll want to: "I want accurate hit boundaries for the moving shapes"
   Why you shouldn't: Wave 2 has a scene name mismatch bug. You'll get error spam with no benefit.

4. **Use hyphens or special characters in shape names**
   Why you'll want to: "Box1-abc12345 is descriptive and unique"
   Why you shouldn't: ValidateIdentifier silently rejects them. Use underscores only.
```

---

### NEW — Phase 2.13: Project Convention Compliance

Before writing "Component Structure," verify the spec follows every project convention discovered in Phase 1.1.

- [ ] **Listed all files in Models/ directory** — does the pattern require a model?
- [ ] **Listed all files in Components/Pages/** — does the new page match sibling structure?
- [ ] **Checked injection patterns** (`private = null!` vs `public required`) against existing pages
- [ ] **Checked namespace conventions** against existing pages
- [ ] **If spec says "Don't do X" — explained WHY NOT** (see Phase 3.9)

**Documentation Template:**
```markdown
## Project Convention Compliance

### Convention: Model-Behind Pattern
Evidence: 11/11 existing pages in Models/ have a dedicated Model class
This spec: ✅ Includes [ModelClassName] inheriting MxComponent

### Convention: Injection Pattern  
Evidence: All pages use `private IFoundryService _foundry = null!`
This spec: ✅ Matches established pattern

### Convention: Namespace
Evidence: All pages use `FoundryWorldsAndDrawings.Components.Pages`
This spec: ✅ Matches
```

**Why This Matters (MultiCanvas3D lesson):**
The spec explicitly stated "No separate Tech class — this is a straightforward page component." This wasn't a deliberate architectural decision — it was a blind spot. Atlas studied the rendering framework deeply but didn't study the project structure broadly. Every page in the project had a Model class. The spec told Indy to skip it. Indy followed the spec because it was well-written. **Good code in the wrong place is harder to catch than bad code anywhere. A good spec that's wrong about one thing is harder to question than a bad spec that's wrong about everything.**

---

### NEW — Phase 2.14: Visual Expectations

Every spec for a component with visual output MUST include a brief description of what the page should look like.

- [ ] **Described expected visual layout** (canvas sizing, spacing, borders)
- [ ] **Noted CSS requirements** beyond component defaults
- [ ] **Specified edge-to-edge vs. padded behavior**
- [ ] **Included sizing strategy** (fixed pixels vs. fill container)

**Documentation Template:**
```markdown
## Visual Expectations

The page should display three canvases in a 1×3 horizontal grid.
- Canvases fill their grid cells edge-to-edge (no white space inside borders)
- Colored borders (red, green, blue) flush with canvas edges
- Grid gaps of 8px between cells
- Responsive: canvases stretch to fill available width

CSS Required:
- Grid container with `grid-template-columns: 1fr 1fr 1fr`
- Canvas elements with `width: 100%; height: 100%`
- No fixed pixel dimensions on Canvas3D (use CSS to fill)
```

**Why This Matters (MultiCanvas3D lesson):**
The spec used `CanvasWidth=600 CanvasHeight=400` (fixed pixels), which left white space inside bordered containers. A single sentence — "canvases should fill their grid cells edge-to-edge" — would have prevented CSS debugging that Indy had to discover empirically.

---

### NEW — Phase 2.15: Model/Domain Section (Mandatory for Page Specs)

Every spec for a new page in this project MUST include a Model section. This is not optional — it's a universal project convention.

- [ ] **Model class name defined** (e.g., `MultiCanvas3DTestModel`)
- [ ] **Base class specified** (typically `MxComponent`)
- [ ] **Constructor parameters listed**
- [ ] **Public methods the code-behind will call**
- [ ] **Clear separation**: what stays in code-behind (Blazor refs, UI state) vs. what goes to model (domain logic, shape creation, animation)

**Documentation Template:**
```markdown
## Model Definition

### MultiCanvas3DTestModel : MxComponent

**Constructor:**
```csharp
public MultiCanvas3DTestModel(IFoundryService foundry, IWorkspace workspace)
```

**Responsibilities:**
- Scene setup (stage creation, shape building)
- Animation logic (rotation calculations)
- Domain state (shape references, angle tracking)

**Code-behind keeps:**
- `Canvas3DComponent` references (Blazor component refs)
- `OnAfterRenderAsync` lifecycle (Blazor-specific)
- UI event handlers

**Code-behind delegates to model:**
- `model.SetupScenes()` — creates stages and shapes
- `model.OnAnimationFrame()` — updates rotations
- `model.Dispose()` — cleans up subscriptions
```

> **The rule:** If 11/11 existing pages have a Model class, page 12 gets a Model class. No exceptions without explicit justification.

---

### NEW — Phase 2.16: UI Layout (v1.3 — Preserve Exactly)

Every spec for a page with a multi-panel layout MUST include an exact layout description. Not aspirational CSS — the actual structure that exists and must be preserved.

- [ ] **ASCII wireframe of the panel layout** — show exact panel arrangement
- [ ] **Exact inline styles and CSS classes** — widths, overflow, max-height
- [ ] **What's in each panel** — button sections, tree views, transform inputs, status alerts
- [ ] **What changes vs. what stays identical** — explicitly list both
- [ ] **"CSS — No Changes" directive** — if CSS isn't changing, say so

**Documentation Template:**
```markdown
## UI Layout (Preserve Exactly)

Layout Structure (exact markup):
<div class="d-flex">
    <Canvas3DComponent .../>
    <div class="controls-panel" style="margin-left: 10px; width: 300px; ...">
    <div class="controls-panel" style="margin-left: 10px; width: 350px; ...">
</div>

What changes: @onclick targets, @bind targets, tree view component
What stays: ALL CSS, HTML structure, class names, panel widths
```

**Why This Matters (SpacialFrameTest/SpacialBoxTest lesson):**
Atlas's initial specs described layout aspirationally — "Left: canvas, Center: controls, Right: tree view." Indy had to discover the exact panel widths, CSS classes, and overflow styles by reading the existing page. The SpacialFrameTest build needed two layout iterations. Including the exact markup in the spec eliminates this entirely. Copy the layout, change the bindings, done.

---

- [ ] **Name validation** — Does the naming API throw or silently reject?
  - `MxObject.Name` setter silently rejects invalid names (no exception, no log)
  - Only letters, digits, underscores. No hyphens, spaces, or special chars.
  
- [ ] **Callback registration** — Does registering a callback confirm success?
  - `OnBeforeRender()` replaces any existing callback (last-write-wins)
  - No confirmation that the callback was registered
  
- [ ] **Flag setting** — Do flag methods have observable side effects?
  - `SetRecomputeBoundary()` opts into Wave 2 which may be broken
  - No visible error if the scene name mismatch prevents boundaries from arriving

- [ ] **Collection adds** — Does Add confirm the item was stored?
  - `AddShape()` may log but won't throw if categorization fails

**The rule:** For every API the spec tells Indy to call, ask: "What happens if this silently fails? How would Indy know?" If the answer is "they wouldn't," add a verification step.

---

### NEW — Phase 3.8: Console Output Verification

"Verified working code" must mean **zero unexpected warnings for 30 seconds of runtime**. Not just "renders visually."

- [ ] **Run the reference implementation for 30 seconds**
- [ ] **Monitor browser console output** (not just the visual result)
- [ ] **Document expected console output** — what messages are normal?
- [ ] **Document unexpected console output** — any warnings, errors, spam?
- [ ] **If animation produces console noise, fix it before putting it in the spec**

**The rule:** If you can't show 30 seconds of clean console output, the code isn't "verified" — it's "visually plausible." Include a "Console Output" section in the spec showing what clean operation looks like.

**Why This Matters (MultiCanvas3D lesson):**
Atlas claimed the Appendix A code was "verified, running code." It produced hundreds of Euler overflow warnings per second within 6 seconds of loading. The shapes rotated correctly — the *visual* was fine. But the console was screaming. Both ClockDemo and Multi-Canvas had this same blind spot: visual verification without console verification.

---

### NEW — Phase 3.9: "Explain Why Not" Audit

Before handoff, search the spec for every directive that tells Indy NOT to do something. Each one must include a reason.

- [ ] **Search spec for "No," "Don't," "Do not," "Skip," "Not needed"**
- [ ] **For each prohibition, verify a reason is given**
- [ ] **Reason must be evaluable** — Indy should be able to look at evidence and agree or disagree

**❌ Don't: Bare assertion**
```markdown
No separate Tech class — this is a straightforward page component.
```
*Indy can't evaluate this. "Straightforward" is subjective. Indy follows it because the spec is authoritative.*

**✅ Do: Evaluable reasoning**
```markdown
No separate Tech class — unlike the 11 existing pages that have complex domain logic 
and multiple shape collections, this page only creates 3 static shapes with simple 
rotation. The logic fits in ~40 lines, below the threshold where a model adds clarity. 
If you find the code-behind exceeding 80 lines, extract a model following the 
Test3DBasicShapesModel pattern.
```
*Indy can count lines, check complexity, and push back if the reasoning doesn't hold.*

**Why This Matters (MultiCanvas3D + Learning Journal 035):**
Spec authority bias: when a detailed, well-reasoned spec makes an explicit "don't" statement, an AI implementer follows it even when visible evidence contradicts it. Indy saw 11 model files and still didn't push back on "No separate Tech class" because the spec was well-written — and **quality in one area creates implied authority in all areas.** The fix isn't "be less compliant" — it's "never issue bare prohibitions." If Atlas can't articulate why not, Atlas probably has a blind spot, not a decision.

---

### NEW — Appendix Additions: Common Mistakes from Tug of War

### ❌ Don't: Add defensive guards to OnBeforeRender
```csharp
// DON'T DO THIS — silently kills every callback
shape.OnBeforeRender((self, tick, fps) =>
{
    if (fps <= 0) return;           // ← SILENT KILLER (fps may be 0 on first frame)
    if (tick == 0) return;          // ← tick is global, already at 300+ when shapes are added
    if (animationDone) return;      // ← use ClearBeforeRender() instead
    // ... animation code never runs
});
```

### ✅ Do: Match the proven working pattern exactly
```csharp
// DO THIS — every working Animation3DPrimitives example
double angle = 0.0;
shape.OnBeforeRender((self, tick, fps) =>
{
    angle += Math.PI / 300;
    self.Transform.Position = new Vector3(x, y, z);
    self.SetTransformStale();
});
```

---

### ❌ Don't: Use hyphens in shape names
```csharp
// DON'T — ValidateIdentifier silently rejects this, shape gets auto-named "FoShape3D_1"
var box = new FoShape3D($"Box1-{guid}", "blue");
```

### ✅ Do: Use only letters, digits, underscores
```csharp
// DO THIS — valid identifier, name sticks
var box = new FoShape3D("Box1", "blue");
// or with uniqueness:
var box = new FoShape3D($"Box1_{counter}", "blue");
```

---

### ❌ Don't: Predict only framework risks
```
// Atlas's TugOfWar predictions focused on:
// - API naming drift (didn't happen)
// - Stale flag issues (shapes were auto-stale via IsNew)
// - FoPipe3D complexity (pipe was trivial)
// - Task.Delay timing (never used)
```

### ✅ Do: Predict implementer behavior risks
```
// What Atlas should have predicted:
// - Indy will add guards to callbacks (defensive coding habit)
// - Indy will use time-based animation instead of frame-based (instinct to be "correct")
// - Indy will call SetRecomputeBoundary without needing it (completeness instinct)
// - Indy will use hyphens in names (readable naming instinct)
```

---

### NEW — Appendix Additions: Common Mistakes from MultiCanvas3D

### ❌ Don't: Skip the project convention scan
```
// Atlas wrote a 2000-word spec with code path traces, infrastructure assumptions,
// and a complete Appendix A — but never typed `ls Models/`.
// Result: Spec explicitly said "No separate Tech class" when 11/11 existing
// pages all have a Model class. Indy followed the spec. Sully caught it.
// Cost: Post-compilation refactoring that a 5-second directory listing would have prevented.
```

### ✅ Do: Scan project structure before writing the spec
```
// Atlas Phase 1.1 should produce:
// "Models/ contains 11 files. All pages have a Model class inheriting MxComponent.
//  This spec MUST include a Model class definition."
// Time cost: 5 minutes. Savings: hours of post-build refactoring.
```

---

### ❌ Don't: Issue bare prohibitions
```markdown
// DON'T — "No separate Tech class — this is a straightforward page component."
// Indy can't evaluate "straightforward." Indy follows it because the spec is authoritative.
// A well-written spec that's wrong about one thing gets inherited trust it didn't earn.
```

### ✅ Do: Explain WHY NOT with evaluable criteria
```markdown
// DO — "No separate Tech class — unlike the 11 existing pages that have complex
// domain logic, this page only creates 3 static shapes with simple rotation.
// The logic fits in ~40 lines. If you find the code-behind exceeding 80 lines,
// extract a model following the Test3DBasicShapesModel pattern."
// Indy can count lines and push back if the reasoning doesn't hold.
```

---

### ❌ Don't: Verify code visually only
```
// Atlas ran the Appendix A code. Shapes rotated. Looked great.
// Console: hundreds of Euler overflow warnings per second.
// "Verified" meant "renders visually" — not "runs cleanly."
```

### ✅ Do: Verify code visually AND in console for 30 seconds
```
// Run the code. Watch the browser for 30 seconds.
// ALSO watch the console for 30 seconds.
// If the console is clean: "Verified — 30s clean console."
// If it's not: fix it before putting it in the spec.
```

---

### NEW — Appendix Additions: Common Mistakes from SpacialFrameTest/SpacialBoxTest

### ❌ Don't: Treat services as black boxes when predicting "no surprises"
```
// Atlas wrote "GeometryVisualizationService — battle-tested, no surprises, 90% confident"
// Never read the implementation. 
// The service hardcodes: arena.EstablishStage<FoStage3D>("Visualization")
// Every marker went to a ghost stage no canvas renders. Perfectly computed, perfectly invisible.
// 30 lines of source code would have revealed this. Atlas never read them.
// Cost: showstopper bug, Indy had to rewrite all five viz methods inline.
```

### ✅ Do: Open every service your spec delegates to
```
// Before writing "this service will just work":
// 1. Open the .cs file (not the interface)
// 2. Search for hardcoded stage names, scene names, arena routing
// 3. Ask: "Does output go to the CALLER'S stage or the SERVICE'S stage?"
// 4. If the service routes internally: flag it as integration seam risk
// Time: 5 minutes. Prevents: the single most expensive bug in the build.
```

---

### ❌ Don't: Assume target files exist
```
// Atlas wrote 200 lines of "extract Method X from code-behind to model"
// SpacialFrameTest.razor didn't exist. Neither did SpacialBoxTest.
// All refactoring guidance was irrelevant — this was greenfield.
// A 5-second file_search would have caught this.
```

### ✅ Do: Verify files exist, then branch your instructions
```
// Before writing refactoring steps:
file_search("**/SpacialFrameTest.razor*")
// If found: write refactoring instructions (extract, move, rewire)
// If not found: write creation instructions (create from scratch, follow these patterns)
// Include BOTH paths if uncertain: "If files exist, refactor. If not, create."
```

---

### ❌ Don't: Design injection from first principles
```
// Atlas prescribed: IFoundryService, IGeometryVisualizationService, NavigationManager
// Every sibling model uses: IWorkspace
// IFoundryService was wrong. IGeometryVisualizationService wasn't in DI. NavigationManager wasn't needed.
// This is the THIRD time Atlas got the injection pattern wrong by designing instead of reading.
```

### ✅ Do: Read 3 sibling constructors first
```
// Before writing any model constructor:
// 1. Open ClockDemoModel.cs — what does it inject?
// 2. Open MultiCanvas3DTestModel.cs — what does it inject?
// 3. Open Animation3DPrimitivesModel.cs — what does it inject?
// Answer: they all inject IWorkspace. Start there. Add more only if domain requires it.
// Time: 2 minutes. Prevents: recurring injection mismatch (3x repeat).
```

---

### ❌ Don't: Put 90% confidence on things you didn't examine
```
// "All APIs work first try — 90% confident" has been wrong TWICE.
// SpacialFrameTest: 90% on the one that broke (viz stage routing)
// SpacialBoxTest: 85% but Indy had to override to make it work
// High confidence + unread implementation = most dangerous combination.
// Confidence is inverse to scrutiny: the more certain you feel, the less you looked.
```

### ✅ Do: Replace confidence claims with verification statements
```
// Instead of: "All APIs work first try — 90% confident"
// Write: "I verified these specific methods against source code:"
// - ✅ GetVertices() — returns List<Point3D>, checked SpacialFrame3D.cs line 71
// - ✅ CreateBox() — params (name, color, w, h, d), checked FoShape3D.cs
// - ⚠️ ShowLabeledVertices() — DID NOT READ implementation, interface only
// The ⚠️ is more valuable than any confidence percentage.
```

---

*This checklist is a living document. Update as new patterns emerge.*
*"Confidence is inverse to scrutiny. The most certain prediction got the least examination." — Sage, Entry 039*
