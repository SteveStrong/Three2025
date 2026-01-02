using FoundryMentorModeler.Model;

namespace Three2025.Apprentice.RackEquipment;

/// <summary>
/// Base knowledge model for rack-mounted equipment using rack units (RU).
/// 1 RU = 1.75 inches = 44.45 mm
/// </summary>
public abstract class RackEquipmentConcept : PartComponent
{
    protected RackEquipmentConcept(string name, double defaultHeightRU, string defaultColor) : base(name)
    {
        // Define rack unit standards and equipment parameters
        Calculations([
            "StandardRackWidth|in: 19",                               // 19" standard
            "StandardDepth|in: 20",                                   // 20" typical depth
            $"HeightRU|RU: {defaultHeightRU}",                        // Height in rack units
            "Width|in: StandardRackWidth@",                           // Width in inches
            "Depth|in: StandardDepth@",                               // Depth in inches
            $"Color: '{defaultColor}'",                               // Equipment color
            "HeightInches|in: HeightRU@",                             // Auto-converts from RU to inches
            "StartRU: 1",                                             // Default to bottom
            "EndRU: StartRU@ + HeightRU@ - 1"                         // End position
        ]);
    }
    
    /// <summary>
    /// Set the starting rack unit position
    /// </summary>
    public void SetPosition(int startRU)
    {
        Calculations([$"StartRU: {startRU}"]);
    }
}

/// <summary>
/// Knowledge model for a 40U rack cabinet
/// </summary>
public class RackCabinetConcept : PartComponent
{
    public RackCabinetConcept(string name = "RackCabinet") : base(name)
    {
        Calculations([
            "TotalRackUnits|RU: 40",
            "RackWidth|in: 19",
            "RackDepth|in: 20",
            "HasPDU: false",
            "TotalHeight|in: TotalRackUnits@",   // Auto-converts 40 RU to 70 inches
            "RailThickness|in: 0.5",
            "RailDepth|in: 0.5",
            
            // Lazy formulas - automatically recalculate when equipment changes
            "equipment: SUBCOMPONENTS()",
            "equipmentCount: COUNT(equipment@)",
            "heights: COLLECT(equipment@, 'HeightRU')",
            "usedRU|RU: SUM(heights@)",
            "availableRU|RU: TotalRackUnits@ - usedRU@"
        ]);
    }
    
    /// <summary>
    /// Add equipment to the cabinet - formulas will automatically recalculate
    /// </summary>
    public void AddEquipment(RackEquipmentConcept equipment)
    {
        // Just add as subcomponent - formulas handle the rest
        AddSubComponent(equipment);
    }
}

// ============================================
// SPECIFIC EQUIPMENT TYPE CONCEPTS
// ============================================

public class BlankingPanelConcept : RackEquipmentConcept
{
    public BlankingPanelConcept(string name = "BlankingPanel", double heightRU = 1) 
        : base(name, heightRU, "gray")
    {
    }
}

public class ZIFConnectorConcept : RackEquipmentConcept
{
    public ZIFConnectorConcept(string name = "ZIFConnector", double heightRU = 4) 
        : base(name, heightRU, "gray")
    {
        Calculations(["SlotCount: 8"]);
    }
}

public class HPL2ModuleConcept : RackEquipmentConcept
{
    public HPL2ModuleConcept(string name = "HPL2", double heightRU = 3) 
        : base(name, heightRU, "purple")
    {
    }
}

public class ITCConcept : RackEquipmentConcept
{
    public ITCConcept(string name = "ITC", double heightRU = 4) 
        : base(name, heightRU, "lightgreen")
    {
    }
}

public class SPPMCConcept : RackEquipmentConcept
{
    public SPPMCConcept(string name = "SPPMC", double heightRU = 3) 
        : base(name, heightRU, "coral")
    {
    }
}

public class GWTConcept : RackEquipmentConcept
{
    public GWTConcept(string name = "GWT", double heightRU = 4) 
        : base(name, heightRU, "steelblue")
    {
    }
}

public class FHPCConcept : RackEquipmentConcept
{
    public FHPCConcept(string name = "FHPC", double heightRU = 4) 
        : base(name, heightRU, "forestgreen")
    {
    }
}

public class StorageDrawerConcept : RackEquipmentConcept
{
    public StorageDrawerConcept(string name = "StorageDrawer", double heightRU = 4) 
        : base(name, heightRU, "mediumpurple")
    {
        Calculations(["HasHandle: true"]);
    }
}

public class ITAConcept : RackEquipmentConcept
{
    public ITAConcept(string name = "ITA", int number = 1, double heightRU = 8) 
        : base(name, heightRU, number == 1 ? "darkslateblue" : "hotpink")
    {
        Calculations([
            $"DeviceNumber: {number}",
            $"Label: 'ITA {number}'"
        ]);
    }
}

public class LPASConcept : RackEquipmentConcept
{
    public LPASConcept(string name = "LPAS", double heightRU = 4) 
        : base(name, heightRU, "plum")
    {
    }
}

