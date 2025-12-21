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
    

    
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeMesh3D", ComputeMesh3D, null);
            geom.ApplyTransformMethod("ComputeTransform3D", ComputeTransform3D, null);
            geom.ApplyBodyMethod("ComputeBody3D", ComputeBody3D, null);
        });
        
        return (result, result.GetBodyParameter());
    }

    private bool ComputeMesh3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        var parameter = geometry?.GetMeshParameter();
        if (parameter == null) return false;
        
        FoShape3D? shape = parameter.GetCashe<FoShape3D>();
        
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

        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

    private bool ComputeTransform3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        var parameter = geometry?.GetTransformParameter();
        if (parameter == null) return false;
        
    
        
        // ═══════════ PHASE 2: APPLY TRANSFORM (ALWAYS) ═══════════
        // Something changed to trigger re-evaluation, so update transform
        var X = FindNumberValue("X", 0.0);
        var Y = FindNumberValue("Y", 0.0);
        var Z = FindNumberValue("Z", 0.0);


        Transform3 transform = new Transform3($"TestShape_{Name}");
        transform.MoveTo(X, Y, Z);
        $"📍 Transform applied: Position=({X}, {Y}, {Z})".WriteInfo();
        
        result.SetValue(ResultStatus.Transform3, transform);
        return true;
    }

    private bool ComputeBody3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        if (geometry == null) return false;
        
        var parameter = geometry.GetBodyParameter();
        if (parameter == null) return false;
        
        $"🔷 BODY: Composing Mesh + Transform".WriteInfo();
        
        // Read Mesh parameter - establishes Mesh → Body dependency
        // If Mesh is Unknown, this triggers Mesh computation and caching
        var meshResult = geometry.GetMeshParameter().GetCurrentValue();
        var shape = meshResult.ValueAs<FoShape3D>();
        
        if (shape == null)
        {
            $"❌ BODY: No shape from Mesh parameter".WriteError();
            return false;
        }
        
        // Read Transform parameter - establishes Transform → Body dependency
        // If Transform is Unknown, this triggers Transform computation
        var transformResult = geometry.GetTransformParameter().GetCurrentValue();
        var transform = transformResult.ValueAs<Transform3>();
        
        if (transform == null)
        {
            $"❌ BODY: No transform from Transform parameter".WriteError();
            return false;
        }
        
        // Apply transform to shape IN-PLACE (no copy, mutates mesh's Transform)
        shape.Transform.Position = transform.Position;
        shape.Transform.Rotation = transform.Rotation;
        shape.Transform.Scale = transform.Scale;
        
        $"✅ BODY: Composed shape with transform at {transform.Position}".WriteSuccess();
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

}
