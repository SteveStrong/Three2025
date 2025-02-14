using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;

public class FoConnector : FoShape3D
{

    public FoConnector(string name) : base(name)
    {
    }

    public FoConnector(string name, string color) : base(name, color)
    {
    }

    public static void Connector(string name, double x, double depth, string color,  FoShape3D box)
    {

        var connect = new FoConnector(name, color)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, -depth / 2),
            }
        };
        connect.CreateBox(name, 0.2, 0.2, 0.2);
        var connName = new FoText3D("Name", "black")
        {
            Text = name,
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, -.2),
            }
        };
        connect.AddSubGlyph3D<FoText3D>(connName);
        box.AddSubGlyph3D<FoConnector>(connect);
        
    }

    public static void ForEquipment(int connectCount, double width, double depth, FoEquipment box)
    {
        Connector("L", -width/2, depth, "Green", box);
        var count = connectCount;
        for (int i = 0; i < count; i++)
        {
            var x = i * width / (1.0 * count) - width / 2.0 + width / (2.0 * count);
            Connector($"cn{i}", x, depth, "Red", box);
        }
        Connector("R", width/2, depth, "Green", box);
    }

    public static void ForTray(double width, double depth, FoTray box)
    {
        Connector("L", -width/2, 0, "Green", box);
        Connector("R",  width/2, 0, "Green", box);
    }
}
