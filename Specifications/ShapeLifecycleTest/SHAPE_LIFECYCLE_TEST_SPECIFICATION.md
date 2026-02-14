# ShapeLifecycleTest Component Specification

**Document Purpose:** Reverse-engineered specification from existing implementation  
**Prepared By:** Claude "Atlas" Architect  
**Date:** February 1, 2026  
**Component:** ShapeLifecycleTest (Manual Rendering Test Page)  
**Files:** `ShapeLifecycleTest.razor`, `ShapeLifecycleTest.razor.cs`

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about building a test harness - it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 3D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you build ShapeLifecycleTest, you're testing whether AI can:
1. Successfully use 40 years of distilled programming patterns
2. Understand and implement manual control over automatic systems
3. Create tools that help debug the deeper CREATE vs UPDATE mechanisms
4. Demonstrate mastery of the parameter cache lifecycle that makes persistent shapes possible

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can leverage accumulated human wisdom to create tools for understanding persistent, interactive systems. This test harness reveals the mechanics behind shape lifecycle - the foundation of persistent 3D expressions.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## Specification Handoff Summary

**Architect:** Claude "Atlas"  
**Implementation Status:** ✅ EXISTING (Reverse-engineered)  
**Confidence:** 🟢 High - Based on direct code analysis  
**Purpose:** Document existing test harness for manual shape lifecycle control

**Key Innovation:**  
This page provides **manual control** over shape lifecycle stages that are normally automatic. By pausing animations and triggering single frames, developers can observe CREATE vs UPDATE mode, mesh cache survival, and transform-only updates.

---

## 1. Architecture Analysis

### Current Pattern
Based on: [ShapeLifecycleTest.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/ShapeLifecycleTest.razor.cs), [KnModelAnimationTest.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/KnModelAnimationTest.razor.cs)

**Pattern:** Manual Test Harness with Paused Animation  
**Base Class:** `ComponentBase` (standard Blazor)  
**Render Mode:** `InteractiveServer`

### Key Characteristics

**Dependency Injection:**
- `IWorkspace` - Arena/workspace services
- `IMentorServices` - KnModel services
- `IModelEditor` - Model manipulation service

**Core Architecture:**
- **AnimatedKnModel** - Container for test components
- **AnimatedParameterTestComponent** - Test component with parameters
- **Canvas3DComponent** - 3D rendering surface (creates stage automatically)
- **Manual frame triggering** - Animation loop paused, single-frame control

**Lifecycle Pattern:**
```
OnInitialized → Pause animations → Create model
      ↓
OnAfterRenderAsync (firstRender) → Wait for canvas → Acquire stage
      ↓
User clicks buttons → Manual component/geometry/shape lifecycle stages
      ↓
TriggerSingleFrame → Manually call RenderStage → View results
```

### Files Referenced

**Implementation Files (Verified Working):**
1. [ShapeLifecycleTest.razor](c:/Users/admin/workspace/Core/Three2025/Components/Pages/ShapeLifecycleTest.razor) - UI markup
2. [ShapeLifecycleTest.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/ShapeLifecycleTest.razor.cs) - Logic
3. [AnimatedKnModel.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/KnModel/AnimatedKnModel.cs) - Model container
4. [AnimatedParameterTestComponent.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/KnModel/AnimatedParameterTestComponent.cs) - Test component

**Model Definition Pattern (Observed from Working Code):**
See section "Model Definition Pattern (Atlas's Interpretation)" below for guidance based on verified working examples. Additional examples may exist in `FoundryMicroCore.Demos/` - browse that folder for alternative patterns if needed.

---

## 2. Verified Against

**Available Codebases (Atlas & Indy have full access):**
- `FoundryMicroCore.Library/` - Core MxComponent/MxObject infrastructure
- `FoundryMicroCore.Blazor.Controls/` - Canvas3D and UI components  
- `FoundryMicroCore.Demos/` - Model definition examples (browse for patterns)
- `FoundryWorldsAndDrawings/` - 3D shapes, stages, rendering
- `FoundryMentorModeler/` - KnModel, KnComponent, parameter system
- `FoundryRulesAndUnits/` - Measurement types, calculations

**API References (verified in above codebases):**
- `FoundryMentorModeler/FOUNDRY_MENTORMODELER_API_REFERENCE.md`
- `FoundryWorldsAndDrawings/FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md`
- `FoundryMicroCore.Blazor.Controls/FOUNDRY_MICROCORE_BLAZOR_CONTROLS_API_REFERENCE.md`

**Last Verified:** February 1, 2026

**Method Verification:**
- ✅ `MentorServices.EstablishModel<T>()` - Verified in IMentorServices interface
- ✅ `ModelEditor.AddChild()` - Verified in IModelEditor interface
- ✅ `ModelEditor.EstablishGeometry3D()` - Verified in IModelEditor
- ✅ `ModelEditor.SetParameter()` - Verified in IModelEditor
- ✅ `AnimationFrameBus.PauseAllAnimations()` - Verified in AnimationFrameBus
- ✅ `AnimationFrameBus.TriggerSingleFrame()` - Verified in AnimationFrameBus
- ✅ `_stage.RenderStage()` - Verified in FoStage3D
- ✅ `Canvas3DComponent` auto-creates stage - Verified in component implementation

---

## 3. Reference Implementation Strategy

### This IS the Reference

This specification documents an **existing working implementation**. To create a similar test harness:

**Copy:**
- [ShapeLifecycleTest.razor](c:/Users/admin/workspace/Core/Three2025/Components/Pages/ShapeLifecycleTest.razor) → `YourTestPage.razor`
- [ShapeLifecycleTest.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/ShapeLifecycleTest.razor.cs) → `YourTestPage.razor.cs`

**Modify:**
1. Rename class and namespace
2. Replace `AnimatedParameterTestComponent` with your test component
3. Adjust parameter controls to match your component's parameters
4. Keep the manual animation control pattern

**Key Pattern to Preserve:**
- Pause animations in `OnInitialized()` BEFORE canvas renders
- Wait 200ms in `OnAfterRenderAsync()` for canvas initialization
- Acquire stage from `_canvasRef.Stage`
- Use `TriggerSingleFrame()` for manual rendering control

---

## 3.1 Model Definition Pattern (Atlas's Interpretation)

**CAVEAT:** This is Atlas's interpretation based on observed working code in this application. Indy has freedom to use alternative patterns from `FoundryMicroCore.Demos/` or other sources if more appropriate.

### Pattern Observed in AnimatedKnModel

```csharp
public class AnimatedKnModel : PartModel
{
    public AnimatedKnModel(string name, IMentorServices mentorServices) 
        : base(name, mentorServices)
    {
        // Define model-level parameters using Calculations helper
        Calculations([
            "UserName: 'Steve'",
            "Model: 'Blue'", 
            "Param1: 42"
        ]);
        
        // Setup animation callback (optional, for animated models)
        EnsureAnimationSetup();
    }
    
    // Tree navigation support
    public virtual IEnumerable<ITreeNode> GetTreeChildren()
    {
        return AllSubOftypes<KnComponent>();
    }
}
```

