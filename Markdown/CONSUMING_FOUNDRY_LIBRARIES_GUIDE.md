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
5. [AnimationFrameBus Events](#animationframebus-events)
6. [Canvas Component Integration](#canvas-component-integration)
7. [Animation System Integration](#animation-system-integration)
8. [Service Injection Patterns](#service-injection-patterns)
9. [Common Page Types](#common-page-types)
10. [Migration from Old APIs](#migration-from-old-apis-trisoc_dashboard-pattern)
11. [Deprecated APIs & Migration Guide](#deprecated-apis--migration-guide)
12. [Quick Reference: TRISoC_Dashboard Migration Patterns](#quick-reference-trisoc_dashboard-migration-patterns)
13. [Troubleshooting](#troubleshooting)
14. [Migration Checklist](#migration-checklist)

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

## Required Dependencies

⚠️ **IMPORTANT**: During active development, **use ProjectReference instead of NuGet packages** to ensure you have the latest APIs (including `BeforeAnimationRefresh()`, `SetTransformStale()`, etc.).

### Scenario 1: Development/Testing (RECOMMENDED)

**Use Case**: You're actively developing, testing, or need the latest features.

**Benefits**:
- ✅ Always have the latest code changes
- ✅ Immediate access to new APIs (no waiting for NuGet publish)
- ✅ Easier debugging (step into library code)
- ✅ See uncommitted changes in real-time

**Setup** (Three2025.csproj example):

```xml
<ItemGroup>
  <!-- Use ProjectReference for latest code -->
  <ProjectReference Include="..\FoundryRulesAndUnits\FoundryRulesAndUnits.csproj" />
  <ProjectReference Include="..\FoundryWorldsAndDrawings\FoundryWorldsAndDrawings.csproj" />
  <ProjectReference Include="..\FoundryMentorModeler\FoundryMentorModeler.csproj" />
  
  <!-- Comment out NuGet packages -->
  <!-- <PackageReference Include="ApprenticeFoundryRulesAndUnits" Version="10.7.0" /> -->
  <!-- <PackageReference Include="ApprenticeFoundryWorldsAndDrawings" Version="0.10.39" /> -->
  <!-- <PackageReference Include="ApprenticeFoundryMentorModeler" Version="0.1.27" /> -->
</ItemGroup>
```

**Directory Structure** (required):
```
YourWorkspace/
├── YourApp/
│   ├── YourApp.csproj  (uses ProjectReference)
│   └── ...
├── FoundryRulesAndUnits/
│   ├── FoundryRulesAndUnits.csproj
│   └── ...
├── FoundryWorldsAndDrawings/
│   ├── FoundryWorldsAndDrawings.csproj
│   └── ...
└── FoundryMentorModeler/  (optional)
    ├── FoundryMentorModeler.csproj
    └── ...
```

**Clone Libraries** (if not already local):
```bash
cd YourWorkspace
git clone https://github.com/SteveStrong/FoundryRulesAndUnits.git
git clone https://github.com/SteveStrong/FoundryWorldsAndDrawings.git
git clone https://github.com/SteveStrong/FoundryMentorModeler.git  # Optional
```

---

### Scenario 2: Production/Stable Releases

**Use Case**: Deploying to production, want stable versioned releases.

**Benefits**:
- ✅ Versioned and tested releases
- ✅ No need to clone library source code
- ✅ Faster build times (pre-compiled)
- ✅ Easier CI/CD integration

**Caveats**:
- ⚠️ NuGet packages may lag behind latest code
- ⚠️ New APIs (like `BeforeAnimationRefresh()`) may not be in published packages yet
- ⚠️ Check package publish date vs code commit date

**Minimum Configuration** (3D/2D Canvas):

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

**Full Configuration** (with MentorModeler):

```xml
<ItemGroup>
  <!-- All of the above, plus: -->
  <PackageReference Include="ApprenticeFoundryMentorModeler" Version="0.1.27" />
</ItemGroup>
```

**⚠️ Version Compatibility Warning**:

The versions shown above may **NOT** include the latest APIs documented in this guide:
- `BeforeAnimationRefresh()` - Added after v0.10.39
- `SetTransformStale()`, `SetGeometryStale()`, `SetMaterialStale()` - Added after v0.10.39
- Auto-published `RefreshUIEvent` - Added after v0.10.39
- Automatic scene/stage linking - Added after v0.10.39

**To check if NuGet is current**:
1. Check NuGet package publish date: [NuGet.org - ApprenticeFoundryWorldsAndDrawings](https://www.nuget.org/packages/ApprenticeFoundryWorldsAndDrawings)
2. Check latest commit date: [GitHub - FoundryWorldsAndDrawings](https://github.com/SteveStrong/FoundryWorldsAndDrawings)
3. If NuGet is older, use ProjectReference instead

---

### Scenario 3: Hybrid Approach

**Use Case**: Testing one library locally while using stable NuGet for others.

```xml
<ItemGroup>
  <!-- Testing FoundryWorldsAndDrawings locally -->
  <ProjectReference Include="..\FoundryWorldsAndDrawings\FoundryWorldsAndDrawings.csproj" />
  
  <!-- Stable NuGet for others -->
  <PackageReference Include="ApprenticeFoundryRulesAndUnits" Version="10.7.0" />
  <PackageReference Include="ApprenticeFoundryMentorModeler" Version="0.1.27" />
</ItemGroup>
```

⚠️ **WARNING**: Mixing ProjectReference and PackageReference can cause conflicts if versions don't match. See troubleshooting section below.

---

### JavaScript Dependencies

Ensure `app-lib.js` is loaded in `App.razor`:

```razor
<script src="_content/ApprenticeFoundryWorldsAndDrawings/app-lib.js"></script>
```

**Note**: Path is the same whether using NuGet or ProjectReference.

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
- ✅ Add shapes directly to page: `page.AddShape(shape)`
- ❌ **DON'T call** `drawing.SetCurrentPage()` - Canvas2DComponent manages this automatically per canvas
- ✅ Setup pages in `OnAfterRenderAsync(firstRender)` to ensure drawing is initialized
- ✅ Each canvas has independent `SceneName` and `PageName`
- ✅ Pages are automatically rendered to their respective canvases

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
            
            // NOTE: SetScene() is NO LONGER NEEDED!
            // Canvas3DComponent automatically creates scene, stage, and links them.
            // Just start adding shapes to arena!
            
            // Optional: Verify scene is ready
            if (Canvas3DReference != null)
            {
                var (found, scene) = Canvas3DReference.GetActiveScene();
                if (found && scene != null)
                {
                    $"Scene '{scene.Title}' ready for use".WriteSuccess();
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
- ✅ Use `@ref` to get reference to Canvas3DComponent (optional)
- ❌ **DON'T call** `arena.SetScene()` - Canvas3DComponent manages this automatically
- ✅ Use `arena.AddShapeToStage()` to add objects (simplest API)
- ✅ Use `Transform3` for positioning/rotating objects
- ✅ Call `StateHasChanged()` in animation callback (use `InvokeAsync`)
- ✅ Scenes and stages are automatically linked by Canvas3DComponent

---

## AnimationFrameBus Events

The `AnimationFrameBus` publishes **TWO TYPES** of events in a specific sequence each animation frame:

### 1. PreAnimationEvent (Computation Phase) 🧮

**Purpose**: For **KnModel** objects (or custom geometry) to **compute and update** their geometry/state BEFORE rendering.

```csharp
// Subscribe to PreAnimationEvent
AnimationFrameBus.SubscribeToPreAnimation(OnPreAnimationFrame);

private void OnPreAnimationFrame(PreAnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        // Update KnModel geometry, compute positions, etc.
        // This runs BEFORE rendering, so changes will be visible
    }
    
    if (evt.IsDrawing2D())
    {
        // Update 2D shapes or data before rendering
    }
}
```

### 2. AnimationEvent (Rendering Phase) 🎨

**Purpose**: For **Canvas components** to **render** the updated geometry to screen.

```csharp
// Subscribe to AnimationEvent
AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);

private void OnAnimationFrame(AnimationEvent animEvent)
{
    if (animEvent.IsWorld3D())
    {
        // Render 3D scene to canvas
    }
    
    if (animEvent.IsDrawing2D())
    {
        // Render 2D drawing to canvas
    }
}
```

### Event Sequence (Critical!) ⚡

Each animation frame triggers events in this **exact order**:

```
1. PreAnimationEvent (Drawing)  ← KnModels update 2D geometry
2. PreAnimationEvent (World)    ← KnModels update 3D geometry
3. AnimationEvent (Drawing)     ← Canvas2D renders updated 2D
4. AnimationEvent (World)       ← Canvas3D renders updated 3D
```

**Why This Matters**:
- ✅ **Correct**: Subscribe to `PreAnimationEvent` for geometry computation/updates
- ✅ **Correct**: Subscribe to `AnimationEvent` for rendering to canvas
- ❌ **Wrong**: Try to update geometry in `AnimationEvent` (too late - already rendering)
- ❌ **Wrong**: Try to render in `PreAnimationEvent` (too early - geometry not finalized)

### When to Use Which Event

| Use Case | Subscribe To | Example |
|----------|-------------|---------|
| **KnModel geometry updates** | `PreAnimationEvent` | Computing trisoc vertices, updating mesh positions |
| **Canvas rendering** | `AnimationEvent` | Canvas2DComponent, Canvas3DComponent |
| **UI state updates** | `AnimationEvent` | FPS counter, tick display (use `InvokeAsync`) |
| **Physics/simulation** | `PreAnimationEvent` | Computing collisions, updating transforms |
| **Data-driven animation** | `PreAnimationEvent` | Reading sensor data, updating object positions |

### Event Properties (Both Events)

```csharp
public class PreAnimationEvent / AnimationEvent
{
    public double fps;                      // Current frames per second
    public int tick;                        // Global animation tick counter
    public PrerenderType prerenderType;     // World (3D) or Drawing (2D)
    
    public bool IsWorld3D();                // Returns true if 3D
    public bool IsDrawing2D();              // Returns true if 2D
}
```

### Disposal (Don't Forget!) 🧹

```csharp
public void Dispose()
{
    // Unsubscribe from BOTH if you subscribed to both
    AnimationFrameBus.UnSubscribeFromPreAnimation(OnPreAnimationFrame);
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

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

### Two Animation Patterns

The Foundry framework provides **two distinct animation patterns** for different use cases:

1. **Component-Level Animation** - Use `AnimationFrameBus.SubscribeToAnimation()`
   - **When**: Page/component needs to respond to animation frames
   - **Purpose**: Update UI state, coordinate multiple objects, global logic
   - **Example**: Update FPS counter, tick display, coordinate scene changes

2. **Object-Level Animation** - Use `BeforeAnimationRefresh()`
   - **When**: Individual 3D objects need per-frame updates
   - **Purpose**: Animate specific object transforms, geometry, materials
   - **Example**: Move a box, rotate a model, update tube geometry

⚠️ **IMPORTANT**: `SetAnimationUpdate()` was **removed**. Use `BeforeAnimationRefresh()` instead.

---

### Pattern 1: Component-Level Animation

**Use Case**: Page needs to respond to animation frames for UI updates or scene coordination.

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

---

### Pattern 2: Object-Level Animation

**Use Case**: Individual 3D object needs to animate itself every frame.

⚠️ **Migration Note**: Replace `SetAnimationUpdate()` with `BeforeAnimationRefresh()`.

**Example - Animating Object Transform**:
```csharp
var text3d = new FoText3D("MovingText", "Hello World")
{
    Transform = new Transform3("TextTransform")
    {
        Position = new Vector3(0, 5, 0)
    }
};

// Set up per-frame animation callback
text3d.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Move object on specific frames
    if (tick % 10 == 0)
    {
        var pos = self.Transform.MoveBy(0, 0, 0.1);
        
        // Reverse direction at boundaries
        if (pos.Z > 10 || pos.Z < -10)
        {
            delta = -delta;
        }
        
        // Mark transform as changed for JavaScript update
        self.SetTransformStale();
    }
});

arena.AddShapeToStage(text3d);
```

**Example - Animating Geometry**:
```csharp
var tube = new FoTube3D("DynamicTube");

tube.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Get world positions from other objects
    var pos1 = box1.GetWorldPosition();
    var pos2 = box2.GetWorldPosition();
    
    // Update tube path
    self.Path3D = new List<Vector3> { pos1, pos2 };
    
    // Mark geometry as changed (property setter does this automatically)
    // But explicit call ensures JavaScript knows to rebuild geometry
    self.SetGeometryStale();
});
```

**Stop Animation**:
```csharp
// Clear animation callback when no longer needed
object3D.ClearAnimationRefresh();
// OR
object3D.ClearAnimationCallback();
```

**Key Methods**:
- `BeforeAnimationRefresh(Action<FoGlyph3D, int, double> action)` - Set animation callback
- `ClearAnimationRefresh()` - Remove animation callback
- `SetTransformStale()` - Mark transform as changed (position/rotation/scale)
- `SetGeometryStale()` - Mark geometry as changed (rebuild mesh)
- `SetMaterialStale()` - Mark material as changed (color/texture)

### Animating Objects

#### Automatic Dirty Tracking (The Magic! ✨)

**Key Principle**: Transform property setters **automatically** trigger dirty tracking. You rarely need to call `SetTransformStale()` manually.

**How It Works**:

```csharp
// When you assign a transform property:
shape.Transform.Position = new Vector3(x, y, z);

// This automatically happens internally:
// 1. Transform3.Position setter calls AssignVector()
// 2. AssignVector() calls SetDirty(true)
// 3. SetDirty() fires OnChange event
// 4. Object3D receives callback via NotifyOwnerOfChange
// 5. Object3D calls SetTransformStale()
// 6. Shape queued for JavaScript update
```

**Result**: ✅ **You don't need to call `SetTransformStale()` manually!**

---

#### 3D Transform Animation (Automatic Tracking)

```csharp
// ✅ CORRECT - Property setters handle everything automatically
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Just assign new values - stale tracking happens automatically!
    self.Transform.Position = new Vector3(x, y, z);
    self.Transform.Rotation = new Euler(rx, ry, rz);
    self.Transform.Scale = new Vector3(sx, sy, sz);
    
    // NO NEED to call SetTransformStale() - already done!
});
```

**Helper Methods** (also automatic):
```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Helper methods also trigger automatic tracking
    var newPos = self.Transform.MoveBy(dx, dy, dz);  // ✅ Auto-tracks
    self.Transform.RotateBy(rx, ry, rz, AngleUnit.Degrees);  // ✅ Auto-tracks
    
    // Scale uniformly
    self.Transform.Scale = new Vector3(factor, factor, factor);  // ✅ Auto-tracks
});
```

---

#### When Manual Stale Calls ARE Needed

**Scenario 1: Geometry Changes**

```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Geometry property changes may not auto-track in all cases
    // Explicitly mark geometry as changed
    self.Width = newWidth;   // May auto-track depending on property
    self.SetGeometryStale();  // ✅ Ensure JavaScript rebuilds geometry
});
```

**Scenario 2: Material Changes**

```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Material changes
    var mesh = self.GetComputedMesh().mesh;
    mesh.Material.Color = "red";
    mesh.Material.Opacity = 0.5;
    // Material properties have OnMaterialChanged callbacks
    // But explicit call ensures update
    self.SetMaterialStale();  // ✅ Ensure JavaScript updates material
});
```

**Scenario 3: Direct Mesh/Object3D Manipulation**

```csharp
// If you bypass FoGlyph3D and work directly with Object3D
var (success, mesh) = shape.GetComputedMesh();
if (success)
{
    mesh.Position = new Vector3(x, y, z);  // Direct Object3D access
    mesh.SetTransformStale();  // ✅ Manual call needed
}
```

---

#### Common Mistake: Mutating Instead of Assigning

```csharp
// ❌ WRONG - Modifies a COPY, doesn't trigger tracking
shape.Transform.Position.X = 10;
// Why: Position getter returns by-value, so you're modifying a copy

// ✅ CORRECT - Assign new instance
var pos = shape.Transform.Position;
pos.X = 10;
shape.Transform.Position = pos;  // Assignment triggers tracking

// ✅ BETTER - Direct assignment
shape.Transform.Position = new Vector3(10, pos.Y, pos.Z);
```

---

#### 2D Shape Animation (Component-Level)

**2D shapes** use a different pattern - animate in the component's animation callback:

```csharp
private void OnAnimationFrame(AnimationEvent animEvent)
{
    if (animEvent.IsWorld2D())
    {
        // Modify 2D shapes directly in animation callback
        shape.MoveTo(x, y);
        shape.MoveBy(dx, dy);
        shape.Rotate(angle); // in radians
    }
}
```

---

### Granular Stale Tracking System

The framework uses **5 granular stale flags** for efficient GPU updates. Each flag targets a specific type of change:

#### The 5 Stale Flags

| Flag | Method | When to Use | What Updates in JavaScript |
|------|--------|-------------|---------------------------|
| **Transform** | `SetTransformStale()` | Position, rotation, scale changed | Three.js matrix update (fast) |
| **Material** | `SetMaterialStale()` | Color, opacity, texture changed | Material properties update (fast) |
| **Geometry** | `SetGeometryStale()` | Shape, vertices, mesh changed | Geometry rebuild (moderate) |
| **Structure** | `SetStructureStale()` | Children added/removed | Scene graph update (moderate) |
| **Data** | `SetDataStale()` | Custom properties, labels changed | Metadata update (fast) |

---

#### Transform Stale (AUTOMATIC)

**Triggered Automatically** when:
- `Transform.Position = newValue`
- `Transform.Rotation = newValue`
- `Transform.Scale = newValue`
- `Transform.MoveBy()`, `Transform.RotateBy()`

**Manual Call Rarely Needed**:
```csharp
// ✅ Automatic - property setter triggers tracking
shape.Transform.Position = new Vector3(10, 20, 30);
// SetTransformStale() already called internally!

// ❌ Manual call NOT needed in most cases
shape.SetTransformStale();  // Redundant if using property setters
```

**When Manual Call IS Needed**:
```csharp
// Direct Object3D manipulation (bypassing FoGlyph3D)
var (success, mesh) = shape.GetComputedMesh();
mesh.Position = new Vector3(x, y, z);
mesh.SetTransformStale();  // ✅ Manual call required
```

---

#### Material Stale (MOSTLY AUTOMATIC)

**Triggered Automatically** when:
- `Material.Color = newColor`
- `Material.Opacity = newOpacity`
- `Material.Wireframe = newValue`

**Example**:
```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    var (success, mesh) = self.GetComputedMesh();
    if (success)
    {
        // Property setters trigger OnMaterialChanged callback
        mesh.Material.Color = "red";    // ✅ Auto-tracked
        mesh.Material.Opacity = 0.5;    // ✅ Auto-tracked
        
        // Optional: Explicit call for safety
        self.SetMaterialStale();  // Ensures JavaScript update
    }
});
```

---

#### Geometry Stale (OFTEN MANUAL)

**When to Call Manually**:
- Changing shape dimensions (`Width`, `Height`, `Radius`)
- Updating path vertices (`Path3D`)
- Custom mesh modifications

**Example - Dynamic Tube**:
```csharp
var tube = new FoTube3D("DynamicTube");

