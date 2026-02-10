using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Blazor.Models;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.PubSub;
using BlazorComponentBus;
using FoundryWorldsAndDrawings.Shared;

namespace FoundryWorldsAndDrawings.Blazor.Components.Pages;

public partial class ClockDemo : ComponentBase, IDisposable
{
    [Inject] private IWorkspace Workspace { get; set; } = null!;
    [Inject] private ISelectionService SelectionService { get; set; } = null!;
    [Inject] private ComponentBus PubSub { get; set; } = null!;
    
    private Canvas3DComponent? _canvasComponent;
    private ClockDemoModel? _model;
    private FoStage3D? _stage;
    private readonly string SceneName = "ClockDemo";

    protected override void OnInitialized()
    {
        _model = new ClockDemoModel(Workspace, SelectionService, SceneName);
        
        // Get reference to the stage for the Shapes tab
        var arena = Workspace.GetArena();
        _stage = arena.EstablishStage<FoStage3D>(SceneName);
        
        // Subscribe to UI refresh events
        PubSub?.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);
        
        "🕐 ClockDemo page initialized".WriteSuccess();
    }
    
    private void HandleCommandResult(MxActionResult result)
    {
        $"ClockDemo: Command executed - {result.Message}".WriteInfo();
        
        // Refresh stage reference and force UI refresh after command
        var arena = Workspace.GetArena();
        _stage = arena.EstablishStage<FoStage3D>(SceneName);
        StateHasChanged();
    }
    
    private void OnRefreshUIEvent(RefreshUIEvent evt)
    {
        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        // Cleanup model (unsubscribe from animation events)
        _model?.Cleanup();
        
        // PubSub cleanup happens automatically
        "🕐 ClockDemo page disposed".WriteInfo();
    }
}
