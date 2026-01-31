#nullable enable

using System.Reflection;
using Three2025.Models.Chat;

namespace Three2025.Services.Chat;

/// <summary>
/// Base class for test scenario collections
/// </summary>
public abstract class ChatTestScenariosBase
{
    /// <summary>
    /// Category/domain name for this scenario collection
    /// </summary>
    public abstract string Domain { get; }
}

/// <summary>
/// 3D Geometry test sequences - operations that accumulate on 3D shapes
/// </summary>
public class ChatTestScenarios3D : ChatTestScenariosBase
{
    public override string Domain => "3D Geometry";

    [TestSequence(
        DisplayName = "🔷 Basic Geometry",
        Description = "Tests basic 3D geometry creation and modification",
        Category = "3D Geometry")]
    public string[] BasicGeometryTest => new[]
    {
        "Create a red box named box1",
        "Change it to yellow",
        "Move the X location to 4",
        "Convert it to a cylinder"
    };

    [TestSequence(
        DisplayName = "🎨 Color Cycle",
        Description = "Tests color changes on 3D shapes",
        Category = "3D Geometry")]
    public string[] ColorCycleTest => new[]
    {
        "Create a blue sphere named sphere1",
        "Change it to green",
        "Change it to red",
        "Make it yellow"
    };

    [TestSequence(
        DisplayName = "📐 Transform Sequence",
        Description = "Tests position and scale transformations on 3D objects",
        Category = "3D Geometry")]
    public string[] TransformSequence => new[]
    {
        "Create a box named box1 at position 0,0,0",
        "Move it to X:5",
        "Move it to Y:3",
        "Make it twice as big",
        "Rotate it 45 degrees"
    };

    [TestSequence(
        DisplayName = "🔄 Shape Conversion",
        Description = "Tests converting between different 3D shape types",
        Category = "3D Geometry")]
    public string[] ShapeConversionTest => new[]
    {
        "Create a box named box1",
        "Convert it to a sphere",
        "Convert box1 to a cylinder",
        "Convert it to a cone"
    };

    [TestSequence(
        DisplayName = "🌈 Full Color Spectrum",
        Description = "Tests all basic color changes on 3D objects",
        Category = "3D Geometry")]
    public string[] FullColorSpectrumTest => new[]
    {
        "Create a white cube named cube1 with sides of length 3",
        "Make cube1 red",
        "Make cube1 orange",
        "Make cube1 green",
        "Make cube1 blue",
        "Make cube1 purple"
    };

    [TestSequence(
        DisplayName = "📍 Position Grid",
        Description = "Tests positioning 3D objects in a grid pattern",
        Category = "3D Geometry")]
    public string[] PositionGridTest => new[]
    {
        "Create a box at X:0, Y:0, Z:0",
        "Create a sphere at X:2, Y:0, Z:0",
        "Create a cylinder at X:4, Y:0, Z:0",
        "Create a cone at X:0, Y:2, Z:0"
    };

    [TestSequence(
        DisplayName = "📏 Size Variation",
        Description = "Tests different size modifications on 3D objects",
        Category = "3D Geometry")]
    public string[] SizeVariationTest => new[]
    {
        "Create a box",
        "Make it twice as big",
        "Make it half the size",
        "Scale it to width 5, height 2, depth 3"
    };

}

/// <summary>
/// 2D Geometry test sequences - operations on 2D shapes and drawings
/// </summary>
public class ChatTestScenarios2D : ChatTestScenariosBase
{
    public override string Domain => "2D Geometry";

    [TestSequence(
        DisplayName = "⬜ Basic 2D Shapes",
        Description = "Tests creating and modifying basic 2D shapes",
        Category = "2D Geometry")]
    public string[] Basic2DShapesTest => new[]
    {
        "Create a red rectangle",
        "Create a blue circle",
        "Create a green triangle"
    };

    [TestSequence(
        DisplayName = "🎨 2D Color Operations",
        Description = "Tests color operations on 2D shapes",
        Category = "2D Geometry")]
    public string[] Color2DTest => new[]
    {
        "Create a circle",
        "Make it red",
        "Change it to blue",
        "Make it transparent"
    };

    [TestSequence(
        DisplayName = "📐 2D Transform",
        Description = "Tests positioning and scaling 2D shapes",
        Category = "2D Geometry")]
    public string[] Transform2DTest => new[]
    {
        "Create a square at X:0, Y:0",
        "Move it to X:100, Y:50",
        "Scale it by 2",
        "Rotate it 45 degrees"
    };
}

/// <summary>
/// Model test sequences - operations on complete models and assemblies
/// </summary>
public class ChatTestScenariosModels : ChatTestScenariosBase
{
    public override string Domain => "Models";

