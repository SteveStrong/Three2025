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

/// <summary>
/// Information about a knowledge shape on the canvas (for conversational modeling)
/// </summary>
public record MentorKnowledgeShapeInfo(
    string Name,
    string Title,
    string KnowledgeType,
    int X,
    int Y,
    string Color,
    string ShapeId,
    string KnowledgeObjectId
);

/// <summary>
/// Result of an attachment operation between two shapes
/// </summary>
public record AttachmentResult(
    bool Success,
    string AttachmentType, // "Containment", "Connection", or "None"
    string Message,
    string? ConnectorId  // ShapeId of connector line if Connection type
);

/// <summary>
/// Information about a knowledge object behind a shape
/// </summary>
public record KnowledgeObjectInfo(
    string Id,
    string Type,
    string Title,
    Dictionary<string, string> Properties,
    List<string> Children,
    List<string> Connections
);

/// <summary>
/// Record of a human action for learning/observation
/// </summary>
public record HumanAction(
    DateTime Timestamp,
    string ActionType, // "CreateShape", "AttachShape", "MoveShape", "DeleteShape"
    string KnowledgeType,
    Dictionary<string, string> Details
);

/// <summary>
/// Statistics about construction patterns learned from observations
/// </summary>
public record ConstructionStats(
    Dictionary<string, int> ShapeTypeFrequency,
    Dictionary<string, List<string>> CommonContainmentPatterns, // Parent → Children types
    Dictionary<string, List<string>> CommonConnectionPatterns,  // Source → Target types
    List<string> FrequentSequences // Ordered sequences of actions
);
