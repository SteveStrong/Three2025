using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryMentorModeler.Model;

#nullable enable

namespace Three2025.Components.Pages;

public record DebugLogEntry(string Type, string Message, DateTime Timestamp);

public partial class GeometryDebugTest : ComponentBase, IDisposable
{
    [Inject] public IWorkspace Workspace { get; init; } = null!;
    [Inject] public IFoundryService FoundryService { get; init; } = null!;
    [Inject] public IMentorServices MentorServices { get; init; } = null!;
    [Inject] public ComponentBus PubSub { get; init; } = null!;

    public Canvas3DComponent? Canvas3DReference = null;
    [Parameter] public int CanvasWidth { get; set; } = 500;
    [Parameter] public int CanvasHeight { get; set; } = 400;

    // Test component with geometry
    private DebugGeometryComponent? _testComponent;
    private FoStage3D? _testStage;
    
    // UI State
    private string _currentGeomType = "Box";
    private string _currentColor = "Blue";
    private string _animationState = "Unknown";
    private int _currentTick = 0;
    private double _currentFps = 0;
    private int _stageShapeCount = 0;
    private int _arenaStageCount = 0;
    private string _geometryCacheState = "Unknown";
    private string _dependencyState = "Unknown";
    
    // Event log
    private List<DebugLogEntry> _eventLogs = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Create a simple test component
        _testComponent = new DebugGeometryComponent("DebugShape", "Blue", new Vector3(0, 1, 0));
        _testComponent.SetLogCallback(AddLog);
        
        // Subscribe to animation events for UI updates
        AnimationFrameBus.SubscribeToPreAnimation(OnPreAnimation);
        AnimationFrameBus.SubscribeToAnimation(OnAnimation);
        
        AddLog("INIT", "GeometryDebugTest initialized");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Wait for canvas to fully initialize with retries
            for (int attempt = 0; attempt < 10; attempt++)
            {
                await Task.Delay(100);
                _testStage = Canvas3DReference?.Stage;
                
                if (_testStage != null && !string.IsNullOrEmpty(_testStage.GetName()))
                {
                    AddLog("INIT", $"Stage '{_testStage.GetName()}' acquired from canvas (attempt {attempt + 1})");
                    break;
                }
            }
            
            if (_testStage == null || string.IsNullOrEmpty(_testStage.GetName()))
            {
                AddLog("ERROR", "Stage is NULL or has empty name after 10 attempts!");
                
                // Try getting stage directly from arena as fallback
                var arena = Workspace?.GetArena();
                if (arena != null)
                {
                    _testStage = arena.GetAllStages().FirstOrDefault(s => s.GetName() == "GeomDebug3D");
                    if (_testStage != null)
                    {
                        AddLog("INIT", $"Stage '{_testStage.GetName()}' found via arena fallback");
                    }
                    else
                    {
                        AddLog("ERROR", $"Arena has {arena.GetAllStages().Count} stages, none named 'GeomDebug3D'");
                        foreach (var s in arena.GetAllStages().Take(5))
                        {
                            AddLog("DEBUG", $"  - Stage: '{s.GetName()}' (Key: '{s.Key}')");
                        }
                    }
                }
            }
            
            UpdateDiagnostics();
            await InvokeAsync(StateHasChanged);
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    // === Animation Control ===
    
    private void StartAnimation()
    {
        AnimationFrameBus.ResumeAllAnimations();
        AddLog("CTRL", "Animation started");
        UpdateDiagnostics();
    }

    private void PauseAnimation()
    {
        AnimationFrameBus.PauseAllAnimations();
        AddLog("CTRL", "Animation paused");
        UpdateDiagnostics();
    }

    private async Task StepOneFrame()
    {
        AddLog("CTRL", "Stepping single frame...");
        await AnimationFrameBus.TriggerSingleFrame();
        UpdateDiagnostics();
        await InvokeAsync(StateHasChanged);
    }

    // === Parameter Changes ===
    
    private void SetGeometryType(string geomType)
    {
        var oldType = _currentGeomType;
        _currentGeomType = geomType;
        
        AddLog("PARAM", $"GeometryType: {oldType} → {geomType}");
        
        var param = _testComponent?.FindParameter("GeometryType");
        if (param != null)
        {
            AddLog("PARAM", $"Found parameter, calling SetValue...");
            param.SetValue(geomType);
            AddLog("PARAM", $"SetValue complete - this should have triggered Smash cascade");
        }
        else
        {
            AddLog("ERROR", "GeometryType parameter not found!");
        }
        
        UpdateDiagnostics();
        InvokeAsync(StateHasChanged);
    }

    private void SetColor(string color)
    {
        var oldColor = _currentColor;
        _currentColor = color;
        
        AddLog("PARAM", $"Color: {oldColor} → {color}");
        
        var param = _testComponent?.FindParameter("Color");
        if (param != null)
        {
            param.SetValue(color);
            AddLog("PARAM", "Color SetValue complete");
        }
        else
        {
            AddLog("ERROR", "Color parameter not found!");
        }
        
        UpdateDiagnostics();
        InvokeAsync(StateHasChanged);
    }

    // === Manual Triggers ===
    
