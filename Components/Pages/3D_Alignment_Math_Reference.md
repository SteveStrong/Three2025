# 3D Alignment Math Reference: Aligning Objects to Vectors in 3D Space

## Overview
When visualizing 3D objects (like cylinders for edges or rectangles for faces), you often need to align them along a specific direction in space. This is common in 3D graphics, CAD, and game development. The math for this is based on vector operations: the cross product, dot product, and axis-angle rotation.

## Key Concepts

### 1. Direction Vector
- The direction vector is the difference between two points: `direction = End - Start`.
- Normalize this vector to get a unit direction: `dir = direction.Normalize()`.

### 2. Default Axis
- Most 3D primitives (like cylinders) are created aligned with a default axis, usually the Y axis `(0, 1, 0)`.
- To align the primitive with an arbitrary direction, you must rotate it so its Y axis matches the desired direction.

### 3. Axis and Angle of Rotation
- **Axis:** The axis to rotate around is given by the cross product: `axis = up.Cross(dir)`.
- **Angle:** The angle to rotate is given by the arccosine of the dot product: `angle = Math.Acos(up.Dot(dir))`.
- If the axis length is near zero, the vectors are parallel or anti-parallel (no rotation or 180° rotation).

### 4. Applying the Rotation
- In 3D engines, you typically convert axis/angle to a quaternion or Euler angles.
- In this project, we approximate the rotation by assigning the angle to the Euler axis with the largest component in the cross product.
- The object is then positioned at the midpoint between the two points.

## Example: Aligning a Cylinder to an Edge
```csharp
var start = new Vector3(x1, y1, z1);
var end = new Vector3(x2, y2, z2);
var direction = end.CreatePlus(-start.X, -start.Y, -start.Z).Normalize();
var up = new Vector3(0, 1, 0);
var axis = up.Cross(direction);
var axisLength = axis.Length();
double angle = 0;
if (axisLength > 1e-6)
{
    axis = axis.Normalize();
    angle = Math.Acos(Math.Max(-1.0, Math.Min(1.0, up.Dot(direction))));
}
else
{
    angle = up.Dot(direction) > 0 ? 0 : Math.PI;
    axis = new Vector3(1, 0, 0); // Arbitrary axis
}
// Assign angle to the Euler axis with the largest component in 'axis'
```

## Application to Faces and Normals
- The same math can be used to align a face (e.g., a thin box) so its normal matches a desired direction.
- Compute the normal vector for the face, then use the above axis/angle method to rotate the face primitive so its local normal aligns with the computed normal.

## Summary Table
| Operation         | Math Used                |
|-------------------|-------------------------|
| Direction Vector  | `End - Start`           |
| Normalize        | `v / |v|`               |
| Cross Product    | `a x b`                 |
| Dot Product      | `a · b`                 |
| Angle            | `acos(a · b)`           |
| Axis             | `a x b`                 |

## References
- [3D Math Primer for Graphics and Game Development](https://gamemath.com/)
- [Wikipedia: Axis–angle representation](https://en.wikipedia.org/wiki/Axis–angle_representation)
- [Three.js: Object3D.lookAt](https://threejs.org/docs/#api/en/core/Object3D.lookAt)

---
This document can be used as a reference for aligning any 3D object (edges, faces, normals) to a direction in space using pure C# math.
