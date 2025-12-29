using Microsoft.AspNetCore.Components;
using Three2025.Models.Testing;
using Three2025.Services.Testing;

namespace Three2025.Components.Shared.Testing;

public partial class TestResultsLog : IDisposable
{
#nullable enable
    
    [Inject] private TestResultsService ResultsService { get; set; } = default!;
    
    private List<TestResult> _results = new();

    protected override void OnInitialized()
    {
        ResultsService.OnResultsChanged += HandleResultsChanged;
        _results = ResultsService.GetAllResults();
    }

    private void HandleResultsChanged()
    {
        _results = ResultsService.GetAllResults();
        InvokeAsync(StateHasChanged);
    }

    private void ClearResults()
    {
        ResultsService.ClearResults();
    }

    private string FormatValue(object? value)
    {
        if (value == null) return "null";
        if (value is string s) return $"\"{s}\"";
        if (value is bool b) return b.ToString().ToLower();
        return value.ToString() ?? "null";
    }

    private string FormatReturnValue(object returnValue)
    {
        if (returnValue == null) return "null";
        
        var type = returnValue.GetType();
        var valueStr = returnValue.ToString() ?? "null";
        
        // Show count for collections
        if (returnValue is System.Collections.ICollection collection)
        {
            return $"Returned {collection.Count} item(s)";
        }
        
        // Truncate long strings
        if (valueStr.Length > 100)
        {
            return valueStr.Substring(0, 97) + "...";
        }
        
        return valueStr;
    }

    public void Dispose()
    {
        ResultsService.OnResultsChanged -= HandleResultsChanged;
    }
}
