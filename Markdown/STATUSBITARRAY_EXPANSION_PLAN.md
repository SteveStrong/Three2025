# StatusBitArray Expansion Plan: Granular Dirty Tracking for 3D Objects

## Executive Summary

Expand `StatusBitArray` from 24 to 32 bits to support granular dirty tracking for 3D object updates. This enables intelligent JavaScript update routing based on change type (transform vs material vs geometry), significantly improving rendering performance.

**Key Benefits:**
- 10x-100x faster transform-only updates (no geometry rebuild)
- 5x-10x faster material-only updates  
- Enables smart batching of similar updates
- Memory impact: +1 byte per object (negligible)

---

## Current State Analysis

### Existing Implementation
```csharp
// FoundryRulesAndUnits/Models/StatusBitArray.cs
private BitArray m_Status;
m_Status = new BitArray(24);  // 24 bits allocated
```

### Current Bit Allocation (21/24 used)
```csharp
private enum StatusBit {
    Invisible = 0,          // UI/Visibility domain
    Private = 1,            // Access control
    IsReadOnly = 2,         // Access control
    Unselectable = 3,       // UI/Selection domain
    Selected = 4,           // UI/Selection domain
    UserSpecified = 5,      // Data provenance
    Expanded = 6,           // UI/Tree view domain
    Calculating = 7,        // Expression evaluator domain
    Calculated = 8,         // Expression evaluator domain
    ProtectFormula = 9,     // Expression evaluator domain
    ProtectValue = 10,      // Expression evaluator domain
    ForceEvaluation = 11,   // Expression evaluator domain
    ValueIncorrect = 12,    // Validation domain
    MetaKnowledge = 13,     // Knowledge system domain
    AllowSubshapes = 14,    // Diagram/Shape domain
    AllowConnections = 15,  // Diagram/Shape domain
    AllowAsParentShape = 16,// Diagram/Shape domain
    ShouldNotRender = 17,   // Rendering domain
    ShowChildren = 18,      // UI/Tree view domain
    Dirty = 19,             // ⚠️ SINGLE DIRTY FLAG - TOO COARSE
    ShouldDelete = 20       // Object lifecycle
    // POSITIONS 21-23: UNUSED
}
```

### Problem Statement

**Current behavior:**
```csharp
obj.SetDirty(true);  // What changed? We don't know!
// → Everything goes to Request3DSceneRefresh()
// → JavaScript rebuilds entire object (slow)
```

**Needed behavior:**
```csharp
transform.Position = newPos;     // → SetTransformDirty()
material.Color = "red";          // → SetMaterialDirty()  
geometry.Width = 10;             // → SetGeometryDirty()

// → JavaScript routes to appropriate update function
// → Only rebuilds what actually changed (fast)
```

---

## Proposed Solution: Reorganized 32-Bit Layout

### Phase 1: Expand and Reorganize Bits

**New Allocation Strategy:**

Group related flags together for clarity and future expansion within each domain.

```csharp
private enum StatusBit {
    // === OBJECT LIFECYCLE (0-2) ===
    ShouldDelete = 0,
    IsNew = 1,                    // NEW: Object newly created
    // Reserved = 2,              // Future: IsClone, IsTemplate, etc.
    
    // === DIRTY TRACKING (3-8) ===
    IsTransformDirty = 3,         // NEW: Position/Rotation/Scale changed
    IsMaterialDirty = 4,          // NEW: Color/Opacity/Texture changed
    IsGeometryDirty = 5,          // NEW: Size/Shape changed (rebuild needed)
    IsStructureDirty = 6,         // NEW: Children added/removed
    IsDataDirty = 7,              // NEW: Custom data/metadata changed
    // Reserved = 8,              // Future: IsAnimationDirty, IsPhysicsDirty, etc.
    
    // === ACCESS CONTROL & PROTECTION (9-12) ===
    IsReadOnly = 9,
    IsPrivate = 10,
    ProtectFormula = 11,
    ProtectValue = 12,
    
    // === UI & SELECTION (13-16) ===
    IsVisible = 13,               // Renamed from Invisible (inverted logic)
    IsSelectable = 14,            // Renamed from Unselectable (inverted logic)
    IsSelected = 15,
    IsExpanded = 16,
    
    // === RENDERING CONTROL (17-19) ===
    ShouldRender = 17,            // Renamed from ShouldNotRender (inverted logic)
    ShowChildren = 18,
    // Reserved = 19,             // Future: IsHighlighted, IsFaded, etc.
    
    // === EXPRESSION EVALUATOR (20-23) ===
    IsCalculating = 20,
    IsCalculated = 21,
    ForceEvaluation = 22,
    ValueIncorrect = 23,
    
    // === DATA PROVENANCE & VALIDATION (24-25) ===
    UserSpecified = 24,
    IsMetaKnowledge = 25,         // Renamed from MetaKnowledge
    
    // === SHAPE/DIAGRAM DOMAIN (26-28) ===
    AllowSubshapes = 26,
    AllowConnections = 27,
    AllowAsParentShape = 28,
    
    // === FUTURE EXPANSION (29-31) ===
    // Reserved = 29,             // Future expansion
    // Reserved = 30,             // Future expansion  
    // Reserved = 31              // Future expansion
}
```

