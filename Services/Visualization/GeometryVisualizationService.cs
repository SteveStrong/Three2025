
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shared;
using FoundryRulesAndUnits.Extensions;

using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Core;

namespace Three2025.Services.Visualization;

public class GeometryVisualizationService : IGeometryVisualizationService
{
    public FoShape3D CreateMarkerAxis(IArena arena, string name, Transform3 transform)
    {

        var axisLength = 1.0;
        var axisRadius = 0.05;
        var headLength = 0.1;
        var axisOffset = 0.8 * axisLength / 2;

        // X axis - Red
        var xAxis = new FoShape3D
        {
            Name = $"{name}_XAxis",
            Color = "#FF0000",
            Transform = new Transform3("XAxisTransform")
            {
                Position = new Vector3(axisOffset, 0, 0)
            }
        }.CreateBox($"{name}_XAxis", axisLength, axisRadius, axisRadius);

        var xHead = new FoShape3D
        {
            Name = $"head",
            Color = "#FF0000",
            Transform = new Transform3("HeadTransform")
            {
                Position = new Vector3(axisOffset, 0, 0),
            }
        }.CreateBox($"head", headLength, headLength, headLength);
        xAxis.AddSubGlyph3D<FoShape3D>(xHead);

        // Y axis - Green  
        var yAxis = new FoShape3D
        {
            Name = $"{name}_YAxis",
            Color = "#00FF00",
            Transform = new Transform3("YAxisTransform")
            {
                Position = new Vector3(0, axisOffset, 0)
            }
        }.CreateBox($"{name}_YAxis", axisRadius, axisLength, axisRadius);

        var yHead = new FoShape3D
        {
            Name = $"head",
            Color = "#00FF00",
            Transform = new Transform3("HeadTransform")
            {
                Position = new Vector3(0, axisOffset, 0),
            }
        }.CreateBox($"head", headLength, headLength, headLength);
        yAxis.AddSubGlyph3D<FoShape3D>(yHead);


        // Z axis - Blue
        var zAxis = new FoShape3D
        {
            Name = $"{name}_ZAxis",
            Color = "#0000FF", 
            Transform = new Transform3("ZAxisTransform")
            {
                Position = new Vector3(0, 0, axisOffset)
            }
        }.CreateBox($"{name}_ZAxis", axisRadius, axisRadius, axisLength);

        var zHead = new FoShape3D
        {
            Name = $"head",
            Color = "#0000FF",
            Transform = new Transform3("HeadTransform")
            {
                Position = new Vector3(0, 0, axisOffset),
            }
        }.CreateBox($"head", headLength, headLength, headLength);
        zAxis.AddSubGlyph3D<FoShape3D>(zHead);

        var groupTransform = new Transform3("GroupTransform");
        groupTransform.Position = Vector3.Zero;
        groupTransform.MoveBy(transform.Position.X, transform.Position.Y, transform.Position.Z);
        groupTransform.Rotation = new Euler(0,0,0);
        groupTransform.RotateBy(transform.Rotation.X, transform.Rotation.Y, transform.Rotation.Z, AngleUnit.Radians);
        groupTransform.Scale = transform.Scale;
        var group = new FoShape3D
        {
            Name = $"{name}_Axes",
            Transform = groupTransform
        }.CreateGroup($"{name}_Axes", headLength, headLength, headLength);

        group.AddSubGlyph3D<FoShape3D>(xAxis);
        group.AddSubGlyph3D<FoShape3D>(yAxis);
        group.AddSubGlyph3D<FoShape3D>(zAxis);

        var stage = arena.CurrentStage();
        arena.AddShapeToStage<FoShape3D>(group, stage.GetName());
        return group;
    }
    public void ShowLabeledVertices(IArena arena, IEnumerable<Point3D> points)
    {
        int i = 0;
        foreach (var point in points)
        {
            var vertexTransform = new Transform3("VertexTransform");
            vertexTransform.Position = Vector3.Zero;
            vertexTransform.MoveBy(point.X, point.Y, point.Z);
            var vertexShape = new FoShape3D
            {
                Name = point.Name ?? $"Vertex{i}",
                Color = "#2196F3",
                Transform = vertexTransform
            }.CreateSphere(point.Name, 0.05, 0.05, 0.05);

            var label = new FoText3D("VertexLabel", "White")
            {
                Text = $"{point.Name}: ({point.X:F2}, {point.Y:F2}, {point.Z:F2})",
                Transform = new Transform3("LabelTransform")
                {
                    Position = new Vector3(0, 0.15, 0),
                }
            };
            vertexShape.AddSubGlyph3D<FoText3D>(label);
            label.Text.WriteSuccess();

            var stage = arena.CurrentStage();
            arena.AddShapeToStage<FoShape3D>(vertexShape, stage.GetName());
            i++;
        }
    }

