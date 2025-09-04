using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;

namespace Three2025.Components.Pages;

public class SpacialFrameTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    //[Inject] public IWorkspace Workspace { get; set; }
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
    private List<FoShape3D> CurrentShapes = new();
    private List<SpacialFrame3D> CurrentFrames = new();

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

        var arena = FoundryService.Arena();
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

        var arena = FoundryService.Arena();
        if (arena == null) return;

        foreach( var frame in CurrentFrames)
        {
            VisualizationService.ShowLabeledVertices(arena, frame.GetVertices());
        }

        StatusMessage = $"Showing {CurrentFrames.Sum(f => f.GetVertices().Count)} vertices as labeled spheres.";
        StateHasChanged();
    }

    public void ShowEdges()
    {
        var arena = FoundryService.Arena();
        if (arena == null) return;

        foreach( var frame in CurrentFrames)
        {
            var edges = frame.GetEdges();
            VisualizationService.ShowLabeledEdges(arena, edges);
        }

        StatusMessage = $"Showing {CurrentFrames.Sum(f => f.GetEdges().Count)} edges as labeled tubes.";
        StateHasChanged();
    }

    public void ShowFaces()
    {
        var arena = FoundryService.Arena();
        if (arena == null) return;

        foreach( var frame in CurrentFrames)
        {
            var faces = frame.GetFaces();
            //VisualizationService.ShowWireframeFaces(arena, faces);
        }

        StatusMessage = $"Showing {CurrentFrames.Sum(f => f.GetFaces().Count)} faces as wireframe outlines with labels.";
        StateHasChanged();
    }

    public void ShowNormals()
    {
        if (CurrentFrame == null) return;
        var arena = FoundryService.Arena();
        if (arena == null) return;
        
        var faces = CurrentFrame.GetFaces();
        VisualizationService.ShowLabeledNormals(arena, faces);
        StatusMessage = $"Showing {faces.Count} face normals as red cylinders with cones and labels, aligned with normals.";
        StateHasChanged();
    }

    public void ShowAxes()
    {
        if (CurrentFrame == null) return;
        var arena = FoundryService.Arena();
        if (arena == null) return;
        
        VisualizationService.ShowCoordinateAxes(arena, CurrentFrame.Transform);
        StatusMessage = "Showing coordinate axes: Red=X, Green=Y, Blue=Z";
        StateHasChanged();
    }

    public void ClearAll()
    {
        var arena = FoundryService.Arena();
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
        var arena = FoundryService.Arena();
        if (arena == null) return;
        
        int i = 0;
        foreach (var v in CurrentFrame.GetVertices())
        {
            VisualizationService.CreateMarkerSphere(arena, $"Vertex{i}", v, "#2196F3", 0.03);
            i++;
        }
        
        // Add coordinate axes
        ShowAxes();
        
        StatusMessage = $"Showing complete frame analysis: {CurrentFrame.GetVertices().Count} vertices + axes";
        StateHasChanged();
    }

    public void ShowAllDetailed()
    {
        if (CurrentFrame == null) return;
        var arena = FoundryService.Arena();
        if (arena == null) return;
        
        var vertices = CurrentFrame.GetVertices();
        var edges = CurrentFrame.GetEdges();
        var faces = CurrentFrame.GetFaces();
        
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
        
        // DEBUG: Log the rotation values to verify units
        StatusMessage = $"Setting rotation: Rx={FrameRx}°, Ry={FrameRy}°, Rz={FrameRz}° (degrees)";
        
        CreateSpacialFrame();
        StateHasChanged();
    }
    
    public void TestRotationUnits()
    {
        if (CurrentFrame == null) return;
        
        // Test rotation conversion
        var testMatrix = BlazorThreeJS.Maths.Matrix3.NewMatrix();
        testMatrix.RotateEuler(90, 0, 0); // Should be 90 degrees around X
        
        var testPoint = new BlazorThreeJS.Maths.Vector3(0, 1, 0); // Point on Y axis
        var transformedPoint = testMatrix.TransformPoint(testPoint);
        
        // After 90° rotation around X, Y should become Z
        // (0,1,0) should become approximately (0,0,1)
        StatusMessage = $"Rotation test: (0,1,0) -> ({transformedPoint.X:F3},{transformedPoint.Y:F3},{transformedPoint.Z:F3}) - Expected: (0,0,1)";
        StateHasChanged();
    }
    
    public void CompareRotationSystems()
    {
        if (CurrentFrame == null) return;
        
        // Test both rotation systems
        var matrix3Rotation = BlazorThreeJS.Maths.Matrix3.NewMatrix().RotateEuler(90, 0, 0);
        var transform3Rotation = new BlazorThreeJS.Maths.Transform3().RotateEuler(90, 0, 0).ToMatrix3();
        
        var testPoint = new BlazorThreeJS.Maths.Vector3(0, 1, 0);
        
        var matrix3Result = matrix3Rotation.TransformPoint(testPoint);
        var transform3Result = transform3Rotation.TransformPoint(testPoint);
        
        StatusMessage = $"Matrix3: (0,1,0) -> ({matrix3Result.X:F3},{matrix3Result.Y:F3},{matrix3Result.Z:F3})\n" +
                       $"Transform3: (0,1,0) -> ({transform3Result.X:F3},{transform3Result.Y:F3},{transform3Result.Z:F3})\n" +
                       $"Expected: (0,0,1) for 90° X rotation";
        StateHasChanged();
    }

    public void Dispose() { }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false,null!);

            scene?.SetAfterUpdateAction((s,j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = FoundryService.Arena();
            if (found)
            {
                arena.SetScene(scene!);
                CreateSpacialFrame();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
