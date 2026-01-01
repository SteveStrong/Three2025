# Rack Equipment Knowledge Model Guide

## Overview

This guide describes the knowledge model system for rack-mounted equipment, using the PartComponent pattern to create a declarative, parameter-based representation that can generate Foundry Object (FO) geometry.

## Architecture

```
DataCenterRackModel (PartComponent)
├── MFCabinet1Concept (RackCabinetConcept)
│   ├── BlankingPanelConcept (RackEquipmentConcept)
│   ├── HPL2ModuleConcept (RackEquipmentConcept)
│   ├── ITCConcept (RackEquipmentConcept)
│   └── StorageDrawerConcept × 3 (RackEquipmentConcept)
├── MFCabinet2Concept (RackCabinetConcept)
│   ├── ZIFConnectorConcept (RackEquipmentConcept)
│   ├── SPPMCConcept (RackEquipmentConcept)
│   └── GWTConcept (RackEquipmentConcept)
├── MFCabinet3Concept (RackCabinetConcept)
│   ├── BlankingPanelConcept (RackEquipmentConcept)
│   ├── SPPMCConcept (RackEquipmentConcept)
│   ├── FHPCConcept (RackEquipmentConcept)
│   └── LPASConcept × 4 (RackEquipmentConcept)
└── MFCabinet4Concept (RackCabinetConcept)
    ├── ZIFConnectorConcept (RackEquipmentConcept)
    ├── ITAConcept #1 (RackEquipmentConcept)
    └── ITAConcept #2 (RackEquipmentConcept)
```

## Key Concepts

### 1. Rack Units (RU)
- **Standard**: 1 RU = 1.75 inches = 44.45 mm
- **40U Cabinet**: 40 RU × 1.75" = 70 inches total height
- **19" Rack Standard**: 19 inches width (EIA-310-D standard)

### 2. Knowledge Model Layer
Uses PartComponent with Calculations([]) to define:
- **Equipment dimensions** in rack units
- **Cabinet utilization** tracking
- **Hierarchical relationships** between cabinets and equipment
- **Automatic calculations** for positions and totals

### 3. FO Generation Layer
RackKnowledgeToFoFactory converts knowledge models to FO objects:
- Reads parameters from knowledge model
- Creates corresponding FO shapes
- Applies positioning based on RU values
- Preserves hierarchy in 3D scene

## Core Classes

### RackEquipmentConcept (Base Class)

```csharp
public abstract class RackEquipmentConcept : PartComponent
{
    protected RackEquipmentConcept(string name, double defaultHeightRU, string defaultColor)
    {
        Calculations([
            "RackUnitHeight: 1.75",                 // Standard 1 RU = 1.75"
            "StandardRackWidth: 19.0",              // 19" standard
            "StandardDepth: 20.0",                  // 20" typical
            "HeightRU: {defaultHeightRU}",          // Equipment height
            "HeightInches: HeightRU@ * RackUnitHeight@",  // Calculated
            "StartRU: 1",                           // Position
            "EndRU: StartRU@ + HeightRU@ - 1"      // End position
        ]);
    }
    
    public void SetPosition(int startRU) { ... }
}
```

**Parameters:**
- `RackUnitHeight`: Constant 1.75 inches per RU
- `HeightRU`: Equipment height in rack units
- `HeightInches`: Calculated actual height
- `StartRU`: Starting rack unit position
- `EndRU`: Calculated ending position
- `Color`: Visual color for rendering

### RackCabinetConcept

```csharp
public class RackCabinetConcept : PartComponent
{
    public RackCabinetConcept(string name = "RackCabinet")
    {
        Calculations([
            "TotalRackUnits: 40",
            "TotalHeight: TotalRackUnits@ * RackUnitHeight@",  // 70"
            "EquipmentCount: 0",
            "UsedRU: 0",
            "AvailableRU: TotalRackUnits@ - UsedRU@"
        ]);
    }
    
    public void AddEquipment(RackEquipmentConcept equipment)
    {
        AddSubComponent(equipment);  // Hierarchical relationship
        // Update utilization calculations...
    }
}
```

**Parameters:**
- `TotalRackUnits`: 40 RU capacity
- `TotalHeight`: Calculated 70 inches
- `EquipmentCount`: Number of devices installed
- `UsedRU`: Total rack units consumed
- `AvailableRU`: Remaining capacity
- `HasPDU`: Boolean for PDU presence

### DataCenterRackModel

```csharp
public class DataCenterRackModel : PartComponent
{
    public DataCenterRackModel(string name = "DataCenter")
    {
        Calculations([
            "CabinetSpacing: 25.0",
            "TotalCabinets: 4",
            "TotalEquipment: {totalEquip}",
            "TotalRUUsed: {totalRU}",
            "TotalRUAvailable: {totalAvail}"
        ]);
        
        // Add all 4 cabinets as subcomponents
        AddSubComponent(new MFCabinet1Concept());
        AddSubComponent(new MFCabinet2Concept());
        AddSubComponent(new MFCabinet3Concept());
        AddSubComponent(new MFCabinet4Concept());
    }
}
```

**Aggregated Parameters:**
- `TotalEquipment`: Sum of all equipment across cabinets
- `TotalRUUsed`: Total rack units consumed
- `TotalRUAvailable`: Total available space
- `CabinetSpacing`: Distance between cabinets

## Equipment Types

### Standard Equipment

