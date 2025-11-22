# Pivot System Architecture - Shifted Center of Gravity

## 🎯 **Core Concept: Pivot as Shifted Center of Gravity**

The pivot system in this 3D engine fundamentally **shifts the center of gravity** of any piece of geometry. This is not just a rotation point - it's a complete repositioning of where the object's center exists in 3D space.

## 🔧 **Implementation Architecture**

### **The Group Container Pattern**
When you set a pivot on geometry:

1. **Original Geometry** - Has its natural center (geometric center)
2. **Pivot Point Set** - Defines where you want the new center of gravity to be
3. **Group Creation** - The system creates a container/group object
4. **Geometry Offset** - The original geometry is **shifted inside the group**
5. **New Center** - The group's center is now at the pivot location

```csharp
// Example: Box with bottom-center pivot
Transform = new Transform3("BoxTransform")
{
    Position = new Vector3(0, 0, 0),           // Group center at origin
    Pivot = new Vector3(0, -BoxHeight/2, 0)   // Bottom becomes the rotation center
}
```

### **What Happens Internally:**
```
BEFORE PIVOT:
┌─────────────┐
│             │  ← Box geometry center at (0,0,0)
│      ●      │  
│             │
└─────────────┘

AFTER PIVOT (0, -BoxHeight/2, 0):
             ┌─────────────┐
             │             │  ← Box geometry shifted UP inside group
●            │             │  ← Group center (new center of gravity)
             │             │    at original pivot location
             └─────────────┘
```

## 🌟 **Why This Design is Powerful**

### **1. Natural Floor Contact**
```csharp
Position = (0, 0, 0)                    // Place group center at Y=0 (floor)
Pivot = (0, -BoxHeight/2, 0)           // Bottom of box becomes center
// Result: Box bottom sits exactly on floor
```

### **2. Intuitive Door Hinge**
```csharp
Position = (0, 0, 0)                    // Hinge at origin
Pivot = (-BoxWidth/2, -BoxHeight/2, 0) // Left-bottom edge becomes center
// Result: Box rotates around its edge like a real door
```

### **3. Perfect Corner Balancing**
```csharp
Position = (0, 0, 0)                    // Balance point at origin
Pivot = (-BoxWidth/2, -BoxHeight/2, -BoxDepth/2) // Corner becomes center
// Result: Box balances on its corner naturally
```

## 🔄 **JavaScript Integration**

The pivot transformation is **passed through to JavaScript** where the actual visualization occurs:

### **Data Flow:**
1. **C# Transform3** - Defines Position, Rotation, Scale, Pivot
2. **Matrix Calculation** - Combines all transformations into matrix
3. **JavaScript Renderer** - Receives transformation data
4. **Three.js Groups** - Creates container objects with offset geometry
5. **Visual Result** - Geometry appears with shifted center of gravity

### **Why JavaScript Handles It:**
- **Performance** - GPU-accelerated matrix operations
- **Three.js Integration** - Natural group/container system
- **Real-time Updates** - Smooth animations and interactions
- **WebGL Efficiency** - Direct hardware rendering

## 📐 **Mathematical Foundation**

### **Pivot Offset Calculation:**
```
finalPosition = groupPosition + pivotOffset
where:
- groupPosition = Transform.Position (where you want pivot to be)
- pivotOffset = -Transform.Pivot (negative because geometry shifts opposite)
- finalPosition = where geometry actually renders
```

### **Rotation Behavior:**
```
rotationCenter = groupPosition  (always at the pivot point)
geometryRotatesAround = pivotPoint (not original geometry center)
```

## ✅ **Best Practices**

### **Floor Positioning:**
```csharp
// ✅ CORRECT - Use pivot for floor contact
Position = new Vector3(0, 0, 0),
Pivot = new Vector3(0, -height/2, 0)

// ❌ WRONG - Don't adjust position for pivot compensation
Position = new Vector3(0, height/2, 0),  // This fights the pivot system
Pivot = new Vector3(0, 0, 0)
```

### **Animation Transitions:**
```csharp
// ✅ CORRECT - Animate pivot to change center of gravity
tweener.Tween(transform.Pivot, newPivotLocation, duration);

// ✅ ALSO CORRECT - Keep position at desired location
transform.Position = new Vector3(0, 0, 0);  // Keep pivot point at floor
```

## 🎭 **Real-World Analogies**

### **Door Hinge:**
- **Pivot** = Where you install the hinge (edge of door)
- **Position** = Where you place the hinge in the room
- **Rotation** = Door swinging around the hinge point

### **Balancing Act:**
- **Pivot** = Point of balance (tip of finger, corner of object)
- **Position** = Where you place that balance point in space
- **Rotation** = Object tipping around the balance point

### **Puppet on String:**
- **Pivot** = Where you attach the string to the puppet
- **Position** = Where you hold the string (in your hand)
- **Rotation** = Puppet spinning around the attachment point

## 🚀 **System Benefits**

