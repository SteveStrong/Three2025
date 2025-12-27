# AI Agent Shape Tool Specification

**Version:** 1.0  
**Date:** December 26, 2025  
**Purpose:** Dual-purpose guide for building AI-accessible shape manipulation tools and for AI agents using them

---

## Overview

This document serves two audiences:

1. **Tool Developers**: Instructions for wrapping the FoundryWorldsAndDrawings library into AI-accessible tools
2. **AI Agents**: Guide for understanding and effectively using shape manipulation tools

The focus is on **dynamic shape manipulation** - programmatically creating, modifying, querying, and deleting 2D and 3D shapes on canvases through API calls rather than declarative markup.

---

## Part 1: Tool Developer Guide

### 1.1 Library Architecture Essentials

#### Core Hierarchy
```
Canvas3DComponent (UI Component)
  └─> FoStage3D (Scene Container)
       └─> FoShape3D (Individual Shapes)
            └─> GeomType, Width, Height, Depth, Color, Visibility
```

**Critical Binding Pattern:**
```csharp
// The Canvas owns the Stage - ALWAYS use the canvas reference
Canvas3DComponent Canvas3DReference;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && Canvas3DReference?.Stage != null)
    {
        // Connect your tool to the stage
        GeometryTool.SetStage(Canvas3DReference.Stage);
    }
}
```

**Why this matters:** Never hardcode stage names. The Canvas3DComponent.Stage property provides the guaranteed-correct stage reference.

#### Service Lifetime Considerations

**CRITICAL:** Service lifetime must match your dependencies.

```csharp
// ❌ WRONG - Singleton cannot resolve Scoped services
services.AddSingleton<IShapeTool, ShapeTool>();
services.AddScoped<IGeometryTech, GeometryTech>();

// ✅ CORRECT - Match lifetimes
services.AddScoped<IShapeTool, ShapeTool>();
services.AddScoped<IGeometryTech, GeometryTech>();
```

**Rule:** If your tool depends on IFoundryService, IFoCollection, or any Stage/Canvas references, use `Scoped` lifetime.

### 1.2 Shape Creation API

All shapes use the same pattern: `new FoShape3D(name, color).Create{Type}(name, width, height, depth)`

#### Available Shape Types

```csharp
// Basic 3D Shapes
shape.CreateBox(name, width, height, depth);        // Rectangular box
shape.CreateSphere(name, width, height, depth);     // Sphere (use equal dimensions)
shape.CreateCylinder(name, width, height, depth);   // Cylinder (width=depth for circular)
shape.CreateCone(name, width, height, depth);       // Cone (width=depth for circular base)
shape.CreateTorus(name, width, height, depth);      // Torus (donut shape)

// Platonic Solids
shape.CreateTetrahedron(name, width, height, depth);
shape.CreateOctahedron(name, width, height, depth);
shape.CreateDodecahedron(name, width, height, depth);
shape.CreateIcosahedron(name, width, height, depth);

// Advanced Shapes
shape.CreateTorusKnot(name, width, height, depth);
shape.CreateCapsule(name, width, height, depth);

// 2D Shapes (for Canvas2D)
shape.CreatePlane(name, width, height, depth);
shape.CreateCircle(name, width, height, depth);
shape.CreateRing(name, width, height, depth);
```

#### Dimension Guidelines

| Shape | Width | Height | Depth | Notes |
|-------|-------|--------|-------|-------|
| Box | X size | Y size | Z size | All can differ |
| Sphere | Radius | Radius | Radius | **Use equal values** |
| Cylinder | Radius | Length | Radius | Width=Depth for circular |
| Cone | Base radius | Height | Base radius | Width=Depth for circular base |
| Torus | Major radius | Minor radius | Major radius | Complex - see examples |

