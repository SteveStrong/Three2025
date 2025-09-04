using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Three2025.Shared;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;
using Three2025.Apprentice;

namespace Three2025.Components.Pages;

public class NormalVisualizationTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
    
    // Test components
    protected FoShape3D TestBox;
    protected List<FoShape3D> NormalVisualizations = new();
    protected List<FoShape3D> FaceVisualizations = new();
    
    // Component properties
    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.0;
    protected double BoxDepth { get; set; } = 1.0;
    
    // Position
    protected double PosX { get; set; } = 0.0;
    protected double PosY { get; set; } = 0.0;
    protected double PosZ { get; set; } = 0.0;
    
    // Rotation (in degrees for UI)
    protected double RotX { get; set; } = 0.0;
    protected double RotY { get; set; } = 0.0;
    protected double RotZ { get; set; } = 0.0;
    
    // Visualization options
    protected bool ShowNormals { get; set; } = true;
    protected bool ShowFaces { get; set; } = false;
    protected double NormalLength { get; set; } = 1.0;

    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;

    protected string StatusMessage { get; set; } = "Ready to test universal geometry face normal visualization.";

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(100); // Allow workspace to initialize
    }

    public void CreateTestBox()
    {
        try
        {
            // Clear any existing components
            ClearAll();
            
            // Create FoShape3D box directly (universal geometry approach)
            TestBox = new FoShape3D();
            TestBox.CreateBox("TestBox", BoxWidth, BoxHeight, BoxDepth);
            TestBox.Transform.Position = new Vector3(PosX, PosY, PosZ);
            TestBox.Transform.Rotation = new Euler(
                RotX * Math.PI / 180.0,  // Convert to radians
                RotY * Math.PI / 180.0,
                RotZ * Math.PI / 180.0
            );

            // Add to arena
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            arena.AddShapeToStage(TestBox);
            
            // Get face count using universal engine
            var faces = UniversalSnapEngine.GetAllFaces(TestBox);
            
            // Show normals if enabled
            if (ShowNormals)
            {
                CreateNormalVisualizations();
            }
            
            // Show faces if enabled
            if (ShowFaces)
            {
                CreateFaceVisualizations();
            }
            
            StatusMessage = $"✅ Created test box {BoxWidth}×{BoxHeight}×{BoxDepth} with {faces.Count} faces";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error creating test box: {ex.Message}";
            StateHasChanged();
        }
    }

    public void UpdatePosition()
    {
        if (TestBox == null) return;
        
        TestBox.Transform.Position = new Vector3(PosX, PosY, PosZ);
        RefreshScene();
        UpdateVisualizations();
        
        StatusMessage = $"Updated position to ({PosX:F1}, {PosY:F1}, {PosZ:F1})";
        StateHasChanged();
    }

    public void UpdateRotation()
    {
        if (TestBox == null) return;
        
        TestBox.Transform.Rotation = new Euler(
            RotX * Math.PI / 180.0,  // Convert to radians
            RotY * Math.PI / 180.0,
            RotZ * Math.PI / 180.0
        );
        RefreshScene();
        UpdateVisualizations();
        
        StatusMessage = $"Updated rotation to ({RotX}°, {RotY}°, {RotZ}°)";
        StateHasChanged();
    }

    private void CreateNormalVisualizations()
    {
        if (TestBox == null) return;
        
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        // Clear existing normal visualizations
        foreach (var normal in NormalVisualizations)
        {
            arena.RemoveShapeFromStage(normal);
        }
        NormalVisualizations.Clear();
        
        // Get all faces using universal engine
        var faces = UniversalSnapEngine.GetAllFaces(TestBox);
        
        // Create a normal visualization for each face
        foreach (var face in faces)
        {
            try
            {
                // Calculate normal visualization manually since we're using Face3D directly
                var faceCenter = face.Center;
                var faceNormal = face.Normal;
                var normalEnd = new Vector3(
                    faceCenter.X + faceNormal.X * NormalLength,
                    faceCenter.Y + faceNormal.Y * NormalLength,
                    faceCenter.Z + faceNormal.Z * NormalLength
                );
                
                // Calculate orientation for the cylinder (pointing from center to end)
                var direction = new Vector3(faceNormal.X, faceNormal.Y, faceNormal.Z);
                var eulerRotation = new Vector3(
                    Math.Atan2(direction.Y, direction.Z),
                    -Math.Atan2(direction.X, Math.Sqrt(direction.Y * direction.Y + direction.Z * direction.Z)),
                    0
                );
                
                // Use the visualization service to create the normal cylinder
                var normalViz = VisualizationService.CreateMarkerCylinder(arena, $"Normal_{face.Name}", 
                    faceCenter, eulerRotation, "yellow", 0.05, NormalLength);
                
                NormalVisualizations.Add(normalViz);
                
                StatusMessage += $" Normal {face.Name}: center({face.Center.X:F2},{face.Center.Y:F2},{face.Center.Z:F2}) normal({faceNormal.X:F2},{faceNormal.Y:F2},{faceNormal.Z:F2})";
            }
            catch (Exception ex)
            {
                StatusMessage += $" ❌ Error creating normal for {face.Name}: {ex.Message}";
            }
        }
    }

    private void CreateFaceVisualizations()
    {
        if (TestBox == null) return;
        
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        // Clear existing face visualizations
        foreach (var faceViz in FaceVisualizations)
        {
            arena.RemoveShapeFromStage(faceViz);
        }
        FaceVisualizations.Clear();
        
        // Get all faces using universal engine
        var faces = UniversalSnapEngine.GetAllFaces(TestBox);
        
        // Create a semi-transparent plane for each face
        foreach (var face in faces)
        {
            try
            {
                // Calculate face visualization transform manually
                var faceCenter = face.Center;
                var faceNormal = face.Normal;
                
                // Calculate orientation for the face plane based on normal
                var euler = new Euler(
                    Math.Atan2(faceNormal.Y, faceNormal.Z),
                    -Math.Atan2(faceNormal.X, Math.Sqrt(faceNormal.Y * faceNormal.Y + faceNormal.Z * faceNormal.Z)),
                    0
                );
                
                // Use the visualization service to create the face plane
                var faceViz = VisualizationService.CreateMarkerPlane(arena, $"Face_{face.Name}", 
                    faceCenter, new Vector3(euler.X, euler.Y, euler.Z), GetFaceColor(face.Name), 
                    face.Width, face.Height, 0.01, 0.3);
                
                FaceVisualizations.Add(faceViz);
            }
            catch (Exception ex)
            {
                StatusMessage += $" ❌ Error creating face visualization for {face.Name}: {ex.Message}";
            }
        }
    }

    private string GetFaceColor(string faceName)
    {
        return faceName switch
        {
            "Front" => "red",
            "Back" => "green", 
            "Left" => "blue",
            "Right" => "yellow",
            "Top" => "purple",
            "Bottom" => "orange",
            _ => "gray"
        };
    }

    private void UpdateVisualizations()
    {
        if (ShowNormals)
        {
            CreateNormalVisualizations();
        }
        
        if (ShowFaces)
        {
            CreateFaceVisualizations();
        }
    }

    private void RefreshScene()
    {
        var arena = Workspace?.GetArena();
        var (found, scene) = arena?.CurrentScene() ?? (false, null);
        if (found && scene != null && TestBox != null)
        {
            TestBox.RefreshToScene(scene);
        }
    }

    // Event handlers
    public async Task ToggleNormals()
    {
        if (ShowNormals)
        {
            CreateNormalVisualizations();
        }
        else
        {
            // Hide normals
            var arena = Workspace?.GetArena();
            foreach (var normal in NormalVisualizations)
            {
                arena?.RemoveShapeFromStage(normal);
            }
            NormalVisualizations.Clear();
        }
        await InvokeAsync(StateHasChanged);
    }

    public async Task ToggleFaces()
    {
        if (ShowFaces)
        {
            CreateFaceVisualizations();
        }
        else
        {
            // Hide faces
            var arena = Workspace?.GetArena();
            foreach (var face in FaceVisualizations)
            {
                arena?.RemoveShapeFromStage(face);
            }
            FaceVisualizations.Clear();
        }
        await InvokeAsync(StateHasChanged);
    }

    public async Task UpdateNormals()
    {
        if (ShowNormals)
        {
            CreateNormalVisualizations();
        }
        await InvokeAsync(StateHasChanged);
    }

    // Quick test methods
    public void TestNoRotation()
    {
        RotX = RotY = RotZ = 0;
        if (TestBox != null)
        {
            UpdateRotation();
        }
    }

    public void Test45Degree()
    {
        RotX = 0; RotY = 45; RotZ = 0;
        if (TestBox != null)
        {
            UpdateRotation();
        }
    }

    public void Test90Degree()
    {
        RotX = 0; RotY = 90; RotZ = 0;
        if (TestBox != null)
        {
            UpdateRotation();
        }
    }

    public void ClearAll()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        // Remove test box
        if (TestBox != null)
        {
            arena.RemoveShapeFromStage(TestBox);
            TestBox = null;
        }
        
        // Remove all visualizations
        foreach (var normal in NormalVisualizations)
        {
            arena.RemoveShapeFromStage(normal);
        }
        NormalVisualizations.Clear();
        
        foreach (var face in FaceVisualizations)
        {
            arena.RemoveShapeFromStage(face);
        }
        FaceVisualizations.Clear();
        
        StatusMessage = "Cleared all components.";
        StateHasChanged();
    }

    public void Dispose()
    {
        ClearAll();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);

            scene?.SetAfterUpdateAction((s, j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = Workspace.GetArena();
            if (found)
            {
                arena.SetScene(scene!);
                // Create initial test box for demonstration
                CreateTestBox();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
