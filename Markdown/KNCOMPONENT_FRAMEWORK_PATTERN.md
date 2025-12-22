# KnComponent Framework Pattern Guide

**Purpose**: This document describes the correct pattern for creating animated, geometry-generating components that leverage the KN framework's built-in infrastructure for dependency management, rendering, and animation.

**Last Updated**: December 18, 2025  
**Reference Implementation**: `AnimatedParameterTestComponent.cs` in FoundryMentorModeler

---

## Overview

The KN framework provides automatic handling of:
- **Dependency discovery** during parameter evaluation (spreadsheet model)
- **Animation loops** via `MentorServices` and `PreAnimationEvent`
- **View routing** via `ViewRegistry` 
- **Stage management** via `RenderContext` finding stages by registered view name
- **Cache invalidation** via property setters marking stale automatically
- **Geometry rendering** via `PostCreation` adding shapes to stages

**Core Principle**: When you need 300+ lines of manual coordination code, you're fighting the framework. The correct pattern is typically ~100 lines with zero manual intervention.

---

## The Framework-Aligned Pattern

### Architecture Flow

```
AnimatedKnModel
    └─> Contains KnComponent instances
            └─> MentorServices animation loop
                    └─> Calls PreAnimationRefresh callbacks
                            └─> Updates parameters (spreadsheet evaluates)
                                    └─> Parameters marked Unknown trigger re-evaluation
                                            └─> RenderContext walks model tree
                                                    └─> Calls EstablishGeometry3D on each component
                                                            └─> Component computes geometry if applicable
                                                                    └─> Shape added to stage for view
```

### Essential Components

1. **AnimatedKnModel**: Container for components, registered with `MentorServices`
2. **KnComponent subclass**: Your model with parameters and geometry logic
3. **EstablishGeometry3D**: Override to provide geometry for a view
4. **PreAnimationRefresh**: Hook called by framework's animation loop
5. **Parameter cache**: Automatic dependency tracking via `FindNumberValue`/`FindStringValue`
6. **Model walk pattern**: RenderContext walks tree and calls EstablishGeometry3D on each component

---

## Step-by-Step Implementation

### Step 1: Define Your KnComponent Subclass

```csharp
public class AnimatedParameterTestComponent : KnComponent
{
    // Properties for UI binding (not parameters - these drive the model)
    public int CurrentTick { get; set; }
    public string Status { get; set; } = "Initializing";
    
    // Component lifecycle
    public AnimatedParameterTestComponent(string name) : base(name)
    {
        // Subscribe to animation loop
        PreAnimationRefresh((comp, evt) => 
        {
            CurrentTick = evt.tick;
            // Update any driving values here
        });
    }
}
```

**Key Points**:
- `PreAnimationRefresh` provides animation hook - called automatically by MentorServices
- Framework walks model tree and calls `EstablishGeometry3D` on each component
- NO `_mentorServices` field - base class provides `GetMentorServices()`
- NO manual `SetupDependencies()` - spreadsheet discovers during evaluation

### Step 2: Create Parameters (Spreadsheet Model)

```csharp
public AnimatedParameterTestComponent(string name) : base(name)
{
    // Create parameters as KnNumber instances
    var xParam = new KnNumber(this, "X_Position")
    {
        Expression = "0.0"  // Initial value
    };
    
    var yParam = new KnNumber(this, "Y_Position")
    {
        Expression = "0.0"
    };
    
    // Parameters auto-register with component's parameter list
    // NO manual dependency wiring needed
}
```

**Key Points**:
- Parameters are `KnNumber`, `KnString`, etc. - stored in component's parameter list
- `Expression` property holds formula (can reference other parameters)
- Dependencies discovered automatically when formula evaluates (calls `FindNumberValue`)
- NO manual `IDependOn` wiring
- NO manual `SetupDependencies()` calls

### Step 3: Define Geometry Creation (EstablishGeometry3D)

