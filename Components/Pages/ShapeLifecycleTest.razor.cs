

using Microsoft.AspNetCore.Components;
using FoundryRulesAndUnits.Models;
using FoundryRulesAndUnits.Extensions;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Evaluator;
using FoundryMentorModeler.Shared;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Viewers;

namespace Three2025.Components.Pages;

public partial class ShapeLifecycleTest : ComponentBase
{
    [Inject]
    public IWorkspace Workspace { get; set; }
    
    [Inject]
    public IMentorServices MentorServices { get; set; }
    
    [Inject]
    public IModelEditor ModelEditor { get; set; }

    private Canvas3DComponent _canvasRef;
    private FoStage3D _stage;
    private AnimatedKnModel _testModel;
    private AnimatedParameterTestComponent _testComponent;

    private string _activeTab = "model";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        // Subscribe to model edit changes to refresh tree
        MentorServices?.PubSub?.SubscribeTo<ModelEditChanged>(OnModelEditChanged);
        
        // Pause animations IMMEDIATELY on page initialization - before canvas even renders
        AnimationFrameBus.PauseAllAnimations();
        $"⏸️ Animation paused at OnInitialized - MANUAL CONTROL ONLY".WriteInfo();

                        
        // Create test model 
        _testModel = MentorServices.EstablishModel<AnimatedKnModel>("LifecycleTestModel");
        _testModel.SetExpanded(true);  // Ensure model shows expanded in tree
        $"📊 Model '{_testModel.Name}' established and expanded".WriteInfo();
    }

    private void OnModelEditChanged(ModelEditChanged message)
    {
        $"📢 Received ModelEditChanged: {message.State}".WriteInfo();
        InvokeAsync(StateHasChanged);
    }

    protected void CreateComponent()
    {
        if (_testComponent != null)
        {
            "⚠️ Test component already exists".WriteWarning();
            return;
        }

        $"🔵 CREATING NEW COMPONENT".WriteInfo();

        // Create new test component with reasonable size
        _testComponent = new AnimatedParameterTestComponent(
            "TestComponent",
            width: 1.0,    // 1x2x3 is much more visible than 100x100x100
            height: 2.0,
            depth: 3.0,
            geomType: "Box"
        );

        // Add component to model via ModelEditor (this is what we're testing)
        ModelEditor!.AddChild(_testModel, _testComponent);
        $"✨ Added AnimatedParameterTestComponent '{_testComponent.Name}' to model via ModelEditor".WriteInfo();
        
        // Debug: Check what GetTreeChildren returns
        var treeChildren = _testModel.GetTreeChildren();
        $"🔍 Model.GetTreeChildren() returned {treeChildren?.Count() ?? 0} items".WriteInfo();
        $"🔍 Model.HasChildren() = {_testModel.GetTreeChildren().Count()}".WriteInfo();

        StateHasChanged();
    }

    protected void CreateGeometry()
    {
        if (_testComponent == null || _stage == null) 
        {
            "⚠️ No test component exists to create geometry".WriteWarning();
            return;
        }

        $"🔵 CREATING GEOMETRY VIA COMPONENT".WriteInfo();

        // Establish geometry via component method
        var view = _stage.GetName();
        var ( geometry, parameter) = ModelEditor.EstablishGeometry3D(_testComponent, view);

        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            $"🔧 ShapeLifecycleTest OnAfterRenderAsync: Setting up".WriteInfo();
            
            // Wait for canvas initialization (canvas creates stage in its OnAfterRenderAsync)
            await Task.Delay(200);
            
            // Stage is auto-created by Canvas3DComponent
            _stage = _canvasRef?.Stage;
            if (_stage != null && MentorServices != null)
            {
                $"✅ Stage acquired from canvas: {_stage.Key}".WriteInfo();
                
                // Update UI to reflect the initialized stage
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                $"❌ ERROR: Stage or MentorServices is null after delay".WriteError();
                $"   _stage: {(_stage != null ? "OK" : "NULL")}".WriteError();
                $"   MentorServices: {(MentorServices != null ? "OK" : "NULL")}".WriteError();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }
    
    private async Task TriggerSingleFrame()
    {
        $"🎬 Triggering single render frame".WriteInfo();
        
        // Use AnimationFrameBus to trigger a single frame that bypasses guards
        await AnimationFrameBus.TriggerSingleFrame();
        
        StateHasChanged();
    }

    private void CreateShape()
    {
        if (_testComponent == null || _stage == null) return;

        $"🔵 CREATE/RECREATE SHAPE via ModelEditor".WriteInfo();

        var view = _stage.GetName();

        // ModelEditor handles: establish parameter, trigger evaluation, publish event
        var (geometry, parameter) = ModelEditor.EstablishGeometry3D(_testComponent, view);
        ModelEditor.ComputeParameter(parameter);

        var shape = parameter.GetValue().AsShape3D();
        if (shape != null)
        {
            $"✅ Shape created - {shape.Name}".WriteSuccess();
            _stage?.AddShape<FoShape3D>(shape);

        }
        else
        {
            $"❌ Shape creation failed - parameter value is NULL".WriteError();
        }

        StateHasChanged();
    }

    private async Task RenderShape()
    {
        if (_testComponent == null || _stage == null) return;

        $"🔵 RENDER SHAPE to stage".WriteInfo();
        
        // Wait a bit for SignalR connection to stabilize
        await Task.Delay(500);
        
        // Check if still valid after delay
        if (_canvasRef == null)
        {
            "⚠️ Canvas ref is null - connection may have dropped".WriteWarning();
            return;
        }

        // Render component's geometry to the stage
        var ctx3D = RenderContext3D.Create(_stage, _stage.GetName(), deep: true);
        _testComponent.RenderGeometry3D(ctx3D);
        
        $"✅ Shape rendered - Stage now has {GetShapeCount()} shapes".WriteSuccess();

        // CRITICAL: With animations paused, must manually call RenderStage
        // This collects stale shapes and sends them to JavaScript
        await _stage.RenderStage(_globalTick++, 60.0);
        
        StateHasChanged();
    }
    
    private int _globalTick = 0;



    private void RemoveShape()
    {
        if (_testComponent == null)
        {
            "⚠️ No component to remove".WriteWarning();
            return;
        }

        $"🔴 REMOVE SHAPE via component removal".WriteInfo();
        $"📊 Stage has {GetShapeCount()} shapes BEFORE removal".WriteInfo();


    
        
        StateHasChanged();
    
        $"📊 Stage has {GetShapeCount()} shapes AFTER removal".WriteInfo();
    }

    private int GetShapeCount()
    {
        if (_stage == null) return 0;
                
        var slots = _stage.AllSlotsOfType<FoGlyph3D>();
        
        // DEBUG: Count shapes in slots
        var totalShapes = slots.Sum(s => s.Count());

        return totalShapes;
    }
    
    public void Dispose()
    {
        // PubSub handles cleanup automatically
    }

}
