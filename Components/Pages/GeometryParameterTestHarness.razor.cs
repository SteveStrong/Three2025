using Microsoft.AspNetCore.Components;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shared;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// Phase 0 Test Harness with Canvas3D visualization.
/// 
/// Proves the key distinction:
/// - Width/Height/Depth/GeomType changes → RECREATE geometry (new GlyphId)
/// - X/Y/Z position changes → MOVE existing geometry (same GlyphId)
/// 
/// This validates the spreadsheet model: only recreate when dependencies change.
/// </summary>
public partial class GeometryParameterTestHarness : ComponentBase, IDisposable
{
    [Inject] public IWorkspace Workspace { get; init; } = null!;
    [Inject] public IFoundryService FoundryService { get; init; } = null!;
    [Inject] public IMentorServices MentorServices { get; init; } = null!;
    [Inject] public IModelEditor ModelEditor { get; init; } = null!;

    // Canvas reference
    public Canvas3DComponent? Canvas3DReference = null;

    // Use the proper model pattern - model contains the component
    private AnimatedKnModel? _testModel;
    private AnimatedParameterTestComponent? _testComponent;

    // Animation state
    private bool _isAnimating = false;
    private int _currentTick = 0;

    // Geometry inputs (these cause recreation)
    private double _widthInput = 1.0;
    private double _heightInput = 2.0;
    private double _depthInput = 3.0;
    private string _geomTypeInput = "Box";

    // Position inputs (these just move, no recreation)
    private double _posXInput = 0.0;
    private double _posYInput = 0.0;
    private double _posZInput = 0.0;

    // Live state display
    private bool _geomIsUnknown = true;

    private string _shapeDimensions = "(not evaluated)";
    private string _shapePosition = "(0, 0, 0)";
    private int _dependsOnCount = 0;

    // Statistics
    private int _recreateCount = 0;
    private int _moveCount = 0;

    // History and logging
    private List<ShapeHistoryEntry> _shapeHistory = new();
    private List<LogEntry> _eventLog = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Create model using MentorServices - same pattern as KnModelAnimationTest
        _testModel = MentorServices.EstablishModel<AnimatedKnModel>("GeomTestHarnessModel");
        _testModel.EnsureAnimationSetup();
        
        // Create and add the test component to the model
        _testComponent = new AnimatedParameterTestComponent("TestPart", _widthInput, _heightInput, _depthInput, _geomTypeInput);
        ModelEditor.AddChild(_testModel, _testComponent);
        
