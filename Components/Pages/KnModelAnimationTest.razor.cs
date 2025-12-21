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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Subscribe to refresh messages from model parameter changes
        MentorServices?.PubSub?.SubscribeTo<RefreshRenderMessage>(OnRefreshRender);
        
        if (MentorServices != null)
        {
            _knModel = MentorServices.EstablishModel<AnimatedKnModel>("KnModelAnimationTestModel");
            
            // Ensure animation callback is set up (may not run if model already exists)
            _knModel.EnsureAnimationSetup();
            
            // Create initial child components using bulk add
            var initialComponents = CreateChildComponents(3);
            foreach (var component in initialComponents)
            {
                ModelEditor.AddChild(_knModel, component);
            }
        }
        
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

            _testPage = Canvas2DReference?.Page;
            _testStage = Canvas3DReference?.Stage;
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
        
        _testStage?.ClearAll();
        _eventLogs.Clear();
        
        AddLog("System", "Test reset");
        InvokeAsync(StateHasChanged);
    }

    private void OnRefreshRender(RefreshRenderMessage message)
    {
        // Handle refresh messages from model parameter changes
        InvokeAsync(StateHasChanged);
    }

    protected void RenderToCanvas()
    {
        if (Canvas3DReference != null)
        {
            Task.Run(async () =>
            {
                await _knModel.RenderArena3D("KnModelTest3D", false, () => AddLog("Model", "3D render complete"));
            });
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
        var amplitudes = new[] { 0.3, 0.5, 0.7, 0.4, 0.6, 0.8 };  // Different bounce heights
        var frequencies = new[] { 0.04, 0.05, 0.06, 0.07, 0.03, 0.08 };  // Different speeds
        var color = colors[(componentCount - 1) % colors.Length];
        var amplitude = amplitudes[(componentCount - 1) % amplitudes.Length];
        var frequency = frequencies[(componentCount - 1) % frequencies.Length];
        
        // Create component with position, color, amplitude, and frequency (constructor initializes KnParameters)
        var component = new AnimatedKnComponent(
            $"Component_{componentCount}", 
            color, 
            new Vector3(xPosition, 1.0, 0),
            amplitude,
            frequency
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

    /// <summary>
    /// Creates multiple child components in bulk for efficient initialization
    /// </summary>
    protected List<AnimatedKnComponent> CreateChildComponents(int count)
    {
        var colors = new[] { "Blue", "Green", "Red", "Purple", "Orange", "Cyan" };
        var amplitudes = new[] { 0.3, 0.5, 0.7, 0.4, 0.6, 0.8 };  // Different bounce heights
        var frequencies = new[] { 0.04, 0.05, 0.06, 0.07, 0.03, 0.08 };  // Different speeds
        var components = new List<AnimatedKnComponent>();
        var existingCount = _knModel.Members<KnComponent>().Count();
        
        for (int i = 0; i < count; i++)
        {
            var componentIndex = existingCount + i + 1;
            var xPosition = (componentIndex - 1) * 2.5 - 2.5;
            var color = colors[(componentIndex - 1) % colors.Length];
            var amplitude = amplitudes[(componentIndex - 1) % amplitudes.Length];
            var frequency = frequencies[(componentIndex - 1) % frequencies.Length];
            
            var component = new AnimatedKnComponent(
                $"Component_{componentIndex}", 
                color, 
                new Vector3(xPosition, 1.0, 0),
                amplitude,
                frequency
            );
            
            components.Add(component);
        }
        
        $"CreateChildComponents: Created {count} components".WriteInfo();
        return components;
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
        MentorServices?.PubSub?.UnSubscribeFrom<RefreshRenderMessage>(OnRefreshRender);
        _ = _testStage?.ClearAll();
        
        $"KnModelAnimationTest: Disposed".WriteInfo();
        
        GC.SuppressFinalize(this);
    }
    

}