tube.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Get endpoint positions
    var pos1 = box1.GetWorldPosition();
    var pos2 = box2.GetWorldPosition();
    
    // Update tube path
    self.Path3D = new List<Vector3> { pos1, pos2 };
    
    // ✅ REQUIRED - Tell JavaScript to rebuild geometry
    self.SetGeometryStale();
});
```

**Example - Box Resize**:
```csharp
box.BeforeAnimationRefresh((self, tick, fps) =>
{
    self.Width = Math.Sin(tick * 0.1) * 5 + 10;
    self.Height = Math.Cos(tick * 0.1) * 5 + 10;
    
    // ✅ REQUIRED - Geometry changed, rebuild mesh
    self.SetGeometryStale();
});
```

---

#### Structure Stale (MANUAL)

**When to Call**:
- Adding children: `parent.AddSubGlyph3D(child)`
- Removing children: `parent.RemoveSubGlyph3D(child)`
- Scene graph hierarchy changes

**Example**:
```csharp
var group = new FoGroup3D("DynamicGroup");

group.BeforeAnimationRefresh((self, tick, fps) =>
{
    if (shouldAddChild)
    {
        var child = new FoShape3D("Child", "blue").CreateSphere("Sphere", 1, 1, 1);
        self.AddSubGlyph3D(child);
        
        // ✅ Tell JavaScript scene graph changed
        self.SetStructureStale();
    }
});
```

---

#### Data Stale (MANUAL)

**When to Call**:
- Custom property changes
- Label text updates
- Metadata changes

**Example**:
```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Update custom property
    self.SetProperty("temperature", currentTemp);
    
    // ✅ Mark metadata as changed
    self.SetDataStale();
});
```

---

#### Combining Multiple Flags

**Scenario**: Object changes both transform AND geometry

```csharp
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Transform change (automatic)
    self.Transform.Position = new Vector3(x, y, z);
    // → SetTransformStale() called automatically
    
    // Geometry change (manual)
    self.Width = newWidth;
    self.SetGeometryStale();  // ✅ Manual call
    
    // Result: Both flags set, JavaScript gets optimized update batch
});
```

**JavaScript receives**:
- Transform update: New matrix (fast)
- Geometry update: Rebuild mesh (moderate)

---

#### Performance Benefits

**Granular Updates Save GPU Time**:

```csharp
// Scenario 1: Only position changed
shape.Transform.Position = newPos;
// → JavaScript: Update matrix only (very fast)
// → No geometry rebuild, no material update

