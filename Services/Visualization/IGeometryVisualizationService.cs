using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;

namespace Three2025.Services.Visualization;

public interface IGeometryVisualizationService
{
    void ShowLabeledVertices(IArena arena, IEnumerable<Point3D> vertices);
    void ShowLabeledEdges(IArena arena, IEnumerable<Edge3D> edges);
    void ShowWireframeFaces(IArena arena, IEnumerable<Face3D> faces);
    void ShowLabeledNormals(IArena arena, IEnumerable<Face3D> faces);
    void ShowCoordinateAxes(IArena arena, Transform3 transform);
    void ShowAll(IArena arena, IEnumerable<Point3D> vertices, IEnumerable<Edge3D> edges, IEnumerable<Face3D> faces);
}
