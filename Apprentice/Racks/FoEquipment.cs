using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;

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

        box.AddSubGlyph3D<FoText3D>(new FoText3D("Front", "black")
        {
            Text = "Front",
            Transform = new Transform3()
            {
                Position = new Vector3(-width/3, 0, depth),
            }
        });

        var count = connectCount;
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

    public List<FoConnector> GetConnectors()
    {
        return Members<FoConnector>().ToList();
    }

}
