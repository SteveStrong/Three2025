# Quick Test Panel Implementation Plan

**Version:** 1.0  
**Date:** December 29, 2025  
**Purpose:** Add hardcoded button panel for manual Shape3DTech tool testing

---

## 🎯 Executive Summary

Create a new **Quick Tests** tab in AgentCanvasIntegration with hardcoded buttons that execute preset test scenarios against Shape3DTech tools. All logic in **code-behind** (.razor.cs) for maintainability.

### Two-Phase Testing Strategy

**Phase 1: Manual Tool Verification** ← THIS COMPONENT
- Verify tools execute correctly with known inputs
- Find bugs in tool implementation
- Fix tools until they produce expected results
- Iterate rapidly with new test cases

**Phase 2: LLM Integration Testing** (After Phase 1 Complete)
- Verify LLM can discover and understand tools
- Test LLM's ability to choose correct tool for task
- Validate LLM provides proper parameters

**This panel focuses on Phase 1** - making sure YOUR tools work before worrying about whether the LLM can use them.

---

## 🚀 Super Easy Extension Pattern

### Adding a New Test (3-Step Process)

**Step 1:** Copy this template into code-behind:
```csharp
private async Task Test_YourTestName() => await ExecuteTest(() =>
{
    var result = Shape3DTech.YourMethod(params);
    return $"Your success message here";
}, "Your failure context");
```

**Step 2:** Add button to UI:
```razor
<button @onclick="Test_YourTestName" style="@GetButtonStyle("#colorcode")">
    Your Button Text
</button>
```

**Step 3:** Done. Run and test.

### Real Example - Adding "Create Orange Torus" Test

**In .razor.cs (code-behind):**
```csharp
private async Task Test_CreateOrangeTorus() => await ExecuteTest(() =>
{
    var result = Shape3DTech.AddShape("OrangeTorus", true, "orange", "torus");
    return $"Created OrangeTorus. Total shapes: {result.Count}";
}, "Create OrangeTorus failed");
```

**In .razor (UI):**
```razor
<button @onclick="Test_CreateOrangeTorus" style="@GetButtonStyle("#ff6600")">
    Create Orange Torus
</button>
```

**That's it.** No try-catch, no repetitive code, just logic and message.

---

## 📐 Architecture Overview

### Component Structure

```
Components/
  └─ Shared/
      └─ Testing/
          ├─ QuickTestPanel.razor          # UI markup only
          └─ QuickTestPanel.razor.cs       # All logic in code-behind
```

### Integration Point

**AgentCanvasIntegration.razor**  
Add new tab: `🎯 Quick Tests` alongside existing tabs:
- 💬 AI Chat
- 🧪 Shape3DTech (reflection-based)
- 🏗️ ModelTech
- 🎨 Shape2DTech
- 📊 Mentor2DTech
- **🎯 Quick Tests** ← NEW

---

## 📝 Implementation Details

### Phase 1: Create QuickTestPanel Component

#### File: `Components/Shared/Testing/QuickTestPanel.razor.cs`

