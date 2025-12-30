namespace Three2025.Apprentice;

/// <summary>
/// Describes the complete capabilities of a tool, including operations, parameters, and supported types.
/// Used for LLM-friendly tool documentation and discovery.
/// </summary>
public class ToolCapabilities
{
   public string ToolName { get; set; } = "";
   public string Version { get; set; } = "1.0";
   public string Description { get; set; } = "";
   public List<CapabilityCategory> Categories { get; set; } = new();
   public List<string> SupportedShapeTypes { get; set; } = new();
   public string CoordinateSystem { get; set; } = "";
}

/// <summary>
/// Groups related operations into logical categories (e.g., "Shape Creation", "Transformations").
/// </summary>
public class CapabilityCategory
{
   public string CategoryName { get; set; } = "";
   public string Description { get; set; } = "";
   public List<ToolOperation> Operations { get; set; } = new();
}

/// <summary>
/// Describes a single operation/method available in a tool, including its parameters and usage.
/// </summary>
public class ToolOperation
{
   public string MethodName { get; set; } = "";
   public string Description { get; set; } = "";
   public List<OperationParameter> Parameters { get; set; } = new();
   public string ReturnType { get; set; } = "";
   public string Example { get; set; } = "";
}

/// <summary>
/// Describes a parameter for a tool operation, including type information and default values.
/// </summary>
public class OperationParameter
{
   public string Name { get; set; } = "";
   public string Type { get; set; } = "";
   public string Description { get; set; } = "";
   public string? DefaultValue { get; set; }
}
