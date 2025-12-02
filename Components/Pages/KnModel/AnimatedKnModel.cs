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
public class AnimatedKnModel : KnModel
{
    private Action? _onRefresh;

    public AnimatedKnModel(string name) : base(name)
    {
        // Use composition pattern - set up the pre-animation action
        PreAnimationRefresh((comp, evt) =>
        {
            // Refresh every 60 frames to avoid spam
            if (evt.tick % 60 == 0)
            {
                _onRefresh?.Invoke();
            }
        });
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

        // Use composition pattern - set up the pre-animation action
        PreAnimationRefresh((comp, evt) =>
        {
            // Update param and refresh UI every 60 frames
            if (evt.tick % 60 == 0)
            {
                param.SetValue(evt.tick);
                _onRefresh?.Invoke();
            }
        });
        
        $"AnimatedKnModel: PreAnimationRefresh set up, PreContextLink is {(PreContextLink != null ? "SET" : "NULL")}".WriteInfo();
    }

    public void SetRefreshAction(Action onRefresh)
    {
        _onRefresh = onRefresh;
    }

    /// <summary>
    /// Override to properly return KnComponent children.
    /// The base class uses EstablishFolderForAllOfType which doesn't add to the list.
    /// Note: Must include AnimatedKnComponent specifically since it has its own slot.
    /// </summary>
    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = base.GetTreeChildren();
        //var list = new List<ITreeNode>();
        
        // Add folders for parameters (like base class)
        //EstablishFolderIfNotEmpty<KnParameter>(list);
        //EstablishFolderForAllOfType<KnComponent>(list);        

        
        // // Also add AnimatedKnComponent members (stored in separate slot due to generic Add<T>)
        // foreach (var component in Members<AnimatedKnComponent>())
        // {
        //     list.Add(component);
        // }
        
        return list;
    }

}
