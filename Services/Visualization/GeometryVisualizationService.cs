using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shared;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Services.Visualization;

public class GeometryVisualizationService : IGeometryVisualizationService
{
    public void ShowLabeledVertices(IArena arena, IEnumerable<Point3D> vertices)
    {
        arena.ClearArena();
        int i = 0;
        foreach (var v in vertices)
        {
            var vertexShape = new FoShape3D
            {
                Name = $"Vertex{i}",
                Color = "#2196F3",
                Transform = new Transform3
                {
                    Position = new Vector3(v.X, v.Y, v.Z)
                }
            }.CreateSphere($"Vertex{i}", 0.05, 0.05, 0.05);

            var label = new FoText3D("VertexLabel", "White")
            {
                Text = $"V{i}: ({v.X:F2}, {v.Y:F2}, {v.Z:F2})",
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
        arena.ClearArena();
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

    public void ShowWireframeFaces(IArena arena, IEnumerable<Face3D> faces)
    {
        arena.ClearArena();
        foreach (var face in faces)
        {
            var (center, euler) = face.GetTransformForVisualization();
            
            // Create wireframe face
            var faceShape = new FoShape3D {
                Name = $"Face_{face.Name}",
                Color = "#4CAF50",
                Transform = new Transform3 {
                    Position = center.AsVector3(),
                    Rotation = new Euler(euler.X, euler.Y, euler.Z, "XYZ")
                }
            }.CreateBox($"Face_{face.Name}", face.Width, face.Height, 0.02);

            // Make it wireframe
            faceShape.AsBoundary();

            // Add label positioned well outside the face along the normal
            var normalOffset = 0.8;
            var labelPosition = new Vector3(
                face.Normal.X * normalOffset,
                face.Normal.Y * normalOffset, 
                face.Normal.Z * normalOffset
            );

            var label = new FoText3D("FaceLabel", "Cyan")
            {
                Text = $"{face.Name}\n({face.Width:F1}×{face.Height:F1})",
                Transform = new Transform3()
                {
                    Position = labelPosition,
                }
            };
            
            // Add label to arena separately to ensure it renders on top
            arena.AddShapeToStage<FoText3D>(label);
            arena.AddShapeToStage<FoShape3D>(faceShape);
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
                    Position = new Vector3(0, length/2, 0),
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
        ShowLabeledVerticesInternal(arena, vertices);
        ShowLabeledEdgesInternal(arena, edges);
        ShowWireframeFacesInternal(arena, faces);
        ShowLabeledNormalsInternal(arena, faces);
    }

    // Internal methods that don't clear arena (for ShowAll)
    private void ShowLabeledVerticesInternal(IArena arena, IEnumerable<Point3D> vertices)
    {
        int i = 0;
        foreach (var v in vertices)
        {
            var vertexShape = new FoShape3D
            {
                Name = $"Vertex{i}",
                Color = "#2196F3",
                Transform = new Transform3
                {
                    Position = new Vector3(v.X, v.Y, v.Z)
                }
            }.CreateSphere($"Vertex{i}", 0.05, 0.05, 0.05);

            var label = new FoText3D("VertexLabel", "White")
            {
                Text = $"V{i}: ({v.X:F2}, {v.Y:F2}, {v.Z:F2})",
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

    private void ShowLabeledEdgesInternal(IArena arena, IEnumerable<Edge3D> edges)
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

    private void ShowWireframeFacesInternal(IArena arena, IEnumerable<Face3D> faces)
    {
        foreach (var face in faces)
        {
            var (center, euler) = face.GetTransformForVisualization();
            
            var faceShape = new FoShape3D {
                Name = $"Face_{face.Name}",
                Color = "#4CAF50",
                Transform = new Transform3 {
                    Position = center.AsVector3(),
                    Rotation = new Euler(euler.X, euler.Y, euler.Z, "XYZ")
                }
            }.CreateBoundary($"Face_{face.Name}", face.Width, face.Height, 0.02);

            //faceShape.AsBoundary();

            var normalOffset = 0.8;
            var labelPosition = new Vector3(
                face.Normal.X * normalOffset,
                face.Normal.Y * normalOffset, 
                face.Normal.Z * normalOffset
            );

            var label = new FoText3D("FaceLabel", "Cyan")
            {
                Text = $"{face.Name}\n({face.Width:F1}×{face.Height:F1})",
                Transform = new Transform3()
                {
                    Position = labelPosition,
                }
            };
            
            arena.AddShapeToStage<FoText3D>(label);
            arena.AddShapeToStage<FoShape3D>(faceShape);
        }
    }

    private void ShowLabeledNormalsInternal(IArena arena, IEnumerable<Face3D> faces)
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
                    Position = new Vector3(0, length/2, 0),
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
}
