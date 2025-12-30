using System.ComponentModel;
using System.Reflection;
using Three2025.Apprentice;
using Three2025.Models.Testing;

namespace Three2025.Services.Testing;

/// <summary>
/// Extracts tool method metadata from ITechnician implementations using reflection.
/// Mirrors the discovery logic used by TechnicianToolProvider for AI tools.
/// </summary>
public class ToolMetadataExtractor
{
    private readonly ILogger<ToolMetadataExtractor> _logger;

    public ToolMetadataExtractor(ILogger<ToolMetadataExtractor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract all tool methods from a technician type
    /// </summary>
    public List<ToolMethodMetadata> ExtractToolMethods(Type technicianType)
    {
        var toolMethods = new List<ToolMethodMetadata>();

        try
        {
            // Get all public methods with [Description] attribute
            var methods = technicianType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null)
                .OrderBy(m => m.Name);

            foreach (var method in methods)
            {
                var metadata = ExtractMethodMetadata(method);
                toolMethods.Add(metadata);
            }

            _logger.LogInformation("Extracted {Count} tool methods from {Type}", 
                toolMethods.Count, technicianType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract tool methods from {Type}", technicianType.Name);
        }

        return toolMethods;
    }

    /// <summary>
    /// Extract metadata from a single method
    /// </summary>
    private ToolMethodMetadata ExtractMethodMetadata(MethodInfo method)
    {
        var descriptionAttr = method.GetCustomAttribute<DescriptionAttribute>();
        
        var metadata = new ToolMethodMetadata
        {
            MethodName = method.Name,
            Description = descriptionAttr?.Description ?? method.Name,
            ReturnType = GetFriendlyTypeName(method.ReturnType),
            MethodInfo = method,
            Category = DetermineCategory(method.Name),
            Parameters = ExtractParameters(method)
        };

        return metadata;
    }

    /// <summary>
    /// Extract parameter metadata including descriptions from [Description] attributes
    /// </summary>
    private List<ParameterMetadata> ExtractParameters(MethodInfo method)
    {
        var parameters = new List<ParameterMetadata>();

        foreach (var param in method.GetParameters())
        {
            var descriptionAttr = param.GetCustomAttribute<DescriptionAttribute>();
            
            var paramMetadata = new ParameterMetadata
            {
                Name = param.Name ?? "unknown",
                Type = GetFriendlyTypeName(param.ParameterType),
                Description = descriptionAttr?.Description ?? "",
                IsOptional = param.IsOptional,
                HasDefaultValue = param.HasDefaultValue,
                DefaultValue = param.HasDefaultValue ? param.DefaultValue : null
            };

            parameters.Add(paramMetadata);
        }

        return parameters;
    }

    /// <summary>
    /// Get a friendly display name for a type
    /// </summary>
    private string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(double)) return "double";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(void)) return "void";
        
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = type.GetGenericArguments()[0];
            return $"List<{GetFriendlyTypeName(itemType)}>";
        }

        return type.Name;
    }

    /// <summary>
    /// Categorize methods based on naming patterns
    /// </summary>
    private string DetermineCategory(string methodName)
    {
        if (methodName.Contains("Add") || methodName.Contains("Create") || methodName.Contains("Duplicate"))
            return "Creation";
        
        if (methodName.Contains("Delete") || methodName.Contains("Remove") || methodName.Contains("Clear"))
            return "Deletion";
        
        if (methodName.Contains("Get") || methodName.Contains("List") || methodName.Contains("Query"))
            return "Query";
        
        if (methodName.Contains("Reposition") || methodName.Contains("Rotate") || 
            methodName.Contains("Scale") || methodName.Contains("Transform") ||
            methodName.Contains("Change") && methodName.Contains("Dimension"))
            return "Transformation";
        
        if (methodName.Contains("Change") || methodName.Contains("Set") || methodName.Contains("Update"))
            return "Modification";
        
        if (methodName.Contains("Save") || methodName.Contains("Restore") || methodName.Contains("Load"))
            return "Persistence";
        
        if (methodName.Contains("Establish") || methodName.Contains("Initialize") || 
            methodName.Contains("Setup") || methodName.Contains("Refresh"))
            return "Setup";

        return "General";
    }
}
