# Matrix Rotation and Angle Conventions in Three2025

## Overview
This document explains how matrix rotations and Euler angles are handled in the Three2025 solution, with a focus on conventions, units, and rendering integration. It is intended to prevent confusion and bugs caused by mixing up degrees and radians.

## Angle Units and Conventions
- **Euler angles are always stored internally in radians.**
- Use `Euler.FromDegrees(x, y, z)` to construct Euler angles from degrees.
- Use `Euler.FromRadians(x, y, z)` to construct Euler angles from radians.
- Always be explicit about units when setting or reading Euler angles.
- All diagnostic tests use degrees for clarity, but the system converts to radians internally.

## Matrix3RotationSimpleTest
- The test class `Matrix3RotationSimpleTest` verifies that `Transform3` rotation produces the expected matrix for simple 90-degree rotations about X, Y, and Z axes.
- These tests ensure that the rendering system receives correct transformation matrices for basic rotations.

### Example Test (Degrees)
```csharp
var t = new Transform3("TestX");
t.Rotation = Euler.FromDegrees(90, 0, 0); // degrees
var m = t.ToMatrix3();
```

### Expected Matrix for 90° Rotation About X Axis (Right-Handed System)
```
1, 0, 0, 0
0, 0, 1, 0
0, -1, 0, 0
0, 0, 0, 1
```

## Rendering Integration
- The transformation matrices produced by `Transform3` are used directly in rendering 3D objects.
- Correct angle conventions ensure that objects are rotated as expected in the scene.
- Always verify matrix output with simple cases before using in complex rendering scenarios.

## Best Practices
- **Never assume the unit of an angle.** Always use explicit factory methods (`FromDegrees`, `FromRadians`).
- When debugging rendering issues, check the matrix output for simple rotations first.
- Keep this documentation up to date as conventions evolve.

---
_Last updated: September 7, 2025_
