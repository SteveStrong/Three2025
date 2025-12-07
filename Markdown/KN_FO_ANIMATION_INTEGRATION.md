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

## Animation Event Flow

### Before (Overcomplicated)
```
PreAnimationEvent
  → Model manually iterates Members<KnComponent>()
  → Model calls component.UpdateFromAnimation(tick, fps)
  → Redundant propagation
```

### After (Simplified)
```
PreAnimationEvent
  → MentorServices.OnPreAnimationEvent()
  → model.OnPreAnimationEvent(evt)
    → PreContextLink?.Invoke() [model's callback]
    → Automatic propagation to Subcomponents<KnComponent>()
      → Each child's OnPreAnimationEvent()
        → PreContextLink?.Invoke() [component's callback]
```

## Key Principles Discovered

### 1. Framework Handles Propagation
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

### 2. Use PreAnimationRefresh for Setup
Both models and components set up their animation callbacks via `PreAnimationRefresh()`:
```csharp
// In constructor
PreAnimationRefresh((comp, evt) =>
{
    // Animation logic here
});
```

### 3. UI Refresh via PubSub
`MentorTreeView` subscribes to `RefreshRenderMessage` and calls `StateHasChanged` automatically. No need for manual `_onRefresh` callbacks from models.

### 4. Subshapes Use Local Names
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

- `Three2025/Components/Pages/KnModel/AnimatedKnModel.cs`
- `Three2025/Components/Pages/KnModel/AnimatedKnComponent.cs`
- `Three2025/Components/Pages/KnModelAnimationTest.razor.cs`
- `FoundryMentorModeler/Mentor/KnComponent.cs` (base OnPreAnimationEvent)
- `FoundryMentorModeler/Mentor/MentorServices.cs` (PreAnimationEvent subscription)
- `FoundryWorldsAndDrawings/Solutions/AnimationFrameBus.cs` (event publishing)

## Next Steps

1. Verify animation events are reaching components (check logs)
2. Implement parameter updates in component's PreAnimationRefresh
3. Wire dependency system: parameter change → geometry invalidation
4. Test geometry type cycling at wave trough
5. Remove direct Transform animation, rely on parameter flow

---
*Last updated: December 6, 2025*
