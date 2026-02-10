# SpacialBoxTest Page Specification

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Target:** Modernize `SpacialBoxTest.razor` / `SpacialBoxTest.razor.cs`  
**Status:** Specification for Indy

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about modernizing SpacialBoxTest — it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 3D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you modernize SpacialBoxTest, you're testing whether AI can:
1. Successfully use 40 years of distilled programming patterns
2. Translate a legacy monolithic code-behind into the modern Model-behind architecture
3. Correctly separate Blazor UI concerns from domain logic (spatial geometry, dirty flags, animation)
4. Replace non-existent components with their actual modern equivalents

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can leverage accumulated human wisdom to modernize a complex test page — one that exercises 3D shape creation, spatial geometry computation, pivot-based animation, dirty flag verification, and tree view visualization — into the clean Model-behind pattern that every other modern page follows.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## Specification Handoff Summary

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Estimated Implementation Time:** 2-4 hours  
**Confidence:** 🟢 High overall (well-charted territory — sibling SpacialFrameTest follows same patterns)

**Primary Reference:** `SpacialFrameTest.razor.cs` — Closest sibling; same domain (spatial geometry), same services, same stage pattern. Copy its structure.

**High Uncertainty Areas:**
1. **Tweener animations** — The existing Tweener code creates `new Tweener()` per animation without pumping `Tweener.Update()`. These animations likely **don't work** as written. The model should either integrate with the animation frame loop or document this as a known issue.
2. **ShapeTreeView component** — Does not exist in the workspace. Must be replaced with `SceneTreePanel` from FoundryMicroCore.Blazor.Controls, or removed entirely.

**Known Limitations:**
1. Tweener-based animations may need rework to actually function (no `Update()` pump)
2. `RadzenShapeTreeView` (used in sibling SpacialFrameTest) also doesn't exist — both pages share this debt

**Verification Checklist:**
- [ ] Compiles without errors
- [ ] Runs without exceptions
- [ ] Box creation works (all 5 presets)
- [ ] Visualization methods work (vertices, edges, faces, normals, quadrants)
- [ ] Submarine model loads
- [ ] Dirty flag tests produce console output
- [ ] Tree view panel renders (with SceneTreePanel or acceptable fallback)
- [ ] Navigation away produces no console errors

---

## Phase 1: Project Convention Scan

### Directory: `Components/Pages/`
**Files found:** 40+ razor/cs pairs

### Universal Patterns (all modern pages follow):
- **Base class:** `ComponentBase` — every page
- **Pattern:** `partial class` — newer pages use partial class (no `@inherits`); some older pages use `@inherits SomeBase` 
- **Namespace:** `Three2025.Components.Pages` — almost all pages (ClockDemo is the outlier)
- **IDisposable:** Most pages implement `IDisposable`
- **Canvas reference:** Either `public Canvas3DComponent Canvas3DReference = null` (older) or `private Canvas3DComponent _canvasRef` (newer)
- **Stage acquisition:** `Canvas3DReference?.Stage` (from canvas) or `arena.EstablishStage<FoStage3D>(sceneName)` (from arena)

### Model-Behind Pattern:
- **ClockDemoModel.cs** exists as a separate `MxComponent` model — the only page with a proper extracted model
- **All other pages** keep logic in code-behind — this is the legacy pattern the project is moving AWAY from
- **Convention direction:** New pages should have a Model class. The existing SpacialBoxTest has 573 lines of code-behind — well above the "extract a model" threshold.

### This Spec MUST Follow:
- [x] Include Model class definition (`SpacialBoxTestModel`)
- [x] Use `partial class` pattern (not `@inherits`)
- [x] Namespace `Three2025.Components.Pages`
- [x] Implement `IDisposable`
- [x] Match injection pattern used by SpacialFrameTest (closest sibling)

---

## Phase 2: Architecture Analysis

**Based on:** `SpacialFrameTest.razor.cs` (445 lines), `SpacialBoxTest.razor.cs` (573 lines)

**Legacy or Modern?:** Legacy — SpacialBoxTest stuffs everything into the code-behind: shape creation, animation, visualization delegation, dirty flag testing, status management. No Model class.

**Intent to preserve:**
- Create a 3D box with configurable dimensions (W×H×D)
- 5 preset shapes (Cube, LongBox, TallBox, WideBox, TinyBox)
- Visualize spatial geometry: vertices, edges, faces, normals, quadrants
- Load submarine model with full spatial frame visualization
- Animated pivot tests: door hinge, corner balance, center spin, reset to floor
- Dirty flag verification: check flags, propagation, tweener integration, initialization timing, object replacement
- Status message display
- Scene tree view panel

**Patterns to NOT carry forward:**
- 573 lines of monolithic code-behind
- Direct `Tweener` construction without animation loop integration
- Reference to non-existent `ShapeTreeView` component
- Mixed UI state (`StatusMessage`, `StateHasChanged()`) with domain logic
- `IsDirty` property usage (API reference says use `IsStale()` / `Set*Stale()` — the existing code uses both `IsDirty` and stale flags, which suggests `IsDirty` is a different mechanism on `Transform3`)

