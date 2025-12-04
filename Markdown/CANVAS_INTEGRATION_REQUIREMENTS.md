# Canvas Integration Requirements for Three2025 Application

## Context for AI Assistant
You previously worked on a clock application that successfully integrated FoundryWorldsAndDrawings canvas components. Now we need to integrate that solution back into this Three2025 application where the canvases are currently not rendering correctly.

## Current Problem State

### What's Broken:
- Canvas components (Canvas3DComponent, Canvas2DComponent) are not rendering correctly in the Three2025 application
- The clock canvas that was working in another application is now not functioning properly
- All canvas-based pages are affected (Clock, Drawing, MatrixTest, Home, etc.)

### What We Just Fixed:
- ✅ Resolved all namespace compilation errors related to `Three2025.Shared` 
- ✅ Updated all component references to use `FoundryWorldsAndDrawings.Shared.Canvas3DComponent`
- ✅ Updated all component references to use `FoundryWorldsAndDrawings.Shared.Canvas2DComponent`
- ✅ Project now builds successfully without errors

### What Still Needs Integration:
- 🔄 Canvas rendering functionality from the working clock application
- 🔄 Proper initialization and lifecycle management of canvas components
- 🔄 Scene management and 3D/2D rendering pipeline
- 🔄 Event handling and user interaction

## Key Questions for Implementation

### For the AI Assistant who worked on the clock application:

**What specific steps did you take to make the canvas components render correctly?**
- What initialization code was required?
- What lifecycle methods needed to be implemented?
- Were there specific JavaScript interop calls needed?
- What dependencies or services needed to be registered?

**What was the working canvas component structure?**
- How was the Canvas3DComponent properly configured?
- What parameters and properties were essential?
- How was the scene management handled?
- What was the correct way to handle the `@ref` bindings?

**What integration challenges did you encounter and solve?**
- Were there specific timing issues with component initialization?
- How did you handle the relationship between Blazor components and Three.js?
- What were the key differences between development and production builds?

**What files need to be examined or updated?**
- Which JavaScript files contain the rendering logic?
- Are there specific CSS files needed for proper canvas display?
- What service registrations are required in Program.cs or startup?
- Which wwwroot assets need to be present and properly referenced?

## Current Application Structure

### Canvas Components in Three2025:
- `Components/Pages/Clock.razor/.razor.cs` - Clock canvas page (should work like in other app)
- `Components/Pages/Drawing.razor/.razor.cs` - 2D drawing canvas page
- `Components/Pages/Home.razor/.razor.cs` - Main 3D scene page
- `Components/Pages/MatrixTest.razor/.razor.cs` - Matrix transformation testing
- Multiple other canvas-dependent pages

### Dependencies Available:
- FoundryWorldsAndDrawings.Shared (Canvas3DComponent, Canvas2DComponent)
- FoundryWorldsAndDrawings.ThreeD.Viewers (ViewerThreeD)
- BlazorThreeJS project (Three.js integration)
- FoundryBlazor project (additional canvas functionality)

### Current Component References:
```csharp
public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;
public FoundryWorldsAndDrawings.Shared.Canvas2DComponent Canvas2DReference = null;
```

## Expected Outcome
After integration, all canvas pages should:
1. Render properly without blank/empty canvases
2. Display 3D scenes with proper lighting and controls
3. Handle user interactions (rotation, zoom, pan)
4. Show 2D graphics with proper drawing capabilities
5. Maintain the clock functionality that was working in the other application

## Debugging Information Needed
1. Are there console errors in the browser developer tools?
2. Are the Three.js libraries loading correctly?
3. Are the JavaScript interop calls succeeding?
4. Is the canvas element being created with proper dimensions?
5. Are the WebGL contexts initializing correctly?

## Files to Investigate
- `/wwwroot/js/` - JavaScript files for canvas functionality
- `/wwwroot/lib/` - Third-party libraries (Three.js, etc.)
- `Program.cs` - Service registrations and app configuration
- Canvas component source files in FoundryWorldsAndDrawings projects
- Any initialization scripts or startup logic

---

**Request to AI Assistant:** Please analyze the current Three2025 application state, compare it with your successful clock application implementation, and provide specific steps to restore proper canvas rendering functionality. Focus on what changed between the working state and current broken state, and what needs to be re-integrated or reconfigured.