// Scenario 2: Only color changed
shape.Material.Color = "red";
// → JavaScript: Update material.color property only (very fast)
// → No geometry rebuild, no matrix update

// Scenario 3: Everything changed
shape.Transform.Position = newPos;
shape.Material.Color = "red";
shape.Width = newWidth;
shape.SetGeometryStale();
// → JavaScript: Update all three (batched)
```

---

#### Legacy API: SetDirty()

**Status**: Still exists for compatibility but **NOT recommended** for 3D objects.

```csharp
// ❌ OLD WAY (pre-granular flags)
shape.SetDirty(true);  // Sets ALL flags, inefficient

// ✅ NEW WAY (granular)
shape.SetTransformStale();  // Only transform flag, efficient
```

**When `SetDirty()` IS Appropriate**:
- FoComponent base class (2D system, business logic)
- Non-rendering dirty flags (data validation, UI state)
- Transform3 internal use (matrix cache invalidation)

**Summary**:
- **3D rendering**: Use granular `SetXxxStale()` methods
- **Business logic**: Use `SetDirty()` or `IsDirty` property
- **Transform properties**: Automatic - no manual call needed!

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

## Scene/Arena/Stage Architecture

### Understanding the Hierarchy

The 3D system has **three layers** that work together:

```
FoArena3D (singleton)
  ↓ manages
FoStage3D (one per canvas/page) 
  ↓ linked to
Scene3D (JavaScript Three.js wrapper)
```

#### 1. FoArena3D - The Container

**Purpose**: Global singleton that manages all stages and scenes across the entire application.

**Access**:
```csharp
var arena = Workspace.GetArena();
```

**Responsibilities**:
- Maintains collection of all FoStage3D instances
- Tracks "current" stage for operations
- Provides high-level API: `AddShapeToStage()`, `RemoveShapeFromStage()`
- Coordinates animation updates across all stages
- Publishes `RefreshUIEvent` when shapes are added/removed

**Key Point**: There is **one arena** for the entire application.

#### 2. FoStage3D - The Business Logic Layer

**Purpose**: Per-canvas container that holds FoGlyph3D shapes and manages their lifecycle.

**Creation** (automatic via Canvas3DComponent):
```csharp
// Canvas3DComponent creates this automatically in OnAfterRenderAsync
var stage = arena.EstablishStage<FoStage3D>("MySceneName");
```

**Responsibilities**:
- Holds collection of FoGlyph3D shapes (FoShape3D, FoText3D, FoModel3D, etc.)
- Processes animation callbacks via `UpdateForAnimation()`
- Linked to matching Scene3D via `AssociatedStage` property
- Manages shape lifecycle (add, remove, clear)

**Key Point**: One stage per Canvas3DComponent, named to match the SceneName.

#### 3. Scene3D - The Rendering Layer

**Purpose**: Wrapper around Three.js scene, manages Object3D instances and JavaScript communication.

**Creation** (automatic via Canvas3DComponent):
```csharp
// Canvas3DComponent creates this automatically
var (found, scene) = Canvas3DReference.GetActiveScene();
```

**Responsibilities**:
- Holds Object3D instances (the actual Three.js meshes)
- Sends update batches to JavaScript (transform/geometry/material changes)
- Manages dirty object queues
- Routes updates to correct JavaScript viewer

**Key Point**: One scene per Canvas3DComponent, automatically linked to matching stage.

---

### The Linking Pattern

**Canvas3DComponent Auto-Setup** (happens in `OnAfterRenderAsync`):

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // 1. Canvas creates/finds Scene3D
        var (found, scene) = GetActiveScene();
        
        // 2. Canvas creates matching FoStage3D
        var arena = Workspace.GetArena();
        var stage = arena.EstablishStage<FoStage3D>(SceneName);
        
        // 3. Canvas links them together
        scene.AssociatedStage = stage;
        ManagedStage = stage;  // Track for cleanup
        
        // 4. Canvas subscribes to animation
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
    }
}
```

**Result**: Scene ↔ Stage are automatically linked by Canvas3DComponent.

---

### SetScene() - Still Needed?

⚠️ **OUTDATED PATTERN**: Manually calling `arena.SetScene()` is **no longer required** in most cases.

**Old Pattern** (from guide examples):
```csharp
// ❌ NO LONGER NECESSARY
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var (found, scene) = Canvas3DReference.GetActiveScene();
        var arena = Workspace.GetArena();
        arena.SetScene(scene);  // ← Canvas3DComponent already did this!
    }
}
```

**Current Pattern**:
```csharp
// ✅ SIMPLIFIED - Just wait for canvas to initialize
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(500);  // Let canvas complete setup
        // Canvas3DComponent already created scene, stage, and linked them
        // Just start adding shapes!
    }
}
```

**When `SetScene()` IS Still Useful**:

1. **Switching scenes dynamically** (advanced scenario):
   ```csharp
   // If you need to swap which scene the arena is rendering
   var otherScene = Scene3D.FindScene("OtherScene");
   arena.SetScene(otherScene);
   ```

2. **Manual scene management** (not using Canvas3DComponent):
   ```csharp
   // If you create scenes without Canvas3DComponent
   var customScene = new Scene3D("CustomScene");
   arena.SetScene(customScene);
   ```

**Best Practice**: Let Canvas3DComponent handle scene/stage setup automatically. Don't call `SetScene()` unless you have a specific advanced use case.

---

### AddShapeToStage() Auto-Publishes RefreshUIEvent

**Important Side Effect**: When you add shapes to the stage, `RefreshUIEvent` is automatically published.

**Code** (from `FoArena3D.cs`):
```csharp
public V AddShapeToStage<V>(V shape) where V : FoGlyph3D
{
    var stage = CurrentStage();
    stage.AddShape<V>(shape);
    
    // Auto-publish UI refresh event
    PubSub!.Publish<RefreshUIEvent>(new RefreshUIEvent("FoArena3D:AddShape"));
    
    // Queue for JavaScript update
    shape.QueueForMeshUpdate();
    
    return shape;
}
```

**What This Means**:
- ShapeTreeView automatically updates when shapes are added/removed
- No need to manually trigger UI refresh
- UI components subscribed to `RefreshUIEvent` will update

**Example**:
```csharp
// This single line triggers:
// 1. Shape added to stage
// 2. RefreshUIEvent published → ShapeTreeView updates
// 3. Shape queued for mesh creation in JavaScript
arena.AddShapeToStage(myBox);
```

---

### Common Patterns

**Pattern 1: Simple Shape Addition**
```csharp
// Canvas3DComponent already setup scene/stage
var arena = Workspace.GetArena();

var box = new FoShape3D("MyBox", "blue").CreateBox("Box", 2, 2, 2);
box.Transform.Position = new Vector3(0, 5, 0);

// Add to current stage (RefreshUIEvent auto-published)
arena.AddShapeToStage(box);
```

**Pattern 2: Multi-Canvas with Named Stages**
```csharp
// Each canvas has its own stage
var stageA = arena.EstablishStage<FoStage3D>("SceneA");
var stageB = arena.EstablishStage<FoStage3D>("SceneB");

// Switch between stages
arena.SetCurrentStage(stageA);
arena.AddShapeToStage(shapeForA);  // Goes to SceneA

arena.SetCurrentStage(stageB);
arena.AddShapeToStage(shapeForB);  // Goes to SceneB
```

**Pattern 3: Direct Stage Access**
```csharp
// Access stage directly (skip SetCurrentStage)
var stage = arena.EstablishStage<FoStage3D>("MyScene");
stage.AddShape(myShape);  // Add directly to specific stage
```

---

### Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│ User Code (Your Razor Page)                        │
│  - Creates FoShape3D, FoText3D, etc.                │
│  - Calls arena.AddShapeToStage()                    │
└────────────────┬────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────┐
│ FoArena3D (Singleton)                               │
│  - Manages all stages                               │
│  - Publishes RefreshUIEvent                         │
│  - Coordinates animation                            │
└────────────────┬────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────┐
│ FoStage3D (Per Canvas)                              │
│  - Holds FoGlyph3D shapes                           │
│  - Processes animation callbacks                    │
│  - Linked to Scene3D                                │
└────────────────┬────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────┐
│ Scene3D (Per Canvas)                                │
│  - Holds Object3D meshes                            │
│  - Sends updates to JavaScript                      │
│  - Manages dirty queues                             │
└────────────────┬────────────────────────────────────┘
                 ↓
