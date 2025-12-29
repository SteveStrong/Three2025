# Mentor 2D Technician Specification

## Overview
A technician for creating and managing 2D mentor shapes from the FoundryMentorModeler library. Enables AI agents to create diagrams, flowcharts, state machines, and other structured visual models on the 2D canvas.

## Purpose
- Provide AI agents with tools to create mentor-based 2D diagrams
- Support diagram elements (boxes, connectors, labels, groups)
- Enable creation of flowcharts, state diagrams, class diagrams, etc.
- Integrate with Shape2DTech for mixed diagrams (basic shapes + mentor shapes)

## Class Design

```csharp
namespace Three2025.Apprentice;

public interface IMentor2DTech
{
    // Canvas Management
    FoPage2D EstablishCanvas2D(string? pageName = null);
    void SetPage(FoPage2D page);
    
    // Diagram Creation
    DiagramInfo CreateDiagram(string name, string diagramType);
    
    // Box/Node Operations
    BoxInfo AddBox(string name, string label, int x, int y, int width, int height, string color);
    BoxInfo AddStateBox(string name, string label, int x, int y, string color);
    BoxInfo AddClassBox(string name, string className, int x, int y);
    BoxInfo AddDecisionBox(string name, string label, int x, int y);
    
    // Connector/Link Operations
    LinkInfo AddLink(string sourceName, string targetName, string linkType, string label);
    LinkInfo AddDirectedLink(string sourceName, string targetName, string label);
    LinkInfo AddBidirectionalLink(string sourceName, string targetName, string label);
    
    // Group Operations
    GroupInfo CreateGroup(string name, List<string> memberNames);
    GroupInfo AddToGroup(string groupName, string memberName);
    GroupInfo RemoveFromGroup(string groupName, string memberName);
    
    // Label Operations
    LabelInfo AddLabel(string name, string text, int x, int y, int fontSize);
    
    // Query Operations
    BoxInfo? FindBox(string name);
    LinkInfo? FindLink(string sourceName, string targetName);
    List<BoxInfo> GetAllBoxes();
    List<LinkInfo> GetAllLinks();
    
    // Layout Operations
    void AutoLayout(string layoutType); // "hierarchical", "grid", "circular", "force-directed"
    void AlignBoxes(List<string> boxNames, string alignment); // "left", "right", "top", "bottom", "center"
    void DistributeBoxes(List<string> boxNames, string direction); // "horizontal", "vertical"
    
    // Modification Operations
    void MoveBox(string name, int x, int y);
    void ResizeBox(string name, int width, int height);
    void UpdateBoxLabel(string name, string newLabel);
    void UpdateLinkLabel(string sourceName, string targetName, string newLabel);
    void DeleteBox(string name);
    void DeleteLink(string sourceName, string targetName);
}

public class Mentor2DTech : IMentor2DTech
{
    private readonly IWorkspace _workspace;
    private FoPage2D? _page;
    private readonly Dictionary<string, object> _mentorShapes = new();
    private readonly ILogger<Mentor2DTech> _logger;
    
    // Implementation...
}
```

## Data Transfer Objects

```csharp
public record DiagramInfo(
    string Name,
    string Type,
    int BoxCount,
    int LinkCount
);

public record BoxInfo(
    string Name,
    string Label,
    int X,
    int Y,
    int Width,
    int Height,
    string Color,
    string ShapeId
);

public record LinkInfo(
    string SourceName,
    string TargetName,
    string LinkType,
    string Label,
    string ShapeId
);

public record GroupInfo(
    string Name,
    List<string> Members,
    string ShapeId
);

public record LabelInfo(
    string Name,
    string Text,
    int X,
    int Y,
    int FontSize,
    string ShapeId
);
```

## Key Features

### 1. Diagram Types
- **Flowchart**: Process boxes, decision diamonds, connectors with arrows
- **State Diagram**: State boxes with rounded corners, transitions with labels
- **Class Diagram**: Class boxes with compartments (name, attributes, methods)
- **Sequence Diagram**: Lifelines and messages between objects
- **Network Diagram**: Nodes and connections with labels

### 2. Box Types
- **Standard Box**: Rectangle with label
- **State Box**: Rounded rectangle for state machines
- **Class Box**: Multi-compartment box with sections
- **Decision Box**: Diamond shape for decision points
- **Actor Box**: Stick figure for use cases
- **Database Box**: Cylinder shape for databases

### 3. Link Types
- **Directed**: Single arrow (A → B)
- **Bidirectional**: Double arrow (A ↔ B)
- **Association**: Plain line
- **Aggregation**: Line with diamond
- **Composition**: Line with filled diamond
- **Inheritance**: Line with hollow triangle
- **Dependency**: Dashed line with arrow

### 4. Layout Algorithms
- **Hierarchical**: Top-down tree layout for flowcharts
- **Grid**: Align items in rows and columns
- **Circular**: Arrange items in a circle
- **Force-directed**: Physics-based automatic spacing

