# Tree Node Title Migration Plan
## From Virtual Method Overrides to Function-Based Formatters

### Overview
This document outlines the multiphase migration from virtual method overrides (`GetTreeNodeTitle()`) to function-based title formatters (`ComputeTreeNodeTitle`). The goal is to eliminate inheritance-based customization in favor of composition, following the same pattern used for animation behaviors.

### Current State
Classes override `GetTreeNodeTitle()` virtual method:
- `GeometryShape`: Position-based formatting
- `AudioPanelShape`: Feature-based formatting  
- `FoGlyph3D`: Technical geometry formatting
- Various model classes: Count/structure-based formatting

### Target State
Classes use static default formatters with optional per-instance overrides:
```csharp
public static readonly Func<FoGlyph, string> DefaultFormatter = g => /* format logic */;
shape.ComputeTreeNodeTitle = DefaultFormatter; // or custom override
```

---

## Phase 1: Add Function Property to Base Class
**Goal**: Add the new property alongside existing virtual methods (no breaking changes)

### 1.1 Modify Base Class (FoGlyph or FoComponent)
```csharp
public class FoGlyph // or appropriate base class
{
    /// <summary>
    /// Function to compute tree node title. When set, overrides virtual GetTreeNodeTitle()
    /// </summary>
    public Func<FoGlyph, string>? ComputeTreeNodeTitle { get; set; }
    
    /// <summary>
    /// Gets tree node title - uses ComputeTreeNodeTitle if set, otherwise calls virtual method
    /// </summary>
    public string GetTreeNodeTitle()
    {
        return ComputeTreeNodeTitle?.Invoke(this) ?? GetTreeNodeTitleVirtual();
    }
    
    /// <summary>
    /// Virtual method for backward compatibility - will be removed in final phase
    /// </summary>
    protected virtual string GetTreeNodeTitleVirtual()
    {
        return GetName(); // Simple default
    }
}
```

### 1.2 Update All Existing Overrides
Change all `public override string GetTreeNodeTitle()` to `protected override string GetTreeNodeTitleVirtual()`

**Files to Update**:
- `FoGlyph3D.cs`
- `GeometryShape.cs`
- `AudioPanelShape.cs`
- All model classes with overrides

### 1.3 Test Phase 1
- All existing functionality should work unchanged
- Tree views should display exactly as before
- New property defaults to null, falls back to virtual methods

---

## Phase 2: Create Static Formatters
**Goal**: Create static default formatters for each class type

### 2.1 Add Static Formatters to Classes
```csharp
public class GeometryShape : FoShape3D
{
    public static readonly Func<FoGlyph, string> DefaultFormatter = g => 
    {
        if (g is FoShape3D shape && shape.Transform?.Position != null)
        {
            var pos = shape.Transform.Position;
            return $"{g.GetName()} {g.Color} @ {pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0}";
        }
        return $"{g.GetName()} {g.Color}";
    };
    
    // Keep existing virtual override for now
    protected override string GetTreeNodeTitleVirtual()
    {
        var pos = Transform!.Position;
        return $"{GetName()} {Color} @ {pos.X:0.0}, {pos.Y:0.0}, {pos.Z:0.0}";
    }
}
```

### 2.2 Add Formatters to Other Classes
- `AudioPanelShape.DefaultFormatter`
- `FoGlyph3D.DefaultFormatter` (technical format)
- Model class formatters (count-based, value-based, etc.)

### 2.3 Test Phase 2
- Static formatters created but not yet used
- Existing virtual methods still in control
- Can manually test formatters: `GeometryShape.DefaultFormatter(shape)`

---

## Phase 3: Migrate Classes One-by-One
**Goal**: Replace virtual overrides with static formatter assignments

### 3.1 Start with Simple Classes
Update constructors to use static formatters:
```csharp
public GeometryShape(string name, ...) : base(name)
{
    // Set the formatter instead of relying on virtual override
    ComputeTreeNodeTitle = DefaultFormatter;
    
    // ... rest of constructor logic
}

// Remove the virtual override entirely
// protected override string GetTreeNodeTitleVirtual() <- DELETE THIS
```

### 3.2 Update Shape3DEditor
When creating shapes dynamically:
```csharp
public OPResult AddShape(string name, string color, string shapeType, ...)
{
    var shape = factory(name, w, h, d);
    
    // Set appropriate formatter based on shape type or context
    shape.ComputeTreeNodeTitle = GeometryShape.DefaultFormatter;
    
    // Optional: Add text tag logic here too
    AddTextTag(shape);
    
    return result;
}
```

