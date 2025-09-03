using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Geometires;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Three2025.Shared;
using FoundryBlazor.Solutions;
using BlazorThreeJS.Core;
using FoundryBlazor.Shared;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;

namespace Three2025.Components.Pages;

public class SpacialBoxTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
    protected SpacialBox3D CurrentBox;

    // Box properties for UI binding
    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.5;
    protected double BoxDepth { get; set; } = 1.0;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = string.Empty;



    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
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
                CreateSpacialBox();
            }
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    public void DoRequestAxisToScene(Scene3D scene)
    {
        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        scene.AddChild(model);
    }

    public void CreateSpacialBox()
    {
        try
        {
            CurrentBox = new SpacialBox3D(BoxWidth, BoxHeight, BoxDepth, "m");

            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            arena.ClearArena();

            var boxShape = new FoShape3D()
                .CreateBox("SpacialBoxMain", BoxWidth, BoxHeight, BoxDepth);

            boxShape.Color = "#4CAF50";
            boxShape.Opacity = 0.8;
            boxShape.Transform.Position = new BlazorThreeJS.Maths.Vector3(CurrentBox.Center.X - BoxWidth / 2, CurrentBox.Center.Y - BoxHeight / 2, CurrentBox.Center.Z - BoxDepth / 2);

            arena.AddShapeToStage<FoShape3D>(boxShape);

            StatusMessage = $"Created SpacialBox3D (FoShape3D): {BoxWidth}×{BoxHeight}×{BoxDepth}m";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating box: {ex.Message}";
            StateHasChanged();
        }
    }

    // Removed: CreateBoxMesh. All box creation now uses FoShape3D and Arena.

    public void ShowBoxInfo()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var info = $"Box Info:\n" +
                  $"Volume: {CurrentBox.Volume:F2} {CurrentBox.Units}³\n" +
                  $"Surface Area: {CurrentBox.SurfaceArea:F2} {CurrentBox.Units}²\n" +
                  $"Center: ({CurrentBox.Center.X:F2}, {CurrentBox.Center.Y:F2}, {CurrentBox.Center.Z:F2})";

        StatusMessage = info;
        StateHasChanged();
    }






    public void ClearAll()
    {
        var arena = Workspace?.GetArena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }


        arena.ClearArena();
        StateHasChanged();
    }



    public void AddAxisToScene(Scene3D scene)
    {
        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        Task.Run(async () => await scene.Request3DModel(model, async (uuid) =>
        {
            var group = new Group3D()
            {
                Name = "Axis",
                Uuid = uuid,
            };
            scene.AddChild(group);
            StatusMessage = "Axis added to scene";
            StateHasChanged();
            await Task.CompletedTask;
        }));
    }

    public void Dispose()
    {
        // Cleanup resources if needed
    }

    // === PRESET SHAPES ===
    protected void CreateCube()
    {
        BoxWidth = BoxHeight = BoxDepth = 2.0;
        CreateSpacialBox();
    }

    protected void CreateLongBox()
    {
        BoxWidth = 4.0; BoxHeight = 1.0; BoxDepth = 1.0;
        CreateSpacialBox();
    }

    protected void CreateTallBox()
    {
        BoxWidth = 1.0; BoxHeight = 4.0; BoxDepth = 1.0;
        CreateSpacialBox();
    }

    protected void CreateWideBox()
    {
        BoxWidth = 1.0; BoxHeight = 1.0; BoxDepth = 4.0;
        CreateSpacialBox();
    }

    protected void CreateTinyBox()
    {
        BoxWidth = BoxHeight = BoxDepth = 0.5;
        CreateSpacialBox();
    }





    public void ShowDiagonals()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        // Show main diagonals connecting opposite corners
        var diagonalPairs = new[]
        {
            (CurrentBox.LeftTopFront, CurrentBox.RightBottomBack),
            (CurrentBox.RightTopFront, CurrentBox.LeftBottomBack),
            (CurrentBox.LeftBottomFront, CurrentBox.RightTopBack),
            (CurrentBox.RightBottomFront, CurrentBox.LeftTopBack)
        };

        int index = 0;
        foreach (var (start, end) in diagonalPairs)
        {
            // Create small spheres at diagonal endpoints
            CreateMarkerSphere($"Diagonal{index}Start", start, "#E91E63", 0.03);
            CreateMarkerSphere($"Diagonal{index}End", end, "#E91E63", 0.03);
            index++;
        }

        StatusMessage = $"Showing {diagonalPairs.Length} main diagonals";
        StateHasChanged();
    }

    public void ShowCenterCross()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }



        var center = CurrentBox.Center;
        var halfW = CurrentBox.Width / 2;
        var halfH = CurrentBox.Height / 2;
        var halfD = CurrentBox.Depth / 2;

        // Create cross points extending from center
        var crossPoints = new[]
        {
            new Point3D(center.X + halfW, center.Y, center.Z), // Right
            new Point3D(center.X - halfW, center.Y, center.Z), // Left
            new Point3D(center.X, center.Y + halfH, center.Z), // Top
            new Point3D(center.X, center.Y - halfH, center.Z), // Bottom
            new Point3D(center.X, center.Y, center.Z + halfD), // Front
            new Point3D(center.X, center.Y, center.Z - halfD)  // Back
        };

        CreateMarkerSphere("CenterPoint", center, "#FF0000", 0.08);

        for (int i = 0; i < crossPoints.Length; i++)
        {
            CreateMarkerSphere($"CrossPoint{i}", crossPoints[i], "#FF5722", 0.05);
        }

        StatusMessage = "Showing center cross with 6 directional points";
        StateHasChanged();
    }

    public void ShowQuadrants()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }



        var center = CurrentBox.Center;
        var quadrantSize = 0.2;
        var colors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FFA500", "#800080" };

        // Create 8 quadrant markers (for a 3D box)
        var quadrants = new[]
        {
            new Point3D(center.X + quadrantSize, center.Y + quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y + quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y - quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y - quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y + quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y + quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y - quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y - quadrantSize, center.Z - quadrantSize)
        };

        for (int i = 0; i < quadrants.Length; i++)
        {
            CreateMarkerSphere($"Quadrant{i}", quadrants[i], colors[i], 0.04);
        }

        StatusMessage = "Showing 8 3D quadrants around center";
        StateHasChanged();
    }

 


    // === MEASUREMENT TOOLS ===
    public void MeasureDistances()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var diagonal = Math.Sqrt(Math.Pow(CurrentBox.Width, 2) + Math.Pow(CurrentBox.Height, 2) + Math.Pow(CurrentBox.Depth, 2));
        var faceDiagonal1 = Math.Sqrt(Math.Pow(CurrentBox.Width, 2) + Math.Pow(CurrentBox.Height, 2));
        var faceDiagonal2 = Math.Sqrt(Math.Pow(CurrentBox.Height, 2) + Math.Pow(CurrentBox.Depth, 2));
        var faceDiagonal3 = Math.Sqrt(Math.Pow(CurrentBox.Width, 2) + Math.Pow(CurrentBox.Depth, 2));

        StatusMessage = $"Distances - Main diagonal: {diagonal:F2}, Face diagonals: {faceDiagonal1:F2}, {faceDiagonal2:F2}, {faceDiagonal3:F2}";
        StateHasChanged();
    }

        // === VISUALIZATION TEST METHODS ===
        public void ShowVertices()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            VisualizationService.ShowLabeledVertices(arena, CurrentBox.Vertices);
            StatusMessage = $"Showing {CurrentBox.Vertices.Count} vertices as labeled spheres.";
            StateHasChanged();
        }

        public void ShowEdges()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            var edges = CurrentBox.GetEdgesWithNames();
            VisualizationService.ShowLabeledEdges(arena, edges);
            StatusMessage = $"Showing {edges.Count} edges as labeled cylinders.";
            StateHasChanged();
        }

        public void ShowFaces()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var faces = CurrentBox.GetFacesWithNormals();
            VisualizationService.ShowWireframeFaces(arena, faces);
            StatusMessage = $"Showing {faces.Count} faces as wireframe outlines with labels.";
            StateHasChanged();
        }

        public void ShowNormals()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var faces = CurrentBox.GetFacesWithNormals();
            VisualizationService.ShowLabeledNormals(arena, faces);
            StatusMessage = $"Showing {faces.Count} face normals as red cylinders, aligned with normals.";
            StateHasChanged();
        }

    public void ShowDimensions()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }


        // Create dimension indicators (simplified - would need line drawing capability)
        var center = CurrentBox.Center;
        CreateMarkerSphere("DimCenter", center, "#FFFF00", 0.05);

        // Show dimension endpoints
        CreateMarkerSphere("WidthEnd1", new Point3D(center.X - CurrentBox.Width / 2, center.Y, center.Z), "#FF0000", 0.03);
        CreateMarkerSphere("WidthEnd2", new Point3D(center.X + CurrentBox.Width / 2, center.Y, center.Z), "#FF0000", 0.03);

        StatusMessage = $"Dimensions: W={CurrentBox.Width:F2}, H={CurrentBox.Height:F2}, D={CurrentBox.Depth:F2}";
        StateHasChanged();
    }

    public void CalculateAngles()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        // Calculate angles between diagonals and faces
        var widthAngle = Math.Atan(CurrentBox.Height / CurrentBox.Width) * 180 / Math.PI;
        var heightAngle = Math.Atan(CurrentBox.Width / CurrentBox.Height) * 180 / Math.PI;
        var depthAngle = Math.Atan(CurrentBox.Depth / CurrentBox.Width) * 180 / Math.PI;

        StatusMessage = $"Angles - Width/Height: {widthAngle:F1}°, Height/Width: {heightAngle:F1}°, Depth/Width: {depthAngle:F1}°";
        StateHasChanged();
    }

    public void ShowVolume3D()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var volume = CurrentBox.Volume;
        var surfaceArea = CurrentBox.SurfaceArea;
        var ratio = surfaceArea / volume;

        StatusMessage = $"3D Metrics - Volume: {volume:F2} {CurrentBox.Units}³, Surface Area: {surfaceArea:F2} {CurrentBox.Units}², SA/V Ratio: {ratio:F2}";
        StateHasChanged();
    }

    // === UTILITY METHODS ===
    public void ResetView()
    {
        StatusMessage = "View reset (would require camera controls)";
        StateHasChanged();
    }

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

    public void ShowAllDetailed()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }
        var arena = Workspace?.GetArena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }
        
        var vertices = CurrentBox.Vertices;
        var edges = CurrentBox.GetEdgesWithNames();
        var faces = CurrentBox.GetFacesWithNormals();
        
        VisualizationService.ShowAll(arena, vertices, edges, faces);
        StatusMessage = $"Showing comprehensive view: {vertices.Count} vertices, {edges.Count} edges, {faces.Count} faces with labels and normals";
        StateHasChanged();
    }
}
