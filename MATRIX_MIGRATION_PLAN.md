# Automated Matrix Migration Plan

## Goal
Consolidate all matrix and vector math into BlazorThreeJS, removing Matrix3D and FoVector3D from FoundryBlazor.

## Current State Analysis

### Files to Remove from FoundryBlazor:
1. `Shapes3D/SpacialFrame/Matrix3D.cs` - Matrix3D class
2. `Shapes3D/SpacialFrame/FoBody3D.cs` - FoVector3D definition  
3. `Shapes3D/SpacialFrame/Matrix3DExtensions.cs` - Extensions for Matrix3D/FoVector3D
4. `Extensions/Vector3DMathExtensions.cs` - Additional FoVector3D extensions
5. `Extensions/VectorExtensions3D.cs` - Vector conversion extensions

### Files Using Matrix3D/FoVector3D:
1. `Shapes3D/SpacialFrame/SpacialFrame3D.cs` - Conversion methods
2. Various extension files with FoVector3D operations

## Automated Migration Steps

### Step 1: Enhance BlazorThreeJS Vector3 Class
Add missing functionality from FoVector3D to Vector3:

```csharp
// Add to BlazorThreeJS/Maths/Vector3.cs
public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
public static Vector3 operator *(Vector3 v, double scalar) => new(v.X * scalar, v.Y * scalar, v.Z * scalar);
public double Length() => Math.Sqrt(X * X + Y * Y + Z * Z);
public Vector3 Normalize() { var len = Length(); return len > 0 ? this * (1.0 / len) : new Vector3(); }
public static Vector3 Cross(Vector3 a, Vector3 b) => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
public static double Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
public static double Distance(Vector3 a, Vector3 b) => (a - b).Length();
public Vector3 Clamp(double min, double max) => new(Math.Clamp(X, min, max), Math.Clamp(Y, min, max), Math.Clamp(Z, min, max));
```

### Step 2: Create Vector3Extensions in BlazorThreeJS
Extract all advanced operations from FoundryBlazor extensions:

```csharp
// Create BlazorThreeJS/Maths/Vector3Extensions.cs
public static class Vector3Extensions
{
    public static Vector3 Project(this Vector3 vector, Vector3 onto) { /* implementation */ }
    public static Vector3 Reflect(this Vector3 vector, Vector3 normal) { /* implementation */ }
    public static bool ApproximatelyEqual(this Vector3 a, Vector3 b, double tolerance = 0.001) { /* implementation */ }
    // ... all other extensions from Vector3DMathExtensions
}
```

### Step 3: Enhance Matrix3 with Matrix3D Capabilities
Add missing methods to Matrix3:

```csharp
// Add to BlazorThreeJS/Maths/Matrix3.cs
public Vector3 GetTranslation() => new(Elements[12], Elements[13], Elements[14]);
public Vector3 GetScale() { /* implementation */ }
public Vector3 GetRotation() { /* implementation */ }
public void SetPosition(Vector3 position) { Elements[12] = position.X; Elements[13] = position.Y; Elements[14] = position.Z; }
public Vector3 TransformPoint(Vector3 point) { /* implementation */ }
public Matrix3 Copy(Matrix3 source) { Array.Copy(source.Elements, Elements, 16); return this; }
```

### Step 4: Create Matrix3Extensions in BlazorThreeJS
Port all Matrix3DExtensions functionality:

```csharp
// Create BlazorThreeJS/Maths/Matrix3Extensions.cs
public static class Matrix3Extensions
{
    public static Matrix3 MoveBy(this Matrix3 matrix, Vector3 delta) { /* implementation */ }
    public static Matrix3 MoveTo(this Matrix3 matrix, Vector3 position) { /* implementation */ }
    public static Matrix3 ScaleUniform(this Matrix3 matrix, double factor) { /* implementation */ }
    public static Matrix3 CreateChild(this Matrix3 parent) { /* implementation */ }
    // ... all other advanced operations from Matrix3DExtensions
}
```

### Step 5: Create Conversion Utilities
Add conversion methods for smooth transition:

```csharp
// Add to BlazorThreeJS/Maths/VectorConversions.cs
public static class VectorConversions
{
    public static Vector3 AsVector3(this Point3D point) => new(point.X, point.Y, point.Z);
    public static Point3D AsPoint3D(this Vector3 vector) => new(vector.X, vector.Y, vector.Z);
}
```

### Step 6: Update FoundryBlazor Files to Use BlazorThreeJS Types

#### Update SpacialFrame3D.cs:
```csharp
// Replace FoVector3D with Vector3
private Vector3 ToVector3(Point3D point) => new(point.X, point.Y, point.Z);
private Point3D ToPoint3D(Vector3 vector, string name = "") => new(vector.X, vector.Y, vector.Z, name);
```

#### Update any FoBody3D usage:
```csharp
// Replace FoVector3D fields with Vector3
protected Vector3 position = new();
protected Vector3 scale = new(1, 1, 1);
protected Vector3 rotation = new();
protected Vector3 pinPoint = new();
protected Matrix3? _matrix; // Use Matrix3 instead of Matrix3D
```

### Step 7: Update Import Statements
Replace all imports throughout FoundryBlazor:

```csharp
// Remove these imports:
// using FoundryBlazor.Shapes3D.SpacialFrame; (for Matrix3D/FoVector3D)

// Add these imports:
using BlazorThreeJS.Maths; // For Vector3 and Matrix3
```

### Step 8: Delete Obsolete Files
Remove the following files from FoundryBlazor:
1. `Shapes3D/SpacialFrame/Matrix3D.cs`
2. `Shapes3D/SpacialFrame/FoBody3D.cs` (FoVector3D definition)
3. `Shapes3D/SpacialFrame/Matrix3DExtensions.cs`
4. `Extensions/Vector3DMathExtensions.cs`
5. `Extensions/VectorExtensions3D.cs`

### Step 9: Update Project References
Ensure FoundryBlazor has proper reference to BlazorThreeJS for the math types.

## Execution Order

1. **Enhance BlazorThreeJS** (Steps 1-4) - Add all missing functionality
2. **Create conversion utilities** (Step 5) - Bridge for smooth transition  
3. **Update FoundryBlazor usage** (Step 6) - Replace type usage
4. **Fix imports** (Step 7) - Update using statements
5. **Remove obsolete files** (Step 8) - Clean up duplicates
6. **Test and verify** - Ensure all functionality preserved

## Benefits After Migration

- **Single source of truth** for all matrix/vector math
- **Unified API** across both projects  
- **Reduced code duplication** 
- **Simplified maintenance**
- **Better Three.js integration** throughout

## Risk Mitigation

- All functionality is preserved through enhanced classes
- Conversion utilities provide smooth transition path
- Step-by-step approach allows validation at each stage
- No breaking changes to external APIs

This migration consolidates 5 duplicate math files into the BlazorThreeJS project while preserving all existing functionality.
