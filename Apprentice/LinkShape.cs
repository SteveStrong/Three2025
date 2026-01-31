using FoundryWorldsAndDrawings.Shape;
using FoundryMicroCore.Core.Extensions;
using FoundryMicroCore.Core;
using FoundryWorldsAndDrawings;
using FoundryMicroCore.Core;
using FoundryRulesAndUnits.Extensions;
using FoundryMicroCore.Core;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryMicroCore.Core;

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
    public static new readonly Func<MxObject, string> DefaultFormatter = g => 
    {
        if (g is LinkShape link)
        {
            var fromName = link.FromShape3D?.Name ?? "none";
            var toName = link.ToShape3D?.Name ?? "none";
            return $"{g.Name} [{link.GeomType}] {link.Color}: {fromName} → {toName}";
        }
        return $"{g.Name} Link";
    };

    public LinkShape(string name, string color = "yellow", string geomType = "Pipe") : base(name, color)
    {
        // Set the formatter for link display
        MxObject.SetCustomTreeViewNodeTitleFunction(this, DefaultFormatter);
        
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
