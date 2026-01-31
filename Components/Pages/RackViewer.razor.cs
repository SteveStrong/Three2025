#nullable enable
using FoundryMicroCore.Core.Extensions;

using Microsoft.AspNetCore.Components;
using BlazorComponentBus;
using FoundryMentorModeler.Model;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Extensions;
using FoundryWorldsAndDrawings.PubSub;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryMicroCore.Core;

// DISABLED: using Plugin_710.Model;

namespace Three2025.Components.Pages;

public class RackViewerBase : ComponentBase, IDisposable
{
    [Inject] protected IWorkspace Workspace { get; set; } = null!;
    [Inject] protected IMentorServices MentorServices { get; set; } = null!;
    [Inject] protected IModelEditor ModelEditor { get; set; } = null!;
    [Inject] public ComponentBus PubSub { get; set; } = null!;
    [Inject] public ICageTech CageTech { get; set; } = null!;

    protected Canvas3DComponent? Canvas3DRef;
    protected int CanvasWidth { get; set; } = 800;
    protected int CanvasHeight { get; set; } = 600;

    protected int ComponentCount { get; set; } = 0;
    protected string _activeTreeTab = "model";
    
    protected Model_710? _model = null;
    protected Rack_710? _mainRack = null;
    protected List<Equipment_710> _standaloneEquipment = new();
    protected ITreeNode? _selectedItem = null;
    protected bool _isLoading = false;
    protected string? _statusMessage = null;
    protected bool _isError = false;

    protected FoStage3D? _stage;

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

    /// <summary>
    /// Add a simple test shape directly to the stage - bypasses model entirely.
    /// Proves the 3D rendering pipeline works independently.
    /// </summary>
    protected void AddTestShape()
    {
        if (_stage == null)
        {
            _statusMessage = "⚠️ Stage not ready";
            _isError = true;
            StateHasChanged();
            return;
        }

        try
        {
            var random = new Random();
            var xPos = -3.0 + (random.NextDouble() * 2.0);  // -3 to -1m (left of rack)
            var zPos = -1.0 + (random.NextDouble() * 2.0);  // -1 to 1m
            
            var shapeName = $"TestShape_{DateTime.Now.Ticks % 10000}";
            
            // Create transform
            var transform = new Transform3($"{shapeName}Transform");
            transform.MoveTo(xPos, 0.5, zPos);
            
            // Create shape directly (no model involvement)
            var testShape = new FoShape3D(shapeName, "Cyan")
            {
                GlyphId = Guid.NewGuid().ToString(),
                Width = 0.5,
                Height = 0.5,
                Depth = 0.5,
                Transform = transform
            }.CreateBox(shapeName, 0.5, 0.5, 0.5);

            // Add directly to stage
            _stage.AddShape(testShape);
            
            $"RackViewer: Added test shape '{shapeName}' directly to stage at ({xPos:F2}, 0.5, {zPos:F2})".WriteSuccess();
            
            _statusMessage = $"✅ Added test shape '{shapeName}' (direct to stage)";
            _isError = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error adding test shape: {ex.Message}";
            _isError = true;
            $"RackViewer ERROR: {ex}".WriteError();
        }
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

            // Get solution SERVICE (not a tree node anymore!)
            var solutionService = _model.GetSolutionService();
            var lookup = solutionService.GetLookup();

            var rackComp = Common_710.New_DT_Component("MainRack");
            rackComp.MarkAsRack();

            _mainRack = solutionService.Build<Rack_710>(rackComp, 1, lookup);
            _mainRack.SetExpanded(true);
            "RackViewer: Rack_710 created via service".WriteSuccess();
            
            // NEW PATTERN: Add rack directly to model (not to solution)
            // CRITICAL: Must use AddChildComponent<KnComponent> so it stores in Members<KnComponent>()
            // Model_710.RenderGeometry3D reads Members<KnComponent>(), not Members<Rack_710>()
            _model.AddChildComponent<KnComponent>(_mainRack);
            "RackViewer: Rack added directly to Model".WriteSuccess();
            
            // DEBUG: Check if rack was added to model
            var componentCount = _0 /* TODO: model.Members<KnComponent>() */.Count;
            $"RackViewer: Model has {componentCount} components (KnComponent) after AddChildComponent".WriteInfo();

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

        var solutionService = _model.GetSolutionService();
        var lookup = solutionService.GetLookup();

        // Add Server
        var equipComp1 = Common_710.New_DT_Component("Server_01");
        equipComp1.MarkAsEquipment();
        var equipment1 = solutionService.Build<Equipment_710>(equipComp1, 1, lookup);

        equipment1.Calculations([
            "X|m: 0",
            "Y|m: 0.5",
            "Z|m: 0",
            "Width|m: 0.7",
            "Height|m: 0.4",
            "Depth|m: 0.5",
            "PivotY|m: -0.2"
        ]);

        // Use AddChild - the canonical API for Base_710 child management
        _mainRack.AddChild(equipment1);
        "RackViewer: Added Server_01".WriteSuccess();
    }

