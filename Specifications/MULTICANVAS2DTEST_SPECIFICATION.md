# MultiCanvas2DTest — Atlas Specification

**Architect:** Claude "Atlas"  
**Date:** February 10, 2026  
**Estimated Implementation Time:** 2–3 hours  
**Confidence:** 🟢 High — 2D patterns are well-established and the legacy code clearly shows intent  

**Primary Reference:** `Components/Pages/ClockDemo.razor.cs` + `ClockDemoModel.cs` — Copy this Model-behind pattern  
**Legacy Reference (intent only):** `Components/Pages/MultiCanvas2DTest.razor.cs` — Shows WHAT, not HOW  

**High Uncertainty Areas:**
1. Canvas2DComponent `PageName` parameter — the legacy code uses it but source audit shows it may not exist. Use `SceneName` only. If canvases don't bind to the right pages, investigate `Canvas2DComponent.razor.cs`.
2. ShapeTreeView vs SceneTreePanel — the legacy code uses `ShapeTreeView`; the modern ClockDemo uses `SceneTreePanel`. 2D pages don't have a `FoStage3D` so SceneTreePanel's `Stage` parameter may not apply. Keep `ShapeTreeView` for 2D or pass model only.

**Known Limitations:**
1. No `CommandPanel` in initial spec — can be added in v2 if desired
2. `ShapeTreeView` is the older global component; it should work fine for 2D

**Quick Verification Checklist:**
- [ ] Compiles without errors
- [ ] Runs without exceptions
- [ ] Three canvas panels render with colored backgrounds
- [ ] Page A: rotating rectangle visible
- [ ] Page B: three bouncing circles visible
- [ ] Page C: two moving boxes with connector between them
- [ ] Tree view shows Drawing/Page hierarchy
- [ ] No console errors for 30 seconds
- [ ] Disposal clean — no errors on navigation away

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about building a multi-canvas 2D test page — it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 2D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you build the MultiCanvas2DTest page, you're testing whether AI can:
1. Successfully use 40 years of distilled programming patterns
2. Drive **multiple independent 2D canvases** from a single model — proving the Drawing/Page architecture supports parallel visual contexts
3. Wire up **per-shape animation callbacks** that produce smooth, continuous motion across all canvases simultaneously
4. Use **FoShape1D connectors with glue** to create dynamic relationships between moving shapes

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can leverage accumulated human wisdom to create **multi-viewport 2D animation systems** — a foundational capability for any serious visual application (CAD, diagramming, simulation). This demonstrates that the Drawing/Page architecture cleanly supports independent visual contexts with shared animation infrastructure.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## 1. Project Convention Scan

Directory: `Components/Pages/`  
Files found: 40+ pages

**Universal Patterns (all modern pages follow):**
- Every page has a code-behind `.razor.cs` with `partial class`
- Modern pages (ClockDemo) use a Model class that inherits `MxComponent`
- Model classes live alongside the page (e.g., `ClockDemoModel.cs` in `Components/Pages/`)
- Code-behind uses `private` injection with `= null!`: `[Inject] private IWorkspace Workspace { get; set; } = null!;`
- OR `[Inject] public required` pattern — both exist; ClockDemo uses `private ... = null!`
- Page namespace: `Three2025.Components.Pages` (for razor) / code-behind may use `FoundryWorldsAndDrawings.Blazor.Components.Pages`
- Model namespace: `FoundryWorldsAndDrawings.Blazor.Models`

**This spec MUST follow:**
- [x] Include a Model class (`MultiCanvas2DTestModel`) inheriting `MxComponent`
- [x] Use Model-behind pattern matching ClockDemo
- [x] Model handles domain logic (page setup, shape creation, animation)
- [x] Code-behind handles Blazor lifecycle only (refs, init, dispose)

---

## 2. Architecture Analysis

**Legacy Reference:** `MultiCanvas2DTest.razor.cs` — This is legacy code that predates the Model-behind pattern  
**Modern Reference:** `ClockDemo.razor.cs` + `ClockDemoModel.cs` — This IS the pattern to follow

**Legacy or Modern?:** The existing MultiCanvas2DTest is **Legacy** — all logic stuffed into code-behind, no Model class, no commands, no editor pattern.

**Intent to preserve (from legacy):**
- Four-panel grid layout: three 2D canvases + one tree view
- Page A: rotating rectangle (45°/sec)
- Page B: three phase-offset sine-wave circles (red/green/blue)  
- Page C: two oscillating boxes (orange/purple) connected by a FoShape1D arrow
- ShapeTreeView showing Drawing/Page hierarchy

**Patterns to NOT carry forward:**
- All logic in code-behind (no model)
- `Task.Run(async () => { await Task.Delay(100); ... })` in OnAfterRender — fragile timing hack
- `[Inject] public required` pattern — use `private ... = null!` to match ClockDemo
- Direct field references to shapes (`_rectA`, `_circleB1`, etc.) — let the model own shape state
- Manual FPS calculation (`AnimationFrameBus.GetCurrentFps()`) — use the 2D callback's tick parameter

**Current Pattern (for the spec):** Model-first with per-shape OnBeforeRender hooks (ClockDemo pattern adapted for 2D)

---

## 3. Verified Against