**Current Pattern (for the spec):** Model-behind with code-behind delegation

---

## Verified Against

- **API Reference:** `FOUNDRY_3D_API_REFERENCE.md` (v25.5.0, Feb 8, 2026) — source-audited
- **API Reference:** `FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md` (v25.5.0, Feb 8, 2026)
- **API Reference:** `FOUNDRY_MICROCORE_BLAZOR_CONTROLS_API_REFERENCE.md` (v1.3.0, Feb 1, 2026)
- **Last verified:** February 10, 2026

### Method Verification:
- ✅ `FoShape3D.CreateBox(name, width, height, depth)` — verified in FOUNDRY_3D_API_REFERENCE.md
- ✅ `FoModel3D.CreateModel(name, url, width, height, depth)` — verified in FOUNDRY_3D_API_REFERENCE.md
- ✅ `FoStage3D.AddShape<T>(value)` — verified, dispatches to Bodies or Links based on interface
- ✅ `FoStage3D.ClearAll()` — verified, async
- ✅ `Canvas3DComponent.Stage` → `FoStage3D?` — verified
- ✅ `Canvas3DComponent.GetActiveScene()` → `(bool, Scene3D)` — verified
- ✅ `Transform3.Position`, `.Rotation`, `.Pivot`, `.Scale` — verified (Rotation is **radians**)
- ✅ `Transform3.IsDirty` — verified as read-only property on Transform3
- ✅ `Transform3.OnChange` — verified as public subscriber
- ✅ `Transform3.SetOwnerNotification(Action<bool>)` — auto-wired by FoGlyph3D
- ✅ `FoGlyph3D.SetTransformStale()` / `SetGeometryStale()` — verified
- ✅ `Euler.FromDegrees(x, y, z)` — used in existing code
- ✅ `SpacialBox3D(FoShape3D, string)` constructor — verified in source
- ✅ `SpacialFrame3D(FoShape3D, string)` constructor — verified in source
- ✅ `IGeometryVisualizationService.ShowLabeledVertices/Edges/Faces/Normals` — verified
- ✅ `IGeometryVisualizationService.CreateMarkerSphere` — verified
- ✅ `SceneTreePanel` — verified: `Model`, `Stage`, `Title`, `EmptyMessage`, `DefaultTab` parameters

### Hallucinated APIs (DO NOT USE):
- ❌ `SetPosition()`, `SetRotation()`, `SetScale()` — do not exist on FoShape3D
- ❌ `shape3D.SetDirty()` — does not exist. Use `Set*Stale()` methods
- ❌ `shape3D.IsChanged()` — does not exist. Use `IsStale()`
- ❌ `ShapeTreeView` — component does not exist in workspace
- ❌ `RadzenShapeTreeView` — component does not exist in workspace

---

## Reference Implementation Strategy

### Primary Reference
**Copy:** `SpacialFrameTest.razor.cs` (445 lines)  
**Demonstrates:** Same injection pattern, same services (IFoundryService, IGeometryVisualizationService), same stage acquisition pattern, same shape lifecycle, same spatial geometry domain.

### Secondary Reference  
**Study:** `ClockDemoModel.cs`  
**Demonstrates:** The Model-behind pattern — how to extract domain logic into an `MxComponent`-derived model class.

### Modification Steps
1. Create `SpacialBoxTestModel.cs` in `Components/Pages/` — extract ALL domain logic from code-behind
2. Slim `SpacialBoxTest.razor.cs` to lifecycle + UI delegation only 
3. Update `SpacialBoxTest.razor` — replace `<ShapeTreeView/>` with `<SceneTreePanel>` (or remove if blocking)
4. Keep existing functionality intact — this is a refactor, not a rewrite

### Delta from Reference (SpacialFrameTest):
- **More features:** SpacialBoxTest has animations, dirty flag tests, submarine model — SpacialFrameTest doesn't
- **Model extraction:** SpacialFrameTest doesn't have a model either (it's also legacy), but SpacialBoxTest is complex enough to warrant one
- **Tweener usage:** SpacialBoxTest uses Unglide Tweener — this needs careful handling in model extraction

---

## Infrastructure Assumptions

### Assumption: Canvas3DComponent Stage Works
- [x] `Canvas3DReference?.Stage` returns the page's `FoStage3D`
- [x] Stage is available after `OnAfterRenderAsync(firstRender)`
- [x] `stage.AddShape(shape)` adds to stage bodies collection
- [x] `stage.ClearAll()` clears all shapes
- If broken: Check `Canvas3DComponent.razor.cs` lifecycle

### Assumption: SpacialBox3D Geometry Works
- [x] `new SpacialBox3D(shape, "m")` wraps an existing FoShape3D
- [x] `GetLocalVertices()`, `GetLocalEdges()`, `GetLocalFaces()` compute geometry from shape dimensions
- [x] All geometry centered at origin via half-extent offsets
- If broken: Check `FoundryWorldsAndDrawings/Shapes3D/SpacialFrame/SpacialBox3D.cs`