    private void RenderToStage()
    {
        if (_stage == null || _model == null)
        {
            "RackViewer: Cannot render - stage or model is null".WriteWarning();
            return;
        }

        try
        {
            "\n========== 🚀 MODEL-DRIVEN RENDER START ==========".WriteWarning();
            "RackViewer: Creating RenderContext3D from stage".WriteInfo();
            
            // Create render context from our stage
            var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
            
            "RackViewer: Calling RenderGeometry3D on model (model-down tree walk)".WriteInfo();
            $"RackViewer: Model has {_0 /* TODO: model.Members<KnComponent>() */.Count} direct children".WriteInfo();
            
            // Render from model - the canonical framework pattern
            // Model → Solution → Rack → Equipment
            // This uses KnModel.RenderGeometry3D which walks Members<KnComponent>
            // Then Base_710.Subcomponents walks HasSub.Members for child components
            _model.RenderGeometry3D(ctx);
            
            "========== ✅ MODEL-DRIVEN RENDER COMPLETE ==========\n".WriteSuccess();
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
        $"RackViewer: Selected - {item.GetTreeViewNodeTitle()}".WriteInfo();
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
            var solutionService = _model.GetSolutionService();
            var lookup = solutionService.GetLookup();

            var equipmentName = $"Equipment_{DateTime.Now.Ticks % 10000}";
            var equipComp = Common_710.New_DT_Component(equipmentName);
            equipComp.MarkAsEquipment();

            var equipment = solutionService.Build<Equipment_710>(equipComp, 1, lookup);

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

            // Use AddChild - the canonical API for Base_710 child management
            _mainRack.AddChild(equipment);

            // Re-render
            RenderToStage();

            ComponentCount = 1 + (_mainRack.ModelComponents<Equipment_710>()?.Count() ?? 0) + _standaloneEquipment.Count;
            _statusMessage = $"✅ Added {equipmentName} to rack";
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

    /// <summary>
    /// Add standalone equipment directly on the floor (not inside a rack)
    /// </summary>
    protected void AddStandaloneEquipment()
    {
        if (_stage == null || _model == null)
        {
            _statusMessage = "⚠️ Create rack model first";
            _isError = true;
            StateHasChanged();
            return;
        }

        try
        {
            var solutionService = _model.GetSolutionService();
            var lookup = solutionService.GetLookup();

            var equipmentName = $"FloorUnit_{DateTime.Now.Ticks % 10000}";
            var equipComp = Common_710.New_DT_Component(equipmentName);
            equipComp.MarkAsEquipment();

            var equipment = solutionService.Build<Equipment_710>(equipComp, 1, lookup);

            // Position on floor, offset from rack
            var random = new Random();
            var xPos = 2.0 + (random.NextDouble() * 2.0); // 2-4m to the right of rack

            equipment.Calculations([
                $"X|m: {xPos:F2}",
                "Y|m: 0",  // On the floor
                "Z|m: 0",
                "Width|m: 0.5",
                "Height|m: 0.8",
                "Depth|m: 0.4",
                "PivotY|m: -0.4"  // Pivot at bottom so it sits on floor
            ]);

            // Add standalone equipment as direct child of model
            _model.AddChildComponent(equipment);
            _standaloneEquipment.Add(equipment);

            // Render standalone equipment directly
            RenderStandaloneEquipment(equipment);

            ComponentCount = 1 + (_mainRack?.ModelComponents<Equipment_710>()?.Count() ?? 0) + _standaloneEquipment.Count;
            _statusMessage = $"✅ Added standalone {equipmentName} on floor";
            _isError = false;

            PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null));
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error adding standalone equipment: {ex.Message}";
            _isError = true;
            $"RackViewer ERROR: {ex}".WriteError();
        }
    }