- **ClockDemo.razor.cs** — Modern code-behind pattern (verified February 10, 2026)
- **ClockDemoModel.cs** — Modern model pattern with MxComponent, commands, editor (verified February 10, 2026)
- **Canvas2DComponent.razor.cs** — Source-audited; parameters are `SceneName` (required), `CanvasWidth` (int, default 1800), `CanvasHeight` (int, default 1200). **No `PageName` parameter exists.**
- **FOUNDRY_ANIMATIONS_AND_LIFECYCLE_REFERENCE.md** — 2D OnBeforeRender signature is `Action<FoGlyph2D, int>` (shape, tick)
- **FoShape1D** — `GlueStartTo(target, "RIGHT")` / `GlueFinishTo(target, "LEFT")` verified in source

**Method Verification:**
- ✅ `drawing.EstablishPage<FoPage2D>("PageA")` — creates or gets page by name
- ✅ `page.AddShape(shape)` — adds FoGlyph2D to page
- ✅ `page.Color = "LightCoral"` — sets page background  
- ✅ `shape.MoveTo(x, y)` — sets PinX/PinY position
- ✅ `shape.OnBeforeRender((shape, tick) => { ... })` — 2D animation callback, signature `Action<FoGlyph2D, int>`
- ✅ `shape.ClearBeforeRender()` — removes animation callback
- ✅ `wire.GlueStartTo(shape, "RIGHT")` — anchor connector start to shape's right connection point
- ✅ `wire.GlueFinishTo(shape, "LEFT")` — anchor connector finish to shape's left connection point
- ✅ `new FoShape2D(width, height, color)` — constructor: int width, int height, string color
- ✅ `new FoShape1D("Arrow", "cyan")` — named connector constructor

**⚠️ CRITICAL DISCREPANCY:** The legacy code uses `Canvas2DComponent ... PageName="PageA"` — but source audit shows Canvas2DComponent has NO `PageName` parameter. It uses `SceneName` only. The `SceneName` value is used to find/create the page. Use `SceneName="PageA"` etc.

---

## 4. Reference Implementation Strategy

### Primary Reference
Copy: `Components/Pages/ClockDemo.razor.cs` (code-behind pattern)  
Copy: `Components/Pages/ClockDemoModel.cs` (model pattern)  
Demonstrates: Model-behind with MxComponent, commands, editor, clean disposal

### Modification Steps
1. Copy `ClockDemo.razor.cs` → `MultiCanvas2DTest.razor.cs`
2. Rename class to `MultiCanvas2DTest`, change injections for 2D (no `ISelectionService` needed initially)
3. Replace single `Canvas3DComponent` ref with no canvas refs (2D canvases self-manage via SceneName)
4. Replace `ClockDemoModel` instantiation with `MultiCanvas2DTestModel`
5. Copy `ClockDemoModel.cs` → `MultiCanvas2DTestModel.cs`
6. Rename class to `MultiCanvas2DTestModel`, change from 3D (arena/stage) to 2D (drawing/pages)
7. Replace clock shape-building with three page setup methods
8. Replace clock animation hooks with 2D animation hooks

### Delta from ClockDemo
- **2D not 3D**: Uses `IDrawing` / `FoPage2D` instead of `IArena` / `FoStage3D`
- **Multiple pages**: Three independent pages (PageA, PageB, PageC) instead of one stage
- **No Canvas refs in code-behind**: Canvas2DComponent self-manages via `SceneName` — the model just sets up pages and shapes, the canvases find their pages by name
- **2D OnBeforeRender signature**: `(FoGlyph2D shape, int tick)` — two params, not three
- **Connectors**: Page C uses `FoShape1D` with glue — no 3D equivalent in ClockDemo
- **Simpler commands**: Build/Clear/Start/Stop (no T-Rex or Submarine)

---

## 5. Infrastructure Assumptions

### Assumption: Multiple Canvas2DComponents Can Coexist
- [ ] Each `Canvas2DComponent` with a unique `SceneName` creates/finds its own `FoPage2D`
- [ ] Each canvas subscribes independently to `AnimationFrameBus`
- [ ] Each canvas filters for `IsDrawing2D()` events and renders its own managed page
- [ ] If broken: Check `Canvas2DComponent.OnAfterRenderAsync` — does it use `ManagedPage` correctly?

### Assumption: Drawing Service Is Shared Singleton  
- [ ] `IDrawing` (via `Workspace.GetDrawing()`) is a singleton shared across all canvases
- [ ] `drawing.EstablishPage<FoPage2D>("PageA")` creates a page in the shared drawing
- [ ] Multiple canvases reading from the same drawing is the intended architecture
- [ ] If broken: Check DI registration for `IDrawing` / `FoDrawing2D`

### Assumption: Per-Shape OnBeforeRender Fires Automatically
- [ ] Callbacks registered via `shape.OnBeforeRender(...)` fire during the render pass
- [ ] No explicit subscription to AnimationFrameBus needed in the model
- [ ] The Canvas2DComponent's render loop calls `drawing.RenderPage()` which invokes shape callbacks
- [ ] If broken: Check `FoGlyph2D.RenderDetailed()` → does it call `_onBeforeRender?.Invoke()`?

