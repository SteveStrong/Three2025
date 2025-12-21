using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// AnimatedParameterTestComponent - uses PreAnimationRefresh like AnimatedKnModel.
/// Part of the GeometryParameterTestHarness test suite.
/// Framework handles all rendering automatically.
/// </summary>
public class AnimatedParameterTestComponent : PartComponent
{
    
    
    public AnimatedParameterTestComponent(string name, double width, double height, double depth, string geomType) 
        : base(name)
    {
        
        Calculations([
            $"Width|m: {width}",
            $"Height|m: {height}",
            $"Depth|m: {depth}",
            $"GeometryType: '{geomType}'",
            $"X: 0",
            $"Y: 0",
            $"Z: 0"
        ]);
        
    }
    

    
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMethod("ComputeTestGeometry", ComputeTestShape3D, null, null);
        });
        
        return (result, result.GetParameter());
    }
    
    private bool ComputeTestShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        var parameter = geometry?.GetParameter();
        if (parameter == null) return false;
        
        FoShape3D shape;
        
        // ═══════════ PHASE 1: ENSURE GEOMETRY EXISTS ═══════════
        if (parameter.IsCasheEmpty())
        {
            // Build new geometry - reads geometry parameters, establishes dependencies
            $"🆕 CREATE: Building new shape geometry".WriteInfo();
            
            var width = FindNumberValue("Width", 1.0);
            var height = FindNumberValue("Height", 1.0);
            var depth = FindNumberValue("Depth", 1.0);
            var geomType = FindStringValue("GeometryType", "Box");
            
            shape = new FoShape3D($"TestShape_{Name}")
            {
                GlyphId = GetKnowId(),
                Width = width,
                Height = height,
                Depth = depth
            };
            
            shape = geomType switch
            {
                "Box" => shape.CreateBox(shape.Name!, width, height, depth),
                "Sphere" => shape.CreateSphere(shape.Name!, width, height, depth),
                "Cylinder" => shape.CreateCylinder(shape.Name!, width, height, depth),
                _ => shape.CreateBox(shape.Name!, width, height, depth)
            };
            
            // Cache the newly created shape
            parameter.SetCashe(shape);
            
            $"✅ Geometry created and cached (Type={geomType}, W={width}, H={height}, D={depth})".WriteSuccess();
        }
        else
        {
            // Reuse existing geometry from cache
            shape = parameter.GetCashe<FoShape3D>()!;
            $"🔄 Reusing cached geometry (GlyphId={shape.GlyphId})".WriteInfo();
        }
        
        // ═══════════ PHASE 2: APPLY TRANSFORM (ALWAYS) ═══════════
        // Something changed to trigger re-evaluation, so update transform
        var X = FindNumberValue("X", 0.0);
        var Y = FindNumberValue("Y", 0.0);
        var Z = FindNumberValue("Z", 0.0);
        
        shape.Transform.MoveTo(X, Y, Z);
        $"📍 Transform applied: Position=({X}, {Y}, {Z})".WriteInfo();
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
    

}
