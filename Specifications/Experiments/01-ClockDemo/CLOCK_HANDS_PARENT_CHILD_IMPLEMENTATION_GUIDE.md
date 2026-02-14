# Clock Hands Parent-Child Hierarchy Implementation Guide

**Document Purpose:** Deep-dive implementation guide for parent-child shape hierarchies in 3D  
**Specific Focus:** Clock hands attached to center post (the issue that blocked ClockDemo)  
**Prepared By:** Claude "Atlas" Architect (with hindsight from AAR)  
**Date:** February 1, 2026  
**Target Audience:** Claude "Indy" Builder (and future implementers)

---

## Executive Summary

**The Problem:** Clock hands need to rotate around their attachment point (base) while being children of a center post. Getting parent-child hierarchies wrong means children don't render.

**The Root Cause:** Collection type mismatch. Generic `Add<T>()` stores items in concrete type collections, but traversal queries base type collections.

**The Solution:** Use `FoGlyph3DEditor` which explicitly stores children in the base `FoGlyph3D` collection type, ensuring `AllSubGlyph3Ds()` finds them during rendering.

**Time to Implement:** 15-30 minutes if you follow this guide exactly.

---

## Architecture Overview

### How Parent-Child Rendering Works

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Stage.AddShape(centerPost)                                │
│    → centerPost stored in stage                              │
│    → centerPost has NO parent (top-level shape)              │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 2. centerPost.AddShape(hourHand)                             │
│    → Uses FoGlyph3DEditor.AddShape()                         │
│    → Stores hourHand in FoGlyph3D collection (base type)     │
│    → Sets hourHand.Parent = centerPost                       │
│    → Sets hourHand.OnDelete = centerPost.RemoveChild         │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 3. Animation Frame Arrives                                   │
│    → CollectChanges() walks stage shapes                     │
│    → centerPost is collected (no parent, so independent)     │
│    → hourHand is NOT collected (has parent, so dependent)    │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 4. GetComputedMesh(sceneName) called on centerPost          │
│    → centerPost.RecomputeMesh() creates Mesh3D for post     │
│    → centerPost.AllSubGlyph3Ds() returns [hourHand, ...]    │
│    → For each child: childMesh = child.GetComputedMesh()    │
│    → parentMesh.AddChild(childMesh)                          │
│    → Result: Mesh3D with children array populated           │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│ 5. JavaScript THREE.js Rendering                             │
│    → Creates THREE.Mesh for centerPost                       │
│    → Creates THREE.Mesh for each child                       │
│    → Calls parentMesh.add(childMesh)                         │
│    → THREE.js applies parent transform to children           │
│    → Result: Rotating parent rotates children                │
└─────────────────────────────────────────────────────────────┘
```

### Critical Insight

**Children are NEVER added to the stage directly.**  
**Children are NEVER collected independently.**  
**Children are rendered via their parent's `GetComputedMesh()` recursion.**

This preserves THREE.js parent-child transform inheritance.

---

## The Collection Type Mismatch Bug (Fixed)

### What Was Broken

```csharp
// WRONG WAY (caused the bug)
var editor = new MxComponentEditor<FoShape3D>(centerPost);
editor.Add(hourHand);  // Generic Add<T> infers type FoShape3D

// Result: hourHand stored in Collection<FoShape3D>
// Problem: AllSubGlyph3Ds() queries Collection<FoGlyph3D>
// Outcome: Child not found during rendering traversal
```

### What's Fixed Now

```csharp
// RIGHT WAY (using specialized editor)
var editor = centerPost.EstablishEditor<FoGlyph3DEditor>();
editor.AddShape(hourHand);

// Inside FoGlyph3DEditor.AddShape():
Add<FoGlyph3D>(child);  // EXPLICIT base type parameter

// Result: hourHand stored in Collection<FoGlyph3D> (base type)
// AllSubGlyph3Ds() queries Collection<FoGlyph3D>
// Outcome: Child found during rendering traversal ✅
```

### Why This Matters

The generic `MxComponentEditor<T>` stores items in `Collection<T>` where T is the concrete type you pass. If you add an `FoShape3D`, it goes into `Collection<FoShape3D>`.

But `AllSubGlyph3Ds()` queries `Collection<FoGlyph3D>` (the base type). If your children are in the wrong collection, traversal misses them and they don't render.

**`FoGlyph3DEditor` was created specifically to solve this** - it forces base type storage.

---

## Implementation Steps

### Step 1: Create Center Post (Parent Shape)

```csharp
// Verified against: FOUNDRY_WORLDS_AND_DRAWINGS_API_REFERENCE.md