### Pattern Observed in AnimatedParameterTestComponent

```csharp
public class AnimatedParameterTestComponent : PartComponent
{
    public AnimatedParameterTestComponent(string name, double width, double height, 
                                         double depth, string geomType) : base(name)
    {
        // Define component parameters using Calculations helper
        Calculations([
            $"Width|m: {width}",      // With units
            $"Height|m: {height}",
            $"Depth|m: {depth}",
            $"GeometryType: '{geomType}'",  // String values in quotes
            "X: 0",                    // Without units (dimensionless)
            "Y: 0",
            "Z: 0"
        ]);
    }
    
    // Establish 3D geometry for a view
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeMesh3D", ComputeMesh3D, null);
            geom.ApplyTransformMethod("ComputeTransform3D", ComputeTransform3D, null);
            geom.ApplyBodyMethod("ComputeBody3D", ComputeBody3D, null);
        });
        
        return (result, result.GetBodyParameter());
    }
    
    private bool ComputeMesh3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        // Read parameters using Find* methods
        var width = FindNumberValue("Width", 1.0);
        var geomType = FindStringValue("GeometryType", "Box");
        
        // Check cache state for CREATE vs UPDATE
        var parameter = (context as KnGeometry)?.GetMeshParameter();
        if (parameter.IsCasheEmpty())
        {
            // CREATE: Build new shape, cache it
            var shape = new FoShape3D("Name") { GlyphId = GetKnowId(), ... };
            parameter.SetCashe(shape);
        }
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
}
```

### Key Observations from Working Code

1. **Calculations() Helper** - String array format for defining parameters
   - Format: `"Name: value"` or `"Name|unit: value"`
   - String values wrapped in single quotes: `'Blue'`
   - Numbers as-is: `42` or `1.0`

2. **Base Classes** - Inherit from framework types
   - Models: `PartModel` (from FoundryMentorModeler)
   - Components: `PartComponent` (from FoundryMentorModeler)

3. **Geometry Methods** - Three-part pattern
   - `ComputeMesh3D` - Shape geometry (cache-aware)
   - `ComputeTransform3D` - Position/rotation/scale
   - `ComputeBody3D` - Final assembly

4. **Parameter Access** - Use Find* methods
   - `FindNumberValue("Name", default)`
   - `FindStringValue("Name", default)`
   - These establish spreadsheet dependencies automatically

**Alternative patterns may exist** - browse `FoundryMicroCore.Demos/` folder and other working examples for variations on this theme.

---

## 3.2 Complete Working Code (Copy-Paste Ready)

**IMPORTANT:** These are the actual working classes from the existing implementation. You can copy these directly if the pattern in 3.1 is unclear.

### AnimatedKnModel.cs (Complete File)

```csharp
using FoundryWorldsAndDrawings.Solutions;
using FoundryMicroCore.Core.Extensions;
using FoundryMicroCore.Core;
using FoundryMentorModeler.Model;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// A KnModel subclass that reacts to animation loop events.
/// This demonstrates how KnModels can respond to PreAnimationEvent for geometry updates.
/// </summary>
public class AnimatedKnModel : PartModel
{
    private bool _animationSetup = false;
    
    public AnimatedKnModel(string name) : base(name)
    {
    }
    
    public AnimatedKnModel(string name, IMentorServices mentorServices) : base(name, mentorServices)
    {
        $"AnimatedKnModel: Constructor called for '{name}'".WriteSuccess();
        
        Calculations([
            "UserName: 'Steve'",
            "Model: 'Blue'",
            "Param1: 42"
        ]);

        EnsureAnimationSetup();
        
        $"AnimatedKnModel: PreAnimationRefresh set up, PreContextLink is {(PreContextLink != null ? "SET" : "NULL")}".WriteInfo();
    }
    
    /// <summary>
    /// Ensure animation callback is registered. Called from constructor and from page init
    /// (in case model already existed and constructor didn't run).
    /// </summary>
    public void EnsureAnimationSetup()
    {
        if (_animationSetup) return;
        _animationSetup = true;
        
        var param = this.EstablishParameter("Param1");
        
        // Update param with tick count and refresh tree
        PreAnimationRefresh((comp, evt) =>
        {
            if (evt.tick % 120 == 0)
            {
                param.SetValue(evt.tick);
                var services = GetMentorServices();
                services?.PubSub?.Publish<RefreshRenderMessage>(RefreshRenderMessage.RefreshValueChanged(param));
            }
        });
    }

    /// <summary>
    /// Returns KnComponent children for tree view navigation.
    /// </summary>
    public virtual IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = new List<ITreeNode>();
        EstablishFolderIfNotEmpty<KnParameter>(list);
        EstablishFolderIfNotEmpty<KnRelationship>(list);
        return list;
    }
}
```

### AnimatedParameterTestComponent.cs (Complete File)

