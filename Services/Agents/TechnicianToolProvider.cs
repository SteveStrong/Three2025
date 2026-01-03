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
        
        _logger.LogInformation($"📋 Found {technicianTypes.Count} ITechnician interfaces: {string.Join(", ", technicianTypes.Select(t => t.Name))}");
        
        // DIAGNOSTIC: Check if IModelTech is in the list
        var hasModelTech = technicianTypes.Any(t => t.Name == "IModelTech");
        _logger.LogInformation($"🔍 DIAGNOSTIC: IModelTech found in interfaces: {hasModelTech}");
        
        var allTools = new List<AIFunction>();
        
        foreach (var interfaceType in technicianTypes)
        {
            try
            {
                _logger.LogInformation($"🔍 Processing interface: {interfaceType.Name}");
                var tools = GetToolsFor(interfaceType);
                allTools.AddRange(tools);
                _logger.LogInformation($"  ➡️ Added {tools.Count()} tools from {interfaceType.Name}");
                
                // DIAGNOSTIC: Special logging for IModelTech
                if (interfaceType.Name == "IModelTech")
                {
                    _logger.LogInformation($"🎯 MODELTECH DIAGNOSTIC: Found {tools.Count()} tools from IModelTech");
                    foreach (var tool in tools.Take(10))
                    {
                        _logger.LogInformation($"  📦 ModelTech tool: {tool.Name}");
                    }
                }
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
            _logger.LogWarning($"⚠️  No implementation registered for {interfaceType.Name} - Check DI registration!");
            return tools;
        }
        
        var implementationType = implementation.GetType();
        _logger.LogInformation($"  🔧 Found implementation: {implementationType.Name} for {interfaceType.Name}");
        
        // Find methods with [AgentTool] or [Description]
        var methods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => 
                m.GetCustomAttribute<AgentToolAttribute>() != null ||
                (m.GetCustomAttribute<DescriptionAttribute>() != null && 
                 m.DeclaringType == implementationType))
            .ToList();
        
        _logger.LogInformation($"  🔍 Found {methods.Count} methods with Description/AgentTool attributes in {implementationType.Name}");
        
        // Only log if tools were found
        if (methods.Count > 0)
        {
            _logger.LogInformation($"  ✓ {interfaceType.Name} -> {implementation.GetType().Name}: {methods.Count} tools");
        }
        else
        {
            _logger.LogWarning($"  ⚠️ {interfaceType.Name} -> {implementation.GetType().Name}: NO TOOLS FOUND");
        }
        
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
                string description = agentAttr?.Description 
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
