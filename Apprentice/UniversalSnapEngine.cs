using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Shapes3D.SpacialFrame;

namespace Three2025.Apprentice;

/// <summary>
/// Universal snapping engine that works with any FoShape3D objects
/// Uses SpacialFrame3D as mathematical workspace for constraint calculations
/// </summary>
public static class UniversalSnapEngine
{
    /// <summary>
    /// Snap any two FoShape3D objects together using face-to-face alignment
    /// </summary>
    /// <param name="objectA">Object to be moved (any FoShape3D)</param>
    /// <param name="faceA">Face name on objectA ("Top", "Bottom", "Front", "Back", "Left", "Right")</param>
    /// <param name="objectB">Target object (any FoShape3D)</param>
    /// <param name="faceB">Face name on objectB</param>
    /// <returns>Result indicating success or failure</returns>
    public static SnapResult SnapObjects(FoShape3D objectA, string faceA, 
                                        FoShape3D objectB, string faceB)
    {
        try
        {
            // 1. WRAP: Create spatial frame workspace around original objects
            var frameA = CreateSpatialFrameWrapper(objectA);
            var frameB = CreateSpatialFrameWrapper(objectB);
            
            // 2. CALCULATE: Get face information from spatial frames
            var faceInfoA = GetFaceByName(frameA, faceA);
            var faceInfoB = GetFaceByName(frameB, faceB);
            
            if (faceInfoA == null || faceInfoB == null)
            {
                return SnapResult.CreateFailed($"Could not find faces: {faceA} on ObjectA or {faceB} on ObjectB");
            }
            
            // 3. CONSTRAINT MATH: Calculate required alignment transform
            var newTransform = CalculateSnapTransform(faceInfoA, faceInfoB);
            
            // 4. PROJECT BACK: Apply calculated transform to original FoShape3D
            objectA.Transform.Position = newTransform.Position;
            objectA.Transform.Rotation = newTransform.Rotation;
            
            return SnapResult.CreateSuccess(newTransform, 1);
        }
        catch (Exception ex)
        {
            return SnapResult.CreateFailed($"Snap operation failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Create a SpacialFrame3D wrapper around any FoShape3D object
    /// This provides the mathematical workspace for constraint calculations
    /// </summary>
    private static SpacialFrame3D CreateSpatialFrameWrapper(FoShape3D shape)
    {
        // Extract bounding dimensions from the shape
        var bounds = GetBoundingBox(shape);
        
        // Create spatial frame specification
        var spec = new FoSpec3D 
        { 
            W = bounds.Width, 
            H = bounds.Height, 
            D = bounds.Depth,
            X = shape.Transform?.Position?.X ?? 0,
            Y = shape.Transform?.Position?.Y ?? 0, 
            Z = shape.Transform?.Position?.Z ?? 0,
            Rx = shape.Transform?.Rotation?.X ?? 0,
            Ry = shape.Transform?.Rotation?.Y ?? 0,
            Rz = shape.Transform?.Rotation?.Z ?? 0
        };
        
        return new SpacialFrame3D(spec);
    }
    
    /// <summary>
    /// Get bounding box dimensions from any FoShape3D
    /// This creates the standardized coordinate space for snapping
    /// </summary>
    private static (double Width, double Height, double Depth) GetBoundingBox(FoShape3D shape)
    {
        // Extract dimensions directly from FoShape3D properties
        double width = shape.Width > 0 ? shape.Width : 2.0;
        double height = shape.Height > 0 ? shape.Height : 1.0;
        double depth = shape.Depth > 0 ? shape.Depth : 1.0;
        
        return (width, height, depth);
    }
    
    /// <summary>
    /// Get a specific face by name from a spatial frame
    /// </summary>
    private static Face3D GetFaceByName(SpacialFrame3D frame, string faceName)
    {
        var faces = frame.GetFacesWithNormals();
        return faces.FirstOrDefault(f => f.Name.Equals(faceName, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Calculate the transform needed to align two faces
    /// This is the core mathematical constraint solver
    /// </summary>
    private static Transform3 CalculateSnapTransform(Face3D faceA, Face3D faceB)
    {
        // Position: Face A center aligns with Face B center
        var targetPosition = new Vector3(faceB.Center.X, faceB.Center.Y, faceB.Center.Z);
        
        // Offset to prevent interpenetration (small gap to avoid z-fighting)
        var offset = faceA.Normal * 0.001;
        var finalPosition = targetPosition + offset;
        
        // Rotation: Face A normal aligns opposite to Face B normal (for face-to-face contact)
        var sourceNormal = faceA.Normal;
        var targetNormal = faceB.Normal * -1; // Opposite direction for contact
        
        // Calculate precise quaternion rotation for face alignment
        var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
        
        return new Transform3
        {
            Position = finalPosition,
            QuaternionRotation = rotationQuaternion
        };
    }
    
    /// <summary>
    /// Get face information from any FoShape3D by creating temporary spatial frame
    /// Useful for visualization and debugging
    /// </summary>
    public static Face3D GetFaceInfo(FoShape3D shape, string faceName)
    {
        var frame = CreateSpatialFrameWrapper(shape);
        return GetFaceByName(frame, faceName);
    }
    
    /// <summary>
    /// Get all available faces from any FoShape3D
    /// Useful for UI face selection
    /// </summary>
    public static List<Face3D> GetAllFaces(FoShape3D shape)
    {
        var frame = CreateSpatialFrameWrapper(shape);
        return frame.GetFacesWithNormals();
    }
    
    /// <summary>
    /// Quick utility methods for common snapping operations
    /// </summary>
    public static class QuickSnap
    {
        public static SnapResult StackOnTop(FoShape3D topObject, FoShape3D bottomObject)
        {
            return SnapObjects(topObject, "Bottom", bottomObject, "Top");
        }
        
        public static SnapResult PlaceSideBySide(FoShape3D rightObject, FoShape3D leftObject)
        {
            return SnapObjects(rightObject, "Left", leftObject, "Right");
        }
        
        public static SnapResult AttachToFront(FoShape3D frontObject, FoShape3D backObject)
        {
            return SnapObjects(frontObject, "Back", backObject, "Front");
        }
        
        public static SnapResult AttachToBack(FoShape3D backObject, FoShape3D frontObject)
        {
            return SnapObjects(backObject, "Front", frontObject, "Back");
        }
    }
}

/// <summary>
/// Result of a snap operation
/// </summary>
public class SnapResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Transform3 FinalTransform { get; set; }
    public int ConstraintsApplied { get; set; }
    
    public static SnapResult CreateSuccess(Transform3 transform, int constraintsApplied)
    {
        return new SnapResult
        {
            Success = true,
            FinalTransform = transform,
            ConstraintsApplied = constraintsApplied
        };
    }
    
    public static SnapResult CreateFailed(string error)
    {
        return new SnapResult
        {
            Success = false,
            ErrorMessage = error
        };
    }
}
