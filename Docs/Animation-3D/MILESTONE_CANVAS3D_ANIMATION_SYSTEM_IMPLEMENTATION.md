# Milestone: Unified Rendering and Animation System

**Date:** November 14, 2025  
**Status:** ✅ Completed and Working  
**Milestone:** Unified 2D/3D rendering architecture with automatic animation startup and Arena-based object lifecycle management

---

## Problem Statement

The Canvas3D component was experiencing JavaScript integration issues that prevented 3D rendering:
- `FoundryWorldsAndDrawings` namespace undefined errors
- Animation system not auto-starting
- 3D objects not responding to animation callbacks
- Unclear distinction between Scene and Arena rendering approaches

---

## Solution Architecture

### 1. JavaScript Namespace Initialization

**File:** `wwwroot/js/foundry-namespace-shim.js`

Created a namespace shim that loads **before** the main `app-lib.js` to prevent "undefined" errors:

```javascript
window.FoundryWorldsAndDrawings = window.FoundryWorldsAndDrawings || {};
```

This ensures the namespace exists before any code tries to reference it.

### 2. Auto-Start Animation System

**File:** `JsLib/src/index.ts`

Modified the JavaScript entry point to automatically start the animation loop:

```typescript
export function Load() {
    window.FoundryApp = new App();
    window.unifiedAnimationManager = unifiedAnimationManager;
    
    // Auto-start animation immediately
    unifiedAnimationManager.startAnimation();
}
```

**Result:** Animation begins as soon as JavaScript loads, no manual intervention needed.

### 3. DotNet Callback Registration

**File:** `AnimationInitializer.razor`

Created a component that auto-registers the C# callback handler on first render:

```razor
@inject IFoundryService FoundryService

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await FoundryService.InitializeAnimationManager();
        }
    }
}
```

This component lives in `MainLayout.razor` and ensures the DotNet reference is registered before any animation frames fire.

### 4. Animation Flow Architecture

```
JavaScript (requestAnimationFrame)
    ↓
UnifiedAnimationManager.animationLoop()
    ↓
dotNetHelper.invokeMethodAsync('TriggerAnimationFrame')
    ↓
FoundryService.TriggerAnimationFrame() [C#]
    ↓
AnimationFrameBus.TriggerAnimationFrame(Drawing) → 2D animations
AnimationFrameBus.TriggerAnimationFrame(World)   → 3D animations
    ↓
Arena.ProcessAnimations() → Calls SetAnimationUpdate() on FoModel3D objects
```

---

## Critical Architecture Discovery: Arena vs Scene

### Arena-Based Rendering (✅ Animations Work)

Objects added to the **Arena** via `arena.AddShapeToStage<T>()` receive animation callbacks:

```csharp
var model = new FoModel3D("MyModel")
{
    Url = GetReferenceTo(@"storage/staticfiles/model.glb"),
    Transform = new Transform3("MyTransform")
    {
        Position = new Vector3(0, 0, 0)
    }
};

model.SetAnimationUpdate((self, tick, fps) =>
{
    // This callback WILL be called every frame
    self.Transform.MoveBy(0.1, 0, 0);
    self.SetDirty(self.Transform.IsDirty);
});

arena.AddShapeToStage<FoModel3D>(model);
```

**Types that work with Arena:**
- `FoModel3D` - 3D models from GLB/GLTF files
- `FoText3D` - 3D text objects
- Other `FoGlyph3D` derived types

### Scene-Based Rendering (❌ Animations Don't Work)

Objects added directly to **Scene** via `scene.AddChild()` do NOT receive animation callbacks:

```csharp
var model = new Model3D()
{
    Name = "MyModel",
    Uuid = Guid.NewGuid().ToString(),
    Url = GetReferenceTo(@"storage/staticfiles/model.glb")
};

model.SetAnimationUpdate((self, tick, fps) =>
{
    // This callback WILL NOT be called!
    // Scene does not process animation callbacks
});

scene.AddChild(model); // ❌ Wrong approach for animations
```

**When to use Scene directly:**
- Static objects that never animate
- Meshes created by Tech classes (e.g., clock face)
- Objects where you manually control updates

