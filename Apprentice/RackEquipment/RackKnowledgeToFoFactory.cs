using FoundryMentorModeler.Model;
using FoundryWorldsAndDrawings.Shape;

namespace Three2025.Apprentice.RackEquipment;

/// <summary>
/// Factory to convert knowledge model (KnConcepts) to Foundry Objects (FO shapes)
/// </summary>
public static class RackKnowledgeToFoFactory
{
    /// <summary>
    /// Generate a complete data center with all cabinets from knowledge model.
    /// Technician operates on provided stage - does not instantiate modeling objects.
    /// </summary>
    public static FoStage3D GenerateDataCenter(DataCenterRackModel model, FoStage3D stage)
    {
        // Use provided stage - technician operates on context, doesn't create it
        
        // Get cabinet spacing
        var spacing = model.FindParameter("CabinetSpacing")?.GetValue().AsNumber() ?? 25.0;
        
        // Generate each cabinet
        var cabinets = model.ModelComponents<RackCabinetConcept>();
        for (int i = 0; i < cabinets.Count; i++)
        {
            var cabinetConcept = cabinets[i];
            var cabinetShape = GenerateCabinet(cabinetConcept);
            
            // Position cabinet along X axis
            cabinetShape.Transform.Position.X = i * spacing;
            
            stage.Add(cabinetShape);
        }
        
        return stage;
    }
    
    /// <summary>
    /// Generate a single cabinet from knowledge model
    /// </summary>
    public static RackCabinetShape GenerateCabinet(RackCabinetConcept concept)
    {
        var cabinetName = concept.Name;
        var hasPDU = concept.FindParameter("HasPDU")?.GetValue().AsBoolean() ?? false;
        
        var cabinet = new RackCabinetShape(cabinetName, hasPDU);
        
        // Generate equipment for this cabinet
        var equipmentConcepts = concept.ModelComponents<RackEquipmentConcept>();
        
        foreach (var equipConcept in equipmentConcepts)
        {
            var equipShape = GenerateEquipment(equipConcept);
            
            // Get position from concept
            var startRU = (int)(equipConcept.FindParameter("StartRU")?.GetValue().AsNumber() ?? 1);
            
            // Add to cabinet at specified position
            cabinet.AddEquipment(equipShape, startRU);
        }
        
        return cabinet;
    }
    
    /// <summary>
    /// Generate equipment shape from knowledge model concept
    /// </summary>
    public static RackEquipmentShape GenerateEquipment(RackEquipmentConcept concept)
    {
        var heightRU = concept.FindParameter("HeightRU")?.GetValue().AsNumber() ?? 1.0;
        var color = concept.FindParameter("Color")?.GetValue().AsString() ?? "gray";
        var name = concept.Name;
        
        // Create appropriate equipment type based on concept type
        return concept switch
        {
            BlankingPanelConcept => new BlankingPanelShape(name, heightRU),
            ZIFConnectorConcept => new ZIFConnectorShape(name),
            HPL2ModuleConcept => new HPL2ModuleShape(name),
            ITCConcept => new ITCShape(name),
            SPPMCConcept => new SPPMCShape(name),
            GWTConcept => new GWTShape(name),
            FHPCConcept => new FHPCShape(name),
            StorageDrawerConcept => new StorageDrawerShape(name),
            ITAConcept ita => 
                new ITAShape(name, (int)(ita.FindParameter("DeviceNumber")?.GetValue().AsNumber() ?? 1)),
            LPASConcept => new LPASShape(name),
            _ => new BlankingPanelShape(name, heightRU)
        };
    }
    
    /// <summary>
    /// Generate all 4 MF cabinets and add to stage
    /// </summary>
    public static List<RackCabinetShape> GenerateAllMFCabinets()
    {
        var cabinets = new List<RackCabinetShape>();
        
        var cabinet1 = GenerateCabinet(new MFCabinet1Concept());
        cabinet1.Transform.Position.X = 0;
        cabinets.Add(cabinet1);
        
        var cabinet2 = GenerateCabinet(new MFCabinet2Concept());
        cabinet2.Transform.Position.X = 25;
        cabinets.Add(cabinet2);
        
        var cabinet3 = GenerateCabinet(new MFCabinet3Concept());
        cabinet3.Transform.Position.X = 50;
        cabinets.Add(cabinet3);
        
        var cabinet4 = GenerateCabinet(new MFCabinet4Concept());
        cabinet4.Transform.Position.X = 75;
        cabinets.Add(cabinet4);
        
        return cabinets;
    }
    
    /// <summary>
    /// Get summary statistics from knowledge model
    /// </summary>
    public static string GetKnowledgeModelSummary(DataCenterRackModel model)
    {
        var totalCabinets = model.FindParameter("TotalCabinets")?.GetValue().AsNumber() ?? 0;
        var totalEquipment = model.FindParameter("TotalEquipment")?.GetValue().AsNumber() ?? 0;
        var totalRUUsed = model.FindParameter("TotalRUUsed")?.GetValue().AsNumber() ?? 0;
        var totalRUAvailable = model.FindParameter("TotalRUAvailable")?.GetValue().AsNumber() ?? 0;
        
        return $@"Data Center Knowledge Model Summary:
• Total Cabinets: {totalCabinets}
• Total Equipment: {totalEquipment}
• Total RU Used: {totalRUUsed} RU
• Total RU Available: {totalRUAvailable} RU
• Utilization: {totalRUUsed / (totalCabinets * 40) * 100:F1}%";
    }
    
    /// <summary>
    /// Get cabinet summary from knowledge model
    /// </summary>
    public static string GetCabinetSummary(RackCabinetConcept concept)
    {
        var name = concept.Name;
        var equipmentCount = concept.FindParameter("EquipmentCount")?.GetValue().AsNumber() ?? 0;
        var usedRU = concept.FindParameter("UsedRU")?.GetValue().AsNumber() ?? 0;
        var availableRU = concept.FindParameter("AvailableRU")?.GetValue().AsNumber() ?? 0;
        var totalRU = concept.FindParameter("TotalRackUnits")?.GetValue().AsNumber() ?? 40;
        var hasPDU = concept.FindParameter("HasPDU")?.GetValue().AsBoolean() ?? false;
        
        var utilization = (usedRU / totalRU) * 100;
        
        return $"{name}: {equipmentCount} items, {usedRU}U used, {availableRU}U free ({utilization:F1}% utilized){(hasPDU ? " [PDU]" : "")}";
    }
}