### Assumption: FoShape1D Glue Updates Automatically
- [ ] `GlueStartTo` / `GlueFinishTo` create persistent connections
- [ ] During render, `RecomputeGlue()` reads target shape positions and updates connector endpoints
- [ ] Moving glued shapes automatically moves connector endpoints
- [ ] If broken: Check `FoShape1D.RecomputeGlue()` and `ComputeStartFor()`/`ComputeFinishFor()`

---

## 6. Code Path Traces

### When you call `drawing.EstablishPage<FoPage2D>("PageA")`
1. `FoDrawing2D.EstablishPage<T>()` delegates to `PageManager.EstablishPage<T>(name)`
2. PageManager checks if page with that name exists — if yes, returns existing
3. If not, creates new `FoPage2D(name)`, adds to collection
4. Page is NOT automatically active — Canvas2DComponent calls `SetActivePage()` when it binds

### When Canvas2DComponent initializes with `SceneName="PageA"`
1. In `OnAfterRenderAsync(firstRender)`, canvas calls `drawing.Pages().FindPage(SceneName)`
2. If found, stores as `ManagedPage`; if not, calls `drawing.EstablishPage<FoPage2D>(SceneName)` to create it
3. Canvas subscribes to `AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent)`
4. Each animation tick, canvas filters for `IsDrawing2D()` then calls `drawing.RenderPage(Ctx, ManagedPage, tick, fps)`

### When a 2D shape's OnBeforeRender fires
1. During `RenderPage()`, drawing iterates shapes on the page
2. For each shape, `RenderDetailed()` is called
3. Inside `RenderDetailed()`, `_onBeforeRender?.Invoke(this, tick)` fires FIRST
4. Then `OnPreDraw` → `OnDraw` → `OnPostDraw` → `OnDrawSelected` execute
5. Shape's updated PinX/PinY/Angle are used for drawing

### When FoShape1D connector renders with glued shapes
1. During render, `FoShape1D.RecomputeGlue()` is called
2. For each glue (START and FINISH), it finds the target shape
3. Reads target's `AttachTo()` coordinates for the named connection point
4. Updates `StartX,StartY` or `FinishX,FinishY` to match target position
5. Connector redraws between updated endpoints
6. **Key**: No animation callback needed on the connector — glue is recomputed every frame automatically

---

## 7. Code Smells to Avoid

### Task.Run + Task.Delay in OnAfterRender
**The legacy code does this:**
```csharp
// ❌ DON'T — fragile timing hack from legacy code
Task.Run(async () =>
{
    await Task.Delay(100);
    SetupPageA();
    SetupPageB();
    SetupPageC();
});
```
**Why it's wrong:** Race condition — 100ms may not be enough for canvases to initialize. Fire-and-forget Task.Run can cause exceptions on disposed objects.

**Do this instead:** Setup pages in `OnInitialized` (or the model constructor). The Canvas2DComponent will find the pages by `SceneName` when it initializes. Pages don't need the canvas to exist — they're data objects. The canvas finds them.
```csharp
// ✅ DO — model sets up pages eagerly; canvases find them later
protected override void OnInitialized()
{
    _model = new MultiCanvas2DTestModel(Workspace);
    _model.BuildAllPages();  // Creates pages and shapes in IDrawing
}
```

### Manual FPS Calculation in 2D Callbacks
**The legacy code does this:**
```csharp
// ❌ DON'T — unnecessary complexity
shape.OnBeforeRender((shape, tick) => {
    var fps = AnimationFrameBus.GetCurrentFps();
    var deltaTime = 1.0 / Math.Max(fps, 1);
    _rotationA += deltaTime * 45.0;
});
```
**Why it's problematic:** Coupling to `AnimationFrameBus.GetCurrentFps()` is an external dependency inside a per-shape callback. FPS can be 0 on first frame.

**Do this instead:** Use frame-based increments like the proven 3D pattern:
```csharp
// ✅ DO — frame-based, matches Golden Pattern
double angle = 0.0;
shape.OnBeforeRender((shape, tick) => {
    angle += 0.75;  // degrees per frame — smooth at 60fps
    angle %= 360.0;
    shape.Angle = angle;
});
```

### Shared Mutable State Across Callbacks
**The legacy code does this:**
```csharp
// ❌ DON'T — _timeB is modified by circleB1's callback, read by circleB2 and circleB3
private double _timeB = 0;
_circleB1.OnBeforeRender((shape, tick) => { _timeB += deltaTime; ... });
_circleB2.OnBeforeRender((shape, tick) => { shape.PinY = ... Math.Sin(_timeB ...); });
```
**Why it's fragile:** Callback execution order determines behavior. If circleB2 runs before circleB1, it reads stale `_timeB`.

**Do this instead:** Use `tick` parameter (guaranteed monotonically increasing) or capture a shared angle variable that only one callback writes:
```csharp
// ✅ DO — use tick for phase-offset sine waves
circle1.OnBeforeRender((shape, tick) => {
    shape.PinY = baseY + (int)(Math.Sin(tick * 0.05) * amplitude);
});
circle2.OnBeforeRender((shape, tick) => {
    shape.PinY = baseY + (int)(Math.Sin(tick * 0.05 + Math.PI * 2.0/3.0) * amplitude);
});
circle3.OnBeforeRender((shape, tick) => {
    shape.PinY = baseY + (int)(Math.Sin(tick * 0.05 + Math.PI * 4.0/3.0) * amplitude);
});
```