**Typical defaults from real usage:**
```csharp
// From MultiCanvas3DTest.razor.cs
sphere.CreateSphere("Sphere", 1, 1, 1);           // Unit sphere
cylinder.CreateCylinder("Cylinder", 1, 3, 1);     // Tall thin cylinder
cone.CreateCone("Cone", 1.5, 3, 1.5);             // Medium cone
```

### 1.3 Stage Management

#### Establishing a Stage

```csharp
public class GeometryTech : IGeometryTech
{
    private FoStage3D? _stage;
    private readonly IFoundryService _foundryService;
    
    public void SetStage(FoStage3D stage)
    {
        _stage = stage;
    }
    
    // Tool method that operates on the stage
    public List<ShapeInfo> AddShape(string name, bool isOn, string color, string shapeType)
    {
        if (_stage == null)
            throw new InvalidOperationException("Stage not initialized");
            
        var shape = new FoShape3D(name, color);
        
        // Create the shape based on type
        switch (shapeType.ToLower())
        {
            case "sphere":
                shape.CreateSphere(name, 2, 2, 2);
                break;
            case "cylinder":
                shape.CreateCylinder(name, 1, 3, 1);
                break;
            // ... other cases
            default:
                shape.CreateBox(name, 2, 1, 1);
                break;
        }
        
        shape.On = isOn;
        
        // Add to stage
        _stage.AddShape(shape);
        
        // Trigger UI update
        RefreshUI();
        
        return GetShapes(); // Return current state
    }
}
```

#### Refreshing the UI

**Critical Pattern:** Use PubSub messaging to update TreeView and other UI components.

```csharp
using FoundryMentorModeler.Model; // For RefreshRenderMessage

public void RefreshUI()
{
    try
    {
        if (_foundryService?.PubSub != null)
        {
            _foundryService.PubSub.Publish(RefreshRenderMessage.ClearAllSelected());
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ RefreshUI failed: {ex.Message}");
    }
}
```

**When to call RefreshUI:**
- After adding shapes
- After deleting shapes
- After modifying shape properties (color, visibility, position)
- After clearing the scene

### 1.4 Wrapping as AI Tools

#### Interface Design

```csharp
public interface IGeometryTech
{
    // State management
    void SetStage(FoStage3D stage);
    
    // Core operations
    [Description("Creates a new 3D shape and adds it to the scene")]
    List<ShapeInfo> AddShape(
        [Description("Unique name for the shape")] string name,
        [Description("Whether the shape is visible (true) or hidden (false)")] bool isOn,
        [Description("Color name (red, blue, green) or hex code (#ff0000)")] string color,
        [Description("Type of shape: box, sphere, cylinder, cone, or torus")] string shapeType = "box");
    
    [Description("Moves a shape to a new position in 3D space")]
    List<ShapeInfo> RepositionShape(
        [Description("Name of the shape to move")] string name,
        [Description("X coordinate")] double x,
        [Description("Y coordinate")] double y,
        [Description("Z coordinate")] double z);
    
    [Description("Changes the color of an existing shape")]
    List<ShapeInfo> ChangeColor(
        [Description("Name of the shape")] string name,
        [Description("New color (name or hex code)")] string color);
    
    [Description("Shows or hides a shape")]
    List<ShapeInfo> ChangeState(
        [Description("Name of the shape")] string name,
        [Description("True to show, false to hide")] bool isOn);
    
    [Description("Removes a shape from the scene")]
    List<ShapeInfo> DeleteShape(
        [Description("Name of the shape to delete")] string name);
    
    [Description("Removes all shapes from the scene")]
    List<ShapeInfo> ClearShapes();
    
    // Query operations
    [Description("Gets information about all shapes in the scene")]
    List<ShapeInfo> GetShapes();
    
    // Utility
    [Description("Returns a random color name")]
    string PickARandomColor();
    
    [Description("Refreshes the UI to show latest changes")]
    void RefreshUI();
}

// Data transfer object for shape information
public class ShapeInfo
{
    public string Name { get; set; }
    public string GeomType { get; set; }
    public string Color { get; set; }
    public bool IsVisible { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Depth { get; set; }
}
```