### 3.3 Migration Order
1. **GeometryShape** (simplest case)
2. **AudioPanelShape** (known custom format)
3. **FoGlyph3D** (affects many shapes)
4. **Model classes** (count-based formatters)

### 3.4 Test Each Migration
After each class migration:
- Verify tree display unchanged
- Test dynamic formatter overrides work
- Ensure no performance regression

---

## Phase 4: Remove Virtual Method Infrastructure
**Goal**: Clean up virtual method once all classes migrated

### 4.1 Remove Virtual Method
```csharp
public class FoGlyph
{
    public Func<FoGlyph, string> ComputeTreeNodeTitle { get; set; } = 
        g => g.GetName(); // Inline default
    
    public string GetTreeNodeTitle()
    {
        return ComputeTreeNodeTitle(this);
    }
    
    // Remove GetTreeNodeTitleVirtual() entirely
}
```

### 4.2 Update All Remaining References
- Search for `GetTreeNodeTitleVirtual` and remove
- Ensure all classes now use static formatters
- Verify no compilation errors

### 4.3 Test Phase 4
- All tree functionality works via functions
- No virtual method overhead
- Per-instance overrides functional

---

## Phase 5: Simplify and Eliminate Classes
**Goal**: Remove classes that exist only for tree formatting

### 5.1 Analyze GeometryShape Usage
Since `Shape3DEditor` can now create shapes with formatters directly:
```csharp
// Instead of: new GeometryShape("box1", "box")
// Use: Shape3DEditor.AddShape("box1", "red", "box")
// Which internally sets: shape.ComputeTreeNodeTitle = GeometryShape.DefaultFormatter
```

### 5.2 Move Static Formatters to Utility Class
```csharp
public static class TreeNodeFormatters
{
    public static readonly Func<FoGlyph, string> Spatial = g => 
        /* GeometryShape formatter logic */;
        
    public static readonly Func<FoGlyph, string> Technical = g => 
        /* FoGlyph3D formatter logic */;
        
    public static readonly Func<FoGlyph, string> AudioPanel = g => 
        /* AudioPanelShape formatter logic */;
}
```

### 5.3 Update Shape3DEditor
```csharp
public OPResult AddShape(string name, string color, string shapeType, bool includeTag = true)
{
    var shape = factory(name, w, h, d);
    
    // Set spatial formatter for user-created shapes
    shape.ComputeTreeNodeTitle = TreeNodeFormatters.Spatial;
    
    if (includeTag)
        AddTextTag(shape);
        
    return AddShapeToStage(shape);
}
```

### 5.4 Eliminate GeometryShape Class
- Move factory logic to `Shape3DEditor`
- Move text tag logic to `Shape3DEditor`  
- Remove `GeometryShape.cs` entirely
- Update all references to use `Shape3DEditor.AddShape()`

---

## Benefits After Migration

### Code Reduction
- **Eliminated**: `GeometryShape` class (~60 lines)
- **Eliminated**: Multiple virtual method overrides
- **Simplified**: Shape creation all in `Shape3DEditor`

### Architectural Improvements  
- **Composition over inheritance** for display behavior
- **Runtime formatter switching** capability
- **Consistent with animation behavior pattern**
- **Per-instance customization** without subclassing

### Performance Benefits
- **Eliminated virtual method dispatch** overhead
- **Direct function call** instead of vtable lookup
- **Same memory usage** (8 bytes per instance for formatter reference)

### Future Flexibility
- **Dynamic tree view modes** (technical, spatial, summary)
- **Context-sensitive formatting** per application area
- **User preference-driven** tree display styles
- **Easy A/B testing** of different formats

---

## Risk Mitigation

### Backward Compatibility
- Each phase maintains existing functionality
- Gradual migration prevents breaking changes
- Virtual method backup during transition

### Testing Strategy
- **Phase-by-phase verification** ensures no regression
- **Tree view regression testing** after each migration
- **Performance benchmarking** to verify no degradation

### Rollback Plan
- Each phase can be reverted independently
- Git branches for each migration phase
- Virtual method infrastructure preserved until final phase

---

## Implementation Timeline

1. **Phase 1**: 1-2 days (add function property, rename virtual methods)
2. **Phase 2**: 1-2 days (create static formatters) 
3. **Phase 3**: 3-5 days (migrate classes gradually)
4. **Phase 4**: 1 day (remove virtual infrastructure)
5. **Phase 5**: 2-3 days (eliminate unnecessary classes)

**Total**: ~1-2 weeks for complete migration

This migration transforms the tree display system from inheritance-based to composition-based while maintaining full functionality and enabling future flexibility.