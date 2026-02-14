# Multi-Canvas 3D Test — Component Specification

## Specification Handoff Summary

**Architect:** Claude "Atlas"  
**Date:** February 9, 2026  
**Estimated Implementation Time:** 2–3 hours  
**Confidence:** 🟢 High — extracted from working reference implementation  

**Primary Reference:** `Components/Pages/MultiCanvas3DTest.razor.cs` — This is the working code. The spec below codifies what it does and how.

**High Uncertainty Areas:**
1. Animation timing — `1.0 / Math.Max(fps, 1)` may produce jerky motion if fps fluctuates. Frame-based increments (Golden Pattern) are more reliable.
2. Scene activation ordering — Three canvases initialize sequentially; all must be active simultaneously.

**Known Limitations:**
1. Tree view only refreshes on button click (no live subscription to shape changes)
2. No dispose cleanup of individual shapes — relies on `ClearAll()` batch

**Verification Checklist:**
- [ ] Compiles without errors
- [ ] Page loads three separate 3D canvases
- [ ] Scene A shows a rotating red cube
- [ ] Scene B shows three bouncing colored spheres
- [ ] Scene C shows four assorted rotating shapes
- [ ] Tree view shows Arena → 3 stages with shapes
- [ ] Clear All removes all shapes from all scenes
- [ ] No console errors on navigation away

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about building a Multi-Canvas 3D Test — it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 3D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you build the Multi-Canvas 3D Test, you're testing whether AI can:
1. Successfully manage **multiple independent 3D rendering contexts** on a single page
2. Route shapes, animations, and render calls to the **correct scene/stage/canvas** without cross-contamination
3. Demonstrate that the Arena → Stage → Scene → JavaScript Viewer pipeline supports **true multi-viewport 3D**

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving the platform can host multiple simultaneous 3D worlds — a fundamental capability for dashboard-style applications where users see several viewports at once.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## Architecture Analysis

**Based on:** `Components/Pages/MultiCanvas3DTest.razor.cs`, `Canvas3DComponent.razor.cs`

**Current Pattern:** Page-level ComponentBase with per-canvas `Canvas3DComponent` children. No separate Tech class — setup logic lives directly in the page code-behind.

**Key Characteristics:**
- **Base class:** `ComponentBase` (standard Blazor — not MxComponent)
- **Stage management:** Each `Canvas3DComponent` auto-creates its own `FoStage3D` via its `SceneName` parameter
- **Shape lifecycle:** Shapes created in page code-behind via `new FoShape3D(...).CreateXxx(...)`, then added to stage via `stage.AddShape(shape)`
- **Animation hookup:** Each shape registers its own `OnBeforeRender` callback. The `Canvas3DComponent` handles `AnimationFrameBus` subscription internally.
- **Multi-canvas key insight:** Each `Canvas3DComponent` creates its own `ViewerThreeD`, which generates a unique `ContainerId` (GUID-based), creates its own Three.js Scene/Camera/Renderer in JavaScript, and renders independently.

**Files to study as reference:**
1. `Components/Pages/MultiCanvas3DTest.razor` — Grid layout with three canvases + tree view
2. `Components/Pages/MultiCanvas3DTest.razor.cs` — Shape setup per scene, button handlers, disposal
3. `FoundryWorldsAndDrawings/Shared/Canvas3DComponent.razor.cs` — Stage/Scene creation, activation, render loop
4. `FoundryWorldsAndDrawings/ThreeD/Viewers/ViewerThreeD.cs` — ContainerId generation, JS interop

---

## Verified Against

- **API Reference:** `FoundryWorldsAndDrawings/FOUNDRY_3D_API_REFERENCE.md`
- **Last verified:** February 9, 2026
- **Source code audited:** FoShape3D.cs, FoStage3D.cs, FoArena3D.cs, Canvas3DComponent.razor.cs

