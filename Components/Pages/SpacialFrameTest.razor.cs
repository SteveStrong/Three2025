using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;
using FoundryRulesAndUnits.Extensions;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Maths;

namespace Three2025.Components.Pages;

public partial class SpacialFrameTest : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
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
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false,null!);

            scene?.SetAfterUpdateAction((s,j) =>
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
                Name = "SpacialFrameMain",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "#FFB050",
                Opacity = 0.8,
                Transform = new Transform3()
                {
                    Position = new Vector3(0, 0, 0),
                    Pivot = new Vector3(0, 0, 0),
                    Rotation = new Euler(0, 0, 0),
                    Scale = new Vector3(1, 1, 1)
                }
            }.CreateBox("SpacialFrameMain", BoxWidth, BoxHeight, BoxDepth);

            // Set up automatic refresh when transform changes
            CurrentShape.Transform.OnChange = (isDirty) =>
            {
                if (isDirty)
                {
                    // Automatically refresh the shape when transform changes
                    AutoRefreshShape();
                }
            };

            arena.AddShapeToStage<FoShape3D>(CurrentShape);

            CurrentFrame = new SpacialFrame3D(CurrentShape, "m");

            // Load the current transform values into the UI controls
            LoadCurrentTransform();

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
    protected void UpdateTransform()
    {
        if (CurrentFrame?.Source == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        try
        {
            var transform = CurrentFrame.Source.Transform;
            var rotX = RotationX * (Math.PI / 180.0);
            var rotY = RotationY * (Math.PI / 180.0);
            var rotZ = RotationZ * (Math.PI / 180.0);
            // Update the transform properties - this will automatically trigger refresh via OnChange
            transform.Position = new Vector3(PositionX, PositionY, PositionZ);
            transform.Pivot = new Vector3(PivotX, PivotY, PivotZ);
            transform.Rotation = new Euler(rotX, rotY, rotZ);
            transform.Scale = new Vector3(ScaleX, ScaleY, ScaleZ);

            StatusMessage = $"Transform updated: Pos({PositionX:F2},{PositionY:F2},{PositionZ:F2}) " +
                          $"Rot({RotationX:F1}°,{RotationY:F1}°,{RotationZ:F1}°) " +
                          $"Scale({ScaleX:F2},{ScaleY:F2},{ScaleZ:F2}) " +
                          $"[Degrees auto-converted to radians for matrix calculations]";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating transform: {ex.Message}";
            StateHasChanged();
        }
    }

    protected void ResetTransform()
    {
        PositionX = PositionY = PositionZ = 0.0;
        PivotX = PivotY = PivotZ = 0.0;
        RotationX = RotationY = RotationZ = 0.0;
        ScaleX = ScaleY = ScaleZ = 1.0;
        UpdateTransform();
    }

    protected void LoadCurrentTransform()
    {
        if (CurrentFrame?.Source?.Transform == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var transform = CurrentFrame.Source.Transform;
        PositionX = transform.Position.X;
        PositionY = transform.Position.Y;
        PositionZ = transform.Position.Z;
        
        PivotX = transform.Pivot.X;
        PivotY = transform.Pivot.Y;
        PivotZ = transform.Pivot.Z;
        
        RotationX = transform.Rotation.X * (180.0 / Math.PI); // Convert to degrees
        RotationY = transform.Rotation.Y * (180.0 / Math.PI);
        RotationZ = transform.Rotation.Z * (180.0 / Math.PI);

        ScaleX = transform.Scale.X;
        ScaleY = transform.Scale.Y;
        ScaleZ = transform.Scale.Z;
        
        StatusMessage = "Current transform values loaded into controls.";
        StateHasChanged();
    }

    // === PRESET TRANSFORMATIONS ===
    protected void ApplyQuickRotationX90()
    {
        RotationX += 90;
        if (RotationX >= 360) RotationX -= 360;
        UpdateTransform();
    }

    protected void ApplyQuickRotationY90()
    {
        RotationY += 90;
        if (RotationY >= 360) RotationY -= 360;
        UpdateTransform();
    }

    protected void ApplyQuickRotationZ90()
    {
        RotationZ += 90;
        if (RotationZ >= 360) RotationZ -= 360;
        UpdateTransform();
    }

    protected void ApplyRandomTransform()
    {
        var random = new Random();
        PositionX = (random.NextDouble() - 0.5) * 4.0; // -2 to 2
        PositionY = (random.NextDouble() - 0.5) * 4.0;
        PositionZ = (random.NextDouble() - 0.5) * 4.0;

        RotationX = random.NextDouble() * 360;
        RotationY = random.NextDouble() * 360;
        RotationZ = random.NextDouble() * 360;
        
        ScaleX = 0.5 + random.NextDouble() * 1.5; // 0.5 to 2.0
        ScaleY = 0.5 + random.NextDouble() * 1.5;
        ScaleZ = 0.5 + random.NextDouble() * 1.5;
        
        UpdateTransform();
    }

    protected void AutoRefreshShape()
    {
        if (CurrentFrame?.Source == null)
            return;

        try
        {
            // Mark the shape as dirty so it will be refreshed
            CurrentFrame.Source.SetDirty(true);

            // Get the scene and refresh the shape efficiently
            var arena = FoundryService.Arena();
            if (arena != null)
            {
                var (found, scene) = arena.CurrentScene();
                if (found)
                {
                    // Use the efficient refresh mechanism
                    CurrentFrame.Source.RefreshToScene(scene);
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error auto-refreshing shape: {ex.Message}";
            StateHasChanged();
        }
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
