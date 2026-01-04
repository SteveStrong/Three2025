# Technician Responsibility Pattern - Architectural Guideline

## Core Principle

**Technicians OPERATE on modeling objects but do NOT INSTANTIATE them.**

Technicians are aware of their operational context (stage, page, component) but always delegate object creation to the modeling layer (Arena, Drawing, Model, WorldManager).

## Separation of Concerns

### ✅ Technician Responsibilities (WHAT)
- **Operate** on shapes, models, stages, pages
- **Query** current state and properties
- **Modify** existing objects
- **Create shapes** within existing contexts
- **Be aware** of the current stage, page, or component they're working with

### ❌ NOT Technician Responsibilities (HOW/WHERE)
- **Instantiate** stages, pages, drawings, or arenas
- **Manage** persistence of modeling objects
- **Control** lifecycle of foundational structures
- **Decide** where objects live in the hierarchy

## The Pattern

### ❌ Anti-Pattern: Direct Instantiation
```csharp
public class MyTech : ITechnician
{
    public FoModel3D CreateModel(string url)
    {
        // ❌ WRONG - Technician creates stage directly
        var stage = new FoStage3D("MyStage");
        
        // ❌ WRONG - Bypasses modeling layer
        var model = new FoModel3D("MyModel") { Url = url };
        stage.AddShape(model);
        
        return model;
    }
}
```

### ✅ Correct Pattern: Delegate to Modeling Layer
```csharp
public class MyTech : ITechnician
{
    private IFoundryService FoundryService;
    private FoStage3D? Stage;
    
    public MyTech(IFoundryService foundryService)
    {
        FoundryService = foundryService;
    }
    
    // Stage is provided or retrieved from modeling layer
    private FoStage3D GetStage()
    {
        if (Stage != null) return Stage;
        
        // ✅ Delegate to arena (modeling layer) for stage retrieval
        var arena = FoundryService.Arena();
        return arena.EstablishStage<FoStage3D>("MyStage");
    }
    
    public FoModel3D CreateModel(string url)
    {
        // ✅ Get stage from modeling layer
        var stage = GetStage();
        
        // ✅ Create shape object (technician's job)
        var model = new FoModel3D("MyModel") { Url = url };
        
        // ✅ Delegate to stage for registration (modeling layer manages persistence)
        stage.AddShape(model);
        
        return model;
    }
}
```

## Architecture Layers

```
┌─────────────────────────────────────────┐
│           Razor Pages/UI                │  ← Entry point
│  - Gets arena from IFoundryService      │
│  - Calls EstablishStage/EstablishPage   │
│  - Passes context to technicians        │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│        Technicians (ITechnician)        │  ← Operate on context
│  - Receive stage/page/component         │
│  - Create shapes within context         │
│  - Query and modify objects             │
│  - NO instantiation of modeling objects │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      Modeling Layer (Arena/Drawing)     │  ← Manage object lifecycle
│  - FoArena3D.EstablishStage()          │
│  - FoDrawing2D.EstablishPage()         │
│  - WorldManager.CreateWorld()           │
│  - Persistence and hierarchy mgmt       │
└─────────────────────────────────────────┘
```

## Real-World Examples

### Example 1: Shape3DTech (Correct)
```csharp
public class Shape3DTech : IShape3DTech
{
    public FoStage3D EstablishGeometryStage(string? stageName = null)
    {
        // ✅ Delegate to arena for stage creation/retrieval
        var arena = Workspace.GetArena();
        var stageToUse = stageName ?? "AgentCanvas3D";
        var stage = arena.EstablishStage<FoStage3D>(stageToUse);
        
        // Store reference for operations
        Stage = stage;
        ShapeEditor.SetStage(stage);
        
        return stage;
    }
}
```

### Example 2: TrisocTech (Fixed)
```csharp
public class TrisocTech : ITrisocTech
{
    private FoStage3D GetStage()
    {
        if (Stage != null) return Stage;
        
        // ✅ Delegate to arena for stage retrieval
        var arena = FoundryService.Arena();
        return arena.EstablishStage<FoStage3D>("Trisoc");
    }
    
    public FoModel3D CreateModel(string name, string url)
    {
        // ✅ Create shape object (technician's responsibility)
        var model = new FoModel3D("Model" + name)
        {
            Url = url,
            Transform = new Transform3("ModelTransform")
            {
                Position = new Vector3(0, 0, 0),
                Scale = new Vector3(1, 1, 1),
            }
        };
        
        // ✅ Delegate to stage for registration (modeling layer)
        var stage = GetStage();
        stage.AddShape(model);
        
        return model;
    }
}
```

### Example 3: RackKnowledgeToFoFactory (Fixed)
```csharp
public static class RackKnowledgeToFoFactory
{
    /// <summary>
    /// Generate a complete data center with all cabinets from knowledge model.
    /// Technician operates on provided stage - does not instantiate modeling objects.
    /// </summary>
    public static FoStage3D GenerateDataCenter(
        DataCenterRackModel model, 
        FoStage3D stage)  // ✅ Stage passed in, not created
    {
        // ✅ Use provided stage - operate on context, don't create it
        
        // Generate cabinets and add to stage
        var cabinets = model.ModelComponents<RackCabinetConcept>();
        foreach (var cabinetConcept in cabinets)
        {
            var cabinetShape = GenerateCabinet(cabinetConcept);
            stage.AddShape(cabinetShape);  // ✅ Add to provided stage
        }
        
        return stage;
    }
}

// Caller (Razor page)
var arena = MentorServices.EstablishArena();  // ✅ Get from modeling layer
var stage = arena.EstablishStage<FoStage3D>("DataCenter");  // ✅ Modeling layer creates
RackKnowledgeToFoFactory.GenerateDataCenter(model, stage);  // ✅ Pass context to technician
```

## Key Benefits

1. **Clear Separation**: Technicians focus on operations, modeling layer handles lifecycle
2. **Testability**: Can inject mock stages/arenas for testing
3. **Reusability**: Technicians work with any stage/page, not tied to specific instances
4. **Maintainability**: Single source of truth for object creation (modeling layer)
5. **Flexibility**: Easy to change persistence strategy without touching technicians

## Migration Checklist

When creating or reviewing technician code:

- [ ] Does technician call `new FoStage3D()`? → Change to `arena.EstablishStage<FoStage3D>()`
- [ ] Does technician call `new FoPage2D()`? → Change to `drawing.EstablishPage<FoPage2D>()`
- [ ] Does technician call `new FoDrawing2D()`? → Change to `mentorServices.EstablishDrawing()`
- [ ] Does technician call `new FoArena3D()`? → Change to `mentorServices.EstablishArena()`
- [ ] Does factory create stages? → Accept stage parameter instead
- [ ] Are shapes created directly? → ✅ OK - technicians create shapes, not containers

## Summary

**Remember**: Technicians are *operators* not *constructors* of the modeling infrastructure.

- ✅ **Technicians CREATE**: Shapes, components, content
- ❌ **Technicians DON'T CREATE**: Stages, pages, drawings, arenas, worlds
- ✅ **Technicians RECEIVE**: Context from modeling layer
- ✅ **Technicians OPERATE**: On provided context
- ✅ **Technicians DELEGATE**: To modeling layer for infrastructure

---

*Last Updated: January 4, 2026*  
*Related Documents: STAGE_CENTRIC_PATTERN.md, PAGE_STAGE_SIMPLIFICATION_ADR.md*