**Method Verification:**
- ✅ `new FoShape3D(name, color)` — constructor verified in FoShape3D.cs
- ✅ `.CreateBox(name, w, h, d)` — fluent factory, returns `this`, sets GeomType="box"
- ✅ `.CreateSphere(name, w, h, d)` — fluent factory, returns `this`
- ✅ `.CreateCylinder(name, w, h, d)` — fluent factory, returns `this`
- ✅ `.CreateCone(name, w, h, d)` — fluent factory, returns `this`
- ✅ `.CreateTorus(name, w, h, d)` — fluent factory, returns `this`
- ✅ `.CreateDodecahedron(name, w, h, d)` — fluent factory, returns `this`
- ✅ `.OnBeforeRender(Action<FoGlyph3D, int, double>)` — inherited from FoGlyph3D
- ✅ `shape.Transform.Position` — lazy-created Transform3 with Vector3
- ✅ `shape.Transform.Rotation` — Euler(x, y, z)
- ✅ `stage.AddShape(shape)` — handles categorization into Bodies/Links, wires parent
- ✅ `stage.ClearAll()` — async, bulk deletes + sends to JS
- ✅ `stage.AllBodies()` — returns IEnumerable<FoGlyph3D>
- ✅ `stage.AllLinks()` — returns IEnumerable<FoGlyph3D>
- ✅ `Canvas3DComponent.Stage` — read-only property, returns ManagedStage
- ✅ `arena.GetAllStages()` — IReadOnlyList<FoStage3D>

---

## Reference Implementation Strategy

### Primary Reference
**Copy:** `Components/Pages/MultiCanvas3DTest.razor.cs`  
**Demonstrates:** Multi-canvas page with per-scene shape setup, animation callbacks, tree view integration

### The Implementation IS the Reference
This specification is extracted from a **working implementation**. The code below is not pseudo-code — it is the actual verified code.

### Delta from Single-Canvas Pattern
What's different from a typical single-canvas page:
- **Three `Canvas3DComponent` instances** instead of one — each with unique `SceneName`
- **No shared stage** — each canvas creates its own stage automatically
- **Access via `_canvasX.Stage`** — parent page reaches into each canvas to get its stage
- **Independent animations** — each shape's `OnBeforeRender` runs in its own stage's render pass
- **Scene activation must be non-exclusive** — `ForceActive()` instead of `SetActiveStage()` (critical fix applied Feb 9, 2026)

---

## Infrastructure Assumptions

### Assumption: Each Canvas3DComponent Creates Its Own Stage
- [x] `Canvas3DComponent` with `SceneName="SceneA"` creates `FoStage3D` named "SceneA"
- [x] Stage is accessible via `_canvasA.Stage` after `OnAfterRenderAsync(firstRender)` completes
- [x] If broken: Check `Canvas3DComponent.razor.cs` lines 92-105

### Assumption: Multiple Scenes Can Be Active Simultaneously
- [x] `scene.ForceActive()` sets `_isActive = true` without deactivating siblings
- [x] `RenderArena` iterates ALL active stages and renders each one
- [x] If broken: Check `StageManagementService.SetActiveStage()` is NOT being called — it enforces exclusive activation

### Assumption: Shapes Route to Correct JavaScript Viewer
- [x] Each `Scene3D` has a unique `ContainerId` (GUID-based)
- [x] Batched updates key operations by `ContainerId`
- [x] JavaScript `ViewManager.ViewerLookup[containerId]` routes to correct `Viewer3D`
- [x] If broken: Check `ViewerThreeD.ContainerId` and `Scene3D.ProcessCollectedChanges()`

### Assumption: Animation Callbacks Fire Per-Stage
- [x] `RenderStage` calls `UpdateForAnimation` on Bodies then Links
- [x] Each shape's `OnBeforeRender` fires during its owning stage's render pass
- [x] If broken: Check `FoStage3D.RenderStage()` — the two-pass architecture

---

## Code Path Traces

### When `Canvas3DComponent` initializes with `SceneName="SceneA"`
1. Blazor renders `<ViewerThreeD SceneName="SceneA" .../>` 
2. `ViewerThreeD` generates `ContainerId = "viewer3d-{8-char-guid}"` (non-global mode)
3. `ViewerThreeD.OnAfterRenderAsync` creates `Scene3D` with that `ContainerId`
4. `ViewerThreeD` calls `JsRuntime.InvokeVoidAsync("FoundryWorldsAndDrawings.Initialize3DViewer", json)` — JSON contains the `ContainerId`
5. JS creates `new Viewer3D()` → `new THREE.Scene()`, `new THREE.WebGLRenderer()`, attaches to `document.getElementById(containerId)`
6. JS registers viewer in `ViewManager.ViewerLookup[containerId]`
7. Back in C#, `Canvas3DComponent.OnAfterRenderAsync` gets the `Scene3D`, creates `FoStage3D` via `arena.EstablishStage<FoStage3D>("SceneA")`
8. Bidirectional link: `scene.LinkToStage(stage)` — stage and scene now reference each other
9. `scene.ForceActive()` marks this scene as active (non-exclusive)
10. Animation subscription starts