┌─────────────────────────────────────────────────────┐
│ JavaScript (Three.js)                               │
│  - Receives update DTOs                             │
│  - Creates/updates Three.js meshes                  │
│  - Renders to canvas                                │
└─────────────────────────────────────────────────────┘
```

---

## 2D vs 3D Scene Management Comparison

### Automatic Management (Both 2D and 3D)

**Key Principle**: Both Canvas2DComponent and Canvas3DComponent **automatically manage** their internal scene/page state. **You should NOT manually set the current page or scene.**

#### Canvas2DComponent Auto-Management

```csharp
// Canvas2DComponent.razor.cs - OnAfterRenderAsync
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Auto-creates page if needed
        var page = drawing.EstablishPage<FoPage2D>(PageName);
        
        // Auto-sets as current page FOR THIS CANVAS ONLY
        drawing.SetCurrentPage(page);  // ← Canvas does this, not you!
        
        // Subscribe to animation if enabled
        if (WithAnimations)
            AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
    }
}
```

**User Code Pattern** (2D):
```csharp
// ✅ CORRECT - Let canvas manage page state
private void SetupPageA()
{
    var page = drawing.EstablishPage<FoPage2D>("PageA");
    var shape = new FoShape2D(100, 100, "blue");
    page.AddShape(shape);  // Add directly to page
}

// ❌ WRONG - Don't call SetCurrentPage()
private void SetupPageA()
{
    var page = drawing.EstablishPage<FoPage2D>("PageA");
    drawing.SetCurrentPage(page);  // ← Canvas already did this!
    page.AddShape(shape);
}
```

#### Canvas3DComponent Auto-Management

```csharp
// Canvas3DComponent.razor.cs - OnAfterRenderAsync
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Auto-creates Scene3D
        var (found, scene) = GetActiveScene();
        
        // Auto-creates matching FoStage3D
        var arena = Workspace.GetArena();
        var stage = arena.EstablishStage<FoStage3D>(SceneName);
        
        // Auto-links scene ↔ stage
        scene.AssociatedStage = stage;
        ManagedStage = stage;
        
        // Subscribe to animation if enabled
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
    }
}
```

**User Code Pattern** (3D):
```csharp
// ✅ CORRECT - Let canvas manage scene state
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(500);  // Let canvas complete setup
        
        // Just add shapes - canvas already linked scene/stage
        var arena = Workspace.GetArena();
        var box = new FoShape3D("Box", "blue").CreateBox("Box", 2, 2, 2);
        arena.AddShapeToStage(box);
    }
}

