using BlazorThreeJS.Objects;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using FoundryBlazor.Solutions;
using FoundryBlazor.Shared;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;
using BlazorThreeJS.Maths;

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

            var arena = FoundryService.Arena();
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
                Name = "SpacialBoxMain",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "#4CAF50",
                Opacity = 0.8,
                Transform = new Transform3()
                {
                    Position = new Vector3(0, 0, 0)
                }
            }.CreateBox("SpacialBoxMain", BoxWidth, BoxHeight, BoxDepth);

            arena.AddShapeToStage<FoShape3D>(boxShape);

            CurrentBox = new SpacialFrame3D(boxShape, "m");

            StatusMessage = $"Created SpacialFrame3D (FoShape3D): {BoxWidth}×{BoxHeight}×{BoxDepth}m";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating box: {ex.Message}";
            StateHasChanged();
        }
    }


    public void ClearAll()
    {
        var arena = FoundryService.Arena();
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



    public void ShowQuadrants()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var arena = FoundryService.Arena();
        if (arena == null) return;

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
            VisualizationService.CreateMarkerSphere(arena, $"Quadrant{i}", quadrants[i], colors[i], 0.04);
        }

        StatusMessage = "Showing 8 3D quadrants around center";
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
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var vertices = CurrentBox.GetLocalVertices();
            VisualizationService.ShowLabeledVertices(arena, vertices);
            StatusMessage = $"Showing {vertices.Count} vertices as labeled spheres.";
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
           var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            var edges = CurrentBox.GetLocalEdges();
            VisualizationService.ShowLabeledEdges(arena, edges);
            StatusMessage = $"Showing {edges.Count} edges as labeled tubes.";
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
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var faces = CurrentBox.GetLocalFaces();
            VisualizationService.ShowLabeledFaces(arena, faces);
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
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var faces = CurrentBox.GetLocalFaces();
            VisualizationService.ShowLabeledNormals(arena, faces);
            StatusMessage = $"Showing {faces.Count} face normals as red cylinders, aligned with normals.";
            StateHasChanged();
        }



}