---

## 8. Known Gotchas

### Canvas2DComponent Has No `PageName` Parameter
- The legacy `.razor` file uses `PageName="PageA"` — **this parameter does not exist in the source**
- ✅ Use `SceneName="PageA"` — Canvas2DComponent uses SceneName to find/create its page
- 🔍 Debug: If canvas shows wrong content, check `Canvas2DComponent.ManagedPage` via breakpoint

### 2D OnBeforeRender Signature Differs from 3D
- **2D**: `Action<FoGlyph2D, int>` → `(shape, tick)` — TWO parameters
- **3D**: `Action<FoGlyph3D, int, double>` → `(shape, tick, fps)` — THREE parameters
- ❌ Using `(shape, tick, fps)` in a 2D callback will not compile
- ✅ Use `(shape, tick)` for all FoGlyph2D/FoShape2D callbacks

### FoShape1D Connector Needs Height Set
- `Height` on a FoShape1D controls line thickness, NOT vertical position
- Legacy code uses `_connectorC.Height = 50;` — this means the arrow is 50px thick
- ✅ Set `Height` to a reasonable value (e.g., 4–8 for a thin arrow, 50 for a wide band)
- 🔍 Debug: If connector invisible, check Height > 0

### Page Setup Timing
- Pages are data objects in `IDrawing` — they can be created before canvases render
- Canvas2DComponent finds its page by `SceneName` during `OnAfterRenderAsync`
- ✅ Create pages in model constructor or in `BuildAllPages()` called from `OnInitialized`
- ❌ DON'T wait for canvases to be ready — pages don't need canvases; canvases find pages

### Animation Starts Automatically
- Canvas2DComponent subscribes to AnimationFrameBus during its own initialization
- No need for the model to call `DoStart()` or `AnimationFrameBus.ResumeAllAnimations()`
- Per-shape `OnBeforeRender` callbacks fire automatically during each canvas's render loop
- ✅ Just register callbacks on shapes. Done.

---

## 9. Troubleshooting Guide

### Canvas Shows Blank (No Shapes)

**Symptom:** Canvas renders with background color but no shapes visible

**Diagnosis Steps:**
1. **Check page exists in drawing:**
   ```csharp
   var drawing = Workspace.GetDrawing()!;
   var page = drawing.Pages().FindPage("PageA");
   $"Page found: {page != null}, shapes: {page?.Members().Count()}".WriteInfo();
   ```
2. **Check Canvas2DComponent bound to correct page:**
   Add temporary logging in model: `$"PageA shapes: {page.Members().Count()}".WriteInfo();`
3. **Verify SceneName matches:** Canvas's `SceneName` must exactly match the name passed to `EstablishPage`

**Common Causes:**
- SceneName mismatch between razor and model
- Pages created after canvas already initialized (timing issue)
- Shapes added to wrong page

---

### Shapes Not Animating

**Symptom:** Shapes appear but don't move/rotate

**Diagnosis Steps:**
1. **Verify callback registered:**
   ```csharp
   var callback = shape.GetBeforeRender();
   $"Callback registered: {callback != null}".WriteInfo();
   ```
2. **Check tick is incrementing:** Add `$"tick={tick}".WriteInfo()` inside callback (remove after testing — will spam console)
3. **Verify shape is on an active page being rendered by a canvas**

**Common Causes:**
- Wrong OnBeforeRender signature (3-arg instead of 2-arg)
- Shape added to page after callback was registered (order matters? — test)
- Canvas not subscribed to animation bus

---

### Connector Not Following Shapes

**Symptom:** Arrow/connector stays in original position while glued shapes move

**Diagnosis Steps:**
1. **Verify glue established:**
   ```csharp
   $"Glues: {connector.AllGlues().Count()}".WriteInfo();
   ```
2. **Check connection point names:** `"RIGHT"` and `"LEFT"` must be uppercase
3. **Verify both target shapes exist on same page**

**Common Causes:**
- Connector added to different page than its target shapes
- Connection point name case mismatch
- Glue method called before target shapes were added to page

---

### Console Spam / Warnings

**Symptom:** Hundreds of warnings per second in browser console

**Diagnosis Steps:**
1. Open browser DevTools console
2. Watch for 30 seconds
3. Filter for `Warning` or `Error` level messages

**Expected clean console:** Only initialization messages, then silence. If you see repeating messages, something in a per-frame callback is logging.

**Common Causes:**
- Logging inside OnBeforeRender (runs 60x/sec)
- Shape name validation warnings (use underscores, not hyphens)

---

## 10. Implementation Steps

### Step 1: Create Model Class — `MultiCanvas2DTestModel.cs`

Create `Components/Pages/MultiCanvas2DTestModel.cs`:

1. Class `MultiCanvas2DTestModel` inheriting `MxComponent`
2. Constructor takes `IWorkspace workspace`
3. Store drawing reference: `_workspace.GetDrawing()!`
4. Call `SetupCommands()` in constructor
5. **Verify:** File compiles

### Step 2: Implement Page Setup Methods in Model

Three private methods: `SetupPageA()`, `SetupPageB()`, `SetupPageC()`