        AddLog("INIT", "Created AnimatedKnModel with AnimatedParameterTestComponent - framework handles everything");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Canvas3DReference != null)
        {
            await Task.Delay(200);
            
            AddLog("INIT", "Canvas ready - framework will handle all stage linkage");
            
            await InvokeAsync(StateHasChanged);
        }
    }

    private void RefreshState()
    {
        if (_testComponent == null) return;
        
        // TODO: Re-add component metrics if needed
        // _currentTick = _testComponent.CurrentTick;
        // _recreateCount = _testComponent.RecreateCount;
        // _geomIsUnknown = _testComponent.GeomIsUnknown;
        // _shapeGuid = _testComponent.ShapeGuid;
        // _shapeDimensions = _testComponent.ShapeDimensions;
        // _dependsOnCount = _testComponent.DependsOnCount;
    }

    // ===================== GEOMETRY CHANGES (cause recreation) =====================

    private void ApplyWidth()
    {
        if (_testComponent == null) return;
        AddLog("GEOM", $"Width: {_widthInput} → triggers RECREATE");
        ModelEditor!.SetParameter(_testComponent, "Width", _widthInput, "m");
        RefreshState();
    }

    private void ApplyHeight()
    {
        if (_testComponent == null) return;
        AddLog("GEOM", $"Height: {_heightInput} → triggers RECREATE");
        ModelEditor!.SetParameter(_testComponent, "Height", _heightInput, "m");
        RefreshState();
    }

    private void ApplyDepth()
    {
        if (_testComponent == null) return;
        AddLog("GEOM", $"Depth: {_depthInput} → triggers RECREATE");
        ModelEditor!.SetParameter(_testComponent, "Depth", _depthInput, "m");
        RefreshState();
    }

    private void ApplyGeomType()
    {
        if (_testComponent == null) return;
        AddLog("GEOM", $"GeomType: {_geomTypeInput} → triggers RECREATE");
        ModelEditor!.SetParameter(_testComponent, "GeometryType", $"'{_geomTypeInput}'");
        RefreshState();
    }

    // ===================== POSITION CHANGES (just move, no recreation) =====================

    private void ApplyPosX()
    {
        if (_testComponent == null) return;
        AddLog("MOVE", $"X: → {_posXInput:F0}");
        ModelEditor!.SetParameter(_testComponent, "PositionX", _posXInput, "m");
        _moveCount++;
    }

    private void ApplyPosY()
    {
        if (_testComponent == null) return;
        AddLog("MOVE", $"Y: → {_posYInput:F0}");
        ModelEditor!.SetParameter(_testComponent, "PositionY", _posYInput, "m");
        _moveCount++;
    }

    private void ApplyPosZ()
    {
        if (_testComponent == null) return;
        AddLog("MOVE", $"Z: → {_posZInput:F0}");
        ModelEditor!.SetParameter(_testComponent, "PositionZ", _posZInput, "m");
        _moveCount++;
    }

    // ===================== CONTROL BUTTONS =====================

    private void ToggleAnimation()
    {
        if (_isAnimating)
        {
            AnimationFrameBus.PauseAllAnimations();
            _isAnimating = false;
            AddLog("CTRL", "Animation PAUSED");
        }
        else
        {
            AnimationFrameBus.ResumeAllAnimations();
            _isAnimating = true;
            AddLog("CTRL", "Animation RESUMED");
        }
    }

    private void ResetTest()
    {
        // Reset inputs
        _widthInput = 10.0;
        _heightInput = 20.0;
        _depthInput = 5.0;
        _geomTypeInput = "Box";
        _posXInput = 0;
        _posYInput = 0;
        _posZInput = 0;
        
        // Reset stats
        _recreateCount = 0;
        _moveCount = 0;
        _currentTick = 0;
        
        // Clear history
        _shapeHistory.Clear();
        _eventLog.Clear();
        
        // Recreate component with reset values
        if (_testModel != null && _testComponent != null)
        {
            ModelEditor.RemoveChild(_testModel, _testComponent);
            _testComponent = new AnimatedParameterTestComponent("TestPart", _widthInput, _heightInput, _depthInput, _geomTypeInput);
            ModelEditor.AddChild(_testModel, _testComponent);
        }
        
        AddLog("CTRL", "Test RESET - component recreated");
    }

    // ===================== LOGGING =====================

    private void AddLog(string type, string message)
    {
        _eventLog.Add(new LogEntry(type, message, _currentTick));
        if (_eventLog.Count > 200)
            _eventLog.RemoveRange(0, 50);
    }

    private void ClearLog() => _eventLog.Clear();

    private string GetLogClass(string type) => type switch
    {
        "MOVE" => "bg-success bg-opacity-10",
        "GEOM" => "bg-danger bg-opacity-25",
        _ => ""
    };

    private string GetLogBadge(string type) => type switch
    {
        "MOVE" => "bg-success",
        "GEOM" => "bg-danger",
        "CTRL" => "bg-primary",
        "INIT" => "bg-secondary",
        _ => "bg-dark"
    };

    public void Dispose()
    {
        // Framework handles cleanup
    }

    private record ShapeHistoryEntry(string GlyphId, string Info, bool IsNew, bool IsMoved);
    private record LogEntry(string Type, string Message, int Tick);
}
