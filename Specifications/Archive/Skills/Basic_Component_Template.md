# Basic MxObject Component Template Skill

## Skill Overview
**Purpose**: Create a minimal MxObject-based Blazor component  
**Difficulty**: Beginner  
**Prerequisites**: Basic Blazor knowledge, FoundryMicroCore.Library reference  
**Output**: Working template for new MxObject components  

## What You'll Learn
- ✅ Minimal MxObject component structure
- ✅ Required using statements and dependencies
- ✅ Basic stage management pattern
- ✅ Clean disposal implementation
- ✅ Animation subscription basics

## Template Files

### Razor Markup Template
**File**: `MyComponent.razor`

```razor
@page "/my-component"
@namespace Three2025.Components.Pages
@inherits MyComponentBase
@rendermode InteractiveServer

<PageTitle>My Component - MxObject</PageTitle>

<h3>My MxObject Component</h3>

<!-- Status Display -->
<div class="status-section">
    <div><strong>Status:</strong> @(_isInitialized ? "Ready" : "Initializing")</div>
    <div><strong>Stage:</strong> @(_stage?.Name ?? "Not created")</div>
</div>

<!-- Canvas -->
<div class="canvas-section">
    <Canvas3DComponent SceneName="MyComponent" @ref="_canvasRef" 
                      CanvasWidth="600" CanvasHeight="400" />
</div>

<!-- Controls -->
<div class="controls-section">
    <button @onclick="DoSomething" disabled="@(!_isInitialized)">
        Do Something
    </button>
    <button @onclick="ClearAll" disabled="@(!_isInitialized)">
        Clear All
    </button>
</div>

<style>
.status-section, .controls-section {
    margin: 1rem 0;
    padding: 1rem;
    background: #f8f9fa;
    border-radius: 4px;
}

.canvas-section {
    margin: 2rem 0;
    border: 1px solid #dee2e6;
    border-radius: 4px;
}

button {
    margin-right: 0.5rem;
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 4px;
    background: #007bff;
    color: white;
}

button:disabled {
    background: #6c757d;
}
</style>
```

### Code-Behind Template  
**File**: `MyComponent.razor.cs`

```csharp
#nullable enable

using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.PubSub;

namespace Three2025.Components.Pages;

/// <summary>
/// Basic MxObject Component Template
/// Demonstrates minimal clean MxObject patterns
/// </summary>
public partial class MyComponentBase : ComponentBase, IDisposable
{
    #region Dependency Injection
    
    [Inject] public required IFoundryService FoundryService { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }
    
    #endregion

    #region Protected Fields (Razor Access)
    
    protected Canvas3DComponent? _canvasRef;
    protected FoStage3D? _stage;
    protected bool _isInitialized = false;
    
    #endregion

    #region Private Fields
    
    private bool _animationSubscribed = false;
    
    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Create dedicated stage for this component
            var arena = FoundryService.Arena();
            _stage = arena.EstablishStage<FoStage3D>("MyComponentStage");
            
            "MyComponent: Stage created successfully".WriteSuccess();
            
            // Subscribe to animation if needed
            AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
            _animationSubscribed = true;
            
            _isInitialized = true;
            "MyComponent: Initialization complete".WriteInfo();
            
            await base.OnInitializedAsync();
        }
        catch (Exception ex)
        {
            $"MyComponent initialization failed: {ex.Message}".WriteError();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _canvasRef != null)
        {
            await Task.Delay(100); // Allow canvas initialization
            
            var (found, scene) = _canvasRef.GetActiveScene();
            if (found && scene != null)
            {
                $"MyComponent: Canvas ready with scene '{scene.Name}'".WriteSuccess();
                
                // Perform any first-render setup here
                await OnCanvasReady();
            }
            else
            {
                "MyComponent: Canvas scene not available".WriteWarning();
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    #endregion

    #region Animation Handling

    private void OnAnimationFrame(AnimationEvent evt)
    {
        if (evt.IsWorld3D())
        {
            // Handle animation frame updates here
            // Example: Update UI based on animation state
            // InvokeAsync(StateHasChanged);
        }
    }

    #endregion

    #region Event Handlers

    protected async Task OnCanvasReady()
    {
        // Override this method for canvas-specific initialization
        "MyComponent: Canvas is ready for use".WriteInfo();
    }

    protected void DoSomething()
    {
        if (_stage == null)
        {
            "Stage not available".WriteWarning();
            return;
        }

        try
        {
            // Example: Add a simple shape
            var shape = new FoShape3D($"Shape_{Guid.NewGuid():N[..6]}")
            {
                Color = "Blue",
                Transform = new Transform3("ShapeTransform")
                {
                    Position = new Vector3(0, 1, 0)
                }
            };

            _stage.AddShape(shape);
            $"Added shape '{shape.Name}' to stage".WriteSuccess();
        }
        catch (Exception ex)
        {
            $"Error adding shape: {ex.Message}".WriteError();
        }
    }

    protected void ClearAll()
    {
        if (_stage != null)
        {
            try
            {
                // TODO: Replace with correct clearing API when available
                // _stage.ClearAllShapes();
                "Clear all requested (method not yet available)".WriteInfo();
            }
            catch (Exception ex)
            {
                $"Error clearing shapes: {ex.Message}".WriteError();
            }
        }
    }

    #endregion

    #region Disposal

    public void Dispose()
    {
        try
        {
            // Unsubscribe from animation
            if _animationSubscribed)
            {
                AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
                _animationSubscribed = false;
                "MyComponent: Animation unsubscribed".WriteInfo();
            }

            // Clear stage resources (when API available)
            if (_stage != null)
            {
                // TODO: Clear stage shapes when API is available
                "MyComponent: Stage cleanup requested".WriteInfo();
            }

            "MyComponent: Disposed cleanly".WriteSuccess();
        }
        catch (Exception ex)
        {
            $"MyComponent disposal error: {ex.Message}".WriteError();
        }
    }

    #endregion
}
```