---

## Implementation Examples

### Example 1: Animated 3D Model Walking Back and Forth

```csharp
public void DoAddWalkingObject()
{
    var arena = Workspace.GetArena();
    var range = 20.0;
    
    // Use array to hold mutable state (reference type for closure)
    var state = new double[] { 0.5 }; // delta velocity
    
    var model = new FoModel3D("WalkingObject")
    {
        Url = GetReferenceTo(@"storage/staticfiles/model.glb"),
        Transform = new Transform3("ObjectTransform")
        {
            Position = new Vector3(0, 0, 0)
        }
    };
    
    model.SetAnimationUpdate((self, tick, fps) =>
    {
        var delta = state[0];
        var pos = self.Transform.MoveBy(0, 0, delta);
        
        // Reverse direction at boundaries
        if (pos.Z > range)
        {
            state[0] = -Math.Abs(state[0]);
            self.Transform.RotateTo(0, Math.PI, 0, AngleUnit.Radians);
        }
        else if (pos.Z < -range)
        {
            state[0] = Math.Abs(state[0]);
            self.Transform.RotateTo(0, 0, 0, AngleUnit.Radians);
        }
        
        self.SetDirty(self.Transform.IsDirty);
    });
    
    arena.AddShapeToStage<FoModel3D>(model);
}
```

### Example 2: Orbital Animation

```csharp
public void DoAddOrbitalObject()
{
    var arena = Workspace.GetArena();
    var angle = 0.0;
    var radius = 22.0;
    
    var model = new FoModel3D("Orbiter")
    {
        Url = GetReferenceTo(@"storage/staticfiles/sub.glb"),
        Transform = new Transform3("OrbitTransform")
        {
            Position = new Vector3(radius, 0, 0)
        }
    };
    
    model.SetAnimationUpdate((self, tick, fps) =>
    {
        angle += Math.PI / 120; // Increment angle
        var x = radius * Math.Cos(angle);
        var z = radius * Math.Sin(angle);
        
        self.Transform.Position = new Vector3(x, 0, z);
        
        // Point in direction of movement
        var rotationY = Math.Atan2(x, z);
        self.Transform.Rotation = new Euler(0, rotationY, 0, AngleUnit.Radians);
        
        self.SetDirty(self.Transform.IsDirty);
    });
    
    arena.AddShapeToStage<FoModel3D>(model);
}
```

### Example 3: Animated 3D Text

```csharp
public void DoAddAnimatedText()
{
    var arena = Workspace.GetArena();
    var delta = 0.5;
    
    var text3d = new FoText3D()
    {
        Text = "Hello World",
        Color = "Yellow",
        FontSize = 2.0,
        Transform = new Transform3("TextTransform")
        {
            Position = new Vector3(0, 5, 0)
        }
    };
    
    text3d.SetAnimationUpdate((self, tick, fps) =>
    {
        bool move = tick % 10 == 0; // Throttle to every 10th frame
        if (!move) return;
        
        var pos = self.Transform.MoveBy(0, 0, delta);
        
        if (pos.Z > 10 || pos.Z < -10)
        {
            delta = -delta; // Reverse direction
        }
        
        self.SetDirty(self.Transform.IsDirty);
    });
    
    arena.AddShapeToStage<FoText3D>(text3d);
}
```

---

## Important Implementation Notes

### 1. Closure Variables and State Persistence

**❌ WRONG - Value types don't persist in closures:**
```csharp
var delta = 0.5; // Value type

model.SetAnimationUpdate((self, tick, fps) =>
{
    delta = -delta; // This creates a local copy, doesn't persist!
});
```

**✅ CORRECT - Use arrays (reference types) for mutable state:**
```csharp
var state = new double[] { 0.5 }; // Array is reference type

model.SetAnimationUpdate((self, tick, fps) =>
{
    state[0] = -state[0]; // Modifies the array element, persists!
});
```

**✅ ALSO CORRECT - Use accumulation for continuously incrementing values:**
```csharp
var angle = 0.0;

model.SetAnimationUpdate((self, tick, fps) =>
{
    angle += Math.PI / 120; // Accumulation works because it reads previous value
});
```

