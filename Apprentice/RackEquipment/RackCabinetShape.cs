using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings;

namespace Three2025.Apprentice.RackEquipment;

/// <summary>
/// Represents a standard 19" rack cabinet with 40U capacity.
/// Manages equipment placement and positioning using rack unit calculations.
/// </summary>
public class RackCabinetShape : FoShape3D
{
    public const int TOTAL_RACK_UNITS = 40;
    public const double CABINET_WIDTH = RackEquipmentShape.RACK_WIDTH_INCHES;
    public const double CABINET_DEPTH = RackEquipmentShape.STANDARD_DEPTH_INCHES;
    
    /// <summary>
    /// Total height of the cabinet in inches (40 RU)
    /// </summary>
    public double TotalHeight => TOTAL_RACK_UNITS * RackEquipmentShape.RU_HEIGHT_INCHES;
    
    /// <summary>
    /// Does this cabinet have a PDU (Power Distribution Unit)?
    /// </summary>
    public bool HasPDU { get; set; }
    
    /// <summary>
    /// Formatter showing cabinet name and capacity
    /// </summary>
    public static new readonly Func<FoBase, string> DefaultFormatter = g => 
    {
        if (g is RackCabinetShape cabinet)
        {
            var pduStatus = cabinet.HasPDU ? "with PDU" : "no PDU";
            var equipCount = cabinet.GetEquipment().Count;
            return $"{cabinet.Key} [{TOTAL_RACK_UNITS}U, {equipCount} devices, {pduStatus}]";
        }
        return g.Key;
    };
    
    public RackCabinetShape(string name, bool hasPDU = false) : base(name)
    {
        HasPDU = hasPDU;
        ComputeTreeNodeTitle = DefaultFormatter;
        
        // Create cabinet frame (rails only, not solid box)
        CreateCabinetFrame();
        
        // Add PDU if specified
        if (HasPDU)
        {
            AddPDU();
        }
    }
    
    private void CreateCabinetFrame()
    {
        // Create vertical rails (left and right)
        var railThickness = 0.5;
        
        // Left rail
        var leftRail = new FoShape3D("LeftRail");
        leftRail.CreateBox("LeftRail", railThickness, TotalHeight, CABINET_DEPTH);
        leftRail.Color = "dimgray";
        leftRail.Transform = new Transform3("LeftRail_Transform")
        {
            Position = new Vector3(-CABINET_WIDTH/2 - railThickness/2, TotalHeight/2, 0)
        };
        AddShape(leftRail);
        
        // Right rail
        var rightRail = new FoShape3D("RightRail");
        rightRail.CreateBox("RightRail", railThickness, TotalHeight, CABINET_DEPTH);
        rightRail.Color = "dimgray";
        rightRail.Transform = new Transform3("RightRail_Transform")
        {
            Position = new Vector3(CABINET_WIDTH/2 + railThickness/2, TotalHeight/2, 0)
        };
        AddShape(rightRail);
        
        // Add rack unit markers (every 5U for reference)
        AddRackUnitMarkers();
    }
    
    private void AddRackUnitMarkers()
    {
        for (int ru = 5; ru <= TOTAL_RACK_UNITS; ru += 5)
        {
            var marker = new FoText3D($"RU_{ru}", "yellow")
            {
                Text = $"{ru}",
                Transform = new Transform3($"RU_{ru}_Transform")
                {
                    Position = new Vector3(-CABINET_WIDTH/2 - 1.5, ru * RackEquipmentShape.RU_HEIGHT_INCHES, CABINET_DEPTH/2 + 1),
                    Scale = new Vector3(0.3, 0.3, 0.3)
                }
            };
            AddShape(marker);
        }
    }
    
    private void AddPDU()
    {
        // PDU mounted on the right side (vertical)
        var pdu = new FoShape3D("PDU");
        pdu.CreateBox("PDU", 2.0, TotalHeight * 0.9, 4.0);
        pdu.Color = "red";
        pdu.Transform = new Transform3("PDU_Transform")
        {
            Position = new Vector3(CABINET_WIDTH/2 + 2.5, TotalHeight/2, 0)
        };
        AddShape(pdu);
    }
    
