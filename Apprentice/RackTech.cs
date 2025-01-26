

using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

public class FoRack : FoShape3D
{
    public FoRack(string name) : base(name)
    {
    }

    public FoRack(string name, string color) : base(name, color)
    {
    }
}

public interface IRackTech : ITechnician
{
    FoShape3D CreateEquipment(string name, double Y, double height);
    FoShape3D CreateRack(string name, double x, double z, double height = 10, double angle = 0);

    (bool success, FoPipe3D pipe) TryCreatePipe(string from, string to);
    (bool success, FoShape3D obj, Vector3 vector) TryFindHitPosition(string path);
}

public class RackTech : IRackTech
{
    public IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();

    public RackTech(IFoundryService foundry)
    {
        FoundryService = foundry;
    }

    public FoShape3D CreateEquipment(string name, double Y, double height)
    {
        var color = DataGenerator.GenerateColor();

        var width = 3.0;
        var depth = 2.0;

        var box = new FoShape3D(name,color)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(0, Y + height/2, 0),
            }
        };
        box.CreateBox(name, width, height, depth);

        var count = DataGenerator.GenerateInt(3, 6);
        for( int i=0; i<count; i++)
        {
            var x = i * width/(1.0 *count) - width/2.0 + width/(2.0 * count);
            var cnn = $"cn{i}";
            var connect = new FoShape3D(cnn,"Red")
            {
                Transform = new Transform3()
                {
                    Position = new Vector3(x, 0, depth/2),
                }
            };
            connect.CreateBox(cnn, 0.2, 0.2, 0.2);
            box.Add<FoShape3D>(connect);
        }


        return box;
    }

    public FoShape3D CreateRack(string name, double x, double z, double height = 10, double angle = 0)
    {
        var width = 3.0;
        var depth = 2.0;

        var list1 = new List<FoShape3D>()
        {
            CreateEquipment("box1", 0, 1.5),
            CreateEquipment("box2", 3, 2.5),
            CreateEquipment("box3", 6, 3.5),
            CreateEquipment("box4", 10, 1.5),
        };

        var group = new FoRack(name)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(x, height/2, z),
                Rotation = new Euler(0, angle, 0),
            }
        };
        group.CreateBoundary(name, width, height, depth); //try to have three.js compute the bounding box

        foreach (var box in list1)
            group.Add<FoShape3D>(box);

        return group;
    }

    public (bool success, FoShape3D obj, Vector3 vector) TryFindHitPosition(string path)
    {
        var arena = FoundryService.Arena();
        var stage = arena.CurrentStage();

        var (s1, p1, cn1) = stage.FindUsingPath<FoRack, FoShape3D>(path);
        if (!s1) return (false, cn1, null);

        var (f1, v1) = cn1.HitPosition();
        if (!f1) return (false, cn1, null);

        return (true, cn1, v1);  
    }

    public (bool success, FoPipe3D pipe) TryCreatePipe(string from, string to)
    {
        var arena = FoundryService.Arena();
        var stage = arena.CurrentStage();

        var (s1, p1, v1) = TryFindHitPosition(from);
        var (s2, p2, v2) = TryFindHitPosition(to);

        if (!s1 || !s2) return (false, null);


        $"Connecting {p1} @ {v1.X:F1},{v1.Y:F1},{v1.Z:F1} to {p2} @ {v2.X:F1},{v2.Y:F1},{v2.Z:F1}".WriteSuccess();

        var color = DataGenerator.GenerateColor();
        var result = new FoPipe3D("pipe", color)
        {
            Key = $"{from}->{to}",
            FromShape3D = p1,
            ToShape3D = p2,

            Radius = 0.15f,
        };
        return (true, result);
    }

}