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
    private Action<string>? _logAction;

    public AnimatedKnModel(string name) : base(name)
    {

        // Use composition pattern - set up the pre-animation action
        PreAnimationRefresh((comp, evt) =>
        {
            // Log every 60 frames to avoid spam
            if (evt.tick % 60 == 0)
            {
                _logAction?.Invoke($"Model '{Name}' PreAnim tick={evt.tick}, fps={evt.fps:F1}, children={Members<KnComponent>().Count()}");
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
            // Log every 60 frames to avoid spam
            if (evt.tick % 60 == 0)
            {
                param.SetValue(evt.tick);
                $"AnimatedKnModel.PreAnimationRefresh: tick={evt.tick}".WriteInfo();
                _logAction?.Invoke($"Model '{Name}' PreAnim tick={evt.tick}, fps={evt.fps:F1}, children={Members<KnComponent>().Count()}");
            }
        });
        
        $"AnimatedKnModel: PreAnimationRefresh set up, PreContextLink is {(PreContextLink != null ? "SET" : "NULL")}".WriteInfo();
    }



    public void SetLogAction(Action<string> logAction)
    {
        _logAction = logAction;
    }

    /// <summary>
    /// Override to properly return KnComponent children.
    /// The base class uses EstablishFolderForAllOfType which doesn't add to the list.
    /// </summary>
    // public override IEnumerable<ITreeNode> GetTreeChildren()
    // {
    //     var list = base.GetTreeChildren();
        
    //     // Add folders for parameters (like base class)
    //     EstablishFolderIfNotEmpty<KnParameter>(list);
        
    //     // Try to get components directly from slot
    //     var componentSlot = GetSlot<KnComponent>();
    //     var slotCount = componentSlot?.Count() ?? 0;
    //     $"AnimatedKnModel.GetTreeChildren: KnComponent slot has {slotCount} items".WriteInfo();
        
    //     // Directly add KnComponent members as tree children
    //     var components = Members<KnComponent>().ToList();
    //     foreach (var component in components)
    //     {
    //         $"  - Found component: {component.Name}".WriteInfo();
    //         list.Add(component);
    //     }
        
    //     $"AnimatedKnModel.GetTreeChildren: returning {list.Count} items ({components.Count} components)".WriteInfo();
    //     return list;
    // }

}