**PageA — Rotating Rectangle:**
```csharp
private void SetupPageA()
{
    var page = _drawing.EstablishPage<FoPage2D>("PageA");
    page.Color = "LightCoral";

    double angle = 0.0;
    var rect = new FoShape2D(100, 100, "DarkBlue");
    rect.MoveTo(400, 300);
    rect.OnBeforeRender((shape, tick) =>
    {
        angle += 0.75;   // ~45°/sec at 60fps
        angle %= 360.0;
        shape.Angle = angle;
    });
    page.AddShape(rect);
}
```

**PageB — Three Phase-Offset Sine Circles:**
```csharp
private void SetupPageB()
{
    var page = _drawing.EstablishPage<FoPage2D>("PageB");
    page.Color = "LightSkyBlue";

    var baseY = 300;
    var amplitude = 100;

    var circle1 = new FoShape2D(60, 60, "red");
    circle1.MoveTo(200, baseY);
    circle1.OnBeforeRender((shape, tick) =>
    {
        shape.PinY = baseY + (int)(Math.Sin(tick * 0.05) * amplitude);
    });
    page.AddShape(circle1);

    var circle2 = new FoShape2D(60, 60, "green");
    circle2.MoveTo(400, baseY);
    circle2.OnBeforeRender((shape, tick) =>
    {
        shape.PinY = baseY + (int)(Math.Sin(tick * 0.05 + Math.PI * 2.0 / 3.0) * amplitude);
    });
    page.AddShape(circle2);

    var circle3 = new FoShape2D(60, 60, "blue");
    circle3.MoveTo(600, baseY);
    circle3.OnBeforeRender((shape, tick) =>
    {
        shape.PinY = baseY + (int)(Math.Sin(tick * 0.05 + Math.PI * 4.0 / 3.0) * amplitude);
    });
    page.AddShape(circle3);
}
```

**PageC — Oscillating Boxes with Connector:**
```csharp
private void SetupPageC()
{
    var page = _drawing.EstablishPage<FoPage2D>("PageC");
    page.Color = "LightGreen";

    double offset = 0.0;
    var baseX1 = 200;
    var baseX2 = 600;

    var box1 = new FoShape2D(80, 80, "orange");
    box1.MoveTo(baseX1, 300);
    box1.OnBeforeRender((shape, tick) =>
    {
        offset = Math.Sin(tick * 0.02) * 50;
        shape.PinX = baseX1 + (int)offset;
    });
    page.AddShape(box1);

    var box2 = new FoShape2D(80, 80, "purple");
    box2.MoveTo(baseX2, 300);
    box2.OnBeforeRender((shape, tick) =>
    {
        shape.PinX = baseX2 - (int)offset;
    });
    page.AddShape(box2);

    // Connector — follows glued shapes automatically
    var connector = new FoShape1D("Arrow", "cyan");
    connector.Height = 6;
    connector.GlueStartTo(box1, "RIGHT");
    connector.GlueFinishTo(box2, "LEFT");
    page.AddShape(connector);
}
```

**Verify:** Model compiles. Pages and shapes are created. No canvas references needed.

### Step 3: Implement Commands in Model

```csharp
private void SetupCommands()
{
    var editor = this.EstablishEditor<MxComponentEditor>();

    editor.EstablishAction("BuildAllPages", action =>
    {
        try
        {
            SetupPageA();
            SetupPageB();
            SetupPageC();
            return MxActionResult.Ok("All pages built successfully");
        }
        catch (Exception ex)
        {
            $"❌ BuildAllPages failed: {ex.Message}".WriteError();
            return MxActionResult.Fail($"Failed: {ex.Message}");
        }
    });

    editor.EstablishCommand("BuildAllPages", "BuildAllPages", configure: cmd =>
    {
        cmd.DisplayName = "🏗️ Build All Pages";
        cmd.Description = "Create all three animated 2D pages";
        cmd.Category = "Page Controls";
    });

    editor.EstablishAction("ClearAllPages", action =>
    {
        _drawing.ClearAll();
        return MxActionResult.Ok("All pages cleared");
    });

    editor.EstablishCommand("ClearAllPages", "ClearAllPages", configure: cmd =>
    {
        cmd.DisplayName = "🗑️ Clear All";
        cmd.Description = "Remove all pages and shapes";
        cmd.Category = "Page Controls";
    });
}
```

**Verify:** Commands compile. `AvailableCommands` property returns the command list.

### Step 4: Add Public API to Model

```csharp
public bool IsBuilt
{
    get
    {
        var page = _drawing.Pages().FindPage("PageA");
        return page != null;
    }
}

public IEnumerable<MxCommand> AvailableCommands => this.Walk<MxCommand>().CollectAsList();

public void Cleanup()
{
    "📐 MultiCanvas2DTestModel: Cleaned up".WriteInfo();
}
```

### Step 5: Create Code-Behind — `MultiCanvas2DTest.razor.cs`

Follow ClockDemo pattern exactly:

