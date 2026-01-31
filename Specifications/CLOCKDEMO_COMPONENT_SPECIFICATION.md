# ClockDemo Component Specification

## Overview
A clean MxObject-based implementation of an interactive 3D clock demonstration page. This serves as a reference implementation for modern FoundryMicroCore patterns.

## Architecture Foundation

### MxObject Integration
- **Base Classes**: All components inherit from MxObject/MxComponent
- **Logging**: FoundryMicroCore.Core.Extensions for colored console output
- **Properties**: Use `.Name` instead of `.Key` throughout
- **Tree View**: Implement `GetTreeViewNodeTitle()` and custom formatters
- **Stage Management**: Dedicated stage per component with proper lifecycle

### Component Structure
```
ClockDemo.razor              # UI markup
ClockDemo.razor.cs           # MxObject-based code-behind  
ClockDemoModels.cs          # Data models using MxObject patterns
ClockDemoTech.cs            # Clock creation/animation logic
```

## Core Features Specification

### 1. Real-Time 3D Clock

**Purpose**: Demonstrate hierarchical 3D animation with time-based updates

**Components**:
- **Clock Face**: Circular base with hour markers (1-12)
- **Hour Hand**: Rotates 360° every 12 hours
- **Minute Hand**: Rotates 360° every 60 minutes  
- **Second Hand**: Rotates 360° every 60 seconds
- **Center Post**: Static vertical axis for hands
- **Digital Display**: Floating text showing HH:MM:SS

**Animation Pattern**:
```csharp
// In ClockTech.cs - MxObject-based
public class ClockTech : MxComponent
{
    private Timer _clockTimer;
    private FoShape3D _clockAssembly;
    
    public void StartClock(FoStage3D targetStage)
    {
        _clockAssembly = CreateClockAssembly();
        targetStage.AddShape(_clockAssembly);
        
        _clockTimer = new Timer(UpdateClockHands, null, 0, 1000);
        "Clock started with 1-second updates".WriteSuccess();
    }
    
    private void UpdateClockHands(object state)
    {
        var now = DateTime.Now;
        
        // Calculate angles (0° = 12 o'clock position)
        var secondAngle = (now.Second * 6) - 90;  // 6° per second
        var minuteAngle = (now.Minute * 6) - 90;  // 6° per minute  
        var hourAngle = ((now.Hour % 12) * 30 + now.Minute * 0.5) - 90; // 30° per hour
        
        // Apply rotations to hands
        _secondHand.Transform.RotateTo(0, 0, MathF.PI * secondAngle / 180);
        _minuteHand.Transform.RotateTo(0, 0, MathF.PI * minuteAngle / 180);
        _hourHand.Transform.RotateTo(0, 0, MathF.PI * hourAngle / 180);
        
        // Update digital display
        _digitalDisplay.Text = now.ToString("HH:mm:ss");
    }
}
```

### 2. Interactive Shape Gallery

**Purpose**: Demonstrate dynamic shape creation and lifecycle management

**Shape Categories**:

**A. 3D Models**
- T-Rex (animated GLB model)
- Submarine (static GLB model)  
- Coordinate axis (reference model)

**B. Procedural Shapes**  
- Random text with colors/positions
- Geometric primitives (boxes, spheres, cylinders)
- Animated shapes with transforms

**C. Composite Shapes**
- TRISOC geometry assemblies
- Multi-part equipment models

**Creation Pattern**:
```csharp
// In ClockDemo.razor.cs - MxObject patterns
public partial class ClockDemo : ComponentBase, IDisposable
{
    private FoStage3D _clockStage;
    private ClockTech _clockTech;
    
    protected void AddRandomText()
    {
        var textShape = new FoText3D($"Text_{Guid.NewGuid():N[..8]}")
        {
            Text = _dataGenerator.RandomSentence(),
            Color = _dataGenerator.RandomColor(),
            FontSize = _dataGenerator.RandomDouble(1.0, 3.0),
            Transform = new Transform3("RandomText")
            {
                Position = new Vector3(
                    _dataGenerator.RandomDouble(-10, 10),
                    _dataGenerator.RandomDouble(2, 8), 
                    _dataGenerator.RandomDouble(-10, 10)
                )
            }
        };
        
        _clockStage.AddShape(textShape);
        $"Added random text: '{textShape.Text}' at {textShape.Transform.Position}".WriteInfo();
    }
    
    protected void AddTRex()
    {
        var tRex = new FoModel3D("TRex")
        {
            Url = GetAssetPath("models/trex.glb"),
            Format = Model3DFormats.Gltf,
            Transform = new Transform3("TRexTransform")
            {
                Position = new Vector3(5, 0, 5),
                Scale = new Vector3(0.5, 0.5, 0.5)
            }
        };
        
        _clockStage.AddShape(tRex);
        "T-Rex added to scene".WriteSuccess();
    }
}
```

