using Microsoft.Extensions.AI;

namespace Three2025.Services.Agents;

/// <summary>
/// Discovers and provides tools from ITechnician implementations to AI agents.
/// Replaces Semantic Kernel's plugin system.
/// </summary>
public interface ITechnicianToolProvider
{
    /// <summary>
    /// Discovers all tools from all registered ITechnician services
    /// </summary>
    IEnumerable<AIFunction> DiscoverAllTools();
    
    /// <summary>
    /// Get tools from a specific technician type
    /// </summary>
    IEnumerable<AIFunction> GetToolsFor<T>() where T : class;
    
    /// <summary>
    /// Get tools from a specific technician by interface type
    /// </summary>
    IEnumerable<AIFunction> GetToolsFor(Type technicianInterface);
    
    /// <summary>
    /// Get count of discovered tools (for diagnostics)
    /// </summary>
    int GetToolCount();
    
    /// <summary>
    /// Get list of all technician types that have tools
    /// </summary>
    IEnumerable<Type> GetTechnicianTypes();
}
