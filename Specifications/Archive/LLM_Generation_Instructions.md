# LLM Code Generation Instructions for ClockDemo

## Generation Context
**Target**: Generate a complete Blazor component from this specification
**Architecture**: Clean MxObject patterns with FoundryMicroCore.Library
**Output**: Two files - ClockDemo.razor and ClockDemo.razor.cs
**Reference**: See Examples/ClockDemo/*.txt files for expected format
**Assets**: See Assets/models/ for required 3D models (use placeholders if models not available)
**Dependencies**: FoundryMicroCore.Library, FoundryWorldsAndDrawings, Canvas3DComponent

## Code Generation Requirements

### File 1: ClockDemo.razor
```
Requirements:
- @page "/clock-demo" route
- @inherits ClockDemoBase pattern
- @rendermode InteractiveServer
- Performance dashboard (FPS, Frame, Shape count)
- Canvas3DComponent with SceneName="ClockDemo" 
- Control panels for Clock, Shape Gallery, Scene Management
- Debug information panel
- Modern CSS with flexbox layout
- All interactive elements use @onclick handlers

UI Layout Structure:
[Header] -> [Performance Dashboard] -> [Canvas + Controls] -> [Debug Panel]

Required @onclick Methods:
- StartClock, StopClock, ToggleClockStyle  
- AddRandomText, AddTRex, AddSubmarine, AddGeometry, AddAxis
- ClearShapes, ResetCamera, ShowStageInfo

CSS Requirements:
- Gradient header background
- Metric cards with color-coded borders
- Responsive flexbox layout  
- Button styling with disabled states
```

### File 2: ClockDemo.razor.cs  
```
Requirements:
- public partial class ClockDemoBase : ComponentBase, IDisposable
- [Inject] IFoundryService, IWorkspace, NavigationManager
- Protected fields for Razor binding (_currentFps, _clockRunning, etc.)
- OnInitializedAsync: Create stage, init tech, subscribe animation
- OnAfterRenderAsync: Canvas setup and auto-start
- OnAnimationFrame: FPS/tick updates with performance warnings
- All button handler methods as protected void
- Complete Dispose pattern with logging
- ClockDemoTech inner class inheriting MxComponent
- Timer-based clock hand animation with real-time updates

Architecture Patterns Required:
- arena.EstablishStage<FoStage3D>("ClockDemoStage") 
- AnimationFrameBus.SubscribeToAnimation/UnSubscribe
- WriteSuccess/WriteInfo/WriteWarning/WriteError logging
- FoText3D, FoModel3D, FoShape3D object creation
- Transform3 with Position/Scale/Rotation
- MockDataGenerator for random values
- Exception handling with logging
```

## Success Validation Criteria

### Compilation Requirements
- Zero compilation errors
- All using statements correct
- All method signatures match Razor bindings
- Proper nullable annotations

### Runtime Requirements  
- Page loads without exceptions
- Canvas renders 3D scene
- FPS counter updates smoothly
- All buttons respond correctly
- Clean disposal on navigation

### MxObject Integration
- Proper MxComponent inheritance
- Stage isolation per component
- Modern logging throughout
- Resource cleanup in Dispose

## Code Generation Constraints

### Must Use These APIs:
```csharp
// Stage Management
var arena = FoundryService.Arena();
_stage = arena.EstablishStage<FoStage3D>("StageName");
_stage.AddShape(shape);

// Animation
AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);

// Logging  
"Message".WriteSuccess(); // Green
"Message".WriteInfo();    // Blue
"Message".WriteWarning(); // Yellow
"Message".WriteError();   // Red

// 3D Objects
var shape = new FoShape3D("Name") { Color = "Blue" };
var text = new FoText3D("Name") { Text = "Hello", FontSize = 1.0 };
var model = new FoModel3D("Name") { Url = "path/to/model.glb" };
```

### Must Avoid These Patterns:
```csharp
// DON'T USE - Legacy APIs
shape.Key (use shape.Name instead)
stage.ClearAllShapes() (not available yet)
stage.LinkToScene() (not available yet) 
model.Format = Model3DFormats.Gltf (not needed)

// DON'T USE - Console/Debug output
Console.WriteLine()
Debug.WriteLine()
(Use Write* extensions instead)
```

## Implementation Notes for LLM

1. **Generate complete files** - don't leave placeholder comments
2. **Handle missing APIs gracefully** - comment out unavailable methods with TODO
3. **Use consistent naming** - ClockDemo prefix for all related classes
4. **Add comprehensive error handling** - try/catch with proper logging
5. **Follow C# conventions** - proper async/await, nullable patterns
6. **Make it educational** - include inline comments explaining MxObject patterns

## Expected Output Structure

The LLM should generate:
1. Complete ClockDemo.razor file (175+ lines)
2. Complete ClockDemo.razor.cs file (500+ lines) 
3. Both files ready to compile and run
4. All interactive features working
5. Clean MxObject architecture throughout
6. No dependencies on existing broken code

This specification provides everything needed to generate a working ClockDemo component from scratch using modern MxObject patterns.