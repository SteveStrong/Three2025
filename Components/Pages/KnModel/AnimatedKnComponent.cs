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
    // Clock animation state
    private bool _clockAnimationEnabled = false;
    private int _currentClockPosition = 0;  // 0-11 (0 = 12 o'clock, going clockwise)
    private double _clockRadius = 3.0;      // Radius of circular path
    private Vector3 _clockCenter = Vector3.Zero;  // Center of circle
    private int _framesPerMove = 60;        // Move to next position every 60 frames (1 second at 60fps)
    private int _frameCounter = 0;
    private string[] _shapeTypes = new[] { "Box", "Sphere", "Cylinder" };
    private int _currentShapeIndex = 0;
    
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

        // KN layer animation callback - updates parameters for clock animation
        PreAnimationRefresh((comp, evt) =>
        {
            if (!_clockAnimationEnabled) return;
            
            _frameCounter++;
            
            // Move to next clock position every second (60 frames at 60fps)
            if (_frameCounter >= _framesPerMove)
            {
                _frameCounter = 0;
                _currentClockPosition = (_currentClockPosition + 1) % 12;
                
                // Calculate angle (0 = 12 o'clock = top, clockwise)
                // 12 o'clock is at angle -90° (or -π/2), going clockwise
                var angleRadians = (_currentClockPosition * Math.PI / 6.0) - (Math.PI / 2.0);
                
                // Calculate X and Z position on circle
                var newX = _clockCenter.X + _clockRadius * Math.Cos(angleRadians);
                var newZ = _clockCenter.Z + _clockRadius * Math.Sin(angleRadians);
                
                $"⏰ CLOCK: Moving to position {_currentClockPosition} (angle={angleRadians:F2} rad, X={newX:F2}, Z={newZ:F2})".WriteInfo();
                
                // Update position parameters - this will trigger Smash cascade
                var xParam = FindParameter("PositionX");
                var zParam = FindParameter("PositionZ");
                
                if (xParam != null && zParam != null)
                {
                    xParam.ApplyFormula($"units({newX}, 'm')", KnBase.UnitService);
                    zParam.ApplyFormula($"units({newZ}, 'm')", KnBase.UnitService);
                }
                
                // At 12 o'clock (position 0), change shape
                if (_currentClockPosition == 0)
                {
                    _currentShapeIndex = (_currentShapeIndex + 1) % _shapeTypes.Length;
                    var newShape = _shapeTypes[_currentShapeIndex];
                    
                    $"🔄 CLOCK: At 12 o'clock! Changing shape to {newShape}".WriteSuccess();
                    
                    var geomParam = FindParameter("GeometryType");
                    if (geomParam != null)
                    {
                        geomParam.ApplyFormula($"'{newShape}'", KnBase.UnitService);
                    }
                }
            }
        });
    }



    /// <summary>
    /// Three-parameter architecture: Mesh (geometry at origin) + Transform (position/rotation) + Body (composition)
    /// Parameter system handles dependencies automatically.
    /// </summary>
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var geometry = Compute3DGeometry(view, null);
        geometry.ApplyMeshMethod("ComputeMesh", ComputeMesh3D);
        geometry.ApplyTransformMethod("ComputeTransform", ComputeTransform3D);
        geometry.ApplyBodyMethod("ComputeBody", ComputeBody3D);
        return (geometry, geometry.GetBodyParameter());
    }

    /// <summary>
    /// MESH: Create or return cached geometry at origin.
    /// Reads only geometry-defining parameters (Width, Height, Depth, GeometryType, Color).
    /// Animation setup happens here since it's part of shape configuration.
    /// </summary>
    private bool ComputeMesh3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        if (geometry == null)
            return false;

        var parameter = geometry.GetMeshParameter();
        FoShape3D? shape = null;

        if (parameter.IsCasheEmpty())
        {
            // ═══════════ CREATE MODE ═══════════
            var name = context.GetName();
            var title = context.Title ?? Name ?? "AnimatedShape";
            
            $"🆕 CREATE MESH: Building animated shape '{name}'".WriteInfo();
            
            // Read geometry parameters only (establishes Mesh dependencies)
            var geomType = FindParameterValue<string>("GeometryType") ?? "Box";
            var color = FindParameterValue<string>("Color") ?? "Blue";
            var width = FindLengthValue("Width", 1.0).Value();
            var height = FindLengthValue("Height", 1.0).Value();
            var depth = FindLengthValue("Depth", 1.0).Value();
            
            // Create shape at origin (no position yet)
            shape = new FoShape3D(name, color)
            {
                GlyphId = GetKnowId(),  // Stable UUID
                Transform = new Transform3($"{name}Transform")
            };
            
            // Create the geometry
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
            
            // Create text label as subshape
            var label3D = new FoText3D("Label", "white")
            {
                Text = "Tick: 0",
                FontSize = 0.3,
                Transform = new Transform3("LabelTransform")
                {
                    Position = new Vector3(0, height + 0.3, depth + 0.3),
                }
            };
            
            // Animate label to show tick
            label3D.BeforeAnimationRefresh((self, tick, fps) =>
            {
                if (self is FoText3D textShape)
                {
                    textShape.Text = $"Tick: {tick}";
                }
            });
            
            shape.AddShape(label3D);
            
            // Setup sinusoidal animation (FO layer - direct manipulation)
            // Read animation parameters (establishes dependencies)
            var amplitude = FindLengthValue("Amplitude", 0.5).Value();
            var frequency = FindParameterValue<double>("Frequency");
            var phaseOffset = FindParameterValue<double>("PhaseOffset");
            
            // Animation callback captures base position (set by Body later)
            // and applies oscillation directly
            var capturedBaseY = 0.0;  // Will be updated by first Body evaluation
            shape.BeforeAnimationRefresh((self, tick, fps) =>
            {
                if (self is FoShape3D s && s.Transform != null)
                {
                    // Use current Y as base on first frame
                    if (tick == 0 || capturedBaseY == 0.0)
                    {
                        capturedBaseY = s.Transform.Position.Y;
                    }
                    
                    // Apply sinusoidal oscillation
                    var newY = capturedBaseY + amplitude * Math.Sin(frequency * tick + phaseOffset);
                    var currentPos = s.Transform.Position;
                    s.Transform.MoveTo(currentPos.X, newY, currentPos.Z);
                }
            });
            
            // Cache the shape
            parameter.SetCashe(shape);
            
            $"✅ CREATE MESH: Shape created and cached (GlyphId={shape.GlyphId})".WriteSuccess();
        }
        else
        {
            // ═══════════ REUSE MODE ═══════════
            shape = parameter.GetCashe<FoShape3D>();
            $"♻️ REUSE MESH: Using cached shape (GlyphId={shape?.GlyphId})".WriteInfo();
        }

        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

    /// <summary>
    /// TRANSFORM: Calculate Transform3 from position parameters.
    /// Reads only position/rotation parameters (PositionX, PositionY, PositionZ, RotationY).
    /// Lightweight - no cache needed.
    /// </summary>
    private bool ComputeTransform3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        if (geometry == null)
            return false;

        var parameter = geometry.GetTransformParameter();
        if (parameter == null)
            return false;

        // Read position parameters (establishes Transform dependencies)
        var posX = FindLengthValue("PositionX", 0.0).Value();
        var posY = FindLengthValue("PositionY", 0.0).Value();
        var posZ = FindLengthValue("PositionZ", 0.0).Value();
        var animOffset = FindLengthValue("AnimationOffset", 0.0).Value();
        var rotY = FindParameterValue<double>("RotationY");

        $"🔧 COMPUTE TRANSFORM: Position=({posX}, {posY + animOffset}, {posZ}), RotY={rotY}".WriteInfo();

        // Create Transform3 object
        var transform = new Transform3($"{Name}_Transform");
        transform.MoveTo(posX, posY + animOffset, posZ);
        transform.Rotation = Euler.FromDegrees(0, rotY, 0);

        result.SetValue(ResultStatus.Transform3, transform);
        return true;
    }

    /// <summary>
    /// BODY: Stateless composition of Mesh + Transform.
    /// Reads Mesh and Transform parameters (establishes Body dependencies).
    /// Applies transform in-place to the cached mesh.
    /// </summary>
    private bool ComputeBody3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var geometry = context as KnGeometry;
        if (geometry == null)
            return false;

        var parameter = geometry.GetBodyParameter();
        if (parameter == null)
            return false;

        // Read Mesh parameter (may return cached shape if Valid)
        var meshResult = geometry.GetMeshParameter().GetCurrentValue();
        var shape = meshResult.ValueAs<FoShape3D>();

        if (shape == null)
        {
            "ERROR: No shape from Mesh parameter".WriteError();
            return false;
        }

        // Read Transform parameter (fresh calculation)
        var transformResult = geometry.GetTransformParameter().GetCurrentValue();
        var transform = transformResult.ValueAs<Transform3>();

        if (transform == null)
        {
            "ERROR: No transform from Transform parameter".WriteError();
            return false;
        }

        $"🎯 COMPOSE BODY: Applying transform to shape (GlyphId={shape.GlyphId})".WriteInfo();

        // Apply transform IN-PLACE (mutates mesh's Transform)
        // Property setters automatically mark shape as stale
        shape.Transform.Position = transform.Position;
        shape.Transform.Rotation = transform.Rotation;
        shape.Transform.Scale = transform.Scale;

        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
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

    // === Clock Animation Control Methods ===
    
    /// <summary>
    /// Enable clock animation - component will move around a circle and change shape at 12 o'clock
    /// </summary>
    public void EnableClockAnimation()
    {
        _clockAnimationEnabled = true;
        $"⏰ Clock animation ENABLED for {Name}".WriteSuccess();
    }
    
    /// <summary>
    /// Disable clock animation
    /// </summary>
    public void DisableClockAnimation()
    {
        _clockAnimationEnabled = false;
        $"⏰ Clock animation DISABLED for {Name}".WriteInfo();
    }
    
    /// <summary>
    /// Get current clock animation state
    /// </summary>
    public bool IsClockAnimationEnabled() => _clockAnimationEnabled;
    
    /// <summary>
    /// Configure clock animation parameters
    /// </summary>
    public void ConfigureClockAnimation(double radius, Vector3 center, int framesPerMove = 60)
    {
        _clockRadius = radius;
        _clockCenter = center;
        _framesPerMove = framesPerMove;
        $"⏰ Clock configured: radius={radius}, center=({center.X},{center.Y},{center.Z}), frames/move={framesPerMove}".WriteInfo();
    }

}