```csharp
using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Blazor.Models;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.PubSub;
using BlazorComponentBus;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Components.Pages;

public partial class MultiCanvas2DTest : ComponentBase, IDisposable
{
    [Inject] private IWorkspace Workspace { get; set; } = null!;
    [Inject] private ComponentBus PubSub { get; set; } = null!;

    private MultiCanvas2DTestModel? _model;

    protected override void OnInitialized()
    {
        _model = new MultiCanvas2DTestModel(Workspace);

        // Build pages immediately — canvases will find them by SceneName
        _model.BuildAllPages();

        PubSub?.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);

        "📐 MultiCanvas2DTest page initialized".WriteSuccess();
    }

    private void HandleCommandResult(MxActionResult result)
    {
        $"MultiCanvas2DTest: Command executed - {result.Message}".WriteInfo();
        StateHasChanged();
    }

    private void OnRefreshUIEvent(RefreshUIEvent evt)
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _model?.Cleanup();
        "📐 MultiCanvas2DTest page disposed".WriteInfo();
    }
}
```

**Verify:** Compiles. Model is created and pages are built during `OnInitialized`.

### Step 6: Create Razor View — `MultiCanvas2DTest.razor`

```razor
@page "/multi-canvas-2d-test"
@rendermode InteractiveServer
@using FoundryMicroCore.Core
@using FoundryMicroCore.Blazor.Controls.Components
@using FoundryMicroCore.Blazor.Controls.Components.Commands
@using FoundryWorldsAndDrawings.Shared
@using FoundryWorldsAndDrawings.Shape

<PageTitle>Multi-Canvas 2D Test</PageTitle>

<div class="mc2d-container">
    <div class="mc2d-header">
        <h2>📐 Multi-Canvas 2D Test — Independent Animated Pages</h2>
        <p class="mc2d-description">
            Three independent 2D canvases with per-shape animation callbacks.
            Each canvas renders its own FoPage2D from the shared Drawing.
        </p>
    </div>

    <div class="mc2d-layout">
        <!-- LEFT: Command Panel -->
        <div class="command-panel-section">
            <h3>Commands</h3>
            @if (_model != null)
            {
                <CommandPanel 
                    Commands="@_model.AvailableCommands"
                    OnCommandExecuted="@HandleCommandResult"
                    GroupByCategory="true"
                    ShowDescriptions="false"
                    CollapsibleCategories="true" />
            }
        </div>

        <!-- CENTER: Three canvases in a column -->
        <div class="canvas-grid-section">
            <!-- Page A -->
            <div class="canvas-cell" style="border-color: #4CAF50;">
                <div class="canvas-label" style="background: #4CAF50;">
                    Page A — Rotating Rectangle
                </div>
                <div class="canvas-body">
                    <Canvas2DComponent SceneName="PageA" CanvasWidth="800" CanvasHeight="400" />
                </div>
            </div>

            <!-- Page B -->
            <div class="canvas-cell" style="border-color: #2196F3;">
                <div class="canvas-label" style="background: #2196F3;">
                    Page B — Sine Wave Circles
                </div>
                <div class="canvas-body">
                    <Canvas2DComponent SceneName="PageB" CanvasWidth="800" CanvasHeight="400" />
                </div>
            </div>

            <!-- Page C -->
            <div class="canvas-cell" style="border-color: #FF9800;">
                <div class="canvas-label" style="background: #FF9800;">
                    Page C — Connected Boxes
                </div>
                <div class="canvas-body">
                    <Canvas2DComponent SceneName="PageC" CanvasWidth="800" CanvasHeight="400" />
                </div>
            </div>
        </div>

        <!-- RIGHT: Tree View -->
        <div class="tree-panel-section">
            <h3>Drawing Hierarchy</h3>
            <ShapeTreeView />
        </div>
    </div>
</div>
```

Add CSS in a `<style>` block matching ClockDemo's layout approach. Key layout: flex row with left command panel (15%), center canvas grid (55%), right tree panel (30%).

**Verify:** Page compiles. Layout renders with three labeled canvas areas.

### Step 7: Verify and Test

1. Build: `dotnet build`
2. Run: `dotnet watch run`
3. Navigate to `/multi-canvas-2d-test`
4. **Verify:** All three canvases show backgrounds and animated shapes
5. **Console check:** Watch browser console for 30 seconds — should be clean after init messages

### Step 8: Disposal Verification

1. Navigate away from the page (click Home or another page)
2. **Verify:** No console errors
3. Navigate back
4. **Verify:** Page rebuilds cleanly

---

## 🏆 GOLDEN PATTERN — 2D Per-Shape Animation (Copy This Exactly)

**Source:** `ClockDemoModel.cs` adapted for 2D signatures

```csharp
// 2D rotating shape — frame-based angle increment
double angle = 0.0;
shape.OnBeforeRender((self, tick) =>
{
    angle += 0.75;       // degrees per frame
    angle %= 360.0;
    self.Angle = angle;
});
page.AddShape(shape);
```

```csharp
// 2D oscillating shape — tick-based sine wave
int baseY = 300;
int amplitude = 100;
shape.OnBeforeRender((self, tick) =>
{
    self.PinY = baseY + (int)(Math.Sin(tick * 0.05) * amplitude);
});
page.AddShape(shape);
```

```csharp
// 2D connector with glue — NO animation callback needed
var wire = new FoShape1D("Arrow", "cyan");
wire.Height = 6;
wire.GlueStartTo(box1, "RIGHT");
wire.GlueFinishTo(box2, "LEFT");
page.AddShape(wire);
// Connector follows glued shapes automatically during render
```

