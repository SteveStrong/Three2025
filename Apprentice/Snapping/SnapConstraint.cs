using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Shapes3D.SpacialFrame;

namespace Three2025.Apprentice.Snapping;

/// <summary>
/// Represents a constraint that defines how two components should be positioned relative to each other
/// </summary>
public abstract class SnapConstraint
{
    /// <summary>First component in the constraint relationship</summary>
    public ISnappable3D ComponentA { get; set; } = null!;
    
    /// <summary>Second component in the constraint relationship</summary>
    public ISnappable3D ComponentB { get; set; } = null!;
    
    /// <summary>Face name on ComponentA (e.g., "Top")</summary>
    public string FaceA { get; set; } = string.Empty;
    
    /// <summary>Face name on ComponentB (e.g., "Bottom")</summary>
    public string FaceB { get; set; } = string.Empty;
    
    /// <summary>Type of constraint behavior</summary>
    public ConstraintType Type { get; set; }
    
    /// <summary>Optional offset from perfect alignment</summary>
    public Vector3 Offset { get; set; }
    
    /// <summary>Whether this constraint is currently active</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Constraint execution priority (lower values execute first)</summary>
    public double Priority { get; set; } = 1.0;
    
    /// <summary>Execute this constraint and return the result</summary>
    public abstract SnapResult Execute();
    
    /// <summary>Validate that this constraint can be executed</summary>
    public abstract ConstraintValidation Validate();
}

/// <summary>
/// Face-to-face snapping constraint (most common type)
/// </summary>
public class FaceToFaceConstraint : SnapConstraint
{
    public FaceToFaceConstraint(ISnappable3D componentA, Face3D faceA, ISnappable3D componentB, Face3D faceB, double priority = 1.0)
    {
        ComponentA = componentA;
        ComponentB = componentB;
        FaceA = faceA?.Name ?? string.Empty;
        FaceB = faceB?.Name ?? string.Empty;
        Priority = priority;
        Type = ConstraintType.Fixed;
    }
    
    public override SnapResult Execute()
    {
        try
        {
            // 1. Get face information
            var faceA = ComponentA.GetFace(FaceA);
            var faceB = ComponentB.GetFace(FaceB);
            
            if (faceA == null || faceB == null)
            {
                return SnapResult.CreateFailed("Could not find specified faces");
            }
            
            // 2. Calculate required transform
            var transform = CalculateSnapTransform(faceA, faceB);
            
            return SnapResult.CreateSuccess(transform, 1);
        }
        catch (Exception ex)
        {
            return SnapResult.CreateFailed($"Constraint execution failed: {ex.Message}");
        }
    }
    
    public override ConstraintValidation Validate()
    {
        // Check that both components exist and have the specified faces
        if (ComponentA == null || ComponentB == null)
            return ConstraintValidation.Invalid("Missing component(s)");
            
        if (!ComponentA.Faces.ContainsKey(FaceA))
            return ConstraintValidation.Invalid($"ComponentA missing face '{FaceA}'");
            
        if (!ComponentB.Faces.ContainsKey(FaceB))
            return ConstraintValidation.Invalid($"ComponentB missing face '{FaceB}'");
            
        return ConstraintValidation.Valid();
    }
    
    private Transform3 CalculateSnapTransform(Face3D faceA, Face3D faceB)
    {
        // Position: Face A center aligns with Face B center
        var targetPosition = new Vector3(faceB.Center.X, faceB.Center.Y, faceB.Center.Z);
        
        // Offset by face A normal to avoid interpenetration
        var offset = faceA.Normal * 0.001; // Small offset to prevent z-fighting
        var finalPosition = targetPosition + offset + Offset;
        
        // 🎯 QUATERNION MAGIC: Direct face-to-face alignment!
        // Face A normal needs to align opposite to Face B normal (for contact)
        var sourceNormal = faceA.Normal;
        var targetNormal = faceB.Normal * -1; // Opposite for face-to-face contact
        
        // Calculate precise quaternion rotation for face alignment
        var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
        
        return new Transform3
        {
            Position = finalPosition,
            QuaternionRotation = rotationQuaternion  // 🚀 Perfect face alignment!
        };
    }
}

/// <summary>
/// Types of constraint behavior
/// </summary>
public enum ConstraintType
{
    /// <summary>Rigid connection - no relative movement</summary>
    Fixed,
    
    /// <summary>Can slide along constraint axis</summary>
    Sliding,
    
    /// <summary>Can rotate around constraint axis</summary>
    Rotating,
    
    /// <summary>Limited movement within tolerance</summary>
    Flexible
}
