# Tug of War — 2D vs 3D Animation Comparison

**Specification by:** Claude "Atlas" (Architect)  
**Date:** February 8, 2026  
**Estimated Implementation Time:** 3–5 hours  
**Confidence:** 🟢 High (specification derived from working reference implementation)

**Route:** `/tugofwar`

> **IMPORTANT FOR INDY:** You will NOT have access to the library source code (FoundryWorldsAndDrawings, FoundryMicroCore, etc.). Library versions may have changed since this spec was written. Rely on:
> 1. **The API reference docs** that ship WITH the libraries (look for `*_API_REFERENCE.md` files)
> 2. **Working examples in Three2025/** — especially `Apprentice/` and `Components/Pages/`
> 3. **The inline code samples in this spec** — these are extracted from working code
> 4. **What actually compiles** — if a method name changed, try the name that works

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about building a Tug of War animation demo — it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 2D and 3D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you build the Tug of War demo, you're testing whether AI can:
1. Successfully use 40 years of distilled programming patterns
2. Operate **both 2D and 3D animation systems simultaneously** from a single Blazor page
3. Use the **per-frame callback pattern** (`BeforeAnimationRefresh`) to drive time-based 3D animation
4. Wire up the **stage-centric pattern** where `Canvas3DComponent` owns a `FoStage3D` and links it to a `Scene3D`
5. Demonstrate that **FoPipe3D** can dynamically connect moving shapes AND grow independently

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can leverage accumulated human wisdom to create **dual-canvas interactive animations** that exercise the full pipeline: shape creation → animation callbacks → stale-flag propagation → JS interop rendering. This is the most comprehensive test of whether the 2D and 3D subsystems can coexist and animate independently on the same page.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## 1. Architecture Analysis

**Current Pattern:** Stage-centric with `ComponentBase` code-behind and per-frame animation callbacks

### Key Characteristics

| Aspect | Detail |
|---|---|
| Base class | `ComponentBase` (standard Blazor) + `IDisposable` |
| Stage management | `Canvas3DComponent.Stage` — canvas creates and owns its `FoStage3D` |
| 2D page management | `Canvas2DComponent.Page` — canvas creates and owns its `FoPage2D` |
| Animation hookup | `AnimationFrameBus.SubscribeToAnimation()` in `OnInitialized()` for FPS/tick display; `BeforeAnimationRefresh()` on individual shapes for per-frame motion |
| Shape lifecycle | Create → `stage.AddShape()` → `arena.RenderArena(0,0)` for immediate push; or let animation loop handle it |
| Disposal | `ClearAnimationRefresh()` on shapes + `UnSubscribeFromAnimation()` on bus |

### Working Examples in Three2025 (Indy CAN Access These)

1. **`Apprentice/FoClockFace3D.cs`** — Parent-child hierarchy, `BeforeAnimationRefresh`, `CreateCylinder`, `CreateBox`, `AddShape` for children
2. **`Apprentice/CuckooClockTech.cs`** — Tech component pattern, shape references, animation state machine
3. **`Components/Pages/DebugCanvas.razor` + `.razor.cs`** — Simplest 3D page: stage retrieval, `FoPipe3D.CreateTube`, adding shapes
4. **`Components/Pages/GlueTest3D.razor` + `.razor.cs`** — `FoPipe3D`, `FoGluePipe3D`, `BeforeAnimationRefresh` on moving shapes, `ClearAnimationRefresh`, stage cleanup
5. **`Components/Pages/DualCanvas2D3DTest.razor` + `.razor.cs`** — Dual-canvas pattern (2D + 3D side-by-side)

### API Reference Docs (Look for These in Library Folders)
- `FOUNDRY_3D_API_REFERENCE.md` — Authoritative 3D method signatures (source-audited)
- `FOUNDRY_2D_API_REFERENCE.md` — 2D method signatures (NOT yet source-audited — verify if issues)
- `FOUNDRY_ANIMATIONS_AND_LIFECYCLE_REFERENCE.md` — Frame loop phases and hook naming
- `FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md` — IWorkspace, IArena, IDrawing
- `FOUNDRY_MICROCORE_API_REFERENCE.md` — MxObject, MxComponent base patterns

---

## 2. Verified API Signatures

> **NOTE:** These were verified against source code on February 8, 2026. If library versions have changed, method names may differ. The API reference docs shipping with the libraries are the ground truth.

### Method Verification

| Method | Status | Where to Verify |
|---|---|---|
| `new FoShape3D("name", "color")` | ✅ | 3D API Ref |
| `shape.CreateBox("name", w, h, d)` | ✅ | 3D API Ref — returns `FoShape3D` (fluent) |
| `shape.SetRecomputeBoundary()` | ✅ | 3D API Ref — opt-in boundary recompute |
| `shape.BeforeAnimationRefresh(callback)` | ✅ | Working in `FoClockFace3D.cs`. **Note:** may be renamed to `OnBeforeRender()` — use whichever compiles |
| `shape.ClearAnimationRefresh()` | ✅ | 3D API Ref. May become `ClearBeforeRender()` |
| `shape.SetTransformStale()` | ✅ | 3D API Ref — MUST call after position changes |
| `shape.SetGeometryStale()` | ✅ | 3D API Ref — MUST call when geometry/path changes |
| `shape.Transform.Position = new Vector3(x,y,z)` | ✅ | 3D API Ref — Transform3 property |
| `shape.GetWorldPosition()` | ✅ | Returns `(bool success, Vector3 pos)` |
| `shape.DistanceBetween(other)` | ✅ | Returns `(bool success, double distance)` |
| `new FoPipe3D("name", "color")` | ✅ | 3D API Ref |
| `pipe.CreatePipe("name", radius)` | ✅ | For FromShape3D→ToShape3D connections |
| `pipe.CreateTube("name", radius, path)` | ✅ | For explicit path-based tubes |
| `pipe.FromShape3D / ToShape3D` | ✅ | Shape endpoint properties |
| `pipe.Path3D` | ✅ | Settable `List<Vector3>` |
| `new FoText3D("name", "color")` | ✅ | 3D API Ref — `Text`, `FontSize` properties |
| `text.PreComputeMesh = (shape) => { }` | ✅ | Delegate for per-frame text update |
| `stage.AddShape(shape)` | ✅ | Dispatches bodies vs links by interface |
| `stage.AllBodies()` / `stage.AllLinks()` | ✅ | Returns collections of shapes |
| `stage.ClearAll()` | ✅ | Async, sends deletions to JS |
| `AnimationFrameBus.SubscribeToAnimation(callback)` | ✅ | 3D API Ref |
| `AnimationFrameBus.UnSubscribeFromAnimation(callback)` | ✅ | 3D API Ref |
| `AnimationFrameBus.PauseAllAnimations()` | ✅ | 3D API Ref |
| `AnimationFrameBus.ResumeAllAnimations()` | ✅ | 3D API Ref |
| `AnimationFrameBus.TriggerSingleFrame()` | ✅ | Async |
| `AnimationFrameBus.RunForFrames(n)` | ✅ | 3D API Ref |
| `AnimationFrameBus.GetAnimationState()` | ✅ | Returns string |
| `AnimationFrameBus.IsGloballyPaused()` | ✅ | Returns bool |
| `AnimationFrameBus.GetCurrentTick()` | ✅ | Returns int |
| `Canvas3DReference.Stage` | ✅ | Returns `FoStage3D?` |
| `Canvas3DReference.GetActiveScene()` | ✅ | Returns `(bool, Scene3D)` |
| `Canvas2DReference.Page` | ✅ | Returns `FoPage2D?` |
| `FoGlyph2D.Animations.Tween<T>(target, props, duration, delay)` | ✅ | 2D tween API |
| `new FoShape2D(w, h, "color")` | ✅ | 2D constructor |
| `shape2d.MoveTo(x, y)` | ✅ | 2D positioning |
| `new FoShape1D("name", "color")` | ✅ | 1D wire/arrow |
| `wire.GlueStartTo(shape, "RIGHT")` | ✅ | 2D glue anchors |
| `wire.GlueFinishTo(shape, "LEFT")` | ✅ | 2D glue anchors |
| `new FoText2D(w, h, "color")` | ✅ | 2D text |
| `arena.RenderArena(tick, fps)` | ✅ | Forces immediate render push |

---

## 3. Inline Code Samples (from Working Code)

> **These are complete, compilable patterns extracted from working examples in Three2025.**
> **Indy: Use these as your primary reference.** If something doesn't compile, check the API reference docs.

### 3A. Simplest 3D Page Pattern (from DebugCanvas)

**Razor markup:**
```razor
@page "/debugcanvas"
@using Blazor.Extensions
@using Blazor.Extensions.Canvas
@namespace Three2025.Components.Pages
@inherits DebugCanvasBase
@rendermode InteractiveServer

<h2>Debug Canvas 3D</h2>
<Canvas3DComponent SceneName="DebugScene" @ref="Canvas3DReference" CanvasWidth="800" CanvasHeight="600" />
<button class="btn btn-primary" @onclick="AddSimpleBox">Add Box</button>
<button class="btn btn-warning" @onclick="ClearScene">Clear</button>
```

**Code-behind:**
```csharp
using FoundryWorldsAndDrawings.Solutions;
using FoundryMicroCore.Core.Extensions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryWorldsAndDrawings.Shared;

namespace Three2025.Components.Pages;

public partial class DebugCanvasBase : ComponentBase
{
    public Canvas3DComponent Canvas3DReference;
    private FoStage3D _debugStage;
    
    [Inject] public IWorkspace Workspace { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(500);  // Wait for Canvas3D to initialize
            _debugStage = Canvas3DReference?.Stage;
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public void AddSimpleBox()
    {
        if (_debugStage == null) _debugStage = Canvas3DReference?.Stage;
        if (_debugStage == null) return;

        var pipe = new FoPipe3D("TestPipe", "#FF0000")
        {
            Transform = new Transform3("PipeTransform")
            {
                Position = new Vector3(0, 0, 0)
            }
        };
        pipe.CreateTube("TestPipe", 0.2, new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(5, 0, 0),
            new Vector3(5, 5, 0)
        });
        _debugStage.AddShape(pipe);
    }

    public async void ClearScene()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        if (found) await scene.ClearAll();
    }
}
```

### 3B. Per-Frame Animation with Parent-Child Hierarchy (from FoClockFace3D)

```csharp
using FoundryRulesAndUnits.Extensions;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Apprentice;

// A shape that IS its own geometry container AND animates per-frame
public class FoClockFace3D : FoShape3D
{
    public double Radius { get; set; } = 12.0;
    public new double Height { get; set; } = 0.2;
    
    private FoText3D _timeText = null!;
    private FoShape3D _centerPost = null!;
    private FoShape3D _secondHand = null!;
    
    public FoClockFace3D(string name = "ClockFace") : base(name, "Blue")
    {
       BuildClock(); 
    }
    
    protected FoClockFace3D BuildClock()
    {
        // Set OWN geometry
        this.CreateCylinder("ClockFaceBase", 2 * Radius, Height, 2 * Radius);
        
        // Add child text — children position relative to parent
        _timeText = new FoText3D("TimeText", "white")
        {
            Text = "Current Time",
            FontSize = 5.0,
            Transform = new Transform3("TimeTextTransform")
            {
                Position = new Vector3(0, 2, 0),
            }
        };
        this.AddShape(_timeText);   // Parent-child relationship
        
        // Add center post as child
        _centerPost = new FoShape3D("Post", "red")
            .CreateBox("PostBox", 1.2, 1.0, 0.2);
        this.AddShape(_centerPost);
        
        // Add second hand AS CHILD OF centerPost (nested hierarchy!)
        // When centerPost rotates, secondHand rotates with it
        _secondHand = new FoShape3D("Hand", "green")
        {
            Transform = new Transform3("HandTransform")
            {
                Position = new Vector3(0.5 * Radius, 1, 0),
            }
        }.CreateBox("HandBox", 1.2 * Radius, 2.0, 0.1);
        _centerPost.AddShape(_secondHand);
        
        // ✅ KEY PATTERN: Register per-frame animation callback
        // Signature: Action<FoGlyph3D, int, double> = (shape, tick, fps)
        BeforeAnimationRefresh(UpdateClockAnimation);
        
        return this;
    }
    
    private void UpdateClockAnimation(FoGlyph3D self, int tick, double fps)
    {
        var framesPerSecond = (int)Math.Round(fps);
        if (framesPerSecond == 0 || tick % framesPerSecond != 0) return;

        var time = DateTime.Now;
        var angle = time.Second * (2 * Math.PI / 60) - Math.PI / 2;

        // Update text content — Text setter auto-marks stale
        if (_timeText != null)
            _timeText.Text = time.ToString("HH:mm:ss");
        
        // Rotate center post — attached hand follows automatically!
        if (_centerPost != null)
            _centerPost.Transform.RotateTo(0, -angle, 0, AngleUnit.Radians);
    }
}
```

### 3C. Dual 2D+3D Canvas Layout (from DualCanvas2D3DTest)

**Razor markup pattern:**
```razor
@page "/dual-canvas-test"
@using FoundryWorldsAndDrawings.Shape
@using FoundryWorldsAndDrawings.Solutions
@using FoundryWorldsAndDrawings.Shared
@rendermode InteractiveServer
@namespace Three2025.Components.Pages

<h3>Dual Canvas Test</h3>
<div class="row">
    <div class="col-md-5">
        <Canvas2DComponent SceneName="DualTest2D" @ref="Canvas2DReference" />
    </div>
    <div class="col-md-5">
        <Canvas3DComponent SceneName="DualTest3D" @ref="Canvas3DReference" />
    </div>
</div>
```

**Code-behind pattern:**
```csharp
public Canvas2DComponent? Canvas2DReference;
public Canvas3DComponent? Canvas3DReference;

// After first render:
var _page = Canvas2DReference?.Page;    // FoPage2D — the 2D stage equivalent
var _stage = Canvas3DReference?.Stage;  // FoStage3D — the 3D stage
```

### 3D. FoPipe3D Connecting Two Shapes (from GlueTest3D)

```csharp
// Create two boxes
var box1 = new FoShape3D("Box1", "blue");
box1.Transform.Position = new Vector3(-2, 0.5, 0);
box1.CreateBox("Box1", 1.0, 1.0, 1.0);

var box2 = new FoShape3D("Box2", "orange");
box2.Transform.Position = new Vector3(2, 0.5, 0);
box2.CreateBox("Box2", 1.0, 1.0, 1.0);

// Create pipe that connects them
var tube = new FoPipe3D("Tube", "cyan")
{
    FromShape3D = box1,    // Start endpoint
    ToShape3D = box2       // End endpoint
};
tube.CreatePipe("ConnectingTube", 0.1);  // radius 0.1
tube.SetRecomputeBoundary();
tube.SetGeometryStale();  // Force initial geometry computation

// Add ALL to stage
_pageStage.AddShape(box1);    // Body
_pageStage.AddShape(box2);    // Body
_pageStage.AddShape(tube);    // Link (FoPipe3D implements IBodyLink3D)
```

### 3E. FoPipe3D with Explicit Path (Growing Pipe)

```csharp
var growingPipe = new FoPipe3D("GrowingPipe", "red");
var initialPath = new List<Vector3>
{
    new Vector3(10, 0, 10),
    new Vector3(10, 1.0, 10)  // Start at height 1.0
};
growingPipe.CreateTube("GrowingPipe", 0.25, initialPath);
growingPipe.SetGeometryStale();  // Force initial geometry

// Animate it: grow taller each frame
growingPipe.BeforeAnimationRefresh((self, tick, fps) =>
{
    if (fps <= 0 || tick == 0) return;  // Guard against invalid frames
    
    var progress = Math.Min(animationTime / duration, 1.0);
    if (progress < 1.0)
    {
        var newHeight = 1.0 + (progress * 4.0);  // Grow from 1 to 5
        self.Path3D = new List<Vector3>           // Replace path
        {
            new Vector3(10, 0, 10),
            new Vector3(10, newHeight, 10)
        };
        self.SetGeometryStale();  // ← CRITICAL: must mark stale after path change
    }
});

_stage.AddShape(growingPipe);
```

### 3F. 2D Shapes with Tween Animation

```csharp
// Get 2D page from canvas component
var page = Canvas2DReference?.Page ?? drawing.FirstPage();

// Create shapes
var s1 = new FoShape2D(50, 50, "Blue");
s1.MoveTo(300, 300);
var s2 = new FoShape2D(50, 50, "Orange");
s2.MoveTo(500, 300);
page?.AddShape(s1);
page?.AddShape(s2);

// Create connecting wire (1D arrow shape)
var wire = new FoShape1D("Arrow", "Cyan")
{
    Height = 50,
    ShapeDraw = async (ctx, obj) => await DrawArrowAsync(ctx, obj.Width, obj.Height, obj.Color)
};
wire.GlueStartTo(s1, "RIGHT");    // Anchor points: "LEFT", "RIGHT", "TOP", "BOTTOM"
wire.GlueFinishTo(s2, "LEFT");
page?.AddShape(wire);

// Create text
var text = new FoText2D(100, 50, "Green") { Text = "Tug of War!" };
text.MoveTo(400, 400);
page?.AddShape(text);

// Tween animation:
// FoGlyph2D.Animations.Tween<T>(target, {properties}, durationSeconds, delaySeconds)
FoGlyph2D.Animations.Tween<FoShape2D>(s1, new { PinX = s1.PinX - 150 }, 2, 0);
FoGlyph2D.Animations.Tween<FoShape2D>(s2, new { PinX = s2.PinX + 150, PinY = s2.PinY + 50 }, 2, 0)
    .OnComplete(() => {
        text.Text = $"dist: {s1.DistanceBetween(s2):F2}";
    });
```

### 3G. Animation Bus Subscription and FPS Tracking

```csharp
// In OnInitialized (NOT OnAfterRenderAsync):
protected override void OnInitialized()
{
    base.OnInitialized();
    AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
}

// AnimationEvent has: tick (int), fps (double), domain (string)
private void OnAnimationFrame(AnimationEvent animEvent)
{
    _frameCount++;
    if (_frameCount >= 15)  // Update display every 15 frames
    {
        _currentFps = animEvent.fps;
        _currentTick = animEvent.tick;
        _frameCount = 0;
        InvokeAsync(StateHasChanged);  // Update UI
    }
}

// In Dispose:
public void Dispose()
{
    _growingPipe?.ClearAnimationRefresh();
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

### 3H. Stage Retrieval Pattern (with timing guard)

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // ⚠️ CRITICAL: Canvas3DComponent creates its stage in ITS OnAfterRenderAsync.
        // We must wait for it to finish before accessing Stage.
        await Task.Delay(100);

        var (found3D, scene3D) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
        if (found3D)
        {
            _tugOfWarStage = Canvas3DReference.Stage;  // Get pre-created stage
            var linkedScene = _tugOfWarStage?.GetAssociatedScene();
            // Stage is now ready for AddShape() calls
        }
    }
    await base.OnAfterRenderAsync(firstRender);
}
```

### 3I. Immediate Render Push (for Static Shapes)

```csharp
// After adding static shapes that won't animate, push to JS immediately:
var arena = Workspace.GetArena();
await arena.RenderArena(0, 0);
// Without this, shapes exist in C# but aren't visible in the browser
```

---

## 4. Reference Implementation Strategy

### This IS the Reference Implementation
The TugOfWar page already exists as a working implementation. This specification documents **what it does, how it works, and how to reproduce it**. Indy's job is to:
1. **Understand** the patterns via the inline code samples above
2. **Reproduce** them accurately if building from scratch (or modifying)
3. **Extend** them for future demos that follow the same dual-canvas pattern

### Delta from Reference (if building a new page from this template)
- Change route from `/tugofwar` to `/your-route`
- Change `SceneName` parameter on both canvases
- Replace shape creation logic with your domain objects
- Replace animation math with your motion formulas
- Keep the same stage-centric infrastructure pattern

---

## 5. Infrastructure Assumptions

### Assumption: Canvas3DComponent Creates and Manages Its Own Stage
- [x] `Canvas3DComponent` creates a `FoStage3D` in `OnAfterRenderAsync(firstRender)` using `arena.EstablishStage<FoStage3D>(SceneName)`
- [x] It links stage ↔ scene via `scene.LinkToStage(stage)`
- [x] The page code retrieves the stage via `Canvas3DReference.Stage`
- [x] **If broken:** Check the API reference docs for `Canvas3DComponent` or look at how `DebugCanvas.razor.cs` does it

### Assumption: Canvas2DComponent Creates and Manages Its Own Page
- [x] `Canvas2DComponent` creates a `FoPage2D` in its initialization
- [x] The page code retrieves it via `Canvas2DReference.Page`
- [x] Fallback: `drawing.FirstPage()` if `Canvas2DReference.Page` is null
- [x] **If broken:** Check the API reference docs for `Canvas2DComponent`

### Assumption: AnimationFrameBus Delivers Events to All Subscribers
- [x] `SubscribeToAnimation(callback)` in `OnInitialized()` receives `AnimationEvent` with `tick`, `fps`, `domain`
- [x] `BeforeAnimationRefresh(callback)` on shapes fires during the frame loop's shape update phase
- [x] Both systems operate independently — 2D tweens and 3D per-frame callbacks coexist
- [x] **If broken:** Check `FOUNDRY_ANIMATIONS_AND_LIFECYCLE_REFERENCE.md` for the frame loop phases

### Assumption: Stage `AddShape()` Routes Bodies vs Links Automatically
- [x] `FoStage3D.AddShape(T)` inspects whether `T` implements `IBodyLink3D`
- [x] Bodies go to `AllBodies()`, Links (like `FoPipe3D`) go to `AllLinks()`
- [x] **If broken:** Check `FOUNDRY_3D_API_REFERENCE.md` section on FoStage3D

### Assumption: `arena.RenderArena(0, 0)` Forces Immediate JS Push
- [x] Calling this after adding static shapes pushes geometry to JavaScript without waiting for the animation loop
- [x] For animated shapes, the animation loop handles rendering each frame
- [x] **If broken:** Check `FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md` for IArena methods

### Assumption: Task.Delay(100) Required Before Accessing Canvas3DReference
- [x] In `OnAfterRenderAsync(firstRender)`, a 100ms delay is needed for `Canvas3DComponent` to finish ITS `OnAfterRenderAsync`
- [x] Without this, `GetActiveScene()` may return `(false, null)`
- [x] **If broken:** Increase delay or add a retry loop (see `DebugCanvas.razor.cs` which uses 500ms)

---

## 5. Code Path Traces

### When the page loads (Blazor lifecycle)

```
1. OnInitialized()
   └─ AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame)   // Subscribe for FPS/tick
   └─ Log "CIRCUIT ACTIVE"

2. First Render → OnAfterRenderAsync(firstRender: true)
   └─ Task.Delay(100)   // Wait for Canvas3D to initialize
   └─ Canvas3DReference.GetActiveScene()   // Returns (bool, Scene3D)
   └─ If found:
      └─ _tugOfWarStage = Canvas3DReference.Stage   // Get the pre-created stage
      └─ Verify: stage.GetAssociatedScene() should be non-null
   └─ If NOT found: Log warning (Canvas3D not ready)
   └─ NOTE: Does NOT auto-start animation — waits for user button click
```

### When user clicks "Start Tug of War Animation" (3D)

```
1. StartTugOfWar3D(startPaused: false)
   └─ Reset _animationTime = 0
   
2. Create Box1 (FoShape3D, "blue")
   └─ new FoShape3D() with Transform at (-2, 0.5, 0)
   └─ .CreateBox("Box1", 1, 1, 1)           // Fluent: returns shape
   └─ .SetRecomputeBoundary()                // Opt-in for boundary recompute
   └─ .BeforeAnimationRefresh((shape, tick, fps) => {
        // Accumulate _animationTime from fps
        // Calculate progress = animTime / duration
        // Move box1 from x=-2 to x=(-2 - 3) = -5 over duration
        // shape.Transform.Position = new Vector3(x, 0.5, x)
        // shape.SetTransformStale()        // CRITICAL!
        // _tube_3D.SetGeometryStale()      // Update pipe
        // When progress >= 1.0: ClearAnimationRefresh on all shapes
      })
      
3. Create Box2 (FoShape3D, "orange") — same pattern, moves from x=2 rightward

4. Create Tube (FoPipe3D, "cyan")
   └─ Set FromShape3D = _box1_3D, ToShape3D = _box2_3D
   └─ .CreatePipe("ConnectingTube", 0.1)     // radius 0.1
   └─ .SetRecomputeBoundary()
   └─ .SetGeometryStale()                    // Force initial geometry
   
5. Create DistanceText (FoText3D, "white")
   └─ text.PreComputeMesh = (shape) => {
        // Each frame: get world positions of both boxes
        // Calculate distance between them
        // Update text.Text with distance value
      }

6. Create GrowingPipe (FoPipe3D, "red")
   └─ .CreateTube("GrowingPipe", 0.25, initialPath)
   └─ .BeforeAnimationRefresh((self, tick, fps) => {
        // Each frame: calculate new height based on progress
        // Replace Path3D with new two-point path (taller)
        // self.SetGeometryStale()
      })
   
7. Add ALL shapes to stage:
   └─ _tugOfWarStage.AddShape(_box1_3D)     // Body
   └─ _tugOfWarStage.AddShape(_box2_3D)     // Body
   └─ _tugOfWarStage.AddShape(_tube_3D)     // Link (IBodyLink3D)
   └─ _tugOfWarStage.AddShape(_distanceText) // Body
   └─ _tugOfWarStage.AddShape(_growingPipe)  // Link

8. await arena.RenderArena(0, 0)             // Push initial geometry to JS
```

### When the animation frame loop fires

```
For each frame:
  1. PreAnimation phase: models push state
  2. ComputeGeometry phase: shapes evaluate geometry
  3. AnimationEvent(domain=Drawing): 2D canvas renders
  4. AnimationEvent(domain=World): 3D stage renders
     └─ Each shape's BeforeAnimationRefresh() fires
        └─ Shape updates its Transform.Position
        └─ Shape calls SetTransformStale()
        └─ Connected pipes get SetGeometryStale()
     └─ Stage collects stale changes
     └─ JS interop sends updated transforms/geometry to Three.js
  5. Page's OnAnimationFrame(AnimationEvent) fires
     └─ Updates FPS display every 15 frames
     └─ Calls InvokeAsync(StateHasChanged) to refresh UI
```

### When user clicks "Start 2D Tug of War"

```
1. StartTugOfWar2D()
   └─ Get drawing, get page (Canvas2DReference.Page or drawing.FirstPage())
   └─ drawing.ClearAll()
   └─ Create FoShape2D s1 (Blue, 50x50) at (300, 300)
   └─ Create FoShape2D s2 (Orange, 50x50) at (500, 300)
   └─ page.AddShape(s1), page.AddShape(s2)
   └─ Create FoShape1D wire (Cyan arrow) with custom ShapeDraw
   └─ wire.GlueStartTo(s1, "RIGHT"), wire.GlueFinishTo(s2, "LEFT")
   └─ page.AddShape(wire)
   └─ Create FoText2D text (Green) at (400, 400)
   └─ FoGlyph2D.Animations.Tween<FoShape2D>(s1, {PinX: s1.PinX-150}, 2s, 0s delay)
   └─ FoGlyph2D.Animations.Tween<FoShape2D>(s2, {PinX: s2.PinX+150, PinY: s2.PinY+50}, 2s, 0s delay)
      └─ .OnComplete(() => { text.Text = "dist: " + distance })
```

### Disposal flow

```
Dispose()
  └─ _growingPipe?.ClearAnimationRefresh()    // Stop shape callbacks
  └─ AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame)  // Stop bus subscription
  └─ Log "TugOfWar Page Disposed"
  
  NOTE: Canvas3DComponent handles its own DisposeAsync:
    - Removes stage from arena
    - Marks scene inactive and disposed
    - Removes scene from global registry
    - Unsubscribes from animation events
```

---

## 6. Code Smells to Avoid

### From 3D API Reference

#### Don't Hallucinate Method Names
**The API reference explicitly warns:** `SetPosition()`, `ShapeType`, `FontFamily`, `StartPoint`, `EndPoint`, `PipeColor` DO NOT EXIST on 3D shapes.
```csharp
// ❌ WRONG — these methods/properties don't exist
shape.SetPosition(1, 2, 3);
pipe.StartPoint = new Vector3(0, 0, 0);
text.FontFamily = "Arial";

// ✅ CORRECT
shape.Transform.Position = new Vector3(1, 2, 3);
pipe.FromShape3D = startShape;   // OR set Path3D directly
text.FontSize = 0.8;             // FontSize exists, FontFamily does not
```

#### Missing Stale Flags After Transform Changes
```csharp
// ❌ WRONG — position changed but stale flag not set
shape.Transform.Position = new Vector3(x, y, z);
// Shape won't re-render!

// ✅ CORRECT
shape.Transform.Position = new Vector3(x, y, z);
shape.SetTransformStale();  // REQUIRED for change to propagate to JS
```

#### Missing GeometryStale on Dynamic Pipes
```csharp
// ❌ WRONG — path changed but stale flag not set
_growingPipe.Path3D = new List<Vector3> { ... };
// Pipe won't update its mesh!

// ✅ CORRECT
_growingPipe.Path3D = new List<Vector3> { ... };
_growingPipe.SetGeometryStale();  // REQUIRED for geometry recompute
```

### Task-Specific Warnings

#### Double-Counting Animation Time
The current implementation has a subtle issue: `_animationTime` is incremented in BOTH `_box1_3D`'s AND `_box2_3D`'s `BeforeAnimationRefresh` callbacks, which means it accumulates at 2x the real rate. This is a **known pattern from the reference** — the animation still works because `progress` is clamped to 1.0. If you need precise timing, use a separate timer or increment in only one callback.

#### Guard Against Invalid FPS/Tick
```csharp
// ✅ Always guard the first line of BeforeAnimationRefresh
if (fps <= 0 || tick == 0) return;  // Skip manual renders with invalid FPS
```

#### GUIDs in Shape Names for Re-entrancy
The implementation appends partial GUIDs to shape names so that clicking "Start" multiple times doesn't create name collisions:
```csharp
new FoShape3D($"Box1-{Guid.NewGuid().ToString().Substring(0, 8)}", "blue")
```
This is intentional — shapes accumulate when the button is clicked multiple times.

---

## 7. Known Gotchas

### Stage Not Available Until After First Render
- **Symptom:** `_tugOfWarStage` is null when trying to add shapes
- **Cause:** `Canvas3DComponent` creates its stage in `OnAfterRenderAsync(firstRender)`. The page's `OnAfterRenderAsync` runs at roughly the same time.
- **Solution:** `await Task.Delay(100)` before accessing `Canvas3DReference.Stage`
- **Recovery:** The `Add3Boxes` method shows the pattern — check for null, retry `Canvas3DReference?.Stage`

### Shapes Accumulate on Multiple Button Clicks
- **By Design:** The implementation does NOT clear existing shapes before adding new ones
- **Reason:** Tests that shapes from different "runs" can coexist in the same stage
- **If you want clean state:** Call `Reset3D()` before `StartTugOfWar3D()`

### 2D ToggleHitTestRender() Triple-Call Pattern
```csharp
drawing.ToggleHitTestRender();          // Turn off if currently on
if (drawing.ToggleHitTestRender())      // Check state  
{
    drawing.ToggleHitTestRender();      // Turn it off
}
```
This awkward pattern ensures hit-test rendering is OFF regardless of initial state. It's a known smell — if the API gains an explicit `SetHitTestRender(bool)`, use that instead.

### Debug vs Normal Mode Animation Duration
- **Normal mode:** 5.0 seconds (`ANIMATION_DURATION`)
- **Debug mode:** 0.5 seconds (`DEBUG_ANIMATION_DURATION`) — 10 frames at 60fps for visible stepping
- **Triggered by:** `StartTugOfWarPaused()` sets `_debugMode = true`

### `PreComputeMesh` vs `BeforeAnimationRefresh`
Two different hook points:
- `BeforeAnimationRefresh(Action<FoGlyph3D, int, double>)` — fires during animation phase, receives (shape, tick, fps)
- `PreComputeMesh = Action<FoGlyph3D>` — fires during geometry computation phase, receives only (shape)
- The DistanceText uses `PreComputeMesh` because it needs to update text content before mesh generation, not during animation

### `async void` Methods for Button Handlers
Several button handlers (`Add3Boxes`, `AddOneBox`, `StartTugOfWar3D`, `Reset3D`) are `async void` rather than `async Task`. This is intentional for Blazor `@onclick` compatibility, but means exceptions won't propagate — they'll only appear in console.

---

## 8. Troubleshooting Guide

### Shapes Not Appearing After "Add 3 Boxes"

**Symptom:** Click button, console shows shapes added, but nothing visible in 3D canvas.

**Diagnosis Steps:**
1. **Check stage is not null:**
   ```csharp
   $"Stage: {_tugOfWarStage?.Name ?? "NULL"}".WriteInfo();
   ```
2. **Check stage has shapes:**
   ```csharp
   var bodies = _tugOfWarStage.AllBodies();
   $"Bodies: {bodies.Count}".WriteInfo();
   ```
3. **Check scene is linked and active:**
   ```csharp
   var scene = _tugOfWarStage.GetAssociatedScene();
   $"Scene: {scene?.Title ?? "NULL"}, Active={scene?.IsActive}".WriteInfo();
   ```
4. **Check RenderArena was called:**
   Without `await arena.RenderArena(0, 0)` the shapes exist in C# but haven't been pushed to JavaScript.

**Common Causes:**
- Stage is null (Canvas3D not yet initialized)
- Forgot `await arena.RenderArena(0, 0)` after adding static shapes
- Scene not linked to stage (check `Canvas3DComponent.OnAfterRenderAsync`)

---

### Animation Not Running (3D Shapes Static)

**Symptom:** Shapes appear but don't move. FPS counter shows 0 or is not updating.

**Diagnosis Steps:**
1. **Check animation state:**
   ```csharp
   $"State: {AnimationFrameBus.GetAnimationState()}".WriteInfo();
   $"Paused: {AnimationFrameBus.IsGloballyPaused()}".WriteInfo();
   ```
2. **Check BeforeAnimationRefresh is firing:**
   First 10 ticks are logged with 📦 prefix — look for these in console.
3. **Check FPS is valid:**
   If `fps <= 0` the guard clause returns early. Check that the global animation loop started.

**Common Causes:**
- `AnimationFrameBus.IsGloballyPaused()` is true — call `ResumeAllAnimations()`
- `Canvas3DComponent` didn't call `FoundryService.StartGlobalAnimation()` (check its `OnAfterRenderAsync`)
- `BeforeAnimationRefresh` callback wasn't attached before adding shape to stage

---

### 2D Tug of War Shapes Not Visible

**Symptom:** Click "Start 2D Tug of War" but canvas is blank.

**Diagnosis Steps:**
1. **Check drawing exists:**
   ```csharp
   var drawing = Workspace.GetDrawing();
   $"Drawing: {drawing != null}".WriteInfo();
   ```
2. **Check page exists:**
   ```csharp
   var page = Canvas2DReference?.Page;
   $"Page: {page != null}".WriteInfo();
   ```
3. **Check shapes were added:**
   After `page.AddShape(s1)`, verify shape count on page.

**Common Causes:**
- `Canvas2DReference` is null (component not yet rendered)
- Drawing was cleared but page wasn't re-established

---

### Pipe Not Connecting / Not Growing

**Symptom:** Tube/pipe is invisible or doesn't update as boxes move.

**Diagnosis Steps:**
1. **Check pipe's endpoint shapes:**
   ```csharp
   $"From: {_tube_3D.FromShape3D?.Name}, To: {_tube_3D.ToShape3D?.Name}".WriteInfo();
   ```
2. **Check SetGeometryStale was called** after every position change of connected boxes.
3. **For growing pipe:** Check that `Path3D` is being set with new values AND `SetGeometryStale()` follows.

**Common Causes:**
- `FromShape3D` / `ToShape3D` not set before `CreatePipe()`
- Missing `SetGeometryStale()` after `Path3D` update
- Initial `SetGeometryStale()` not called after `CreateTube()`

---

### Debug Stepping Not Working

**Symptom:** "Step" button doesn't advance the animation.

**Diagnosis Steps:**
1. **Check paused state:** `AnimationFrameBus.IsGloballyPaused()` should be `true`
2. **Check TriggerSingleFrame is awaited:**
   ```csharp
   await AnimationFrameBus.TriggerSingleFrame();
   ```

**Common Causes:**
- Animation wasn't paused first (must pause before stepping)
- `TriggerSingleFrame()` not awaited

---

## 9. Step-by-Step Test Sequence

### Test 1: Page Load

**Action:** Navigate to `/tugofwar`

**Expected Results:**
- ✅ Page title "Tug of War Test - 2D vs 3D Animation Comparison" visible
- ✅ Two canvas areas side by side (800x600 each)
- ✅ Control buttons visible: "Start Both Animations", "Reset Both", Debug panel, FPS/Tick display
- ✅ 3D canvas shows default scene (grid floor, ambient lighting)
- ✅ 2D canvas is blank/empty
- ✅ ShapeTreeView panel on right (initially empty or showing scene root)

**Console Output:**
```text
TugOfWar Page OnInitialized - CIRCUIT ACTIVE
TugOfWar Page OnAfterRenderAsync - firstRender=true - INTERACTIVE MODE ACTIVE
Canvas3DComponentBase TugOfWar3D OnAfterRenderAsync - first render
TugOfWar: GetActiveScene found=true, scene=TugOfWar3D, Canvas3DReference=True
TugOfWar: Retrieved TugOfWarStage 'TugOfWar3D' linked to scene 'TugOfWar3D'
```

**If Failed:**
- No canvas → Check `@rendermode @(new InteractiveServerRenderMode(prerender: false))`
- Stage null warning → Canvas3D didn't initialize; increase Task.Delay

---

### Test 2: Add 3 Static Boxes

**Action:** Click "Add 3 Boxes" button

**Expected Results:**
- ✅ Three colored boxes appear in 3D canvas (blue at -3, red at 0, green at 3 on X axis)
- ✅ All boxes at y=0.5, z=3
- ✅ Console shows body count increasing
- ✅ ShapeTreeView updates to show Box1, Box2, Box3

**Console Output:**
```text
SIMPLE TEST: Adding 3 static boxes
Stage 'TugOfWar3D' linked to scene: TugOfWar3D, Scene.IsActive=True
Before adding: Stage has 0 shapes (0 bodies, 0 links)
Adding Box1...
After adding: Stage has 3 bodies, 0 links
  Body: Box1, Stale=True, Type=FoShape3D
  Body: Box2, Stale=True, Type=FoShape3D
  Body: Box3, Stale=True, Type=FoShape3D
```

**If Failed:**
- Boxes not visible → Check `arena.RenderArena(0, 0)` called
- Stage null → Canvas3D race condition; click again

---

### Test 3: Add Individual Boxes

**Action:** Click "Add 1 Box" three times

**Expected Results:**
- ✅ Three cyan boxes appear progressively at x=0, x=1.5, x=3.0 (z=5)
- ✅ Each click adds one more box immediately
- ✅ Previous boxes remain (shapes accumulate)

---

### Test 4: Start 3D Tug of War Animation

**Action:** Click "Start Tug of War Animation"

**Expected Results:**
- ✅ Two boxes appear (blue and orange) with cyan tube connecting them
- ✅ Blue box moves left and diagonally; orange box moves right and upward
- ✅ Cyan connecting tube stretches dynamically as boxes separate
- ✅ Distance text label updates in real-time
- ✅ Red growing pipe (at position 10,0,10) grows vertically from height 1 to 5
- ✅ FPS counter shows ~60fps, tick counter increments
- ✅ Animation completes after ~5 seconds
- ✅ Console shows "All 3D animations completed" when done

**Console Output (first few frames):**
```text
🚀 StartTugOfWar3D called - startPaused=false
🎬 Starting 3D Tug of War Animation (NORMAL mode, 5s duration)
🎯 Adding shapes to TugOfWarStage...
✅ Added Box1 to TugOfWarStage at ...
✅ Added Box2 to TugOfWarStage at ...
✅ Added connecting tube to TugOfWarStage (Type=FoPipe3D, IsIBodyLink3D=True)
✅ Added distance text to TugOfWarStage at ...
📤 Pushing initial geometry to JavaScript...
📦 Box1 BeforeAnimationRefresh: tick=1, fps=60.0, animTime=0.00
📦 Box2 BeforeAnimationRefresh: tick=1, fps=60.0
🔴 GrowingPipe BeforeAnimationRefresh: tick=1, fps=60.0
```

---

### Test 5: Debug Stepping

**Action:** Click "Start PAUSED (for stepping)"

**Expected Results:**
- ✅ Animation state shows "Paused"
- ✅ Geometry appears but doesn't animate
- ✅ Click "⏭ Step" — animation advances one frame
- ✅ Step count increments in debug display
- ✅ Click "▶ Resume" — animation runs to completion

---

### Test 6: Start 2D Tug of War

**Action:** Click "Start 2D Tug of War"

**Expected Results:**
- ✅ Blue square appears at (300, 300)
- ✅ Orange square appears at (500, 300)
- ✅ Cyan arrow connecting them (glued left↔right)
- ✅ "Tug of War!" text at (400, 400)
- ✅ Blue square tweens leftward (-150 px over 2s)
- ✅ Orange square tweens rightward (+150 px) and downward (+50 px)
- ✅ Arrow stretches to follow shapes
- ✅ On completion, text updates to show distance between shapes

---

### Test 7: Both Animations Simultaneously

**Action:** Click "Start Both Animations"

**Expected Results:**
- ✅ 2D and 3D animations run simultaneously
- ✅ FPS remains stable (>30fps)
- ✅ No console errors about cross-canvas interference

---

### Test 8: Reset and Clear

**Action:** Click "Reset Both"

**Expected Results:**
- ✅ 2D canvas clears
- ✅ 3D animations stop, shapes removed
- ✅ Console: "3D scene reset - objects deleted"

**Action:** Click "Clear Scene"

**Expected Results:**
- ✅ All 3D shapes removed from stage
- ✅ Console shows remaining shape count = 0

---

### Verification Checklist

- [ ] Page loads without exceptions
- [ ] 3D canvas initializes with scene
- [ ] Static boxes can be added and appear immediately
- [ ] Animated boxes move over 5 seconds
- [ ] Connecting pipe stretches between moving boxes
- [ ] Growing pipe extends vertically
- [ ] Distance text updates in real-time
- [ ] Debug pause/step/resume works correctly
- [ ] 2D shapes appear and tween correctly
- [ ] Glued arrow follows 2D shapes
- [ ] Both animations can run simultaneously
- [ ] Reset clears all shapes and stops animations
- [ ] FPS stays above 30
- [ ] No console errors during normal operation
- [ ] Dispose cleans up without errors when navigating away

---

## 10. Success Criteria

### Compilation
- [ ] Zero compilation errors
- [ ] Zero compilation warnings (ideally)
- [ ] All using statements resolve
- [ ] All method signatures match Razor bindings

### Runtime (First Load)
- [ ] Page loads without exceptions
- [ ] Canvas3D initializes and shows scene
- [ ] Canvas2D initializes
- [ ] Stage-scene linkage established (confirmed in console)
- [ ] FPS counter starts updating

### Runtime (3D Functionality)
- [ ] Static boxes appear when "Add 3 Boxes" clicked
- [ ] Animated boxes move smoothly over 5 seconds
- [ ] Connecting tube stretches dynamically
- [ ] Distance text updates per-frame
- [ ] Growing pipe extends from height 1 to 5
- [ ] All animations self-terminate after duration
- [ ] Multiple animation runs coexist (shapes accumulate)

### Runtime (2D Functionality)
- [ ] Shapes appear at correct positions
- [ ] Tween animation moves shapes smoothly over 2 seconds
- [ ] Glued arrow follows shapes
- [ ] Completion callback updates text

### Runtime (Debug Controls)
- [ ] Pause stops all animation
- [ ] Step advances exactly one frame
- [ ] Resume continues from paused state
- [ ] Animation state display is accurate

### Performance
- [ ] FPS stable above 30fps during animation
- [ ] No stuttering/janking in 3D rendering
- [ ] 2D tween animation is smooth

### Known Issues (Acceptable)
- [ ] `_animationTime` double-counted in Box1+Box2 callbacks (clamped, doesn't break behavior)
- [ ] `async void` button handlers don't propagate exceptions
- [ ] 2D API reference not yet source-audited (signatures may drift)
- [ ] 100ms Task.Delay is a timing hack (acceptable for now)
- [ ] ToggleHitTestRender triple-call pattern is a workaround

### Disposal
- [ ] No console errors on navigation away
- [ ] Animation callbacks unsubscribed
- [ ] Bus subscription removed
- [ ] Canvas3D handles its own stage/scene cleanup

---

## 11. Confidence Levels

| Area | Confidence | Notes |
|---|---|---|
| Overall Architecture | 🟢 High | Derived from working implementation |
| 3D Shape Creation | 🟢 High | Verified against source-audited API ref |
| `BeforeAnimationRefresh` | 🟢 High | Verified in FoClockFace3D.cs and TugOfWar.cs — working code |
| `SetTransformStale` / `SetGeometryStale` | 🟢 High | Documented in API ref, used extensively in working code |
| FoPipe3D (connecting) | 🟢 High | `FromShape3D`/`ToShape3D` + `CreatePipe` verified |
| FoPipe3D (growing) | 🟢 High | `CreateTube` with `Path3D` replacement verified |
| FoText3D + PreComputeMesh | 🟡 Medium | `PreComputeMesh` is a delegate, not fluent — lifecycle doc suggests it should become `OnPreComputeMesh()`. Current code works. |
| Canvas3D Stage Linkage | 🟢 High | Verified against Canvas3DComponent source code |
| 2D Tween Animation | 🟢 High | `FoGlyph2D.Animations.Tween` verified in multiple files |
| 2D Shape/Wire/Glue | 🟡 Medium | 2D API ref is NOT source-audited per doc warning |
| Debug Step/Pause/Resume | 🟢 High | `AnimationFrameBus` methods verified in API ref |
| Disposal | 🟢 High | Pattern matches Canvas3DComponent's `DisposeAsync` |

---

## 12. Handoff Summary

**Architect:** Claude "Atlas"  
**Date:** February 8, 2026  
**Estimated Implementation Time:** 3–5 hours (from scratch); <1 hour if copying reference  
**Confidence:** 🟢 High overall — this spec documents a WORKING implementation

**What Indy Has Access To:**
- This spec (self-contained with inline code samples)
- Working examples in `Three2025/Apprentice/` and `Three2025/Components/Pages/`
- API reference docs shipping with the libraries (`*_API_REFERENCE.md`)
- The TugOfWar implementation files themselves (TugOfWar.razor + .razor.cs)

**What Indy Does NOT Have:**
- Library source code (FoundryWorldsAndDrawings, FoundryMicroCore, etc.)
- Library versions may differ from when this spec was written

**High Uncertainty Areas:**
1. **Method naming** — `BeforeAnimationRefresh` may have been renamed to `OnBeforeRender`. Use whichever compiles. Check `FoClockFace3D.cs` for what works.
2. **`PreComputeMesh` delegate style** — May need to become `OnPreComputeMesh()` in future refactor
3. **2D API surface** — Not source-audited; if 2D shapes misbehave, check API reference docs
4. **Task.Delay(100) timing** — Works reliably but is a timing hack; if it breaks on slow machines, try 500ms (as `DebugCanvas.razor.cs` does)

**Known Limitations:**
1. `_animationTime` double-counting (acceptable — clamped at 1.0)
2. `async void` handlers (acceptable for Blazor @onclick)
3. No automatic cleanup of accumulated shapes (by design — tests coexistence)

**Verification Checklist (Quick):**
- [ ] Compiles without errors
- [ ] Page loads at `/tugofwar` without exceptions
- [ ] "Add 3 Boxes" shows three colored boxes
- [ ] "Start Tug of War Animation" → boxes move, pipe stretches, pipe grows
- [ ] "Start 2D Tug of War" → shapes tween with arrow
- [ ] Debug pause/step/resume works
- [ ] FPS stable above 30
- [ ] No errors on navigation away

---

## Appendix A: Using Statements Required

```csharp
using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core.Extensions;        // .WriteInfo(), .WriteError(), .WriteSuccess()
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;          // .WriteWarning()
using FoundryRulesAndUnits.Models;              // MockDataGenerator
using FoundryWorldsAndDrawings.Solutions;       // IWorkspace, IFoundryService
using FoundryWorldsAndDrawings.Shape;           // FoShape2D, FoShape1D, FoText2D, FoGlyph2D, FoPage2D
using FoundryWorldsAndDrawings.ThreeD;          // FoShape3D, FoPipe3D, FoText3D, FoStage3D
using FoundryWorldsAndDrawings.Shared;          // Canvas2DComponent, Canvas3DComponent
using FoundryWorldsAndDrawings.ThreeD.Viewers;  // (ViewerThreeD if needed)
using BlazorComponentBus;                       // ComponentBus
using FoundryWorldsAndDrawings.PubSub;          // AnimationFrameBus, AnimationEvent
using FoundryWorldsAndDrawings.ThreeD.Maths;    // Vector3
using FoundryWorldsAndDrawings.ThreeD.Core;     // Transform3
```

## Appendix B: Razor Declaration Pattern

```razor
@page "/tugofwar"
@using FoundryWorldsAndDrawings.Shared
@using FoundryWorldsAndDrawings.Shape

@inherits TugOfWarBase
@rendermode @(new InteractiveServerRenderMode(prerender: false))

@namespace Three2025.Components.Pages
```

**Critical:** `prerender: false` is REQUIRED — interactive Blazor Server pages must not prerender or component references will be null at first render.

## Appendix C: Naming Transition Notes

The lifecycle/animation documentation describes a rename in progress:

| Old Name (Currently In Source) | New Name (Documented Target) |
|---|---|
| `BeforeAnimationRefresh()` | `OnBeforeRender()` |
| `BeforeAnimationRefreshNOOP()` | `OnBeforeRenderOff()` |
| `ClearAnimationRefresh()` | `ClearBeforeRender()` |

**Guidance for Indy:** Use whichever name **compiles**. The existing TugOfWar code uses the OLD names. If the library has been updated since this spec was written, try the NEW names. The signatures are identical — only the method name changes.