var centerPost = new FoShape3D("ClockCenterPost")
{
    GeomType = "Cylinder",
    Width = 0.1,   // Diameter of post
    Height = 0.3,  // Height of post
    Depth = 0.1,
    Color = "Black",
    Opacity = 1.0
};

// Position slightly forward so hands are visible
centerPost.Transform.Position = new Vector3(0, 0, 0.2);

// Add to stage (makes it top-level, independent shape)
_stage.AddShape(centerPost);
```

**Verification:**
- [ ] `centerPost` visible in canvas
- [ ] `_stage.GetCollection<FoGlyph3D>()` contains centerPost

### Step 2: Create Clock Hand Shapes (Children)

```csharp
// Hour hand - shortest, thickest
var hourHand = new FoShape3D("HourHand")
{
    GeomType = "Box",
    Width = 0.08,   // Thickness
    Height = 1.0,   // Length
    Depth = 0.05,   // Depth
    Color = "Red",
    Opacity = 1.0
};

// Minute hand - longer, medium thickness
var minuteHand = new FoShape3D("MinuteHand")
{
    GeomType = "Box",
    Width = 0.05,
    Height = 1.5,
    Depth = 0.05,
    Color = "Blue",
    Opacity = 1.0
};

// Second hand - longest, thinnest
var secondHand = new FoShape3D("SecondHand")
{
    GeomType = "Box",
    Width = 0.03,
    Height = 2.0,
    Depth = 0.05,
    Color = "Yellow",
    Opacity = 1.0
};
```

**Important:** Do NOT add these to the stage. They are children, not top-level shapes.

### Step 3: Add Hands as Children of Center Post

**THIS IS THE CRITICAL STEP** - Use the virtual method, not the editor directly:

```csharp
// RIGHT WAY: Use virtual method on parent
// Polymorphism automatically uses FoGlyph3DEditor
centerPost.AddShape(hourHand);
centerPost.AddShape(minuteHand);
centerPost.AddShape(secondHand);

// What happens internally:
// 1. centerPost.AddShape() calls EstablishEditor<FoGlyph3DEditor>()
// 2. Editor.AddShape() calls WireParentAndDeleteAction(centerPost, child)
//    - Sets child.Parent = centerPost
//    - Sets child.OnDelete = centerPost.RemoveChild
// 3. Editor.AddShape() calls Add<FoGlyph3D>(child)  ← EXPLICIT BASE TYPE
// 4. Editor calls centerPost.SetStructureStale()
```

**Verification Immediately After:**
```csharp
var children = centerPost.AllSubGlyph3Ds();
$"Center post has {children.Count} children".WriteInfo();
// Should print: "Center post has 3 children"

foreach (var child in children)
{
    $"  Child: {child.Name}".WriteInfo();
}
// Should print: "Child: HourHand", "Child: MinuteHand", "Child: SecondHand"

// Also verify collection directly
var collection = centerPost.GetCollection<FoGlyph3D>();
$"FoGlyph3D collection has {collection.Count} items".WriteInfo();
// Should be 3

// If this is 0, you have the collection type mismatch bug
```

### Step 4: Position Hands Relative to Parent

Children use **local coordinates** relative to parent's origin:

```csharp
// Position hands so they extend upward from center post
// Hand geometry center is at (0, 0, 0) by default
// To rotate from base, offset upward by half the hand length

hourHand.Transform.Position = new Vector3(0, hourHand.Height / 2, 0.05);
minuteHand.Transform.Position = new Vector3(0, minuteHand.Height / 2, 0.1);
secondHand.Transform.Position = new Vector3(0, secondHand.Height / 2, 0.15);

