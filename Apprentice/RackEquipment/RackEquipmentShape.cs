using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Models;
using FoundryWorldsAndDrawings;

namespace Three2025.Apprentice.RackEquipment;

/// <summary>
/// Base class for all rack-mounted equipment using standard rack units (RU).
/// 1 RU = 1.75 inches = 44.45 mm
/// Standard rack width = 19 inches
/// </summary>
public abstract class RackEquipmentShape : FoShape3D
{
    // Standard rack unit dimensions
    public const double RU_HEIGHT_INCHES = 1.75;
    public const double RACK_WIDTH_INCHES = 19.0;
    public const double STANDARD_DEPTH_INCHES = 20.0;
    
    /// <summary>
    /// Height of this equipment in rack units
    /// </summary>
    public double HeightInRU { get; protected set; }
    
    /// <summary>
    /// Starting rack unit position (1-based, from bottom)
    /// </summary>
    public int StartRU { get; set; }
    
    /// <summary>
    /// Ending rack unit position (inclusive)
    /// </summary>
    public int EndRU => StartRU + (int)Math.Ceiling(HeightInRU) - 1;
    
    /// <summary>
    /// Formatter showing equipment name, RU size, and position
    /// </summary>
    public static new readonly Func<FoBase, string> DefaultFormatter = g => 
    {
        if (g is RackEquipmentShape equip)
        {
            return $"{equip.Key} ({equip.HeightInRU}U) @ RU {equip.StartRU}-{equip.EndRU}";
        }
        return g.Key;
    };
    
    protected RackEquipmentShape(string name, double heightInRU, string color = "gray") : base(name)
    {
        HeightInRU = heightInRU;
        Color = color;
        ComputeTreeNodeTitle = DefaultFormatter;
    }
    
    /// <summary>
    /// Calculate actual height in inches from rack units
    /// </summary>
    protected double CalculateHeightInches() => HeightInRU * RU_HEIGHT_INCHES;
    
    /// <summary>
    /// Calculate Y position for equipment at a given RU (measured from cabinet bottom = RU 1)
    /// </summary>
    public static double CalculateYPosition(int startRU, double heightInRU)
    {
        // Position center of equipment at the center of its RU span
        double centerRU = startRU + (heightInRU / 2.0) - 0.5;
        return centerRU * RU_HEIGHT_INCHES;
    }
    
    /// <summary>
    /// Create the basic equipment box with standard dimensions
    /// </summary>
    protected void CreateEquipmentBox(double width = RACK_WIDTH_INCHES, double depth = STANDARD_DEPTH_INCHES)
    {
        double height = CalculateHeightInches();
        CreateBox(Key, width, height, depth);
    }
}

// ============================================
// BLANKING PANELS
// ============================================

public class BlankingPanelShape : RackEquipmentShape
{
    public BlankingPanelShape(string name, double heightInRU = 1) 
        : base(name, heightInRU, "gray")
    {
        CreateEquipmentBox();
    }
}

// ============================================
// CONNECTORS
// ============================================

public class ZIFConnectorShape : RackEquipmentShape
{
    public ZIFConnectorShape(string name, double heightInRU = 4) 
        : base(name, heightInRU, "gray")
    {
        CreateEquipmentBox();
        AddConnectorIndicators();
    }
    
    private void AddConnectorIndicators()
    {
        // Add visual indicators for ZIF connector slots
        int slotCount = 8;
        double spacing = RACK_WIDTH_INCHES / (slotCount + 1);
        
        for (int i = 0; i < slotCount; i++)
        {
            var slot = new FoShape3D($"Slot_{i + 1}");
            slot.CreateBox($"Slot_{i + 1}", 0.5, CalculateHeightInches() * 0.8, 0.2);
            slot.Color = "darkgray";
            slot.Transform = new Transform3($"Slot_{i + 1}_Transform")
            {
                Position = new Vector3(-RACK_WIDTH_INCHES/2 + spacing * (i + 1), 0, STANDARD_DEPTH_INCHES/2 + 0.1)
            };
            AddShape(slot);
        }
    }
}

// ============================================
// MODULES
// ============================================

public class HPL2ModuleShape : RackEquipmentShape
{
    public HPL2ModuleShape(string name, double heightInRU = 3) 
        : base(name, heightInRU, "purple")
    {
        CreateEquipmentBox();
    }
}

public class ITCShape : RackEquipmentShape
{
    public ITCShape(string name, double heightInRU = 4) 
        : base(name, heightInRU, "lightgreen")
    {
        CreateEquipmentBox();
    }
}