```csharp
public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view)
{
    // Use Compute3DGeometry helper to create geometry parameter
    var result = Compute3DGeometry(view, geom =>
    {
        geom.ApplyMethod("ComputeTestGeometry", ComputeTestShape3D, null, null);
    });
    
    return (result, result.GetParameter());
}

private bool ComputeTestShape3D(KnInstance context, List<OPResult> args, OPResult result)
{
    // Get parameter values - evaluation happens here, establishes dependencies
    var xValue = FindNumberValue("X_Position", 0.0);
    var yValue = FindNumberValue("Y_Position", 0.0);
    
    // Create shape with stable GlyphId
    var shape = new FoShape3D($"TestShape_{Name}")
    {
        GlyphId = GetKnowId(),  // Stable identity across re-evaluations
        Transform = new Transform3("Transform")
        {
            Position = new Vector3(xValue, yValue, 0)
        }
    };
    
    shape.CreateBox("TestBox", 1.0, 1.0, 1.0);
    
    // Return shape via result
    result.SetValue(ResultStatus.Shape3D, shape);
    return true;
}
```

**Key Points**:
- Override `EstablishGeometry3D` - framework calls during model walk
- `Compute3DGeometry` helper creates KnGeometry with compute method
- `FindNumberValue`/`FindStringValue` trigger dependency discovery during evaluation
- Use `GetKnowId()` for stable GlyphId (persists across parameter changes)
- Return shape via `result.SetValue(ResultStatus.Shape3D, shape)`
- Framework walks model tree, calls this method, renders returned geometry
- NO manual `RenderContext` calls
- NO manual stage references

### Step 4: Wire into UI/Test Harness (CRITICAL PATTERN)

```csharp
public partial class GeometryDebugTest : ComponentBase, IDisposable
{
    [Inject] public IMentorServices MentorServices { get; init; } = null!;
    [Inject] public IModelEditor ModelEditor { get; init; } = null!;
    
    public Canvas3DComponent? Canvas3DReference = null;
    private AnimatedKnModel? _testModel;
    private DebugGeometryComponent? _testComponent;
    private FoStage3D? _testStage;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // CRITICAL: Subscribe to animation events to render geometry each frame
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        
        // Optional: Subscribe for tree refresh
        MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(OnModelEditChanged);
        
        // Create model - registers with MentorServices
        _testModel = MentorServices.EstablishModel<AnimatedKnModel>("GeomDebugModel");
        _testModel.SetExpanded(true);
        
        // Create component
        _testComponent = new DebugGeometryComponent("DebugShape", "Blue", new Vector3(0, 1, 0));
        
        // Add to model
        ModelEditor.AddChild(_testModel, _testComponent);
    }
    
    // CRITICAL: This handler drives geometry rendering
    private void OnAnimationEvent(AnimationEvent evt)
    {
        if (_testStage != null && _testModel != null && evt.IsWorld3D())
        {
            var ctx = RenderContext3D.CreateFromStage(_testStage, deep: true);
            _testModel.RenderGeometry3D(ctx);  // Walks tree, evaluates geometry
        }
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(200); // Wait for canvas init
            
            _testStage = Canvas3DReference?.Stage;
            if (_testStage != null)
            {
                // Establish initial geometry
                var view = _testStage.GetName();
                var (geometry, parameter) = ModelEditor.EstablishGeometry3D(_testComponent, view);
                
                // Start animations
                AnimationFrameBus.ResumeAllAnimations();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    
    // UI can update parameters directly
    private void SetGeometryType(string geomType)
    {
        // ModelEditor.SetParameter triggers Smash cascade
        ModelEditor.SetParameter(_testComponent, "GeometryType", $"'{geomType}'");
        // OnAnimationEvent will call RenderGeometry3D on next frame
    }
    
    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
        MentorServices?.PubSub?.UnSubscribeFrom<ModelEditChanged>(OnModelEditChanged);
    }
}
```

**Key Points**:
- `EstablishModel<AnimatedKnModel>` registers with MentorServices
- `ModelEditor.AddChild` adds component to model
- **CRITICAL**: `AnimationFrameBus.SubscribeToAnimation` - without this, geometry never renders!
- `OnAnimationEvent` calls `model.RenderGeometry3D(ctx)` - walks tree and evaluates geometry
- `ModelEditor.SetParameter` triggers Smash cascade
- Framework evaluates parameters when Unknown
- Canvas3DComponent automatically calls RenderStage to send to JavaScript
- Always unsubscribe in Dispose

---

