# ClockDemo Component Implementation Skill

## Skill Overview
**Purpose**: Create a clean MxObject-based 3D clock demonstration component  
**Difficulty**: Intermediate  
**Prerequisites**: FoundryMicroCore.Library integration, Canvas3D basics  
**Output**: Working reference implementation for MxObject patterns  

## What You'll Learn
- ✅ Clean MxObject lifecycle management
- ✅ Dedicated stage-per-component pattern  
- ✅ Animation integration with performance monitoring
- ✅ Interactive 3D shape creation and management
- ✅ Proper resource disposal and cleanup

## Architecture Foundation

### Core Principles
1. **MxObject Inheritance**: All components inherit from MxObject/MxComponent
2. **Stage Isolation**: Each component gets its own dedicated FoStage3D
3. **Clean Lifecycle**: Proper initialization, operation, and disposal
4. **Logging Integration**: Use FoundryMicroCore.Core.Extensions throughout
5. **API Modernization**: Use .Name instead of .Key, new collection patterns

### File Structure
```
Components/Pages/
├── ClockDemo.razor              # UI markup and styling
├── ClockDemo.razor.cs           # MxObject-based code-behind
└── ClockDemo.md                 # Component documentation
```

## Step-by-Step Implementation

### Step 1: Create Basic Razor Component

**File**: `ClockDemo.razor`

```razor
@page "/clock-demo"
@namespace Three2025.Components.Pages
@inherits ClockDemoBase
@rendermode InteractiveServer

<PageTitle>Clock Demo - MxObject Reference</PageTitle>

<!-- Performance Dashboard -->
<div class="performance-section">
    <div class="metric-card fps">
        <h4>FPS</h4>
        <span class="value">@_currentFps.ToString("F1")</span>
    </div>
    <div class="metric-card tick">
        <h4>Frame</h4>
        <span class="value">@_currentTick</span>
    </div>
</div>

<!-- 3D Canvas -->
<div class="canvas-section">
    <Canvas3DComponent SceneName="ClockDemo" @ref="_canvasRef" 
                      CanvasWidth="800" CanvasHeight="600" />
</div>

<!-- Controls -->
<div class="controls-section">
    <button @onclick="StartClock" disabled="@_clockRunning">Start Clock</button>
    <button @onclick="StopClock" disabled="@(!_clockRunning)">Stop Clock</button>
    <button @onclick="AddRandomText">Add Text</button>
    <button @onclick="ClearShapes">Clear All</button>
</div>

<style>
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

.metric-card .value {
    font-size: 2rem;
    font-weight: bold;
    display: block;
}

.canvas-section {
    margin-bottom: 2rem;
}

.controls-section {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
}

button {
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 4px;
    background: #007bff;
    color: white;
    cursor: pointer;
}

button:disabled {
    background: #6c757d;
    cursor: not-allowed;
}
</style>
```

### Step 2: Create MxObject-Based Code-Behind

**File**: `ClockDemo.razor.cs`

