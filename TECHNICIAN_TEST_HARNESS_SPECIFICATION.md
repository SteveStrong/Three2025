# Technician Test Harness Specification

## Vision

A **zero-maintenance testing infrastructure** that automatically generates interactive test UIs for all ITechnician implementations by reading the same `[Description]` metadata that AI agents use. When a developer adds a new tool method, the test UI instantly reflects it—no additional code required.

---

## Core Principle

> **Human sees what LLM sees**

The test harness uses reflection to discover tool methods and their metadata, presenting the exact same information to human testers that the AI receives. This ensures:
- Perfect transparency in AI behavior
- Self-documenting test interfaces
- Zero duplication of metadata
- Automatic scaling as tools evolve

---

## Architecture

### 1. **Generic Test Panel Component** (Reusable)

**File**: `Components/Shared/Testing/TechnicianTestPanel<T>.razor`

A **generic Blazor component** that works with ANY `ITechnician` implementation:

```razor
@typeparam TTechnician where TTechnician : ITechnician

<TechnicianTestPanel TechnicianType="typeof(IGeometryTech)" />
<TechnicianTestPanel TechnicianType="typeof(IModelTech)" />
<TechnicianTestPanel TechnicianType="typeof(IClockTech)" />
```

**Responsibilities**:
- Use reflection to discover all methods with `[Description]` attribute
- Extract method name, description, parameters, and their descriptions
- Dynamically render a button for each discovered method
- Execute methods with test parameters
- Display results and logs
- Works with any ITechnician without modification

### 2. **Test Value Provider** (Strategy Pattern)

**File**: `Services/Testing/ITestValueProvider.cs`

Provides test parameter values for method execution:

```csharp
public interface ITestValueProvider
{
    object GetTestValue(ParameterInfo parameter, MethodInfo method);
}

// Default implementation
public class DefaultTestValueProvider : ITestValueProvider
{
    // Strategy: Use sensible defaults based on parameter name/type
    // Example: "name" param → "Test{ShapeType}", "x/y/z" → 0, "color" → "red"
}

// Custom implementation for specific scenarios
public class RandomTestValueProvider : ITestValueProvider
{
    // Generates random valid values for variety testing
}
```

### 3. **Test Execution Service** (Orchestration)

**File**: `Services/Testing/TechnicianTestExecutor.cs`

Handles method invocation via reflection:

```csharp
public class TechnicianTestExecutor
{
    public async Task<TestResult> ExecuteToolMethod(
        ITechnician technician,
        MethodInfo method,
        ITestValueProvider valueProvider)
    {
        // 1. Get test values for all parameters
        // 2. Invoke method via reflection
        // 3. Capture result and any exceptions
        // 4. Return structured TestResult
    }
}

public class TestResult
{
    public bool Success { get; set; }
    public string MethodName { get; set; }
    public object? ReturnValue { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> ParametersUsed { get; set; }
    public TimeSpan ExecutionTime { get; set; }
}
```

### 4. **Tool Metadata Extractor** (Shared with AI)

**File**: `Services/Testing/ToolMetadataExtractor.cs`

Mirrors the logic in `TechnicianToolProvider` but returns metadata for UI rendering:

```csharp
public class ToolMetadataExtractor
{
    public List<ToolMethodMetadata> ExtractToolMethods(Type technicianInterface)
    {
        // Find all methods with [Description] attribute
        // Extract method description, parameter descriptions, return type
        // Return structured metadata for UI rendering
    }
}

public class ToolMethodMetadata
{
    public string MethodName { get; set; }
    public string Description { get; set; }
    public List<ParameterMetadata> Parameters { get; set; }
    public string ReturnType { get; set; }
    public MethodInfo MethodInfo { get; set; }
}

public class ParameterMetadata
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public object? DefaultValue { get; set; }
    public bool IsOptional { get; set; }
}
```

---

## UI Component Design

### TechnicianTestPanel.razor

**Props**:
```csharp
[Parameter] public Type TechnicianType { get; set; } // e.g., typeof(IGeometryTech)
[Parameter] public string Title { get; set; } = "Tool Test Panel"
[Parameter] public bool ShowParameterDetails { get; set; } = true
[Parameter] public bool GroupByCategory { get; set; } = false
[Parameter] public ITestValueProvider? CustomValueProvider { get; set; }
```

