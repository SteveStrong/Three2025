# Canvas Component Setup Pattern

## The Correct Pattern for Razor Pages with Canvas Components

### OnAfterRenderAsync - Initial Geometry Creation

The `OnAfterRenderAsync(firstRender)` method is where you:

1. **Get references to Canvas resources** (Stage, Scene, Page)
2. **Render initial geometry immediately** using the KN→FO pipeline
3. **Then subscribe to animation** for ongoing updates (if needed)

### Example: Proper Canvas Setup

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && Canvas3DReference != null)
    {
        // 1. Wait for canvas initialization
        await Task.Delay(200);
        
        // 2. Get stage/scene references from Canvas
        _stage = Canvas3DReference.Stage;
        var (found, scene) = Canvas3DReference.GetActiveScene();
        
        // 3. IMMEDIATELY render initial geometry using KN→FO pipeline
        RenderInitialGeometry();
        
        // 4. THEN subscribe to animation for updates (optional)
        AnimationFrameBus.SubscribeToComputeGeometry(OnComputeGeometry);
        AnimationFrameBus.ResumeAllAnimations();
        
        await InvokeAsync(StateHasChanged);
    }
}

private void RenderInitialGeometry()
{
    if (_testComponent == null || _stage == null) return;
    
    var arena = Workspace?.GetArena();
    if (arena == null) return;
    
    // Use RenderGeometry3D to create shape via KN→FO pipeline
    var ctx = RenderContext3D.Create(arena, "MyView", deep: false);
    _testComponent.RenderGeometry3D(ctx);
    
    // Retrieve the created shape from cache
    var shape = _geomParam?.GetCashe<FoShape3D>();
    if (shape != null)
    {
        _currentShape = shape;
        // Shape is already added to stage by RenderContext.PostCreation
    }
}
```

### Why This Pattern?

**OnAfterRenderAsync is for initial creation, not animation**
- The Canvas is ready
- Stage/Scene are initialized and linked
- This is when shapes should be created and added
- Animation is only for ongoing updates, NOT initial render

**Wrong Pattern (DO NOT DO THIS):**
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _stage = Canvas3DReference.Stage;
        
        // ❌ WRONG: Subscribing to animation without initial render
        AnimationFrameBus.SubscribeToComputeGeometry(OnComputeGeometry);
        AnimationFrameBus.ResumeAllAnimations();
        // Nothing appears on screen because no initial geometry was created!
    }
}
```

### Key Principles

1. **OnAfterRenderAsync = Initial Creation**
   - Get Canvas resources
   - Call RenderGeometry3D/RenderDrawing2D immediately
   - Shapes appear on screen

2. **Animation Loop = Ongoing Updates**
   - Parameter changes
   - Transformations
   - Dynamic behavior

3. **RenderContext.PostCreation adds shapes to Stage**
   - Don't manually call `_stage.AddShape()` when using RenderGeometry3D
   - PostCreation handles this automatically

4. **GetCashe() vs GetCurrentValueAs()**
   - After RenderGeometry3D, use `GetCashe()` to retrieve shape
   - `GetCurrentValueAs()` triggers evaluation - avoid in animation loops

### Reference Examples

- ✅ [SpacialBoxTest.razor.cs](Components/Pages/SpacialBoxTest.razor.cs) - Proper pattern
- ✅ [KnModelAnimationTest.razor.cs](Components/Pages/KnModelAnimationTest.razor.cs) - Proper pattern
- ✅ [GeometryParameterTestHarness.razor.cs](Components/Pages/GeometryParameterTestHarness.razor.cs) - Fixed pattern
- ✅ [DualCanvas2D3DTest.razor.cs](Components/Pages/DualCanvas2D3DTest.razor.cs) - Fixed pattern
