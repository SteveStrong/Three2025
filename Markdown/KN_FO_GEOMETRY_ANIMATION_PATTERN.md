# KN→FO Geometry Animation Pattern

## Overview

This document describes the pattern for animating KnModel geometry where **parameter changes trigger geometry recreation**, not just transform updates. This leverages the **built-in dependency mechanism** - when properly configured, changing a parameter automatically smashes dependent geometry and triggers cleanup.

## Two Types of Animation

### Type 1: Transform Animation (Simple)
For position, rotation, scale changes that don't require geometry recreation:
- Use `BeforeAnimationRefresh` on the FoShape3D
- Directly modify `Transform.Position`, `Transform.Rotation`, etc.
- The existing shape moves/rotates/scales

```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    if (self is FoShape3D s && s.Transform != null)
    {
        var newY = baseY + amplitude * Math.Sin(frequency * tick);
        s.Transform.MoveTo(baseX, newY, baseZ);
    }
});
```

### Type 2: Geometry Recreation Animation (This Document)
For changes that require a new shape (geometry type, color, material):
- Set up **dependencies** so geometry parameter depends on GeometryType, Color, etc.
- Set up **BeforeSmash callback** to delete old shape when geometry is smashed
- Use `PreAnimationRefresh` on the KnComponent to update parameters
- Call `RenderGeometry3D` to trigger recreation (since cache is now empty)

## The Dependency Mechanism

The key insight: **Parameters have a `ContributesTo` relationship.** When parameter A depends on parameter B:
- `B.ContributesTo.Contains(A)` is true
- When B is smashed, A gets smashed too via `ContributesTo.ForEach(p => p.Smash())`

```
GeometryType parameter ──ContributesTo──► Geometry parameter
                                                │
                                                ▼
                                    BeforeSmash callback
                                    (deletes old shape,
                                     clears cache)
```

## The Animation Flow

```
AnimationFrameBus
    │
    ▼
┌─────────────────────────────────────────────────────────────┐
│ PreAnimationEvent (for KN model updates)                    │
│   └─► MentorServices.OnPreAnimationEvent()                  │
│        └─► model.OnPreAnimationEvent(evt)                   │
│             ├─► PreAnimationRefresh callbacks               │
│             │    └─► UpdateParameter("GeometryType", ...)   │
│             │         └─► GeometryType.Smash()              │
│             │              └─► Geometry.Smash() (via deps)  │
│             │                   └─► BeforeSmash callback    │
│             │                        └─► Delete old shape   │
│             │                        └─► Clear cache        │
│             └─► RenderGeometry3D(ctx)                       │
│                  └─► ComputeShape3D recreates (cache empty) │
└─────────────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────────┐
│ AnimationEvent (for FO rendering)                           │
│   └─► FoShape3D.BeforeAnimationRefresh (transform updates)  │
│   └─► Three.js renders the scene                            │
└─────────────────────────────────────────────────────────────┘
```

## Implementation Pattern

### Step 1: Model Override - Trigger Re-render After Parameter Updates

```csharp
public class AnimatedKnModel : PartModel
{
    /// <summary>
    /// Override to trigger geometry re-rendering after parameter updates.
    /// The dependency mechanism handles cache clearing via BeforeSmash,
    /// but we still need to call RenderGeometry3D to recreate shapes.
    /// </summary>
    public override void OnPreAnimationEvent(PreAnimationEvent evt)
    {
        // 1. Let base class propagate to all children
        //    This triggers PreAnimationRefresh callbacks which may update parameters
        //    Parameter updates trigger smash cascade via dependencies
        base.OnPreAnimationEvent(evt);
        
        // 2. Re-render geometry to pick up any cache invalidations
        var arena = GetArena();
        if (arena != null)
        {
            var ctx = RenderContext3D.Create(arena, "ViewName", deep: true);
            RenderGeometry3D(ctx);
        }
    }
}
```

### Step 2: Component - Set Up Dependencies and BeforeSmash

```csharp
public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view, IArena? page)
{
    var result = Compute3DGeometry(view, geom => 
    {
        // Set up compute method WITH BeforeSmash callback for cleanup
        geom.ApplyMethod("ComputeGeometry", ComputeShape3D, null, (param, opResult) => 
        {
            // Called when geometry parameter is smashed (via dependency cascade)
            var oldShape = geom.GetCashe<FoShape3D>();
            oldShape?.Delete();  // Remove from scene
            geom.ClearCashe();   // Allow recreation
        });
        
        // Establish dependencies: geometry depends on these parameters
        var geomParam = geom.GetParameter();
        SetupGeometryDependencies(geomParam);
    });

    return (result, result.GetParameter());
}

private void SetupGeometryDependencies(KnGeometryParameter geomParam)
{
    var geomTypeParam = FindParameter("GeometryType");
    var colorParam = FindParameter("Color");
    
    // IDependOn creates the ContributesTo link
    if (geomTypeParam != null)
        geomParam.IDependOn(geomTypeParam);
    
    if (colorParam != null)
        geomParam.IDependOn(colorParam);
}
```

### Step 3: Component - Just Update Parameters

```csharp
private void CycleGeometryType()
{
    var currentType = FindParameterValue<string>("GeometryType") ?? "Box";
    var nextType = currentType.ToLower() switch
    {
        "box" => "Sphere",
        "sphere" => "Cylinder",
        "cylinder" => "Box",
        _ => "Box"
    };
    
    // Just update the parameter - dependency mechanism handles the rest!
    // This triggers: GeometryType.Smash() → Geometry.Smash() → BeforeSmash → cleanup
    UpdateParameter("GeometryType", nextType);
}
```

## Why Plugin710 Passes null, null

Looking at Plugin710 code:
```csharp
geom.ApplyMethod("ComputeGeometry", ComputeShape3D, null, null);
```

Plugin710 components don't set up dependencies because they're typically:
1. Static geometry that doesn't change type/color after creation
2. User-driven changes trigger full re-render via UI actions
3. Not animated with parameter changes during runtime

For **animated** components where parameters change during the animation loop, you NEED:
- Dependencies (`IDependOn`) to cascade the smash
- `BeforeSmash` callback to clean up the old shape

## The Two Missing Pieces (Without Dependencies)

If you DON'T set up dependencies:
1. `UpdateParameter("GeometryType", "Sphere")` smashes GeometryType
2. Geometry parameter **doesn't know it should smash** - no `ContributesTo` link
3. Cache stays valid with old shape
4. Even calling `RenderGeometry3D` won't help - cache isn't empty!

If you set up dependencies but NOT `BeforeSmash`:
1. Parameter change smashes geometry (good!)
2. But old shape stays in scene (duplicates appear)

## Summary: What Makes It Work

| Component | Purpose |
|-----------|---------|
| `IDependOn(param)` | Creates ContributesTo link so smash cascades |
| `BeforeSmash` callback | Deletes old shape AND clears cache |
| `OnPreAnimationEvent` override | Calls `RenderGeometry3D` to recreate from empty cache |
| `ComputeShape3D` | Checks `IsCasheEmpty()` and creates new shape |

## Files Reference

- `AnimatedKnModel.cs` - Model with `OnPreAnimationEvent` override
- `AnimatedKnComponent.cs` - Component with dependencies and BeforeSmash
- `FoundryMentorModeler/Mentor/KnParameter.cs` - `IDependOn`, `ContributesTo`, `Smash`
- `FoundryMentorModeler/Mentor/KnGeometry.cs` - `ApplyMethod` with BeforeSmash
