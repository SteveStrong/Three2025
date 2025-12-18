using Microsoft.AspNetCore.Components;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using System.Timers;
using Timer = System.Timers.Timer;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// Phase 0 Test Harness: Proves KN layer spreadsheet mechanics via timer-driven evaluation.
/// 
/// Key behaviors demonstrated:
/// 1. Geometry stays KNOWN (stable) across many ticks - no recalculation
/// 2. Changing a parameter (Width/Height) triggers Smash cascade → geometry becomes UNKNOWN
/// 3. Next tick evaluates the UNKNOWN geometry → creates NEW shape
/// 4. Geometry returns to KNOWN state, stays stable again
/// 
/// This proves the spreadsheet model: pull-based, dependency-driven, idempotent.
/// Uses a simple timer since we don't need a canvas for KN layer testing.
/// </summary>
public partial class GeometryParameterTestHarness : ComponentBase, IDisposable
{
    [Inject] public IMentorServices MentorServices { get; init; } = null!;

    // Test component and geometry
    private TestHarnessComponent? _testComponent;
    private KnGeometry? _geometry;
    private KnGeometryParameter? _geomParam;

    // Timer for driving evaluation (simulates animation loop without canvas)
    private Timer? _evaluationTimer;
    private bool _isAnimating = false;
    private int _currentTick = 0;

    // Parameter inputs
    private double _widthInput = 10.0;
    private double _heightInput = 20.0;
    private double _widthValue = 10.0;
    private double _heightValue = 20.0;

    // Live state display
    private bool _geomIsUnknown = true;
    private bool _geomCacheEmpty = true;
    private bool _shapeExists = false;
    private string? _shapeGuid;
    private string _shapeDimensions = "(not evaluated)";
    private int _dependsOnCount = 0;

    // Statistics
    private int _evalCount = 0;
    private int _noWorkCount = 0;
    private int _recreateCount = 0;
    private string? _lastShapeGuid;