```csharp
using Microsoft.AspNetCore.Components;
using Three2025.Apprentice;
using System;
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
    
    // ================================================================
    // CATEGORY: SHAPE CREATION
    // ================================================================
    
    private async Task Test_CreateRedBox() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.AddShape("RedBox", true, "red", "box");
        return $"Created RedBox. Total shapes: {result.Count}";
    }, "Create RedBox failed");
    
    private async Task Test_CreateBlueBoxCustomSize() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.AddShapeWithDimensions(
            name: "BlueBox", 
            isOn: true, 
            color: "blue", 
            shapeType: "box",
            width: 3.0, 
            height: 2.0, 
            depth: 1.5
        );
        return $"Created BlueBox (3×2×1.5). Total shapes: {result.Count}";
    }, "Create BlueBox failed");
    
    private async Task Test_CreateGreenSphere() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.AddShape("GreenSphere", true, "green", "sphere");
        return $"Created GreenSphere. Total shapes: {result.Count}";
    }, "Create GreenSphere failed");
    
    private async Task Test_CreateYellowCylinder() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.AddShape("YellowCylinder", true, "yellow", "cylinder");
        return $"Created YellowCylinder. Total shapes: {result.Count}";
    }, "Create YellowCylinder failed");
    
    private async Task Test_CreatePurpleCone() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.AddShape("PurpleCone", true, "purple", "cone");
        return $"Created PurpleCone. Total shapes: {result.Count}";
    }, "Create PurpleCone failed");
    
    // ================================================================
    // CATEGORY: COLOR CHANGES
    // ================================================================
    
    private async Task Test_ChangeRedBoxToOrange() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("RedBox", "orange");
        return $"Changed RedBox to orange. Total shapes: {result.Count}";
    }, "Change color failed");
    
    private async Task Test_ChangeBlueBoxToPurple() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeColor("BlueBox", "purple");
        return $"Changed BlueBox to purple. Total shapes: {result.Count}";
    }, "Change color failed");
    
    private async Task Test_ChangeGreenSphereToRandomColor() => await ExecuteTest(() =>
    {
        var randomColor = Shape3DTech.PickARandomColor();
        var result = Shape3DTech.ChangeColor("GreenSphere", randomColor);
        return $"Changed GreenSphere to {randomColor}. Total shapes: {result.Count}";
    }, "Change color failed");
    
    private async Task Test_ChangeAllToRandomColors() => await ExecuteTest(() =>
    {
        var shapes = Shape3DTech.GetShapes();
        int changedCount = 0;
        
        foreach (var shape in shapes)
        {
            var randomColor = Shape3DTech.PickARandomColor();
            Shape3DTech.ChangeColor(shape.Name, randomColor);
            changedCount++;
        }
        
        return $"Changed {changedCount} shapes to random colors";
    }, "Batch color change failed");
    
    // ================================================================
    // CATEGORY: TRANSFORMATIONS
    // ================================================================
    
    private async Task Test_MoveRedBoxToOrigin()
    {
        try
        {
            var result = Shape3DTech.RepositionShape("RedBox", x: 0, y: 0, z: 0);
            SetTestResult($"✅ Moved RedBox to origin (0,0,0). Total shapes: {result.Count}", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ Reposition failed: {ex.Message}", "error");
        }
        await Task.CompletedTask;
    }
    
    private async Task Test_MoveRedBoxToPosition()
    {
        try
        {
            var result = Shape3DTech.RepositionShape("RedBox", x: 5.0, y: 2.0, z: -3.0);
            SetTestResult($"✅ Moved RedBox to (5, 2, -3). Total shapes: {result.Count}", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ Reposition failed: {ex.Message}", "error");
        }
        await Task.CompletedTask;
    }
    
    private async Task Test_RotateBlueBox45Degrees()
    {
        try
        {
            var result = Shape3DTech.RotateShape => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("RedBox", x: 0, y: 0, z: 0);
        return $"Moved RedBox to origin (0,0,0). Total shapes: {result.Count}";
    }, "Reposition failed");
    
    private async Task Test_MoveRedBoxToPosition() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RepositionShape("RedBox", x: 5.0, y: 2.0, z: -3.0);
        return $"Moved RedBox to (5, 2, -3). Total shapes: {result.Count}";
    }, "Reposition failed");
    
    private async Task Test_RotateBlueBox45Degrees() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("BlueBox", xDegrees: 45, yDegrees: 0, zDegrees: 0);
        return $"Rotated BlueBox 45° on X-axis. Total shapes: {result.Count}";
    }, "Rotate failed");
    
    private async Task Test_RotateBlueBoxComplex() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.RotateShape("BlueBox", xDegrees: 45, yDegrees: 30, zDegrees: 90);
        return $"Rotated BlueBox (45°, 30°, 90°). Total shapes: {result.Count}";
    }, "Rotate failed");
    
    private async Task Test_ScaleGreenSphere() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ScaleShape("GreenSphere", scaleX: 1.5, scaleY: 1.5, scaleZ: 1.5);
        return $"Scaled GreenSphere to 1.5x. Total shapes: {result.Count}";
    }, "Scale failed");
    
    private async Task Test_ScaleGreenSphereNonUniform() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ScaleShape("GreenSphere", scaleX: 2.0, scaleY: 1.0, scaleZ: 0.5);
        return $"Scaled GreenSphere non-uniformly (2×1×0.5). Total shapes: {result.Count}";
    }, "Scale failed");
    
    private async Task Test_ToggleAllVisibility()
    {
        try
        {
            var shapes = Shape3DTech.GetShapes();
            foreach (var shape in shapes)
            {
                Shape3DTech.ChangeState(shape.Name, !shape.IsOn);
            }
            SetTestResult($"✅ Toggled visibility for {shapes.Count} shapes", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ Toggle visibility failed: {ex.Message}", "error");
        }
        await Task.CompletedTask;
    }
    
    // ================================================================
    // CATEGORY: COMPLEX WORKFLOWS
    // ================================================================
    
    private async Task Test_CreateBoxAndChangeColor()
    {
        try
        {
            // Step 1: Create box
            var createResult = Shape3DTech.AddShape("WorkflowBox", true, "red", "box");
            SetTestResult($"⏳ Step 1/2: Created WorkflowBox...", "info");
            await Task.Delay(800); // Visual delay for user to see progression
            
            // Step 2: Change color
            var colorResult = Shape3DTech.ChangeColor("WorkflowBox", "blue");
            SetTestResult($"✅ Workflow complete: Created WorkflowBox and changed to blue. Total shapes: {colorResult.Count}", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ Workflow failed: {ex.Message}", "error");
        }
    }
    
    private async Task Test_CreateMoveRotateScale()
    {
        try
        {
            // Create
            var createResult = Shape3DTech.AddShape("ComplexBox", true, "purple", "box");
            SetTestResult($"⏳ Step 1/4: Created ComplexBox...", "info");
            await Task.Delay(600);
            
            // Move
            var moveResult = Shape3DTech.RepositionShape("ComplexBox", 3.0, 1.0, -2.0);
            SetTestResult($"⏳ Step 2/4:  => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeState("RedBox", false);
        return $"Hidden RedBox. Total shapes: {result.Count}";
    }, "Hide failed");
    
    private async Task Test_ShowRedBox() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.ChangeState("RedBox", true);
        return $"Shown RedBox. Total shapes: {result.Count}";
    }, "Show failed");
    
    private async Task Test_ToggleAllVisibility() => await ExecuteTest(() =>
    {
        var shapes = Shape3DTech.GetShapes();
        foreach (var shape in shapes)
        {
            Shape3DTech.ChangeState(shape.Name, !shape.IsOn);
        } => await ExecuteTestAsync(async () =>
    {
        // Step 1: Create box
        var createResult = Shape3DTech.AddShape("WorkflowBox", true, "red", "box");
        SetTestResult($"⏳ Step 1/2: Created WorkflowBox...", "info");
        await Task.Delay(800);
        
        // Step 2: Change color
        var colorResult = Shape3DTech.ChangeColor("WorkflowBox", "blue");
        return $"Workflow complete: Created WorkflowBox and changed to blue. Total shapes: {colorResult.Count}";
    }, "Workflow failed");
    
    private async Task Test_CreateMoveRotateScale() => await ExecuteTestAsync(async () =>
    {
        // Create
        var createResult = Shape3DTech.AddShape("ComplexBox", true, "purple", "box");
        SetTestResult($"⏳ Step 1/4: Created ComplexBox...", "info");
        await Task.Delay(600);
        
        // Move
        var moveResult = Shape3DTech.RepositionShape("ComplexBox", 3.0, 1.0, -2.0);
        SetTestResult($"⏳ Step 2/4: Moved ComplexBox...", "info");
        await Task.Delay(600);
        
        // Rotate
        var rotateResult = Shape3DTech.RotateShape("ComplexBox", 30, 45, 60);
        SetTestResult($"⏳ Step 3/4: Rotated ComplexBox...", "info");
        await Task.Delay(600);
        
        // Scale
        var scaleResult = Shape3DTech.ScaleShape("ComplexBox", 1.5, 1.5, 1.5);
        return $"Workflow complete: Created, moved, rotated, and scaled ComplexBox. Total shapes: {scaleResult.Count}";
    }, "Workflow failed");
    
    private async Task Test_DuplicateAndModify() => await ExecuteTestAsync(async () =>
    {
        // Create original
        var createResult = Shape3DTech.AddShape("Original", true, "red", "sphere");
        SetTestResult($"⏳ Step 1/3: Created Original sphere...", "info");
        await Task.Delay(600);
        
        // Duplicate
        var dupResult = Shape3DTech.DuplicateShape("Original", "Copy1", offsetX: 3.0, offsetY: 0, offsetZ: 0);
        SetTestResult($"⏳ Step 2/3: Duplicated to Copy1...", "info");
        await Task.Delay(600);
        
        // Change copy color
        var colorResult = Shape3DTech.ChangeColor("Copy1", "blue");
        return $"Workflow complete: Created Original, duplicated as Copy1, changed copy to blue. Total shapes: {colorResult.Count}";
    }, "Workflow failed");   {
            var result = Shape3DTech.DeleteShape("BlueBox");
            SetTestResult($"✅ Deleted BlueBox. Remaining shapes: {result.Count}", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ Delete failed: {ex.Message}", "error");
        }
        await Task.CompletedTask;
    }
    
    private async Task Test_ClearAllShapes()
    {
        try
        {
            Shape3DTech.ClearShapes();
            SetTestResult($"✅ Cleared all shapes from scene", "success");
        }
        catch (Exception ex)
        {
            SetTestResult($"❌ Clear failed: {ex.Message}", "error");
        }
        await Task.CompletedTask;
    }
    
    // ================================================================
    // UTILITY METHODS
    // ==================================== => await ExecuteTest(() =>
    { => await ExecuteTest(() =>
    {
        var result = Shape3DTech.DeleteShape("RedBox");
        return $"Deleted RedBox. Remaining shapes: {result.Count}";
    }, "Delete failed");
    
    private async Task Test_DeleteBlueBox() => await ExecuteTest(() =>
    {
        var result = Shape3DTech.DeleteShape("BlueBox");
        return $"Deleted BlueBox. Remaining shapes: {result.Count}";
    }, "Delete failed");
    
    private async Task Test_ClearAllShapes() => await ExecuteTest(() =>
    {
        Shape3DTech.ClearShapes();
        return $"Cleared all shapes from scene";
    }, "Clear failed");zor
@using Three2025.Apprentice

<div style="display: flex; flex-direction: column; height: 100%; padding: 1rem; overflow-y: auto; background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);">
    
    <!-- Header -->
    <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 4px 6px rgba(0,0,0,0.1);">
        <h3 style="margin: 0; font-size: 1.2rem; font-weight: 700;">🎯 Quick Test Scenarios</h3>
        <p style="margin: 0.5rem 0 0 0; font-size: 0.85rem; opacity: 0.9;">
            Hardcoded tests with preset parameters for rapid API validation
        </p>
    </div>
    
    <!-- Test Result Display -->
    @if (!string.IsNullOrEmpty(lastTestResult))
    {
        <div style="padding: 0.75rem; border-radius: 6px; margin-bottom: 1rem; 
                    background: @GetResultBackground(); 
                    border: 2px solid @GetResultBorder(); 
                    color: @GetResultColor();
                    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
                    animation: slideIn 0.3s ease-out;">
            <strong style="font-size: 0.9rem;">Last Test:</strong><br/>
            <span style="font-size: 0.85rem;">@lastTestResult</span>
        </div>
    }
    
    <!-- Shape Creation Tests -->
    <div style="background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
        <h4 style="margin: 0 0 0.75rem 0; color: #28a745; border-bottom: 2px solid #28a745; padding-bottom: 0.5rem; font-size: 1rem;">
            📦 Shape Creation
        </h4>
        <div style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button @onclick="Test_CreateRedBox" style="@GetButtonStyle("#dc3545")">
                Create Red Box
            </button>
            <button @onclick="Test_CreateBlueBoxCustomSize" style="@GetButtonStyle("#0dcaf0")">
                Create Blue Box (3×2×1.5)
            </button>
            <button @onclick="Test_CreateGreenSphere" style="@GetButtonStyle("#28a745")">
                Create Green Sphere
            </button>
            <button @onclick="Test_CreateYellowCylinder" style="@GetButtonStyle("#ffc107")">
                Create Yellow Cylinder
            </button>
            <button @onclick="Test_CreatePurpleCone" style="@GetButtonStyle("#6f42c1")">
                Create Purple Cone
            </button>
        </div>
    </div>
    
    <!-- Color Change Tests -->
    <div style="background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
        <h4 style="margin: 0 0 0.75rem 0; color: #fd7e14; border-bottom: 2px solid #fd7e14; padding-bottom: 0.5rem; font-size: 1rem;">
            🎨 Color Changes
        </h4>
        <div style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button @onclick="Test_ChangeRedBoxToOrange" style="@GetButtonStyle("#fd7e14")">
                Change RedBox → Orange
            </button>
            <button @onclick="Test_ChangeBlueBoxToPurple" style="@GetButtonStyle("#6f42c1")">
                Change BlueBox → Purple
            </button>
            <button @onclick="Test_ChangeGreenSphereToRandomColor" style="@GetButtonStyle("#20c997")">
                Change GreenSphere → Random
            </button>
            <button @onclick="Test_ChangeAllToRandomColors" style="@GetButtonStyle("#d63384")">
                Change All → Random Colors
            </button>
        </div>
    </div>
    
    <!-- Transformation Tests -->
    <div style="background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
        <h4 style="margin: 0 0 0.75rem 0; color: #0d6efd; border-bottom: 2px solid #0d6efd; padding-bottom: 0.5rem; font-size: 1rem;">
            🔧 Transformations
        </h4>
        <div style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button @onclick="Test_MoveRedBoxToOrigin" style="@GetButtonStyle("#0d6efd")">
                Move RedBox → Origin (0,0,0)
            </button>
            <button @onclick="Test_MoveRedBoxToPosition" style="@GetButtonStyle("#0d6efd")">
                Move RedBox → (5, 2, -3)
            </button>
            <button @onclick="Test_RotateBlueBox45Degrees" style="@GetButtonStyle("#0d6efd")">
                Rotate BlueBox → 45° (X-axis)
            </button>
            <button @onclick="Test_RotateBlueBoxComplex" style="@GetButtonStyle("#0d6efd")">
                Rotate BlueBox → (45°, 30°, 90°)
            </button>
            <button @onclick="Test_ScaleGreenSphere" style="@GetButtonStyle("#0d6efd")">
                Scale GreenSphere → 1.5x
            </button>
            <button @onclick="Test_ScaleGreenSphereNonUniform" style="@GetButtonStyle("#0d6efd")">
                Scale GreenSphere → (2×1×0.5)
            </button>
        </div>
    </div>
    
    <!-- Visibility Tests -->
    <div style="background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
        <h4 style="margin: 0 0 0.75rem 0; color: #6c757d; border-bottom: 2px solid #6c757d; padding-bottom: 0.5rem; font-size: 1rem;">
            👁️ Visibility
        </h4>
        <div style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button @onclick="Test_HideRedBox" style="@GetButtonStyle("#6c757d")">
                Hide RedBox
            </button>
            <button @onclick="Test_ShowRedBox" style="@GetButtonStyle("#198754")">
                Show RedBox
            </button>
            <button @onclick="Test_ToggleAllVisibility" style="@GetButtonStyle("#6610f2")">
                Toggle All Visibility
            </button>
        </div>
    </div>
    
    <!-- Complex Workflow Tests -->
    <div style="background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
        <h4 style="margin: 0 0 0.75rem 0; color: #9b59b6; border-bottom: 2px solid #9b59b6; padding-bottom: 0.5rem; font-size: 1rem;">
            🎭 Complex Workflows
        </h4>
        <div style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button @onclick="Test_CreateBoxAndChangeColor" style="@GetButtonStyle("#9b59b6")">
                Create Box → Change Color
            </button>
            <button @onclick="Test_CreateMoveRotateScale" style="@GetButtonStyle("#9b59b6")">
                Create → Move → Rotate → Scale
            </button>
            <button @onclick="Test_DuplicateAndModify" style="@GetButtonStyle("#9b59b6")">
                Duplicate Shape → Modify Copy
            </button>
        </div>
    </div>
    
    <!-- Query & Info Tests -->
    <div style="background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
        <h4 style="margin: 0 0 0.75rem 0; color: #17a2b8; border-bottom: 2px solid #17a2b8; padding-bottom: 0.5rem; font-size: 1rem;">
            🔍 Query & Info
        </h4>
        <div style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button @onclick="Test_ListAllShapes" style="@GetButtonStyle("#17a2b8")">
                📋 List All Shapes
            </button>
            <button @onclick="Test_GetShapeDetails" style="@GetButtonStyle("#17a2b8")">
                🔍 Get First Shape Details
            </button>
        </div>
    </div>
    
    <!-- Cleanup Tests -->
    <div style="background: white; padding: 1rem; border-radius: 8px; margin-bottom: 1rem; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
        <h4 style="margin: 0 0 0.75rem 0; color: #d63384; border-bottom: 2px solid #d63384; padding-bottom: 0.5rem; font-size: 1rem;">
            🧹 Cleanup & Deletion
        </h4>
        <div style="display: flex; flex-direction: column; gap: 0.5rem;">
            <button @onclick="Test_DeleteRedBox" style="@GetButtonStyle("#dc3545")">
                🗑️ Delete RedBox
            </button>
            <button @onclick="Test_DeleteBlueBox" style="@GetButtonStyle("#dc3545")">
                🗑️ Delete BlueBox
            </button>
            <button @onclick="Test_ClearAllShapes" style="@GetButtonStyle("#d63384")">
                🧹 Clear All Shapes
            </button>
        </div>
    </div>
    
</div>

<style>
    @@keyframes slideIn {
        from {
            opacity: 0;
            transform: translateY(-10px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }
    
    button:hover {
        opacity: 0.85;
        transform: translateY(-2px);
        box-shadow: 0 4px 8px rgba(0,0,0,0.2) !important;
    }
    
    button:active {
        transform: translateY(0);
        box-shadow: 0 2px 4px rgba(0,0,0,0.1) !important;
    }
</style>
```