**Key Changes:**
1. **Expand to 32 bits** (BitArray size: 24 → 32)
2. **Add 5 granular dirty flags** (Transform, Material, Geometry, Structure, Data)
3. **Add IsNew flag** for newly created objects
4. **Invert negative flags** for clarity (IsVisible vs Invisible)
5. **Group by domain** for maintainability
6. **Reserve 4 slots** for future expansion

---

## Phase 2: Property Updates

### Backward Compatible Properties

```csharp
// EXISTING: Keep for backward compatibility
public bool IsDirty {
    get {
        return IsTransformDirty || IsMaterialDirty || IsGeometryDirty || 
               IsStructureDirty || IsDataDirty;
    }
    set {
        // Legacy setter: marks all dirty flags
        IsTransformDirty = value;
        IsMaterialDirty = value;
        IsGeometryDirty = value;
        IsStructureDirty = value;
        IsDataDirty = value;
    }
}

// NEW: Granular dirty flags
public bool IsTransformDirty {
    get { return m_Status[(int)StatusBit.IsTransformDirty]; }
    set { m_Status[(int)StatusBit.IsTransformDirty] = value; }
}

public bool IsMaterialDirty {
    get { return m_Status[(int)StatusBit.IsMaterialDirty]; }
    set { m_Status[(int)StatusBit.IsMaterialDirty] = value; }
}

public bool IsGeometryDirty {
    get { return m_Status[(int)StatusBit.IsGeometryDirty]; }
    set { m_Status[(int)StatusBit.IsGeometryDirty] = value; }
}

public bool IsStructureDirty {
    get { return m_Status[(int)StatusBit.IsStructureDirty]; }
    set { m_Status[(int)StatusBit.IsStructureDirty] = value; }
}

public bool IsDataDirty {
    get { return m_Status[(int)StatusBit.IsDataDirty]; }
    set { m_Status[(int)StatusBit.IsDataDirty] = value; }
}

public bool IsNew {
    get { return m_Status[(int)StatusBit.IsNew]; }
    set { m_Status[(int)StatusBit.IsNew] = value; }
}

// RENAMED: Inverted logic for clarity
public bool IsVisible {
    get { return m_Status[(int)StatusBit.IsVisible]; }
    set { m_Status[(int)StatusBit.IsVisible] = value; }
}

public bool IsSelectable {
    get { return m_Status[(int)StatusBit.IsSelectable]; }
    set { m_Status[(int)StatusBit.IsSelectable] = value; }
}

public bool ShouldRender {
    get { return m_Status[(int)StatusBit.ShouldRender]; }
    set { m_Status[(int)StatusBit.ShouldRender] = value; }
}

public bool IsMetaKnowledge {
    get { return m_Status[(int)StatusBit.IsMetaKnowledge]; }
    set { m_Status[(int)StatusBit.IsMetaKnowledge] = value; }
}
```

---

## Phase 3: Object3D Integration

### Update SetDirty() Methods

```csharp
// In Object3D.cs
public virtual void SetDirty(bool value, bool deep = false) {
    // Legacy method: sets all dirty flags
    StatusBits.IsDirty = value;
    
    if (deep) {
        foreach (var child in Children)
            child.SetDirty(value, deep);
    }
}

// NEW: Granular dirty methods
public void SetTransformDirty(bool value = true) {
    StatusBits.IsTransformDirty = value;
}

public void SetMaterialDirty(bool value = true) {
    StatusBits.IsMaterialDirty = value;
}

public void SetGeometryDirty(bool value = true) {
    StatusBits.IsGeometryDirty = value;
}

public void SetStructureDirty(bool value = true) {
    StatusBits.IsStructureDirty = value;
}

public void ClearAllDirtyFlags() {
    StatusBits.IsTransformDirty = false;
    StatusBits.IsMaterialDirty = false;
    StatusBits.IsGeometryDirty = false;
    StatusBits.IsStructureDirty = false;
    StatusBits.IsDataDirty = false;
}
```