### 3. Performance Monitoring

**Purpose**: Real-time performance diagnostics and frame rate monitoring

**Metrics Displayed**:
- **FPS**: Current frames per second
- **Animation Tick**: Frame counter
- **Shape Count**: Objects in stage
- **Memory Usage**: Optional GC metrics

**UI Layout**:
```razor
<!-- Performance Dashboard -->
<div class="performance-dashboard">
    <div class="metric-card fps">
        <h4>FPS</h4>
        <span class="metric-value">@_currentFps.ToString("F1")</span>
    </div>
    <div class="metric-card tick">
        <h4>Frame</h4>
        <span class="metric-value">@_currentTick</span>
    </div>
    <div class="metric-card shapes">
        <h4>Shapes</h4>
        <span class="metric-value">@GetShapeCount()</span>
    </div>
</div>
```

**Animation Integration**:
```csharp
private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _currentFps = evt.fps;
        _currentTick = evt.tick;
        
        // Optional: Performance warnings
        if (_currentFps < 30)
        {
            $"Performance warning: FPS dropped to {_currentFps:F1}".WriteWarning();
        }
        
        InvokeAsync(StateHasChanged);
    }
}
```

### 4. Stage Management & Lifecycle

**Purpose**: Demonstrate proper MxObject lifecycle patterns

**Stage Creation**:
```csharp
protected override async Task OnInitializedAsync()
{
    // Create dedicated stage for this page
    var arena = _foundryService.Arena();
    _clockStage = arena.EstablishStage<FoStage3D>("ClockDemoStage");
    
    "ClockDemo stage created".WriteInfo();
    
    // Initialize tech components
    _clockTech = new ClockTech("ClockTech");
    
    // Subscribe to animation bus
    AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
    
    await base.OnInitializedAsync();
}
```

**Canvas Integration**:
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(100); // Allow canvas initialization
        
        if (_canvasRef != null)
        {
            var (found, scene) = _canvasRef.GetActiveScene();
            if (found && scene != null)
            {
                // Link our stage to the scene
                _clockStage.LinkToScene(scene);
                $"ClockDemo stage linked to scene '{scene.Name}'".WriteSuccess();
                
                // Auto-start the clock
                await StartClockDemo();
            }
        }
    }
}
```

**Cleanup & Disposal**:
```csharp
public void Dispose()
{
    // Stop clock timer
    _clockTech?.StopClock();
    
    // Clear only our shapes
    _clockStage?.ClearAllShapes();
    "ClockDemo shapes cleared".WriteInfo();
    
    // Unsubscribe from animation
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
    
    "ClockDemo disposed cleanly".WriteInfo();
}
```

## User Interface Specification

### Layout Structure
```razor
@page "/clock-demo"
@namespace Three2025.Components.Pages
@inherits ClockDemoBase
@rendermode InteractiveServer

<PageTitle>Clock Demo - MxObject Reference Implementation</PageTitle>

<!-- Header with description -->
<div class="demo-header">
    <h2>🕐 Clock Demo - Clean MxObject Implementation</h2>
    <p class="description">
        Reference implementation demonstrating FoundryMicroCore patterns:
        MxObject lifecycle, stage management, animation integration, and performance monitoring.
    </p>
</div>

<!-- Performance Dashboard -->
<div class="performance-section">
    <!-- FPS/Metrics as specified above -->
</div>

<!-- Main Content Layout -->
<div class="demo-container">
    <!-- 3D Canvas -->
    <div class="canvas-section">
        <Canvas3DComponent SceneName="ClockDemo" @ref="_canvasRef" 
                          CanvasWidth="800" CanvasHeight="600" />
    </div>
    
    <!-- Control Panels -->
    <div class="controls-section">
        <!-- Clock Controls -->
        <div class="control-panel">
            <h4>🕐 Clock Controls</h4>
            <button class="btn btn-success" @onclick="StartClock">Start Clock</button>
            <button class="btn btn-warning" @onclick="StopClock">Stop Clock</button>
            <button class="btn btn-info" @onclick="ToggleClockStyle">Toggle Style</button>
        </div>
        
        <!-- Shape Gallery -->
        <div class="control-panel">
            <h4>🎨 Shape Gallery</h4>
            <div class="button-grid">
                <button @onclick="AddRandomText">Random Text</button>
                <button @onclick="AddTRex">T-Rex Model</button>
                <button @onclick="AddSubmarine">Submarine</button>
                <button @onclick="AddGeometry">Random Geometry</button>
                <button @onclick="AddAxis">Coordinate Axis</button>
            </div>
        </div>
        
        <!-- Scene Management -->
        <div class="control-panel">
            <h4>🎭 Scene Management</h4>
            <button class="btn btn-primary" @onclick="ClearShapes">Clear All Shapes</button>
            <button class="btn btn-secondary" @onclick="ResetCamera">Reset Camera</button>
            <button class="btn btn-info" @onclick="ShowStageInfo">Stage Info</button>
        </div>
    </div>