### Assumption: Visualization Service Works
- [x] `VisualizationService.ShowLabeledVertices(arena, vertices)` creates marker spheres
- [x] Service creates its own "Visualization" stage via `arena.EstablishStage<FoStage3D>("Visualization")`
- [x] All marker shapes render immediately after creation
- If broken: Check `FoundryWorldsAndDrawings/Services/GeometryVisualizationService.cs`

### Assumption: Transform3 Dirty Tracking Works
- [x] `Transform3.IsDirty` reflects whether transform has uncommitted changes
- [x] `Transform3.OnChange` callback fires when properties are assigned (object replacement triggers, mutation does not)
- [x] `FoGlyph3D` auto-wires `SetOwnerNotification` so transform changes call `SetTransformStale()`
- If broken: Check `Transform3` source and `FoGlyph3D.Transform` setter

### ⚠️ Assumption: Tweener Animations Work (LOW CONFIDENCE)
- [ ] `new Tweener()` per animation button click — tweener is NOT stored as field, NOT pumped
- [ ] No visible `Tweener.Update(deltaTime)` call in code-behind or animation frame subscription
- [ ] The existing animations likely **do not visually run** — they configure tweens but never update them
- If broken: Either integrate with `AnimationFrameBus` or use `OnBeforeRender` pattern instead
- **Investigation:** `Shape3DTech.cs` line 343 shows the working pattern: `Unglide.Tween.TweenerImpl.Tweener.Update(frameTime)` inside an async loop

---

## Code Smells to Avoid

### From MicroCore (CODE_SMELLS_ANALYSIS.md)

#### Collection Query Anti-Pattern
**Don't:**
```csharp
var item = editor.AllMembers().Items.ToArray().FirstOrDefault(x => x.Name == "foo");
```
**Do:**
```csharp
var item = editor.FindByName("foo"); // O(1) dictionary lookup
```

### Task-Specific Warnings

#### Non-Existent Component References
**Problem:** `<ShapeTreeView/>` and `<RadzenShapeTreeView/>` don't exist in the workspace
**Solution:** Use `<SceneTreePanel Stage="@_stage" />` from FoundryMicroCore.Blazor.Controls

#### Orphaned Tweener Instances
**Problem:** `new Tweener()` per button click without storing or pumping — animations silently do nothing
**Solution:** Either store tweener as field and pump in animation frame, or use `OnBeforeRender` callbacks

#### Property Mutation vs Object Replacement (Already Documented in Code)
**Problem:** `transform.Rotation.Y += 0.1` mutates the existing Euler — no dirty flag triggered
**Solution:** `transform.Rotation = new Euler(x, y + 0.1, z)` creates new object — triggers dirty flag
**Note:** The existing code documents this correctly in comments. Preserve in model.

---

## Known Gotchas

### ShapeTreeView Doesn't Exist
- The current razor file references `<ShapeTreeView/>` — this component does not exist anywhere in the workspace
- **SpacialFrameTest** has the same problem with `<RadzenShapeTreeView/>`
- **Solution:** Replace with `<SceneTreePanel Stage="@_spacialBoxStage" />` or remove the third panel
- **Risk:** The project may not yet have the `@using` for `FoundryMicroCore.Blazor.Controls.Components.TreeView` in `_Imports.razor`. Check and add if needed.

### Tweener Not Being Pumped
- Every animation method creates `new Tweener()` as a local variable
- No `tweener.Update(dt)` is ever called
- The tweener is garbage-collected when the method returns
- **This means the Door Hinge, Corner Balance, Center Spin, and Reset animations likely don't work**
- **Solution in model:** Document as known limitation, or wire into animation frame

### Transform3 Rotation Is Radians
- `Transform3.Rotation` is in **radians** 
- Use `Euler.FromDegrees(x, y, z)` for degree-based input
- Existing code does this correctly — preserve pattern

### Model3D URL Path
- Uses `Path.Combine(Navigation.BaseUri, filename)` which is a URL join, not filesystem join
- `Path.Combine` works on Windows but may produce wrong separators for URLs
- The existing pattern works in practice — preserve it

---

## Troubleshooting Guide

### Box Not Appearing

**Symptom:** Page loads, axis appears, but green box is not visible

**Diagnosis Steps:**
1. Check `_spacialBoxStage` is not null: `$"Stage: {_spacialBoxStage?.Name}".WriteInfo()`
2. Check `CurrentBox` was created: `$"Box created: {CurrentBox != null}".WriteInfo()`
3. Verify `_scene` was obtained from canvas
4. Check `IsReady` returns true (both Arena and Scene not null)

**Common Causes:**
- `Canvas3DReference` is null (razor `@ref` binding issue)
- `GetActiveScene()` returned `(false, null)` — canvas not ready
- Stage not obtained: `Canvas3DReference?.Stage` returned null

### Visualization Markers Not Showing

**Symptom:** "Show Vertices" clicked but no blue spheres appear