---

### Phase 2: Integrate into AgentCanvasIntegration

#### Modification 1: Add Tab Button

**File:** `Components/Pages/AgentCanvasIntegration.razor`

**Location:** Around line 60-65 (after existing tabs)

**Add this tab button:**

```razor
<li class="nav-item" role="presentation">
    <button class="nav-link @(activeChatTab == "quicktest" ? "active" : "")" 
            @onclick="@(() => activeChatTab = "quicktest")" 
            type="button" role="tab"
            style="font-size: 0.9rem; font-weight: 600;">
        🎯 Quick Tests
    </button>
</li>
```

#### Modification 2: Add Tab Content

**File:** `Components/Pages/AgentCanvasIntegration.razor`

**Location:** Around line 135-140 (after existing else if blocks for other tabs)

**Add this content block:**

```razor
else if (activeChatTab == "quicktest")
{
    <QuickTestPanel />
}
```

---

### Phase 3: Update Imports

#### Modification: Add Using Directive

**File:** `Components/_Imports.razor`

**Add this line:**

```razor
@using Three2025.Components.Shared.Testing
```

---

## 🎨 Visual Design Features

### Color-Coded Categories
- **Shape Creation** (Green): #28a745
- **Color Changes** (Orange): #fd7e14
- **Transformations** (Blue): #0d6efd
- **Visibility** (Gray): #6c757d
- **Complex Workflows** (Purple): #9b59b6
- **Query & Info** (Cyan): #17a2b8
- **Cleanup** (Pink): #d63384

