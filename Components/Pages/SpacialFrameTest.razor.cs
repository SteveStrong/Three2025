using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Geometires;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;
using Three2025.Shared;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;

namespace Three2025.Components.Pages;

public class SpacialFrameTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }

    public Canvas3DComponentBase Canvas3DReference = null;
    protected SpacialFrame3D CurrentFrame;

    protected double FrameWidth { get; set; } = 2.0;
    protected double FrameHeight { get; set; } = 1.5;
    protected double FrameDepth { get; set; } = 1.0;
    protected double FrameX { get; set; } = 0.0;
    protected double FrameY { get; set; } = 0.0;
    protected double FrameZ { get; set; } = 0.0;
    protected double FrameRx { get; set; } = 0.0;
    protected double FrameRy { get; set; } = 0.0;
    protected double FrameRz { get; set; } = 0.0;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = string.Empty;

    public void CreateSpacialFrame()
    {
        var spec = new FoSpec3D
        {
            W = FrameWidth,
            H = FrameHeight,
            D = FrameDepth,
            Px = FrameWidth / 2,
            Py = FrameHeight / 2,
            Pz = FrameDepth / 2,
            X = FrameX,
            Y = FrameY,
            Z = FrameZ,
            Rx = FrameRx,
            Ry = FrameRy,
            Rz = FrameRz
        };
        CurrentFrame = new SpacialFrame3D(spec, "m");
        CurrentFrame.UpdateTransform();

        var arena = Workspace?.GetArena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }

        arena.ClearArena();

        var boxShape = new FoShape3D()
        {
            Name = "SpacialFrameMain",
            Color = "#8BC34A",
            Opacity = 0.5,
            Transform = CurrentFrame.Transform,
            GlyphId = Guid.NewGuid().ToString()
        }
        .CreateBox("SpacialFrameMain", FrameWidth, FrameHeight, FrameDepth);

        arena.AddShapeToStage<FoShape3D>(boxShape);
 

        StatusMessage = $"Created SpacialFrame3D: {FrameWidth}×{FrameHeight}×{FrameDepth} at ({FrameX},{FrameY},{FrameZ})";
        StateHasChanged();
    }

    public void ShowVertices()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        arena.ClearArena();
        int i = 0;
        foreach (var v in CurrentFrame.Vertices)
        {
            CreateMarkerSphere($"Vertex{i}", v, "#2196F3", 0.05);
            i++;
        }
        StatusMessage = $"Showing {CurrentFrame.Vertices.Count} vertices as spheres.";
        StateHasChanged();
    }

    public void ShowEdges()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        arena.ClearArena();
        var edges = CurrentFrame.GetEdgesWithNames();
        foreach (var edge in edges)
        {
            var edgeShape = new FoShape3D {
                Name = $"Edge_{edge.Name}",
                Color = "#333",
                GlyphId = Guid.NewGuid().ToString(),
                Transform = new Transform3 {
                    Position = new Vector3(edge.Midpoint.X, edge.Midpoint.Y, edge.Midpoint.Z),
                    Rotation = edge.EulerRotation
                }
            }.CreateCylinder(edge.Name, 0.03, edge.Length, 0.03);
            arena.AddShapeToStage<FoShape3D>(edgeShape);
        }
        StatusMessage = $"Showing {edges.Count} edges as cylinders.";
        StateHasChanged();
    }

    public void ShowFaces()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        arena.ClearArena();
        var faces = CurrentFrame.GetFacesWithNormals();
        foreach (var face in faces)
        {
            var (center, euler) = face.GetTransformForVisualization();
            var faceShape = new FoShape3D {
                Name = $"Face_{face.Name}",
                Color = "#4CAF50",
                Opacity = 0.4,
                Transform = new Transform3 {
                    Position = center.AsVector3(),
                    Rotation = new Euler(euler.X, euler.Y, euler.Z, "XYZ")
                }
            }.CreateBox($"Face_{face.Name}", face.Width, face.Height, 0.02);
            arena.AddShapeToStage<FoShape3D>(faceShape);
        }
        StatusMessage = $"Showing {faces.Count} faces as thin boxes, oriented along normals.";
        StateHasChanged();
    }

    public void ShowNormals()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        var faces = CurrentFrame.GetFacesWithNormals();
        foreach (var face in faces)
        {
            var (position, n, euler, length) = face.GetNormalCylinderTransform(0.4);
            var normalShape = new FoShape3D {
                Name = $"Normal_{face.Name}",
                Color = "#F00",
                Transform = new Transform3 {
                    Position = position.AsVector3(),
                    Rotation = new Euler(euler.X, euler.Y, euler.Z, "XYZ")
                }
            }.CreateCylinder($"Normal_{face.Name}", 0.015, length, 0.015);
            arena.AddShapeToStage<FoShape3D>(normalShape);
        }
        StatusMessage = $"Showing {faces.Count} face normals as red cylinders, aligned with normals.";
        StateHasChanged();
    }

    public void ClearAll()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        arena.ClearArena();
        StateHasChanged();
    }

    public void Dispose() { }

    private void CreateMarkerSphere(string name, Point3D position, string color, double radius)
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
        var arena = Workspace?.GetArena();
        arena?.AddShapeToStage<FoShape3D>(shape);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false,null!);

            scene?.SetAfterUpdateAction((s,j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = Workspace.GetArena();
            if (found)
            {
                arena.SetScene(scene!);
                CreateSpacialFrame();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
