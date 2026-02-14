# Stage-Centric Refactoring Notes

## Overview
This document captures lessons learned from the December 2024 refactoring of Three2025 to use the new stage-centric API pattern from FoundryWorldsAndDrawings.

## Key Documentation Files
- **`STAGE_CENTRIC_PATTERN.md`** - Primary documentation for the new pattern (in Three2025 root)
- **`CLOCK_ARCHITECTURE_GUIDE.md`** - Example of stage-centric implementation
- **`FoundryWorldsAndDrawings/Shapes3D/FoArena3D.cs`** - IArena interface definition
- **`FoundryWorldsAndDrawings/Shapes3D/FoStage3D.cs`** - Stage implementation
- **`FoundryWorldsAndDrawings/Shapes2D/FoDrawing2D.cs`** - IDrawing interface (2D equivalent)
- **`FoundryWorldsAndDrawings/Shapes2D/FoPage2D.cs`** - Page implementation (2D equivalent)

## API Changes Summary

### Removed Methods (Legacy)
```csharp
// 3D - These no longer exist on IArena:
arena.CurrentStage()
arena.AddShapeToStage(shape, stageName)
arena.ClearArena()
stage.ClearStage()

// 2D - These no longer exist on IDrawing:
drawing.CurrentPage()
drawing.AddShapeToPage(shape, pageName)
drawing.SetCurrentPage(pageName)
```

### New Pattern (Stage-Centric)

#### For Razor Pages with Canvas3DComponent:
```csharp
public partial class MyPage : ComponentBase
{
    public Canvas3DComponent Canvas3DReference;
    private FoStage3D _myStage;  // Track this page's stage

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Get stage directly from canvas
            _myStage = Canvas3DReference?.Stage;
        }
    }

    public void AddSomething()
    {
        // Lazy initialization for methods called before OnAfterRenderAsync
        if (_myStage == null) _myStage = Canvas3DReference?.Stage;
        
        var shape = new FoShape3D("MyShape", "#FF0000");
        _myStage?.AddShape(shape);
    }

    public async Task ClearEverything()
    {
        // ClearAll() is async
        await _myStage?.ClearAll();
        // Or fire-and-forget: _ = _myStage?.ClearAll();
    }
}
```

#### For 2D Pages with Canvas2DComponent:
```csharp
public Canvas2DComponent Canvas2DReference;
private FoPage2D _myPage;

// In OnAfterRenderAsync:
_myPage = Canvas2DReference?.Page;

// Or use drawing.FirstPage() if no canvas reference
var page = drawing.FirstPage();
page.AddShape(shape);
```

#### For Technicians/Services (no Canvas reference):
```csharp
public class MyTech : IMyTech
{
    private FoStage3D? Stage { get; set; }
    
    // Allow pages to inject their stage
    public void SetStage(FoStage3D stage)
    {
        Stage = stage;
    }

    // Fallback to establishing a named stage if none injected
    private FoStage3D GetStage()
    {
        if (Stage != null) return Stage;
        var arena = Workspace.GetArena();
        return arena.EstablishStage<FoStage3D>("MyTechStage");
    }

    public void DoWork()
    {
        var stage = GetStage();
        stage.AddShape(shape);
    }
}
```

## Files Modified in This Refactoring

### Razor Pages (Components/Pages/)
| File | Changes |
|------|---------|
| `GlueTest3D.razor.cs` | Added `_pageStage`, uses `Canvas3DReference.Stage` |
| `Clock.razor.cs` | `ClearStage()` → `ClearAll()` |
| `TugOfWar.razor.cs` | Fixed both 2D and 3D patterns |
| `Home.razor.cs` | Added `_homeStage`, converted 8 methods |
| `Drawing.razor.cs` | Added `_drawingStage`, fixed 15+ methods |
| `DebugCanvas.razor.cs` | Added `_debugStage` |
| `MatrixTest.razor.cs` | Uses `Canvas3DReference.Stage` |
| `SpacialBoxTest.razor.cs` | Uses `Canvas3DReference.Stage` |
| `SpacialFrameTest.razor.cs` | Uses `Canvas3DReference.Stage` |
| `LegoSnappingTest.razor.cs` | Uses `Canvas3DReference.Stage` |
| `KnModelAnimationTest.razor.cs` | Uses both `Canvas3DReference.Stage` and `Canvas2DReference.Page` |

### Apprentice Technicians
| File | Changes |
|------|---------|
| `LightingTech.cs` | Added `SetStage()`, `GetStage()` pattern |
| `TrisocTech.cs` | Added `SetStage()`, `GetStage()` pattern |
| `ThreeDPlugin.cs` | Added `SetStage()`, `GetStage()` pattern |
| `RackTech.cs` | Added `SetStage()`, `GetStage()` pattern |
| `CageTech.cs` | Added `SetStage()`, `GetStage()` pattern |
| `ClockTech.cs` | Fixed `ParentStage` → `GetParentOfType<FoStage3D>()` |

### Services
| File | Changes |
|------|---------|
| `GeometryVisualizationService.cs` | Uses `EstablishStage<FoStage3D>("Visualization")` |
| `VisioWorkbook.cs` | Uses `drawing.FirstPage()` instead of `CurrentPage()` |

## Common Gotchas

1. **`ClearAll()` is async** - Use `await` or `_ = stage.ClearAll()` for fire-and-forget
2. **Lazy initialization** - Methods may be called before `OnAfterRenderAsync`, so always check for null and try to get stage
3. **`ParentStage` doesn't exist** - Use `shape.GetParentOfType<FoStage3D>()` instead
4. **Nullable annotations** - Add `#nullable enable` when adding nullable Stage properties

## Search Patterns for Finding Legacy Code
```bash
# Find remaining legacy API usage:
grep -r "CurrentStage\|AddShapeToStage\|ClearStage\|ClearArena\|CurrentPage\|AddShapeToPage" --include="*.cs"
```

## Future Considerations

1. **Technician Stage Injection** - Pages should call `technician.SetStage(_myStage)` in `OnAfterRenderAsync` to ensure technicians work with the page's stage
2. **Multi-Stage Scenarios** - Some pages may need multiple stages; the pattern supports this via `arena.EstablishStage<T>(name)`
3. **Stage Lifecycle** - Stages are managed by the Arena; clearing happens via `stage.ClearAll()`

## Contact
Questions about this refactoring pattern? Check the `STAGE_CENTRIC_PATTERN.md` document or review the `Canvas3DComponent.razor.cs` implementation in FoundryWorldsAndDrawings.
