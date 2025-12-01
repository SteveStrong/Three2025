using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryMentorModeler.Model;
using FoundryRulesAndUnits.Extensions;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// A KnComponent subclass that creates and animates 3D geometry.
/// Demonstrates using the PreContextLink composition pattern with geometry creation.
/// Similar pattern to FoRack - creates shapes that can be added to a stage.
/// </summary>
public class AnimatedKnComponent : KnComponent
{
    public int EventCount { get; set; } = 0;
    public double CurrentValue { get; set; } = 0;
    
    // The 3D geometry this component manages
    public FoShape3D? Shape3D { get; private set; }
    
    // Configuration for the geometry
    public string GeometryType { get; set; } = "Box";
    public string Color { get; set; } = "Blue";
    public double Width { get; set; } = 1.0;
    public double Height { get; set; } = 1.0;
    public double Depth { get; set; } = 1.0;
    public Vector3 Position { get; set; } = new Vector3(0, 0, 0);

    public AnimatedKnComponent() : base("AnimatedComponent")
    {
        SetupAnimationBehavior();
    }

    public AnimatedKnComponent(string name) : base(name)
    {
        SetupAnimationBehavior();
    }
    
    public AnimatedKnComponent(string name, string color, Vector3 position) : base(name)
    {
        Color = color;
        Position = position;
        SetupAnimationBehavior();
    }

    private void SetupAnimationBehavior()
    {
        // Use composition pattern - set up the pre-animation action
        PreAnimationRefresh((comp, evt) =>
        {
            EventCount++;
            // Simulate some computation based on animation tick
            CurrentValue = Math.Sin(evt.tick * 0.05) * 100;
            
            // Update geometry if it exists - animate the Y position with a sine wave
            if (Shape3D?.Transform != null)
            {
                var baseY = Position.Y;
                var animatedY = baseY + Math.Sin(evt.tick * 0.02) * 0.5;
                Shape3D.Transform.Position = new Vector3(Position.X, animatedY, Position.Z);
                
                // Also rotate slowly
                var rotation = evt.tick * 0.5;
                Shape3D.Transform.Rotation = Euler.FromDegrees(0, rotation, 0);
            }
        });
    }

    /// <summary>
    /// Creates the 3D geometry for this component.
    /// Call this after the component is configured and before adding to a stage.
    /// </summary>
    public FoShape3D CreateGeometry()
    {
        var shapeName = Name ?? "AnimatedShape";
        $"AnimatedKnComponent.CreateGeometry: Creating {GeometryType} '{shapeName}' at ({Position.X}, {Position.Y}, {Position.Z})".WriteInfo();
        
        Shape3D = new FoShape3D(shapeName, Color)
        {
            Transform = new Transform3($"{shapeName}Transform")
            {
                Position = Position,
                Rotation = Euler.FromDegrees(0, 0, 0),
            }
        };
        
        // Create the appropriate geometry type
        switch (GeometryType.ToLower())
        {
            case "sphere":
                Shape3D.CreateSphere(shapeName, Width, Height, Depth);
                break;
            case "cylinder":
                Shape3D.CreateCylinder(shapeName, Width, Height, Depth);
                break;
            case "box":
            default:
                Shape3D.CreateBox(shapeName, Width, Height, Depth);
                break;
        }
        
        return Shape3D;
    }
    
    /// <summary>
    /// Creates a more complex group geometry with sub-shapes.
    /// Similar to how FoRack creates a rack with equipment.
    /// </summary>
    public FoGroup3D CreateGroupGeometry()
    {
        var shapeName = Name ?? "AnimatedGroup";
        $"AnimatedKnComponent.CreateGroupGeometry: Creating group '{shapeName}'".WriteInfo();
        
        var group = new FoGroup3D(shapeName)
        {
            Transform = new Transform3($"{shapeName}Transform")
            {
                Position = Position,
                Rotation = Euler.FromDegrees(0, 0, 0),
            }
        };
        
        // Create main body
        var body = new FoShape3D($"{shapeName}_Body", Color)
        {
            Transform = new Transform3("BodyTransform")
            {
                Position = new Vector3(0, 0, 0),
            }
        }.CreateBox($"{shapeName}_Body", Width, Height, Depth);
        group.AddSubGlyph3D(body);
        
        // Add a decorative top (sphere uses width/height/depth for ellipsoid dimensions)
        var sphereSize = 0.2;
        var top = new FoShape3D($"{shapeName}_Top", "Yellow")
        {
            Transform = new Transform3("TopTransform")
            {
                Position = new Vector3(0, Height / 2 + 0.1, 0),
            }
        }.CreateSphere($"{shapeName}_Top", sphereSize, sphereSize, sphereSize);
        group.AddSubGlyph3D(top);
        
        // Add corner markers
        var cornerOffset = 0.4;
        var corners = new[] {
            new Vector3(-cornerOffset, -Height/2, -cornerOffset),
            new Vector3(cornerOffset, -Height/2, -cornerOffset),
            new Vector3(-cornerOffset, -Height/2, cornerOffset),
            new Vector3(cornerOffset, -Height/2, cornerOffset),
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
        
        return group;
    }

    public override string GetTreeNodeTitle()
    {
        var shapeInfo = Shape3D != null ? $", Shape:{GeometryType}" : "";
        return $"{Name} (Events:{EventCount}, Value:{CurrentValue:F2}{shapeInfo})";
    }
}
