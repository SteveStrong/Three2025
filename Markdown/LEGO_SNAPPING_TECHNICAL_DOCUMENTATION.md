# LEGO-Style Snapping System Technical Documentation

## Overview

This document explains the technical implementation of the LEGO-style snapping system built for the Three2025 application. The system enables precise face-to-face alignment of any 3D geometry through a constraint-based approach that uses `SpacialFrame3D` as a mathematical workspace for constraint calculations.

## Core Philosophy

### Universal Geometry Snapping Strategy

The snapping system works directly with existing `FoShape3D` objects through a elegant 4-step process:

1. **Accept Any FoShape3D**: Work with spheres, cylinders, complex meshes, imported models - any 3D geometry
2. **Wrap in Spatial Frame**: Create temporary `SpacialFrame3D` around object's bounding dimensions
3. **Calculate Constraints**: Use spatial frame's standardized face system for mathematical alignment
4. **Project Back**: Apply calculated transforms to the original `FoShape3D` object

This approach provides:

1. **Universal Geometry Support**: Any 3D content becomes instantly snappable
2. **Predictable Assembly**: Users think in terms of "attach the bottom of A to the top of B"
3. **Local Naming Convention**: Face names (Top, Bottom, Front, Back, Left, Right) remain consistent
4. **Precise Alignment**: Mathematical calculation ensures perfect face contact
5. **Non-Invasive Integration**: Existing objects require no modification to become snappable

## System Architecture

### Key Components

#### 1. Universal Snapping Engine
The core function that works with any `FoShape3D` objects:

```csharp
public static class SnapEngine
{
    public static SnapResult SnapObjects(FoShape3D objectA, string faceA, 
                                        FoShape3D objectB, string faceB)
    {
        // 1. WRAP: Create spatial frame workspace
        var frameA = CreateSpatialFrameWrapper(objectA);
        var frameB = CreateSpatialFrameWrapper(objectB);
        
        // 2. CALCULATE: Constraint math on spatial frames
        var faceInfoA = frameA.GetFacesWithNormals().First(f => f.Name == faceA);
        var faceInfoB = frameB.GetFacesWithNormals().First(f => f.Name == faceB);
        var newTransform = CalculateSnapTransform(faceInfoA, faceInfoB);
        
        // 3. PROJECT BACK: Apply to original FoShape3D
        objectA.Transform.Position = newTransform.Position;
        objectA.Transform.Rotation = newTransform.Rotation;
        
        return SnapResult.Success();
    }
    
    private static SpacialFrame3D CreateSpatialFrameWrapper(FoShape3D shape)
    {
        var bounds = shape.GetBoundingBox();
        var spec = new FoSpec3D { 
            W = bounds.Width, H = bounds.Height, D = bounds.Depth,
            X = shape.Transform.Position.X,
            Y = shape.Transform.Position.Y, 
            Z = shape.Transform.Position.Z
        };
        return new SpacialFrame3D(spec);
    }
}
```

#### 2. Face3D Structure (from SpacialFrame3D)
Represents a geometric face with all necessary alignment data:

```csharp
public class Face3D
{
    public string Name { get; set; }           // "Top", "Bottom", "Front", etc.
    public List<Point3D> Vertices { get; set; } // Face corner vertices
    public Point3D Center { get; set; }        // World position of face center
    public Vector3 Normal { get; set; }        // World-space normal vector
    public double Width { get; set; }          // Face dimensions
    public double Height { get; set; }
}
```

#### 3. Spatial Frame Wrapper Creation
Creates a mathematical workspace around any `FoShape3D` geometry:

```csharp
private static SpacialFrame3D CreateSpatialFrameWrapper(FoShape3D shape)
{
    // Extract bounding dimensions from any geometry type
    var bounds = shape.GetBoundingBox();
    
    // Create spatial frame specification
    var spec = new FoSpec3D 
    { 
        W = bounds.Width,  H = bounds.Height,  D = bounds.Depth,
        X = shape.Transform.Position.X,
        Y = shape.Transform.Position.Y, 
        Z = shape.Transform.Position.Z,
        Rx = shape.Transform.Rotation.X,
        Ry = shape.Transform.Rotation.Y,
        Rz = shape.Transform.Rotation.Z
    };
    
    // Return spatial frame with standardized face definitions
    return new SpacialFrame3D(spec);
}
```