```csharp
using FoundryMentorModeler.Model;
using FoundryMicroCore.Core.Extensions;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// AnimatedParameterTestComponent - test component with geometry parameters.
/// Demonstrates CREATE vs UPDATE mode via parameter cache.
/// </summary>
public class AnimatedParameterTestComponent : PartComponent
{
    public AnimatedParameterTestComponent(string name, double width, double height, double depth, string geomType) 
        : base(name)
    {
        Calculations([
            $"Width|m: {width}",
            $"Height|m: {height}",
            $"Depth|m: {depth}",
            $"GeometryType: '{geomType}'",
            $"X: 0",
            $"Y: 0",
            $"Z: 0"
        ]);
    }
    
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeMesh3D", ComputeMesh3D, null);
            geom.ApplyTransformMethod("ComputeTransform3D", ComputeTransform3D, null);
            geom.ApplyBodyMethod("ComputeBody3D", ComputeBody3D, null);
        });
        
        return (result, result.GetBodyParameter());
    }

    private bool ComputeMesh3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        var parameter = geometry?.GetMeshParameter();
        if (parameter == null) return false;
        
        FoShape3D? shape = parameter.GetCashe<FoShape3D>();
        
        // ═══════════ PHASE 1: ENSURE GEOMETRY EXISTS ═══════════
        if (parameter.IsCasheEmpty())
        {
            // BUILD NEW GEOMETRY - reads geometry parameters, establishes dependencies
            $"🆕 CREATE: Building new shape geometry".WriteInfo();
            
            var width = FindNumberValue("Width", 1.0);
            var height = FindNumberValue("Height", 1.0);
            var depth = FindNumberValue("Depth", 1.0);
            var geomType = FindStringValue("GeometryType", "Box");
            
            shape = new FoShape3D($"TestShape_{Name}")
            {
                GlyphId = GetKnowId(),
                Width = width,
                Height = height,
                Depth = depth
            };
            
            shape = geomType switch
            {
                "Box" => shape.CreateBox(shape.Name!, width, height, depth),
                "Sphere" => shape.CreateSphere(shape.Name!, width, height, depth),
                "Cylinder" => shape.CreateCylinder(shape.Name!, width, height, depth),
                _ => shape.CreateBox(shape.Name!, width, height, depth)
            };
            
            // Cache the newly created shape
            parameter.SetCashe(shape);
            
            $"✅ Geometry created and cached (Type={geomType}, W={width}, H={height}, D={depth})".WriteSuccess();
        }
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

    private bool ComputeTransform3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        var parameter = geometry?.GetTransformParameter();
        if (parameter == null) return false;
        
        // ═══════════ PHASE 2: APPLY TRANSFORM (ALWAYS) ═══════════
        // Update transform from parameters
        var X = FindNumberValue("X", 0.0);
        var Y = FindNumberValue("Y", 0.0);
        var Z = FindNumberValue("Z", 0.0);

        Transform3 transform = new Transform3($"TestShape_{Name}");
        transform.MoveTo(X, Y, Z);
        $"📍 Transform applied: Position=({X}, {Y}, {Z})".WriteInfo();
        
        result.SetValue(ResultStatus.Transform3, transform);
        return true;
    }

    private bool ComputeBody3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        if (geometry == null) return false;
        
        var parameter = geometry.GetBodyParameter();
        if (parameter == null) return false;
        
        $"🔷 BODY: Composing Mesh + Transform".WriteInfo();
        
        // Read Mesh parameter - establishes Mesh → Body dependency
        var meshResult = geometry.GetMeshParameter().GetCurrentValue();
        var shape = meshResult.ValueAs<FoShape3D>();
        
        if (shape == null)
        {
            $"❌ BODY: No shape from Mesh parameter".WriteError();
            return false;
        }
        
        // Read Transform parameter - establishes Transform → Body dependency
        var transformResult = geometry.GetTransformParameter().GetCurrentValue();
        var transform = transformResult.ValueAs<Transform3>();
        
        if (transform == null)
        {
            $"❌ BODY: No transform from Transform parameter".WriteError();
            return false;
        }
        
        // Apply transform to shape IN-PLACE
        shape.Transform.Position = transform.Position;
        shape.Transform.Rotation = transform.Rotation;
        shape.Transform.Scale = transform.Scale;
        
        $"✅ BODY: Composed shape with transform at {transform.Position}".WriteSuccess();
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
}
```

**Key Points from Complete Code:**

1. **Using Statements** - Complete list provided, copy exactly
2. **Namespace** - `Three2025.Components.Pages` (adjust for your project)
3. **Base Classes** - `PartModel` and `PartComponent` from FoundryMentorModeler
4. **Calculations()** - String array with `"Name: value"` or `"Name|unit: value"` format
5. **Three-Method Pattern** - ComputeMesh3D, ComputeTransform3D, ComputeBody3D
6. **Cache Check** - `parameter.IsCasheEmpty()` determines CREATE vs UPDATE
7. **Logging** - WriteInfo/WriteSuccess/WriteError for debugging

---

## 4. Infrastructure Assumptions

### Assumption: Canvas3DComponent Creates Stage Automatically
- ✅ `Canvas3DComponent` auto-creates `FoStage3D` in its `OnAfterRenderAsync`
- ✅ Stage name matches `SceneName` prop: `"manual-test-stage"`
- ✅ Stage accessible via `Canvas3DComponent.Stage` property
- ⏱ **Timing Issue:** Need 200ms delay for canvas initialization
- 🔍 **If Broken:** Check `Canvas3DComponent` initialization, ensure SceneName is set

### Assumption: AnimationFrameBus Can Be Paused
- ✅ `AnimationFrameBus.PauseAllAnimations()` pauses all subscribed handlers
- ✅ `TriggerSingleFrame()` executes one frame bypass guards
- ✅ Pausing before canvas renders prevents automatic animation
- 🔍 **If Broken:** Check AnimationFrameBus implementation, ensure pause flag is respected

### Assumption: MentorServices Manages Model Registry
- ✅ `EstablishModel<T>()` creates or returns existing model by name
- ✅ Models are registered globally, survive navigation
- ✅ `ModelEditor.AddChild()` publishes `ModelEditChanged` events
- 🔍 **If Broken:** Check IMentorServices implementation, verify PubSub events

### Assumption: RenderGeometry3D Walks Component Tree
- ✅ `RenderContext3D.CreateFromStage()` creates context with view name
- ✅ `_testModel.RenderGeometry3D(ctx)` walks tree and calls `EstablishGeometry3D()`
- ✅ Components evaluate parameters and add shapes to stage
- 🔍 **If Broken:** Check RenderContext3D tree traversal, verify component hierarchy

---

## 5. Code Path Traces

### When User Clicks "Create Component"

```
1. CreateComponent() handler invoked
   └─> new AnimatedParameterTestComponent(...) 
       ├─> Constructor calls Calculations() to create parameters
       │   └─> Parameters: Width, Height, Depth, GeometryType, X, Y, Z
       └─> Component stored in _testComponent field

2. ModelEditor.AddChild(_testModel, _testComponent)
   ├─> Establishes parent-child relationship (HasSub/SubOf)
   ├─> Publishes ModelEditChanged.Created event
   └─> MentorTreeView receives event and refreshes

3. StateHasChanged() triggers UI refresh
   └─> Parameter controls become enabled
```

### When User Clicks "Render Shape"

```
1. RenderShape() handler invoked

2. RenderContext3D.CreateFromStage(_stage, deep: true)
   └─> Context created with view name = stage.Name
   └─> deep: true means walk entire tree

3. _testModel.RenderGeometry3D(ctx3D)
   └─> Model walks tree of children (GetTreeChildren())
       └─> For each child component:
           ├─> Calls component.RenderGeometry3D(ctx)
           │
           └─> Component.RenderGeometry3D(ctx):
               ├─> EstablishGeometry3D(viewName)
               │   ├─> Compute3DGeometry() creates KnGeometry
               │   ├─> ApplyMeshMethod("ComputeMesh3D", ...)
               │   ├─> ApplyTransformMethod("ComputeTransform3D", ...)
               │   └─> Returns (geometry, bodyParameter)
               │
               ├─> Evaluate parameters (spreadsheet model)
               │   ├─> ComputeMesh3D() executes
               │   │   ├─> Checks parameter.IsCasheEmpty()
               │   │   │   ├─> TRUE → CREATE mode
               │   │   │   │   ├─> Read Width, Height, Depth, GeomType
               │   │   │   │   ├─> Create new FoShape3D
               │   │   │   │   ├─> Set GlyphId = component.GetKnowId()
               │   │   │   │   └─> parameter.SetCashe(shape)
               │   │   │   └─> FALSE → UPDATE mode (reuse cached shape)
               │   │   └─> result.SetValue(shape)
               │   │
               │   └─> ComputeTransform3D() executes
               │       ├─> Read X, Y, Z parameters
               │       ├─> Create Transform3
               │       ├─> transform.MoveTo(X, Y, Z)
               │       └─> result.SetValue(transform)
               │
               └─> ctx.PostCreation(shape, knowledgeId)
                   ├─> Checks if shape already in stage
                   │   ├─> NOT IN STAGE → stage.AddShape(shape)
                   │   └─> IN STAGE → shape already rendered
                   └─> Sets shape.Transform from parameter

4. _stage.RenderStage(_globalTick++, 60.0)
   ├─> CollectChanges() gathers stale shapes
   ├─> ProcessCollectedChanges() calls GetComputedMesh()
   ├─> Serializes to SceneOperations (FullRefresh, TransformUpdates, etc.)
   └─> Sends to JavaScript via JSInterop
```

