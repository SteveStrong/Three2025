
using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Extensions;

namespace Three2025.Apprentice;
public class Node3D : FoShape3D
{
    public Node3D(string name, string color): base(name, color)
    {
    }

    public Link3D CreateLink(string name, string color, Node3D start, Node3D finish)
    {
        var link = new Link3D(name, color, start, finish);
        return link;
    }

    public Vector3 Boundary(Node3D target)
    {
        var a = Transform.Position;
        var b = target.Transform.Position;
        return a.BoundingBox(b);
    }

    public Vector3 Center(Node3D target)
    {
        var a = Transform.Position;
        var b = target.Transform.Position;
        return a.Center(b);
    }

    public double Distance(Node3D target)
    {
        var a = Transform.Position;
        var b = target.Transform.Position;
        return a.Distance(b);
    }



    public override string GetTreeNodeTitle()
    {
        var pos = Transform.Position;
        return $"{base.GetTreeNodeTitle()} POS: {pos.X:F1} {pos.Y:F1} {pos.Z:F1}";
    }

}