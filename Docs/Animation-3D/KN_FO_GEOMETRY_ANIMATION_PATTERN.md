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

### High-Level: Two-Phase Animation Cycle

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        BROWSER ANIMATION FRAME                               │
│                    (requestAnimationFrame callback)                          │
└──────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  PHASE 1: PreAnimationEvent                                                  │
│  ─────────────────────────────────────────────────────────────────────────── │
│  Purpose: KN Model parameter updates, geometry recreation                    │
│  Target:  Knowledge layer (KnModel, KnComponent, KnParameter)                │
└──────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│  PHASE 2: AnimationEvent                                                     │
│  ─────────────────────────────────────────────────────────────────────────── │
│  Purpose: FO shape transform updates, Three.js rendering                     │
│  Target:  Foundry Objects layer (FoShape3D, FoStage3D, Three.js)             │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Detailed: Complete KN→FO Animation Flow

This diagram shows exactly how a parameter change flows from the Knowledge Model
through to a recreated 3D shape rendered in Three.js:

```
Browser Animation Frame (requestAnimationFrame)
    │
    ▼
AnimationFrameBus.PreAnimationEvent
    │
    ▼
MentorServices.OnPreAnimationEvent(evt)
    │
    ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ AnimatedKnModel.OnPreAnimationEvent(evt)                                     │
│                                                                              │
│   ┌────────────────────────────────────────────────────────────────────────┐ │
│   │ STEP 1: base.OnPreAnimationEvent(evt)                                  │ │
│   │         Propagates to all child components                             │ │
│   └────────────────────────────────────────────────────────────────────────┘ │
│       │                                                                      │
│       ▼                                                                      │
│   ┌────────────────────────────────────────────────────────────────────────┐ │
│   │ AnimatedKnComponent.PreAnimationRefresh callback                       │ │
│   │                                                                        │ │
│   │   SetValue("Color", newColor)                                          │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   ColorParam.Smash()                                                   │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   ContributesTo.ForEach(p => p.Smash())                                │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   GeometryParam.Smash()  ◄── dependency link from IDependOn()          │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   BeforeSmash callback fires:                                          │ │
│   │       ├─► oldShape.SetShouldDelete()  (marks for removal)              │ │
│   │       └─► ClearCashe()                (allows recreation)              │ │
│   │                                                                        │ │
│   │   SetValue("GeometryType", newShape)                                   │ │
│   │       └─► (same smash cascade as above)                                │ │
│   └────────────────────────────────────────────────────────────────────────┘ │
│       │                                                                      │
│       ▼                                                                      │
│   ┌────────────────────────────────────────────────────────────────────────┐ │
│   │ STEP 2: RenderGeometry3D(ctx)  ◄── THE KEY STEP!                       │ │
│   │         Without this, empty caches stay empty forever                  │ │
│   │                                                                        │ │
│   │   Geometry3DValueFor(view)                                             │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   GetCurrentValue()                                                    │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   Evaluate() ── IsUnknown()? YES (was smashed) ── run Formula          │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   ComputeShape3D()                                                     │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   IsCasheEmpty()? YES ── Create new FoShape3D                          │ │
│   │       │                                                                │ │
│   │       ▼                                                                │ │
│   │   New shape added to stage with BeforeAnimationRefresh callback        │ │
│   └────────────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────────┘
    │
    ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│ AnimationFrameBus.AnimationEvent                                             │
│                                                                              │
│   FoStage3D processes all shapes:                                            │
│       │                                                                      │
│       ▼                                                                      │
│   FoShape3D.BeforeAnimationRefresh callbacks                                 │
│       │   (for transform-only updates: position, rotation, scale)            │
│       │                                                                      │
│       ▼                                                                      │
│   Three.js renders the scene                                                 │
│       │                                                                      │
│       ▼                                                                      │
│   Canvas displays updated 3D view                                            │
└──────────────────────────────────────────────────────────────────────────────┘
```

### Critical Insight: Why RenderGeometry3D Is Essential

The smash cascade clears caches but **does not trigger re-evaluation**:

