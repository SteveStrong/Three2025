using FoundryWorldsAndDrawings.Solutions;
using FoundryMicroCore.Core.Extensions;
using FoundryMicroCore.Core;
using FoundryMentorModeler.Model;
using FoundryMicroCore.Core;
using FoundryRulesAndUnits.Extensions;
using FoundryMicroCore.Core;
using FoundryRulesAndUnits.Models;
using FoundryMicroCore.Core;

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
            "UserName: 'Steve'",
            "Model: 'Blue'",
            "Param1: 42"
        ]);


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
               // $"AnimatedKnModel '{Name}': PreAnimationRefresh tick={evt.tick}".WriteInfo();
            }
        });
    }

    /// <summary>
    /// Returns KnComponent children for tree view navigation.
    /// </summary>
    public virtual IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = new List<ITreeNode>();
        EstablishFolderIfNotEmpty<KnParameter>(list);
        EstablishFolderIfNotEmpty<KnRelationship>(list);
        return list;
    }

}
