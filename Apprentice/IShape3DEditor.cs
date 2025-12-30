#nullable enable

using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.Shape;
using FoundryMentorModeler.Evaluator;

namespace Three2025.Apprentice;

/// <summary>
/// Event arguments for shape property changes
/// </summary>
public class ShapePropertyChangedEventArgs : EventArgs
{
   public required string ShapeName { get; init; }
   public required string PropertyName { get; init; }
   public object? OldValue { get; init; }
   public object? NewValue { get; init; }
   public DateTime Timestamp { get; init; } = DateTime.Now;
}

/// <summary>
/// Editor interface for shape manipulation with event-driven updates
/// </summary>
public interface IShape3DEditor
{
   /// <summary>
   /// Set the stage that this editor operates on
   /// </summary>
   void SetStage(FoStage3D stage);
   
    
   /// <summary>
   /// Change the color of a shape
   /// </summary>
   OPResult SetColor(string shapeName, string color);
   
   /// <summary>
   /// Change the position of a shape
   /// </summary>
   OPResult SetPosition(string shapeName, Vector3 position);
   
   /// <summary>
   /// Change the rotation of a shape
   /// </summary>
   OPResult SetRotation(string shapeName, Euler rotation);
   
   /// <summary>
   /// Change the scale of a shape
   /// </summary>
   OPResult SetScale(string shapeName, Vector3 scale);
   
   /// <summary>
   /// Change the geometry type of a shape
   /// </summary>
   OPResult SetGeometry(string shapeName, string shapeType, double? width = null, double? height = null, double? depth = null);
   
   /// <summary>
   /// Change the dimensions of a shape
   /// </summary>
   OPResult SetDimensions(string shapeName, double width, double height, double depth);
   
   /// <summary>
   /// Establish a text label as a child of a shape (creates if missing, updates if exists)
   /// </summary>
   OPResult EstablishTextLabel(string parentShapeName, string labelName, string text, Vector3? relativePosition = null, double? fontSize = null, string? color = null);
   
   /// <summary>
   /// Remove a child shape from its parent
   /// </summary>
   OPResult RemoveChildShape(string parentShapeName, string childShapeName);
   
   /// <summary>
   /// Get the names of all child shapes for a parent shape
   /// </summary>
   OPResult GetChildShapes(string parentShapeName);
   
   /// <summary>
   /// Delete a shape from the stage
   /// </summary>
   OPResult DeleteShape(string name);
   
   /// <summary>
   /// Delete multiple shapes from the stage
   /// </summary>
   OPResult DeleteMultipleShapes(List<string> names);
   
   /// <summary>
   /// Clear all shapes from the stage
   /// </summary>
   OPResult ClearShapes();
   
   /// <summary>
   /// Get a shape by name
   /// </summary>
   OPResult GetShapeByName(string name);
   
   /// <summary>
   /// Get all shapes from the stage
   /// </summary>
   OPResult GetAllShapes();
   
   /// <summary>
   /// Add a new shape to the stage
   /// </summary>
   OPResult AddShape(FoShape3D shape);
}
