# Stage-Centric Pattern for Multi-Canvas 3D Applications

**Status**: ✅ Implemented in Phase 0.5  
**Date**: November 26, 2025  
**Branch**: `finish-tugofwar`

## Overview

The **Stage-Centric Pattern** ensures that Blazor pages interact with their specific 3D stage rather than using global arena-wide operations. This is **essential preparation** for multi-canvas solutions where multiple `Canvas3DComponent` instances must coexist without interfering with each other.

## Problem Statement

### Before Phase 0.5 (Arena-Focused)
Pages used global arena operations that affected ALL stages:
```csharp
// ❌ WRONG - Affects entire arena (all canvases)
arena.ClearArena();              // Wipes ALL stages
arena.AddShapeToStage(shape);     // Uses "current stage" global state
arena.SetScene(scene);            // Sets global current scene
```

**Critical Issues**:
- Opening Clock page → `arena.ClearArena()` → wipes shapes from MultiCanvas3DTest
- TugOfWar adds shapes → wrong stage → visual corruption
- **Multi-canvas testing impossible** - pages step on each other

### After Phase 0.5 (Stage-Focused)
Pages interact only with their specific stage:
```csharp
// ✅ CORRECT - Affects only this page's stage
_pageStage?.ClearStage();         // Clears only this page's shapes
_pageStage?.AddShape(shape);      // Adds to this page's stage
// arena.SetScene() removed       // Canvas already handles scene linking
```

## Architectural Foundation: 2D/3D Parallel

The stage-centric pattern mirrors the existing **2D page management** architecture:

| Aspect | 2D (Canvas2D) | 3D (Canvas3D) |
|--------|---------------|---------------|
| **Container** | `FoDrawing2D` | `FoArena3D` |
| **Managed Unit** | `FoPage2D` | `FoStage3D` |
| **Canvas Parameter** | `PageName` | `SceneName` |
| **Canvas Creates** | `drawing.EstablishPage<FoPage2D>(PageName)` | `arena.EstablishStage<FoStage3D>(SceneName)` |
| **Canvas Tracks** | `ManagedPage` field | `ManagedStage` field |
| **User Code Retrieves** | `drawing.EstablishPage(name)` | `arena.EstablishStage(name)` |
| **Clear Method** | `page.ClearPage()` | `stage.ClearStage()` |
| **Add Method** | `page.AddShape(shape)` | `stage.AddShape(shape)` |

**Key Insight**: Both Canvas2D and Canvas3D manage their page/stage lifecycle. User code retrieves the existing page/stage using the canvas's parameter.

## Implementation Pattern

### Step 1: Add Stage Field
```csharp
public partial class YourPage : ComponentBase
{
    public Canvas3DComponent Canvas3DReference = null;
    private FoStage3D? _yourPageStage; // ✅ Track this page's stage
```

### Step 2: Retrieve Stage in OnAfterRenderAsync
```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
        var arena = Workspace.GetArena();
        
        if (found)
        {
            // ✅ Get this page's stage (Canvas already created it and linked to scene)
            _yourPageStage = arena.EstablishStage<FoStage3D>(Canvas3DReference.SceneName);
            $"YourPage: Retrieved stage '{_yourPageStage?.Name}' from Canvas".WriteSuccess();
            
            // NO: arena.SetScene() - Canvas already handled scene linking
            DoYourSetup();
        }
    }
    await base.OnAfterRenderAsync(firstRender);
}
```

### Step 3: Replace Arena Operations

| Old (Arena-Focused) | New (Stage-Focused) | Notes |
|---------------------|---------------------|-------|
| `arena.ClearArena()` | `_yourPageStage?.ClearStage()` | Only clears this page's shapes |
| `arena.AddShapeToStage<T>(shape)` | `_yourPageStage?.AddShape(shape)` | Adds to this page's stage explicitly |
| `arena.SetScene(scene)` | *(remove call)* | Canvas already linked stage to scene |

### Step 4: Add Required Using Directives
```csharp
using FoundryWorldsAndDrawings.Shape;        // For FoStage3D
using FoundryRulesAndUnits.Extensions;       // For WriteSuccess()
```

## Canvas3DComponent Lifecycle

Understanding how Canvas3D manages stages is crucial:

```csharp
// Canvas3DComponent.razor.cs (lines 77-88)
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && !string.IsNullOrEmpty(SceneName))
    {
        var arena = Workspace?.GetArena();
        var scene = arena?.GetScene();
        
        // Canvas creates the stage
        var stage = arena.EstablishStage<FoStage3D>(SceneName);
        
        // Canvas links stage to scene
        scene.LinkToStage(stage);
        
        // Canvas tracks the stage
        ManagedStage = stage;
    }
}
```

**Key Points**:
1. Canvas creates stage **before** page's `OnAfterRenderAsync` runs
2. Canvas already linked stage to scene
3. `EstablishStage()` is **idempotent** - safe to call multiple times with same name
4. Page retrieves existing stage, doesn't create new one

## Phase 0.5 Implementation Results

### Pages Fixed (38 Total Violations)
1. **Clock.razor.cs**: 9 violations (2 ClearArena, 1 SetScene, 6 AddShapeToStage)
2. **TugOfWar.razor.cs**: 8 violations (3 ClearArena, 1 SetScene, 4 AddShapeToStage)
3. **Trisoc.razor.cs**: 4 violations (1 SetScene, 3 AddShapeToStage)
4. **SpacialBoxTest.razor.cs**: 5 violations (2 ClearArena, 1 SetScene, 2 AddShapeToStage)
5. **MatrixTest.razor.cs**: 5 violations (2 ClearArena, 1 SetScene, 2 AddShapeToStage)
6. **LegoSnappingTest.razor.cs**: 4 violations (1 ClearArena, 1 SetScene, 2 AddShapeToStage)
7. **SpacialFrameTest.razor.cs**: 3 violations (1 ClearArena, 1 SetScene, 1 AddShapeToStage)