| Type | Height | Color | Special Parameters |
|------|--------|-------|-------------------|
| BlankingPanel | 1 RU | gray | - |
| ZIFConnector | 4 RU | gray | SlotCount: 8 |
| HPL2Module | 3 RU | purple | - |
| ITC | 4 RU | lightgreen | - |
| SPPMC | 3 RU | coral | - |
| GWT | 4 RU | steelblue | - |
| FHPC | 4 RU | forestgreen | - |
| StorageDrawer | 4 RU | mediumpurple | HasHandle: true |
| ITA | 8 RU | darkslateblue/hotpink | DeviceNumber, Label |
| LPAS | 4 RU | plum | - |

## Usage Examples

### Creating a Knowledge Model

```csharp
// Create data center model with all cabinets
var dataCenterModel = new DataCenterRackModel("MF_DataCenter");

// Access parameters
var totalEquip = dataCenterModel.FindParameter("TotalEquipment")?.GetValue().AsNumber();
var totalRU = dataCenterModel.FindParameter("TotalRUUsed")?.GetValue().AsNumber();
```

### Generating FO Geometry

```csharp
// Generate FO stage from knowledge model
var stage = RackKnowledgeToFoFactory.GenerateDataCenter(dataCenterModel);

// Add to canvas
foreach (var shape in stage.Shapes)
{
    canvas3D.Stage.AddShape(shape);
}
```

### Custom Equipment Placement

```csharp
// Create custom cabinet
var customCabinet = new RackCabinetConcept("Custom_Rack");

// Add equipment at specific positions
var server = new ITCConcept("WebServer");
server.SetPosition(10);  // Start at RU 10
customCabinet.AddEquipment(server);

var storage = new StorageDrawerConcept("Backup_Storage");
storage.SetPosition(15);  // Start at RU 15
customCabinet.AddEquipment(storage);

// Check utilization
var used = customCabinet.FindParameter("UsedRU")?.GetValue().AsNumber();
var available = customCabinet.FindParameter("AvailableRU")?.GetValue().AsNumber();
```

## Calculation System

### Parameter References

Use `@` suffix to reference other parameters in calculations:

```csharp
Calculations([
    "Width: 19.0",
    "Height: 70.0",
    "Area: Width@ * Height@"  // References Width and Height
]);
```

### Aggregation

Use ModelComponents<> to access subcomponents:

```csharp
var cabinets = dataCenter.ModelComponents<RackCabinetConcept>();
var totalRU = cabinets.Sum(c => c.FindParameter("UsedRU")?.GetValue().AsNumber() ?? 0);
```

## FO Factory Pattern

### Conversion Process

1. **Read Knowledge Model**: Extract parameters from PartComponent
2. **Create FO Shape**: Instantiate corresponding FO object
3. **Apply Parameters**: Set dimensions, colors, positions
4. **Build Hierarchy**: Preserve parent-child relationships
5. **Return Stage**: Complete FO scene ready for rendering

### Example Conversion

```csharp
public static RackCabinetShape GenerateCabinet(RackCabinetConcept concept)
{
    // Read parameters from knowledge model
    var name = concept.Name;
    var hasPDU = concept.FindParameter("HasPDU")?.GetValue().AsBoolean() ?? false;
    
    // Create FO shape
    var cabinet = new RackCabinetShape(name, hasPDU);
    
    // Generate equipment from subcomponents
    var equipment = concept.ModelComponents<RackEquipmentConcept>();
    foreach (var equipConcept in equipment)
    {
        var equipShape = GenerateEquipment(equipConcept);
        var startRU = (int)(equipConcept.FindParameter("StartRU")?.GetValue().AsNumber() ?? 1);
        cabinet.AddEquipment(equipShape, startRU);
    }
    
    return cabinet;
}
```

## Benefits

### Declarative Design
- Parameters define "what" not "how"
- Calculations express relationships
- Easy to modify and understand

### Separation of Concerns
- Knowledge model: Business logic and parameters
- FO objects: Rendering and geometry
- Factory: Conversion between layers

### Testability
- Parameters can be validated
- Calculations can be verified
- No rendering required for testing

### Reusability
- Equipment types defined once
- Cabinet configurations composable
- Factory logic generic

## Integration with UI

The knowledge model integrates with the AgentCanvasIntegration page:

```csharp
protected async Task RackEquip_CreateFromKnowledgeModel()
{
    // Create knowledge model
    var dataCenterModel = new DataCenterRackModel("MF_DataCenter");
    
    // Get summary
    rackKnowledgeModelSummary = RackKnowledgeToFoFactory.GetKnowledgeModelSummary(dataCenterModel);
    
    // Generate FO objects
    var dataCenter = RackKnowledgeToFoFactory.GenerateDataCenter(dataCenterModel);
    
    // Add to stage
    foreach (var shape in dataCenter.Shapes)
    {
        stage.AddShape(shape);
    }
}
```

## Future Enhancements

### Possible Extensions
- Unit conversion (RU ↔ mm ↔ in) using FoundryRulesAndUnits
- Validation rules (equipment must fit in cabinet)
- Power consumption tracking
- Weight distribution calculations
- Thermal analysis parameters
- Network topology modeling
- Cost estimation

### Advanced Patterns
- Dynamic equipment addition/removal
- Constraint checking (anti-collision)
- Optimization algorithms (packing efficiency)
- What-if scenario analysis
- Time-series capacity planning

## Reference Files

- `RackEquipmentKnowledge.cs`: Knowledge model definitions
- `RackKnowledgeToFoFactory.cs`: FO generation factory
- `RackEquipmentShape.cs`: FO object implementations
- `RackCabinetShape.cs`: Cabinet FO objects
- `AgentCanvasIntegration.razor.cs`: UI integration

## See Also

- `BoxWithDimensions.cs`: Example PartComponent pattern
- `MentorPlayground.cs`: DoRackConcept reference
- `RACK_EQUIPMENT_PATTERN.md`: FO object documentation