// ❌ WRONG - Don't call SetScene()
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var (found, scene) = Canvas3DReference.GetActiveScene();
        var arena = Workspace.GetArena();
        arena.SetScene(scene);  // ← Canvas already did this!
        arena.AddShapeToStage(box);
    }
}
```

---

### Key Differences: 2D vs 3D

| Aspect | Canvas2DComponent | Canvas3DComponent |
|--------|-------------------|-------------------|
| **Container** | `IDrawing` (singleton) | `FoArena3D` (singleton) |
| **Per-Canvas Object** | `FoPage2D` | `FoStage3D` + `Scene3D` |
| **Shape Storage** | `FoPage2D.AddShape()` | `FoStage3D.AddShape()` |
| **Render Target** | `Scene3D` (JavaScript wrapper) | `Scene3D` (JavaScript wrapper) |
| **Auto-Managed State** | `drawing.SetCurrentPage()` | `scene ↔ stage linking` |
| **User Access** | `drawing.EstablishPage()` | `arena.AddShapeToStage()` |
| **Canvas Prop** | `PageName` + `SceneName` | `SceneName` only |
| **Manual Override** | ❌ Don't call `SetCurrentPage()` | ❌ Don't call `SetScene()` |

---

### Why the Difference?

**2D System**: 
- Multiple pages can share a single Scene (HTML5 Canvas element)
- Each canvas renders a specific page to its canvas element
- `SetCurrentPage()` tells the drawing which page to render on this canvas
- Canvas2DComponent calls `SetCurrentPage()` internally per canvas

**3D System**:
- Each Scene3D maps 1:1 to a Three.js scene
- Each canvas creates its own Scene3D instance
- FoStage3D is the business logic layer that feeds Scene3D
- `SetScene()` tells arena which scene to use (usually not needed)
- Canvas3DComponent creates and links scene ↔ stage automatically

---

### Multi-Canvas Behavior

#### 2D Multi-Canvas

```razor
<!-- Each canvas renders different page -->
<Canvas2DComponent SceneName="PageA" PageName="PageA" />
<Canvas2DComponent SceneName="PageB" PageName="PageB" />
```

**What Happens**:
- Canvas A calls `drawing.SetCurrentPage(pageA)` internally → renders PageA
- Canvas B calls `drawing.SetCurrentPage(pageB)` internally → renders PageB
- Each canvas manages its own rendering independently
- Pages are stored globally in `IDrawing` singleton

#### 3D Multi-Canvas

```razor
<!-- Each canvas renders different scene -->
<Canvas3DComponent SceneName="SceneA" />
<Canvas3DComponent SceneName="SceneB" />
```

**What Happens**:
- Canvas A creates Scene3D("SceneA") + FoStage3D("SceneA") → links them
- Canvas B creates Scene3D("SceneB") + FoStage3D("SceneB") → links them
- Each canvas manages its own scene/stage independently
- Stages are stored globally in `FoArena3D.StageManager`

---

### Summary: Hands-Off Management

**For Both 2D and 3D**:

✅ **DO**:
- Let Canvas components create and manage their scenes/pages
- Use `EstablishPage()` to get/create pages
- Use `arena.AddShapeToStage()` to add 3D objects
- Use `page.AddShape()` to add 2D shapes
- Trust the automatic linking system

❌ **DON'T**:
- Call `drawing.SetCurrentPage()` - Canvas2DComponent does this
- Call `arena.SetScene()` - Canvas3DComponent does this
- Manually link stages to scenes - Canvas3DComponent does this
- Create scenes/stages manually unless you know why

**Result**: Simpler code, fewer bugs, automatic lifecycle management.

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

### Issue: NuGet vs ProjectReference Conflicts

**Symptoms**: 
- "The type or namespace name 'X' does not exist"
- "Method 'BeforeAnimationRefresh' does not exist"
- Duplicate type definitions
- Version conflicts in build output

**Cause**: Mixing NuGet packages and ProjectReferences for the same library, or version mismatches.

**Solutions**:

1. ✅ **Choose One Approach** (don't mix):
   ```xml
   <!-- EITHER use all ProjectReferences -->
   <ItemGroup>
     <ProjectReference Include="..\FoundryRulesAndUnits\FoundryRulesAndUnits.csproj" />
     <ProjectReference Include="..\FoundryWorldsAndDrawings\FoundryWorldsAndDrawings.csproj" />
   </ItemGroup>
   
   <!-- OR use all NuGet packages -->
   <ItemGroup>
     <PackageReference Include="ApprenticeFoundryRulesAndUnits" Version="10.7.0" />
     <PackageReference Include="ApprenticeFoundryWorldsAndDrawings" Version="0.10.39" />
   </ItemGroup>
   ```

2. ✅ **Clear NuGet Cache** (if switching from NuGet to ProjectReference):
   ```bash
   dotnet nuget locals all --clear
   dotnet clean
   dotnet restore
   dotnet build
   ```

3. ✅ **Check Transitive Dependencies**:
   - If `FoundryWorldsAndDrawings` depends on `FoundryRulesAndUnits`
   - Both must use same approach (both ProjectReference OR both NuGet)
   - Check library .csproj files for their dependencies

4. ✅ **Verify Directory Structure** (for ProjectReference):
   ```
   YourWorkspace/
   ├── YourApp/
   ├── FoundryRulesAndUnits/     ← Must exist at this path
   └── FoundryWorldsAndDrawings/ ← Must exist at this path
   ```

---

### Issue: "BeforeAnimationRefresh" Does Not Exist

**Symptoms**: 
- Compiler error: "'FoGlyph3D' does not contain a definition for 'BeforeAnimationRefresh'"
- IntelliSense only shows `SetAnimationUpdate()`

**Cause**: Using outdated NuGet package that predates API rename.

**Solutions**:

1. ✅ **Switch to ProjectReference** (RECOMMENDED):
   ```xml
   <!-- Remove NuGet -->
   <!-- <PackageReference Include="ApprenticeFoundryWorldsAndDrawings" Version="0.10.39" /> -->
   
   <!-- Add ProjectReference -->
   <ProjectReference Include="..\FoundryWorldsAndDrawings\FoundryWorldsAndDrawings.csproj" />
   ```

2. ✅ **Check for Newer NuGet Version**:
   - Visit [NuGet.org](https://www.nuget.org/packages/ApprenticeFoundryWorldsAndDrawings)
   - Look for version > 0.10.39 with recent publish date
   - Update version in .csproj

3. ✅ **Temporary Workaround** (use old API):
   ```csharp
   // If stuck on old NuGet, use deprecated API temporarily
   shape.SetAnimationUpdate((self, tick, fps) => { /* ... */ });
   ```

---

### Issue: Missing "SetTransformStale" or Dirty Flag Methods

**Symptoms**: 
- "'Object3D' does not contain a definition for 'SetTransformStale'"
- Animations not updating in JavaScript

**Cause**: Using NuGet package from before granular dirty flag system.

**Solutions**:

1. ✅ **Switch to ProjectReference** to get latest code
2. ✅ **Use Older Dirty Flag API** (temporary workaround):
   ```csharp
   // Old API (pre-granular flags)
   shape.Value3D.SetDirty(true);  // Instead of SetTransformStale()
   ```

---

### Issue: Auto-Published RefreshUIEvent Not Working

**Symptoms**: ShapeTreeView not updating when shapes added

**Cause**: Using NuGet package from before auto-publish feature.

**Solutions**:

1. ✅ **Switch to ProjectReference** to get latest code
2. ✅ **Manual Refresh** (temporary workaround):
   ```csharp
   arena.AddShapeToStage(shape);
   // Old way - manually publish
   PubSub.Publish<RefreshUIEvent>(new RefreshUIEvent("ManualRefresh"));
   ```

---

### Issue: "BeforeAnimationRefresh Does Not Exist" on Model3D

**Symptoms**: 
- Compiler error: "'Model3D' does not contain a definition for 'BeforeAnimationRefresh'"
- IntelliSense shows no animation methods on model

**Cause**: Using raw `Model3D` instead of `FoModel3D` wrapper. Raw DTOs don't support animation callbacks.

**Root Problem**: 
`Model3D` is a **low-level JavaScript DTO** (data transfer object) that only holds properties for sending to Three.js. It has NO methods for animation, dirty tracking, or event handling.

**Solutions**:

1. ✅ **Replace Model3D with FoModel3D**:
   ```csharp
   // ❌ WRONG - Raw DTO has no methods
   var model = new Model3D()
   {
       Uuid = "my-model",
       Name = "MyModel",
       Url = url,
       Format = Model3DFormats.Gltf
   };
   // model.BeforeAnimationRefresh(...);  // ❌ Doesn't exist!
   
   // ✅ CORRECT - Wrapper has full API
   var model = new FoModel3D("MyModel")
   {
       Url = url  // Format auto-detected
   };
   
   model.BeforeAnimationRefresh((self, tick, fps) =>  // ✅ Works!
   {
       self.Transform.RotateBy(0, 0.01, 0, AngleUnit.Radians);
       self.SetTransformStale();
   });
   ```

2. ✅ **Update Object Creation Pattern**:
   ```csharp
   // ❌ OLD PATTERN (found in legacy TRISoC_Dashboard pages)
   var model = new Model3D() { /* ... */ };
   scene.AddChild(model);
   
   // ✅ NEW PATTERN
   var model = new FoModel3D("MyModel") { /* ... */ };
   arena.AddShapeToStage(model);
   ```

3. ✅ **Check All Object Types**:
   - Replace `Model3D` → `FoModel3D`
   - Replace `Mesh3D` → `FoShape3D`
   - Replace `Group3D` → `FoGroup3D`
   - Replace `Text3D` → `FoText3D`

**Why FoModel3D Exists**:
- `Model3D` = Simple DTO for JavaScript (just properties)
- `FoModel3D` = Full-featured wrapper (animation, tracking, events, hierarchy)

**Migration Checklist**:
- [ ] Replace all `new Model3D()` with `new FoModel3D()`
- [ ] Replace all `scene.AddChild()` with `arena.AddShapeToStage()`
- [ ] Remove manual `Format` assignment (auto-detected from `.glb`)
- [ ] Remove manual `Uuid` generation (auto-generated)
- [ ] Test animations work

---

### Issue: Geometry Updates Not Rendering (PreAnimationEvent vs AnimationEvent)

**Symptoms**: 
- KnModel geometry changes not visible
- Updating objects in AnimationEvent doesn't work
- Rendering happens before geometry is computed

**Cause**: Subscribing to wrong event type or updating at wrong time in animation cycle.

**Understanding the Event Order**:
```
Each Frame:
1. PreAnimationEvent (Drawing)   ← Update 2D geometry HERE
2. PreAnimationEvent (World)     ← Update 3D geometry HERE
3. AnimationEvent (Drawing)      ← Render 2D HERE
4. AnimationEvent (World)        ← Render 3D HERE
```

**Solutions**:

1. ✅ **Use PreAnimationEvent for Geometry Updates** (KnModel pattern):
   ```csharp
   // ✅ CORRECT - Update geometry BEFORE rendering
   AnimationFrameBus.SubscribeToPreAnimation(OnPreAnimationFrame);
   
   private void OnPreAnimationFrame(PreAnimationEvent evt)
   {
       if (evt.IsWorld3D())
       {
           // Compute/update KnModel geometry
           myModel.UpdateVertices();
           myModel.SetGeometryStale();
       }
   }
   ```

2. ✅ **Use AnimationEvent for Rendering/UI Updates**:
   ```csharp
   // ✅ CORRECT - Update UI AFTER geometry computed
   AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
   
   private void OnAnimationFrame(AnimationEvent evt)
   {
       if (evt.IsWorld3D())
       {
           // Update UI with latest FPS/tick
           _fps = evt.fps;
           InvokeAsync(StateHasChanged);
       }
   }
   ```

3. ❌ **DON'T Update Geometry in AnimationEvent**:
   ```csharp
   // ❌ WRONG - Too late, rendering already started
   private void OnAnimationFrame(AnimationEvent evt)
   {
       myModel.UpdateVertices();  // Won't render until NEXT frame
   }
   ```

4. ✅ **Unsubscribe from Both Events in Dispose**:
   ```csharp
   public void Dispose()
   {
       AnimationFrameBus.UnSubscribeFromPreAnimation(OnPreAnimationFrame);
       AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
   }
   ```

**When to Use Which**:
- **PreAnimationEvent**: Physics, geometry computation, KnModel updates, data processing
- **AnimationEvent**: Rendering, UI updates, FPS display, canvas operations

---

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
3. ✅ Wait for canvas initialization: `await Task.Delay(500)` in `OnAfterRenderAsync`
4. ✅ Check SceneName matches: Canvas SceneName must match stage name
5. ✅ Check camera position (default: looking at origin)
6. ✅ Don't call `arena.SetScene()` - Canvas3DComponent handles this automatically

### Issue: Transform Changes Not Visible

**Symptoms**: Updating position/rotation has no effect

**Common Causes & Solutions**:

1. ✅ **Mutating Copy Instead of Assigning**:
   ```csharp
   // ❌ WRONG - Modifies a copy
   shape.Transform.Position.X = 10;
   
   // ✅ CORRECT - Assign new instance
   shape.Transform.Position = new Vector3(10, shape.Transform.Position.Y, shape.Transform.Position.Z);
   ```

2. ✅ **Use `BeforeAnimationRefresh()` for per-frame animation**:
   ```csharp
   shape.BeforeAnimationRefresh((self, tick, fps) => {
       self.Transform.Position = newPosition;  // Auto-tracked!
   });
   ```

3. ✅ **Property setters auto-track** - Don't need manual `SetTransformStale()`:
   ```csharp
   // This is enough - SetTransformStale() called automatically
   shape.Transform.Position = new Vector3(x, y, z);
   ```

4. ✅ **Manual call only needed for direct Object3D access**:
   ```csharp
   var (success, mesh) = shape.GetComputedMesh();
   mesh.Position = new Vector3(x, y, z);
   mesh.SetTransformStale();  // ← Manual call needed here
   ```

5. ✅ **For geometry changes, call `SetGeometryStale()`**:
   ```csharp
   shape.Width = newWidth;
   shape.SetGeometryStale();  // ← Geometry requires manual call
   ```

---

## Migration Checklist

### For Each New Page

- [ ] **0. Choose Dependency Approach**
  - [ ] Development: Use ProjectReference for latest code
  - [ ] Production: Check NuGet package versions are current
  - [ ] Don't mix NuGet and ProjectReference for same library
  - [ ] Clear NuGet cache if switching approaches

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
  - [ ] Add `@ref` to Canvas component (optional)
  - [ ] DON'T call `arena.SetScene()` - Canvas3DComponent handles automatically
  - [ ] Use `arena.AddShapeToStage()` to add objects (auto-publishes RefreshUIEvent)
  - [ ] OR use `EstablishPage()` for 2D canvases
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

- [ ] **1. Update Object Types** (CRITICAL!)
  - [ ] Replace `Model3D` with `FoModel3D`
  - [ ] Replace `Mesh3D` with `FoShape3D`
  - [ ] Replace `Group3D` with `FoGroup3D`
  - [ ] Replace `Text3D` with `FoText3D`
  - [ ] Verify NO raw DTO types remain (only use `Fo*` wrappers)

- [ ] **2. Update Adding Objects** (CRITICAL!)
  - [ ] Replace `scene.AddChild()` with `arena.AddShapeToStage()`
  - [ ] Replace `scene.Request3DModel()` with `new FoModel3D()`
  - [ ] Remove manual scene lookups
  - [ ] Remove manual `RefreshUIEvent` publishing

- [ ] **3. Update Animation APIs** (CRITICAL!)
  - [ ] Replace `SetAnimationUpdate()` with `BeforeAnimationRefresh()`
  - [ ] Replace `ClearAnimationUpdate()` with `ClearAnimationRefresh()`
  - [ ] Remove `SetAfterUpdateAction()` calls (automatic now)
  - [ ] Add `SetTransformStale()` in animation callbacks

- [ ] **4. Update Namespaces**
  - [ ] Replace old namespace references
  - [ ] Update `@using` directives
  - [ ] Fix any compilation errors

- [ ] **5. Update Canvas Components**
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

## Migration from Old APIs (TRISoC_Dashboard Pattern)

If you're migrating code from older examples (Clock.razor.cs, WorldPage.razor.cs, Home.razor.cs), you'll encounter **three critical anti-patterns** that must be updated.

### ❌ Anti-Pattern 1: Using Raw `Model3D` Instead of `FoModel3D`

**Problem**: Raw `Model3D` objects are **low-level JavaScript DTOs** that don't support:
- ❌ `BeforeAnimationRefresh()` callbacks
- ❌ Dirty flag tracking (`SetTransformStale()`, `SetGeometryStale()`)
- ❌ `Transform3` property setters (no automatic tracking)
- ❌ Event-based architecture
- ❌ Tree hierarchy management

**Old Pattern** (found in legacy pages):
```csharp
// ❌ WRONG - Raw Model3D (no animation support!)
var model = new Model3D()
{
    Uuid = "my-model",
    Name = "MyModel",
    Url = url,
    Format = Model3DFormats.Gltf,
    Transform = new Transform3("Transform")
};
scene.AddChild(model);  // Direct scene manipulation
```

**New Pattern**:
```csharp
// ✅ CORRECT - Use FoModel3D wrapper
var model = new FoModel3D("MyModel")
{
    Url = url,  // Format auto-detected from .glb extension
    Transform = new Transform3("ModelTransform")
    {
        Position = new Vector3(0, 0, 0),
        Scale = new Vector3(1, 1, 1)
    }
};

