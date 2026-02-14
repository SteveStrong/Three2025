# SpacialFrameTest Page Specification

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Target:** Modernize `SpacialFrameTest.razor` / `SpacialFrameTest.razor.cs`  
**Status:** Specification for Indy

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about modernizing SpacialFrameTest — it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 3D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you modernize SpacialFrameTest, you're testing whether AI can:
1. Successfully use 40 years of distilled programming patterns
2. Translate a legacy monolithic code-behind into the modern Model-behind architecture
3. Correctly separate interactive transform controls (UI state) from spatial geometry computation (domain logic)
4. Consistently apply the same modernization pattern across a family of related pages

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can apply the Model-behind extraction pattern **consistently** across sibling pages — the same transformation applied to SpacialBoxTest now applied to its closest relative, demonstrating repeatable modernization rather than one-off heroics.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## Specification Handoff Summary

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Estimated Implementation Time:** 1.5-3 hours  
**Confidence:** 🟢 High (direct sibling of SpacialBoxTest — same domain, same services, simpler page)

**Primary Reference:** `SpacialBoxTest` specification and implementation — apply the SAME model extraction pattern.

**High Uncertainty Areas:**
1. **`<RadzenShapeTreeView/>`** — Does not exist in the workspace (same issue as `<ShapeTreeView/>` in SpacialBoxTest). Same dependency blocker: `Three2025.csproj` doesn't reference `FoundryMicroCore.Blazor.Controls`.
2. **Transform UI binding boundary** — Position/Rotation/Pivot/Scale values are UI-bound but also drive domain logic. The boundary between "UI state" and "model state" needs care.

**Known Limitations:**
1. `RadzenShapeTreeView` doesn't exist — same situation as SpacialBoxTest
2. Scale controls are commented out in the razor markup — preserve this (intentionally disabled)

**Verification Checklist:**
- [ ] Compiles without errors
- [ ] Page loads, axis + orange box visible
- [ ] All 5 preset shapes work
- [ ] Transform controls (position, rotation, pivot) update shape
- [ ] +1 and +90° buttons work
- [ ] Reset Transform works
- [ ] Show Matrix displays transform matrix
- [ ] All visualizations work (vertices, edges, faces, normals, quadrants)
- [ ] Clear removes all shapes
- [ ] Navigation away — no errors

---

## Phase 1: Project Convention Scan

### What We Already Know (From SpacialBoxTest Spec)

All conventions from the SpacialBoxTest specification apply identically:
- **Base class:** `ComponentBase`
- **Pattern:** `partial class` 
- **Namespace:** `Three2025.Components.Pages`
- **IDisposable:** Yes
- **Injection pattern:** `[Inject] public ... { get; init; }` / `{ get; set; }`
- **Canvas pattern:** `public Canvas3DComponent Canvas3DReference = null`
- **Stage:** `Canvas3DReference?.Stage`
- **Model-behind direction:** Extract domain logic to separate model class

### This Spec MUST Follow:
- [x] Include Model class definition (`SpacialFrameTestModel`)
- [x] Use `partial class` pattern
- [x] Namespace `Three2025.Components.Pages`
- [x] Implement `IDisposable`
- [x] Match same injection pattern as SpacialBoxTest

---

## Phase 2: Architecture Analysis

**Based on:** `SpacialFrameTest.razor.cs` (445 lines)

**Legacy or Modern?:** Legacy — all domain logic in code-behind.

**Intent to preserve:**
- Create an orange 3D box with configurable dimensions
- Interactive transform controls: Position (X/Y/Z), Rotation (degrees, auto-converted to radians), Pivot (X/Y/Z)
- Scale controls (currently commented out in razor — preserve disabled state)
- Increment buttons (+1 for position/pivot, +90° for rotation)
- Reset Transform to defaults
- Show Transform Matrix
- Visualization: vertices, edges, faces, normals, quadrants (using world-space SpacialFrame3D transforms)
- Auto-refresh: changing any transform input recreates the shape
- 5 preset shapes (same as SpacialBoxTest)

**Key Difference from SpacialBoxTest:**
- Uses `SpacialFrame3D` (world-space transforms) not `SpacialBox3D` (local-space only)
- Has interactive Transform controls (Position, Rotation, Pivot inputs) — SpacialBoxTest does not
- No animations, no dirty flag tests, no submarine model — simpler domain logic
- `CreateSpacialFrame()` clears stage and recreates shape on every change (rebuild pattern)

**Patterns to NOT carry forward:**
- 445 lines of monolithic code-behind
- 12 nearly-identical `Increment*()` methods that each do `value += 1.0; CreateSpacialFrame();`
- Repeated boilerplate null-check patterns in every visualization method
- Reference to non-existent `RadzenShapeTreeView`