```
WITHOUT RenderGeometry3D:                  WITH RenderGeometry3D:
─────────────────────────                  ─────────────────────────
Parameter.SetValue()                       Parameter.SetValue()
    │                                          │
    ▼                                          ▼
Smash cascade                              Smash cascade
    │                                          │
    ▼                                          ▼
Cache cleared                              Cache cleared
    │                                          │
    ▼                                          ▼
??? Nothing calls Evaluate()               RenderGeometry3D()
    │                                          │
    ▼                                          ▼
Shape stays deleted!                       Evaluate() → ComputeShape3D()
                                               │
                                               ▼
                                           New shape created!
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

## The Three Critical Gotchas

### Gotcha 1: Dependencies Are Cleared After Smash

The `KnParameter.Smash()` method clears `ContributesTo` and `DependsOn` lists after cascading:

```csharp
// In KnParameter.Smash()
ContributesTo.ForEach(p => p.Smash());
ContributesTo.Clear();  // ← Dependencies gone!
DependsOn.Clear();      // ← Dependencies gone!
```

**Solution**: Re-establish dependencies in `ComputeShape3D` every time a shape is created:

```csharp
if (geometry.IsCasheEmpty())
{
    shape = CreateComponentGeometry(...);
    geometry.SetCashe(shape);
    
    // CRITICAL: Re-establish dependencies after each creation
    SetupGeometryDependencies(geometry.GetParameter());
}
```

### Gotcha 2: BeforeSmash Only Fires When Value Is Valid

The `BeforeSmash` callback (`OnValueSmash`) only fires if `IsValid()` is true:

```csharp
// In KnParameter.Smash()
if (IsValid())  // ← Only if not Unknown!
    OnValueSmash?.Invoke(this, Value);
```

This means if you smash an already-unknown parameter, the callback won't fire.
This is usually fine because an unknown parameter has no cache to clear.

### Gotcha 3: View/Stage Name Must Match

When `OnPreAnimationEvent` calls `RenderGeometry3D`, it must use the **same view name**
that was used for the initial render:

```
Initial render:       RenderArena3D("KnModelTest3D", ...)  → shapes go to stage "KnModelTest3D"
Animation re-render:  RenderGeometry3D(ctx with "default") → shapes go to stage "default" (WRONG!)
```

**Solution**: Iterate through arena stages with associated scenes:

```csharp
foreach (var stage in arena.GetAllStages())
{
    if (stage.GetAssociatedScene() == null) continue;  // Skip orphan stages
    var ctx = RenderContext3D.Create(arena, stage.Name, deep: true);
    RenderGeometry3D(ctx);
}
```

### Gotcha 4: ClearCashe() Has Smart Caching for Text Shapes!

**CRITICAL**: `KnGeometryParameter.ClearCashe()` has "smart caching" logic that 
**preserves the cache** for shapes with FoText3D sub-elements!

```csharp
// In KnGeometryParameter.ClearCashe()
if (hasTextElements)
{
    // Keep cache intact - no recreation needed!
    return this;  // ← DOES NOT SET _cashe = null!
}
```

If your shape has any text sub-elements (like labels), calling `ClearCashe()` 
in your BeforeSmash callback **will NOT clear the cache**!

**Solution**: Use `GetParameter().SetCashe(null!)` directly to force cache clear:

```csharp
geom.ApplyMethod("ComputeGeometry", ComputeShape3D, null, (param, opResult) => 
{
    var oldShape = geom.GetCashe<FoShape3D>();
    oldShape?.Delete();  // Remove from scene
    
    // CRITICAL: Use SetCashe(null!) to FORCE cache clear
    // Do NOT use ClearCashe() - it has smart caching for text shapes!
    geom.GetParameter().SetCashe(null!);
});
```

## Summary: What Makes It Work

| Component | Purpose |
|-----------|---------|
| `IDependOn(param)` | Creates ContributesTo link so smash cascades |
| `BeforeSmash` callback | Deletes old shape AND **forces** cache clear with `SetCashe(null!)` |
| `OnPreAnimationEvent` override | Calls `RenderGeometry3D` for each stage with scene |
| `ComputeShape3D` | Checks `IsCasheEmpty()`, creates shape, **re-establishes dependencies** |
| `arena.GetAllStages()` | Gets stages to re-render (filter by `GetAssociatedScene()`) |

## Critical Anti-Patterns to Avoid

1. **DON'T use `ClearCashe()`** for shapes with text children - use `SetCashe(null!)` directly
2. **DON'T forget to re-establish dependencies** after shape creation (smash clears them)
3. **DON'T hardcode view names** - iterate stages with associated scenes
4. **DON'T call `RenderGeometry3D` without an arena** - check for null first

## Files Reference

- `AnimatedKnModel.cs` - Model with `OnPreAnimationEvent` override
- `AnimatedKnComponent.cs` - Component with dependencies and BeforeSmash
- `FoundryMentorModeler/Mentor/KnParameter.cs` - `IDependOn`, `ContributesTo`, `Smash`
- `FoundryMentorModeler/Mentor/KnGeometry.cs` - `ApplyMethod` with BeforeSmash
- `FoundryMentorModeler/Mentor/KnGeometryParameter.cs` - `ClearCashe()` smart caching logic
