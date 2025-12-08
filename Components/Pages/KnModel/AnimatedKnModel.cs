using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// A KnModel subclass that reacts to animation loop events.
/// This demonstrates how KnModels can respond to PreAnimationEvent for geometry updates.
/// </summary>
public class AnimatedKnModel : PartModel
{
    private bool _animationSetup = false;
    
    public AnimatedKnModel(string name) : base(name)
    {
    }
    
    public AnimatedKnModel(string name, IMentorServices mentorServices) : base(name, mentorServices)
    {
        $"AnimatedKnModel: Constructor called for '{name}'".WriteSuccess();
        
        Calculations([
            "X: 10",
            "Y: 100",
            "Z: 10000",
            "GeomType: 'Box'",
            "Material: 'Blue'"
        ]);

        var param = this.EstablishParameter("Param1");
        param.SetValue(42);

        EnsureAnimationSetup();
        
        $"AnimatedKnModel: PreAnimationRefresh set up, PreContextLink is {(PreContextLink != null ? "SET" : "NULL")}".WriteInfo();
    }
    
    /// <summary>
    /// Ensure animation callback is registered. Called from constructor and from page init
    /// (in case model already existed and constructor didn't run).
    /// </summary>
    public void EnsureAnimationSetup()
    {
        if (_animationSetup) return;
        _animationSetup = true;
        
        var param = this.EstablishParameter("Param1");
        
        // Update param with tick count and refresh tree
        PreAnimationRefresh((comp, evt) =>
        {
            if (evt.tick % 120 == 0)
            {
                param.SetValue(evt.tick);
                // Get services from model - no need to pass it in
                var services = GetMentorServices();
                // Targeted refresh - only update this specific parameter's tree node
                services?.PubSub?.Publish<RefreshRenderMessage>(RefreshRenderMessage.RefreshValueChanged(param));
                $"AnimatedKnModel '{Name}': PreAnimationRefresh tick={evt.tick}".WriteInfo();
            }
        });
    }

    /// <summary>
    /// Override to trigger geometry re-rendering after parameter updates.
    /// The dependency mechanism handles cache clearing via BeforeSmash,
    /// but we still need to call RenderGeometry3D to recreate shapes.
    /// </summary>
    public override void OnPreAnimationEvent(PreAnimationEvent evt)
    {
        // Only log periodically to avoid flooding
        var shouldLog = evt.tick % 120 == 0;
        
        if (shouldLog)
            $"AnimatedKnModel '{Name}': OnPreAnimationEvent BEGIN tick={evt.tick}".WriteInfo();
        
        // 1. Let base class propagate to all children
        //    This triggers PreAnimationRefresh callbacks which may update parameters
        //    Parameter updates trigger smash cascade via dependencies
        base.OnPreAnimationEvent(evt);
        
        if (shouldLog)
            $"AnimatedKnModel '{Name}': After base.OnPreAnimationEvent, about to RenderGeometry3D".WriteInfo();
        
        // 2. Re-render geometry to pick up any cache invalidations
        //    Only render to stages that have an associated scene (visible canvas)
        var arena = GetArena();
        if (arena != null)
        {
            // Get all stages from the arena - only those linked to a scene
            var stageCount = 0;
            var renderedCount = 0;
            foreach (var stage in arena.GetAllStages())
            {
                stageCount++;
                
                // Skip stages without a scene - they have no canvas to display
                if (stage.GetAssociatedScene() == null)
                {
                    if (shouldLog)
                        $"AnimatedKnModel: Stage '{stage.Name}' has no scene - skipping".WriteInfo();
                    continue;
                }
                
                var view = stage.Name;
                if (string.IsNullOrEmpty(view)) continue;
                
                if (shouldLog)
                    $"AnimatedKnModel: RenderGeometry3D to stage/view '{view}'".WriteSuccess();
                    
                var ctx = RenderContext3D.Create(arena, view, deep: true);
                RenderGeometry3D(ctx);
                renderedCount++;
            }
            
            if (shouldLog)
                $"AnimatedKnModel: Total stages={stageCount}, rendered to={renderedCount}".WriteInfo();
        }
        else
        {
            if (shouldLog)
                $"AnimatedKnModel '{Name}': GetArena() returned NULL".WriteError();
        }
    }

    /// <summary>
    /// Override to properly return KnComponent children.
    /// </summary>
    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = new List<ITreeNode>();
        EstablishFolderIfNotEmpty<KnParameter>(list);
        EstablishFolderIfNotEmpty<KnRelationship>(list);
        return list;
    }

}
