using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core.Extensions;
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
    
    // Clock animation control
    protected bool _clockAnimationEnabled = false;
    
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
    
    protected void ToggleClockAnimation()
    {
        _clockAnimationEnabled = !_clockAnimationEnabled;
        
        // TODO: Replace with new collection API
        // var firstComponent = _knModel?.Members<KnComponent>()?.FirstOrDefault();
        var firstComponent = null as KnComponent; // Temporary placeholder
        
        // Get first component to control
        if (firstComponent != null)
        {
            if (_clockAnimationEnabled)
            {
                // TODO: EnableClockAnimation() no longer exists
                // firstComponent.EnableClockAnimation();
                AddLog("Control", $"Clock animation ENABLED on {firstComponent.Name}");
            }
            else
            {
                // TODO: DisableClockAnimation() no longer exists
                // firstComponent.DisableClockAnimation();
                AddLog("Control", $"Clock animation DISABLED on {firstComponent.Name}");
            }
        }
        else
        {
            AddLog("Control", "No components available for clock animation");
        }
        
        InvokeAsync(StateHasChanged);
    }

    protected void ResetTest()
    {
        // Clear components from model but keep the model
        // TODO: GetSlot<AnimatedKnComponent>() no longer exists
        // _knModel.GetSlot<AnimatedKnComponent>()?.Clear();
        
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
        // TODO: Replace with new collection API
        // var componentCount = _knModel?.Members<KnComponent>()?.Count() ?? 1;
        var componentCount = 1; // Temporary placeholder
        
        // Position components in a row
        var xPosition = (componentCount - 1) * 3.0 - 6.0;
        var colors = new[] { "Blue", "Green", "Red", "Purple", "Orange", "Cyan" };
        var color = colors[(componentCount - 1) % colors.Length];
        
        // Use DebugGeometryComponent instead - it's simpler and has working animation
        var component = new DebugGeometryComponent(
            $"Shape_{componentCount}", 
            color, 
            new Vector3(xPosition, 1.0, 0)
        );
        
        // Set up animation
        component.SetModelEditor(ModelEditor!);
        component.SetAnimationEnabled(true);
        
        // Add to model via ModelEditor (fires ModelEditChanged event, triggers tree refresh)
        ModelEditor!.AddChild(_knModel, component);
        $"AddChildComponent: Added DebugGeometryComponent with animation enabled".WriteSuccess();
        AddLog("Model", $"Added {component.Name} with animation enabled");
        
        // Ensure model is expanded so tree shows children
        _knModel.SetExpanded(true);
        
        AddLog("Component", $"Added animated component '{component.Name}' with {color} color");
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Creates multiple child components in bulk for efficient initialization
    /// </summary>
    protected List<DebugGeometryComponent> CreateChildComponents(int count)
    {
        var colors = new[] { "Blue", "Green", "Red", "Purple", "Orange", "Cyan" };
        var components = new List<DebugGeometryComponent>();
        var existingCount = _knModel?.GetCollection<KnComponent>().Count() ?? 0;
        
        for (int i = 0; i < count; i++)
        {
            var componentIndex = existingCount + i + 1;
            var xPosition = (componentIndex - 1) * 3.0 - 3.0; // Spread out more
            var color = colors[(componentIndex - 1) % colors.Length];
            
            var component = new DebugGeometryComponent(
                $"Shape_{componentIndex}", 
                color, 
                new Vector3(xPosition, 1.0, 0)
            );
            
            // Set up animation
            component.SetModelEditor(ModelEditor!);
            component.SetAnimationEnabled(true);
            
            components.Add(component);
        }
        
        $"CreateChildComponents: Created {count} animated components".WriteInfo();
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