### Build Status
✅ **All pages compile successfully** - `dotnet build` clean

## Multi-Canvas Benefits

This pattern enables:

### ✅ Canvas Isolation
- Each canvas has unique `SceneName` → unique stage → no shape interference
- Clearing one page doesn't affect other pages
- Adding shapes goes to correct stage automatically

### ✅ Parallel Development
- Multiple developers can work on different pages without conflicts
- Pages don't need to know about each other's existence

### ✅ Testing Capabilities
- Can open Clock + TugOfWar + MultiCanvas3DTest simultaneously
- Each page maintains its own 3D objects independently
- Visual debugging easier (no cross-contamination)

### ✅ Performance Predictability
- Stage operations are scoped (no global locks needed)
- Clear performance - only clears relevant shapes
- Frame budget protected (16ms target)

## Future Multi-Canvas Scenarios

With stage-centric pattern in place, these scenarios become possible:

### Scenario 1: Dashboard with Multiple 3D Views
```razor
<div class="dashboard">
    <Canvas3DComponent SceneName="overview" />
    <Canvas3DComponent SceneName="detail" />
    <Canvas3DComponent SceneName="comparison" />
</div>
```

### Scenario 2: Split-Screen Editing
```razor
<div class="editor">
    <Canvas3DComponent SceneName="editor-left" />  <!-- Edit mode -->
    <Canvas3DComponent SceneName="editor-right" /> <!-- Preview mode -->
</div>
```

### Scenario 3: Component Comparison
```razor
@foreach (var variant in designVariants)
{
    <Canvas3DComponent SceneName="@($"variant-{variant.Id}")" />
}
```

## Testing Plan

### Manual Testing (Todo #7)
1. Open Clock page → verify shapes appear
2. Open TugOfWar page → verify Clock shapes still visible in background
3. Open MultiCanvas3DTest → verify all 3 pages coexist
4. Navigate between pages → verify no visual artifacts
5. Clear Clock page → verify TugOfWar/MultiCanvas unchanged

### Automated Testing (Future)
- Unit tests for `EstablishStage()` idempotency
- Integration tests for multi-canvas lifecycle
- Performance tests for stage operations under load

## Common Pitfalls to Avoid

### ❌ DON'T: Use arena-wide operations
```csharp
arena.ClearArena();              // Affects ALL stages
arena.AddShapeToStage(shape);     // Uses global "current stage"
```

### ✅ DO: Use stage-specific operations
```csharp
_myStage?.ClearStage();          // Only this stage
_myStage?.AddShape(shape);       // Explicit stage
```

### ❌ DON'T: Set scene manually
```csharp
arena.SetScene(scene);           // Canvas already did this
```

### ✅ DO: Trust Canvas lifecycle
```csharp
// Canvas already:
// 1. Created stage
// 2. Linked stage to scene
// 3. Stored in ManagedStage
// Just retrieve it:
_myStage = arena.EstablishStage<FoStage3D>(Canvas3DReference.SceneName);
```

### ❌ DON'T: Create stage in page
```csharp
_myStage = new FoStage3D();      // Bypasses Canvas management
```

### ✅ DO: Retrieve existing stage
```csharp
_myStage = arena.EstablishStage<FoStage3D>(Canvas3DReference.SceneName);
```

## Migration Checklist

When converting a page to stage-centric pattern:

- [ ] Add `private FoStage3D? _pageStage;` field
- [ ] Add `using FoundryWorldsAndDrawings.Shape;` directive
- [ ] Add `using FoundryRulesAndUnits.Extensions;` directive (for WriteSuccess)
- [ ] In `OnAfterRenderAsync`: Call `_pageStage = arena.EstablishStage<FoStage3D>(Canvas3DReference.SceneName);`
- [ ] Remove all `arena.SetScene()` calls
- [ ] Replace all `arena.ClearArena()` → `_pageStage?.ClearStage()`
- [ ] Replace all `arena.AddShapeToStage()` → `_pageStage?.AddShape()`
- [ ] Build and verify no errors
- [ ] Test page in isolation
- [ ] Test page with other pages open

## Related Documentation

- **ANIMATION_RACE_CONDITION_FIX_PLAN.md** - Phase 0.5 section (lines ~1880+)
- **Canvas3DComponent.razor.cs** - Stage creation logic (lines 77-88)
- **Canvas2DComponent.razor.cs** - 2D parallel pattern reference (lines 56-57)
- **FoStage3D.cs** - Stage API (ClearStage, AddShape methods)

## Next Steps

- [ ] Complete Todo #7: Test multi-page scenario
- [ ] Tag `v3.0.3-phase0.5` after validation
- [ ] Begin Phase 1A: Rendering Guard (prevent overlapping render calls)
- [ ] Future: Document multi-canvas best practices
- [ ] Future: Create reusable multi-canvas layout components

## Conclusion

The **Stage-Centric Pattern** is **mandatory preparation** for multi-canvas solutions. By ensuring pages interact only with their specific stage:

1. **Prevents cross-contamination** between canvases
2. **Enables parallel development** of 3D features
3. **Maintains architectural consistency** with 2D page management
4. **Protects frame budget** via scoped operations
5. **Unblocks race condition fixes** (subsequent phases require working multi-canvas)

This pattern is now **standard practice** for all pages using Canvas3DComponent.