**Diagnosis Steps:**
1. Check `CurrentBox` is not null (`EnsureBoxCreated()` should catch this)
2. Check `VisualizationService` is injected (not null)
3. Check `Arena` — `FoundryService?.Arena()` must return non-null
4. Look for console output from visualization service

**Common Causes:**
- Box not created yet
- Arena not available (FoundryService not injected or not initialized)
- Visualization service creates shapes on separate "Visualization" stage — they should render

### Dirty Flag Tests Show Unexpected Values

**Symptom:** `Shape.IsDirty` or `Transform.IsDirty` don't match expectations

**Diagnosis Steps:**
1. Remember: `IsDirty` on Transform3 is separate from `IsStale()` on FoGlyph3D
2. After object replacement (`transform.Position = new Vector3(...)`) → should trigger notify
3. After mutation (`transform.Rotation.Y += 0.1`) → will NOT trigger notify
4. Check `transform.OnChange` subscription is wired

**Common Causes:**
- Confusing `Transform3.IsDirty` with `FoGlyph3D.IsStale()`
- Mutation instead of object replacement
- OnChange subscription not wired (FoGlyph3D wires this automatically on `Transform` setter)

---

## Component Structure

### Files to Create/Modify

| File | Action | Purpose |
|------|--------|---------|
| `Components/Pages/SpacialBoxTestModel.cs` | **CREATE** | Model class — domain logic |
| `Components/Pages/SpacialBoxTest.razor.cs` | **MODIFY** | Slim code-behind — lifecycle + delegation |
| `Components/Pages/SpacialBoxTest.razor` | **MODIFY** | Replace ShapeTreeView, wire model |

---

## Model Definition

### SpacialBoxTestModel (New File)

**Location:** `Components/Pages/SpacialBoxTestModel.cs`  
**Namespace:** `Three2025.Components.Pages`

```csharp
namespace Three2025.Components.Pages;

public class SpacialBoxTestModel
{
    // === Dependencies (injected via constructor) ===
    private readonly IFoundryService _foundryService;
    private readonly IGeometryVisualizationService _visualizationService;
    private readonly NavigationManager _navigation;

    // === Shape State ===
    public SpacialBox3D? CurrentBox { get; private set; }
    public FoStage3D? Stage { get; set; }

    // === Dimension State ===
    public double BoxWidth { get; set; } = 2.0;
    public double BoxHeight { get; set; } = 1.5;
    public double BoxDepth { get; set; } = 1.0;

    // === UI State ===
    public string StatusMessage { get; private set; } = string.Empty;
    public Action? OnStateChanged { get; set; }

    private string _mainBoxGuid = Guid.NewGuid().ToString();
    private IArena Arena => _foundryService?.Arena();

    public SpacialBoxTestModel(
        IFoundryService foundryService,
        IGeometryVisualizationService visualizationService,
        NavigationManager navigation)
    {
        _foundryService = foundryService;
        _visualizationService = visualizationService;
        _navigation = navigation;
    }
}
```

**Constructor Parameters:**
- `IFoundryService foundryService` — Arena access for shapes
- `IGeometryVisualizationService visualizationService` — Marker rendering
- `NavigationManager navigation` — URL resolution for models

**Responsibilities (moved FROM code-behind):**
- Box creation and update logic (`CreateSpacialBox`, `CreateNewBox`, `UpdateExistingBox`)
- Preset shape methods (`CreateCube`, `CreateLongBox`, etc.)
- Visualization methods (`ShowVertices`, `ShowEdges`, `ShowFaces`, `ShowNormals`, `ShowQuadrants`)
- Submarine model loading (`ShowSubModel`)
- Animation methods (`AnimateDoorHinge`, `AnimateCornerBalance`, `AnimateCenterSpin`, `AnimateResetToFloor`)
- Dirty flag test methods (all 6 test methods)
- Status management (`SetStatus`)
- Axis loading (`AddAxisToScene`)
- Helper methods (`EnsureReady`, `EnsureBoxCreated`, `GetReferenceTo`)

