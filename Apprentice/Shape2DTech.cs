using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Technician for creating and managing 2D shapes on a canvas with glue/connection support
/// </summary>
public class Shape2DTech : IShape2DTech
{
    private readonly IWorkspace _workspace;
    private FoPage2D? _page;
    private readonly Dictionary<string, FoGlyph2D> _shapes = new();
    private readonly ILogger<Shape2DTech> _logger;

    public Shape2DTech(IWorkspace workspace, ILogger<Shape2DTech> logger)
    {
        _workspace = workspace;
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
                _page = drawing.EstablishPage<FoPage2D>(pageName);
                _logger.LogInformation("Shape2DTech: Connected to page '{PageName}'", pageName);
            }
            else
            {
                // Fallback: use first page (original behavior)
                _page = drawing.FirstPage();
                _logger.LogInformation("Shape2DTech: Using first page '{PageName}'", _page.Name);
            }
            
            return _page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish Canvas2D");
            throw;
        }
    }

    public void SetPage(FoPage2D page)
    {
        _page = page;
    }

    private FoPage2D GetPage()
    {
        if (_page == null)
        {
            // Default to AgentCanvas2D page which is the visible canvas
            EstablishCanvas2D("AgentCanvas2D");
        }
        return _page!;
    }

    // ============================================
    // CREATION OPERATIONS
    // ============================================

    [Description("Add a rectangle to the 2D canvas with specified dimensions and position")]
    public Shape2DInfo AddRectangle(
        [Description("Unique name for the rectangle")] string name,
        [Description("Width in pixels")] int width,
        [Description("Height in pixels")] int height,
        [Description("Color name (red, blue, green) or hex code (#ff0000)")] string color,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y)
    {
        try
        {
            $"Shape2DTech.AddRectangle: {name}, {width}x{height}, {color} at ({x},{y})".WriteInfo();

            if (_shapes.ContainsKey(name))
            {
                throw new ArgumentException($"Shape with name '{name}' already exists");
            }

            var page = GetPage();
            var shape = new FoShape2D(width, height, color)
            {
                Name = name
            };
            
            shape.MoveTo(x, y);
            page.AddShape(shape);
            _shapes[name] = shape;

            return ConvertToShape2DInfo(shape, "rectangle");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add rectangle {Name}", name);
            throw;
        }
    }

    [Description("Add a circle to the 2D canvas with specified radius and position")]
    public Shape2DInfo AddCircle(
        [Description("Unique name for the circle")] string name,
        [Description("Radius in pixels")] int radius,
        [Description("Color name (red, blue, green) or hex code (#ff0000)")] string color,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y)
    {
        try
        {
            $"Shape2DTech.AddCircle: {name}, radius={radius}, {color} at ({x},{y})".WriteInfo();

            if (_shapes.ContainsKey(name))
            {
                throw new ArgumentException($"Shape with name '{name}' already exists");
            }

            var page = GetPage();
            var diameter = radius * 2;
            var shape = new FoShape2D(diameter, diameter, color)
            {
                Name = name
            };
            shape.ShapeDraw = shape.DrawCircle; // Use the DrawCircle rendering method
            
            shape.MoveTo(x, y);
            page.AddShape(shape);
            _shapes[name] = shape;

            return ConvertToShape2DInfo(shape, "circle", radius);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add circle {Name}", name);
            throw;
        }
    }

    [Description("Add a text label to the canvas at specified position")]
    public Shape2DInfo AddText(
        [Description("Unique name for the text")] string name,
        [Description("The text content to display")] string text,
        [Description("X coordinate position")] int x,
        [Description("Y coordinate position")] int y,
        [Description("Text color name or hex code")] string color)
    {
        try
        {
            $"Shape2DTech.AddText: {name}, '{text}', {color} at ({x},{y})".WriteInfo();

            if (_shapes.ContainsKey(name))
            {
                throw new ArgumentException($"Shape with name '{name}' already exists");
            }

            var page = GetPage();
            var shape = new FoText2D(name, 100, 30, color)
            {
                Text = text,
                TextColor = color
            };
            
            shape.MoveTo(x, y);
            page.AddShape(shape);
            _shapes[name] = shape;

            return ConvertToShape2DInfo(shape, "text");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add text {Name}", name);
            throw;
        }
    }

    [Description("Connect two shapes with a line that automatically tracks their positions when they move")]
    public Shape2DInfo ConnectShapes(
        [Description("Unique name for the connector line")] string connectorName,
        [Description("Name of the shape where the line starts")] string startShapeName,
        [Description("Name of the shape where the line ends")] string endShapeName,
        [Description("Color of the connector line (optional, defaults to Black)")] string color = "Black",
        [Description("Thickness of the line in pixels (optional, defaults to 2)")] int thickness = 2)
    {
        try
        {
            $"Shape2DTech.ConnectShapes: {connectorName} connecting {startShapeName} -> {endShapeName}".WriteInfo();

            if (_shapes.ContainsKey(connectorName))
            {
                throw new ArgumentException($"Connector with name '{connectorName}' already exists");
            }

            if (!_shapes.ContainsKey(startShapeName))
            {
                throw new ArgumentException($"Start shape '{startShapeName}' not found");
            }

            if (!_shapes.ContainsKey(endShapeName))
            {
                throw new ArgumentException($"End shape '{endShapeName}' not found");
            }

            var startShape = _shapes[startShapeName];
            var endShape = _shapes[endShapeName];
            var page = GetPage();

            // Create the connector
            var connector = new FoShape1D(connectorName, color)
            {
                Thickness = thickness
            };

            // THE MAGIC: Glue the connector to both shapes
            connector.GlueStartTo(startShape);
            connector.GlueFinishTo(endShape);

            page.AddShape(connector);
            _shapes[connectorName] = connector;

            $"Shape2DTech.ConnectShapes: Successfully glued {connectorName}".WriteSuccess();

            return ConvertToShape2DInfo(connector, "connector", startShapeName: startShapeName, endShapeName: endShapeName, thickness: thickness);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect shapes with {ConnectorName}", connectorName);
            throw;
        }
    }

    // ============================================
    // TRANSFORMATION OPERATIONS
    // ============================================

    [Description("Move a shape to a new position immediately")]
    public Shape2DInfo MoveShape(
        [Description("Name of the shape to move")] string name,
        [Description("New X coordinate")] int x,
        [Description("New Y coordinate")] int y)
    {
        try
        {
            $"Shape2DTech.MoveShape: {name} to ({x},{y})".WriteInfo();

            if (!_shapes.ContainsKey(name))
            {
                throw new ArgumentException($"Shape '{name}' not found");
            }

            var shape = _shapes[name];
            shape.MoveTo(x, y);

            return ConvertToShape2DInfo(shape, GetShapeType(shape));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move shape {Name}", name);
            throw;
        }
    }

    [Description("Animate a shape moving smoothly to a new position")]
    public Shape2DInfo AnimateMove(
        [Description("Name of the shape to animate")] string name,
        [Description("Target X coordinate")] int x,
        [Description("Target Y coordinate")] int y)
    {
        try
        {
            $"Shape2DTech.AnimateMove: {name} to ({x},{y})".WriteInfo();

            if (!_shapes.ContainsKey(name))
            {
                throw new ArgumentException($"Shape '{name}' not found");
            }

            var shape = _shapes[name];
            shape.AnimatedMoveTo(x, y);

            return ConvertToShape2DInfo(shape, GetShapeType(shape));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to animate shape {Name}", name);
            throw;
        }
    }

    [Description("Change the color of an existing shape")]
    public Shape2DInfo SetColor(
        [Description("Name of the shape")] string name,
        [Description("New color name or hex code")] string color)
    {
        try
        {
            $"Shape2DTech.SetColor: {name} to {color}".WriteInfo();

            if (!_shapes.ContainsKey(name))
            {
                throw new ArgumentException($"Shape '{name}' not found");
            }

            var shape = _shapes[name];
            shape.Color = color;

            return ConvertToShape2DInfo(shape, GetShapeType(shape));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set color for shape {Name}", name);
            throw;
        }
    }

    // ============================================
    // QUERY OPERATIONS
    // ============================================

    [Description("Get a list of all shapes currently on the 2D canvas")]
    public List<Shape2DInfo> GetShapes()
    {
        try
        {
            var result = new List<Shape2DInfo>();
            foreach (var kvp in _shapes)
            {
                result.Add(ConvertToShape2DInfo(kvp.Value, GetShapeType(kvp.Value)));
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get shapes");
            throw;
        }
    }

    [Description("Get information about a specific shape by name")]
    public Shape2DInfo? GetShape(
        [Description("Name of the shape to find")] string name)
    {
        try
        {
            if (!_shapes.ContainsKey(name))
            {
                return null;
            }

            var shape = _shapes[name];
            return ConvertToShape2DInfo(shape, GetShapeType(shape));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get shape {Name}", name);
            throw;
        }
    }

    // ============================================
    // DELETION OPERATIONS
    // ============================================

    [Description("Delete a specific shape from the canvas")]
    public void DeleteShape(
        [Description("Name of the shape to delete")] string name)
    {
        try
        {
            $"Shape2DTech.DeleteShape: {name}".WriteInfo();

            if (!_shapes.ContainsKey(name))
            {
                throw new ArgumentException($"Shape '{name}' not found");
            }

            var shape = _shapes[name];
            var page = GetPage();
            
            shape.Delete();
            _shapes.Remove(name);

            $"Shape2DTech.DeleteShape: {name} removed".WriteSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete shape {Name}", name);
            throw;
        }
    }

    [Description("Clear all shapes from the canvas")]
    public void ClearAll()
    {
        try
        {
            $"Shape2DTech.ClearAll: Removing {_shapes.Count} shapes".WriteInfo();

            foreach (var shape in _shapes.Values)
            {
                shape.Delete();
            }
            _shapes.Clear();

            var page = GetPage();
            page.ClearAll();

            "Shape2DTech.ClearAll: All shapes removed".WriteSuccess();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all shapes");
            throw;
        }
    }

    // ============================================
    // HELPER METHODS
    // ============================================

    private string GetShapeType(FoGlyph2D shape)
    {
        return shape switch
        {
            FoShape1D => "connector",
            FoText2D => "text",
            FoShape2D s when s.ShapeDraw == s.DrawCircle => "circle",
            FoShape2D => "rectangle",
            _ => "unknown"
        };
    }

    private Shape2DInfo ConvertToShape2DInfo(FoGlyph2D shape, string shapeType, int? radius = null, string? startShapeName = null, string? endShapeName = null, int? thickness = null)
    {
        var info = new Shape2DInfo
        {
            Name = shape.Name ?? shape.GetGlyphId(),
            ShapeType = shapeType,
            Color = shape.Color,
            X = (int)shape.PinX,
            Y = (int)shape.PinY,
            Width = (int)shape.Width,
            Height = (int)shape.Height,
            IsVisible = true
        };

        if (shape is FoText2D textShape)
        {
            info.Text = textShape.Text;
        }

        if (shapeType == "circle" && radius.HasValue)
        {
            info.Width = radius.Value * 2;
            info.Height = radius.Value * 2;
        }

        if (shape is FoShape1D && startShapeName != null && endShapeName != null)
        {
            info.StartShape = startShapeName;
            info.EndShape = endShapeName;
            info.Thickness = thickness;
        }

        return info;
    }
}