### When `stage.AddShape(cube)` is called
1. `FoStage3D.AddShape<T>(cube)` handles move semantics (removes from old stage if needed)
2. Cube is categorized as `IBody3D` → added to `Bodies` list
3. Cube is stored in typed collection via `MxComponentEditor`
4. Parent reference set: `cube.Parent = stage`
5. On next animation frame, `RenderStage` calls `UpdateForAnimation` → fires cube's `OnBeforeRender`
6. Cube's transform changes get collected by `Shape3DChangeCollector`
7. Collector sends batch to `Scene3D.ProcessCollectedChanges()` keyed by `ContainerId`
8. JS receives batch → `ViewManager.ViewerLookup[containerId].processBatchOperations(ops)`
9. Three.js creates/updates mesh in the correct scene only

### When animation renders all three canvases
1. `AnimationFrameBus` fires animation event
2. **Each** `Canvas3DComponent` receives the event (all three subscribed)
3. Each calls `arena.RenderArena(tick, fps)`
4. `RenderArena` gets ALL stages, filters to `IsActive == true`
5. With fix: all 3 stages are active → all 3 get `RenderStage(tick, fps)` called
6. Each stage processes its own shapes → sends updates to its own Scene3D → routed to its own JS Viewer

**CRITICAL:** `RenderArena` is called by each canvas component, so it runs 3× per frame. The stage render is idempotent per tick (shapes clear stale flags after export), so duplicate calls are harmless but wasteful. This is a known design point, not a bug.

---

## 🏆 GOLDEN PATTERN — Copy This Exactly

**Source:** `MultiCanvas3DTest.razor.cs` Scene A setup (working code)

```csharp
// Create shape with fluent factory
var cube = new FoShape3D("RotatingCube", "red")
    .CreateBox("RotatingCube", 2, 2, 2);

// Register per-frame animation callback
cube.OnBeforeRender((shape, tick, fps) =>
{
    var dt = 1.0 / Math.Max(fps, 1);
    var rot = shape.Transform.Rotation;
    var newY = rot.Y + dt * 1.0;        // 1 rad/s
    shape.Transform.Rotation = new Euler(rot.X, newY, rot.Z);
});

// Add to stage — this is what routes the shape to the correct canvas
stage.AddShape(cube);
```

**CRITICAL RULES:**
- ❌ Do NOT add `if (fps <= 0) return` guards — fps may be 0 on first frame, silently kills callback
- ❌ Do NOT add `if (tick == 0) return` guards — tick is global, already at 300+ when shapes are added
- ❌ Do NOT call `SetTransformStale()` manually — setting `Transform.Rotation` or `Transform.Position` triggers it automatically
- ❌ Do NOT call `SetRecomputeBoundary()` unless you need Wave 2 world positions
- ❌ Do NOT use hyphens in shape names — `ValidateIdentifier` silently rejects them
- ✅ Use `Math.Max(fps, 1)` to avoid division by zero
- ✅ Set transform properties directly. The stale system handles the rest.
- ✅ Use `stage.AddShape(shape)` to route shapes to the correct canvas

---

## Implementer Behavior Warnings

### Things You Will Be Tempted To Do (DON'T)

1. **Add null/guard checks to OnBeforeRender callbacks**  
   Why you'll want to: "What if fps is 0? What if the shape isn't ready?"  
   Why you shouldn't: Use `Math.Max(fps, 1)` and trust the pipeline.

2. **Use `SetActiveStage()` instead of `ForceActive()`**  
   Why you'll want to: "SetActiveStage sounds like the right API"  
   Why you shouldn't: It enforces **exclusive** activation — only the last canvas initialized will render. Use `scene.ForceActive()` for multi-canvas.

3. **Add shapes before canvas is ready**  
   Why you'll want to: "Set things up in OnInitializedAsync"  
   Why you shouldn't: `_canvasA.Stage` is null until `OnAfterRenderAsync(firstRender)` completes. Use `Task.Delay(500)` or check for null.

4. **Create a shared stage for all canvases**  
   Why you'll want to: "One stage with all shapes is simpler"  
   Why you shouldn't: Each canvas needs its own stage. The `ContainerId` routing sends ALL shapes in a stage to ONE JavaScript viewer.

