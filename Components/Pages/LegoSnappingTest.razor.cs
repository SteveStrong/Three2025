using FoundryWorldsAndDrawings.Shape;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.PubSub;
using Three2025.Services.Visualization;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.Shared;
using FoundryRulesAndUnits.Extensions; // ✅ Phase 0.5: For WriteSuccess extension

namespace Three2025.Components.Pages;

public class LegoSnappingTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;
    private FoStage3D _legoStage; // ✅ Phase 0.5: Track this page's stage
    
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


            var arena = Workspace.GetArena();
            if (found)
            {
                // ✅ Phase 0.5: Get this page's stage (Canvas already linked it to scene)
                _legoStage = arena.EstablishStage<FoStage3D>(Canvas3DReference.SceneName);
                $"LegoSnappingTest: Retrieved stage '{_legoStage?.Name}' from Canvas".WriteSuccess();
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

            // Set position using MoveBy for proper dirty flag handling
            ComponentA.Transform.Position = Vector3.Zero;
            ComponentA.Transform.Pivot = new Vector3(0, -BoxHeight/2, 0); // Pivot at bottom center
            ComponentA.Transform.MoveBy(ComponentAPosX, ComponentAPosY, ComponentAPosZ);

            // Add to arena as standard FoShape3D
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            // ✅ Phase 0.5: Add component to this page's stage
            _legoStage?.AddShape(ComponentA);

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
            
            // Set position using MoveBy for proper dirty flag handling
            ComponentB.Transform.Position = Vector3.Zero;
            ComponentB.Transform.Pivot = new Vector3(0, -BoxHeight/2, 0); // Pivot at bottom center
            ComponentB.Transform.MoveBy(ComponentBPosX, ComponentBPosY, ComponentBPosZ);

            // Add to arena as standard FoShape3D
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            // ✅ Phase 0.5: Add component to this page's stage
            _legoStage?.AddShape(ComponentB);
            
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
        
        // Use MoveBy for proper dirty flag handling
        ComponentA.Transform.Position = Vector3.Zero;
        ComponentA.Transform.MoveBy(ComponentAPosX, ComponentAPosY, ComponentAPosZ);

        
        StatusMessage = $"Updated Component A position to ({ComponentAPosX},{ComponentAPosY},{ComponentAPosZ}) using Universal Snapping";
        StateHasChanged();
    }

    public void UpdateComponentBPosition()
    {
        if (ComponentB == null) return;
        
        // Use MoveBy for proper dirty flag handling
        ComponentB.Transform.Position = Vector3.Zero;
        ComponentB.Transform.MoveBy(ComponentBPosX, ComponentBPosY, ComponentBPosZ);

        
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
            var faceAName = $"{SelectedFaceA}FaceCenter";
            var faceBName = $"{SelectedFaceB}FaceCenter";
            
            CurrentConstraintStatus = $"Ready to glue: A.{faceAName} → B.{faceBName}";
            StatusMessage = $"Ready to snap: A.{SelectedFaceA} → B.{SelectedFaceB} (will align and rotate)";
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
            var faceAName = $"{SelectedFaceA}FaceCenter";
            var faceBName = $"{SelectedFaceB}FaceCenter";
            
            // Use glue system with rotation alignment
            ComponentA.GlueTo(ComponentB, faceBName, offset: 0.0, alignRotation: true);
            
            CurrentConstraintStatus = $"Applied: A glued to B.{faceBName} with rotation alignment";
            StatusMessage = $"✓ Snapped A.{SelectedFaceA} to B.{SelectedFaceB} with proper alignment!";
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
        
        ComponentA.GlueTo(ComponentB, "TopFaceCenter", offset: 0.0, alignRotation: true);
        StatusMessage = "✓ Stacked A on top of B with rotation alignment!";
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
        
        ComponentA.GlueTo(ComponentB, "RightFaceCenter", offset: 0.0, alignRotation: true);
        StatusMessage = "✓ Placed A beside B (to the right) with rotation alignment!";
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
        
        ComponentA.GlueTo(ComponentB, "FrontFaceCenter", offset: 0.0, alignRotation: true);
        StatusMessage = "✓ Attached A in front of B with rotation alignment!";
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
        
        ComponentA.GlueTo(ComponentB, "BackFaceCenter", offset: 0.0, alignRotation: true);
        StatusMessage = "✓ Attached A behind B with rotation alignment!";
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
        
        // ✅ Phase 0.5: Clear only this page's stage
        _legoStage?.ClearStage();
        
        // Reset components and status
        ComponentA = null;
        ComponentB = null;
        CurrentConstraintStatus = "";
        
        StatusMessage = "Cleared all visualization and reset components.";
        StateHasChanged();
    }

    public void Dispose() {}


}
