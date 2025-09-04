using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using Microsoft.AspNetCore.Components;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;
using Three2025.Apprentice;

namespace Three2025.Components.Pages;

public class LegoSnappingTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
    
    // Universal geometry snapping components - work with any FoShape3D
    protected FoShape3D ComponentA;
    protected FoShape3D ComponentB;
    
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

    // Properties for UI display
    protected string CurrentConstraintStatus { get; set; } = "";

 
    [Parameter] public int CanvasWidth { get; set; } = 1400;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = "Ready to test Universal Geometry Snapping - works with any FoShape3D objects!";

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
    
    public void CreateComponentA()
    {
        try
        {
            // Create standard FoShape3D box - any geometry becomes snappable
            ComponentA = new FoShape3D("ComponentA", "#2196F3");
            ComponentA.CreateBox("ComponentA", BoxWidth, BoxHeight, BoxDepth);

            // Set position using standard Transform3
            ComponentA.Transform.Position = new Vector3(ComponentAPosX, ComponentAPosY, ComponentAPosZ);

            // Add to arena as standard FoShape3D
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            arena.AddShapeToStage<FoShape3D>(ComponentA);

            // Get face count using the new universal engine
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
            // Create standard FoShape3D box - any geometry becomes snappable
            ComponentB = new FoShape3D("ComponentB", "#4CAF50");
            ComponentB.CreateBox("ComponentB", BoxWidth, BoxHeight, BoxDepth);
            
            // Set position using standard Transform3
            ComponentB.Transform.Position = new Vector3(ComponentBPosX, ComponentBPosY, ComponentBPosZ);

            // Add to arena as standard FoShape3D
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            arena.AddShapeToStage<FoShape3D>(ComponentB);
            
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
        if (ComponentA == null) return;
        
        // Use standard Transform3 position update
        ComponentA.Transform.Position = new Vector3(ComponentAPosX, ComponentAPosY, ComponentAPosZ);
        
        var arena = Workspace?.GetArena();
        var (found, scene) = arena?.CurrentScene() ?? (false, null);
        if (found && scene != null)
        {
            ComponentA.RefreshToScene(scene);
        }
        
        StatusMessage = $"Updated Component A position to ({ComponentAPosX},{ComponentAPosY},{ComponentAPosZ}) using Universal Snapping";
        StateHasChanged();
    }

    public void UpdateComponentBPosition()
    {
        if (ComponentB == null) return;
        
        // Use standard Transform3 position update
        ComponentB.Transform.Position = new Vector3(ComponentBPosX, ComponentBPosY, ComponentBPosZ);
        
        var arena = Workspace?.GetArena();
        var (found, scene) = arena?.CurrentScene() ?? (false, null);
        if (found && scene != null)
        {
            ComponentB.RefreshToScene(scene);
        }
        
        StatusMessage = $"Updated Component B position to ({ComponentBPosX},{ComponentBPosY},{ComponentBPosZ}) using Universal Snapping";
        StateHasChanged();
    }

    public void CreateConstraint()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created before creating constraints.";
            CurrentConstraintStatus = "No components available";
            StateHasChanged();
            return;
        }

        try
        {


            CurrentConstraintStatus = "Ready to apply";
            StatusMessage = $"Ready to snap: A.{SelectedFaceA} → B.{SelectedFaceB}";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error preparing snap: {ex.Message}";
            CurrentConstraintStatus = "Error in constraint preparation";
            StateHasChanged();
        }
    }

    public void ExecuteConstraint()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created before snapping.";
            CurrentConstraintStatus = "No components available";
            StateHasChanged();
            return;
        }

        try
        {
            // Use the new universal snapping engine

            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            CurrentConstraintStatus = $"Error: {ex.Message}";
            StatusMessage = $"Error executing snap: {ex.Message}";
            StateHasChanged();
        }
    }

    // Quick preset methods using universal snapping
    public void StackAOnTopOfB()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created first.";
            StateHasChanged();
            return;
        }
        
       StateHasChanged();
    }

    public void PlaceASideBySideWithB()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created first.";
            StateHasChanged();
            return;
        }
        
       StateHasChanged();
    }

    public void AttachAToFrontOfB()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created first.";
            StateHasChanged();
            return;
        }
        
         StateHasChanged();
    }

    public void AttachAToBackOfB()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created first.";
            StateHasChanged();
            return;
        }
        
        StateHasChanged();
    }

    public void ShowFaces()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int faceCount = 0;
        
        StatusMessage = $"Showing {faceCount} faces as wireframe outlines with labels using Universal Snapping.";
        StateHasChanged();
    }

    public void ShowNormals()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int normalCount = 0;
        
         
        StatusMessage = $"Showing {normalCount} face normals as red cylinders with cones using Universal Snapping.";
        StateHasChanged();
    }

    public void ShowSnapPoints()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int pointCount = 0;
        
  
        
        StatusMessage = $"Showing {pointCount} snap points (face centers) as colored spheres using Universal Snapping.";
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
        
        // Reset components and status
        ComponentA = null;
        ComponentB = null;
        CurrentConstraintStatus = "";
        
        StatusMessage = "Cleared all visualization and reset components.";
        StateHasChanged();
    }

    public void Dispose() {}


}