### Interactive Features
- Hover effects on buttons (lift + shadow)
- Animated result banner (slide-in)
- Status-based colors (success/error/warning/info)
- Smooth transitions

### Layout
- Scrollable panel for many tests
- Compact grouping by category
- Full-width buttons for easy clicking
- Gradient background for visual appeal

---

## 📊 Test Categories Breakdown

### 1. Shape Creation (5 tests)
- Create basic shapes with default colors
- Create custom-sized shapes
- Test different shape types (box, sphere, cylinder, cone)

### 2. Color Changes (4 tests)
- Change specific shape to specific color
- Change shape to random color
- Batch color changes

### 3. Transformations (6 tests)
- Position to origin
- Position to specific coordinates
- Simple rotation (single axis)
- Complex rotation (multiple axes)
- Uniform scaling
- Non-uniform scaling

### 4. Visibility (3 tests)
- Hide specific shape
- Show specific shape
- Toggle all shapes

### 5. Complex Workflows (3 tests)
- Create → Color change (2-step)
- Create → Move → Rotate → Scale (4-step)
- Duplicate → Modify copy (3-step)

### 6. Query & Info (2 tests)
- List all shapes with details
- Get detailed info about first shape

### 7. Cleanup (3 tests)
- Delete specific shapes
- Clear entire scene

