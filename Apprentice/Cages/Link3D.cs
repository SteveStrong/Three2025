using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings;

namespace Three2025.Apprentice;
public class Link3D : FoPipe3D
{   /// <summary>
   /// Default formatter for Link3D - shows connection between nodes
   /// </summary>
   public static new readonly Func<FoBase, string> DefaultFormatter = link => 
   {
      if (link is Link3D l3d)
      {
         return $"{link.Key} from {l3d.Start?.GetName()} to {l3d.Finish?.GetName()}";
      }
      return $"{link.Key} Link";
   };
    public Node3D Start { get; set; }
    public Node3D Finish { get; set; }


    public Link3D(string name, string color, Node3D start, Node3D finish): base(name, color)
    {
        Start = start;
        Finish = finish;
        
        // Set the formatter to use our static formatter
        ComputeTreeNodeTitle = DefaultFormatter;

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



}