    /// <summary>
    /// TEST: Add rack directly (bypassing model-down rendering) to prove rack geometry works
    /// </summary>
    protected void AddTestRackDirect()
    {
        if (_stage == null || _mainRack == null)
        {
            _statusMessage = "⚠️ Create rack model first";
            _isError = true;
            StateHasChanged();
            return;
        }

        try
        {
            "TEST: Rendering rack directly to stage (bypass model)".WriteWarning();
            
            var ctx = RenderContext3D.CreateFromStage(_stage, deep: true);
            _mainRack.RenderGeometry3D(ctx);
            
            _statusMessage = $"✅ TEST: Rendered rack directly (bypassing model tree walk)";
            _isError = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error rendering rack directly: {ex.Message}";
            _isError = true;
            $"RackViewer ERROR: {ex}".WriteError();
        }
    }

    private void RenderStandaloneEquipment(Equipment_710 equipment)
    {
        if (_stage == null) return;

        try
        {
            "\n========== 🎯 DIRECT RENDER START ==========".WriteWarning();
            $"RackViewer: Rendering equipment {equipment.Name} DIRECTLY (bypassing tree walk)".WriteInfo();
            
            var ctx = RenderContext3D.CreateFromStage(_stage, deep: false);
            equipment.RenderGeometry3D(ctx);
            
            $"RackViewer: Rendered standalone equipment {equipment.Name}".WriteSuccess();
            "========== ✅ DIRECT RENDER COMPLETE ==========\n".WriteSuccess();
        }
        catch (Exception ex)
        {
            $"RackViewer: Failed to render standalone equipment - {ex.Message}".WriteError();
        }
    }

