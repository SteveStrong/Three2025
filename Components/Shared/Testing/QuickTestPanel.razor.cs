using Microsoft.AspNetCore.Components;
using Three2025.Apprentice;
using System;
using System.Linq;
using System.Threading.Tasks;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Components.Shared.Testing;

public partial class QuickTestPanel : ComponentBase
{
    [Inject] private IShape3DTech Shape3DTech { get; set; } = default!;
    
    // Test result display properties
    private string lastTestResult = "";
    private string lastTestStatus = "info"; // "success", "error", "info", "warning"
    
    // Geometry type tracking
    private string currentGeometryType = "box";
    private readonly string[] availableGeometryTypes = { "box", "sphere", "cylinder", "cone", "torus" };
    
    // Link geometry type tracking
    private string currentLinkGeometryType = "Pipe";
    private readonly string[] availableLinkGeometryTypes = { "Pipe", "Tube", "Line" };
    
    // ================================================================
    // UTILITY: ERROR HANDLING WRAPPER
    // ================================================================
    
    private async Task ExecuteTest(Func<string> testAction, string failureContext)
    {
        try
        {
            var successMessage = testAction();
            SetTestResult($"✅ {successMessage}", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ {failureContext}: {ex.Message}", "error");
        }
        await Task.CompletedTask;
    }
    
    private async Task ExecuteTestAsync(Func<Task<string>> testAction, string failureContext)
    {
        try
        {
            var successMessage = await testAction();
            SetTestResult($"✅ {successMessage}", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ {failureContext}: {ex.Message}", "error");
        }
    }
    
    private void SetTestResult(string message, string status)
    {
        lastTestResult = message;
        lastTestStatus = status;
        StateHasChanged();
    }
    
    // ================================================================
    // STEP 1: CREATE TEST BOX
    // ================================================================
    
    private async Task Test_CreateTestBox() => await ExecuteTest(() =>
    {
        // Check if TestBox1 already exists
        var existing = Shape3DTech.GetShapeByName("TestBox1");
        if (existing != null)
        {            // Reset to red color and origin position
            Shape3DTech.ChangeColor("TestBox1", "red");
            existing.Transform.Position = new Vector3(0, 0, 0);
            existing.Transform.Rotation = new Euler(0, 0, 0);
            return $"Reset TestBox1 to red at origin (geometry: {currentGeometryType})";
        }
        
        // Create new box with current geometry type
        var result = Shape3DTech.AddShape("TestBox1", "red", currentGeometryType);
        return $"Created TestBox1 (red {currentGeometryType}). Total shapes: {result.Count}";
    }, "Create TestBox failed");
    
    // ================================================================
    // STEP 2: COLOR PROGRESSION
    // ================================================================
    
    private async Task Test_ChangeToOrange() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("TestBox1", "orange");
        return $"Changed TestBox to orange";
    }, "Change color failed");
    