    [TestSequence(
        DisplayName = "🏗️ Model Creation",
        Description = "Tests creating and managing 3D models",
        Category = "Models")]
    public string[] ModelCreationTest => new[]
    {
        "Create a new model",
        "Add a box to the model",
        "Add a sphere to the model",
        "Group them together"
    };

    [TestSequence(
        DisplayName = "🔧 Model Operations",
        Description = "Tests common model operations",
        Category = "Models")]
    public string[] ModelOperationsTest => new[]
    {
        "Create a model with a box and sphere",
        "Clone the model",
        "Move the clone to X:10",
        "Scale the original by 0.5"
    };

    [TestSequence(
        DisplayName = "📦 Model Assembly",
        Description = "Tests building complex assemblies",
        Category = "Models")]
    public string[] ModelAssemblyTest => new[]
    {
        "Create a base platform as a box",
        "Add a cylinder on top",
        "Add a sphere on the cylinder",
        "Color the assembly blue"
    };

    [TestSequence(
        DisplayName = "🗄️ Rack Knowledge Model",
        Description = "Interactive demonstration of rack equipment knowledge models with calculated parameters",
        Category = "Models")]
    public string[] RackKnowledgeModelTest => new[]
    {
        "Navigate to /rack-knowledge-model",
        "Click Create Knowledge Model button",
        "Explore the data center hierarchy in the tree",
        "Click on MF_Cabinet_1 to see utilization",
        "Click on equipment items to see RU calculations",
        "Click Generate 3D Geometry to create FO shapes"
    };
}

/// <summary>
/// Knowledge engineering test scenarios for Mentor 2D modeling
/// These prompts ask engineering questions that should result in concept/property creation
/// </summary>
public class ChatTestScenariosKnowledge : ChatTestScenariosBase
{
    public override string Domain => "Knowledge Engineering";

    [TestSequence(
        DisplayName = "🏗️ Structural Engineering",
        Description = "Model structural engineering concepts with properties",
        Category = "Knowledge Engineering")]
    public string[] StructuralEngineeringTest => new[]
    {
        "I need to model a steel beam for a bridge. What properties should I consider?",
        "Add material properties like yield strength and elastic modulus",
        "Include geometric properties like moment of inertia", 
        "Show the relationship between beam dimensions and load capacity"
    };

    [TestSequence(
        DisplayName = "⚙️ Mechanical Systems",
        Description = "Model mechanical components and their specifications", 
        Category = "Knowledge Engineering")]
    public string[] MechanicalSystemsTest => new[]
    {
        "Design a motor specification with key performance parameters",
        "Add efficiency and power ratings",
        "Include operating temperature range and RPM specifications",
        "Model the relationship between torque and speed"
    };

    [TestSequence(
        DisplayName = "🔌 Electrical Components", 
        Description = "Model electrical engineering concepts",
        Category = "Knowledge Engineering")]
    public string[] ElectricalComponentsTest => new[]
    {
        "Create a Resistor model with electrical properties",
        "Add voltage and current ratings", 
        "Include power dissipation and tolerance specifications",
        "Show how resistance affects power consumption"
    };

    [TestSequence(
        DisplayName = "📊 System Requirements",
        Description = "Model system-level engineering requirements",
        Category = "Knowledge Engineering")]
    public string[] SystemRequirementsTest => new[]
    {
        "Model a water pump system with performance requirements",
        "Add flow rate and pressure head specifications",
        "Include efficiency and NPSH requirements",
        "Model the pump curve relationship between flow and head"
    };

    [TestSequence(
        DisplayName = "🧪 Material Properties",
        Description = "Model material science concepts with measured properties",
        Category = "Knowledge Engineering")]
    public string[] MaterialPropertiesTest => new[]
    {
        "Model aluminum alloy 6061 with its key properties",
        "Add density, thermal conductivity, and strength properties",
        "Include corrosion resistance and machinability ratings",
        "Show the relationship between alloy composition and properties"
    };

    [TestSequence(
        DisplayName = "🔧 Tool Capabilities",
        Description = "Test knowledge modeling tool discovery",
        Category = "Knowledge Engineering")]
    public string[] ToolCapabilitiesTest => new[]
    {
        "What tools do you have for creating engineering models?",
        "Create a simple model with properties",
        "Show how models can contain component instances",
        "Demonstrate connecting related models with relationships"
    };

    [TestSequence(
        DisplayName = "🎯 Specific Motor Model",
        Description = "Create a motor concept with exact properties",
        Category = "Knowledge Engineering")]
    public string[] SpecificMotorTest => new[]
    {
        "Create a Motor model",
        "Add an RPM property to the Motor",
        "Add a Voltage property to the Motor", 
        "Add a Current property to the Motor",
        "Add a Power property to the Motor"
    };

