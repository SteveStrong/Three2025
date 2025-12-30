using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Apprentice;

/// <summary>
/// Wrapper for FoPipe3D that supports dynamic geometry type changes (Pipe, Tube, Line).
/// Demonstrates IBodyLink3D functionality with configurable visual representation.
/// </summary>
public class LinkShape : FoPipe3D
{
    /// <summary>
    /// Default formatter for link shapes - shows name, geometry type, color, and connection info
    /// </summary>
    public static new readonly Func<FoBase, string> DefaultFormatter = g => 
    {
        if (g is LinkShape link)
        {
            var fromName = link.FromShape3D?.Name ?? "none";
            var toName = link.ToShape3D?.Name ?? "none";
            return $"{g.Key} [{link.GeomType}] {link.Color}: {fromName} → {toName}";
        }
        return $"{g.Key} Link";
    };

    public LinkShape(string name, string color = "yellow", string geomType = "Pipe") : base(name, color)
    {
        // Set the formatter for link display
        ComputeTreeNodeTitle = DefaultFormatter;
        
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
}
