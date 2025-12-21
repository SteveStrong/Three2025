# Clock Architecture Guide

## Overview

The clock is a 3D animated component created by `ClockTech.CreateClockOnArena()` and updated every second via `UpdateArenaClock()`. It demonstrates hierarchical scene graph composition, sub-glyph inheritance, and time-based animation.

## Clock Structure (Scene Graph Hierarchy)

```
Clock (FoShape3D - Root Cylinder)
├── Position: (0, 1.2*radius, 0) = (0, 14.4, 0)
├── Rotation: (π/2, 0, 0) - Cylinder rotated 90° on X-axis to stand upright
├── Scale: 12 radius, 0.6 height
│
├── Numbers 1-12 (FoText3D children)
│   └── Each positioned at (radius-1.0, 0.6 height, around circumference)
│       - Angle for number N: N * (2π/12) - π/2 (starts at 12 o'clock)
│       - Inherits clock's parent transforms
│       - Fixed at height 0.6 to avoid clipping into clock face
│
├── Post (FoShape3D - Center cylinder, 0.2×1.0×0.2)
│   └── Position: (0, 0, 0) - Inherits from Clock
│       Rotation: (0, -angle, 0) - **ROTATES EVERY SECOND** based on time
│
└── Hand (FoShape3D - extends from post, child of Post)
    ├── Position: (0.6*radius, 1, 0) = (7.2, 1, 0)
    │   - **CRITICAL**: Hand's local X=0.6*radius places it at 60% of clock radius
    │   - This offset means hand rotates from its END (not center) due to parent Post rotation
    │   - When Post rotates around Y-axis, hand arc traces the second indicator
    │
    ├── Scale: (1.2*radius, 2.0, 0.1) = (14.4, 2.0, 0.1)
    │   - Extends 7.2 units in +X direction (0.6*radius to 1.8*radius from origin)
    │   - Width 0.1 to be thin and blade-like
    │
    └── TimeText (FoText3D - child of Hand)
        ├── Position: (0.6*radius, 0, 0) = (7.2, 0, 0)
        │   - Placed at end of hand in hand's local space
        │   - Shows current time "HH:mm:ss"
        │
        └── FontSize: 1.5
            - Updates every second with current time
```

## Coordinate System & Transforms

### Clock Root Positioning
- **Position**: (0, 1.2*radius, 0) = (0, 14.4, 0)
  - Centers horizontally at origin
  - **Bottom** of clock sits at Y = 1.2*radius - 0.3 (height/2) = 14.1
  - **Top** of clock sits at Y = 1.2*radius + 0.3 = 14.7
  - ✅ This places the base slightly above Y=0 (floor-safe)

- **Rotation**: (π/2, 0, 0)
  - Rotates cylinder 90° around X-axis
  - Default cylinder is vertical (along Y), rotation makes it horizontal
  - After rotation: face points in +Z direction, numbers arranged around Z-axis

- **Radius**: 12 units
  - Diameter: 24 units
  - Numbers positioned at radius-1.0 = 11 units from center
  - Hand positioned at 0.6*radius = 7.2 units from center

### Clock Face Geometry
- **Width**: 24 units (diameter = 2*radius)
- **Height (thickness)**: 0.6 units
- **Numbers Height**: 0.6 units
  - ✅ Positioned at the height of the face to avoid clipping
  - All 12 numbers use same Y coordinate

### Hand Geometry & Pivot Point
```
Hand (local coordinates):
  Position in parent space: (7.2, 1, 0)
  
  Before rotation (local space):
    X extent: -0.6*radius to +0.6*radius = -7.2 to +7.2
    Width in hand's X direction = 1.2*radius = 14.4
    
  After Post rotates (parent space):
    Hand's position (7.2, 1, 0) becomes the pivot/fulcrum
    Hand blade extends in the direction perpendicular to the radius
    Rotation creates arc motion around Post's center at (0, 1, 0)
```

**Why the hand rotates from the end:**
- Hand is positioned at X=0.6*radius (7.2 units out)
- Hand extends ±0.6*radius (±7.2 units) in its local space
- When Post rotates around Y, hand's position (7.2, 1, 0) acts as the rotation axis
- Hand blade sweeps arc from 7.2 units to 14.4 units from center (0.6*radius to 1.8*radius)
- This creates proper second-hand behavior with the hand rotating from its mounting point

## Update Cycle (Every Second)

### 1. Time Calculation
```csharp
var time = DateTime.Now;
var angle = time.Second * (2π/60) - π/2;  // Convert seconds to radians
// -π/2 offset positions second=0 at 12 o'clock (top of clock)
```