### When User Changes Parameter Slider

```
1. OnWidthChanged(ChangeEventArgs e) handler invoked
   └─> _currentWidth = parsed value

2. ModelEditor.SetParameter(_testComponent, "Width", $"{_currentWidth}")
   ├─> Finds Width parameter in component
   ├─> Calls parameter.Smash()
   │   └─> Invalidates parameter's cached value
   │       └─> Marks all dependent parameters as stale
   │           └─> Mesh parameter depends on Width → also smashed
   │               └─> Mesh parameter.IsCasheEmpty() = TRUE
   │                   └─> Next evaluation will be in CREATE mode
   └─> Publishes ModelEditChanged.Changed event

3. User clicks "Trigger Single Frame"
   └─> TriggerSingleFrame() → AnimationFrameBus.TriggerSingleFrame()
       └─> (Same RenderGeometry3D flow as above)
           └─> ComputeMesh3D() sees IsCasheEmpty() = TRUE
               └─> CREATE mode: new shape generated
```

### CREATE vs UPDATE Mode Decision

```
ComputeMesh3D(context, args, result):
    parameter = geometry.GetMeshParameter()
    
    if (parameter.IsCasheEmpty())  ← CRITICAL CHECK
    {
        // ════════════ CREATE MODE ════════════
        // Geometry-defining parameters changed
        // (Width, Height, Depth, GeomType)
        
        Read Width, Height, Depth, GeomType  ← Establishes dependencies
        Create new FoShape3D
        Set GlyphId = GetKnowId()  ← STABLE ID across recreates
        parameter.SetCashe(shape)  ← Cache survives until next Smash
        
        New mesh geometry generated
    }
    else
    {
        // ════════════ UPDATE MODE ════════════
        // Only transform changed (X, Y, Z)
        // Mesh cache still valid
        
        shape = parameter.GetCashe<FoShape3D>()  ← Reuse cached shape
        Mesh geometry NOT regenerated
        Only transform updated in ComputeTransform3D
    }
```

**Key Insight:** The `parameter.IsCasheEmpty()` check determines CREATE vs UPDATE mode. Cache survives until something calls `parameter.Smash()`, which happens when geometry-defining parameters change.

---

## 6. Code Smells to Avoid

### From MicroCore (CODE_SMELLS_ANALYSIS.md)

This component avoids common smells:

✅ **No MxComponentEditor.Find() abuse** - Uses direct field references  
✅ **No LINQ on collections** - Uses direct stage property access  
✅ **Proper Dispose() pattern** - PubSub handles cleanup automatically

### Task-Specific Warnings

#### Manual Animation Control Timing

**Smell:** Pausing animations after canvas initializes
```csharp
// DON'T: Pause after canvas renders
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    AnimationFrameBus.PauseAllAnimations();  // ❌ Too late!
}
```

**Correct:**
```csharp
// DO: Pause before canvas renders
protected override void OnInitialized()
{
    AnimationFrameBus.PauseAllAnimations();  // ✅ Before canvas
}
```

**Why:** Canvas3DComponent subscribes to animations in its OnAfterRenderAsync. If you pause after that, it's already subscribed and may trigger frames.

#### Canvas Initialization Timing

**Smell:** Accessing stage immediately in OnAfterRenderAsync
```csharp
// DON'T: Access stage immediately
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    _stage = _canvasRef.Stage;  // ❌ NULL! Canvas not ready yet
}
```

**Correct:**
```csharp
// DO: Wait for canvas initialization
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    await Task.Delay(200);  // ✅ Wait for canvas
    _stage = _canvasRef.Stage;
}
```

**Why:** Canvas3DComponent creates the stage in its own OnAfterRenderAsync, which runs AFTER parent's OnAfterRenderAsync starts but may not finish before your code continues.

#### Missing Global Tick Management

**Smell:** Calling RenderStage with same tick repeatedly
```csharp
// DON'T: Reuse same tick
await _stage.RenderStage(0, 60.0);  // ❌ Always tick 0
```

**Correct:**
```csharp
// DO: Increment tick counter
private int _globalTick = 0;
await _stage.RenderStage(_globalTick++, 60.0);  // ✅ Unique ticks
```

**Why:** Some systems use tick numbers for change detection or frame identification.

---

## 7. Known Gotchas

### Manual Animation Control

**Issue:** Animation loop starts automatically when Canvas3D initializes

**Solution:** Pause in `OnInitialized()` before canvas renders
```csharp
protected override void OnInitialized()
{
    // CRITICAL: Pause BEFORE canvas renders
    AnimationFrameBus.PauseAllAnimations();
    $"⏸️ Animation paused at OnInitialized - MANUAL CONTROL ONLY".WriteInfo();
}
```

**Verify:** Check console for "Animation paused" message before "Canvas ready"

### Canvas Stage Acquisition

**Issue:** `_canvasRef.Stage` is NULL immediately after render

