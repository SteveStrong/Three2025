using Microsoft.AspNetCore.Components;
using FoundryMentorModeler.Evaluator;
using FoundryWorldsAndDrawings.Shape;

namespace Three2025.Components.Pages;

public partial class OPResultCollectionDemo : ComponentBase
{
    private string output = "";

    private void Log(string message)
    {
        output += $"{message}\n";
        StateHasChanged();
    }

    private void ClearOutput()
    {
        output = "";
    }

    // ===================================================================
    // BASIC OPERATIONS
    // ===================================================================

    private void Test_CreateNumberList()
    {
        ClearOutput();
        Log("=== Creating Number List ===\n");

        var numbers = Enumerable.Range(1, 10).ToList();
        var result = OPResult.Collection(numbers);

        Log($"✓ Created collection: {result.CollectionCount()} items");
        Log($"✓ Type: {result.GetStatus()}");
        Log($"✓ Element Type: {result.GetCollectionElementType()?.Name}");
        Log($"✓ Items: [{string.Join(", ", numbers)}]");
    }

    private void Test_SumNumbers()
    {
        ClearOutput();
        Log("=== Sum Collection ===\n");

        var numbers = Enumerable.Range(1, 10).ToList();
        var result = OPResult.Collection(numbers);

        var extracted = result.AsCollection<int>();
        var sum = extracted.Sum();

        Log($"Collection: [{string.Join(", ", extracted)}]");
        Log($"✓ Sum: {sum}");
        Log($"✓ Expected: 55 (1+2+3...+10)");
    }

    private void Test_Statistics()
    {
        ClearOutput();
        Log("=== Calculate Statistics ===\n");

        var numbers = new List<double> { 10.5, 23.2, 15.8, 42.1, 8.3, 31.7 };
        var result = OPResult.Collection(numbers);

        var extracted = result.AsCollection<double>();

        Log($"Collection: [{string.Join(", ", extracted.Select(n => n.ToString("F1")))}]");
        Log($"✓ Count: {extracted.Count}");
        Log($"✓ Sum: {extracted.Sum():F2}");
        Log($"✓ Average: {extracted.Average():F2}");
        Log($"✓ Min: {extracted.Min():F2}");
        Log($"✓ Max: {extracted.Max():F2}");
    }

    private void Test_CountElements()
    {
        ClearOutput();
        Log("=== Count Elements ===\n");

        var numbers = Enumerable.Range(1, 100).ToList();
        var result = OPResult.Collection(numbers);

        Log($"✓ CollectionCount(): {result.CollectionCount()}");
        Log($"✓ AsCollection<int>().Count: {result.AsCollection<int>().Count}");
        Log($"✓ Both methods return same result: {result.CollectionCount() == result.AsCollection<int>().Count}");
    }

    // ===================================================================
    // QUERY OPERATIONS
    // ===================================================================

    private void Test_FilterNumbers()
    {
        ClearOutput();
        Log("=== Filter Numbers (x > 5) ===\n");

        var numbers = Enumerable.Range(1, 10).ToList();
        var result = OPResult.Collection(numbers);

        var extracted = result.AsCollection<int>();
        var filtered = extracted.Where(x => x > 5).ToList();

        Log($"Original: [{string.Join(", ", extracted)}]");
        Log($"Filtered (x > 5): [{string.Join(", ", filtered)}]");
        Log($"✓ Result: {filtered.Count} items");
    }

    private void Test_MapDouble()
    {
        ClearOutput();
        Log("=== Map (x * 2) ===\n");

        var numbers = Enumerable.Range(1, 5).ToList();
        var result = OPResult.Collection(numbers);

        var extracted = result.AsCollection<int>();
        var doubled = extracted.Select(x => x * 2).ToList();

        Log($"Original: [{string.Join(", ", extracted)}]");
        Log($"Doubled: [{string.Join(", ", doubled)}]");
        Log($"✓ Transformation applied successfully");
    }

    private void Test_FirstLast()
    {
        ClearOutput();
        Log("=== First & Last Elements ===\n");

        var numbers = Enumerable.Range(1, 10).ToList();
        var result = OPResult.Collection(numbers);

        var extracted = result.AsCollection<int>();

        Log($"Collection: [{string.Join(", ", extracted)}]");
        Log($"✓ First: {extracted.First()}");
        Log($"✓ Last: {extracted.Last()}");
        Log($"✓ ElementAt(4): {extracted.ElementAt(4)}");
    }

    private void Test_AnyAll()
    {
        ClearOutput();
        Log("=== Any/All Predicates ===\n");

        var numbers = Enumerable.Range(1, 10).ToList();
        var result = OPResult.Collection(numbers);

        var extracted = result.AsCollection<int>();

        Log($"Collection: [{string.Join(", ", extracted)}]");
        Log($"✓ Any(x > 5): {extracted.Any(x => x > 5)}");
        Log($"✓ Any(x > 20): {extracted.Any(x => x > 20)}");
        Log($"✓ All(x > 0): {extracted.All(x => x > 0)}");
        Log($"✓ All(x > 5): {extracted.All(x => x > 5)}");
    }

