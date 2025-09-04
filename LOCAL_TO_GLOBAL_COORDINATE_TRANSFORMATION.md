# Local to Global Coordinate Transformation for 3D Object Alignment

## Overview

This document explains the critical distinction between **local** and **global** coordinate systems in 3D space, the role of **pivot points** in transformations, and how proper transformation between coordinate systems is essential for accurate object alignment, particularly in LEGO-style snapping systems.

## Architectural Foundation: SpacialBox3D vs SpacialFrame3D

### **SpacialBox3D** - Pure Local Coordinate System
**SpacialBox3D** serves as the **geometric foundation** that:
- Defines vertices, faces, edges, normals in **pure local coordinates** only
- Has **no transformation logic** built into it
- Always returns geometry relative to its **birth state** (origin-centered, unrotated)
- Acts as the **canonical geometric definition**
- **All methods prefixed with "Local"** to indicate coordinate space

```csharp
// SpacialBox3D - Always returns LOCAL coordinates
var box = new SpacialBox3D(2.0, 1.0, 1.0);
var faces = box.GetLocalFacesWithNormals();
var frontNormal = faces.First(f => f.Name == "Front").Normal;
// ALWAYS returns Vector3(0, 0, 1) - pure local, canonical geometry
```

### **SpacialFrame3D** - Global Coordinate Transformation Engine
**SpacialFrame3D** inherits from SpacialBox3D and **adds transformation capability**:
- Takes the local geometry from SpacialBox3D
- Applies transformation matrices (position, rotation, scale, pivot)
- Returns **world coordinates** by transforming the local data
- **Simple method names** since these ARE the final global results

```csharp
// SpacialFrame3D - Returns GLOBAL coordinates
var frame = new SpacialFrame3D(spec);
frame.SetTransform(x: 5, y: 0, z: 0, rx: 0, ry: 90, rz: 0); // 90° Y rotation

var faces = frame.GetFacesWithNormals();
var frontNormal = faces.First(f => f.Name == "Front").Normal;
// Returns Vector3(-1, 0, 0) - transformed to global space!
```

### **Clear Naming Convention**
- **SpacialBox3D**: `GetLocal*()` methods → Always local coordinate data
- **SpacialFrame3D**: `Get*()` methods → Always global coordinate data (no "Global" prefix needed)

This creates a **clean separation of concerns**:
- **SpacialBox3D**: Pure geometry definitions (immutable local space)
- **SpacialFrame3D**: Transformation engine (applies matrices to local geometry)

## The Fundamental Problem

When working with 3D objects that can be positioned and rotated in space, we need to distinguish between:

1. **Local Coordinates** - The object's "birth" state at origin with no rotation
2. **Global Coordinates** - Where the object actually exists in world space after transformation
3. **Pivot Point** - The point around which rotations occur (not necessarily the geometric center)

**Critical Insight**: Alignment calculations must work with global coordinates, and transformations must account for the pivot point offset.

## Pivot Point Fundamentals

### What is a Pivot Point?

A **pivot point** is the fixed point in space around which an object rotates. Unlike scaling or translation, rotation always occurs around a specific point.

**Key Concept**: The pivot point can be:
- **Geometric center** - Most common, object rotates around its center
- **Corner point** - Object rotates around one of its corners
- **Edge midpoint** - Object rotates around the middle of an edge
- **Face center** - Object rotates around the center of a face
- **Arbitrary point** - Even outside the object entirely

### Pivot Point Examples

```csharp
// Different pivot points for the same box
var boxCenter = new Vector3(0, 0, 0);           // Geometric center
var cornerPivot = new Vector3(-1, -0.5, -0.5);  // Bottom-left-back corner
var topFacePivot = new Vector3(0, 0.5, 0);      // Center of top face
var externalPivot = new Vector3(2, 1, 1);       // Point outside the box
```

### Why Pivot Points Matter

When you rotate an object around different pivot points, **the same rotation produces different final positions**:

```csharp
// 90° rotation around Y-axis with different pivots
// Box originally at center (0, 0, 0) with width=2, height=1, depth=1

// Pivot at geometric center (0, 0, 0)
// Result: Box rotates in place, center remains at (0, 0, 0)

// Pivot at corner (-1, -0.5, -0.5) 
// Result: Box swings around corner, center moves to (0.5, -0.5, 1)

// Pivot at external point (3, 0, 0)
// Result: Box orbits around external point, center moves to (3, 0, -3)
```

## Local Coordinate System (Birth State)

### Box Face Normals in Local Space
Every axis-aligned box has predictable face normals in its local coordinate system:

```csharp
var localNormals = new Dictionary<string, Vector3>
{
    ["Top"]    = new Vector3( 0,  1,  0),  // Points up    (+Y)
    ["Bottom"] = new Vector3( 0, -1,  0),  // Points down  (-Y)
    ["Front"]  = new Vector3( 0,  0,  1),  // Points forward (+Z)
    ["Back"]   = new Vector3( 0,  0, -1),  // Points backward (-Z)
    ["Right"]  = new Vector3( 1,  0,  0),  // Points right (+X)
    ["Left"]   = new Vector3(-1,  0,  0)   // Points left  (-X)
};
```

### Box Face Centers in Local Space
For a box with dimensions (width, height, depth):

```csharp
var localFaceCenters = new Dictionary<string, Vector3>
{
    ["Top"]    = new Vector3(0, +height/2, 0),
    ["Bottom"] = new Vector3(0, -height/2, 0),
    ["Front"]  = new Vector3(0, 0, +depth/2),
    ["Back"]   = new Vector3(0, 0, -depth/2),
    ["Right"]  = new Vector3(+width/2, 0, 0),
    ["Left"]   = new Vector3(-width/2, 0, 0)
};
```

### Box Vertices in Local Space
```csharp
var localVertices = new Vector3[]
{
    new Vector3(-width/2, -height/2, -depth/2),  // Bottom-left-back
    new Vector3(+width/2, -height/2, -depth/2),  // Bottom-right-back
    new Vector3(-width/2, +height/2, -depth/2),  // Top-left-back
    new Vector3(+width/2, +height/2, -depth/2),  // Top-right-back
    new Vector3(-width/2, -height/2, +depth/2),  // Bottom-left-front
    new Vector3(+width/2, -height/2, +depth/2),  // Bottom-right-front
    new Vector3(-width/2, +height/2, +depth/2),  // Top-left-front
    new Vector3(+width/2, +height/2, +depth/2)   // Top-right-front
};
```

**These values are constant** regardless of where the object is positioned, how it's rotated, or what pivot point is used.

## Global Coordinate System (World Space)

### Transform Matrix with Pivot Point

The critical difference: **rotation must be applied around the pivot point, not the origin**.

**Standard Transform Order with Pivot**:
1. **Translate to pivot** - Move object so pivot point is at origin
2. **Apply rotation** - Rotate around the now-centered pivot
3. **Translate from pivot** - Move object back by inverse of step 1
4. **Apply final translation** - Move to final world position

```csharp
// Complete transformation with pivot point
Matrix4x4 CreateTransformWithPivot(Vector3 position, Vector3 rotation, Vector3 pivotPoint)
{
    var translationMatrix = Matrix4x4.CreateTranslation(position);
    var rotationMatrix = 
        Matrix4x4.CreateRotationX(rotation.X) * 
        Matrix4x4.CreateRotationY(rotation.Y) * 
        Matrix4x4.CreateRotationZ(rotation.Z);
    
    var pivotTranslation = Matrix4x4.CreateTranslation(pivotPoint);
    var inversePivotTranslation = Matrix4x4.CreateTranslation(-pivotPoint);
    
    // Order matters: Final = Translation * PivotTranslation * Rotation * InversePivotTranslation
    return translationMatrix * pivotTranslation * rotationMatrix * inversePivotTranslation;
}
```

### Practical Transform Application

```csharp
// For positions (face centers, vertices) - full transform
Vector3 globalPosition = transformMatrix.TransformPoint(localPosition);

// For directions (normals) - rotation only, no translation or pivot offset
Vector3 globalNormal = rotationMatrix.TransformDirection(localNormal);
```

### Example: Corner Pivot Rotation

Consider a box with dimensions (2, 1, 1) positioned at (0, 0, 0), rotated 90° around Y-axis with pivot at bottom-left-back corner (-1, -0.5, -0.5):

```csharp
// Local state
Vector3 localTopCenter = new Vector3(0, 0.5, 0);        // Top face center
Vector3 localFrontNormal = new Vector3(0, 0, 1);        // Front face normal
Vector3 pivotPoint = new Vector3(-1, -0.5, -0.5);       // Bottom-left-back corner

// After 90° Y rotation around corner pivot
Matrix4x4 transform = CreateTransformWithPivot(
    position: Vector3.Zero,           // No additional translation
    rotation: new Vector3(0, 90°, 0), // 90° around Y
    pivotPoint: pivotPoint
);

Vector3 globalTopCenter = transform.TransformPoint(localTopCenter);     // (0.5, 0.5, 1)
Vector3 globalFrontNormal = rotationMatrix.TransformDirection(localFrontNormal); // (-1, 0, 0)
```

