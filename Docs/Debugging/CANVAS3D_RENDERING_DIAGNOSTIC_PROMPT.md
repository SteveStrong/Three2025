# Canvas3D Rendering Diagnostic Prompt

## Problem Summary
A Blazor Server application (Three2025) that references a Razor class library (FoundryWorldsAndDrawings) was experiencing Canvas3D component rendering failures due to JavaScript namespace issues. The error "FoundryWorldsAndDrawings was undefined" occurred when ViewerThreeD.cs attempted JavaScript interop calls.

## Current State (After Revert)
The codebase has been reverted to a working state where:
- Canvas3D components should be rendering properly
- JavaScript-C# interop should be working  
- Three.js 3D scenes should be displaying
- The unified animation system integration is functional

## Specific Technical Issue
The error occurred when ViewerThreeD.cs tried to call:
```csharp
await JsRuntime!.InvokeVoidAsync("FoundryWorldsAndDrawings.Initialize3DViewer", json);
```

The JavaScript namespace 'FoundryWorldsAndDrawings' was undefined at the time of this call.

## Required Diagnostic Steps

### 1. Verify Current Working State
**CRITICAL: DO NOT CHANGE ANYTHING until current working state is confirmed**
- Navigate to http://localhost:5228/Home
- Verify Canvas3D components are rendering 3D content
- Verify Canvas2D components are working
- Check browser console for any errors
- Confirm Three.js scenes are displaying properly

### 2. JavaScript Namespace Analysis
Examine the following files to understand current namespace creation:
- `C:\Users\admin\workspace\Core\FoundryWorldsAndDrawings\JsLib\src\index.ts`
- Check how `window.FoundryWorldsAndDrawings` is created
- Verify `Initialize3DViewer` method is properly exposed
- Confirm the Load() function execution timing

### 3. Static Web Asset Configuration
Compare and analyze:
- `C:\Users\admin\workspace\Core\FoundryWorldsAndDrawings\FoundryWorldsAndDrawings.csproj`
  - Check `StaticWebAssetBasePath` value
  - Check `PackageId` value
- `C:\Users\admin\workspace\Core\Three2025\Components\App.razor`
  - Check script src path for app-lib.js
  - Ensure paths align with StaticWebAssetBasePath

### 4. Component Initialization Timing
Examine potential timing issues:
- `C:\Users\admin\workspace\Core\FoundryWorldsAndDrawings\ThreeD\Viewers\ViewerThreeD.cs`
  - Check OnAfterRenderAsync implementation
  - Verify JavaScript call timing
- `C:\Users\admin\workspace\Core\FoundryWorldsAndDrawings\Shared\Canvas3DComponent.razor.cs`
  - Check component lifecycle events
  - Verify unified animation system integration

### 5. Build Process Verification
Check that JavaScript compilation is working:
- Verify `C:\Users\admin\workspace\Core\FoundryWorldsAndDrawings\wwwroot\js\app-lib.js` exists
- Confirm webpack build process completed successfully
- Check that FoundryWorldsAndDrawings project builds without errors

## Implementation Constraints

### What NOT to Change
- Do NOT modify static web asset paths unless absolutely necessary
- Do NOT change component initialization order
- Do NOT alter the unified animation system integration
- Do NOT modify Canvas3D component inheritance hierarchy
- Do NOT change the Three.js WebGL renderer setup

### What CAN be Changed
- JavaScript namespace creation timing
- Script loading order or method
- Error handling around JavaScript interop calls
- Conditional checks for namespace availability

## Target Solution
Implement the MINIMAL necessary changes to ensure `FoundryWorldsAndDrawings.Initialize3DViewer` is available when ViewerThreeD.cs calls it, while preserving all existing working functionality.

## Success Criteria
After implementing the fix:
1. Navigate to /Home - both Canvas2D and Canvas3D components render properly
2. No JavaScript console errors related to undefined namespaces
3. 3D scenes display with proper WebGL rendering
4. Animation system continues to function
5. All existing 3D model loading and rendering capabilities work

## Key Files to Monitor
- `index.ts` - JavaScript namespace creation
- `ViewerThreeD.cs` - C# JavaScript interop calls
- `App.razor` - Script loading
- `FoundryWorldsAndDrawings.csproj` - Static web asset config
- Browser console - Runtime errors

## Debugging Commands
```powershell
# Build JavaScript
cd C:\Users\admin\workspace\Core\FoundryWorldsAndDrawings\JsLib
npm run build

# Start server
cd C:\Users\admin\workspace\Core\Three2025
dotnet run

# Test URL
http://localhost:5228/Home
```

## Expected Timeline
This should be a focused fix taking 15-30 minutes maximum, not a major refactoring. The goal is surgical precision to fix only the namespace availability issue.