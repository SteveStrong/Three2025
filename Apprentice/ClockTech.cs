

using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

// public class FoRack : FoShape3D
// {
//     public FoRack(string name) : base(name)
//     {
//     }

//     public FoRack(string name, string color) : base(name, color)
//     {
//     }
// }

public interface IClockTech : ITechnician
{

}

public class ClockTech : IClockTech
{
    public IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();

    public ClockTech(IFoundryService foundry)
    {
        FoundryService = foundry;
    }

    public bool ComputeHitBoundaries(Action OnComplete)
    {
        var arena = FoundryService.Arena();
        var (success, scene) = arena.CurrentScene();

        if (!success) return false;
        scene.UpdateHitBoundaries(OnComplete);
        return true;
    } 



}