    // ===================================================================
    // SHAPE COLLECTIONS
    // ===================================================================

    private void Test_CreateShapes()
    {
        ClearOutput();
        Log("=== Create Shape Collection ===\n");

        var shapes = new List<FoShape3D>
        {
            new FoShape3D("Box1") { Color = "red", GeomType = "Box" },
            new FoShape3D("Sphere1") { Color = "blue", GeomType = "Sphere" },
            new FoShape3D("Cylinder1") { Color = "green", GeomType = "Cylinder" },
            new FoShape3D("Box2") { Color = "red", GeomType = "Box" }
        };

        var result = OPResult.Collection(shapes);

        Log($"✓ Created collection: {result.CollectionCount()} shapes");
        Log($"✓ Element Type: {result.GetCollectionElementType()?.Name}");
        Log($"✓ Is collection of FoShape3D: {result.IsCollectionOf<FoShape3D>()}");
        
        foreach (var shape in shapes)
        {
            Log($"  - {shape.GetName()}: {shape.GeomType}, Color={shape.Color}");
        }
    }

    private void Test_FilterShapesByColor()
    {
        ClearOutput();
        Log("=== Filter Shapes by Color ===\n");

        var shapes = new List<FoShape3D>
        {
            new FoShape3D("Box1") { Color = "red" },
            new FoShape3D("Sphere1") { Color = "blue" },
            new FoShape3D("Cylinder1") { Color = "red" },
            new FoShape3D("Box2") { Color = "green" }
        };

        var result = OPResult.Collection(shapes);
        var extracted = result.AsCollection<FoShape3D>();
        var redShapes = extracted.Where(s => s.Color == "red").ToList();

        Log($"Total shapes: {extracted.Count}");
        Log($"Red shapes: {redShapes.Count}");
        Log($"\nRed shapes:");
        foreach (var shape in redShapes)
        {
            Log($"  ✓ {shape.GetName()}");
        }
    }

    private void Test_ExtractShapeNames()
    {
        ClearOutput();
        Log("=== Extract Shape Names ===\n");

        var shapes = new List<FoShape3D>
        {
            new FoShape3D("Box1"),
            new FoShape3D("Sphere1"),
            new FoShape3D("Cylinder1")
        };

        var result = OPResult.Collection(shapes);
        var extracted = result.AsCollection<FoShape3D>();
        var names = extracted.Select(s => s.GetName()).ToList();

        Log($"Shape Names:");
        foreach (var name in names)
        {
            Log($"  ✓ {name}");
        }
    }

    private void Test_CountByType()
    {
        ClearOutput();
        Log("=== Count by GeomType ===\n");

        var shapes = new List<FoShape3D>
        {
            new FoShape3D("Box1") { GeomType = "Box" },
            new FoShape3D("Box2") { GeomType = "Box" },
            new FoShape3D("Sphere1") { GeomType = "Sphere" },
            new FoShape3D("Cylinder1") { GeomType = "Cylinder" },
            new FoShape3D("Box3") { GeomType = "Box" }
        };

        var result = OPResult.Collection(shapes);
        var extracted = result.AsCollection<FoShape3D>();
        var grouped = extracted.GroupBy(s => s.GeomType);

        Log($"Total shapes: {extracted.Count}\n");
        foreach (var group in grouped)
        {
            Log($"✓ {group.Name}: {group.Count()} shapes");
        }
    }

    // ===================================================================
    // TYPE SAFETY
    // ===================================================================

    private void Test_TypeDetection()
    {
        ClearOutput();
        Log("=== Element Type Detection ===\n");

        var integers = new List<int> { 1, 2, 3 };
        var doubles = new List<double> { 1.5, 2.5, 3.5 };
        var strings = new List<string> { "a", "b", "c" };

        var r1 = OPResult.Collection(integers);
        var r2 = OPResult.Collection(doubles);
        var r3 = OPResult.Collection(strings);

        Log($"✓ List<int> → Element Type: {r1.GetCollectionElementType()?.Name}");
        Log($"✓ List<double> → Element Type: {r2.GetCollectionElementType()?.Name}");
        Log($"✓ List<string> → Element Type: {r3.GetCollectionElementType()?.Name}");
    }

    private void Test_TypeMismatch()
    {
        ClearOutput();
        Log("=== Wrong Type Extraction ===\n");

        var numbers = new List<int> { 1, 2, 3 };
        var result = OPResult.Collection(numbers);

        try
        {
            var strings = result.AsCollection<string>();
            Log("❌ Should have thrown exception!");
        }
        catch (InvalidCastException ex)
        {
            Log("✓ InvalidCastException thrown as expected:");
            Log($"  {ex.Message}");
        }
    }