    private void ManualSmashGeometry()
    {
        AddLog("MANUAL", "=== SMASH GEOMETRY ===");
        
        var (geom, geomParam) = _testComponent?.GetGeometry3D("GeomDebug3D") ?? (null, null);
        
        if (geomParam != null)
        {
            AddLog("SMASH", $"Before smash: IsCasheEmpty={geom?.IsCasheEmpty()}");
            geomParam.Smash();
            AddLog("SMASH", $"After smash: IsCasheEmpty={geom?.IsCasheEmpty()}");
        }
        else
        {
            AddLog("ERROR", "No geometry parameter found to smash");
        }
        
        UpdateDiagnostics();
        InvokeAsync(StateHasChanged);
    }

    private void ManualEvaluateGeometry()
    {
        AddLog("MANUAL", "=== EVALUATE GEOMETRY ===");
        
        if (_testComponent == null)
        {
            AddLog("ERROR", "Test component is null");
            return;
        }

        // This should call ComputeShape3D if cache is empty
        var (geom, _) = _testComponent.EstablishGeometry3D("GeomDebug3D", Workspace?.GetArena());
        var result = geom.GetCurrentValue();
        
        AddLog("EVAL", $"Evaluation result: Status={result.GetStatus()}, HasShape={result.AsShape3D() != null}");
        AddLog("EVAL", $"After eval: IsCasheEmpty={geom.IsCasheEmpty()}");
        
        UpdateDiagnostics();
        InvokeAsync(StateHasChanged);
    }

    private void ManualRenderToStage()
    {
        AddLog("MANUAL", "=== RENDER TO STAGE ===");
        
        if (_testStage == null)
        {
            AddLog("ERROR", "Stage is null - cannot render");
            return;
        }
        
        if (_testComponent == null)
        {
            AddLog("ERROR", "Test component is null");
            return;
        }

        var arena = Workspace?.GetArena();
        if (arena == null)
        {
            AddLog("ERROR", "Arena is null");
            return;
        }

        // Create render context
        var ctx = RenderContext3D.Create(arena, "GeomDebug3D", deep: false);
        
        AddLog("RENDER", $"Created context for view '{ctx.ViewName}', stage={ctx.Stage?.Name}");
        
        // Render the component
        _testComponent.RenderGeometry3D(ctx);
        
        AddLog("RENDER", $"RenderGeometry3D complete, stage shape count={_testStage.Members<FoShape3D>().Count}");
        
        UpdateDiagnostics();
        InvokeAsync(StateHasChanged);
    }

    private void ManualFullCycle()
    {
        AddLog("MANUAL", "========== FULL CYCLE ==========");
        ManualSmashGeometry();
        ManualEvaluateGeometry();
        ManualRenderToStage();
        AddLog("MANUAL", "========== CYCLE COMPLETE ==========");
    }

    private async Task ClearStage()
    {
        AddLog("MANUAL", "=== CLEAR STAGE ===");
        
        if (_testStage != null)
        {
            await _testStage.ClearAll();
            AddLog("CLEAR", "Stage cleared");
        }
        
        UpdateDiagnostics();
        await InvokeAsync(StateHasChanged);
    }

    private void ClearLog()
    {
        _eventLogs.Clear();
        InvokeAsync(StateHasChanged);
    }

    // === Event Handlers ===
    
    private void OnPreAnimation(PreAnimationEvent evt)
    {
        _currentTick = evt.tick;
        _currentFps = evt.fps;
        
        // Call component's PreAnimation if it has one
        _testComponent?.OnPreAnimationEvent(evt);
        
        if (evt.tick % 60 == 0)
        {
            UpdateDiagnostics();
            InvokeAsync(StateHasChanged);
        }
    }

    private void OnAnimation(AnimationEvent evt)
    {
        // Update state display
        _animationState = AnimationFrameBus.GetAnimationState();
    }

    // === Diagnostics ===
    
    private void UpdateDiagnostics()
    {
        _animationState = AnimationFrameBus.GetAnimationState();
        _currentTick = AnimationFrameBus.GetCurrentTick();
        _currentFps = AnimationFrameBus.GetCurrentFps();
        
        // Stage info
        _stageShapeCount = _testStage?.Members<FoShape3D>().Count ?? -1;
        
        // Arena info
        var arena = Workspace?.GetArena();
        _arenaStageCount = arena?.GetAllStages().Count ?? -1;
        
        // Geometry cache state
        var (geom, geomParam) = _testComponent?.GetGeometry3D("GeomDebug3D") ?? (null, null);
        if (geom != null)
        {
            var cache = geom.GetCashe<FoShape3D>();
            _geometryCacheState = cache != null ? $"Has shape: {cache.Name}" : "EMPTY";
        }
        else
        {
            _geometryCacheState = "No geometry established";
        }
        
        // Dependency state
        if (geomParam != null)
        {
            var depCount = geomParam.DependsOn.Count;
            var contribCount = 0;
            
            var colorParam = _testComponent?.FindParameter("Color");
            var geomTypeParam = _testComponent?.FindParameter("GeometryType");
            
            if (colorParam != null) contribCount += colorParam.ContributesTo.Count;
            if (geomTypeParam != null) contribCount += geomTypeParam.ContributesTo.Count;
            
            _dependencyState = $"DependsOn={depCount}, ContributesTo={contribCount}";
        }
        else
        {
            _dependencyState = "No param";
        }
    }

    private void AddLog(string type, string message)
    {
        _eventLogs.Add(new DebugLogEntry(type, message, DateTime.Now));
        $"[GeomDebug] [{type}] {message}".WriteInfo();
    }

    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromPreAnimation(OnPreAnimation);
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimation);
    }
}
