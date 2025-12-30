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

   List<FoShape3D> GetShapes();

   FoShape3D? GetShapeByName(string name);

   List<FoShape3D> AddShape(string name, string color, string shapeType = "box", double x = 0.0, double y = 0.0, double z = 0.0);

   List<FoShape3D> AddShapeWithDimensions(string name, string color, string shapeType, double width, double height, double depth, double x = 0.0, double y = 0.0, double z = 0.0);

   List<FoShape3D> DeleteShape(string name);

   List<FoShape3D> DeleteMultipleShapes(List<string> names);

   List<FoShape3D> RepositionShape(string name, double x, double y, double z);

   List<FoShape3D> RotateShape(string name, double xDegrees, double yDegrees, double zDegrees);

   List<FoShape3D> ScaleShape(string name, double scaleX, double scaleY, double scaleZ);

   List<FoShape3D> ChangeShapeDimensions(string name, double width, double height, double depth);


   List<FoShape3D> ChangeColor(string name, string color);

   List<FoShape3D> DuplicateShape(string sourceName, string newName, double offsetX, double offsetY, double offsetZ);
}
