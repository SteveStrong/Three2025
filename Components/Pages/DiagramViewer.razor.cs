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
using Three2025.Components.DiagramWidgets;

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
    protected string _loadingMessage = "Initializing...";
    
    protected int _nodeCount;
    protected int _linkCount;

    // ============================================
    // CORE DIAGRAM PATTERN - INITIALIZATION
    // ============================================
    protected override void OnInitialized()
    {
        "DiagramViewer: Initializing".WriteInfo();
        
        // STEP 1: Establish the diagram (BEFORE any rendering!)
        _diagram = MentorServices.EstablishDiagram<MentorDiagram>("DiagramViewerCanvas");
        
        // STEP 2: Register node-widget mappings (CRITICAL!)
        // Register all node types with their default rendering
        _diagram.Register<SystemBlockEditor, SystemBlockWidget>(true);
        _diagram.Register<CircuitNodeEditor, CircuitNodeWidget>(true);
        _diagram.Register<CircuitGroupEditor, CircuitGroupWidget>(true);
        
        "DiagramViewer: Diagram established and widgets registered".WriteSuccess();
        
        base.OnInitialized();
    }

    /// <summary>
    /// Create a system-level diagram using the proper model rendering pattern.
    /// </summary>
    protected async Task CreateSystemDiagram()
    {
        _isLoading = true;
        _loadingMessage = "📐 Building system model...";
        _statusMessage = "Creating system diagram...";
        StateHasChanged();

        try
        {
            await Task.Delay(50);

            // STEP 1: Create the model
            _model = MentorServices.EstablishModel<Model_710>("DiagramViewerModel");
            _model.SetExpanded(true);

            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            // STEP 2: Build domain model with system blocks
            var blockNames = new[] { "MainSystem", "SubSystem1", "SubSystem2" };
            SystemBlock_710? parentBlock = null;
            
            foreach (var (name, index) in blockNames.Select((n, i) => (n, i)))
            {
                var blockComp = Common_710.New_DT_Component(name);
                blockComp.MarkAsBlock("");
                var block = solution.Build<SystemBlock_710>(blockComp, index + 1, lookup);
                
                block.Calculations([
                    $"sPinX: {150 + (index * 200)}",
                    $"sPinY: {200}",
                    "sWidth: 180",
                    "sHeight: 120"
                ]);
                
                if (parentBlock == null)
                {
                    _model.AddChildComponent<KnComponent>(block);
                    parentBlock = block;
                }
                else
                {
                    parentBlock.AddChild(block);
                }
            }

            // STEP 3: Render model to diagram using RenderEditor pattern
            _diagram!.ClearAll();
            _model.RenderDiagram("SystemView", clear: true, () => 
            {
                "System diagram rendering complete".WriteSuccess();
            });

            UpdateCounts();
            
            _statusMessage = $"✅ System diagram created with {_nodeCount} nodes";
            _isError = false;
            
            await RefreshTree();
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
    /// Create a circuit-level diagram with circuit nodes and connections.
    /// </summary>
    protected async Task CreateCircuitDiagram()
    {
        _isLoading = true;
        _loadingMessage = "⚡ Building circuit model...";
        _statusMessage = "Creating circuit diagram...";
        StateHasChanged();

        try
        {
            await Task.Delay(50);

            // STEP 1: Create model
            _model = MentorServices.EstablishModel<Model_710>("DiagramViewerModel");
            _model.SetExpanded(true);

            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            // STEP 2: Build circuit nodes in domain model
            var nodeNames = new[] { "Node_A", "Node_B", "Node_C", "Node_D" };
            var nodes = new List<CircuitNode_710>();
            
            foreach (var (name, index) in nodeNames.Select((n, i) => (n, i)))
            {
                var nodeComp = Common_710.New_DT_Component(name);
                // CircuitNode_710 doesn't need marking
                var node = solution.Build<CircuitNode_710>(nodeComp, index + 1, lookup);
                
                // Position nodes in a grid pattern
                var col = index % 2;
                var row = index / 2;
                node.Calculations([
                    $"sPinX: {200 + (col * 250)}",
                    $"sPinY: {200 + (row * 200)}",
                    "sWidth: 120",
                    "sHeight: 80"
                ]);
                
                _model.AddChildComponent<KnComponent>(node);
                nodes.Add(node);
            }

            // STEP 3: Render model to diagram
            _diagram!.ClearAll();
            _model.RenderDiagram("CircuitView", clear: true, () => 
            {
                "Circuit diagram rendering complete".WriteSuccess();
            });

            UpdateCounts();
            _statusMessage = $"✅ Circuit diagram created with {_nodeCount} nodes";
            _isError = false;

            await RefreshTree();
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
            
            // Position with offset from previous nodes
            block.Calculations([
                $"sPinX: {100 + (_nodeCount * 30) % 600}",
                $"sPinY: {100 + ((_nodeCount * 30) / 600) * 150}",
                "sWidth: 180",
                "sHeight: 120"
            ]);
            
            _model.AddChildComponent<KnComponent>(block);

            // Re-render entire diagram following the guide pattern
            _model.RenderDiagram("SystemView", clear: true, () => { });
            
            UpdateCounts();
            _statusMessage = $"✅ Added {blockName}";
            await RefreshTree();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error adding block: {ex.Message}";
            _isError = true;
        }
    }

    protected async Task AddCircuitNode()
    {
        if (_model == null || _diagram == null) return;

        try
        {
            var solution = _model.EstablishSolution();
            var lookup = solution.GetLookup();

            var nodeName = $"Node_{DateTime.Now.Ticks % 10000}";
            var nodeComp = Common_710.New_DT_Component(nodeName);
            // CircuitNode_710 doesn't need marking
            var node = solution.Build<CircuitNode_710>(nodeComp, _nodeCount + 1, lookup);
            
            // Position with offset
            node.Calculations([
                $"sPinX: {150 + (_nodeCount * 30) % 600}",
                $"sPinY: {150 + ((_nodeCount * 30) / 600) * 150}",
                "sWidth: 120",
                "sHeight: 80"
            ]);
            
            _model.AddChildComponent<KnComponent>(node);

            // Re-render entire diagram
            _model.RenderDiagram("CircuitView", clear: true, () => { });
            
            UpdateCounts();
            _statusMessage = $"✅ Added {nodeName}";
            await RefreshTree();
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
