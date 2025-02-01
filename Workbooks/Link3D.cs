


using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;

namespace Three2025.Model;
public class Link3D : FoShape3D
{
    public Node3D Start { get; set; }
    public Node3D Finish { get; set; }
    public Link3D(string name, string color): base(name, color)
    {
    }

    public Link3D(string name, string color, Node3D start, Node3D finish, double inflate=0.01): base(name, color)
    {
        Start = start;
        Finish = finish;

        var bb = start.Boundary(finish);
        var pos = start.Center(finish);

        SetPosition(pos .X, pos.Y, pos.Z);
        bb.X = bb.X < inflate ? inflate : bb.X;
        bb.Y = bb.Y < inflate ? inflate : bb.Y;
        bb.Z = bb.Z < inflate ? inflate : bb.Z;

        CreateBox(name, bb.X, bb.Y, bb.Z);
    }

    public Link3D SetPosition(double x, double y, double z)
    {
        Transform.Position = new Vector3(x, y, z);
        return this;
    }
    public Link3D Reposition(double dx, double dy, double dz)
    {
        var pos = Transform.Position.CreatePlus(dx, dy, dz);
        Transform.Position = pos;
        return this;
    }

    public override string GetTreeNodeTitle()
    {
        return $"{base.GetTreeNodeTitle()}  from {Start.GetName()} to {Finish.GetName()}";
    }

}