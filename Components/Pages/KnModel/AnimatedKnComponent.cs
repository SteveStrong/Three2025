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
/// Reference pattern: Rack_710.cs
/// </summary>
public class AnimatedKnComponent : PartComponent
{
    public int EventCount { get; set; } = 0;
    public double CurrentValue { get; set; } = 0;
    
    // The 3D geometry this component manages (cached via KnGeometry)
    public FoShape3D? Shape3D { get; private set; }

    public AnimatedKnComponent() : base("AnimatedComponent")
    {
        InitializeParameters();
        SetupAnimationBehavior();
    }

    public AnimatedKnComponent(string name) : base(name)
    {
        InitializeParameters();
        SetupAnimationBehavior();
    }
    
    public AnimatedKnComponent(string name, string color, Vector3 position) : base(name)
    {
        InitializeParameters();
        SetupAnimationBehavior();
        
        // Set initial parameter values
        SetParameterValue("Color", color);
        SetParameterValue("PositionX", position.X, "m");
        SetParameterValue("PositionY", position.Y, "m");
        SetParameterValue("PositionZ", position.Z, "m");
    }

    /// <summary>
    /// Initialize KnParameters for geometry configuration.
    /// Parameters drive geometry creation through the dependency system.
    /// Uses Calculations() API for consistent parameter establishment.
    /// </summary>
    private void InitializeParameters()
    {
        Calculations([
            // Geometry type and appearance
            "GeometryType: 'Box'",
            "Color: 'Blue'",
            
            // Dimension parameters with units
            "Width: units(1.0, 'm')",
            "Height: units(1.0, 'm')",
            "Depth: units(1.0, 'm')",
            
            // Position parameters with units
            "PositionX: units(0.0, 'm')",
            "PositionY: units(0.0, 'm')",
            "PositionZ: units(0.0, 'm')",
            
            // Animation parameters
            "AnimationOffset: units(0.0, 'm')",
            "RotationY: units(0.0, 'deg')"
        ]);
    }

    /// <summary>
    /// Helper to set a parameter value using UpdateParameter API (smashes dependents)
    /// </summary>
    private void SetParameterValue(string name, object value, string? units = null)
    {
        if (units != null && value is double d)
        {
            UpdateParameter(name, d, units);
        }
        else
        {
            UpdateParameter(name, value);
        }
    }

    private void SetupAnimationBehavior()
    {
        // TEMPORARILY DISABLED: Focus on verifying basic geometry rendering first
        // Once geometry renders correctly, re-enable animation
        
        // Use composition pattern - set up the pre-animation action
        // Animation updates parameters, which invalidates geometry cache
        // PreAnimationRefresh((comp, evt) =>
        // {
        //     EventCount++;
        //     // Simulate some computation based on animation tick
        //     CurrentValue = Math.Sin(evt.tick * 0.05) * 100;
        //     
        //     // Update animation parameters - this will invalidate geometry cache
        //     var baseY = FindLengthValue("PositionY", 0.0).Value();
        //     var animatedOffset = Math.Sin(evt.tick * 0.02) * 0.5;
        //     SetParameterValue("AnimationOffset", animatedOffset, "m");
        //     
        //     // Update rotation parameter
        //     var rotation = evt.tick * 0.5;
        //     SetParameterValue("RotationY", rotation, "deg");
        //     
        //     // Update the cached shape's transform directly for smooth animation
        //     // (The parameter changes mark geometry dirty for next full render)
        //     if (Shape3D?.Transform != null)
        //     {
        //         Shape3D.Transform.Position = new Vector3(
        //             FindLengthValue("PositionX", 0.0).Value(),
        //             baseY + animatedOffset,
        //             FindLengthValue("PositionZ", 0.0).Value()
        //         );
        //         Shape3D.Transform.Rotation = Euler.FromDegrees(0, rotation, 0);
        //     }
        // });
    }

