


using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Extensions;

namespace Three2025.Model;
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

    public Vector3 Boundry(Node3D target)
    {
        var a = GetPosition()!;
        var b = target.GetPosition()!;
        return a.BoundingBox(b);
    }

    public Vector3 Center(Node3D target)
    {
        var a = GetPosition()!;
        var b = target.GetPosition()!;
        return a.Center(b);
    }

    public double Distance(Node3D target)
    {
        var a = GetPosition()!;
        var b = target.GetPosition()!;
        return a.Distance(b);
    }

    public Node3D SetPosition(double x, double y, double z)
    {
        Position = new Vector3(x, y, z);
        return this;
    }
    public Node3D Reposition(double dx, double dy, double dz)
    {
        var pos = GetPosition()!;
        var x = pos.X + dx;
        var y = pos.Y + dy;
        var z = pos.Z + dz;
        Position = new Vector3(x, y, z);
        return this;
    }

    public override string GetTreeNodeTitle()
    {
        return $"{base.GetTreeNodeTitle()} {GeomType} @ {Position?.X:F1} {Position?.Y:F1} {Position?.Z:F1}";
    }

}