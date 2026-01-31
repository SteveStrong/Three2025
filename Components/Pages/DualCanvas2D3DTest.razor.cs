using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core.Extensions;
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
/// Dual Canvas Test: Same KnModel rendered to both 2D Page and 3D Stage.
/// 
/// Proves symmetric rendering architecture:
/// - X, Y, Width, Height sync between 2D and 3D
/// - Z is 3D-only (fixed for 2D)
/// - Same parameter changes affect both visualizations
/// - Smash invalidates both geometries
/// </summary>
public partial class DualCanvas2D3DTest : ComponentBase, IDisposable
{
    [Inject] public IWorkspace Workspace { get; init; } = null!;
    [Inject] public IFoundryService FoundryService { get; init; } = null!;
    [Inject] public IMentorServices MentorServices { get; init; } = null!;

    // Canvas references
    public Canvas2DComponent? Canvas2DReference = null;
    public Canvas3DComponent? Canvas3DReference = null;
    private FoPage2D? _page;
    private FoStage3D? _stage;

    // Test component - single model driving both views
    private DualViewComponent? _testComponent;
    
    // 2D geometry
    private KnGeometry? _geometry2D;
    private KnParameter? _geomParam2D;
    private FoShape2D? _currentShape2D;
    
    // 3D geometry
    private KnGeometry? _geometry3D;
    private KnParameter? _geomParam3D;
    private FoShape3D? _currentShape3D;

    // Shared inputs (sync between 2D and 3D)
    private double _widthInput = 80.0;
    private double _heightInput = 60.0;
    private double _posXInput = 200.0;
    private double _posYInput = 150.0;
    private double _posZInput = 0.0;  // 3D only

    // State display
    private bool _is2DUnknown = true;
    private bool _is3DUnknown = true;
    private string _shape2DInfo = "(not created)";
    private string _shape3DInfo = "(not created)";
    private int _currentTick = 0;
    private int _recreate2DCount = 0;
    private int _recreate3DCount = 0;


    // Logging
    private List<LogEntry> _eventLog = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitializeTest();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Wait for canvases to be ready
            await Task.Delay(200);
            
            _page = Canvas2DReference?.Page;
            _stage = Canvas3DReference?.Stage;
            
            AddLog("INIT", $"2D Page: {_page?.Name ?? "null"}, 3D Stage: {_stage?.Name ?? "null"}");
            
            // Render initial geometry using proper KN→FO pipeline
            RenderInitialGeometry();
            
            // Subscribe to animation events for ongoing updates
            AnimationFrameBus.SubscribeToComputeGeometry(OnComputeGeometry);
            AnimationFrameBus.ResumeAllAnimations();
            
            AddLog("INIT", "Initial geometry rendered, animation started");
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <summary>
    /// Render initial geometry to both 2D and 3D canvases.
    /// Called once in OnAfterRenderAsync.
    /// </summary>
    private void RenderInitialGeometry()
    {
        if (_testComponent == null || _page == null || _stage == null) return;

        // Render 2D geometry - cleaner API using page name directly
        var ctx2D = RenderContext2D.CreateFromPage(_page);
        _testComponent.RenderGeometry2D(ctx2D);
        
        var result2D = _geomParam2D?.PeekValue();
        var shape2D = result2D?.AsShape2D();
        if (shape2D != null)
        {
            _currentShape2D = shape2D;
            AddLog("2D", $"Initial 2D shape created: {shape2D.Name}");
        }

        // Render 3D geometry - cleaner API using stage name directly
        var ctx3D = RenderContext3D.CreateFromStage(_stage);
        _testComponent.RenderGeometry3D(ctx3D);
        
        var result3D = _geomParam3D?.PeekValue();
        var shape3D = result3D?.AsShape3D();
        if (shape3D != null)
        {
            _currentShape3D = shape3D;
            AddLog("3D", $"Initial 3D shape created: {shape3D.GlyphId.Substring(0, 8)}...");
        }

        RefreshState();
    }

    private void InitializeTest()
    {
        // Create a single component that will render to both 2D and 3D
        _testComponent = new DualViewComponent("DualShape", _widthInput, _heightInput, _posXInput, _posYInput, _posZInput);
        
        // Establish both geometries (but don't evaluate yet)
        (_geometry2D, _geomParam2D) = _testComponent.EstablishGeometry2D("DualTest2D", null);
        (_geometry3D, _geomParam3D) = _testComponent.EstablishGeometry3D("DualTest3D");
        
        AddLog("INIT", "Created DualViewComponent with 2D and 3D geometries");
        RefreshState();
    }