**Layout**:
```
┌─────────────────────────────────────────────────┐
│ 🧪 GeometryTech Test Panel                    │
│ 19 tool methods discovered                     │
├─────────────────────────────────────────────────┤
│ Setup & Metadata                                │
│ ├─ [RefreshUI] Send message to refresh TreeView│
│ ├─ [EstablishGeometryStage] Initialize stage   │
│ └─ [GetToolCapabilities] Get tool documentation│
│                                                 │
│ Persistence                                     │
│ ├─ [ClearShapes] Remove all shapes             │
│ ├─ [SaveShapes] Save shapes to file            │
│ └─ [RestoreShapes] Load shapes from file       │
│                                                 │
│ Shape Creation                                  │
│ ├─ [AddShape] ⓘ Create and add 3D shape       │
│ │   ├─ name: "TestBox" (name of shape)        │
│ │   ├─ isOn: true (visibility)                │
│ │   ├─ color: "red" (shape color)             │
│ │   ├─ shapeType: "box" (box, sphere, etc.)   │
│ │   └─ x,y,z: 0,0,0 (position)                │
│ └─ [AddShapeWithDimensions] ⓘ Create with size│
│                                                 │
│ [ ▶ Run All Tests ]  [ 🔄 Reset Stage ]       │
├─────────────────────────────────────────────────┤
│ Test Results Log:                               │
│ ✓ AddShape: Success (47ms)                     │
│   → Returned 1 shape(s)                        │
│ ✓ RepositionShape: Success (12ms)              │
│   → Shape moved to (10, 5, -3)                 │
│ ✗ DeleteShape: Failed (5ms)                    │
│   → Error: Shape "NonExistent" not found       │
└─────────────────────────────────────────────────┘
```

**Interaction**:
- Click individual button → Executes that one method
- Hover over ⓘ icon → Shows detailed parameter info
- "Run All Tests" → Executes all methods in sequence
- Results appear in log at bottom
- 3D canvas updates in real-time as methods execute

---

## Implementation Flow

### Step 1: Component Initialization

```csharp
protected override async Task OnInitializedAsync()
{
    // 1. Resolve ITechnician from DI based on TechnicianType
    technician = ServiceProvider.GetService(TechnicianType);
    
    // 2. Extract tool metadata via reflection
    toolMethods = MetadataExtractor.ExtractToolMethods(TechnicianType);
    
    // 3. Group by category if requested (optional)
    if (GroupByCategory)
        groupedMethods = GroupMethodsByCategory(toolMethods);
    
    // 4. Initialize test value provider
    valueProvider = CustomValueProvider ?? new DefaultTestValueProvider();
}
```

### Step 2: Button Click Handler

```csharp
private async Task ExecuteMethod(ToolMethodMetadata methodMeta)
{
    AddLog($"Executing: {methodMeta.MethodName}...");
    
    var result = await TestExecutor.ExecuteToolMethod(
        technician, 
        methodMeta.MethodInfo, 
        valueProvider
    );
    
    if (result.Success)
    {
        AddLog($"✓ {methodMeta.MethodName}: Success ({result.ExecutionTime.TotalMilliseconds}ms)");
        if (result.ReturnValue != null)
            AddLog($"  → Returned: {FormatReturnValue(result.ReturnValue)}");
    }
    else
    {
        AddLog($"✗ {methodMeta.MethodName}: Failed");
        AddLog($"  → Error: {result.ErrorMessage}");
    }
    
    StateHasChanged();
}
```

### Step 3: Test Value Generation

```csharp
public class DefaultTestValueProvider : ITestValueProvider
{
    public object GetTestValue(ParameterInfo parameter, MethodInfo method)
    {
        var paramName = parameter.Name.ToLower();
        var paramType = parameter.ParameterType;
        
        // Strategy based on parameter name
        if (paramName == "name")
            return $"Test{method.Name}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        
        if (paramName == "color")
            return new[] { "red", "blue", "green", "yellow" }[Random.Shared.Next(4)];
        
        if (paramName == "ison" || paramName == "visible")
            return true;
        
        if (paramName.Contains("x") || paramName.Contains("y") || paramName.Contains("z"))
            return 0.0;
        
        // Fallback to type defaults
        if (paramType == typeof(string))
            return "TestValue";
        if (paramType == typeof(bool))
            return true;
        if (paramType == typeof(int) || paramType == typeof(double))
            return 0;
        
        return parameter.DefaultValue ?? Activator.CreateInstance(paramType);
    }
}
```

