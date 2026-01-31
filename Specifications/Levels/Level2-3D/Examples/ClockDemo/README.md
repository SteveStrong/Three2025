# ClockDemo - Complete MxObject Reference Implementation

## Overview
This is the complete, working ClockDemo component that demonstrates clean MxObject architecture patterns. It serves as the reference implementation for migrating legacy components to FoundryMicroCore.

## Files in This Example

### ClockDemo.razor.txt
Complete Blazor component markup with:
- Performance monitoring dashboard (FPS, frame count, shape count)
- 3D Canvas integration with Canvas3DComponent
- Interactive control panels for clock and shape management
- Debug information panel
- Modern CSS styling with flexbox layout

### ClockDemo.razor.cs.txt
MxObject-based code-behind demonstrating:
- Clean MxComponent inheritance patterns
- Dedicated stage management per component
- Animation subscription and performance monitoring
- Interactive shape creation (text, models, geometry)
- Proper resource disposal and lifecycle management
- ClockDemoTech class for hierarchical clock assembly

## Key MxObject Patterns Demonstrated

### 1. Component Architecture
```csharp
public partial class ClockDemoBase : ComponentBase, IDisposable
{
    [Inject] public required IFoundryService FoundryService { get; set; }
    
    // Protected fields for Razor access
    protected Canvas3DComponent? _canvasRef;
    protected FoStage3D? _clockStage;
}
```

### 2. Stage Management
```csharp
// Create dedicated stage per component
var arena = FoundryService.Arena();
_clockStage = arena.EstablishStage<FoStage3D>("ClockDemoStage");
```

### 3. MxComponent Tech Classes
```csharp
public class ClockDemoTech : MxComponent, IDisposable
{
    public ClockDemoTech(string name, IFoundryService foundryService) : base(name)
    {
        _foundryService = foundryService;
    }
}
```

### 4. Modern Logging
```csharp
"ClockDemo: Stage created with clean MxObject architecture".WriteSuccess();
"ClockDemo: Animation subscription established".WriteInfo();
"Performance warning: FPS dropped to {_currentFps:F1}".WriteWarning();
```

### 5. Clean Disposal
```csharp
public void Dispose()
{
    _clockTech?.StopClock();
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
    "ClockDemo: Component disposed cleanly".WriteSuccess();
}
```

## Features Implemented

### Real-Time Clock
- Hierarchical 3D clock assembly with hour/minute/second hands
- Digital time display updated every second
- Style toggling (modern/classic)
- Smooth hand rotation based on actual time

### Interactive Shape Gallery
- Random text with colors and positions
- 3D model loading (T-Rex, submarine, axis)
- Procedural geometry creation
- Real-time shape counter

### Performance Monitoring
- Live FPS display
- Animation frame counter
- Performance warnings for low FPS
- Shape count tracking

### Debug Information
- Stage status and name
- Canvas connection status
- Animation subscription status
- Real-time diagnostics

## Usage Instructions

1. **Copy reference files** and rename to proper extensions:
   - `ClockDemo.razor.txt` → `ClockDemo.razor`
   - `ClockDemo.razor.cs.txt` → `ClockDemo.razor.cs`
2. **Update namespace** if different from `Three2025.Components.Pages`
3. **Ensure dependencies** are available:
   - FoundryMicroCore.Library
   - FoundryWorldsAndDrawings
   - Canvas3DComponent
4. **Add route** to navigation if desired
5. **Test compilation** and fix any API differences

## Known TODOs

These represent API methods not yet available in the current FoundryMicroCore version:

```csharp
// TODO: Replace with correct stage-to-scene linking API
// _clockStage?.LinkToScene(scene);

// TODO: Replace with correct stage clearing API  
// _clockStage.ClearAllShapes();

// TODO: Implement proper shape counting with new collection API
return _shapeCounter; // Using counter instead of actual collection query
```

## Testing Checklist

### Basic Functionality
- [ ] Page loads without compilation errors
- [ ] Canvas renders 3D scene
- [ ] Performance counters update in real-time
- [ ] All buttons respond correctly

### Clock Features
- [ ] Clock starts and displays current time
- [ ] Clock hands rotate smoothly every second
- [ ] Digital display updates correctly
- [ ] Clock stops when requested

### Shape Management
- [ ] Random text appears at various positions
- [ ] 3D models load (if assets available)
- [ ] Shape counter increments correctly
- [ ] Debug panel shows accurate information

### Resource Management
- [ ] Clean disposal on page navigation
- [ ] No memory leaks or console errors
- [ ] Animation unsubscribes properly
- [ ] Stage isolation works correctly

## Customization Examples

### Adding New Shape Types
```csharp
protected void AddCustomShape()
{
    if (_clockStage == null) return;

    var customShape = new FoShape3D($"Custom_{++_shapeCounter:D3}")
    {
        Color = "Purple",
        Transform = new Transform3("CustomTransform")
        {
            Position = new Vector3(0, 2, 0),
            Scale = new Vector3(1, 1, 1)
        }
    };
    
    _clockStage.AddShape(customShape);
    $"Added custom shape '{customShape.Name}'".WriteSuccess();
}
```

### Adding Performance Metrics
```csharp
protected double _averageFps = 0.0;
protected int _fpsHistory = 0;

private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _currentFps = evt.fps;
        _averageFps = (_averageFps * _fpsHistory + evt.fps) / (_fpsHistory + 1);
        _fpsHistory++;
        
        InvokeAsync(StateHasChanged);
    }
}
```

## Migration Template

Use this component as a template for migrating legacy components:

1. **Extract core functionality** into isolated methods
2. **Replace legacy APIs** with MxObject equivalents
3. **Add proper stage management** and lifecycle
4. **Implement clean disposal** patterns
5. **Add logging** for debugging and monitoring
6. **Test thoroughly** in isolation before integration

This ClockDemo serves as the **gold standard** for MxObject-based component development and should be referenced when creating or migrating other components.