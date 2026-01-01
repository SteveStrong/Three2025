#nullable enable

using FoundryWorldsAndDrawings.Shape;
using FoundryMentorModeler.Evaluator;

namespace Three2025.Apprentice;

/// <summary>
/// Shape editor interface for 2D canvas operations following OPResult pattern.
/// All operations return OPResult for consistent error handling.
/// </summary>
public interface IShape2DEditor
{
   /// <summary>
   /// Connect this editor to a specific page
   /// </summary>
   void SetPage(FoPage2D page);
   
   // ============================================
   // PROPERTY OPERATIONS
   // ============================================
   
   OPResult SetColor(string shapeName, string color);
   OPResult SetPosition(string shapeName, double x, double y);
   OPResult MoveBy(string shapeName, double deltaX, double deltaY);
   OPResult SetDimensions(string shapeName, double width, double height);
   
   // ============================================
   // SHAPE CREATION
   // ============================================
   
   OPResult AddRectangle(string name, double width, double height, string color, double x, double y);
   OPResult AddCircle(string name, double radius, string color, double x, double y);
   OPResult AddText(string name, string text, double x, double y, string color);
   
   // ============================================
   // CONNECTION OPERATIONS
   // ============================================
   
   OPResult ConnectShapes(string connectorName, string startShapeName, string endShapeName, string color = "Black", double thickness = 2);
   
   // ============================================
   // HIERARCHY OPERATIONS
   // ============================================
   
   OPResult GetChildShapes(string parentShapeName);
   OPResult RemoveChildShape(string parentShapeName, string childShapeName);
   
   // ============================================
   // QUERY OPERATIONS
   // ============================================
   
   OPResult GetShapeByName(string name);
   OPResult GetAllShapes();
   
   // ============================================
   // DELETION OPERATIONS
   // ============================================
   
   OPResult DeleteShape(string name);
   OPResult DeleteMultipleShapes(List<string> names);
   OPResult ClearShapes();
   
   // ============================================
   // SHAPE MANAGEMENT
   // ============================================
   
   OPResult AddShape(FoGlyph2D shape);
}
