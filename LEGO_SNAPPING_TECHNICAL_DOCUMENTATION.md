# LEGO-Style Snapping System Technical Documentation

## Overview

This document explains the technical implementation of the LEGO-style snapping system built for the Three2025 application. The system enables precise face-to-face alignment of 3D components through a constraint-based approach that mimics real-world LEGO block assembly.

## Core Philosophy

### Face-to-Face Snapping Strategy

The snapping system is built around the concept of **face-to-face alignment**, where components connect by aligning their named faces (Top, Bottom, Front, Back, Left, Right) in precise opposition. This approach provides:

1. **Predictable Assembly**: Users think in terms of "attach the bottom of A to the top of B"
2. **Local Naming Convention**: Face names remain consistent regardless of component orientation
3. **Precise Alignment**: Mathematical calculation ensures perfect face contact
4. **Constraint Persistence**: Relationships are maintained as constraint objects

## System Architecture

### Key Components

#### 1. ISnappable3D Interface
The foundation interface that any snappable component must implement:

```csharp
public interface ISnappable3D
{
    Dictionary<string, Face3D> Faces { get; }           // Named faces (Top, Bottom, etc.)
    Dictionary<string, SnapPoint> SnapPoints { get; }   // Precise snap locations
    List<SnapConstraint> Constraints { get; }           // Active relationships
    
    Face3D GetFace(string faceName);                    // Retrieve specific face
    bool CanSnapTo(ISnappable3D other, string myFace, string otherFace);
    Transform3 CalculateSnapTransform(string myFace, string otherFace, ISnappable3D other);
}
```

#### 2. Face3D Structure
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

#### 3. SnapBox Implementation
Concrete implementation wrapping SpacialFrame3D geometry:

```csharp
public class SnapBox : ISnappable3D
{
    private SpacialFrame3D spatial;           // Underlying geometry
    private FoShape3D visualComponent;       // Visual representation
    
    // Automatically generates faces from SpacialFrame3D geometry
    private void CreateFacesFromGeometry()
    {
        var facesWithNormals = spatial.GetFacesWithNormals();
        foreach (var face in facesWithNormals)
        {
            Faces[face.Name] = face;
        }
    }
}
```

## Constraint System

### Constraint Creation Process

#### Step 1: Face Selection
```csharp
// User selects faces through UI
string selectedFaceA = "Bottom";  // Component A's face
string selectedFaceB = "Top";     // Component B's face
```

#### Step 2: Constraint Object Creation
```csharp
public void CreateConstraint()
{
    var faceA = ComponentA.GetFace(SelectedFaceA);
    var faceB = ComponentB.GetFace(SelectedFaceB);
    
    // Create constraint linking the two faces
    CurrentConstraint = new FaceToFaceConstraint(ComponentA, faceA, ComponentB, faceB, 1.0);
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
