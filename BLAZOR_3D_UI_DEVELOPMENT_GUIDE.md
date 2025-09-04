# Blazor 3D UI Development Guide

> **A comprehensive guide for building robust Blazor-based 3D visualization interfaces**

## Table of Contents
- [Essential Initialization Patterns](#essential-initialization-patterns)
- [Canvas3DComponent Setup](#canvas3dcomponent-setup)
- [Scene Management](#scene-management)
- [Architecture Patterns](#architecture-patterns)
- [Common Pitfalls & Solutions](#common-pitfalls--solutions)
- [Best Practices](#best-practices)
- [Debugging Techniques](#debugging-techniques)

---

## Essential Initialization Patterns

### Critical `OnAfterRenderAsync` Implementation

**❌ NEVER skip this pattern** - All 3D visualization components MUST implement proper initialization:

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // 1. Get active scene from canvas
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);

        // 2. Set up UI refresh event handling
        scene?.SetAfterUpdateAction((s, j) =>
        {
            FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
        });

        // 3. Connect arena to scene
        var arena = Workspace.GetArena();
        if (found)
        {
            arena.SetScene(scene!);
            
            // 4. Initialize content (optional but recommended)
            CreateInitialContent();
        }
    }
    await base.OnAfterRenderAsync(firstRender);
}
```

### Why This Pattern is Critical

1. **Scene Connection**: Links Canvas3D to the 3D rendering engine
2. **Arena Integration**: Connects workspace management to visual scene
3. **Event Propagation**: Enables UI refresh and state synchronization
4. **Automatic Content**: Provides immediate visual feedback to users

---

## Canvas3DComponent Setup

### Correct Canvas Declaration

**Razor Component (.razor)**:
```razor
<Canvas3DComponent SceneName="YourSceneName" @ref="Canvas3DReference" CanvasWidth=@CanvasWidth CanvasHeight=@CanvasHeight />
```

**Code-Behind (.razor.cs)**:
```csharp
public class YourComponentBase : ComponentBase, IDisposable
{
    // Use Canvas3DComponentBase, not Canvas3DComponent
    public Canvas3DComponentBase Canvas3DReference = null;
    
    [Parameter] public int CanvasWidth { get; set; } = 1400;
    [Parameter] public int CanvasHeight { get; set; } = 1000;
}
```

### Canvas Parameter Patterns

| ✅ **Correct** | ❌ **Incorrect** |
|----------------|------------------|
| `CanvasWidth=@CanvasWidth` | `CanvasWidth="@CanvasWidth"` |
| `SceneName="YourScene"` | Missing SceneName |
| `Canvas3DComponentBase` | `Canvas3DComponent` |

---

## Scene Management

### Required Dependency Injection

```csharp
public class YourComponentBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }
}
```

### Arena Operations

```csharp
// Adding shapes to scene
var arena = Workspace?.GetArena();
arena?.AddShapeToStage<SnapBox>(yourShape);

// Updating positions
yourShape.SetPosition(x, y, z);
var (found, scene) = arena?.CurrentScene() ?? (false, null);
if (found && scene != null)
{
    yourShape.RefreshToScene(scene);
}

// Clearing scene
arena?.ClearArena();
```

---

## Architecture Patterns

### Leverage Existing Geometry Systems

**✅ DO**: Use SpacialFrame3D for geometry calculations
```csharp
public class SnapBox : FoShape3D, ISnappable3D
{
    private SpacialFrame3D _spatialFrame;
    
    public SnapBox(string name, double width, double height, double depth, string color)
        : base(name, color)
    {
        _spatialFrame = new SpacialFrame3D(width, height, depth);
        // Leverage existing face calculations, normals, transforms
    }
    
    public Dictionary<string, Face3D> Faces => _spatialFrame.Faces;
}
```

**❌ DON'T**: Duplicate geometry calculations
```csharp
// Don't recreate what SpacialFrame3D already provides
private void CalculateOwnFaces() { /* redundant work */ }
```

### Inheritance vs Composition

**Preferred Pattern**: Inheritance + Delegation
```csharp
public class SnapBox : FoShape3D, ISnappable3D
{
    private SpacialFrame3D _spatialFrame; // Delegate geometry to this
    
    // Inherit from FoShape3D for visual integration
    // Implement ISnappable3D for snapping behavior
}
```

---

## Common Pitfalls & Solutions

### 1. Missing Scene Initialization

**Problem**: 3D content doesn't render
**Symptoms**: Empty canvas, no visual feedback
**Solution**: Always implement `OnAfterRenderAsync` pattern

### 2. Incorrect Canvas Parameters

**Problem**: Canvas initialization errors
**Symptoms**: Compilation errors, runtime exceptions
**Solution**: Use exact parameter format from working examples

### 3. Arena-Scene Disconnection

**Problem**: Shapes added but not visible
**Symptoms**: Code runs without errors, nothing appears
**Solution**: Ensure `arena.SetScene(scene)` is called

### 4. Missing Event Handling

**Problem**: UI doesn't update when shapes change
**Symptoms**: Stale visual state, manual refresh needed
**Solution**: Set up `RefreshUIEvent` publishing

### 5. Geometry System Duplication

**Problem**: Inconsistent calculations, maintenance burden
**Symptoms**: Normals don't match rotations, face calculations differ
**Solution**: Use SpacialFrame3D as single source of truth

---

## Best Practices

### Component Structure

1. **Inherit from ComponentBase**: Enable Blazor lifecycle
2. **Implement IDisposable**: Clean up resources
3. **Use proper injection**: Get required services
4. **Follow naming conventions**: Match established patterns

### Visualization Methods

```csharp
public void ShowFaces()
{
    var arena = Workspace?.GetArena();
    if (arena == null) return;
    
    var faces = component.Faces.Values.ToList();
    VisualizationService.ShowWireframeFaces(arena, faces);
    
    StatusMessage = $"Showing {faces.Count} faces";
    StateHasChanged();
}

public void ShowNormals()
{
    var arena = Workspace?.GetArena();
    if (arena == null) return;
    
    var faces = component.Faces.Values.ToList();
    VisualizationService.ShowLabeledNormals(arena, faces);
    
    StatusMessage = $"Showing {faces.Count} normals";
    StateHasChanged();
}

// Use service utilities for custom markers
public void ShowSnapPoints()
{
    var arena = Workspace?.GetArena();
    if (arena == null) return;
    
    foreach (var snapPoint in component.SnapPoints.Values)
    {
        VisualizationService.CreateMarkerSphere(arena, $"Snap_{snapPoint.Name}", 
            snapPoint.WorldPosition, "#FF9800", 0.05);
    }
}

public void ShowDebugMarkers()
{
    var arena = Workspace?.GetArena();
    if (arena == null) return;
    
    // Create coordinate markers at key points
    VisualizationService.CreateCoordinateMarker(arena, "Origin", new Point3D(0, 0, 0), 0.2);
    
    // Create custom cylinders for directions
    VisualizationService.CreateMarkerCylinder(arena, "Direction", 
        centerPoint, rotation, "red", 0.03, 1.0);
}
```

### State Management

```csharp
// Always update UI after operations
StatusMessage = "Operation completed";
StateHasChanged();

// Check for null references
var arena = Workspace?.GetArena();
if (arena == null) return;

// Provide user feedback
try
{
    // Operation
    StatusMessage = "✅ Success";
}
catch (Exception ex)
{
    StatusMessage = $"❌ Error: {ex.Message}";
}
finally
{
    StateHasChanged();
}
```

---

## Debugging Techniques

### 1. Focused Test Components

Create minimal test pages to isolate specific functionality:
```csharp
// Example: NormalVisualizationTest.razor
// Purpose: Debug face normal visualization with rotation
// Scope: Single SnapBox with rotation controls
```

### 2. Progressive Enhancement

Start simple and add complexity:
1. Basic canvas setup
2. Single shape creation
3. Position controls
4. Rotation controls
5. Visualization features
6. Advanced interactions

### 3. Consistent Logging

```csharp
StatusMessage = $"Created {component.Name}: {component.Faces.Count} faces at ({x},{y},{z})";
StatusMessage = $"Applied rotation: {rotX}°, {rotY}°, {rotZ}°";
StatusMessage = $"Showing {normalCount} normals as yellow cylinders";
```

### 4. Reference Implementation Pattern

Always compare with working examples:
- Use LegoSnappingTest.razor as reference
- Copy initialization patterns exactly
- Verify parameter usage matches

---

## Quick Checklist for New 3D UI Components

### Setup ✅
- [ ] Inherit from ComponentBase, implement IDisposable
- [ ] Add required dependency injection
- [ ] Use Canvas3DComponentBase reference
- [ ] Set correct canvas parameters

### Initialization ✅
- [ ] Implement OnAfterRenderAsync
- [ ] Get active scene from canvas
- [ ] Set up UI refresh events
- [ ] Connect arena to scene
- [ ] Create initial content

### Architecture ✅
- [ ] Use SpacialFrame3D for geometry
- [ ] Implement proper inheritance pattern
- [ ] Delegate calculations to existing systems
- [ ] Follow established naming conventions

### User Experience ✅
- [ ] Provide status messages
- [ ] Handle errors gracefully
- [ ] Update UI state appropriately
- [ ] Offer progressive disclosure of features

---

## Example Templates

### Minimal 3D Component Template

```csharp
public class MyComponent3DBase : ComponentBase, IDisposable
{
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
    
    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;
    
    protected string StatusMessage { get; set; } = "Ready";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
            scene?.SetAfterUpdateAction((s, j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = Workspace.GetArena();
            if (found)
            {
                arena.SetScene(scene!);
                CreateInitialContent();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void CreateInitialContent()
    {
        // Your initialization logic here
        StatusMessage = "3D scene initialized";
        StateHasChanged();
    }

    public void Dispose()
    {
        // Cleanup logic
    }
}
```

---

## Conclusion

This guide captures essential patterns for building robust Blazor 3D visualization UIs. The key lesson: **never skip proper initialization**, always leverage existing geometry systems, and maintain consistency with established patterns.

Remember: When in doubt, reference working implementations like `LegoSnappingTest.razor` and follow these proven patterns exactly.

---

*Last Updated: September 3, 2025*
*Version: 1.0*
