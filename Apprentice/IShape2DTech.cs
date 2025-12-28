using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Technician for creating and managing 2D shapes on a canvas with glue/connection support
/// </summary>
public interface IShape2DTech : ITechnician
{
    /// <summary>
    /// Initialize and return the 2D canvas page
    /// </summary>
    /// <param name="pageName">Optional page name to connect to. If null, uses first page or creates default.</param>
    FoPage2D EstablishCanvas2D(string? pageName = null);
    
    /// <summary>
    /// Set the page this technician works with
    /// </summary>
    void SetPage(FoPage2D page);
    
    // ============================================
    // CREATION OPERATIONS
    // ============================================
    
    [Description("Add a rectangle to the 2D canvas with specified dimensions and position")]
    Shape2DInfo AddRectangle(
        [Description("Unique name for the rectangle")] string name,
        [Description("Width in pixels")] int width,
        [Description("Height in pixels")] int height,
        [Description("Color name (red, blue, green) or hex code (#ff0000)")] string color,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y);
    
    [Description("Add a circle to the 2D canvas with specified radius and position")]
    Shape2DInfo AddCircle(
        [Description("Unique name for the circle")] string name,
        [Description("Radius in pixels")] int radius,
        [Description("Color name (red, blue, green) or hex code (#ff0000)")] string color,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y);
    
    [Description("Add a text label to the canvas at specified position")]
    Shape2DInfo AddText(
        [Description("Unique name for the text")] string name,
        [Description("The text content to display")] string text,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y,
        [Description("Text color name or hex code")] string color);
    
    [Description("Connect two shapes with a line that automatically tracks their positions when they move")]
    Shape2DInfo ConnectShapes(
        [Description("Unique name for the connector line")] string connectorName,
        [Description("Name of the shape where the line starts")] string startShapeName,
        [Description("Name of the shape where the line ends")] string endShapeName,
        [Description("Color of the connector line (optional, defaults to Black)")] string color = "Black",
        [Description("Thickness of the line in pixels (optional, defaults to 2)")] int thickness = 2);
    
    // ============================================
    // TRANSFORMATION OPERATIONS
    // ============================================
    
    [Description("Move a shape to a new position immediately")]
    Shape2DInfo MoveShape(
        [Description("Name of the shape to move")] string name,
        [Description("New X coordinate")] int x,
        [Description("New Y coordinate")] int y);
    
    [Description("Animate a shape moving smoothly to a new position")]
    Shape2DInfo AnimateMove(
        [Description("Name of the shape to animate")] string name,
        [Description("Target X coordinate")] int x,
        [Description("Target Y coordinate")] int y);
    
    [Description("Change the color of an existing shape")]
    Shape2DInfo SetColor(
        [Description("Name of the shape")] string name,
        [Description("New color name or hex code")] string color);
    
    // ============================================
    // QUERY OPERATIONS
    // ============================================
    
    [Description("Get a list of all shapes currently on the 2D canvas")]
    List<Shape2DInfo> GetShapes();
    
    [Description("Get information about a specific shape by name")]
    Shape2DInfo? GetShape(
        [Description("Name of the shape to find")] string name);
    
    // ============================================
    // DELETION OPERATIONS
    // ============================================
    
    [Description("Delete a specific shape from the canvas")]
    void DeleteShape(
        [Description("Name of the shape to delete")] string name);
    
    [Description("Clear all shapes from the canvas")]
    void ClearAll();
}
