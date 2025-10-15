
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;

public class FoEquipment : FoShape3D
{

    public FoEquipment(string name) : base(name)
    {
    }

    public FoEquipment(string name, string color) : base(name, color)
    {
    }

    public static FoEquipment CreateEquipment(string name, double Y, double height, int connectCount)
    {
        //var color = DataGenerator.GenerateColor();

        var width = 3.0;
        var depth = 2.0;

        var box = new FoEquipment(name, "Orange")
        {
            Transform = new Transform3("EquipmentTransform")
            {
                Position = new Vector3(0, Y + height / 2, 0),
            }
        };
        box.CreateBox(name, width, height, depth);

        var boxName = new FoText3D("Name", "white")
        {
            Text = name,
            Transform = new Transform3("NameTransform")
            {
                Position = new Vector3(width, 0, 0),
            }
        };
        box.AddSubGlyph3D<FoText3D>(boxName);

        box.AddSubGlyph3D<FoText3D>(new FoText3D("Front", "black")
        {
            Text = "Front",
            Transform = new Transform3("FrontTransform")
            {
                Position = new Vector3(-width / 3, 0, depth),
            }
        });

        FoConnector.ForEquipment(connectCount, width, depth, box);


        return box;
    }



    public List<FoConnector> GetConnectors()
    {
        var members = AllSubGlyph3Ds().Where(x => x is FoConnector).Cast<FoConnector>().ToList();
        return members;
    }

}
