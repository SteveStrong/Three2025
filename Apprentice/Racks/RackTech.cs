using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;
using FoundryWorldsAndDrawings.ThreeD.Maths;

#nullable enable

public interface IRackTech : ITechnician
{
    void SetStage(FoStage3D stage);
    void DoAddEquipmentArena();
    FoRack CreateRack(string name, double x, double z, double height = 10, double angle = 0);

    (bool success, FoPipe3D? pipe) TryCreatePipe(string from, string to);
    (bool success, T? obj, Vector3? vector) TryFindHitPosition<T>(string path) where T: FoGlyph3D;
}

public class RackTech : IRackTech
{
    protected IWorkspace Workspace { get; init; }
    protected IFoundryService FoundryService { get; init; }
    protected FoStage3D? Stage { get; set; }


    public RackTech(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
        FoundryService = foundry;
    }

    /// <summary>
    /// Set the stage to use. Call this from a page to inject its stage.
    /// </summary>
    public void SetStage(FoStage3D stage)
    {
        Stage = stage;
    }

    private FoStage3D GetStage()
    {
        if (Stage != null) return Stage;
        var arena = Workspace.GetArena();
        return arena.EstablishStage<FoStage3D>("Rack");
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

        var stage = GetStage();
        foreach (var box in list)
        {
            stage.AddShape(box);
        }

    }


    public FoRack CreateRack(string name, double x, double z, double height = 10, double angle = 0)
    {

        var rack = FoRack.CreateRack(name, x, z, height, angle);
                
        var stage = GetStage();
        stage.AddShape(rack);  
  
        return rack;
    }

    public (bool success, T? obj, Vector3? vector) TryFindHitPosition<T>(string path) where T: FoGlyph3D
    {
        var stage = GetStage();

        var (s1, p1, cn1) = stage.FindUsingPath<FoRack, T>(path);
        if (!s1) return (false, cn1, null);

        var (f1, v1) = cn1!.HitPosition();
        if (!f1) return (false, cn1, null);

        return (true, cn1, v1);  
    }

    public (bool success, FoPipe3D? pipe) TryCreatePipe(string from, string to)
    {
        var stage = GetStage();

        var (s1, p1, v1) = TryFindHitPosition<FoGlyph3D>(from);
        var (s2, p2, v2) = TryFindHitPosition<FoGlyph3D>(to);

        if (!s1 || !s2) return (false, null);


        $"Connecting {p1} @ {v1!.X:F1},{v1.Y:F1},{v1.Z:F1} to {p2} @ {v2!.X:F1},{v2.Y:F1},{v2.Z:F1}".WriteSuccess();

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