#nullable enable
using FoundryMicroCore.Core.Extensions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Components.Pages;

/// <summary>
/// Tug of War test - demonstrates KnModel driving both 2D and 3D rendering.
/// The same KnModel parameters (RopePosition, LeftForce, RightForce) drive
/// synchronized shapes in both Canvas2D and Canvas3D views.
/// </summary>
public partial class TugOfWarTest : ComponentBase, IDisposable
{
    // Canvas references
    private Canvas2DComponent? _canvas2D;
    private Canvas3DComponent? _canvas3D;
    
    // KnModel components
    private TugOfWarModel? _model;
    private RopeComponent? _ropeComponent;
    private TeamComponent? _leftTeam;
    private TeamComponent? _rightTeam;
    
    // Current shapes
    private FoShape2D? _ropeShape2D;
    private FoShape2D? _leftMarker2D;
    private FoShape2D? _rightMarker2D;
    private FoShape2D? _centerMarker2D;
    private FoShape3D? _ropeShape3D;
    private FoShape3D? _leftMarker3D;
    private FoShape3D? _rightMarker3D;
    private FoShape3D? _centerMarker3D;
    
    // Game state
    private double _leftForce = 50;
    private double _rightForce = 50;
    private double _ropePosition = 0;  // -100 to +100, 0 is center
    
    // Display info
    private string _shape2DInfo = "Waiting...";
    private string _shape3DInfo = "Waiting...";
    private List<string> _logs = new();
    
    // Auto-play timer
    private System.Threading.Timer? _autoPlayTimer;
    private Random _random = new();
    
    protected override void OnInitialized()
    {
        InitializeModel();
        AnimationFrameBus.SubscribeToComputeGeometry(OnComputeGeometry);
        AddLog("INIT", "Tug of War model initialized");
    }
    
    private void InitializeModel()
    {
        _model = new TugOfWarModel("TugOfWar");
        
        // Create rope component
        _ropeComponent = new RopeComponent("Rope", 0);
        _model.Add<RopeComponent>(_ropeComponent);
        
        // Create team components
        _leftTeam = new TeamComponent("LeftTeam", "Red", _leftForce);
        _rightTeam = new TeamComponent("RightTeam", "Blue", _rightForce);
        _model.Add<TeamComponent>(_leftTeam);
        _model.Add<TeamComponent>(_rightTeam);
    }
    
    private void OnPage2DReady(FoPage2D page)
    {
        AddLog("2D", "Canvas2D ready - establishing geometry");
        Establish2DGeometry(page);
    }
    
    private void OnStage3DReady(FoStage3D stage)
    {
        AddLog("3D", "Canvas3D ready - establishing geometry");
        Establish3DGeometry(stage);
    }
    
    private void Establish2DGeometry(FoPage2D page)
    {
        if (_ropeComponent == null) return;
        
        // Establish rope geometry directly - no RenderContext needed for test
        var (ropeGeom, ropeParam) = _ropeComponent.EstablishGeometry2D("TugOfWar2D", null);
        
        // Get the computed shape
        var ropeValue = ropeGeom.GetMeshParameterValue();
        if (ropeValue.IsSuccess())
        {
            _ropeShape2D = ropeValue.AsShape2D();
            if (_ropeShape2D != null)
            {
                page.AddShape(_ropeShape2D);
            }
        }
        
        // Create team markers
        _leftMarker2D = new FoShape2D(40, 60, "Crimson");
        _leftMarker2D.MoveTo(100, 150);
        page.AddShape(_leftMarker2D);
        
        _rightMarker2D = new FoShape2D(40, 60, "DodgerBlue");
        _rightMarker2D.MoveTo(460, 150);
        page.AddShape(_rightMarker2D);
        
        // Center marker
        _centerMarker2D = new FoShape2D(20, 40, "Gold");
        _centerMarker2D.MoveTo(290, 160);
        page.AddShape(_centerMarker2D);
        
        Update2DInfo();
        AddLog("2D", "All 2D shapes established");
    }
    