- Second 0: angle = 0 - π/2 = -π/2 (12 o'clock position)
- Second 15: angle = 15*(2π/60) - π/2 = π/2 - π/2 = 0 (3 o'clock)
- Second 30: angle = 30*(2π/60) - π/2 = π - π/2 = π/2 (6 o'clock)
- Second 45: angle = 45*(2π/60) - π/2 = 3π/2 - π/2 = π (9 o'clock)

### 2. Post Rotation (Lines 172-179)
```csharp
var post = Clock.FindSubGlyph3D<FoShape3D>("Post");
if (post != null)
{
    post.Transform.RotateTo(0, -angle, 0, AngleUnit.Radians);
}
```

**Action**: Rotate Post around Y-axis by `-angle`
- This rotates the hand, which is a child of Post
- ✅ Hand rotates with Post as they share the same parent transform
- Negative angle because we rotate the post opposite to the angle for proper hand motion

### 3. TimeText Update (Lines 183-188)
```csharp
var hand = post.FindSubGlyph3D<FoShape3D>("Hand");
if (hand != null)
{
    var timeText = hand.FindSubGlyph3D<FoText3D>("TimeText");
    if (timeText != null)
    {
        timeText.Text = time.ToString("HH:mm:ss");
    }
}
```

**Action**: Update text on hand with current time
- TimeText is child of Hand
- Text updates every second with "HH:mm:ss" format
- Inherits hand's transforms automatically

### 4. Clock Face Rotation (Line 194)
```csharp
Clock.Transform.RotateTo(Math.PI / 2, 0, angle, AngleUnit.Radians);
```

**Action**: Rotate entire clock around Z-axis (after the X-axis rotation)
- Keeps clock face numbers always facing toward hand
- Numbers rotate with the hand to maintain visual alignment
- Numbers themselves don't move relative to hand - they're cosmetic

## Expected Behaviors (Verification Checklist)

### ✅ Clock Face
- [ ] Visible as a horizontal disc
- [ ] Red color
- [ ] Sitting approximately at Y=14.4 (raised slightly above floor)
- [ ] Diameter of 24 units (radius 12)

### ✅ Numbers 1-12
- [ ] All 12 numbers visible around clock circumference
- [ ] Positioned at radius 11 units from center
- [ ] All at same height (0.6)
- [ ] Number "12" points upward (toward +Z after initial rotation)
- [ ] White color
- [ ] Font size 1.2

### ✅ Center Post
- [ ] Red cylinder in middle of clock
- [ ] Height 1.0, width/depth 0.2
- [ ] Centered at clock's center
- [ ] **ROTATES** every second

### ✅ Second Hand
- [ ] Green blade extending from post
- [ ] Length 14.4 units, spanning from 7.2 to 14.4 from center
- [ ] Thin (width 0.1)
- [ ] **ROTATES WITH POST** every second
- [ ] Completes full rotation every 60 seconds

### ✅ Time Text
- [ ] White text at end of hand
- [ ] Format: "HH:mm:ss" (e.g., "14:23:45")
- [ ] Updates every second
- [ ] Moves/rotates with hand as child
- [ ] Font size 1.5

## Common Issues & Diagnostics

### Issue: Numbers appear inside clock face (clipping)
- **Cause**: Number height is wrong
- **Fix**: Verify in `LetterText3D()` that `height` parameter = 0.6
- **Check**: `new FoText3D() { Transform = new Transform3() { Position = new Vector3(x, **0.6**, z) } }`

### Issue: Hand doesn't rotate
- **Cause**: Post rotation not working or hand not child of post
- **Check**: Verify `centerPost.AddShape(secondHand)` is called
- **Check**: Verify `post.Transform.RotateTo(0, -angle, 0)` is being called
- **Check**: Verify `-angle` calculation: `time.Second * (2π/60) - π/2`

### Issue: Hand rotates from wrong point (middle instead of end)
- **Cause**: Hand position not set to (0.6*radius, 1, 0)
- **Fix**: In `CreateClockOnArena()`, verify:
  ```csharp
  Position = new Vector3(0.6 * radius, 1, 0),  // radius=12, so X=7.2
  ```

### Issue: Clock tilted or at wrong angle
- **Cause**: Initial rotation wrong
- **Fix**: Clock rotation should be `(π/2, 0, 0)` - only rotate around X to stand it upright
- **Check**: `new Transform3("ClockTransform") { Rotation = new Euler(Math.PI / 2, 0, 0) }`

### Issue: TimeText doesn't appear or is in wrong position
- **Cause**: TimeText not child of hand, or position wrong
- **Fix**: Verify `secondHand.AddShape(timeText)` is called
- **Fix**: Verify position is `(0.6*radius, 0, 0)` in hand's local space

### Issue: Clock is at wrong height
- **Cause**: Position Y value incorrect
- **Expected**: Y = 1.2 * radius = 14.4 (places bottom ~14.1, above floor at Y=0)
- **Fix**: `Position = new Vector3(0, **1.2 * radius**, 0)`

## Architecture Patterns Demonstrated

1. **Scene Graph Hierarchy**: Clock (root) → Post → Hand → TimeText
2. **Transform Inheritance**: Children inherit parent rotations and positions
3. **Shape System**: All components use `FoShape3D` and `FoText3D` with `AddShape()`
4. **Animation via Transform**: Time calculation → angle computation → Transform.RotateTo()
5. **Visitor Pattern**: `FindSubGlyph3D<T>()` locates children by name and type

## Related Files

- **Implementation**: `c:\Users\admin\workspace\Core\Three2025\Apprentice\ClockTech.cs`
- **Scene Graph**: `FoGlyph3D.cs` - base class with hierarchy support
- **Shape Rendering**: `FoShape3D.cs` - geometry and mesh computation
- **Text Rendering**: `FoText3D.cs` - text sub-glyph support
- **Transform System**: `Transform3.cs` - position, rotation, scale operations

## Testing Steps

1. **Launch the arena**: Run Three2025 and navigate to arena with clock
2. **Visual Verification**: 
   - See clock face, numbers, post, hand, and time text
   - Confirm positioning and colors match above
3. **Animation Verification**:
   - Watch hand rotate smoothly every second
   - Confirm time text updates every second
   - Confirm hand completes full rotation every 60 seconds
4. **Timing Verification**:
   - At second=0: Hand points to "12" (top)
   - At second=15: Hand points to "3" (right)
   - At second=30: Hand points to "6" (bottom)
   - At second=45: Hand points to "9" (left)
