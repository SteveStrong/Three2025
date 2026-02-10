using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Units;

namespace FoundryWorldsAndDrawings.Blazor.Models;

/// <summary>
/// Model for ClockDemo page.
/// Demonstrates real-time 3D animation with rotating clock hands and updating digital display.
/// 
/// Features:
/// - Analog clock with hour/minute/second hands that rotate based on current time
/// - Digital display (FoText3D) that updates every second
/// - Animation driven by per-shape OnBeforeRender lifecycle hooks
/// - Commands to build, start, stop, and clear the clock
/// </summary>
[DiscoverableComponent(
    DisplayName = "Clock Demo",
    Description = "Real-time 3D clock with analog hands and digital display",
    Tags = new[] { "demo", "3d", "animation", "clock" })]
[ModelComponent(
    InitializeAction = "BuildClock",
    PrimaryActions = new[] { "BuildAndStart", "StartClock", "StopClock", "ClearClock", "SpawnTRex", "SpawnSubmarine" },
    ResetAction = "ClearClock",
    AutoRunAction = "BuildAndStart")]
public class ClockDemoModel : MxComponent
{
    private readonly IWorkspace _workspace;
    private readonly ISelectionService? _selection;
    private readonly string _sceneName;

    public ClockDemoModel(IWorkspace workspace, ISelectionService selection, string sceneName)
        : base("ClockDemo")
    {
        _workspace = workspace;
        _selection = selection;
        _sceneName = sceneName;

        SetupCommands();
        "🕐 ClockDemoModel: Ready (per-shape OnBeforeRender hooks)".WriteSuccess();
    }

    #region Clock Building

    private FoStage3D GetStage()
    {
        var arena = _workspace.GetArena();
        return arena.EstablishStage<FoStage3D>(_sceneName);
    }

    /// <summary>
    /// Attach clock animation hooks to the four animated shapes.
    /// Stateless — reads DateTime.Now each frame, so fresh delegates work any time.
    /// </summary>
    private void AttachClockAnimations(FoStage3D stage)
    {
        var (_, secondPivot) = stage.FindMember<FoShape3D>("SecondPivot");
        var (_, minutePivot) = stage.FindMember<FoShape3D>("MinutePivot");
        var (_, hourPivot) = stage.FindMember<FoShape3D>("HourPivot");
        var (_, digitalDisplay) = stage.FindMember<FoText3D>("DigitalDisplay");

        secondPivot?.OnBeforeRender((shape, tick, fps) =>
        {
            var angle = DateTime.Now.Second * 6.0;
            shape.Transform.RotateTo(0, -angle, 0, AngleUnit.Degrees);
        });

        minutePivot?.OnBeforeRender((shape, tick, fps) =>
        {
            var now = DateTime.Now;
            var angle = now.Minute * 6.0 + now.Second * 0.1;
            shape.Transform.RotateTo(0, -angle, 0, AngleUnit.Degrees);
        });

        hourPivot?.OnBeforeRender((shape, tick, fps) =>
        {
            var now = DateTime.Now;
            var angle = (now.Hour % 12) * 30.0 + now.Minute * 0.5;
            shape.Transform.RotateTo(0, -angle, 0, AngleUnit.Degrees);
        });

        digitalDisplay?.OnBeforeRender((shape, tick, fps) =>
        {
            if (shape is FoText3D textShape)
                textShape.Text = DateTime.Now.ToString("HH:mm:ss");
        });
    }

    /// <summary>
    /// Clear clock animation hooks from the four animated shapes.
    /// </summary>
    private void DetachClockAnimations(FoStage3D stage)
    {
        var (_, secondPivot) = stage.FindMember<FoShape3D>("SecondPivot");
        var (_, minutePivot) = stage.FindMember<FoShape3D>("MinutePivot");
        var (_, hourPivot) = stage.FindMember<FoShape3D>("HourPivot");
        var (_, digitalDisplay) = stage.FindMember<FoText3D>("DigitalDisplay");

        secondPivot?.ClearBeforeRender();
        minutePivot?.ClearBeforeRender();
        hourPivot?.ClearBeforeRender();
        digitalDisplay?.ClearBeforeRender();
    }

