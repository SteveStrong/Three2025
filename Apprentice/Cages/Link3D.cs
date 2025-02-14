


using BlazorThreeJS.Maths;
using FoundryBlazor.Extensions;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Apprentice;
public class Link3D : FoPipe3D
{
    public Node3D Start { get; set; }
    public Node3D Finish { get; set; }


    public Link3D(string name, string color, Node3D start, Node3D finish): base(name, color)
    {
        Start = start;
        Finish = finish;

        FromShape3D = Start;
        ToShape3D = Finish;

        CreatePipe(name, 0.1);
    }

    public double Distance()
    {
        var start = Start.GetPosition();
        var finish = Finish.GetPosition();
        return start.Distance(finish);
    }

    //public Link3D SetPosition(double x, double y, double z)
    //{
    //    Transform.Position = new Vector3(x, y, z);
    //    return this;
    //}
    //public Link3D Reposition(double dx, double dy, double dz)
    //{
    //    var pos = Transform.Position.CreatePlus(dx, dy, dz);
    //    Transform.Position = pos;
    //    return this;
    //}

    public override string GetTreeNodeTitle()
    {
        return $"{base.GetTreeNodeTitle()}  from {Start.GetName()} to {Finish.GetName()}";
    }

}