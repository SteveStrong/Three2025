using FoundryWorldsAndDrawings.Shape;


namespace Three2025.Apprentice;
#nullable enable


public interface IShape3DTech : ITechnician
{
   void SetStage(FoStage3D stage);

   /// <summary>
   /// Establish a geometry stage for managing 3D shapes
   /// </summary>
   /// <param name="stageName">Optional stage name to connect to. If null, uses "AgentCanvas3D" as default.</param>
   FoStage3D EstablishGeometryStage(string? stageName = null);

   ToolCapabilities GetToolCapabilities();

   void ClearShapes();

   void SaveShapes();

   void RestoreShapes();

   string PickARandomColor();

   List<Shape3DInfo> GetShapes();

   Shape3DInfo? GetShapeByName(string name);

   List<Shape3DInfo> AddShape(string name, bool isOn, string color, string shapeType = "box", double x = 0.0, double y = 0.0, double z = 0.0);

   List<Shape3DInfo> AddShapeWithDimensions(string name, bool isOn, string color, string shapeType, double width, double height, double depth, double x = 0.0, double y = 0.0, double z = 0.0);

   List<Shape3DInfo> DeleteShape(string name);

   List<Shape3DInfo> DeleteMultipleShapes(List<string> names);

   List<Shape3DInfo> RepositionShape(string name, double x, double y, double z);

   List<Shape3DInfo> RotateShape(string name, double xDegrees, double yDegrees, double zDegrees);

   List<Shape3DInfo> ScaleShape(string name, double scaleX, double scaleY, double scaleZ);

   List<Shape3DInfo> ChangeShapeDimensions(string name, double width, double height, double depth);

   List<Shape3DInfo> ChangeState(string name, bool isOn);

   List<Shape3DInfo> ChangeColor(string name, string color);

   List<Shape3DInfo> DuplicateShape(string sourceName, string newName, double offsetX, double offsetY, double offsetZ);
}