#### Key Design Principles

1. **[Description] Attributes**: Essential for AI understanding. Describe:
   - What the method does (summary)
   - What each parameter means
   - Valid values/ranges when applicable

2. **Return State Snapshots**: Return `List<ShapeInfo>` showing current scene state after operations
   - Provides immediate feedback
   - Agents can verify changes
   - Enables chained operations

3. **Consistent Naming**: Use clear, action-oriented method names
   - `AddShape` not `CreateShape` (create implies construction only)
   - `RepositionShape` not `MoveShape` (reposition is more precise)
   - `ChangeState` not `ToggleVisibility` (change is more flexible)

4. **Default Parameters**: Use sensible defaults to simplify common cases
   - `shapeType = "box"` - most common shape
   - `isOn = true` - shapes usually visible
   - Agents can omit optional parameters

5. **Error Handling**: Return meaningful error information
   ```csharp
   if (!ShapeExists(name))
   {
       Console.WriteLine($"❌ Shape '{name}' not found");
       return GetShapes(); // Return current state even on error
   }
   ```

### 1.5 Integration with AI Tool Discovery

#### Tool Provider Pattern

```csharp
public class TechnicianToolProvider : ITechnicianToolProvider
{
    private readonly IServiceProvider _serviceProvider;
    
    public List<AIFunction> DiscoverAllTools()
    {
        var tools = new List<AIFunction>();
        var technicianInterfaces = GetTechnicianInterfaces();
        
        foreach (var interfaceType in technicianInterfaces)
        {
            // Resolve the implementation
            var implementation = _serviceProvider.GetService(interfaceType);
            if (implementation == null) continue;
            
            // Find methods with [Description] attribute
            var methods = interfaceType.GetMethods()
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null);
            
            foreach (var method in methods)
            {
                // Convert to AIFunction using AIFunctionFactory
                var aiFunction = AIFunctionFactory.Create(method, implementation);
                tools.Add(aiFunction);
            }
        }
        
        return tools;
    }
}
```

**Registration:**
```csharp
// Program.cs
builder.Services.AddScoped<IGeometryTech, GeometryTech>();
builder.Services.AddScoped<ITechnicianToolProvider, TechnicianToolProvider>();
builder.Services.AddScoped<IChatOrchestrator, ChatOrchestrator>();
```

### 1.6 Complete Working Example

**From Three2025/Apprentice/GeometryTech.cs** (condensed):

```csharp
public class GeometryTech : IGeometryTech
{
    private FoStage3D? _stage;
    private readonly IFoundryService _foundryService;
    private readonly Random _gen = new Random();
    
    public GeometryTech(IFoundryService foundryService)
    {
        _foundryService = foundryService;
    }
    
    public void SetStage(FoStage3D stage) => _stage = stage;
    
    [Description("Creates a new 3D shape and adds it to the scene")]
    public List<ShapeInfo> AddShape(
        string name, bool isOn, string color,
        [Description("The type of shape: box, sphere, cylinder, cone, or torus")] 
        string shapeType = "box")
    {
        if (_stage == null)
            throw new InvalidOperationException("Stage not initialized");
            
        var shape = new FoShape3D(name, color);
        
        switch (shapeType.ToLower())
        {
            case "sphere":
                shape.CreateSphere(name, 2, 2, 2);
                break;
            case "cylinder":
                shape.CreateCylinder(name, 1, 3, 1);
                break;
            case "cone":
                shape.CreateCone(name, 1.5, 3, 1.5);
                break;
            case "torus":
                shape.CreateTorus(name, 1, 0.4, 1, 0.4);
                break;
            default:
                shape.CreateBox(name, 2, _gen.NextDouble() * 4 + 1, _gen.NextDouble() * 4 + 1);
                break;
        }
        
        shape.On = isOn;
        _stage.AddShape(shape);
        
        Console.WriteLine($"✅ Created {shapeType} '{name}' with color '{color}', visible={isOn}");
        RefreshUI();
        
        return GetShapes();
    }
    
    [Description("Gets information about all shapes in the scene")]
    public List<ShapeInfo> GetShapes()
    {
        if (_stage == null) return new List<ShapeInfo>();
        
        return _stage.Children
            .OfType<FoShape3D>()
            .Select(s => new ShapeInfo
            {
                Name = s.Name,
                GeomType = s.GeomType,
                Color = s.ColorFill,
                IsVisible = s.On,
                X = s.X,
                Y = s.Y,
                Z = s.Z,
                Width = s.Width,
                Height = s.Height,
                Depth = s.Depth
            })
            .ToList();
    }
    
    public void RefreshUI()
    {
        try
        {
            _foundryService?.PubSub?.Publish(RefreshRenderMessage.ClearAllSelected());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ RefreshUI failed: {ex.Message}");
        }
    }
}
```