    // History and logging
    private List<ShapeHistoryEntry> _shapeHistory = new();
    private List<LogEntry> _eventLog = new();
    private bool _showAllTicks = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitializeTest();
    }

    private void InitializeTest()
    {
        // Create a fresh test component
        _testComponent = new TestHarnessComponent("TestPart", _widthValue, _heightValue);
        
        // Establish geometry (but DON'T evaluate yet - that happens in animation loop)
        (_geometry, _geomParam) = _testComponent.EstablishGeometry3D("TestView", null);
        
        AddLog("INIT", "Created TestPart component");
        AddLog("INIT", $"Geometry established with View={_geometry?.View}");
        AddLog("INIT", "Geometry is UNKNOWN - waiting for animation to evaluate");
        
        RefreshState();
    }

    private void ToggleAnimation()
    {
        if (_isAnimating)
        {
            // Stop the timer
            _evaluationTimer?.Stop();
            _evaluationTimer?.Dispose();
            _evaluationTimer = null;
            _isAnimating = false;
            AddLog("CTRL", "Animation STOPPED");
        }
        else
        {
            // Start a timer that fires every 100ms (10 Hz - plenty for testing)
            _evaluationTimer = new Timer(100);
            _evaluationTimer.Elapsed += OnTimerTick;
            _evaluationTimer.AutoReset = true;
            _evaluationTimer.Start();
            _isAnimating = true;
            AddLog("CTRL", "Animation STARTED - timer-driven evaluation loop");
        }
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Called by timer (not canvas animation). This drives the "pull" evaluation.
    /// We use a timer instead of AnimationFrameBus because that requires a Canvas.
    /// </summary>
    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        if (_geomParam == null) return;

        _currentTick++;
        _evalCount++;

        // Check state BEFORE evaluation
        var wasUnknown = _geomParam.IsUnknown();
        var wasCacheEmpty = _geomParam.IsCasheEmpty();
        var previousGuid = _shapeGuid;

        // Debug: Log state before evaluation
        if (_currentTick % 50 == 1) // Every 50 ticks to reduce noise
        {
            $"Tick {_currentTick}: Before eval - IsUnknown={wasUnknown}, CacheEmpty={wasCacheEmpty}".WriteInfo();
        }

        // This is the key operation - the "pull" that triggers calculation if needed
        // If Known: returns cached value immediately (no work)
        // If Unknown: evaluates formula, creates new shape AND stores in cache
        // NOTE: Must use GetCurrentValueAs<T>() to populate the _cashe field!
        var shape = _geomParam.GetCurrentValueAs<FoShape3D>();

        // Debug: Log state after evaluation
        var isNowUnknown = _geomParam.IsUnknown();
        var isCacheNowEmpty = _geomParam.IsCasheEmpty();
        if (_currentTick % 50 == 1 || wasUnknown)
        {
            $"Tick {_currentTick}: After eval - IsUnknown={isNowUnknown}, CacheEmpty={isCacheNowEmpty}, Shape={shape?.GlyphId ?? "null"}".WriteInfo();
        }

        // Refresh state AFTER evaluation
        RefreshState();

        // Track what happened
        if (wasUnknown)
        {
            // We did real work - created a new shape
            _recreateCount++;
            
            var isNewShape = _shapeGuid != _lastShapeGuid;
            _shapeHistory.Add(new ShapeHistoryEntry(_shapeGuid ?? "null", _shapeDimensions, _currentTick, isNewShape));
            _lastShapeGuid = _shapeGuid;
            
            AddLog("EVAL", $"Was UNKNOWN → evaluated → NEW shape: {_shapeGuid}");
        }
        else
        {
            // No work needed - value was already known
            _noWorkCount++;
            
            if (_showAllTicks)
            {
                AddLog("TICK", $"Already KNOWN → no work (shape: {_shapeGuid})");
            }
        }

        InvokeAsync(StateHasChanged);
    }

    private void RefreshState()
    {
        if (_geomParam == null) return;

        _geomIsUnknown = _geomParam.IsUnknown();
        _geomCacheEmpty = _geomParam.IsCasheEmpty();
        
        // Get shape info if it exists
        var shape = _geomParam.GetCashe<FoShape3D>();
        _shapeExists = shape != null;
        
        if (shape != null)
        {
            _shapeGuid = shape.GlyphId.Length > 8 ? shape.GlyphId.Substring(0, 8) + "..." : shape.GlyphId;
            _shapeDimensions = $"{shape.Width:F1} × {shape.Height:F1} × {shape.Depth:F1}";
        }
        else
        {
            _shapeGuid = null;
            _shapeDimensions = "(not evaluated)";
        }

        // Dependency info
        _dependsOnCount = _geomParam.DependsOn?.Count ?? 0;
    }

    private void ApplyWidth()
    {
        if (_testComponent == null) return;

        var widthParam = _testComponent.FindParameter("Width");
        if (widthParam == null)
        {
            AddLog("ERROR", "Width parameter not found!");
            return;
        }

        var oldValue = _widthValue;
        _widthValue = _widthInput;
        
        AddLog("CHANGE", $"Setting Width: {oldValue} → {_widthValue}");
        
        // This triggers the dependency cascade!
        widthParam.SetValue(_widthValue);
        
        RefreshState();
        
        AddLog("SMASH", $"Dependency cascade fired → Geometry IsUnknown = {_geomIsUnknown}");
        
        InvokeAsync(StateHasChanged);
    }

    private void ApplyHeight()
    {
        if (_testComponent == null) return;

        var heightParam = _testComponent.FindParameter("Height");
        if (heightParam == null)
        {
            AddLog("ERROR", "Height parameter not found!");
            return;
        }

        var oldValue = _heightValue;
        _heightValue = _heightInput;
        
        AddLog("CHANGE", $"Setting Height: {oldValue} → {_heightValue}");
        
        heightParam.SetValue(_heightValue);
        
        RefreshState();
        
        AddLog("SMASH", $"Dependency cascade fired → Geometry IsUnknown = {_geomIsUnknown}");
        
        InvokeAsync(StateHasChanged);
    }

    private void ManualSmash()
    {
        if (_geomParam == null) return;

        AddLog("SMASH", "Manual Smash() called on geometry parameter");
        
        _geomParam.Smash();
        
        RefreshState();
        
        AddLog("SMASH", $"After Smash: IsUnknown = {_geomIsUnknown}, Cache empty = {_geomCacheEmpty}");
        
        InvokeAsync(StateHasChanged);
    }

    private void ClearLog()
    {
        _eventLog.Clear();
        InvokeAsync(StateHasChanged);
    }

    private void AddLog(string type, string message)
    {
        _eventLog.Add(new LogEntry(type, message, _currentTick));
        
        // Keep log from growing too large
        if (_eventLog.Count > 500)
            _eventLog.RemoveRange(0, 100);
    }

    private string GetLogClass(string type) => type switch
    {
        "EVAL" => "bg-success bg-opacity-25",
        "SMASH" => "bg-warning bg-opacity-25",
        "CHANGE" => "bg-info bg-opacity-25",
        "ERROR" => "bg-danger bg-opacity-25",
        _ => ""
    };

    private string GetLogBadge(string type) => type switch
    {
        "EVAL" => "bg-success",
        "SMASH" => "bg-warning text-dark",
        "CHANGE" => "bg-info",
        "ERROR" => "bg-danger",
        "CTRL" => "bg-primary",
        "INIT" => "bg-secondary",
        "TICK" => "bg-light text-dark",
        _ => "bg-dark"
    };

    public void Dispose()
    {
        _evaluationTimer?.Stop();
        _evaluationTimer?.Dispose();
        _evaluationTimer = null;
    }

    private record ShapeHistoryEntry(string GlyphId, string Dimensions, int Tick, bool IsNew);
    private record LogEntry(string Type, string Message, int Tick);
}

