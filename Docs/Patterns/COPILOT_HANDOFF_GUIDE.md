# Three2025 Project Handoff Guide for Future Copilot Agents

## Project Overview

**Project**: Three2025 - Blazor Server application with 3D rendering capabilities  
**Repository**: Three2025 (Owner: SteveStrong)  
**Branch**: develop  
**Last Updated**: November 14, 2025  

## Quick Start for New Agents

### 1. Understanding the Architecture
- **Main App**: Three2025 (Blazor Server .NET 9.0)
- **External Library**: FoundryWorldsAndDrawings (Referenced but not in workspace)
- **3D Rendering**: Three.js integration via JavaScript interop
- **UI Framework**: Radzen Blazor components

### 2. Getting the Application Running
```powershell
cd C:\Users\admin\workspace\Core\Three2025
dotnet run
```
**Test URL**: http://localhost:5228/Home

### 3. Common Issues You'll Encounter

#### JavaScript Namespace Issues
**Symptoms**: "FoundryWorldsAndDrawings was undefined"  
**Files to Check**:
- `Components/App.razor` - Script loading order
- `wwwroot/js/foundry-namespace-shim.js` - Namespace availability
- Browser console for errors

#### Canvas3D Not Rendering
**Files to Check**:
- `Components/Pages/Home.razor.cs` - Component usage
- `Components/_Imports.razor` - Namespace imports
- Browser developer tools - WebGL errors

## Critical File Map

### Core Application Files
```
Three2025/
├── Program.cs                           # App configuration & DI setup
├── Three2025.csproj                     # Project references & packages
├── Components/
│   ├── App.razor                        # Script loading & layout
│   ├── _Imports.razor                   # Global using statements
│   └── Pages/
│       ├── Home.razor                   # Main 3D canvas page
│       ├── Home.razor.cs                # Component logic & 3D operations
│       ├── Drawing.razor                # Secondary 3D page
│       └── *.razor.cs                   # Other page components
├── wwwroot/js/
│   ├── foundry-namespace-shim.js        # JavaScript namespace fix
│   ├── script-monitor.js                # Script loading detection
│   └── canvas-debug.js                  # Debugging utilities
└── Services/
    └── Visualization/                   # Geometry services
```

### External Dependencies (Referenced but not in workspace)
```
FoundryWorldsAndDrawings/               # External library
├── JsLib/src/index.ts                 # JavaScript source
├── wwwroot/js/app-lib.js              # Compiled JS library
├── Shared/Canvas3DComponent.razor.cs   # Main 3D component
└── ThreeD/Viewers/ViewerThreeD.cs     # JavaScript interop
```

## Known Working Solutions

### JavaScript Integration Fix (November 2025)
**Problem**: Canvas3D components failing due to undefined JavaScript namespace  
**Solution**: Implemented namespace shim pattern  
**Files Modified**:
- Added `wwwroot/js/foundry-namespace-shim.js`
- Added `wwwroot/js/script-monitor.js` 
- Updated `Components/App.razor` script order
- Enhanced `wwwroot/js/canvas-debug.js`

**Key Pattern**: Always ensure JavaScript namespace exists before components try to use it.

## Debugging Toolkit

### Browser Console Commands
```javascript
// Check namespace status
window.checkFoundryNamespace();

// Debug canvas elements
window.debugCanvas();

// Monitor script loading
document.querySelectorAll('script[src*="app-lib.js"]');
```

### Terminal Commands
```powershell
# Kill stuck processes
Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"} | Stop-Process -Force

# Clean build
dotnet clean && dotnet build

# Check port usage
netstat -ano | findstr :5228
```

## Component Usage Patterns

### Canvas3D Component Usage
```csharp
// In *.razor.cs files
public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
        // Work with scene...
    }
}
```

### Adding 3D Objects
```csharp
// Example from Home.razor.cs
public void DoAddConeToScene()
{
    var arena = Workspace.GetArena();
    var (found, scene) = arena.CurrentScene();
    if (!found) return;

    var mesh = new Mesh3D
    {
        Geometry = new ConeGeometry(radius: 0.5f, height: 2),
        Material = new MeshStandardMaterial() { Color = color }
    };
    scene.AddChild(mesh);
}
```

