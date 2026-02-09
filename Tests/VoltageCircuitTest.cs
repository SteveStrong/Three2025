using FoundryMentorModeler.Model;
using FoundryRulesAndUnits.Units;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Tests;

/// <summary>
/// Test for evaluating voltage divider circuit parameters
/// Demonstrates how to create a model and trigger parameter calculations
/// </summary>
public static class VoltageCircuitTest
{
    /// <summary>
    /// Create and evaluate a voltage divider circuit
    /// </summary>
    public static void TestVoltageCircuit()
    {
        Console.WriteLine("=== Voltage Divider Circuit Test ===\n");

        // Create the model (like your image shows)
        var model = new KnModel("VoltageDividerCircuit");

        // Create Specifications component
        var specs = new KnComponent("Specifications");
        model.AddChildComponent<KnComponent>(specs);
        var inputVoltage = specs.Parameter("InputVoltage", 12.0, "V");
        var outputVoltage = specs.Parameter("OutputVoltage", 5.0, "V");
        var loadCurrent = specs.Parameter("LoadCurrent", 0.01, "A");

        // Create VoltageSource component
        var voltageSource = new KnComponent("VoltageSource");
        model.AddChildComponent<KnComponent>(voltageSource);
        var sourceVoltage = voltageSource.Parameter("Voltage", 12.0, "V");

        // Create Resistor1 component
        var resistor1 = new KnComponent("Resistor1");
        model.AddChildComponent<KnComponent>(resistor1);
        var r1 = resistor1.Parameter("Resistance", 1000.0, ""); // Ohms

        // Create Resistor2 component
        var resistor2 = new KnComponent("Resistor2");
        model.AddChildComponent<KnComponent>(resistor2);
        var r2 = resistor2.Parameter("Resistance", 714.0, ""); // Ohms

        Console.WriteLine("Model Structure Created:\n");
        Console.WriteLine($"  {model.Title}");
        Console.WriteLine($"    ├── {specs.Title}");
        Console.WriteLine($"    │   ├── InputVoltage = {inputVoltage.GetCurrentValue().Value()}");
        Console.WriteLine($"    │   ├── OutputVoltage = {outputVoltage.GetCurrentValue().Value()}");
        Console.WriteLine($"    │   └── LoadCurrent = {loadCurrent.GetCurrentValue().Value()}");
        Console.WriteLine($"    ├── {voltageSource.Title}");
        Console.WriteLine($"    │   └── Voltage = {sourceVoltage.GetCurrentValue().Value()}");
        Console.WriteLine($"    ├── {resistor1.Title}");
        Console.WriteLine($"    │   └── Resistance = {r1.GetCurrentValue().Value()}");
        Console.WriteLine($"    └── {resistor2.Title}");
        Console.WriteLine($"        └── Resistance = {r2.GetCurrentValue().Value()}");

        Console.WriteLine("\n=== Parameter Values ===\n");

        // Evaluate all parameters and show their values
        var inputVoltageResult = inputVoltage.GetCurrentValue();
        Console.WriteLine($"InputVoltage: {inputVoltageResult.Value()} ({inputVoltage.GetUnits()})");
        Console.WriteLine($"  Type: {inputVoltageResult.Value()?.GetType().Name}");

        var outputVoltageResult = outputVoltage.GetCurrentValue();
        Console.WriteLine($"\nOutputVoltage: {outputVoltageResult.Value()} ({outputVoltage.GetUnits()})");
        Console.WriteLine($"  Type: {outputVoltageResult.Value()?.GetType().Name}");

        var loadCurrentResult = loadCurrent.GetCurrentValue();
        Console.WriteLine($"\nLoadCurrent: {loadCurrentResult.Value()} ({loadCurrent.GetUnits()})");
        Console.WriteLine($"  Type: {loadCurrentResult.Value()?.GetType().Name}");

        // Now let's add some calculated parameters
        Console.WriteLine("\n=== Adding Calculated Parameters ===\n");

        // CRITICAL SYNTAX RULES FOR FORMULAS:
        // ✓ units(value, 'unit') - function syntax requires SINGLE QUOTES around unit
        // ✓ 'value unit'         - simple syntax if unit is registered (e.g., '12 V', '5 A')
        // ✗ units(value, unit)   - WRONG - no quotes
        // ✗ units(value, "unit") - WRONG - double quotes not supported

        // Total resistance - use simple names for formulas
        var totalResistance = specs.Parameter("TotalResistance", 0.0, "");
        totalResistance.ApplyFormula("1000 + 714", KnBase.UnitService);
        
        // Current through circuit - using correct units() syntax with single quotes
        var current = specs.Parameter("Current", 0.0, "A");
        current.ApplyFormula("units(12, 'V') / 1714", KnBase.UnitService);

        // Voltage across R2 (output voltage) - using simple formula syntax
        var vOut = specs.Parameter("CalculatedOutputVoltage", 0.0, "V");
        vOut.ApplyFormula("'12 V' * 714 / 1714", KnBase.UnitService);

        // Power in R1 - mixing both syntaxes (both valid)
        var powerR1 = resistor1.Parameter("Power", 0.0, "W");
        powerR1.ApplyFormula("units(0.007, 'A') * units(0.007, 'A') * 1000", KnBase.UnitService);

        // Power in R2 - using simple syntax
        var powerR2 = resistor2.Parameter("Power", 0.0, "W");
        powerR2.ApplyFormula("'0.007 A' * '0.007 A' * 714", KnBase.UnitService);

        // Trigger evaluation
        Console.WriteLine("Evaluating calculated parameters...\n");

        var totalResResult = totalResistance.GetCurrentValue();
        Console.WriteLine($"TotalResistance: {totalResResult.AsNumber()} Ω");

        var currentResult = current.GetCurrentValue();
        Console.WriteLine($"Current: {currentResult.AsNumber():F4} A");

        var vOutResult = vOut.GetCurrentValue();
        Console.WriteLine($"CalculatedOutputVoltage: {vOutResult.AsNumber():F2} V");

        var power1Result = powerR1.GetCurrentValue();
        Console.WriteLine($"Power(R1): {power1Result.AsNumber():F4} W");

        var power2Result = powerR2.GetCurrentValue();
        Console.WriteLine($"Power(R2): {power2Result.AsNumber():F4} W");

        Console.WriteLine("\n=== Verification ===\n");
        Console.WriteLine($"Expected Output: 5V, Calculated: {vOutResult.AsNumber():F2}V");
        Console.WriteLine($"Total Power: {(power1Result.AsNumber() + power2Result.AsNumber()):F4}W");
        
        Console.WriteLine("\n✓ Test Complete!");
    }