    /// <summary>
    /// Create a showcase/gallery of different Plugin710 component types.
    /// Demonstrates that the platform can render various component types correctly.
    /// Arranges components spatially for visual inspection.
    /// </summary>
    protected async Task CreateComponentShowcase()
    {
        if (_stage == null || _model == null)
        {
            _statusMessage = "⚠️ Stage or model not ready";
            _isError = true;
            StateHasChanged();
            return;
        }

        _isLoading = true;
        _statusMessage = "🎭 Creating component showcase...";
        StateHasChanged();

        try
        {
            await Task.Delay(50);

            var solutionService = _model.GetSolutionService();
            var lookup = solutionService.GetLookup();

            var showcaseComponents = new List<Base_710>();
            var spacing = 3.0; // meters between components
            var currentX = -9.0; // start position

            // 1. Rack with Equipment
            var rackComp = Common_710.New_DT_Component("ShowcaseRack");
            rackComp.MarkAsRack();
            var rack = solutionService.Build<Rack_710>(rackComp, 1, lookup);
            rack.Calculations([
                $"X|m: {currentX}",
                "Y|m: 0",
                "Z|m: 0",
                "Width|m: 0.8",
                "Height|m: 2.0",
                "Depth|m: 0.6",
                "PivotY|m: -1.0"
            ]);
            _model.AddChildComponent<KnComponent>(rack);
            showcaseComponents.Add(rack);

            // Add equipment to rack
            var equipInRackComp = Common_710.New_DT_Component("RackServer");
            equipInRackComp.MarkAsEquipment();
            var equipInRack = solutionService.Build<Equipment_710>(equipInRackComp, 1, lookup);
            equipInRack.Calculations([
                "X|m: 0",
                "Y|m: 0.5",
                "Z|m: 0",
                "Width|m: 0.7",
                "Height|m: 0.4",
                "Depth|m: 0.5",
                "PivotY|m: -0.2"
            ]);
            rack.AddChild(equipInRack);
            currentX += spacing;

            // 2. Standalone Equipment (different colors/sizes)
            var equipTypes = new[]
            {
                ("Server", 0.7, 0.4, 0.5, "DarkGray"),
                ("Storage", 0.8, 0.6, 0.6, "Navy"),
                ("Network", 0.5, 0.3, 0.4, "DarkGreen"),
                ("Power", 0.4, 0.5, 0.4, "DarkRed")
            };

            foreach (var (name, width, height, depth, color) in equipTypes)
            {
                var equipComp = Common_710.New_DT_Component($"Showcase_{name}");
                equipComp.MarkAsEquipment();
                equipComp.MetaData().SetValue("Color", color);
                
                var equipment = solutionService.Build<Equipment_710>(equipComp, showcaseComponents.Count + 1, lookup);
                equipment.Calculations([
                    $"X|m: {currentX}",
                    "Y|m: 0",
                    "Z|m: 0",
                    $"Width|m: {width}",
                    $"Height|m: {height}",
                    $"Depth|m: {depth}",
                    $"PivotY|m: {-height / 2}"
                ]);
                _model.AddChildComponent<KnComponent>(equipment);
                showcaseComponents.Add(equipment);
                currentX += spacing;
            }

            // 3. Cable (if we can create one)
            try
            {
                var cableComp = Common_710.New_DT_Component("ShowcaseCable");
                cableComp.MarkAsCable();
                var cable = solutionService.Build<Cable_710>(cableComp, showcaseComponents.Count + 1, lookup);
                cable.Calculations([
                    $"X|m: {currentX}",
                    "Y|m: 0.5",
                    "Z|m: 0"
                ]);
                _model.AddChildComponent<KnComponent>(cable);
                showcaseComponents.Add(cable);
                currentX += spacing;
            }
            catch (Exception ex)
            {
                $"Showcase: Cable creation skipped - {ex.Message}".WriteWarning();
            }

            // Render all components
            RenderToStage();

            ComponentCount = showcaseComponents.Count;
            _statusMessage = $"✅ Component showcase created with {ComponentCount} components";
            _isError = false;

            // Signal tree to refresh
            await Task.Run(() => PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(null)));
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error creating showcase: {ex.Message}";
            _isError = true;
            $"CreateComponentShowcase ERROR: {ex}".WriteError();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    protected void ClearModel()
    {
        _model = null;
        _mainRack = null;
        _standaloneEquipment.Clear();
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

    /// <summary>
    /// Create routing cage visualization around all racks in the scene.
    /// This creates nodes at connector points and links between them for cable routing.
    /// </summary>
    protected void CreateRoutingCage()
    {
        if (_stage == null || _mainRack == null)
        {
            _statusMessage = "⚠️ Create rack model first";
            _isError = true;
            StateHasChanged();
            return;
        }

        try
        {
            "RackViewer: Creating routing cage from model components (model-driven)".WriteInfo();
            
            // Model-driven approach: CageTech works with FoRack shapes from the stage
            // It will find racks automatically from the stage
            CageTech.CreateRoutingCage();
            
            var (nodes, links) = CageTech.GetNodesAndLinks();
            _statusMessage = $"✅ Routing cage created: {nodes.Count} nodes, {links.Count} links";
            _isError = false;
            
            "RackViewer: Routing cage created successfully".WriteSuccess();
        }
        catch (Exception ex)
        {
            _statusMessage = $"❌ Error creating routing cage: {ex.Message}";
            _isError = true;
            $"CreateRoutingCage ERROR: {ex}".WriteError();
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