// Add animation support
model.BeforeAnimationRefresh((self, tick, fps) =>
{
    self.Transform.RotateBy(0, 0.01, 0, AngleUnit.Radians);
    self.SetTransformStale();
});

arena.AddShapeToStage(model);  // Proper arena API
```

**What FoModel3D Provides**:
- ✅ Inherits from `FoShape3D` → `FoGlyph3D` → full feature set
- ✅ Automatic dirty tracking via property setters
- ✅ `BeforeAnimationRefresh()` support
- ✅ Granular stale flags (`SetTransformStale()`, `SetMaterialStale()`)
- ✅ Tree hierarchy (`AddSubGlyph3D()`, `GetTreeChildren()`)
- ✅ Event-based updates via `RefreshUIEvent`
- ✅ Proper disposal and lifecycle management

---

### ❌ Anti-Pattern 2: Using `scene.AddChild()` Instead of `arena.AddShapeToStage()`

**Problem**: Direct scene manipulation **bypasses the tracking system**:
- ❌ No `RefreshUIEvent` published (UI doesn't update)
- ❌ No automatic dirty flag queuing
- ❌ No stage-based organization
- ❌ Objects not tracked in ShapeTreeView
- ❌ Manual scene/stage linking required

**Old Pattern** (found in legacy pages):
```csharp
// ❌ WRONG - Direct scene manipulation
var (found, scene) = arena.CurrentScene();
if (found)
{
    var model = new Model3D() { /* ... */ };
    scene.AddChild(model);  // Low-level API
}
```

**New Pattern**:
```csharp
// ✅ CORRECT - Use arena.AddShapeToStage()
var arena = Workspace.GetArena();
var model = new FoModel3D("MyModel") { /* ... */ };

arena.AddShapeToStage(model);
// → Adds to current stage
// → Links to scene automatically
// → Publishes RefreshUIEvent
// → Queues for JavaScript update
// → Sets up OnDelete callback
```

**What `AddShapeToStage()` Does** (you get all this for free):
```csharp
// Inside AddShapeToStage() implementation:
public V AddShapeToStage<V>(V shape) where V : FoGlyph3D
{
    var stage = CurrentStage();
    
    // 1. Add to stage (tracking)
    stage.AddShape<V>(shape);
    
    // 2. Set up deletion callback
    shape.OnDelete = (FoGlyph3D item) =>
    {
        item.DeleteFromStage(stage);
        PubSub.Publish<RefreshUIEvent>(new RefreshUIEvent("FoArena3D:RemoveShape"));
    };
    
    // 3. Publish UI refresh event
    PubSub.Publish<RefreshUIEvent>(new RefreshUIEvent("FoArena3D:AddShape"));
    
    // 4. Queue for JavaScript update
    shape.QueueForMeshUpdate();
    
    return shape;
}
```

**Migration Checklist**:
1. ✅ Replace `scene.AddChild(rawModel)` with `arena.AddShapeToStage(foModel)`
2. ✅ Replace `scene.Request3DModel()` with `FoModel3D` constructor
3. ✅ Remove manual scene lookups - trust Canvas3DComponent
4. ✅ Remove manual `RefreshUIEvent` publishing - it's automatic

---

### ❌ Anti-Pattern 3: Raw `Object3D` Types (No FoGlyph3D Features)

**Problem**: Using raw `Object3D`, `Mesh3D`, `Group3D` types directly **loses all high-level features**.

**Old Pattern**:
```csharp
// ❌ WRONG - Raw Object3D (no features!)
var group = new Group3D("MyGroup");
group.AddChild(new Mesh3D());  // Low-level API
scene.AddChild(group);  // No tracking
```

**New Pattern**:
```csharp
// ✅ CORRECT - Use FoGroup3D wrapper
var group = new FoGroup3D("MyGroup");
var box = new FoShape3D("Box", "blue").CreateBox("Box", 2, 2, 2);
group.AddSubGlyph3D(box);  // High-level API

arena.AddShapeToStage(group);
```

**Type Mapping** (Old → New):

| ❌ Old (Raw DTO) | ✅ New (Fo Wrapper) | Features Added |
|------------------|---------------------|----------------|
| `Model3D` | `FoModel3D` | Animation, dirty tracking, hierarchy |
| `Mesh3D` | `FoShape3D` | Property setters, stale flags, events |
| `Group3D` | `FoGroup3D` | Child management, tree view |
| `Object3D` | `FoGlyph3D` | Full feature set, lifecycle |
| `Text3D` | `FoText3D` | Text rendering, alignment |

**Rule of Thumb**:
- ✅ **Use `Fo*` classes** for application code (FoModel3D, FoShape3D, FoGroup3D)
- ❌ **Avoid raw classes** (Model3D, Mesh3D, Group3D) - these are internal DTOs

---

### Common Migration Scenarios

#### Scenario 1: Loading GLTF Models

**Before** (TRISoC_Dashboard pattern):
```csharp
// ❌ OLD
var model = new Model3D()
{
    Uuid = Guid.NewGuid().ToString(),
    Name = "Submarine",
    Url = GetReferenceTo(@"storage/StaticFiles/submarine.glb"),
    Format = Model3DFormats.Gltf,
    Transform = new Transform3("SubTransform")
    {
        Position = new Vector3(0, 0, 0)
    }
};

var (found, scene) = arena.CurrentScene();
if (found)
    scene.AddChild(model);
```

**After** (current best practice):
```csharp
// ✅ NEW
var model = new FoModel3D("Submarine")
{
    Url = GetReferenceTo(@"storage/StaticFiles/submarine.glb"),
    Transform = new Transform3("SubTransform")
    {
        Position = new Vector3(0, 0, 0)
    }
};

arena.AddShapeToStage(model);
```

**Benefits**:
- ✅ 40% less code
- ✅ Automatic format detection (`.glb` → `Model3DFormats.Gltf`)
- ✅ No manual scene lookup needed
- ✅ Automatic tracking and UI refresh

---

#### Scenario 2: Adding Animation to Models

**Before**:
```csharp
// ❌ OLD - No animation support on raw Model3D
var model = new Model3D() { /* ... */ };
scene.AddChild(model);

// Can't do this - Model3D has no SetAnimationUpdate!
// model.SetAnimationUpdate(...); // ❌ Doesn't exist
```

**After**:
```csharp
// ✅ NEW - Animation built into FoModel3D
var model = new FoModel3D("AnimatedModel") { /* ... */ };

model.BeforeAnimationRefresh((self, tick, fps) =>
{
    // Rotate model
    self.Transform.RotateBy(0, 0.01, 0, AngleUnit.Radians);
    self.SetTransformStale();
});

arena.AddShapeToStage(model);
```

---

#### Scenario 3: Creating Custom Shapes

**Before**:
```csharp
// ❌ OLD
var mesh = new Mesh3D()
{
    Uuid = Guid.NewGuid().ToString(),
    Geometry = new BoxGeometry(2, 2, 2),
    Material = new MeshBasicMaterial() { Color = "blue" }
};
scene.AddChild(mesh);
```

**After**:
```csharp
// ✅ NEW
var box = new FoShape3D("MyBox", "blue")
    .CreateBox("Box", 2, 2, 2);

box.Transform = new Transform3("BoxTransform")
{
    Position = new Vector3(0, 0, 0)
};

