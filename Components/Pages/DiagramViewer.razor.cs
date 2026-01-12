#nullable enable

using Microsoft.AspNetCore.Components;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Diagram;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryAppStore.Extensions;
using Plugin_710.Model;
using Blazor.Diagrams.Core.Geometry;
using BlazorComponentBus;

namespace Three2025.Components.Pages;

public class DiagramViewerBase : ComponentBase, IDisposable
{
    [Inject] protected IMentorServices MentorServices { get; set; } = null!;
    [Inject] protected ComponentBus PubSub { get; set; } = null!;

    protected Model_710? _model;
    protected MentorDiagram? _diagram;
    protected ITreeNode? _selectedItem;
    
    protected string? _statusMessage;
    protected bool _isError;
    protected bool _isLoading;
    
    protected int _nodeCount;
    protected int _linkCount;

    protected override async Task OnInitializedAsync()
    {
        "DiagramViewer: Initializing".WriteInfo();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Create a system-level diagram with SystemBlock nodes and SystemLink connections.
    /// </summary>
    protected async Task CreateSystemDiagram()
    {
        _isLoading = true;
        _statusMessage = "📐 Creating system diagram...";
        StateHasChanged();

        try
        {
            await Task.Delay(50);

            // Create model
            _model = MentorServices.EstablishModel<Model_710>("DiagramViewerModel");
            _model.SetExpanded(true);
            "DiagramViewer: Model_710 created".WriteSuccess();

            // Create solution
            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            // Create diagram via Model_710's RenderDiagram
            _diagram = _model.RenderDiagram("SystemView", clear: true, () => { });
            "DiagramViewer: Diagram created".WriteSuccess();

            // Create some system blocks
            var blockNames = new[] { "PowerSystem", "NetworkSystem", "StorageSystem" };
            var xPos = 100.0;
            var yPos = 100.0;
            
            foreach (var name in blockNames)
            {
                var blockComp = Common_710.New_DT_Component(name);
                blockComp.MarkAsBlock("");
                var block = solution.Build<SystemBlock_710>(blockComp, 1, lookup);
                
                block.Calculations([
                    $"sPinX: {xPos}",
                    $"sPinY: {yPos}",
                    "sWidth: 200",
                    "sHeight: 100"
                ]);
                
                _model.AddChildComponent<KnComponent>(block);
                
                xPos += 250;
                if (xPos > 700)
                {
                    xPos = 100;
                    yPos += 150;
                }
            }

            // Render blocks to diagram
            _diagram = _model.RenderDiagram("SystemView", clear: false, () => { });

            UpdateCounts();
            
            _statusMessage = $"✅ System diagram created with {_nodeCount} blocks";
            _isError = false;

            await Task.Run(() => PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null)));
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error creating diagram: {ex.Message}";
            _isError = true;
            $"CreateSystemDiagram ERROR: {ex}".WriteError();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Create a circuit-level diagram with Equipment, Racks, and Cables.
    /// </summary>
    protected async Task CreateCircuitDiagram()
    {
        _isLoading = true;
        _statusMessage = "⚡ Creating circuit diagram...";
        StateHasChanged();

        try
        {
            await Task.Delay(50);

            // Create model
            _model = MentorServices.EstablishModel<Model_710>("DiagramViewerModel");
            _model.SetExpanded(true);

            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            // Create diagram
            _diagram = _model.RenderDiagram("CircuitView", clear: true, () => { });

            // Create a rack
            var rackComp = Common_710.New_DT_Component("MainRack");
            rackComp.MarkAsRack();
            var rack = solution.Build<Rack_710>(rackComp, 1, lookup);
            rack.Calculations([
                "sPinX: 150",
                "sPinY: 150",
                "sWidth: 120",
                "sHeight: 200"
            ]);
            _model.AddChildComponent<KnComponent>(rack);

            // Create equipment inside rack
            var equipNames = new[] { "Server1", "Server2", "Storage1" };
            foreach (var (name, index) in equipNames.Select((n, i) => (n, i)))
            {
                var equipComp = Common_710.New_DT_Component(name);
                equipComp.MarkAsEquipment();
                var equipment = solution.Build<Equipment_710>(equipComp, index + 1, lookup);
                equipment.Calculations([
                    $"sPinX: {200 + index * 150}",
                    $"sPinY: {300}",
                    "sWidth: 100",
                    "sHeight: 80"
                ]);
                rack.AddChild(equipment);
            }

            // Render to diagram
            _diagram = _model.RenderDiagram("CircuitView", clear: false, () => { });

            UpdateCounts();

            _statusMessage = $"✅ Circuit diagram created with {_nodeCount} components";
            _isError = false;

            await Task.Run(() => PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null)));
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error creating circuit diagram: {ex.Message}";
            _isError = true;
            $"CreateCircuitDiagram ERROR: {ex}".WriteError();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    protected async Task AddSystemBlock()
    {
        if (_model == null || _diagram == null) return;

        try
        {
            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            var blockName = $"System_{DateTime.Now.Ticks % 10000}";
            var blockComp = Common_710.New_DT_Component(blockName);
            blockComp.MarkAsBlock("");
            var block = solution.Build<SystemBlock_710>(blockComp, _nodeCount + 1, lookup);
            
            block.Calculations([
                $"sPinX: {100 + (_nodeCount * 30)}",
                $"sPinY: {100 + (_nodeCount * 30)}",
                "sWidth: 200",
                "sHeight: 100"
            ]);
            
            _model.AddChildComponent<KnComponent>(block);

            // Re-render diagram
            _diagram = _model.RenderDiagram("SystemView", clear: false, () => { });
            
            UpdateCounts();
            _statusMessage = $"✅ Added {blockName}";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error adding block: {ex.Message}";
            _isError = true;
        }
    }

    protected async Task AddCircuitBlock()
    {
        if (_model == null || _diagram == null) return;

        try
        {
            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            var equipName = $"Equipment_{DateTime.Now.Ticks % 10000}";
            var equipComp = Common_710.New_DT_Component(equipName);
            equipComp.MarkAsEquipment();
            var equipment = solution.Build<Equipment_710>(equipComp, _nodeCount + 1, lookup);
            
            equipment.Calculations([
                $"sPinX: {150 + (_nodeCount * 30)}",
                $"sPinY: {150 + (_nodeCount * 30)}",
                "sWidth: 100",
                "sHeight: 80"
            ]);
            
            _model.AddChildComponent<KnComponent>(equipment);

            // Re-render diagram
            _diagram = _model.RenderDiagram("CircuitView", clear: false, () => { });
            
            UpdateCounts();
            _statusMessage = $"✅ Added {equipName}";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error adding equipment: {ex.Message}";
            _isError = true;
        }
    }

    protected void ClearDiagram()
    {
        if (_diagram != null)
        {
            _diagram.ClearAll();
            _diagram = null;
        }
        
        _model = null;
        _selectedItem = null;
        UpdateCounts();
        
        _statusMessage = "🗑️ Diagram cleared";
        _isError = false;
        StateHasChanged();
    }

    protected void HandleItemSelected(ITreeNode item)
    {
        _selectedItem = item;
        $"DiagramViewer: Selected - {item.GetTreeNodeTitle()}".WriteInfo();
        StateHasChanged();
    }

    protected async Task RefreshTree()
    {
        await Task.Run(() => PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null)));
        StateHasChanged();
    }

    private void UpdateCounts()
    {
        if (_diagram != null)
        {
            _nodeCount = _diagram.Nodes.Count;
            _linkCount = _diagram.Links.Count;
        }
        else
        {
            _nodeCount = 0;
            _linkCount = 0;
        }
    }

    public void Dispose()
    {
        "DiagramViewer: Disposing".WriteInfo();
        _diagram = null;
        _model = null;
    }
}