5. **Use hyphens or special characters in shape names**  
   Why you'll want to: "Box-1 is readable"  
   Why you shouldn't: `ValidateIdentifier` silently rejects them. Use underscores: `Box_1`.

---

## Component Structure

```
MultiCanvas3DTest.razor          # Grid layout with 3 canvases + tree view
MultiCanvas3DTest.razor.cs       # Shape setup, button handlers, disposal
```

No separate Tech class — this is a straightforward page component.

---

## UI Specification

### Layout
- 2×2 CSS Grid (`grid-template-columns: 1fr 1fr`)
- Top-left: **Scene A** (green border) — Rotating Cube
- Top-right: **Scene B** (blue border) — Color Spheres  
- Bottom-left: **Scene C** (orange border) — Mixed Shapes
- Bottom-right: **Tree View** (purple border) — Arena/Stage/Scene hierarchy

### Controls
- **"Add Shapes to All Scenes"** button — calls Setup methods (idempotent via `_shapesAdded` flag)
- **"Clear All"** button — calls `ClearAll()` on each stage
- **"Refresh Tree"** button — updates counters and re-renders tree
- **Status line** — shows scene count and total shape count

### Canvas Dimensions
Each `Canvas3DComponent` renders at 600×400 pixels. Grid cells flex to fill available space.

---

## Scene Specifications

### Scene A — Rotating Cube
- 1 red box, 2×2×2 units
- Rotates around Y-axis at 1 radian/second
- Demonstrates: basic shape + continuous rotation animation

### Scene B — Color Spheres  
- 3 spheres: red, green, blue
- Positioned at x = -3, 0, +3
- Each bounces vertically using `sin(t * 2.0 + phaseOffset)` × 2.0 amplitude
- Phase offsets: 0, 2π/3, 4π/3 (120° apart)
- Demonstrates: multiple shapes per scene + parametric animation

### Scene C — Mixed Shapes
- 4 shapes: Cylinder (orange, x=-4), Cone (purple, x=0), Torus (cyan, x=4, y=1), Dodecahedron (gold, x=8)
- Each rotates on different axes at different speeds
- Demonstrates: variety of geometry types + multi-axis rotation

---

## Implementation Steps

### Step 1: Create Component Files
1. Create `MultiCanvas3DTest.razor` with `@page "/multi-canvas-3d-test"`
2. Create `MultiCanvas3DTest.razor.cs` with `ComponentBase, IDisposable`
3. Add `@using FoundryWorldsAndDrawings.Shared` and `@using FoundryMicroCore.Blazor.Controls.Components.TreeView`
4. **Verify:** Files compile with no errors

### Step 2: Layout with Three Canvases
1. Build 2×2 grid layout in Razor
2. Place `<Canvas3DComponent SceneName="SceneA" @ref="_canvasA" CanvasWidth=600 CanvasHeight=400 />` in each cell
3. Place `<UnifiedTreeView>` in bottom-right cell
4. **Verify:** Page loads, three gray canvases appear with grid lines

### Step 3: Setup Scene A (Rotating Cube)
1. In `OnAfterRenderAsync(firstRender)`, after `Task.Delay(500)`, access `_canvasA.Stage`
2. Create cube using Golden Pattern above
3. **Verify:** Red cube visible in top-left canvas, rotating

### Step 4: Setup Scene B (Color Spheres)
1. Access `_canvasB.Stage`
2. Create 3 spheres with loop: colors, x-offsets, phase offsets
3. Use `sin()` bounce in `OnBeforeRender`
4. **Verify:** Three spheres bouncing in top-right canvas, NONE in Scene A

### Step 5: Setup Scene C (Mixed Shapes)
1. Access `_canvasC.Stage`
2. Create 4 shapes: Cylinder, Cone, Torus, Dodecahedron
3. Each with distinct position and rotation animation
4. **Verify:** Four shapes rotating in bottom-left canvas, NONE in other scenes

### Step 6: Wire Up Tree View and Controls
1. Get `arena as ITreeNode` for `UnifiedTreeView`
2. Implement `DoAddShapes`, `DoClearAll`, `DoRefreshTree`
3. `UpdateCounts()` counts stages and shapes
4. **Verify:** Tree shows FoArena3D → 3 FoStage3D children. Counters update.

### Step 7: Disposal
1. Implement `IDisposable.Dispose()` — log message  
2. Canvas3DComponent handles its own cleanup internally
3. **Verify:** No console errors on navigation away

