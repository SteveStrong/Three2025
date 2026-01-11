#nullable enable

using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryMentorModeler.Model;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryAppStore.Extensions;
using Plugin_710.Model;

namespace Three2025.Components.Pages;

public class RackViewerBase : ComponentBase, IDisposable
{
    [Inject] protected IWorkspace Workspace { get; set; } = null!;
    [Inject] protected IMentorServices MentorServices { get; set; } = null!;
    [Inject] protected IModelEditor ModelEditor { get; set; } = null!;
    [Inject] public ComponentBus PubSub { get; set; } = null!;

    protected Canvas3DComponent? Canvas3DRef;
    protected int CanvasWidth { get; set; } = 800;
    protected int CanvasHeight { get; set; } = 600;

    protected int ComponentCount { get; set; } = 0;
    protected string _activeTreeTab = "model";
    
    protected Model_710? _model = null;
    protected Rack_710? _mainRack = null;
    protected ITreeNode? _selectedItem = null;
    protected bool _isLoading = false;
    protected string? _statusMessage = null;
    protected bool _isError = false;

    private FoStage3D? _stage;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Subscribe to refresh messages from model parameter changes
        MentorServices?.PubSub?.SubscribeTo<RefreshRenderMessage>(OnRefreshRender);
        
        // Don't create model here - wait for user to click "Create Rack Model" button
    }

    private void OnRefreshRender(RefreshRenderMessage message)
    {
        // Handle refresh messages from model parameter changes
        InvokeAsync(StateHasChanged);
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            "RackViewer: First render - waiting for canvas".WriteInfo();

            // Wait for canvas to fully initialize
            await Task.Delay(100);

            // Get stage from Canvas (stage-centric pattern)
            _stage = Canvas3DRef?.Stage;

            if (_stage != null)
            {
                $"RackViewer: Retrieved stage '{_stage.Name}' from Canvas".WriteSuccess();
            }
            else
            {
                "RackViewer: Stage not available yet".WriteWarning();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected async Task CreateRackModel()
    {
        if (_stage == null)
        {
            _statusMessage = "⚠️ Stage not ready";
            _isError = true;
            StateHasChanged();
            return;
        }

        _isLoading = true;
        _statusMessage = null;
        StateHasChanged();

        try
        {
            await Task.Delay(50);

            // Create Model_710
            _model = MentorServices.EstablishModel<Model_710>("RackViewerModel");
            _model.SetExpanded(true);
            "RackViewer: Model_710 created".WriteSuccess();

            // Get solution and build Rack_710
            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            var rackComp = Common_710.New_DT_Component("MainRack");
            rackComp.MarkAsRack();

            _mainRack = solution.Build<Rack_710>(rackComp, 1, lookup);
            _mainRack.SetExpanded(true);
            "RackViewer: Rack_710 created".WriteSuccess();

            // Set rack parameters
            _mainRack.Calculations([
                "X|m: 0",
                "Y|m: 0",
                "Z|m: 0",
                "Width|m: 0.8",
                "Height|m: 2.0",
                "Depth|m: 0.6",
                "PivotY|m: -1.0"
            ]);

            // Add initial equipment
            AddInitialEquipment();

            // Render to 3D
            RenderToStage();

            ComponentCount = 1 + (_mainRack.ModelComponents<Equipment_710>()?.Count() ?? 0);
            _statusMessage = $"✅ Created rack model with {ComponentCount} components";
            _isError = false;

            // Signal tree to refresh
            await Task.Run(() => PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null)));
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error creating model: {ex.Message}";
            _isError = true;
            $"RackViewer ERROR: {ex}".WriteError();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void AddInitialEquipment()
    {
        if (_mainRack == null || _model == null) return;

        var solution = _model.EstablishSolution();
        var lookup = solution.GetLookup();

        // Add Server
        var equipComp1 = Common_710.New_DT_Component("Server_01");
        equipComp1.MarkAsEquipment();
        var equipment1 = solution.Build<Equipment_710>(equipComp1, 1, lookup);

        equipment1.Calculations([
            "X|m: 0",
            "Y|m: 0.5",
            "Z|m: 0",
            "Width|m: 0.7",
            "Height|m: 0.4",
            "Depth|m: 0.5",
            "PivotY|m: -0.2"
        ]);

        _mainRack.Add(equipment1);
        "RackViewer: Added Server_01".WriteSuccess();
    }

    private void RenderToStage()
    {
        if (_stage == null || _mainRack == null)
        {
            "RackViewer: Cannot render - stage or rack is null".WriteWarning();
            return;
        }

        try
        {
            "RackViewer: Creating RenderContext3D from stage".WriteInfo();
            
            // Create render context from our stage
            var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
            
            "RackViewer: Calling RenderGeometry3D on rack".WriteInfo();
            
            // Render rack (will render all child equipment too)
            _mainRack.RenderGeometry3D(ctx);
            
            "RackViewer: Render complete".WriteSuccess();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            $"RackViewer: Render failed - {ex.Message}".WriteError();
        }
    }

    protected void HandleItemSelected(ITreeNode item)
    {
        _selectedItem = item;
        $"RackViewer: Selected - {item.GetTreeNodeTitle()}".WriteInfo();
        StateHasChanged();
    }

    protected void AddEquipment()
    {
        if (_mainRack == null || _model == null)
        {
            _statusMessage = "⚠️ Create rack model first";
            _isError = true;
            StateHasChanged();
            return;
        }

        try
        {
            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            var equipmentName = $"Equipment_{DateTime.Now.Ticks % 10000}";
            var equipComp = Common_710.New_DT_Component(equipmentName);
            equipComp.MarkAsEquipment();

            var equipment = solution.Build<Equipment_710>(equipComp, 1, lookup);

            var random = new Random();
            var yPos = -0.5 + (random.NextDouble() * 1.0);

            equipment.Calculations([
                "X|m: 0",
                $"Y|m: {yPos:F2}",
                "Z|m: 0",
                "Width|m: 0.7",
                "Height|m: 0.3",
                "Depth|m: 0.5",
                "PivotY|m: -0.15"
            ]);

            _mainRack.Add(equipment);

            // Re-render
            RenderToStage();

            ComponentCount = 1 + (_mainRack.ModelComponents<Equipment_710>()?.Count() ?? 0);
            _statusMessage = $"✅ Added {equipmentName}";
            _isError = false;

            PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null));
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error adding equipment: {ex.Message}";
            _isError = true;
        }
    }

    protected void ClearModel()
    {
        _model = null;
        _mainRack = null;
        _selectedItem = null;
        ComponentCount = 0;
        _statusMessage = "🗑️ Model cleared";
        _isError = false;

        if (_stage != null)
        {
            var shapes = _stage.ClearAll();
        }

        StateHasChanged();
    }

    public void Dispose()
    {
        "RackViewer: Disposing".WriteInfo();
        _stage = null;
        _mainRack = null;
        _model = null;
    }
}