**Total: 26 preset test scenarios**

---

## 🔄 Workflow Example

**User Workflow:**

1. **Click** "Create Red Box"
   - Executes: `Shape3DTech.AddShape("RedBox", true, "red", "box")`
   - Shows: "✅ Created RedBox. Total shapes: 1"

2. **Click** "Change RedBox → Orange"
   - Executes: `Shape3DTech.ChangeColor("RedBox", "orange")`
   - Shows: "✅ Changed RedBox to orange. Total shapes: 1"

3. **Click** "Move RedBox → (5, 2, -3)"
   - Executes: `Shape3DTech.RepositionShape("RedBox", 5.0, 2.0, -3.0)`
   - Shows: "✅ Moved RedBox to (5, 2, -3). Total shapes: 1"

4. **Click** "Delete RedBox"
   - Executes: `Shape3DTech.DeleteShape("RedBox")`
   - Shows: "✅ Deleted RedBox. Remaining shapes: 0"

---

## ✅ Key Advantages for Rapid Tool Verification

1. **Brain-Dead Simple to Extend**: Copy 4 lines, change values, done
2. **No Boilerplate**: Zero try-catch blocks, zero repetitive code
3. **Instant Visual Feedback**: See success/error immediately
4. **Test-Driven Tool Development**: Write test → Run → Fix tool → Run again
5. **Multi-Step Workflows**: Verify complex tool sequences
6. **Catch Tool Bugs Early**: Before wasting time with LLM integration
7. **Fast Iteration**: Add 10 tests in 5 minutes
8. **Complements AI Testing**: Manual tests prove tools work, then test LLM usage

