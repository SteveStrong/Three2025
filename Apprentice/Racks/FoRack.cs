using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Models;

public class FoRack : FoShape3D
{
    
    private static MockDataGenerator gen { get; set; } = new();
    public FoRack(string name) : base(name)
    {
    }

    public FoRack(string name, string color) : base(name, color)
    {
    }

    public List<FoEquipment> GetEquipment()
    {
        var members = AllSubGlyph3Ds().Where(x => x is FoEquipment).Cast<FoEquipment>().ToList();
        return members;
    }
    
    public List<FoTray> GetTrays()
    {
        var members = AllSubGlyph3Ds().Where(x => x is FoTray).Cast<FoTray>().ToList();
        return members;
    }

    public static FoRack CreateRack(string name, double x, double z, double height = 10, double angle = 0)
    {
        var width = 3.0;
        var depth = 2.0;
        var drop = height/2;

        var list1 = new List<FoEquipment>()
        {
            FoEquipment.CreateEquipment("box1", 0-drop, 1.0, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("box2", 3-drop, 2.5, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("box3", 6-drop, 3.5, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("box4", 10-drop, 1.5, gen.GenerateInt(2, 5)),
        };

        var list2 = new List<FoEquipment>()
        {
            FoEquipment.CreateEquipment("box1", 1-drop, 1.5, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("box2", 4-drop, 2.5, gen.GenerateInt(2, 5)),
            FoEquipment.CreateEquipment("box3", 10-drop, 1.0, gen.GenerateInt(2, 5)),
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

        var tray = FoTray.CreateTray("Tray", height-2, .2);
        group.AddSubGlyph3D<FoTray>(tray);


        return group;
    }
}
