using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Apprentice;

/// <summary>
/// Wrapper for FoPipe3D that supports dynamic geometry type changes (Pipe, Tube, Line).
/// Demonstrates IBodyLink3D functionality with configurable visual representation.
/// </summary>
public class LinkShape : FoPipe3D
{
    public LinkShape(string name, string color = "yellow", string geomType = "Pipe") : base(name, color)
    {
        SetLinkGeometry(geomType);
    }

    /// <summary>
    /// Changes the link's geometry type and triggers rebuild.
    /// </summary>
    /// <param name="geomType">One of: "Pipe", "Tube", or "Line"</param>
    public void SetLinkGeometry(string geomType)
    {
        GeomType = geomType switch
        {
            "pipe" or "Pipe" => "Pipe",
            "tube" or "Tube" => "Tube",
            "line" or "Line" => "Line",
            _ => "Pipe" // default
        };

        // Mark geometry as stale to trigger rebuild on next render
        SetGeometryStale();
        
        $"[LinkShape] {Name}: Geometry type changed to {GeomType}".WriteSuccess();
    }

    public override string GetTreeNodeTitle()
    {
        var fromName = FromShape3D?.Name ?? "none";
        var toName = ToShape3D?.Name ?? "none";
        return $"{GetName()} [{GeomType}] {Color}: {fromName} → {toName}";
    }
}
