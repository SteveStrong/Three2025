using BlazorThreeJS.Maths;
using FoundryBlazor;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Units;


namespace Three2025.Model;

public class TriSocGeometry : FoComponent
{

    protected CableWorld _world;

    public TriSocGeometry(CableWorld world)
    {
        _world = world;

    }



    public List<FoText3D> GenerateLabels()
    {
        var width = new Length(.1, "m");
        var height = new Length(.1, "m");
        var depth = new Length(.1, "m");

        var root = new FoGroup3D("Labels");
        _world.AddGlyph3D<FoGroup3D>(root);

        var box = new SpacialBox3D(width.Value(), height.Value(), depth.Value(), "m");

        GenerateText(root, box.Center, "Center");

        GenerateText(root, box.TopLeftFront, "TopLeftFront");
        GenerateText(root, box.TopRightFront, "TopRightFront");
        GenerateText(root, box.BottomLeftFront, "BottomLeftFront");
        GenerateText(root, box.BottomRightFront, "BottomRightFront");

        GenerateText(root, box.TopLeftBack, "TopLeftBack");
        GenerateText(root, box.TopRightBack, "TopRightBack");
        GenerateText(root, box.BottomLeftBack, "BottomLeftBack");
        GenerateText(root, box.BottomRightBack, "BottomRightBack");


        return root.GetSlot<FoText3D>().Values();
    }

    public FoText3D GenerateText(FoGroup3D group, Point3D point, string text)
    {

        var shape = new FoText3D(text, "green")
        {
            Transform = new Transform3()
            {
                Position = point.AsVector3(),
            },
            Text = text
        };
        group.Add<FoText3D>(shape);

        return shape;
    }

    public FoShape3D GenerateMarker(FoGroup3D group, Point3D point, string text)
    {

        var shape = new FoShape3D(text, "green")
        {
            Transform = new Transform3()
            {
                Position = point.AsVector3(),
            }
        };
        shape.CreateSphere(text, .1, .1, .1);
        group.Add<FoShape3D>(shape);

        return shape;
    }


    public List<FoShape3D> GenerateMarkers()
    {
        var width = new Length(.1, "m");
        var height = new Length(.1, "m");
        var depth = new Length(.1, "m");

        var root = new FoGroup3D("Labels");
        _world.AddGlyph3D<FoGroup3D>(root);

        var box = new SpacialBox3D(width.Value(), height.Value(), depth.Value(), "m");

        GenerateMarker(root, box.Center, "Center");

        GenerateMarker(root, box.TopLeftFront, "TopLeftFront");
        GenerateMarker(root, box.TopRightFront, "TopRightFront");
        GenerateMarker(root, box.BottomLeftFront, "BottomLeftFront");
        GenerateMarker(root, box.BottomRightFront, "BottomRightFront");

        GenerateMarker(root, box.TopLeftBack, "TopLeftBack");
        GenerateMarker(root, box.TopRightBack, "TopRightBack");
        GenerateMarker(root, box.BottomLeftBack, "BottomLeftBack");
        GenerateMarker(root, box.BottomRightBack, "BottomRightBack");


        return root.GetSlot<FoShape3D>().Values();
    }

    public List<Node3D> GenerateColumn(string groupName, double x, double z, Length Height, Length Step)
    {

        var h = Height.Value();
        var s = Step.Value();

        var columns = new List<Node3D>();
        var y = 0.0;

        var root = new FoGroup3D(groupName);
        _world.AddGlyph3D<FoGroup3D>(root);

        while (y <= h)
        {
            var name = $"{groupName}:{x:F1}:{y:F1}:{z:F1}";
            var shape = new Node3D(name, "blue")
            {
                Transform = new Transform3()
                {
                    Position = new Vector3(x, y, z),
                }
            };
            shape.CreateBox(name, .05, .03, .05);
            columns.Add(shape);
            root.AddShape<FoShape3D>(shape);
            y += s;
        }

        for (int i = 1; i < columns.Count; i++)
        {
            var start = columns[i - 1];
            var finish = columns[i];
            var link = new Link3D($"Link::{x:F1}:{z:F1}{i}", "blue", start, finish, 0.01);
            root.AddShape<FoShape3D>(link);
        }
        return columns;
    }

    //create a method to link together 2 columns of List<Node3D>
    public void LinkColumns(string name, List<Node3D> start, List<Node3D> finish)
    {
        var root = new FoGroup3D(name);
        _world.AddGlyph3D<FoGroup3D>(root);

        for (int i = 0; i < start.Count(); i++)
        {
            var s = start[i];
            var f = finish[i];
            var link = new Link3D($"{name}:{i}", "blue", s, f, 0.01);
            root.AddShape<FoShape3D>(link);
        }
    }



}