// Z-offset (0.05, 0.1, 0.15) prevents z-fighting (hands at different depths)
```

**Why offset by half height?**
- Hand geometry's center is at (0, 0, 0)
- If position is (0, 0, 0), hand extends equally above/below origin
- Offset by Height/2 moves base to origin, hand extends upward
- Now rotation around Z-axis spins from base (clock-like motion)

### Step 5: Animate Hand Rotations

In your `OnAnimationFrame` handler:

```csharp
private void OnAnimationFrame(long tick, double fps, double timeSinceStartSeconds)
{
    if (!_clockRunning)
        return;

    var now = DateTime.Now;
    
    // Calculate angles (0-360 degrees, converted to radians)
    // Clock hands rotate CLOCKWISE, so negate angles
    var secondAngle = -(now.Second * 6.0) * Math.PI / 180.0;  // 6° per second
    var minuteAngle = -(now.Minute * 6.0 + now.Second * 0.1) * Math.PI / 180.0;  // 6° per minute + smooth
    var hourAngle = -((now.Hour % 12) * 30.0 + now.Minute * 0.5) * Math.PI / 180.0;  // 30° per hour + smooth
    
    // Rotate hands around Z-axis (perpendicular to clock face)
    secondHand.Transform.Rotation = new Vector3(0, 0, secondAngle);
    minuteHand.Transform.Rotation = new Vector3(0, 0, minuteAngle);
    hourHand.Transform.Rotation = new Vector3(0, 0, hourAngle);
    
    // CRITICAL: Mark transform as stale so rendering system detects change
    secondHand.SetTransformStale();
    minuteHand.SetTransformStale();
    hourHand.SetTransformStale();
    
    // Also mark parent as structure stale (children changed)
    centerPost.SetStructureStale();
}
```

**Key Points:**
- Angles in **radians**, not degrees (multiply degrees by π/180)
- **Negate angles** for clockwise rotation
- **SetTransformStale()** is REQUIRED or changes won't render
- **SetStructureStale()** on parent ensures children are re-processed

---

## Code Path Trace: How Children Render

### When Animation Frame Arrives

```
1. AnimationFrameBus.OnAnimationFrame()
   └─> Calls all subscribed handlers (including your OnAnimationFrame)
       └─> You update hand rotations and call SetTransformStale()

2. AnimationFrameBus.SortIntoBuckets()
   └─> Iterates all shapes in all stages
   └─> For centerPost (no parent): Added to collector.Independents
   └─> For hourHand (has parent): SKIPPED (not added to collector)
       ↓
       Code in FoGlyph3D.CollectChanges():
       var hasGlyphParent = Parent is FoGlyph3D && Parent is not FoStage3D;
       if (hasGlyphParent) {
           // Don't add to collector - parent will render us
           return;
       }

3. Scene3D.ProcessCollectedChanges()
   └─> Wave 1: Process Independents
       └─> For centerPost:
           └─> GetComputedMesh(sceneName) called

4. FoGlyph3D.GetComputedMesh(sceneName)
   ├─> centerPost.RecomputeMesh() creates Mesh3D for post
   │
   ├─> var subGlyphs = AllSubGlyph3Ds()
   │   └─> Calls EstablishEditor<FoGlyph3DEditor>()
   │   └─> Returns editor.AllShapes()
   │       └─> Queries GetCollection<FoGlyph3D>()  ← Base type query
   │       └─> Returns [hourHand, minuteHand, secondHand]
   │
   └─> For each child in subGlyphs:
       ├─> var (success, childMesh) = child.GetComputedMesh(sceneName)
       │   └─> Recursive call! child renders itself
       │       └─> hourHand.RecomputeMesh() creates Mesh3D for hand
       │
       └─> parentMesh.AddChild(childMesh)
           └─> Adds childMesh to parent's Children array
           └─> child.ClearAllStaleFlags() ← CRITICAL: Prevents infinite rebuild

5. Result: Mesh3D object with:
   {
     "Type": "Mesh3D",
     "Uuid": "centerPost-uuid",
     "Geometry": { BoxGeometry },
     "Material": { color: "Black" },
     "Transform": { position, rotation, scale },
     "Children": [
       { "Uuid": "hourHand-uuid", "Transform": {...}, ... },
       { "Uuid": "minuteHand-uuid", "Transform": {...}, ... },
       { "Uuid": "secondHand-uuid", "Transform": {...}, ... }
     ]
   }

