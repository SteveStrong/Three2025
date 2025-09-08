
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;
using FoundryRulesAndUnits.Extensions;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Objects;
using Microsoft.AspNetCore.Components;
using BlazorThreeJS.Maths;
using FoundryBlazor.Shared;

namespace Three2025.Components.Pages;

public partial class MatrixOrientationTest : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }

    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
    protected FoShape3D CurrentShape;
    protected SpacialFrame3D CurrentFrame;

    // Box properties for UI binding
    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.5;
    protected double BoxDepth { get; set; } = 1.0;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    public double[] CurrentMatrix { get; set; }
    public List<double[]> PresetMatrices { get; set; } = new List<double[]>();
    public int CurrentPresetIndex { get; set; } = -1;

    protected string StatusMessage { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        PresetMatrices = new List<double[]>
        {
            new double[] { 1,0,0,0, 0,0,1,0, 0,-1,0,0, 0,0,0,1 }, // X 90
            new double[] { 0,0,-1,0, 0,1,0,0, 1,0,0,0, 0,0,0,1 }, // Y 90
            new double[] { 0,1,0,0, -1,0,0,0, 0,0,1,0, 0,0,0,1 }, // Z 90
            new Transform3("CombinedXYZ") { Rotation = Euler.FromDegrees(90,90,90) }.ToMatrix3().Elements
        };
        ApplyPreset(0);
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

    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
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



            CurrentShape = new FoShape3D()
            {
                Name = "SourceShape",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "#FFB050",
                Opacity = 0.8,
                Transform = new Transform3("BoxTransform")
                {
                    // Update the transform properties - this will automatically trigger refresh via OnChange
                    //Position = new Vector3(PositionX, PositionY, PositionZ),
                    //Pivot = new Vector3(PivotX, PivotY, PivotZ),
                    //Rotation = new Euler(RotationX, RotationY, RotationZ, AngleUnit.Degrees),
                    OnChange = (isDirty) =>
                    {
                        $"Transform OnChange fired. isDirty={isDirty}".WriteInfo(1);
                        if (isDirty)
                        {
                            //it is likely this fires many times as the transform is marked dirty
                            //with every small change  there migbt be some value in debouncing this
                            //or haveing a function the applys all the changes at once 
                            // Immediately start UI updates when transform becomes dirty
                            StatusMessage = "🔄 Transform updating...";
                            AutoRefreshShape();
                            StateHasChanged();
                        }
                    },
                    OnComputed = (matrix) =>
                    {
                        $"Transform OnComputed fired. Matrix is now ready.".WriteInfo(1);
                        // Matrix is ready - safe to refresh the shape
                        AutoRefreshShape();
                        StatusMessage = "✅ Transform computed and shape refreshed";
                        StateHasChanged();
                    }
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

    public void AutoRefreshShape()
    {
        $"🔄 AutoRefreshShape called".WriteInfo();
        CreateSpacialFrame();
    }  

    public void ApplyPreset(int index)
    {
        if (index >= 0 && index < PresetMatrices.Count)
        {
            CurrentPresetIndex = index;
            CurrentMatrix = PresetMatrices[index];
            CreateTestBox();
            StateHasChanged();
        }
    }

    public async Task RunAllPresets()
    {
        for (int i = 0; i < PresetMatrices.Count; i++)
        {
            ApplyPreset(i);
            await Task.Delay(800);
        }
    }

    public void CreateTestBox()
    {
        var arena = FoundryService.Arena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }
        arena.ClearArena();
        // For demonstration, use preset angles for rotation
        double rx = 0, ry = 0, rz = 0;
        if (CurrentPresetIndex == 0) { rx = 90; ry = 0; rz = 0; }
        else if (CurrentPresetIndex == 1) { rx = 0; ry = 90; rz = 0; }
        else if (CurrentPresetIndex == 2) { rx = 0; ry = 0; rz = 90; }
        else if (CurrentPresetIndex == 3) { rx = 90; ry = 90; rz = 90; }
        CurrentShape = new FoShape3D()
        {
            Name = "TestBox",
            GlyphId = Guid.NewGuid().ToString(),
            Color = "#80B0FF",
            Opacity = 0.8,
            Transform = new Transform3("BoxTransform")
            {
                Rotation = Euler.FromDegrees(rx, ry, rz)
            }
        }.CreateBox("TestBox", 2.0, 1.5, 1.0);
        arena.AddShapeToStage<FoShape3D>(CurrentShape);
        CurrentFrame = new SpacialFrame3D(CurrentShape, "m");
        StatusMessage = $"Created TestBox with preset index {CurrentPresetIndex}";
        StateHasChanged();
    }

    public void ShowTransformMatrix()
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

    public void Dispose()
    {
        // Cleanup resources if needed
    }
}

