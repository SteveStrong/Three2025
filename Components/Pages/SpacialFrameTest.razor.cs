using FoundryWorldsAndDrawings.Shape;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.PubSub;
using Three2025.Services.Visualization;
using FoundryRulesAndUnits.Extensions;

using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Components.Pages;

public partial class SpacialFrameTest : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;
    protected SpacialFrame3D CurrentFrame;
    protected FoShape3D CurrentShape;

    // Box properties for UI binding
    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.5;
    protected double BoxDepth { get; set; } = 1.0;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = string.Empty;

    // Transform properties for UI binding
    protected double PositionX { get; set; } = 0.0;
    protected double PositionY { get; set; } = 0.0;
    protected double PositionZ { get; set; } = 0.0;

    protected double PivotX { get; set; } = 0.0;
    protected double PivotY { get; set; } = 0.0;
    protected double PivotZ { get; set; } = 0.0;

    protected double RotationX { get; set; } = 0.0;
    protected double RotationY { get; set; } = 0.0;
    protected double RotationZ { get; set; } = 0.0;

    protected double ScaleX { get; set; } = 1.0;
    protected double ScaleY { get; set; } = 1.0;
    protected double ScaleZ { get; set; } = 1.0;



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

            scene?.SetAfterUpdateAction((s, j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = FoundryService.Arena();
            if (found)
            {
                arena.SetScene(scene!);
                DoRequestAxisToScene(scene!);
                CreateSpacialFrame();
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

    public void AutoRefreshShape()
    {
        $"🔄 AutoRefreshShape called".WriteInfo();
        CreateSpacialFrame();
    }

    public void CreateSpacialFrame()
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

            $"Creating SpacialFrame3D with Rotation {RotationX}×{RotationY}×{RotationZ}° at Position ({PositionX}, {PositionY}, {PositionZ}), Pivot ({PivotX}, {PivotY}, {PivotZ}), Scale ({ScaleX}, {ScaleY}, {ScaleZ})".WriteInfo();


            CurrentShape = new FoShape3D()
            {
                Name = "SourceShape",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "#FFB050",
                Opacity = 0.8,
                Transform = new Transform3("BoxTransform")
                {
                    // Update the transform properties - this will automatically trigger refresh via OnChange
                    Position = new Vector3(PositionX, PositionY, PositionZ),
                    Pivot = new Vector3(PivotX, PivotY, PivotZ),
                    Rotation = new Euler(RotationX, RotationY, RotationZ, AngleUnit.Degrees),
                    Scale = new Vector3(ScaleX, ScaleY, ScaleZ),

                    // OnComputed = (matrix) =>
                    // {
                    //     $"Transform OnComputed fired. Matrix is now ready.".WriteInfo(1);
                    //     // Matrix is ready - safe to refresh the shape
                    //     AutoRefreshShape();
                    //     StatusMessage = "✅ Transform computed and shape refreshed";
                    //     StateHasChanged();
                    // }
                }
            }.CreateBox("SourceShape", BoxWidth, BoxHeight, BoxDepth);

            arena.AddShapeToStage<FoShape3D>(CurrentShape);

            CurrentFrame = new SpacialFrame3D(CurrentShape, "m");

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





    public void Dispose()
    {
        // Cleanup resources if needed
    }

    // === PRESET SHAPES ===
    protected void CreateCube()
    {
        BoxWidth = BoxHeight = BoxDepth = 2.0;
        CreateSpacialFrame();
    }

    protected void CreateLongBox()
    {
        BoxWidth = 4.0; BoxHeight = 1.0; BoxDepth = 1.0;
        CreateSpacialFrame();
    }

    protected void CreateTallBox()
    {
        BoxWidth = 1.0; BoxHeight = 4.0; BoxDepth = 1.0;
        CreateSpacialFrame();
    }

    protected void CreateWideBox()
    {
        BoxWidth = 1.0; BoxHeight = 1.0; BoxDepth = 4.0;
        CreateSpacialFrame();
    }

    protected void CreateTinyBox()
    {
        BoxWidth = BoxHeight = BoxDepth = 0.5;
        CreateSpacialFrame();
    }

    // === TRANSFORMATION METHODS ===

    protected void ResetTransform()
    {
        PositionX = PositionY = PositionZ = 0.0;
        PivotX = PivotY = PivotZ = 0.0;
        RotationX = RotationY = RotationZ = 0.0;
        ScaleX = ScaleY = ScaleZ = 1.0;
        CreateSpacialFrame();
    }



    // === PRESET TRANSFORMATIONS ===
    protected void ApplyQuickRotationX90()
    {
        RotationX += 90;
        if (RotationX >= 360) RotationX -= 360;
        CreateSpacialFrame();
    }

    protected void ApplyQuickRotationY90()
    {
        RotationY += 90;
        if (RotationY >= 360) RotationY -= 360;
        CreateSpacialFrame();
    }

    protected void ApplyQuickRotationZ90()
    {
        RotationZ += 90;
        if (RotationZ >= 360) RotationZ -= 360;
        CreateSpacialFrame();
    }

    // === POSITION INCREMENT METHODS ===
    protected void IncrementPositionX()
    {
        PositionX += 1.0;
        CreateSpacialFrame();
    }

    protected void IncrementPositionY()
    {
        PositionY += 1.0;
        CreateSpacialFrame();
    }

    protected void IncrementPositionZ()
    {
        PositionZ += 1.0;
        CreateSpacialFrame();
    }

    // === PIVOT INCREMENT METHODS ===
    protected void IncrementPivotX()
    {
        PivotX += 1.0;
        CreateSpacialFrame();
    }

    protected void IncrementPivotY()
    {
        PivotY += 1.0;
        CreateSpacialFrame();
    }

    protected void IncrementPivotZ()
    {
        PivotZ += 1.0;
        CreateSpacialFrame();
    }



    protected void ShowTransformMatrix()
    {
        if (CurrentFrame?.Source?.Transform == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        try
        {
            var matrix = CurrentFrame.Source.Transform.ToMatrix3();
            var matrixString = matrix.ToStringFormatted();
            StatusMessage = $"Transform Matrix:\n{matrixString}";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error getting matrix: {ex.Message}";
            StateHasChanged();
        }
    }



    public void ShowQuadrants()
    {
        if (CurrentFrame == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var arena = FoundryService.Arena();
        if (arena == null) return;

        var center = CurrentFrame.Center;
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
        if (CurrentFrame == null)
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

        $"Expect this vertices to be transformed correctly. ".WriteInfo();
        var vertices = CurrentFrame.GetVertices();
        VisualizationService.ShowLabeledVertices(arena, vertices);
        StatusMessage = $"Showing {vertices.Count} vertices as labeled spheres.";
        StateHasChanged();
    }

    public void ShowEdges()
    {
        if (CurrentFrame == null)
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

        var edges = CurrentFrame.GetEdges();
        VisualizationService.ShowLabeledEdges(arena, edges);
        StatusMessage = $"Showing {edges.Count} edges as labeled tubes.";
        StateHasChanged();
    }

    public void ShowFaces()
    {
        if (CurrentFrame == null)
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

        var faces = CurrentFrame.GetFaces();
        VisualizationService.ShowLabeledFaces(arena, faces);
        StatusMessage = $"Showing {faces.Count} faces as wireframe outlines with labels.";
        StateHasChanged();
    }

    public void ShowNormals()
    {
        if (CurrentFrame == null)
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

        var faces = CurrentFrame.GetFaces();
        VisualizationService.ShowLabeledNormals(arena, faces);
        StatusMessage = $"Showing {faces.Count} face normals as red cylinders, aligned with normals.";
        StateHasChanged();
    }


}