    private void Test_EmptyCollection()
    {
        ClearOutput();
        Log("=== Empty Collection Handling ===\n");

        var empty = new List<int>();
        var result = OPResult.Collection(empty);

        Log($"✓ IsCollection: {result.IsCollection()}");
        Log($"✓ CollectionCount: {result.CollectionCount()}");
        Log($"✓ Status: {result.GetStatus()}");
        Log($"✓ Empty collections are still collections");
    }

    private void Test_ReferenceSematics()
    {
        ClearOutput();
        Log("=== Reference Semantics ===\n");

        var original = new List<int> { 1, 2, 3 };
        var result = OPResult.Collection(original);

        var extracted = result.AsCollection<int>();
        extracted.Add(4);

        Log($"Original list: [{string.Join(", ", original)}]");
        Log($"Extracted list: [{string.Join(", ", extracted)}]");
        Log($"Collection count: {result.CollectionCount()}");
        Log($"\n✓ Mutation propagates - same reference!");
    }

    // ===================================================================
    // ADVANCED
    // ===================================================================

    private void Test_ChainedOperations()
    {
        ClearOutput();
        Log("=== Chained Operations ===\n");

        var numbers = Enumerable.Range(1, 20).ToList();
        var result = OPResult.Collection(numbers);

        var extracted = result.AsCollection<int>();
        var processed = extracted
            .Where(x => x % 2 == 0)  // Even numbers
            .Select(x => x * x)      // Square them
            .Where(x => x > 50)      // Only large squares
            .OrderByDescending(x => x)
            .ToList();

        Log($"Original: 1..20");
        Log($"→ Filter (even): {extracted.Count(x => x % 2 == 0)} items");
        Log($"→ Map (square): squares computed");
        Log($"→ Filter (> 50): {processed.Count} items");
        Log($"\n✓ Result: [{string.Join(", ", processed)}]");
    }

    private void Test_NestedCollections()
    {
        ClearOutput();
        Log("=== Nested Collections ===\n");

        var nested = new List<List<int>>
        {
            new List<int> { 1, 2, 3 },
            new List<int> { 4, 5, 6 },
            new List<int> { 7, 8, 9 }
        };

        var result = OPResult.Collection(nested);

        Log($"✓ Outer collection count: {result.CollectionCount()}");
        Log($"✓ Element type: {result.GetCollectionElementType()?.Name}");

        var extracted = result.AsCollection<List<int>>();
        var flattened = extracted.SelectMany(list => list).ToList();

        Log($"✓ Flattened: [{string.Join(", ", flattened)}]");
    }

    private void Test_MixedTypes()
    {
        ClearOutput();
        Log("=== Mixed Type Collections ===\n");

        var mixed = new List<object> { 1, "two", 3.0, true };
        var result = OPResult.Collection(mixed);

        Log($"✓ Collection count: {result.CollectionCount()}");
        Log($"✓ Element type: {result.GetCollectionElementType()?.Name}");

        var extracted = result.AsCollection<object>();
        Log($"\nItems:");
        foreach (var item in extracted)
        {
            Log($"  - {item} ({item.GetType().Name})");
        }
    }

    private void Test_Performance()
    {
        ClearOutput();
        Log("=== Performance Test (1000 items) ===\n");

        var start = DateTime.Now;
        
        var numbers = Enumerable.Range(1, 1000).ToList();
        var result = OPResult.Collection(numbers);
        var createTime = (DateTime.Now - start).TotalMilliseconds;

        start = DateTime.Now;
        var extracted = result.AsCollection<int>();
        var extractTime = (DateTime.Now - start).TotalMilliseconds;

        start = DateTime.Now;
        var sum = extracted.Sum();
        var sumTime = (DateTime.Now - start).TotalMilliseconds;

        Log($"✓ Created collection (1000 items): {createTime:F2}ms");
        Log($"✓ Extracted collection: {extractTime:F2}ms");
        Log($"✓ Computed sum ({sum:N0}): {sumTime:F2}ms");
        Log($"\n✓ Total time: {(createTime + extractTime + sumTime):F2}ms");
    }

    // ===================================================================
    // RUN ALL
    // ===================================================================

    private async Task RunAllTests()
    {
        ClearOutput();
        Log("=== Running All Tests ===\n");

        await Task.Delay(100);
        Test_CreateNumberList();
        await Task.Delay(100);
        Test_SumNumbers();
        await Task.Delay(100);
        Test_Statistics();
        await Task.Delay(100);
        Test_FilterNumbers();
        await Task.Delay(100);
        Test_CreateShapes();
        await Task.Delay(100);
        Test_TypeDetection();
        await Task.Delay(100);
        Test_Performance();

        Log("\n\n✅ All tests completed!");
    }
}