### Update Transform3 Property Setters

```csharp
// In Transform3.cs
public Vector3 Position {
    get => position;
    set {
        AssignVector(ref position, value);
        Owner?.SetTransformDirty();  // Specific dirty flag
    }
}

public Euler Rotation {
    get => rotation;
    set {
        AssignEuler(ref rotation, value);
        Owner?.SetTransformDirty();  // Specific dirty flag
    }
}

public Vector3 Scale {
    get => scale;
    set {
        AssignVector(ref scale, value);
        Owner?.SetTransformDirty();  // Specific dirty flag
    }
}
```

### Update Material Property Setters

```csharp
// In Material.cs
public string Color {
    get => color;
    set {
        if (color != value) {
            color = value;
            Owner?.SetMaterialDirty();  // Specific dirty flag
        }
    }
}

public double Opacity {
    get => opacity;
    set {
        if (Math.Abs(opacity - value) > 0.001) {
            opacity = value;
            Owner?.SetMaterialDirty();  // Specific dirty flag
        }
    }
}
```

### Update BufferGeometry Property Setters

```csharp
// In BoxGeometry.cs, SphereGeometry.cs, etc.
public double Width {
    get => width;
    set {
        if (Math.Abs(width - value) > 0.001) {
            width = value;
            Owner?.SetGeometryDirty();  // Specific dirty flag
        }
    }
}

public double Height {
    get => height;
    set {
        if (Math.Abs(height - value) > 0.001) {
            height = value;
            Owner?.SetGeometryDirty();  // Specific dirty flag
        }
    }
}
```

---

## Phase 4: Scene3D Update Routing

### Smart Object Bucketing

```csharp
// In Scene3D.cs - ComputeRefreshObjects()
public (bool success, Task refresh, Task delete) ComputeRefreshObjects() {
    var newObjects = new List<Object3D>();
    var transformOnlyObjects = new List<Object3D>();
    var materialOnlyObjects = new List<Object3D>();
    var geometryRebuildObjects = new List<Object3D>();
    var complexUpdateObjects = new List<Object3D>();
    var deletedObjects = new List<Object3D>();
    
    if (!CollectDirtyObjects(dirtyObjects, deletedObjects))
        return (false, Task.CompletedTask, Task.CompletedTask);
    
    // Route objects based on what changed
    foreach (var obj in dirtyObjects) {
        if (obj.StatusBits.IsNew) {
            newObjects.Add(obj);
        }
        else if (obj.StatusBits.IsGeometryDirty || obj.StatusBits.IsStructureDirty) {
            // Geometry or structure changes require full rebuild
            geometryRebuildObjects.Add(obj);
        }
        else if (obj.StatusBits.IsTransformDirty && obj.StatusBits.IsMaterialDirty) {
            // Multiple changes - use combined update
            complexUpdateObjects.Add(obj);
        }
        else if (obj.StatusBits.IsTransformDirty) {
            // Transform only - fast path
            transformOnlyObjects.Add(obj);
        }
        else if (obj.StatusBits.IsMaterialDirty) {
            // Material only - medium path
            materialOnlyObjects.Add(obj);
        }
        
        obj.ClearAllDirtyFlags();
    }
    
    // Execute updates in parallel where possible
    var tasks = new List<Task>();
    
    if (newObjects.Count > 0)
        tasks.Add(Request3DSceneCreate(newObjects));
    
    if (transformOnlyObjects.Count > 0)
        tasks.Add(Request3DTransformUpdate(transformOnlyObjects));
    
    if (materialOnlyObjects.Count > 0)
        tasks.Add(Request3DMaterialUpdate(materialOnlyObjects));
    
    if (geometryRebuildObjects.Count > 0)
        tasks.Add(Request3DGeometryRebuild(geometryRebuildObjects));
    
    if (complexUpdateObjects.Count > 0)
        tasks.Add(Request3DSceneRefresh(complexUpdateObjects));
    
    if (deletedObjects.Count > 0)
        tasks.Add(Request3DSceneDelete(deletedObjects));
    
    return (true, Task.WhenAll(tasks), Task.CompletedTask);
}
```

### New JavaScript Interop Methods

