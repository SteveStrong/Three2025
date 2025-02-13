

using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

public interface IRackTech : ITechnician
{
    FoEquipment CreateEquipment(string name, double Y, double height);
    FoRack CreateRack(string name, double x, double z, double height = 10, double angle = 0);

    bool ComputeHitBoundaries(Action OnComplete);
    (bool success, FoPipe3D pipe) TryCreatePipe(string from, string to);
    (bool success, T obj, Vector3 vector) TryFindHitPosition<T>(string path) where T: FoGlyph3D;
}

public class RackTech : IRackTech
{
    protected IWorkspace Workspace { get; init; }
    protected IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();

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

    public FoEquipment CreateEquipment(string name, double Y, double height)
    {
        //var color = DataGenerator.GenerateColor();

        var width = 3.0;
        var depth = 2.0;

        var box = new FoEquipment(name,"Orange")
        {
            Transform = new Transform3()
            {
                Position = new Vector3(0, Y + height/2, 0),
            }
        };
        box.CreateBox(name, width, height, depth);

        var boxName = new FoText3D("Name", "white")
        {
            Text = name,
            Transform = new Transform3()
            {
                Position = new Vector3(width, 0, 0),
            }
        };
        box.AddSubGlyph3D<FoText3D>(boxName);

        var count = DataGenerator.GenerateInt(2, 5);
        for( int i=0; i<count; i++)
        {
            var x = i * width/(1.0 *count) - width/2.0 + width/(2.0 * count);
            var cnn = $"cn{i}";
            var connect = new FoConnector(cnn,"Red")
            {
                Transform = new Transform3()
                {
                    Position = new Vector3(x, 0, -depth/2),
                }
            };
            connect.CreateBox(cnn, 0.2, 0.2, 0.2);
            var connName = new FoText3D("Name", "black")
            {
                Text = cnn,
                Transform = new Transform3()
                {
                    Position = new Vector3(0, 0, -.2),
                }
            };
            connect.AddSubGlyph3D<FoText3D>(connName);

            box.AddSubGlyph3D<FoConnector>(connect);
        }


        return box;
    }




    public FoRack CreateRack(string name, double x, double z, double height = 10, double angle = 0)
    {
        var width = 3.0;
        var depth = 2.0;
        var drop = height/2;

        var list1 = new List<FoEquipment>()
        {
            CreateEquipment("box1", 0-drop, 1.0),
            CreateEquipment("box2", 3-drop, 2.5),
            CreateEquipment("box3", 6-drop, 3.5),
            CreateEquipment("box4", 10-drop, 1.5),
        };

        var list2 = new List<FoEquipment>()
        {
            CreateEquipment("box1", 1-drop, 1.5),
            CreateEquipment("box2", 4-drop, 2.5),
            CreateEquipment("box3", 10-drop, 1.0),
        };

        var list = name.EndsWith("1") ? list1 : list2;

        var group = new FoRack(name)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(x, height/2, z),
                //Pivot = new Vector3(0, -height/2, 0),
                Rotation = new Euler(0, angle, 0),
            }
        };
        group.CreateBoundary(name, width, height, depth); //try to have three.js compute the bounding box

        var rackName = new FoText3D("Name", "White")
        {
            Text = name,
            Transform = new Transform3()
            {
                Position = new Vector3(0, -1 - height/2, 0),
            }
        };
        group.AddSubGlyph3D<FoText3D>(rackName);       

        foreach (var box in list)
            group.AddSubGlyph3D<FoEquipment>(box);

        var arena = Workspace.GetArena();
        arena.AddShapeToStage<FoRack>(group);  

        return group;
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

        var color = DataGenerator.GenerateColor();
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