### 5. Smart Features
- **Auto-routing**: Connectors automatically route around boxes
- **Snap-to-grid**: Optional alignment to grid
- **Connection points**: Predefined attachment points on boxes
- **Label positioning**: Smart label placement on links
- **Group behaviors**: Move/resize groups as units

## Integration Points

### With Shape2DTech
```csharp
// Mixed diagrams combining basic shapes and mentor shapes
shape2DTech.AddRectangle("background", 800, 600, "lightgray", 0, 0);
mentor2DTech.AddBox("process1", "Start Process", 100, 100, 120, 60, "green");
mentor2DTech.AddBox("process2", "End Process", 400, 100, 120, 60, "red");
mentor2DTech.AddDirectedLink("process1", "process2", "Execute");
```

### With FoundryMentorModeler
```csharp
// Use existing mentor shape types from FoundryMentorModeler
using FoundryMentorModeler.Diagram;

var diagramBox = new DiagramBox(name, label);
diagramBox.MoveTo(x, y);
_page.AddShape(diagramBox);
```

## Tool Method Descriptions

All methods include `[Description]` attributes for AI discoverability:

```csharp
[Description("Add a box node to the diagram with label and position")]
public BoxInfo AddBox(
    [Description("Unique name for the box")] string name,
    [Description("Display label text")] string label,
    [Description("X coordinate in pixels")] int x,
    [Description("Y coordinate in pixels")] int y,
    [Description("Width in pixels")] int width,
    [Description("Height in pixels")] int height,
    [Description("Box color (name or hex)")] string color)
```

## Usage Examples

### Example 1: Simple Flowchart
```csharp
mentor2DTech.CreateDiagram("myFlow", "flowchart");
mentor2DTech.AddBox("start", "Start", 100, 100, 100, 50, "green");
mentor2DTech.AddBox("process", "Process Data", 100, 200, 120, 60, "blue");
mentor2DTech.AddDecisionBox("check", "Valid?", 100, 320);
mentor2DTech.AddBox("end", "End", 100, 440, 100, 50, "red");

mentor2DTech.AddDirectedLink("start", "process", "");
mentor2DTech.AddDirectedLink("process", "check", "");
mentor2DTech.AddDirectedLink("check", "end", "Yes");
```

### Example 2: State Machine
```csharp
mentor2DTech.CreateDiagram("stateMachine", "state");
mentor2DTech.AddStateBox("idle", "Idle", 100, 100, "gray");
mentor2DTech.AddStateBox("active", "Active", 300, 100, "green");
mentor2DTech.AddStateBox("error", "Error", 200, 250, "red");

mentor2DTech.AddDirectedLink("idle", "active", "start");
mentor2DTech.AddDirectedLink("active", "idle", "stop");
mentor2DTech.AddDirectedLink("active", "error", "fault");
mentor2DTech.AddDirectedLink("error", "idle", "reset");
```

### Example 3: Class Diagram
```csharp
mentor2DTech.CreateDiagram("classes", "class");
mentor2DTech.AddClassBox("vehicle", "Vehicle", 100, 100);
mentor2DTech.AddClassBox("car", "Car", 100, 250);
mentor2DTech.AddClassBox("truck", "Truck", 250, 250);

mentor2DTech.AddLink("car", "vehicle", "inheritance", "");
mentor2DTech.AddLink("truck", "vehicle", "inheritance", "");
```

## Implementation Phases

### Phase 1: Core Functionality
- Basic box creation and positioning
- Simple directed links between boxes
- Find and query operations
- Integration with FoPage2D

### Phase 2: Advanced Shapes
- Multiple box types (state, class, decision)
- Multiple link types (association, inheritance, etc.)
- Label operations
- Group operations

### Phase 3: Layout & Intelligence
- Auto-layout algorithms
- Alignment and distribution tools
- Smart connector routing
- Snap-to-grid functionality

### Phase 4: Diagram Templates
- Pre-built diagram templates
- Import/export diagram definitions
- Style themes for diagrams
- Validation and constraint checking

## Dependencies

- **FoundryMentorModeler**: Mentor shape library
- **FoundryWorldsAndDrawings**: Canvas and 2D infrastructure
- **IWorkspace**: Service-based architecture
- **Shape2DTech**: Complement for mixed diagrams

## Testing Strategy

- Unit tests for each tool method
- Integration tests with FoPage2D
- Visual regression tests for diagram layouts
- AI agent integration tests via TechnicianTestPanel

## Success Criteria

1. ✅ AI can create flowcharts through natural language
2. ✅ Boxes and links render correctly on canvas
3. ✅ Interactive selection and manipulation works
4. ✅ Auto-layout produces readable diagrams
5. ✅ Integration with existing Shape2DTech seamless
6. ✅ Performance adequate for 50+ box diagrams

## Future Enhancements

- **Collaborative editing**: Multiple agents working on same diagram
- **Versioning**: Diagram history and undo/redo
- **Templates**: Pre-built diagram templates library
- **Export**: Generate diagram as SVG, PNG, or code
- **Validation**: Enforce diagram rules (e.g., state machine validity)
- **Animation**: Animate diagram construction or execution flow
