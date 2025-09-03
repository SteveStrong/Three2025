using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Shapes3D.SpacialFrame;

namespace Three2025.Apprentice.Snapping;

/// <summary>
/// A snappable box that extends SpacialBox3D with snapping capabilities
/// </summary>
public class SnapBox : ISnappable3D
{
    public Dictionary<string, Face3D> Faces { get; private set; }
    public Dictionary<string, SnapPoint> SnapPoints { get; private set; }
    public List<SnapConstraint> Constraints { get; private set; }
    
    private SpacialBox3D spatial;
    private FoShape3D visualComponent;
    
    public SnapBox(SpacialBox3D spatialBox, FoShape3D visual = null)
    {
        spatial = spatialBox;
        visualComponent = visual;
        
        Faces = new Dictionary<string, Face3D>();
        SnapPoints = new Dictionary<string, SnapPoint>();
        Constraints = new List<SnapConstraint>();
        
        InitializeSnappingSystem();
    }
    
    public SnapBox(double width, double height, double depth, string units = "m")
    {
        spatial = new SpacialBox3D(width, height, depth, units);
        visualComponent = null;
        
        Faces = new Dictionary<string, Face3D>();
        SnapPoints = new Dictionary<string, SnapPoint>();
        Constraints = new List<SnapConstraint>();
        
        InitializeSnappingSystem();
    }
    
    private void InitializeSnappingSystem()
    {
        // Create faces using existing SpacialBox3D geometry
        CreateFacesFromGeometry();
        CreateSnapPointsFromFaces();
    }
    
    private void CreateFacesFromGeometry()
    {
        // Use the existing Face3D data from SpacialBox3D
        var facesWithNormals = spatial.GetFacesWithNormals();
        
        foreach (var face in facesWithNormals)
        {
            Faces[face.Name] = face;
        }
    }
    
    private void CreateSnapPointsFromFaces()
    {
        // Create snap points at face centers
        foreach (var face in Faces.Values)
        {
            SnapPoints[face.Name + "Center"] = new SnapPoint
            {
                Name = face.Name + "Center",
                LocalPosition = new Vector3(face.Center.X, face.Center.Y, face.Center.Z),
                Normal = face.Normal,
                Type = SnapType.FaceToFace,
                Tolerance = 0.01
            };
        }
    }
    
    public Face3D GetFace(string faceName)
    {
        if (Faces.TryGetValue(faceName, out var face))
            return face;
        return null;
    }
    
    public SnapPoint GetSnapPoint(string pointName)
    {
        if (SnapPoints.TryGetValue(pointName, out var point))
            return point;
        return null;
    }
    
    public bool CanSnapTo(ISnappable3D other, string myFace, string otherFace)
    {
        try
        {
            // Basic compatibility check
            var myFaceInfo = GetFace(myFace);
            var otherFaceInfo = other.GetFace(otherFace);
            
            if (myFaceInfo == null || otherFaceInfo == null)
                return false;
            
            // Check if faces are roughly compatible in size
            var sizeRatio = Math.Min(myFaceInfo.Width, myFaceInfo.Height) / 
                           Math.Max(otherFaceInfo.Width, otherFaceInfo.Height);
            
            return sizeRatio > 0.1 && sizeRatio < 10.0; // Reasonable size compatibility
        }
        catch
        {
            return false;
        }
    }
    
    public Transform3 CalculateSnapTransform(string myFace, string otherFace, ISnappable3D other)
    {
        var myFaceInfo = GetFace(myFace);
        var otherFaceInfo = other.GetFace(otherFace);
        
        if (myFaceInfo == null || otherFaceInfo == null)
        {
            return new Transform3(); // Return identity transform
        }
        
        // Calculate transform to align faces
        var targetPosition = new Vector3(otherFaceInfo.Center.X, otherFaceInfo.Center.Y, otherFaceInfo.Center.Z);
        var offset = myFaceInfo.Normal * 0.001; // Small offset to prevent z-fighting
        var finalPosition = targetPosition + offset;
        
        // 🎯 QUATERNION MAGIC: Calculate proper face-to-face alignment
        var sourceNormal = myFaceInfo.Normal;
        var targetNormal = new Vector3(-otherFaceInfo.Normal.X, -otherFaceInfo.Normal.Y, -otherFaceInfo.Normal.Z); // Opposite for face contact
        var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
        
        return new Transform3
        {
            Position = finalPosition,
            QuaternionRotation = rotationQuaternion // Perfect quaternion alignment!
        };
    }
}