## Constraint System

### Simplified Constraint Process

#### Step 1: Object and Face Selection
```csharp
// Works with any FoShape3D objects
FoShape3D shapeA = CreateSphere("sphere", 1.0, "blue");     // Sphere geometry
FoShape3D shapeB = CreateBox("box", 2, 2, 2, "red");        // Box geometry

// Select faces using standardized names
string selectedFaceA = "Bottom";  // Sphere's bottom face (from spatial frame)
string selectedFaceB = "Top";     // Box's top face (from spatial frame)
```

#### Step 2: Direct Constraint Execution
```csharp
public void SnapObjects()
{
    // Single function call - no special interfaces or wrapper classes needed
    var result = SnapEngine.SnapObjects(shapeA, selectedFaceA, shapeB, selectedFaceB);
    
    if (result.Success)
    {
        // shapeA is now positioned to align its bottom face with shapeB's top face
        Console.WriteLine("Objects snapped successfully!");
    }
}
```

#### Step 3: Constraint Validation
```csharp
public override ConstraintValidation Validate()
{
    if (ComponentA == null || ComponentB == null)
        return ConstraintValidation.Invalid("Missing component(s)");
        
    if (!ComponentA.Faces.ContainsKey(FaceA))
        return ConstraintValidation.Invalid($"ComponentA missing face '{FaceA}'");
        
    return ConstraintValidation.Valid();
}
```

### Transform Calculation Algorithm

The core of the snapping system is the mathematical calculation that determines exactly how to move Component A to align with Component B:

```csharp
private Transform3 CalculateSnapTransform(Face3D faceA, Face3D faceB)
{
    // 1. POSITION CALCULATION
    // Face A center must align with Face B center
    var targetPosition = new Vector3(faceB.Center.X, faceB.Center.Y, faceB.Center.Z);
    
    // 2. OFFSET APPLICATION
    // Small offset prevents z-fighting (visual artifacts)
    var offset = faceA.Normal * 0.001;
    var finalPosition = targetPosition + offset;
    
    // 3. ROTATION CALCULATION (QUATERNION MAGIC)
    // Face A normal must point opposite to Face B normal for contact
    var sourceNormal = faceA.Normal;
    var targetNormal = faceB.Normal * -1;  // Opposite direction for face-to-face contact
    
    // Calculate precise quaternion rotation for perfect alignment
    var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
    
    return new Transform3
    {
        Position = finalPosition,
        QuaternionRotation = rotationQuaternion  // Perfect face alignment!
    };
}
```

### Constraint Execution

#### Execution Flow
```csharp
public static SnapResult ExecuteConstraint(SnapConstraint constraint)
{
    // 1. Validate constraint before execution
    var validation = constraint.Validate();
    if (!validation.IsValid)
        return SnapResult.CreateFailed($"Constraint validation failed: {validation.Reason}");
    
    // 2. Execute the constraint
    var result = constraint.Execute();
    
    // 3. Register with both components if successful
    if (result.Success)
    {
        RegisterConstraintWithComponents(constraint);
    }
    
    return result;
}
```

#### Constraint Registration
```csharp
private static void RegisterConstraintWithComponents(SnapConstraint constraint)
{
    // Bidirectional relationship - both components know about the constraint
    if (!constraint.ComponentA.Constraints.Contains(constraint))
        constraint.ComponentA.Constraints.Add(constraint);
        
    if (!constraint.ComponentB.Constraints.Contains(constraint))
        constraint.ComponentB.Constraints.Add(constraint);
}
```

## Visual Feedback System

### Real-time Visualization

The UI provides comprehensive visual feedback to help users understand the snapping process:

#### Face Visualization
```csharp
public void ShowFaces()
{
    foreach (var face in ComponentA.Faces.Values)
    {
        VisualizationService.ShowWireframeFaces(arena, new List<Face3D> { face });
    }
}
```

#### Normal Vector Display
```csharp
public void ShowNormals()
{
    var faces = ComponentA.Faces.Values.ToList();
    VisualizationService.ShowLabeledNormals(arena, faces);
    // Shows red cylinders indicating face direction
}
```