    private async Task Test_ChangeToYellow() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("TestBox1", "yellow");
        return $"Changed TestBox to yellow";
    }, "Change color failed");
    
    private async Task Test_ChangeToGreen() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("TestBox1", "green");
        return $"Changed TestBox to green";
    }, "Change color failed");
    
    // ================================================================
    // GEOMETRY TYPE SELECTION
    // ================================================================
    
    private async Task Test_ChangeGeometryType(string newType) => await ExecuteTest(() =>
    {
        currentGeometryType = newType;
        
        // Check if TestBox1 exists
        var existing = Shape3DTech.GetShapeByName("TestBox1");
        if (existing != null)
        {
            // Delete old shape and create new one with same color and position
            var currentColor = existing.Color;
            var currentPosition = existing.Transform.Position;
            var currentRotation = existing.Transform.Rotation;
            
            Shape3DTech.DeleteShape("TestBox1");
            var result = Shape3DTech.AddShape("TestBox1", currentColor, newType);
            
            // Restore position and rotation
            var newShape = Shape3DTech.GetShapeByName("TestBox1");
            if (newShape != null)
            {
                newShape.Transform.Position = currentPosition;
                newShape.Transform.Rotation = currentRotation;
            }
            
            return $"Changed geometry to {newType} (preserved color and position)";
        }
        
        return $"Geometry type set to {newType} (will be used for next create)";
    }, "Change geometry type failed");
    
    // ================================================================
    // STEP 3: RESIZE OPERATIONS
    // ================================================================
    
    private async Task Test_MakeTaller() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) return "TestBox1 not found";
        
        var newHeight = box.Height * 2.0;
        var result = Shape3DTech.ChangeShapeDimensions("TestBox1", box.Width, newHeight, box.Depth);
        return $"Made TestBox1 taller ({box.Height:F1} → {newHeight:F1})";
    }, "Resize failed");
    
    private async Task Test_MakeWider() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) return "TestBox1 not found";
        
        var newWidth = box.Width * 2.0;
        var result = Shape3DTech.ChangeShapeDimensions("TestBox1", newWidth, box.Height, box.Depth);
        return $"Made TestBox1 wider ({box.Width:F1} → {newWidth:F1})";
    }, "Resize failed");
    
    private async Task Test_MakeDeeper() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) return "TestBox1 not found";
        
        var newDepth = box.Depth * 2.0;
        var result = Shape3DTech.ChangeShapeDimensions("TestBox1", box.Width, box.Height, newDepth);
        return $"Made TestBox1 deeper ({box.Depth:F1} → {newDepth:F1})";
    }, "Resize failed");
    
    private async Task Test_MakeLarger() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) return "TestBox1 not found";
        
        var newWidth = box.Width * 1.5;
        var newHeight = box.Height * 1.5;
        var newDepth = box.Depth * 1.5;
        var result = Shape3DTech.ChangeShapeDimensions("TestBox1", newWidth, newHeight, newDepth);
        return $"Made TestBox1 larger (all dimensions × 1.5)";
    }, "Resize failed");
    
    private async Task Test_MakeSmaller() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) return "TestBox1 not found";
        
        var newWidth = box.Width * 0.5;
        var newHeight = box.Height * 0.5;
        var newDepth = box.Depth * 0.5;
        var result = Shape3DTech.ChangeShapeDimensions("TestBox1", newWidth, newHeight, newDepth);
        return $"Made TestBox1 smaller (all dimensions × 0.5)";
    }, "Resize failed");
    
    // ================================================================
    // STEP 5: CREATE LINK SHAPES (IBodyLink3D)
    // ================================================================
    
    private async Task Test_CreateLinkShape() => await ExecuteTest(() =>
    {
        // Create two body shapes if they don't exist
        var body1 = Shape3DTech.GetShapeByName("Body1");
        if (body1 == null)
        {
            Shape3DTech.AddShape("Body1", "cyan", "sphere", -3, 0, 0);
            body1 = Shape3DTech.GetShapeByName("Body1");
        }
        
        var body2 = Shape3DTech.GetShapeByName("Body2");
        if (body2 == null)
        {
            Shape3DTech.AddShape("Body2", "magenta", "sphere", 3, 0, 0);
            body2 = Shape3DTech.GetShapeByName("Body2");
        }
        
        // Create a link with current geometry type
        var result = Shape3DTech.CreateLinkShape("TestLink1", "yellow", body1!, body2!, 0.2, currentLinkGeometryType);
        return $"Created Link ({currentLinkGeometryType}) between Body1 and Body2. This is an IBodyLink3D (dependent shape)";
    }, "Create Link failed");
    
    private async Task Test_ChangeLinkGeometry() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeLinkGeometryType("TestLink1", currentLinkGeometryType);
        return $"Changed TestLink1 geometry to {currentLinkGeometryType}";
    }, "Change Link Geometry failed");
    
    private async Task Test_CreatePipeLink() => await ExecuteTest(() =>
    {
        // Create two body shapes if they don't exist
        var body1 = Shape3DTech.GetShapeByName("Body1");
        if (body1 == null)
        {
            Shape3DTech.AddShape("Body1", "cyan", "sphere", -3, 0, 0);
            body1 = Shape3DTech.GetShapeByName("Body1");
        }
        
        var body2 = Shape3DTech.GetShapeByName("Body2");
        if (body2 == null)
        {
            Shape3DTech.AddShape("Body2", "magenta", "sphere", 3, 0, 0);
            body2 = Shape3DTech.GetShapeByName("Body2");
        }
        
        // Create a pipe link connecting them
        var result = Shape3DTech.CreatePipeLink("PipeLink1", "yellow", body1!, body2!, 0.2);
        return $"Created Pipe Link between Body1 and Body2. This is an IBodyLink3D (dependent shape)";
    }, "Create Pipe Link failed");
    
    private async Task Test_CreatePathwayLink() => await ExecuteTest(() =>
    {
        // Create two body shapes if they don't exist
        var body1 = Shape3DTech.GetShapeByName("Body1");
        if (body1 == null)
        {
            Shape3DTech.AddShape("Body1", "cyan", "sphere", -3, 0, 0);
            body1 = Shape3DTech.GetShapeByName("Body1");
        }
        
        var body2 = Shape3DTech.GetShapeByName("Body2");
        if (body2 == null)
        {
            Shape3DTech.AddShape("Body2", "magenta", "sphere", 3, 0, 0);
            body2 = Shape3DTech.GetShapeByName("Body2");
        }
        
        // Create a pathway link
        var result = Shape3DTech.CreatePathwayLink("Pathway1", body1!, body2!);
        return $"Created Pathway Link between Body1 and Body2. This is an IBodyLink3D (dependent shape)";
    }, "Create Pathway Link failed");
    
    // ================================================================
    // STEP 4: POSITION OPERATIONS
    // ================================================================
    
    private async Task Test_MoveToOrigin() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("TestBox1", x: 0, y: 0, z: 0);
        return $"Moved TestBox1 to origin (0, 0, 0)";
    }, "Move failed");
    
    private async Task Test_MoveRight() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) throw new Exception("TestBox1 not found");
        box.Transform.MoveBy(1.0, 0, 0);
        return $"Moved TestBox1 right (+1.0 on X-axis)";
    }, "Move failed");
    
    private async Task Test_MoveUp() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) throw new Exception("TestBox1 not found");
        box.Transform.MoveBy(0, 1.0, 0);
        return $"Moved TestBox1 up (+1.0 on Y-axis)";
    }, "Move failed");
    
    private async Task Test_MoveForward() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) throw new Exception("TestBox1 not found");
        box.Transform.MoveBy(0, 0, 1.0);
        return $"Moved TestBox1 forward (+1.0 on Z-axis)";
    }, "Move failed");
    
    private async Task Test_MoveToPosition() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("TestBox1", x: 3.0, y: 2.0, z: -4.0);
        return $"Moved TestBox1 to (3, 2, -4)";
    }, "Move failed");
    
    // ================================================================
    // STEP 5: ROTATION OPERATIONS
    // ================================================================
    
    private async Task Test_RotateX45() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) throw new Exception("TestBox1 not found");
        box.Transform.RotateBy(45, 0, 0, AngleUnit.Degrees);
        return $"Rotated TestBox1 +45° around X-axis (cumulative)";
    }, "Rotate failed");
    
    private async Task Test_RotateY45() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) throw new Exception("TestBox1 not found");
        box.Transform.RotateBy(0, 45, 0, AngleUnit.Degrees);
        return $"Rotated TestBox1 +45° around Y-axis (cumulative)";
    }, "Rotate failed");
    
    private async Task Test_RotateZ45() => await ExecuteTest(() =>
    {
        var box = Shape3DTech.GetShapeByName("TestBox1");
        if (box == null) throw new Exception("TestBox1 not found");
        box.Transform.RotateBy(0, 0, 45, AngleUnit.Degrees);
        return $"Rotated TestBox1 +45° around Z-axis (cumulative)";
    }, "Rotate failed");
    
    private async Task Test_RotateX90() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox1", xDegrees: 90, yDegrees: 0, zDegrees: 0);
        return $"Rotated TestBox1 90° around X-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateY90() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox1", xDegrees: 0, yDegrees: 90, zDegrees: 0);
        return $"Rotated TestBox1 90° around Y-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateZ90() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox1", xDegrees: 0, yDegrees: 0, zDegrees: 90);
        return $"Rotated TestBox1 90° around Z-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateMultiAxis() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox1", xDegrees: 30, yDegrees: 45, zDegrees: 60);
        return $"Rotated TestBox1 (30° X, 45° Y, 60° Z)";
    }, "Rotate failed");
    
    // ================================================================
    // STEP 7: DEMO COMPLEX SHAPES
    // ================================================================
    
    private async Task Test_CreateAudioPanel() => await ExecuteTest(() =>
    {
        // Check if AudioPanel already exists
        var existing = Shape3DTech.GetShapeByName("AudioPanel");
        if (existing != null)
        {
            Shape3DTech.DeleteShape("AudioPanel");
        }
        
        // Create the audio panel
        var audioPanel = new AudioPanelShape("AudioPanel", panelWidth: 16.0, panelHeight: 4.0);
        Shape3DTech.EstablishGeometryStage();
        // TODO: Need to add AddShape method that takes FoShape3D to IShape3DTech
        var stage = Shape3DTech.EstablishGeometryStage();
        stage.AddShape(audioPanel);
        Shape3DTech.RefreshUI();
        
        return $"Created AudioPanel with 10 connectors (XLR, 1/4\", Combo jacks). This demonstrates complex shape assembly!";
    }, "Create Audio Panel failed");
    
    // ================================================================
    // UTILITY: CLEANUP
    // ================================================================
    
    private async Task Test_DeleteTestBox() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.DeleteShape("TestBox1");
        return $"Deleted TestBox1. Remaining shapes: {result.Count}";
    }, "Delete failed");
    
    private async Task Test_ClearAll() => await ExecuteTest(() =>
    {
        Shape3DTech.ClearShapes();
        return $"Cleared all shapes from scene";
    }, "Clear failed");
    

    

    
    // ================================================================
    // UTILITY: UI STYLING METHODS
    // ================================================================
    
    private string GetButtonStyle(string color)
    {
        return $@"
            padding: 0.75rem 1rem;
            background: {color};
            color: white;
            border: none;
            border-radius: 6px;
            cursor: pointer;
            font-weight: 600;
            font-size: 0.9rem;
            width: 100%;
            text-align: left;
            transition: all 0.2s ease;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        ";
    }
    
    private string GetGeometryTypeButtonStyle(string type)
    {
        var isActive = type == currentGeometryType;
        var bgColor = isActive ? "#0d6efd" : "#6c757d";
        var fontWeight = isActive ? "700" : "500";
        return $@"
            padding: 0.4rem 0.8rem;
            background: {bgColor};
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: {fontWeight};
            font-size: 0.75rem;
            transition: all 0.2s ease;
            box-shadow: 0 1px 2px rgba(0,0,0,0.1);
            text-transform: capitalize;
        ";
    }
    
    private string GetLinkGeometryTypeButtonStyle(string type)
    {
        var isActive = type == currentLinkGeometryType;
        var bgColor = isActive ? "#198754" : "#6c757d";
        var fontWeight = isActive ? "700" : "500";
        return $@"
            padding: 0.4rem 0.8rem;
            background: {bgColor};
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-weight: {fontWeight};
            font-size: 0.75rem;
            transition: all 0.2s ease;
            box-shadow: 0 1px 2px rgba(0,0,0,0.1);
            text-transform: capitalize;
        ";
    }
    
    private string GetResultBackground()
    {
        return lastTestStatus switch
        {
            "success" => "#d4edda",
            "error" => "#f8d7da",
            "warning" => "#fff3cd",
            "info" => "#d1ecf1",
            _ => "#e2e3e5"
        };
    }
    
    private string GetResultBorder()
    {
        return lastTestStatus switch
        {
            "success" => "#c3e6cb",
            "error" => "#f5c6cb",
            "warning" => "#ffeeba",
            "info" => "#bee5eb",
            _ => "#d6d8db"
        };
    }
    
    private string GetResultColor()
    {
        return lastTestStatus switch
        {
            "success" => "#155724",
            "error" => "#721c24",
            "warning" => "#856404",
            "info" => "#0c5460",
            _ => "#383d41"
        };
    }
}
