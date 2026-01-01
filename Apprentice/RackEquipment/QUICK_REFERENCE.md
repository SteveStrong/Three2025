# MF Rack Equipment - Quick Reference

## 🎯 What Was Created

A complete 19" rack-mounted equipment modeling system based on the MF Space Allocation image, using **FO objects** with **rack unit (RU) calculations**.

## 📁 Files Created/Modified

### New Files
1. **RackEquipmentShape.cs** - Base class + 20+ equipment types
2. **RackCabinetShape.cs** - Cabinet container + MFCabinetFactory
3. **RACK_EQUIPMENT_PATTERN.md** - Complete documentation

### Modified Files  
4. **QuickTestPanel.razor.cs** - Added 5 test methods
5. **QuickTestPanel.razor** - Added "🗄️ Rack Equipment" UI section

## 🗄️ Equipment Types (20+ Classes)

```csharp
// Connectors & Panels
BlankingPanelShape      // Space filler (1U+)
ZIFConnectorShape       // With visual slots (4U)

// Processing Modules
HPL2ModuleShape         // 3U, purple
SPPMCShape             // 3U, coral  
GWTShape               // 4U, steel blue
FHPCShape              // 4U, forest green
TPPMCShape             // 3U, coral
DCTShape               // 1U, coral

// Storage & Infrastructure
ITCShape               // 4U, light green
StorageDrawerShape     // 4U with handle, purple
RSIShape               // 4U, dark green
HPDSShape              // 2U, light green

// Power & Distribution
IPSShape               // 1U, dark gray
MCCShape               // 3U, black
PDCSShape              // 1U, dark sea green
PDSAShape              // 1U, dark sea green
LPASShape              // 4U, plum

// Computing
ITAShape               // 8U, labeled (ITA 1, ITA 2)
ASMShape               // 3U, light green
ASGShape               // 2U, plum
LPMShape               // 3U, light green
```

## 🏗️ Cabinet System

### RackCabinetShape
- **40 RU capacity** (70 inches tall)
- **19-inch width** (standard)
- **Optional PDU** (red side mount)
- **RU markers** every 5 units
- **Auto-positioning** from RU values

### Key Methods
```csharp
// Add equipment at specific RU
cabinet.AddEquipment(equipment, startRU: 10);

// Query equipment
var devices = cabinet.GetEquipment();
var freeSpace = cabinet.GetAvailableRU();

// Check collision
bool overlaps = cabinet.HasCollision(startRU1, endRU1, startRU2, endRU2);
```

## 📊 Cabinet Configurations

| Cabinet | PDU? | Devices | Free RU | Key Equipment |
|---------|------|---------|---------|---------------|
| **MF Cabinet 1** | ✅ | 9 | 5 RU | HPL2, ITC, 3× Storage, RSI, IPS, MCC |
| **MF Cabinet 2** | ✅ | 9 | 23 RU | ZIF, SPPMC, GWT, DCT, PDCS, 4× PDSA |
| **MF Cabinet 3** | ✅ | 9 | 3 RU | SPPMC, FHPC, TPPMC, HPDS, 4× LPAS |
| **MF Cabinet 4** | ❌ | 6 | 18 RU | ZIF, ITA 1 (8U), ITA 2 (8U), ASM, ASG, LPM |

**Total:** 4 cabinets, 33 devices

## 🚀 Usage Examples

### Create Individual Cabinet
```csharp
var cabinet1 = MFCabinetFactory.CreateMFCabinet1();
stage.AddShape(cabinet1);
```

### Create All Cabinets
```csharp
var cabinets = MFCabinetFactory.CreateAllMFCabinets(spacing: 25.0);
foreach (var cabinet in cabinets)
    stage.AddShape(cabinet);
```

### Custom Cabinet
```csharp
var custom = new RackCabinetShape("MyRack", hasPDU: true);
custom.AddEquipment(new SPPMCShape("Server1"), startRU: 1);
custom.AddEquipment(new GWTShape("Gateway1"), startRU: 4);
custom.AddEquipment(new StorageDrawerShape("Storage1"), startRU: 10);
```

## 🧪 Testing (QuickTestPanel)

Navigate to **🗄️ Rack Equipment: MF Cabinets** section:

