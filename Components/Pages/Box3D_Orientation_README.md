# 3D Box Geometry and Orientation in Three2025

## Overview
This project uses a robust, testable approach to 3D box geometry, ensuring that all faces, edges, and normals are mathematically correct and visually consistent in BlazorThreeJS.

## Face Orientation
- **Face3D** encapsulates all math for face orientation and transformation.
- The orientation logic rotates the local Z axis `(0,0,1)` to align with the face's normal vector.
- This matches the default orientation of `CreateBox` in BlazorThreeJS, which expects the box's local Z axis to be perpendicular to the face.
- Face vertices are ordered so that the normal points outward, and the winding order is consistent for right-handed coordinate systems.

## Edge Orientation
- Edges are represented by `Edge3D` objects, each with a midpoint, length, and Euler rotation.
- Edge orientation math ensures that cylinders or lines representing edges are always correctly aligned between their endpoints.

## Normal Visualization
- Normals are visualized by rotating the local Z axis to the face normal using axis-angle → quaternion → Euler conversion.
- The normal visualization may require further adjustment for perfect visual alignment, but the math is encapsulated and ready for refinement.

## Encapsulation and Separation of Concerns
- All geometry and rendering math is contained in helper/model classes (`Face3D`, `Edge3D`, etc.), not in the UI code.
- The UI/test code simply delegates to these helpers, ensuring maintainability and testability.

## Best Practices
- When creating new geometry, always ensure the reference axis matches the rendering primitive's default orientation.
- Use right-handed winding for face vertices to ensure normals point outward.
- Encapsulate all math in model/helper classes for clarity and reuse.

---

*For further details, see the implementation in `Face3D.cs`, `SpacialBox3D.cs`, and related files.*