---

## Part 2: AI Agent Usage Guide

### 2.1 Conceptual Model

#### What You're Working With

- **Canvas**: The drawing surface (3D or 2D)
- **Stage**: Container that holds shapes (owned by canvas)
- **Shape**: Individual geometric objects (boxes, spheres, etc.)

#### Mental Model
```
Think of it like a theater:
- Canvas = The theater building
- Stage = The performance space
- Shapes = The actors/props on stage
```

### 2.2 Operation Patterns

#### Creating Shapes

**Basic Creation:**
```
User: "Create a red box"
Agent: AddShape(name="RedBox", isOn=true, color="red", shapeType="box")
Result: Box appears on canvas
```

**With Positioning:**
```
User: "Add a blue sphere at position 5, 0, 0"
Agent: 
  1. AddShape(name="BlueSphere", isOn=true, color="blue", shapeType="sphere")
  2. RepositionShape(name="BlueSphere", x=5, y=0, z=0)
Result: Sphere appears at specified location
```

**Multiple Shapes:**
```
User: "Create three colored boxes in a row"
Agent:
  1. AddShape("RedBox", true, "red", "box")
  2. RepositionShape("RedBox", -3, 0, 0)
  3. AddShape("GreenBox", true, "green", "box")
  4. RepositionShape("GreenBox", 0, 0, 0)
  5. AddShape("BlueBox", true, "blue", "box")
  6. RepositionShape("BlueBox", 3, 0, 0)
```

#### Modifying Shapes

**Changing Color:**
```
User: "Make the box blue"
Agent: ChangeColor(name="RedBox", color="blue")
```

**Hiding/Showing:**
```
User: "Hide the sphere"
Agent: ChangeState(name="BlueSphere", isOn=false)

User: "Show it again"
Agent: ChangeState(name="BlueSphere", isOn=true)
```

**Moving:**
```
User: "Move the cone up 5 units"
Agent: 
  1. GetShapes() // Find current position
  2. RepositionShape(name="Cone", x=currentX, y=currentY+5, z=currentZ)
```

#### Querying State

**What's in the scene:**
```
User: "What shapes do I have?"
Agent: GetShapes()
Result: List of all shapes with properties
Response: "You have 3 shapes: RedBox (red box at 0,0,0), BlueSphere (blue sphere at 5,0,0), GreenCone (green cone at -2,1,0)"
```

#### Deleting Shapes

**Remove specific:**
```
User: "Delete the red box"
Agent: DeleteShape(name="RedBox")
```

**Clear all:**
```
User: "Clear the scene"
Agent: ClearShapes()
```

### 2.3 Parameter Conventions

#### Color Values

**Named colors** (preferred for simplicity):
```
"red", "blue", "green", "yellow", "orange", "purple", "pink", "cyan", "magenta"
"white", "black", "gray", "brown"
```

**Hex codes** (for precise colors):
```
"#ff0000" (red)
"#00ff00" (green)
"#0000ff" (blue)
"#ff00ff" (magenta)
```