### Why This Matters for Your Workflow

**Without this panel:**
- Find tool bug during LLM testing → Hard to isolate problem
- Is the tool broken or is the LLM using it wrong?
- Slow debugging cycle

**With this panel:**
- Test tool directly → See exactly what it returns
- Fix tool bugs immediately
- Prove tool works perfectly
- Now you KNOW if LLM issue is bad tool vs bad LLM parameters


---

## 🎓 Extension Guide - Copy This Every Time

### Template for New Single-Action Test

```csharp
// In QuickTestPanel.razor.cs
private async Task Test_DescriptiveName() => await ExecuteTest(() =>
{
    var result = Shape3DTech.MethodName(param1, param2, param3);
    return $"What happened successfully";
}, "What operation failed");
```

```razor
<!-- In QuickTestPanel.razor - add to appropriate category section -->
<button @onclick="Test_DescriptiveName" style="@GetButtonStyle("#colorcode")">
    Button Label
</button>
```

### Template for Multi-Step Workflow Test

```csharp
// In QuickTestPanel.razor.cs
private async Task Test_WorkflowName() => await ExecuteTestAsync(async () =>
{
    // Step 1
    var step1Result = Shape3DTech.Method1(params);
    SetTestResult($"⏳ Step 1/3: Description...", "info");
    await Task.Delay(600);
    
    // Step 2
    var step2Result = Shape3DTech.Method2(params);
    SetTestResult($"⏳ Step 2/3: Description...", "info");
    await Task.Delay(600);
    
    // Step 3
    var finalResult = Shape3DTech.Method3(params);
    return $"Workflow complete: Summary of what happened";
}, "Workflow failed");
```

