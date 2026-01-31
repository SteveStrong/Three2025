using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Three2025.Models.Testing;
using Three2025.Services.Testing;
using Three2025.Apprentice;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Shape;

namespace Three2025.Components.Shared.Testing;

public partial class TechnicianTestPanel
{
#nullable enable
    
    [Parameter] public Type? TechnicianType { get; set; }
    [Parameter] public string Title { get; set; } = "Tool Test Panel";
    [Parameter] public bool GroupByCategory { get; set; } = true;

    [Inject] private ToolMetadataExtractor MetadataExtractor { get; set; } = default!;
    [Inject] private TechnicianTestExecutor TestExecutor { get; set; } = default!;
    [Inject] private TestResultsService ResultsService { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;
    [Inject] private ILogger<TechnicianTestPanel> Logger { get; set; } = default!;

    private List<ToolMethodMetadata>? _toolMethods;
    private Dictionary<string, List<ToolMethodMetadata>> _groupedMethods = new();
    private HashSet<string> _expandedCategories = new(); // Track which categories are expanded
    private object? _technicianInstance;
    private bool _isLoading = true;
    private bool _isExecuting = false;
    private string? _currentlyExecuting;

    protected override async Task OnInitializedAsync()
    {
        await DiscoverTools();
    }

    private async Task DiscoverTools()
    {
        _isLoading = true;
        
        try
        {
            if (TechnicianType == null)
            {
                Logger.LogWarning("TechnicianType parameter not provided");
                return;
            }

            // Resolve technician instance from DI
            _technicianInstance = ServiceProvider.GetService(TechnicianType);
            
            if (_technicianInstance == null)
            {
                Logger.LogError("Failed to resolve {Type} from DI", TechnicianType.Name);
                return;
            }

            // Extract tool methods from the ACTUAL implementation type (not the interface)
            // [Description] attributes are on Shape3DTech class, not IShape3DTech interface
            var actualType = _technicianInstance.GetType();
            _toolMethods = MetadataExtractor.ExtractToolMethods(actualType);

            // Group by category if requested
            if (GroupByCategory)
            {
                _groupedMethods = _toolMethods
                    .GroupBy(m => m.Category)
                    .ToDictionary(g => g.Name, g => g.ToList());
            }

            // Initialize Shape3DTech stage if this is a geometry technician
            if (_technicianInstance is IShape3DTech geometryTech)
            {
                try
                {
                    Logger.LogInformation("Connecting Shape3DTech to 'AgentCanvas3D' stage via IArena...");
                    geometryTech.EstablishGeometryStage("AgentCanvas3D");
                    Logger.LogInformation("Shape3DTech connected to stage successfully");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to initialize Shape3DTech");
                }
            }
            
            // Initialize ModelTech model if this is a model technician
            if (_technicianInstance is IModelTech modelTech)
            {
                try
                {
                    Logger.LogInformation("Initializing ModelTech with default model...");
                    modelTech.EstablishModel("DefaultModel");
                    Logger.LogInformation("ModelTech initialized successfully");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to initialize ModelTech");
                }
            }
            
            // Initialize Shape2DTech canvas if this is a 2D shape technician
            if (_technicianInstance is IShape2DTech shape2DTech)
            {
                try
                {
                    Logger.LogInformation("Connecting Shape2DTech to 'AgentCanvas2D' page via IWorkspace...");
                    shape2DTech.EstablishCanvas2D("AgentCanvas2D");
                    Logger.LogInformation("Shape2DTech connected to page successfully");
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to initialize Shape2DTech");
                }
            }

            Logger.LogInformation("Discovered {Count} tool methods from {Type}", 
                _toolMethods.Count, TechnicianType.Name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to discover tools for {Type}", TechnicianType?.Name);
        }
        finally
        {
            _isLoading = false;
        }

        await InvokeAsync(StateHasChanged);
    }

    private void ToggleCategory(string category)
    {
        if (_expandedCategories.Contains(category))
            _expandedCategories.Remove(category);
        else
            _expandedCategories.Add(category);
    }

    private async Task ExecuteMethod(ToolMethodMetadata method)
    {
        if (_technicianInstance == null || _isExecuting)
            return;

        _currentlyExecuting = method.MethodName;
        await InvokeAsync(StateHasChanged);

        try
        {
            var result = await TestExecutor.ExecuteToolMethod(_technicianInstance, method);
            ResultsService.AddResult(result);
        }
        finally
        {
            _currentlyExecuting = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ExecuteAllMethods()
    {
        if (_technicianInstance == null || _toolMethods == null || _isExecuting)
            return;

        _isExecuting = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            var results = await TestExecutor.ExecuteAllMethods(_technicianInstance, _toolMethods);
            foreach (var result in results)
            {
                ResultsService.AddResult(result);
            }
        }
        finally
        {
            _isExecuting = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void ClearResults()
    {
        ResultsService.ClearResults();
    }

    private string FormatValue(object? value)
    {
        if (value == null) return "null";
        if (value is string s) return $"\"{s}\"";
        if (value is double d) return d.ToString("F1");
        if (value is float f) return f.ToString("F1");
        return value.ToString() ?? "null";
    }

    private string FormatReturnValue(object? value)
    {
        if (value == null) return "null";
        
        if (value is System.Collections.IList list)
        {
            return $"Returned {list.Count} item(s)";
        }
        
        if (value is string s)
        {
            return s.Length > 50 ? s.Substring(0, 47) + "..." : s;
        }
        
        return value.ToString() ?? "null";
    }
}
