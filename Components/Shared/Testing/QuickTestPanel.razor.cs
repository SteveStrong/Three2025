using Microsoft.AspNetCore.Components;
using Three2025.Apprentice;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Three2025.Components.Shared.Testing;

public partial class QuickTestPanel : ComponentBase
{
    [Inject] private IShape3DTech Shape3DTech { get; set; } = default!;
    
    // Test result display properties
    private string lastTestResult = "";
    private string lastTestStatus = "info"; // "success", "error", "info", "warning"
    
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
        var result = Shape3DTech.AddShape("TestBox", "red", "box");
        return $"Created TestBox (red). Total shapes: {result.Count}";
    }, "Create TestBox failed");
    
    // ================================================================
    // STEP 2: COLOR PROGRESSION
    // ================================================================
    
    private async Task Test_ChangeToOrange() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("TestBox", "orange");
        return $"Changed TestBox to orange";
    }, "Change color failed");
    
    private async Task Test_ChangeToYellow() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("TestBox", "yellow");
        return $"Changed TestBox to yellow";
    }, "Change color failed");
    
    private async Task Test_ChangeToGreen() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("TestBox", "green");
        return $"Changed TestBox to green";
    }, "Change color failed");
    
    // ================================================================
    // STEP 3: RESIZE OPERATIONS
    // ================================================================
    
    private async Task Test_MakeTaller() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ScaleShape("TestBox", scaleX: 1.0, scaleY: 2.0, scaleZ: 1.0);
        return $"Made TestBox taller (2x height)";
    }, "Resize failed");
    
    private async Task Test_MakeWider() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ScaleShape("TestBox", scaleX: 2.0, scaleY: 1.0, scaleZ: 1.0);
        return $"Made TestBox wider (2x width)";
    }, "Resize failed");
    
    private async Task Test_MakeDeeper() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ScaleShape("TestBox", scaleX: 1.0, scaleY: 1.0, scaleZ: 2.0);
        return $"Made TestBox deeper (2x depth)";
    }, "Resize failed");
    
    private async Task Test_MakeLarger() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ScaleShape("TestBox", scaleX: 1.5, scaleY: 1.5, scaleZ: 1.5);
        return $"Made TestBox larger (1.5x all dimensions)";
    }, "Resize failed");
    
    private async Task Test_MakeSmaller() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ScaleShape("TestBox", scaleX: 0.5, scaleY: 0.5, scaleZ: 0.5);
        return $"Made TestBox smaller (0.5x all dimensions)";
    }, "Resize failed");
    
    // ================================================================
    // STEP 4: POSITION OPERATIONS
    // ================================================================
    
    private async Task Test_MoveToOrigin() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("TestBox", x: 0, y: 0, z: 0);
        return $"Moved TestBox to origin (0, 0, 0)";
    }, "Move failed");
    
    private async Task Test_MoveRight() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("TestBox", x: 5.0, y: 0, z: 0);
        return $"Moved TestBox right (+5 on X-axis)";
    }, "Move failed");
    
    private async Task Test_MoveUp() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("TestBox", x: 0, y: 5.0, z: 0);
        return $"Moved TestBox up (+5 on Y-axis)";
    }, "Move failed");
    
    private async Task Test_MoveForward() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("TestBox", x: 0, y: 0, z: 5.0);
        return $"Moved TestBox forward (+5 on Z-axis)";
    }, "Move failed");
    
    private async Task Test_MoveToPosition() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("TestBox", x: 3.0, y: 2.0, z: -4.0);
        return $"Moved TestBox to (3, 2, -4)";
    }, "Move failed");
    
    // ================================================================
    // STEP 5: ROTATION OPERATIONS
    // ================================================================
    
    private async Task Test_RotateX45() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox", xDegrees: 45, yDegrees: 0, zDegrees: 0);
        return $"Rotated TestBox 45° around X-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateY45() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox", xDegrees: 0, yDegrees: 45, zDegrees: 0);
        return $"Rotated TestBox 45° around Y-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateZ45() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox", xDegrees: 0, yDegrees: 0, zDegrees: 45);
        return $"Rotated TestBox 45° around Z-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateX90() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox", xDegrees: 90, yDegrees: 0, zDegrees: 0);
        return $"Rotated TestBox 90° around X-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateY90() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox", xDegrees: 0, yDegrees: 90, zDegrees: 0);
        return $"Rotated TestBox 90° around Y-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateZ90() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox", xDegrees: 0, yDegrees: 0, zDegrees: 90);
        return $"Rotated TestBox 90° around Z-axis";
    }, "Rotate failed");
    
    private async Task Test_RotateMultiAxis() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("TestBox", xDegrees: 30, yDegrees: 45, zDegrees: 60);
        return $"Rotated TestBox (30° X, 45° Y, 60° Z)";
    }, "Rotate failed");
    
    // ================================================================
    // UTILITY: CLEANUP
    // ================================================================
    
    private async Task Test_DeleteTestBox() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.DeleteShape("TestBox");
        return $"Deleted TestBox. Remaining shapes: {result.Count}";
    }, "Delete failed");
    
    private async Task Test_ClearAll() => await ExecuteTest(() =>
    {
        Shape3DTech.ClearShapes();
        return $"Cleared all shapes from scene";
    }, "Clear failed");
    
    // ================================================================
    // CATEGORY: QUERY
    // ================================================================
    
    private async Task Test_ListAllShapes() => await ExecuteTest(() =>
    {
        var shapes = Shape3DTech.GetShapes();
        if (shapes.Count == 0)
        {
            return "No shapes in scene";
        }
        
        var shapeList = string.Join(", ", shapes.Select(s => $"{s.Name}({s.Color})"));
        return $"Found {shapes.Count} shapes: {shapeList}";
    }, "List shapes failed");
    
    private async Task Test_GetTestBoxDetails() => await ExecuteTest(() =>
    {
        var shapes = Shape3DTech.GetShapes();
        var testBox = shapes.FirstOrDefault(s => s.Name == "TestBox");
        
        if (testBox == null)
        {
            return "TestBox not found in scene";
        }
        
        return $"TestBox: Color={testBox.Color}, Pos=({testBox.X:F1},{testBox.Y:F1},{testBox.Z:F1}), Size=({testBox.Width:F1}×{testBox.Height:F1}×{testBox.Depth:F1})";
    }, "Get details failed");
    
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
