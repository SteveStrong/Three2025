using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;
using BlazorThreeJS.Core;

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
            Transform = new Transform3
            {
                Position = new Vector3(axisOffset, 0, 0)
            }
        }.CreateBox($"{name}_XAxis", axisLength, axisRadius, axisRadius);

        var xHead = new FoShape3D
        {
            Name = $"head",
            Color = "#FF0000",
            Transform = new Transform3()
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
            Transform = new Transform3
            {
                Position = new Vector3(0, axisOffset, 0)
            }
        }.CreateBox($"{name}_YAxis", axisRadius, axisLength, axisRadius);

        var yHead = new FoShape3D
        {
            Name = $"head",
            Color = "#00FF00",
            Transform = new Transform3()
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
            Transform = new Transform3
            {
                Position = new Vector3(0, 0, axisOffset)
            }
        }.CreateBox($"{name}_ZAxis", axisRadius, axisRadius, axisLength);

        var zHead = new FoShape3D
        {
            Name = $"head",
            Color = "#0000FF",
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, axisOffset),
            }
        }.CreateBox($"head", headLength, headLength, headLength);
        zAxis.AddSubGlyph3D<FoShape3D>(zHead);

        var group = new FoShape3D
        {
            Name = $"{name}_Axes",
            Transform = new Transform3
            {
                Position = transform.Position,
                Rotation = transform.Rotation,
                Scale = transform.Scale
            }
        }.CreateGroup($"{name}_Axes", headLength, headLength, headLength);

        group.AddSubGlyph3D<FoShape3D>(xAxis);
        group.AddSubGlyph3D<FoShape3D>(yAxis);
        group.AddSubGlyph3D<FoShape3D>(zAxis);

        arena.AddShapeToStage<FoShape3D>(group);
        return group;
    }
    public void ShowLabeledVertices(IArena arena, IEnumerable<Point3D> points)
    {
        int i = 0;
        foreach (var point in points)
        {
            var vertexShape = new FoShape3D
            {
                Name = point.Name ?? $"Vertex{i}",
                Color = "#2196F3",
                Transform = new Transform3
                {
                    Position = new Vector3(point.X, point.Y, point.Z)
                }
            }.CreateSphere(point.Name, 0.05, 0.05, 0.05);

            var label = new FoText3D("VertexLabel", "White")
            {
                Text = $"{point.Name}: ({point.X:F2}, {point.Y:F2}, {point.Z:F2})",
                Transform = new Transform3()
                {
                    Position = new Vector3(0, 0.15, 0),
                }
            };
            vertexShape.AddSubGlyph3D<FoText3D>(label);
            label.Text.WriteSuccess();

            arena.AddShapeToStage<FoShape3D>(vertexShape);
            i++;
        }
    }

    public void ShowLabeledEdges(IArena arena, IEnumerable<Edge3D> edges)
    {
        foreach (var edge in edges)
        {
            // Create path from edge start to end points for tube geometry
            var edgePath = new List<Vector3>
            {
                new Vector3(edge.Start.X, edge.Start.Y, edge.Start.Z),
                new Vector3(edge.End.X, edge.End.Y, edge.End.Z)
            };

            var edgeShape = new FoPipe3D($"Edge_{edge.Name}", "#333")
            {
                GlyphId = Guid.NewGuid().ToString(),
            }.CreateTube(edge.Name, 0.03, edgePath);

            var label = new FoText3D("EdgeLabel", "Yellow")
            {
                Text = $"{edge.Name}: L={edge.Length:F2}",
                Transform = new Transform3()
                {
                    Position = new Vector3(edge.Midpoint.X, edge.Midpoint.Y, edge.Midpoint.Z + 0.1),
                }
            };
            edgeShape.AddSubGlyph3D<FoText3D>(label);
            label.Text.WriteSuccess();

            arena.AddShapeToStage<FoPipe3D>(edgeShape);
        }
    }

    public void ShowLabeledFaces(IArena arena, IEnumerable<Face3D> faces)
    {
        //this works because the Width Height Depth properties were added to Face3D
        //and computed from the vertices Min Max values

        foreach (var face in faces)
        {
            var center = face.Center;
            var normalShape = new FoShape3D
            {
                Name = $"Face_{face.Name}",
                Color = "#F00",
                Transform = new Transform3
                {
                    Position = center.AsVector3(),
                }
            }.CreateBoundary($"Face_{face.Name}", face.Width, face.Height, face.Depth);



            var LabelName = new FoText3D("Name", "White")
            {
                Text = $"{face.Name} {center.X:F2}, {center.Y:F2}, {center.Z:F2}",
                Transform = new Transform3(),
                TextAlign = Text3DAlign.Center,
            };
            normalShape.AddSubGlyph3D<FoText3D>(LabelName);
            LabelName.Text.WriteSuccess();

            arena.AddShapeToStage<FoShape3D>(normalShape);
        }
    }

    public void ShowLabeledNormals(IArena arena, IEnumerable<Face3D> faces)
    {
        foreach (var face in faces)
        {
            var (mid, n, euler, length) = face.GetNormalCylinderTransform(0.4);

            var normalShape = new FoShape3D {
                Name = $"Normal_{face.Name}",
                Color = "#F00",
                Transform = new Transform3 {
                    Position = mid.AsVector3(),
                    Rotation = new Euler(euler.X, euler.Y, euler.Z, "XYZ")
                }
            }.CreateCylinder($"Normal_{face.Name}", 0.015, length, 0.015);

            var cone = new FoShape3D
            {
                Name = $"NormalCone_{face.Name}",
                Color = "#F00",
                Transform = new Transform3()
                {
                    Position = new Vector3(0, length / 2, 0),
                }
            }.CreateCone($"NormalCone_{face.Name}", 0.1, 0.2, 0.1);

            normalShape.AddSubGlyph3D<FoShape3D>(cone);

            var LabelName = new FoText3D("Name", "White")
            {
                Text = $"{face.Name} {n.X:F2}, {n.Y:F2}, {n.Z:F2}",
                Transform = new Transform3()
                {
                    Position = new Vector3(0, length, 0),
                }
            };
            normalShape.AddSubGlyph3D<FoText3D>(LabelName);
            LabelName.Text.WriteSuccess();

            arena.AddShapeToStage<FoShape3D>(normalShape);
        }
    }

    public void ShowCoordinateAxes(IArena arena, Transform3 transform)
    {
        var axisLength = 2.0;
        var axisRadius = 0.02;

        // X axis - Red
        var xAxis = new FoShape3D
        {
            Name = "XAxis",
            Color = "#FF0000",
            Transform = new Transform3
            {
                Position = transform.Position,
                Rotation = new Euler(0, 0, -Math.PI/2, "XYZ")
            }
        }.CreateCylinder("XAxis", axisRadius, axisLength, axisRadius);

        // Y axis - Green  
        var yAxis = new FoShape3D
        {
            Name = "YAxis",
            Color = "#00FF00",
            Transform = new Transform3
            {
                Position = transform.Position,
                Rotation = new Euler(0, 0, 0, "XYZ")
            }
        }.CreateCylinder("YAxis", axisRadius, axisLength, axisRadius);

        // Z axis - Blue
        var zAxis = new FoShape3D
        {
            Name = "ZAxis",
            Color = "#0000FF", 
            Transform = new Transform3
            {
                Position = transform.Position,
                Rotation = new Euler(Math.PI/2, 0, 0, "XYZ")
            }
        }.CreateCylinder("ZAxis", axisRadius, axisLength, axisRadius);

        arena.AddShapeToStage<FoShape3D>(xAxis);
        arena.AddShapeToStage<FoShape3D>(yAxis);
        arena.AddShapeToStage<FoShape3D>(zAxis);
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
            Transform = new Transform3() {
                Position = new Vector3(position.X, position.Y, position.Z)
            }
        }.CreateSphere(name, radius, radius, radius);
        
        arena?.AddShapeToStage<FoShape3D>(shape);
        return shape;
    }
    
    public FoShape3D CreateMarkerCylinder(IArena arena, string name, Point3D position, Vector3 rotation, string color, double radius, double height)
    {
        var shape = new FoShape3D()
        {
            Name = name,
            Color = color,
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3() {
                Position = new Vector3(position.X, position.Y, position.Z),
                Rotation = new Euler(rotation.X, rotation.Y, rotation.Z)
            }
        }.CreateCylinder(name, radius, height, radius);
        
        arena?.AddShapeToStage<FoShape3D>(shape);
        return shape;
    }
    
    public FoShape3D CreateMarkerPlane(IArena arena, string name, Point3D position, Vector3 rotation, string color, double width, double height, double depth, double opacity = 1.0)
    {
        var shape = new FoShape3D()
        {
            Name = name,
            Color = color,
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3() {
                Position = new Vector3(position.X, position.Y, position.Z),
                Rotation = new Euler(rotation.X, rotation.Y, rotation.Z)
            },
            Opacity = opacity
        }.CreatePlane(name, width, height, depth);
        
        arena?.AddShapeToStage<FoShape3D>(shape);
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
