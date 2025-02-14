using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;

public class FoTray : FoShape3D
{

    public FoTray(string name) : base(name)
    {
    }

    public FoTray(string name, string color) : base(name, color)
    {
    }

    public static FoTray CreateTray(string name, double Y, double height)
    {

        var width = 3.0;
        var depth = 2.0;

        var box = new FoTray(name, "aqua")
        {
            Transform = new Transform3()
            {
                Position = new Vector3(0, Y + height / 2, 0),
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

        FoConnector.ForTray(width, depth, box);
        return box;
    }



    public List<FoConnector> GetConnectors()
    {
        var members = AllSubGlyph3Ds().Where(x => x is FoConnector).Cast<FoConnector>().ToList();
        return members;
    }

}
