using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;
using Three2025.Apprentice;

namespace Three2025.Services.Agents;

public class TechnicianToolProvider : ITechnicianToolProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TechnicianToolProvider> _logger;
    private readonly Dictionary<Type, List<AIFunction>> _toolCache = new();
    private readonly object _cacheLock = new();
    
    public TechnicianToolProvider(
        IServiceProvider serviceProvider, 
        ILogger<TechnicianToolProvider> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public IEnumerable<AIFunction> DiscoverAllTools()
    {
        _logger.LogInformation("🔍 Starting tool discovery from ITechnician implementations...");
        
        var technicianTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ITechnician).IsAssignableFrom(t) && 
                       t.IsInterface && 
                       t != typeof(ITechnician))
            .ToList();
        
        _logger.LogInformation($"📋 Found {technicianTypes.Count} ITechnician interfaces");
        
        var allTools = new List<AIFunction>();
        
        foreach (var interfaceType in technicianTypes)
        {
            try
            {
                var tools = GetToolsFor(interfaceType);
                allTools.AddRange(tools);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to discover tools from {interfaceType.Name}");
            }
        }
        
        _logger.LogInformation($"✅ Discovered {allTools.Count} total tools from {technicianTypes.Count} technicians");
        return allTools;
    }
    
    public IEnumerable<AIFunction> GetToolsFor<T>() where T : class
    {
        return GetToolsFor(typeof(T));
    }
    
    public IEnumerable<AIFunction> GetToolsFor(Type technicianInterface)
    {
        lock (_cacheLock)
        {
            if (_toolCache.TryGetValue(technicianInterface, out var cachedTools))
            {
                _logger.LogDebug($"💾 Cache hit for {technicianInterface.Name}");
                return cachedTools;
            }
        }
        
        var tools = ExtractToolsFromTechnician(technicianInterface);
        
        lock (_cacheLock)
        {
            _toolCache[technicianInterface] = tools;
        }
        
        return tools;
    }
    
    public int GetToolCount()
    {
        return DiscoverAllTools().Count();
    }
    
    public IEnumerable<Type> GetTechnicianTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ITechnician).IsAssignableFrom(t) && 
                       t.IsInterface && 
                       t != typeof(ITechnician));
    }
    
    private List<AIFunction> ExtractToolsFromTechnician(Type interfaceType)
    {
        var tools = new List<AIFunction>();
        
        // Get the concrete implementation from DI
        var implementation = _serviceProvider.GetService(interfaceType);
        if (implementation == null)
        {
            _logger.LogWarning($"⚠️  No implementation registered for {interfaceType.Name}");
            return tools;
        }
        
        var implementationType = implementation.GetType();
        
        // Find methods with [AgentTool] or [Description]
        var methods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => 
                m.GetCustomAttribute<AgentToolAttribute>() != null ||
                (m.GetCustomAttribute<DescriptionAttribute>() != null && 
                 m.DeclaringType == implementationType))
            .ToList();
        
        _logger.LogInformation($"  📦 {interfaceType.Name}: Found {methods.Count} tool methods");
        
        foreach (var method in methods)
        {
            try
            {
                var agentAttr = method.GetCustomAttribute<AgentToolAttribute>();
                var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                
                // Determine tool name (priority: AgentTool > method name)
                string toolName = agentAttr?.Name ?? method.Name;
                
                if (string.IsNullOrWhiteSpace(toolName))
                {
                    toolName = method.Name;
                }
                
                // Get description
                string? description = agentAttr?.Description 
                    ?? descAttr?.Description 
                    ?? $"Invokes {method.Name}";
                
                // Create AIFunction using AIFunctionFactory
                var func = AIFunctionFactory.Create(
                    method,
                    target: implementation,
                    name: toolName,
                    description: description);
                
                tools.Add(func);
                
                _logger.LogDebug($"    ✓ {toolName}: {description}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"    ✗ Failed to convert {method.Name} to AIFunction");
            }
        }
        
        return tools;
    }
}