/// <summary>
/// Complete MF Cabinet models with equipment layout
/// </summary>
public class MFCabinet1Concept : RackCabinetConcept
{
    public MFCabinet1Concept(string name = "MF_Cabinet_1") : base(name)
    {
        Calculations(["HasPDU: true"]);
        
        // Add equipment at specific RU positions
        var blanking = new BlankingPanelConcept("BlankingPanel");
        blanking.SetPosition(1);  // RU 1-4
        AddEquipment(blanking);
        
        var hpl2 = new HPL2ModuleConcept("HPL2");
        hpl2.SetPosition(5);  // RU 5-7
        AddEquipment(hpl2);
        
        var itc = new ITCConcept("ITC");
        itc.SetPosition(10);  // RU 10-13
        AddEquipment(itc);
        
        // Add 3 storage drawers
        var storage1 = new StorageDrawerConcept("Storage_1");
        storage1.SetPosition(17);  // RU 17-20
        AddEquipment(storage1);
        
        var storage2 = new StorageDrawerConcept("Storage_2");
        storage2.SetPosition(21);  // RU 21-24
        AddEquipment(storage2);
        
        var storage3 = new StorageDrawerConcept("Storage_3");
        storage3.SetPosition(26);  // RU 26-29
        AddEquipment(storage3);
    }
}

public class MFCabinet2Concept : RackCabinetConcept
{
    public MFCabinet2Concept(string name = "MF_Cabinet_2") : base(name)
    {
        Calculations(["HasPDU: true"]);
        
        var zif = new ZIFConnectorConcept("ZIF_Connectors");
        zif.SetPosition(1);  // RU 1-4
        AddEquipment(zif);
        
        var sppmc = new SPPMCConcept("SPPMC");
        sppmc.SetPosition(8);  // RU 8-10
        AddEquipment(sppmc);
        
        var gwt = new GWTConcept("GWT");
        gwt.SetPosition(11);  // RU 11-14
        AddEquipment(gwt);
    }
}

public class MFCabinet3Concept : RackCabinetConcept
{
    public MFCabinet3Concept(string name = "MF_Cabinet_3") : base(name)
    {
        Calculations(["HasPDU: true"]);
        
        var blanking = new BlankingPanelConcept("BlankingPanel");
        blanking.SetPosition(1);
        AddEquipment(blanking);
        
        var sppmc = new SPPMCConcept("SPPMC");
        sppmc.SetPosition(8);
        AddEquipment(sppmc);
        
        var fhpc = new FHPCConcept("FHPC");
        fhpc.SetPosition(12);
        AddEquipment(fhpc);
        
        // Add 4 LPAS units
        for (int i = 0; i < 4; i++)
        {
            var lpas = new LPASConcept($"LPAS_{i + 1}");
            lpas.SetPosition(23 + (i * 4));  // RU 23, 28, 32, 36
            AddEquipment(lpas);
        }
    }
}

public class MFCabinet4Concept : RackCabinetConcept
{
    public MFCabinet4Concept(string name = "MF_Cabinet_4") : base(name)
    {
        Calculations(["HasPDU: false"]);  // No PDU
        
        var zif = new ZIFConnectorConcept("ZIF_Connectors");
        zif.SetPosition(1);
        AddEquipment(zif);
        
        var ita1 = new ITAConcept("ITA_1", 1);
        ita1.SetPosition(10);  // RU 10-17 (8U)
        AddEquipment(ita1);
        
        var ita2 = new ITAConcept("ITA_2", 2);
        ita2.SetPosition(21);  // RU 21-28 (8U)
        AddEquipment(ita2);
    }
}

/// <summary>
/// Complete data center model with all 4 MF cabinets
/// </summary>
public class DataCenterRackModel : PartComponent
{
    public DataCenterRackModel(string name = "DataCenter") : base(name)
    {
        Calculations([
            "CabinetSpacing|in: 25",
            "TotalCabinets: 4",
            
            // Lazy formulas - automatically aggregate from all cabinet subcomponents
            "cabinets: SUBCOMPONENTS('RackCabinetConcept')",
            "equipmentCounts: COLLECT(cabinets@, 'equipmentCount')",
            "TotalEquipment: SUM(equipmentCounts@)",
            "usedRUs: COLLECT(cabinets@, 'usedRU')",
            "TotalRUUsed|RU: SUM(usedRUs@)",
            "TotalRUAvailable|RU: (TotalCabinets@ * 40) - TotalRUUsed@"
        ]);
        
        // Create all 4 cabinets - formulas will automatically aggregate
        var cabinet1 = new MFCabinet1Concept("MF_Cabinet_1");
        AddSubComponent(cabinet1);
        
        var cabinet2 = new MFCabinet2Concept("MF_Cabinet_2");
        AddSubComponent(cabinet2);
        
        var cabinet3 = new MFCabinet3Concept("MF_Cabinet_3");
        AddSubComponent(cabinet3);
        
        var cabinet4 = new MFCabinet4Concept("MF_Cabinet_4");
        AddSubComponent(cabinet4);
    }
}