**Key Insight**: The top face center moved from (0, 0.5, 0) to (0.5, 0.5, 1) because the entire box swung around the corner pivot point!

## Pivot Point Strategies

### 1. Geometric Center Pivot (Most Common)
```csharp
Vector3 centerPivot = new Vector3(0, 0, 0);  // Object rotates in place
```
**Use Case**: General object manipulation, most intuitive for users

### 2. Contact Point Pivot
```csharp
Vector3 contactPivot = GetFaceCenter("Bottom");  // Rotate around contact with ground
```
**Use Case**: Objects resting on surfaces, realistic physics simulation

### 3. Corner Pivot
```csharp
Vector3 cornerPivot = new Vector3(-width/2, -height/2, -depth/2);  // Bottom corner
```
**Use Case**: Hinges, doors, mechanical joints

### 4. Edge Pivot
```csharp
Vector3 edgePivot = new Vector3(0, -height/2, 0);  // Bottom edge center
```
**Use Case**: Rolling objects, cylindrical rotations

### 5. External Pivot
```csharp
Vector3 externalPivot = new Vector3(10, 0, 0);  // Point outside object
```
**Use Case**: Orbital motion, crane arms, satellite systems

## Implementation Pattern with SpacialFrame3D Architecture

### Enhanced SnapBox Class Using SpacialFrame3D

```csharp
public class SnapBox : FoShape3D, ISnappable3D
{
    private SpacialFrame3D _spatialFrame;
    
    public void SetTransform(Vector3 position, Vector3 rotation, Vector3 pivotPoint)
    {
        // Update the spatial frame transform - this handles all coordinate transformation
        _spatialFrame.SetTransform(position.X, position.Y, position.Z, 
                                  rotation.X, rotation.Y, rotation.Z);
        
        // Update visual component to match
        Transform.Position = position;
        Transform.Rotation = rotation;
    }
    
    // GLOBAL SPACE - Current world state values (ready for alignment)
    public Face3D GetFace(string faceName)
    {
        // SpacialFrame3D.GetFacesWithNormals() returns GLOBAL coordinates
        return _spatialFrame.GetFacesWithNormals()
                           .First(f => f.Name == faceName);
    }
    
    public Vector3 GetFaceCenter(string faceName)
    {
        // SpacialFrame3D.GetFaceCenters() returns GLOBAL coordinates
        var centers = _spatialFrame.GetFaceCenters();
        return centers.First(c => c.Name == faceName).ToVector3();
    }
    
    public Vector3 GetNormal(string faceName)
    {
        // SpacialFrame3D.GetFacesWithNormals() normals are GLOBAL
        return GetFace(faceName).Normal;
    }
    
    // LOCAL SPACE - Birth state values (if needed for debugging)
    public Vector3 GetLocalNormal(string faceName)
    {
        // SpacialFrame3D.GetLocalFacesWithNormals() returns LOCAL coordinates
        return _spatialFrame.GetLocalFacesWithNormals()
                           .First(f => f.Name == faceName).Normal;
    }
                           .First(f => f.Name == faceName).Normal;
    }
}
```

### SpacialFrame3D-Aware Constraint System

```csharp
public class FaceToFaceConstraint
{
    public SnapResult Execute()
    {
        // Get faces in GLOBAL coordinates - ready for alignment calculations
        var faceA_global = ComponentA.GetFace(FaceNameA);   
        var faceB_global = ComponentB.GetFace(FaceNameB);   
        
        // These are already in world space - no additional transformation needed!
        var targetPosition = faceB_global.Center;
        var sourceNormal = faceA_global.Normal;
        var targetNormal = faceB_global.Normal * -1;
        
        // Calculate alignment using global coordinates
        var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
        
        // Create final transform (ComponentA's SpacialFrame3D will handle pivot automatically)
        var finalTransform = CalculateAlignmentTransform(targetPosition, rotationQuaternion);
        
        return new SnapResult(true, finalTransform);
    }
}
```

## Universal Geometry Constraint System (Updated Architecture)

### Simplified FoShape3D-Based Approach

The constraint system has been updated to work directly with any `FoShape3D` objects without requiring special wrapper classes:

```csharp
// Works with ANY FoShape3D geometry - spheres, cylinders, complex meshes, etc.
public static SnapResult SnapObjects(FoShape3D objectA, string faceA, 
                                    FoShape3D objectB, string faceB)
{
    // 1. WRAP: Create spatial frame workspace around original objects
    var frameA = CreateSpatialFrameWrapper(objectA);
    var frameB = CreateSpatialFrameWrapper(objectB);
    
    // 2. CALCULATE: Use spatial frame's coordinate system for constraint math
    var faceInfoA = frameA.GetFacesWithNormals().First(f => f.Name == faceA);
    var faceInfoB = frameB.GetFacesWithNormals().First(f => f.Name == faceB);
    
    // 3. CONSTRAINT MATH: Calculate required alignment transform
    var targetPosition = new Vector3(faceInfoB.Center.X, faceInfoB.Center.Y, faceInfoB.Center.Z);
    var sourceNormal = faceInfoA.Normal;
    var targetNormal = faceInfoB.Normal * -1; // Opposite for face-to-face contact
    var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
    
    var newTransform = new Transform3
    {
        Position = targetPosition + (faceInfoA.Normal * 0.001), // Small offset
        QuaternionRotation = rotationQuaternion
    };
    
    // 4. PROJECT BACK: Apply calculated transform to original FoShape3D
    objectA.Transform.Position = newTransform.Position;
    objectA.Transform.Rotation = newTransform.Rotation;
    
    return SnapResult.Success();
}

private static SpacialFrame3D CreateSpatialFrameWrapper(FoShape3D shape)
{
    var bounds = shape.GetBoundingBox();
    var spec = new FoSpec3D 
    { 
        W = bounds.Width, H = bounds.Height, D = bounds.Depth,
        X = shape.Transform.Position.X,
        Y = shape.Transform.Position.Y, 
        Z = shape.Transform.Position.Z
    };
    return new SpacialFrame3D(spec);
}
```

### Universal Snapping Benefits

1. **No Special Interfaces**: Any `FoShape3D` becomes instantly snappable
2. **Geometry Independence**: Spheres, boxes, complex meshes all use same API
3. **Mathematical Workspace**: `SpacialFrame3D` provides standardized constraint calculation environment
4. **Clean Projection**: Results applied back to original objects without modification

## Common Pivot Point Scenarios

### Scenario 1: LEGO Brick on Baseplate
```csharp
// LEGO brick resting on baseplate - pivot at bottom center
Vector3 legoBasePivot = new Vector3(0, -height/2, 0);

// When rotated, brick spins around its base contact point
// Top of brick traces a circle, base stays in contact with surface
```

### Scenario 2: Hinged Door
```csharp
// Door attached to frame - pivot at edge
Vector3 doorHingePivot = new Vector3(-width/2, 0, 0);  // Left edge center

// Door swings around hinge line
// Handle traces large arc, hinge stays stationary
```

### Scenario 3: Planetary Motion
```csharp
// Planet orbiting sun - pivot at external point
Vector3 orbitPivot = new Vector3(100, 0, 0);  // Sun position

// Planet orbits around distant sun
// Planet center traces circular path around sun
```

### Scenario 4: Crane Arm
```csharp
// Crane arm - pivot at base
Vector3 craneBasePivot = new Vector3(0, -height, 0);  // Base of crane

// Entire arm rotates around base
// Tip of arm traces large arc, base remains fixed
```

## Verification Strategy

### Testing Pivot Point Effects

```csharp
public void InspectPivotTransformation()
{
    var box = new SnapBox("Test", 2, 1, 1, "blue");
    
    // Test different pivot points with same rotation
    var rotationY90 = new Vector3(0, 90, 0);
    
    // Scenario 1: Center pivot
    box.SetTransform(Vector3.Zero, rotationY90, Vector3.Zero);
    var centerResult = box.GetFaceCenter("Front");
    
    // Scenario 2: Corner pivot  
    var cornerPivot = new Vector3(-1, -0.5, -0.5);
    box.SetTransform(Vector3.Zero, rotationY90, cornerPivot);
    var cornerResult = box.GetFaceCenter("Front");
    
    // Scenario 3: External pivot
    var externalPivot = new Vector3(5, 0, 0);
    box.SetTransform(Vector3.Zero, rotationY90, externalPivot);
    var externalResult = box.GetFaceCenter("Front");
    
    Debug.WriteLine($"Center Pivot Result: {centerResult}");    // Should be close to original
    Debug.WriteLine($"Corner Pivot Result: {cornerResult}");    // Should show swing motion
    Debug.WriteLine($"External Pivot Result: {externalResult}"); // Should show orbital motion
}
```

### Visual Pivot Debugging

