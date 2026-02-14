# KN↔FO Animation Integration Architecture

## Overview

This document captures the architectural refinements for integrating Knowledge objects (KN layer) with Foundry objects (FO layer) during animation loops. The goal is a tight, efficient animation system where parameter changes in the model layer flow through to geometry updates in the rendering layer.

## Layer Separation

- **KN Layer (Knowledge/Model)**: `KnComponent`, `KnModel`, `KnParameter`, `KnGeometry`
  - Handles data, parameters, dependencies, and business logic
  - Lives in `FoundryMentorModeler`

- **FO Layer (Foundry/Rendering)**: `FoShape3D`, `FoStage3D`, `FoArena3D`, `FoText3D`
  - Handles 3D rendering, transforms, and visual representation
  - Lives in `FoundryWorldsAndDrawings`

## Complete Animation Cycle (CRITICAL PATTERN)

### The Three-Phase Cycle

```
PHASE 1: PreAnimationEvent
  → MentorServices.OnPreAnimationEvent()
    → model.OnPreAnimationEvent(evt)
      → PreContextLink?.Invoke() [model's callback - parameter updates]
      → Automatic propagation to Subcomponents<KnComponent>()
        → Each child.OnPreAnimationEvent()
          → PreContextLink?.Invoke() [component's callback]
          → Parameter changes trigger Smash cascade

PHASE 2: AnimationEvent (REQUIRED IN PAGE)
  → Page.OnAnimationEvent(evt)
    → model.RenderGeometry3D(ctx)
      → Walks component tree recursively
      → Calls EstablishGeometry3D on each component
      → Evaluates geometry parameters (if Unknown)
      → Adds shapes to stage via PostCreation

PHASE 3: AnimationEvent (Canvas Rendering)
  → Canvas3DComponent.OnAnimationEvent(evt)
    → RenderFrame()
      → stage.RenderStage(tick, fps)
        → Sends shapes to JavaScript/Three.js
```

### ⚠️ CRITICAL: Page Must Subscribe to AnimationEvent

**Without this subscription, parameter changes trigger Smash but geometry never re-evaluates!**

```csharp
public partial class MyPage : ComponentBase, IDisposable
{
    private AnimatedKnModel? _model;
    private FoStage3D? _stage;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // REQUIRED: Subscribe to animation events
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        
        // Optional: Subscribe for tree refresh
        MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(OnModelEditChanged);
        
        // Create model
        _model = MentorServices.EstablishModel<AnimatedKnModel>("MyModel");
        _model.SetExpanded(true);
    }

    // CRITICAL: This method must call RenderGeometry3D
    private void OnAnimationEvent(AnimationEvent evt)
    {
        if (_stage != null && _model != null && evt.IsWorld3D())
        {
            var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
            _model.RenderGeometry3D(ctx);  // ← THIS IS REQUIRED
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(200); // Wait for canvas init
            
            _stage = Canvas3DReference?.Stage;
            if (_stage != null)
            {
                // Establish initial geometry
                var view = _stage.GetName();
                var (geometry, parameter) = ModelEditor.EstablishGeometry3D(_component, view);
                
                // Start animations
                AnimationFrameBus.ResumeAllAnimations();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
        MentorServices?.PubSub?.UnSubscribeFrom<ModelEditChanged>(OnModelEditChanged);
    }
}
```

## Key Principles Discovered

### 1. Framework Handles PreAnimation Propagation
The base `KnComponent.OnPreAnimationEvent()` automatically propagates to all children:
```csharp
public virtual void OnPreAnimationEvent(PreAnimationEvent evt)
{
    PreContextLink?.Invoke(this, evt);
    foreach (var child in Subcomponents<KnComponent>().ToList())
    {
        child.OnPreAnimationEvent(evt);
    }
}
```
**No manual iteration needed in model classes.**

### 2. Page Must Drive Geometry Rendering
`MentorServices.OnComputeGeometryEvent()` is now a NOOP - geometry evaluation is pull-based. **The page MUST subscribe to AnimationEvent and call `model.RenderGeometry3D(ctx)` each frame.**

### 3. Use PreAnimationRefresh for Setup
Both models and components set up their animation callbacks via `PreAnimationRefresh()`:
```csharp
// In constructor
PreAnimationRefresh((comp, evt) =>
{
    // Animation logic here - update parameters
    ModelEditor.SetParameter(this, "Width", $"{newWidth}");
});
```

### 4. UI Refresh via PubSub
`MentorTreeView` subscribes to `RefreshRenderMessage` and calls `StateHasChanged` automatically. Subscribe to `ModelEditChanged` in your page for tree updates.

### 5. Subshapes Use Local Names
When creating child shapes (like labels), use simple names - uniqueness comes from hierarchy:
```csharp
var label3D = new FoText3D("Label", "white")  // Not "{name}_Label"
shape.AddSubGlyph3D(label3D);  // Parent-child relationship provides uniqueness
```

## Current Implementation

### AnimatedKnModel
- Sets up `PreAnimationRefresh` for model-level work
- Updates `Param1` with tick count every 60 frames (visible in tree)
- Logs receipt of animation events
- Children notified automatically by framework

### AnimatedKnComponent  
- Sets up own `PreAnimationRefresh` via `SetupAnimation()`
- Will handle parameter updates that drive geometry changes
- Geometry created via `EstablishGeometry3D` → `ComputeShape3D` pattern

## Planned: Parameter-Driven Geometry Animation

### Goal
Change geometry type (Box → Sphere → Cylinder) when shape reaches bottom of sinusoidal bounce.

### Flow
```
PreAnimationRefresh callback
  → Calculate sinusoidal position
  → Detect trough (bottom of wave)
  → Update "GeometryType" parameter
  → Dependency system invalidates Geometry
  → ComputeShape3D recalculates with new type
  → New mesh renders
```

### Parameters Involved
- `AnimationOffset` - drives Y position offset
- `GeometryType` - "Box", "Sphere", "Cylinder"
- `PositionX` - used for phase offset calculation

## Animation Callback Patterns

### On FoShape3D (Direct Transform Animation)
```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    if (self is FoShape3D s && s.Transform != null)
    {
        var newY = baseY + amplitude * Math.Sin(frequency * tick + phase);
        s.Transform.MoveTo(baseX, newY, baseZ);
    }
});
```

### On KnComponent (Parameter-Driven Animation)
```csharp
PreAnimationRefresh((comp, evt) =>
{
    // Update parameters - dependency system handles the rest
    UpdateParameter("AnimationOffset", newOffset, "m");
});
```

## Files Involved

- `Three2025/Components/Pages/GeometryDebugTest.razor.cs` - **Reference implementation**
- `Three2025/Components/Pages/KnModel/AnimatedKnModel.cs`
- `Three2025/Components/Pages/KnModel/DebugGeometryComponent.cs`
- `FoundryMentorModeler/Mentor/KnComponent.cs` (base OnPreAnimationEvent)
- `FoundryMentorModeler/Mentor/MentorServices.cs` (PreAnimationEvent subscription)
- `FoundryWorldsAndDrawings/Solutions/AnimationFrameBus.cs` (event publishing)
- `FoundryWorldsAndDrawings/Shared/Canvas3DComponent.razor.cs` (automatic RenderStage)

## Reference Implementation

See `GeometryDebugTest.razor.cs` for the complete working pattern:
- AnimationEvent subscription in OnInitialized
- OnAnimationEvent calling RenderGeometry3D
- ModelEditChanged subscription for tree updates
- Proper Dispose cleanup

---
*Last updated: December 21, 2025*