    /// <summary>
    /// Build all clock shapes on the stage
    /// </summary>
    private void BuildClockShapes()
    {
        var stage = GetStage();

        // Clear any existing shapes first
        stage.ClearAll();

        // Clock scale factor - 3x larger for better visibility
        const double scale = 3.0;

        // === CLOCK FACE (flat cylinder lying in XZ plane) ===
        var clockFace = new FoShape3D("ClockFace", "white")
        {
            Transform = { Position = new Vector3(0, 0, 0) }
        };
        clockFace.CreateCylinder("", 3.0 * scale, 0.1, 3.0 * scale);
        stage.AddShape(clockFace);

        // === HOUR NUMBERS (1-12 around the face) ===
        var hourLabels = new[] { "12", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" };
        for (int i = 0; i < 12; i++)
        {
            var angle = i * 30.0 * Math.PI / 180.0;  // 30° per hour in radians
            var radius = 2.3 * scale;
            var x = Math.Sin(angle) * radius;
            var z = -Math.Cos(angle) * radius;

            var hourText = new FoText3D($"Hour_{hourLabels[i]}", i % 3 == 0 ? "gold" : "black")
            {
                Text = hourLabels[i],
                FontSize = 0.35 * scale,
                Transform = { Position = new Vector3(x, 0.15, z) }
            };
            stage.AddShape(hourText);
        }



        // === HOUR HAND: pivot rotates at center, hand is offset child ===
        var hourLength = 1.4 * scale;
        var hourPivot = new FoShape3D("HourPivot", "darkgray")
        {
            Transform = { Position = new Vector3(0, 0.1, 0) }
        };
        hourPivot.CreateBox("", 0.01, 0.01, 0.01);  // Tiny invisible pivot
        stage.AddShape(hourPivot);

        var hourHand = new FoShape3D("HourHand", "red")
        {
            Transform = new Transform3("HourHandTransform")
            {
                Position = new Vector3(0, 0, -hourLength / 2),
            }
        };
        hourHand.CreateBox("", 0.18 * scale, 0.1, hourLength);
        hourPivot.AddShape(hourHand);

        // === MINUTE HAND: pivot rotates at center, hand is offset child ===
        var minuteLength = 2.0 * scale;
        var minutePivot = new FoShape3D("MinutePivot", "darkgray")
        {
            Transform = { Position = new Vector3(0, 0.2, 0) }
        };
        minutePivot.CreateBox("", 0.01, 0.01, 0.01);  // Tiny invisible pivot
        stage.AddShape(minutePivot);

        var minuteHand = new FoShape3D("MinuteHand", "blue")
        {
            Transform = new Transform3("MinuteHandTransform")
            {
                Position = new Vector3(0, 0, -minuteLength / 2),
            }
        };
        minuteHand.CreateBox("", 0.12 * scale, 0.08, minuteLength);
        minutePivot.AddShape(minuteHand);

        // === SECOND HAND: pivot rotates at center, hand is offset child ===
        var secondLength = 2.5 * scale;
        var secondPivot = new FoShape3D("SecondPivot", "darkgray")
        {
            Transform = { Position = new Vector3(0, 0.3, 0) }
        };
        secondPivot.CreateBox("", 0.01, 0.01, 0.01);  // Tiny invisible pivot
        stage.AddShape(secondPivot);

        var secondHand = new FoShape3D("SecondHand", "lime")
        {
            Transform = new Transform3("SecondHandTransform")
            {
                Position = new Vector3(0, 0, -secondLength / 2),
            }
        };
        secondHand.CreateBox("", 0.06 * scale, 0.05, secondLength);
        secondPivot.AddShape(secondHand);

        // === DIGITAL DISPLAY ===
        var digitalDisplay = new FoText3D("DigitalDisplay", "cyan")
        {
            Text = DateTime.Now.ToString("HH:mm:ss"),
            FontSize = 0.4 * scale,
            Transform = new Transform3("DigitalTransform")
            {
                Position = new Vector3(2.5, 0.5, -2.5),
            }
        };
        stage.AddShape(digitalDisplay);

        // Attach animation hooks — clock starts immediately
        AttachClockAnimations(stage);

        $"🕐 Clock built with {stage.AllBodies().Count()} shapes (3x scale)".WriteSuccess();
    }

    #endregion

    #region Commands Setup

    private void SetupCommands()
    {
        var editor = this.EstablishEditor<MxComponentEditor>();

        // === BUILD CLOCK ===
        editor.EstablishAction("BuildClock", action =>
        {
            try
            {
                BuildClockShapes();
                return MxActionResult.Ok("Clock built successfully");
            }
            catch (Exception ex)
            {
                $"❌ BuildClock failed: {ex.Message}".WriteError();
                return MxActionResult.Fail($"Failed: {ex.Message}");
            }
        });

        editor.EstablishCommand("BuildClock", "BuildClock", configure: cmd =>
        {
            cmd.DisplayName = "🕐 Build Clock";
            cmd.Description = "Create all clock shapes (face, hands, display)";
            cmd.Category = "Clock Controls";
            cmd.Steps = new List<string>
            {
                "Clear any existing shapes from stage",
                "Create white clock face (3x scale, radius 9)",
                "Add center post cylinder",
                "Place hour numbers 1-12 around the face",
                "Create hour hand (red), minute hand (blue), second hand (green)",
                "Add digital display in front of clock"
            };
        });

        // === START CLOCK ===
        editor.EstablishAction("StartClock", action =>
        {
            var stage = GetStage();
            var (found, _) = stage.FindMember<FoShape3D>("SecondPivot");
            if (!found)
                return MxActionResult.Fail("No clock on stage");

            AttachClockAnimations(stage);
            return MxActionResult.Ok("Clock animation started");
        });

        editor.EstablishCommand("StartClock", "StartClock", configure: cmd =>
        {
            cmd.DisplayName = "▶️ Start Clock";
            cmd.Description = "Start real-time clock animation";
            cmd.Category = "Clock Controls";
            cmd.Steps = new List<string>
            {
                "Reattach OnBeforeRender hooks to pivot shapes",
                "Hands will rotate based on current system time",
                "Digital display updates every frame"
            };
        });

        // === STOP CLOCK ===
        editor.EstablishAction("StopClock", action =>
        {
            DetachClockAnimations(GetStage());
            return MxActionResult.Ok("Clock animation stopped");
        });

        editor.EstablishCommand("StopClock", "StopClock", configure: cmd =>
        {
            cmd.DisplayName = "⏹️ Stop Clock";
            cmd.Description = "Pause clock animation";
            cmd.Category = "Clock Controls";
            cmd.Steps = new List<string>
            {
                "Detach OnBeforeRender hooks from pivot shapes",
                "Hands freeze at current position",
                "Digital display stops updating"
            };
        });

        // === CLEAR CLOCK ===
        editor.EstablishAction("ClearClock", action =>
        {
            GetStage().ClearAll();
            return MxActionResult.Ok("Clock cleared");
        });

        editor.EstablishCommand("ClearClock", "ClearClock", configure: cmd =>
        {
            cmd.DisplayName = "🗑️ Clear Clock";
            cmd.Description = "Remove all clock shapes";
            cmd.Category = "Clock Controls";
            cmd.Steps = new List<string>
            {
                "Stop all animations",
                "Remove all shapes from stage",
                "Reset T-Rex and Submarine references",
                "Reset clock state to unbuilt"
            };
        });

        // === BUILD & START (convenience) ===
        editor.EstablishAction("BuildAndStart", action =>
        {
            try
            {
                BuildClockShapes();  // Hooks are attached during build — clock starts immediately
                "🕐 Clock built and started".WriteSuccess();
                return MxActionResult.Ok("Clock built and started");
            }
            catch (Exception ex)
            {
                $"❌ BuildAndStart failed: {ex.Message}".WriteError();
                return MxActionResult.Fail($"Failed: {ex.Message}");
            }
        });

        editor.EstablishCommand("BuildAndStart", "BuildAndStart", configure: cmd =>
        {
            cmd.DisplayName = "🚀 Build & Start";
            cmd.Description = "Build clock and start animation immediately";
            cmd.Category = "Clock Controls";
            cmd.Steps = new List<string>
            {
                "Execute Build Clock (create all shapes)",
                "Execute Start Clock (enable animations)",
                "Clock hands begin rotating immediately"
            };
        });

        // === SPAWN T-REX ===
        editor.EstablishAction("SpawnTRex", action =>
        {
            var arena = _workspace.GetArena();
            var stage = arena.EstablishStage<FoStage3D>(_sceneName);

            // Already exists? Nothing to do.
            var (found, _) = stage.FindMember<FoModel3D>("TRex");
            if (found)
                return MxActionResult.Ok("T-Rex already on stage");

            // Oval patrol: ellipse with long X axis behind the clock
            double semiMajorX = 20.0;
            double semiMinorZ = 4.0;
            double zOffset = -8.0;
            double angle = 0.0;

            var tRex = new FoModel3D("TRex", "green")
            {
                Url = "storage/StaticFiles/T_Rex.glb",
                Transform = 
                { 
                    Position = new Vector3(-20, 0, -8),
                    Rotation = new Euler(0, Math.PI / 2, 0)
                }
            };

            tRex.OnBeforeRender((shape, tick, fps) =>
            {
                angle += Math.PI / 180;
                var x = semiMajorX * Math.Cos(angle);
                var z = semiMinorZ * Math.Sin(angle) + zOffset;
                shape.Transform.Position = new Vector3(x, 0, z);

                var tangentX = -semiMajorX * Math.Sin(angle);
                var tangentZ = semiMinorZ * Math.Cos(angle);
                shape.Transform.Rotation = new Euler(0, Math.Atan2(tangentX, tangentZ), 0, AngleUnit.Radians);
                shape.SetTransformStale();
            });

            stage.AddShape(tRex);
            return MxActionResult.Ok("T-Rex patrolling!");
        });

        editor.EstablishCommand("SpawnTRex", "SpawnTRex", configure: cmd =>
        {
            cmd.DisplayName = "🦖 T-Rex";
            cmd.Description = "Spawn a T-Rex that walks across the scene";
            cmd.Category = "Fun Extras";
            cmd.Steps = new List<string>
            {
                "Load T_Rex.glb model (first click only)",
                "Position behind the clock (Z=-8)",
                "Toggle oval patrol animation on/off",
                "T-Rex walks elliptical path with smooth turns"
            };
        });

        // === SPAWN SUBMARINE ===
        editor.EstablishAction("SpawnSubmarine", action =>
        {
            var arena = _workspace.GetArena();
            var stage = arena.EstablishStage<FoStage3D>(_sceneName);

            // Already exists? Nothing to do.
            var (found, _) = stage.FindMember<FoModel3D>("Submarine");
            if (found)
                return MxActionResult.Ok("Submarine already on stage");

            const double radius = 30.0;
            const double depth = -3.0;
            double angle = 0.0;

            var submarine = new FoModel3D("Submarine", "yellow")
            {
                Url = "storage/StaticFiles/sub.glb",
                Transform = 
                { 
                    Position = new Vector3(radius, depth, 0),
                    Scale = new Vector3(0.1, 0.1, 0.1)
                }
            };

            submarine.OnBeforeRender((shape, tick, fps) =>
            {
                angle += Math.PI / 120;
                if (angle >= 2 * Math.PI) angle -= 2 * Math.PI;

                var subX = radius * Math.Cos(angle);
                var subZ = radius * Math.Sin(angle);
                shape.Transform.Position = new Vector3(subX, depth, subZ);

                var dirX = -Math.Sin(angle);
                var dirZ = Math.Cos(angle);
                shape.Transform.Rotation = new Euler(0, Math.Atan2(dirX, dirZ) + Math.PI / 2, 0, AngleUnit.Radians);
                shape.SetTransformStale();
            });

            stage.AddShape(submarine);
            return MxActionResult.Ok("Submarine circling below!");
        });

        editor.EstablishCommand("SpawnSubmarine", "SpawnSubmarine", configure: cmd =>
        {
            cmd.DisplayName = "🚢 Submarine";
            cmd.Description = "Spawn a submarine that circles beneath the surface";
            cmd.Category = "Fun Extras";
            cmd.Steps = new List<string>
            {
                "Load sub.glb model (first click only)",
                "Position below the clock surface (Y=-3)",
                "Toggle circular swimming animation on/off",
                "Submarine follows circular path (radius 12)",
                "Heading rotates to follow path tangent"
            };
        });
    }

    #endregion

    #region Public API

    /// <summary>
    /// Get clock running state — true if SecondPivot has an active hook
    /// </summary>
    public bool IsRunning
    {
        get
        {
            var (found, pivot) = GetStage().FindMember<FoShape3D>("SecondPivot");
            return found && pivot.GetBeforeRender() != null;
        }
    }

    /// <summary>
    /// Get clock built state — true if SecondPivot exists on stage
    /// </summary>
    public bool IsBuilt
    {
        get
        {
            var (found, _) = GetStage().FindMember<FoShape3D>("SecondPivot");
            return found;
        }
    }

    /// <summary>
    /// Expose commands for CommandPanel
    /// </summary>
    public IEnumerable<MxCommand> AvailableCommands => this.Walk<MxCommand>().CollectAsList();

    /// <summary>
    /// Expose model for TreeView - show the arena hierarchy
    /// </summary>
    public override IEnumerable<ITreeNode> GetTreeViewChildNodes()
    {
        var arena = _workspace.GetArena();
        yield return arena;
    }

    /// <summary>
    /// Cleanup — ClearAll on stage destroys shapes and their hooks.
    /// </summary>
    public void Cleanup()
    {
        "🕐 ClockDemoModel: Cleaned up".WriteInfo();
    }

    #endregion
}