6. JavaScript Receives This and Creates THREE.js Scene Graph:
   centerPostMesh = new THREE.Mesh(...)
   centerPostMesh.add(new THREE.Mesh(...))  // hourHand
   centerPostMesh.add(new THREE.Mesh(...))  // minuteHand
   centerPostMesh.add(new THREE.Mesh(...))  // secondHand
   
   → THREE.js automatically applies parent transform to children
   → Rotating centerPost rotates all hands
   → Rotating individual hands works relative to parent
```

### The Key Insight

**Children are rendered recursively during parent's `GetComputedMesh()`.**  
**They are NEVER added to the collector independently.**  
**They are NEVER sent to JavaScript as separate top-level objects.**

This is by design - it preserves THREE.js's parent-child transform hierarchy.

---

## Troubleshooting Guide

### Issue 1: Hands Not Visible At All

**Symptom:** Center post renders, but no hands visible.

**Diagnosis:**
```csharp
// Check if children were added
var children = centerPost.AllSubGlyph3Ds();
$"Children count: {children.Count}".WriteInfo();

if (children.Count == 0)
{
    // Problem: Children not in collection
    var collection = centerPost.GetCollection<FoGlyph3D>();
    $"FoGlyph3D collection count: {collection.Count}".WriteInfo();
    
    // Also check concrete type collection (wrong place)
    var shapeCollection = centerPost.GetCollection<FoShape3D>();
    $"FoShape3D collection count: {shapeCollection.Count}".WriteInfo();
    
    if (shapeCollection.Count > 0)
    {
        "❌ COLLECTION TYPE MISMATCH BUG".WriteError();
        "Children stored in FoShape3D collection instead of FoGlyph3D".WriteError();
        "Solution: Use centerPost.AddShape() not editor.Add()".WriteError();
    }
}
```

**Causes:**
1. Used `MxComponentEditor` instead of `FoGlyph3DEditor`
2. Used `editor.Add<FoShape3D>()` instead of `editor.Add<FoGlyph3D>()`
3. Added hands to stage instead of centerPost

**Solution:**
```csharp
// Always use the virtual method:
centerPost.AddShape(hourHand);  // Not editor.Add(hourHand)
```

### Issue 2: Hands Visible But Not Rotating

**Symptom:** Hands appear, but stay static even though rotation code runs.

**Diagnosis:**
```csharp
// In OnAnimationFrame, add logging
hourHand.Transform.Rotation = new Vector3(0, 0, angle);
$"Set hourHand rotation to {angle} radians".WriteInfo();

// Check if SetTransformStale was called
hourHand.SetTransformStale();
var isStale = hourHand.IsTransformStale();
$"After SetTransformStale: isStale={isStale}".WriteInfo();
// Should print true
```

**Causes:**
1. Forgot to call `SetTransformStale()` after rotation
2. Forgot to call `SetStructureStale()` on parent
3. Child not being re-rendered (parent cache stale)

**Solution:**
```csharp
// ALWAYS call stale flags after transform changes
hourHand.Transform.Rotation = new Vector3(0, 0, angle);
hourHand.SetTransformStale();  // ← REQUIRED

// Also mark parent structure as changed
centerPost.SetStructureStale();  // ← Ensures children are processed
```

### Issue 3: Hands Rotate From Center, Not Base

**Symptom:** Hands spin around their midpoint instead of rotating from attachment point.

**Cause:** Hand geometry center is at origin, needs offset.

**Solution:**
```csharp
// Position hand so base is at parent's origin
hourHand.Transform.Position = new Vector3(0, hourHand.Height / 2, 0.05);
//                                         ^  ^^^^^^^^^^^^^^^^^^^^^
//                                         |  Offset by half height
//                                         Center X, offset Y upward
```

**Why This Works:**
- Hand geometry extends from -Height/2 to +Height/2 in Y
- Offsetting position by +Height/2 moves base to origin
- Rotation around Z-axis at origin now spins from base

**Alternative (Advanced):** Use `Transform.Pivot` to set rotation origin:
```csharp
hourHand.Transform.Pivot = new Vector3(0, -hourHand.Height / 2, 0);
// Sets rotation pivot to bottom of hand geometry
```

### Issue 4: Hands Added to Stage Instead of Parent

**Symptom:** Hands render, but don't follow parent transforms.

**Diagnosis:**
```csharp
var hourHandParent = hourHand.Parent;
$"HourHand parent: {hourHandParent?.Name ?? "NULL"}".WriteInfo();
// Should print: "HourHand parent: ClockCenterPost"