</div>

<!-- Debug Information Panel -->
<div class="debug-section">
    <h5>🔧 Debug Information</h5>
    <div class="debug-info">
        <div><strong>Stage:</strong> @(_clockStage?.Name ?? "Not created")</div>
        <div><strong>Canvas:</strong> @(_canvasRef?.SceneName ?? "Not ready")</div>
        <div><strong>Shape Count:</strong> @GetShapeCount()</div>
        <div><strong>Animation:</strong> @(_animationActive ? "Active" : "Inactive")</div>
    </div>
</div>
```

### CSS Styling
```css
.demo-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 2rem;
    border-radius: 8px;
    margin-bottom: 2rem;
}

.performance-section {
    display: flex;
    gap: 1rem;
    margin-bottom: 2rem;
}

.metric-card {
    flex: 1;
    background: #f8f9fa;
    border: 2px solid #dee2e6;
    border-radius: 8px;
    padding: 1rem;
    text-align: center;
}

.metric-card.fps { border-color: #28a745; }
.metric-card.tick { border-color: #007bff; }
.metric-card.shapes { border-color: #ffc107; }

.metric-value {
    font-size: 2rem;
    font-weight: bold;
    display: block;
    margin-top: 0.5rem;
}

.demo-container {
    display: flex;
    gap: 2rem;
    margin-bottom: 2rem;
}

.canvas-section {
    flex: 2;
}

.controls-section {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.control-panel {
    background: white;
    border: 1px solid #dee2e6;
    border-radius: 8px;
    padding: 1rem;
}

.button-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 0.5rem;
    margin-top: 1rem;
}

.debug-section {
    background: #f8f9fa;
    border: 1px solid #dee2e6;
    border-radius: 8px;
    padding: 1rem;
}

.debug-info {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 1rem;
    margin-top: 1rem;
    font-family: monospace;
}
```

## Implementation Phases

### Phase 1: Foundation (Day 1)
- [ ] Create ClockDemo.razor with basic layout
- [ ] Implement MxObject-based code-behind
- [ ] Add Canvas3D integration
- [ ] Set up stage management and lifecycle
- [ ] Verify basic shape addition works

### Phase 2: Clock Implementation (Day 2) 
- [ ] Create ClockTech component with MxObject patterns
- [ ] Implement hierarchical clock assembly
- [ ] Add real-time animation updates
- [ ] Integrate timer-based hand movements
- [ ] Add digital time display

### Phase 3: Shape Gallery (Day 3)
- [ ] Implement interactive shape creation
- [ ] Add 3D model loading (T-Rex, submarine)
- [ ] Create procedural shape generators
- [ ] Add random positioning and styling
- [ ] Implement shape lifecycle management

### Phase 4: Polish & Documentation (Day 4)
- [ ] Add performance monitoring
- [ ] Implement debug information panel
- [ ] Create comprehensive error handling
- [ ] Add inline code documentation
- [ ] Write usage examples and patterns

## Success Criteria

### Technical Requirements
- ✅ **Zero compilation errors** - Clean MxObject integration
- ✅ **Proper disposal** - No memory leaks on page navigation
- ✅ **Stage isolation** - Only affects ClockDemo shapes
- ✅ **Animation performance** - Maintains 60 FPS with multiple shapes
- ✅ **Error handling** - Graceful degradation on failures

### Documentation Requirements  
- ✅ **Pattern examples** - Each MxObject pattern documented
- ✅ **Usage guide** - Step-by-step implementation instructions
- ✅ **Migration notes** - How to convert legacy components
- ✅ **API reference** - Method signatures and parameters
- ✅ **Troubleshooting** - Common issues and solutions

### User Experience Requirements
- ✅ **Intuitive interface** - Clear controls and feedback
- ✅ **Responsive performance** - Smooth interactions
- ✅ **Visual polish** - Professional styling and layout
- ✅ **Educational value** - Demonstrates best practices clearly
- ✅ **Extensibility** - Easy to add new features

## Conclusion

This ClockDemo component will serve as the **gold standard** for MxObject-based component development, providing:

1. **Clean Reference Implementation** - No legacy technical debt
2. **Comprehensive Pattern Library** - All MxObject patterns demonstrated
3. **Performance Baseline** - Optimized animation and lifecycle management  
4. **Documentation Template** - Reusable specification format
5. **Migration Roadmap** - Clear path for converting other components

The component prioritizes **clarity over complexity**, **patterns over performance**, and **education over features** - making it an ideal foundation for the FoundryMicroCore architecture migration.