arena.AddShapeToStage(box);
```

---

### Migration Priority Guide

**High Priority** (breaks in latest version):
1. ⚠️ Replace `Model3D` with `FoModel3D`
2. ⚠️ Replace `scene.AddChild()` with `arena.AddShapeToStage()`
3. ⚠️ Replace `SetAnimationUpdate()` with `BeforeAnimationRefresh()`

**Medium Priority** (deprecated but still works):
1. 📝 Replace `scene.Request3DModel()` with `new FoModel3D()`
2. 📝 Remove manual scene lookups
3. 📝 Remove manual `RefreshUIEvent` publishing

**Low Priority** (cosmetic improvements):
1. 💡 Use property setters instead of manual `SetDirty()`
2. 💡 Use granular stale flags (`SetTransformStale()` vs `SetDirty()`)
3. 💡 Add type-safe shape retrieval (`arena.GetShapes<FoModel3D>()`)

---

## Deprecated APIs & Migration Guide

This section documents APIs that have been **removed or replaced** in recent versions. If you encounter these in older code or documentation, use the migration paths below.

### Animation APIs

#### ❌ SetAnimationUpdate() → ✅ BeforeAnimationRefresh()

**Status**: **REMOVED** in v10.7+

**Old Pattern**:
```csharp
// ❌ NO LONGER WORKS
shape.SetAnimationUpdate((self, tick, fps) =>
{
    self.Transform.Position = new Vector3(x, y, z);
});
```

**New Pattern**:
```csharp
// ✅ USE THIS
shape.BeforeAnimationRefresh((self, tick, fps) =>
{
    self.Transform.Position = new Vector3(x, y, z);
    self.SetTransformStale();  // Mark for JavaScript update
});
```

**Why Changed**: 
- `BeforeAnimationRefresh()` better reflects timing (runs before scene refresh)
- Consistent naming with the animation pipeline flow
- Same callback signature, just renamed

**Migration Steps**:
1. Find all uses of `SetAnimationUpdate()`
2. Replace with `BeforeAnimationRefresh()`
3. Ensure dirty flags are set (`SetTransformStale()`, `SetGeometryStale()`)
4. Replace `ClearAnimationUpdate()` with `ClearAnimationRefresh()`

---

#### ❌ SetAfterUpdateAction() → ✅ Auto-published RefreshUIEvent

**Status**: **REMOVED** in v10.7+

**Old Pattern**:
```csharp
// ❌ NO LONGER WORKS
shape.SetAfterUpdateAction(() =>
{
    StateHasChanged();  // Manually trigger UI refresh
});
```

**New Pattern**:
```csharp
// ✅ AUTOMATIC - No code needed!
arena.AddShapeToStage(shape);
// → RefreshUIEvent automatically published
// → ShapeTreeView and other UI components update automatically
```

**Why Changed**:
- Manual UI refresh callbacks were error-prone
- `AddShapeToStage()` now auto-publishes `RefreshUIEvent`
- UI components subscribe to `RefreshUIEvent` for automatic updates
- Reduces boilerplate code

**Migration Steps**:
1. Remove all `SetAfterUpdateAction()` calls
2. Trust automatic `RefreshUIEvent` publishing
3. If custom UI refresh needed, subscribe to `RefreshUIEvent` instead

---

#### ❌ RefreshToScene() → ✅ arena.AddShapeToStage()

**Status**: **REMOVED** in v10.7+

**Old Pattern**:
```csharp
// ❌ NO LONGER WORKS
var shape = new FoShape3D("MyBox", "blue").CreateBox("Box", 2, 2, 2);
shape.RefreshToScene(scene);  // Manual scene refresh
```

**New Pattern**:
```csharp
// ✅ USE THIS
var arena = Workspace.GetArena();
var shape = new FoShape3D("MyBox", "blue").CreateBox("Box", 2, 2, 2);
arena.AddShapeToStage(shape);
// → Automatically added to current stage
// → Stage linked to scene automatically
// → RefreshUIEvent published
// → Shape queued for JavaScript update
```

**Why Changed**:
- Scene-level API was too low-level and error-prone
- Stage-first API is clearer and safer
- Automatic scene/stage linking prevents mismatches
- Consistent with Canvas3DComponent auto-management

**Migration Steps**:
1. Replace `shape.RefreshToScene(scene)` with `arena.AddShapeToStage(shape)`
2. Remove manual scene lookups
3. Trust Canvas3DComponent to create/link scenes and stages

---

### Removed Classes

#### ❌ FoMeshWrapper

**Status**: **COMPLETELY REMOVED**

**Old Usage**:
```csharp
// ❌ NO LONGER EXISTS
var wrapper = new FoMeshWrapper(mesh3D);
wrapper.UpdateGeometry();
```

**New Pattern**:
```csharp
// ✅ USE DIRECT MESH3D
var mesh = new Mesh3D();
mesh.SetGeometryDirty(true);  // Directly on Mesh3D
```

**Why Removed**:
- Wrapper added unnecessary abstraction layer
- Direct `Mesh3D` usage is simpler and clearer
- Dirty flag system makes wrapper obsolete

**Migration Steps**:
1. Remove all `FoMeshWrapper` references
2. Use `Mesh3D` directly
3. Call dirty flag methods directly on mesh objects

---

#### ❌ DrawFace() Method

**Status**: **DEPRECATED** (still exists in legacy code but should not be used)

**Found In**: `TrisocTech.cs`, `CageTech.cs` (legacy examples)

**Old Pattern**:
```csharp
// ❌ DEPRECATED - Do not use in new code
DrawFace(root, "Left", leftFace.FaceMesh("blue", .8));
```

**New Pattern**:
```csharp
// ✅ USE MODERN SHAPE API
var face = new FoShape3D("LeftFace", "blue")
    .CreateCustomMesh("LeftFace", faceMesh);
face.Transform.Position = new Vector3(x, y, z);
root.AddSubGlyph3D(face);
```

**Why Deprecated**:
- `DrawFace()` was a legacy helper method
- Modern API uses `FoShape3D.CreateCustomMesh()`
- Better integration with dirty tracking system
- Consistent with other shape creation patterns

**Migration Steps**:
1. Replace `DrawFace()` calls with `FoShape3D` creation
2. Use `CreateCustomMesh()` for custom geometry
3. Use `AddSubGlyph3D()` for hierarchies

---

### Scene Management Changes

#### ❌ arena.SetScene(scene) → ✅ Automatic via Canvas3DComponent

**Status**: **STILL EXISTS** but **NOT RECOMMENDED** for normal use

**Old Pattern**:
```csharp
// ❌ UNNECESSARY in most cases
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var (found, scene) = Canvas3DReference.GetActiveScene();
        var arena = Workspace.GetArena();
        arena.SetScene(scene);  // Canvas already did this!
    }
}
```

**New Pattern**:
```csharp
// ✅ SIMPLIFIED - Let canvas handle it
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(500);  // Let canvas complete setup
        // Canvas3DComponent already created scene, stage, and linked them
        // Just start adding shapes!
        var arena = Workspace.GetArena();
        arena.AddShapeToStage(myShape);
    }
}
```

**When Still Useful**:
- Advanced: Dynamically switching between multiple scenes
- Custom: Manual scene management without Canvas3DComponent

**Migration Steps**:
1. Remove `arena.SetScene()` calls from `OnAfterRenderAsync`
2. Trust Canvas3DComponent to handle scene/stage linking
3. Only use `SetScene()` for advanced multi-scene scenarios

---

#### ❌ drawing.SetCurrentPage() → ✅ Automatic via Canvas2DComponent

**Status**: **STILL EXISTS** but **NOT RECOMMENDED** for normal use

**Old Pattern**:
```csharp
// ❌ UNNECESSARY with multiple canvases
var page = drawing.EstablishPage<FoPage2D>("PageA");
drawing.SetCurrentPage(page);  // Canvas already does this per canvas!
page.AddShape(shape);
```

**New Pattern**:
```csharp
// ✅ SIMPLIFIED - Let canvas handle it
var page = drawing.EstablishPage<FoPage2D>("PageA");
page.AddShape(shape);  // Canvas2DComponent manages current page per canvas
```

**When Still Useful**:
- Single canvas rendering multiple pages sequentially
- Custom page switching logic

**Migration Steps**:
1. Remove `drawing.SetCurrentPage()` calls in multi-canvas scenarios
2. Each Canvas2DComponent manages its own current page
3. Only use `SetCurrentPage()` for single-canvas page switching

---

### JavaScript API Changes

#### ❌ AppBrowser.* → ✅ UnifiedAnimationManager

**Status**: **REMOVED** in unified animation refactor

**Old Pattern**:
```typescript
// ❌ NO LONGER EXISTS
AppBrowser.Initialize();
AppBrowser.startAnimation();
AppBrowser.stopAnimation();
AppBrowser.Finalize();
```

**New Pattern**:
```typescript
// ✅ USE THIS
window.FoundryWorldsAndDrawings.unifiedAnimationManager.startAnimation();
window.FoundryWorldsAndDrawings.unifiedAnimationManager.stopAnimation();
window.FoundryWorldsAndDrawings.unifiedAnimationManager.isAnimationRunning();
```

**Why Changed**:
- Unified animation system for both 2D and 3D
- Better namespace organization
- Centralized animation lifecycle management

**Migration Steps**:
1. Replace `AppBrowser.*` with `window.FoundryWorldsAndDrawings.unifiedAnimationManager.*`
2. Remove manual `Initialize()`/`Finalize()` calls
3. Use C# `FoundryService.StartGlobalAnimation()` instead of JavaScript calls

---

#### ❌ IThreeDService → ✅ Removed (use IWorkspace/IArena)

**Status**: **COMPLETELY REMOVED**

**Old Pattern**:
```csharp
// ❌ NO LONGER EXISTS
[Inject] public IThreeDService ThreeDService { get; set; }

ThreeDService.SetActiveScene(scene);
ThreeDService.ForceRenderAnimationFrame();
if (ThreeDService.GetIsCurrentlyRendering()) { }
ThreeDService.QueueDirtyObject(obj);
```

**New Pattern**:
```csharp
// ✅ USE THIS
[Inject] public IWorkspace Workspace { get; set; }

var arena = Workspace.GetArena();
arena.SetScene(scene);  // Usually not needed - Canvas handles it
// Animation frame triggered automatically by AnimationFrameBus
if (arena.IsCurrentlyRendering) { }
AnimationFrameBus.QueueDirtyObject(obj);
```

**Why Removed**:
- Redundant with `IWorkspace` and `IArena` abstractions
- Unified animation system made service obsolete
- Better separation of concerns

**Migration Steps**:
1. Remove `IThreeDService` injections
2. Use `Workspace.GetArena()` instead
3. Use `AnimationFrameBus` static methods for global operations
4. Use `IArena` instance methods for arena-specific operations

---

### Migration Priority Guide

**High Priority** (Will break builds):
- ✅ Replace `SetAnimationUpdate()` → `BeforeAnimationRefresh()`
- ✅ Remove `FoMeshWrapper` usage
- ✅ Remove `IThreeDService` injections
- ✅ Replace `AppBrowser.*` JavaScript calls

**Medium Priority** (Works but unnecessary):
- ✅ Remove `arena.SetScene()` from OnAfterRenderAsync
- ✅ Remove `drawing.SetCurrentPage()` in multi-canvas scenarios
- ✅ Replace `RefreshToScene()` → `arena.AddShapeToStage()`

**Low Priority** (Optional cleanup):
- ✅ Remove `SetAfterUpdateAction()` calls
- ✅ Replace `DrawFace()` with modern API
- ✅ Update outdated documentation references

---

### Version History

| Version | Date | Breaking Changes |
|---------|------|------------------|
| v10.7+ | Nov 2025 | `SetAnimationUpdate()` → `BeforeAnimationRefresh()` |
| v10.7+ | Nov 2025 | Removed `SetAfterUpdateAction()`, auto-publish `RefreshUIEvent` |
| v10.7+ | Nov 2025 | Removed `RefreshToScene()`, use `arena.AddShapeToStage()` |
| v10.6+ | Oct 2025 | Removed `FoMeshWrapper` |
| v10.5+ | Sep 2025 | Unified animation system, removed `IThreeDService` |
| v10.5+ | Sep 2025 | Removed `AppBrowser.*` JavaScript API |

---

## Quick Reference: TRISoC_Dashboard Migration Patterns

This section provides **copy-paste ready examples** for the most common migration scenarios you'll encounter.

### Pattern 1: Loading a GLTF Model (MOST COMMON)

```csharp
// ❌ OLD (TRISoC_Dashboard - DON'T USE)
var model = new Model3D()
{
    Uuid = Guid.NewGuid().ToString(),
    Name = "MyModel",
    Url = GetReferenceTo(@"storage/StaticFiles/model.glb"),
    Format = Model3DFormats.Gltf,
    Transform = new Transform3("Trans") { Position = new Vector3(0, 0, 0) }
};
var (found, scene) = arena.CurrentScene();
if (found) scene.AddChild(model);

