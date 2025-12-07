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

        // Update param with tick count - child components are notified automatically
        PreAnimationRefresh((comp, evt) =>
        {
            if (evt.tick % 60 == 0)
            {
                param.SetValue(evt.tick);
                $"AnimatedKnModel '{name}': PreAnimationRefresh tick={evt.tick}".WriteInfo();
            }
        });
        
        $"AnimatedKnModel: PreAnimationRefresh set up, PreContextLink is {(PreContextLink != null ? "SET" : "NULL")}".WriteInfo();
    }

    /// <summary>
    /// Override to properly return KnComponent children.
    /// </summary>
    public override IEnumerable<ITreeNode> GetTreeChildren()
    {
        var list = base.GetTreeChildren().ToList();
        return list;
    }

}
