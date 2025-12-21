using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryRulesAndUnits.Extensions;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// A PartComponent subclass that creates and animates 3D geometry.
/// Uses proper KN→FO pattern: KnParameters → EstablishGeometry3D → ComputeShape3D → FoShape3D
/// 
/// Animation is handled via BeforeAnimationRefresh on the FoShape3D - this is the FO layer animation.
/// PreAnimationRefresh on the component is for KN layer parameter updates (currently just logging).
/// </summary>
public class AnimatedKnComponent : PartComponent
{
    
    public AnimatedKnComponent(string name, string color, Vector3 position, double amplitude = 0.5, double frequency = 0.05) : base(name)
    {
        Calculations([
            // Geometry type and appearance
            "GeometryType: 'Box'",
            $"Color: '{color}'",
            
            // Dimension parameters with units
            "Width: units(1.0, 'm')",
            "Height: units(1.0, 'm')",
            "Depth: units(1.0, 'm')",
            
            // Position parameters with units
            $"PositionX: units({position.X}, 'm')",
            $"PositionY: units({position.Y}, 'm')",
            $"PositionZ: units({position.Z}, 'm')",
            
            // Animation parameters - each component can have unique values
            $"Amplitude: units({amplitude}, 'm')",
            $"Frequency: {frequency}",
            "PhaseOffset: PositionX * 0.8",
            "AnimationOffset: units(0.0, 'm')",
            "RotationY: units(0.0, 'deg')"
        ]);

        // KN layer animation callback - currently unused
        // Could be used for parameter updates if needed
        PreAnimationRefresh((comp, evt) =>
        {
            // Reserved for future parameter animation
        });
    }



    /// <summary>
    /// Uses base PartComponent pattern - just apply the compute method.
    /// Parameter system handles dependencies automatically.
    /// </summary>
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view)
    {
        var geometry = Compute3DGeometry(view, null);
        geometry.ApplyMethod("ComputeGeometry", ComputeShape3D, null, null);
        return (geometry, geometry.GetParameter());
    }

    /// <summary>
    /// Create or return existing shape using parameter value check.
    /// </summary>
    private bool ComputeShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        if (geometry == null)
            return false;

        var parameter = geometry.GetParameter();
        FoShape3D? shape = null;

        if (parameter.IsValid())
        {
            // Shape already exists - just use it
            var currentValue = parameter.GetValue();
            if (currentValue.IsSuccess())
            {
                shape = currentValue.AsShape3D();
            }
        }
        else
        {
            // Need to create new shape
            var name = context.GetName();
            var title = context.Title ?? Name ?? "AnimatedShape";
            shape = CreateComponentGeometry(context, name, title);
        }

        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

    /// <summary>
    /// Create the 3D geometry for this component.
    /// Reads from KnParameters to configure geometry.
    /// Animation is set up via BeforeAnimationRefresh on the shape.
    /// </summary>
    protected  FoShape3D? CreateComponentGeometry(KnInstance context, string name, string title)
    {
        // Read configuration from parameters
        var geomType = FindParameterValue<string>("GeometryType") ?? "Box";
        var color = FindParameterValue<string>("Color") ?? "Blue";
        var width = FindLengthValue("Width", 1.0).Value();
        var height = FindLengthValue("Height", 1.0).Value();
        var depth = FindLengthValue("Depth", 1.0).Value();
        var posX = FindLengthValue("PositionX", 0.0).Value();
        var posY = FindLengthValue("PositionY", 0.0).Value();
        var posZ = FindLengthValue("PositionZ", 0.0).Value();
        var animOffset = FindLengthValue("AnimationOffset", 0.0).Value();
        var rotY = 0.0;

        $"AnimatedKnComponent.CreateComponentGeometry: Creating {geomType} '{name}' at ({posX}, {posY}, {posZ})".WriteInfo();
        
        var shape = new FoShape3D(name, color)
        {
            Transform = new Transform3($"{name}Transform")
            {
                Position = new Vector3(posX, posY + animOffset, posZ),
                Rotation = Euler.FromDegrees(0, rotY, 0),
            }
        };
        
        // Create the appropriate geometry type
        switch (geomType.ToLower())
        {
            case "sphere":
                shape.CreateSphere(name, width, height, depth);
                break;
            case "cylinder":
                shape.CreateCylinder(name, width, height, depth);
                break;
            case "box":
            default:
                shape.CreateBox(name, width, height, depth);
                break;
        }
        
        // Create text label as subshape positioned above the geometry
        var label3D = new FoText3D("Label", "white")
        {
            Text = "Tick: 0",
            FontSize = 0.3,
            Transform = new Transform3("LabelTransform")
            {
                Position = new Vector3(0, height + 0.3, depth + 0.3),
            }
        };
        
        // Animate the label to show current tick
        label3D.BeforeAnimationRefresh((self, tick, fps) =>
        {
            if (self is FoText3D textShape)
            {
                textShape.Text = $"Tick: {tick}";
            }
        });
        
        shape.AddShape(label3D);
        
        // Add sinusoidal animation to the shape itself (FO layer animation)
        // Capture base position for oscillation
        var baseX = posX;
        var baseY = posY + animOffset;
        var baseZ = posZ;
        var amplitude = FindLengthValue("Amplitude", 0.5).Value();
        var frequency = FindParameterValue<double>("Frequency");
        var phaseOffset = FindParameterValue<double>("PhaseOffset");
        
        shape.BeforeAnimationRefresh((self, tick, fps) =>
        {
            if (self is FoShape3D s && s.Transform != null)
            {
                // Sinusoidal Y position with phase offset for each component
                var newY = baseY + amplitude * Math.Sin(frequency * tick + phaseOffset);
                s.Transform.MoveTo(baseX, newY, baseZ);
            }
        });
        
        return shape;
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

}