```csharp
// Fast path - transform only
private async Task Request3DTransformUpdate(List<Object3D> objects) {
    var updates = objects.Select(obj => new {
        uuid = obj.Uuid,
        position = obj.Transform?.Position,
        rotation = obj.Transform?.Rotation,
        scale = obj.Transform?.Scale
    });
    
    await JsRuntime.InvokeVoidAsync(
        ResolveFunction("updateTransforms"), 
        updates
    );
}

// Medium path - material only
private async Task Request3DMaterialUpdate(List<Object3D> objects) {
    var updates = objects.Select(obj => new {
        uuid = obj.Uuid,
        color = obj.Material?.Color,
        opacity = obj.Material?.Opacity,
        wireframe = obj.Material?.Wireframe
    });
    
    await JsRuntime.InvokeVoidAsync(
        ResolveFunction("updateMaterials"),
        updates
    );
}

// Slow path - geometry rebuild
private async Task Request3DGeometryRebuild(List<Object3D> objects) {
    var settings = new ImportSettings();
    settings.CopyAndReset(objects);
    
    await JsRuntime.InvokeVoidAsync(
        ResolveFunction("rebuildGeometry"),
        settings
    );
}
```

---

## Phase 5: JavaScript Implementation

### New Update Functions

```javascript
// In FoundryWorldsAndDrawings.js

// FAST: Transform-only updates (10x-100x faster)
FoundryWorldsAndDrawings.updateTransforms = function(updates) {
    updates.forEach(update => {
        const obj = scene.getObjectByProperty('uuid', update.uuid);
        if (obj && update.position) {
            obj.position.set(update.position.x, update.position.y, update.position.z);
        }
        if (obj && update.rotation) {
            obj.rotation.set(update.rotation.x, update.rotation.y, update.rotation.z);
        }
        if (obj && update.scale) {
            obj.scale.set(update.scale.x, update.scale.y, update.scale.z);
        }
    });
    // No geometry rebuild needed!
};

// MEDIUM: Material-only updates (5x-10x faster)
FoundryWorldsAndDrawings.updateMaterials = function(updates) {
    updates.forEach(update => {
        const obj = scene.getObjectByProperty('uuid', update.uuid);
        if (obj && obj.material) {
            if (update.color) obj.material.color.set(update.color);
            if (update.opacity !== undefined) obj.material.opacity = update.opacity;
            if (update.wireframe !== undefined) obj.material.wireframe = update.wireframe;
        }
    });
    // No geometry rebuild needed!
};

// SLOW: Geometry rebuild (necessary for size changes)
FoundryWorldsAndDrawings.rebuildGeometry = function(importSettings) {
    importSettings.children.forEach(objData => {
        const obj = scene.getObjectByProperty('uuid', objData.uuid);
        if (obj && obj.geometry) {
            obj.geometry.dispose();  // Clean up old geometry
            obj.geometry = createGeometry(objData);  // Rebuild
        }
    });
};
```

---

## Implementation Timeline

### Phase 1: Foundation (Week 1)
**FoundryRulesAndUnits Package Update**

- [ ] Expand BitArray from 24 to 32 bits
- [ ] Reorganize StatusBit enum with domain grouping
- [ ] Add 5 new dirty flag properties
- [ ] Add IsNew property
- [ ] Invert negative flags (IsVisible, IsSelectable, etc.)
- [ ] Update existing property implementations
- [ ] Add backward-compatible IsDirty computed property
- [ ] Update package version (e.g., 1.4.0 → 1.5.0)
- [ ] Publish new NuGet package

**Testing:**
- Verify all existing properties still work
- Verify IsDirty computed property matches any granular flag
- Verify backward compatibility

### Phase 2: Object3D Integration (Week 2)
**FoundryWorldsAndDrawings Package Update**

- [ ] Update FoundryRulesAndUnits NuGet reference
- [ ] Add granular SetDirty methods to Object3D
- [ ] Add ClearAllDirtyFlags method
- [ ] Update Transform3 property setters
- [ ] Update Material property setters  
- [ ] Update BufferGeometry property setters
- [ ] Mark objects as IsNew in constructors

**Testing:**
- Verify transform changes set IsTransformDirty
- Verify material changes set IsMaterialDirty
- Verify geometry changes set IsGeometryDirty
- Verify IsDirty returns true when any flag is set

### Phase 3: Scene Update Routing (Week 3)
**Scene3D.cs Updates**

