# Blazor Diagram Implementation Reference Guide
## eDesignStudio ScenarioChat Pattern Analysis

**Date:** January 13, 2026  
**Source Project:** eDesignStudio  
**Target Framework:** Blazor InteractiveServer with .NET 9  
**Diagram Package:** Z.Blazor.Diagrams

---

## Table of Contents

1. [JavaScript/CSS Loading Configuration](#1-javascriptcss-loading-configuration)
2. [Widget Registration Pattern](#2-widget-registration-pattern)
3. [Model Rendering Flow](#3-model-rendering-flow)
4. [CascadingValue Configuration](#4-cascadingvalue-configuration)
5. [Widget Component Structure](#5-widget-component-structure)
6. [Common Gotchas and Solutions](#6-common-gotchas-and-solutions)
7. [Error Messages and Fixes](#7-error-messages-and-fixes)
8. [Complete Working Example](#8-complete-working-example)
9. [Key Takeaways](#9-key-takeaways)

---

## 1. JavaScript/CSS Loading Configuration

### Location
`Components/App.razor`

### Complete Head Section
```html
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
    <RadzenTheme Theme="material" @rendermode="InteractiveServer" />
    <link href="_content/BlazorDatasheet/sheet-styles.css" rel="stylesheet"/>
    
    <!-- Z.Blazor.Diagrams CSS -->
    <link href="_content/Z.Blazor.Diagrams/style.min.css" rel="stylesheet" />
    <link href="_content/Z.Blazor.Diagrams/default.styles.min.css" rel="stylesheet" />
    
    <link rel="stylesheet" href="app.css" />
    <link rel="stylesheet" href="eDesignStudio.styles.css" />
    <link rel="icon" type="image/png" href="favicon.png" />
    <HeadOutlet />
</head>
```

### Complete Body Script Section
```html
<body>
    <Routes />
    
    <!-- CRITICAL: Script loading order -->
    <script src="_framework/blazor.web.js"></script>
    <script src="_content/Blazor.Extensions.Canvas/blazor.extensions.canvas.js"></script>
    <script src="_content/BlazorDatasheet/blazor-datasheet.js" type="text/javascript"></script>
    <script src="_content/ApprenticeFoundryBlazorThreeJS/dist/app-lib.js"></script>
    <script src="_content/ApprenticeFoundryBlazor/js/IntegrationHelper.js"></script>
    <script src="_content/ApprenticeFoundryBlazor/js/app-lib.js"></script>
    <script src="_content/Radzen.Blazor/Radzen.Blazor.js?v=@(typeof(Radzen.Colors).Assembly.GetName().Version)"></script>
    
    <!-- Z.Blazor.Diagrams JavaScript - MUST BE AFTER blazor.web.js -->
    <script src="_content/Z.Blazor.Diagrams/script.min.js"></script>
</body>
```

### Key Points
- **Package Name:** `Z.Blazor.Diagrams` (NOT `Blazor.Diagrams`)
- **CSS Location:** In `<head>` section
- **JavaScript Location:** At end of `<body>`, AFTER `blazor.web.js`
- **Loading Order:** Blazor framework → Other libraries → Radzen → Diagrams (LAST)

---

## 2. Widget Registration Pattern

### Location
`Components/Pages/ScenarioChat.razor.cs`

### Complete OnInitialized Method
```csharp
protected override void OnInitialized()
{
    var url = Navigation?.BaseUri ?? "";
    Workspace.SetBaseUrl(url);

    // Create workbook for scenario management
    Workbook = Workspace.EstablishWorkbook<MentorWorkbook>("scenarios");
    Workbook.SetMentorService(MentorServices!, MentorStudio!);
    Workspace.SetCurrentWorkbook(Workbook);

    // ═══════════════════════════════════════════════════════════
    // DIAGRAM ESTABLISHMENT AND WIDGET REGISTRATION
    // ═══════════════════════════════════════════════════════════
    
    // Step 1: Establish the diagram instance
    ScenarioDiagram = MentorServices!.EstablishDiagram<MentorDiagram>("ScenarioCanvas");
    
    // Step 2: Register widget - SINGLE LINE!
    // Pattern: diagram.Register<TEditorClass, TWidgetComponent>(overwriteExisting)
    ScenarioDiagram.Register<ScenarioBlockEditor, ScenarioBlockWidget>(true);
    
    // ═══════════════════════════════════════════════════════════

    Workspace.CreateCommands(Workspace, JsRuntime, Navigation, url);

    // Add welcome message
    ChatMessages.Add(new ChatMessage
    {
        Content = "🤖 Welcome to Scenario Navigator",
        IsUser = false,
        Timestamp = DateTime.Now
    });

    base.OnInitialized();
}
```

### Registration Pattern Breakdown
```csharp
// Generic pattern:
diagram.Register<TEditor, TWidget>(overwriteExisting);

// Concrete example:
ScenarioDiagram.Register<ScenarioBlockEditor, ScenarioBlockWidget>(true);

// Where:
// - TEditor: Your custom node editor class (inherits from DiagramNode)
// - TWidget: Your Razor component (the visual widget)
// - overwriteExisting: true = replace any existing registration
```

### Critical Timing
- **Registration happens in:** `OnInitialized()`
- **NOT in:** `OnAfterRenderAsync()` (too late)
- **Reason:** Widget types must be registered before any nodes are created

---

## 3. Model Rendering Flow

### Overview
The rendering follows a hierarchical pattern:
```
Model → Solution → Block → Children (recursive)
```

### A. ScenarioModel_710.RenderDiagram()

**Location:** `Models/ScenarioModel_710.cs`

```csharp
public override MentorDiagram RenderDiagram(string view, bool clear, Action OnComplete)
{
    // Step 1: Get or clear the diagram
    var (found, diagram) = ClearDiagram(clear);
    if (!found) 
        return diagram;
    
    // Step 2: Get the solution (creates if doesn't exist)
    var solution = EstablishSolution();
    
    // Step 3: Create render context and delegate to solution
    solution.RenderEditor(RenderContextEditor.Create(diagram, view, true));
    
    // Step 4: Execute completion callback
    OnComplete?.Invoke();
    
    return diagram;
}

public (bool found, MentorDiagram diagram) ClearDiagram(bool clear = true)
{
    var (found, diagram) = _services.CurrentDiagram();
    if (clear) 
        diagram.ClearAll();

    return (found, diagram);
}
```

### B. ScenarioSolution_710.RenderEditor()

**Location:** `Models/ScenarioSolution_710.cs`

```csharp
// Solution delegates to its current root block
public void RenderEditor(RenderContextEditor ctx)
{
    // The solution maintains a CurrentSystemBlock
    CurrentSystemBlock.RenderEditor(ctx);
    
    // Note: The solution also handles layout management
}

// Example from actual implementation:
public void ChangeRootScenarioBlock(ScenarioBlock_710 block)
{
    var (found, diagram) = ClearDiagram(true);

    CurrentSystemBlock = block;
    CurrentSystemBlock.RenderEditor(RenderContextEditor.Create(diagram, "System", false));
    LayoutDiagramTreeFromRoot(CurrentSystemBlock, LayoutType_710.None, true);
}
```

### C. ScenarioBlock_710.EstablishEditor()

**Location:** `Models/ScenarioBlock_710.cs`

```csharp
public override KnEditor2DParameter EstablishEditor(string view, MentorDiagram diagram)
{
    // Step 1: Try to get existing editor parameter
    var result = base.EstablishEditor(view, diagram);
    if (result.IsValid())
        return result;

    // Step 2: Create the diagram node (this is the visual representation)
    var node = EstablishDiagramNode<ScenarioBlockEditor>(result, diagram);
    
    // Step 3: Configure the node
    node.Visible = this.IsVisible;
    
    // Step 4: Wire up events
    node.Moved += (item) =>
    {
        var comp = node.GetScenarioBlock()!;
        var solution = comp.GetParentOfType<ScenarioSolution_710>()!;
        solution.DidRootItemMove(comp);
    };

    return result;
}
```

### Context Flow Diagram
```
┌─────────────────────────────────────────────────────────────┐
│ Model.RenderDiagram(view, clear, onComplete)               │
│                                                             │
│  1. ClearDiagram(clear) → gets/clears diagram              │
│  2. EstablishSolution() → gets/creates solution            │
│  3. Creates RenderContextEditor:                           │
│     - diagram reference                                     │
│     - view name ("System", "Schematic", etc.)              │
│     - deep flag (render children?)                         │
│  4. Calls solution.RenderEditor(context)                   │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ Solution.RenderEditor(context)                              │
│                                                             │
│  1. Gets CurrentSystemBlock (root block for this view)     │
│  2. Calls block.RenderEditor(context)                      │
│  3. May handle layout management                           │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
┌─────────────────────────────────────────────────────────────┐
│ Block.EstablishEditor(view, diagram)                        │
│                                                             │
│  1. Check if editor already exists → return if valid       │
│  2. EstablishDiagramNode<TEditor>(result, diagram)         │
│     → Creates node on diagram                              │
│     → Widget automatically matched by registration         │
│  3. Configure node (visibility, events, etc.)              │
│  4. Return editor parameter                                │
└─────────────────┬───────────────────────────────────────────┘
                  │
                  ▼
        ┌─────────────────┐
        │ If has children │
        └────────┬─────────┘
                 │
                 ▼
        Recursive call to child blocks
        (same EstablishEditor pattern)
```

### Key Observations
1. **Context object** carries diagram reference and settings through the chain
2. **EstablishDiagramNode<T>** is the magic method that creates nodes
3. **Widget matching** happens automatically based on registration
4. **Recursive rendering** for hierarchical structures
5. **Lazy creation** - editor created only if doesn't exist

---

## 4. CascadingValue Configuration

### Location
`Components/Pages/ScenarioChat.razor`

### Complete DiagramCanvas Setup
```razor
<div class="scenario-canvas">
    @if (IsLoading)
    {
        <div class="canvas-loading-overlay">
            <div class="loading-content">
                <RadzenProgressBarCircular ShowValue="false" Size="ProgressBarCircularSize.Large" />
                <h4 class="loading-title">@LoadingMessage</h4>
                <p class="loading-subtitle">Please wait while we prepare your scenario workspace...</p>
            </div>
        </div>
    }
    else
    {
        <!-- ═══════════════════════════════════════════════════════════ -->
        <!-- CASCADING VALUE AND DIAGRAM CANVAS                          -->
        <!-- ═══════════════════════════════════════════════════════════ -->
        
        <CascadingValue Value="ScenarioDiagram" IsFixed="true">
            <DiagramCanvas>
                <Widgets>
                    <SelectionBoxWidget />
                    <GridWidget Size="50" Mode="GridMode.Line" BackgroundColor="white" />
                    <NavigatorWidget Width="200" Height="120" 
                        Class="border border-black bg-white absolute"
                        Style="bottom: 15px; right: 15px;" />
                </Widgets>
            </DiagramCanvas>
        </CascadingValue>
        
        <!-- ═══════════════════════════════════════════════════════════ -->
    }
</div>
```

### Configuration Breakdown

#### CascadingValue
```razor
<CascadingValue Value="ScenarioDiagram" IsFixed="true">
```

**Properties:**
- `Value="ScenarioDiagram"` - The `MentorDiagram` instance from code-behind
- `IsFixed="true"` - **IMPORTANT:** Optimization flag (value won't change)

**Type Information:**
- Value is `MentorDiagram` (inherits from `BlazorDiagram`)
- **NO casting required** - pass directly
- DO NOT cast to `BlazorDiagram` - the framework handles it

#### Built-in Widgets
```razor
<Widgets>
    <SelectionBoxWidget />
    <GridWidget Size="50" Mode="GridMode.Line" BackgroundColor="white" />
    <NavigatorWidget Width="200" Height="120" 
        Class="border border-black bg-white absolute"
        Style="bottom: 15px; right: 15px;" />
</Widgets>
```

**Available Built-in Widgets:**
- `SelectionBoxWidget` - Allows multi-select with drag
- `GridWidget` - Background grid display
- `NavigatorWidget` - Mini-map for large diagrams
- `DeleteWidget` - Delete selected nodes (not shown in this example)

### Property Declarations (Code-Behind)
```csharp
public partial class ScenarioChatBase : ComponentBase
{
    [Inject] private IMentorServices MentorServices { get; set; } = default!;
    
    // The diagram property that gets passed to CascadingValue
    protected MentorDiagram ScenarioDiagram { get; set; } = default!;
    
    protected bool IsLoading = false;
    protected string LoadingMessage = "Loading scenarios...";
}
```

---

## 5. Widget Component Structure

### Complete Widget Example: ScenarioBlockWidget.razor

**Location:** `Models/ScenarioBlockWidget.razor`

```razor
@using Blazor.Diagrams.Components.Renderers
@using Microsoft.AspNetCore.Components.Web
@using Blazor.Diagrams.Core.Geometry
@using Blazor.Diagrams.Components
@using Radzen
@using Radzen.Blazor
@using Radzen.Blazor.Rendering

@namespace eDesign.DiagramComponents

<!-- CRITICAL: Disable prerendering for proper JS interop -->
@rendermode @(new InteractiveServerRenderMode(false))

<!-- ═══════════════════════════════════════════════════════════ -->
<!-- STYLES - Scoped to this component                           -->
<!-- ═══════════════════════════════════════════════════════════ -->
<style>
    .scenarioblock-node {
        border: 2px solid black;
        text-align: center;
    }

    .scenarioblock-node.selected {
        border-color: blue;
    }

    .scenarioblock-node .diagram-port {
        --port-size: 10px;
        --port-offset: calc(-0.5 * var(--port-size));
        margin: 0px;
        width: var(--port-size);
        height: var(--port-size);
        border-radius: 50%;
        background-color: #dcdcdc;
        border: 1px solid black;
    }
</style>

<!-- ═══════════════════════════════════════════════════════════ -->
<!-- MAIN WIDGET CONTENT                                         -->
<!-- ═══════════════════════════════════════════════════════════ -->
<div class="scenarioblock-node @(Node.IsSelected ? "selected" : "")">
    <RadzenStack Style="@OuterStyle()" Gap="0" Orientation="Orientation.Vertical">
        
        <!-- Display name -->
        <RadzenStack Orientation="Orientation.Horizontal" 
            JustifyContent="JustifyContent.Center"
            Style="border: 1px solid black;font-size: 8pt;">
            <div>@GetDisplayName()</div>
        </RadzenStack>

        <!-- Description -->
        <RadzenStack Orientation="Orientation.Horizontal" 
            JustifyContent="JustifyContent.Center"
            Style="border: 1px solid black;font-size: 8pt;">
            <div>@GetDescription()</div>
        </RadzenStack>

        <!-- Tags -->
        <RadzenStack Orientation="Orientation.Horizontal" 
            JustifyContent="JustifyContent.Center"
            Style="border: 1px solid black;font-size: 8pt;">
            <div>@GetTags()</div>
        </RadzenStack>
    </RadzenStack>

    <!-- Ports (connection points) -->
    @foreach (var port in PortList())
    {
        <PortRenderer Port="port" Class="@port.Color" />
    }
</div>

<!-- ═══════════════════════════════════════════════════════════ -->
<!-- CODE SECTION                                                 -->
<!-- ═══════════════════════════════════════════════════════════ -->
@code {
    #nullable enable
    
    // ═══════════════════════════════════════════════════════════
    // CRITICAL: Parameter must be the EDITOR type, not model
    // ═══════════════════════════════════════════════════════════
    [Parameter] public ScenarioBlockEditor Node { get; set; }

    // Get the underlying model from the editor
    public ScenarioBlock_710 GetScenarioBlock()
    {
        return Node.GetScenarioBlock();
    }

    // Get list of ports for rendering
    public List<DiagramPort> PortList()
    {
        var list = new List<DiagramPort>();
        list.AddRange(Node.Ports.Cast<DiagramPort>());
        return list;
    }

    public string GetDisplayName() => GetScenarioBlock()?.DisplayName ?? "";
    public string GetDescription() => GetScenarioBlock()?.Description ?? "";
    public string GetTags() => string.Join(", ", GetScenarioBlock()?.GetTags() ?? new List<string>());

    public string OuterStyle()
    {
        var width = 200;
        var style = $"width:{width}px;border: 1px solid black;background-color: lightgray";
        return style;
    }
}
```

### Editor Class Pattern

**ScenarioBlockEditor.cs** - The node editor that bridges model and widget:

```csharp
using Blazor.Diagrams.Core.Geometry;
using FoundryMentorModeler.Diagram;
using System.Timers;

namespace eDesignStudio.Model;

public class ScenarioBlockEditor : DiagramNode
{
    // Constructor receives the model component and position
    public ScenarioBlockEditor(KnComponent source, Point position) 
        : base(source, position)
    {
        Title = source.Name;
    }

    // Bridge method to access the underlying model
    public ScenarioBlock_710 GetScenarioBlock()
    {
        if (GetComponent() is ScenarioBlock_710 block)
        {
            return block;
        }
        return null;
    }

    // Selection state (used by widget)
    public bool IsSelected => GetIsSelected();
}
```

### Key Widget Pattern Elements

1. **Parameter Type:** Always the Editor class, never the model
   ```csharp
   [Parameter] public ScenarioBlockEditor Node { get; set; }
   ```

2. **Selection State:** Use `Node.IsSelected` property
   ```razor
   <div class="@(Node.IsSelected ? "selected" : "")">
   ```

3. **Nullable Directive:** Enable in code section
   ```csharp
   @code {
       #nullable enable
       // ...
   }
   ```

4. **Render Mode:** Disable prerendering
   ```razor
   @rendermode @(new InteractiveServerRenderMode(false))
   ```

5. **Bridge Pattern:** Editor provides access to model
   ```csharp
   public ScenarioBlock_710 GetScenarioBlock()
   {
       return Node.GetScenarioBlock();
   }
   ```

---

## 6. Common Gotchas and Solutions

### 1. Initialization Order Issues

**Problem:** Diagram operations fail with "not initialized" errors

**Solution:** Follow strict initialization order:

```csharp
// ✅ CORRECT ORDER
protected override void OnInitialized()
{
    // 1. Register diagram and widgets (synchronous)
    ScenarioDiagram = MentorServices.EstablishDiagram<MentorDiagram>("ScenarioCanvas");
    ScenarioDiagram.Register<ScenarioBlockEditor, ScenarioBlockWidget>(true);
    
    base.OnInitialized();
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // 2. Load data (async operations OK here)
        await LoadScenarioData();
        
        // 3. Create model structure
        CurrentModel710 = await EstablishScenarioTree();
        
        // 4. Render to diagram (JS interop ready now)
        CurrentModel710.RenderDiagram(CurrentModel710.Name, true, () => { });
    }
    await base.OnAfterRenderAsync(firstRender);
}
```

### 2. Widget Discovery and Registration

**Problem:** Widgets not rendering, using default node rendering

**Checklist:**
1. ✅ Namespace matches between widget and registration
2. ✅ Widget has `[Parameter] public TEditor Node { get; set; }`
3. ✅ Registration uses correct generic types
4. ✅ Registration called in `OnInitialized()`
5. ✅ Widget has correct `@namespace` directive

```razor
<!-- Widget must declare namespace -->
@namespace eDesign.DiagramComponents

@code {
    // Parameter type must match registration
    [Parameter] public ScenarioBlockEditor Node { get; set; }
}
```

```csharp
// Registration must match widget namespace and types
ScenarioDiagram.Register<ScenarioBlockEditor, ScenarioBlockWidget>(true);
//                        ↑ Editor class    ↑ Widget component in eDesign.DiagramComponents
```

---

## 7. Error Messages and Fixes

### Error A: No Interop Methods Registered

**Error Message:**
```
Uncaught Error: No interop methods are registered for renderer
```

**Root Cause:** Prerendering is enabled, but diagram requires client-side JS

**Fix:**
```razor
<!-- Add this to your widget -->
@rendermode @(new InteractiveServerRenderMode(false))
<!--                                            ↑ false = disable prerendering -->
```

### Error B: JavaScript Undefined

**Error Message:**
```
Error: Microsoft.JSInterop.JSException: Could not find 'BlazorDiagrams.getBoundingClientRect'
```

**Root Causes:**
1. Wrong package (using `Blazor.Diagrams` instead of `Z.Blazor.Diagrams`)
2. Script not loaded
3. Script loaded before Blazor framework

**Fix:**

```html
<!-- ✅ CORRECT: Use Z.Blazor.Diagrams -->
<link href="_content/Z.Blazor.Diagrams/style.min.css" rel="stylesheet" />
<link href="_content/Z.Blazor.Diagrams/default.styles.min.css" rel="stylesheet" />
<script src="_content/Z.Blazor.Diagrams/script.min.js"></script>

<!-- ❌ WRONG: Don't use -->
<link href="_content/Blazor.Diagrams/style.min.css" rel="stylesheet" />
<script src="_content/Blazor.Diagrams/script.min.js"></script>
```

---

## 8. Complete Working Example

[Complete example sections from the source document would go here...]

---

## 9. Key Takeaways

### Critical Success Factors

1. **✅ Use Z.Blazor.Diagrams Package**
   - NOT the original `Blazor.Diagrams`
   - The "Z." prefix indicates the maintained fork
   - Script path: `_content/Z.Blazor.Diagrams/script.min.js`

2. **✅ Registration in OnInitialized()**
   - Single line: `diagram.Register<TEditor, TWidget>(true)`
   - Must happen BEFORE any nodes are created
   - Do NOT register in `OnAfterRenderAsync()`

3. **✅ Rendering in OnAfterRenderAsync(firstRender)**
   - Wait for `firstRender == true`
   - Ensures JS interop is ready
   - Load data → Build model → Render diagram

4. **✅ CascadingValue Configuration**
   - Pass `MentorDiagram` directly (no casting)
   - Set `IsFixed="true"` for performance
   - Contains `<DiagramCanvas>` with optional `<Widgets>`

5. **✅ Widget Parameter Type**
   - `[Parameter] public TEditor Node { get; set; }`
   - Parameter is the **Editor** class, not the model
   - Editor provides bridge to model via methods like `GetScenarioBlock()`

6. **✅ Disable Prerendering**
   - `@rendermode @(new InteractiveServerRenderMode(false))`
   - Prevents JS interop errors
   - Required for diagram widgets

7. **✅ Script Loading Order**
   - Load `blazor.web.js` first
   - Load diagram script near end
   - Must be in `<body>` section

### Common Pitfalls to Avoid

❌ **Don't** render diagrams in `OnInitialized()`  
❌ **Don't** use parameter type as model class  
❌ **Don't** forget to disable prerendering  
❌ **Don't** use wrong package name (Blazor.Diagrams vs Z.Blazor.Diagrams)  
❌ **Don't** load scripts before Blazor framework  
❌ **Don't** cast MentorDiagram to BlazorDiagram  
❌ **Don't** access Node in field initializers  

---

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Source Project:** eDesignStudio  
**Framework:** .NET 9 Blazor InteractiveServer