    public void ShowLabeledEdges(IArena arena, IEnumerable<Edge3D> edges)
    {
        foreach (var edge in edges)
        {
            // Create path from edge start to end points for tube geometry
            var edgePath = edge.AsPath();

            var edgeShape = new FoPipe3D($"Edge_{edge.Name}", "#333")
            {
                GlyphId = Guid.NewGuid().ToString(),
            }.CreateTube(edge.Name, 0.03, edgePath);

            var mid = edge.Midpoint;
            var labelTransform = new Transform3("LabelTransform");
            labelTransform.Position = Vector3.Zero;
            labelTransform.MoveBy(mid.X, mid.Y, mid.Z + 0.1);
            var label = new FoText3D("EdgeLabel", "Yellow")
            {
                Text = $"{edge.Name}: L={edge.Length:F2} M={mid.X:F2}, {mid.Y:F2}, {mid.Z:F2}",
                Transform = labelTransform
            };
            edgeShape.AddSubGlyph3D<FoText3D>(label);
            label.Text.WriteSuccess();

            var stage = arena.CurrentStage();
            arena.AddShapeToStage<FoPipe3D>(edgeShape, stage.GetName());
        }
    }

    public void ShowLabeledFaces(IArena arena, IEnumerable<Face3D> faces)
    {
        //this works because the Width Height Depth properties were added to Face3D
        //and computed from the vertices Min Max values

        foreach (var face in faces)
        {
            var center = face.Center;
            var faceTransform = new Transform3("FaceTransform");
            faceTransform.Position = Vector3.Zero;
            faceTransform.MoveBy(center.X, center.Y, center.Z);
            var normalShape = new FoShape3D
            {
                Name = $"Face_{face.Name}",
                Color = "#F00",
                Transform = faceTransform
            }.CreateBoundary($"Face_{face.Name}", face.Width, face.Height, face.Depth);



            var LabelName = new FoText3D("Name", "White")
            {
                Text = $"{face.Name} {center.X:F2}, {center.Y:F2}, {center.Z:F2}",
                Transform = new Transform3("LabelTransform"),
                TextAlign = Text3DAlign.Center,
            };
            normalShape.AddSubGlyph3D<FoText3D>(LabelName);
            LabelName.Text.WriteSuccess();

            var stage = arena.CurrentStage();
            arena.AddShapeToStage<FoShape3D>(normalShape, stage.GetName());
        }
    }

    public void ShowLabeledNormals(IArena arena, IEnumerable<Face3D> faces)
    {
        foreach (var face in faces)
        {
            var (mid, n, euler, length) = face.GetNormalCylinderTransform(0.4);

            var normalTransform = new Transform3("NormalTransform");
            normalTransform.Position = Vector3.Zero;
            normalTransform.MoveBy(mid.X, mid.Y, mid.Z);
            normalTransform.Rotation = new Euler(0,0,0);
            normalTransform.RotateBy(euler.X, euler.Y, euler.Z, AngleUnit.Radians);
            var normalShape = new FoShape3D {
                Name = $"Normal_{face.Name}",
                Color = "#F00",
                Transform = normalTransform
            }.CreateCylinder($"Normal_{face.Name}", 0.015, length, 0.015);

            var cone = new FoShape3D
            {
                Name = $"NormalCone_{face.Name}",
                Color = "#F00",
                Transform = new Transform3("NormalConeTransform")
                {
                    Position = new Vector3(0, length / 2, 0),
                }
            }.CreateCone($"NormalCone_{face.Name}", 0.1, 0.2, 0.1);

            normalShape.AddSubGlyph3D<FoShape3D>(cone);

            var LabelName = new FoText3D("Name", "White")
            {
                Text = $"{face.Name} {n.X:F2}, {n.Y:F2}, {n.Z:F2}",
                Transform = new Transform3("LabelTransform")
                {
                    Position = new Vector3(0, length, 0),
                }
            };
            normalShape.AddSubGlyph3D<FoText3D>(LabelName);
            LabelName.Text.WriteSuccess();

            var stage = arena.CurrentStage();
            arena.AddShapeToStage<FoShape3D>(normalShape, stage.GetName());
        }
    }