if (hourHandParent == null)
{
    "❌ Hand has no parent - was added to stage directly".WriteError();
}
```

**Solution:**
```csharp
// DON'T DO THIS:
_stage.AddShape(hourHand);  // ❌ Makes hourHand independent

// DO THIS:
centerPost.AddShape(hourHand);  // ✅ Makes hourHand a child
```

### Issue 5: Children Render Once Then Disappear

**Symptom:** First frame shows children, then they vanish.

**Cause:** Children's stale flags not being cleared, causing infinite rebuild loop.

**Diagnosis:**
```csharp
// In FoGlyph3D.GetComputedMesh(), verify child cleanup:
foreach (var subGlyph in subGlyphs)
{
    var (success, childMesh) = subGlyph.GetComputedMesh(sceneName);
    if (success)
    {
        _cachedValue3D.AddChild(childMesh);
        subGlyph.ClearAllStaleFlags();  // ← CRITICAL
        $"Cleared stale flags for {subGlyph.Name}".WriteInfo();
    }
}
```

**Solution:** Verify `FoGlyph3D.GetComputedMesh()` clears child stale flags after rendering.

---

## Verification Checklist

Use this checklist to verify your implementation:

### Build/Compile
- [ ] Zero compilation errors
- [ ] Zero compilation warnings
- [ ] All using statements resolve

### Initial Render
- [ ] Center post visible in canvas
- [ ] All three hands visible
- [ ] Hands positioned correctly (extending upward)
- [ ] No z-fighting (hands at different depths)

### Parent-Child Relationship
- [ ] `centerPost.AllSubGlyph3Ds()` returns 3 children
- [ ] Each hand's `Parent` property points to centerPost
- [ ] Children NOT in `_stage.GetCollection<FoGlyph3D>()`
- [ ] Children IN `centerPost.GetCollection<FoGlyph3D>()`

### Animation
- [ ] Second hand completes one rotation in 60 seconds
- [ ] Minute hand completes one rotation in 60 minutes
- [ ] Hour hand completes one rotation in 12 hours
- [ ] Hands rotate clockwise (viewed from front)
- [ ] Hands rotate from base, not center
- [ ] FPS remains stable (>30 fps)

### Console Output
- [ ] No errors in browser console
- [ ] No warnings about missing shapes
- [ ] Collection counts are correct in logs
- [ ] Transform updates logged for each hand

---

## Reference Implementation

### Complete Code Example

```csharp
// In your component's OnInitializedAsync:

// Create center post (parent)
var centerPost = new FoShape3D("ClockCenterPost")
{
    GeomType = "Cylinder",
    Width = 0.1,
    Height = 0.3,
    Depth = 0.1,
    Color = "Black",
    Opacity = 1.0
};
centerPost.Transform.Position = new Vector3(0, 0, 0.2);
_stage.AddShape(centerPost);  // Top-level shape

// Create hour hand (child)
var hourHand = new FoShape3D("HourHand")
{
    GeomType = "Box",
    Width = 0.08,
    Height = 1.0,
    Depth = 0.05,
    Color = "Red",
    Opacity = 1.0
};
hourHand.Transform.Position = new Vector3(0, hourHand.Height / 2, 0.05);

// Create minute hand (child)
var minuteHand = new FoShape3D("MinuteHand")
{
    GeomType = "Box",
    Width = 0.05,
    Height = 1.5,
    Depth = 0.05,
    Color = "Blue",
    Opacity = 1.0
};
minuteHand.Transform.Position = new Vector3(0, minuteHand.Height / 2, 0.1);

// Create second hand (child)
var secondHand = new FoShape3D("SecondHand")
{
    GeomType = "Box",
    Width = 0.03,
    Height = 2.0,
    Depth = 0.05,
    Color = "Yellow",
    Opacity = 1.0
};
secondHand.Transform.Position = new Vector3(0, secondHand.Height / 2, 0.15);

