# Animated Geometry Page Template

**Purpose**: Minimal template for creating a Blazor page with animated, parameter-driven 3D geometry.

**Last Updated**: December 21, 2025  
**Reference Implementation**: `GeometryDebugTest.razor.cs`

---

## The Complete Pattern

### Required Files

1. **YourPage.razor** - Blazor markup
2. **YourPage.razor.cs** - Code-behind with animation subscriptions
3. **YourComponent.cs** - KnComponent subclass with geometry logic

---

## Code-Behind Template (YourPage.razor.cs)

```csharp
using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryMentorModeler.Model;

#nullable enable

namespace YourNamespace;

public partial class YourPage : ComponentBase, IDisposable
{
    [Inject] public IWorkspace Workspace { get; init; } = null!;
    [Inject] public IMentorServices MentorServices { get; init; } = null!;
    [Inject] public IModelEditor ModelEditor { get; init; } = null!;

    public Canvas3DComponent? Canvas3DReference = null;
    
    private AnimatedKnModel? _model;
    private YourComponent? _component;
    private FoStage3D? _stage;
    
    // STEP 1: Initialize and Subscribe
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // CRITICAL: Subscribe to animation events
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        
        // Optional: Subscribe for tree refresh
        MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(OnModelEditChanged);
        
        // Create model
        _model = MentorServices.EstablishModel<AnimatedKnModel>("YourModel");
        _model.SetExpanded(true);
        
        // Create component with initial parameters
        _component = new YourComponent("YourComponent", /* parameters */);
        
        // Add to model
        ModelEditor.AddChild(_model, _component);
    }

    // STEP 2: Handle Animation Events - Render Geometry
    private void OnAnimationEvent(AnimationEvent evt)
    {
        // This method is called every animation frame
        if (_stage != null && _model != null && evt.IsWorld3D())
        {
            // Create render context from stage
            var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
            
            // Walk model tree and render all geometry
            _model.RenderGeometry3D(ctx);
        }
    }

    // STEP 3: Optional - Handle Model Edit Events for Tree Refresh
    private void OnModelEditChanged(ModelEditChanged message)
    {
        InvokeAsync(StateHasChanged);
    }

    // STEP 4: Acquire Stage Reference After First Render
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Wait for canvas initialization
            await Task.Delay(200);
            
            _stage = Canvas3DReference?.Stage;
            if (_stage != null && _component != null)
            {
                // Establish initial geometry parameter
                var view = _stage.GetName();
                var (geometry, parameter) = ModelEditor.EstablishGeometry3D(_component, view);
                
                // Start animations
                AnimationFrameBus.ResumeAllAnimations();
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    // STEP 5: Update Parameters (triggers automatic re-rendering)
    private void UpdateParameter(string paramName, string value)
    {
        if (_component == null) return;
        
        // ModelEditor.SetParameter triggers Smash cascade
        // OnAnimationEvent will call RenderGeometry3D on next frame
        ModelEditor.SetParameter(_component, paramName, value);
    }

    // STEP 6: Cleanup - Unsubscribe from Events
    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
        MentorServices?.PubSub?.UnSubscribeFrom<ModelEditChanged>(OnModelEditChanged);
    }
}
```

---

## Razor Markup Template (YourPage.razor)

```razor
@page "/your-page"

@using FoundryWorldsAndDrawings.Shared
@using FoundryWorldsAndDrawings.Shape
@using FoundryMentorModeler.Model

@rendermode InteractiveServer

<PageTitle>Your Page</PageTitle>

<div class="container-fluid">
    <h3>Your Page Title</h3>
    
    <div class="row">
        <!-- Controls -->
        <div class="col-md-4">
            <div class="card mb-3">
                <div class="card-header">Controls</div>
                <div class="card-body">
                    <button class="btn btn-primary" 
                            @onclick="@(() => UpdateParameter("SomeParam", "NewValue"))">
                        Update Parameter
                    </button>
                </div>
            </div>
        </div>
        
        <!-- 3D Canvas -->
        <div class="col-md-5">
            <div class="card">
                <div class="card-header">3D View</div>
                <div class="card-body p-0" style="height: 400px;">
                    <Canvas3DComponent SceneName="YourScene3D" @ref="Canvas3DReference" />
                </div>
            </div>
        </div>
        
        <!-- Model Tree -->
        <div class="col-md-3">
            <div class="card">
                <div class="card-header">Model</div>
                <div class="card-body">
                    <MentorTreeView/>
                </div>
            </div>
        </div>
    </div>
</div>
```

---

## Component Template (YourComponent.cs)

