# Foundry Framework - Architectural Principles

> **Golden Rules**: Core patterns that must be followed when extending the framework

## Principle 1: Never Bypass the Modeling Layer with `new`

### The Rule
**If a class comes from the modeling layer (Arena, Drawing, Stage, Page, World), you MUST NOT instantiate it with `new`. Always ask the framework to provide it.**

### Why It Exists
The modeling layer manages:
- **Persistence**: Objects survive across scoped service lifetimes
- **Lifecycle**: Proper initialization, cleanup, and disposal
- **Discovery**: Framework can find objects when needed
- **Events**: PubSub wiring for UI updates

When you call `new`, you create an **orphan object** that exists outside this management system.

### Common Violations

```csharp
// ❌ WRONG - Creates orphan stage
var stage = new FoStage3D("MyStage");
stage.AddShape(shape);
// Next request: Stage is lost, shapes disappear

// ❌ WRONG - Creates orphan page
var page = new FoPage2D("MyPage");
page.AddShape(shape);

// ❌ WRONG - Creates orphan arena
var arena = new FoArena3D();
```

### Correct Pattern

```csharp
// ✅ CORRECT - Ask arena for stage
var arena = foundryService.Arena();  // or Workspace.GetArena()
var stage = arena.EstablishStage<FoStage3D>("MyStage");
stage.AddShape(shape);
// Next request: Arena returns SAME stage with all shapes

// ✅ CORRECT - Ask drawing for page
var drawing = mentorServices.EstablishDrawing();
var page = drawing.EstablishPage<FoPage2D>("MyPage");
page.AddShape(shape);

// ✅ CORRECT - Ask services for arena
var arena = mentorServices.EstablishArena();
```

### Quick Check: "Am I Creating Infrastructure?"

Ask yourself: **"Is this a container/manager or content?"**

