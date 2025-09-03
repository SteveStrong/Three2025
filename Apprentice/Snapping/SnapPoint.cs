using BlazorThreeJS.Maths;

namespace Three2025.Apprentice.Snapping;

/// <summary>
/// Represents a point where components can snap together
/// </summary>
public class SnapPoint
{
    /// <summary>Human-readable name (e.g., "TopCenter", "BottomLeft")</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Position relative to component's local origin</summary>
    public Vector3 LocalPosition { get; set; }
    
    /// <summary>Position in world coordinates (computed from LocalPosition + Transform)</summary>
    public Vector3 WorldPosition { get; set; }
    
    /// <summary>Direction vector indicating snapping orientation</summary>
    public Vector3 Normal { get; set; }
    
    /// <summary>Type of snap connection</summary>
    public SnapType Type { get; set; }
    
    /// <summary>Allowable misalignment distance</summary>
    public double Tolerance { get; set; } = 0.01;
}

/// <summary>
/// Types of snap connections available
/// </summary>
public enum SnapType
{
    /// <summary>LEGO-style protruding connection</summary>
    Stud,
    
    /// <summary>LEGO-style receiving connection</summary>
    Hole,
    
    /// <summary>Magnetic attraction snapping</summary>
    Magnetic,
    
    /// <summary>Flat surface alignment</summary>
    FaceToFace,
    
    /// <summary>Linear edge alignment</summary>
    EdgeToEdge,
    
    /// <summary>Point-to-point alignment</summary>
    CornerToCorner,
    
    /// <summary>Process flow connection (factory layout)</summary>
    ProcessConnection
}
