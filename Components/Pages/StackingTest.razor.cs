using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Geometires;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Three2025.Shared;
using FoundryBlazor.Solutions;
using BlazorThreeJS.Core;
using FoundryBlazor.Shared;
using FoundryBlazor.PubSub;
using Three2025.Services.Visualization;

namespace Three2025.Components.Pages;

public class StackingTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public Canvas3DComponentBase Canvas3DReference = null;
    
    // Component references
    protected FoShape3D BlockComponent;
    protected FoShape3D CylinderComponent;
    protected SpacialBox3D BlockSpatial;

    // Component properties for UI binding
    protected double BlockWidth { get; set; } = 2.0;
    protected double BlockHeight { get; set; } = 1.0;
    protected double BlockDepth { get; set; } = 2.0;
    
    protected double CylinderRadius { get; set; } = 0.8;
    protected double CylinderHeight { get; set; } = 1.5;

    // Canvas properties
    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    // Stacking state
    protected bool IsStacked { get; set; } = false;
    protected bool CanStack => BlockComponent != null && CylinderComponent != null && !IsStacked;
    protected string ContactType { get; set; } = "None";
    protected string StabilityStatus { get; set; } = "N/A";
    protected double OverhangDistance { get; set; } = 0.0;

    protected string StatusMessage { get; set; } = string.Empty;

    // Stacking configuration
    private Vector3 originalCylinderPosition;
    private Vector3 stackedCylinderPosition;

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

            var arena = Workspace.GetArena();
            if (found)
            {
                arena.SetScene(scene!);
                CreateBoth(); // Start with both components visible
            }
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    #region Component Creation

    public void CreateBlock()
    {
        try
        {
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            // Remove existing block if any
            if (BlockComponent != null)
            {
                arena.RemoveShapeFromStage(BlockComponent);
            }

            // Create spatial representation for analysis
            BlockSpatial = new SpacialBox3D(BlockWidth, BlockHeight, BlockDepth, "m");

            // Create visual block component
            BlockComponent = new FoShape3D()
                .CreateBox("StackingBlock", BlockWidth, BlockHeight, BlockDepth);

            BlockComponent.Color = "#4CAF50"; // Green block
            BlockComponent.Opacity = 0.8;
            
            // Position block at origin (bottom face on ground)
            BlockComponent.Transform.Position = new Vector3(0, BlockHeight / 2, 0);

            arena.AddShapeToStage<FoShape3D>(BlockComponent);

            StatusMessage = $"Created block: {BlockWidth}×{BlockHeight}×{BlockDepth}m";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating block: {ex.Message}";
            StateHasChanged();
        }
    }

    public void CreateCylinder()
    {
        try
        {
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            // Remove existing cylinder if any
            if (CylinderComponent != null)
            {
                arena.RemoveShapeFromStage(CylinderComponent);
            }

            // Create visual cylinder component
            CylinderComponent = new FoShape3D()
                .CreateCylinder("StackingCylinder", CylinderRadius * 2, CylinderHeight, CylinderRadius * 2);

            CylinderComponent.Color = "#2196F3"; // Blue cylinder
            CylinderComponent.Opacity = 0.8;
            
            // Position cylinder to the side initially (bottom face on ground)
            originalCylinderPosition = new Vector3(3, CylinderHeight / 2, 0);
            CylinderComponent.Transform.Position = originalCylinderPosition;

            arena.AddShapeToStage<FoShape3D>(CylinderComponent);

            StatusMessage = $"Created cylinder: R={CylinderRadius}, H={CylinderHeight}m";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating cylinder: {ex.Message}";
            StateHasChanged();
        }
    }

    public void CreateBoth()
    {
        CreateBlock();
        CreateCylinder();
        StatusMessage = "Created both block and cylinder components";
        StateHasChanged();
    }

    #endregion

    #region Stacking Operations

    /// <summary>
    /// Stacks the cylinder on top of the block using proper face-to-face alignment mathematics.
    /// This method calculates the exact centers of the block's top face and cylinder's bottom face,
    /// then computes the precise transformation needed to align these face centers perfectly.
    /// </summary>
    public void StackCylinderOnBlock()
    {
        if (!CanStack)
        {
            StatusMessage = "Cannot stack: Missing components or already stacked";
            StateHasChanged();
            return;
        }

        try
        {
            // Calculate face centers for proper alignment
            var blockTopFaceCenter = CalculateBlockTopFaceCenter();
            var cylinderBottomFaceCenter = CalculateCylinderBottomFaceCenter();
            
            // Calculate the alignment transform needed
            var alignmentOffset = CalculateFaceAlignmentOffset(blockTopFaceCenter, cylinderBottomFaceCenter);
            
            // Apply the transform to align face centers
            var newCylinderPosition = new Vector3(
                CylinderComponent.Transform.Position.X + alignmentOffset.X,
                CylinderComponent.Transform.Position.Y + alignmentOffset.Y,
                CylinderComponent.Transform.Position.Z + alignmentOffset.Z
            );

            // Apply stacking position
            CylinderComponent.Transform.Position = newCylinderPosition;
            stackedCylinderPosition = newCylinderPosition;

            // Update stacking state
            IsStacked = true;
            ContactType = "Circular-to-Rectangular";
            
            // Analyze the stack
            AnalyzeCurrentStack();

            // Verification: Calculate final alignment error
            var finalBlockTop = CalculateBlockTopFaceCenter();
            var finalCylinderBottom = CalculateCylinderBottomFaceCenter();
            var alignmentError = Vector3.Distance(finalBlockTop, finalCylinderBottom);

            StatusMessage = $"✅ Stacked! Face centers aligned (Error: {alignmentError:F4} units)";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error stacking: {ex.Message}";
            StateHasChanged();
        }
    }

    public void UnstackComponents()
    {
        if (!IsStacked)
        {
            StatusMessage = "Components are not currently stacked";
            StateHasChanged();
            return;
        }

        try
        {
            // Return cylinder to original position
            CylinderComponent.Transform.Position = originalCylinderPosition;

            // Reset stacking state
            IsStacked = false;
            ContactType = "None";
            StabilityStatus = "N/A";
            OverhangDistance = 0.0;

            StatusMessage = "Components unstacked and returned to original positions";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error unstacking: {ex.Message}";
            StateHasChanged();
        }
    }

    public void ShowStackingPreview()
    {
        if (!CanStack)
        {
            StatusMessage = "Cannot preview: Missing components or already stacked";
            StateHasChanged();
            return;
        }

        try
        {
            var arena = Workspace?.GetArena();
            if (arena == null) return;

            // Create temporary preview guides
            var blockTopCenter = CalculateBlockTopCenter();
            
            // Show stacking target point
            CreateTemporaryMarker("StackTarget", blockTopCenter, "#FFD700", 0.1);
            
            // Show cylinder destination
            var previewPosition = new Vector3(blockTopCenter.X, blockTopCenter.Y + CylinderHeight / 2, blockTopCenter.Z);
            CreateTemporaryMarker("StackPreview", previewPosition, "#FF6B6B", 0.08);

            StatusMessage = "Showing stacking preview guides (gold = target, red = cylinder center)";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error showing preview: {ex.Message}";
            StateHasChanged();
        }
    }

    #endregion

    #region Preset Configurations

    protected void CreateSmallStack()
    {
        BlockWidth = BlockHeight = BlockDepth = 1.0;
        CylinderRadius = 0.3;
        CylinderHeight = 0.8;
        CreateBoth();
    }

    protected void CreateMediumStack()
    {
        BlockWidth = BlockDepth = 2.0;
        BlockHeight = 1.0;
        CylinderRadius = 0.8;
        CylinderHeight = 1.5;
        CreateBoth();
    }

    protected void CreateLargeStack()
    {
        BlockWidth = BlockDepth = 3.0;
        BlockHeight = 1.5;
        CylinderRadius = 1.2;
        CylinderHeight = 2.0;
        CreateBoth();
    }

    protected void CreateTightFit()
    {
        BlockWidth = BlockDepth = 2.0;
        BlockHeight = 1.0;
        CylinderRadius = 1.0; // Cylinder diameter equals block width
        CylinderHeight = 1.0;
        CreateBoth();
    }

    protected void CreateOverhangDemo()
    {
        BlockWidth = BlockDepth = 1.5;
        BlockHeight = 1.0;
        CylinderRadius = 1.2; // Cylinder larger than block - will overhang
        CylinderHeight = 1.0;
        CreateBoth();
    }

    #endregion

    #region Visualization

    public void ShowBlockFaces()
    {
        if (BlockComponent == null || BlockSpatial == null)
        {
            StatusMessage = "No block created yet. Please create a block first.";
            StateHasChanged();
            return;
        }

        var arena = Workspace?.GetArena();
        if (arena == null) return;

        var faces = BlockSpatial.GetFacesWithNormals();
        VisualizationService.ShowWireframeFaces(arena, faces);
        StatusMessage = $"Showing {faces.Count} block faces as wireframe with labels";
        StateHasChanged();
    }

    public void ShowCylinderFaces()
    {
        if (CylinderComponent == null)
        {
            StatusMessage = "No cylinder created yet. Please create a cylinder first.";
            StateHasChanged();
            return;
        }

        // Create cylindrical face representations
        var cylinderCenter = CylinderComponent.Transform.Position;
        
        // Top and bottom circular faces
        var topFaceCenter = new Point3D(cylinderCenter.X, cylinderCenter.Y + CylinderHeight/2, cylinderCenter.Z, "TopFace");
        var bottomFaceCenter = new Point3D(cylinderCenter.X, cylinderCenter.Y - CylinderHeight/2, cylinderCenter.Z, "BottomFace");
        
        CreateTemporaryMarker("CylTopFace", topFaceCenter, "#00FF00", 0.05);
        CreateTemporaryMarker("CylBottomFace", bottomFaceCenter, "#FF0000", 0.05);

        StatusMessage = "Showing cylinder faces: Green=Top, Red=Bottom";
        StateHasChanged();
    }

    public void ShowContactArea()
    {
        if (!IsStacked)
        {
            StatusMessage = "Stack components first to show contact area";
            StateHasChanged();
            return;
        }

        var blockTopCenter = CalculateBlockTopCenter();
        
        // Calculate contact area (intersection of cylinder base and block top)
        var contactRadius = Math.Min(CylinderRadius, Math.Min(BlockWidth/2, BlockDepth/2));
        
        // Create contact area visualization (simplified as a circle)
        CreateTemporaryMarker("ContactArea", blockTopCenter, "#FFA500", contactRadius);

        StatusMessage = $"Contact area radius: {contactRadius:F2} units (orange circle)";
        StateHasChanged();
    }

    public void ShowSnapGuides()
    {
        if (BlockComponent == null)
        {
            StatusMessage = "Create block first to show snap guides";
            StateHasChanged();
            return;
        }

        var blockTopCenter = CalculateBlockTopCenter();
        
        // Show block top face center
        CreateTemporaryMarker("BlockTopCenter", blockTopCenter, "#FFD700", 0.08);
        
        // Show block top face corners
        var halfW = BlockWidth / 2;
        var halfD = BlockDepth / 2;
        var topY = blockTopCenter.Y;
        
        var corners = new[]
        {
            new Point3D(blockTopCenter.X + halfW, topY, blockTopCenter.Z + halfD, "TopFrontRight"),
            new Point3D(blockTopCenter.X - halfW, topY, blockTopCenter.Z + halfD, "TopFrontLeft"),
            new Point3D(blockTopCenter.X + halfW, topY, blockTopCenter.Z - halfD, "TopBackRight"),
            new Point3D(blockTopCenter.X - halfW, topY, blockTopCenter.Z - halfD, "TopBackLeft")
        };

        for (int i = 0; i < corners.Length; i++)
        {
            CreateTemporaryMarker($"Corner{i}", corners[i], "#FF69B4", 0.04);
        }

        StatusMessage = "Snap guides: Gold=center, Pink=corners";
        StateHasChanged();
    }

    public void ShowAll()
    {
        ShowBlockFaces();
        ShowCylinderFaces();
        if (IsStacked)
        {
            ShowContactArea();
        }
        ShowSnapGuides();
        StatusMessage = "Showing all visualization elements";
        StateHasChanged();
    }

    #endregion

    #region Analysis Tools

    public void AnalyzeStability()
    {
        if (!IsStacked)
        {
            StatusMessage = "Stack components first to analyze stability";
            StateHasChanged();
            return;
        }

        try
        {
            // Calculate center of mass and support area
            var cylinderArea = Math.PI * CylinderRadius * CylinderRadius;
            var blockTopArea = BlockWidth * BlockDepth;
            var supportRatio = Math.Min(cylinderArea, blockTopArea) / cylinderArea;

            // Determine stability
            if (supportRatio >= 0.9)
                StabilityStatus = "Very Stable";
            else if (supportRatio >= 0.6)
                StabilityStatus = "Stable";
            else if (supportRatio >= 0.3)
                StabilityStatus = "Marginally Stable";
            else
                StabilityStatus = "Unstable";

            StatusMessage = $"Stability Analysis: {StabilityStatus} (Support ratio: {supportRatio:P1})";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error analyzing stability: {ex.Message}";
            StateHasChanged();
        }
    }

    public void CheckAlignment()
    {
        if (!IsStacked)
        {
            StatusMessage = "Stack components first to check alignment";
            StateHasChanged();
            return;
        }

        // Calculate actual face centers for precise alignment measurement
        var blockTopFaceCenter = CalculateBlockTopFaceCenter();
        var cylinderBottomFaceCenter = CalculateCylinderBottomFaceCenter();

        // Calculate alignment error between face centers
        var alignmentError = Vector3.Distance(blockTopFaceCenter, cylinderBottomFaceCenter);
        
        // Calculate horizontal alignment error (X and Z only)
        var horizontalBlockCenter = new Vector3(blockTopFaceCenter.X, 0, blockTopFaceCenter.Z);
        var horizontalCylinderCenter = new Vector3(cylinderBottomFaceCenter.X, 0, cylinderBottomFaceCenter.Z);
        var horizontalError = Vector3.Distance(horizontalBlockCenter, horizontalCylinderCenter);
        
        // Calculate vertical alignment error (Y only)
        var verticalError = Math.Abs(blockTopFaceCenter.Y - cylinderBottomFaceCenter.Y);
        
        string alignmentStatus = alignmentError < 0.01 ? "Perfect" : 
                               alignmentError < 0.1 ? "Good" : 
                               alignmentError < 0.5 ? "Fair" : "Poor";

        StatusMessage = $"Face Alignment: {alignmentStatus} | Total Error: {alignmentError:F4} units | " +
                       $"Horizontal: {horizontalError:F4} | Vertical: {verticalError:F4}";
        StateHasChanged();
    }

    public void MeasureOverhang()
    {
        if (!IsStacked)
        {
            StatusMessage = "Stack components first to measure overhang";
            StateHasChanged();
            return;
        }

        // Calculate how much cylinder extends beyond block edges
        var blockHalfWidth = BlockWidth / 2;
        var blockHalfDepth = BlockDepth / 2;
        
        var maxOverhang = Math.Max(0, CylinderRadius - Math.Min(blockHalfWidth, blockHalfDepth));
        
        OverhangDistance = maxOverhang;
        
        string overhangStatus = maxOverhang == 0 ? "No overhang" :
                              maxOverhang < 0.2 ? "Minimal overhang" :
                              maxOverhang < 0.5 ? "Moderate overhang" : "Significant overhang";

        StatusMessage = $"Overhang: {overhangStatus} ({maxOverhang:F2} units)";
        StateHasChanged();
    }

    public void ValidateConstraints()
    {
        if (!IsStacked)
        {
            StatusMessage = "Stack components first to validate constraints";
            StateHasChanged();
            return;
        }

        var issues = new List<string>();

        // Check if cylinder bottom is properly aligned with block top
        var blockTopY = BlockHeight;
        var cylinderBottomY = CylinderComponent.Transform.Position.Y - CylinderHeight / 2;
        var heightDifference = Math.Abs(blockTopY - cylinderBottomY);

        if (heightDifference > 0.01)
            issues.Add($"Height misalignment: {heightDifference:F3} units");

        // Check overhang
        if (OverhangDistance > CylinderRadius * 0.5)
            issues.Add("Excessive overhang may cause instability");

        // Check size compatibility
        var sizeRatio = (CylinderRadius * 2) / Math.Min(BlockWidth, BlockDepth);
        if (sizeRatio > 2.0)
            issues.Add("Cylinder much larger than block base");

        StatusMessage = issues.Any() ? 
            $"Constraint Issues: {string.Join("; ", issues)}" : 
            "✅ All constraints validated successfully";
        StateHasChanged();
    }

    #endregion

    #region Utility Methods

    public void ClearAll()
    {
        var arena = Workspace?.GetArena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }

        arena.ClearArena();
        
        BlockComponent = null;
        CylinderComponent = null;
        BlockSpatial = null;
        IsStacked = false;
        ContactType = "None";
        StabilityStatus = "N/A";
        OverhangDistance = 0.0;
        
        StatusMessage = "All components cleared";
        StateHasChanged();
    }

    public void ResetPositions()
    {
        if (CylinderComponent != null)
        {
            CylinderComponent.Transform.Position = originalCylinderPosition;
            IsStacked = false;
            ContactType = "None";
            StatusMessage = "Components reset to original positions";
            StateHasChanged();
        }
    }

    // Face Center Calculations for Proper Stacking Alignment
    
    /// <summary>
    /// Calculates the exact center point of the block's top face
    /// </summary>
    private Vector3 CalculateBlockTopFaceCenter()
    {
        if (BlockComponent == null) return Vector3.Zero;
        
        var blockPosition = BlockComponent.Transform.Position;
        var blockScale = BlockComponent.Transform.Scale;
        
        // Top face center is at the block's position + half the height
        // The Y coordinate represents the top surface of the block
        return new Vector3(
            blockPosition.X,
            blockPosition.Y + (BlockHeight * blockScale.Y) / 2,
            blockPosition.Z
        );
    }
    
    /// <summary>
    /// Calculates the exact center point of the cylinder's bottom face in its current position
    /// </summary>
    private Vector3 CalculateCylinderBottomFaceCenter()
    {
        if (CylinderComponent == null) return Vector3.Zero;
        
        var cylinderPosition = CylinderComponent.Transform.Position;
        var cylinderScale = CylinderComponent.Transform.Scale;
        
        // Bottom face center is at the cylinder's position - half the height
        // The Y coordinate represents the bottom surface of the cylinder
        return new Vector3(
            cylinderPosition.X,
            cylinderPosition.Y - (CylinderHeight * cylinderScale.Y) / 2,
            cylinderPosition.Z
        );
    }
    
    /// <summary>
    /// Calculates the offset needed to align two face centers
    /// </summary>
    private Vector3 CalculateFaceAlignmentOffset(Vector3 targetFaceCenter, Vector3 currentFaceCenter)
    {
        // The offset is simply the difference between target and current positions
        return new Vector3(
            targetFaceCenter.X - currentFaceCenter.X,
            targetFaceCenter.Y - currentFaceCenter.Y,
            targetFaceCenter.Z - currentFaceCenter.Z
        );
    }

    // Legacy Methods (keeping for backward compatibility)
    
    private Vector3 CalculateBlockTopCenter()
    {
        if (BlockComponent == null) return Vector3.Zero;
        
        var blockPosition = BlockComponent.Transform.Position;
        return new Vector3(
            blockPosition.X,
            blockPosition.Y + BlockHeight / 2, // Top face
            blockPosition.Z
        );
    }

    private void AnalyzeCurrentStack()
    {
        if (!IsStacked) return;

        // Update analysis values
        MeasureOverhang();
        AnalyzeStability();
        CheckAlignment();
    }

    private void CreateTemporaryMarker(string name, Point3D position, string color, double radius)
    {
        var shape = new FoShape3D()
        {
            Name = name,
            Color = color,
            GlyphId = Guid.NewGuid().ToString(),
            Opacity = 0.7,
            Transform = new Transform3() {
                Position = new Vector3(position.X, position.Y, position.Z)
            }
        }.CreateSphere(name, radius, radius, radius);

        var arena = Workspace?.GetArena();
        arena?.AddShapeToStage<FoShape3D>(shape);
    }

    private void CreateTemporaryMarker(string name, Vector3 position, string color, double radius)
    {
        CreateTemporaryMarker(name, new Point3D(position.X, position.Y, position.Z, name), color, radius);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup resources if needed
    }
}