```csharp
#nullable enable

using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;

namespace Three2025.Components.Pages;

public partial class ClockDemoBase : ComponentBase, IDisposable
{
    [Inject] public required IFoundryService FoundryService { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }

    // Protected fields for Razor access
    protected Canvas3DComponent? _canvasRef;
    protected FoStage3D? _clockStage;
    protected bool _clockRunning = false;
    protected double _currentFps = 0.0;
    protected int _currentTick = 0;

    private ClockTech? _clockTech;
    private MockDataGenerator _dataGenerator = new();

    protected override async Task OnInitializedAsync()
    {
        // Create dedicated stage - MxObject Pattern
        var arena = FoundryService.Arena();
        _clockStage = arena.EstablishStage<FoStage3D>("ClockDemoStage");
        
        "ClockDemo: Stage created".WriteSuccess();
        
        // Initialize tech components
        _clockTech = new ClockTech("ClockTech", FoundryService);
        
        // Subscribe to animation
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _canvasRef != null)
        {
            await Task.Delay(100); // Allow canvas initialization
            
            var (found, scene) = _canvasRef.GetActiveScene();
            if (found && scene != null)
            {
                "ClockDemo: Canvas ready".WriteSuccess();
                // Stage-to-scene integration happens here
            }
        }
    }

    private void OnAnimationFrame(AnimationEvent evt)
    {
        if (evt.IsWorld3D())
        {
            _currentFps = evt.fps;
            _currentTick = evt.tick;
            InvokeAsync(StateHasChanged);
        }
    }

    protected void StartClock()
    {
        if (_clockTech != null && _clockStage != null)
        {
            _clockTech.StartClock(_clockStage);
            _clockRunning = true;
            StateHasChanged();
        }
    }

    protected void StopClock()
    {
        _clockTech?.StopClock();
        _clockRunning = false;
        StateHasChanged();
    }

    protected void AddRandomText()
    {
        if (_clockStage == null) return;

        var text = new FoText3D($"Text_{Guid.NewGuid():N[..6]}")
        {
            Text = _dataGenerator.RandomSentence(),
            Color = _dataGenerator.RandomColor(),
            Transform = new Transform3("TextTransform")
            {
                Position = new Vector3(
                    _dataGenerator.RandomDouble(-5, 5),
                    _dataGenerator.RandomDouble(2, 6),
                    _dataGenerator.RandomDouble(-5, 5)
                )
            }
        };

        _clockStage.AddShape(text);
        $"Added text: {text.Text}".WriteInfo();
    }

    protected void ClearShapes()
    {
        // TODO: Implement with correct API
        "Clear shapes requested".WriteInfo();
    }

    public void Dispose()
    {
        _clockTech?.StopClock();
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
        "ClockDemo: Disposed cleanly".WriteInfo();
    }
}

/// <summary>
/// Clock creation and animation logic - MxComponent implementation
/// </summary>
public class ClockTech : MxComponent
{
    private readonly IFoundryService _foundryService;
    private Timer? _clockTimer;
    private FoShape3D? _clockAssembly;

    public ClockTech(string name, IFoundryService foundryService) : base(name)
    {
        _foundryService = foundryService;
    }

    public void StartClock(FoStage3D targetStage)
    {
        if (_clockAssembly == null)
        {
            _clockAssembly = CreateClockAssembly();
            targetStage.AddShape(_clockAssembly);
        }

        _clockTimer = new Timer(UpdateClock, null, 0, 1000);
        "Clock started".WriteSuccess();
    }

    public void StopClock()
    {
        _clockTimer?.Dispose();
        _clockTimer = null;
        "Clock stopped".WriteInfo();
    }

    private FoShape3D CreateClockAssembly()
    {
        var assembly = new FoShape3D("ClockAssembly");
        
        // Create clock components here
        // TODO: Add clock face, hands, etc.
        
        return assembly;
    }

    private void UpdateClock(object? state)
    {
        // TODO: Update clock hands based on current time
        var now = DateTime.Now;
        // Animation logic here
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _clockTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

## Key Patterns Demonstrated

### 1. MxObject Lifecycle
```csharp
public class ClockTech : MxComponent
{
    public ClockTech(string name, IFoundryService service) : base(name)
    {
        // MxComponent requires name parameter
    }
    
    protected override void Dispose(bool disposing)
    {
        // Clean disposal pattern
        base.Dispose(disposing);
    }
}
```

### 2. Stage Management
```csharp
// Create dedicated stage per component
_clockStage = arena.EstablishStage<FoStage3D>("ClockDemoStage");

// Add shapes to isolated stage
_clockStage.AddShape(myShape);
```

### 3. Animation Integration
```csharp
private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _currentFps = evt.fps;
        _currentTick = evt.tick;
        InvokeAsync(StateHasChanged);
    }
}
```

### 4. Modern Logging
```csharp
"Operation completed successfully".WriteSuccess();
"Information message".WriteInfo();
"Warning about performance".WriteWarning();
"Error occurred".WriteError();
```

## Testing Checklist

### Basic Functionality
- [ ] Page loads without compilation errors
- [ ] Canvas renders properly
- [ ] FPS counter updates in real-time
- [ ] Buttons respond to clicks

### MxObject Integration  
- [ ] Stage created with correct name
- [ ] Shapes added to dedicated stage
- [ ] Clean disposal on page navigation
- [ ] No memory leaks

### Performance
- [ ] Maintains 60 FPS with multiple shapes
- [ ] Smooth animation updates
- [ ] Responsive UI interactions

## Common Issues & Solutions

### Issue: Compilation Errors
**Solution**: Ensure all required using statements are present:
```csharp
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
```

### Issue: Canvas Not Rendering
**Solution**: Verify Canvas3DComponent reference and scene access:
```csharp
var (found, scene) = _canvasRef.GetActiveScene();
if (!found) return; // Canvas not ready
```

### Issue: Animation Not Working
**Solution**: Confirm animation subscription:
```csharp
AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
// Must unsubscribe in Dispose()!
```

## Next Steps

1. **Extend Clock Features**: Add hour/minute/second hands
2. **Add More Shapes**: Implement T-Rex, submarine, geometry
3. **Performance Optimization**: Monitor FPS and optimize as needed
4. **Error Handling**: Add try-catch blocks for robustness
5. **Documentation**: Create usage examples and troubleshooting guide

## Success Criteria

**Technical**:
- ✅ Zero compilation errors
- ✅ Clean MxObject integration  
- ✅ Proper resource disposal
- ✅ 60 FPS performance

**Educational**:
- ✅ Clear MxObject patterns demonstrated
- ✅ Reusable code templates
- ✅ Comprehensive documentation
- ✅ Migration pathway established