```csharp
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;

#nullable enable

namespace YourNamespace;

public class YourComponent : PartComponent
{
    public YourComponent(string name, /* parameters */) : base(name)
    {
        // Define parameters using Calculations helper
        Calculations([
            "Width|m: 1.0",
            "Height|m: 2.0",
            "Color: 'Blue'"
        ]);
        
        // Optional: Set up animation callback
        PreAnimationRefresh((comp, evt) =>
        {
            // Update parameters based on animation
            // ModelEditor.SetParameter will be called from page
        });
    }

    // Establish 3D geometry for a view
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            // Three-phase geometry computation
            geom.ApplyMeshMethod("ComputeMesh3D", ComputeMesh3D, null);
            geom.ApplyTransformMethod("ComputeTransform3D", ComputeTransform3D, null);
            geom.ApplyBodyMethod("ComputeBody3D", ComputeBody3D, null);
        });
        
        return (result, result.GetBodyParameter());
    }

    // Phase 1: Create and cache mesh geometry
    private bool ComputeMesh3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        // Get parameter values (triggers dependency discovery)
        var (foundW, width) = FindNumberValue("Width");
        var (foundH, height) = FindNumberValue("Height");
        
        if (!foundW || !foundH) return false;
        
        // Check if mesh already cached
        if (!context.IsCasheEmpty("MeshCache"))
        {
            var cached = context.GetCashe("MeshCache");
            result.SetValue(ResultStatus.Mesh3D, cached);
            return true;
        }
        
        // Create new mesh
        var mesh = FoMesh3D.CreateBox(width.Value, height.Value, 1.0, "white");
        context.SetCashe("MeshCache", mesh);
        result.SetValue(ResultStatus.Mesh3D, mesh);
        return true;
    }

    // Phase 2: Compute transform (always evaluated)
    private bool ComputeTransform3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var transform = new FoTransform3D();
        transform.MoveTo(0, 0, 0);
        result.SetValue(ResultStatus.Transform3D, transform);
        return true;
    }

    // Phase 3: Compose body from mesh + transform
    private bool ComputeBody3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var (foundM, meshParam) = FindParameter("Mesh3D");
        var (foundT, transformParam) = FindParameter("Transform3D");
        
        if (!foundM || !foundT) return false;
        
        var meshResult = meshParam.GetCurrentValue();
        var transformResult = transformParam.GetCurrentValue();
        
        var mesh = meshResult.AsMesh3D();
        var transform = transformResult.AsTransform3D();
        
        if (mesh == null || transform == null) return false;
        
        // Get color
        var (foundC, color) = FindStringValue("Color");
        if (foundC) mesh.SetColor(color.Value);
        
        // Create shape with stable GlyphId
        var shape = new FoShape3D(Name, GetKnowId(), mesh, transform);
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
}
```

---

## Critical Points

### 1. AnimationEvent Subscription is REQUIRED
Without `AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent)`, parameter changes will trigger Smash but geometry will never re-evaluate.

### 2. OnAnimationEvent Must Call RenderGeometry3D
```csharp
private void OnAnimationEvent(AnimationEvent evt)
{
    if (_stage != null && _model != null && evt.IsWorld3D())
    {
        var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
        _model.RenderGeometry3D(ctx);  // ← Walks tree, evaluates geometry
    }
}
```

### 3. Canvas3DComponent Handles RenderStage Automatically
You don't need to call `stage.RenderStage()` - Canvas3DComponent subscribes to AnimationEvent and does this automatically.

### 4. Always Dispose Properly
```csharp
public void Dispose()
{
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
    MentorServices?.PubSub?.UnSubscribeFrom<ModelEditChanged>(OnModelEditChanged);
}
```

---

## The Complete Flow

```
User clicks button
  → UpdateParameter("Width", "2.0")
    → ModelEditor.SetParameter(component, "Width", "2.0")
      → Parameter marked Unknown
      → Smash cascade to dependent parameters
        → Next animation frame:
          → OnAnimationEvent called
            → model.RenderGeometry3D(ctx)
              → Walks component tree
                → component.EstablishGeometry3D(view)
                  → ComputeMesh3D: cache miss, create new mesh
                  → ComputeTransform3D: evaluate transform
                  → ComputeBody3D: compose shape
                  → Returns shape
                → ctx.PostCreation(shape, glyphId)
                  → Adds shape to stage
          → Canvas3DComponent.OnAnimationEvent
            → stage.RenderStage(tick, fps)
              → Sends shapes to JavaScript/Three.js
```

---

## Common Mistakes

### ❌ Not Subscribing to AnimationEvent
```csharp
protected override void OnInitialized()
{
    _model = MentorServices.EstablishModel<AnimatedKnModel>("Model");
    // Missing: AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
}
```
**Result**: Parameter changes trigger Smash but nothing re-renders.

### ❌ Manually Calling RenderStage
```csharp
private void OnAnimationEvent(AnimationEvent evt)
{
    var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
    _model.RenderGeometry3D(ctx);
    await _stage.RenderStage(tick, fps);  // ← DON'T DO THIS
}
```
**Result**: Canvas3DComponent already does this - you'll render twice per frame.

### ❌ Forgetting to Dispose
```csharp
public void Dispose()
{
    // Missing: AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
}
```
**Result**: Memory leaks, event handlers persist after page navigation.

---

**See Also**:
- `GeometryDebugTest.razor.cs` - Reference implementation
- `KN_FO_ANIMATION_INTEGRATION.md` - Complete animation cycle explanation
- `KNCOMPONENT_FRAMEWORK_PATTERN.md` - Framework patterns and anti-patterns