## Common Troubleshooting Scenarios

### Scenario 1: Application Won't Start
**Symptoms**: Port already in use, build errors  
**Actions**:
1. Check for existing dotnet processes: `Get-Process | Where-Object {$_.ProcessName -like "*dotnet*"}`
2. Kill if necessary: `taskkill /PID [PID] /F`
3. Clean build: `dotnet clean && dotnet build`

### Scenario 2: 3D Content Not Displaying
**Symptoms**: Blank canvas, no 3D objects  
**Actions**:
1. Open browser developer tools
2. Check console for JavaScript errors
3. Run `window.debugCanvas()` in console
4. Verify WebGL context creation
5. Check network tab for failed script loads

### Scenario 3: JavaScript Interop Failures
**Symptoms**: "Function not defined" errors  
**Actions**:
1. Run `window.checkFoundryNamespace()` in console
2. Check script loading order in App.razor
3. Verify FoundryWorldsAndDrawings reference in .csproj
4. Check static web asset paths

## Service Dependencies

### Key Services in Program.cs
```csharp
// External library services
builder.Services.AddFoundryWorldsAndDrawingsServices(envConfig);

// Application services
builder.Services.AddScoped<IRackTech, RackTech>();
builder.Services.AddScoped<ICageTech, CageTech>();
builder.Services.AddScoped<IGeometryVisualizationService, GeometryVisualizationService>();
```

## Important Configuration

### Project References
```xml
<!-- In Three2025.csproj -->
<ProjectReference Include="..\FoundryWorldsAndDrawings\FoundryWorldsAndDrawings.csproj" />
```

### Static File Serving
```csharp
// In Program.cs
app.UseStaticFiles(); // Serves wwwroot
// External library assets served automatically via _content/[LibraryName]/
```

## Testing Checklist

When making changes, verify:
- [ ] Application starts without errors: `dotnet run`
- [ ] Home page loads: http://localhost:5228/Home
- [ ] No JavaScript console errors
- [ ] Canvas3D components render 3D content
- [ ] Browser console commands work: `window.debugCanvas()`
- [ ] 3D object creation functions work (try "Add Cone" button)

## Red Flags - Issues to Watch For

### JavaScript Issues
- "FoundryWorldsAndDrawings was undefined" errors
- Script loading 404 errors in network tab
- WebGL context creation failures
- Three.js not loaded warnings

### Component Issues  
- Canvas3DReference is null in OnAfterRenderAsync
- Scene not found when adding 3D objects
- Component lifecycle timing problems

### Build Issues
- FoundryWorldsAndDrawings project reference failures
- Static web asset path mismatches
- Missing JavaScript compilation outputs

## Contact Points for Help

### Documentation Resources
- `FOUNDRY_JAVASCRIPT_INTEGRATION_GUIDE.md` - JavaScript integration patterns
- `CANVAS3D_RENDERING_DIAGNOSTIC_PROMPT.md` - Original diagnostic guide
- Browser developer tools - Runtime debugging
- GitHub repository history - Previous solutions

### Code Examples
- `Components/Pages/Home.razor.cs` - Comprehensive 3D object creation examples
- `wwwroot/js/*.js` - JavaScript integration patterns
- `Components/_Imports.razor` - Namespace import patterns

## Success Patterns That Work

1. **Defensive JavaScript**: Always check namespace availability before use
2. **Event-Driven Architecture**: Use custom events for timing coordination  
3. **Comprehensive Logging**: Console.log everything for debugging
4. **Graceful Degradation**: Provide stubs when libraries fail to load
5. **Progressive Enhancement**: Start simple, add complexity incrementally

## Final Notes

This project integrates complex 3D rendering with Blazor Server using JavaScript interop. The primary challenge is coordinating timing between C# component lifecycle and JavaScript library loading. The solutions implemented focus on defensive programming and providing rich diagnostic information.

When in doubt, check the browser console first - it's your best friend for debugging JavaScript integration issues.

Good luck! 🚀