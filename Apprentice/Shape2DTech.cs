using System.ComponentModel;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryMentorModeler.Evaluator;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Technician for creating and managing 2D shapes on a canvas with glue/connection support.
/// Delegates to Shape2DEditor for core operations.
/// </summary>
public class Shape2DTech : IShape2DTech
{
    private readonly IWorkspace _workspace;
    private readonly IShape2DEditor _editor;
    private readonly ILogger<Shape2DTech> _logger;
    private readonly IFoundryService _foundryService;

    public Shape2DTech(IWorkspace workspace, IFoundryService foundryService, ILogger<Shape2DTech> logger)
    {
        _workspace = workspace;
        _foundryService = foundryService;
        _editor = new Shape2DEditor(foundryService); // Create editor dynamically
        _logger = logger;
    }

    public FoPage2D EstablishCanvas2D(string? pageName = null)
    {
        try
        {
            var drawing = _workspace.GetDrawing();
            
            // If page name is specified, try to find/establish that page
            if (!string.IsNullOrEmpty(pageName))
            {
                var page = drawing.EstablishPage<FoPage2D>(pageName);
                _editor.SetPage(page);
                _logger.LogInformation("Shape2DTech: Connected to page '{PageName}'", pageName);
                return page;
            }
            else
            {
                // Fallback: use first page (original behavior)
                var page = drawing.FirstPage();
                _editor.SetPage(page);
                _logger.LogInformation("Shape2DTech: Using first page '{PageName}'", page.Name);
                return page;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish Canvas2D");
            throw;
        }
    }

    public void SetPage(FoPage2D page)
    {
        _editor.SetPage(page);
    }

    // ============================================
    // CREATION OPERATIONS
    // ============================================

    [Description("Add a rectangle to the 2D canvas with specified dimensions and position")]
    public OPResult AddRectangle(
        [Description("Unique name for the rectangle")] string name,
        [Description("Width in pixels")] int width,
        [Description("Height in pixels")] int height,
        [Description("Color name (red, blue, green) or hex code (#ff0000)")] string color,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y)
    {
        $"Shape2DTech.AddRectangle: {name}, {width}x{height}, {color} at ({x},{y})".WriteInfo();
        return _editor.AddRectangle(name, width, height, color, x, y);
    }

    [Description("Add a circle to the 2D canvas with specified radius and position")]
    public OPResult AddCircle(
        [Description("Unique name for the circle")] string name,
        [Description("Radius in pixels")] int radius,
        [Description("Color name (red, blue, green) or hex code (#ff0000)")] string color,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y)
    {
        $"Shape2DTech.AddCircle: {name}, radius={radius}, {color} at ({x},{y})".WriteInfo();
        return _editor.AddCircle(name, radius, color, x, y);
    }

    [Description("Add a text label to the canvas at specified position")]
    public OPResult AddText(
        [Description("Unique name for the text")] string name,
        [Description("The text content to display")] string text,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y,
        [Description("Text color name or hex code")] string color)
    {
        $"Shape2DTech.AddText: {name}, '{text}', {color} at ({x},{y})".WriteInfo();
        return _editor.AddText(name, text, x, y, color);
    }

    [Description("Connect two shapes with a line that automatically tracks their positions when they move")]
    public OPResult ConnectShapes(
        [Description("Unique name for the connector line")] string connectorName,
        [Description("Name of the shape where the line starts")] string startShapeName,
        [Description("Name of the shape where the line ends")] string endShapeName,
        [Description("Color of the connector line (optional, defaults to Black)")] string color = "Black",
        [Description("Thickness of the line in pixels (optional, defaults to 2)")] int thickness = 2)
    {
        $"Shape2DTech.ConnectShapes: {connectorName} connecting {startShapeName} -> {endShapeName}".WriteInfo();
        return _editor.ConnectShapes(connectorName, startShapeName, endShapeName, color, thickness);
    }

    // ============================================
    // TRANSFORMATION OPERATIONS
    // ============================================

    [Description("Move a shape to a new position immediately")]
    public OPResult MoveShape(
        [Description("Name of the shape to move")] string name,
        [Description("New X coordinate")] int x,
        [Description("New Y coordinate")] int y)
    {
        $"Shape2DTech.MoveShape: {name} to ({x},{y})".WriteInfo();
        return _editor.SetPosition(name, x, y);
    }

    [Description("Animate a shape moving smoothly to a new position")]
    public OPResult AnimateMove(
        [Description("Name of the shape to animate")] string name,
        [Description("Target X coordinate")] int x,
        [Description("Target Y coordinate")] int y)
    {
        $"Shape2DTech.AnimateMove: {name} to ({x},{y})".WriteInfo();
        
        // Get the shape first
        var result = _editor.GetShapeByName(name);
        if (result.IsError())
            return result;
        
        var shape = result.AsShape2D();
        shape.AnimatedMoveTo(x, y);
        
        return result;
    }

    [Description("Change the color of an existing shape")]
    public OPResult SetColor(
        [Description("Name of the shape")] string name,
        [Description("New color name or hex code")] string color)
    {
        $"Shape2DTech.SetColor: {name} to {color}".WriteInfo();
        return _editor.SetColor(name, color);
    }

    // ============================================
    // QUERY OPERATIONS
    // ============================================

    [Description("Get a list of all shapes currently on the 2D canvas")]
    public OPResult GetShapes()
    {
        return _editor.GetAllShapes();
    }

    [Description("Get information about a specific shape by name")]
    public OPResult GetShape(
        [Description("Name of the shape to find")] string name)
    {
        return _editor.GetShapeByName(name);
    }

    // ============================================
    // DELETION OPERATIONS
    // ============================================

    [Description("Delete a specific shape from the canvas")]
    public OPResult DeleteShape(
        [Description("Name of the shape to delete")] string name)
    {
        $"Shape2DTech.DeleteShape: {name}".WriteInfo();
        return _editor.DeleteShape(name);
    }

    [Description("Clear all shapes from the canvas")]
    public OPResult ClearAll()
    {
        $"Shape2DTech.ClearAll".WriteInfo();
        return _editor.ClearShapes();
    }
}