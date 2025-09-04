using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using FoundryBlazor.Shape;
using FoundryBlazor.Shapes3D.SpacialFrame;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Three2025.Shared;
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

    [Parameter] public int CanvasWidth { get; set; } = 1400;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = "Ready to test Universal Geometry Snapping - works with any FoShape3D objects!";

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
            var faceCount = UniversalSnapEngine.GetAllFaces(ComponentA).Count;
            StatusMessage = $"✅ Created Component A using Universal Snapping: {faceCount} faces available at ({ComponentAPosX},{ComponentAPosY},{ComponentAPosZ})";
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
            
            // Get face count using the new universal engine
            var faceCount = UniversalSnapEngine.GetAllFaces(ComponentB).Count;
            StatusMessage = $"✅ Created Component B using Universal Snapping: {faceCount} faces available at ({ComponentBPosX},{ComponentBPosY},{ComponentBPosZ})";
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
            StateHasChanged();
            return;
        }

        try
        {
            // Use universal snapping engine to get face information
            var faceA = UniversalSnapEngine.GetFaceInfo(ComponentA, SelectedFaceA);
            var faceB = UniversalSnapEngine.GetFaceInfo(ComponentB, SelectedFaceB);
            
            if (faceA == null || faceB == null)
            {
                StatusMessage = $"Could not find faces: {SelectedFaceA} on A or {SelectedFaceB} on B";
                StateHasChanged();
                return;
            }

            StatusMessage = $"Ready to snap: A.{SelectedFaceA} → B.{SelectedFaceB}";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error preparing snap: {ex.Message}";
            StateHasChanged();
        }
    }

    public void ExecuteConstraint()
    {
        if (ComponentA == null || ComponentB == null)
        {
            StatusMessage = "Both components must be created before snapping.";
            StateHasChanged();
            return;
        }

        try
        {
            // Use the new universal snapping engine
            var result = UniversalSnapEngine.SnapObjects(ComponentA, SelectedFaceA, ComponentB, SelectedFaceB);
            
            if (result.Success)
            {
                // Update local position values to match the snapped position
                ComponentAPosX = ComponentA.Transform.Position.X;
                ComponentAPosY = ComponentA.Transform.Position.Y;
                ComponentAPosZ = ComponentA.Transform.Position.Z;
                
                // Refresh the visual
                var arena = Workspace?.GetArena();
                var (found, scene) = arena?.CurrentScene() ?? (false, null);
                if (found && scene != null)
                {
                    ComponentA.RefreshToScene(scene);
                }
                
                StatusMessage = $"✅ Snap successful! {SelectedFaceA} of A aligned with {SelectedFaceB} of B.";
            }
            else
            {
                StatusMessage = $"❌ Snap failed: {result.ErrorMessage}";
            }
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
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
        
        var result = UniversalSnapEngine.QuickSnap.StackOnTop(ComponentA, ComponentB);
        if (result.Success)
        {
            ComponentAPosX = ComponentA.Transform.Position.X;
            ComponentAPosY = ComponentA.Transform.Position.Y;
            ComponentAPosZ = ComponentA.Transform.Position.Z;
            
            var arena = Workspace?.GetArena();
            var (found, scene) = arena?.CurrentScene() ?? (false, null);
            if (found && scene != null)
            {
                ComponentA.RefreshToScene(scene);
            }
            
            StatusMessage = "✅ Stacked A on top of B successfully!";
        }
        else
        {
            StatusMessage = $"❌ Stack operation failed: {result.ErrorMessage}";
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
        
        var result = UniversalSnapEngine.QuickSnap.PlaceSideBySide(ComponentA, ComponentB);
        if (result.Success)
        {
            ComponentAPosX = ComponentA.Transform.Position.X;
            ComponentAPosY = ComponentA.Transform.Position.Y;
            ComponentAPosZ = ComponentA.Transform.Position.Z;
            
            var arena = Workspace?.GetArena();
            var (found, scene) = arena?.CurrentScene() ?? (false, null);
            if (found && scene != null)
            {
                ComponentA.RefreshToScene(scene);
            }
            
            StatusMessage = "✅ Placed A side-by-side with B successfully!";
        }
        else
        {
            StatusMessage = $"❌ Side-by-side operation failed: {result.ErrorMessage}";
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
        
        var result = UniversalSnapEngine.QuickSnap.AttachToFront(ComponentA, ComponentB);
        if (result.Success)
        {
            ComponentAPosX = ComponentA.Transform.Position.X;
            ComponentAPosY = ComponentA.Transform.Position.Y;
            ComponentAPosZ = ComponentA.Transform.Position.Z;
            
            var arena = Workspace?.GetArena();
            var (found, scene) = arena?.CurrentScene() ?? (false, null);
            if (found && scene != null)
            {
                ComponentA.RefreshToScene(scene);
            }
            
            StatusMessage = "✅ Attached A to front of B successfully!";
        }
        else
        {
            StatusMessage = $"❌ Attach to front operation failed: {result.ErrorMessage}";
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
        
        var result = UniversalSnapEngine.QuickSnap.AttachToBack(ComponentA, ComponentB);
        if (result.Success)
        {
            ComponentAPosX = ComponentA.Transform.Position.X;
            ComponentAPosY = ComponentA.Transform.Position.Y;
            ComponentAPosZ = ComponentA.Transform.Position.Z;
            
            var arena = Workspace?.GetArena();
            var (found, scene) = arena?.CurrentScene() ?? (false, null);
            if (found && scene != null)
            {
                ComponentA.RefreshToScene(scene);
            }
            
            StatusMessage = "✅ Attached A to back of B successfully!";
        }
        else
        {
            StatusMessage = $"❌ Attach to back operation failed: {result.ErrorMessage}";
        }
        StateHasChanged();
    }

    public void ShowFaces()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int faceCount = 0;
        
        if (ComponentA != null)
        {
            var faces = UniversalSnapEngine.GetAllFaces(ComponentA);
            VisualizationService.ShowWireframeFaces(arena, faces);
            faceCount += faces.Count;
        }
        
        if (ComponentB != null)
        {
            var faces = UniversalSnapEngine.GetAllFaces(ComponentB);
            VisualizationService.ShowWireframeFaces(arena, faces);
            faceCount += faces.Count;
        }
        
        StatusMessage = $"Showing {faceCount} faces as wireframe outlines with labels using Universal Snapping.";
        StateHasChanged();
    }

    public void ShowNormals()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int normalCount = 0;
        
        if (ComponentA != null)
        {
            var faces = UniversalSnapEngine.GetAllFaces(ComponentA);
            VisualizationService.ShowLabeledNormals(arena, faces);
            normalCount += faces.Count;
        }
        
        if (ComponentB != null)
        {
            var faces = UniversalSnapEngine.GetAllFaces(ComponentB);
            VisualizationService.ShowLabeledNormals(arena, faces);
            normalCount += faces.Count;
        }
        
        StatusMessage = $"Showing {normalCount} face normals as red cylinders with cones using Universal Snapping.";
        StateHasChanged();
    }

    public void ShowSnapPoints()
    {
        var arena = Workspace?.GetArena();
        if (arena == null) return;
        
        int pointCount = 0;
        
        if (ComponentA != null)
        {
            var faces = UniversalSnapEngine.GetAllFaces(ComponentA);
            foreach (var face in faces)
            {
                VisualizationService.CreateMarkerSphere(arena, $"SnapPointA_{face.Name}", 
                    face.Center, "#FF9800", 0.05);
                pointCount++;
            }
        }
        
        if (ComponentB != null)
        {
            var faces = UniversalSnapEngine.GetAllFaces(ComponentB);
            foreach (var face in faces)
            {
                VisualizationService.CreateMarkerSphere(arena, $"SnapPointB_{face.Name}", 
                    face.Center, "#9C27B0", 0.05);
                pointCount++;
            }
        }
        
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
        
        // Reset components
        ComponentA = null;
        ComponentB = null;
        
        StatusMessage = "Cleared all visualization and reset components.";
        StateHasChanged();
    }

    public void Dispose() {}

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