**Buttons:**
- `Create All 4 MF Cabinets` - Complete layout
- `MF Cabinet 1-4` - Individual cabinets

**Test Methods:**
```csharp
Test_CreateAllMFCabinets()
Test_CreateMFCabinet1()
Test_CreateMFCabinet2()
Test_CreateMFCabinet3()
Test_CreateMFCabinet4()
```

## 📐 RU Calculations

### Constants
```csharp
const double RU_HEIGHT_INCHES = 1.75;
const double RACK_WIDTH_INCHES = 19.0;
const int TOTAL_RACK_UNITS = 40;
```

### Position Calculation
```csharp
// Equipment at RU 10-13 (4U height)
// Y position = (10 + 4/2 - 0.5) × 1.75 = 20.125 inches
double yPos = CalculateYPosition(startRU: 10, heightInRU: 4.0);
```

### RU Span
```csharp
equipment.StartRU = 10;          // Bottom of equipment
equipment.HeightInRU = 4.0;      // Size
equipment.EndRU = 10 + 4 - 1;    // Top = 13
```

## 🎨 Design Patterns

1. **Composite**: Cabinet contains Equipment contains Components
2. **Template Method**: Base class defines structure, subclasses customize
3. **Factory**: Pre-configured cabinet layouts
4. **Parameter Calculation**: RU-based automatic positioning

## 🔗 Pattern Inspiration

- **AudioPanelShape**: Composite shapes with positioned connectors
- **MobileRouterShape**: Complex device with labeled components  
- **FoRack**: Basic rack structure (enhanced with RU precision)

## ✅ Features Implemented

- ✅ RU-based sizing (1 RU = 1.75")
- ✅ Automatic Y positioning from RU values
- ✅ 20+ equipment types with accurate colors
- ✅ Cabinet rails + PDU mounting
- ✅ RU markers for visual reference
- ✅ Collision detection (RU overlap check)
- ✅ Space utilization tracking
- ✅ Factory pattern for MF cabinets 1-4
- ✅ Test panel integration
- ✅ Formatted equipment display

## 🚧 Future Extensions

### 1. Knowledge Model Integration
```csharp
var rackConcept = new KnConcept("DataCenterRack");
rackConcept.Units<KnVariable>("TotalRU: RU");
rackConcept.Calculations(["TotalRU: 40", "Width: 19"]);
```

### 2. Cable Routing (IBodyLink3D)
```csharp
var powerCable = new LinkShape("Power", "black", "Tube");
powerCable.FromShape3D = pdu;
powerCable.ToShape3D = server;
```

### 3. Thermal Modeling
```csharp
public double PowerWatts { get; set; } = 500;
public double ThermalBTU => PowerWatts * 3.412;
```

### 4. Network Topology
```csharp
var network = ConnectDevices(switch1, server1, "RJ45", "Cat6");
```

### 5. Unit System Integration
```csharp
var height = unitSystem.CreateLength(4, "RU");
var inches = height.As("in");  // 7.0
var mm = height.As("mm");      // 177.8
```

## 📝 Summary

**Approach Chosen:** ✅ **FO Objects with RU Parameters**

**Why not Knowledge Model?**
- FO objects are simpler for direct 3D visualization
- RU calculations are straightforward (1 RU = 1.75")
- Pattern matches AudioPanelShape/MobileRouterShape
- Can add KnModel layer later if needed

**Result:**
- 4 fully-configured cabinets
- 33 equipment pieces
- Precise RU-based positioning
- Test panel integration
- ~800 lines of clean, reusable code

## 🎯 Next Steps

1. **Test**: Run `Create All 4 MF Cabinets` in QuickTestPanel
2. **Customize**: Modify equipment colors/labels as needed
3. **Extend**: Add new equipment types following pattern
4. **Integrate**: Connect with cable routing system
5. **Document**: Add equipment specs (power, network ports, etc.)

---

**Pattern Origin:** AudioPanelShape + MobileRouterShape composite patterns  
**Industry Standard:** 19" rack, 1 RU = 1.75", 40U cabinet  
**Total Equipment Types:** 20+ classes  
**Total Cabinets:** 4 pre-configured MF layouts
