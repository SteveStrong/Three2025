using System.Diagnostics;
using System.Reflection;
using Three2025.Models.Testing;

namespace Three2025.Services.Testing;

#nullable enable

/// <summary>
/// Executes tool methods via reflection and captures results.
/// Handles parameter injection, exception capture, and timing.
/// </summary>
public class TechnicianTestExecutor
{
    private readonly ITestValueProvider _valueProvider;
    private readonly ILogger<TechnicianTestExecutor> _logger;

    public TechnicianTestExecutor(
        ITestValueProvider valueProvider,
        ILogger<TechnicianTestExecutor> logger)
    {
        _valueProvider = valueProvider;
        _logger = logger;
    }

    /// <summary>
    /// Execute a tool method with test parameter values
    /// </summary>
    public async Task<TestResult> ExecuteToolMethod(
        object technicianInstance,
        ToolMethodMetadata methodMetadata)
    {
        var result = new TestResult
        {
            MethodName = methodMetadata.MethodName,
            ExecutedAt = DateTime.Now
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Generate parameter values
            var parameters = methodMetadata.MethodInfo.GetParameters();
            var paramValues = new object?[parameters.Length];

            _logger.LogInformation("🔍 Preparing to call {Method} with {Count} parameters", 
                methodMetadata.MethodName, parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramValue = _valueProvider.GetTestValue(parameters[i], methodMetadata.MethodInfo);
                
                // Convert parameter value to the expected type if needed
                var paramType = parameters[i].ParameterType;
                paramValue = ConvertParameterValue(paramValue, paramType);
                
                paramValues[i] = paramValue;
                
                var paramName = parameters[i].Name ?? $"param{i}";
                var actualType = paramValue?.GetType().Name ?? "null";
                
                _logger.LogInformation("   📌 Parameter[{Index}]: {Name} (expected: {ExpectedType}, actual: {ActualType}, value: {Value})",
                    i, paramName, paramType.Name, actualType, paramValue);
                
                result.ParametersUsed[paramName] = paramValue;
            }

            // Invoke the method
            _logger.LogInformation("🚀 Invoking {Method}...", methodMetadata.MethodName);
            var returnValue = methodMetadata.MethodInfo.Invoke(technicianInstance, paramValues);

            // Handle async methods
            if (returnValue is Task task)
            {
                await task;
                
                // Extract result from Task<T>
                var resultProperty = task.GetType().GetProperty("Result");
                if (resultProperty != null)
                {
                    returnValue = resultProperty.GetValue(task);
                }
                else
                {
                    returnValue = null;
                }
            }

            stopwatch.Stop();
            
            result.Success = true;
            result.ReturnValue = returnValue;
            result.ExecutionTime = stopwatch.Elapsed;

            _logger.LogInformation("Executed {Method} successfully in {Time}ms", 
                methodMetadata.MethodName, stopwatch.ElapsedMilliseconds);
        }
        catch (TargetInvocationException ex)
        {
            stopwatch.Stop();
            
            // Unwrap the inner exception
            var innerException = ex.InnerException ?? ex;
            
            result.Success = false;
            result.ErrorMessage = innerException.Message;
            result.Exception = innerException;
            result.ExecutionTime = stopwatch.Elapsed;

            _logger.LogError(innerException, "Failed to execute {Method}", methodMetadata.MethodName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.Exception = ex;
            result.ExecutionTime = stopwatch.Elapsed;

            _logger.LogError(ex, "Error executing {Method}", methodMetadata.MethodName);
        }

        return result;
    }

    /// <summary>
    /// Execute all tool methods in sequence
    /// </summary>
    public async Task<List<TestResult>> ExecuteAllMethods(
        object technicianInstance,
        List<ToolMethodMetadata> methods)
    {
        var results = new List<TestResult>();

        foreach (var method in methods)
        {
            var result = await ExecuteToolMethod(technicianInstance, method);
            results.Add(result);
            
            // Small delay between tests
            await Task.Delay(100);
        }

        return results;
    }

    /// <summary>
    /// Convert a parameter value to the expected type
    /// </summary>
    private object? ConvertParameterValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        var valueType = value.GetType();
        
        // Already correct type
        if (targetType.IsAssignableFrom(valueType))
            return value;

        try
        {
            // Handle numeric conversions
            if (targetType == typeof(int))
            {
                if (value is double d)
                    return (int)Math.Round(d);
                if (value is float f)
                    return (int)Math.Round(f);
                return Convert.ToInt32(value);
            }

            if (targetType == typeof(double))
            {
                return Convert.ToDouble(value);
            }

            if (targetType == typeof(float))
            {
                return Convert.ToSingle(value);
            }

            if (targetType == typeof(long))
            {
                if (value is double d)
                    return (long)Math.Round(d);
                return Convert.ToInt64(value);
            }

            if (targetType == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }

            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            // Use Convert.ChangeType as fallback
            return Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert {ValueType} to {TargetType}, using original value", 
                valueType.Name, targetType.Name);
            return value;
        }
    }
}