## Key Patterns Explained

### 1. Protected Fields for Razor Access
```csharp
// Must be protected for Razor binding
protected bool _isInitialized = false;
protected FoStage3D? _stage;
```

### 2. Stage Creation Pattern
```csharp
var arena = FoundryService.Arena();
_stage = arena.EstablishStage<FoStage3D>("MyComponentStage");
```

### 3. Animation Subscription Pattern
```csharp
// Subscribe in OnInitializedAsync
AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);

// Always unsubscribe in Dispose
AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
```

### 4. Canvas Integration Pattern
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && _canvasRef != null)
    {
        await Task.Delay(100); // Allow initialization
        var (found, scene) = _canvasRef.GetActiveScene();
        if (found && scene != null)
        {
            await OnCanvasReady(); // Custom setup
        }
    }
}
```

### 5. Modern Logging Pattern
```csharp
"Operation completed".WriteSuccess();    // Green
"Information message".WriteInfo();       // Blue  
"Warning message".WriteWarning();        // Yellow
"Error occurred".WriteError();           // Red
```

## Usage Instructions

1. **Copy both template files** to your component location
2. **Rename files** to match your component name
3. **Replace "MyComponent"** with your actual component name throughout
4. **Update the @page directive** to your desired route
5. **Customize the DoSomething() method** for your specific functionality
6. **Add any additional fields/methods** as needed
7. **Test thoroughly** to ensure clean initialization and disposal

## Testing Checklist

- [ ] Component loads without errors
- [ ] Stage is created successfully  
- [ ] Canvas renders properly
- [ ] Buttons respond correctly
- [ ] Animation subscription works (if used)
- [ ] Clean disposal on navigation
- [ ] No console errors or warnings

## Common Customizations

### Add Performance Monitoring
```csharp
protected double _currentFps = 0.0;

private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _currentFps = evt.fps;
        InvokeAsync(StateHasChanged);
    }
}
```

### Add Shape Counter
```csharp
protected int _shapeCount = 0;

protected void DoSomething()
{
    // ... create shape ...
    _shapeCount++;
    StateHasChanged();
}
```

### Add Error Handling
```csharp
protected string _lastError = "";

protected void DoSomething()
{
    try
    {
        // ... operation ...
        _lastError = ""; // Clear on success
    }
    catch (Exception ex)
    {
        _lastError = ex.Message;
        ex.Message.WriteError();
    }
    StateHasChanged();
}
```

## Next Steps

1. **Test the template** with minimal changes first
2. **Add your specific functionality** incrementally  
3. **Follow other skills** for advanced patterns
4. **Document your customizations** for future reference
5. **Share working patterns** back to the skill collection