    [TestSequence(
        DisplayName = "🎯 Specific Pump Model", 
        Description = "Create a pump concept with exact flow properties",
        Category = "Knowledge Engineering")]
    public string[] SpecificPumpTest => new[]
    {
        "Create a Pump model",
        "Add a Flow Rate property to the Pump",
        "Add a Pressure property to the Pump",
        "Add an Efficiency property to the Pump",
        "Add a NPSH property to the Pump"
    };

    [TestSequence(
        DisplayName = "🎯 Specific Valve Model",
        Description = "Create a valve concept with control properties", 
        Category = "Knowledge Engineering")]
    public string[] SpecificValveTest => new[]
    {
        "Create a Valve model",
        "Add a Size property to the Valve",
        "Add a Material property to the Valve",
        "Add a Pressure Rating property to the Valve",
        "Add a Flow Coefficient property to the Valve"
    };

    [TestSequence(
        DisplayName = "🎯 Battery Specifications",
        Description = "Create a battery with electrical specifications",
        Category = "Knowledge Engineering")]
    public string[] BatterySpecTest => new[]
    {
        "Create a Battery model",
        "Add a Capacity property with units Ah", 
        "Add a Voltage property with value 12V",
        "Add a Chemistry property with value Lithium-Ion",
        "Add a Cycle Life property"
    };

    [TestSequence(
        DisplayName = "🎯 Heat Exchanger Model",
        Description = "Create heat exchanger with thermal properties",
        Category = "Knowledge Engineering")]
    public string[] HeatExchangerTest => new[]
    {
        "Create a Heat Exchanger model",
        "Add a Heat Transfer Rate property in BTU/hr",
        "Add an Inlet Temperature property",
        "Add an Outlet Temperature property",
        "Add a Pressure Drop property"
    };

    [TestSequence(
        DisplayName = "🎯 Building HVAC System",
        Description = "Create interconnected HVAC components",
        Category = "Knowledge Engineering")]
    public string[] HVACSystemTest => new[]
    {
        "Create an HVAC System model",
        "Create an Air Handler component with CFM property",
        "Create a Chiller component with Cooling Capacity property",  
        "Create a Ductwork component with Size property",
        "Connect these components in the HVAC System"
    };
}

/// <summary>
/// Central discovery class for all test scenarios
/// </summary>
public static class ChatTestScenarios
{
    /// <summary>
    /// All scenario collection types to scan
    /// </summary>
    private static readonly Type[] ScenarioTypes = new[]
    {
        typeof(ChatTestScenarios3D),
        typeof(ChatTestScenarios2D),
        typeof(ChatTestScenariosModels),
        typeof(ChatTestScenariosKnowledge)
    };

    /// <summary>
    /// Discovers all test sequences from all scenario collections using reflection
    /// </summary>
    public static Dictionary<string, TestSequenceMetadata> GetAllSequences()
    {
        var sequences = new Dictionary<string, TestSequenceMetadata>();

        foreach (var scenarioType in ScenarioTypes)
        {
            var instance = Activator.CreateInstance(scenarioType) as ChatTestScenariosBase;
            if (instance == null) continue;

            foreach (var prop in scenarioType.GetProperties())
            {
                if (prop.PropertyType == typeof(string[]))
                {
                    var attr = prop.GetCustomAttribute<TestSequenceAttribute>();
                    var prompts = prop.GetValue(instance) as string[];

                    var key = $"{scenarioType.Name}.{prop.Name}";
                    sequences[key] = new TestSequenceMetadata
                    {
                        Name = key,
                        DisplayName = attr?.DisplayName ?? prop.Name,
                        Description = attr?.Description ?? string.Empty,
                        Category = attr?.Category ?? instance.Domain,
                        Prompts = prompts ?? Array.Empty<string>()
                    };
                }
            }
        }

        return sequences;
    }

    /// <summary>
    /// Gets a specific test sequence by name
    /// </summary>
    public static TestSequenceMetadata? GetSequence(string name)
    {
        var all = GetAllSequences();
        return all.TryGetValue(name, out var sequence) ? sequence : null;
    }

    /// <summary>
    /// Gets all sequences grouped by category
    /// </summary>
    public static Dictionary<string, List<TestSequenceMetadata>> GetSequencesByCategory()
    {
        return GetAllSequences()
            .Values
            .GroupBy(s => s.Category)
            .OrderBy(g => g.Name)
            .ToDictionary(g => g.Name, g => g.OrderBy(s => s.DisplayName).ToList());
    }

    /// <summary>
    /// Gets all sequences from a specific domain
    /// </summary>
    public static List<TestSequenceMetadata> GetSequencesByDomain(string domain)
    {
        return GetAllSequences()
            .Values
            .Where(s => s.Category == domain)
            .OrderBy(s => s.DisplayName)
            .ToList();
    }
}
