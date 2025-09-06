#nullable enable
using Microsoft.AspNetCore.Components;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Shape;
using BlazorThreeJS.Cameras;
using BlazorThreeJS.Helpers;
using BlazorThreeJS.Lights;
using System.Text;
using FoundryBlazor.Shared;
using BlazorThreeJS.Objects;
using FoundryRulesAndUnits.Extensions;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;


namespace Three2025.Components.Pages;

public partial class MatrixTransformTest : ComponentBase, IDisposable
{
    #region Dependency Injection
    [Inject] public NavigationManager Navigation { get; set; } = null!;
    [Inject] public IWorkspace Workspace { get; set; } = null!;
    [Inject] public IFoundryService FoundryService { get; init; } = null!;
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; } = null!;
    #endregion

    #region Fields and Properties
    
    private Transform3 MainTransform = new Transform3();
    private Vector3 originalPoint = new Vector3(1, 1, 0);
    private Vector3 transformedPoint = new Vector3(0, 0, 0);
    private Vector3 rotationDegrees = new Vector3(0, 0, 0);
    private string currentMatrixDisplay = "";
    
    public Canvas3DComponentBase Canvas3DReference = null!;
    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;
    
    // Matrix analysis results
    private (Vector3 Position, Vector3 Rotation, Vector3 Scale)? matrixAnalysis;
    
    // Intermediate variables for collecting changes before applying to Transform3
    private Vector3 workingPosition = new Vector3(0, 0, 0);
    private Vector3 workingScale = new Vector3(1, 1, 1);
    private Vector3 workingPivot = new Vector3(0, 0, 0);
    // rotationDegrees already serves as working variable for rotation
    

    
    #endregion

    #region Lifecycle Methods
    
    protected override void OnInitialized()
    {
        ResetTransform();
        // Set up change notification
        MainTransform.OnChange = (isDirty) => {

            InvokeAsync(() => {
                try
                {
                    RenderScene();
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in OnChange handler: {ex.Message}");
                }
            });
            
        };
        

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
            if (found && scene != null)
            {
                arena.SetScene(scene);
                RenderScene();
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
        return path;
    }
    
    #endregion

    #region Visualization Setup
    
    private void RenderScene()
    {
        try
        {
            var arena = FoundryService.Arena();
            if (arena == null) return;

            arena.ClearArena();
            (var found, var scene) = arena.CurrentScene();
            if (!found || scene == null) return;
            DoRequestAxisToScene(scene);

            var axis = VisualizationService.CreateMarkerAxis(arena, "MainTransformAxis", MainTransform);


            // Create original point sphere (blue)
            var originalPointSphere = new FoShape3D()
            {
                Name = "OriginalPoint",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "blue",
                Opacity = 0.8,
                Transform = new Transform3()
                {
                    Position = originalPoint
                }
            }.CreateSphere("OriginalPoint", 0.3, 0.3, 0.3);

            // Create transformed point sphere (green)
            var transformedPointSphere = new FoShape3D()
            {
                Name = "TransformedPoint",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "green",
                Opacity = 0.8,
                Transform = new Transform3()
                {
                    Position = transformedPoint
                }
            }.CreateSphere("TransformedPoint", 0.3, 0.3, 0.3);

            // Create pivot point marker (red - smaller and more distinctive)
            var pivotPointMarker = new FoShape3D()
            {
                Name = "PivotPoint",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "red",
                Opacity = 1.0,
                Transform = new Transform3()
                {
                    Position = workingPivot
                }
            }.CreateSphere("PivotPoint", 0.15, 0.15, 0.15); // Smaller sphere for pivot

            // Add to arena
            arena.AddShapeToStage<FoShape3D>(originalPointSphere);
            arena.AddShapeToStage<FoShape3D>(transformedPointSphere);
            arena.AddShapeToStage<FoShape3D>(pivotPointMarker);
            
            UpdateMatrixDisplay();
            
        }
        catch (Exception ex)
        {
            $"Error initializing visualization: {ex.Message}".WriteError();
        }
    }
    
    #endregion



 
    
    private void UpdateMatrixDisplay()
    {
        try
        {
            var matrix = MainTransform.ToMatrix3();
            var matrixData = matrix.GetMatrix();
            
            var sb = new StringBuilder();
            sb.AppendLine("4x4 Transformation Matrix:");
            sb.AppendLine();
            
            // Display as a proper 4x4 matrix
            for (int row = 0; row < 4; row++)
            {
                sb.Append("│ ");
                for (int col = 0; col < 4; col++)
                {
                    int index = col * 4 + row; // Column-major order
                    sb.Append($"{matrixData[index],8:F3} ");
                }
                sb.AppendLine("│");
            }
            
            // Add interpretation
            sb.AppendLine();
            sb.AppendLine("Matrix Elements:");
            sb.AppendLine($"Translation: ({matrixData[12]:F3}, {matrixData[13]:F3}, {matrixData[14]:F3})");
            sb.AppendLine($"Dirty Flag: {MainTransform.IsDirty}");
            
            currentMatrixDisplay = sb.ToString();
        }
        catch (Exception ex)
        {
            currentMatrixDisplay = $"Error displaying matrix: {ex.Message}";
        }
    }


    #region Event Handlers
    
    /// <summary>
    /// Single centralized function to update the matrix from ALL UI inputs
    /// This collects all user input values and applies them as complete vectors
    /// to properly trigger the Transform3.OnChange mechanism
    /// </summary>
    private void UpdateMatrixFromUIData()
    {
        try
        {
            // Collect all UI input values and apply them as complete vectors
            // This ensures the dirty flag mechanism works properly
            
            // 1. Apply position from working variables
            MainTransform.Position = workingPosition;
            
            // 2. Apply scale from working variables  
            MainTransform.Scale = workingScale;
            
            // 3. Apply pivot from working variables
            MainTransform.Pivot = workingPivot;
            
            // 4. Apply rotation converted from degrees to radians
            var radians = new Euler(
                rotationDegrees.X * Math.PI / 180.0,
                rotationDegrees.Y * Math.PI / 180.0,
                rotationDegrees.Z * Math.PI / 180.0
            );
            MainTransform.Rotation = radians;
            
            // The Transform3.OnChange will fire automatically and handle the rest
        }
        catch (Exception ex)
        {
            $"Error in UpdateMatrix: {ex.Message}".WriteError();
        }
    }
    

    
    #endregion



    #region Quick Action Methods
    
    private void ResetTransform()
    {
        // Update ALL UI input values
        workingPosition = new Vector3(0, 0, 0);
        workingScale = new Vector3(1, 1, 1);
        workingPivot = new Vector3(0, 0, 0);
        rotationDegrees = new Vector3(0, 0, 0);
        matrixAnalysis = null;
        
        // Apply everything through the centralized function
        UpdateMatrixFromUIData();
    }
    
    #endregion

    #region IDisposable Implementation
    
    public void Dispose()
    {
        // Cleanup resources if needed
    }
    
    #endregion
}