    private void OnComputeGeometry(ComputeGeometryEvent evt)
    {
        if (_page == null || _stage == null) return;
        
        _currentTick = evt.tick;
        
        // Process 2D geometry
        if (_geomParam2D != null)
        {
            var was2DUnknown = _geomParam2D.IsUnknown();
            var shape2D = _geomParam2D.GetCurrentValueAs<FoShape2D>();
            
            if (shape2D != null && _currentShape2D != shape2D)
            {
                _currentShape2D = shape2D;
                _page.Add(shape2D);
                
                if (was2DUnknown)
                {
                    _recreate2DCount++;
                    AddLog("2D", $"RECREATED shape: {shape2D.Name}");
                }
            }
        }
        
        // Process 3D geometry
        if (_geomParam3D != null)
        {
            var was3DUnknown = _geomParam3D.IsUnknown();
            var shape3D = _geomParam3D.GetCurrentValueAs<FoShape3D>();
            
            if (shape3D != null && _currentShape3D != shape3D)
            {
                _currentShape3D = shape3D;
                _stage.AddShape(shape3D);
                
                if (was3DUnknown)
                {
                    _recreate3DCount++;
                    AddLog("3D", $"RECREATED shape: {shape3D.GlyphId.Substring(0, 8)}...");
                }
            }
        }
        
        RefreshState();
        InvokeAsync(StateHasChanged);
    }

    private void RefreshState()
    {
        _is2DUnknown = _geomParam2D?.IsUnknown() ?? true;
        _is3DUnknown = _geomParam3D?.IsUnknown() ?? true;
        
        if (_currentShape2D != null)
        {
            _shape2DInfo = $"{_currentShape2D.Width}×{_currentShape2D.Height} at ({_currentShape2D.PinX},{_currentShape2D.PinY})";
        }
        
        if (_currentShape3D != null)
        {
            var pos = _currentShape3D.Transform.Position;
            _shape3DInfo = $"{_currentShape3D.Width:F0}×{_currentShape3D.Height:F0} at ({pos.X:F0},{pos.Y:F0},{pos.Z:F0})";
        }
    }

    private void ApplyChanges()
    {
        if (_testComponent == null) return;
        
        // Update all shared parameters
        _testComponent.FindParameter("Width")?.SetValue(_widthInput);
        _testComponent.FindParameter("Height")?.SetValue(_heightInput);
        _testComponent.FindParameter("PosX")?.SetValue(_posXInput);
        _testComponent.FindParameter("PosY")?.SetValue(_posYInput);
        _testComponent.FindParameter("PosZ")?.SetValue(_posZInput);
        
        AddLog("PARAM", $"Applied: W={_widthInput}, H={_heightInput}, X={_posXInput}, Y={_posYInput}, Z={_posZInput}");
        RefreshState();
    }

    private void SmashGeometry()
    {
        _geomParam2D?.Smash();
        _geomParam3D?.Smash();
        
        AddLog("SMASH", "Smashed both 2D and 3D geometries");
        RefreshState();
    }

    private void ResetTest()
    {
        AnimationFrameBus.UnSubscribeFromComputeGeometry(OnComputeGeometry);
        
        // Clear shapes
        if (_currentShape2D != null && _page != null)
        {
            _page.Remove(_currentShape2D);
        }
        if (_currentShape3D != null && _stage != null)
        {
            _currentShape3D.SetShouldDelete();
            _stage.RemoveShape(_currentShape3D);
        }
        
        // Reset inputs
        _widthInput = 80.0;
        _heightInput = 60.0;
        _posXInput = 200.0;
        _posYInput = 150.0;
        _posZInput = 0.0;
        
        // Reset stats
        _recreate2DCount = 0;
        _recreate3DCount = 0;
        _currentShape2D = null;
        _currentShape3D = null;
        _eventLog.Clear();
        
        InitializeTest();
        
        AnimationFrameBus.SubscribeToComputeGeometry(OnComputeGeometry);
        AddLog("CTRL", "Test RESET");
    }

