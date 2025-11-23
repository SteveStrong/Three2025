# Consuming Foundry Libraries in Razor Pages

## Document Purpose

This guide explains how to integrate FoundryWorldsAndDrawings and FoundryMentorModeler libraries into new or existing Blazor applications, particularly for creating pages with 3D/2D canvas components, animations, and interactive scenes.

**Context**: This documents the successful migration of MultiCanvas2DTest and Clock pages in Three2025 application, serving as a template for future page migrations and new page creation.

---

## Table of Contents

1. [Library Architecture Overview](#library-architecture-overview)
2. [Required NuGet Packages](#required-nuget-packages)
3. [Project Configuration](#project-configuration)
4. [Razor Page Patterns](#razor-page-patterns)
5. [Canvas Component Integration](#canvas-component-integration)
6. [Animation System Integration](#animation-system-integration)
7. [Service Injection Patterns](#service-injection-patterns)
8. [Common Page Types](#common-page-types)
9. [Troubleshooting](#troubleshooting)
10. [Migration Checklist](#migration-checklist)

---

## Library Architecture Overview

### Core Libraries

**FoundryWorldsAndDrawings** (Primary 3D/2D Framework)
- `Canvas3DComponent` - Three.js-powered 3D rendering
- `Canvas2DComponent` - HTML5 Canvas 2D rendering
- Scene management (Scene3D, FoStage3D, Arena)
- Shape primitives (FoShape3D, FoShape2D, FoShape1D)
- Transform system (Transform3 with position/rotation/scale)
- Animation system (AnimationFrameBus)

**FoundryMentorModeler** (Higher-level Abstractions)
- `MentorWorkbook` - Workbook pattern for multi-page applications
- Diagram support
- Evaluation framework
- Advanced modeling constructs

**FoundryRulesAndUnits** (Foundation)
- Mathematical types (Vector3, Euler, Quaternion, Matrix3)
- Unit system
- Extension methods
- Base interfaces

### Architecture Principles

```
Application (Three2025)
    ↓ depends on
FoundryMentorModeler (optional - high-level patterns)
    ↓ depends on
FoundryWorldsAndDrawings (core 3D/2D framework)
    ↓ depends on
FoundryRulesAndUnits (math/units foundation)
```

**Key Insight**: You can consume FoundryWorldsAndDrawings directly for most scenarios. FoundryMentorModeler adds workbook patterns for complex multi-document applications.

---

## Required NuGet Packages

### Minimum Configuration (3D/2D Canvas)

```xml
<ItemGroup>
  <!-- Core Foundry Libraries -->
  <PackageReference Include="ApprenticeFoundryRulesAndUnits" Version="10.7.0" />
  <PackageReference Include="ApprenticeFoundryWorldsAndDrawings" Version="0.10.39" />
  
  <!-- Blazor Extensions for Canvas -->
  <PackageReference Include="Blazor.Extensions.Canvas" Version="1.1.1" />
  
  <!-- UI Framework (optional but recommended) -->
  <PackageReference Include="Radzen.Blazor" Version="5.8.7" />
</ItemGroup>
```

### Full Configuration (with MentorModeler)

```xml
<ItemGroup>
  <!-- All of the above, plus: -->
  <PackageReference Include="ApprenticeFoundryMentorModeler" Version="0.1.27" />
</ItemGroup>
```

### JavaScript Dependencies

Ensure `app-lib.js` is loaded in `App.razor`:

```razor
<script src="_content/ApprenticeFoundryWorldsAndDrawings/app-lib.js"></script>
```

---

## Project Configuration

### 1. Update .csproj

**Three2025.csproj** example:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <!-- Foundry Libraries -->
    <PackageReference Include="ApprenticeFoundryRulesAndUnits" Version="10.7.0" />
    <PackageReference Include="ApprenticeFoundryWorldsAndDrawings" Version="0.10.39" />
    <PackageReference Include="ApprenticeFoundryMentorModeler" Version="0.1.27" />
    
    <!-- Blazor Canvas -->
    <PackageReference Include="Blazor.Extensions.Canvas" Version="1.1.1" />
    
    <!-- UI -->
    <PackageReference Include="Radzen.Blazor" Version="5.8.7" />
  </ItemGroup>
</Project>
```

### 2. Configure Dependency Injection (Program.cs)

```csharp
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.PubSub;

var builder = WebApplication.CreateBuilder(args);

// Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Foundry Services (REQUIRED)
builder.Services.AddSingleton<IFoundryService, FoundryService>();
builder.Services.AddSingleton<IWorkspace, Workspace>();

// Animation system
builder.Services.AddSingleton<ComponentBus>();

// Radzen (optional)
builder.Services.AddRadzenComponents();

var app = builder.Build();

// ... rest of configuration
app.Run();
```

**Critical**: Both `IFoundryService` and `IWorkspace` must be registered as singletons for proper state management across pages.

### 3. Global Imports (_Imports.razor)

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop

<!-- Foundry Namespaces -->
@using FoundryWorldsAndDrawings.Shared
@using FoundryWorldsAndDrawings.Solutions
@using FoundryWorldsAndDrawings.Shape
@using FoundryWorldsAndDrawings.PubSub
@using FoundryWorldsAndDrawings.ThreeD.Viewers
@using FoundryWorldsAndDrawings.ThreeD.Maths
@using FoundryWorldsAndDrawings.ThreeD.Objects
@using FoundryRulesAndUnits.Extensions
@using FoundryRulesAndUnits.Models

<!-- Your App Namespace -->
@using Three2025.Components
@using Three2025.Components.Pages
@using Three2025.Apprentice
```

---

## Razor Page Patterns

### Pattern 1: Multi-Canvas 2D Page

**Use Case**: Multiple independent 2D canvases on one page (drawing, diagramming, multi-viewport)

**Example**: `MultiCanvas2DTest.razor`

```razor
@page "/multi-canvas-2d-test"
@using FoundryWorldsAndDrawings.Shared
@rendermode InteractiveServer

<PageTitle>Multi-Canvas 2D Test</PageTitle>

<div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;">
    
    <!-- Page A -->
    <div style="border: 2px solid #4CAF50;">
        <div style="background: #4CAF50; color: white; padding: 8px;">
            Page A - Animated Rectangle
        </div>
        <div>
            <Canvas2DComponent SceneName="PageA" PageName="PageA" 
                               CanvasWidth="800" CanvasHeight="600" />
        </div>
    </div>

    <!-- Page B -->
    <div style="border: 2px solid #2196F3;">
        <div style="background: #2196F3; color: white; padding: 8px;">
            Page B - Moving Circles
        </div>
        <div>
            <Canvas2DComponent SceneName="PageB" PageName="PageB" 
                               CanvasWidth="800" CanvasHeight="600" />
        </div>
    </div>

</div>
```

**Code-Behind** (`MultiCanvas2DTest.razor.cs`):

```csharp
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Components.Pages;

public partial class MultiCanvas2DTest : IDisposable
{
    [Inject] public required IFoundryService Foundry { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }

    private IDrawing drawing => Workspace.GetDrawing()!;

    private FoShape2D _rectA = null!;
    private FoShape2D _circleB = null!;

    protected override async Task OnInitializedAsync()
    {
        "MultiCanvas2DTest: Initializing".WriteInfo();
        // Pages will be setup after first render
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SetupPageA();
            SetupPageB();
            StateHasChanged();
        }
    }

    private void SetupPageA()
    {
        var page = drawing.EstablishPage<FoPage2D>("PageA");
        page.Color = "LightCoral";
        
        _rectA = new FoShape2D(100, 100, "DarkBlue");
        _rectA.MoveTo(400, 300);
        page.AddShape(_rectA);

        $"Page A setup - {page.Members<FoShape2D>().Count()} shapes".WriteSuccess();
    }

    private void SetupPageB()
    {
        var page = drawing.EstablishPage<FoPage2D>("PageB");
        page.Color = "LightSkyBlue";
        
        _circleB = new FoShape2D(60, 60, "red");
        _circleB.MoveTo(200, 300);
        page.AddShape(_circleB);

        $"Page B setup - {page.Members<FoShape2D>().Count()} shapes".WriteSuccess();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
```

**Key Patterns**:
- ✅ Use `EstablishPage<FoPage2D>()` - creates page if it doesn't exist
- ✅ **DON'T call** `SetCurrentPage()` - Canvas2DComponent manages that per canvas
- ✅ Setup pages in `OnAfterRenderAsync(firstRender)` to ensure drawing is initialized
- ✅ Each canvas has independent `SceneName` and `PageName`

---

### Pattern 2: 3D Canvas with Animation

**Use Case**: 3D scene with animated objects, user interaction, dynamic content

**Example**: `Clock.razor`

```razor
@page "/clock"
@namespace Three2025.Components.Pages
@inherits ClockBase
@rendermode InteractiveServer

<PageTitle>Clock</PageTitle>

<h2>Canvas3DComponent Clock</h2>

<div style="padding: 10px; background-color: #e8f5e9;">
    <strong>FPS:</strong> @_currentFps.ToString("F1") 
    <strong>Tick:</strong> @_currentTick
</div>

<div class="d-flex">
   <div class="canvas-container">
       <Canvas3DComponent SceneName="Clock3D" @ref="Canvas3DReference" 
                          CanvasWidth=@CanvasWidth CanvasHeight=@CanvasHeight />
   </div>
   <ShapeTreeView/>
</div>

<RadzenStack>
    <button class="btn btn-primary" @onclick="DoClockFace">Add Clock Face</button>
    <button class="btn btn-primary" @onclick="DoRunClock">Start Clock</button>
</RadzenStack>
```

**Code-Behind** (`Clock.razor.cs`):

```csharp
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using Three2025.Apprentice;

namespace Three2025.Components.Pages;

public partial class ClockBase : ComponentBase, IDisposable
{
    [Inject] public required IFoundryService FoundryService { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }
    [Inject] public required IClockTech Tech { get; set; }
    [Inject] public required IJSRuntime JsRuntime { get; set; }

    protected Canvas3DComponent Canvas3DReference = null!;
    protected int CanvasWidth = 1200;
    protected int CanvasHeight = 800;

    private double _currentFps = 0;
    private int _currentTick = 0;

    protected override async Task OnInitializedAsync()
    {
        // Subscribe to animation events
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Wait for canvas initialization
            await Task.Delay(500);
            
            if (Canvas3DReference != null)
            {
                var (found, scene) = Canvas3DReference.GetActiveScene();
                
                if (found && scene != null)
                {
                    var arena = Workspace.GetArena();
                    arena.SetScene(scene);
                    $"Scene set in arena successfully".WriteSuccess();
                }
            }
        }
    }

    private void OnAnimationFrame(AnimationEvent animEvent)
    {
        if (animEvent.IsWorld3D())
        {
            _currentFps = animEvent.fps;
            _currentTick = animEvent.tick;
            InvokeAsync(StateHasChanged);
        }
    }

    public void DoClockFace()
    {
        var clockFace = new FoClockFace3D("ArenaClockFace")
        {
            Radius = 12.0,
            Height = 0.2,
            FontSize = 1.2,
            Transform = new Transform3("ClockTransform")
            {
                Position = new Vector3(0, 0, 0),
            }
        };

        var arena = Workspace.GetArena();
        arena.AddShapeToStage<FoClockFace3D>(clockFace);
    }

    public void DoRunClock()
    {
        Tech.RunClock();
    }

    public void Dispose()
    {
        var arena = Workspace.GetArena();
        arena.ClearArena();
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
    }
}
```

**Key Patterns**:
- ✅ Subscribe to `AnimationFrameBus` in `OnInitializedAsync()`
- ✅ **Unsubscribe in** `Dispose()` to prevent memory leaks
- ✅ Use `@ref` to get reference to Canvas3DComponent
- ✅ Call `GetActiveScene()` to access the 3D scene
- ✅ Use `Arena` for scene-level operations
- ✅ Use `Transform3` for positioning/rotating objects
- ✅ Call `StateHasChanged()` in animation callback (use `InvokeAsync`)

---

## Canvas Component Integration

### Canvas3DComponent (Three.js 3D Rendering)

**Props**:
```csharp
[Parameter] public string SceneName { get; set; } = "DefaultScene";
[Parameter] public int CanvasWidth { get; set; } = 1200;
[Parameter] public int CanvasHeight { get; set; } = 800;
```

**Usage**:
```razor
<Canvas3DComponent SceneName="MyScene3D" @ref="canvasRef" 
                   CanvasWidth="1200" CanvasHeight="800" />
```

**Access Scene**:
```csharp
Canvas3DComponent canvasRef;

var (found, scene) = canvasRef.GetActiveScene();
if (found && scene != null)
{
    // Add objects to scene
    scene.AddChild(myMesh);
}
```

### Canvas2DComponent (HTML5 Canvas 2D Rendering)

**Props**:
```csharp
[Parameter] public string SceneName { get; set; } = "DefaultScene";
[Parameter] public string PageName { get; set; } = "DefaultPage";
[Parameter] public string CanvasWidth { get; set; } = "800";
[Parameter] public string CanvasHeight { get; set; } = "600";
```

**Usage**:
```razor
<Canvas2DComponent SceneName="MyScene2D" PageName="MyPage" 
                   CanvasWidth="800" CanvasHeight="600" />
```

**Access Page**:
```csharp
// Canvas2DComponent manages page internally
// Access via drawing service:
var page = drawing.EstablishPage<FoPage2D>("MyPage");
page.AddShape(myShape);
```

**Critical Difference**: Canvas2DComponent handles page management internally. Don't call `SetCurrentPage()` when using multiple canvases.

---

## Animation System Integration

### Subscribe to Animation Events

```csharp
protected override async Task OnInitializedAsync()
{
    AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
}

private void OnAnimationFrame(AnimationEvent animEvent)
{
    if (animEvent.IsWorld3D())
    {
        // Handle 3D animation
        _tick = animEvent.tick;
        _fps = animEvent.fps;
        
        // Update UI (use InvokeAsync for thread safety)
        InvokeAsync(StateHasChanged);
    }
    
    if (animEvent.IsWorld2D())
    {
        // Handle 2D animation
    }
}

public void Dispose()
{
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

### Animation Event Types

```csharp
animEvent.IsWorld3D()     // Three.js animation frame
animEvent.IsWorld2D()     // Canvas2D animation frame
animEvent.IsAny()         // Any animation frame
animEvent.tick            // Frame number
animEvent.fps             // Frames per second
```

### Animating Objects

**3D Shapes**:
```csharp
// Modify transform properties
shape.Transform.Position = new Vector3(x, y, z);
shape.Transform.Rotation = new Euler(rx, ry, rz);
shape.Transform.Scale = new Vector3(sx, sy, sz);

// Helper methods
shape.Transform.MoveBy(dx, dy, dz);
shape.Transform.RotateBy(rx, ry, rz, AngleUnit.Degrees);
```

**2D Shapes**:
```csharp
shape.MoveTo(x, y);
shape.MoveBy(dx, dy);
shape.Rotate(angle); // in radians
```

---

## Service Injection Patterns

### Required Injections

```csharp
public partial class MyPage : ComponentBase, IDisposable
{
    // REQUIRED - Core services
    [Inject] public required IFoundryService FoundryService { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }
    
    // OPTIONAL - Common utilities
    [Inject] public required IJSRuntime JsRuntime { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    
    // OPTIONAL - Custom services (inject your own)
    [Inject] public required IClockTech Tech { get; set; }
}
```

### Accessing Services

```csharp
// Get drawing (2D)
var drawing = Workspace.GetDrawing();
var page = drawing.EstablishPage<FoPage2D>("MyPage");

// Get arena (3D)
var arena = Workspace.GetArena();
var stage = arena.CurrentStage();

// Get active scene (3D)
var (found, scene) = arena.CurrentScene();

// Foundry service utilities
var dataGen = FoundryService.GetMockDataGenerator();
```

---

## Common Page Types

### 1. Simple 3D Viewer

**Purpose**: Display static or dynamic 3D models

**Template**:
```csharp
public partial class Simple3DViewer : ComponentBase, IDisposable
{
    [Inject] public required IFoundryService Foundry { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }

    protected Canvas3DComponent canvasRef = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(500);
            var (found, scene) = canvasRef.GetActiveScene();
            if (found) AddObjectsToScene(scene);
        }
    }

    private void AddObjectsToScene(Scene3D scene)
    {
        var box = new FoShape3D("Box", "Blue")
            .CreateBox("MyBox", 2, 2, 2);
        
        var arena = Workspace.GetArena();
        arena.AddShapeToStage(box);
    }

    public void Dispose() { }
}
```

### 2. Multi-Canvas 2D Drawing App

**Purpose**: Multiple independent drawing canvases

**Template**: See [Pattern 1: Multi-Canvas 2D Page](#pattern-1-multi-canvas-2d-page)

### 3. Animated 3D Scene

**Purpose**: 3D scene with real-time animation

**Template**: See [Pattern 2: 3D Canvas with Animation](#pattern-2-3d-canvas-with-animation)

### 4. Interactive Diagram Editor

**Purpose**: Clickable 2D diagrams with connections

**Template**:
```csharp
public partial class DiagramEditor : ComponentBase
{
    [Inject] public required IWorkspace Workspace { get; set; }

    private FoShape2D _nodeA = null!;
    private FoShape2D _nodeB = null!;
    private FoShape1D _connector = null!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var page = Workspace.GetDrawing().EstablishPage<FoPage2D>("Diagram");
            
            _nodeA = new FoShape2D(80, 60, "lightblue");
            _nodeA.MoveTo(200, 200);
            page.AddShape(_nodeA);

            _nodeB = new FoShape2D(80, 60, "lightcoral");
            _nodeB.MoveTo(400, 300);
            page.AddShape(_nodeB);

            _connector = new FoShape1D("black", 2);
            _connector.Connect(_nodeA, _nodeB);
            page.AddShape(_connector);
        }
    }
}
```

---

## Troubleshooting

### Issue: Canvas Not Rendering

**Symptoms**: Blank canvas, no errors

**Solutions**:
1. ✅ Check `@rendermode InteractiveServer` is set on page
2. ✅ Verify `app-lib.js` is loaded in `App.razor`
3. ✅ Ensure `IFoundryService` and `IWorkspace` are registered in `Program.cs`
4. ✅ Add delay before accessing canvas: `await Task.Delay(500);`
5. ✅ Check browser console for JavaScript errors

### Issue: Animation Not Working

**Symptoms**: Objects don't move, FPS stuck at 0

**Solutions**:
1. ✅ Verify `AnimationFrameBus.SubscribeToAnimation()` is called
2. ✅ Check `Dispose()` unsubscribes from animation
3. ✅ Ensure `AnimationInitializer.razor` is in `MainLayout.razor`
4. ✅ Use `InvokeAsync(StateHasChanged)` in animation callback

### Issue: Multiple Canvases Interfering

**Symptoms**: One canvas updates, others freeze

**Solutions**:
1. ✅ DON'T call `SetCurrentPage()` - let Canvas2DComponent handle it
2. ✅ Use unique `SceneName` for each canvas
3. ✅ Use unique `PageName` for each canvas
4. ✅ Use `EstablishPage()` instead of `AddPage()`

### Issue: Objects Not Appearing in Scene

**Symptoms**: Code runs, no errors, but nothing visible

**Solutions**:
1. ✅ Check transform position (is object at camera position?)
2. ✅ Verify object is added to arena/stage: `arena.AddShapeToStage()`
3. ✅ Call `StateHasChanged()` after adding objects
4. ✅ Check camera position (default: looking at origin)

### Issue: Transform Changes Not Visible

**Symptoms**: Updating position/rotation has no effect

**Solutions**:
1. ✅ Use property setters, not direct field assignment
2. ✅ Ensure `SetDirty(true)` is called (automatic via properties)
3. ✅ Check if object is in animation loop (might be overridden)
4. ✅ Call `QueueForMeshUpdate()` if needed

---

## Migration Checklist

### For Each New Page

- [ ] **1. Create Razor File**
  - [ ] Add `@page "/route"` directive
  - [ ] Add `@rendermode InteractiveServer`
  - [ ] Add `@using` directives or rely on `_Imports.razor`
  - [ ] Create markup with Canvas3D/Canvas2D components

- [ ] **2. Create Code-Behind (.razor.cs)**
  - [ ] Inherit from `ComponentBase`
  - [ ] Implement `IDisposable` if using animations
  - [ ] Inject `IFoundryService` and `IWorkspace`
  - [ ] Inject other services as needed

- [ ] **3. Setup Lifecycle**
  - [ ] Override `OnInitializedAsync()` for subscriptions
  - [ ] Override `OnAfterRenderAsync(firstRender)` for setup
  - [ ] Implement `Dispose()` for cleanup

- [ ] **4. Canvas Integration**
  - [ ] Add `@ref` to Canvas component
  - [ ] Call `GetActiveScene()` or use `EstablishPage()`
  - [ ] Add objects to scene/page
  - [ ] Test rendering

- [ ] **5. Animation (if needed)**
  - [ ] Subscribe to `AnimationFrameBus`
  - [ ] Implement animation callback
  - [ ] Unsubscribe in `Dispose()`
  - [ ] Test animation loop

- [ ] **6. Testing**
  - [ ] Verify canvas renders
  - [ ] Verify objects appear
  - [ ] Verify animations work
  - [ ] Verify no console errors
  - [ ] Test disposal/navigation away

### For Migrating Existing Pages

- [ ] **1. Update Namespaces**
  - [ ] Replace old namespace references
  - [ ] Update `@using` directives
  - [ ] Fix any compilation errors

- [ ] **2. Update Canvas Components**
  - [ ] Replace old canvas components with `Canvas3DComponent`/`Canvas2DComponent`
  - [ ] Update property bindings (`SceneName`, `PageName`, etc.)
  - [ ] Add `@ref` if accessing programmatically

- [ ] **3. Update Service Injections**
  - [ ] Use `IFoundryService` instead of old service names
  - [ ] Use `IWorkspace` for drawing/arena access
  - [ ] Remove deprecated injections

- [ ] **4. Update Scene/Page Access**
  - [ ] Replace direct scene access with `GetActiveScene()`
  - [ ] Replace page access with `EstablishPage()`
  - [ ] Update arena access: `Workspace.GetArena()`

- [ ] **5. Update Animation**
  - [ ] Migrate to `AnimationFrameBus`
  - [ ] Update event handler signatures
  - [ ] Test animation flow

- [ ] **6. Test Migration**
  - [ ] Build project
  - [ ] Run page
  - [ ] Verify all features work
  - [ ] Check for console errors
  - [ ] Test navigation and disposal

---

## Example: Complete Page Template

```razor
@* MyNewPage.razor *@
@page "/my-new-page"
@rendermode InteractiveServer

<PageTitle>My New Page</PageTitle>

<h2>My 3D Scene</h2>

<div class="d-flex">
   <Canvas3DComponent SceneName="MyScene3D" @ref="canvasRef" 
                      CanvasWidth="1200" CanvasHeight="800" />
   <ShapeTreeView/>
</div>

<button class="btn btn-primary" @onclick="AddBox">Add Box</button>
```

```csharp
// MyNewPage.razor.cs
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Components.Pages;

public partial class MyNewPage : ComponentBase, IDisposable
{
    [Inject] public required IFoundryService FoundryService { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }

    protected Canvas3DComponent canvasRef = null!;
    private int _boxCount = 0;

    protected override async Task OnInitializedAsync()
    {
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(500);
            var (found, scene) = canvasRef.GetActiveScene();
            if (found && scene != null)
            {
                var arena = Workspace.GetArena();
                arena.SetScene(scene);
                "Scene initialized".WriteSuccess();
            }
        }
    }

    private void OnAnimationFrame(AnimationEvent animEvent)
    {
        if (animEvent.IsWorld3D())
        {
            // Animation logic here
        }
    }

    private void AddBox()
    {
        var box = new FoShape3D($"Box{++_boxCount}", "Blue")
            .CreateBox($"MyBox{_boxCount}", 2, 2, 2);
        
        box.Transform.Position = new Vector3(
            Random.Shared.Next(-10, 10),
            Random.Shared.Next(0, 10),
            Random.Shared.Next(-10, 10)
        );

        var arena = Workspace.GetArena();
        arena.AddShapeToStage(box);
        
        StateHasChanged();
    }

    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
        Workspace.GetArena().ClearArena();
    }
}
```

---

## Summary

**Key Takeaways**:

1. **Library Structure**: FoundryRulesAndUnits → FoundryWorldsAndDrawings → (optional) FoundryMentorModeler
2. **DI Setup**: Register `IFoundryService` and `IWorkspace` as singletons
3. **Canvas Components**: Use `Canvas3DComponent` for 3D, `Canvas2DComponent` for 2D
4. **Page Setup**: Use `OnAfterRenderAsync(firstRender)` with delay for initialization
5. **Animation**: Subscribe to `AnimationFrameBus`, unsubscribe in `Dispose()`
6. **Multi-Canvas**: Use `EstablishPage()`, don't call `SetCurrentPage()`
7. **Transforms**: Use property setters for position/rotation/scale changes

**Next Steps**:
- Review [MultiCanvas2DTest.razor](../Components/Pages/MultiCanvas2DTest.razor) for 2D examples
- Review [Clock.razor](../Components/Pages/Clock.razor) for 3D animation examples
- Consult [COPILOT_HANDOFF_GUIDE.md](./COPILOT_HANDOFF_GUIDE.md) for additional context
- Check [MULTI_CANVAS_ARCHITECTURE.md](../../FoundryWorldsAndDrawings/MULTI_CANVAS_ARCHITECTURE.md) for architectural details

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-23  
**Author**: GitHub Copilot (documenting work from 2025-11-22)  
**Context**: Three2025 application migration to FoundryWorldsAndDrawings libraries