## Anti-Patterns: What NOT to Do

### ❌ Missing AnimationEvent Subscription

```csharp
// WRONG - No AnimationEvent subscription
protected override void OnInitialized()
{
    _testModel = MentorServices.EstablishModel<AnimatedKnModel>("Model");
    ModelEditor.AddChild(_testModel, _testComponent);
    // Parameter changes trigger Smash but geometry never re-evaluates!
}

// RIGHT - Subscribe to AnimationEvent
protected override void OnInitialized()
{
    AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);  // ← CRITICAL
    _testModel = MentorServices.EstablishModel<AnimatedKnModel>("Model");
    ModelEditor.AddChild(_testModel, _testComponent);
}

private void OnAnimationEvent(AnimationEvent evt)
{
    if (_testStage != null && _testModel != null && evt.IsWorld3D())
    {
        var ctx = RenderContext3D.CreateFromStage(_testStage, deep: true);
        _testModel.RenderGeometry3D(ctx);  // ← Walks tree and evaluates
    }
}
```

### ❌ Manual Rendering

```csharp
// WRONG - Manual RenderContext calls
private async Task RenderInitialGeometry()
{
    var renderContext = new RenderContext(MentorServices, "GeomTestHarness3D");
    await renderContext.RenderGeometry3D(_currentComponent);
}

// RIGHT - Framework walks model via OnAnimationEvent
private void OnAnimationEvent(AnimationEvent evt)
{
    var ctx = RenderContext3D.CreateFromStage(_testStage, deep: true);
    _testModel.RenderGeometry3D(ctx);  // Walks tree automatically
}
```

### ❌ Manual Cache Management

```csharp
// WRONG - Manual cache inspection/clearing
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    _currentComponent.ClearCashe();
    var cache = _currentComponent.GetCashe();
    foreach (var entry in cache) { /* ... */ }
}

// RIGHT - Parameters evaluate when Unknown automatically
// Just update parameter.Expression - framework handles cache
```

### ❌ Manual Dependency Wiring

```csharp
// WRONG - Manual IDependOn setup
private void SetupDependencies()
{
    var xParam = FindNumberValue("X_Position").Item2;
    var yParam = FindNumberValue("Y_Position").Item2;
    yParam.AddDependency(xParam);
}

// RIGHT - Dependencies discovered during evaluation
private void RenderGeometry3D(...)
{
    // Calling FindNumberValue establishes dependency automatically
    var (foundX, xValue) = FindNumberValue("X_Position");
}
```

### ❌ Manual Stage References

```csharp
// WRONG - Holding stage reference in model-centric code
public class MyComponent : KnComponent
{
    private FoStage3D _stage;  // ❌ NO!
    
    private void OnAfterRender()
    {
        _stage = Canvas3DReference.Stage;
    }
}

// RIGHT - Framework routes geometry to stage via view name
public class MyComponent : KnComponent
{
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view)
    {
        // RenderContext calls this, routes geometry to appropriate stage
        var result = Compute3DGeometry(view, geom => { /* ... */ });
        return (result, result.GetParameter());
    }
}
```

### ❌ Manual Transform Updates

```csharp
// WRONG - Manual stale marking
shape.Transform.Position = newPosition;
shape.SetTransformStale();

// RIGHT - Property setter handles automatically
shape.Transform.Position = newPosition;
// SetTransformStale() called inside setter
```

### ❌ Manual Shape Tracking

```csharp
// WRONG - Manual shape lifecycle tracking
private FoShape3D _currentShape;
private FoShape3D _previousShape;

private void UpdateGeometry()
{
    if (_previousShape != null)
        _stage.RemoveShape(_previousShape);
    
    _currentShape = new FoShape3D(...);
    _stage.AddShape(_currentShape);
    _previousShape = _currentShape;
}

// RIGHT - Use stable GlyphId and GetShape
private void RenderGeometry3D(...)
{
    var glyphId = $"Shape_{GetKnowId()}";
    var shape = GetShape(glyphId);
    if (shape == null)
    {
        shape = new FoShape3D(glyphId, "color");
        // Framework adds to stage via RenderContext
    }
    // Update existing shape
    shape.Transform.Position = newPosition;
}
```

### ❌ Manual MentorServices Field