**CRITICAL RULES:**
- ❌ Do NOT call `AnimationFrameBus.GetCurrentFps()` inside callbacks
- ❌ Do NOT use time-based `deltaTime` calculations — use frame-based increments
- ❌ Do NOT add `if (tick == 0) return` guards
- ❌ Do NOT use `Task.Run` + `Task.Delay` for setup timing
- ❌ Do NOT use hyphens in shape names — underscores only
- ✅ Set position/angle. That's it. The render pipeline handles everything else.

---

## 11. Implementer Behavior Warnings

### Things You Will Be Tempted To Do (DON'T)

1. **Wait for canvases to be ready before creating pages**
   Why you'll want to: "Canvas needs to exist so the page has somewhere to render"
   Why you shouldn't: Pages are data objects in IDrawing. Canvases find them by SceneName. Create pages in OnInitialized, canvases will pick them up.

2. **Use `Task.Delay` to synchronize canvas initialization**
   Why you'll want to: "The legacy code does it — there must be a timing issue"
   Why you shouldn't: The legacy code had pages and canvases tangled together. With the Model pattern, the model owns the data; canvases are just views. No timing needed.

3. **Calculate deltaTime from FPS in 2D callbacks**
   Why you'll want to: "Time-based animation is more correct than frame-based"
   Why you shouldn't: Every working example uses frame-based increments with `tick`. FPS can be 0 on first frame. Match the pattern.

4. **Add canvas `@ref` fields in the code-behind**
   Why you'll want to: "I need to tell the canvas which page to use"
   Why you shouldn't: Canvas2DComponent self-manages via SceneName. You don't talk to it. It talks to IDrawing.

5. **Register shape callbacks AFTER adding shapes to the page**
   Why you'll want to: "Shape needs to be 'on the page' first for callbacks to fire"
   Why you shouldn't: Callback registration is on the shape object itself, not the page. Register before or after AddShape — both work. But for clarity, register before AddShape.

---

## 12. Project Convention Compliance

### Convention: Model-Behind Pattern
Evidence: ClockDemo (modern) has `ClockDemoModel.cs` inheriting `MxComponent`
This spec: ✅ Includes `MultiCanvas2DTestModel` inheriting `MxComponent`

### Convention: Injection Pattern — `private ... = null!`
Evidence: ClockDemo uses `[Inject] private IWorkspace Workspace { get; set; } = null!;`
This spec: ✅ Matches established pattern

### Convention: Namespace — Code-behind
Evidence: ClockDemo code-behind uses `namespace FoundryWorldsAndDrawings.Blazor.Components.Pages;`
This spec: ✅ Code-behind uses `namespace Three2025.Components.Pages;` (matching the razor's implicit namespace — **verify against ClockDemo's actual namespace and adjust if needed**)

### Convention: Namespace — Model
Evidence: ClockDemoModel uses `namespace FoundryWorldsAndDrawings.Blazor.Models;`
This spec: ✅ Model uses `namespace FoundryWorldsAndDrawings.Blazor.Models;`

### Convention: Commands via EstablishEditor + EstablishAction + EstablishCommand
Evidence: ClockDemoModel sets up all commands this way
This spec: ✅ Follows same pattern for Build/Clear commands

---

## 13. Visual Expectations

The page should display a three-column layout:
- **Left (15%):** Command panel with Build/Clear commands
- **Center (55%):** Three 2D canvas panels stacked vertically, each with a colored header label
- **Right (30%):** ShapeTreeView showing Drawing/Page hierarchy

Each canvas panel has:
- A colored header bar (green for A, blue for B, orange for C) with the page description
- A canvas area below that fills the cell
- Canvas backgrounds determined by page `Color` property (LightCoral, LightSkyBlue, LightGreen)

Canvases should fill their container cells. Use CSS to let them stretch rather than fixed pixel dimensions that leave gaps.

---

## 14. Model/Domain Section

### MultiCanvas2DTestModel : MxComponent

**File:** `Components/Pages/MultiCanvas2DTestModel.cs`

**Constructor:**
```csharp
public MultiCanvas2DTestModel(IWorkspace workspace) : base("MultiCanvas2DTest")
```

**Responsibilities:**
- Page creation (three FoPage2D instances via IDrawing)
- Shape creation and positioning
- Animation callback registration (per-shape OnBeforeRender)
- FoShape1D connector with glue
- Command setup (Build, Clear)

**Code-behind keeps:**
- Blazor lifecycle (`OnInitialized`, `Dispose`)
- PubSub subscription for UI refresh
- `HandleCommandResult` callback
- Model instantiation

**Code-behind delegates to model:**
- `_model.BuildAllPages()` — creates all pages and shapes
- `_model.AvailableCommands` — exposes commands to CommandPanel
- `_model.Cleanup()` — cleanup on dispose

> **The rule:** Model owns the domain (drawing, pages, shapes, animation). Code-behind owns Blazor mechanics (lifecycle, DI, UI binding). They meet through the model's public API.

---

## 15. Step-by-Step Test Sequence

### Step 1: Page Load

**Action:** Navigate to `/multi-canvas-2d-test`

**Expected Results:**
- ✅ Page renders without exceptions
- ✅ Three canvas panels visible with colored headers
- ✅ Command panel on left shows "Build All Pages" and "Clear All" buttons
- ✅ Tree view panel on right

