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
    [Inject] public IMentorServices MentorServices { get; init; } = null!;
    [Inject] public IModelEditor ModelEditor { get; init; } = null!;

    public Canvas3DComponent? Canvas3DReference = null;
    [Parameter] public int CanvasWidth { get; set; } = 500;
    [Parameter] public int CanvasHeight { get; set; } = 400;

    // Test model and component
    private AnimatedKnModel? _testModel;
    private DebugGeometryComponent? _testComponent;
    private FoStage3D? _testStage;
    
    // UI State
    private string _currentGeomType = "Box";
    private string _currentColor = "Blue";
    private string _activeTab = "model";
    private bool _componentAnimationEnabled = false;
    
    // Dimension state (matches component defaults)
    private double _width = 1.5;
    private double _height = 1.5;
    private double _depth = 1.5;
    
    // Position state (matches component defaults)
    private double _positionX = 0.0;
    private double _positionY = 1.0;
    private double _positionZ = 0.0;
    
    // Event log
    private List<DebugLogEntry> _eventLogs = new();

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Subscribe to model edit changes to refresh tree
        MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(OnModelEditChanged);
        
        // Subscribe to animation events to render geometry each frame
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        
        // Create model that handles animation lifecycle automatically
        _testModel = MentorServices!.EstablishModel<AnimatedKnModel>("GeomDebugModel");
        _testModel.SetExpanded(true);
        
        // Create test component
        _testComponent = new DebugGeometryComponent("DebugShape", "Blue", new Vector3(0, 1, 0));
        _testComponent.SetLogCallback(AddLog);
        
        if (ModelEditor != null)
        {
            _testComponent.SetModelEditor(ModelEditor); // Provide ModelEditor for animation
            
            // Add component to model
            ModelEditor.AddChild(_testModel, _testComponent);
        }
        
        AddLog("INIT", "GeometryDebugTest initialized - AnimatedKnModel will auto-render on changes");
    }

    private void OnModelEditChanged(ModelEditChanged message)
    {
        InvokeAsync(StateHasChanged);
    }

    private void OnAnimationEvent(AnimationEvent evt)
    {
        // Render geometry to stage on each animation frame
        if (_testStage != null && _testModel != null && evt.IsWorld3D())
        {
            var ctx = RenderContext3D.CreateFromStage(_testStage, deep: true);
            _testModel.RenderGeometry3D(ctx);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            AddLog("INIT", "Setting up canvas and stage");
            
            // Wait for canvas initialization
            await Task.Delay(200);
            
            _testStage = Canvas3DReference?.Stage;
            if (_testStage != null && MentorServices != null)
            {
                AddLog("INIT", $"Stage acquired: {_testStage.Key}");
                
                // Establish initial geometry so we have something to render
                if (_testComponent != null)
                {
                    var view = _testStage.GetName();
                    var (geometry, parameter) = ModelEditor.EstablishGeometry3D(_testComponent, view);
                    AddLog("INIT", $"Initial geometry parameter established: {parameter?.Name}");
                }
                
                // Start animation - PreAnimationRefresh will automatically render each frame
                AnimationFrameBus.ResumeAllAnimations();
                AddLog("INIT", "Animation started - parameter changes will auto-render");
                
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                AddLog("ERROR", "Stage or MentorServices is null after delay");
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    private void SetGeometryType(string geomType)
    {
        if (_testComponent == null) return;
        
        _currentGeomType = geomType;
        AddLog("PARAM", $"Setting GeometryType to '{geomType}' - will trigger auto-render on next frame");
        
        // ModelEditor.SetParameter triggers Smash cascade
        // AnimatedKnModel.PreAnimationRefresh will automatically render on next animation frame
        ModelEditor.SetParameter(_testComponent, "GeometryType", $"'{geomType}'");
    }

    private void SetColor(string color)
    {
        if (_testComponent == null) return;
        
        _currentColor = color;
        AddLog("PARAM", $"Setting Color to '{color}' - will trigger auto-render on next frame");
        
        // ModelEditor.SetParameter triggers Smash cascade
        // AnimatedKnModel.PreAnimationRefresh will automatically render on next animation frame
        ModelEditor.SetParameter(_testComponent, "Color", $"'{color}'");
    }
    
    private void OnWidthChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        if (double.TryParse(e.Value.ToString(), out var width))
        {
            _width = width;
            AddLog("PARAM", $"Setting Width to {width}m");
            ModelEditor.SetParameter(_testComponent, "Width", $"units({width}, 'm')");
        }
    }
    
    private void OnHeightChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        if (double.TryParse(e.Value.ToString(), out var height))
        {
            _height = height;
            AddLog("PARAM", $"Setting Height to {height}m");
            ModelEditor.SetParameter(_testComponent, "Height", $"units({height}, 'm')");
        }
    }
    
    private void OnDepthChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        if (double.TryParse(e.Value.ToString(), out var depth))
        {
            _depth = depth;
            AddLog("PARAM", $"Setting Depth to {depth}m");
            ModelEditor.SetParameter(_testComponent, "Depth", $"units({depth}, 'm')");
        }
    }
    
    private void OnXPositionChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        if (double.TryParse(e.Value.ToString(), out var posX))
        {
            _positionX = posX;
            AddLog("PARAM", $"Setting PositionX to {posX}m");
            ModelEditor.SetParameter(_testComponent, "PositionX", $"units({posX}, 'm')");
        }
    }
    
    private void OnYPositionChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        if (double.TryParse(e.Value.ToString(), out var posY))
        {
            _positionY = posY;
            AddLog("PARAM", $"Setting PositionY to {posY}m");
            ModelEditor.SetParameter(_testComponent, "PositionY", $"units({posY}, 'm')");
        }
    }
    
    private void OnZPositionChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        if (double.TryParse(e.Value.ToString(), out var posZ))
        {
            _positionZ = posZ;
            AddLog("PARAM", $"Setting PositionZ to {posZ}m");
            ModelEditor.SetParameter(_testComponent, "PositionZ", $"units({posZ}, 'm')");
        }
    }

    private async Task ClearStage()
    {
        AddLog("CLEAR", "Clearing stage");
        
        if (_testStage != null)
        {
            await _testStage.ClearAll();
            AddLog("CLEAR", $"Stage cleared - now has {_testStage.Members<FoShape3D>().Count} shapes");
        }
        
        await InvokeAsync(StateHasChanged);
    }

    private void ClearLog()
    {
        _eventLogs.Clear();
        StateHasChanged();
    }
    
    private void ToggleComponentAnimation()
    {
        if (_testComponent == null) return;
        
        _componentAnimationEnabled = !_componentAnimationEnabled;
        _testComponent.SetAnimationEnabled(_componentAnimationEnabled);
        
        var status = _componentAnimationEnabled ? "ENABLED" : "DISABLED";
        AddLog("ANIM", $"Component animation {status}");
        
        StateHasChanged();
    }

    private void AddLog(string type, string message)
    {
        _eventLogs.Add(new DebugLogEntry(type, message, DateTime.Now));
        $"[GeomDebug] [{type}] {message}".WriteInfo();
    }

    private string GetLogClass(string type) => type switch
    {
        "ERROR" => "table-danger",
        "SMASH" => "table-warning",
        "CREATE" => "table-success",
        "COMPUTE" => "table-success",
        "RENDER" => "table-info",
        "PARAM" => "table-primary",
        "FRAME" => "table-warning",
        "ANIM" => "table-success",
        _ => ""
    };

    public void Dispose()
    {
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationEvent);
        MentorServices?.PubSub?.UnSubscribeFrom<ModelEditChanged>(OnModelEditChanged);
    }
}