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
    private IModelEditor? _modelEditor;
    
    // Animation state
    private int _animationFrameCount = 0;
    private bool _animationEnabled = false;
    private List<Action<int>> _animationChain = new();
    
    // Animation constants
    private const int POSITION_PERIOD = 120;  // Frames for full circle
    private const int SIZE_PERIOD = 90;       // Frames for size pulse
    private const int SHAPE_PERIOD = 180;     // Frames between shape changes
    private const int COLOR_PERIOD = 240;     // Frames between color changes
    private const int ROTATION_PERIOD = 200;  // Frames for full rotation
    private const double CIRCLE_RADIUS = 3.0; // Radius of circular motion
    
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
            "RotationX: 0.0",
            "RotationY: 0.0",
            "RotationZ: 0.0",
        ]);
        
        // Build animation chain
        _animationChain.Add(AnimatePosition);
        _animationChain.Add(AnimateSize);
        _animationChain.Add(AnimateShape);
        _animationChain.Add(AnimateColor);
        _animationChain.Add(AnimateRotation);
        
        $"DebugGeometryComponent '{name}' created with parameters".WriteSuccess();
    }

    public void SetLogCallback(Action<string, string> callback)
    {
        _logCallback = callback;
    }
    
    /// <summary>
    /// Set the ModelEditor reference needed for animation parameter updates
    /// </summary>
    public void SetModelEditor(IModelEditor editor)
    {
        _modelEditor = editor;
    }
    
    /// <summary>
    /// Enable or disable animation for this component (Option B: per-component control)
    /// </summary>
    public void SetAnimationEnabled(bool enabled)
    {
        _animationEnabled = enabled;
        if (enabled)
        {
            // Set up PreAnimationRefresh callback when animation is enabled
            PreAnimationRefresh((comp, evt) =>
            {
                if (!_animationEnabled) return; // No-op pattern
                
                _animationFrameCount++;
                
                // Execute all animation functions in the chain
                foreach (var animationFunc in _animationChain)
                {
                    animationFunc(_animationFrameCount);
                }
            });
            
            Log("ANIM", "Animation ENABLED - PreAnimationRefresh callback registered");
        }
        else
        {
            // Disable by clearing the callback (no-op)
            PreAnimationRefreshNOOP(null!);
            Log("ANIM", "Animation DISABLED");
        }
    }
    
    /// <summary>
    /// Animation Function 1: Circular motion in XZ plane
    /// </summary>
    private void AnimatePosition(int frameCount)
    {
        if (_modelEditor == null) return;
        
        var angle = (frameCount % POSITION_PERIOD) * (2.0 * Math.PI / POSITION_PERIOD);
        var x = CIRCLE_RADIUS * Math.Cos(angle);
        var z = CIRCLE_RADIUS * Math.Sin(angle);
        var y = 1.0; // Keep at constant height
        
        _modelEditor.SetParameter(this, "PositionX", $"units({x:F3}, 'm')");
        _modelEditor.SetParameter(this, "PositionY", $"units({y:F3}, 'm')");
        _modelEditor.SetParameter(this, "PositionZ", $"units({z:F3}, 'm')");
    }
    
    /// <summary>
    /// Animation Function 2: Pulsing size using sine waves at different frequencies
    /// </summary>
    private void AnimateSize(int frameCount)
    {
        if (_modelEditor == null) return;
        
        var phase = (frameCount % SIZE_PERIOD) * (2.0 * Math.PI / SIZE_PERIOD);
        
        // Each dimension pulses at a slightly different phase for visual interest
        var width = 1.5 + 0.5 * Math.Sin(phase);
        var height = 1.5 + 0.5 * Math.Sin(phase + Math.PI / 3);
        var depth = 1.5 + 0.5 * Math.Sin(phase + 2 * Math.PI / 3);
        
        _modelEditor.SetParameter(this, "Width", $"units({width:F3}, 'm')");
        _modelEditor.SetParameter(this, "Height", $"units({height:F3}, 'm')");
        _modelEditor.SetParameter(this, "Depth", $"units({depth:F3}, 'm')");
    }
    
    /// <summary>
    /// Animation Function 3: Cycle through geometry types
    /// </summary>
    private void AnimateShape(int frameCount)
    {
        if (_modelEditor == null) return;
        
        // Only change shape at specific intervals
        if (frameCount % SHAPE_PERIOD != 0) return;
        
        var shapeIndex = (frameCount / SHAPE_PERIOD) % 3;
        var shape = shapeIndex switch
        {
            0 => "Box",
            1 => "Sphere",
            2 => "Cylinder",
            _ => "Box"
        };
        
        _modelEditor.SetParameter(this, "GeometryType", $"'{shape}'");
        Log("ANIM", $"Shape changed to {shape}");
    }
    
    /// <summary>
    /// Animation Function 4: Cycle through colors
    /// </summary>
    private void AnimateColor(int frameCount)
    {
        if (_modelEditor == null) return;
        
        // Only change color at specific intervals
        if (frameCount % COLOR_PERIOD != 0) return;
        
        var colorIndex = (frameCount / COLOR_PERIOD) % 3;
        var color = colorIndex switch
        {
            0 => "Blue",
            1 => "Green",
            2 => "Red",
            _ => "Blue"
        };
        
        _modelEditor.SetParameter(this, "Color", $"'{color}'");
        Log("ANIM", $"Color changed to {color}");
    }
    
    /// <summary>
    /// Animation Function 5: Continuous rotation around X axis
    /// </summary>
    private void AnimateRotation(int frameCount)
    {
        if (_modelEditor == null) return;
        
        // Continuous rotation around X axis (360 degrees over ROTATION_PERIOD frames)
        var angle = (frameCount % ROTATION_PERIOD) * (2.0 * Math.PI / ROTATION_PERIOD);
        
        _modelEditor.SetParameter(this, "RotationX", $"{angle:F3}");
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
        
        // Read rotation parameters (plain doubles, radians)
        var rotX = FindNumberValue("RotationX", 0.0);
        var rotY = FindNumberValue("RotationY", 0.0);
        var rotZ = FindNumberValue("RotationZ", 0.0);
        
        var transform = new Transform3($"{context.GetName()}Transform");
        transform.MoveTo(posX, posY, posZ);
        transform.Rotation = new Euler(rotX, rotY, rotZ);
        
        Log("TRANSFORM", $"Transform created: Position=({posX}, {posY}, {posZ}), Rotation=({rotX:F2}, {rotY:F2}, {rotZ:F2})");
        
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