```csharp
public void VisualizePivotPoint(SnapBox box)
{
    var arena = Workspace?.GetArena();
    if (arena == null) return;
    
    // Show the actual pivot point
    VisualizationService.CreateMarkerSphere(arena, "PivotPoint", 
        box.GetGlobalPivotPoint(), "#FF0000", 0.1);
    
    // Show how face centers relate to pivot
    foreach (var faceName in new[] {"Top", "Bottom", "Front", "Back", "Left", "Right"})
    {
        var faceCenter = box.GetFaceCenter(faceName);
        
        // Draw line from pivot to face center
        VisualizationService.CreateMarkerCylinder(arena, $"PivotTo{faceName}",
            box.GetPivotPoint(), 
            (faceCenter - box.GetPivotPoint()).Normalized(),
            "#FFFF00", 0.01, Vector3.Distance(box.GetPivotPoint(), faceCenter));
    }
}
```

## Key Principles with SpacialFrame3D Architecture

### 1. **Clear Coordinate Space Distinction**
- **SpacialBox3D**: `GetLocal*()` methods always return birth-state geometry
- **SpacialFrame3D**: `Get*()` methods always return world-transformed geometry
- **No ambiguity**: Method names immediately tell you the coordinate space

### 2. **Transformation Responsibility**
- **SpacialBox3D**: Pure geometric definitions, no transformation logic
- **SpacialFrame3D**: Handles all transformation math (position, rotation, scale, pivot)
- **Clean separation**: Geometry definition vs transformation application

### 3. **Snapping System Benefits**
```csharp
// Always work with global coordinates for alignment
var faceA = snapBoxA.GetFace("Front");  // Already in world space
var faceB = snapBoxB.GetFace("Back");   // Already in world space

// Direct alignment calculation - no additional transforms needed
var alignment = CalculateAlignment(faceA.Normal, faceB.Normal);
```

### 4. **Normals Don't Care About Pivots**
```csharp
// Normals are directions - pivot point doesn't affect them
Vector3 globalNormal = rotationMatrix * localNormal;  // No pivot needed
```

### 5. **Positions Care About Everything**
```csharp
// Positions are affected by rotation around pivot + final translation
Vector3 globalPosition = fullTransformWithPivot * localPosition;
```

### 6. **Choose Appropriate Pivots**
- **User interaction**: Often geometric center for intuitive manipulation
- **Physical simulation**: Contact points or joints for realistic behavior  
- **Mechanical systems**: Hinge points, axles, or attachment points
- **LEGO snapping**: Might be contact faces or edges for realistic assembly

## Conclusion

Successful 3D object alignment with pivot points depends on:

1. **Starting with local coordinates** - Known, predictable geometry
2. **Defining appropriate pivot points** - Based on interaction context
3. **Computing global coordinates** - Applying rotation around pivot, then translation
4. **Aligning based on global coordinates** - Working with actual world positions

The addition of pivot point considerations makes transformations more complex but also more powerful and realistic. For LEGO-style snapping systems, understanding how different pivot points affect final object positions is crucial for creating intuitive and physically plausible snapping behavior.

**Remember**: Objects exist in world space after pivot-aware transformations. Your alignment calculations must account for where objects actually end up, not just where they would be with simple center-based rotations.

## Actual API Reference

### **SpacialBox3D - Local Coordinate Methods**
```csharp
var box = new SpacialBox3D(width, height, depth);

// All methods return LOCAL coordinates (birth state)
List<Point3D> vertices = box.GetLocalVertices();
List<Point3D> centers = box.GetLocalFaceCenters();
List<Face3D> faces = box.GetLocalFacesWithNormals();
List<Edge3D> edges = box.GetLocalEdgesWithNames();
```

### **SpacialFrame3D - Global Coordinate Methods**
```csharp
var frame = new SpacialFrame3D(spec);
frame.SetTransform(x, y, z, rx, ry, rz);

// All methods return GLOBAL coordinates (transformed) - no "Global" prefix needed
List<Point3D> vertices = frame.GetVertices();
List<Point3D> centers = frame.GetFaceCenters();
List<Face3D> faces = frame.GetFacesWithNormals();
List<Edge3D> edges = frame.GetEdgesWithNames();
```

### **Usage in Tests**
```csharp
// SpacialBoxTest.razor.cs - Testing local coordinates
var faces = CurrentBox.GetLocalFacesWithNormals();    // Local normals
var edges = CurrentBox.GetLocalEdgesWithNames();      // Local edges

// SpacialFrameTest.razor.cs - Testing global coordinates  
var faces = CurrentFrame.GetFacesWithNormals();       // Global normals (no "Global" prefix)
var edges = CurrentFrame.GetEdgesWithNames();         // Global edges (no "Global" prefix)
```

This API design ensures **no confusion** about coordinate spaces and provides a foundation for robust LEGO-style snapping systems.
