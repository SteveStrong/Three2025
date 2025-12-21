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
    /// Helper to find a parameter value by name
    /// Uses IsValid() to avoid aggressive evaluation
    /// </summary>
    private T? FindParameterValue<T>(string name)
    {
        var param = FindParameter(name);
        if (param != null && param.IsValid())
        {
            var value = param.GetValue().Value();
            if (value is T typedValue)
                return typedValue;
        }
        return default;
    }

    /// <summary>
    /// Get existing geometry without creating - for diagnostics
    /// </summary>
    public (KnGeometry?, KnGeometryParameter?) GetGeometry3D(string view)
    {
        var geom = Members<KnGeometry>().FirstOrDefault(x => x.IsNamed(view));
        return (geom, geom?.GetParameter());
    }

    /// <summary>
    /// Creates the actual shape geometry - called by ComputeShape3D when needed
    /// </summary>
    protected  FoShape3D? CreateComponentGeometry(KnInstance context, string name, string title)
    {
        Log("CREATE", $"CreateComponentGeometry called: name={name}, title={title}");
        
        // Read parameters
        var geomType = FindParameterValue<string>("GeometryType") ?? "Box";
        var color = FindParameterValue<string>("Color") ?? "Blue";
        var width = FindLengthValue("Width", 1.0).Value();
        var height = FindLengthValue("Height", 1.0).Value();
        var depth = FindLengthValue("Depth", 1.0).Value();
        var posX = FindLengthValue("PositionX", 0.0).Value();
        var posY = FindLengthValue("PositionY", 0.0).Value();
        var posZ = FindLengthValue("PositionZ", 0.0).Value();
        
        Log("CREATE", $"Parameters: type={geomType}, color={color}, size=({width},{height},{depth}), pos=({posX},{posY},{posZ})");
        
        var shape = new FoShape3D(name, color)
        {
            Transform = new Transform3($"{name}Transform")
            {
                Position = new Vector3(posX, posY, posZ),
            }
        };
        
        // Create geometry based on type
        switch (geomType.ToLower())
        {
            case "sphere":
                shape.CreateSphere(name, width, height, depth);
                Log("CREATE", "Created SPHERE geometry");
                break;
            case "cylinder":
                shape.CreateCylinder(name, width, height, depth);
                Log("CREATE", "Created CYLINDER geometry");
                break;
            case "box":
            default:
                shape.CreateBox(name, width, height, depth);
                Log("CREATE", "Created BOX geometry");
                break;
        }
        
        return shape;
    }

    /// <summary>
    /// Simple pattern - just apply the compute method
    /// Dependencies tracked automatically, cleanup handled by Smash()
    /// </summary>
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view)
    {
        Log("ESTAB", $"EstablishGeometry3D called for view '{view}'");
        
        var geometry = Compute3DGeometry(view, null);
        geometry.ApplyMethod("ComputeGeometry", ComputeShape3D, null, null);
        
        var param = geometry.GetParameter();
        var status = param.IsValid() ? "Valid" : param.IsUnknown() ? "Unknown" : "Invalid";
        Log("ESTAB", $"EstablishGeometry3D complete, parameter status: {status}");
        
        return (geometry, param);
    }

    /// <summary>
    /// Clean pattern: Check IsValid(), create if needed, store with SetValue()
    /// No manual cache management, no manual dependency setup
    /// </summary>
    private bool ComputeShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        Log("COMPUTE", ">>> ComputeShape3D called <<<");
        
        var geometry = context as KnGeometry;
        if (geometry == null)
        {
            Log("ERROR", "Context is not KnGeometry!");
            return false;
        }

        var parameter = geometry.GetParameter();
        FoShape3D? shape = null;

        // PEEK first - IsValid() checks without forcing evaluation
        if (parameter.IsValid())
        {
            Log("COMPUTE", "Parameter is Valid - retrieving existing shape");
            var currentValue = parameter.GetValue();
            if (currentValue.IsSuccess())
            {
                shape = currentValue.AsShape3D();
                Log("COMPUTE", $"Retrieved existing shape: {shape?.Name ?? "NULL"}");
            }
        }
        else
        {
            Log("COMPUTE", "Parameter is NOT Valid - creating new shape");
            
            var name = context.GetName();
            var title = context.Title ?? Name ?? "DebugShape";
            
            shape = CreateComponentGeometry(context, name, title);
            
            if (shape != null)
            {
                Log("COMPUTE", $"Created new shape: {shape.Name}");
            }
            else
            {
                Log("ERROR", "CreateComponentGeometry returned NULL!");
            }
        }

        // Store result - this becomes the cached value
        result.SetValue(ResultStatus.Shape3D, shape);
        Log("COMPUTE", $"Result set with shape: {shape?.Name ?? "NULL"}");
        
        return true;
    }
}
