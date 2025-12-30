using FoundryWorldsAndDrawings.Shape;

using FoundryRulesAndUnits.Extensions;

using FoundryWorldsAndDrawings.ThreeD.Maths;


namespace Three2025.Apprentice;

public class GeometryShape : FoShape3D
{
   public GeometryShape(string name, string shapeType = "box", double? width = null, double? height = null, double? depth = null) : base(name)
   {
      // Use provided dimensions or defaults
      var w = width ?? 2.0;
      var h = height ?? 2.0;
      var d = depth ?? 2.0;

      // Switch expression to select factory method
      Func<string, double, double, double, FoShape3D> factory = shapeType.ToLower() switch
      {
         "box" => CreateBox,
         "sphere" => CreateSphere,
         "cylinder" => CreateCylinder,
         "cone" => CreateCone,
         "torus" => CreateTorus,
         "tetrahedron" => CreateTetrahedron,
         "octahedron" => CreateOctahedron,
         "dodecahedron" => CreateDodecahedron,
         "icosahedron" => CreateIcosahedron,
         "torusknot" => CreateTorusKnot,
         "capsule" => CreateCapsule,
         "plane" => CreatePlane,
         "circle" => CreateCircle,
         "ring" => CreateRing,
         _ => CreateBox // default
      };

      factory(name, w, h, d);

      var tag = new FoText3D("tag")
      {
         Text = name,
         FontSize = 0.5,
         Transform = new Transform3("TagTransform")
         {
            Position = new Vector3(3, 0, 0),
         },
         Color = "black"
      };
      AddShape(tag);
      GetTreeNodeTitle().WriteSuccess();
   }

   public override string GetTreeNodeTitle()
   {
      var pos = Transform!.Position;
      return $"{GetName()} {Color}  @ {pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0}";
   }
}