1. **Intuitive Behavior** - Objects behave like real-world physics
2. **Clean Mathematics** - No complex position compensation needed
3. **Animation Friendly** - Smooth transitions between different centers of gravity
4. **Performance Optimized** - JavaScript handles heavy lifting efficiently
5. **Designer Friendly** - Easy to understand and predict behavior

## 🚨 **CRITICAL: Dirty Flag Triggering Patterns**

### **The Object Replacement Requirement**
The dirty flag system only triggers when **objects are replaced**, not when **properties are mutated**:

```csharp
// ✅ WORKS - Object replacement triggers dirty flag
transform.Rotation = new Euler(rotation.X, rotation.Y + 0.1, rotation.Z);
transform.Position = new Vector3(pos.X + 1, pos.Y, pos.Z);

// ❌ FAILS - Property mutation does NOT trigger dirty flag  
transform.Rotation.Y += 0.1;        // Mutates existing Euler object
transform.Position.X += 1;          // Mutates existing Vector3 object
```

### **Why This Design?**
- **Property Setters**: Only fire when you assign to the property itself
- **Object Mutation**: Changing fields inside an object bypasses the setter
- **Dirty Flag Location**: Lives in the property setter, not in the object's fields

### **Two Working Patterns:**

#### **Pattern 1: Object Replacement (Recommended)**
```csharp
// Create new objects to trigger property setters
var current = transform.Rotation;
transform.Rotation = new Euler(current.X, current.Y + deltaY, current.Z);
// ✅ Property setter fires → AssignEuler() → SetDirty(true) → OnChange event
```

#### **Pattern 2: Manual Dirty Flag**
```csharp
// If you must mutate properties directly, manually mark dirty
transform.Rotation.Y += deltaY;     // Direct mutation (no dirty flag)
shape.SetDirty(true);              // Manual dirty flag trigger
// ✅ Shape gets marked dirty → queued for JavaScript update
```

### **Animation Integration**
```csharp
// ✅ Tweener with object replacement
tweener.Tween(transform.Rotation, new { Y = targetY }, duration);
// Tweener creates new Euler objects → automatic dirty flags

// ✅ Manual approach
void UpdateRotation() {
    var current = transform.Rotation;
    transform.Rotation = new Euler(current.X, current.Y + speed, current.Z);
    // Each assignment creates new object → triggers dirty flag
}
```

## 🚨 **CRITICAL: Initialization Dirty Flag Issue**

### **Root Cause: Parent-Child Relationship Timing**

The Transform3 dirty flag system depends on the **parent-child relationship being established FIRST** before setting properties. During initialization, this timing is critical:

**❌ PROBLEMATIC PATTERN**:
```csharp
// Transform created and properties set BEFORE assignment to shape
var transform = new Transform3();
transform.Position = new Vector3(1, 2, 3);  // ❌ No parent to notify!
transform.Pivot = new Vector3(0, -0.5f, 0); // ❌ SetDirty(true) has no shape to notify

var shape = new FoShape3D();
shape.Transform = transform;  // Parent-child relationship established TOO LATE
// Result: Shape not marked dirty, no initial JavaScript render
```

**✅ CORRECT PATTERN**:
```csharp
// Establish parent-child relationship FIRST, then set properties
var transform = new Transform3();
var shape = new FoShape3D();
shape.Transform = transform;  // ✅ Parent-child relationship established FIRST

// NOW property setters can notify the parent shape
shape.Transform.Position = new Vector3(1, 2, 3);  // ✅ Notifies shape via NotifyOwnerOfChange
shape.Transform.Pivot = new Vector3(0, -0.5f, 0); // ✅ Shape gets marked dirty
```

### **The Connection Mechanism**

When a Transform3 is assigned to a shape, `AssignTransform()` establishes the notification system:

```csharp
// From FoGlyph3D.AssignTransform()
newValue.SetOwnerNotification((value) => {
    SetDirty(value);  // Transform can now notify parent shape of changes
});
```

**Key Insight**: Property setters on an "orphaned" Transform3 (no parent shape) will mark the transform dirty but won't propagate to any parent object, so the initial render queue never gets the update.

### **Fix Implementation**

**In Object Initializers**: Transform assignment happens during object creation, so subsequent property sets work correctly:
```csharp
var shape = new FoShape3D() 
{
    Transform = new Transform3()  // ✅ Assignment happens in initializer
};
shape.Transform.Pivot = new Vector3(0, -0.5f, 0);  // ✅ Parent relationship exists
```

**For Two-Step Creation**: Always assign transform before setting properties:
```csharp
var transform = new Transform3();
var shape = new FoShape3D();
shape.Transform = transform;        // ✅ FIRST: Establish relationship
transform.Position = new Vector3(1, 2, 3);  // ✅ THEN: Set properties
```

### **Why This Matters**
- **Initial Render**: Without dirty flag, JavaScript doesn't know to apply pivot transformation
- **Subsequent Updates**: Work fine because they trigger property setters
- **Floor Contact**: Only appears after first property change/refresh

---

*This pivot system is the foundation of all interactive 3D behavior in the application. Understanding the "shifted center of gravity" concept is crucial for effective 3D development.*