---

## Verified Against

Same API references as SpacialBoxTest (already verified — see that spec for details):
- `FOUNDRY_3D_API_REFERENCE.md` (v25.5.0)
- `FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md` (v25.5.0)
- `FOUNDRY_MICROCORE_BLAZOR_CONTROLS_API_REFERENCE.md` (v1.3.0)

### Additional Method Verification:
- ✅ `new Euler(x, y, z, AngleUnit.Degrees)` — constructs Euler from degrees
- ✅ `Transform3.ToMatrix3()` — returns matrix representation
- ✅ `matrix.ToStringFormatted()` — used in existing code for matrix display
- ✅ `SpacialFrame3D.GetVertices()` — returns world-transformed vertices
- ✅ `SpacialFrame3D.GetEdges()` — returns world-transformed edges  
- ✅ `SpacialFrame3D.GetFaces()` — returns world-transformed faces

### Hallucinated APIs (DO NOT USE):
Same list as SpacialBoxTest — these do NOT exist:
- ❌ `SetPosition()`, `SetRotation()`, `SetScale()` — do not exist on FoShape3D. Use `Transform.Position = ...`
- ❌ `shape3D.SetDirty()` — does not exist. Use `Set*Stale()` methods
- ❌ `ShapeTreeView` / `RadzenShapeTreeView` — components do not exist in workspace

---

## Infrastructure Assumptions

### Assumption: Canvas3DComponent Stage Works
- [x] Same pattern as SpacialBoxTest — verified working
- [x] `Canvas3DReference?.Stage` returns the page's FoStage3D
- If broken: Check Canvas3DComponent.razor.cs lifecycle

### Assumption: SpacialFrame3D World-Space Transforms Work
- [x] `SpacialFrame3D(shape, "m")` wraps a FoShape3D and uses its Transform for world-space computation
- [x] `GetVertices()` returns vertices transformed by shape's Transform3
- [x] Changing Position/Rotation/Pivot on Transform → GetVertices returns correspondingly transformed points
- If broken: Check `FoundryWorldsAndDrawings/Shapes3D/SpacialFrame/SpacialFrame3D.cs`

### Assumption: Euler AngleUnit.Degrees Works
- [x] `new Euler(x, y, z, AngleUnit.Degrees)` converts degrees to internal radians
- [x] Existing code uses this exact pattern and it works
- If broken: Verify Euler constructor signature accepts AngleUnit parameter

### Assumption: Clear-and-Recreate Pattern Works
- [x] `stage.ClearAll()` followed by `stage.AddShape(newShape)` renders the new shape
- [x] No stale references left behind after ClearAll
- If broken: Check whether ClearAll is async and needs await

---

## Code Smells to Avoid

Same general guidance as SpacialBoxTest spec — see that document for full list. Key ones for this page:

### Task-Specific Warnings

#### Non-Existent Component Reference
**Problem:** `<RadzenShapeTreeView/>` doesn't exist in the workspace
**Solution:** Same resolution as SpacialBoxTest (SceneTreePanel or remove)

#### Rebuild-on-Every-Keystroke
**Problem:** `@bind:after="AutoRefreshShape"` fires on every input change, clearing and recreating the entire shape
**Note:** This is the existing behavior — do NOT change it during modernization. Just move it to the model.

---

## Known Gotchas

### RadzenShapeTreeView Doesn't Exist
Same issue as SpacialBoxTest. `<RadzenShapeTreeView/>` at line 144 of the razor file does not exist anywhere in the workspace. **Resolved:** Replace with `<SceneTreePanel Stage="@_model?.Stage" />` — the project reference and @using are already in place.