### 2. Always Call SetDirty()

The rendering system needs to know when transforms change:

```csharp
model.SetAnimationUpdate((self, tick, fps) =>
{
    self.Transform.MoveBy(1, 0, 0);
    self.SetDirty(self.Transform.IsDirty); // ✅ Required!
});
```

### 3. Performance: Throttle Heavy Operations

Not every operation needs to run every frame (~30-60 fps):

```csharp
model.SetAnimationUpdate((self, tick, fps) =>
{
    bool doExpensiveWork = tick % 10 == 0; // Every 10th frame
    if (!doExpensiveWork) return;
    
    // Heavy computation here
});
```

---

## Known Issues and Future Work

### GLB Models with Internal Skeletal Animations

**Issue:** Some GLB files (like `T_Rex.glb`) contain built-in skeletal animations. When loaded, Three.js renders all bone positions simultaneously, creating a "multiple instances" visual effect.

**Example:** The T-Rex walking animation shows multiple dinosaur silhouettes instead of a single animated model.

**Current Workaround:** Use static GLB models without skeletal animations (like `sub.glb`, `BoxAnimated.glb`).

**Future Fix (JavaScript Level):**
- Access the Three.js `AnimationMixer` for loaded GLB models
- Control animation clip playback (play/pause/stop)
- Sync internal animation with transform animation
- This requires changes to the GLB loader in `app-lib.js`

### Transform Animation Works Correctly

Despite the GLB skeletal animation issue, transform-based animations (position, rotation, scale) work perfectly:
- ✅ Submarine walks back and forth cleanly
- ✅ Text oscillates correctly
- ✅ Box moves and rotates smoothly
- ✅ T-Rex **position and rotation** work (just shows multiple skeletal frames)

---

## File Checklist

When implementing Canvas3D animations, ensure these files are configured:

- ✅ `wwwroot/js/foundry-namespace-shim.js` - Namespace creation
- ✅ `JsLib/src/index.ts` - Auto-start animation
- ✅ `JsLib/src/app.ts` - DotNet reference handling
- ✅ `JsLib/src/Animation/UnifiedAnimationManager.ts` - Animation loop
- ✅ `FoundryService.cs` - TriggerAnimationFrame() callback
- ✅ `AnimationInitializer.razor` - Auto-register DotNet helper
- ✅ `MainLayout.razor` - Include AnimationInitializer component
- ✅ `Clock.razor.cs` (or your page) - Subscribe to AnimationEvent

---

## Testing Checklist

To verify the animation system is working:

1. **Namespace Check:**
   - Open browser console
   - Type `window.FoundryWorldsAndDrawings`
   - Should return an object, not undefined

2. **Animation Auto-Start:**
   - Load the Clock page
   - Check console for "Clock Page: Animation frame for World" logs
   - Should appear automatically without clicking buttons

3. **3D Object Animation:**
   - Click "Add Submarine to Arena" button
   - Submarine should move in a circle
   - Check console for submarine position logs

4. **Transform Animation:**
   - Add any FoModel3D or FoText3D to Arena
   - Set animation callback with position changes
   - Object should move smoothly
   - Console logs should show position updates

---

## Performance Characteristics

- **Frame Rate:** ~30-60 fps depending on browser
- **Callback Overhead:** Minimal, callbacks are invoked via JSInvoke
- **Memory:** Animation state stored in closures (minimal overhead)
- **Rendering:** Three.js handles WebGL rendering, very efficient

---

## Summary

The Canvas3D animation system is now fully operational with the following capabilities:

✅ **Auto-starts** when page loads  
✅ **Arena-based animations** work for FoModel3D, FoText3D  
✅ **Transform animations** (position, rotation, scale) functional  
✅ **Clean architecture** separating 2D (Drawing) and 3D (World) events  
✅ **Extensible** - easy to add new animated objects  

**Limitation:** GLB skeletal animations require future JavaScript work to control AnimationMixer.

For questions or issues, reference this document and the example code in `Clock.razor.cs`.
