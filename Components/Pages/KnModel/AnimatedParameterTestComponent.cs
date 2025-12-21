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
            $"Width: {width}",
            $"Height: {height}",
            $"Depth: {depth}",
            $"GeometryType: '{geomType}'",
            $"X: 0",
            $"Y: 0",
            $"Z: 0"
        ]);
        
    }
    

    
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view)
    {
        // $"🛠 EstablishGeometry3D called for view '{view}' on '{Name}'".WriteInfo();
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMethod("ComputeTestGeometry", ComputeTestShape3D, null, null);
        });
        
        return (result, result.GetParameter());
    }
    
    private bool ComputeTestShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var width = FindNumberValue("Width", 1.0);
        var height = FindNumberValue("Height", 1.0);
        var depth = FindNumberValue("Depth", 1.0);
        var geomType = FindStringValue("GeometryType", "Box");

        var X = FindNumberValue("X", 0.0);
        var Y = FindNumberValue("Y", 0.0);
        var Z = FindNumberValue("Z", 0.0);
        
        var shape = new FoShape3D($"TestShape_{Name}")
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
        
        // Move box away from camera so it's visible
        shape.Transform.Position = new Vector3(X, Y, Z);
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
    
    // Setters for test harness
    public void SetWidth(double value) => FindParameter("Width")?.SetValue(value);
    public void SetHeight(double value) => FindParameter("Height")?.SetValue(value);
    public void SetDepth(double value) => FindParameter("Depth")?.SetValue(value);
    public void SetGeomType(string value) => FindParameter("GeometryType")?.SetValue(value);
    public void SetPositionX(double value) => FindParameter("X")?.SetValue(value);
    public void SetPositionY(double value) => FindParameter("Y")?.SetValue(value);
    public void SetPositionZ(double value) => FindParameter("Z")?.SetValue(value);
}