**Console Output:**
```
📐 MultiCanvas2DTestModel: Ready
SetupPageA: Starting
SetupPageB: Starting
SetupPageC: Starting
📐 MultiCanvas2DTest page initialized
```

**If Failed:**
- Blank page → Check compilation errors
- Missing canvases → Verify SceneName values match between razor and model

### Step 2: Verify Page A — Rotating Rectangle

**Action:** Look at the top canvas (Page A)

**Expected Results:**
- ✅ LightCoral background visible
- ✅ Dark blue 100x100 rectangle near center
- ✅ Rectangle rotates smoothly (~45°/sec)

**If Failed:**
- No rotation → Check OnBeforeRender callback uses `(shape, tick)` not `(shape, tick, fps)`
- Wrong position → Verify MoveTo(400, 300)

### Step 3: Verify Page B — Sine Wave Circles

**Action:** Look at the middle canvas (Page B)

**Expected Results:**
- ✅ LightSkyBlue background visible
- ✅ Three circles: red (left), green (center), blue (right)
- ✅ Circles bob up and down in a smooth sine wave
- ✅ Circles are phase-offset (120° apart) — they form a wave pattern

**If Failed:**
- All circles move in sync → Check phase offsets (`+ Math.PI * 2.0/3.0` and `+ Math.PI * 4.0/3.0`)
- Circles static → Check tick multiplier (0.05) — too small = imperceptible

### Step 4: Verify Page C — Connected Boxes

**Action:** Look at the bottom canvas (Page C)

**Expected Results:**
- ✅ LightGreen background visible
- ✅ Orange box on left, purple box on right
- ✅ Boxes oscillate toward and away from each other
- ✅ Cyan connector/arrow stretches between them, following their movement
- ✅ Connector endpoints stay attached (glued) to box edges

**If Failed:**
- Connector doesn't follow → Check GlueStartTo/GlueFinishTo called BEFORE page.AddShape(connector)
- Connector invisible → Check `connector.Height > 0`
- Boxes don't move → Same diagnosis as Step 2

### Step 5: Console Verification (30 seconds)

**Action:** Open browser DevTools console. Watch for 30 seconds.

**Expected Results:**
- ✅ Initialization messages appear once
- ✅ No repeating warnings or errors
- ✅ No Euler overflow warnings
- ✅ No shape name validation warnings

**If Failed:**
- Repeating messages → Something is logging inside a per-frame callback — find and remove
- Name warnings → Shape names contain hyphens or special characters — use underscores

### Step 6: Disposal

**Action:** Navigate to Home page, then back to `/multi-canvas-2d-test`

**Expected Results:**
- ✅ No console errors on navigation
- ✅ Page rebuilds and all animations restart

**If Failed:**
- Errors on navigate away → Check Dispose() cleanup
- Duplicate shapes on return → Check `ClearAll()` or `EstablishPage` idempotency

---

## 16. Success Criteria

### Compilation
- [ ] Zero compilation errors
- [ ] Zero compilation warnings related to our code
- [ ] All using statements resolve
- [ ] Model class compiles independently

### Runtime (First Load)
- [ ] Page loads without exceptions
- [ ] All three Canvas2DComponents initialize
- [ ] Pages created in IDrawing with correct names
- [ ] Command panel renders with commands

### Runtime (Functionality)
- [ ] Page A: rectangle rotates smoothly
- [ ] Page B: three circles oscillate with phase offsets
- [ ] Page C: two boxes oscillate, connector follows via glue
- [ ] Tree view shows Drawing hierarchy with three pages
- [ ] Commands work: Clear removes shapes, Build recreates them

### Console Cleanliness
- [ ] 30 seconds of clean console (no repeating warnings)
- [ ] No shape name validation errors
- [ ] No null reference exceptions

### Disposal
- [ ] No console errors on navigation away
- [ ] No memory leaks (callbacks cleaned up)

### Known Acceptable Issues
- [ ] ShapeTreeView may need a manual refresh to show updates (it's the older component)
- [ ] Canvas sizing may need CSS tuning to fill containers perfectly

---

## Confidence Levels

- **Architecture Pattern:** 🟢 High — Directly copied from ClockDemo (verified, working)
- **Model-Behind Pattern:** 🟢 High — Universal project convention, ClockDemo is the reference
- **Canvas2DComponent Setup:** 🟢 High — Source-audited, SceneName pattern confirmed
- **Page Creation (EstablishPage):** 🟢 High — Verified in API and source
- **2D OnBeforeRender Signature:** 🟢 High — `(shape, tick)` confirmed in lifecycle reference
- **FoShape1D Connector + Glue:** 🟡 Medium — Pattern verified in source, but glue behavior during animation not personally traced end-to-end
- **Frame-Based vs Time-Based Animation:** 🟢 High — All working 3D examples use frame-based; adapting for 2D
- **ShapeTreeView for 2D:** 🟡 Medium — Used extensively in legacy pages, should work; if not, can omit
- **Page Setup Timing (OnInitialized):** 🟡 Medium — Confident that pages-before-canvases works based on source analysis, but the legacy code explicitly delayed. If pages don't appear, try moving BuildAllPages to OnAfterRender.

---

*Specification complete. Indy, you have a clear modern reference (ClockDemo), verified API signatures, and a domain-separated model. The legacy code shows you WHAT to build; this spec shows you HOW. Go build it.* 🗺️⚙️
