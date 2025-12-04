using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryWorldsAndDrawings;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryMentorModeler.Model;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// Event log entry for tracking animation events
/// </summary>
public record EventLogEntry(string Type, string Message, DateTime Timestamp);

public partial class KnModelAnimationTest : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; } = null!;
    [Inject] public IWorkspace Workspace { get; init; } = null!;
    [Inject] public IFoundryService FoundryService { get; init; } = null!;
    [Inject] public IMentorServices MentorServices { get; init; } = null!;
    [Inject] public ComponentBus PubSub { get; init; } = null!;
    [Inject] public IModelEditor ModelEditor { get; init; } = null!;

    public Canvas3DComponent? Canvas3DReference = null;
    public Canvas2DComponent? Canvas2DReference = null;
    [Parameter] public int CanvasWidth { get; set; } = 800;
    [Parameter] public int CanvasHeight { get; set; } = 600;
    
    // KnModel instance - created on load, handles its own animation events
    protected AnimatedKnModel _knModel { get; set; } = null!;
    
    // Event logging
    protected List<EventLogEntry> _eventLogs = new();
    protected bool _logAllEvents = false;
    
    // Tree tab selection
    protected string _activeTreeTab = "model";
    
    // Canvas tab selection
    protected string _activeCanvasTab = "3d";

    // Stage for 3D objects
    private FoStage3D? _testStage;
    private FoPage2D? _testPage;
    private int _shapeCount = 0;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        _knModel = MentorServices.EstablishModel<AnimatedKnModel>("KnModelAnimationTestModel");
        // Set up refresh callback so model triggers UI update when parameters change
        _knModel.SetRefreshAction(() => InvokeAsync(StateHasChanged));
        // Start expanded so tree children are visible

        AddChildComponent();
        AddChildComponent();
        AddChildComponent();
        _knModel.SetExpanded(true);
        var list = _knModel.Members<KnComponent>().ToList();
        var xxx = _knModel.GetTreeChildren();

        $"KnModelAnimationTest: KnModel '{_knModel.Name}' ready".WriteSuccess();
        AddLog("System", $"KnModel '{_knModel.Name}' ready - events flow through MentorServices");
        
        // Signal MentorTreeView to refresh after model is created
        PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            $"KnModelAnimationTest OnAfterRenderAsync: Setting up".WriteInfo();
            
            await Task.Delay(200); // Wait for canvas initialization

            var drawing = Workspace.GetDrawing();
            _testPage = drawing.EstablishPage<FoPage2D>("KnModelTest2D");
            
            var arena = Workspace.GetArena();
            _testStage = arena.EstablishStage<FoStage3D>("KnModelTest3D");


        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    // === Button handlers ===
    
    protected void StartAnimation()
    {
        AnimationFrameBus.ResumeAllAnimations();
        AddLog("Control", "Animation started");
        InvokeAsync(StateHasChanged);
    }

    protected void PauseAnimation()
    {
        AnimationFrameBus.PauseAllAnimations();
        AddLog("Control", "Animation paused");
        InvokeAsync(StateHasChanged);
    }

    protected void ResumeAnimation()
    {
        AnimationFrameBus.ResumeAllAnimations();
        AddLog("Control", "Animation resumed");
        InvokeAsync(StateHasChanged);
    }

    protected void ResetTest()
    {
        // Clear components from model but keep the model
        _knModel.GetSlot<AnimatedKnComponent>()?.Clear();
        
        _testStage?.ClearStage();
        _shapeCount = 0;
        _eventLogs.Clear();
        
        AddLog("System", "Test reset");
        InvokeAsync(StateHasChanged);
    }

    protected void AddAnimatedBoxOBSOLITE()
    {
        if (_testStage == null)
        {
            AddLog("Error", "Stage not ready - cannot add box");
            return;
        }

        _shapeCount++;
        var boxIndex = _shapeCount;
        var x = (boxIndex - 1) * 3.0 - 3.0; // Spread boxes horizontally
        
        var box = new FoShape3D().CreateBox($"AnimatedBox_{boxIndex}", 1.0, 1.0, 1.0);
        box.Color = GetColorForIndex(boxIndex);
        box.Transform = new Transform3($"BoxTransform_{boxIndex}")
        {
            Position = new Vector3(x, 0.5, 0),
            Rotation = Euler.FromDegrees(0, 0, 0),
        };

        _testStage.AddShape(box);
        
        AddLog("Shape", $"Added box '{box.Key}' at position ({x:F1}, 0.5, 0)");
        $"KnModelAnimationTest: Added box '{box.Key}'".WriteSuccess();
    }

    protected void AddRotatingGroupOBSOLITE()
    {
        if (_testStage == null)
        {
            AddLog("Error", "Stage not ready - cannot add group");
            return;
        }

        _shapeCount++;
        var groupIndex = _shapeCount;
        
        var group = new FoGroup3D($"RotatingGroup_{groupIndex}")
        {
            Transform = new Transform3($"GroupTransform_{groupIndex}")
            {
                Position = new Vector3(0, 2, 0),
                Rotation = Euler.FromDegrees(0, 0, 0),
            }
        };

        // Add some child boxes to the group
        for (int i = 0; i < 3; i++)
        {
            var angle = i * (2 * Math.PI / 3);
            var childBox = new FoShape3D().CreateBox($"GroupChild_{groupIndex}_{i}", 0.5, 0.5, 0.5);
            childBox.Color = GetColorForIndex(i + 1);
            childBox.Transform = new Transform3($"ChildTransform_{groupIndex}_{i}")
            {
                Position = new Vector3(Math.Cos(angle) * 1.5, 0, Math.Sin(angle) * 1.5),
            };
            group.AddShape(childBox);
        }

        _testStage.AddShape(group);
        AddLog("Shape", $"Added rotating group '{group.Key}' with 3 children");
    }



    protected void RenderToCanvas()
    {
        if (Canvas3DReference != null)
        {
            _knModel.RenderArena3D("KnModelTest3D", false, () => AddLog("Model", "3D render complete"));
        }
        if (Canvas2DReference != null)
        {
            _knModel.RenderDrawing2D("KnModelTest2D", false, () => AddLog("Model", "2D render complete"));
        }
    }

    protected void RefreshTree()
    {
        AddLog("Tree", "Tree refresh requested");
        InvokeAsync(StateHasChanged);
    }

    protected void AddChildComponent()
    {        
        var componentCount = _knModel.Members<KnComponent>().Count() + 1;
        
        // Position components in a row
        var xPosition = (componentCount - 1) * 2.5 - 2.5;
        var colors = new[] { "Blue", "Green", "Red", "Purple", "Orange", "Cyan" };
        var color = colors[(componentCount - 1) % colors.Length];
        
        // Create component with position and color (constructor initializes KnParameters)
        var component = new AnimatedKnComponent(
            $"Component_{componentCount}", 
            color, 
            new Vector3(xPosition, 1.0, 0)
        );
        
        // Set geometry configuration via parameters (replaces object initializer)
        // The constructor already sets these parameters, but we can override:
        // - GeometryType: "Box" (default)
        // - Width: 1.0m (default) 
        // - Height: 1.0m (default)
        // - Depth: 1.0m (default)
        // Custom dimensions can be set via: component.FindParameter("Width")?.ApplyFormula("units(1.5, 'm')", KnBase.UnitService);
        
        // Add to model via ModelEditor (fires ModelEditChanged event, triggers tree refresh)
        ModelEditor.AddChild(_knModel, component);
        $"AddChildComponent: After ModelEditor.AddChild, AnimatedKnComponent count = {_knModel.Members<AnimatedKnComponent>().Count()}".WriteSuccess();
        AddLog("Model", $"Added {component.Name} via ModelEditor - tree should auto-refresh");
        
        // Create and add geometry to stage
        // var shape = component.CreateGeometry();
        // _testStage.AddShape(shape);
        
        // Ensure model is expanded so tree shows children
        _knModel.SetExpanded(true);
        
        // Get geometry type from parameter for logging
        var geomType = component.FindParameter("GeometryType")?.GetValue().Value()?.ToString() ?? "Box";
        AddLog("Component", $"Added KnComponent '{component.Name}' with {color} {geomType} geometry");
        InvokeAsync(StateHasChanged);
    }



    protected void ClearEventLog()
    {
        _eventLogs.Clear();
        AddLog("System", "Event log cleared");
    }

    private void AddLog(string type, string message)
    {
        _eventLogs.Add(new EventLogEntry(type, message, DateTime.Now));
        
        // Keep log size manageable
        if (_eventLogs.Count > 200)
        {
            _eventLogs.RemoveRange(0, 50);
        }
        
        // Trigger UI update - needed when called from animation callbacks
        InvokeAsync(StateHasChanged);
    }

    protected string GetLogColor(string type)
    {
        return type switch
        {
            "PreAnim" => "#4fc3f7",   // Light blue
            "Anim" => "#81c784",       // Light green
            "Model" => "#ffb74d",      // Orange
            "Shape" => "#ba68c8",      // Purple
            "Control" => "#fff176",    // Yellow
            "System" => "#90a4ae",     // Grey
            "Error" => "#ef5350",      // Red
            "Warning" => "#ffa726",    // Orange
            "Tree" => "#4db6ac",       // Teal
            "Component" => "#7986cb", // Indigo
            "Scene" => "#f06292",      // Pink
            _ => "#ffffff"
        };
    }

    private string GetColorForIndex(int index)
    {
        var colors = new[] { "red", "green", "blue", "yellow", "purple", "orange", "cyan", "magenta" };
        return colors[(index - 1) % colors.Length];
    }

    public void Dispose()
    {
        _testStage?.ClearStage();
        
        $"KnModelAnimationTest: Disposed".WriteInfo();
        
        GC.SuppressFinalize(this);
    }
}