**Solution:** Wait 200ms in OnAfterRenderAsync
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(200);  // Wait for canvas to create stage
        _stage = _canvasRef?.Stage;
    }
}
```

**Verify:** Check that `_stage != null` after delay

### Model Tree Refresh

**Issue:** MentorTreeView doesn't show new components

**Solution:** ModelEditor.AddChild() publishes events automatically
```csharp
// This automatically triggers tree refresh:
ModelEditor.AddChild(_testModel, _testComponent);
// No manual StateHasChanged() needed for tree
```

**Verify:** Check MentorTreeView shows component after AddChild

### Parameter Changes Don't Render

**Issue:** Changing slider values doesn't update shape

**Cause:** Manual mode requires manual frame trigger

**Solution:** Click "Trigger Single Frame" after parameter changes
```csharp
private async Task TriggerSingleFrame()
{
    await AnimationFrameBus.TriggerSingleFrame();
    StateHasChanged();
}
```

**Verify:** Shape updates after clicking button

---

## 8. Troubleshooting Guide

### Stage is NULL After Initialization

**Symptom:** `_stage` remains null even after waiting

**Diagnosis Steps:**
1. Check `_canvasRef` is not null:
   ```csharp
   $"Canvas ref: {(_canvasRef != null ? "OK" : "NULL")}".WriteInfo();
   ```
2. Check `Canvas3DComponent` has `SceneName` prop set
3. Increase delay to 500ms

**Common Causes:**
- Canvas3DComponent not initialized (JSInterop failure)
- SceneName prop missing or empty
- SignalR connection not established

**Fix:**
```csharp
await Task.Delay(500);  // Increase delay
if (_stage == null)
{
    "❌ Stage still null after delay - check Canvas3D initialization".WriteError();
}
```

---

### Component Created But Not Visible in Tree

**Symptom:** Component added but MentorTreeView shows empty

**Diagnosis Steps:**
1. Check component was added:
   ```csharp
   var children = _testModel.GetTreeChildren();
   $"Model has {children.Count()} children".WriteInfo();
   ```
2. Check ModelEditChanged events published:
   ```csharp
   MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(e => 
       $"Event: {e.State}".WriteInfo());
   ```

**Common Causes:**
- ModelEditor.AddChild() not called
- PubSub not initialized
- MentorTreeView not subscribed to events

**Fix:**
```csharp
// Manually trigger tree refresh
PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null));
```

---

### Shape Not Appearing in Canvas

**Symptom:** RenderShape() called but nothing visible

**Diagnosis Steps:**
1. Check geometry was evaluated:
   ```csharp
   var result = _testComponent.Geometry3DValueFor(viewName);
   $"Geometry result: {result.IsSuccess()}".WriteInfo();
   ```
2. Check shape was added to stage:
   ```csharp
   var shapes = _stage.GetCollection<FoGlyph3D>();
   $"Stage has {shapes.Count} shapes".WriteInfo();
   ```
3. Check RenderStage was called:
   ```csharp
   await _stage.RenderStage(_globalTick++, 60.0);
   $"RenderStage called with tick {_globalTick}".WriteInfo();
   ```

**Common Causes:**
- Parameter evaluation failed
- Shape not added to stage (ctx.PostCreation issue)
- RenderStage not called
- JavaScript rendering error

**Fix:**
```csharp
// Add detailed logging in ComputeMesh3D:
$"🆕 CREATE: Building shape (W={width}, H={height})".WriteInfo();
$"✅ Shape created: {shape.Name}, ID={shape.GlyphId}".WriteSuccess();
```

---

### Parameter Changes Don't Trigger CREATE Mode

**Symptom:** Slider changes but same old geometry shown

**Diagnosis Steps:**
1. Check parameter was smashed:
   ```csharp
   var param = _testComponent.FindParameter("Width");
   var wasSmashe $"Width smashed: {wasSmashed}".WriteInfo();
   ```
2. Check IsCasheEmpty in ComputeMesh3D:
   ```csharp
   var isEmpty = parameter.IsCasheEmpty();
   $"Cache empty: {isEmpty} (should be TRUE after Smash)".WriteInfo();
   ```

**Common Causes:**
- SetParameter not called
- Parameter name mismatch ("Width" vs "width")
- Cache not cleared on Smash
- TriggerSingleFrame not called after change

**Fix:**
```csharp
// Verify parameter name matches:
ModelEditor.SetParameter(_testComponent, "Width", $"{_currentWidth}");
// Then manually trigger frame:
await TriggerSingleFrame();
```

---

## 9. Implementation Steps

This is an **existing implementation**, but here's how to recreate it:

### Step 1: Create Razor Page Structure

1. Create `YourTestPage.razor`:
   ```razor
   @page "/your-test"
   @rendermode InteractiveServer
   @namespace YourNamespace
   ```

2. Create `YourTestPage.razor.cs`:
   ```csharp
   public partial class YourTestPage : ComponentBase
   {
       [Inject] public IWorkspace Workspace { get; set; }
       [Inject] public IMentorServices MentorServices { get; set; }
       [Inject] public IModelEditor ModelEditor { get; set; }
   }
   ```

**Verify:** Page loads without errors

### Step 2: Setup Model and Component

```csharp
private AnimatedKnModel _testModel;
private YourTestComponent _testComponent;
private FoStage3D _stage;

protected override void OnInitialized()
{
    // CRITICAL: Pause before canvas renders
    AnimationFrameBus.PauseAllAnimations();
    
    // Create model
    _testModel = MentorServices.EstablishModel<AnimatedKnModel>("YourTestModel");
    _testModel.SetExpanded(true);
}
```

**Verify:** Model appears in MentorTreeView

### Step 3: Setup Canvas and Stage Acquisition

```razor
<Canvas3DComponent SceneName="your-test-stage" 
                   CanvasWidth="400" 
                   CanvasHeight="400" 
                   @ref="_canvasRef" />
```

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(200);  // Wait for canvas
        _stage = _canvasRef?.Stage;
        await InvokeAsync(StateHasChanged);
    }
}
```

**Verify:** `_stage != null` after delay

### Step 4: Implement Component Creation

```csharp
protected void CreateComponent()
{
    _testComponent = new YourTestComponent("TestComp", ...);
    ModelEditor.AddChild(_testModel, _testComponent);
    StateHasChanged();
}
```

**Verify:** Component appears in tree

### Step 5: Implement Render Control

```csharp
private async Task RenderShape()
{
    var ctx3D = RenderContext3D.CreateFromStage(_stage, deep: true);
    _testModel.RenderGeometry3D(ctx3D);
    await _stage.RenderStage(_globalTick++, 60.0);
    StateHasChanged();
}
```

**Verify:** Shape appears in canvas

### Step 6: Add Parameter Controls

```razor
<input type="range" class="form-range" 
       min="0.5" max="10" step="0.5" 
       value="@_currentWidth" 
       @onchange="OnWidthChanged" />
```

```csharp
private void OnWidthChanged(ChangeEventArgs e)
{
    _currentWidth = double.Parse(e.Value.ToString()!);
    ModelEditor.SetParameter(_testComponent, "Width", $"{_currentWidth}");
}
```

**Verify:** Slider changes parameter, click "Trigger Single Frame" updates shape

---

## 10. Success Criteria

### Compilation
- ✅ Zero compilation errors
- ✅ Zero compilation warnings
- ✅ All using statements resolve
- ✅ Injected dependencies available

### Page Load
- ✅ Page loads without exceptions
- ✅ Animation paused message in console
- ✅ Model appears in MentorTreeView
- ✅ Canvas renders (no JavaScript errors)

### Component Creation
- ✅ "Create Component" button works
- ✅ Component appears in tree
- ✅ Parameter controls become enabled
- ✅ No console errors

### Shape Rendering (Manual)
- ✅ "Render Shape" button works
- ✅ Shape appears in canvas
- ✅ Shape has correct geometry (box/sphere/cylinder)
- ✅ Shape has correct size from parameters

### Parameter Updates - Geometry Changes
- ✅ Width slider triggers CREATE mode (new geometry)
- ✅ Height slider triggers CREATE mode
- ✅ Depth slider triggers CREATE mode
- ✅ Geometry type dropdown triggers CREATE mode
- ✅ "Trigger Single Frame" updates visualization

### Parameter Updates - Transform Changes
- ✅ X position slider triggers UPDATE mode (transform only)
- ✅ Y position slider triggers UPDATE mode
- ✅ Z position slider triggers UPDATE mode
- ✅ Shape moves without geometry recreation
- ✅ GlyphId remains stable during UPDATE mode