    /// <summary>
    /// Add equipment to the rack at a specific RU position
    /// </summary>
    public void AddEquipment(RackEquipmentShape equipment, int startRU)
    {
        equipment.StartRU = startRU;
        
        // Calculate Y position (cabinet bottom is at Y=0)
        double yPos = RackEquipmentShape.CalculateYPosition(startRU, equipment.HeightInRU);
        
        equipment.Transform = new Transform3($"{equipment.Key}_Transform")
        {
            Position = new Vector3(0, yPos, 0)
        };
        
        AddShape(equipment);
    }
    
    /// <summary>
    /// Get all equipment in this cabinet
    /// </summary>
    public List<RackEquipmentShape> GetEquipment()
    {
        return AllSubGlyph3Ds()
            .Where(x => x is RackEquipmentShape)
            .Cast<RackEquipmentShape>()
            .OrderBy(e => e.StartRU)
            .ToList();
    }
    
    /// <summary>
    /// Check if there's a collision between equipment at given positions
    /// </summary>
    public bool HasCollision(int startRU1, int endRU1, int startRU2, int endRU2)
    {
        return !(endRU1 < startRU2 || endRU2 < startRU1);
    }
    
    /// <summary>
    /// Calculate available space in the rack
    /// </summary>
    public int GetAvailableRU()
    {
        int usedRU = 0;
        foreach (var equip in GetEquipment())
        {
            usedRU += (int)Math.Ceiling(equip.HeightInRU);
        }
        return TOTAL_RACK_UNITS - usedRU;
    }
}

/// <summary>
/// Factory class for creating the MF Cabinet configurations
/// </summary>
public static class MFCabinetFactory
{
    /// <summary>
    /// Create MF Cabinet 1 with exact equipment layout from image
    /// </summary>
    public static RackCabinetShape CreateMFCabinet1()
    {
        var cabinet = new RackCabinetShape("MF_Cabinet_1", hasPDU: true);
        
        // From image analysis (bottom to top):
        cabinet.AddEquipment(new BlankingPanelShape("BlankingPanel"), startRU: 1);         // RU 1-4
        cabinet.AddEquipment(new HPL2ModuleShape("HPL2"), startRU: 5);                      // RU 5-7
        cabinet.AddEquipment(new ITCShape("ITC"), startRU: 10);                             // RU 10-13
        cabinet.AddEquipment(new StorageDrawerShape("Storage_1"), startRU: 17);             // RU 17-20
        cabinet.AddEquipment(new StorageDrawerShape("Storage_2"), startRU: 21);             // RU 21-24
        cabinet.AddEquipment(new StorageDrawerShape("Storage_3"), startRU: 26);             // RU 26-29
        cabinet.AddEquipment(new RSIShape("RSI"), startRU: 32);                             // RU 32-35
        cabinet.AddEquipment(new IPSShape("IPS"), startRU: 37);                             // RU 37
        cabinet.AddEquipment(new MCCShape("MCC"), startRU: 38);                             // RU 38-40
        
        return cabinet;
    }
    
    /// <summary>
    /// Create MF Cabinet 2 with exact equipment layout from image
    /// </summary>
    public static RackCabinetShape CreateMFCabinet2()
    {
        var cabinet = new RackCabinetShape("MF_Cabinet_2", hasPDU: true);
        
        // From image analysis (bottom to top):
        cabinet.AddEquipment(new ZIFConnectorShape("ZIF_Connectors"), startRU: 1);         // RU 1-4
        cabinet.AddEquipment(new SPPMCShape("SPPMC"), startRU: 8);                          // RU 8-10
        cabinet.AddEquipment(new GWTShape("GWT"), startRU: 11);                             // RU 11-14
        cabinet.AddEquipment(new DCTShape("DCT"), startRU: 17);                             // RU 17
        cabinet.AddEquipment(new PDCSShape("PDCS"), startRU: 23);                           // RU 23
        cabinet.AddEquipment(new PDSAShape("PDSA_1"), startRU: 25);                         // RU 25
        cabinet.AddEquipment(new PDSAShape("PDSA_2"), startRU: 26);                         // RU 26
        cabinet.AddEquipment(new PDSAShape("PDSA_3"), startRU: 27);                         // RU 27
        cabinet.AddEquipment(new PDSAShape("PDSA_4"), startRU: 28);                         // RU 28
        
        return cabinet;
    }
    