```razor
<!-- In QuickTestPanel.razor - usually in Complex Workflows section -->
<button @onclick="Test_WorkflowName" style="@GetButtonStyle("#9b59b6")">
    Workflow Description
</button>
```

### Color Codes by Category

Use these in `GetButtonStyle("#color")`:

| Category | Color Code | Example |
|----------|-----------|---------|
| Creation | `#28a745` | Create actions |
| Color Changes | `#fd7e14` | Modify colors |
| Transformations | `#0d6efd` | Move/rotate/scale |
| Visibility | `#6c757d` | Show/hide |
| Workflows | `#9b59b6` | Multi-step |
| Query | `#17a2b8` | Get info |
| Deletion | `#dc3545` | Delete/clear |

### Quick Checklist for Each New Test

- [ ] Copy template
- [ ] Change method name to `Test_WhatItDoes`
- [ ] Update `Shape3DTech.MethodName` to actual method
- [ ] Write clear success message
- [ ] Write clear failure context
- [ ] Add button to appropriate category in .razor
- [ ] Use correct color for category
- [ ] Test it works
- [ ] If tool fails → Fix tool → Test again

---

## 💡 Common Test Patterns

### Testing Tool Returns Correct Data
```csharp
private async Task Test_VerifyShapeData() => await ExecuteTest(() =>
{
    Shape3DTech.AddShape("TestShape", true, "red", "box");
    var shapes = Shape3DTech.GetShapes();
    var testShape = shapes.FirstOrDefault(s => s.Name == "TestShape");
    
    if (testShape == null) throw new Exception("Shape not found!");
    if (testShape.Color != "red") throw new Exception($"Expected red, got {testShape.Color}");
    
    return $"Verified TestShape exists with correct color";
}, "Shape verification failed");
```