```csharp
// WRONG - Storing MentorServices reference
public class MyComponent : KnComponent
{
    private IMentorServices _mentorServices;
    
    public MyComponent(IMentorServices services) : base("...")
    {
        _mentorServices = services;  // ❌ NO!
    }
}

// RIGHT - Use base class method
public class MyComponent : KnComponent
{
    private void SomeMethod()
    {
        var services = GetMentorServices();  // ✅ YES!
    }
}
```

---

## Complete Reference Example

See `AnimatedParameterTestComponent.cs` in FoundryMentorModeler for full implementation (~100 lines):

```csharp
public class AnimatedParameterTestComponent : KnComponent
{
    public int CurrentTick { get; set; }
    public string Status { get; set; } = "Initializing";
    
    public AnimatedParameterTestComponent(string name) : base(name)
    {
        // 1. Subscribe to animation
        PreAnimationRefresh((comp, evt) => 
        {
            CurrentTick = evt.tick;
            Status = $"Tick {evt.tick} @ {evt.fps:F1} FPS";
        });
        
        // 2. Create parameters using Calculations helper
        Calculations([
            "X_Position: 0.0",
            "Y_Position: 0.0",
            "ShapeColor: 'cyan'"
        ]);
        
        // 3. Establish geometry - called by framework during model walk
        EstablishGeometry3D("GeomTestHarness3D");
    }
    
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMethod("ComputeTestGeometry", ComputeTestShape3D, null, null);
        });
        
        return (result, result.GetParameter());
    }
    
    private bool ComputeTestShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        // Evaluate parameters (establishes dependencies)
        var xValue = FindNumberValue("X_Position", 0.0);
        var yValue = FindNumberValue("Y_Position", 0.0);
        var colorValue = FindStringValue("ShapeColor", "cyan");
        
        // Create shape with stable GlyphId
        var shape = new FoShape3D($"TestShape_{Name}")
        {
            GlyphId = GetKnowId(),
            Color = colorValue,
            Transform = new Transform3("Transform")
            {
                Position = new Vector3(xValue, yValue, 0)
            }
        };
        
        shape.CreateBox("TestBox", 1.0, 1.0, 1.0);
        
        // Return via result - framework routes to appropriate stage
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
}
```

---

## Framework Interaction Summary

| **Concern** | **Framework Mechanism** | **What You Do** | **What You DON'T Do** |
|-------------|------------------------|-----------------|----------------------|
| **Animation Loop** | MentorServices calls `PreAnimationEvent` | `PreAnimationRefresh((comp, evt) => {...})` | ❌ Manual render loop |
| **Model Walk** | RenderContext walks tree | Override `EstablishGeometry3D(view)` | ❌ Manual `ViewRegistry` registration |
| **Dependency Tracking** | Spreadsheet model discovers during eval | `FindNumberValue("ParamName")` | ❌ Manual `SetupDependencies()` |
| **Cache Invalidation** | Parameters marked `Unknown` when changed | Set `parameter.Expression = newValue` | ❌ Manual `ClearCashe()` |
| **Geometry Creation** | Model walk calls EstablishGeometry3D | `Compute3DGeometry(view, geom => {...})` | ❌ Manual `RenderContext` calls |
| **Stage Management** | RenderContext routes by view name | Return geometry from EstablishGeometry3D | ❌ Hold `_stage` field |
| **Transform Updates** | Property setters mark stale | `shape.Transform.Position = newPos` | ❌ Manual `SetTransformStale()` |
| **Shape Lifecycle** | Stable GlyphId | `GlyphId = GetKnowId()` | ❌ Track `_currentShape`/`_previousShape` |

---

## Common Pitfalls

### Pitfall 1: Geometry Not Rendering on First Load

**Symptom**: Logs show "PostCreation called" but nothing appears

**Cause**: Parameter cache is `Good` from initialization, so `PostCreation` never fires

**Solution**: Either:
- Initialize parameters as `Unknown` 
- Call `ClearCashe()` once in constructor to force initial evaluation
- Ensure `Expression` property triggers `Unknown` state

### Pitfall 2: Geometry Updates But Position Doesn't Change

**Symptom**: Color changes work, position changes don't

