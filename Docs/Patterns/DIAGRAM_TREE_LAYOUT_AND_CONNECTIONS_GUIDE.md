# Diagram Tree Layout and Connections Guide

**Date:** January 13, 2026  
**Framework:** Blazor InteractiveServer with .NET 9  
**Packages:** FoundryMentorModeler, Plugin710, Z.Blazor.Diagrams

---

## Table of Contents

1. [Overview](#overview)
2. [Tree Layout System](#tree-layout-system)
3. [Creating Connections Between Nodes](#creating-connections-between-nodes)
4. [Adding Layout to DiagramViewer](#adding-layout-to-diagramviewer)
5. [Complete Implementation Example](#complete-implementation-example)
6. [Advanced Layout Patterns](#advanced-layout-patterns)

---

## Overview

Your codebase includes a powerful **LayoutTree_710<V>** system for automatic tree layout and **DiagramLink** infrastructure for connecting nodes. This guide shows how to:

1. **Arrange nodes hierarchically** (horizontal/vertical tree layouts)
2. **Connect nodes with links** (parent-child relationships, port-based connections)
3. **Apply automatic spacing** (computed branch sizes, margins)

### Key Classes

- `LayoutTree_710<V>` - Tree layout algorithm engine
- `LayoutType_710` enum - Horizontal, Vertical, or None
- `DiagramLink` - Base class for connections
- `SystemLinkEditor`, `CircuitLinkEditor` - Specific link types
- `Common_710.CreateDiagramParentTree<V>()` - Build layout tree from model

---

## Tree Layout System

### Layout Types

```csharp
public enum LayoutType_710
{ 
    None,        // No automatic layout
    Horizontal,  // Children arranged left-to-right
    Vertical     // Children arranged top-to-bottom
}
```

### How It Works

**Phase 1: Build Tree Structure**
```csharp
// Creates a hierarchical tree from your model
var layout710 = Common_710.CreateDiagramParentTree<SystemBlock_710>(rootBlock);

// Recursively walks model.ModelComponents<T>() to build parent-child tree
```

**Phase 2: Compute Branch Sizes**
```csharp
// Calculates how much space each branch needs
layout710.ComputeNodeBranchSize(margin, TreeLayoutRules.HorizontalLayout);
```

**Phase 3: Compute Positions**
```csharp
// Places nodes at calculated positions
layout710.ComputeNodeBranchLocation(startPoint, margin, TreeLayoutRules.HorizontalLayout);
```

### Simplified Layout Methods

```csharp
// Horizontal layout (children to the right)
layout710.HorizontalLayout(
    PinX: 500,              // Starting X position
    PinY: 200,              // Starting Y position  
    margin: new Point(10, 150)  // X-spacing: 10px, Y-spacing: 150px
);

// Vertical layout (children below)
layout710.VerticalLayout(
    PinX: 500,
    PinY: 200,
    margin: new Point(150, 10)  // X-spacing: 150px, Y-spacing: 10px
);
```

### Layout Rules (Advanced)

Control layout style per tree level:

```csharp
public static class TreeLayoutRules
{
    // Each index represents a tree level (0=root, 1=children, 2=grandchildren, etc.)
    public static List<BoxLayoutStyle> HorizontalLayout = new()
    {
        BoxLayoutStyle.Horizontal,  // Level 0: Horizontal
        BoxLayoutStyle.Horizontal,  // Level 1: Horizontal
        BoxLayoutStyle.Horizontal,  // Level 2: Horizontal
        // ...
    };
    
    public static List<BoxLayoutStyle> VerticalLayout = new()
    {
        BoxLayoutStyle.Vertical,    // All levels vertical
        BoxLayoutStyle.Vertical,
        // ...
    };
}
```

**BoxLayoutStyle Options:**
- `Horizontal` - Children arranged left-to-right
- `Vertical` - Children arranged top-to-bottom
- `HorizontalStacked` - Children stacked vertically but in horizontal flow
- `None` - No layout applied

---

## Creating Connections Between Nodes

### Connection Types

**1. Node-to-Node (Direct)**
```csharp
// Connects center of one node to center of another
EstablishConnectNodes<SystemLinkEditor>(
    result,      // KnEditor2DParameter from base.EstablishEditor()
    fromNode,    // SystemBlockEditor (source node)
    toNode,      // SystemBlockEditor (target node)
    diagram      // MentorDiagram instance
);
```

**2. Port-to-Port (Precise)**
```csharp
// Connects specific ports on nodes
EstablishConnectPorts<CircuitLinkEditor>(
    result,      // KnEditor2DParameter
    "Black",     // Link color
    startPort,   // DiagramPort (source)
    finishPort,  // DiagramPort (target)
    diagram      // MentorDiagram
);
```

### Creating a Link Model Class

Example from `SystemLink_710.cs`:

```csharp
public class SystemLink_710 : Base_710
{
    public SystemBlock_710? From { get; set; }  // Source node
    public SystemBlock_710? To { get; set; }    // Target node
    
    public LayoutType_710 LayoutType { get; set; } = LayoutType_710.Horizontal;

    public SystemLink_710(DT_Component component) : base(component)
    {
    }

    public override KnEditor2DParameter EstablishEditor(string view, MentorDiagram diagram)
    {
        var result = base.EstablishEditor(view, diagram);
        if (result.IsValid())
            return result;

        // Validate both ends exist
        if (From == null || To == null)
        {
            $"Link {Name} has null From or To".WriteInfo();
            return result;
        }

        // Ensure both nodes have editors (renders them if needed)
        var fromEditor = From.EstablishEditor(view, diagram);
        var toEditor = To.EstablishEditor(view, diagram);
        
        if (fromEditor == null || toEditor == null)
        {
            $"Link {Name} has null fromEditor or toEditor".WriteInfo();
            return result;
        }

        // Try to get specific ports based on layout
        var (success, fromPort, toPort) = GetPortsForLayout(LayoutType);

        if (!success)
        {
            // No specific ports - connect node centers
            var fromShape = fromEditor.GetCurrentValueAs<SystemBlockEditor>();
            var toShape = toEditor.GetCurrentValueAs<SystemBlockEditor>();
            
            EstablishConnectNodes<SystemLinkEditor>(result, fromShape, toShape, diagram);
        }
        else
        {
            // Connect specific ports
            var start = fromPort!.EstablishEditor(view, diagram).GetCurrentValueAs<DiagramPort>();
            var finish = toPort!.EstablishEditor(view, diagram).GetCurrentValueAs<DiagramPort>();
            
            EstablishConnectPorts<SystemLinkEditor>(result, "Black", start, finish, diagram);
        }

        // Only show link if both ends are visible
        SetVisible(From.IsVisible && To.IsVisible, true);

        return result;
    }

    // Determine which ports to use based on layout direction
    public (bool success, SystemPort_710? fromPort, SystemPort_710? toPort) GetPortsForLayout(LayoutType_710 layout)
    {
        var (fromPort, toPort) = layout switch
        {
            LayoutType_710.Horizontal => (From?.GetPort("RIGHT"), To?.GetPort("LEFT")),
            LayoutType_710.Vertical => (From?.GetPort("BOTTOM"), To?.GetPort("TOP")),
            _ => (null, null)
        };

        return (fromPort != null && toPort != null, fromPort, toPort);
    }
}
```

### Creating Link Editor

```csharp
public class SystemLinkEditor : DiagramLink
{
    // Port-based constructor
    public SystemLinkEditor(KnComponent source, string color, PortModel sourcePort, PortModel targetPort) 
        : base(source, color, sourcePort, targetPort)
    {
    }

    // Node-based constructor
    public SystemLinkEditor(KnComponent source, NodeModel sourceNode, NodeModel targetNode) 
        : base(source, sourceNode, targetNode)
    {
    }
}
```

---

## Adding Layout to DiagramViewer

Let's enhance your current DiagramViewer with layout capabilities:

### Step 1: Add Layout Options to UI

Update `DiagramViewer.razor`:

```razor
@page "/diagram-viewer"
@using Three2025.Components.DiagramWidgets
@rendermode InteractiveServer

<!-- Add layout buttons -->
<div class="layout-controls mb-3">
    <RadzenButton Text="Horizontal Layout" Icon="view_column" 
        Click="() => ApplyLayout(LayoutType_710.Horizontal)" 
        ButtonStyle="ButtonStyle.Info" />
    <RadzenButton Text="Vertical Layout" Icon="view_stream" 
        Click="() => ApplyLayout(LayoutType_710.Vertical)" 
        ButtonStyle="ButtonStyle.Info" />
    <RadzenButton Text="Reset Layout" Icon="refresh" 
        Click="() => ApplyLayout(LayoutType_710.None)" 
        ButtonStyle="ButtonStyle.Secondary" />
</div>
```

### Step 2: Implement Layout Method

Update `DiagramViewer.razor.cs`:

```csharp
using Plugin_710.Model;
using System.Drawing;

public partial class DiagramViewer : ComponentBase
{
    // ... existing code ...

    /// <summary>
    /// Apply automatic tree layout to the current diagram
    /// </summary>
    private void ApplyLayout(LayoutType_710 layoutType)
    {
        if (_currentModel == null || _diagram == null)
        {
            ShowMessage("No diagram to layout", NotificationSeverity.Warning);
            return;
        }

        try
        {
            // Get the root node (assumes Model710 has a root SystemBlock or solution)
            var solution = _currentModel.GetSolution();
            var rootBlock = solution?.CurrentSystemBlock;
            
            if (rootBlock == null)
            {
                ShowMessage("No root block found for layout", NotificationSeverity.Warning);
                return;
            }

            // Apply layout
            solution.LayoutDiagramTreeFromRoot(rootBlock, layoutType, clear: false);
            
            ShowMessage($"{layoutType} layout applied successfully", NotificationSeverity.Success);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ShowMessage($"Layout error: {ex.Message}", NotificationSeverity.Error);
        }
    }

    /// <summary>
    /// Create a system diagram with automatic layout
    /// </summary>
    private void CreateSystemDiagramWithLayout(LayoutType_710 layoutType = LayoutType_710.Horizontal)
    {
        if (_diagram == null) return;

        try
        {
            _isLoading = true;
            _loadingMessage = "Creating diagram with layout...";
            StateHasChanged();

            // Clear existing diagram
            _diagram.ClearAll();

            // Create model structure (your existing logic)
            _currentModel = new Model_710("DiagramViewerModel", MentorServices!);
            var solution = _currentModel.EstablishSolution();

            // Create root and children
            var mainSystem = solution.AddSystemBlock("MainSystem");
            var sub1 = mainSystem.AddSystemBlock("SubSystem1");
            var sub2 = mainSystem.AddSystemBlock("SubSystem2");
            var sub3 = mainSystem.AddSystemBlock("SubSystem3");

            // Create links between nodes
            var link1 = solution.AddSystemLink("Link1", mainSystem, sub1);
            var link2 = solution.AddSystemLink("Link2", mainSystem, sub2);
            var link3 = solution.AddSystemLink("Link3", mainSystem, sub3);
            
            // Set link layout types to match overall layout
            link1.LayoutType = layoutType;
            link2.LayoutType = layoutType;
            link3.LayoutType = layoutType;

            // Apply automatic layout
            solution.LayoutDiagramTreeFromRoot(mainSystem, layoutType, clear: false);

            // Render to diagram (this will use the computed positions)
            _currentModel.RenderDiagram("System", clear: false, () =>
            {
                _isLoading = false;
                ShowMessage($"System diagram created with {layoutType} layout", NotificationSeverity.Success);
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            _isLoading = false;
            ShowMessage($"Error creating diagram: {ex.Message}", NotificationSeverity.Error);
            StateHasChanged();
        }
    }
}
```

### Step 3: Adding Link Support to Solution_710

If your `Solution_710` doesn't have `AddSystemLink`, add it:

```csharp
public class Solution_710 : Base_710
{
    public SystemLink_710 AddSystemLink(string name, SystemBlock_710 from, SystemBlock_710 to)
    {
        var comp = Common_710.New_DT_Component(name);
        var link = new SystemLink_710(comp)
        {
            From = from,
            To = to
        };
        
        AddChildComponent<SystemLink_710>(link);
        return link;
    }

    public void LayoutDiagramTreeFromRoot(SystemBlock_710 source, LayoutType_710 layoutType, bool clear = false)
    {
        if (layoutType == LayoutType_710.None)
            layoutType = source.LayoutType;

        if (clear)
        {
            source.ClearAllGeometry();
            ClearDiagram(true);
        }

        // Set layout type on all blocks
        source.LayoutType = layoutType;
        foreach (var item in GetLookup().Values
            .Where(obj => obj is SystemBlock_710)
            .Cast<SystemBlock_710>())
        {
            item.LayoutType = layoutType;
        }

        // Set layout type on all links
        foreach (var item in GetLookup().Values
            .Where(obj => obj is SystemLink_710)
            .Cast<SystemLink_710>())
        {
            item.LayoutType = layoutType;
        }

        // Create layout tree
        var layout710 = Common_710.CreateDiagramParentTree<SystemBlock_710>(source);

        // Get starting position
        var (x, y) = source.LocationParameters();
        x = x == 0 ? 500 : x;
        y = y == 0 ? 200 : y;
        source.MoveTo(x, y);

        // Apply layout algorithm
        if (layoutType == LayoutType_710.Horizontal)
            layout710.HorizontalLayout(x, y, new Point(10, 150));

        if (layoutType == LayoutType_710.Vertical)
            layout710.VerticalLayout(x, y, new Point(150, 10));
    }
}
```

---

## Complete Implementation Example

### Full DiagramViewer with Layout and Links

```csharp
// DiagramViewer.razor.cs
public partial class DiagramViewer : ComponentBase
{
    [Inject] private IMentorServices? MentorServices { get; set; }
    
    private MentorDiagram? _diagram;
    private Model_710? _currentModel;
    private bool _isLoading = false;
    private string _loadingMessage = "";

    protected override void OnInitialized()
    {
        // Establish diagram and register widgets
        _diagram = MentorServices!.EstablishDiagram<MentorDiagram>("DiagramCanvas");
        
        _diagram.Register<SystemBlockEditor, SystemBlockWidget>(true);
        _diagram.Register<CircuitNodeEditor, CircuitNodeWidget>(true);
        _diagram.Register<CircuitGroupEditor, CircuitGroupWidget>(true);
        
        base.OnInitialized();
    }

    private void CreateHierarchicalDiagram()
    {
        if (_diagram == null) return;

        _isLoading = true;
        _loadingMessage = "Building hierarchical diagram...";
        StateHasChanged();

        // Create model
        _currentModel = new Model_710("HierarchicalModel", MentorServices!);
        var solution = _currentModel.EstablishSolution();

        // Build tree structure
        var root = solution.AddSystemBlock("Root");
        
        // Level 1
        var child1 = root.AddSystemBlock("Child1");
        var child2 = root.AddSystemBlock("Child2");
        var child3 = root.AddSystemBlock("Child3");
        
        // Level 2
        var grandchild1 = child1.AddSystemBlock("Grandchild1");
        var grandchild2 = child1.AddSystemBlock("Grandchild2");
        var grandchild3 = child2.AddSystemBlock("Grandchild3");

        // Create links
        solution.AddSystemLink("L1", root, child1).LayoutType = LayoutType_710.Horizontal;
        solution.AddSystemLink("L2", root, child2).LayoutType = LayoutType_710.Horizontal;
        solution.AddSystemLink("L3", root, child3).LayoutType = LayoutType_710.Horizontal;
        solution.AddSystemLink("L4", child1, grandchild1).LayoutType = LayoutType_710.Horizontal;
        solution.AddSystemLink("L5", child1, grandchild2).LayoutType = LayoutType_710.Horizontal;
        solution.AddSystemLink("L6", child2, grandchild3).LayoutType = LayoutType_710.Horizontal;

        // Apply horizontal layout with automatic spacing
        solution.LayoutDiagramTreeFromRoot(root, LayoutType_710.Horizontal, clear: false);

        // Render to diagram
        _currentModel.RenderDiagram("System", clear: false, () =>
        {
            _isLoading = false;
            StateHasChanged();
        });
    }

    private void SwitchToVerticalLayout()
    {
        if (_currentModel == null) return;

        var solution = _currentModel.GetSolution();
        var root = solution?.CurrentSystemBlock;
        
        if (root != null)
        {
            solution!.LayoutDiagramTreeFromRoot(root, LayoutType_710.Vertical, clear: false);
            StateHasChanged();
        }
    }
}
```

---

## Advanced Layout Patterns

### Custom Margins Per Layout

```csharp
// Horizontal with wide spacing
layout710.HorizontalLayout(
    PinX: 500, 
    PinY: 200, 
    margin: new Point(50, 200)  // 50px between siblings, 200px between levels
);

// Vertical with compact spacing
layout710.VerticalLayout(
    PinX: 500, 
    PinY: 200, 
    margin: new Point(300, 20)  // 300px between siblings, 20px between levels
);
```

### Mixed Layout Rules

```csharp
// Root horizontal, children vertical, grandchildren horizontal again
public static List<BoxLayoutStyle> CustomLayout = new()
{
    BoxLayoutStyle.Horizontal,  // Level 0
    BoxLayoutStyle.Vertical,    // Level 1
    BoxLayoutStyle.Horizontal,  // Level 2
    BoxLayoutStyle.Vertical,    // Level 3
};

// Apply custom layout
layout710.Layout(500, 200, new Point(50, 150), CustomLayout);
```

### Conditional Links

```csharp
// Only create links between visible nodes
if (From.IsVisible && To.IsVisible)
{
    var link = solution.AddSystemLink($"Link_{From.Name}_{To.Name}", From, To);
    link.LayoutType = currentLayoutType;
}

// Links will automatically hide when ends are hidden
SetVisible(From.IsVisible && To.IsVisible, true);
```

### Port Configuration for Links

```csharp
public class SystemBlock_710 : Base_710
{
    // Create ports for different connection directions
    private void CreatePorts()
    {
        AddPort("TOP", PortAlignment.Top);
        AddPort("BOTTOM", PortAlignment.Bottom);
        AddPort("LEFT", PortAlignment.Left);
        AddPort("RIGHT", PortAlignment.Right);
    }

    public SystemPort_710? GetPort(string alignment)
    {
        return ModelComponents<SystemPort_710>()
            ?.FirstOrDefault(p => p.Alignment == alignment);
    }
}
```

---

## Key Takeaways

### ✅ Tree Layout
1. **Build tree** with `Common_710.CreateDiagramParentTree<T>(root)`
2. **Choose layout** type: `Horizontal` or `Vertical`
3. **Set margins** to control spacing
4. **Apply layout** with `HorizontalLayout()` or `VerticalLayout()`

### ✅ Connections
1. **Create link model** class (inherits `Base_710`)
2. **Create link editor** class (inherits `DiagramLink`)
3. **Connect nodes** with `EstablishConnectNodes<T>()` or `EstablishConnectPorts<T>()`
4. **Handle visibility** - links auto-hide when ends are hidden

### ✅ Integration
1. **Register widget** in `OnInitialized()` if you have custom link widgets
2. **Set LayoutType** on links to match diagram layout
3. **Render after layout** - positions are computed, then rendered
4. **Update incrementally** - can re-layout without full rebuild

---

## Common Patterns

### Pattern 1: Simple Tree with Links
```csharp
var root = solution.AddSystemBlock("Root");
var child = root.AddSystemBlock("Child");
var link = solution.AddSystemLink("Connection", root, child);
solution.LayoutDiagramTreeFromRoot(root, LayoutType_710.Horizontal, clear: false);
```

### Pattern 2: Dynamic Re-layout
```csharp
// Switch between layouts on user action
void ToggleLayout()
{
    var newType = currentType == LayoutType_710.Horizontal 
        ? LayoutType_710.Vertical 
        : LayoutType_710.Horizontal;
    
    solution.LayoutDiagramTreeFromRoot(rootBlock, newType, clear: false);
}
```

### Pattern 3: Expand/Collapse with Layout
```csharp
void ToggleNodeChildren(SystemBlock_710 block)
{
    block.ShowChildren = !block.ShowChildren;
    
    // Re-layout to accommodate changes
    var solution = block.GetParentOfType<Solution_710>();
    solution?.LayoutDiagramTreeFromRoot(rootBlock, currentLayoutType, clear: false);
}
```

---

**Document Version:** 1.0  
**Last Updated:** January 13, 2026  
**Framework:** .NET 9 Blazor InteractiveServer
