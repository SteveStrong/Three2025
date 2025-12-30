namespace Three2025.Models.Testing;

#nullable enable

/// <summary>
/// Result of executing a tool method test
/// </summary>
public class TestResult
{
    public bool Success { get; set; }
    public string MethodName { get; set; } = "";
    public object? ReturnValue { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public Dictionary<string, object?> ParametersUsed { get; set; } = new();
    public TimeSpan ExecutionTime { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.Now;
}
