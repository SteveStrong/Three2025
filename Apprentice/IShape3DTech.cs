using FoundryWorldsAndDrawings.Shape;
using FoundryMentorModeler.Evaluator;


namespace Three2025.Apprentice;
#nullable enable


public interface IShape3DTech : ITechnician
{
   void SetStage(FoStage3D stage);

   /// <summary>
   /// Establish a geometry stage for managing 3D shapes
   /// </summary>
   /// <param name="stageName">Optional stage name to connect to. If null, uses "AgentCanvas3D" as default.</param>
   OPResult EstablishGeometryStage(string? stageName = null);

   void ClearShapes();

   void SaveShapes();

   void RestoreShapes();

   OPResult PickARandomColor();

   OPResult GetShapes();

   OPResult GetShapeByName(string name);

   OPResult AddShape(string name, string color, string shapeType = "box", double x = 0.0, double y = 0.0, double z = 0.0);

   OPResult AddShapeWithDimensions(string name, string color, string shapeType, double width, double height, double depth, double x = 0.0, double y = 0.0, double z = 0.0);
   OPResult DeleteShape(string name);

   OPResult DeleteMultipleShapes(List<string> names);

   OPResult RepositionShape(string name, double x, double y, double z);

   OPResult MoveShapeBy(string name, double deltaX, double deltaY, double deltaZ);

   /// <summary>
   /// Animate a shape moving by a relative offset (delta) from its current position
   /// </summary>
   Task<OPResult> AnimateMoveShapeBy(string name, double deltaX, double deltaY, double deltaZ, float durationSeconds = 1.0f, string easing = "QuadInOut");

   OPResult RotateShape(string name, double xDegrees, double yDegrees, double zDegrees);

   OPResult ScaleShape(string name, double scaleX, double scaleY, double scaleZ);

   OPResult ChangeShapeDimensions(string name, double width, double height, double depth);


   OPResult ChangeColor(string name, string color);
   
   OPResult ChangeGeometry(string name, string shapeType, double? width = null, double? height = null, double? depth = null);
   
   OPResult EstablishTextLabel(string parentShapeName, string labelName, string text, double? relativeX = null, double? relativeY = null, double? relativeZ = null, double? fontSize = null, string? color = null);

   OPResult CreateLinkShape(string name, string color, FoShape3D fromShape, FoShape3D toShape, double radius, string geomType = "Pipe");
   
   OPResult ChangeLinkGeometryType(string name, string geomType);
   
   void RefreshUI();

   OPResult CreatePipeLink(string name, string color, FoShape3D fromShape, FoShape3D toShape, double radius);

   OPResult CreatePathwayLink(string name, FoShape3D fromShape, FoShape3D toShape);
   
}