**When user says "random":**
```
Agent: PickARandomColor() // Returns a random named color
```

#### Shape Names

**Guidelines:**
- Must be unique (tool will warn if name exists)
- Use descriptive names: "RedBox", "CenterSphere", "Tower1"
- CamelCase or snake_case both work
- No spaces (use underscores): "big_box" not "big box"

**Pattern for multiple similar shapes:**
```
"Box1", "Box2", "Box3"
"RedSphere", "BlueSphere", "GreenSphere"
"Tower_Left", "Tower_Center", "Tower_Right"
```

#### Position Values

**3D Coordinates** (x, y, z):
- **X**: Left (-) to Right (+)
- **Y**: Down (-) to Up (+)
- **Z**: Back (-) to Front (+)

**Typical ranges:**
- -10 to +10 for most scenes
- 0, 0, 0 is the center
- Start with positions in increments of 2-5 for spacing

**Examples:**
```
Center: (0, 0, 0)
Right of center: (5, 0, 0)
Above center: (0, 3, 0)
Back-left corner: (-5, 0, -5)
```

### 2.4 Shape Types and Their Characteristics

#### Box
- **Usage**: Most versatile, good for buildings, platforms, walls
- **Dimensions**: Width, Height, Depth can all differ
- **Typical sizes**: 1-5 for each dimension
- **Examples**: `AddShape("Platform", true, "brown", "box")`

#### Sphere
- **Usage**: Balls, planets, rounded objects
- **Dimensions**: Use equal width/height/depth for perfect sphere
- **Typical sizes**: 1-3 radius
- **Examples**: `AddShape("Ball", true, "red", "sphere")`
- **Note**: If dimensions differ, creates an ellipsoid

#### Cylinder
- **Usage**: Columns, pipes, trees
- **Dimensions**: Width=Depth (radius), Height (length)
- **Typical sizes**: radius 0.5-2, height 2-5
- **Examples**: `AddShape("Column", true, "gray", "cylinder")`

#### Cone
- **Usage**: Trees, traffic cones, rockets
- **Dimensions**: Width=Depth (base radius), Height
- **Typical sizes**: radius 1-2, height 2-4
- **Examples**: `AddShape("Tree", true, "green", "cone")`

#### Torus
- **Usage**: Rings, donuts, decorative elements
- **Dimensions**: Complex (major/minor radius)
- **Typical sizes**: Use defaults from library (1, 0.4, 1, 0.4)
- **Examples**: `AddShape("Ring", true, "gold", "torus")`

### 2.5 Multi-Step Recipes

#### Scene Setup Pattern
```
User: "Create a simple room with floor, walls, and a table"
Agent:
  1. AddShape("Floor", true, "lightgray", "box")
  2. RepositionShape("Floor", 0, -1, 0)  // Below origin
  
  3. AddShape("WallBack", true, "white", "box")
  4. RepositionShape("WallBack", 0, 2, -5)
  
  5. AddShape("TableTop", true, "brown", "box")
  6. RepositionShape("TableTop", 0, 0, 0)
  
  7. AddShape("TableLeg1", true, "brown", "cylinder")
  8. RepositionShape("TableLeg1", -1, -1, -1)
```

#### Iterative Refinement
```
User: "Make it red"
Agent: 
  1. GetShapes() // Find what user is referring to
  2. ChangeColor(name=last_created_shape, color="red")

User: "No, darker"
Agent: ChangeColor(name=same_shape, color="#8b0000") // Dark red hex

User: "Perfect, move it to the left"
Agent: RepositionShape(name=same_shape, x=currentX-3, y=currentY, z=currentZ)
```