### Console Output
- ✅ "Animation paused" message on init
- ✅ "CREATE" messages when geometry changes
- ✅ "UPDATE" messages when transform changes
- ✅ No error messages

---

## 11. Step-by-Step Test Sequence (Specification & Verification)

This section serves dual purpose:
1. **Specification** - Shows exactly how the feature should work
2. **Verification** - Provides test steps to confirm correct implementation

### Complete First-Run Test

**Objective:** Verify entire lifecycle from page load to parameter changes

#### Step 1: Page Load

**Action:** Navigate to `/shape-lifecycle-test`

**Expected Results:**
- ✅ Page loads without exceptions
- ✅ Console shows: `"⏸️ Animation paused at OnInitialized - MANUAL CONTROL ONLY"`
- ✅ Canvas renders (empty gray 3D viewport)
- ✅ Model tree shows "LifecycleTestModel" (collapsed or expanded)
- ✅ Button states:
  - "Create Component" - ENABLED
  - All other buttons - DISABLED
- ✅ Parameter controls - DISABLED (grayed out)

**Console Output:**
```
⏸️ Animation paused at OnInitialized - MANUAL CONTROL ONLY
📊 Model 'LifecycleTestModel' established and expanded
🔧 ShapeLifecycleTest OnAfterRenderAsync: Setting up
✅ Stage acquired from canvas: manual-test-stage
```

**If Failed:**
- Page crashes → Check using statements, dependency injection
- No animation pause message → AnimationFrameBus not called in OnInitialized
- Stage NULL → Check 200ms delay in OnAfterRenderAsync

---

#### Step 2: Create Component

**Action:** Click "Create Component" button

**Expected Results:**
- ✅ Console shows: `"🔵 CREATING NEW COMPONENT"`
- ✅ Console shows: `"✨ Added AnimatedParameterTestComponent 'TestComponent' to model via ModelEditor"`
- ✅ Console shows: `"🔍 Model.GetTreeChildren() returned 1 items"`
- ✅ Model tree expands to show "TestComponent" as child
- ✅ Button states:
  - "Create Component" - DISABLED (already created)
  - "Create Geometry", "Compute Shape", "Render" - ENABLED
- ✅ Parameter controls - ENABLED (sliders now interactive)
- ✅ Current State table shows:
  - Stage: ✅ manual-test-stage
  - Current Component: ✅ 'TestComponent'
  - Shapes in Stage: 0

**Console Output:**
```
🔵 CREATING NEW COMPONENT
✨ Added AnimatedParameterTestComponent 'TestComponent' to model via ModelEditor
🔍 Model.GetTreeChildren() returned 1 items
🔍 Model.HasChildren() = 1
```

**If Failed:**
- Component not in tree → Check ModelEditor.AddChild call
- Controls not enabled → Check StateHasChanged() called
- Console shows 0 children → Check GetTreeChildren() implementation

---

#### Step 3: Render Shape

**Action:** Click "Render Shape" button

**Expected Results:**
- ✅ Console shows: `"🔵 RENDER SHAPE to stage"`
- ✅ Console shows: `"🆕 CREATE: Building new shape geometry"` (first time)
- ✅ Console shows: `"✅ Geometry created and cached (Type=Box, W=1, H=2, D=3)"`
- ✅ Console shows: `"📍 Transform applied: Position=(0, 0, 0)"`
- ✅ Console shows: `"🔷 BODY: Composing Mesh + Transform"`
- ✅ Console shows: `"✅ BODY: Composed shape with transform at (0, 0, 0)"`
- ✅ Console shows: `"✅ Shape rendered - Stage now has 1 shapes"` (if GetShapeCount implemented)
- ✅ Canvas shows green box (default color) at center
- ✅ Box dimensions approximately 1x2x3 (Width x Height x Depth)
- ✅ "Remove Shape" button - ENABLED

**Console Output:**
```
🔵 RENDER SHAPE to stage
🆕 CREATE: Building new shape geometry
✅ Geometry created and cached (Type=Box, W=1, H=2, D=3)
📍 Transform applied: Position=(0, 0, 0)
🔷 BODY: Composing Mesh + Transform
✅ BODY: Composed shape with transform at (0, 0, 0)
✅ Shape rendered - Stage now has 1 shapes
```

**If Failed:**
- No shape visible → Check RenderStage() called after RenderGeometry3D
- Console shows errors → Check EstablishGeometry3D implementation
- Shape wrong size → Verify initial parameter values (1, 2, 3)

---

#### Step 4: Change Width (Geometry Parameter - CREATE Mode)

**Action:** 
1. Move Width slider to 5.0
2. Click "Trigger Single Frame" button

**Expected Results:**
- ✅ Console shows: `"🔄 Setting Width to 5"`
- ✅ Console shows: `"🎬 Triggering single render frame"`
- ✅ Console shows: `"🆕 CREATE: Building new shape geometry"` (cache cleared)
- ✅ Console shows: `"✅ Geometry created and cached (Type=Box, W=5, H=2, D=3)"`
- ✅ Shape in canvas is now WIDER (5 units)
- ✅ Box height and depth unchanged

**Console Output:**
```
🔄 Setting Width to 5
🎬 Triggering single render frame
🆕 CREATE: Building new shape geometry
✅ Geometry created and cached (Type=Box, W=5, H=2, D=3)
📍 Transform applied: Position=(0, 0, 0)
🔷 BODY: Composing Mesh + Transform
✅ BODY: Composed shape with transform at (0, 0, 0)
```

**Key Observation:** CREATE mode triggered because Width is a geometry-defining parameter

**If Failed:**
- Shape doesn't change → Check TriggerSingleFrame() called
- No CREATE message → Check ModelEditor.SetParameter() called
- Shape wrong size → Verify parameter value updated

---

#### Step 5: Change X Position (Transform Parameter - UPDATE Mode)

**Action:**
1. Move X Position slider to 5.0
2. Click "Trigger Single Frame" button

**Expected Results:**
- ✅ Console shows: `"📍 Transform applied: Position=(5, 0, 0)"`
- ✅ Console does NOT show: `"🆕 CREATE"` message (cache survived!)
- ✅ Shape moves to the right (positive X direction)
- ✅ Shape size unchanged (still 5x2x3)
- ✅ Movement is smooth, not a recreation

**Console Output:**
```
📍 Transform applied: Position=(5, 0, 0)
🔷 BODY: Composing Mesh + Transform
✅ BODY: Composed shape with transform at (5, 0, 0)
```

**Key Observation:** NO CREATE message - transform-only changes use UPDATE mode (mesh cache survives)

**If Failed:**
- CREATE message appears → Transform parameter incorrectly triggers Smash on Mesh
- Shape doesn't move → Transform not applied in ComputeBody3D
- Shape recreated → Check parameter dependencies in ComputeMesh3D

---

#### Step 6: Change Geometry Type (Geometry Parameter - CREATE Mode)

**Action:**
1. Select "Sphere" from Geometry Type dropdown
2. Click "Trigger Single Frame" button