### Testing Tool Handles Invalid Input
```csharp
private async Task Test_DeleteNonexistentShape() => await ExecuteTest(() =>
{
    try
    {
        Shape3DTech.DeleteShape("NonExistentShape");
        throw new Exception("Should have thrown exception!");
    }
    catch (InvalidOperationException)
    {
        return $"Correctly rejected invalid shape name";
    }
}, "Error handling test failed");
```

### Testing Tool State Changes
```csharp
private async Task Test_ToggleVisibility() => await ExecuteTest(() =>
{
    Shape3DTech.AddShape("ToggleTest", true, "blue", "sphere");
    var before = Shape3DTech.GetShapes().First(s => s.Name == "ToggleTest").IsOn;
    
    Shape3DTech.ChangeState("ToggleTest", false);
    var after = Shape3DTech.GetShapes().First(s => s.Name == "ToggleTest").IsOn;
    
    if (before == after) throw new Exception("Visibility didn't change!");
    
    return $"Verified visibility toggle: {before} → {after}";
}, "Toggle test failed");
```
---

## 🚀 Implementation Checklist

- [ ] Create `QuickTestPanel.razor.cs` with all test methods
- [ ] Create `QuickTestPanel.razor` with UI markup
- [ ] Modify `AgentCanvasIntegration.razor` to add new tab button
- [ ] Modify `AgentCanvasIntegration.razor` to add tab content
- [ ] Add using directive to `_Imports.razor`
- [ ] Test each button individually
- [ ] Test complex workflows
- [ ] Verify error handling
- [ ] Document any issues found

---

## 📈 Future Enhancements (Optional)

### Phase 2: Editable Parameters
- Add input fields for custom values
- Keep preset buttons alongside custom inputs

### Phase 3: Test Recording
- Record successful test sequences
- Replay saved sequences
- Export as test scripts

### Phase 4: Test History
- Log all test executions with timestamps
- Show success/failure statistics
- Filter by category or status

---

## 🎯 Success Criteria

✅ All buttons execute without errors  
✅ Visual feedback shows on every click  
✅ Complex workflows complete all steps  
✅ Error messages are clear and actionable  
✅ Panel integrates seamlessly with existing UI  
✅ Code-behind keeps logic separate from markup  
✅ Easy to add new test scenarios by copying pattern

---

**End of Implementation Plan**
