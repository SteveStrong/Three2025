using FoundryWorldsAndDrawings;
using FoundryMicroCore.Core;
using FoundryWorldsAndDrawings.Shape;
using FoundryMicroCore.Core;

namespace Three2025.Apprentice;

/// <summary>
/// Collection of common tree node formatters for different display contexts
/// </summary>
public static class TreeNodeFormatters
{
    /// <summary>
    /// Spatial formatter - emphasizes position information for 3D objects
    /// </summary>
    public static readonly Func<MxObject, string> Spatial = g => 
    {
        if (g is FoShape3D shape && shape.Transform?.Position != null)
        {
            var pos = shape.Transform.Position;
            return $"{g.Name} {shape.Color} @ {pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0}";
        }
        return $"{g.Name} {(g as FoShape3D)?.Color}";
    };
    
    /// <summary>
    /// Technical formatter - detailed geometry and technical information
    /// </summary>
    public static readonly Func<MxObject, string> Technical = g => 
    {
        if (g is FoGlyph3D glyph)
        {
            return $"{glyph.GeomType}: {g.Name} {glyph.Color} {g.GetType().Name} B:{glyph.Width:F1} {glyph.Height:F1} {glyph.Depth:F1} => ";
        }
        return $"{g.Name} {g.GetType().Name}";
    };
    
    /// <summary>
    /// Summary formatter - clean, minimal display
    /// </summary>
    public static readonly Func<MxObject, string> Summary = g => 
    {
        return $"{g.Name} [{g.GetType().Name}]";
    };
    
    /// <summary>
    /// Equipment formatter - specialized for complex equipment with feature counts
    /// </summary>
    public static readonly Func<MxObject, string> Equipment = g => 
    {
        if (g is FoShape3D shape && shape.Transform?.Position != null)
        {
            var pos = shape.Transform.Position;
            return $"{g.Name} Equipment @ ({pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0})";
        }
        return $"{g.Name} Equipment";
    };
}