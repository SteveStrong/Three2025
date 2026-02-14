# Clock Razor Page - Animation Architecture Explained

## Document Purpose

This document explains **how the Clock.razor page animation system works** from the ground up, specifically focusing on:

1. How the **global animation loop** is automatically initialized by the base libraries
2. How your Razor page **taps into** this pre-existing animation system
3. The **exact execution flow** from JavaScript → C# → Your Page
4. **Critical patterns** you must follow to make animations work

**Context**: The Clock page demonstrates a 3D scene with animated models (T-Rex, submarine, rotating boxes). Understanding how it works will help you fix similar pages in TRISoC_Dashboard.

---

## Table of Contents

1. [Animation System Overview](#animation-system-overview)
2. [How the Global Animation Loop Initializes](#how-the-global-animation-loop-initializes)
3. [Clock Page Implementation Pattern](#clock-page-implementation-pattern)
4. [Execution Flow Diagram](#execution-flow-diagram)
5. [Critical Code Patterns](#critical-code-patterns)
6. [Common Mistakes to Avoid](#common-mistakes-to-avoid)
7. [Debugging Tips](#debugging-tips)

---

## Animation System Overview

### The Big Picture

The FoundryWorldsAndDrawings library has a **unified global animation system** that runs automatically. Your page doesn't start or manage the animation loop - it just **subscribes to events** from the already-running loop.

```
┌────────────────────────────────────────────────────────────┐
│ JavaScript (browser's requestAnimationFrame)               │
│  - Runs at ~60 FPS                                         │
│  - Calls C# method every frame                             │
└─────────────────────────┬──────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│ FoundryService.TriggerAnimationFrame() [C#]                │
│  - Receives frame tick from JavaScript                     │
│  - Publishes events to all subscribers                     │
└─────────────────────────┬──────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│ AnimationFrameBus (static event bus)                       │
│  - PreAnimationEvent (geometry updates)                    │
│  - AnimationEvent (rendering/UI updates)                   │
└─────────────────────────┬──────────────────────────────────┘
                          ↓
┌────────────────────────────────────────────────────────────┐
│ Your Page (Clock.razor.cs)                                 │
│  - Subscribes: AnimationFrameBus.SubscribeToAnimation()    │
│  - Receives: OnAnimationFrame(AnimationEvent evt)          │
│  - Updates: _currentFps, _currentTick, UI state            │
└────────────────────────────────────────────────────────────┘
```

**Key Insight**: The animation loop is **NOT page-specific**. It's a **global singleton** that runs as long as any canvas exists. Your page just listens to it.

---

## How the Global Animation Loop Initializes

### Step 1: Canvas3DComponent Auto-Initialization

When you add `<Canvas3DComponent>` to your Razor page, it **automatically** initializes the animation system:

```csharp
// Canvas3DComponent.razor.cs - OnAfterRenderAsync
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // 1. Create Scene3D and FoStage3D
        var (found, scene) = GetActiveScene();
        
        // 2. Subscribe to AnimationFrameBus
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        
        // 3. Start global animation system
        await DoStart();  // ← THIS STARTS THE ANIMATION LOOP
    }
}
```

### Step 2: FoundryService.StartGlobalAnimation()

```csharp
// FoundryService.cs
public async Task StartGlobalAnimation()
{
    try
    {
        // Initialize DotNet callback reference
        await InitializeAnimationManager();
        
        // Call JavaScript to start requestAnimationFrame loop
        await js.InvokeVoidAsync("FoundryApp.StartGlobalAnimation");
        
        "FoundryService: Global animation started".WriteSuccess();
    }
    catch (Exception ex) { /* ... */ }
}
```

**What This Does**:
1. Registers a C# callback with JavaScript (`TriggerAnimationFrame`)
2. Starts JavaScript `requestAnimationFrame` loop
3. JavaScript calls back to C# every frame (~60 FPS)

### Step 3: JavaScript Animation Loop

```javascript
// app-lib.js (simplified)
class FoundryApp {
    StartGlobalAnimation() {
        this.animationRunning = true;
        this.animate();
    }
    
    animate() {
        if (!this.animationRunning) return;
        
        // Call C# callback
        this.dotNetHelper.invokeMethodAsync('TriggerAnimationFrame');
        
        // Request next frame
        requestAnimationFrame(() => this.animate());
    }
}
```

### Step 4: C# Receives Animation Frame

```csharp
// FoundryService.cs
[JSInvokable]
public async Task TriggerAnimationFrame()
{
    // Publish events to ALL subscribers (all pages, all canvases)
    await AnimationFrameBus.TriggerBothAnimationFrames();
}
```

```csharp
// AnimationFrameBus.cs
public static async Task TriggerBothAnimationFrames()
{
    // Calculate FPS and increment tick
    var now = DateTime.Now;
    _currentFps = 1.0 / (now - _lastRender).TotalSeconds;
    _lastRender = now;
    ++_globalTick;
    
    // Publish PreAnimationEvent (for geometry updates)
    _preAnimationEvent.fps = _currentFps;
    _preAnimationEvent.tick = _globalTick;
    _preAnimationEvent.prerenderType = PrerenderType.Drawing;
    await _bus.Publish<PreAnimationEvent>(_preAnimationEvent);
    
    _preAnimationEvent.prerenderType = PrerenderType.World;
    await _bus.Publish<PreAnimationEvent>(_preAnimationEvent);
    
    // Publish AnimationEvent (for rendering/UI updates)
    _animationEvent.fps = _currentFps;
    _animationEvent.tick = _globalTick;
    _animationEvent.prerenderType = PrerenderType.Drawing;
    await _bus.Publish<AnimationEvent>(_animationEvent);
    
    _animationEvent.prerenderType = PrerenderType.World;
    await _bus.Publish<AnimationEvent>(_animationEvent);
}
```

**Critical Point**: This publishes to **ALL subscribers**. If you have 3 pages open, all 3 receive the same event at the same tick.

---

## Clock Page Implementation Pattern

### Complete Code Flow

Here's the **exact pattern** the Clock page uses:

#### 1. Razor Markup (Clock.razor)

```razor
@page "/clock"
@inherits ClockBase
@rendermode InteractiveServer

<PageTitle>Clock</PageTitle>

<!-- FPS/Tick Display -->
<div style="padding: 10px;">
    <strong>FPS:</strong> @_currentFps.ToString("F1") 
    <strong>Tick:</strong> @_currentTick
</div>

<!-- 3D Canvas -->
<Canvas3DComponent SceneName="Clock3D" @ref="Canvas3DReference" 
                   CanvasWidth="1000" CanvasHeight="800" />
```

**Key Points**:
- `@ref="Canvas3DReference"` - Get reference to canvas (optional, but useful)
- `SceneName="Clock3D"` - Unique name for this scene
- `@rendermode InteractiveServer` - **REQUIRED** for animations

#### 2. Code-Behind Initialization (Clock.razor.cs)

```csharp
public partial class ClockBase : ComponentBase, IDisposable
{
    [Inject] public IWorkspace Workspace { get; init; }
    
    public Canvas3DComponent Canvas3DReference = null;
    
    protected double _currentFps = 0;
    protected int _currentTick = 0;
    
    protected override void OnInitialized()
    {
        var arena = Workspace.GetArena();
        arena.ClearArena();  // Clear any previous page's objects
        
        // ✅ SUBSCRIBE TO ANIMATION EVENTS
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
        
        base.OnInitialized();
    }
}
```

**Critical Pattern**: 
- Subscribe in `OnInitialized()` - **NOT** `OnAfterRenderAsync`
- The animation loop is already running (started by Canvas3DComponent)
- You're just **listening** to it

#### 3. Animation Event Handler

```csharp
private void OnAnimationFrame(AnimationEvent animEvent)
{
    // Filter for 3D events only (ignore 2D canvas events)
    if (animEvent.IsWorld3D())
    {
        // Update state
        _currentFps = animEvent.fps;
        _currentTick = animEvent.tick;
        
        // Update UI (async to avoid blocking animation thread)
        InvokeAsync(StateHasChanged);
    }
}
```

**Critical Patterns**:
- ✅ Use `animEvent.IsWorld3D()` to filter events
- ✅ Use `InvokeAsync(StateHasChanged)` for UI updates
- ❌ DON'T call `StateHasChanged()` directly (can cause threading issues)

#### 4. Adding Objects to Scene

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // ✅ WAIT for canvas initialization
        await Task.Delay(500);
        
        // ✅ ADD OBJECTS TO ARENA (not scene!)
        var arena = Workspace.GetArena();
        
        var model = new FoModel3D("T-Rex")
        {
            Url = GetReferenceTo(@"storage/staticfiles/T_Rex.glb"),
            Transform = new Transform3("Trans") 
            { 
                Position = new Vector3(0, 5, 0) 
            }
        };
        
        // ✅ SET ANIMATION CALLBACK
        model.BeforeAnimationRefresh((self, tick, fps) =>
        {
            // Animate object every frame
            var pos = self.Transform.MoveBy(0, 0, 0.5);
            self.SetTransformStale();  // Mark for JavaScript update
        });
        
        // ✅ ADD TO ARENA
        arena.AddShapeToStage(model);
    }
}
```

**Critical Patterns**:
- ✅ `await Task.Delay(500)` - Wait for canvas ready
- ✅ Use `FoModel3D` (NOT `Model3D`)
- ✅ Use `arena.AddShapeToStage()` (NOT `scene.AddChild()`)
- ✅ Set animation callback with `BeforeAnimationRefresh()`
- ✅ Call `SetTransformStale()` after transform changes

#### 5. Disposal

```csharp
public void Dispose()
{
    var arena = Workspace.GetArena();
    arena.ClearArena();
    
    // ✅ UNSUBSCRIBE FROM ANIMATION
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

**Critical Pattern**: Always unsubscribe to prevent memory leaks.

---

## Execution Flow Diagram

### Complete Frame-by-Frame Flow

```
TIME: Frame N (e.g., tick=1234, t=0.0167s @ 60 FPS)

┌─────────────────────────────────────────────────────────────┐
│ 1. JavaScript requestAnimationFrame                         │
│    - Browser calls animate() at 60 FPS                      │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. JavaScript → C# Callback                                 │
│    dotNetHelper.invokeMethodAsync('TriggerAnimationFrame')  │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. FoundryService.TriggerAnimationFrame()                   │
│    - Receives frame notification from JavaScript            │
│    - Calls AnimationFrameBus.TriggerBothAnimationFrames()   │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. AnimationFrameBus.TriggerBothAnimationFrames()           │
│    - Calculate FPS: 1.0 / (now - _lastRender).TotalSeconds  │
│    - Increment tick: ++_globalTick                          │
│    - Update event properties:                               │
│      _preAnimationEvent.fps = 60.2                          │
│      _preAnimationEvent.tick = 1234                         │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. PUBLISH: PreAnimationEvent (Drawing)                     │
│    await _bus.Publish<PreAnimationEvent>()                  │
│    → 2D objects update geometry                             │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 6. PUBLISH: PreAnimationEvent (World)                       │
│    await _bus.Publish<PreAnimationEvent>()                  │
│    → FoModel3D.BeforeAnimationRefresh() callbacks fire      │
│    → T-Rex moves: pos.Z += 0.5                              │
│    → SetTransformStale() queues object for JavaScript update│
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 7. PUBLISH: AnimationEvent (Drawing)                        │
│    await _bus.Publish<AnimationEvent>()                     │
│    → Canvas2DComponent.OnAnimationEvent() fires             │
│    → 2D canvas renders updated shapes                       │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 8. PUBLISH: AnimationEvent (World)                          │
│    await _bus.Publish<AnimationEvent>()                     │
│    → Clock.OnAnimationFrame() fires ✅                       │
│    → _currentFps = 60.2                                     │
│    → _currentTick = 1234                                    │
│    → InvokeAsync(StateHasChanged) ← UI updates              │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│ 9. Canvas3DComponent.OnAnimationEvent() fires               │
│    → Processes stale object queue                           │
│    → Sends transform updates to JavaScript                  │
│    → Three.js updates mesh positions                        │
│    → Browser renders frame                                  │
└─────────────────────────────────────────────────────────────┘

TIME: Frame N+1 (tick=1235, t=0.0167s later)
[Loop repeats at 60 FPS...]
```

---

## Critical Code Patterns

### Pattern 1: Page Subscription (REQUIRED)

```csharp
// ✅ CORRECT - Subscribe in OnInitialized
protected override void OnInitialized()
{
    AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
    base.OnInitialized();
}

private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _currentTick = evt.tick;
        _currentFps = evt.fps;
        InvokeAsync(StateHasChanged);
    }
}

public void Dispose()
{
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

```csharp
// ❌ WRONG - Don't subscribe in OnAfterRenderAsync
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // ❌ TOO LATE - animation already started
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
    }
}
```

### Pattern 2: Object-Level Animation (BeforeAnimationRefresh)

```csharp
// ✅ CORRECT - Object animates itself
var model = new FoModel3D("AnimatedModel")
{
    Url = GetReferenceTo(@"storage/staticfiles/model.glb"),
    Transform = new Transform3("Trans") { Position = new Vector3(0, 0, 0) }
};

model.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Runs every PreAnimationEvent (World) - BEFORE rendering
    self.Transform.Position = new Vector3(
        Math.Sin(tick * 0.01) * 10,
        0,
        Math.Cos(tick * 0.01) * 10
    );
    self.SetTransformStale();  // ✅ Required!
});

arena.AddShapeToStage(model);
```

```csharp
// ❌ WRONG - Using deprecated API
var model = new Model3D() { /* ... */ };  // ❌ Wrong type!
scene.AddChild(model);  // ❌ Wrong method!
model.SetAnimationUpdate(...);  // ❌ Doesn't exist on Model3D!
```

### Pattern 3: Adding Objects to Scene

```csharp
// ✅ CORRECT - Use arena.AddShapeToStage()
var arena = Workspace.GetArena();
var model = new FoModel3D("MyModel") { /* ... */ };
arena.AddShapeToStage(model);
// → Automatically adds to current stage
// → Links to scene automatically
// → Publishes RefreshUIEvent
// → Queues for JavaScript update
```

```csharp
// ❌ WRONG - Direct scene manipulation
var (found, scene) = arena.CurrentScene();
if (found)
{
    var model = new Model3D() { /* ... */ };  // ❌ Wrong type
    scene.AddChild(model);  // ❌ No tracking, no events
}
```

### Pattern 4: UI Updates in Animation Callback

```csharp
// ✅ CORRECT - Use InvokeAsync
private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _currentFps = evt.fps;
        _currentTick = evt.tick;
        
        InvokeAsync(StateHasChanged);  // ✅ Thread-safe UI update
    }
}
```

```csharp
// ❌ WRONG - Direct StateHasChanged
private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        StateHasChanged();  // ❌ Can cause threading issues
    }
}
```

### Pattern 5: Canvas Initialization Delay

```csharp
// ✅ CORRECT - Wait for canvas ready
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(500);  // ✅ Wait for canvas initialization
        
        var model = new FoModel3D("Model") { /* ... */ };
        arena.AddShapeToStage(model);
    }
}
```

```csharp
// ❌ WRONG - Immediate object creation
protected override void OnInitialized()
{
    // ❌ Canvas not ready yet!
    var model = new FoModel3D("Model") { /* ... */ };
    arena.AddShapeToStage(model);
}
```

---

## Common Mistakes to Avoid

### Mistake 1: Using Raw Model3D Instead of FoModel3D

```csharp
// ❌ WRONG - No animation support
var model = new Model3D()
{
    Url = url,
    Transform = new Transform3("Trans")
};
scene.AddChild(model);
// model.BeforeAnimationRefresh(...);  ❌ Method doesn't exist!
```

**Why It Fails**: `Model3D` is a low-level DTO with no methods. It can't receive animation callbacks.

**Fix**: Use `FoModel3D` wrapper:
```csharp
// ✅ CORRECT
var model = new FoModel3D("MyModel")
{
    Url = url,
    Transform = new Transform3("Trans")
};
model.BeforeAnimationRefresh((self, tick, fps) => { /* ... */ });
arena.AddShapeToStage(model);
```

### Mistake 2: Not Subscribing to AnimationFrameBus

```csharp
// ❌ WRONG - Never subscribes
protected override void OnInitialized()
{
    // Missing subscription!
}

