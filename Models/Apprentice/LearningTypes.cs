namespace Three2025.Models.Apprentice;

#nullable enable

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
