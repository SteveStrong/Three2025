using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryRulesAndUnits.Extensions;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// Simplified geometry component for debugging the parameter → geometry lifecycle.
/// Has explicit logging at each step to trace what's happening.
/// </summary>
public class DebugGeometryComponent : PartComponent
{
    private Action<string, string>? _logCallback;
    
    public DebugGeometryComponent(string name, string color, Vector3 position) : base(name)
    {
        Calculations([
            "GeometryType: 'Box'",
            $"Color: '{color}'",
            "Width: units(1.5, 'm')",
            "Height: units(1.5, 'm')",
            "Depth: units(1.5, 'm')",
            $"PositionX: units({position.X}, 'm')",
            $"PositionY: units({position.Y}, 'm')",
            $"PositionZ: units({position.Z}, 'm')",
        ]);
        
        $"DebugGeometryComponent '{name}' created with parameters".WriteSuccess();
    }

    public void SetLogCallback(Action<string, string> callback)
    {
        _logCallback = callback;
    }

    private void Log(string type, string message)
    {
        _logCallback?.Invoke(type, message);
        $"[DebugGeom] [{type}] {message}".WriteInfo();
    }

    /// <summary>
    /// Get existing geometry without creating - for diagnostics
    /// </summary>
    public (KnGeometry?, KnParameter?) GetGeometry3D(string view)
    {
        var geom = Members<KnGeometry>().FirstOrDefault(x => x.IsNamed(view));
        return (geom, geom?.GetBodyParameter());
    }

    /// <summary>
    /// Modern three-phase pattern: Mesh → Transform → Body
    /// </summary>
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        Log("ESTAB", $"EstablishGeometry3D called for view '{view}'");
        
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeMesh3D", ComputeMesh3D);
            geom.ApplyTransformMethod("ComputeTransform3D", ComputeTransform3D);
            geom.ApplyBodyMethod("ComputeBody3D", ComputeBody3D);
        });
        
        var param = result.GetBodyParameter();
        var status = param.IsValid() ? "Valid" : param.IsUnknown() ? "Unknown" : "Invalid";
        Log("ESTAB", $"EstablishGeometry3D complete, BodyParameter status: {status}");
        
        return (result, param);
    }

    /// <summary>
    /// PHASE 1: Create mesh geometry (cached)
    /// </summary>
    private bool ComputeMesh3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        Log("MESH", ">>> ComputeMesh3D called <<<");
        
        var geometry = context as KnGeometry;
        var parameter = geometry?.GetMeshParameter();
        if (parameter == null)
        {
            Log("ERROR", "MeshParameter is NULL!");
            return false;
        }

        FoShape3D? shape = parameter.GetCashe<FoShape3D>();
        
        // Only create if cache is empty
        if (parameter.IsCasheEmpty())
        {
            Log("MESH", "Cache EMPTY - creating new shape");
            
            // Read parameters - establishes dependencies
            var geomType = FindStringValue("GeometryType", "Box");
            var color = FindStringValue("Color", "Blue");
            var width = FindLengthValue("Width", 1.0).Value();
            var height = FindLengthValue("Height", 1.0).Value();
            var depth = FindLengthValue("Depth", 1.0).Value();
            
            Log("MESH", $"Parameters: type={geomType}, color={color}, size=({width},{height},{depth})");
            
            var name = context.GetName();
            shape = new FoShape3D(name, color)
            {
                GlyphId = GetKnowId(),
                Width = width,
                Height = height,
                Depth = depth
            };
            
            // Create geometry based on type
            shape = geomType.ToLower() switch
            {
                "sphere" => shape.CreateSphere(name, width, height, depth),
                "cylinder" => shape.CreateCylinder(name, width, height, depth),
                _ => shape.CreateBox(name, width, height, depth)
            };
            
            parameter.SetCashe(shape);
            Log("MESH", $"Created {geomType} and cached");
        }
        else
        {
            Log("MESH", $"Cache HIT - reusing existing shape: {shape?.Name}");
        }

        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

    /// <summary>
    /// PHASE 2: Compute transform (always evaluated)
    /// </summary>
    private bool ComputeTransform3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        Log("TRANSFORM", ">>> ComputeTransform3D called <<<");
        
        var geometry = context as KnGeometry;
        var parameter = geometry?.GetTransformParameter();
        if (parameter == null)
        {
            Log("ERROR", "TransformParameter is NULL!");
            return false;
        }
        
        // Read position parameters - establishes dependencies
        var posX = FindLengthValue("PositionX", 0.0).Value();
        var posY = FindLengthValue("PositionY", 0.0).Value();
        var posZ = FindLengthValue("PositionZ", 0.0).Value();
        
        var transform = new Transform3($"{context.GetName()}Transform");
        transform.MoveTo(posX, posY, posZ);
        
        Log("TRANSFORM", $"Transform created: Position=({posX}, {posY}, {posZ})");
        
        result.SetValue(ResultStatus.Transform3, transform);
        return true;
    }

    /// <summary>
    /// PHASE 3: Compose Mesh + Transform into Body
    /// </summary>
    private bool ComputeBody3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        Log("BODY", ">>> ComputeBody3D called <<<");
        
        var geometry = context as KnGeometry;
        if (geometry == null)
        {
            Log("ERROR", "Context is not KnGeometry!");
            return false;
        }
        
        // Read Mesh parameter - establishes dependency
        var meshResult = geometry.GetMeshParameter().GetCurrentValue();
        var shape = meshResult.ValueAs<FoShape3D>();
        
        if (shape == null)
        {
            Log("ERROR", "No shape from Mesh parameter!");
            return false;
        }
        
        // Read Transform parameter - establishes dependency
        var transformResult = geometry.GetTransformParameter().GetCurrentValue();
        var transform = transformResult.ValueAs<Transform3>();
        
        if (transform == null)
        {
            Log("ERROR", "No transform from Transform parameter!");
            return false;
        }
        
        // Apply transform to shape IN-PLACE
        var height = FindLengthValue("Height", 1.0).Value();

        shape.Transform.Pivot = new Vector3(0, -height/2, 0);
        shape.Transform.Position = transform.Position;
        shape.Transform.Rotation = transform.Rotation;
        shape.Transform.Scale = transform.Scale;
        
        Log("BODY", $"Composed shape '{shape.Name}' with transform at {transform.Position}");
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
}
