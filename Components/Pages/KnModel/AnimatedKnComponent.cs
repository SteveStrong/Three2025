using FoundryWorldsAndDrawings.Solutions;
using FoundryMentorModeler.Model;

#nullable enable

namespace Three2025.Components.Pages;

/// <summary>
/// A KnComponent subclass that can be added to the model and react to events.
/// Demonstrates using the PreContextLink composition pattern.
/// </summary>
public class AnimatedKnComponent : KnComponent
{
    public int EventCount { get; set; } = 0;
    public double CurrentValue { get; set; } = 0;

    public AnimatedKnComponent() : base("AnimatedComponent")
    {
        SetupAnimationBehavior();
    }

    public AnimatedKnComponent(string name) : base(name)
    {
        SetupAnimationBehavior();
    }

    private void SetupAnimationBehavior()
    {
        // Use composition pattern - set up the pre-animation action
        PreAnimationRefresh((comp, evt) =>
        {
            EventCount++;
            // Simulate some computation based on animation tick
            CurrentValue = Math.Sin(evt.tick * 0.05) * 100;
        });
    }

    public override string GetTreeNodeTitle()
    {
        return $"{Name} (Events:{EventCount}, Value:{CurrentValue:F2})";
    }
}