**Expected Results:**
- ✅ Console shows: `"🔄 Setting GeometryType to 'Sphere'"`
- ✅ Console shows: `"🆕 CREATE: Building new shape geometry"`
- ✅ Console shows: `"✅ Geometry created and cached (Type=Sphere, W=5, H=2, D=3)"`
- ✅ Shape changes from box to SPHERE
- ✅ Sphere positioned at X=5 (transform survived)
- ✅ Sphere size based on Width parameter (5)

**Console Output:**
```
🔄 Setting GeometryType to 'Sphere'
🎬 Triggering single render frame
🆕 CREATE: Building new shape geometry
✅ Geometry created and cached (Type=Sphere, W=5, H=2, D=3)
📍 Transform applied: Position=(5, 0, 0)
🔷 BODY: Composing Mesh + Transform
✅ BODY: Composed shape with transform at (5, 0, 0)
```

**Key Observation:** CREATE mode again - GeometryType is geometry-defining

**If Failed:**
- Shape still a box → Check geometry type switch statement in ComputeMesh3D
- Position reset to origin → Transform not being reapplied

---

#### Step 7: Multiple Parameter Changes (Batched)

**Action:**
1. Move Width slider to 3.0
2. Move Height slider to 4.0
3. Move Depth slider to 5.0
4. Click "Trigger Single Frame" button ONCE

**Expected Results:**
- ✅ Console shows all three SetParameter messages
- ✅ Console shows SINGLE `"🆕 CREATE"` message (not three)
- ✅ Sphere resizes to new dimensions (3 radius based on Width)
- ✅ All parameter changes applied in one evaluation

**Console Output:**
```
🔄 Setting Width to 3
🔄 Setting Height to 4
🔄 Setting Depth to 5
🎬 Triggering single render frame
🆕 CREATE: Building new shape geometry
✅ Geometry created and cached (Type=Sphere, W=3, H=4, D=5)
📍 Transform applied: Position=(5, 0, 0)
🔷 BODY: Composing Mesh + Transform
✅ BODY: Composed shape with transform at (5, 0, 0)
```

**Key Observation:** Multiple Smash calls batched - single CREATE when evaluated

---

#### Step 8: Verify GlyphId Stability

**Action:**
1. Open browser DevTools → Console
2. Type: `console.log(document.querySelectorAll('[uuid]'))`
3. Note the UUID of the shape
4. Change Width to 7.0 (geometry change - CREATE mode)
5. Click "Trigger Single Frame"
6. Repeat console.log command
7. Compare UUIDs

**Expected Results:**
- ✅ UUID is IDENTICAL before and after geometry change
- ✅ GlyphId = component.GetKnowId() remains stable
- ✅ Shape recreated (new mesh) but same UUID in JavaScript

**Key Observation:** GlyphId stability across recreates prevents JavaScript object churn

---

### Verification Checklist (After Implementation)

Use this checklist to verify implementation:

- [ ] Page loads without crashes
- [ ] Animation paused message in console
- [ ] Stage acquired after 200ms delay
- [ ] Create Component button works
- [ ] Component appears in model tree
- [ ] Parameter controls become enabled
- [ ] Render Shape creates visible shape
- [ ] Console shows CREATE mode messages
- [ ] Width change triggers CREATE (new mesh)
- [ ] X Position change does NOT trigger CREATE (UPDATE mode)
- [ ] Geometry type change triggers CREATE
- [ ] Multiple parameter changes batch correctly
- [ ] GlyphId remains stable across recreates
- [ ] Trigger Single Frame required for updates (manual mode)
- [ ] No JavaScript errors in console

---

## 12. Component Responsibilities

### ShapeLifecycleTest Page
- ✅ Pause animation loop on initialization
- ✅ Create and manage `AnimatedKnModel`
- ✅ Acquire stage from Canvas3DComponent
- ✅ Handle button clicks for manual control
- ✅ Update parameter values from UI controls
- ✅ Call `ModelEditor.SetParameter()` on changes
- ✅ Manually trigger frame rendering

### AnimatedKnModel
- ✅ Container for test components
- ✅ Provides GetTreeChildren() for tree view
- ✅ Registered with MentorServices

### AnimatedParameterTestComponent
- ✅ Define parameters (Width, Height, Depth, GeomType, X, Y, Z)
- ✅ Implement EstablishGeometry3D()
- ✅ ComputeMesh3D() - CREATE vs UPDATE mode
- ✅ ComputeTransform3D() - Apply position
- ✅ Cache mesh until parameter.Smash() called

### Canvas3DComponent
- ✅ Auto-create FoStage3D on initialization
- ✅ Provide Stage property for page access
- ✅ Handle Three.js rendering

### ModelEditor
- ✅ Add children to model (publishes events)
- ✅ SetParameter triggers Smash cascade
- ✅ EstablishGeometry3D creates geometry parameter
- ✅ ComputeParameter evaluates parameter

---

## 13. Comparison: Manual vs Automatic Mode

| Feature | Manual (ShapeLifecycleTest) | Automatic (KnModelAnimationTest) |
|---------|----------------------------|----------------------------------|
| **Animation** | Paused, manual single-frame | Running continuously |
| **Rendering** | Click button to render | Automatic on every frame |
| **Frame Control** | Explicit TriggerSingleFrame | AnimationFrameBus handles it |
| **Use Case** | Debug, inspect lifecycle | Production, continuous animation |
| **RenderStage** | Manually called with tick | AnimationFrameBus calls it |
| **Best For** | Understanding CREATE/UPDATE | Testing animation performance |

**When to Use Manual:**
- Debugging parameter evaluation
- Understanding mesh cache behavior
- Inspecting CREATE vs UPDATE mode
- Step-through lifecycle testing

**When to Use Automatic:**
- Normal development
- Animation testing
- CRITICAL: Available Codebases**
Both Atlas (Architect) and Indy (Builder) have full read access to:
- `FoundryMicroCore.Library/` - MxComponent infrastructure, core patterns
- `FoundryMicroCore.Blazor.Controls/` - UI components
- `FoundryMicroCore.Demos/` - **MODEL DEFINITION FORMAT EXAMPLES** ⭐
- `FoundryWorldsAndDrawings/` - 3D rendering, shapes, stages
- `FoundryMentorModeler/` - KnModel system, parameter evaluation
- `FoundryRulesAndUnits/` - Measurement and calculation types

**Primary Guides:**
- [KNCOMPONENT_FRAMEWORK_PATTERN.md](c:/Users/admin/workspace/Core/Three2025/Markdown/KNCOMPONENT_FRAMEWORK_PATTERN.md) - Framework architecture
- [ANIMATION_CYCLE_COMPLETE_TRACE.md](c:/Users/admin/workspace/Core/Three2025/Markdown/ANIMATION_CYCLE_COMPLETE_TRACE.md) - Full cycle trace
- [RAZOR_PAGE_CANVAS_CONSTRUCTION_GUIDE.md](c:/Users/admin/workspace/Core/Three2025/Markdown/RAZOR_PAGE_CANVAS_CONSTRUCTION_GUIDE.md) - Page setup