    /// <summary>
    /// Test unit validation on the voltage values
    /// </summary>
    public static void TestUnitValidation()
    {
        Console.WriteLine("\n=== Unit Validation Test ===\n");

        // Test the units from the image: 12|V, 5|V, 0.01|A
        var voltageValidation = FoundryMentorModeler.Shared.UnitValidationHelper.ValidateUnit("V");
        Console.WriteLine(FoundryMentorModeler.Shared.UnitValidationHelper.FormatValidationMessage(voltageValidation));

        var currentValidation = FoundryMentorModeler.Shared.UnitValidationHelper.ValidateUnit("A");
        Console.WriteLine(FoundryMentorModeler.Shared.UnitValidationHelper.FormatValidationMessage(currentValidation));

        // Test invalid unit
        var badValidation = FoundryMentorModeler.Shared.UnitValidationHelper.ValidateUnit("InvalidUnit");
        Console.WriteLine(FoundryMentorModeler.Shared.UnitValidationHelper.FormatValidationMessage(badValidation));

        Console.WriteLine("\n✓ Validation Complete!");
    }

    /// <summary>
    /// Run both tests
    /// </summary>
    public static void RunAll()
    {
        TestVoltageCircuit();
        Console.WriteLine("\n" + new string('=', 50) + "\n");
        TestUnitValidation();
    }
}
