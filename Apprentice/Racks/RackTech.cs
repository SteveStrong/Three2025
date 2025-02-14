

using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

public interface IRackTech : ITechnician
{
    void DoAddEquipmentArena();
    FoRack CreateRack(string name, double x, double z, double height = 10, double angle = 0);

    bool ComputeHitBoundaries(Action OnComplete);
    (bool success, FoPipe3D pipe) TryCreatePipe(string from, string to);
    (bool success, T obj, Vector3 vector) TryFindHitPosition<T>(string path) where T: FoGlyph3D;
}

public class RackTech : IRackTech
{
    protected IWorkspace Workspace { get; init; }
    protected IFoundryService FoundryService { get; init; }


    public RackTech(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
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

 
    public void DoAddEquipmentArena()
    {
        MockDataGenerator gen = new();

        var list = new List<FoEquipment>()
        {
            FoEquipment.CreateEquipment("x1", 0, 1.5, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("x2", 3, 2.5, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("x3", 6, 3.5, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("x4", 10, 1.5, gen.GenerateInt(2, 5)),
        };

        var arena = Workspace.GetArena();
        foreach (var box in list)
        {
            arena.AddShapeToStage(box);
        }

    }


    public FoRack CreateRack(string name, double x, double z, double height = 10, double angle = 0)
    {

        var rack = FoRack.CreateRack(name, x, z, height, angle);
                
        var arena = Workspace.GetArena();
        arena.AddShapeToStage<FoRack>(rack);  
  
        return rack;
    }

    public (bool success, T obj, Vector3 vector) TryFindHitPosition<T>(string path) where T: FoGlyph3D
    {
        var arena = FoundryService.Arena();
        var stage = arena.CurrentStage();

        var (s1, p1, cn1) = stage.FindUsingPath<FoRack, T>(path);
        if (!s1) return (false, cn1, null);

        var (f1, v1) = cn1.HitPosition();
        if (!f1) return (false, cn1, null);

        return (true, cn1, v1);  
    }

    public (bool success, FoPipe3D pipe) TryCreatePipe(string from, string to)
    {
        var arena = FoundryService.Arena();
        var stage = arena.CurrentStage();

        var (s1, p1, v1) = TryFindHitPosition<FoGlyph3D>(from);
        var (s2, p2, v2) = TryFindHitPosition<FoGlyph3D>(to);

        if (!s1 || !s2) return (false, null);


        $"Connecting {p1} @ {v1.X:F1},{v1.Y:F1},{v1.Z:F1} to {p2} @ {v2.X:F1},{v2.Y:F1},{v2.Z:F1}".WriteSuccess();

        var color = "Red";
        var result = new FoPipe3D("pipe", color)
        {
            Key = $"PIPE: {from}->{to}",
            FromShape3D = p1 as FoShape3D,
            ToShape3D = p2 as FoShape3D,

            Radius = 0.15f,
        };
        return (true, result);
    }

}