#### Building from User Description
```
User: "Create a traffic light"
Agent:
  1. AddShape("Pole", true, "gray", "cylinder")
  2. RepositionShape("Pole", 0, 0, 0)
  
  3. AddShape("RedLight", true, "red", "sphere")
  4. RepositionShape("RedLight", 0, 2, 0)
  
  5. AddShape("YellowLight", true, "yellow", "sphere")
  6. RepositionShape("YellowLight", 0, 1, 0)
  
  7. AddShape("GreenLight", true, "green", "sphere")
  8. RepositionShape("GreenLight", 0, 0, 0)
```

### 2.6 Error Recovery

#### Shape Not Found
```
Agent: ChangeColor("NonexistentBox", "blue")
System: ❌ Shape 'NonexistentBox' not found
Agent Recovery: GetShapes() to see what exists, then try correct name
```

#### Invalid Color
```
Agent: AddShape("Box", true, "rainbowpurplegreen", "box")
System: May use default color or closest match
Agent Recovery: Use standard color names or hex codes
```

#### Stage Not Initialized
```
System: ❌ Stage not initialized
Meaning: Canvas/Stage connection not established yet
Recovery: This is a page lifecycle issue, not agent error - inform user
```

### 2.7 Best Practices for Agents

#### Always Return Context
When user asks "what shapes do I have?", call `GetShapes()` and format nicely:
```
"You currently have 3 shapes:
1. RedBox - red box at position (0, 0, 0)
2. BlueSphere - blue sphere at position (5, 0, 0)  
3. GreenCone - green cone at position (-2, 1, 0)"
```

#### Verify Before Assuming
User: "Make the box blue"
- If multiple boxes exist, ask which one
- If no box exists, inform user
- Don't assume which shape they mean

#### Use Descriptive Names
When creating shapes, use names that include color/type:
- ✅ "RedBox", "LargeSphere", "CenterCone"
- ❌ "Shape1", "Object", "Thing"

#### Chain Operations Efficiently
Instead of:
```
GetShapes() → AddShape() → GetShapes() → RepositionShape() → GetShapes()
```

Do:
```
AddShape() → RepositionShape() → GetShapes() (final state)
```

Each operation returns current state, so only call `GetShapes()` when you need to make decisions or report to user.

#### Handle Ambiguity
User: "Create a tower"
Agent interpretation options:
1. Single tall cylinder
2. Stack of boxes
3. Cone on top of cylinder

**Best approach:** Choose reasonable default and explain:
```
Agent: "I'll create a tower using a gray cylinder for the body and a red cone for the roof."
[Creates and positions shapes]
Agent: "Tower created! Would you like me to adjust the colors or proportions?"
```

---

## Part 3: Quick Reference

### 3.1 Complete API Surface

#### Creation & Modification
| Method | Purpose | Returns |
|--------|---------|---------|
| `AddShape(name, isOn, color, shapeType)` | Create new shape | Current scene state |
| `RepositionShape(name, x, y, z)` | Move shape | Current scene state |
| `ChangeColor(name, color)` | Recolor shape | Current scene state |
| `ChangeState(name, isOn)` | Show/hide shape | Current scene state |
| `DeleteShape(name)` | Remove shape | Current scene state |
| `ClearShapes()` | Remove all shapes | Empty state |

#### Querying
| Method | Purpose | Returns |
|--------|---------|---------|
| `GetShapes()` | List all shapes | List of ShapeInfo |

#### Utilities
| Method | Purpose | Returns |
|--------|---------|---------|
| `PickARandomColor()` | Get random color | Color name string |
| `RefreshUI()` | Update tree view | void |

### 3.2 Shape Type Catalog

| Type | Use Cases | Typical Dimensions | Example |
|------|-----------|-------------------|---------|
| box | Buildings, platforms, walls | (2, 1, 1) | Furniture, architecture |
| sphere | Balls, planets, heads | (2, 2, 2) | Round objects |
| cylinder | Columns, pipes, trees | (1, 3, 1) | Vertical structures |
| cone | Trees, rockets, hats | (1.5, 3, 1.5) | Pointed objects |
| torus | Rings, donuts | (1, 0.4, 1, 0.4) | Circular rings |

