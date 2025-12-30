#nullable enable

using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.Shape;

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
   bool SetColor(string shapeName, string color);
   
   /// <summary>
   /// Change the position of a shape
   /// </summary>
   bool SetPosition(string shapeName, Vector3 position);
   
   /// <summary>
   /// Change the rotation of a shape
   /// </summary>
   bool SetRotation(string shapeName, Euler rotation);
   
   /// <summary>
   /// Change the scale of a shape
   /// </summary>
   bool SetScale(string shapeName, Vector3 scale);
   
   /// <summary>
   /// Change the dimensions of a shape
   /// </summary>
   bool SetDimensions(string shapeName, double width, double height, double depth);
   
   /// <summary>
   /// Delete a shape from the stage
   /// </summary>
   bool DeleteShape(string name);
   
   /// <summary>
   /// Delete multiple shapes from the stage
   /// </summary>
   int DeleteMultipleShapes(List<string> names);
   
   /// <summary>
   /// Clear all shapes from the stage
   /// </summary>
   bool ClearShapes();
}