#### Snap Point Indicators
```csharp
public void ShowSnapPoints()
{
    foreach (var snapPoint in ComponentA.SnapPoints.Values)
    {
        CreateMarkerSphere($"SnapPointA_{snapPoint.Name}", 
            worldPosition, "#FF9800", 0.05);
    }
}
```

## Mathematical Foundations

### Quaternion-Based Rotation

The system uses quaternions for rotation calculations because they provide:

1. **Gimbal Lock Avoidance**: No singularities in rotation space
2. **Smooth Interpolation**: Natural rotation paths
3. **Composition**: Easy to combine multiple rotations
4. **Precision**: Accurate face-to-face alignment

#### From-To Rotation Calculation
```csharp
// Calculate rotation needed to align sourceNormal with targetNormal
var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
```

### Face Normal Computation

Face normals are calculated from the underlying SpacialFrame3D geometry:

```csharp
public override List<Face3D> GetFacesWithNormals()
{
    // Transform normal vectors according to the frame's rotation
    var rotMatrix = Matrix3.NewMatrix().Identity().RotateEuler(Rx, Ry, Rz);
    var frontNormal = rotMatrix.TransformPoint(new Vector3(0, 0, 1));
    var backNormal = rotMatrix.TransformPoint(new Vector3(0, 0, -1));
    // ... etc for all faces
    
    return new List<Face3D>
    {
        new Face3D("Front", frontFace, frontNormal),
        new Face3D("Back", backFace, backNormal),
        // ... etc
    };
}
```

## Usage Patterns

### Quick Assembly Operations

The system provides preset operations for common assembly patterns:

#### Stacking
```csharp
public void StackAOnTopOfB()
{
    SelectedFaceA = "Bottom";  // A's bottom face
    SelectedFaceB = "Top";     // B's top face
    CreateConstraint();
    ExecuteConstraint();
}
```

#### Side-by-Side Placement
```csharp
public void PlaceASideBySideWithB()
{
    SelectedFaceA = "Left";    // A's left face
    SelectedFaceB = "Right";   // B's right face
    CreateConstraint();
    ExecuteConstraint();
}
```

### Multi-Constraint Assembly

For complex assemblies, multiple constraints can be created and executed in priority order:

```csharp
public static List<SnapResult> ExecuteConstraints(List<SnapConstraint> constraints)
{
    var sortedConstraints = constraints
        .Where(c => c.IsActive)
        .OrderBy(c => c.Priority)  // Lower values execute first
        .ToList();
    
    foreach (var constraint in sortedConstraints)
    {
        var result = ExecuteConstraint(constraint);
        if (!result.Success) break;  // Stop on first failure
    }
}
```

## Error Handling and Validation

### Constraint Validation
- Component existence verification
- Face availability checking
- Geometric compatibility assessment
- Collision detection (future enhancement)

### Execution Results
```csharp
public class SnapResult
{
    public bool Success { get; private set; }
    public string ErrorMessage { get; private set; }
    public Transform3 FinalTransform { get; private set; }
    public int ConstraintsApplied { get; private set; }
}
```

## Future Enhancements

### Planned Features

1. **Collision Detection**: Prevent interpenetration during snapping
2. **Grid-Based Snapping**: LEGO-style stud/hole alignment
3. **Hierarchical Assemblies**: Sub-component constraint propagation
4. **Constraint Solving**: Simultaneous multi-constraint resolution
5. **Animation**: Smooth transition during snapping operations

### Extension Points

The system is designed for extensibility:

- **New Snap Types**: EdgeToEdge, CornerToCorner, etc.
- **Custom Geometries**: Beyond simple boxes
- **Constraint Types**: Flexible, Sliding, Rotating constraints
- **Layout Patterns**: Linear arrangements, grids, etc.

## Conclusion

The LEGO-style snapping system provides a robust foundation for 3D component assembly through:

- **Precise mathematical alignment** using quaternion rotations
- **Constraint-based relationships** that maintain assembly integrity
- **Intuitive user interface** with real-time visual feedback
- **Extensible architecture** supporting future enhancements

The system successfully demonstrates face-to-face snapping with perfect geometric alignment, providing a solid foundation for more complex assembly operations and constraint-based 3D modeling applications.