**Code-behind keeps:**
- `Canvas3DComponent` reference (Blazor component ref — can't live in model)
- `OnAfterRenderAsync` lifecycle (Blazor-specific)
- `[Inject]` services (Blazor DI — passes to model constructor)
- `[Parameter]` properties (`CanvasWidth`, `CanvasHeight`)
- `Scene3D` reference (obtained from Canvas in lifecycle)
- Delegation calls to model
- `StateHasChanged()` calls (triggered via model's `OnStateChanged` callback)
- `IDisposable.Dispose()`

**Code-behind delegates to model:**
- `_model.Initialize(stage, scene)` — passes stage/scene from lifecycle
- `_model.CreateSpacialBox()` — all box creation
- `_model.ShowVertices()` / `ShowEdges()` / `ShowFaces()` / `ShowNormals()` / `ShowQuadrants()` — visualization
- `_model.ShowSubModel()` — submarine
- `_model.AnimateDoorHinge()` / etc. — animations
- `_model.VerifyDirtyFlags()` / etc. — dirty flag tests
- `_model.ClearAll()` — clear scene

---

## Code-Behind Structure (Slimmed)

### SpacialBoxTest.razor.cs (After Refactoring)

**Target:** Under 80 lines. All domain logic lives in `SpacialBoxTestModel`.

```csharp
namespace Three2025.Components.Pages;

public partial class SpacialBoxTest : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponent Canvas3DReference = null;
    private SpacialBoxTestModel _model;
    private Scene3D _scene;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    // Delegate to model for binding
    protected string StatusMessage => _model?.StatusMessage ?? string.Empty;

    protected override void OnInitialized()
    {
        _model = new SpacialBoxTestModel(FoundryService, VisualizationService, Navigation);
        _model.OnStateChanged = () => InvokeAsync(StateHasChanged);
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
            if (!found || scene == null) return base.OnAfterRenderAsync(firstRender);

            _scene = scene;
            _model.Stage = Canvas3DReference?.Stage;
            _model.Initialize(_scene);
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    // === Button handlers — all delegate to model ===
    protected void CreateCube() => _model.CreateCube();
    protected void CreateLongBox() => _model.CreateLongBox();
    // ... etc — one-line delegates

    public void Dispose()
    {
        _model = null;
        _scene = null;
    }
}
```

---

## Razor Markup Changes

### SpacialBoxTest.razor

**Key changes from current:**

1. **Replace `<ShapeTreeView/>`** — this component doesn't exist:

```razor
@* ❌ CURRENT (doesn't compile — ShapeTreeView doesn't exist) *@
<ShapeTreeView/>

@* ✅ REPLACEMENT — SceneTreePanel from FoundryMicroCore.Blazor.Controls *@
<SceneTreePanel Stage="@_model?.Stage" Title="SpacialBox Scene" EmptyMessage="Create a box to see shapes." DefaultTab="shapes" />
```

**⚠️ CRITICAL (Found During Prediction Review):** `Three2025.csproj` does NOT reference `FoundryMicroCore.Blazor.Controls`. SceneTreePanel will NOT resolve without:
1. Adding `<ProjectReference Include="..\FoundryMicroCore\FoundryMicroCore.Blazor.Controls\FoundryMicroCore.Blazor.Controls.csproj" />` to `Three2025.csproj`
2. Adding `@using FoundryMicroCore.Blazor.Controls.Components.TreeView` to `_Imports.razor` or the page

**If Sully approves the dependency:** Add both the project reference and the @using.  
**If Sully declines:** Remove the `<ShapeTreeView/>` panel entirely — the page works fine without a tree view. Do not build a custom replacement.

2. **Wire button handlers through model** — existing `@onclick` bindings stay the same since code-behind methods still exist as one-line delegates.

3. **Status message binding** — change from `StatusMessage` property to `_model?.StatusMessage`:
```razor
@if (!string.IsNullOrEmpty(_model?.StatusMessage))
{
    <div class="alert alert-info">
        @_model.StatusMessage
    </div>
}
```

4. **No other markup changes needed** — button layout, CSS, Canvas3DComponent are all correct.

---

## UI Layout (Preserve Exactly)

The page uses a proven **3-panel flexbox layout** that must be preserved exactly:

```
┌──────────────────────────────────────────────────────────────────────┐
│ <h3>SpacialBox3D Test Page</h3>                                     │
├───────────────────────┬─────────────────┬────────────────────────────┤
│                       │ Controls Panel  │ Tree View Panel            │
│   Canvas3DComponent   │ (300px wide)    │ (350px wide)               │
│   SceneName=          │                 │                            │
│   "SpacialBoxScene"   │ ┌─────────────┐ │ ┌──────────────────────┐   │
│   1200×1000           │ │Scene Controls│ │ │ <ShapeTreeView/>     │   │
│                       │ │ [Clear]      │ │ │ → replace with       │   │
│   (shows axis model   │ │ [Show Sub]   │ │ │   SceneTreePanel     │   │
│    + orange/green box)│ ├─────────────┤ │ │   or remove           │   │
│                       │ │Preset Shapes │ │ └──────────────────────┘   │
│                       │ │ [Cube]       │ │                            │
│                       │ │ [Long Box]   │ │                            │
│                       │ │ [Tall Box]   │ │                            │
│                       │ │ [Wide Box]   │ │                            │
│                       │ │ [Tiny Box]   │ │                            │
│                       │ ├─────────────┤ │                            │
│                       │ │Animated Pivot│ │                            │
│                       │ │ [🚪 Door]    │ │                            │
│                       │ │ [⚖️ Corner]  │ │                            │
│                       │ │ [🌀 Spin]    │ │                            │
│                       │ │ [🏠 Reset]   │ │                            │
│                       │ ├─────────────┤ │                            │
│                       │ │Box Visual    │ │                            │
│                       │ │ [Vertices]   │ │                            │
│                       │ │ [Edges]      │ │                            │
│                       │ │ [Faces]      │ │                            │
│                       │ │ [Normals]    │ │                            │
│                       │ │ [Quadrants]  │ │                            │
│                       │ ├─────────────┤ │                            │
│                       │ │Dirty Flag    │ │                            │
│                       │ │ [🧪 Check]   │ │                            │
│                       │ │ [🔄 Propagn] │ │                            │
│                       │ │ [🎬 Tweener] │ │                            │
│                       │ │ [🔬 Init]    │ │                            │
│                       │ │ [⏰ Timing]  │ │                            │
│                       │ │ [🔄 ObjRepl] │ │                            │
│                       │ ├─────────────┤ │                            │
│                       │ │ Status Alert │ │                            │
│                       │ │ (alert-info) │ │                            │
│                       │ └─────────────┘ │                            │
└───────────────────────┴─────────────────┴────────────────────────────┘
```

### Layout Structure (Exact Markup)
```razor
<h3>SpacialBox3D Test Page</h3>

<div class="d-flex">
    <Canvas3DComponent SceneName="SpacialBoxScene" @ref="Canvas3DReference" 
                       CanvasWidth=@CanvasWidth CanvasHeight=@CanvasHeight />
    
    <div class="controls-panel" style="margin-left: 10px; width: 300px; overflow-y: auto; max-height: 1000px;">
        <!-- 6 button sections + status alert -->
    </div>
    
    <div class="controls-panel" style="margin-left: 10px; width: 350px; overflow: auto; max-height: 1000px;">
        <!-- Tree view panel -->
    </div>
</div>
```

### Key Layout Details
- **Container:** `div.d-flex` (flexbox row, gap: 20px from CSS)
- **Canvas:** No explicit width style — uses CanvasWidth/CanvasHeight parameters (1200×1000)
- **Controls panel:** `controls-panel` class, 300px wide, scrollable, max-height 1000px
- **Tree panel:** `controls-panel` class, 350px wide, scrollable, max-height 1000px
- **Button sections:** Each uses a category-specific class (`scene-buttons`, `preset-buttons`, `pivot-buttons`, `visual-buttons`, `dirty-flag-buttons`) — all styled as vertical flex columns with 3px gap
- **All buttons:** Full-width (`width: 100%`), small font (12px, 5px 8px padding)
- **Status alert:** Conditional `alert alert-info` at bottom of controls panel

### CSS (Preserve — No Changes)
The existing `<style>` block defines `.controls-panel`, `.btn`, `.d-flex` overrides. Do NOT modify.

### What Changes in the Markup
Only THREE things change:
1. `@onclick="MethodName"` → `@onclick="_model.MethodName"` (or `@onclick="() => _model?.MethodName()"`) on all buttons
2. `@StatusMessage` → `@_model.StatusMessage` in the status alert
3. `<ShapeTreeView/>` → `<SceneTreePanel .../>` or removed (tree panel)

---

## Implementation Steps

### Step 1: Create SpacialBoxTestModel.cs
1. Create new file `Components/Pages/SpacialBoxTestModel.cs`
2. Move ALL domain methods from `SpacialBoxTest.razor.cs` into the model
3. Constructor takes `IFoundryService`, `IGeometryVisualizationService`, `NavigationManager`
4. Add `OnStateChanged` callback (replaces direct `StateHasChanged()` calls)
5. Add `Initialize(Scene3D scene)` method for post-lifecycle setup
6. **Verify:** File compiles standalone

### Step 2: Slim SpacialBoxTest.razor.cs
1. Remove all domain methods (now in model)
2. Keep only lifecycle, injections, parameters, canvas ref
3. Create model in `OnInitialized()`
4. Wire `OnStateChanged` callback
5. Pass stage/scene to model in `OnAfterRenderAsync`
6. Add one-line delegate methods for each button handler
7. **Verify:** Both files compile together

### Step 3: Update SpacialBoxTest.razor
1. Replace `<ShapeTreeView/>` with `<SceneTreePanel Stage="@_model?.Stage" />`
2. Add `@using FoundryMicroCore.Blazor.Controls.Components.TreeView` if needed
3. Update StatusMessage binding to use `_model?.StatusMessage`
4. **Verify:** Page compiles, loads, shows canvas + controls + tree panel

### Step 4: Verify Shape Operations
1. Load page — axis should appear
2. Click "Cube (2×2×2)" — green box should appear
3. Click "Show Vertices" — blue marker spheres at corners
4. Click "Show Edges" — pipe markers along edges
5. Click "Show Faces" — face boundary markers
6. Click "Show Normals" — red arrow markers
7. **Verify:** All visualization methods work as before

### Step 5: Verify Dirty Flag Tests
1. Create a box (any preset)
2. Click "🧪 Check Dirty Flags" — console should show dirty flag status
3. Click "🔄 Test Dirty Propagation" — console should show before/after
4. Click "🎬 Test Tweener Dirty Flags" — console output
5. Click "🔬 Test Initialization Dirty Flags" — console output
6. Click "⏰ Test Initialization Timing" — console output showing both patterns
7. Click "🔄 Test Object Replacement" — console output
8. **Verify:** All console output matches existing behavior

### Step 6: Verify Scene Operations
1. Click "Clear" — all shapes removed
2. Click "Show Submarine Model" — sub.glb loads with full spatial frame visualization
3. **Verify:** Clear and model loading work

### Step 7: Test Navigation Disposal
1. Navigate away from page
2. Check console for errors
3. Navigate back
4. Verify fresh state
5. **Verify:** No errors, no leaks

---

## Step-by-Step Test Sequence

### Step 1: Page Load

**Action:** Navigate to `/spacialboxtest`

**Expected Results:**
- ✅ Page title "SpacialBox3D Test Page" visible
- ✅ Canvas3D viewport renders with 3D scene
- ✅ 5-meter axis model visible in viewport
- ✅ Green box (2×1.5×1) visible at origin
- ✅ Controls panel visible with all button sections
- ✅ Tree view panel renders (SceneTreePanel)
- ✅ Status shows "Created: 2×1.5×1m"

**Console Output:**
```
✅ SpacialBoxTest: Retrieved stage 'SpacialBoxScene' from Canvas
✅ [URL path for axis model]
✅ Created: 2×1.5×1m
```

**If Failed:**
- No canvas → Check Canvas3DComponent binding (`@ref`)
- No axis → Check `GetReferenceTo` URL resolution
- No box → Check `IsReady` — Arena or Scene may be null

### Step 2: Preset Shape Creation

**Action:** Click "Tall Box (1×4×1)"

**Expected Results:**
- ✅ Green box changes to tall proportions
- ✅ Status shows "Updated: 1×4×1m" (or "Created: 1×4×1m" if fresh)
- ✅ Box visible at origin with pivot at bottom

### Step 3: Show Vertices

**Action:** Click "Show Vertices"

**Expected Results:**
- ✅ 8 blue spheres appear at box corners
- ✅ Status shows "Showing 8 vertices"

### Step 4: Dirty Flag Check

**Action:** Click "🧪 Check Dirty Flags"

**Expected Results:**
- ✅ Console shows dirty flag status with shape and transform values
- ✅ Status message updates with dirty flag info

**Console Output:**
```
🧪 DIRTY FLAG STATUS:
   Shape.IsDirty: [true/false]
   Transform.IsDirty: [true/false]
   Transform.OwnerName: [name]
```

### Step 5: Clear and Rebuild

**Action:** Click "Clear", then "Cube (2×2×2)"

**Expected Results:**
- ✅ All shapes removed on Clear
- ✅ New cube created
- ✅ Tree view updates

---

## Success Criteria

### Compilation
- [ ] Zero compilation errors
- [ ] Zero compilation warnings (or only pre-existing ones)
- [ ] All using statements resolve
- [ ] `SpacialBoxTestModel.cs` compiles independently

### Runtime (First Load)
- [ ] Page loads without exceptions
- [ ] Canvas3D initializes and renders
- [ ] Axis model loads and displays
- [ ] Initial box created and visible
- [ ] Tree view panel renders (no missing component errors)

### Runtime (Functionality)
- [ ] All 5 preset shapes work
- [ ] All visualization methods produce markers
- [ ] Submarine model loads
- [ ] Clear removes all shapes
- [ ] All 6 dirty flag tests produce console output
- [ ] Status message updates correctly

### Architecture
- [ ] `SpacialBoxTestModel.cs` contains all domain logic
- [ ] `SpacialBoxTest.razor.cs` is under 100 lines
- [ ] No domain logic in code-behind (only lifecycle + delegation)
- [ ] `ShapeTreeView` replaced with `SceneTreePanel` or equivalent

### Known Issues (Acceptable)
- [ ] Animation methods (Door Hinge, Corner Balance, Center Spin, Reset) may not visually animate — pre-existing issue (Tweener not pumped)
- [ ] `RadzenShapeTreeView` in sibling SpacialFrameTest is not addressed by this spec

### Disposal
- [ ] No console errors on navigation away
- [ ] Model and scene references nulled
- [ ] No memory leaks

---

## Project Convention Compliance

### Convention: Model-Behind Pattern
**Evidence:** ClockDemo has `ClockDemoModel.cs` as the direction. Code-behind at 573 lines is well above extraction threshold.
**This spec:** ✅ Creates `SpacialBoxTestModel` as a plain class (not MxComponent — this page doesn't need persistent memory, just logic separation)

> **Why not MxComponent?** Unlike ClockDemoModel which participates in the MxCore object graph with `[DiscoverableComponent]` and `[ModelComponent]` attributes, SpacialBoxTestModel is a simple logic container. It doesn't need discovery, persistence, or tree node participation. A plain class is appropriate. If Sully disagrees and wants MxComponent, the model's constructor and method signatures don't change — only the base class and attributes.

### Convention: Injection Pattern
**Evidence:** SpacialFrameTest uses `[Inject] public IFoundryService FoundryService { get; init; }`
**This spec:** ✅ Matches SpacialFrameTest pattern (closest sibling)

### Convention: Namespace
**Evidence:** All pages use `Three2025.Components.Pages`
**This spec:** ✅ Matches

### Convention: File Location
**Evidence:** ClockDemoModel.cs is in `Components/Pages/` alongside its page
**This spec:** ✅ Model placed in `Components/Pages/SpacialBoxTestModel.cs`

---

## Implementer Behavior Warnings

### Things You Will Be Tempted To Do (DON'T)

1. **Make SpacialBoxTestModel inherit MxComponent**
   Why you'll want to: "ClockDemoModel does it, so I should too"
   Why you shouldn't: ClockDemoModel needs MxCore discovery and tree participation. SpacialBoxTestModel is a pure logic extraction — a plain class is simpler and sufficient. Add MxComponent only if Sully requests it.

2. **Fix the Tweener animations while refactoring**
   Why you'll want to: "The animations don't work, I should fix them as part of modernization"
   Why you shouldn't: The scope is Model extraction, not feature fixes. If you fix animations, you risk introducing new bugs in untested territory. Document the Tweener issue but don't fix it.

3. **Create a new tree component to replace ShapeTreeView**
   Why you'll want to: "ShapeTreeView doesn't exist, I need to build something"
   Why you shouldn't: `SceneTreePanel` already exists in FoundryMicroCore.Blazor.Controls. Use it. If it doesn't render correctly, remove the tree panel entirely rather than building a new component.

4. **Move Canvas3DComponent reference into the model**
   Why you'll want to: "The model should own everything"
   Why you shouldn't: `Canvas3DComponent` is a Blazor component reference (`@ref`). It can only be bound in the razor/code-behind. The model receives the `Stage` (extracted from canvas) — that's the right boundary.

5. **Rename methods or reorganize the button sections**
   Why you'll want to: "Better naming would improve clarity"
   Why you shouldn't: The razor markup binds to method names. Every rename requires updating both `.razor` and `.razor.cs`. Keep existing names for a clean diff.

---

## Silent Failure Audit

### ShapeTreeView Reference
- **What happens:** Blazor either throws a compile error (component not found) or silently renders nothing
- **How Indy would know:** Compile error if lucky; empty panel if unlucky
- **Mitigation:** Replace with SceneTreePanel — verified to exist

### Tweener with No Update Pump
- **What happens:** `new Tweener()` configures tween targets, but since `Update()` is never called, the tween values never change. The shape stays at its initial position/rotation.
- **How Indy would know:** Click "Door Hinge" — nothing moves. No error, no warning.
- **Mitigation:** Document as known limitation. Not in scope to fix.

### FoShape3D Name Validation
- **What happens:** `ValidateIdentifier` silently rejects names with hyphens/special chars
- **How Indy would know:** Shape gets auto-named instead of specified name
- **Mitigation:** Existing code uses "SpacialBoxMain" (valid). Preserve this.

---

## Console Output Verification

**Expected clean operation (30 seconds after load):**
```
✅ SpacialBoxTest: Retrieved stage 'SpacialBoxScene' from Canvas
✅ [axis model URL]
✅ Created: 2×1.5×1m
```

**After 30 seconds of idle:**
- No error spam
- No Euler overflow warnings
- No repeated log messages

**Note:** The existing code has `WriteSuccess()` and `WriteInfo()` calls throughout. These should produce console output on button clicks but NOT continuously.

---

## Build Journal Requirement

Maintain `BUILD_JOURNAL_SPACIALBOXTEST.md` in the project root. Log as you go:
- Phase start/end times
- Decisions that differed from spec (and why)
- Surprises (APIs that didn't work as described)
- Console output observations
- Where the spec helped vs. where it misled

Include After-Action Questions at the end for Sage:
1. Was the Model extraction boundary correct? (What stayed in code-behind that shouldn't have, or vice versa?)
2. Did SceneTreePanel work as a drop-in replacement for ShapeTreeView?
3. Were the Tweener animations actually broken as predicted, or did they work?
4. How many lines is the final code-behind? Target was under 100.
5. Any new code smells discovered during refactoring?

---

## Confidence Levels

| Area | Confidence | Rationale |
|------|-----------|-----------|
| **Model extraction pattern** | 🟢 High | Follows ClockDemoModel precedent, well-defined boundary |
| **Shape creation / presets** | 🟢 High | Direct copy from working code, verified APIs |
| **Visualization delegation** | 🟢 High | IGeometryVisualizationService interface verified |
| **Dirty flag tests** | 🟢 High | Direct move — no logic changes, just location |
| **SceneTreePanel replacement** | 🟡 Medium | Component verified, but may need @using import; stage binding untested for this specific case |
| **Tweener animations** | 🔴 Low | Likely broken in original code (no Update pump). Moving to model doesn't fix this. |
| **URL resolution (GetReferenceTo)** | 🟢 High | Existing pattern works — preserved unchanged |
| **Disposal** | 🟢 High | Simple null-out — matching existing pattern |

---

*This specification follows the Atlas Specification Checklist v1.2 (February 10, 2026).*
