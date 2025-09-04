using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Shared;

namespace Three2025.Apprentice.Snapping
{
    /// <summary>
    /// A snappable box that leverages SpacialFrame3D for all geometry calculations.
    /// This class uses the existing spatial framework for faces, normals, transforms, etc.
    /// Instead of creating our own geometry system, we delegate to SpacialFrame3D.
    /// </summary>
    public class SnapBox : FoShape3D, ISnappable3D
    {
        private SpacialFrame3D _spatialFrame;
        private Dictionary<string, SnapPoint> _snapPoints;
        private List<SnapConstraint> _constraints;

        // Override to hide inherited properties
        public new string Name { get; set; }
        public new string Color { get; set; }

        // Implement ISnappable3D interface using SpacialFrame3D capabilities
        public string Id => GetGlyphId();
        public Point3D Position => new Point3D(
            _spatialFrame?.X ?? Transform?.Position?.X ?? 0,
            _spatialFrame?.Y ?? Transform?.Position?.Y ?? 0,
            _spatialFrame?.Z ?? Transform?.Position?.Z ?? 0
        );
        
        /// <summary>
        /// Gets all faces as Dictionary from the underlying SpacialFrame3D.
        /// This automatically handles transformations, rotations, etc.
        /// </summary>
        public Dictionary<string, Face3D> Faces 
        { 
            get 
            {
                var faceList = _spatialFrame.GetFacesWithNormals();
                return faceList.ToDictionary(f => f.Name, f => f);
            }
        }

        /// <summary>
        /// Snap points for precise connection, generated from face centers.
        /// </summary>
        public Dictionary<string, SnapPoint> SnapPoints => _snapPoints;

        /// <summary>
        /// Active constraints involving this component.
        /// </summary>
        public List<SnapConstraint> Constraints => _constraints;

        /// <summary>
        /// Creates a new SnapBox using the SpacialFrame3D system for geometry management.
        /// </summary>
        public SnapBox(string name, double width, double height, double depth, string color = "blue") 
            : base(name, color)
        {
            Name = name;
            Color = color;
            
            // Create spatial frame with the specified dimensions
            // The frame handles all face calculations, normals, transforms, etc.
            var spec = new FoSpec3D 
            { 
                W = width, 
                H = height, 
                D = depth,
                X = 0, Y = 0, Z = 0,
                Px = width/2, Py = height/2, Pz = depth/2
            };
            _spatialFrame = new SpacialFrame3D(spec);
            
            // Create the visual component using FoShape3D
            CreateBox(name, width, height, depth);
            
            // Initialize collections
            _snapPoints = new Dictionary<string, SnapPoint>();
            _constraints = new List<SnapConstraint>();
            
            // Create snap points from the spatial frame faces
            InitializeSnapPoints();
        }

        /// <summary>
        /// Initialize snap points from the spatial frame face centers.
        /// </summary>
        private void InitializeSnapPoints()
        {
            foreach (var face in Faces.Values)
            {
                _snapPoints[face.Name + "Center"] = new SnapPoint
                {
                    Name = face.Name + "Center",
                    LocalPosition = new Vector3(face.Center.X, face.Center.Y, face.Center.Z),
                    Normal = face.Normal,
                    Type = SnapType.FaceToFace,
                    Tolerance = 0.01
                };
            }
        }

        /// <summary>
        /// Sets the position and updates both the spatial frame and visual component.
        /// </summary>
        public void SetPosition(double x, double y, double z)
        {
            // Update the spatial frame transform - this recalculates all face positions/normals
            _spatialFrame.SetTransform(x, y, z, 
                _spatialFrame.Rx, _spatialFrame.Ry, _spatialFrame.Rz);
            
            // Update the visual component transform position
            Transform.Position = new Vector3(x, y, z);
        }

        /// <summary>
        /// Sets the rotation and updates both the spatial frame and visual component.
        /// </summary>
        public void SetRotation(double rx, double ry, double rz)
        {
            // Update the spatial frame transform - this recalculates rotated normals
            _spatialFrame.SetTransform(_spatialFrame.X, _spatialFrame.Y, _spatialFrame.Z, 
                rx, ry, rz);
            
            // Update the visual component rotation using Euler angles
            Transform.Rotation = new Euler(rx, ry, rz);
        }

        /// <summary>
        /// Gets a specific face by name, leveraging SpacialFrame3D's face system.
        /// </summary>
        public Face3D GetFace(string faceName)
        {
            return Faces.Values.FirstOrDefault(f => f.Name.Equals(faceName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a specific snap point by name.
        /// </summary>
        public SnapPoint GetSnapPoint(string pointName)
        {
            _snapPoints.TryGetValue(pointName, out var point);
            return point;
        }

        /// <summary>
        /// Check if this SnapBox can snap to another ISnappable3D object.
        /// Uses the Face3D system for compatibility checking.
        /// </summary>
        public bool CanSnapTo(ISnappable3D other, string myFace, string otherFace)
        {
            try
            {
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

        /// <summary>
        /// Calculate the transform needed to snap this object's face to another object's face.
        /// Uses Face3D's built-in transform calculations and SpacialFrame3D geometry.
        /// </summary>
        public Transform3 CalculateSnapTransform(string myFace, string otherFace, ISnappable3D other)
        {
            var myFaceInfo = GetFace(myFace);
            var otherFaceInfo = other.GetFace(otherFace);
            
            if (myFaceInfo == null || otherFaceInfo == null)
            {
                return new Transform3(); // Return identity transform
            }
            
            // Calculate transform to align faces using the existing Face3D system
            var targetPosition = new Vector3(otherFaceInfo.Center.X, otherFaceInfo.Center.Y, otherFaceInfo.Center.Z);
            var offset = myFaceInfo.Normal * 0.001; // Small offset to prevent z-fighting
            var finalPosition = targetPosition + offset;
            
            // Use Face3D's quaternion calculations for proper face-to-face alignment
            var sourceNormal = myFaceInfo.Normal;
            var targetNormal = new Vector3(-otherFaceInfo.Normal.X, -otherFaceInfo.Normal.Y, -otherFaceInfo.Normal.Z); // Opposite for face contact
            var rotationQuaternion = Quaternion.FromToRotation(sourceNormal, targetNormal);
            
            return new Transform3
            {
                Position = finalPosition,
                QuaternionRotation = rotationQuaternion
            };
        }

        /// <summary>
        /// Access the underlying spatial frame for advanced geometry operations.
        /// This provides access to all the SpacialFrame3D capabilities including 
        /// edges, face normals, vertices, transforms, etc.
        /// </summary>
        public SpacialFrame3D SpatialFrame => _spatialFrame;
    }
}
