using FoundryMentorModeler.Model;

namespace FoundryMentorModeler.Tests;

/// <summary>
/// Test PartComponent demonstrating LIST() and reduction operators
/// </summary>
public class BoxWithDimensions : PartComponent
{
    public BoxWithDimensions(string name = "BoxWithDimensions") : base(name)
    {
        Calculations([
            "width1: 10.0",
            "width2: 20.0",
            "width3: 30.0",
            "widths: LIST(width1@, width2@, width3@)",
            "total: SUM(widths)",
            "count: COUNT(widths)",
            "average: AVG(widths)",
            "minimum: MIN(widths)",
            "maximum: MAX(widths)",
            "first: FIRST(widths)",
            "last: LAST(widths)"
        ]);

        // Add success criteria
        FindParameter("total")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 60.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("count")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 3.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("average")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return Math.Abs(p.GetValue().AsNumber() - 20.0) < 0.001 ? CalculationStatus.Success : CalculationStatus.Failure;
        });
    }
}

/// <summary>
/// Box with empty collection for testing edge cases
/// </summary>
public class BoxWithEmptyList : PartComponent
{
    public BoxWithEmptyList(string name = "BoxWithEmptyList") : base(name)
    {
        Calculations([
            "empty: LIST()",
            "count: COUNT(empty)",
            "sum: SUM(empty)"
        ]);

        // Add success criteria for empty collection edge cases
        FindParameter("count")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 0.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("sum")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 0.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });
    }
}

/// <summary>
/// Box with single value
/// </summary>
public class BoxWithSingleValue : PartComponent
{
    public BoxWithSingleValue(string name = "BoxWithSingleValue") : base(name)
    {
        Calculations([
            "width: 42.0",
            "widths: LIST(width@)",
            "total: SUM(widths)",
            "average: AVG(widths)",
            "min: MIN(widths)",
            "max: MAX(widths)"
        ]);

        // Add success criteria
        FindParameter("total")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 42.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("average")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 42.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("min")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 42.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("max")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 42.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });
    }
}

/// <summary>
/// Box with many values
/// </summary>
public class BoxWithManyValues : PartComponent
{
    public BoxWithManyValues(string name = "BoxWithManyValues") : base(name)
    {
        Calculations([
            "v1: 5.0",
            "v2: 15.0",
            "v3: 25.0",
            "v4: 35.0",
            "v5: 45.0",
            "values: LIST(v1@, v2@, v3@, v4@, v5@)",
            "sum: SUM(values)",
            "count: COUNT(values)",
            "avg: AVG(values)",
            "min: MIN(values)",
            "max: MAX(values)",
            "first: FIRST(values)",
            "last: LAST(values)"
        ]);

        // Add success criteria for each parameter
        FindParameter("sum")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            return result.AsNumber() == 125.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("count")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            return result.AsNumber() == 5.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("avg")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            return Math.Abs(result.AsNumber() - 25.0) < 0.001 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("min")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            return result.AsNumber() == 5.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("max")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            return result.AsNumber() == 45.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("first")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            return result.AsNumber() == 5.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("last")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            return result.AsNumber() == 45.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });
    }
}

/// <summary>
/// Box with measured length values to test unit conversions with reduction operators
/// </summary>
public class BoxWithLengthMeasures : PartComponent
{
    public BoxWithLengthMeasures(string name = "BoxWithLengthMeasures") : base(name)
    {
        Calculations([
            "length1: units(10, 'ft')",
            "length2: units(5, 'm')",
            "length3: units(100, 'in')",
            "lengths: LIST(length1@, length2@, length3@)",
            "totalFeet: units(SUM(lengths), 'ft')",
            "totalMeters: units(SUM(lengths), 'm')",
            "count: COUNT(lengths)",
            "avgFeet: units(AVG(lengths), 'ft')",
            "minMeters: units(MIN(lengths), 'm')",
            "maxInches: units(MAX(lengths), 'in')"
        ]);

        // Add success criteria - testing unit conversions
        // 10 ft + 5 m (16.404 ft) + 100 in (8.333 ft) = ~34.737 ft
        FindParameter("totalFeet")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            if (!result.IsNumberWithUnits()) return CalculationStatus.Failure;
            var value = result.AsMeasuredValue().As("ft");
            return Math.Abs(value - 34.737) < 0.01 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        // Convert to meters: ~10.588 m
        FindParameter("totalMeters")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            if (!result.IsNumberWithUnits()) return CalculationStatus.Failure;
            var value = result.AsMeasuredValue().As("m");
            return Math.Abs(value - 10.588) < 0.01 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        FindParameter("count")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            return p.GetValue().AsNumber() == 3.0 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        // Average: ~11.579 ft
        FindParameter("avgFeet")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            if (!result.IsNumberWithUnits()) return CalculationStatus.Failure;
            var value = result.AsMeasuredValue().As("ft");
            return Math.Abs(value - 11.579) < 0.01 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        // Minimum is 5 m
        FindParameter("minMeters")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            if (!result.IsNumberWithUnits()) return CalculationStatus.Failure;
            var value = result.AsMeasuredValue().As("m");
            return Math.Abs(value - 5.0) < 0.01 ? CalculationStatus.Success : CalculationStatus.Failure;
        });

        // Maximum is 5 m = ~196.85 in
        FindParameter("maxInches")?.WithSuccessTest(p => {
            if (!p.HasCalculatedValue()) return CalculationStatus.NotCalculated;
            var result = p.GetValue();
            if (!result.IsNumberWithUnits()) return CalculationStatus.Failure;
            var value = result.AsMeasuredValue().As("in");
            return Math.Abs(value - 196.85) < 0.5 ? CalculationStatus.Success : CalculationStatus.Failure;
        });
    }
}