private void OnAnimationFrame(AnimationEvent evt)
{
    // This NEVER fires!
    _currentFps = evt.fps;
}
```

**Why It Fails**: Without subscribing, your callback never gets called.

**Fix**:
```csharp
// ✅ CORRECT
protected override void OnInitialized()
{
    AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
}

public void Dispose()
{
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

### Mistake 3: Forgetting to Call SetTransformStale()

```csharp
// ❌ WRONG - Changes not visible in scene
model.BeforeAnimationRefresh((self, tick, fps) =>
{
    self.Transform.Position = new Vector3(x, y, z);
    // Missing: self.SetTransformStale();  ❌
});
```

**Why It Fails**: JavaScript doesn't know the transform changed. The mesh stays at the old position.

**Fix**:
```csharp
// ✅ CORRECT
model.BeforeAnimationRefresh((self, tick, fps) =>
{
    self.Transform.Position = new Vector3(x, y, z);
    self.SetTransformStale();  // ✅ Queues JavaScript update
});
```

### Mistake 4: Using scene.AddChild() Instead of arena.AddShapeToStage()

```csharp
// ❌ WRONG - Bypasses tracking system
var (found, scene) = arena.CurrentScene();
if (found)
{
    scene.AddChild(model);  // ❌ No tracking, no events
}
```

**Why It Fails**:
- No `RefreshUIEvent` published (UI doesn't update)
- Not tracked in ShapeTreeView
- No automatic dirty flag queuing

**Fix**:
```csharp
// ✅ CORRECT
arena.AddShapeToStage(model);
// → Automatic tracking
// → RefreshUIEvent published
// → Appears in ShapeTreeView
```

### Mistake 5: Not Filtering Events by Type

```csharp
// ❌ WRONG - Processes ALL events (2D and 3D)
private void OnAnimationFrame(AnimationEvent evt)
{
    // Fires for BOTH 2D and 3D events!
    _currentFps = evt.fps;  // ❌ Will update twice per frame
    InvokeAsync(StateHasChanged);
}
```

**Why It Fails**: You get called 4 times per frame (2 PreAnimation + 2 Animation), causing unnecessary UI updates.

**Fix**:
```csharp
// ✅ CORRECT - Filter by event type
private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())  // ✅ Only process 3D events
    {
        _currentFps = evt.fps;
        InvokeAsync(StateHasChanged);
    }
}
```

---

## Debugging Tips

### Debug Pattern 1: Verify Animation Loop is Running

```csharp
private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        // ✅ Add logging every 60 frames
        if (evt.tick % 60 == 0)
        {
            $"Clock: Animation frame {evt.tick} at {evt.fps:F1} FPS".WriteInfo();
        }
        
        _currentFps = evt.fps;
        _currentTick = evt.tick;
        InvokeAsync(StateHasChanged);
    }
}
```

**Expected Output** (console):
```
Clock: Animation frame 60 at 60.2 FPS
Clock: Animation frame 120 at 59.8 FPS
Clock: Animation frame 180 at 60.1 FPS
```

### Debug Pattern 2: Verify Object Animation Callback

```csharp
model.BeforeAnimationRefresh((self, tick, fps) =>
{
    // ✅ Log every 60 frames
    if (tick % 60 == 0)
    {
        var pos = self.Transform.Position;
        $"Model {self.Name} at position {pos.X:F2}, {pos.Y:F2}, {pos.Z:F2}".WriteInfo();
    }
    
    self.Transform.Position = new Vector3(x, y, z);
    self.SetTransformStale();
});
```

**Expected Output**:
```
Model T-Rex at position 0.00, 5.00, 30.50
Model T-Rex at position 0.00, 5.00, 60.75
Model T-Rex at position 0.00, 5.00, 91.00
```

### Debug Pattern 3: Check if Canvas Initialized

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        $"Canvas3DReference is {(Canvas3DReference == null ? "NULL" : "valid")}".WriteInfo();
        
        await Task.Delay(500);
        
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        $"Scene found: {found}, Scene name: {scene?.Name ?? "NULL"}".WriteInfo();
        
        if (found)
        {
            var arena = Workspace.GetArena();
            var stage = arena.CurrentStage();
            $"Current stage: {stage?.Key ?? "NULL"}".WriteInfo();
        }
    }
}
```

**Expected Output**:
```
Canvas3DReference is valid
Scene found: True, Scene name: Clock3D
Current stage: Clock3D
```

### Debug Pattern 4: Verify Subscription

```csharp
protected override void OnInitialized()
{
    "Clock: Subscribing to AnimationEvent".WriteSuccess();
    AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
}