---

## Step-by-Step Test Sequence

### Step 1: Page Load

**Action:** Navigate to `/multi-canvas-3d-test`

**Expected Results:**
- ✅ Three 3D canvases appear in a 2×2 grid
- ✅ Scene A (top-left, green border): Red cube rotating around Y-axis
- ✅ Scene B (top-right, blue border): Three spheres (red/green/blue) bouncing
- ✅ Scene C (bottom-left, orange border): Four shapes (cylinder/cone/torus/dodecahedron) rotating
- ✅ Tree view (bottom-right, purple border): Shows FoArena3D with 3 child stages
- ✅ Status line: "Scenes: 3 | Shapes: 8"

**Console Output:**
```
MultiCanvas3D: Scene A — added RotatingCube to stage 'SceneA'
MultiCanvas3D: Scene B — added 3 spheres to stage 'SceneB'
MultiCanvas3D: Scene C — added 4 shapes to stage 'SceneC'
```

**If Failed:**
- Only one canvas has shapes → Check scene activation (must use `ForceActive()` not `SetActiveStage()`)
- Shapes in wrong canvas → Check you're accessing the correct `_canvasX.Stage`
- Stage is null → `Task.Delay(500)` may not be enough; increase or poll

### Step 2: Verify Scene Independence

**Action:** Observe all three canvases for 10 seconds

**Expected Results:**
- ✅ Scene A cube rotates smoothly, stays in Scene A only
- ✅ Scene B spheres bounce with 120° phase separation
- ✅ Scene C shapes each rotate on their own axes
- ✅ No shape "leaks" into another canvas
- ✅ All animations are smooth (no stutter)

**If Failed:**
- Shapes appear in wrong canvas → `ContainerId` routing issue; check `Scene3D.ContainerId` matches `ViewerThreeD.ContainerId`
- Jerky animation → fps may be fluctuating; consider frame-based increments instead of `dt`

### Step 3: Clear All

**Action:** Click "Clear All" button

**Expected Results:**
- ✅ All shapes disappear from all three canvases
- ✅ Canvases show empty scenes (gray background with grid)
- ✅ Status line: "Scenes: 3 | Shapes: 0"
- ✅ Tree view stages still exist but have no children

**Console Output:**
```
MultiCanvas3D: All stages cleared
```

### Step 4: Re-add Shapes

**Action:** Click "Add Shapes to All Scenes"

**Expected Results:**
- ✅ All shapes reappear in their correct canvases
- ✅ Animations resume
- ✅ Status line: "Scenes: 3 | Shapes: 8"

### Step 5: Refresh Tree

**Action:** Click "🔄 Refresh Tree" button

**Expected Results:**
- ✅ Counters update
- ✅ Tree view refreshes

---

## Code Smells to Avoid

### From MicroCore (CODE_SMELLS_ANALYSIS.md)

#### Shape Name Validation
**Don't:**
```csharp
var box = new FoShape3D($"Box1-{guid}", "blue");  // Hyphen silently rejected
```
**Do:**
```csharp
var box = new FoShape3D($"Box1_{counter}", "blue");  // Underscores are safe
```

#### Exclusive Scene Activation
**Don't:**
```csharp
arena.Stages().SetActiveStage(stage);  // Kills all other scenes
```
**Do:**
```csharp
scene.ForceActive();  // Activates without deactivating siblings
```

### Task-Specific Warnings