    public void ShowCoordinateAxes(IArena arena, Transform3 transform)
    {
        var axisLength = 2.0;
        var axisRadius = 0.02;

        // X axis - Red
        var xAxisTransform = new Transform3("XAxisTransform");
        xAxisTransform.Position = Vector3.Zero;
        xAxisTransform.MoveBy(transform.Position.X, transform.Position.Y, transform.Position.Z);
        xAxisTransform.Rotation = new Euler(0,0,0);
        xAxisTransform.RotateBy(0, 0, -Math.PI/2, AngleUnit.Radians);
        var xAxis = new FoShape3D
        {
            Name = "XAxis",
            Color = "#FF0000",
            Transform = xAxisTransform
        }.CreateCylinder("XAxis", axisRadius, axisLength, axisRadius);

        // Y axis - Green  
        var yAxisTransform = new Transform3("YAxisTransform");
        yAxisTransform.Position = Vector3.Zero;
        yAxisTransform.MoveBy(transform.Position.X, transform.Position.Y, transform.Position.Z);
        yAxisTransform.Rotation = new Euler(0,0,0);
        yAxisTransform.RotateBy(0, 0, 0, AngleUnit.Radians);
        var yAxis = new FoShape3D
        {
            Name = "YAxis",
            Color = "00FF00",
            Transform = yAxisTransform
        }.CreateCylinder("YAxis", axisRadius, axisLength, axisRadius);

        // Z axis - Blue
        var zAxisTransform = new Transform3("ZAxisTransform");
        zAxisTransform.Position = Vector3.Zero;
        zAxisTransform.MoveBy(transform.Position.X, transform.Position.Y, transform.Position.Z);
        zAxisTransform.Rotation = new Euler(0,0,0);
        zAxisTransform.RotateBy(Math.PI/2, 0, 0, AngleUnit.Radians);
        var zAxis = new FoShape3D
        {
            Name = "ZAxis",
            Color = "#0000FF", 
            Transform = zAxisTransform
        }.CreateCylinder("ZAxis", axisRadius, axisLength, axisRadius);

        var stage = arena.CurrentStage();
        arena.AddShapeToStage<FoShape3D>(xAxis, stage.GetName());
        arena.AddShapeToStage<FoShape3D>(yAxis, stage.GetName());
        arena.AddShapeToStage<FoShape3D>(zAxis, stage.GetName());
    }

    public void ShowAll(IArena arena, IEnumerable<Point3D> vertices, IEnumerable<Edge3D> edges, IEnumerable<Face3D> faces)
    {
        arena.ClearArena();
        
        // Show all components without clearing arena between them
        ShowLabeledVertices(arena, vertices);
        ShowLabeledEdges(arena, edges);
        ShowLabeledFaces(arena, faces);
        ShowLabeledNormals(arena, faces);
    }

    // Internal methods that don't clear arena (for ShowAll)




 
    
    // Marker creation utilities
    public FoShape3D CreateMarkerSphere(IArena arena, string name, Point3D position, string color, double radius)
    {
        var shape = new FoShape3D()
        {
            Name = name,
            Color = color,
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3("SphereTransform") {
                Position = new Vector3(position.X, position.Y, position.Z)
            }
        }.CreateSphere(name, radius, radius, radius);
        
        var stage = arena.CurrentStage();
        arena?.AddShapeToStage<FoShape3D>(shape, stage.GetName());
        return shape;
    }
    
    public FoShape3D CreateMarkerCylinder(IArena arena, string name, Point3D position, Vector3 rotation, string color, double radius, double height)
    {
        var shape = new FoShape3D()
        {
            Name = name,
            Color = color,
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3("CylinderTransform") {
                Position = new Vector3(position.X, position.Y, position.Z),
                Rotation = new Euler(rotation.X, rotation.Y, rotation.Z)
            }
        }.CreateCylinder(name, radius, height, radius);
        
        var stage = arena.CurrentStage();
        arena?.AddShapeToStage<FoShape3D>(shape, stage.GetName());
        return shape;
    }
    
    public FoShape3D CreateMarkerPlane(IArena arena, string name, Point3D position, Vector3 rotation, string color, double width, double height, double depth, double opacity = 1.0)
    {
        var shape = new FoShape3D()
        {
            Name = name,
            Color = color,
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3("PlaneTransform") {
                Position = new Vector3(position.X, position.Y, position.Z),
                Rotation = new Euler(rotation.X, rotation.Y, rotation.Z)
            },
            Opacity = opacity
        }.CreatePlane(name, width, height, depth);
        
        var stage = arena.CurrentStage();
        arena?.AddShapeToStage<FoShape3D>(shape, stage.GetName());
        return shape;
    }
    
    public void CreateCoordinateMarker(IArena arena, string name, Point3D position, double size = 0.1)
    {
        // Create X, Y, Z axis markers as small colored spheres
        CreateMarkerSphere(arena, $"{name}_X", new Point3D(position.X + size, position.Y, position.Z), "#FF0000", size * 0.3);
        CreateMarkerSphere(arena, $"{name}_Y", new Point3D(position.X, position.Y + size, position.Z), "#00FF00", size * 0.3);
        CreateMarkerSphere(arena, $"{name}_Z", new Point3D(position.X, position.Y, position.Z + size), "#0000FF", size * 0.3);
        CreateMarkerSphere(arena, $"{name}_Origin", position, "#FFFFFF", size * 0.2);
    }


}