---

## Integration Patterns

### Pattern 1: Dedicated Test Page

```razor
@page "/geometry-tests"

<TechnicianTestPanel TechnicianType="typeof(IGeometryTech)" 
                     Title="🔺 Geometry Tool Tests" />
```

### Pattern 2: Tabbed Within Existing Page

```razor
<RadzenSplitterPane Size="25%">
    <ul class="nav nav-tabs">
        <li><button @onclick='() => activeTab = "chat"'>💬 Chat</button></li>
        <li><button @onclick='() => activeTab = "test"'>🧪 Test</button></li>
    </ul>
    
    @if (activeTab == "chat")
    {
        <ChatPanel ... />
    }
    else
    {
        <TechnicianTestPanel TechnicianType="typeof(IGeometryTech)" />
    }
</RadzenSplitterPane>
```

### Pattern 3: Modal/Popup

```razor
<RadzenDialog>
    <TechnicianTestPanel TechnicianType="typeof(IGeometryTech)" 
                         ShowParameterDetails="false" />
</RadzenDialog>
```

### Pattern 4: Multi-Technician Dashboard

```razor
<RadzenTabs>
    <Tabs>
        <RadzenTabsItem Text="🔺 Geometry">
            <TechnicianTestPanel TechnicianType="typeof(IGeometryTech)" />
        </RadzenTabsItem>
        <RadzenTabsItem Text="📐 Model">
            <TechnicianTestPanel TechnicianType="typeof(IModelTech)" />
        </RadzenTabsItem>
        <RadzenTabsItem Text="⏰ Clock">
            <TechnicianTestPanel TechnicianType="typeof(IClockTech)" />
        </RadzenTabsItem>
    </Tabs>
</RadzenTabs>
```

---

## Adding New Tool Methods

**Developer workflow**:

1. Add method to ITechnician interface:
```csharp
public interface IGeometryTech : ITechnician
{
    [Description("Explode a shape into particles")]
    List<ShapeInfo> ExplodeShape(
        [Description("Name of shape to explode")] string name,
        [Description("Number of particles")] int particleCount = 50);
}
```

2. Implement in technician class:
```csharp
public class GeometryTech : IGeometryTech
{
    [Description("Explode a shape into particles")]
    public List<ShapeInfo> ExplodeShape(string name, int particleCount = 50)
    {
        // Implementation
    }
}
```

3. **Test UI automatically updates** - no additional work required!
   - New button appears: `[ExplodeShape] Explode a shape into particles`
   - Parameter info shows: `name: "TestExplodeShape_xxxxx"`, `particleCount: 50`
   - Click button to test

4. AI agents automatically get the new tool - no additional work required!

---

## Advanced Features

### Custom Test Scenarios

```csharp
public class GeometryTestScenarios : ITestValueProvider
{
    private int _testCase = 0;
    
    public object GetTestValue(ParameterInfo parameter, MethodInfo method)
    {
        // Cycle through different test cases
        if (method.Name == "AddShape" && parameter.Name == "shapeType")
        {
            var shapes = new[] { "box", "sphere", "cylinder", "cone" };
            return shapes[_testCase++ % shapes.Length];
        }
        
        // Default behavior
        return DefaultTestValueProvider.GetTestValue(parameter, method);
    }
}
```

### Automated Test Sequences

```csharp
// Run a coordinated sequence of tests
await RunTestSequence(new[]
{
    ("EstablishGeometryStage", null),
    ("AddShape", new { shapeType = "box" }),
    ("RepositionShape", new { x = 5.0, y = 2.0, z = 0.0 }),
    ("RotateShape", new { xDegrees = 45.0 }),
    ("ChangeColor", new { color = "blue" }),
    ("DeleteShape", null)
});
```

### Performance Profiling