    /// <summary>
    /// Override EstablishGeometry3D following Rack_710 pattern.
    /// Uses Compute3DGeometry with ApplyMethod for lazy geometry creation.
    /// </summary>
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view, IArena? page)
    {
        $"AnimatedKnComponent.EstablishGeometry3D: Called for view '{view}'".WriteInfo();
        var result = Compute3DGeometry(view, geom => 
        {
            geom.ApplyMethod("ComputeGeometry", ComputeShape3D, null, null);
        });

        return (result, result.GetParameter());
    }

    /// <summary>
    /// Compute shape following proper IsCasheEmpty pattern from Rack_710.
    /// Creates geometry on first call, updates on subsequent calls.
    /// </summary>
    private bool ComputeShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {

        $"AnimatedKnComponent.ComputeShape3D: Called".WriteInfo();
        var geometry = context as KnGeometry;
        if (geometry == null)
            return false;

        var shape = geometry.GetCashe<FoShape3D>();

        if (geometry.IsCasheEmpty())
        {
            // First time: Create geometry from parameters
            var name = context.GetName();
            var title = context.Title ?? Name ?? "AnimatedShape";
            
            shape = CreateComponentGeometry(context, name, title);
            
            if (shape != null)
            {
                geometry.SetCashe(shape);
                Shape3D = shape; // Keep reference for animation updates
            }
        }
        else
        {
            // Update existing geometry from parameters
            UpdateShape3D(geometry, shape);
        }

        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

    /// <summary>
    /// Create the 3D geometry for this component.
    /// Reads from KnParameters to configure geometry.
    /// </summary>
    protected override FoShape3D? CreateComponentGeometry(KnInstance context, string name, string title)
    {
        $"AnimatedKnComponent.CreateComponentGeometry: Creating geometry for '{name}'".WriteInfo();
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
        var rotY = FindAngleValue("RotationY", 0.0).Value();

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
        
        return shape;
    }

    /// <summary>
    /// Update existing shape from current parameter values.
    /// Called when cache exists but parameters may have changed.
    /// </summary>
    private bool UpdateShape3D(KnGeometry geometry, FoShape3D? shape)
    {
        if (shape?.Transform == null)
            return false;

        // Update transform from current parameter values
        var posX = FindLengthValue("PositionX", 0.0).Value();
        var posY = FindLengthValue("PositionY", 0.0).Value();
        var posZ = FindLengthValue("PositionZ", 0.0).Value();
        var animOffset = FindLengthValue("AnimationOffset", 0.0).Value();
        var rotY = FindAngleValue("RotationY", 0.0).Value();

        shape.Transform.Position = new Vector3(posX, posY + animOffset, posZ);
        shape.Transform.Rotation = Euler.FromDegrees(0, rotY, 0);
        shape.SetDirty(true);

        return true;
    }
    
    /// <summary>
    /// Creates a more complex group geometry with sub-shapes.
    /// Similar to how FoRack creates a rack with equipment.
    /// </summary>
    public FoGroup3D CreateGroupGeometry()
    {
        var shapeName = Name ?? "AnimatedGroup";
        var color = FindParameterValue<string>("Color") ?? "Blue";
        var width = FindLengthValue("Width", 1.0).Value();
        var height = FindLengthValue("Height", 1.0).Value();
        var depth = FindLengthValue("Depth", 1.0).Value();
        var posX = FindLengthValue("PositionX", 0.0).Value();
        var posY = FindLengthValue("PositionY", 0.0).Value();
        var posZ = FindLengthValue("PositionZ", 0.0).Value();

        $"AnimatedKnComponent.CreateGroupGeometry: Creating group '{shapeName}'".WriteInfo();
        
        var group = new FoGroup3D(shapeName)
        {
            Transform = new Transform3($"{shapeName}Transform")
            {
                Position = new Vector3(posX, posY, posZ),
                Rotation = Euler.FromDegrees(0, 0, 0),
            }
        };
        
        // Create main body
        var body = new FoShape3D($"{shapeName}_Body", color)
        {
            Transform = new Transform3("BodyTransform")
            {
                Position = new Vector3(0, 0, 0),
            }
        }.CreateBox($"{shapeName}_Body", width, height, depth);
        group.AddSubGlyph3D(body);
        
        // Add a decorative top (sphere uses width/height/depth for ellipsoid dimensions)
        var sphereSize = 0.2;
        var top = new FoShape3D($"{shapeName}_Top", "Yellow")
        {
            Transform = new Transform3("TopTransform")
            {
                Position = new Vector3(0, height / 2 + 0.1, 0),
            }
        }.CreateSphere($"{shapeName}_Top", sphereSize, sphereSize, sphereSize);
        group.AddSubGlyph3D(top);
        
        // Add corner markers
        var cornerOffset = 0.4;
        var corners = new[] {
            new Vector3(-cornerOffset, -height/2, -cornerOffset),
            new Vector3(cornerOffset, -height/2, -cornerOffset),
            new Vector3(-cornerOffset, -height/2, cornerOffset),
            new Vector3(cornerOffset, -height/2, cornerOffset),
        };
        
        for (int i = 0; i < corners.Length; i++)
        {
            var corner = new FoShape3D($"{shapeName}_Corner{i}", "Red")
            {
                Transform = new Transform3($"Corner{i}Transform")
                {
                    Position = corners[i],
                }
            }.CreateCylinder($"{shapeName}_Corner{i}", 0.1, 0.3, 0.1);
            group.AddSubGlyph3D(corner);
        }
        
        // Store reference for animation updates
        Shape3D = body;
        
        body.BeforeAnimationRefresh((self, tick, fps) =>
        {
            bool move = tick % 10 == 0;
            if (!move) return;

            var delta = Math.Sin(tick * 0.05) * 100;


            var pos = self.Transform.MoveBy(0, 0, delta);
            if (pos.Z > 10 || pos.Z < -10)
            {
                delta = -delta;
            }
            //self.SetTransformStale();
        });
        return group;
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

    public override string GetTreeNodeTitle()
    {
        var geomType = FindParameterValue<string>("GeometryType") ?? "Box";
        var shapeInfo = Shape3D != null ? $", Shape:{geomType}" : "";
        return $"{Name} (Events:{EventCount}, Value:{CurrentValue:F2}{shapeInfo})";
    }
}