#### Accessing Stage Before Canvas Initializes
**Problem:** `_canvasA.Stage` is null in `OnInitializedAsync` because the canvas hasn't rendered yet
**Solution:** Access stages in `OnAfterRenderAsync(firstRender)` after a delay:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (!firstRender) return;
    await Task.Delay(500);  // Let canvases initialize
    var stage = _canvasA?.Stage;  // Now available
}
```

---

## Known Gotchas

### RenderArena Called Multiple Times Per Frame
Each `Canvas3DComponent` subscribes to `AnimationFrameBus` and calls `arena.RenderArena()`. With 3 canvases, this runs 3× per frame. Stages clear stale flags after export, so duplicates are no-ops. This is wasteful but harmless.

### Task.Delay for Initialization Timing
The `Task.Delay(500)` in `OnAfterRenderAsync` is a pragmatic wait for all three `Canvas3DComponent` instances to complete their JS interop initialization. If the delay is too short on slow machines, stages may be null. A more robust approach would poll `_canvasA?.Stage != null`.

### Tree View is Static
`UnifiedTreeView` renders once. Changes to shapes don't auto-refresh the tree. User must click "🔄 Refresh Tree" to see updates.

---

## Troubleshooting Guide

### Shapes Only Appear in One Canvas

**Symptom:** All shapes visible in one canvas (usually the last one), other canvases empty

**Diagnosis Steps:**
1. Check console for `"SetActiveStage"` — if present, exclusive activation is killing other scenes
2. Verify `ForceActive()` is used instead of `SetActiveStage()`
3. Check `RenderArena` log: `"Active stages=X"` — should be 3, not 1

**Common Causes:**
- `SetActiveStage()` called instead of `ForceActive()`
- A code path in `Scene3D.SetActive()` still routes through `SetActiveStage()`

### Stage is Null When Adding Shapes

**Symptom:** "Stage A not ready" warning in console

**Diagnosis:**
1. Check timing — are you accessing `_canvasA.Stage` before `OnAfterRenderAsync`?
2. Increase `Task.Delay` or add null check + retry

### Shapes in Wrong Canvas

**Symptom:** Red cube appears in Scene C instead of Scene A

**Diagnosis:**
1. Check you're using `_canvasA.Stage` (not `_canvasC.Stage`) for Scene A shapes
2. Check `ContainerId` routing — each scene must have unique ID
3. Add logging: `$"Adding to stage '{stage.Name}' ContainerId='{scene.ContainerId}'"` 

---

## Success Criteria

### Compilation
- [ ] Zero compilation errors
- [ ] Zero compilation warnings (excluding NuGet pruning warnings)

### Runtime (First Load)
- [ ] Page loads three 3D canvases in a grid
- [ ] All three canvases render simultaneously
- [ ] Each canvas has its own independent scene

### Runtime (Functionality)
- [ ] Scene A: Red cube rotates around Y-axis
- [ ] Scene B: Three colored spheres bounce with phase separation
- [ ] Scene C: Four shapes rotate on different axes
- [ ] No cross-canvas shape contamination
- [ ] Tree view shows Arena → 3 stages
- [ ] Clear All removes all shapes
- [ ] Add Shapes restores all shapes

### Disposal
- [ ] No console errors on navigation away
- [ ] No memory leaks (shapes cleaned up by Canvas3DComponent internally)

---

## Confidence Levels

- **Multi-canvas layout:** 🟢 High — Standard Blazor grid + CSS
- **Canvas3DComponent usage:** 🟢 High — Verified from working code
- **Shape creation (Golden Pattern):** 🟢 High — Extracted from working MultiCanvas3DTest
- **Scene activation (ForceActive):** 🟢 High — Fix verified and tested Feb 9, 2026
- **Animation callbacks:** 🟢 High — Same pattern as all working 3D demos
- **ContainerId routing:** 🟢 High — Traced through JS ViewManager
- **Tree view integration:** 🟡 Medium — `UnifiedTreeView` with `ITreeNode` is straightforward but static
- **Initialization timing (Task.Delay):** 🟡 Medium — 500ms works on dev machine, may need tuning

---

## Appendix A: Complete Working Implementation (COPY THIS)

Indy's #1 feedback from Tug of War: "What I needed was the whole file." Here it is.

### File 1: `MultiCanvas3DTest.razor`

```razor
@page "/multi-canvas-3d-test"
@using FoundryWorldsAndDrawings.Shared
@using FoundryMicroCore.Blazor.Controls.Components.TreeView
@rendermode InteractiveServer

<PageTitle>Multi-Canvas 3D Test</PageTitle>

<h2>Multi-Canvas 3D Test — Three Independent Scenes</h2>

<div class="d-flex gap-2 mb-2">
    <button class="btn btn-sm btn-primary" @onclick="DoAddShapes">Add Shapes to All Scenes</button>
    <button class="btn btn-sm btn-danger" @onclick="DoClearAll">Clear All</button>
    <button class="btn btn-sm btn-outline-secondary" @onclick="DoRefreshTree">🔄 Refresh Tree</button>
    <span class="text-muted align-self-center" style="font-size: 0.85rem;">
        Scenes: @_sceneCount &nbsp; | &nbsp; Shapes: @_shapeCount
    </span>
</div>

