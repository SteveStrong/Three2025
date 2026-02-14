# Razor Page Canvas Construction Guide

**Purpose**: Complete specification for building Blazor Razor pages with Canvas2D/Canvas3D components, animation controls, tree views, and button controls.

**Version**: 1.0  
**Date**: January 22, 2026  
**Platform**: .NET 9, Blazor Server  
**Libraries**: FoundryWorldsAndDrawings, FoundryMentorModeler, Radzen.Blazor

---

## Table of Contents

1. [Overview](#overview)
2. [⚠️ CRITICAL: JavaScript Library Injection](#-critical-javascript-library-injection-apprazor)
3. [Required Namespaces and Imports](#required-namespaces-and-imports)
4. [Canvas Components Specification](#canvas-components-specification)
5. [Animation Control System](#animation-control-system)
6. [Tree View Components](#tree-view-components)
7. [Button and Command Patterns](#button-and-command-patterns)
8. [Layout Patterns with RadzenSplitter](#layout-patterns-with-radzensplitter)
9. [Complete Page Template](#complete-page-template)
10. [Code-Behind Template](#code-behind-template)
11. [Common Patterns and Best Practices](#common-patterns-and-best-practices)
12. [KnModel/KnComponent Architecture](#knmodelkncomponent-architecture)
13. [Common Mistakes to Avoid](#common-mistakes-to-avoid)

---

## Overview

This document provides a complete specification for building Blazor Razor pages that incorporate:

- **Canvas2DComponent**: 2D drawing surface using HTML5 Canvas
- **Canvas3DComponent**: 3D visualization surface using Three.js
- **Animation Control**: Start, pause, resume, and stop animation loops
- **Tree Views**: Hierarchical display of models (MentorTreeView) and shapes (ShapeTreeView)
- **Buttons and Commands**: User interaction controls using Bootstrap and Radzen components

### Architecture Principles

1. **Unified Animation System**: Single animation loop drives both 2D and 3D canvases
2. **Component Independence**: Each canvas manages its own stage/page lifecycle
3. **Event-Driven Updates**: Use pub/sub messaging for state changes
4. **Dependency Injection**: All services injected via `[Inject]` attribute

---

## ⚠️ CRITICAL: JavaScript Library Injection (App.razor)

**You MUST configure your `App.razor` (or `_Host.cshtml`) file correctly or the canvas components will not work.**

### Complete App.razor Template

```html
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    
    <!-- Bootstrap CSS -->
    <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
    
    <!-- Radzen Theme (if using Radzen components) -->
    <RadzenTheme Theme="material" @rendermode="InteractiveServer" />
    
    <!-- Z.Blazor.Diagrams CSS (if using diagrams) -->
    <link rel="stylesheet" href="_content/Z.Blazor.Diagrams/style.min.css" />
    <link rel="stylesheet" href="_content/Z.Blazor.Diagrams/default.styles.min.css" />
    
    <!-- Your application CSS -->
    <link rel="stylesheet" href="app.css" />
    <link rel="stylesheet" href="YourApp.styles.css" />
    
    <HeadOutlet />
</head>

<body>
    <Routes />
    
    <!-- ═══════════════════════════════════════════════════════════════ -->
    <!-- SCRIPT LOADING ORDER IS CRITICAL - DO NOT CHANGE ORDER         -->
    <!-- ═══════════════════════════════════════════════════════════════ -->
    
    <!-- 1. Blazor Framework (MUST BE FIRST) -->
    <script src="_framework/blazor.web.js"></script>
    
    <!-- 2. Canvas2D - Required for Canvas2DComponent -->
    <script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
    
    <!-- 3. FoundryWorldsAndDrawings - Required for BOTH Canvas2D and Canvas3D -->
    <!-- This provides the unified animation system and Three.js integration -->
    <script src="_content/FoundryWorldsAndDrawings/js/app-lib.js"></script>
    
    <!-- 4. Radzen (if using Radzen components) -->
    <script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
    
    <!-- 5. Z.Blazor.Diagrams (if using diagram components) -->
    <script src="_content/Z.Blazor.Diagrams/script.min.js"></script>
    
</body>

</html>
```

### Script Purpose Reference

| Script | Required For | Purpose |
|--------|-------------|---------|
| `_framework/blazor.web.js` | **All Blazor** | Blazor framework - MUST be first |
| `Blazor.Extensions.Canvas/blazor.extensions.canvas.js` | **Canvas2DComponent** | HTML5 Canvas 2D rendering |
| `FoundryWorldsAndDrawings/js/app-lib.js` | **Canvas2D + Canvas3D** | Unified animation system, Three.js integration |
| `Radzen.Blazor/Radzen.Blazor.js` | Radzen components | RadzenSplitter, RadzenButton, etc. |
| `Z.Blazor.Diagrams/script.min.js` | Diagram components | Node/link diagram editor |

### ⚠️ Common Mistakes

**Wrong Package Name (NuGet vs Local)**:
```html
<!-- If using NuGet package: -->
<script src="_content/ApprenticeFoundryWorldsAndDrawings/js/app-lib.js"></script>

<!-- If using local project reference: -->
<script src="_content/FoundryWorldsAndDrawings/js/app-lib.js"></script>
```

**Missing Script = Silent Failure**:
- Missing `blazor.extensions.canvas.js` → Canvas2D shows blank
- Missing `app-lib.js` → Canvas3D shows blank, no animation
- Wrong script order → Intermittent failures

### Verifying Scripts Are Loading

Open browser DevTools (F12) → Network tab → Reload page:
1. Check each script returns 200 OK (not 404)
2. Check Console for JavaScript errors
3. If 404: verify the `_content/` path matches your package/project name

---

## Required Namespaces and Imports

### Essential Using Statements (Razor File)

```razor
@using FoundryWorldsAndDrawings.Shared          @* Canvas2DComponent, Canvas3DComponent *@
@using FoundryWorldsAndDrawings.Shape           @* FoShape2D, FoShape3D, FoStage3D, FoPage2D *@
@using FoundryWorldsAndDrawings.Solutions       @* IWorkspace, RenderContext3D *@
@using FoundryMentorModeler.Model               @* KnModel, KnComponent, KnParameter *@
@using FoundryMentorModeler.Shared              @* MentorTreeView *@
```

### Code-Behind Using Statements

```csharp
using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;

#nullable enable
```

---

## Canvas Components Specification

### Canvas3DComponent

The 3D canvas component wraps a Three.js viewer and provides a stage for 3D shapes.

#### Component Declaration

```razor
<Canvas3DComponent 
    SceneName="UniqueSceneName" 
    @ref="Canvas3DReference"
    CanvasWidth="@CanvasWidth"
    CanvasHeight="@CanvasHeight" />
```

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `SceneName` | `string` | **Yes** (EditorRequired) | - | Unique identifier for this 3D scene/stage |
| `CanvasWidth` | `int` | No | 250 | Width in pixels |
| `CanvasHeight` | `int` | No | 400 | Height in pixels |
| `CanvasStyle` | `string` | No | "width:max-content; border:1px solid black;cursor:default" | CSS style string |
| `IsGlobal` | `bool` | No | `false` | If true, all browser tabs share the same viewer |

#### Accessing the Stage

```csharp
// Get the stage managed by this canvas
FoStage3D? _stage = Canvas3DReference?.Stage;

// Get the active scene
var (found, scene) = Canvas3DReference.GetActiveScene();
```

#### Key Methods

```csharp
// Start/Stop global animation
await Canvas3DReference.DoStart();  // Start animation loop
await Canvas3DReference.DoStop();   // Stop animation loop
```

---

### Canvas2DComponent

The 2D canvas component uses Blazor.Extensions.Canvas for HTML5 Canvas drawing.

#### Component Declaration

```razor
<Canvas2DComponent 
    SceneName="UniquePageName" 
    @ref="Canvas2DReference"
    CanvasWidth="@CanvasWidth"
    CanvasHeight="@CanvasHeight" />
```

#### Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `SceneName` | `string` | **Yes** (EditorRequired) | Unique identifier for this 2D page |
| `CanvasWidth` | `int` | No | 1800 | Width in pixels |
| `CanvasHeight` | `int` | No | 1200 | Height in pixels |

#### Accessing the Page

```csharp
// Get the page managed by this canvas
FoPage2D? _page = Canvas2DReference?.Page;
```

#### Key Methods

```csharp
// Start/Stop global animation
await Canvas2DReference.DoStart();  // Start animation loop
await Canvas2DReference.DoStop();   // Stop animation loop
```

---

## Animation Control System

### AnimationFrameBus API

The `AnimationFrameBus` is a static class that provides global animation control.

#### Animation State Control

```csharp
// Pause all animations globally
AnimationFrameBus.PauseAllAnimations();

// Resume all animations globally
AnimationFrameBus.ResumeAllAnimations();

// Get current animation state
string state = AnimationFrameBus.GetAnimationState();  // "Running", "Paused", etc.

// Get current FPS
double fps = AnimationFrameBus.GetCurrentFps();

// Get current tick count
int tick = AnimationFrameBus.GetCurrentTick();
```

#### Subscribing to Animation Events

```csharp
// Subscribe to animation frame events
AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);

// Unsubscribe when disposing
AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);

// Animation event handler
private void OnAnimationEvent(AnimationEvent evt)
{
    // Check event type
    if (evt.IsWorld3D())  // 3D animation frame
    {
        // Render 3D geometry
    }
    
    if (evt.IsDrawing2D())  // 2D animation frame
    {
        // Render 2D shapes
    }
}
```

### Animation Control Button Pattern

```razor
<!-- Animation Control Buttons -->
<div style="display: flex; gap: 10px; align-items: center;">
    <button class="btn btn-success" @onclick="StartAnimation">▶️ Start</button>
    <button class="btn btn-warning" @onclick="PauseAnimation">⏸️ Pause</button>
    <button class="btn btn-info" @onclick="ResumeAnimation">▶️ Resume</button>
    <button class="btn btn-secondary" @onclick="ResetTest">🔄 Reset</button>
    
    <!-- Animation State Display -->
    <div style="font-family: monospace; font-weight: bold; padding: 5px 10px; background: #fff;">
        @AnimationFrameBus.GetAnimationState()
    </div>
    
    <!-- FPS/Tick Display -->
    <span>FPS: @AnimationFrameBus.GetCurrentFps().ToString("F1")</span>
    <span>Tick: @AnimationFrameBus.GetCurrentTick()</span>
</div>
```

### Code-Behind Animation Methods

```csharp
protected void StartAnimation()
{
    AnimationFrameBus.ResumeAllAnimations();
    AddLog("Animation", "Started animation loop");
}

protected void PauseAnimation()
{
    AnimationFrameBus.PauseAllAnimations();
    AddLog("Animation", "Paused animation loop");
}

protected void ResumeAnimation()
{
    AnimationFrameBus.ResumeAllAnimations();
    AddLog("Animation", "Resumed animation loop");
}

protected void ResetTest()
{
    AnimationFrameBus.PauseAllAnimations();
    // Reset your model/component state here
    StateHasChanged();
}
```

### Manual Frame Rendering (No Animation Loop)

For pages requiring manual control without continuous animation:

```csharp
protected override void OnInitialized()
{
    base.OnInitialized();
    
    // Pause animations on page load for manual control only
    AnimationFrameBus.PauseAllAnimations();
    $"⏸️ Animation paused - MANUAL CONTROL ONLY".WriteInfo();
}

private async Task TriggerSingleFrame()
{
    // Render a single frame manually
    if (_stage != null && _model != null)
    {
        var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
        _model.RenderGeometry3D(ctx);
    }
}
```

---

## Tree View Components

### MentorTreeView

Displays the KnModel hierarchy (models, components, parameters).

#### Basic Usage

```razor
<MentorTreeView />
```

#### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowModels` | `bool` | `true` | Show KnModel tree |
| `ShowDiagrams` | `bool` | `false` | Show diagram tree |
| `Show2DDrawing` | `bool` | `false` | Show 2D drawing tree |
| `Show3DGeometry` | `bool` | `false` | Show 3D geometry tree |

#### Example with Parameters

```razor
<MentorTreeView 
    ShowModels="true" 
    ShowDiagrams="true" 
    Show2DDrawing="false" 
    Show3DGeometry="false" />
```

### ShapeTreeView

Displays the shape hierarchy (worlds, drawings, arenas, stages).

#### Basic Usage

```razor
<ShapeTreeView />
```

The ShapeTreeView automatically displays:
- Worlds (if any)
- Drawing (2D pages)
- Arena (3D stages)

### Tabbed Tree View Pattern

```razor
<ul class="nav nav-tabs" role="tablist">
    <li class="nav-item" role="presentation">
        <button class="nav-link @(_activeTreeTab == "model" ? "active" : "")" 
                @onclick="@(() => _activeTreeTab = "model")" 
                type="button" role="tab">
            📊 Model Tree
        </button>
    </li>
    <li class="nav-item" role="presentation">
        <button class="nav-link @(_activeTreeTab == "shape" ? "active" : "")" 
                @onclick="@(() => _activeTreeTab = "shape")" 
                type="button" role="tab">
            🔷 Shape Tree
        </button>
    </li>
    <li class="nav-item" role="presentation">
        <button class="nav-link @(_activeTreeTab == "params" ? "active" : "")" 
                @onclick="@(() => _activeTreeTab = "params")" 
                type="button" role="tab">
            ⚙️ Parameters
        </button>
    </li>
</ul>

<div class="tab-content" style="flex: 1; overflow: auto; padding-top: 10px;">
    @if (_activeTreeTab == "model")
    {
        <MentorTreeView/>
    }
    else if (_activeTreeTab == "shape")
    {
        <ShapeTreeView/>
    }
    else
    {
        <!-- Custom parameter display -->
        <div style="padding: 10px; background-color: #e8f5e9; border-radius: 5px;">
            <strong>Live Parameters:</strong>
            @foreach (var param in _model.Members<KnParameter>())
            {
                <div style="font-family: monospace; margin-top: 5px;">
                    @param.Name: <span style="color: #1976d2; font-weight: bold;">@param.GetValue()</span>
                </div>
            }
        </div>
    }
</div>
```

---

## Button and Command Patterns

### Bootstrap Button Styles

```razor
<!-- Primary Actions -->
<button class="btn btn-primary" @onclick="DoAction">Primary Action</button>
<button class="btn btn-success" @onclick="CreateItem">✅ Create</button>
<button class="btn btn-danger" @onclick="DeleteItem">🗑️ Delete</button>

<!-- Secondary Actions -->
<button class="btn btn-warning" @onclick="PauseAction">⏸️ Pause</button>
<button class="btn btn-info" @onclick="InfoAction">ℹ️ Info</button>
<button class="btn btn-secondary" @onclick="ResetAction">🔄 Reset</button>

<!-- Outline Variants -->
<button class="btn btn-outline-primary" @onclick="OptionalAction">Optional</button>
<button class="btn btn-outline-secondary btn-sm" @onclick="SmallAction">Small</button>

<!-- Disabled State -->
<button class="btn btn-primary" @onclick="DoAction" disabled="@_isLoading">
    @if (_isLoading) { <span class="spinner-border spinner-border-sm me-1"></span> }
    Do Action
</button>
```

### Radzen Button Components

```razor
@using Radzen

<!-- Radzen Buttons with Click Actions -->
<RadzenButton Text="Primary" Click="@(args => OnClick("Primary"))" ButtonStyle="ButtonStyle.Primary" />
<RadzenButton Text="Success" Click="@(args => OnClick("Success"))" ButtonStyle="ButtonStyle.Success" />
<RadzenButton Text="Warning" Click="@(args => OnClick("Warning"))" ButtonStyle="ButtonStyle.Warning" />
<RadzenButton Text="Danger" Click="@(args => OnClick("Danger"))" ButtonStyle="ButtonStyle.Danger" />
<RadzenButton Text="Info" Click="@(args => OnClick("Info"))" ButtonStyle="ButtonStyle.Info" />
<RadzenButton Text="Secondary" Click="@(args => OnClick("Secondary"))" ButtonStyle="ButtonStyle.Secondary" />
```

### Button Group with Separators

```razor
<div style="display: flex; gap: 10px; align-items: center; flex-wrap: wrap;">
    <button class="btn btn-success" @onclick="StartAnimation">Start</button>
    <button class="btn btn-warning" @onclick="PauseAnimation">Pause</button>
    <button class="btn btn-info" @onclick="ResumeAnimation">Resume</button>
    
    <!-- Visual Separator -->
    <span style="border-left: 2px solid #999; height: 30px; margin: 0 5px;"></span>
    
    <button class="btn btn-primary" @onclick="AddComponent">➕ Add Component</button>
    <button class="btn btn-danger" @onclick="ClearAll">🗑️ Clear</button>
</div>
```

### Conditional Button States

```razor
<button class="btn @(_isAnimating ? "btn-danger" : "btn-success")" 
        @onclick="ToggleAnimation">
    @(_isAnimating ? "⏹️ Stop Animation" : "▶️ Start Animation")
</button>

<button class="btn btn-primary" 
        @onclick="AddItem" 
        disabled="@(_model == null || _isProcessing)">
    @if (_isProcessing) 
    { 
        <span class="spinner-border spinner-border-sm me-1"></span> 
    }
    Add Item
</button>
```

---

## Layout Patterns with RadzenSplitter

### Horizontal Split Layout

```razor
<RadzenSplitter Orientation="Orientation.Horizontal" style="height: 100%;">
    <!-- Left Pane: Canvas -->
    <RadzenSplitterPane Size="65%" Min="50%" Max="80%">
        <div style="height: 100%;">
            <Canvas3DComponent SceneName="MyScene" @ref="Canvas3DReference" />
        </div>
    </RadzenSplitterPane>

    <!-- Right Pane: Tree View -->
    <RadzenSplitterPane Size="35%" Min="20%" Max="50%">
        <div style="height: 100%; overflow: auto;">
            <MentorTreeView/>
        </div>
    </RadzenSplitterPane>
</RadzenSplitter>
```

### Three-Column Layout

```razor
<div style="display: flex; gap: 15px; height: calc(100vh - 160px);">
    <!-- Left Panel: Canvas -->
    <div style="flex: 1; min-width: 250px;">
        <Canvas3DComponent SceneName="Scene3D" @ref="Canvas3DReference" />
    </div>

    <!-- Center Panel: Model Tree -->
    <div style="flex: 1; min-width: 250px;">
        <MentorTreeView/>
    </div>

    <!-- Right Panel: Event Log -->
    <div style="flex: 0.5; min-width: 150px;">
        <div style="height: 100%; overflow: auto; background: #1e1e1e; color: #d4d4d4; padding: 10px;">
            <!-- Event log content -->
        </div>
    </div>
</div>
```

### Tabbed Canvas Display

Use CSS visibility to prevent component disposal on tab switch:

```razor
<ul class="nav nav-tabs" role="tablist">
    <li class="nav-item">
        <button class="nav-link @(_activeCanvasTab == "3d" ? "active" : "")" 
                @onclick="@(() => _activeCanvasTab = "3d")" type="button">
            🎮 3D Canvas
        </button>
    </li>
    <li class="nav-item">
        <button class="nav-link @(_activeCanvasTab == "2d" ? "active" : "")" 
                @onclick="@(() => _activeCanvasTab = "2d")" type="button">
            📐 2D Canvas
        </button>
    </li>
</ul>

<div style="position: relative; height: 400px;">
    <!-- Use CSS visibility instead of @if to prevent disposal -->
    <div style="width: 100%; height: 100%; position: absolute; @(_activeCanvasTab == "3d" ? "" : "display: none;")">
        <Canvas3DComponent SceneName="Tab3D" @ref="Canvas3DReference" />
    </div>
    <div style="width: 100%; height: 100%; position: absolute; @(_activeCanvasTab == "2d" ? "" : "display: none;")">
        <Canvas2DComponent SceneName="Tab2D" @ref="Canvas2DReference" />
    </div>
</div>
```

---

## Complete Page Template

### Razor File (YourPage.razor)

```razor
@page "/your-page"

@using FoundryWorldsAndDrawings.Shared
@using FoundryWorldsAndDrawings.Shape
@using FoundryWorldsAndDrawings.Solutions
@using FoundryMentorModeler.Model
@using FoundryMentorModeler.Shared

@rendermode InteractiveServer

@namespace YourNamespace.Components.Pages

<PageTitle>Your Page Title</PageTitle>

<style>
    .canvas-container {
        width: 100% !important;
        height: 100% !important;
    }
    .canvas-container canvas {
        width: 100% !important;
        height: 100% !important;
    }
</style>

<div class="container-fluid p-3" style="height: 100vh; display: flex; flex-direction: column;">
    
    <!-- Header -->
    <div class="card mb-3" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white;">
        <div class="card-body p-3">
            <h3 class="mb-2">🏗️ Your Page Title</h3>
            <p class="mb-0 small">Description of your page functionality</p>
        </div>
    </div>

    <!-- Control Panel -->
    <div class="card mb-3">
        <div class="card-body p-2">
            <div class="d-flex gap-2 flex-wrap align-items-center">
                <!-- Animation Controls -->
                <button class="btn btn-success" @onclick="StartAnimation">▶️ Start</button>
                <button class="btn btn-warning" @onclick="PauseAnimation">⏸️ Pause</button>
                <button class="btn btn-info" @onclick="ResumeAnimation">▶️ Resume</button>
                
                <span style="border-left: 2px solid #999; height: 30px; margin: 0 5px;"></span>
                
                <!-- Action Buttons -->
                <button class="btn btn-primary" @onclick="CreateModel" disabled="@_isLoading">
                    @if (_isLoading) { <span class="spinner-border spinner-border-sm me-1"></span> }
                    🧠 Create Model
                </button>
                <button class="btn btn-success" @onclick="AddComponent" disabled="@(_model == null)">
                    ➕ Add Component
                </button>
                <button class="btn btn-danger" @onclick="ClearModel" disabled="@(_model == null)">
                    🗑️ Clear
                </button>
                
                <span style="border-left: 2px solid #999; height: 30px; margin: 0 5px;"></span>
                
                <!-- Animation State Display -->
                <div style="font-family: monospace; font-weight: bold; padding: 5px 10px; background: #f8f9fa; border-radius: 4px;">
                    State: @AnimationFrameBus.GetAnimationState()
                </div>
                <span style="font-family: monospace;">FPS: @AnimationFrameBus.GetCurrentFps().ToString("F1")</span>
                <span style="font-family: monospace;">Tick: @AnimationFrameBus.GetCurrentTick()</span>
            </div>
        </div>
    </div>

    @if (_statusMessage != null)
    {
        <div class="alert @(_isError ? "alert-danger" : "alert-success") alert-dismissible fade show" role="alert">
            @_statusMessage
            <button type="button" class="btn-close" @onclick="() => _statusMessage = null"></button>
        </div>
    }

    <!-- Main Content Area -->
    <div style="flex: 1; overflow: hidden;">
        <RadzenSplitter Orientation="Orientation.Horizontal" style="height: 100%;">
            
            <!-- Left Side: Canvas (3D or 2D or both via tabs) -->
            <RadzenSplitterPane Size="65%" Min="50%" Max="80%">
                <div style="display: flex; flex-direction: column; height: 100%;">
                    <div class="card-header bg-dark text-white">
                        <h5 class="mb-0">🎨 3D View</h5>
                    </div>
                    <div class="canvas-container" style="flex: 1;">
                        <Canvas3DComponent 
                            SceneName="YourScene3D" 
                            @ref="Canvas3DReference" 
                            CanvasWidth="@CanvasWidth" 
                            CanvasHeight="@CanvasHeight" />
                    </div>
                </div>
            </RadzenSplitterPane>

            <!-- Right Side: Tree Views -->
            <RadzenSplitterPane Size="35%" Min="20%" Max="50%">
                <div style="display: flex; flex-direction: column; height: 100%;">
                    <div class="card-header" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white;">
                        <h5 class="mb-0">📊 Model & Shapes</h5>
                    </div>
                    
                    <!-- Tree Tab Selection -->
                    <ul class="nav nav-tabs border-bottom">
                        <li class="nav-item">
                            <button class="nav-link @(_activeTreeTab == "model" ? "active" : "")" 
                                    @onclick='() => _activeTreeTab = "model"'>
                                🧠 Model
                            </button>
                        </li>
                        <li class="nav-item">
                            <button class="nav-link @(_activeTreeTab == "shapes" ? "active" : "")" 
                                    @onclick='() => _activeTreeTab = "shapes"'>
                                📦 Shapes
                            </button>
                        </li>
                    </ul>
                    
                    <div style="flex: 1; overflow: auto; padding: 8px;">
                        @if (_activeTreeTab == "model")
                        {
                            <MentorTreeView/>
                        }
                        else
                        {
                            <ShapeTreeView/>
                        }
                    </div>
                </div>
            </RadzenSplitterPane>
            
        </RadzenSplitter>
    </div>

</div>
```

---

## Code-Behind Template

### YourPage.razor.cs

```csharp
using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryWorldsAndDrawings;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;

#nullable enable

namespace YourNamespace.Components.Pages;

public partial class YourPage : ComponentBase, IDisposable
{
    // ==========================================
    // DEPENDENCY INJECTION
    // ==========================================
    
    [Inject] public IWorkspace Workspace { get; init; } = null!;
    [Inject] public IMentorServices MentorServices { get; init; } = null!;
    [Inject] public IModelEditor ModelEditor { get; init; } = null!;

    // ==========================================
    // COMPONENT REFERENCES
    // ==========================================
    
    public Canvas3DComponent? Canvas3DReference = null;
    public Canvas2DComponent? Canvas2DReference = null;
    
    // ==========================================
    // UI STATE
    // ==========================================
    
    protected int CanvasWidth { get; set; } = 800;
    protected int CanvasHeight { get; set; } = 600;
    
    private string _activeTreeTab = "model";
    private string _activeCanvasTab = "3d";
    
    private bool _isLoading = false;
    private bool _isError = false;
    private string? _statusMessage = null;
    
    // ==========================================
    // MODEL STATE
    // ==========================================
    
    private AnimatedKnModel? _model;
    private KnComponent? _currentComponent;
    private FoStage3D? _stage;
    private FoPage2D? _page;

    // ==========================================
    // LIFECYCLE METHODS
    // ==========================================

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Subscribe to animation events
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        
        // Subscribe to model edit changes for tree refresh
        MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(OnModelEditChanged);
        
        // Create initial model
        _model = MentorServices.EstablishModel<AnimatedKnModel>("YourModelName");
        _model.SetExpanded(true);
        
        $"📊 Model '{_model.Name}' established".WriteInfo();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Wait for canvas initialization
            await Task.Delay(200);
            
            // Get stage/page references from canvas components
            _stage = Canvas3DReference?.Stage;
            _page = Canvas2DReference?.Page;
            
            if (_stage != null)
            {
                $"✅ Stage '{_stage.GetName()}' acquired".WriteSuccess();
                
                // Optional: Establish initial geometry
                if (_currentComponent != null)
                {
                    var view = _stage.GetName();
                    var (geometry, parameter) = ModelEditor.EstablishGeometry3D(_currentComponent, view);
                }
                
                // Start animation
                AnimationFrameBus.ResumeAllAnimations();
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    // ==========================================
    // ANIMATION HANDLERS
    // ==========================================

    private void OnAnimationEvent(AnimationEvent evt)
    {
        // Handle 3D animation frames
        if (evt.IsWorld3D() && _stage != null && _model != null)
        {
            var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
            _model.RenderGeometry3D(ctx);
        }
        
        // Handle 2D animation frames
        if (evt.IsDrawing2D() && _page != null && _model != null)
        {
            // 2D rendering logic here
        }
    }

    private void OnModelEditChanged(ModelEditChanged message)
    {
        $"📢 Received ModelEditChanged: {message.State}".WriteInfo();
        InvokeAsync(StateHasChanged);
    }

    // ==========================================
    // ANIMATION CONTROL METHODS
    // ==========================================

    protected void StartAnimation()
    {
        AnimationFrameBus.ResumeAllAnimations();
        _statusMessage = "Animation started";
        _isError = false;
    }

    protected void PauseAnimation()
    {
        AnimationFrameBus.PauseAllAnimations();
        _statusMessage = "Animation paused";
        _isError = false;
    }

    protected void ResumeAnimation()
    {
        AnimationFrameBus.ResumeAllAnimations();
        _statusMessage = "Animation resumed";
        _isError = false;
    }

    // ==========================================
    // MODEL MANIPULATION METHODS
    // ==========================================

    protected void CreateModel()
    {
        _isLoading = true;
        try
        {
            _model = MentorServices.EstablishModel<AnimatedKnModel>("NewModel");
            _model.SetExpanded(true);
            _statusMessage = $"Created model '{_model.Name}'";
            _isError = false;
        }
        catch (Exception ex)
        {
            _statusMessage = $"Error: {ex.Message}";
            _isError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    protected void AddComponent()
    {
        if (_model == null) return;
        
        var componentCount = _model.Members<KnComponent>().Count() + 1;
        var component = new YourComponent($"Component_{componentCount}");
        
        // Add to model via ModelEditor (fires events, triggers tree refresh)
        ModelEditor.AddChild(_model, component);
        _currentComponent = component;
        
        _statusMessage = $"Added component '{component.Name}'";
        _isError = false;
    }

    protected void ClearModel()
    {
        if (_model == null) return;
        
        // Clear all children
        var children = _model.Members<KnComponent>().ToList();
        foreach (var child in children)
        {
            ModelEditor.DestroyComponent(child);
        }
        
        _currentComponent = null;
        _statusMessage = "Model cleared";
        _isError = false;
    }

    // ==========================================
    // CLEANUP
    // ==========================================

    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
        MentorServices?.PubSub?.UnSubscribeFrom<ModelEditChanged>(OnModelEditChanged);
    }
}
```

---

## Common Patterns and Best Practices

### 1. Always Dispose Subscriptions

```csharp
public class YourPage : ComponentBase, IDisposable
{
    protected override void OnInitialized()
    {
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(OnModelEditChanged);
    }

    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
        MentorServices?.PubSub?.UnSubscribeFrom<ModelEditChanged>(OnModelEditChanged);
    }
}
```

### 2. Wait for Canvas Initialization

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // CRITICAL: Wait for canvas to fully initialize
        await Task.Delay(200);
        
        _stage = Canvas3DReference?.Stage;
        // Now safe to use _stage
    }
}
```

### 3. Use SceneName for Unique Identification

Every canvas MUST have a unique `SceneName`:

```razor
<Canvas3DComponent SceneName="HomePage3D" />  <!-- Unique per page -->
<Canvas3DComponent SceneName="DebugView3D" /> <!-- Different from above -->
```

### 4. Handle Null States Gracefully

```csharp
private void OnAnimationEvent(AnimationEvent evt)
{
    // Always check for null before using
    if (evt.IsWorld3D() && _stage != null && _model != null)
    {
        // Safe to render
    }
}
```

### 5. Use StateHasChanged with InvokeAsync

```csharp
private void OnModelEditChanged(ModelEditChanged message)
{
    // Use InvokeAsync when updating UI from event handlers
    InvokeAsync(StateHasChanged);
}
```

### 6. Prefer CSS Display over @if for Tabs

```razor
<!-- ✅ Correct: Use CSS visibility -->
<div style="@(_activeTab == "3d" ? "" : "display: none;")">
    <Canvas3DComponent SceneName="Tab3D" />
</div>

<!-- ❌ Wrong: @if causes disposal/recreation -->
@if (_activeTab == "3d")
{
    <Canvas3DComponent SceneName="Tab3D" />
}
```

### 7. Use ModelEditor for All Model Changes

```csharp
// ✅ Correct: Use ModelEditor (fires events)
ModelEditor.AddChild(_model, component);
ModelEditor.DestroyComponent(component);
ModelEditor.SetParameter(component, "Width", 5.0, "m");

// ❌ Wrong: Direct manipulation (no events)
_model.AddMember(component);
```

---

## KnModel/KnComponent Architecture

### Creating a KnModel

```csharp
// Establish a model (creates or retrieves existing)
_model = MentorServices.EstablishModel<AnimatedKnModel>("MyModelName");
_model.SetExpanded(true);  // Show expanded in tree view
```

### Creating KnComponents

```csharp
// Create component with parameters
var component = new MyComponent("ComponentName", width: 2.0, height: 3.0);

// Add to model via ModelEditor (fires events, triggers tree refresh)
ModelEditor.AddChild(_model, component);
```

### KnComponent with Parameters Pattern

```csharp
public class MyComponent : PartComponent
{
    public MyComponent(string name, double width = 1.0, double height = 1.0) : base(name)
    {
        // Define parameters using Calculations helper
        Calculations([
            $"Width|m: {width}",
            $"Height|m: {height}",
            "Color: 'Blue'"
        ]);
        
        // Optional: Set up animation callback
        PreAnimationRefresh((comp, evt) =>
        {
            // Update parameters based on animation tick
            // Called before each animation frame
        });
    }

    // Establish 3D geometry for a view
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeMesh3D", ComputeMesh3D, null);
            geom.ApplyTransformMethod("ComputeTransform3D", ComputeTransform3D, null);
            geom.ApplyBodyMethod("ComputeBody3D", ComputeBody3D, null);
        });
        
        return (result, result.GetBodyParameter());
    }
}
```

### Parameter Access Patterns

```csharp
// Get number parameter value (triggers dependency discovery)
var (found, width) = FindNumberValue("Width");
if (found)
{
    double value = width.Value;  // The numeric value
    string units = width.Units;  // The units (e.g., "m")
}

// Get string parameter value
var (foundColor, color) = FindStringValue("Color");
if (foundColor)
{
    string colorName = color.Value;
}

// Set parameter value (triggers Smash cascade)
ModelEditor.SetParameter(component, "Width", 5.0, "m");
```

### The Geometry Pipeline

```
KnComponent.EstablishGeometry3D(view)
  → Compute3DGeometry(view, builder)
    → ComputeMesh3D: Create/cache mesh geometry
    → ComputeTransform3D: Compute position/rotation/scale
    → ComputeBody3D: Compose mesh + transform into shape
  → Returns (KnGeometry, KnParameter)
    → KnParameter holds cached FoShape3D
```

### Rendering the Model

```csharp
private void OnAnimationEvent(AnimationEvent evt)
{
    if (evt.IsWorld3D() && _stage != null && _model != null)
    {
        // Create render context from stage
        var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
        
        // Walk model tree and render all geometry
        _model.RenderGeometry3D(ctx);
        
        // Note: Canvas3DComponent handles actual rendering to Three.js
    }
}
```

---

## Common Mistakes to Avoid

### ❌ Not Subscribing to AnimationEvent

```csharp
protected override void OnInitialized()
{
    _model = MentorServices.EstablishModel<AnimatedKnModel>("Model");
    // Missing: AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
}
```
**Result**: Parameter changes trigger Smash but nothing re-renders.

### ❌ Using @if for Tab Content (Causes Disposal)

```razor
@* WRONG - component gets disposed on tab switch *@
@if (_activeTab == "3d")
{
    <Canvas3DComponent SceneName="Tab3D" />
}
```
**Fix**: Use CSS `display: none;` instead.

### ❌ Manually Calling RenderStage

```csharp
private void OnAnimationEvent(AnimationEvent evt)
{
    var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
    _model.RenderGeometry3D(ctx);
    await _stage.RenderStage(tick, fps);  // ← DON'T DO THIS
}
```
**Result**: Canvas3DComponent already does this - you'll render twice per frame.

### ❌ Forgetting to Dispose Subscriptions

```csharp
public void Dispose()
{
    // Missing unsubscribe calls
}
```
**Result**: Memory leaks, handlers persist after page navigation.

### ❌ Not Waiting for Canvas Initialization

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _stage = Canvas3DReference?.Stage;  // May be null!
        // Use stage immediately...
    }
}
```
**Fix**: Add `await Task.Delay(200);` before accessing stage.

### ❌ Duplicate SceneNames

```razor
<Canvas3DComponent SceneName="MyScene" />
<Canvas3DComponent SceneName="MyScene" />  @* WRONG - same name! *@
```
**Result**: Stage/Scene conflicts. Each canvas needs a unique SceneName.

---

## MxObject Architecture (FoundryMicroCore)

The tree views and shape hierarchy are built on the `MxObject` base class from `FoundryMicroCore.Core`.

### Key Base Classes

```csharp
// From FoundryMicroCore.Core namespace
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
```

| Class | Purpose |
|-------|---------|
| `MxObject` | Base class for all managed objects with collection support |
| `MxComponent` | Component with typed collections (`GetCollection<T>()`) |
| `MxFolder` | Tree folder node for organizing items in tree views |
| `ITreeNode` | Interface for tree view display |

### Using MxFolder for Tree Organization

```csharp
// Create a folder to group items in tree view
var folder = new MxFolder("Equipment");
folder.AddChild(equipment1);
folder.AddChild(equipment2);

// Add folder to tree
treeNodes.Add(folder);
```

### MxComponent Collection Pattern

```csharp
public class MyComponent : MxComponent
{
    // Get typed collection of children
    public ICollection<ChildType> Children => GetCollection<ChildType>();
    
    // Add tree nodes for display
    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = new List<ITreeNode>();
        
        // Use extension method to add folder for typed items
        this.AddTreeNodeFor<ChildType>(list);
        
        return list;
    }
}
```

### Extension Method for Tree Organization

```csharp
// From FoundryWorldsAndDrawings.Extensions
public static void AddTreeNodeFor<T>(this MxComponent component, List<ITreeNode> list) 
    where T : MxObject, ITreeNode
{
    var collection = component.GetCollection<T>();
    if (collection.Count > 0)
    {
        var folder = new MxFolder($"{typeof(T).Name} ({collection.Count})");
        foreach (var item in collection)
            folder.AddChild(item);
        list.Add(folder);
    }
}
```

---

## Radzen Components Reference

### RadzenSplitter

Resizable split panels.

```razor
<RadzenSplitter Orientation="Orientation.Horizontal" style="height: 100%;">
    <RadzenSplitterPane Size="60%" Min="40%" Max="80%">
        <!-- Left content -->
    </RadzenSplitterPane>
    <RadzenSplitterPane Size="40%" Min="20%" Max="60%">
        <!-- Right content -->
    </RadzenSplitterPane>
</RadzenSplitter>
```

### RadzenStack

Flexbox-based layout.

```razor
<RadzenStack Orientation="Orientation.Vertical" JustifyContent="JustifyContent.Start" Gap="1rem">
    <button class="btn btn-primary">Button 1</button>
    <button class="btn btn-secondary">Button 2</button>
</RadzenStack>

<RadzenStack Orientation="Orientation.Horizontal" AlignItems="AlignItems.Center" Gap="0.5rem">
    <span>Label:</span>
    <input type="text" />
</RadzenStack>
```

### RadzenButton

Styled button component.

```razor
<RadzenButton 
    Text="Click Me" 
    Click="@OnButtonClick" 
    ButtonStyle="ButtonStyle.Primary"
    Disabled="@isDisabled" />
```

### RadzenCard

Card container.

```razor
<RadzenCard Class="mb-3">
    <h5>Card Title</h5>
    <p>Card content here</p>
</RadzenCard>
```

---

## Service Injection Reference

### Required Services for Canvas Pages

```csharp
// Core services - always needed
[Inject] public IWorkspace Workspace { get; init; } = null!;

// Model management - for KnModel/KnComponent
[Inject] public IMentorServices MentorServices { get; init; } = null!;
[Inject] public IModelEditor ModelEditor { get; init; } = null!;

// Optional - for pub/sub messaging
[Inject] public ComponentBus? PubSub { get; set; }

// Optional - for foundry services
[Inject] public IFoundryService? FoundryService { get; set; }
```

### Service Registration in Program.cs

```csharp
// Add Foundry services (includes IWorkspace, IFoundryService, etc.)
builder.Services.AddFoundryWorldsAndDrawingsServices(envConfig);

// Add Mentor services (includes IMentorServices, IModelEditor, etc.)
builder.Services.AddFoundryMentorModelerServices();

// Add Radzen components
builder.Services.AddRadzenComponents();
```

---

## Script Loading (App.razor)

```html
<head>
    <!-- Radzen Theme -->
    <RadzenTheme Theme="material" @rendermode="InteractiveServer" />
    
    <!-- Z.Blazor.Diagrams (if using diagrams) -->
    <link rel="stylesheet" href="_content/Z.Blazor.Diagrams/style.min.css" />
    <link rel="stylesheet" href="_content/Z.Blazor.Diagrams/default.styles.min.css" />
</head>

<body>
    <Routes />
    
    <!-- Required Scripts -->
    <script src="_framework/blazor.web.js"></script>
    <script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
    
    <!-- FoundryWorldsAndDrawings unified animation -->
    <script src="_content/FoundryWorldsAndDrawings/js/app-lib.js"></script>
    
    <!-- Radzen (if using Radzen components) -->
    <script src="_content/Radzen.Blazor/Radzen.Blazor.js"></script>
    
    <!-- Z.Blazor.Diagrams (if using diagrams) -->
    <script src="_content/Z.Blazor.Diagrams/script.min.js"></script>
</body>
```

---

## Additional Resources

- **UNIFIED_ANIMATION_ARCHITECTURE.md**: Complete animation system documentation
- **CANVAS_SETUP_PATTERN.md**: Canvas initialization patterns
- **ANIMATED_GEOMETRY_PAGE_TEMPLATE.md**: Full geometry animation template
- **KNCOMPONENT_FRAMEWORK_PATTERN.md**: KnComponent implementation guide
- **KN_FO_GEOMETRY_INTEGRATION_GUIDE.md**: KnComponent to FoShape geometry pipeline

---

**Document Version**: 1.0  
**Last Updated**: January 22, 2026  
**Author**: GitHub Copilot  
**Purpose**: LLM instruction guide for building Razor pages with Canvas and Tree components
