using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// A KnModel subclass that reacts to animation loop events.
/// This demonstrates how KnModels can respond to PreAnimationEvent for geometry updates.
/// </summary>
public class AnimatedKnModel : KnModel
{
    private Action<string>? _logAction;

    public AnimatedKnModel() : base("AnimatedKnModel")
    {
        Calculations([
            "X: 10",
            "Y: 100",
            "Z: 10000",
            "GeomType: 'Box'",
            "Material: 'Blue'"
        ]);
        
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

    public AnimatedKnModel(string title, Action<string>? logAction = null) : base(title)
    {
        _logAction = logAction;
        
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

    public void SetLogAction(Action<string> logAction)
    {
        _logAction = logAction;
    }

    public override string GetTreeNodeTitle()
    {
        return Name ?? "AnimatedKnModel";
    }
}
