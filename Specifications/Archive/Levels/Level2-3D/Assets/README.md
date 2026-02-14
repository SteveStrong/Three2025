# Assets for ClockDemo Component

## Overview
This folder contains reference assets used by the ClockDemo component specifications. These are the 3D models referenced in the generated code.

## Required Models

### TRex.glb
- **Usage**: Demonstrates animated 3D model loading
- **Size**: Should be reasonably sized for web (< 5MB)
- **Format**: GLTF binary (.glb)
- **Position**: Placed at random positions in scene
- **Scale**: Scaled to 30% (0.3, 0.3, 0.3)

### submarine.glb  
- **Usage**: Demonstrates static 3D model loading
- **Size**: Should be reasonably sized for web (< 2MB)
- **Format**: GLTF binary (.glb)  
- **Position**: Placed at Y=2 with random X,Z
- **Scale**: Scaled to 50% (0.5, 0.5, 0.5)

### fiveMeterAxis.glb
- **Usage**: Coordinate system reference model
- **Size**: Small utility model (< 500KB)
- **Format**: GLTF binary (.glb)
- **Position**: Placed at origin (0, 0, 0)
- **Scale**: Default scale (1, 1, 1)

## Implementation Options

### Option 1: Include Actual Models (Recommended)
Copy actual .glb files to this folder for immediate use:
```
Assets/models/
├── TRex.glb
├── submarine.glb  
└── fiveMeterAxis.glb
```

### Option 2: Placeholder Models
Create simple placeholder .glb files or use built-in primitives:
```csharp
// Fallback to primitive shapes if models not found
var shape = new FoShape3D($"Placeholder_{name}")
{
    Color = "Blue",
    // Primitive geometry instead of model
};
```

### Option 3: Optional Loading with Graceful Fallbacks
```csharp
protected void AddTRex()
{
    if (_clockStage == null) return;

    var modelPath = GetAssetPath("models/TRex.glb");
    
    // Try to load model, fallback to primitive if not found
    if (File.Exists(modelPath))
    {
        var tRex = new FoModel3D($"TRex_{++_shapeCounter:D3}")
        {
            Url = modelPath,
            // ... transform etc
        };
        _clockStage.AddShape(tRex);
    }
    else
    {
        // Fallback to primitive shape
        var placeholder = new FoShape3D($"TRexPlaceholder_{++_shapeCounter:D3}")
        {
            Color = "Green",
            Transform = new Transform3("TRexTransform") 
            {
                // Same position as model would be
            }
        };
        _clockStage.AddShape(placeholder);
        "Using placeholder for TRex model (model file not found)".WriteWarning();
    }
}
```

## Asset Sources

### Free 3D Model Resources
- **Sketchfab**: https://sketchfab.com (CC licensed models)
- **Poly Haven**: https://polyhaven.com/models (CC0 models)
- **Quaternius**: http://quaternius.com (free low-poly models)
- **Kenney Assets**: https://kenney.nl/assets (game-ready models)

### Simple Placeholder Creation
For testing, you can create simple models in Blender:
1. Create basic shapes (cube, cylinder, sphere)
2. Export as GLTF binary (.glb)
3. Keep file sizes small (< 1MB each)

## Usage in Generated Code

The generated code will reference these assets via:
```csharp
private string GetAssetPath(string relativePath)
{
    var baseUrl = Navigation?.BaseUri ?? "";
    return $"{baseUrl}storage/StaticFiles/{relativePath}";
}
```

**Important**: Update the target project's `wwwroot` folder structure:
```
wwwroot/
└── storage/
    └── StaticFiles/
        └── models/
            ├── TRex.glb
            ├── submarine.glb
            └── fiveMeterAxis.glb
```

## Deployment Instructions

1. **Copy Assets**: Copy .glb files from `Specifications/Assets/models/` to target project's `wwwroot/storage/StaticFiles/models/`
2. **Verify Paths**: Ensure file paths match the `GetAssetPath()` method
3. **Test Loading**: Check browser console for 404 errors on model files
4. **Optimize**: Compress models if needed for web performance

## Alternative: Asset-Free Version

If you prefer not to include 3D models, modify the generated code to use only primitive shapes:
```csharp
// Replace model loading with primitive creation
var tRex = new FoShape3D($"TRex_{++_shapeCounter:D3}")
{
    Color = "Green",
    Transform = new Transform3("TRexTransform")
    {
        Position = new Vector3(x, 0, z),
        Scale = new Vector3(2, 3, 1) // Dinosaur-like proportions
    }
};
```

This approach eliminates external dependencies while maintaining the interactive functionality.