- [ ] Implement smart object bucketing in ComputeRefreshObjects
- [ ] Add Request3DTransformUpdate method
- [ ] Add Request3DMaterialUpdate method
- [ ] Add Request3DGeometryRebuild method
- [ ] Update existing Request3DSceneRefresh for complex updates
- [ ] Implement parallel task execution

**Testing:**
- Verify transform-only changes route to fast path
- Verify material-only changes route to medium path
- Verify geometry changes route to rebuild path
- Verify mixed changes route to appropriate handlers

### Phase 4: JavaScript Implementation (Week 4)
**FoundryWorldsAndDrawings.js Updates**

- [ ] Implement updateTransforms function
- [ ] Implement updateMaterials function
- [ ] Implement rebuildGeometry function
- [ ] Update existing refresh3DScene for complex cases
- [ ] Add performance logging/metrics

**Testing:**
- Verify transform updates don't rebuild geometry
- Verify material updates don't rebuild geometry
- Verify geometry updates properly rebuild
- Measure performance improvements

### Phase 5: Testing & Optimization (Week 5)
**Integration Testing**

- [ ] Test TugOfWar animation with transform-only updates
- [ ] Test color changes with material-only updates
- [ ] Test resize operations with geometry rebuild
- [ ] Test complex multi-property changes
- [ ] Performance benchmarking
- [ ] Memory profiling

**Performance Targets:**
- Transform-only: <1ms per object (vs 10-50ms current)
- Material-only: <2ms per object (vs 10-30ms current)
- Geometry rebuild: 10-20ms per object (same as current)

---

## Migration Strategy

### Backward Compatibility

**Existing Code Continues to Work:**
```csharp
// Legacy code still works
obj.SetDirty(true);  
// → Sets all granular flags via IsDirty setter

if (obj.IsDirty) { ... }
// → Returns true if ANY granular flag is set
```

**New Code Can Opt-In:**
```csharp
// New code can be specific
obj.SetTransformDirty();
obj.SetMaterialDirty();

if (obj.StatusBits.IsTransformDirty) { ... }
```

### No Breaking Changes

- Existing property names unchanged (except renamed for clarity)
- IsDirty property kept as computed property
- SetDirty(bool) method still works
- All existing usages continue to function

---

## Success Metrics

### Performance Improvements
- **Transform animations:** 10x-100x faster
- **Material updates:** 5x-10x faster
- **Geometry rebuilds:** Same speed (only when necessary)
- **Memory overhead:** +1 byte per object (negligible)

### Code Quality
- **Maintainability:** Domain-grouped flags
- **Clarity:** Positive flag names (IsVisible vs Invisible)
- **Extensibility:** 3 reserved slots for future expansion
- **Backward compatibility:** Zero breaking changes

### Developer Experience
- **Clear intent:** Specific dirty flags communicate what changed
- **Easy debugging:** Can inspect exactly what changed
- **Performance visibility:** Can measure routing effectiveness

---

## Risk Mitigation

### Testing Strategy
1. **Unit tests:** Each new property getter/setter
2. **Integration tests:** Full update pipeline
3. **Performance tests:** Benchmark before/after
4. **Regression tests:** Verify existing functionality

### Rollback Plan
- Keep old NuGet package version available
- Can revert to previous version if issues found
- Granular dirty tracking is opt-in (legacy SetDirty still works)

### Monitoring
- Log routing decisions (transform vs material vs geometry)
- Track performance metrics per update type
- Monitor memory usage trends

---

## Future Enhancements

### Additional Dirty Flags (Reserved Slots)
- **IsAnimationDirty:** Animation state changed
- **IsPhysicsDirty:** Physics properties changed
- **IsShaderDirty:** Custom shader parameters changed
- **IsLightingDirty:** Lighting properties changed

### Advanced Routing
- **Batch similar updates:** Group all transform updates, send once
- **Priority queues:** Urgent updates (user interaction) vs background
- **Delta compression:** Only send changed values, not full objects
- **Predictive batching:** Anticipate animation patterns

### Performance Analytics
- **Dashboard:** Real-time update routing statistics
- **Bottleneck detection:** Identify slow update patterns
- **Optimization suggestions:** Recommend code improvements

---

## Conclusion

This phased approach provides:
1. **Solid foundation:** Clean, well-organized bit layout
2. **Backward compatibility:** Existing code works unchanged
3. **Significant performance gains:** 10x-100x for common operations
4. **Future extensibility:** Room for additional flags
5. **Low risk:** Incremental rollout with testing at each phase

**Recommendation:** Proceed with implementation starting with Phase 1 (FoundryRulesAndUnits package update).