// Add hands as children (CRITICAL: use virtual method)
centerPost.AddShape(hourHand);
centerPost.AddShape(minuteHand);
centerPost.AddShape(secondHand);

// Verify children were added
var children = centerPost.AllSubGlyph3Ds();
$"✅ Center post has {children.Count} children".WriteSuccess();

// Store references for animation
_centerPost = centerPost;
_hourHand = hourHand;
_minuteHand = minuteHand;
_secondHand = secondHand;
```

```csharp
// In your OnAnimationFrame handler:

private void OnAnimationFrame(long tick, double fps, double timeSinceStartSeconds)
{
    if (!_clockRunning || _hourHand == null)
        return;

    var now = DateTime.Now;
    
    // Calculate angles in radians (negate for clockwise)
    var secondAngle = -(now.Second * 6.0) * Math.PI / 180.0;
    var minuteAngle = -(now.Minute * 6.0 + now.Second * 0.1) * Math.PI / 180.0;
    var hourAngle = -((now.Hour % 12) * 30.0 + now.Minute * 0.5) * Math.PI / 180.0;
    
    // Update transforms
    _secondHand.Transform.Rotation = new Vector3(0, 0, secondAngle);
    _minuteHand.Transform.Rotation = new Vector3(0, 0, minuteAngle);
    _hourHand.Transform.Rotation = new Vector3(0, 0, hourAngle);
    
    // Mark as stale (CRITICAL)
    _secondHand.SetTransformStale();
    _minuteHand.SetTransformStale();
    _hourHand.SetTransformStale();
    _centerPost.SetStructureStale();
}
```

---

## Known Limitations (Acceptable for MVP)

1. **Hands rotate from center offset, not true pivot**
   - Current solution: Offset position by Height/2
   - Better solution: Use `Transform.Pivot` (not yet fully implemented)
   - Impact: Works correctly for clock, but not generalizable to doors/hinges

2. **No rotation easing/interpolation**
   - Current: Immediate angle updates
   - Better: Smooth interpolation between angles
   - Impact: Minor visual jitter at low FPS

3. **No collision detection between hands**
   - Current: Hands can overlap/intersect
   - Better: Z-ordering or collision avoidance
   - Impact: Visual only, no functional issue

---

## API Reference Quick Links

- **FoGlyph3D.AddShape()**: [FoGlyph3D.cs](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/Shapes3D/FoGlyph3D.cs#L690)
- **FoGlyph3DEditor.AddShape()**: [FoGlyph3DEditor.cs](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/Shapes3D/FoGlyph3DEditor.cs#L47)
- **FoGlyph3D.AllSubGlyph3Ds()**: [FoGlyph3D.cs](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/Shapes3D/FoGlyph3D.cs#L715)
- **FoGlyph3D.GetComputedMesh()**: [FoGlyph3D.cs](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/Shapes3D/FoGlyph3D.cs#L82)
- **FoGlyph3D.CollectChanges()**: [FoGlyph3D.cs](c:/Users/admin/workspace/Core/FoundryWorldsAndDrawings/Shapes3D/FoGlyph3D.cs#L839)

---

## Summary: The 5 Critical Points

1. **Use `centerPost.AddShape(hand)` not `editor.Add(hand)`**
   - Virtual method ensures FoGlyph3DEditor is used
   - Editor stores in base FoGlyph3D collection (not concrete type)

2. **Children are NEVER added to stage**
   - Only parents go to stage
   - Children added to parent via AddShape()

3. **Children are rendered recursively**
   - Parent's GetComputedMesh() calls child.GetComputedMesh()
   - Parent's Mesh3D has Children array with child meshes
   - JavaScript receives complete hierarchy, not separate objects

4. **Always call SetTransformStale() after rotation**
   - Without this, changes aren't detected
   - Also call SetStructureStale() on parent

5. **Offset child position by half its height**
   - Makes rotation happen from base, not center
   - Alternative: Use Transform.Pivot (advanced)

---

*This guide is based on the ClockDemo After Action Review findings and verified against the current FoundryWorldsAndDrawings implementation as of February 1, 2026.*
