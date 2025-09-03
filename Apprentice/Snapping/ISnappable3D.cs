using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Shapes3D.SpacialFrame;

namespace Three2025.Apprentice.Snapping;

/// <summary>
/// Interface for 3D objects that can participate in snapping operations
/// </summary>
public interface ISnappable3D
{
    /// <summary>Named faces available for snapping (e.g., "Top", "Bottom", "Left")</summary>
    Dictionary<string, Face3D> Faces { get; }
    
    /// <summary>Named snap points for precise connection</summary>
    Dictionary<string, SnapPoint> SnapPoints { get; }
    
    /// <summary>Active constraints involving this component</summary>
    List<SnapConstraint> Constraints { get; }
    
    /// <summary>Get a specific face by name</summary>
    Face3D GetFace(string faceName);
    
    /// <summary>Get a specific snap point by name</summary>
    SnapPoint GetSnapPoint(string faceName);
    
    /// <summary>Check if this component can snap to another</summary>
    bool CanSnapTo(ISnappable3D other, string myFace, string otherFace);
    
    /// <summary>Calculate the transform needed to snap to another component</summary>
    Transform3 CalculateSnapTransform(string myFace, string otherFace, ISnappable3D other);
}