    private void Establish3DGeometry(FoStage3D stage)
    {
        if (_ropeComponent == null) return;
        
        // Establish rope geometry directly - pass null for arena, we'll add to stage manually
        var (ropeGeom, ropeParam) = _ropeComponent.EstablishGeometry3D("TugOfWar3D");
        
        // Get the computed shape
        var ropeValue = ropeGeom.GetMeshParameterValue();
        if (ropeValue.IsSuccess())
        {
            _ropeShape3D = ropeValue.AsShape3D();
            if (_ropeShape3D != null)
            {
                stage.AddShape(_ropeShape3D);
            }
        }
        
        // Create team markers in 3D
        _leftMarker3D = new FoShape3D("LeftTeam3D");
        _leftMarker3D = _leftMarker3D.CreateBox("LeftTeam3D", 40, 60, 30);
        _leftMarker3D.Transform.Position = new Vector3(-150, 0, 0);
        _leftMarker3D.Color = "Crimson";
        stage.AddShape(_leftMarker3D);
        
        _rightMarker3D = new FoShape3D("RightTeam3D");
        _rightMarker3D = _rightMarker3D.CreateBox("RightTeam3D", 40, 60, 30);
        _rightMarker3D.Transform.Position = new Vector3(150, 0, 0);
        _rightMarker3D.Color = "DodgerBlue";
        stage.AddShape(_rightMarker3D);
        
        // Center marker in 3D
        _centerMarker3D = new FoShape3D("CenterMarker3D");
        _centerMarker3D = _centerMarker3D.CreateBox("CenterMarker3D", 20, 40, 40);
        _centerMarker3D.Transform.Position = new Vector3(0, 0, 0);
        _centerMarker3D.Color = "Gold";
        stage.AddShape(_centerMarker3D);
        
        Update3DInfo();
        AddLog("3D", "All 3D shapes established");
    }
    
    private void OnComputeGeometry(ComputeGeometryEvent evt)
    {
        UpdateShapePositions();
        InvokeAsync(StateHasChanged);
    }
    
    private void OnForceChanged(ChangeEventArgs e)
    {
        ApplyForces();
    }
    
    private void LeftPull()
    {
        _leftForce = Math.Min(100, _leftForce + 20);
        ApplyForces();
        AddLog("PULL", "Red team pulled! Force now: " + _leftForce.ToString("F0"));
    }
    
    private void RightPull()
    {
        _rightForce = Math.Min(100, _rightForce + 20);
        ApplyForces();
        AddLog("PULL", "Blue team pulled! Force now: " + _rightForce.ToString("F0"));
    }
    
    private void ApplyForces()
    {
        // Update model parameters
        _leftTeam?.FindParameter("Force")?.SetValue(_leftForce);
        _rightTeam?.FindParameter("Force")?.SetValue(_rightForce);
        
        // Calculate new rope position based on force differential
        var forceDiff = _rightForce - _leftForce;
        _ropePosition = Math.Clamp(_ropePosition + forceDiff * 0.1, -100, 100);
        
        // Update rope component
        _ropeComponent?.FindParameter("Position")?.SetValue(_ropePosition);
        
        // Smash geometries to trigger re-evaluation
        _ropeComponent?.FindGeometry(KnowledgeType.Geometry2D)?.GetMeshParameter().Smash();
        _ropeComponent?.FindGeometry(KnowledgeType.Geometry3D)?.GetMeshParameter().Smash();
        
        // Apply natural friction/decay
        _leftForce = Math.Max(30, _leftForce - 2);
        _rightForce = Math.Max(30, _rightForce - 2);
        
        UpdateShapePositions();
        AddLog("FORCE", $"L={_leftForce:F0} R={_rightForce:F0} Pos={_ropePosition:F1}");
    }
    
    private void UpdateShapePositions()
    {
        // Update 2D positions based on rope position
        if (_centerMarker2D != null)
        {
            var centerX = 290 + (int)(_ropePosition * 1.5);
            _centerMarker2D.MoveTo(centerX, 160);
        }
        
        if (_leftMarker2D != null)
        {
            var leftX = 100 + (int)(_ropePosition * 0.5);
            _leftMarker2D.MoveTo(leftX, 150);
        }
        
        if (_rightMarker2D != null)
        {
            var rightX = 460 + (int)(_ropePosition * 0.5);
            _rightMarker2D.MoveTo(rightX, 150);
        }
        
        // Update 3D positions based on rope position
        if (_centerMarker3D != null)
        {
            _centerMarker3D.Transform.Position = new Vector3((float)_ropePosition * 1.5f, 0, 0);
        }
        
        if (_leftMarker3D != null)
        {
            _leftMarker3D.Transform.Position = new Vector3(-150 + (float)_ropePosition * 0.5f, 0, 0);
        }
        
        if (_rightMarker3D != null)
        {
            _rightMarker3D.Transform.Position = new Vector3(150 + (float)_ropePosition * 0.5f, 0, 0);
        }
        
        Update2DInfo();
        Update3DInfo();
    }
    
    private void Update2DInfo()
    {
        if (_centerMarker2D != null)
        {
            _shape2DInfo = $"Center: ({_centerMarker2D.PinX}, {_centerMarker2D.PinY})";
        }
    }
    
