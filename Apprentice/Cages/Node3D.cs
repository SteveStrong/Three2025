
using BlazorThreeJS.Maths;
using FoundryBlazor.Shape;
using FoundryBlazor.Extensions;
using BlazorThreeJS.Core;

#nullable enable

namespace Three2025.Apprentice;
public class Node3D : FoShape3D
{

    public List<Link3D> Links { get; set; } = new();

    public Node3D(string name, string color): base(name, color)
    {
    }

    public string GetTitle()
    {
        var parent = GetParent() as FoGlyph3D;
        if (parent != null)
            return $"{parent.GetName()}.{GetName()}";

        return GetName();
    }

    public override (bool success, Vector3 path) HitPosition()
    {
        return (true, GetTransform().Position);
    }

    public int LinkCount()
    {
        return Links.Count;
    }

    public bool HasLinks()
    {
        return Links.Count > 0;
    }

    public void AddLink(Link3D link)
    {
        if ( !Links.Contains(link) )
            Links.Add(link);
    }

    public void RemoveLink(Link3D link)
    {
        if ( Links.Contains(link) )
            Links.Remove(link);
    }

    public List<Node3D> Neighbors()
    {
        var neighbors = new List<Node3D>();
        foreach (var link in Links)
        {
            if (link.Start == this)
                neighbors.Add(link.Finish);
            else
                neighbors.Add(link.Start);
        }
        return neighbors;
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
        return $"{base.GetTreeNodeTitle()} Links: {LinkCount()} POS: {pos.X:F1} {pos.Y:F1} {pos.Z:F1}";
    }

}