    /// <summary>
    /// Create MF Cabinet 3 with exact equipment layout from image
    /// </summary>
    public static RackCabinetShape CreateMFCabinet3()
    {
        var cabinet = new RackCabinetShape("MF_Cabinet_3", hasPDU: true);
        
        // From image analysis (bottom to top):
        cabinet.AddEquipment(new BlankingPanelShape("BlankingPanel"), startRU: 1);         // RU 1-4
        cabinet.AddEquipment(new SPPMCShape("SPPMC"), startRU: 8);                          // RU 8-10
        cabinet.AddEquipment(new FHPCShape("FHPC"), startRU: 12);                           // RU 12-15
        cabinet.AddEquipment(new TPPMCShape("TPPMC"), startRU: 17);                         // RU 17-19
        cabinet.AddEquipment(new HPDSShape("HPDS"), startRU: 21);                           // RU 21-22
        cabinet.AddEquipment(new LPASShape("LPAS_1"), startRU: 23);                         // RU 23-26
        cabinet.AddEquipment(new LPASShape("LPAS_2"), startRU: 28);                         // RU 28-31
        cabinet.AddEquipment(new LPASShape("LPAS_3"), startRU: 32);                         // RU 32-35
        cabinet.AddEquipment(new LPASShape("LPAS_4"), startRU: 36);                         // RU 36-39
        
        return cabinet;
    }
    
    /// <summary>
    /// Create MF Cabinet 4 with exact equipment layout from image
    /// </summary>
    public static RackCabinetShape CreateMFCabinet4()
    {
        var cabinet = new RackCabinetShape("MF_Cabinet_4", hasPDU: false);
        
        // From image analysis (bottom to top):
        cabinet.AddEquipment(new ZIFConnectorShape("ZIF_Connectors"), startRU: 1);         // RU 1-4
        cabinet.AddEquipment(new ITAShape("ITA_1", 1), startRU: 10);                        // RU 10-17
        cabinet.AddEquipment(new ITAShape("ITA_2", 2), startRU: 21);                        // RU 21-28
        cabinet.AddEquipment(new ASMShape("ASM"), startRU: 32);                             // RU 32-34
        cabinet.AddEquipment(new ASGShape("ASG"), startRU: 35);                             // RU 35-36
        cabinet.AddEquipment(new LPMShape("LPM"), startRU: 39);                             // RU 39-40
        
        return cabinet;
    }
    
    /// <summary>
    /// Create all 4 MF cabinets with spacing between them
    /// </summary>
    public static List<RackCabinetShape> CreateAllMFCabinets(double spacing = 5.0)
    {
        var cabinets = new List<RackCabinetShape>();
        
        var cabinet1 = CreateMFCabinet1();
        cabinet1.Transform = new Transform3("Cabinet1_Transform")
        {
            Position = new Vector3(-spacing * 1.5, 0, 0)
        };
        cabinets.Add(cabinet1);
        
        var cabinet2 = CreateMFCabinet2();
        cabinet2.Transform = new Transform3("Cabinet2_Transform")
        {
            Position = new Vector3(-spacing * 0.5, 0, 0)
        };
        cabinets.Add(cabinet2);
        
        var cabinet3 = CreateMFCabinet3();
        cabinet3.Transform = new Transform3("Cabinet3_Transform")
        {
            Position = new Vector3(spacing * 0.5, 0, 0)
        };
        cabinets.Add(cabinet3);
        
        var cabinet4 = CreateMFCabinet4();
        cabinet4.Transform = new Transform3("Cabinet4_Transform")
        {
            Position = new Vector3(spacing * 1.5, 0, 0)
        };
        cabinets.Add(cabinet4);
        
        return cabinets;
    }
}
