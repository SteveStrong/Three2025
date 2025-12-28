using System.Reflection;

namespace Three2025.Services.Testing;

#nullable enable

/// <summary>
/// Interface for providing test parameter values
/// </summary>
public interface ITestValueProvider
{
    object? GetTestValue(ParameterInfo parameter, MethodInfo method);
}

/// <summary>
/// Default strategy for generating sensible test parameter values.
/// Uses parameter names and types to infer appropriate test values.
/// </summary>
public class DefaultTestValueProvider : ITestValueProvider
{
    private readonly Random _random = new();
    private readonly string[] _colors = { "red", "blue", "green", "yellow", "orange", "purple", "cyan", "magenta" };
    private readonly string[] _shapeTypes = { "box", "sphere", "cylinder", "cone", "torus" };
    
    private int _testCounter = 0;

    public object? GetTestValue(ParameterInfo parameter, MethodInfo method)
    {
        var paramName = parameter.Name?.ToLower() ?? "";
        var paramType = parameter.ParameterType;

        // Try default value first
        if (parameter.HasDefaultValue && parameter.DefaultValue != null)
        {
            // For optional params, use defaults 50% of the time for variety
            if (_random.Next(2) == 0)
                return parameter.DefaultValue;
        }

        // Name-based strategies
        if (paramName == "name")
            return GenerateTestName(method);
        
        if (paramName == "sourcename")
            return "TestSource";
        
        if (paramName == "newname")
            return $"Copy_{_testCounter++}";
        
        if (paramName == "color")
            return _colors[_random.Next(_colors.Length)];
        
        if (paramName == "shapetype")
            return _shapeTypes[_random.Next(_shapeTypes.Length)];
        
        if (paramName is "ison" or "visible" or "active" or "enabled")
            return true;
        
        // Position/coordinate parameters
        if (paramName is "x")
        {
            var value = _random.Next(-5, 6);
            return paramType == typeof(int) ? value : value * 1.0;
        }
        
        if (paramName is "y")
        {
            var value = _random.Next(0, 6);
            return paramType == typeof(int) ? value : value * 1.0;
        }
        
        if (paramName is "z")
        {
            var value = _random.Next(-5, 6);
            return paramType == typeof(int) ? value : value * 1.0;
        }
        
        // Dimension parameters
        if (paramName is "width" or "height" or "depth" or "size" or "radius")
        {
            var value = 1.0 + _random.NextDouble() * 2.0; // 1.0 to 3.0
            return paramType == typeof(int) ? (int)Math.Round(value) : value;
        }
        
        // Thickness parameter (for connectors)
        if (paramName is "thickness")
        {
            var value = _random.Next(1, 6);
            return paramType == typeof(int) ? value : value * 1.0;
        }
        
        // Rotation parameters (degrees)
        if (paramName.Contains("degree") || paramName.Contains("rotation") || 
            paramName.Contains("angle"))
            return _random.Next(0, 361) * 1.0;
        
        // Scale parameters
        if (paramName.Contains("scale"))
            return 0.5 + _random.NextDouble() * 1.5; // 0.5 to 2.0
        
        // Offset parameters
        if (paramName.Contains("offset"))
            return _random.Next(-3, 4) * 1.0;
        
        // Count/number parameters
        if (paramName.Contains("count") || paramName.Contains("number"))
            return _random.Next(1, 11);
        
        // Type-based fallbacks
        if (paramType == typeof(string))
            return $"TestValue_{_testCounter++}";
        
        if (paramType == typeof(bool))
            return true;
        
        if (paramType == typeof(int))
            return _random.Next(1, 10);
        
        if (paramType == typeof(double))
            return _random.NextDouble() * 10.0;
        
        if (paramType == typeof(float))
            return (float)(_random.NextDouble() * 10.0);
        
        // List types
        if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = paramType.GetGenericArguments()[0];
            if (itemType == typeof(string))
            {
                return new List<string> { $"Item1_{_testCounter}", $"Item2_{_testCounter}" };
            }
        }

        // Last resort
        if (parameter.HasDefaultValue)
            return parameter.DefaultValue;
        
        if (paramType.IsValueType)
            return Activator.CreateInstance(paramType);
        
        return null;
    }

    private string GenerateTestName(MethodInfo method)
    {
        var baseName = method.Name.Replace("Add", "").Replace("Create", "").Replace("Test", "");
        return $"Test{baseName}_{_testCounter++}";
    }
}