<div style="display: grid; grid-template-columns: 1fr 1fr; grid-template-rows: 1fr 1fr; height: calc(100vh - 120px); gap: 10px; padding: 0 10px 10px 10px;">
    
    <!-- Top-Left: Scene A -->
    <div style="border: 2px solid #4CAF50; border-radius: 8px; overflow: hidden; display: flex; flex-direction: column;">
        <div style="background: #4CAF50; color: white; padding: 8px; font-weight: bold;">
            Scene A - Rotating Cube
        </div>
        <div style="flex: 1; position: relative;">
            <Canvas3DComponent SceneName="SceneA" @ref="_canvasA" CanvasWidth=600 CanvasHeight=400 />
        </div>
    </div>

    <!-- Top-Right: Scene B -->
    <div style="border: 2px solid #2196F3; border-radius: 8px; overflow: hidden; display: flex; flex-direction: column;">
        <div style="background: #2196F3; color: white; padding: 8px; font-weight: bold;">
            Scene B - Color Spheres
        </div>
        <div style="flex: 1; position: relative;">
            <Canvas3DComponent SceneName="SceneB" @ref="_canvasB" CanvasWidth=600 CanvasHeight=400 />
        </div>
    </div>

    <!-- Bottom-Left: Scene C -->
    <div style="border: 2px solid #FF9800; border-radius: 8px; overflow: hidden; display: flex; flex-direction: column;">
        <div style="background: #FF9800; color: white; padding: 8px; font-weight: bold;">
            Scene C - Mixed Shapes
        </div>
        <div style="flex: 1; position: relative;">
            <Canvas3DComponent SceneName="SceneC" @ref="_canvasC" CanvasWidth=600 CanvasHeight=400 />
        </div>
    </div>

    <!-- Bottom-Right: Tree View -->
    <div style="border: 2px solid #9C27B0; border-radius: 8px; overflow: hidden; display: flex; flex-direction: column;">
        <div style="background: #9C27B0; color: white; padding: 8px; font-weight: bold;">
            Arena / Stage / Scene Hierarchy
        </div>
        <div style="flex: 1; overflow: auto; background: #f5f5f5; padding: 10px;">
            @if (_arenaNode != null)
            {
                <UnifiedTreeView RootNode="@_arenaNode" ShowTypeNames="true" ShowBadges="true" />
            }
            else
            {
                <p class="text-muted">Waiting for arena…</p>
            }
        </div>
    </div>

</div>
```

### File 2: `MultiCanvas3DTest.razor.cs`

```csharp
using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Components.Pages;

/// <summary>
/// Demonstrates three independent Canvas3DComponents on a single page,
/// each with its own scene/stage, plus a unified tree view of the arena hierarchy.
/// </summary>
public partial class MultiCanvas3DTest : ComponentBase, IDisposable
{
    [Inject] public required IWorkspace Workspace { get; set; }

    // Canvas references — each creates its own Stage/Scene pair via SceneName
    private Canvas3DComponent _canvasA = null!;
    private Canvas3DComponent _canvasB = null!;
    private Canvas3DComponent _canvasC = null!;

    // Arena tree root for the UnifiedTreeView
    private ITreeNode? _arenaNode;

    // UI counters
    private int _sceneCount;
    private int _shapeCount;
    private bool _shapesAdded;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        // Give all three Canvas3DComponents time to initialise their scenes/stages
        await Task.Delay(500);

        var arena = Workspace.GetArena();
        _arenaNode = arena as ITreeNode;

        // Populate each scene with starter shapes
        SetupSceneA();
        SetupSceneB();
        SetupSceneC();
        _shapesAdded = true;

