namespace Three2025.Apprentice;

#nullable enable

/// <summary>
/// Information about a 2D shape on the canvas
/// </summary>
public class Shape2DInfo
{
    public string Name { get; set; } = string.Empty;
    public string ShapeType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsVisible { get; set; }
    public string? Text { get; set; }
    public int? Thickness { get; set; }
    public string? StartShape { get; set; }
    public string? EndShape { get; set; }

    public override string ToString()
    {
        if (ShapeType == "connector")
        {
            return $"{Name} ({ShapeType}): {StartShape} -> {EndShape}, Color={Color}, Thickness={Thickness}";
        }
        else if (ShapeType == "text")
        {
            return $"{Name} ({ShapeType}): \"{Text}\" at ({X},{Y}), Color={Color}";
        }
        else
        {
            return $"{Name} ({ShapeType}): {Width}x{Height} at ({X},{Y}), Color={Color}";
        }
    }
}
