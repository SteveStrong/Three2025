using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Geometires;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Three2025.Shared;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;

namespace Three2025.Components.Pages;

public class SpacialFrameTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

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
        
        VisualizationService.ShowLabeledVertices(arena, CurrentFrame.Vertices);
        StatusMessage = $"Showing {CurrentFrame.Vertices.Count} vertices as labeled spheres.";
        StateHasChanged();
    }

    public void ShowEdges()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        var edges = CurrentFrame.GetEdgesWithNames();
        VisualizationService.ShowLabeledEdges(arena, edges);
        StatusMessage = $"Showing {edges.Count} edges as labeled cylinders.";
        StateHasChanged();
    }

    public void ShowFaces()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        var faces = CurrentFrame.GetFacesWithNormals();
        VisualizationService.ShowWireframeFaces(arena, faces);
        StatusMessage = $"Showing {faces.Count} faces as wireframe outlines with labels.";
        StateHasChanged();
    }

    public void ShowNormals()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        var faces = CurrentFrame.GetFacesWithNormals();
        VisualizationService.ShowLabeledNormals(arena, faces);
        StatusMessage = $"Showing {faces.Count} face normals as red cylinders with cones and labels, aligned with normals.";
        StateHasChanged();
    }

    public void ShowAxes()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        VisualizationService.ShowCoordinateAxes(arena, CurrentFrame.Transform);
        StatusMessage = "Showing coordinate axes: Red=X, Green=Y, Blue=Z";
        StateHasChanged();
    }

    public void ClearAll()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        arena.ClearArena();
        StateHasChanged();
    }

    public void ShowAll()
    {
        if (CurrentFrame == null) return;
        ClearAll();
        
        // Show the main frame with transparency
        CreateSpacialFrame();
        
        // Add vertices as small spheres
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int i = 0;
        foreach (var v in CurrentFrame.Vertices)
        {
            CreateMarkerSphere($"Vertex{i}", v, "#2196F3", 0.03);
            i++;
        }
        
        // Add coordinate axes
        ShowAxes();
        
        StatusMessage = $"Showing complete frame analysis: {CurrentFrame.Vertices.Count} vertices + axes";
        StateHasChanged();
    }

    public void ShowAllDetailed()
    {
        if (CurrentFrame == null) return;
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        var vertices = CurrentFrame.Vertices;
        var edges = CurrentFrame.GetEdgesWithNames();
        var faces = CurrentFrame.GetFacesWithNormals();
        
        VisualizationService.ShowAll(arena, vertices, edges, faces);
        StatusMessage = $"Showing comprehensive view: {vertices.Count} vertices, {edges.Count} edges, {faces.Count} faces with labels and normals";
        StateHasChanged();
    }

    // Preset rotation tests for validation
    public void SetRotationPreset(string preset)
    {
        switch (preset.ToLower())
        {
            case "identity":
                FrameRx = FrameRy = FrameRz = 0;
                break;
            case "45x":
                FrameRx = 45; FrameRy = FrameRz = 0;
                break;
            case "45y":
                FrameRy = 45; FrameRx = FrameRz = 0;
                break;
            case "45z":
                FrameRz = 45; FrameRx = FrameRy = 0;
                break;
            case "45xyz":
                FrameRx = FrameRy = FrameRz = 45;
                break;
            case "90x":
                FrameRx = 90; FrameRy = FrameRz = 0;
                break;
            case "90y":
                FrameRy = 90; FrameRx = FrameRz = 0;
                break;
            case "90z":
                FrameRz = 90; FrameRx = FrameRy = 0;
                break;
        }
        CreateSpacialFrame();
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
