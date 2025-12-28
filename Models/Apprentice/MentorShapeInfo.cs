namespace Three2025.Models.Apprentice;

#nullable enable

/// <summary>
/// Information about a diagram box/node
/// </summary>
public record BoxInfo(
    string Name,
    string Label,
    int X,
    int Y,
    int Width,
    int Height,
    string Color,
    string ShapeId
);

/// <summary>
/// Information about a diagram link/connector
/// </summary>
public record LinkInfo(
    string SourceName,
    string TargetName,
    string LinkType,
    string Label,
    string ShapeId
);

/// <summary>
/// Information about a diagram group
/// </summary>
public record GroupInfo(
    string Name,
    List<string> Members,
    string ShapeId
);

/// <summary>
/// Information about a diagram label
/// </summary>
public record LabelInfo(
    string Name,
    string Text,
    int X,
    int Y,
    int FontSize,
    string ShapeId
);
