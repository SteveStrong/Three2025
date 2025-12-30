using System.ComponentModel;

namespace Three2025.Services.Agents;

#nullable enable

/// <summary>
/// Marks a method as available to AI agents as a tool.
/// Replaces legacy [KernelFunction] from Semantic Kernel.
/// Compatible with Microsoft.Agents.AI and AIFunctionFactory.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AgentToolAttribute : Attribute
{
    /// <summary>
    /// The name of the tool as exposed to the AI agent
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// Optional description override (use [Description] attribute instead for consistency)
    /// </summary>
    public string? Description { get; set; }
    
    public AgentToolAttribute(string name)
    {
        Name = name;
    }
    
    /// <summary>
    /// Creates a tool with auto-generated name from method name
    /// </summary>
    public AgentToolAttribute() : this(string.Empty)
    {
    }
}