/// <summary>
/// Minimal test component for Phase 0 testing.
/// Creates a simple geometry that depends on Width and Height parameters.
/// NO rendering, NO animation callbacks - just parameter mechanics.
/// </summary>
public class TestHarnessComponent : KnComponent
{
    public TestHarnessComponent(string name, double width, double height) : base(name)
    {
        // Create simple parameters
        Calculations([
            $"Width: {width}",
            $"Height: {height}",
            "Depth: 5.0",
            "GeometryType: 'Box'"
        ]);
    }

    /// <summary>
    /// Establish geometry with a formula that creates a FoShape3D.
    /// Sets up dependencies so geometry depends on Width, Height, Depth.
    /// </summary>
    public override (KnGeometry, KnGeometryParameter) EstablishGeometry3D(string view, IArena? arena)
    {
        var result = Compute3DGeometry(view, geom =>
        {
            // Set up compute method with BeforeSmash cleanup
            geom.ApplyMethod("ComputeTestGeometry", ComputeTestShape3D, null, (param, opResult) =>
            {
                // BeforeSmash callback - cleanup old shape
                var oldShape = geom.GetCashe<FoShape3D>();
                if (oldShape != null)
                {
                    oldShape.SetShouldDelete();
                    oldShape.OnDelete?.Invoke(oldShape);
                }
                geom.GetParameter().SetCashe(null!);
                
                $"TestHarnessComponent: BeforeSmash - old shape cleaned up".WriteInfo();
            });

            // Set up dependencies: geometry depends on Width, Height, Depth
            SetupDependencies(geom.GetParameter());
        });

        return (result, result.GetParameter());
    }

    private void SetupDependencies(KnGeometryParameter geomParam)
    {
        var widthParam = FindParameter("Width");
        var heightParam = FindParameter("Height");
        var depthParam = FindParameter("Depth");

        // When Width/Height/Depth change, geometry should be smashed
        if (widthParam != null) geomParam.IDependOn(widthParam);
        if (heightParam != null) geomParam.IDependOn(heightParam);
        if (depthParam != null) geomParam.IDependOn(depthParam);
        
        $"TestHarnessComponent: Dependencies set up - DependsOn count = {geomParam.DependsOn?.Count ?? 0}".WriteInfo();
    }

    /// <summary>
    /// Creates a simple FoShape3D using the parameter values.
    /// This is the "formula" that gets evaluated when geometry is Unknown.
    /// </summary>
    private bool ComputeTestShape3D(KnInstance context, List<OPResult> args, OPResult result)
    {
        var width = FindNumberValue("Width", 1.0);
        var height = FindNumberValue("Height", 1.0);
        var depth = FindNumberValue("Depth", 1.0);
        var geomType = FindStringValue("GeometryType", "Box");

        $"TestHarnessComponent: Computing geometry - W={width}, H={height}, D={depth}".WriteSuccess();

        // Create the shape (without any rendering context)
        var shape = new FoShape3D($"TestShape_{Name}")
        {
            Width = width,
            Height = height,
            Depth = depth
        };

        // Create the geometry based on type
        shape = geomType switch
        {
            "Box" => shape.CreateBox(shape.Name!, width, height, depth),
            "Sphere" => shape.CreateSphere(shape.Name!, width, height, depth),
            "Cylinder" => shape.CreateCylinder(shape.Name!, width, height, depth),
            _ => shape.CreateBox(shape.Name!, width, height, depth)
        };

        $"TestHarnessComponent: Created {geomType} with GlyphId = {shape.GlyphId}".WriteSuccess();

        result.SetValue(ResultStatus.Shape3D, shape);
        return true;
    }
}