| Type | Container (Don't `new`) | Content (OK to `new`) |
|------|------------------------|----------------------|
| Stage | `FoStage3D` ❌ | `FoShape3D` ✅ |
| Page | `FoPage2D` ❌ | `FoGlyph2D` ✅ |
| Arena | `FoArena3D` ❌ | Shapes ✅ |
| Drawing | `FoDrawing2D` ❌ | Shapes ✅ |
| World | `FoWorld3D` ❌ | Models ✅ |
| Model (KnModel) | ❌ via MentorServices | Components ✅ |

**Rule of thumb**: If it **contains** other objects, ask the framework for it. If it **is** the content, you can create it.

---

## Principle 2: Technicians Operate, They Don't Construct

### The Rule
**Technicians (ITechnician implementations) receive their operational context from the framework. They never instantiate stages, pages, drawings, or arenas.**

### Why It Exists
Clear separation of concerns:
- **Technicians**: Business logic, operations on shapes/models
- **Modeling Layer**: Infrastructure, persistence, lifecycle
- **Pages/UI**: Coordination, context establishment

### Pattern

```csharp
public class MyTech : ITechnician
{
    private IFoundryService FoundryService;
    private FoStage3D? Stage;
    
    // ✅ CORRECT - Accept context or retrieve from framework
    private FoStage3D GetStage()
    {
        if (Stage != null) return Stage;
        var arena = FoundryService.Arena();
        return arena.EstablishStage<FoStage3D>("MyStage");
    }
    
    // ✅ CORRECT - Create shapes (content), add to framework-provided stage
    public FoShape3D CreateBox(string name)
    {
        var box = new FoShape3D(name).CreateBox("Box", 1, 1, 1);
        var stage = GetStage();
        stage.AddShape(box);
        return box;
    }
    
    // ❌ WRONG - Never do this
    public FoStage3D CreateMyOwnStage()
    {
        return new FoStage3D("MyStage");  // Orphan!
    }
}
```

**See**: [TECHNICIAN_RESPONSIBILITY_PATTERN.md](Markdown/TECHNICIAN_RESPONSIBILITY_PATTERN.md) for detailed guidance.

---

## Principle 3: Services Flow Downward, Never Create Your Own

### The Rule
**Use dependency injection to receive services. Never instantiate services yourself.**

### Why It Exists
- **Lifetime Management**: Framework controls Scoped/Singleton lifetimes
- **Testing**: Can inject mocks
- **Configuration**: Single point of setup in Program.cs

### Pattern

```csharp
// ✅ CORRECT - Receive via DI
public class MyTech : ITechnician
{
    private IFoundryService FoundryService;
    private IWorkspace Workspace;
    
    public MyTech(IFoundryService foundryService, IWorkspace workspace)
    {
        FoundryService = foundryService;
        Workspace = workspace;
    }
}

// ❌ WRONG - Never do this
public class MyTech : ITechnician
{
    private IFoundryService FoundryService = new FoundryService();  // NO!
}
```

---

## Principle 4: Trust the Layers - Don't Take Shortcuts

### The Rule
**When a framework provides a method for something, use it. Don't work around it because it seems faster or simpler.**

### Why It Exists
Every layer in the framework exists to solve a problem:
- **Arena**: Stage persistence across requests
- **EstablishStage()**: Find-or-create with proper wiring
- **AddShape()**: Registration + events + UI updates
- **PubSub**: Automatic UI refresh notifications

Shortcuts bypass these guarantees and cause bugs that are hard to trace.

### Real Example of Shortcut Cost

```csharp
// Shortcut taken: "I'll just create my own stage"
var stage = new FoStage3D("DataCenter");
_generatedStage = RackKnowledgeToFoFactory.GenerateDataCenter(model);
// Bug: Shapes disappear on next request because stage isn't in arena

// Cost to fix:
// - Changed factory signature
// - Updated all callers
// - Added documentation
// - Debugged for hours

// Should have been:
var arena = MentorServices.EstablishArena();
var stage = arena.EstablishStage<FoStage3D>("DataCenter");
RackKnowledgeToFoFactory.GenerateDataCenter(model, stage);
// Works immediately, shapes persist
```

**Trust the framework. The indirection is the feature, not overhead.**

---

## Code Review Checklist

When adding new features, check:

- [ ] Are any modeling layer classes instantiated with `new`? (Stage, Page, Arena, Drawing, World)
- [ ] Do technicians receive context or ask framework for it? (Never `new FoStage3D()`)
- [ ] Are services injected via constructor? (Never `new MyService()`)
- [ ] Are shapes added to framework-provided containers? (Not orphan objects)
- [ ] Do factories accept context parameters? (Not create their own infrastructure)

**If any checkbox fails, refactor before merging.**

---

## When You're Tempted to Shortcut

Ask yourself:

1. **"Why does the framework have this layer?"**
   - There's a reason. Find it before bypassing.

2. **"What problem does `EstablishStage()` solve vs `new`?"**
   - Persistence, find-or-create, event wiring, lifecycle management

3. **"Will this survive a scoped service refresh?"**
   - If you created it, probably not. If framework created it, yes.

4. **"Can I explain why my shortcut is better than the framework?"**
   - If not, use the framework.

---

## Summary: The One Rule

> **"If the framework provides a way to get something, use that way. Never call `new` on infrastructure classes."**

Everything else follows from this.

---

## Related Documents

- [TECHNICIAN_RESPONSIBILITY_PATTERN.md](Markdown/TECHNICIAN_RESPONSIBILITY_PATTERN.md) - Technician architectural guidance
- [STAGE_CENTRIC_PATTERN.md](Markdown/STAGE_CENTRIC_PATTERN.md) - Stage-centric architecture for multi-canvas apps
- [PAGE_STAGE_SIMPLIFICATION_ADR.md](../FoundryWorldsAndDrawings/Markdown/PAGE_STAGE_SIMPLIFICATION_ADR.md) - Evolution of stage management

---

*Last Updated: January 4, 2026*  
*Status: **Active - Enforce on all new code***