### Scale Controls Are Intentionally Commented Out
Lines ~103-119 of the razor file have Scale X/Y/Z inputs wrapped in `@* ... *@`. Do NOT uncomment them. The model should still have ScaleX/Y/Z properties (they're used in CreateSpacialFrame), but the UI for them stays disabled.

### Rotation Values Are Degrees in UI, Radians Internally
The input fields show degrees. `CreateSpacialFrame()` passes them to `new Euler(RotationX, RotationY, RotationZ, AngleUnit.Degrees)` which handles conversion. Do not manually convert — the Euler constructor does it.

### Transform3 Project Reference Blocker
**RESOLVED:** `Three2025.csproj` now references `FoundryMicroCore.Blazor.Controls`. No action needed.

---

## Troubleshooting Guide

### Shape Not Appearing After Transform Change

**Symptom:** Change Position X input, shape disappears or doesn't move

**Diagnosis Steps:**
1. Check `_model.Stage` is not null
2. Check `AutoRefreshShape` is being called (add `WriteInfo` trace)
3. Verify `ClearAll()` succeeded before `AddShape()`
4. Check Transform values are propagating: log PositionX/Y/Z before CreateBox

**Common Causes:**
- Stage is null (Canvas not initialized)
- `@bind:after` not wired to model method (binding syntax issue)
- Shape positioned off-screen (extreme position values)

### Rotation Appears Wrong

**Symptom:** Entering 90 in Rotation Y doesn't rotate 90°

**Diagnosis Steps:**
1. Verify `AngleUnit.Degrees` is passed to Euler constructor
2. Log the raw Euler values: `$"Rotation: {RotationX}, {RotationY}, {RotationZ}°".WriteInfo()`
3. Check you're not double-converting (degrees → radians manually AND via AngleUnit)

**Common Causes:**
- Using `Euler.FromDegrees()` which is a different factory method
- Passing radians when degrees were intended

### Show Matrix Shows Error

**Symptom:** "Error getting matrix" status message

**Diagnosis Steps:**
1. Check `CurrentFrame?.Source?.Transform` is not null
2. Verify shape was created before matrix request
3. Check `ToMatrix3()` doesn't throw for identity transforms

**Common Causes:**
- No shape created yet (user pressed Show Matrix before creating box)
- CurrentFrame is null after ClearAll

---

## Reference Implementation Strategy

### Primary Reference
**Copy pattern from:** `SpacialBoxTestModel.cs` (as specified/created from SpacialBoxTest spec)  
**Demonstrates:** Model extraction pattern in the identical domain

### Delta from SpacialBoxTest:
| Aspect | SpacialBoxTest | SpacialFrameTest |
|---|---|---|
| Domain class | `SpacialBox3D` (local space) | `SpacialFrame3D` (world space) |
| Transform controls | None | Position, Rotation, Pivot, Scale (disabled) |
| Animations | Door hinge, corner balance, center spin, reset | None |
| Dirty flag tests | 6 test methods | None |
| Submarine model | Yes | No |
| Remake pattern | Update existing or create new | Always clear + recreate |
| Transform binding | N/A | 12 UI-bound properties |
| Matrix display | No | Yes (`ShowTransformMatrix`) |
| Total methods | ~25 | ~20 |

### The Extraction Decision: Where Do Transform Values Live?

**The boundary question:** Position/Rotation/Pivot/Scale values are bound to UI inputs (`@bind="PositionX"`) AND used in domain logic (`CreateSpacialFrame()`). Where do they live?

**Answer: In the Model.** The values ARE domain state (they define the shape's transform). The UI binds to model properties. The code-behind is just the binding relay.

```razor
@* Razor binds to model *@
<input type="number" @bind="_model.PositionX" @bind:after="_model.AutoRefreshShape" />
```

This means the model owns PositionX/Y/Z, RotationX/Y/Z, PivotX/Y/Z, ScaleX/Y/Z, BoxWidth/Height/Depth — all the properties that UI inputs bind to.

---

## Component Structure

### Files to Create/Modify

| File | Action | Purpose |
|---|---|---|
| `Components/Pages/SpacialFrameTestModel.cs` | **CREATE** | Model class — domain logic + transform state |
| `Components/Pages/SpacialFrameTest.razor.cs` | **MODIFY** | Slim code-behind — lifecycle + delegation |
| `Components/Pages/SpacialFrameTest.razor` | **MODIFY** | Replace RadzenShapeTreeView, bind to model |

---

## Model Definition

### SpacialFrameTestModel (New File)

**Location:** `Components/Pages/SpacialFrameTestModel.cs`  
**Namespace:** `Three2025.Components.Pages`

```csharp
namespace Three2025.Components.Pages;

public class SpacialFrameTestModel
{
    // === Dependencies ===
    private readonly IFoundryService _foundryService;
    private readonly IGeometryVisualizationService _visualizationService;
    private readonly NavigationManager _navigation;

    // === Shape State ===
    public SpacialFrame3D? CurrentFrame { get; private set; }
    public FoShape3D? CurrentShape { get; private set; }
    public FoStage3D? Stage { get; set; }

    // === Dimension State (UI-bound) ===
    public double BoxWidth { get; set; } = 2.0;
    public double BoxHeight { get; set; } = 1.5;
    public double BoxDepth { get; set; } = 1.0;

    // === Transform State (UI-bound) ===
    public double PositionX { get; set; } = 0.0;
    public double PositionY { get; set; } = 0.0;
    public double PositionZ { get; set; } = 0.0;

    public double PivotX { get; set; } = 0.0;
    public double PivotY { get; set; } = 0.0;
    public double PivotZ { get; set; } = 0.0;

    public double RotationX { get; set; } = 0.0;
    public double RotationY { get; set; } = 0.0;
    public double RotationZ { get; set; } = 0.0;

    public double ScaleX { get; set; } = 1.0;
    public double ScaleY { get; set; } = 1.0;
    public double ScaleZ { get; set; } = 1.0;

    // === UI State ===
    public string StatusMessage { get; private set; } = string.Empty;
    public Action? OnStateChanged { get; set; }

    private IArena? Arena => _foundryService?.Arena();

    public SpacialFrameTestModel(
        IFoundryService foundryService,
        IGeometryVisualizationService visualizationService,
        NavigationManager navigation)
    {
        _foundryService = foundryService;
        _visualizationService = visualizationService;
        _navigation = navigation;
    }

    // === Initialization (called from OnAfterRenderAsync) ===
    public void Initialize(Scene3D scene) { ... }

    // === Core: CreateSpacialFrame (clear + recreate) ===
    public void CreateSpacialFrame() { ... }
    public void AutoRefreshShape() => CreateSpacialFrame();

    // === Presets ===
    public void CreateCube() { BoxWidth = BoxHeight = BoxDepth = 2.0; CreateSpacialFrame(); }
    public void CreateLongBox() { ... }
    public void CreateTallBox() { ... }
    public void CreateWideBox() { ... }
    public void CreateTinyBox() { ... }

    // === Transform Operations ===
    public void ResetTransform() { ... }
    public void ApplyQuickRotationX90() { RotationX += 90; if (RotationX >= 360) RotationX -= 360; CreateSpacialFrame(); }
    public void ApplyQuickRotationY90() { ... }
    public void ApplyQuickRotationZ90() { ... }
    public void IncrementPositionX() { PositionX += 1.0; CreateSpacialFrame(); }
    // ... (all 6 increment methods)
    public void ShowTransformMatrix() { ... }

    // === Visualization ===
    public void ShowVertices() { ... }
    public void ShowEdges() { ... }
    public void ShowFaces() { ... }
    public void ShowNormals() { ... }
    public void ShowQuadrants() { ... }

    // === Scene ===
    public void ClearAll() { ... }

    // === Helpers ===
    public string GetReferenceTo(string filename) { ... }
    private void SetStatus(string message) { StatusMessage = message; OnStateChanged?.Invoke(); }
}
```

**Responsibilities (moved FROM code-behind):**
- ALL domain methods (create, visualize, transform, presets, matrix)
- ALL UI-bound properties (dimensions, position, rotation, pivot, scale, status)
- Stage reference (set by code-behind after lifecycle)

**Code-behind keeps:**
- `Canvas3DComponent` reference (`@ref` binding)
- `OnAfterRenderAsync` lifecycle
- `[Inject]` services
- `[Parameter]` properties (`CanvasWidth`, `CanvasHeight`)
- Model creation + wiring
- `IDisposable.Dispose()`

---

## Code-Behind Structure (Slimmed)

### SpacialFrameTest.razor.cs (After Refactoring)

**Target:** Under 50 lines. This page is simpler than SpacialBoxTest.

```csharp
namespace Three2025.Components.Pages;

public partial class SpacialFrameTest : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponent Canvas3DReference = null;
    private SpacialFrameTestModel _model;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected override void OnInitialized()
    {
        _model = new SpacialFrameTestModel(FoundryService, VisualizationService, Navigation);
        _model.OnStateChanged = () => InvokeAsync(StateHasChanged);
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
            if (found)
            {
                _model.Stage = Canvas3DReference?.Stage;
                _model.Initialize(scene!);
            }
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose() { _model = null; }
}
```

**Note:** No one-line delegate methods needed! The razor binds directly to `_model.MethodName` since the model's methods are public. For example:
```razor
<button @onclick="_model.CreateCube">Cube (2×2×2)</button>
<input @bind="_model.PositionX" @bind:after="_model.AutoRefreshShape" />
```

---

## Razor Markup Changes

### SpacialFrameTest.razor

**Three changes:**

**1. Replace `<RadzenShapeTreeView/>`:**

```razor
@* ❌ CURRENT (doesn't exist) *@
<RadzenShapeTreeView/>

@* ✅ OPTION A: SceneTreePanel (requires adding project reference — see SpacialBoxTest spec) *@
<SceneTreePanel Stage="@_model?.Stage" Title="SpacialFrame Scene" DefaultTab="shapes" />

@* ✅ OPTION B: Just remove it (if Sully declines the dependency) *@
@* Panel removed — tree view requires FoundryMicroCore.Blazor.Controls reference *@
```

**✅ RESOLVED:** `Three2025.csproj` now references `FoundryMicroCore.Blazor.Controls` and `_Imports.razor` includes the `@using` directives. SceneTreePanel is available. Just use it:
```razor
<SceneTreePanel Stage="@_model?.Stage" Title="SpacialFrame Scene" DefaultTab="shapes" />
```

**2. Bind inputs to model properties:**
```razor
@* ❌ CURRENT *@
<input type="number" step="0.1" @bind="PositionX" @bind:after="AutoRefreshShape" />

@* ✅ REPLACEMENT *@
<input type="number" step="0.1" @bind="_model.PositionX" @bind:after="_model.AutoRefreshShape" />
```

Apply this pattern to ALL inputs: PositionX/Y/Z, RotationX/Y/Z, PivotX/Y/Z.

**3. Bind buttons to model methods:**
```razor
@* ❌ CURRENT *@
<button class="btn btn-outline-primary" @onclick="CreateCube">Cube (2×2×2)</button>

@* ✅ REPLACEMENT *@
<button class="btn btn-outline-primary" @onclick="_model.CreateCube">Cube (2×2×2)</button>
```

Apply this pattern to ALL buttons.

**4. Status message binding:**
```razor
@if (!string.IsNullOrEmpty(_model?.StatusMessage))
{
    <div class="alert alert-info">
        @_model.StatusMessage
    </div>
}
```

**5. No other changes** — CSS, layout, commented-out Scale controls all stay as-is.

---

## UI Layout (Preserve Exactly)

The page uses the same **3-panel flexbox layout** as SpacialBoxTest, but the controls panel is wider (350px) and includes transform input sections:

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│ <h3>SpacialFrame3D Test Page</h3>                                               │
├───────────────────────┬──────────────────────────────┬──────────────────────────┤
│                       │ Controls Panel (350px)       │ Tree View Panel (350px)  │
│   Canvas3DComponent   │                              │                          │
│   SceneName=          │ ┌──────────────────────────┐ │ ┌──────────────────────┐ │
│   "SpacialFrameScene" │ │ Scene Controls           │ │ │ <RadzenShapeTree..>  │ │
│   1200×1000           │ │  [Clear]                 │ │ │ → replace with       │ │
│                       │ ├──────────────────────────┤ │ │   SceneTreePanel     │ │
│   (shows axis model   │ │ Preset Shapes            │ │ │   or remove          │ │
│    + orange box)      │ │  [Cube] [Long] [Tall]    │ │ └──────────────────────┘ │
│                       │ │  [Wide] [Tiny]           │ │                          │
│                       │ ├──────────────────────────┤ │                          │
│                       │ │ Box Visualization        │ │                          │
│                       │ │  [Vertices] [Edges]      │ │                          │
│                       │ │  [Faces] [Normals]       │ │                          │
│                       │ │  [Quadrants]             │ │                          │
│                       │ ├──────────────────────────┤ │                          │
│                       │ │ Transformation Controls  │ │                          │
│                       │ │ ┌────────────────────┐   │ │                          │
│                       │ │ │ Position           │   │ │                          │
│                       │ │ │ X: [____] [+1]     │   │ │                          │
│                       │ │ │ Y: [____] [+1]     │   │ │                          │
│                       │ │ │ Z: [____] [+1]     │   │ │                          │
│                       │ │ └────────────────────┘   │ │                          │
│                       │ │ ┌────────────────────┐   │ │                          │
│                       │ │ │ Rotation (degrees)  │   │ │                          │
│                       │ │ │ X: [____] [+90°]   │   │ │                          │
│                       │ │ │ Y: [____] [+90°]   │   │ │                          │
│                       │ │ │ Z: [____] [+90°]   │   │ │                          │
│                       │ │ └────────────────────┘   │ │                          │
│                       │ │ @* Scale (commented) *@  │ │                          │
│                       │ │ ┌────────────────────┐   │ │                          │
│                       │ │ │ Pivot Point        │   │ │                          │
│                       │ │ │ X: [____] [+1]     │   │ │                          │
│                       │ │ │ Y: [____] [+1]     │   │ │                          │
│                       │ │ │ Z: [____] [+1]     │   │ │                          │
│                       │ │ └────────────────────┘   │ │                          │
│                       │ │ [Reset Transform]        │ │                          │
│                       │ │ [Show Matrix]            │ │                          │
│                       │ ├──────────────────────────┤ │                          │
│                       │ │ 🔍 Transformation Debug  │ │                          │
│                       │ │  (empty — no buttons)    │ │                          │
│                       │ ├──────────────────────────┤ │                          │
│                       │ │ Status Alert (alert-info) │ │                          │
│                       │ └──────────────────────────┘ │                          │
└───────────────────────┴──────────────────────────────┴──────────────────────────┘
```

### Layout Structure (Exact Markup)
```razor
<h3>SpacialFrame3D Test Page</h3>

<div class="d-flex">
    <Canvas3DComponent SceneName="SpacialFrameScene" @ref="Canvas3DReference"
                       CanvasWidth=@CanvasWidth CanvasHeight=@CanvasHeight />
    
    <div class="controls-panel" style="margin-left: 10px; width: 350px; overflow-y: auto; max-height: 1000px;">
        <!-- Scene Controls (Clear button) -->
        <!-- Preset Shapes (5 buttons) -->
        <!-- Box Visualization (5 buttons) -->
        <!-- Transform sections: Position, Rotation, [Scale commented], Pivot -->
        <!-- Transform buttons: Reset + Show Matrix -->
        <!-- 🔍 Transformation Debugging (empty section) -->
        <!-- Status alert -->
    </div>
    
    <div class="controls-panel" style="margin-left: 10px; width: 350px; overflow: auto; max-height: 1000px;">
        <!-- Tree view panel -->
    </div>
</div>
```

### Key Layout Details
- **Container:** `div.d-flex` (flexbox row, gap: 20px from CSS)
- **Canvas:** 1200×1000, SceneName `"SpacialFrameScene"`
- **Controls panel:** 350px wide (wider than SpacialBoxTest's 300px)
- **Tree panel:** 350px wide, scrollable
- **Transform sections:** Each wrapped in `div.transform-section` (white bg, 1px border, rounded, nested inputs)
- **Input rows:** `div.control-group` — flex row with `<label>` (20px min), `<input type="number">` (flex: 1), `<button class="btn btn-sm">` (+1 or +90°)
- **Scale section:** Entirely commented out with `@* ... *@` — preserve this
- **Rotation subtitle:** Has `<small class="text-muted">→ auto-converted to radians</small>`

### CSS (Preserve — No Changes)
The `<style>` block at the bottom defines `.control-group`, `.transform-section`, `.controls-panel`, `.btn`, `.btn-sm`, `.d-flex` styles. Do NOT modify.

### What Changes in the Markup
FOUR things change:
1. `@onclick="MethodName"` → `@onclick="_model.MethodName"` on all buttons
2. `@bind="PropertyName"` → `@bind="_model.PropertyName"` on all inputs
3. `@bind:after="AutoRefreshShape"` → `@bind:after="_model.AutoRefreshShape"` (or lambda wrapper if needed)
4. `<RadzenShapeTreeView/>` → `<SceneTreePanel .../>` or removed (tree panel)
5. Status alert: `@StatusMessage` → `@_model?.StatusMessage`

All CSS, HTML structure, class names, and layout stay **exactly the same**.

---

## Implementation Steps

### Step 1: Create SpacialFrameTestModel.cs
1. Create new file
2. Move all domain methods from code-behind
3. Move all UI-bound properties (dimensions, transforms, status)
4. Constructor takes IFoundryService, IGeometryVisualizationService, NavigationManager
5. Add `OnStateChanged` callback
6. Replace all `StateHasChanged()` calls with `OnStateChanged?.Invoke()`
7. **Verify:** File compiles

### Step 2: Slim SpacialFrameTest.razor.cs
1. Remove all domain methods and properties
2. Keep only lifecycle, injections, parameters, canvas ref
3. Create model in `OnInitialized()`
4. Wire `OnStateChanged`
5. Pass stage/scene in `OnAfterRenderAsync`
6. **Verify:** Both files compile

### Step 3: Update SpacialFrameTest.razor
1. Replace `<RadzenShapeTreeView/>` (same decision as SpacialBoxTest)
2. Change ALL `@bind="PropertyName"` to `@bind="_model.PropertyName"`
3. Change ALL `@bind:after="MethodName"` to `@bind:after="_model.MethodName"`
4. Change ALL `@onclick="MethodName"` to `@onclick="_model.MethodName"`
5. Update StatusMessage binding
6. **Verify:** Page compiles, loads, renders

### Step 4: Verify Transform Controls
1. Change Position X input → shape moves
2. Click +1 on Position Y → shape moves up
3. Change Rotation X to 45 → shape tilts
4. Click +90° on Rotation Y → shape rotates 90°
5. Change Pivot X to 1.0 → shape shifts pivot
6. Click Reset Transform → everything resets to defaults
7. Click Show Matrix → status shows matrix values
8. **Verify:** All transform controls work as before

### Step 5: Verify Visualizations
1. Click Show Vertices → blue spheres at world-space corners
2. Click Show Edges → pipes along edges
3. Click Show Faces → face boundaries
4. Click Show Normals → red arrows
5. Click Show Quadrants → 8 colored markers around center
6. **Apply a rotation first, then visualize** — markers should follow the transformed positions
7. **Verify:** World-space transforms applied correctly

### Step 6: Verify Lifecycle
1. Click Clear → shapes removed
2. Navigate away → no errors
3. Navigate back → fresh state
4. **Verify:** Clean lifecycle

---

## Step-by-Step Test Sequence

### Step 1: Page Load

**Action:** Navigate to `/spacialframetest`

**Expected Results:**
- ✅ Page title "SpacialFrame3D Test Page" visible
- ✅ Canvas3D viewport with axis model
- ✅ Orange box (2×1.5×1) at origin
- ✅ Controls panel with all sections visible
- ✅ Transform inputs showing defaults (0, 0, 0 for position/rotation/pivot)
- ✅ Status: "Created SpacialFrame3D (FoShape3D): 2×1.5×1m"

**Console Output:**
```
✅ SpacialFrameTest: Retrieved stage 'SpacialFrameScene' from Canvas
✅ [axis model URL]
Creating SpacialFrame3D with Rotation 0×0×0° at Position (0, 0, 0), Pivot (0, 0, 0), Scale (1, 1, 1)
```

### Step 2: Transform Controls

**Action:** Set Position Y to 1.0 (type in input, tab out)

**Expected Results:**
- ✅ Orange box moves up 1 unit
- ✅ Status updates with creation message
- ✅ Console shows "🔄 AutoRefreshShape called"

**Action:** Click +90° on Rotation Y

**Expected Results:**
- ✅ Box rotates 90° around Y axis
- ✅ RotationY input now shows 90

### Step 3: Preset Shape + Visualization

**Action:** Click "Tall Box (1×4×1)", then "Show Vertices"

**Expected Results:**
- ✅ Box changes to tall proportions
- ✅ 8 blue marker spheres at world-space corners
- ✅ Vertices are at TRANSFORMED positions (reflecting any position/rotation changes)

### Step 4: Reset and Matrix

**Action:** Click "Reset Transform", then "Show Matrix"

**Expected Results:**
- ✅ All inputs reset to 0/0/0 (position, rotation, pivot) and 1/1/1 (scale)
- ✅ Box returns to origin
- ✅ Status shows transform matrix (identity or near-identity)

### Step 5: Clear and Rebuild

**Action:** Click "Clear", then "Cube (2×2×2)"

**Expected Results:**
- ✅ Clear removes all shapes
- ✅ New cube appears

**If Failed:**
- No canvas → Check Canvas3DComponent @ref binding
- No box → Check _model.Stage is set in OnAfterRenderAsync
- Inputs not updating shape → Check @bind:after wiring to model

---

## Success Criteria

### Compilation
- [ ] Zero compilation errors
- [ ] `SpacialFrameTestModel.cs` compiles independently

### Runtime (First Load)
- [ ] Page loads without exceptions
- [ ] Axis model visible
- [ ] Orange box (2×1.5×1) visible at origin
- [ ] Transform controls show default values (0, 0, 0 for position/rotation/pivot; 1, 1, 1 for scale)

### Runtime (Transform Controls)
- [ ] Changing any Position input moves the shape
- [ ] Changing any Rotation input rotates the shape (degrees → radians conversion works)
- [ ] Changing any Pivot input shifts the pivot point
- [ ] +1 increment buttons work (position, pivot)
- [ ] +90° rotation buttons work
- [ ] Reset Transform restores all defaults
- [ ] Show Matrix displays formatted matrix

### Runtime (Visualization)
- [ ] All 5 Show methods produce markers at correct world-space positions
- [ ] Markers follow transforms (rotate shape, then show vertices → vertices at rotated positions)

### Architecture
- [ ] `SpacialFrameTestModel.cs` contains all domain logic
- [ ] `SpacialFrameTest.razor.cs` is under 50 lines
- [ ] Razor binds directly to `_model.*` — no code-behind delegate methods
- [ ] `RadzenShapeTreeView` replaced (same decision as SpacialBoxTest)

### Known Issues (Acceptable)
- [ ] Scale controls remain commented out in razor (intentional)
- [ ] Empty debug-buttons section in razor (pre-existing)

### Disposal
- [ ] No console errors on navigation away

---

## Project Convention Compliance

### Convention: Model-Behind Pattern
**Evidence:** SpacialBoxTest is being modernized with the same pattern in a parallel spec.
**This spec:** ✅ Creates `SpacialFrameTestModel` as a plain class (same decision as SpacialBoxTest — see that spec for reasoning)

### Convention: Consistent Sibling Treatment
**Evidence:** SpacialBoxTest and SpacialFrameTest are siblings (same domain, same services, same layout). They should be modernized the same way.
**This spec:** ✅ Follows identical extraction pattern

### Convention: Tree View Decision
**Evidence:** SpacialBoxTest spec encountered the same `Three2025.csproj` missing reference issue.
**This spec:** ✅ Follow whatever decision was made for SpacialBoxTest (add reference, or remove panel)

---

## Implementer Behavior Warnings

### Things You Will Be Tempted To Do (DON'T)

1. **Consolidate the 12 increment methods into a generic helper**
   Why you'll want to: "These are all identical: `value += 1.0; CreateSpacialFrame();`"
   Why you shouldn't: The razor binds to specific method names (`@onclick="_model.IncrementPositionX"`). A generic `Increment(ref double value)` changes the binding pattern. Keep the simple, repetitive methods. Refactoring the duplication is a separate task.

2. **Uncomment the Scale controls**
   Why you'll want to: "Scale is disabled, I should enable it since I'm modernizing"
   Why you shouldn't: They're commented out intentionally. The `OnComputed` callback is also commented out. These are separate features, not part of this modernization scope.

3. **Add code-behind delegate methods instead of binding directly to model**
   Why you'll want to: "SpacialBoxTest spec shows delegate methods in the code-behind"
   Why you shouldn't: SpacialBoxTest needed delegates because the spec was written before realizing direct binding works. `@onclick="_model.CreateCube"` is simpler and achieves the same result. Use direct binding — it makes the code-behind shorter.

4. **Change the clear-and-recreate pattern to an update pattern**
   Why you'll want to: "Clearing and recreating the entire shape on every input change is wasteful"
   Why you shouldn't: The existing behavior clears the stage and recreates the shape. Changing to an in-place update would require verifying that stale flags propagate correctly for every property change. Out of scope.

---

## Silent Failure Audit

### RadzenShapeTreeView Reference
- **What happens:** Compile error or empty panel
- **How Indy would know:** Compile error (most likely)
- **Mitigation:** Same resolution as SpacialBoxTest

### @bind:after on Model Method
- **What happens:** If `_model.AutoRefreshShape` signature doesn't match what Blazor expects for `@bind:after`, it silently won't fire
- **Expected signature:** `void AutoRefreshShape()` or `Task AutoRefreshShape()` — both work with `@bind:after`
- **Mitigation:** `AutoRefreshShape()` returns void in the model — this matches. If issues occur, fall back to a code-behind wrapper.

### Direct @onclick to Model Method
- **What happens:** `@onclick="_model.CreateCube"` requires `_model` to be non-null at render time
- **Since `_model` is created in `OnInitialized()`** (which runs before first render), this is safe
- **Edge case:** If the razor renders before `OnInitialized` somehow — `_model` would be null → NullReferenceException. Use `@onclick="() => _model?.CreateCube()"` as defensive pattern if this occurs.

---

## Confidence Levels

| Area | Confidence | Rationale |
|------|-----------|-----------|
| **Model extraction** | 🟢 High | Direct sibling of SpacialBoxTest — same pattern, simpler page |
| **Transform binding** | 🟢 High | `@bind="_model.Property"` is standard Blazor |
| **Direct method binding** | 🟢 High | `@onclick="_model.Method"` is standard Blazor |
| **Visualization** | 🟢 High | Identical to SpacialBoxTest — same APIs |
| **Tree view** | 🟡 Medium | Same blocker as SpacialBoxTest — depends on that decision |
| **@bind:after pattern** | 🟢 High | Verified — void methods work with @bind:after |

---

## Build Journal Requirement

Maintain `BUILD_JOURNAL_SPACIALFRAMETEST.md` in the project root. Log as you go:
- Phase start/end times
- Decisions that differed from spec (and why)
- Whether SpacialBoxTest's tree view decision was reused
- Whether direct model binding (`_model.Method`) worked or needed delegates
- Final line count for code-behind (target: <50)

---

*This specification follows the Atlas Specification Checklist v1.2 (February 10, 2026).*
