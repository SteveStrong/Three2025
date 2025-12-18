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
    /// </summary>
    private T? FindParameterValue<T>(string name)
    {
        var param = FindParameter(name);
        if (param != null)
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
    /// Required by base class - creates the actual shape geometry
    /// </summary>
    protected override FoShape3D? CreateComponentGeometry(KnInstance context, string name, string title)
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
    /// Override EstablishGeometry3D with extensive logging
    /// </summary>
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view, IArena? arena)
    {
        Log("ESTAB", $"EstablishGeometry3D called for view '{view}'");
        
        var result = Compute3DGeometry(view, geom => 
        {
            Log("ESTAB", $"Compute3DGeometry action - setting up ApplyMethod");
            
            // Set up compute method with BeforeSmash callback
            geom.ApplyMethod("ComputeGeometry", ComputeShape3D, null, (param, opResult) => 
            {
                Log("SMASH", ">>> BeforeSmash callback fired! <<<");
                
                var oldShape = geom.GetCashe<FoShape3D>();
                Log("SMASH", $"Old shape in cache: {(oldShape != null ? oldShape.Name : "NULL")}");
                
                // Delete old shape
                if (oldShape != null)
                {
                    Log("SMASH", $"Deleting old shape '{oldShape.Name}'");
                    oldShape.Delete();
                }
                
                // Force clear cache
                Log("SMASH", "Forcing cache clear with SetCashe(null!)");
                geom.GetParameter().SetCashe(null!);
                
                Log("SMASH", $"After clear: IsCasheEmpty={geom.IsCasheEmpty()}");
            });
            
            // Setup dependencies
            SetupDependencies(geom.GetParameter());
        });

        Log("ESTAB", $"EstablishGeometry3D complete, IsCasheEmpty={result.IsCasheEmpty()}");
        return (result, result.GetParameter());
    }

    private void SetupDependencies(KnGeometryParameter geomParam)
    {
        var geomTypeParam = FindParameter("GeometryType");
        var colorParam = FindParameter("Color");
        
        if (geomTypeParam != null)
        {
            geomParam.IDependOn(geomTypeParam);
            Log("DEP", $"Geometry depends on GeometryType (ContributesTo count: {geomTypeParam.ContributesTo.Count})");
        }
        
        if (colorParam != null)
        {
            geomParam.IDependOn(colorParam);
            Log("DEP", $"Geometry depends on Color (ContributesTo count: {colorParam.ContributesTo.Count})");
        }
    }

    private bool ComputeShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        Log("COMPUTE", ">>> ComputeShape3D called <<<");
        
        var geometry = context as KnGeometry;
        if (geometry == null)
        {
            Log("ERROR", "Context is not KnGeometry!");
            return false;
        }

        var isCacheEmpty = geometry.IsCasheEmpty();
        Log("COMPUTE", $"IsCasheEmpty = {isCacheEmpty}");

        if (isCacheEmpty)
        {
            Log("COMPUTE", "Cache is empty - calling CreateComponentGeometry");
            
            var name = context.GetName();
            var title = context.Title ?? Name ?? "DebugShape";
            
            var shape = CreateComponentGeometry(context, name, title);
            
            if (shape != null)
            {
                // Store in cache
                geometry.SetCashe(shape);
                Log("COMPUTE", $"Shape stored in cache: {shape.Name}");
                
                // Re-establish dependencies (they get cleared on Smash)
                SetupDependencies(geometry.GetParameter());
                Log("COMPUTE", "Dependencies re-established");
                
                result.SetValue(ResultStatus.Shape3D, shape);
            }
            else
            {
                Log("ERROR", "CreateComponentGeometry returned NULL!");
            }
        }
        else
        {
            var existingShape = geometry.GetCashe<FoShape3D>();
            Log("COMPUTE", $"Cache HIT - existing shape: {existingShape?.Name ?? "NULL"}");
            result.SetValue(ResultStatus.Shape3D, existingShape);
        }

        return true;
    }
}
