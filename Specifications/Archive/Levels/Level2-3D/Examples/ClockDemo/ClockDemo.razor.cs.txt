#nullable enable

using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

namespace Three2025.Components.Pages;

/// <summary>
/// ClockDemo - Clean MxObject Reference Implementation
/// 
/// Demonstrates proper FoundryMicroCore patterns:
/// - MxObject lifecycle management
/// - Dedicated stage per component
/// - Clean disposal and resource management
/// - Animation integration with performance monitoring
/// - Interactive shape creation and management
/// </summary>
public partial class ClockDemoBase : ComponentBase, IDisposable
{
    [Inject] public required IFoundryService FoundryService { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    // Core Components - Clean MxObject Architecture
    protected Canvas3DComponent? _canvasRef;
    protected FoStage3D? _clockStage;
    protected ClockDemoTech? _clockTech;
    private MockDataGenerator _dataGenerator = new();

    // Animation & Performance Tracking
    protected bool _animationActive = false;
    protected double _currentFps = 0.0;
    protected int _currentTick = 0;

    // Component State
    protected bool _clockRunning = false;
    protected int _shapeCounter = 0;

    protected override async Task OnInitializedAsync()
    {
        // Initialize workspace
        Workspace.SetBaseUrl(Navigation?.BaseUri ?? "");
        
        // Create dedicated stage for this component - MxObject Pattern
        var arena = FoundryService.Arena();
        _clockStage = arena.EstablishStage<FoStage3D>("ClockDemoStage");
        
        "ClockDemo: Stage created with clean MxObject architecture".WriteSuccess();
        
        // Initialize tech components
        _clockTech = new ClockDemoTech("ClockDemoTech", FoundryService);
        
        // Subscribe to animation bus - Clean resource management
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
        _animationActive = true;
        
        "ClockDemo: Animation subscription established".WriteInfo();
        
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Allow canvas to initialize
            await Task.Delay(100);
            
            if (_canvasRef != null)
            {
                var (found, scene) = _canvasRef.GetActiveScene();
                if (found && scene != null)
                {
                    // Link our dedicated stage to the scene - Stage-centric pattern
                    // TODO: Replace with correct stage-to-scene linking API
                    // _clockStage?.LinkToScene(scene);
                    $"ClockDemo: Stage '{_clockStage?.Name}' ready for scene '{scene.Name}' (linking deferred)".WriteSuccess();
                    
                    // Auto-start demonstration
                    await StartClockDemo();
                }
                else
                {
                    "ClockDemo: Failed to get active scene from canvas".WriteWarning();
                }
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    #region Animation & Performance Monitoring

    private void OnAnimationFrame(AnimationEvent evt)
    {
        if (evt.IsWorld3D())
        {
            _currentFps = evt.fps;
            _currentTick = evt.tick;
            
            // Optional: Performance warnings using MxObject logging
            if (_currentFps < 30 && _currentTick % 60 == 0) // Check every 60 frames
            {
                $"Performance warning: FPS dropped to {_currentFps:F1}".WriteWarning();
            }
            
            InvokeAsync(StateHasChanged);
        }
    }

    #endregion

    #region Clock Controls - Clean MxObject Implementation

    protected async Task StartClockDemo()
    {
        if (_clockTech != null && _clockStage != null && !_clockRunning)
        {
            await _clockTech.StartClock(_clockStage);
            _clockRunning = true;
            "ClockDemo: Clock started successfully".WriteSuccess();
            StateHasChanged();
        }
    }

    protected void StartClock()
    {
        _ = StartClockDemo();
    }

    protected void StopClock()
    {
        if (_clockTech != null && _clockRunning)
        {
            _clockTech.StopClock();
            _clockRunning = false;
            "ClockDemo: Clock stopped".WriteInfo();
            StateHasChanged();
        }
    }

    protected void ToggleClockStyle()
    {
        _clockTech?.ToggleClockStyle();
        "ClockDemo: Clock style toggled".WriteInfo();
    }

    #endregion

    #region Shape Gallery - Interactive Demonstrations

    protected void AddRandomText()
    {
        if (_clockStage == null) return;

        var textShape = new FoText3D($"Text_{++_shapeCounter:D3}")
        {
            Text = _dataGenerator.RandomSentence(),
            Color = _dataGenerator.RandomColor(),
            FontSize = _dataGenerator.RandomDouble(1.0, 3.0),
            Transform = new Transform3("RandomTextTransform")
            {
                Position = new Vector3(
                    _dataGenerator.RandomDouble(-8, 8),
                    _dataGenerator.RandomDouble(3, 7), 
                    _dataGenerator.RandomDouble(-8, 8)
                )
            }
        };
        
        _clockStage.AddShape(textShape);
        $"Added random text '{textShape.Name}': '{textShape.Text}' at {textShape.Transform.Position}".WriteInfo();
    }

    protected void AddTRex()
    {
        if (_clockStage == null) return;

        var tRex = new FoModel3D($"TRex_{++_shapeCounter:D3}")
        {
            Url = GetAssetPath("models/TRex.glb"),
            Transform = new Transform3("TRexTransform")
            {
                Position = new Vector3(
                    _dataGenerator.RandomDouble(-6, 6), 
                    0, 
                    _dataGenerator.RandomDouble(-6, 6)
                ),
                Scale = new Vector3(0.3, 0.3, 0.3)
            }
        };
        
        _clockStage.AddShape(tRex);
        $"Added T-Rex '{tRex.Name}' at {tRex.Transform.Position}".WriteSuccess();
    }

    protected void AddSubmarine()
    {
        if (_clockStage == null) return;

        var submarine = new FoModel3D($"Sub_{++_shapeCounter:D3}")
        {
            Url = GetAssetPath("models/submarine.glb"),
            Transform = new Transform3("SubmarineTransform")
            {
                Position = new Vector3(
                    _dataGenerator.RandomDouble(-5, 5), 
                    2, 
                    _dataGenerator.RandomDouble(-5, 5)
                ),
                Scale = new Vector3(0.5, 0.5, 0.5)
            }
        };
        
        _clockStage.AddShape(submarine);
        $"Added Submarine '{submarine.Name}' at {submarine.Transform.Position}".WriteSuccess();
    }

    protected void AddGeometry()
    {
        if (_clockStage == null) return;

        // Create random geometric shape
        var geometryTypes = new[] { "Box", "Sphere", "Cylinder" };
        var geometryType = geometryTypes[_dataGenerator.RandomInt(0, geometryTypes.Length - 1)];
        
        var shape = new FoShape3D($"{geometryType}_{++_shapeCounter:D3}")
        {
            Color = _dataGenerator.RandomColor(),
            Transform = new Transform3("GeometryTransform")
            {
                Position = new Vector3(
                    _dataGenerator.RandomDouble(-7, 7),
                    _dataGenerator.RandomDouble(1, 5),
                    _dataGenerator.RandomDouble(-7, 7)
                ),
                Scale = new Vector3(
                    _dataGenerator.RandomDouble(0.5, 2.0),
                    _dataGenerator.RandomDouble(0.5, 2.0),
                    _dataGenerator.RandomDouble(0.5, 2.0)
                )
            }
        };
        
        _clockStage.AddShape(shape);
        $"Added {geometryType} geometry '{shape.Name}' at {shape.Transform.Position}".WriteInfo();
    }

    protected void AddAxis()
    {
        if (_clockStage == null) return;

        var axis = new FoModel3D($"Axis_{++_shapeCounter:D3}")
        {
            Url = GetAssetPath("models/fiveMeterAxis.glb"),
            Transform = new Transform3("AxisTransform")
            {
                Position = new Vector3(0, 0, 0)
            }
        };
        
        _clockStage.AddShape(axis);
        $"Added coordinate axis '{axis.Name}'".WriteSuccess();
    }

    #endregion

    #region Scene Management

    protected void ClearShapes()
    {
        if (_clockStage != null)
        {
            var shapeCount = GetShapeCount();
            // TODO: Replace with correct stage clearing API
            // _clockStage.ClearAllShapes();
            _shapeCounter = 0;
            $"ClockDemo: Would clear {shapeCount} shapes from stage (method disabled)".WriteInfo();
            StateHasChanged();
        }
    }

    protected void ResetCamera()
    {
        // TODO: Implement camera reset functionality
        "Camera reset requested (not yet implemented)".WriteInfo();
    }

    protected void ShowStageInfo()
    {
        if (_clockStage != null)
        {
            var info = $"Stage: {_clockStage.Name}, Shapes: {GetShapeCount()}, Scene: {_canvasRef?.SceneName ?? "Unknown"}";
            info.WriteInfo();
        }
    }

    #endregion

    #region Utility Methods

    protected int GetShapeCount()
    {
        // TODO: Implement proper shape counting with new collection API
        // For now, use counter as placeholder
        return _shapeCounter;
    }

    private string GetAssetPath(string relativePath)
    {
        var baseUrl = Navigation?.BaseUri ?? "";
        return $"{baseUrl}storage/StaticFiles/{relativePath}";
    }

    #endregion

    #region Lifecycle & Disposal - Clean MxObject Patterns

    public void Dispose()
    {
        try
        {
            // Stop clock operations
            if (_clockTech != null && _clockRunning)
            {
                _clockTech.StopClock();
                _clockRunning = false;
                "ClockDemo: Clock stopped during disposal".WriteInfo();
            }

            // Clear only our shapes - Stage isolation pattern
            if (_clockStage != null)
            {
                var shapeCount = GetShapeCount();
                // TODO: Replace with correct stage clearing API
                // _clockStage.ClearAllShapes();
                $"ClockDemo: Would clear {shapeCount} shapes during disposal (method disabled)".WriteInfo();
            }

            // Unsubscribe from animation - Clean resource management
            if (_animationActive)
            {
                AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
                _animationActive = false;
                "ClockDemo: Animation subscription removed".WriteInfo();
            }

            // Dispose tech components
            _clockTech?.Dispose();
            
            "ClockDemo: Component disposed cleanly with MxObject patterns".WriteSuccess();
        }
        catch (Exception ex)
        {
            $"ClockDemo disposal error: {ex.Message}".WriteError();
        }
    }

    #endregion
}

/// <summary>
/// ClockDemo Tech Component - MxObject-based Clock Creation and Animation
/// 
/// Demonstrates hierarchical 3D object creation with time-based animation
/// using clean FoundryMicroCore patterns.
/// </summary>
public class ClockDemoTech : MxComponent, IDisposable
{
    private readonly IFoundryService _foundryService;
    private Timer? _clockTimer;
    private FoShape3D? _clockAssembly;
    private FoShape3D? _hourHand;
    private FoShape3D? _minuteHand;
    private FoShape3D? _secondHand;
    private FoText3D? _digitalDisplay;
    private bool _modernStyle = true;

    public ClockDemoTech(string name, IFoundryService foundryService) : base(name)
    {
        _foundryService = foundryService;
    }

    public async Task StartClock(FoStage3D targetStage)
    {
        if (_clockAssembly == null)
        {
            _clockAssembly = CreateClockAssembly();
            targetStage.AddShape(_clockAssembly);
            $"ClockTech: Clock assembly created and added to stage '{targetStage.Name}'".WriteSuccess();
        }

        // Start timer for real-time updates
        _clockTimer = new Timer(UpdateClockHands, null, 0, 1000);
        $"ClockTech: Clock animation started with 1-second updates".WriteInfo();
    }

    public void StopClock()
    {
        _clockTimer?.Dispose();
        _clockTimer = null;
        "ClockTech: Clock animation stopped".WriteInfo();
    }

    public void ToggleClockStyle()
    {
        _modernStyle = !_modernStyle;
        $"ClockTech: Clock style changed to {(_modernStyle ? "modern" : "classic")}".WriteInfo();
        
        // TODO: Implement style changes
        if (_clockAssembly != null)
        {
            _clockAssembly.Color = _modernStyle ? "Silver" : "Gold";
        }
    }

    private FoShape3D CreateClockAssembly()
    {
        // Create hierarchical clock assembly
        var assembly = new FoShape3D("ClockAssembly");

        // Clock face (base)
        var clockFace = new FoShape3D("ClockFace")
        {
            Color = _modernStyle ? "Silver" : "Gold",
            Transform = new Transform3("ClockFaceTransform")
            {
                Position = new Vector3(0, 0.1, 0),
                Scale = new Vector3(6, 0.2, 6)
            }
        };

        // Center post
        var centerPost = new FoShape3D("CenterPost")
        {
            Color = "Black",
            Transform = new Transform3("CenterPostTransform")
            {
                Position = new Vector3(0, 1, 0),
                Scale = new Vector3(0.1, 2, 0.1)
            }
        };

        // Hour hand
        _hourHand = new FoShape3D("HourHand")
        {
            Color = "Red",
            Transform = new Transform3("HourHandTransform")
            {
                Position = new Vector3(0, 1.5, 0),
                Scale = new Vector3(2, 0.1, 0.1)
            }
        };

        // Minute hand
        _minuteHand = new FoShape3D("MinuteHand")
        {
            Color = "Blue",
            Transform = new Transform3("MinuteHandTransform")
            {
                Position = new Vector3(0, 1.6, 0),
                Scale = new Vector3(3, 0.05, 0.05)
            }
        };

        // Second hand
        _secondHand = new FoShape3D("SecondHand")
        {
            Color = "Green",
            Transform = new Transform3("SecondHandTransform")
            {
                Position = new Vector3(0, 1.7, 0),
                Scale = new Vector3(3.5, 0.02, 0.02)
            }
        };

        // Digital display
        _digitalDisplay = new FoText3D("DigitalDisplay")
        {
            Text = DateTime.Now.ToString("HH:mm:ss"),
            Color = "White",
            FontSize = 1.0,
            Transform = new Transform3("DisplayTransform")
            {
                Position = new Vector3(0, 3, 0)
            }
        };

        // Assemble hierarchy
        assembly.AddShape(clockFace);
        assembly.AddShape(centerPost);
        assembly.AddShape(_hourHand);
        assembly.AddShape(_minuteHand);
        assembly.AddShape(_secondHand);
        assembly.AddShape(_digitalDisplay);

        "ClockTech: Hierarchical clock assembly created with all components".WriteInfo();
        return assembly;
    }

    private void UpdateClockHands(object? state)
    {
        try
        {
            var now = DateTime.Now;
            
            // Calculate angles (0° = 12 o'clock position, clockwise)
            var secondAngle = (now.Second * 6) - 90;  // 6° per second
            var minuteAngle = (now.Minute * 6 + now.Second * 0.1) - 90;  // 6° per minute + smooth seconds
            var hourAngle = ((now.Hour % 12) * 30 + now.Minute * 0.5) - 90; // 30° per hour + smooth minutes
            
            // Convert to radians
            var secondRad = MathF.PI * secondAngle / 180;
            var minuteRad = MathF.PI * minuteAngle / 180;
            var hourRad = MathF.PI * hourAngle / 180;
            
            // Apply rotations to hands (rotate around Y axis)
            _secondHand?.Transform.RotateTo(0, secondRad, 0, AngleUnit.Radians);
            _minuteHand?.Transform.RotateTo(0, minuteRad, 0, AngleUnit.Radians);
            _hourHand?.Transform.RotateTo(0, hourRad, 0, AngleUnit.Radians);
            
            // Update digital display
            if (_digitalDisplay != null)
            {
                _digitalDisplay.Text = now.ToString("HH:mm:ss");
            }
        }
        catch (Exception ex)
        {
            $"ClockTech: Error updating clock hands: {ex.Message}".WriteError();
        }
    }

    public new void Dispose()
    {
        _clockTimer?.Dispose();
        _clockTimer = null;
        "ClockTech: Disposed cleanly".WriteInfo();
    }
}