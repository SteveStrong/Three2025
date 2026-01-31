# Level 2: 3D Visualization Components  

## Overview
3D-enabled MxObject components using **FoundryMicroCore.Library + FoundryWorldsAndDrawings**. Adds 3D visualization capabilities to MxObject foundation.

## Dependencies Required
```xml
<PackageReference Include="FoundryMicroCore.Library" Version="1.0.0" />
<PackageReference Include="FoundryWorldsAndDrawings" Version="1.0.0" />
```

## Capabilities (Level 1 +)
- ✅ Canvas3DComponent integration
- ✅ 3D shape creation (FoShape3D, FoText3D, FoModel3D)  
- ✅ Stage management and scene integration
- ✅ Transform3 positioning, scaling, rotation
- ✅ 3D asset loading (GLB models)
- ✅ Animation frame synchronization
- ✅ Performance monitoring (FPS tracking)

## Featured Example: ClockDemo

### ClockDemo Component ⭐
**Location**: `Examples/ClockDemo/`
**Purpose**: Complete reference implementation for Level 2 capabilities

**Features Demonstrated**:
- Real-time animated 3D clock with moving hands
- Interactive shape gallery (text, models, geometry)  
- Performance monitoring dashboard
- Canvas3D integration with stage management
- 3D asset loading with graceful fallbacks
- Clean MxObject lifecycle throughout

**Files**:
- `ClockDemo.razor.txt` - Complete UI with styling
- `ClockDemo.razor.cs.txt` - MxObject-based code-behind  
- `README.md` - Usage and customization guide

## Assets Included
**Location**: `Assets/models/`
- `TRex.glb.placeholder` - Animated dinosaur model
- `submarine.glb.placeholder` - Static vehicle model  
- `fiveMeterAxis.glb.placeholder` - Coordinate system reference

See `Assets/README.md` for deployment instructions and fallback options.

## Key Patterns Demonstrated

### Stage Management
```csharp
// Create dedicated stage per component
var arena = FoundryService.Arena();  
_stage = arena.EstablishStage<FoStage3D>("ComponentStage");

// Add shapes to isolated stage
_stage.AddShape(shape);
```

### 3D Object Creation
```csharp
var shape = new FoShape3D("MyShape") { Color = "Blue" };
var text = new FoText3D("MyText") { Text = "Hello", FontSize = 2.0 };  
var model = new FoModel3D("MyModel") { Url = "path/to/model.glb" };
```

### Animation Integration  
```csharp
AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);

private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _currentFps = evt.fps;
        // Update UI based on animation state
    }
}
```

### Transform Manipulation
```csharp
shape.Transform = new Transform3("ShapeTransform")
{
    Position = new Vector3(x, y, z),
    Scale = new Vector3(1, 2, 1),  
    Rotation = new Vector3(0, MathF.PI/4, 0)
};
```

## Use Cases
- Interactive 3D visualizations
- 3D model viewers and galleries
- Animated 3D scenes and dashboards  
- Performance-monitored 3D applications
- Educational 3D demonstrations

## Skills Available  
- **3D_Component_Integration.md** - Canvas3D + MxObject patterns
- **Stage_Management_Pattern.md** - Dedicated stages per component
- **3D_Asset_Loading.md** - GLB models with fallbacks
- **Animation_Performance.md** - FPS monitoring and optimization
- **Transform_Management.md** - 3D positioning and animation

## LLM Generation Ready
**Primary Input**: `../../LLM_Generation_Instructions.md`
**Reference**: `Examples/ClockDemo/*.txt` files
**Assets**: `Assets/models/` with fallback documentation

## Success Criteria
- ✅ All Level 1 criteria met (MxObject foundation)
- ✅ Canvas3DComponent renders 3D scene successfully
- ✅ 3D shapes appear and respond to transforms
- ✅ Animation frame updates maintain 60 FPS
- ✅ Stage isolation prevents cross-component interference
- ✅ Clean disposal prevents memory leaks
- ✅ Asset loading works with graceful fallbacks

## Migration Path
- **From Level 1**: Add FoundryWorldsAndDrawings dependency, integrate Canvas3D
- **To Level 3**: Add FoundryMentorModeler for knowledge modeling capabilities

Perfect for projects needing **3D visualization without knowledge modeling complexity**.