        UpdateCounts();
        await InvokeAsync(StateHasChanged);
    }

    // ── Scene A — a single rotating red cube ──────────────────────────────
    private void SetupSceneA()
    {
        var stage = _canvasA?.Stage;
        if (stage == null) { "MultiCanvas3D: Stage A not ready".WriteWarning(); return; }

        var cube = new FoShape3D("RotatingCube", "red")
            .CreateBox("RotatingCube", 2, 2, 2);

        cube.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            var newY = rot.Y + dt * 1.0;        // 1 rad/s
            shape.Transform.Rotation = new Euler(rot.X, newY, rot.Z);
        });

        stage.AddShape(cube);
        $"MultiCanvas3D: Scene A — added RotatingCube to stage '{stage.Name}'".WriteSuccess();
    }

    // ── Scene B — three bouncing spheres ──────────────────────────────────
    private void SetupSceneB()
    {
        var stage = _canvasB?.Stage;
        if (stage == null) { "MultiCanvas3D: Stage B not ready".WriteWarning(); return; }

        var colors = new[] { "red", "green", "blue" };
        var offsets = new[] { -3.0, 0.0, 3.0 };
        var phaseOffset = new[] { 0.0, Math.PI * 2.0 / 3.0, Math.PI * 4.0 / 3.0 };

        for (int i = 0; i < 3; i++)
        {
            var idx = i;                                       // capture for closure
            var name = $"Sphere{colors[idx]}";
            var sphere = new FoShape3D(name, colors[idx])
                .CreateSphere(name, 1, 1, 1);
            sphere.Transform.Position = new Vector3(offsets[idx], 0, 0);

            sphere.OnBeforeRender((shape, tick, fps) =>
            {
                var t = tick / Math.Max(fps, 1);              // elapsed seconds
                var y = Math.Sin(t * 2.0 + phaseOffset[idx]) * 2.0;
                var pos = shape.Transform.Position;
                shape.Transform.Position = new Vector3(pos.X, y, pos.Z);
            });

            stage.AddShape(sphere);
        }

        $"MultiCanvas3D: Scene B — added 3 spheres to stage '{stage.Name}'".WriteSuccess();
    }

    // ── Scene C — assorted shapes (cylinder, cone, torus, dodecahedron) ───
    private void SetupSceneC()
    {
        var stage = _canvasC?.Stage;
        if (stage == null) { "MultiCanvas3D: Stage C not ready".WriteWarning(); return; }

        var cylinder = new FoShape3D("Cylinder", "orange")
            .CreateCylinder("Cylinder", 1, 3, 1);
        cylinder.Transform.Position = new Vector3(-4, 0, 0);
        cylinder.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X, rot.Y + dt * 0.5, rot.Z);
        });
        stage.AddShape(cylinder);

        var cone = new FoShape3D("Cone", "purple")
            .CreateCone("Cone", 1.5, 3, 1.5);
        cone.Transform.Position = new Vector3(0, 0, 0);
        cone.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X, rot.Y - dt * 0.7, rot.Z);
        });
        stage.AddShape(cone);

        var torus = new FoShape3D("Torus", "cyan")
            .CreateTorus("Torus", 1.2, 0.4, 1.2);
        torus.Transform.Position = new Vector3(4, 1, 0);
        torus.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X + dt * 0.8, rot.Y + dt * 0.3, rot.Z);
        });
        stage.AddShape(torus);

        var dodeca = new FoShape3D("Dodecahedron", "gold")
            .CreateDodecahedron("Dodecahedron", 1.5, 1.5, 1.5);
        dodeca.Transform.Position = new Vector3(8, 0, 0);
        dodeca.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X + dt * 0.4, rot.Y + dt * 0.6, rot.Z + dt * 0.2);
        });
        stage.AddShape(dodeca);

        $"MultiCanvas3D: Scene C — added 4 shapes to stage '{stage.Name}'".WriteSuccess();
    }

    // ── Button handlers ───────────────────────────────────────────────────
    private void DoAddShapes()
    {
        if (_shapesAdded) return;
        SetupSceneA();
        SetupSceneB();
        SetupSceneC();
        _shapesAdded = true;
        UpdateCounts();
    }

    private async Task DoClearAll()
    {
        var stageA = _canvasA?.Stage;
        var stageB = _canvasB?.Stage;
        var stageC = _canvasC?.Stage;

        if (stageA != null) await stageA.ClearAll();
        if (stageB != null) await stageB.ClearAll();
        if (stageC != null) await stageC.ClearAll();

        _shapesAdded = false;
        UpdateCounts();
        $"MultiCanvas3D: All stages cleared".WriteInfo();
    }

    private void DoRefreshTree()
    {
        UpdateCounts();
        StateHasChanged();
    }

    private void UpdateCounts()
    {
        var arena = Workspace.GetArena();
        if (arena == null) return;
        _sceneCount = arena.GetAllStages().Count;
        _shapeCount = arena.GetAllStages().Sum(s => s.AllBodies().Count() + s.AllLinks().Count());
    }

    public void Dispose()
    {
        "MultiCanvas3DTest: Disposing".WriteInfo();
    }
}
```

---

*Specification extracted from working implementation by Atlas, February 9, 2026*