public class SPPMCShape : RackEquipmentShape
{
    public SPPMCShape(string name, double heightInRU = 3) 
        : base(name, heightInRU, "coral")
    {
        CreateEquipmentBox();
    }
}

public class GWTShape : RackEquipmentShape
{
    public GWTShape(string name, double heightInRU = 4) 
        : base(name, heightInRU, "steelblue")
    {
        CreateEquipmentBox();
    }
}

public class DCTShape : RackEquipmentShape
{
    public DCTShape(string name, double heightInRU = 1) 
        : base(name, heightInRU, "coral")
    {
        CreateEquipmentBox();
    }
}

public class PDCSShape : RackEquipmentShape
{
    public PDCSShape(string name, double heightInRU = 1) 
        : base(name, heightInRU, "darkseagreen")
    {
        CreateEquipmentBox();
    }
}

public class PDSAShape : RackEquipmentShape
{
    public PDSAShape(string name, double heightInRU = 1) 
        : base(name, heightInRU, "darkseagreen")
    {
        CreateEquipmentBox();
    }
}

public class FHPCShape : RackEquipmentShape
{
    public FHPCShape(string name, double heightInRU = 4) 
        : base(name, heightInRU, "forestgreen")
    {
        CreateEquipmentBox();
    }
}

public class TPPMCShape : RackEquipmentShape
{
    public TPPMCShape(string name, double heightInRU = 3) 
        : base(name, heightInRU, "coral")
    {
        CreateEquipmentBox();
    }
}

public class HPDSShape : RackEquipmentShape
{
    public HPDSShape(string name, double heightInRU = 2) 
        : base(name, heightInRU, "lightgreen")
    {
        CreateEquipmentBox();
    }
}

public class LPASShape : RackEquipmentShape
{
    public LPASShape(string name, double heightInRU = 4) 
        : base(name, heightInRU, "plum")
    {
        CreateEquipmentBox();
    }
}

public class ITAShape : RackEquipmentShape
{
    public ITAShape(string name, int number, double heightInRU = 8) 
        : base(name, heightInRU, number == 1 ? "darkslateblue" : "hotpink")
    {
        CreateEquipmentBox();
        
        // Add ITA number label
        var label = new FoText3D($"ITA_{number}_Label", "white")
        {
            Text = $"ITA {number}",
            Transform = new Transform3($"Label_Transform")
            {
                Position = new Vector3(0, 0, STANDARD_DEPTH_INCHES/2 + 0.1),
                Scale = new Vector3(0.5, 0.5, 0.5)
            }
        };
        AddShape(label);
    }
}

public class ASMShape : RackEquipmentShape
{
    public ASMShape(string name, double heightInRU = 3) 
        : base(name, heightInRU, "lightgreen")
    {
        CreateEquipmentBox();
    }
}

public class ASGShape : RackEquipmentShape
{
    public ASGShape(string name, double heightInRU = 2) 
        : base(name, heightInRU, "plum")
    {
        CreateEquipmentBox();
    }
}

public class LPMShape : RackEquipmentShape
{
    public LPMShape(string name, double heightInRU = 3) 
        : base(name, heightInRU, "lightgreen")
    {
        CreateEquipmentBox();
    }
}

// ============================================
// STORAGE & SPECIAL
// ============================================

public class StorageDrawerShape : RackEquipmentShape
{
    public StorageDrawerShape(string name, double heightInRU = 4) 
        : base(name, heightInRU, "mediumpurple")
    {
        CreateEquipmentBox();
        AddDrawerHandle();
    }
    
    private void AddDrawerHandle()
    {
        var handle = new FoShape3D("Handle");
        handle.CreateCylinder("Handle", 0.3, 0.3, RACK_WIDTH_INCHES * 0.6);
        handle.Color = "silver";
        handle.Transform = new Transform3("Handle_Transform")
        {
            Position = new Vector3(0, 0, STANDARD_DEPTH_INCHES/2 + 0.2),
            Rotation = new Euler(0, 0, Math.PI / 2)
        };
        AddShape(handle);
    }
}

public class RSIShape : RackEquipmentShape
{
    public RSIShape(string name, double heightInRU = 4) 
        : base(name, heightInRU, "darkgreen")
    {
        CreateEquipmentBox();
    }
}

public class IPSShape : RackEquipmentShape
{
    public IPSShape(string name, double heightInRU = 1) 
        : base(name, heightInRU, "darkgray")
    {
        CreateEquipmentBox();
    }
}

public class MCCShape : RackEquipmentShape
{
    public MCCShape(string name, double heightInRU = 3) 
        : base(name, heightInRU, "black")
    {
        CreateEquipmentBox();
    }
}