    private void Update3DInfo()
    {
        if (_centerMarker3D != null)
        {
            var pos = _centerMarker3D.Transform.Position;
            _shape3DInfo = $"Center: ({pos.X:F0}, {pos.Y:F0}, {pos.Z:F0})";
        }
    }
    
    private void ResetGame()
    {
        _leftForce = 50;
        _rightForce = 50;
        _ropePosition = 0;
        ApplyForces();
        AddLog("RESET", "Game reset to center");
    }
    
    private void StartAutoPlay()
    {
        if (_autoPlayTimer != null)
        {
            _autoPlayTimer.Dispose();
            _autoPlayTimer = null;
            AddLog("AUTO", "Auto-play stopped");
            return;
        }
        
        _autoPlayTimer = new System.Threading.Timer(_ =>
        {
            InvokeAsync(() =>
            {
                // Random pulls from each side
                if (_random.NextDouble() > 0.5)
                {
                    _leftForce = Math.Min(100, _leftForce + _random.Next(5, 25));
                }
                else
                {
                    _rightForce = Math.Min(100, _rightForce + _random.Next(5, 25));
                }
                ApplyForces();
                StateHasChanged();
            });
        }, null, 0, 500);
        
        AddLog("AUTO", "Auto-play started");
    }
    
    private double GetLeftPercent() => Math.Max(0, 45 - _ropePosition * 0.45);
    private double GetRightPercent() => Math.Max(0, 45 + _ropePosition * 0.45);
    
    private void AddLog(string category, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _logs.Add($"[{timestamp}] [{category}] {message}");
        if (_logs.Count > 100) _logs.RemoveAt(0);
    }
    
    private void ClearLog()
    {
        _logs.Clear();
    }
    
    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromComputeGeometry(OnComputeGeometry);
        _autoPlayTimer?.Dispose();
    }
}

// ==========================================
// KnModel Components for Tug of War
// ==========================================

/// <summary>
/// The overall tug of war model containing rope and teams.
/// </summary>
public class TugOfWarModel : KnModel
{
    public TugOfWarModel(string name) : base(name)
    {
    }
}

/// <summary>
/// Rope component - its position is driven by team forces.
/// Renders as a horizontal bar in both 2D and 3D.
/// </summary>
public class RopeComponent : KnComponent
{
    public RopeComponent(string name, double initialPosition) : base(name)
    {
        Calculations([
            $"Position: {initialPosition}",
            "Length: 400",
            "Thickness: 20"
        ]);
    }
    
    public (KnGeometry, KnParameter) EstablishGeometry2D(string view, IPage2D? page)
    {
        var result = Compute2DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeRope2D", ComputeRope2D);
            
            // Geometry depends on Position parameter
            var posParam = FindParameter("Position");
            if (posParam != null)
            {
                geom.GetMeshParameter().IDependOn(posParam);
            }
        });
        return (result, result.GetMeshParameter());
    }
    
    private bool ComputeRope2D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var position = FindNumberValue("Position", 0.0);
        var length = FindNumberValue("Length", 400.0);
        var thickness = FindNumberValue("Thickness", 20.0);
        
        $"RopeComponent: Computing 2D rope at position {position}".WriteInfo();
        
        // Rope is a horizontal bar
        var shape = new FoShape2D((int)length, (int)thickness, "SaddleBrown");
        var centerX = 300 + (int)(position * 1.5);  // Center of canvas + offset
        shape.MoveTo(centerX - (int)(length / 2), 175);
        
        result.SetValue(ResultStatus.Shape2D, shape);
        return true;
    }
    
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeRope3D", ComputeRope3D);
            
            // Geometry depends on Position parameter
            var posParam = FindParameter("Position");
            if (posParam != null)
            {
                geom.GetMeshParameter().IDependOn(posParam);
            }
        });
        return (result, result.GetMeshParameter());
    }
    
    private bool ComputeRope3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var position = FindNumberValue("Position", 0.0);
        var length = FindNumberValue("Length", 400.0);
        var thickness = FindNumberValue("Thickness", 20.0);
        
        $"RopeComponent: Computing 3D rope at position {position}".WriteInfo();
        
        // Rope as a 3D box
        var shape = new FoShape3D("Rope3D");
        shape = shape.CreateBox("Rope3D", length, thickness, thickness);
        shape.Transform.Position = new Vector3((float)(position * 1.5), 0, 0);
        shape.Color = "SaddleBrown";
        
        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
}

/// <summary>
/// Team component - represents one side of the tug of war.
/// </summary>
public class TeamComponent : KnComponent
{
    public string TeamColor { get; }
    
    public TeamComponent(string name, string color, double initialForce) : base(name)
    {
        TeamColor = color;
        Calculations([
            $"Force: {initialForce}",
            "Members: 3"
        ]);
    }
}
