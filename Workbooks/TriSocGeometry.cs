using BlazorThreeJS.Maths;
using FoundryBlazor;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Units;


namespace Three2025.Model;

public class TriSocGeometry : FoComponent
{


    public TriSocGeometry()
    {

    }



    public List<FoText3D> GenerateLabels()
    {
        var width = new Length(.1, "m");
        var height = new Length(.1, "m");
        var depth = new Length(.1, "m");

        var root = new FoGroup3D("Labels");
        //_world.AddGlyph3D<FoGroup3D>(root);

        var box = new SpacialBox3D(width.Value(), height.Value(), depth.Value(), "m");

        GenerateText(root, box.Center, "Center");

        GenerateText(root, box.LeftTopFront, "TopLeftFront");
        GenerateText(root, box.RightTopFront, "TopRightFront");
        GenerateText(root, box.LeftBottomFront, "BottomLeftFront");
        GenerateText(root, box.RightBottomFront, "BottomRightFront");

        GenerateText(root, box.LeftTopBack, "TopLeftBack");
        GenerateText(root, box.RightTopBack, "TopRightBack");
        GenerateText(root, box.LeftBottomBack, "BottomLeftBack");
        GenerateText(root, box.RightBottomBack, "BottomRightBack");


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


    public (FoGroup3D root, List<FoShape3D> list) GenerateMarkers()
    {
        var width = new Length(.1, "m");
        var height = new Length(.1, "m");
        var depth = new Length(.1, "m");

        var root = new FoGroup3D("Labels");
        //_world.AddGlyph3D<FoGroup3D>(root);

        var box = new SpacialBox3D(width.Value(), height.Value(), depth.Value(), "m");

        GenerateMarker(root, box.Center, "Center");

        GenerateMarker(root, box.LeftTopFront, "TopLeftFront");
        GenerateMarker(root, box.RightTopFront, "TopRightFront");
        GenerateMarker(root, box.LeftBottomFront, "BottomLeftFront");
        GenerateMarker(root, box.RightBottomFront, "BottomRightFront");

        GenerateMarker(root, box.LeftTopBack, "TopLeftBack");
        GenerateMarker(root, box.RightTopBack, "TopRightBack");
        GenerateMarker(root, box.LeftBottomBack, "BottomLeftBack");
        GenerateMarker(root, box.RightBottomBack, "BottomRightBack");


        return (root, root.GetSlot<FoShape3D>().Values());
    }


}
