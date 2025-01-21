using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using FoundryBlazor;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;

using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryRulesAndUnits.Units;


namespace Three2025.Model;

public class CableChannels : FoComponent
{

    protected CableWorld _world;

    public CableChannels(CableWorld world)
    {
        _world = world;
        GenerateGeometry();
    }


    public void GenerateGeometry()
    {
        var width = new Length(1, "m");
        var height = new Length(2.6, "m");
        var depth = new Length(.8, "m");
        var step = new Length(.2, "m");

        var (x, z) = (5.0, 5.0);
        GenerateCage(x, z, width, height, depth, step);
        // x += width.Value() + 0.05;
        // GenerateCage(x, z, width, height, depth, step);
        // x += width.Value() + 0.05;
        // GenerateCage(x, z, width, height, depth, step);
        // x += width.Value() + 0.05;
        // GenerateCage(x, z, width, height, depth, step);
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


    public void GenerateCage(double x, double z, Length Width, Length Height, Length Depth, Length Step)
    {

        var w = Width.Value();
        var d = Depth.Value();

        var lf = GenerateColumn("Left-Front", x, z, Height, Step);
        var lb = GenerateColumn("Left-Back", x, z+d, Height, Step);
        LinkColumns($"Left-Side", lf, lb);

        var cb = GenerateColumn("Center-Back", x +w/2, z+d, Height, Step);
        LinkColumns($"Left-Back-Side", lb, cb);

        var rb = GenerateColumn("Right-Back", x +w, z+d, Height, Step);
        LinkColumns($"Right-Back-Side", cb, rb);

        var rf = GenerateColumn("Right-Front", x +w, z, Height, Step);
        LinkColumns($"Right-Side", rb, rf);
    }

}
