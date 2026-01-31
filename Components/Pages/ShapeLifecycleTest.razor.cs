
using FoundryMicroCore.Core.Extensions;

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
    
    // Track current parameter values for UI
    private double _currentWidth = 1.0;
    private double _currentHeight = 2.0;
    private double _currentDepth = 3.0;
    private string _currentGeomType = "Box";
    
    // Track current position values for Transform-only updates
    private double _currentX = 0.0;
    private double _currentY = 0.0;
    private double _currentZ = 0.0;

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
            width: _currentWidth,
            height: _currentHeight,
            depth: _currentDepth,
            geomType: _currentGeomType
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
                $"✅ Stage acquired from canvas: {_stage.Name}".WriteInfo();
                
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
        //await Task.Delay(500);
        
        // Check if still valid after delay
        if (_canvasRef == null)
        {
            "⚠️ Canvas ref is null - connection may have dropped".WriteWarning();
            return;
        }

        // Render component's geometry to the stage
        var ctx3D = RenderContext3D.CreateFromStage(_stage, deep: true);
        _testModel.RenderGeometry3D(ctx3D);
        
        $"✅ Shape rendered - Stage now has {GetShapeCount()} shapes".WriteSuccess();

        // CRITICAL: With animations paused, must manually call RenderStage
        // This collects stale shapes and sends them to JavaScript
        await _stage.RenderStage(_globalTick++, 60.0);
        
        StateHasChanged();
    }
    
    private int _globalTick = 0;

    // Parameter change handlers - geometry changes should trigger recreation
    private void OnGeometryTypeChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        
        _currentGeomType = e.Value.ToString()!;
        $"🔄 Setting GeometryType to '{_currentGeomType}'".WriteInfo();
        
        ModelEditor!.SetParameter(_testComponent, "GeometryType", $"'{_currentGeomType}'");
    }

    private void OnWidthChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        
        _currentWidth = double.Parse(e.Value.ToString()!);
        $"🔄 Setting Width to {_currentWidth}".WriteInfo();
        
        ModelEditor!.SetParameter(_testComponent, "Width", $"{_currentWidth}");
    }

    private void OnHeightChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        
        _currentHeight = double.Parse(e.Value.ToString()!);
        $"🔄 Setting Height to {_currentHeight}".WriteInfo();
        
        ModelEditor!.SetParameter(_testComponent, "Height", $"{_currentHeight}");
    }

    private void OnDepthChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || e.Value == null) return;
        
        _currentDepth = double.Parse(e.Value.ToString()!);
        $"🔄 Setting Depth to {_currentDepth}".WriteInfo();
        
        ModelEditor!.SetParameter(_testComponent, "Depth", $"{_currentDepth}");
    }

    // Position change handlers - test transform-only updates via UPDATE mode
    private void OnXPositionChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || _stage == null || e.Value == null) return;
        
        _currentX = double.Parse(e.Value.ToString()!);
        
        ModelEditor!.SetParameter(_testComponent, "X", $"{_currentX}");
    }

    private void OnYPositionChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || _stage == null || e.Value == null) return;
        
        _currentY = double.Parse(e.Value.ToString()!);
        
        ModelEditor!.SetParameter(_testComponent, "Y", $"{_currentY}");
    }

    private void OnZPositionChanged(ChangeEventArgs e)
    {
        if (_testComponent == null || _stage == null || e.Value == null) return;
        
        _currentZ = double.Parse(e.Value.ToString()!);
        
        ModelEditor!.SetParameter(_testComponent, "Z", $"{_currentZ}");
    }

    private async Task RefreshStage()
    {
        if ( _stage == null) return;
        
        // PHASE 1: Flush pending deletions from smashed parameters
        // This sends the old shape's deletion to JavaScript BEFORE creating new one
        await _stage.RenderStage(_globalTick++, 60.0);
    }

    private async Task UpdateShape()
    {
        if (_testComponent == null || _stage == null) return;
        
        // Trigger re-evaluation of component's geometry
        // If cache empty → CREATE mode (new shape)
        // If cache has shape → UPDATE mode (modify existing)
        var ctx3D = RenderContext3D.CreateFromStage(_stage, deep: true);
        _testComponent.RenderGeometry3D(ctx3D);
        
        // Send updates to JavaScript (TransformUpdates or FullRefresh depending on what changed)
        await _stage.RenderStage(_globalTick++, 60.0);
        
        StateHasChanged();
    }



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
        
        // TODO: Replace with new collection API
        // var slots = _stage.AllSlotsOfType<FoGlyph3D>();
        // var totalShapes = slots.Sum(s => s.Count());
        var totalShapes = 0; // Temporary placeholder

        return totalShapes;
    }
    
    public void Dispose()
    {
        // PubSub handles cleanup automatically
    }

}