    private void AddLog(string type, string message)
    {
        _eventLog.Add(new LogEntry(type, message, _currentTick));
        if (_eventLog.Count > 100) _eventLog.RemoveRange(0, 50);
    }

    private string GetLogBadge(string type) => type switch
    {
        "2D" => "bg-primary",
        "3D" => "bg-success",
        "PARAM" => "bg-warning text-dark",
        "SMASH" => "bg-danger",
        "INIT" => "bg-secondary",
        "CTRL" => "bg-info",
        _ => "bg-dark"
    };

    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromComputeGeometry(OnComputeGeometry);
    }

    private record LogEntry(string Type, string Message, int Tick);
}

/// <summary>
/// Component that renders to both 2D and 3D views.
/// Parameters are shared: Width, Height, PosX, PosY
/// PosZ is 3D-only.
/// </summary>
public class DualViewComponent : KnComponent
{
    public DualViewComponent(string name, double width, double height, double posX, double posY, double posZ) : base(name)
    {
        Calculations([
            $"Width: {width}",
            $"Height: {height}",
            $"PosX: {posX}",
            $"PosY: {posY}",
            $"PosZ: {posZ}"
        ]);
    }

    // ===== 2D GEOMETRY =====
    
    public (KnGeometry, KnParameter) EstablishGeometry2D(string view, IArena? arena)
    {
        var result = Compute2DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeDual2D", ComputeShape2D);
            SetupDependencies(geom.GetMeshParameter(), include3D: false);
        });
        return (result, result.GetMeshParameter());
    }

    private bool ComputeShape2D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var width = FindNumberValue("Width", 80.0);
        var height = FindNumberValue("Height", 60.0);
        var posX = FindNumberValue("PosX", 200.0);
        var posY = FindNumberValue("PosY", 150.0);

        $"DualViewComponent: Computing 2D shape - W={width}, H={height}, at ({posX},{posY})".WriteSuccess();

        var shape = new FoShape2D((int)width, (int)height, "CornflowerBlue");
        shape.MoveTo((int)posX, (int)posY);

        result.SetValue(ResultStatus.Shape2D, shape);
        return true;
    }

    // ===== 3D GEOMETRY =====
    
    public override (KnGeometry, KnParameter) EstablishGeometry3D(string view)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            geom.ApplyMeshMethod("ComputeDual3D", ComputeShape3D);
            SetupDependencies(geom.GetMeshParameter(), include3D: true);
        });
        return (result, result.GetMeshParameter());
    }

    private bool ComputeShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var width = FindNumberValue("Width", 80.0);
        var height = FindNumberValue("Height", 60.0);
        var depth = 20.0;  // Fixed depth for 3D
        var posX = FindNumberValue("PosX", 200.0);
        var posY = FindNumberValue("PosY", 150.0);
        var posZ = FindNumberValue("PosZ", 0.0);

        $"DualViewComponent: Computing 3D shape - W={width}, H={height}, D={depth}, at ({posX},{posY},{posZ})".WriteSuccess();

        var shape = new FoShape3D($"Dual3D_{Name}")
        {
            Width = width,
            Height = height,
            Depth = depth
        };
        shape = shape.CreateBox(shape.Name!, width, height, depth);
        shape.Transform.Position = new Vector3(posX, posY, posZ);

        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }

    // ===== SHARED DEPENDENCY SETUP =====
    
    private void SetupDependencies(KnParameter geomParam, bool include3D)
    {
        // Shared dependencies: Width, Height, PosX, PosY
        var widthParam = FindParameter("Width");
        var heightParam = FindParameter("Height");
        var posXParam = FindParameter("PosX");
        var posYParam = FindParameter("PosY");

        if (widthParam != null) geomParam.IDependOn(widthParam);
        if (heightParam != null) geomParam.IDependOn(heightParam);
        if (posXParam != null) geomParam.IDependOn(posXParam);
        if (posYParam != null) geomParam.IDependOn(posYParam);

        // 3D-only: PosZ
        if (include3D)
        {
            var posZParam = FindParameter("PosZ");
            if (posZParam != null) geomParam.IDependOn(posZParam);
        }

        $"DualViewComponent: Dependencies set up (include3D={include3D}) - DependsOn count = {geomParam.DependsOn?.Count ?? 0}".WriteInfo();
    }
}
