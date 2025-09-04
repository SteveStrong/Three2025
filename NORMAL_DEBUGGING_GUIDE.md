# 🧭 Face Normal Debugging Guide

## Why Drawing Normals for Rotated Faces is Hard

### The Problem Chain
1. **SpacialFrame3D** calculates rotated face positions and normals
2. **Face3D.GetNormalVisualizationTransform()** calculates where to draw the normal arrow
3. **FoShape3D cylinder** represents the normal visually
4. **Transform order** must be correct: position, rotation, scale

### Step-by-Step Debugging Process

#### Step 1: Verify Base Case (No Rotation)
```csharp
// Create box at origin with no rotation
var box = new SnapBox("test", 2, 1, 1, "blue");
box.SetPosition(0, 0, 0);
box.SetRotation(0, 0, 0);

// Check face normals
foreach(var face in box.Faces.Values)
{
    Console.WriteLine($"{face.Name}: Center({face.Center.X:F2},{face.Center.Y:F2},{face.Center.Z:F2}) Normal({face.Normal.X:F2},{face.Normal.Y:F2},{face.Normal.Z:F2})");
}
```

**Expected Results:**
- Top face: Normal should be (0, 1, 0) pointing up
- Bottom face: Normal should be (0, -1, 0) pointing down
- Front face: Normal should be (0, 0, 1) pointing forward
- Back face: Normal should be (0, 0, -1) pointing backward
- Left face: Normal should be (-1, 0, 0) pointing left
- Right face: Normal should be (1, 0, 0) pointing right

#### Step 2: Test Simple Y Rotation (90 degrees)
```csharp
box.SetRotation(0, Math.PI/2, 0);  // 90° Y rotation
```

**Expected Results After 90° Y Rotation:**
- Top face: Normal should still be (0, 1, 0) - unchanged
- Bottom face: Normal should still be (0, -1, 0) - unchanged  
- Front face: Normal should be (1, 0, 0) - rotated to point right
- Back face: Normal should be (-1, 0, 0) - rotated to point left
- Left face: Normal should be (0, 0, 1) - rotated to point forward
- Right face: Normal should be (0, 0, -1) - rotated to point backward

#### Step 3: Debug Normal Visualization
```csharp
var (midpoint, normal, euler, length) = face.GetNormalVisualizationTransform(1.0);

// Verify midpoint calculation
// Should be: face.Center + (normal * length/2)
var expectedMidpoint = new Point3D(
    face.Center.X + normal.X * 0.5,
    face.Center.Y + normal.Y * 0.5, 
    face.Center.Z + normal.Z * 0.5
);
```

### Common Issues and Solutions

#### Issue 1: Normal Vectors Not Rotating
**Symptom**: Normals always point in original directions regardless of box rotation

**Root Cause**: SpacialFrame3D.GetFacesWithNormals() not applying rotation matrix to normals

**Fix**: Ensure rotation matrix is applied to normal vectors:
```csharp
// In SpacialFrame3D.GetFacesWithNormals()
var rotMatrix = Matrix3.NewMatrix().Identity().RotateEuler(Rx, Ry, Rz);
var rotatedNormal = rotMatrix.TransformPoint(originalNormal);
```

#### Issue 2: Normal Position Incorrect
**Symptom**: Normal arrows appear in wrong locations

**Root Cause**: Face centers not properly transformed to world coordinates

**Fix**: Apply full transform chain:
```csharp
// Transform face center to world coordinates  
var worldFaceCenter = spatialFrame.TransformPoint(localFaceCenter);
var normalStart = worldFaceCenter;
var normalEnd = worldFaceCenter + (rotatedNormal * length);
var normalMidpoint = new Point3D(
    (normalStart.X + normalEnd.X) / 2,
    (normalStart.Y + normalEnd.Y) / 2,
    (normalStart.Z + normalEnd.Z) / 2
);
```

#### Issue 3: Normal Arrow Orientation Wrong
**Symptom**: Normal arrows point in correct direction but are oriented incorrectly

**Root Cause**: Euler angle calculation from normal vector is incorrect

**Fix**: Use proper axis-angle to quaternion to Euler conversion:
```csharp
// Align cylinder's Y-axis with normal vector
var up = new Vector3(0, 1, 0);
var quaternion = Quaternion.FromToRotation(up, normal);
var euler = QuaternionToEuler(quaternion);
```

### Testing Methodology

1. **Start with our focused test page**: `/normal-test`
2. **Create simple box**: 2×1×1 dimensions
3. **No rotation first**: Verify all 6 normals point correctly
4. **Add 45° Y rotation**: Check if normals rotate correctly
5. **Toggle visualizations**: Compare face planes vs normal arrows
6. **Check status values**: Verify face centers and normal values in UI

This systematic approach will help us isolate exactly where the normal visualization breaks down!