// ✅ NEW (Current - USE THIS)
var model = new FoModel3D("MyModel")
{
    Url = GetReferenceTo(@"storage/StaticFiles/model.glb"),
    Transform = new Transform3("Trans") { Position = new Vector3(0, 0, 0) }
};
arena.AddShapeToStage(model);
```

### Pattern 2: Animated Rotating Model

```csharp
// ❌ OLD (Doesn't work - Model3D has no animation API)
var model = new Model3D() { /* ... */ };
scene.AddChild(model);
// model.SetAnimationUpdate(...);  // ❌ Method doesn't exist!

// ✅ NEW (Current - USE THIS)
var model = new FoModel3D("RotatingModel")
{
    Url = GetReferenceTo(@"storage/StaticFiles/model.glb"),
    Transform = new Transform3("Trans") { Position = new Vector3(0, 2, 0) }
};

model.BeforeAnimationRefresh((self, tick, fps) =>
{
    self.Transform.RotateBy(0, 0.01, 0, AngleUnit.Radians);
    self.SetTransformStale();
});

arena.AddShapeToStage(model);
```

### Pattern 3: Creating Custom Geometry (Box, Sphere, etc.)

```csharp
// ❌ OLD (Low-level mesh creation)
var mesh = new Mesh3D()
{
    Uuid = Guid.NewGuid().ToString(),
    Geometry = new BoxGeometry(2, 2, 2),
    Material = new MeshBasicMaterial() { Color = "blue" },
    Transform = new Transform3("BoxTrans") { Position = new Vector3(0, 0, 0) }
};
scene.AddChild(mesh);

// ✅ NEW (High-level shape creation - USE THIS)
var box = new FoShape3D("MyBox", "blue")
    .CreateBox("Box", 2, 2, 2);

box.Transform = new Transform3("BoxTrans") 
{ 
    Position = new Vector3(0, 0, 0) 
};

arena.AddShapeToStage(box);
```

### Pattern 4: Grouping Multiple Objects

```csharp
// ❌ OLD (Low-level group manipulation)
var group = new Group3D("MyGroup");
var mesh1 = new Mesh3D() { /* ... */ };
var mesh2 = new Mesh3D() { /* ... */ };
group.AddChild(mesh1);
group.AddChild(mesh2);
scene.AddChild(group);

// ✅ NEW (High-level hierarchy - USE THIS)
var group = new FoGroup3D("MyGroup");

var box = new FoShape3D("Box", "red").CreateBox("Box", 1, 1, 1);
var sphere = new FoShape3D("Sphere", "blue").CreateSphere("Sphere", 0.5);

group.AddSubGlyph3D(box);
group.AddSubGlyph3D(sphere);

arena.AddShapeToStage(group);
```

### Pattern 5: Adding 3D Text

```csharp
// ❌ OLD (If you find this pattern)
var text = new Text3D()
{
    Uuid = Guid.NewGuid().ToString(),
    Text = "Hello",
    Transform = new Transform3("TextTrans")
};
scene.AddChild(text);

// ✅ NEW (Current - USE THIS)
var text = new FoText3D("Label", "Hello World")
{
    Transform = new Transform3("TextTrans")
    {
        Position = new Vector3(0, 5, 0)
    }
};

arena.AddShapeToStage(text);
```

### Pattern 6: Page Initialization with Delay

```csharp
// ❌ OLD (Immediate setup - often causes timing issues)
protected override void OnInitialized()
{
    var model = new Model3D() { /* ... */ };
    var (found, scene) = arena.CurrentScene();
    if (found) scene.AddChild(model);
}

// ✅ NEW (Use OnAfterRenderAsync with delay - USE THIS)
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(500);  // Wait for canvas initialization
        
        var model = new FoModel3D("MyModel") { /* ... */ };
        arena.AddShapeToStage(model);
        
        StateHasChanged();
    }
}
```

### Pattern 7: Animation Subscription (Component-Level)

```csharp
// ❌ OLD (If you find references to RunAnimation property)
if (RunAnimation)  // ❌ Property doesn't exist anymore
{
    // Animation logic
}

// ✅ NEW (Subscribe to AnimationFrameBus - USE THIS)
protected override void OnInitialized()
{
    AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
}

private void OnAnimationFrame(AnimationEvent evt)
{
    if (evt.IsWorld3D())
    {
        _tick = evt.tick;
        _fps = evt.fps;
        InvokeAsync(StateHasChanged);
    }
}

public void Dispose()
{
    AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
}
```

### Pattern 8: Removing Manual Scene Management

```csharp
// ❌ OLD (Manual scene/stage linking - DON'T DO THIS)
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        var arena = Workspace.GetArena();
        if (found) arena.SetScene(scene);  // ❌ Manual linking
    }
}

// ✅ NEW (Trust Canvas3DComponent - USE THIS)
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await Task.Delay(500);
        // Canvas3DComponent already linked scene/stage automatically!
        // Just add your objects:
        var model = new FoModel3D("MyModel") { /* ... */ };
        arena.AddShapeToStage(model);
    }
}
```

### Search & Replace Guide

Use these regex patterns to find old code:

| Find This | Replace With | Why |
|-----------|-------------|-----|
| `new Model3D\(` | `new FoModel3D(` | Use high-level wrapper |
| `new Mesh3D\(` | `new FoShape3D(` | Use high-level wrapper |
| `new Group3D\(` | `new FoGroup3D(` | Use high-level wrapper |
| `scene.AddChild\(` | `arena.AddShapeToStage(` | Use proper tracking API |
| `.SetAnimationUpdate\(` | `.BeforeAnimationRefresh(` | Use current animation API |
| `arena.SetScene\(` | `// AUTO-MANAGED` | Remove manual scene linking |
| `Uuid = Guid.NewGuid()` | `// AUTO-GENERATED` | Remove manual UUID generation |
| `Format = Model3DFormats.Gltf` | `// AUTO-DETECTED` | Remove manual format setting |

### Common Gotchas & Solutions

**Gotcha 1: "BeforeAnimationRefresh not found"**
```csharp
// Problem: Using Model3D instead of FoModel3D
var model = new Model3D();  // ❌ Has no animation API
model.BeforeAnimationRefresh(...);  // ❌ Doesn't exist!

// Solution: Use FoModel3D
var model = new FoModel3D("MyModel");  // ✅ Has full API
model.BeforeAnimationRefresh(...);  // ✅ Works!
```

**Gotcha 2: Objects not appearing in scene**
```csharp
// Problem: Added to scene but not to stage
scene.AddChild(model);  // ❌ No tracking, no UI update

// Solution: Add to arena/stage
arena.AddShapeToStage(model);  // ✅ Full tracking, UI updates
```

**Gotcha 3: Animation not running**
```csharp
// Problem: Set callback but never added to scene
model.BeforeAnimationRefresh(...);  // Set callback
// But forgot to add to arena! ❌

// Solution: Always add to arena
model.BeforeAnimationRefresh(...);
arena.AddShapeToStage(model);  // ✅ Now callbacks will fire
```

**Gotcha 4: Transform changes not visible**
```csharp
// Problem: Modifying a copy instead of assigning
model.Transform.Position.X = 10;  // ❌ Modifies copy, no tracking

// Solution: Assign new instance
model.Transform.Position = new Vector3(10, 0, 0);  // ✅ Triggers tracking
```

---

## Summary

**Key Takeaways**:

1. **Library Structure**: FoundryRulesAndUnits → FoundryWorldsAndDrawings → (optional) FoundryMentorModeler
2. **DI Setup**: Register `IFoundryService` and `IWorkspace` as singletons
3. **Canvas Components**: Use `Canvas3DComponent` for 3D, `Canvas2DComponent` for 2D
4. **Page Setup**: Use `OnAfterRenderAsync(firstRender)` with delay for initialization
5. **Object Types**: Use `FoModel3D`, `FoShape3D`, `FoGroup3D` (NOT raw `Model3D`, `Mesh3D`)
6. **Adding Objects**: Use `arena.AddShapeToStage()` (NOT `scene.AddChild()`)
7. **Animation Events**: Use `PreAnimationEvent` for geometry updates, `AnimationEvent` for rendering/UI
8. **Event Order**: PreAnimation (Drawing) → PreAnimation (World) → Animation (Drawing) → Animation (World)
9. **Multi-Canvas**: Use `EstablishPage()`, don't call `SetCurrentPage()` or `SetScene()`
10. **Transforms**: Property setters **automatically** trigger dirty tracking - no manual calls needed!
11. **Granular Flags**: Use `SetGeometryStale()`, `SetMaterialStale()` for non-transform changes
12. **Scene Management**: Let Canvas components handle scene/stage linking automatically
13. **Object Animation**: Use `BeforeAnimationRefresh()` for per-object animation (NOT `SetAnimationUpdate()`)
14. **UI Refresh**: Trust automatic `RefreshUIEvent` publishing
15. **Dependencies**: Use ProjectReference during development for latest APIs

**Next Steps**:
- Review [MultiCanvas2DTest.razor](../Components/Pages/MultiCanvas2DTest.razor) for 2D examples
- Review [Clock.razor](../Components/Pages/Clock.razor) for 3D animation examples
- Consult [COPILOT_HANDOFF_GUIDE.md](./COPILOT_HANDOFF_GUIDE.md) for additional context
- Check [MULTI_CANVAS_ARCHITECTURE.md](../../FoundryWorldsAndDrawings/MULTI_CANVAS_ARCHITECTURE.md) for architectural details

---

**Document Version**: 2.1  
**Last Updated**: 2025-11-23  
**Author**: GitHub Copilot (documenting work from 2025-11-22)  
**Context**: Three2025 application migration to FoundryWorldsAndDrawings libraries  
**Major Updates**:  
- v2.0: Added comprehensive migration guide for TRISoC_Dashboard pattern (Model3D → FoModel3D, scene.AddChild → arena.AddShapeToStage)
- v2.1: Added Quick Reference section with 8 copy-paste patterns, search & replace guide, and common gotchas
