using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;

namespace Three2025.Services.Visualization;

public interface IGeometryVisualizationService
{
    void ShowLabeledVertices(IArena arena, IEnumerable<Point3D> vertices);
    void ShowLabeledEdges(IArena arena, IEnumerable<Edge3D> edges);
    void ShowLabeledFaces(IArena arena, IEnumerable<Face3D> faces);
    void ShowLabeledNormals(IArena arena, IEnumerable<Face3D> faces);
    void ShowCoordinateAxes(IArena arena, Transform3 transform);
    void ShowAll(IArena arena, IEnumerable<Point3D> vertices, IEnumerable<Edge3D> edges, IEnumerable<Face3D> faces);
    
    // Marker creation utilities
    FoShape3D CreateMarkerAxis(IArena arena, string name, Transform3 transform);
    FoShape3D CreateMarkerSphere(IArena arena, string name, Point3D position, string color, double radius);
    FoShape3D CreateMarkerCylinder(IArena arena, string name, Point3D position, Vector3 rotation, string color, double radius, double height);
    FoShape3D CreateMarkerPlane(IArena arena, string name, Point3D position, Vector3 rotation, string color, double width, double height, double depth, double opacity = 1.0);
    void CreateCoordinateMarker(IArena arena, string name, Point3D position, double size = 0.1);
}