private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D() && evt.tick == 1)
    {
        "Clock: FIRST animation frame received!".WriteSuccess();
    }
}

public void Dispose()
{
    "Clock: Unsubscribing from AnimationEvent".WriteWarning();
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

**Expected Output**:
```
Clock: Subscribing to AnimationEvent
Clock: FIRST animation frame received!
[Navigate away...]
Clock: Unsubscribing from AnimationEvent
```

---

## Summary: How to Make Your Clock Work

### Checklist for Success

1. **✅ Use Correct Object Types**
   - `FoModel3D` (NOT `Model3D`)
   - `FoShape3D` (NOT `Mesh3D`)
   - `FoGroup3D` (NOT `Group3D`)

2. **✅ Subscribe to Animation Events**
   - Call `AnimationFrameBus.SubscribeToAnimation()` in `OnInitialized()`
   - Implement `OnAnimationFrame(AnimationEvent evt)` handler
   - Filter with `evt.IsWorld3D()`
   - Unsubscribe in `Dispose()`

3. **✅ Add Objects Properly**
   - Use `arena.AddShapeToStage()` (NOT `scene.AddChild()`)
   - Wait for canvas ready (`await Task.Delay(500)`)
   - Add in `OnAfterRenderAsync(firstRender)`

4. **✅ Set Up Object Animation**
   - Use `model.BeforeAnimationRefresh((self, tick, fps) => { ... })`
   - Call `self.SetTransformStale()` after transform changes
   - Use property assignment (NOT mutation of copies)

5. **✅ Update UI Safely**
   - Use `InvokeAsync(StateHasChanged)` in animation callback
   - Don't call `StateHasChanged()` directly

### The Golden Rule

**You don't START the animation loop - you LISTEN to it.**

The animation loop is started automatically by Canvas3DComponent when it renders. Your job is to:
1. Subscribe to events (`AnimationFrameBus.SubscribeToAnimation`)
2. Add objects to the arena (`arena.AddShapeToStage`)
3. Set animation callbacks (`model.BeforeAnimationRefresh`)
4. Update UI in response to events (`InvokeAsync(StateHasChanged)`)

That's it! The library handles everything else.

---

**Document Version**: 1.0  
**Created**: 2025-11-23  
**Author**: GitHub Copilot  
**For**: TRISoC_Dashboard migration assistance  
**Working Examples**: Clock.razor.cs, MultiCanvas2DTest.razor.cs
