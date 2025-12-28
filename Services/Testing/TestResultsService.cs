#nullable enable
using Three2025.Models.Testing;

namespace Three2025.Services.Testing;

/// <summary>
/// Centralized service for managing test results across all technician test panels
/// </summary>
public class TestResultsService
{
    private readonly List<TestResult> _results = new();
    private readonly object _lock = new();

    public event Action? OnResultsChanged;

    /// <summary>
    /// Add a new test result
    /// </summary>
    public void AddResult(TestResult result)
    {
        lock (_lock)
        {
            _results.Add(result);
        }
        OnResultsChanged?.Invoke();
    }

    /// <summary>
    /// Get all test results
    /// </summary>
    public List<TestResult> GetAllResults()
    {
        lock (_lock)
        {
            return new List<TestResult>(_results);
        }
    }

    /// <summary>
    /// Clear all test results
    /// </summary>
    public void ClearResults()
    {
        lock (_lock)
        {
            _results.Clear();
        }
        OnResultsChanged?.Invoke();
    }

    /// <summary>
    /// Get results for a specific method
    /// </summary>
    public List<TestResult> GetResultsForMethod(string methodName)
    {
        lock (_lock)
        {
            return _results.Where(r => r.MethodName == methodName).ToList();
        }
    }

    /// <summary>
    /// Get count of successful/failed tests
    /// </summary>
    public (int success, int failed) GetStats()
    {
        lock (_lock)
        {
            return (_results.Count(r => r.Success), _results.Count(r => !r.Success));
        }
    }
}