**Cause**: Not using parameter values in `RenderGeometry3D` - using stale local variables

**Solution**: Always call `FindNumberValue`/`FindStringValue` inside `RenderGeometry3D`:
```csharp
// WRONG
private double _cachedX;
private void RenderGeometry3D(...)
{
    shape.Transform.Position = new Vector3(_cachedX, 0, 0);  // Stale!
}

// RIGHT
private void RenderGeometry3D(...)
{
    var (found, xValue) = FindNumberValue("X_Position");
    shape.Transform.Position = new Vector3(xValue, 0, 0);  // Fresh!
}
```

### Pitfall 3: Multiple Shapes Created Instead of Updating One

**Symptom**: New geometry created every frame instead of updating existing

**Cause**: GlyphId changes (e.g., using `Guid.NewGuid()` or `_frameCounter`)

**Solution**: Use `GetKnowId()` for stable identity:
```csharp
// WRONG
var glyphId = $"Shape_{Guid.NewGuid()}";  // New ID every call!

// RIGHT
var glyphId = $"Shape_{GetKnowId()}";  // Stable across calls
```

### Pitfall 4: Dependencies Not Discovered

**Symptom**: Changing parameter A doesn't trigger re-evaluation of parameter B

**Cause**: Parameter B's formula doesn't call `FindNumberValue("A")` during evaluation

**Solution**: Ensure formulas explicitly reference dependencies:
```csharp
// Parameter A
var aParam = new KnNumber(this, "A") { Expression = "5.0" };

// Parameter B (depends on A)
var bParam = new KnNumber(this, "B") 
{ 
    Expression = "A * 2"  // Must reference "A" in formula
};

// In RenderGeometry3D, calling FindNumberValue establishes dependency
var (foundB, bValue) = FindNumberValue("B");  
// Evaluation of "B" calls FindNumberValue("A") → dependency discovered
```

---

## Testing Pattern

Use `AnimatedKnModel` + test harness page:

```csharp
// In test harness page
protected override async Task OnInitializedAsync()
{
    _testModel = MentorServices.EstablishModel<AnimatedKnModel>("TestModel");
    _testComponent = new MyAnimatedComponent("TestComp");
    ModelEditor.AddChild(_testModel, _testComponent);
}

// In Razor markup
<Canvas3DComponent @ref="Canvas3DReference" 
                   StageName="MyView3D"  <!-- View name for EstablishGeometry3D -->
                   Width="800" 
                   Height="600" />
```

Framework handles everything:
- Animation loop via MentorServices
- Model walk via RenderContext tree traversal
- Stage management via view name routing
- Dependency tracking via spreadsheet model
- Geometry rendering via EstablishGeometry3D

---

## Benefits of This Pattern

1. **Less Code**: ~100 lines vs ~300+ lines of manual coordination
2. **Fewer Bugs**: No race conditions from manual cache/stage management
3. **Better Maintainability**: Framework handles infrastructure, you handle logic
4. **Architectural Alignment**: Follows FO/Arena/Stage hierarchy correctly
5. **Automatic Optimization**: Framework batches renders, manages cache efficiently
6. **Future-Proof**: Can generate from CML automatically

---

## When to Deviate

**Never.** If you think you need manual intervention, you're misunderstanding the framework. Ask:

1. "Why am I calling this manually instead of letting framework call it?"
2. "Why am I tracking this state instead of using framework's tracking?"
3. "Why am I coordinating this instead of using framework's coordination?"

If you need 10+ lines of manual coordination code, you're doing it wrong. Stop and rethink.

---

## Additional Resources

- **Reference Implementation**: `AnimatedParameterTestComponent.cs` (FoundryMentorModeler)
- **Test Harness**: `GeometryParameterTestHarness.razor.cs` (Three2025)
- **Architecture Docs**: 
  - `STAGE_CENTRIC_PATTERN.md` - Stage/Arena/Scene hierarchy
  - `CLOCK_ARCHITECTURE_GUIDE.md` - Animation loop architecture

---

## Version History

- **v1.0** (Dec 18, 2025): Initial documentation based on AnimatedParameterTestComponent refactor
  - Established anti-pattern catalog
  - Documented framework interaction mechanisms
  - Created step-by-step implementation guide
