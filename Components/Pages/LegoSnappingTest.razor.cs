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
using Three2025.Apprentice.Snapping;

namespace Three2025.Components.Pages;

public class LegoSnappingTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
    
    // Snapping components
    protected SnapBox ComponentA;
    protected SnapBox ComponentB;
    protected FaceToFaceConstraint CurrentConstraint;
    
    // Component dimensions
    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.0;
    protected double BoxDepth { get; set; } = 1.0;
    
    // Component positions
    protected double ComponentAPosX { get; set; } = -3.0;
    protected double ComponentAPosY { get; set; } = 0.0;
    protected double ComponentAPosZ { get; set; } = 0.0;
    
    protected double ComponentBPosX { get; set; } = 3.0;
    protected double ComponentBPosY { get; set; } = 0.0;
    protected double ComponentBPosZ { get; set; } = 0.0;
    
    // Face selection for constraints
    protected string SelectedFaceA { get; set; } = "Bottom";
    protected string SelectedFaceB { get; set; } = "Top";

    [Parameter] public int CanvasWidth { get; set; } = 1400;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = "Ready to create LEGO-style snapping components.";

    // Visual tracking
    private FoShape3D ComponentAVisual;
    private FoShape3D ComponentBVisual;

    public void CreateComponentA()
    {
        try
        {
            var spec = new FoSpec3D
            {
                W = BoxWidth,
                H = BoxHeight,
                D = BoxDepth,
                Px = BoxWidth / 2,
                Py = BoxHeight / 2,
                Pz = BoxDepth / 2,
                X = ComponentAPosX,
                Y = ComponentAPosY,
                Z = ComponentAPosZ
            };
            
            var spatialBox = new SpacialFrame3D(spec, "m");
            ComponentA = new SnapBox(spatialBox);

            // Create visual representation
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            ComponentAVisual = new FoShape3D()
            {
                Name = "ComponentA",
                Color = "#2196F3", // Blue
                Opacity = 0.7,
                Transform = new Transform3() { Position = new Vector3(ComponentAPosX, ComponentAPosY, ComponentAPosZ) },
                GlyphId = Guid.NewGuid().ToString()
            }.CreateBox("ComponentA", BoxWidth, BoxHeight, BoxDepth);

            arena.AddShapeToStage<FoShape3D>(ComponentAVisual);
            StatusMessage = $"Created Component A: {BoxWidth}×{BoxHeight}×{BoxDepth} at ({ComponentAPosX},{ComponentAPosY},{ComponentAPosZ})";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating Component A: {ex.Message}";
            StateHasChanged();
        }
    }

    public void CreateComponentB()
    {
        try
        {
            var spec = new FoSpec3D
            {
                W = BoxWidth,
                H = BoxHeight,
                D = BoxDepth,
                Px = BoxWidth / 2,
                Py = BoxHeight / 2,
                Pz = BoxDepth / 2,
                X = ComponentBPosX,
                Y = ComponentBPosY,
                Z = ComponentBPosZ
            };
            
            var spatialBox = new SpacialFrame3D(spec, "m");
            ComponentB = new SnapBox(spatialBox);

            // Create visual representation
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            ComponentBVisual = new FoShape3D()
            {
                Name = "ComponentB",
                Color = "#4CAF50", // Green
                Opacity = 0.7,
                Transform = new Transform3() { Position = new Vector3(ComponentBPosX, ComponentBPosY, ComponentBPosZ) },
                GlyphId = Guid.NewGuid().ToString()
            }.CreateBox("ComponentB", BoxWidth, BoxHeight, BoxDepth);

            arena.AddShapeToStage<FoShape3D>(ComponentBVisual);
            StatusMessage = $"Created Component B: {BoxWidth}×{BoxHeight}×{BoxDepth} at ({ComponentBPosX},{ComponentBPosY},{ComponentBPosZ})";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating Component B: {ex.Message}";
            StateHasChanged();
        }
    }

    public void UpdateComponentAPosition()
    {
        if (ComponentA == null || ComponentAVisual == null) return;
        
        ComponentAVisual.Transform.Position = new Vector3(ComponentAPosX, ComponentAPosY, ComponentAPosZ);
        
        var arena = Workspace?.GetArena();
        var (found, scene) = arena?.CurrentScene() ?? (false, null);
        if (found && scene != null)
        {
            ComponentAVisual.RefreshToScene(scene);
        }
        
        StatusMessage = $"Updated Component A position to ({ComponentAPosX},{ComponentAPosY},{ComponentAPosZ})";
        StateHasChanged();
    }

    public void UpdateComponentBPosition()
    {
        if (ComponentB == null || ComponentBVisual == null) return;
        
        ComponentBVisual.Transform.Position = new Vector3(ComponentBPosX, ComponentBPosY, ComponentBPosZ);
        
        var arena = Workspace?.GetArena();
        var (found, scene) = arena?.CurrentScene() ?? (false, null);
        if (found && scene != null)
        {
            ComponentBVisual.RefreshToScene(scene);
        }
        
        StatusMessage = $"Updated Component B position to ({ComponentBPosX},{ComponentBPosY},{ComponentBPosZ})";
        StateHasChanged();
    }

    public void CreateConstraint()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created before creating constraints.";
            StateHasChanged();
            return;
        }

        try
        {
            var faceA = ComponentA.GetFace(SelectedFaceA);
            var faceB = ComponentB.GetFace(SelectedFaceB);
            
            if (faceA == null || faceB == null)
            {
                StatusMessage = $"Could not find faces: {SelectedFaceA} on A or {SelectedFaceB} on B";
                StateHasChanged();
                return;
            }

            CurrentConstraint = new FaceToFaceConstraint(ComponentA, faceA, ComponentB, faceB, 1.0);
            StatusMessage = $"Created constraint: A.{SelectedFaceA} → B.{SelectedFaceB}";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating constraint: {ex.Message}";
            StateHasChanged();
        }
    }

    public void ExecuteConstraint()
    {
        if (CurrentConstraint == null)
        {
            StatusMessage = "No constraint to execute. Create a constraint first.";
            StateHasChanged();
            return;
        }

        try
        {
            var result = SnapEngine.ExecuteConstraint(CurrentConstraint);
            
            if (result.Success)
            {
                // Update visual position of Component A to match the constraint
                var newTransform = result.FinalTransform;
                ComponentAPosX = newTransform.Position.X;
                ComponentAPosY = newTransform.Position.Y;
                ComponentAPosZ = newTransform.Position.Z;
                
                UpdateComponentAPosition();
                StatusMessage = $"✅ Constraint executed successfully! Applied {result.ConstraintsApplied} constraint(s).";
            }
            else
            {
                StatusMessage = $"❌ Constraint execution failed: {result.ErrorMessage}";
            }
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error executing constraint: {ex.Message}";
            StateHasChanged();
        }
    }

    // Quick preset methods
    public void StackAOnTopOfB()
    {
        SelectedFaceA = "Bottom";
        SelectedFaceB = "Top";
        CreateConstraint();
        if (CurrentConstraint != null) ExecuteConstraint();
    }

    public void PlaceASideBySideWithB()
    {
        SelectedFaceA = "Left";
        SelectedFaceB = "Right";
        CreateConstraint();
        if (CurrentConstraint != null) ExecuteConstraint();
    }

    public void AttachAToFrontOfB()
    {
        SelectedFaceA = "Back";
        SelectedFaceB = "Front";
        CreateConstraint();
        if (CurrentConstraint != null) ExecuteConstraint();
    }

    public void AttachAToBackOfB()
    {
        SelectedFaceA = "Front";
        SelectedFaceB = "Back";
        CreateConstraint();
        if (CurrentConstraint != null) ExecuteConstraint();
    }

    public void ShowFaces()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int faceCount = 0;
        
        if (ComponentA != null)
        {
            foreach (var face in ComponentA.Faces.Values)
            {
                VisualizationService.ShowWireframeFaces(arena, new List<Face3D> { face });
                faceCount++;
            }
        }
        
        if (ComponentB != null)
        {
            foreach (var face in ComponentB.Faces.Values)
            {
                VisualizationService.ShowWireframeFaces(arena, new List<Face3D> { face });
                faceCount++;
            }
        }
        
        StatusMessage = $"Showing {faceCount} faces as wireframe outlines with labels.";
        StateHasChanged();
    }

    public void ShowNormals()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int normalCount = 0;
        
        if (ComponentA != null)
        {
            var faces = ComponentA.Faces.Values.ToList();
            VisualizationService.ShowLabeledNormals(arena, faces);
            normalCount += faces.Count;
        }
        
        if (ComponentB != null)
        {
            var faces = ComponentB.Faces.Values.ToList();
            VisualizationService.ShowLabeledNormals(arena, faces);
            normalCount += faces.Count;
        }
        
        StatusMessage = $"Showing {normalCount} face normals as red cylinders with cones.";
        StateHasChanged();
    }

    public void ShowSnapPoints()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int pointCount = 0;
        
        if (ComponentA != null)
        {
            foreach (var snapPoint in ComponentA.SnapPoints.Values)
            {
                CreateMarkerSphere($"SnapPointA_{snapPoint.Name}", 
                    new Point3D(snapPoint.LocalPosition.X + ComponentAPosX, 
                               snapPoint.LocalPosition.Y + ComponentAPosY, 
                               snapPoint.LocalPosition.Z + ComponentAPosZ), 
                    "#FF9800", 0.05);
                pointCount++;
            }
        }
        
        if (ComponentB != null)
        {
            foreach (var snapPoint in ComponentB.SnapPoints.Values)
            {
                CreateMarkerSphere($"SnapPointB_{snapPoint.Name}", 
                    new Point3D(snapPoint.LocalPosition.X + ComponentBPosX, 
                               snapPoint.LocalPosition.Y + ComponentBPosY, 
                               snapPoint.LocalPosition.Z + ComponentBPosZ), 
                    "#9C27B0", 0.05);
                pointCount++;
            }
        }
        
        StatusMessage = $"Showing {pointCount} snap points as colored spheres.";
        StateHasChanged();
    }

    public void ShowAll()
    {
        ShowFaces();
        ShowNormals();
        ShowSnapPoints();
        StatusMessage = "Showing complete visualization: faces, normals, and snap points.";
        StateHasChanged();
    }

    public void ClearVisualization()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        arena.ClearArena();
        
        // Reset components
        ComponentA = null;
        ComponentB = null;
        CurrentConstraint = null;
        ComponentAVisual = null;
        ComponentBVisual = null;
        
        StatusMessage = "Cleared all visualization and reset components.";
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

    public void Dispose() { }

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
                // Create initial components for demonstration
                CreateComponentA();
                CreateComponentB();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
}