**API References:**
- [FOUNDRY_MENTORMODELER_API_REFERENCE.md](c:/Users/admin/workspace/Core/FoundryMentorModeler/FOUNDRY_MENTORMODELER_API_REFERENCE.md)
- [FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md)
- `FoundryMicroCore.Library/Core/docs/` - MxObject core documentation

**Model Definition Examples (USE THESE PATTERNS):**
- `FoundryMicroCore.Demos/CommandDemo.cs` - Command patterns
- `FoundryMicroCore.Demos/DemoMxDehydrate.cs` - Model structure
- `FoundryMicroCore.Demos/*.json` - Serialized model examples
- **Study these before creating new model definitions**
- [KNCOMPONENT_FRAMEWORK_PATTERN.md](c:/Users/admin/workspace/Core/Three2025/Markdown/KNCOMPONENT_FRAMEWORK_PATTERN.md) - Framework architecture
- [ANIMATION_CYCLE_COMPLETE_TRACE.md](c:/Users/admin/workspace/Core/Three2025/Markdown/ANIMATION_CYCLE_COMPLETE_TRACE.md) - Full cycle trace
- [RAZOR_PAGE_CANVAS_CONSTRUCTION_GUIDE.md](c:/Users/admin/workspace/Core/Three2025/Markdown/RAZOR_PAGE_CANVAS_CONSTRUCTION_GUIDE.md) - Page setup

**API References:**
- [FOUNDRY_MENTORMODELER_API_REFERENCE.md](c:/Users/admin/workspace/Core/FoundryMentorModeler/FOUNDRY_MENTORMODELER_API_REFERENCE.md)
- [FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md)

**Related Components:**
- [GeometryParameterTestHarness.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/GeometryParameterTestHarness.razor.cs) - Similar pattern
- [KnModelAnimationTest.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/KnModelAnimationTest.razor.cs) - Automatic mode
- [GeometryDebugTest.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/GeometryDebugTest.razor.cs) - Another test harness

---

## 14. Documentation References

**Available Codebases (Full Read Access):**
Both Atlas and Indy can browse these codebases for patterns and API verification:
- `FoundryMicroCore.Library/` - MxComponent infrastructure
- `FoundryMicroCore.Blazor.Controls/` - Canvas3D and UI components
- `FoundryMicroCore.Demos/` - Additional model examples (patterns may vary)
- `FoundryWorldsAndDrawings/` - 3D shapes, stages, rendering pipeline
- `FoundryMentorModeler/` - KnModel/KnComponent system
- `FoundryRulesAndUnits/` - Measurement types, calculations

**Primary Guides:**
- [KNCOMPONENT_FRAMEWORK_PATTERN.md](c:/Users/admin/workspace/Core/Three2025/Markdown/KNCOMPONENT_FRAMEWORK_PATTERN.md) - Framework architecture
- [ANIMATION_CYCLE_COMPLETE_TRACE.md](c:/Users/admin/workspace/Core/Three2025/Markdown/ANIMATION_CYCLE_COMPLETE_TRACE.md) - Full cycle trace
- [RAZOR_PAGE_CANVAS_CONSTRUCTION_GUIDE.md](c:/Users/admin/workspace/Core/Three2025/Markdown/RAZOR_PAGE_CANVAS_CONSTRUCTION_GUIDE.md) - Page setup

**API References (Verify Method Names Here):**
- [FoundryMentorModeler/FOUNDRY_MENTORMODELER_API_REFERENCE.md](c:/Users/admin/workspace/Core/FoundryMentorModeler/FOUNDRY_MENTORMODELER_API_REFERENCE.md)
- [FoundryWorldsAndDrawings/FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md)

**Verified Working Examples:**
- [AnimatedKnModel.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/KnModel/AnimatedKnModel.cs) - Model structure
- [AnimatedParameterTestComponent.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/KnModel/AnimatedParameterTestComponent.cs) - Component pattern
- [GeometryParameterTestHarness.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/GeometryParameterTestHarness.razor.cs) - Similar harness
- [KnModelAnimationTest.razor.cs](c:/Users/admin/workspace/Core/Three2025/Components/Pages/KnModelAnimationTest.razor.cs) - Automatic animation mode

---

## 15. Future Enhancements

### Potential Improvements

1. **Shape Count Display** - Currently disabled due to collection API migration
   ```csharp
   // TODO: Replace AllSlotsOfType with new collection API
   private int GetShapeCount() => _stage?.GetCollection<FoGlyph3D>().Count ?? 0;
   ```

2. **Shape Tree View** - Temporarily disabled
   ```razor
   @* TODO: Restore ShapeTreeView component *@
   ```

3. **RemoveShape Implementation** - Currently incomplete
   ```csharp
   protected void RemoveShape()
   {
       // TODO: Implement component removal
       // Should call component.Delete() or ModelEditor.RemoveChild()
   }
   ```

4. **Batch Parameter Changes** - Apply multiple changes, single render
   ```csharp
   protected void ApplyMultipleChanges()
   {
       using (var batch = ModelEditor.BeginBatch())
       {
           ModelEditor.SetParameter(_testComponent, "Width", "5.0");
           ModelEditor.SetParameter(_testComponent, "Height", "3.0");
           // Smash cascades batched, single render triggered
       }
   }
   ```

5. **GlyphId Stability Verification** - Visual indicator
   ```razor
   <div>GlyphId: @(_testComponent?.GetKnowId() ?? "N/A")</div>
   <div class="badge @(wasRecreated ? "bg-warning" : "bg-success")">
       @(wasRecreated ? "RECREATED" : "CACHE REUSED")
   </div>
   ```

---

## 16. Summary: The 5 Critical Patterns

1. **Pause Animations Early**
   - Call `AnimationFrameBus.PauseAllAnimations()` in `OnInitialized()`
   - BEFORE Canvas3DComponent initializes
   - Ensures manual control from start

2. **Wait for Canvas Initialization**
   - `await Task.Delay(200)` in OnAfterRenderAsync
   - Then acquire `_stage = _canvasRef.Stage`
   - Prevents NULL reference errors

3. **Use ModelEditor for Parameter Changes**
   - `ModelEditor.SetParameter()` triggers Smash cascade
   - Smash clears cache, next evaluation is CREATE mode
   - Don't modify parameters directly

4. **Manual Frame Triggering**
   - Call `TriggerSingleFrame()` after parameter changes
   - This executes RenderGeometry3D → RenderStage flow
   - Without this, changes don't render

5. **Understand CREATE vs UPDATE**
   - `parameter.IsCasheEmpty()` determines mode
   - Geometry parameters (W/H/D/Type) → Smash → CREATE
   - Transform parameters (X/Y/Z) → UPDATE (cache survives)
   - GlyphId remains stable across recreates

---

*This specification documents the existing ShapeLifecycleTest implementation as of February 1, 2026. All method names verified against current API references.*