### 3.3 Coordinate System

```
        +Y (Up)
         |
         |
         |
         O-------- +X (Right)
        /
       /
      /
    +Z (Forward)
```

**Reference positions:**
- Origin: `(0, 0, 0)`
- Right: `(5, 0, 0)`
- Left: `(-5, 0, 0)`
- Up: `(0, 5, 0)`
- Down: `(0, -5, 0)`
- Forward: `(0, 0, 5)`
- Back: `(0, 0, -5)`

### 3.4 Color Reference

**Standard Named Colors:**
```
red, green, blue, yellow, orange, purple, pink, cyan, magenta
white, black, gray, brown, lightgray, darkgray
```

**Hex Format:**
```
#RRGGBB where RR, GG, BB are hex values 00-FF
Examples: #ff0000 (red), #00ff00 (green), #0000ff (blue)
```

### 3.5 Code Snippets

#### Page Integration
```csharp
@page "/shape-canvas"
@inject IGeometryTech GeometryTech

<Canvas3DComponent @ref="Canvas3DReference" SceneName="MyScene" />

@code {
    private Canvas3DComponent Canvas3DReference;
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Canvas3DReference?.Stage != null)
        {
            GeometryTech.SetStage(Canvas3DReference.Stage);
            Console.WriteLine("✅ Connected GeometryTech to canvas");
        }
    }
}
```

#### Manual Shape Creation (Without Tool)
```csharp
var stage = Canvas3DReference.Stage;

// Create a red box
var box = new FoShape3D("MyBox", "red")
    .CreateBox("MyBox", 2, 1, 1);
box.MoveTo(0, 0, 0);
stage.AddShape(box);

// Create a blue sphere
var sphere = new FoShape3D("MySphere", "blue")
    .CreateSphere("MySphere", 1, 1, 1);
sphere.MoveTo(5, 0, 0);
stage.AddShape(sphere);

// Trigger UI refresh
_foundryService.PubSub.Publish(RefreshRenderMessage.ClearAllSelected());
```

#### Querying Shapes
```csharp
var shapes = stage.Children.OfType<FoShape3D>();

foreach (var shape in shapes)
{
    Console.WriteLine($"{shape.Name}: {shape.GeomType} at ({shape.X}, {shape.Y}, {shape.Z})");
    Console.WriteLine($"  Color: {shape.ColorFill}, Visible: {shape.On}");
    Console.WriteLine($"  Size: {shape.Width} x {shape.Height} x {shape.Depth}");
}
```

---

## Related Documentation

- **[3D_GEOMETRY_LIFECYCLE_GUIDE.md](3D_GEOMETRY_LIFECYCLE_GUIDE.md)** - Deep dive into shape lifecycle
- **[MULTI_CANVAS_ARCHITECTURE.md](MULTI_CANVAS_ARCHITECTURE.md)** - Multi-scene support
- **[DIRTY_TRACKING_GUIDE.md](DIRTY_TRACKING_GUIDE.md)** - Change tracking system
- **[COPILOT_HANDOFF_GUIDE.md](COPILOT_HANDOFF_GUIDE.md)** - General developer onboarding

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 26, 2025 | Initial specification based on GeometryTech implementation |

---

## Examples from Real Implementations

### GeometryTech (Three2025)
Complete working implementation of all patterns in this spec.
- **Location**: `Three2025/Apprentice/GeometryTech.cs`
- **Usage**: `Three2025/Components/Pages/AgentCanvasIntegration.razor`

### MultiCanvas3DTest
Shows manual shape creation without tools.
- **Location**: `Three2025/Components/Pages/MultiCanvas3DTest.razor.cs`
- **Demonstrates**: Direct FoShape3D API usage

### GeometryVisualizationService
Advanced shape manipulation for geometry visualization.
- **Location**: `Three2025/Services/Visualization/GeometryVisualizationService.cs`
- **Demonstrates**: Complex shape hierarchies, coordinate transformations
