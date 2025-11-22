
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.PubSub;
using Three2025.Services.Visualization;

using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Maths;


namespace Three2025.Components.Pages;

public partial class MatrixTest : ComponentBase, IDisposable
{
    // Rotation state
    protected double RotationX { get; set; } = 0;
    protected double RotationY { get; set; } = 0;
    protected double RotationZ { get; set; } = 0;

    protected string TransformedVertexInfo => GetTransformedVertexPosition();

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;
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
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);


            var arena = FoundryService.Arena();
            if (found)
            {
                arena.SetScene(scene!);
                DoRequestAxisToScene(scene!);
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

            //ok you need to remember that for spacialbox it is in a local coord system with 0,0,0 being the 
            // left , bottom, back corner
            //we should test by drawing the axis and then drawing the box
            //then we can see where the box is in relation to the axis

            var boxShape = FoRack.CreateRack("Rack", 0, 0, 10, 0);
            boxShape.Transform = new Transform3("RackTransform")
            {
                Position = new Vector3(0, 0, 0),
                Rotation = Euler.FromDegrees(0, 0, 0),
                Scale = new Vector3(1, 1, 1)
            };

            arena.AddShapeToStage<FoRack>(boxShape);

            CurrentBox = new SpacialBox3D(boxShape, "m");

            StatusMessage = $"Created SpacialBox3D (FoShape3D): {BoxWidth}×{BoxHeight}×{BoxDepth}m";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating box: {ex.Message}";
            StateHasChanged();
        }
    }

    public void ApplyRotation(double x, double y, double z)
    {
        RotationX += x;
        RotationY += y;
        RotationZ += z;

        if (CurrentBox != null)
        {
            CurrentBox.Shape.Transform.Rotation = Euler.FromDegrees(RotationX, RotationY, RotationZ);
            StatusMessage = $"Applied rotation: X={RotationX}°, Y={RotationY}°, Z={RotationZ}°";
            StateHasChanged();
        }
    }

    public void ResetRotation()
    {
        RotationX = RotationY = RotationZ = 0;
        if (CurrentBox != null)
        {
            CurrentBox.Shape.Transform.Rotation = Euler.FromDegrees(0, 0, 0);
            StatusMessage = "Rotation reset.";
            StateHasChanged();
        }
    }

    public string GetTransformedVertexPosition()
    {
        if (CurrentBox == null) return "No box";
        // Local left, top, front vertex for box at origin (left=-, top=+, front=+)
        var vertex = new double[] { BoxWidth, BoxHeight, BoxDepth, 1 }; // adjust as needed for your box definition

        var m = CurrentBox.Shape.Transform.ToMatrix3().Elements;
        var result = MultiplyMatrixAndVector(m, vertex);
        return $"Transformed vertex: ({result[0]:F2}, {result[1]:F2}, {result[2]:F2})";
    }

    private static double[] MultiplyMatrixAndVector(double[] m, double[] v)
    {
        var r = new double[4];
        for (int row = 0; row < 4; row++)
        {
            r[row] = 0;
            for (int col = 0; col < 4; col++)
            {
                r[row] += m[row * 4 + col] * v[col];
            }
        }
        return r;
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

    public void ShowSubmarineModel()
    {
        var arena = FoundryService.Arena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }

        var model = new FoModel3D()
        {
            Name = "Submarine",
        }.CreateModel("sub", GetReferenceTo(@"storage/StaticFiles/sub.glb"), 12.0, 4.5, 4.5);

        arena.AddShapeToStage<FoModel3D>(model);

        var box = new SpacialFrame3D(model, "m");

        var vertices = box.GetVertices();
        VisualizationService.ShowLabeledVertices(arena, vertices);

        var edges = box.GetEdges();
        VisualizationService.ShowLabeledEdges(arena, edges);

        var faces = box.GetFaces();
        VisualizationService.ShowLabeledFaces(arena, faces);
        VisualizationService.ShowLabeledNormals(arena, faces);

        StatusMessage = "Submarine model added to scene.";
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