```csharp
// Track execution times across all tests
var performanceReport = TestExecutor.GeneratePerformanceReport();
// Slowest: ChangeShapeDimensions (47ms avg)
// Fastest: GetShapes (3ms avg)
// Total: 234ms for 19 methods
```

### Regression Detection

```csharp
// Compare current results against baseline
var baseline = LoadBaseline("GeometryTech_v1.2.json");
var current = await RunAllTests();
var regressions = CompareResults(baseline, current);
// Detected: RotateShape now throws exception on negative angles
```

---

## Benefits Summary

### For Developers
✅ **Zero maintenance** - add method, test UI appears  
✅ **No test code to write** - reflection handles everything  
✅ **Instant feedback** - click button, see result  
✅ **Portable** - drop component anywhere  

### For Testers
✅ **Visual verification** - see 3D canvas change in real-time  
✅ **Interactive debugging** - click individual methods  
✅ **Self-documenting** - UI shows exactly what LLM sees  
✅ **Reproducible** - same test parameters every time  

### For System Architecture
✅ **Single source of truth** - `[Description]` attributes drive everything  
✅ **Scales automatically** - works with any ITechnician  
✅ **Consistency guaranteed** - human and AI see same metadata  
✅ **Future-proof** - new technicians work immediately  

---

## File Structure

```
Three2025/
├─ Components/
│  └─ Shared/
│     └─ Testing/
│        ├─ TechnicianTestPanel.razor          (Generic test UI)
│        ├─ TechnicianTestPanel.razor.cs       (Code-behind)
│        └─ TestResultLog.razor                (Results display)
│
├─ Services/
│  └─ Testing/
│     ├─ ITestValueProvider.cs                 (Value strategy interface)
│     ├─ DefaultTestValueProvider.cs           (Default strategy)
│     ├─ TechnicianTestExecutor.cs             (Execution service)
│     └─ ToolMetadataExtractor.cs              (Reflection service)
│
├─ Models/
│  └─ Testing/
│     ├─ TestResult.cs                         (Result DTO)
│     ├─ ToolMethodMetadata.cs                 (Metadata DTO)
│     └─ ParameterMetadata.cs                  (Parameter DTO)
│
└─ Components/Pages/
   ├─ AgentCanvasIntegration.razor             (Tab 2: Test Panel)
   └─ TechnicianTestsDashboard.razor           (Dedicated test page)
```

---

## DI Registration

```csharp
// Program.cs
builder.Services.AddScoped<ToolMetadataExtractor>();
builder.Services.AddScoped<TechnicianTestExecutor>();
builder.Services.AddScoped<ITestValueProvider, DefaultTestValueProvider>();
```

---

## Success Criteria

✅ Adding a new `[Description]` method to any ITechnician automatically:
   - Appears in test UI within 1 second of page load
   - Shows correct parameter metadata from attributes
   - Executes successfully when clicked
   - Uses sensible test values without configuration

✅ Human testers can:
   - Understand what each method does (same as LLM)
   - See what parameters will be used (same as LLM)
   - Execute methods individually or in sequence
   - Verify 3D canvas updates match expectations

✅ Zero code changes required when:
   - New technician is added
   - Existing method signature changes
   - New parameters are added with defaults
   - Method descriptions are updated

---

## Future Enhancements

### Phase 2: Editable Parameters
Allow users to override test values in UI:
```
[AddShape] ⚙️ Configure
  name: [TestBox_______] 🔄 Randomize
  color: [red    ▼]
  x: [0.0____] y: [0.0____] z: [0.0____]
  [ Execute ]
```

### Phase 3: Test Recording
Record successful test sequences for replay:
```
[💾 Save as "Create Red Box"] 
[▶️ Replay "Complex Scene Setup"]
```

### Phase 4: AI-Assisted Testing
Use LLM to generate interesting test scenarios:
```
"Generate test sequence to verify shape collision detection"
→ AI creates: AddShape, Reposition overlap, check state
```

---

## Conclusion

This specification defines a **self-maintaining test infrastructure** that eliminates the traditional burden of writing and updating test code. By leveraging the same reflection and metadata that powers AI tool discovery, we create a system where:

> **The tool IS the test**

When developers focus on writing good tool methods with clear descriptions for the AI, they simultaneously create comprehensive, interactive test harnesses for human verification—**without writing a single line of test code**.
