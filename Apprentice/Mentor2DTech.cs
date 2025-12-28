using System.ComponentModel;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryRulesAndUnits.Extensions;
using Three2025.Models.Apprentice;

namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Technician for creating and managing 2D diagram shapes with AI-discoverable tools
/// Uses FoShape2D with styling to create diagram-like boxes and connectors
/// </summary>
public class Mentor2DTech : IMentor2DTech
{
    private readonly IWorkspace _workspace;
    private FoPage2D? _page;
    private readonly Dictionary<string, FoGlyph2D> _boxes = new();
    private readonly Dictionary<string, FoGlyph2D> _links = new();
    private readonly ILogger<Mentor2DTech> _logger;

    public Mentor2DTech(IWorkspace workspace, ILogger<Mentor2DTech> logger)
    {
        _workspace = workspace;
        _logger = logger;
    }

    public FoPage2D EstablishCanvas2D(string? pageName = null)
    {
        try
        {
            var drawing = _workspace.GetDrawing();
            
            if (!string.IsNullOrEmpty(pageName))
            {
                _page = drawing.EstablishPage<FoPage2D>(pageName);
                _logger.LogInformation("Mentor2DTech: Connected to page '{PageName}'", pageName);
            }
            else
            {
                _page = drawing.FirstPage();
                _logger.LogInformation("Mentor2DTech: Using first page '{PageName}'", _page.Name);
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
            EstablishCanvas2D("AgentCanvas2D");
        }
        return _page!;
    }

    // ============================================
    // BOX/NODE OPERATIONS
    // ============================================

    [Description("Add a standard rectangular box to the diagram with label and position")]
    public BoxInfo AddBox(
        [Description("Unique name for the box")] string name,
        [Description("Display label text")] string label,
        [Description("X coordinate in pixels")] int x,
        [Description("Y coordinate in pixels")] int y,
        [Description("Width in pixels")] int width,
        [Description("Height in pixels")] int height,
        [Description("Box color (red, blue, green) or hex code (#ff0000)")] string color)
    {
        try
        {
            $"Mentor2DTech.AddBox: {name}, '{label}' at ({x},{y}) size {width}x{height}, {color}".WriteInfo();

            if (_boxes.ContainsKey(name))
            {
                throw new ArgumentException($"Box with name '{name}' already exists");
            }

            var page = GetPage();
            var box = new FoShape2D(width, height, color)
            {
                Name = name
            };
            
            box.MoveTo(x, y);
            page.AddShape(box);
            _boxes[name] = box;

            return new BoxInfo(name, label, x, y, width, height, color, box.GlyphId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add box '{Name}'", name);
            throw;
        }
    }

    [Description("Add a state box with rounded corners for state diagrams")]
    public BoxInfo AddStateBox(
        [Description("Unique name for the state")] string name,
        [Description("State label text")] string label,
        [Description("X coordinate in pixels")] int x,
        [Description("Y coordinate in pixels")] int y,
        [Description("State color (red, blue, green) or hex code")] string color)
    {
        try
        {
            $"Mentor2DTech.AddStateBox: {name}, '{label}' at ({x},{y}), {color}".WriteInfo();

            if (_boxes.ContainsKey(name))
            {
                throw new ArgumentException($"Box with name '{name}' already exists");
            }

            var page = GetPage();
            var box = new FoShape2D(120, 60, color) // Rounded rectangle approximation
            {
                Name = name
            };
            
            box.MoveTo(x, y);
            page.AddShape(box);
            _boxes[name] = box;

            return new BoxInfo(name, label, x, y, 120, 60, color, box.GlyphId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add state box '{Name}'", name);
            throw;
        }
    }

    [Description("Add a diamond-shaped decision box for flowcharts")]
    public BoxInfo AddDecisionBox(
        [Description("Unique name for the decision")] string name,
        [Description("Decision question text")] string label,
        [Description("X coordinate in pixels")] int x,
        [Description("Y coordinate in pixels")] int y)
    {
        try
        {
            $"Mentor2DTech.AddDecisionBox: {name}, '{label}' at ({x},{y})".WriteInfo();

            if (_boxes.ContainsKey(name))
            {
                throw new ArgumentException($"Box with name '{name}' already exists");
            }

            var page = GetPage();
            // Create diamond shape using square (simple approximation for now)
            var box = new FoShape2D(100, 100, "yellow")
            {
                Name = name
            };
            
            box.MoveTo(x, y);
            page.AddShape(box);
            _boxes[name] = box;

            return new BoxInfo(name, label, x, y, 100, 100, "yellow", box.GlyphId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add decision box '{Name}'", name);
            throw;
        }
    }

    // ============================================
    // LINK OPERATIONS
    // ============================================

    [Description("Add a directed link (arrow) connecting two boxes")]
    public LinkInfo AddDirectedLink(
        [Description("Name of the source box")] string sourceName,
        [Description("Name of the target box")] string targetName,
        [Description("Optional label for the link")] string label)
    {
        try
        {
            $"Mentor2DTech.AddDirectedLink: {sourceName} -> {targetName}, label='{label}'".WriteInfo();

            if (!_boxes.ContainsKey(sourceName))
            {
                throw new ArgumentException($"Source box '{sourceName}' not found");
            }
            if (!_boxes.ContainsKey(targetName))
            {
                throw new ArgumentException($"Target box '{targetName}' not found");
            }

            var page = GetPage();
            var sourceBox = _boxes[sourceName];
            var targetBox = _boxes[targetName];
            
            // Create a line connecting the two boxes
            var linkName = $"{sourceName}_to_{targetName}";
            int x1 = sourceBox.PinX + sourceBox.Width / 2;
            int y1 = sourceBox.PinY + sourceBox.Height / 2;
            int x2 = targetBox.PinX + targetBox.Width / 2;
            int y2 = targetBox.PinY + targetBox.Height / 2;
            
            var link = new FoShape1D(x1, y1, x2, y2, 2, "black")
            {
                Name = linkName
            };
            
            page.AddShape(link);
            _links[linkName] = link;

            return new LinkInfo(sourceName, targetName, "directed", label, link.GlyphId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add link from '{Source}' to '{Target}'", sourceName, targetName);
            throw;
        }
    }

    // ============================================
    // QUERY OPERATIONS
    // ============================================

    [Description("Find a box by name and return its information")]
    public BoxInfo? FindBox([Description("Name of the box to find")] string name)
    {
        if (_boxes.TryGetValue(name, out var box))
        {
            return new BoxInfo(
                box.Name ?? name,
                "",  // Label not stored in base FoGlyph2D
                box.PinX,
                box.PinY,
                box.Width,
                box.Height,
                box.Color,
                box.GlyphId
            );
        }
        return null;
    }

    [Description("Get a list of all boxes in the diagram")]
    public List<BoxInfo> GetAllBoxes()
    {
        return _boxes.Values.Select(box => new BoxInfo(
            box.Name ?? "",
            "",  // Label not stored in base FoGlyph2D
            box.PinX,
            box.PinY,
            box.Width,
            box.Height,
            box.Color,
            box.GlyphId
        )).ToList();
    }

    [Description("Get a list of all links in the diagram")]
    public List<LinkInfo> GetAllLinks()
    {
        return _links.Values.Select(link => new LinkInfo(
            link.Name ?? "",
            "",  // target name not easily accessible
            "directed",
            "",  // Label not stored in base FoGlyph2D
            link.GlyphId
        )).ToList();
    }

    // ============================================
    // MODIFICATION OPERATIONS
    // ============================================

    [Description("Move a box to a new position")]
    public void MoveBox(
        [Description("Name of the box to move")] string name,
        [Description("New X coordinate")] int x,
        [Description("New Y coordinate")] int y)
    {
        try
        {
            if (!_boxes.TryGetValue(name, out var box))
            {
                throw new ArgumentException($"Box '{name}' not found");
            }

            box.MoveTo(x, y);
            $"Mentor2DTech.MoveBox: {name} moved to ({x},{y})".WriteInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move box '{Name}'", name);
            throw;
        }
    }

    [Description("Update the label text of a box")]
    public void UpdateBoxLabel(
        [Description("Name of the box")] string name,
        [Description("New label text")] string newLabel)
    {
        try
        {
            if (!_boxes.TryGetValue(name, out var box))
            {
                throw new ArgumentException($"Box '{name}' not found");
            }

            // TODO: Labels require FoText2D overlay shapes
            box.Name = newLabel;  // Update name as approximation
            $"Mentor2DTech.UpdateBoxLabel: {name} label updated to '{newLabel}'".WriteInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update box label '{Name}'", name);
            throw;
        }
    }

    [Description("Delete a box from the diagram")]
    public void DeleteBox([Description("Name of the box to delete")] string name)
    {
        try
        {
            if (!_boxes.TryGetValue(name, out var box))
            {
                throw new ArgumentException($"Box '{name}' not found");
            }

            var page = GetPage();
            page.RemoveShape(box);
            _boxes.Remove(name);
            
            // Remove any links connected to this box (simplified for now)
            var linksToRemove = new List<string>();
            // TODO: Track source/target relationships for proper link cleanup
            
            foreach (var linkName in linksToRemove)
            {
                var link = _links[linkName];
                page.RemoveShape(link);
                _links.Remove(linkName);
            }

            $"Mentor2DTech.DeleteBox: {name} deleted with {linksToRemove.Count} connected links".WriteInfo();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete box '{Name}'", name);
            throw;
        }
    }
}
