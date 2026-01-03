using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Extensions;
using Three2025.Models.Apprentice;

#nullable enable

namespace Three2025.Apprentice;

/// <summary>
/// Editor for creating and manipulating 2D diagram elements (boxes, links)
/// Maintains state of created shapes for the lifetime of the editor
/// </summary>
public interface IMentor2DEditor
{
    void SetPage(FoPage2D page);
    FoGlyph2D AddBox(string name, int x, int y, int width, int height, string color);
    FoGlyph2D AddStateBox(string name, int x, int y, string color);
    FoGlyph2D AddDecisionBox(string name, int x, int y);
    FoGlyph2D AddDirectedLink(string sourceName, string targetName);
    FoGlyph2D? FindBox(string name);
    FoGlyph2D? FindLink(string linkName);
    bool HasBox(string name);
    IEnumerable<FoGlyph2D> GetAllBoxes();
    IEnumerable<FoGlyph2D> GetAllLinks();
    void Clear();
}

public class Mentor2DEditor : IMentor2DEditor
{
    private readonly IFoundryService _foundryService;
    private FoPage2D? _page;
    private readonly Dictionary<string, FoGlyph2D> _boxes = new();
    private readonly Dictionary<string, FoGlyph2D> _links = new();

    public Mentor2DEditor(IFoundryService foundryService)
    {
        _foundryService = foundryService;
    }

    public void SetPage(FoPage2D page)
    {
        _page = page;
        $"📊 Mentor2DEditor connected to page: {page.GetName()}".WriteInfo();
    }

    private void ShapeChanged()
    {
        _foundryService.PubSub().Publish(RefreshUIEvent.TreeView());
    }

    // ============================================
    // BOX/NODE OPERATIONS
    // ============================================

    public FoGlyph2D AddBox(string name, int x, int y, int width, int height, string color)
    {
        if (_page == null)
        {
            throw new InvalidOperationException("Page not set. Call SetPage first.");
        }

        if (_boxes.ContainsKey(name))
        {
            throw new ArgumentException($"Box with name '{name}' already exists");
        }

        var box = new FoShape2D(width, height, color)
        {
            Name = name
        };
        
        box.MoveTo(x, y);
        _page.AddShape(box);
        _boxes[name] = box;
        
        ShapeChanged();
        return box;
    }

    public FoGlyph2D AddStateBox(string name, int x, int y, string color)
    {
        if (_page == null)
        {
            throw new InvalidOperationException("Page not set. Call SetPage first.");
        }

        if (_boxes.ContainsKey(name))
        {
            throw new ArgumentException($"Box with name '{name}' already exists");
        }

        var box = new FoShape2D(120, 60, color) // Rounded rectangle approximation
        {
            Name = name
        };
        
        box.MoveTo(x, y);
        _page.AddShape(box);
        _boxes[name] = box;
        
        ShapeChanged();
        return box;
    }

    public FoGlyph2D AddDecisionBox(string name, int x, int y)
    {
        if (_page == null)
        {
            throw new InvalidOperationException("Page not set. Call SetPage first.");
        }

        if (_boxes.ContainsKey(name))
        {
            throw new ArgumentException($"Box with name '{name}' already exists");
        }

        // Create diamond shape using square (simple approximation for now)
        var box = new FoShape2D(100, 100, "yellow")
        {
            Name = name
        };
        
        box.MoveTo(x, y);
        _page.AddShape(box);
        _boxes[name] = box;
        
        ShapeChanged();
        return box;
    }

    // ============================================
    // LINK OPERATIONS
    // ============================================

    public FoGlyph2D AddDirectedLink(string sourceName, string targetName)
    {
        if (_page == null)
        {
            throw new InvalidOperationException("Page not set. Call SetPage first.");
        }

        if (!_boxes.ContainsKey(sourceName))
        {
            throw new ArgumentException($"Source box '{sourceName}' not found");
        }
        if (!_boxes.ContainsKey(targetName))
        {
            throw new ArgumentException($"Target box '{targetName}' not found");
        }

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
        
        _page.AddShape(link);
        _links[linkName] = link;
        
        ShapeChanged();
        return link;
    }

    // ============================================
    // QUERY OPERATIONS
    // ============================================

    public FoGlyph2D? FindBox(string name)
    {
        return _boxes.TryGetValue(name, out var box) ? box : null;
    }

    public FoGlyph2D? FindLink(string linkName)
    {
        return _links.TryGetValue(linkName, out var link) ? link : null;
    }

    public bool HasBox(string name)
    {
        return _boxes.ContainsKey(name);
    }

    public IEnumerable<FoGlyph2D> GetAllBoxes()
    {
        return _boxes.Values;
    }

    public IEnumerable<FoGlyph2D> GetAllLinks()
    {
        return _links.Values;
    }

    public void Clear()
    {
        _boxes.Clear();
        _links.Clear();
    }
}
