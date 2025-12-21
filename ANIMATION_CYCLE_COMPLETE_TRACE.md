# Complete Animation Cycle Trace

**From Canvas mount to Shape creation/change/deletion/redraw**

---

## 🎬 PHASE 1: Canvas Component Lifecycle (Component Mount)

### 1.1 Canvas3DComponent.OnAfterRenderAsync (firstRender=true)

**File**: `FoundryWorldsAndDrawings/Shared/Canvas3DComponent.razor.cs:72`

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Get scene from ViewerThreeD reference
        var (found, scene) = GetActiveScene();
        
        // Create FoStage3D in arena
        var stage = arena.EstablishStage<FoStage3D>(SceneName);
        
        // Link stage ↔ scene bidirectionally
        scene.LinkToStage(stage);
        ManagedStage = stage;
        
        // 🔑 KEY: Mark scene as ACTIVE (enables rendering)
        scene.SetActive();
        
        // Subscribe to animation events
        AnimationFrameBus.SubscribeToAnimation(OnAnimationEvent);
        
        // Start global animation loop (affects all canvases)
        await FoundryService.StartGlobalAnimation();
    }
}
```

**Result**: 
- ✅ Scene marked IsActive = true
- ✅ Stage created and linked to scene
- ✅ Animation loop started (ticks every 16ms @ 60fps)

---

## 🔄 PHASE 2: Animation Frame Loop (Every Tick)

### 2.1 AnimationFrameBus → OnAnimationEvent

**File**: `Canvas3DComponent.razor.cs:196`

```csharp
private async void OnAnimationEvent(AnimationEvent message)
{
    if (message.IsWorld3D())
        await RenderFrame(message.tick, message.fps);
}
```

### 2.2 RenderFrame → Arena.RenderArena

**File**: `Canvas3DComponent.razor.cs:204`

```csharp
public async Task RenderFrame(int tick, double fps)
{
    if (_isRendering) return;  // Guard against overlap
    
    _isRendering = true;
    arena.SetCurrentlyRendering(true, tick);
    
    // 🔑 KEY: Delegates to arena to render all stages
    await arena.RenderArena(tick, fps);
    
    _isRendering = false;
    arena.SetCurrentlyRendering(false, tick);
}
```

### 2.3 FoArena3D.RenderArena

**File**: `FoundryWorldsAndDrawings/Shapes3D/FoArena3D.cs:147`

```csharp
public async Task RenderArena(int tick, double fps)
{
    var allStages = Stages().GetAllStages();
    
    // 🔑 KEY: Two-tier performance guard
    // 1. Skip stages without scenes (orphan stages)
    // 2. Skip stages with inactive scenes (unmounted Canvas)
    foreach (var stage in allStages.Where(s => s.GetAssociatedScene()?.IsActive == true))
    {
        await stage.RenderStage(tick, fps);
    }
}
```

**Result**:
- Only renders stages with IsActive scenes
- Skips stages for unmounted canvases

---

## 📐 PHASE 3: Pre-Animation Event (Parameter Updates)

**HAPPENS BEFORE GEOMETRY COMPUTATION**

### 3.1 Workspace.PreRender

**File**: `Canvas3DComponent.razor.cs:224`

```csharp
Workspace?.PreRender(tick, RenderDomain.World);
```

This triggers the **PreAnimationEvent** chain:

1. **AnimationFrameBus** → All subscribed components
2. **KnComponent.OnPreAnimationEvent** → Propagates to children
3. **User code** → Updates parameters (e.g., Height changes)

### 3.2 Parameter Change → Smash

**Example**: User changes Height parameter in AnimatedParameterTestComponent

```csharp
public void SetHeight(double value) 
{
    FindParameter("Height")?.SetValue(value);
    // SetValue internally calls Smash if value changes
}
```

### 3.3 KnGeometryParameter.Smash

**File**: `FoundryMentorModeler/Mentor/KnGeomertyParameter.cs:56`

```csharp
public override bool Smash(Action<OPResult>? OnComplete = null)
{
    // 1️⃣ Grab old shape BEFORE clearing Value
    var oldShape = Value?.ValueAs<FoShape3D>();
    if (oldShape != null)
    {
        // Mark for deletion - animation loop will handle cleanup
        oldShape.SetShouldDelete();
    }
    
    // 2️⃣ Register geometry as dirty for pull-based evaluation
    var parentGeometry = GetKnParent() as KnGeometry;
    if (parentGeometry != null)
    {
        DirtyGeometryRegistry.MarkDirty(parentGeometry);
    }
    
    // 3️⃣ Clear Value and mark Unknown
    return base.Smash(OnComplete);
}
```

**Result**:
- ✅ Old shape marked ShouldDelete = true
- ✅ Geometry registered in DirtyGeometryRegistry
- ✅ Parameter marked Unknown (will trigger re-evaluation)

---

## 🧮 PHASE 4: Geometry Computation (Tier 2 Optimization)

### 4.1 MentorServices.OnComputeGeometryEvent

**File**: `FoundryMentorModeler/Mentor/MentorServices.cs:170`

**Triggered by**: Animation loop event system

```csharp
private void OnComputeGeometryEvent(ComputeGeometryEvent evt)
{
    // 🔑 KEY: Tier 2 Optimization - only evaluate DIRTY geometries
    if (DirtyGeometryRegistry.HasDirty)
    {
        var dirtyGeometries = DirtyGeometryRegistry.TakeAll();
        
        foreach (var geometry in dirtyGeometries)
        {
            // Evaluate formula → create new shape → store in cache
            geometry.GetCurrentValue();
        }
    }
    
    // Fallback: Walk all models (for non-registry scenarios)
    if (!_useRegistryOnly)
    {
        foreach (var model in MentorModel.GetAllModels())
        {
            model.EnsureGeometriesEvaluated();
        }
    }
}
```

### 4.2 KnGeometry.GetCurrentValue

**Calls formula** → `ComputeTestShape3D`

**File**: `AnimatedParameterTestComponent.cs:49`

```csharp
private bool ComputeTestShape3D(KnInstance context, List<OPResult> args, OPResult result)
{
    var width = FindNumberValue("Width", 1.0);
    var height = FindNumberValue("Height", 1.0);  // Gets NEW value
    var depth = FindNumberValue("Depth", 1.0);
    
    // 🔑 KEY: Create NEW shape instance
    var shape = new FoShape3D($"TestShape_{Name}")
    {
        GlyphId = GetKnowId(),  // Same ID as old shape
        Width = width,
        Height = height,        // NEW height
        Depth = depth
    };
    
    shape = shape.CreateBox(shape.Name!, width, height, depth);
    
    // Store in result cache
    result.SetValue(ResultStatus.Shape3D, shape);
    return true;
}
```

**Result**:
- ✅ NEW FoShape3D created with updated dimensions
- ✅ Stored in parameter's Value cache
- ✅ Parameter no longer Unknown
- ❌ Shape NOT yet added to stage (waits for RenderGeometry3D)

---

## 🎨 PHASE 5: Stage Rendering (Shape Lifecycle)

### 5.1 FoStage3D.RenderStage

**File**: `FoundryWorldsAndDrawings/Shapes3D/FoStage3D.cs:207`

```csharp
public virtual async Task RenderStage(int tick, double fps)
{
    if (_associatedScene == null) return;  // Skip orphan stages
    
    var collector = new Shape3DChangeCollector();
    var deleteThese = new List<FoGlyph3D>();
    
    // ═══════════════════════════════════════════════════
    // PASS 1: Run all animation callbacks
    // ═══════════════════════════════════════════════════
    foreach (var glyph in AllShapes())
    {
        glyph.UpdateForAnimation(tick, fps);
        
        if (glyph.IsShouldDelete())
            deleteThese.Add(glyph);  // Old shape marked by Smash
    }
    
    // ═══════════════════════════════════════════════════
    // PASS 2: Collect all stale/changed shapes
    // ═══════════════════════════════════════════════════
    foreach (var glyph in AllShapes())
    {
        glyph.CollectChanges(collector);
        // Old shape adds itself to collector.Deletions
    }
    
    // Process pending deletions (replaced shapes)
    foreach (var glyph in _pendingDeletions)
    {
        collector.AddDeletion(glyph);
    }
    _pendingDeletions.Clear();
    
    // ═══════════════════════════════════════════════════
    // Optimization: Skip if no changes
    // ═══════════════════════════════════════════════════
    if (!collector.HasAnyChanges() && !IsStale())
        return;  // Early exit
    
    // ═══════════════════════════════════════════════════
    // Send changes to JavaScript/Three.js
    // ═══════════════════════════════════════════════════
    await _associatedScene.ProcessCollectedChanges(collector, this);
    ClearAllStaleFlags();
    
    // ═══════════════════════════════════════════════════
    // CLEANUP: Remove deleted shapes from C# hierarchy
    // ═══════════════════════════════════════════════════
    foreach (var glyph in deleteThese)
    {
        this.RemoveShape<FoGlyph3D>(glyph);  // Old shape removed
    }
    foreach (var mesh in collector.Deletions)
    {
        if (mesh is FoGlyph3D glyph && !deleteThese.Contains(glyph))
            this.RemoveShape<FoGlyph3D>(glyph);
    }
}
```

**Result**:
- ✅ Old shape found with IsShouldDelete() = true
- ✅ Old shape added to collector.Deletions
- ✅ Deletion sent to JavaScript
- ✅ Old shape removed from stage C# collection
- ❌ NEW shape still not in stage (needs RenderGeometry3D)

---

## 🏗️ PHASE 6: Model Rendering (NEW with idempotent PostCreation)

### 6.1 KnModel.RenderAll3DViews

**File**: `FoundryMentorModeler/Mentor/KnModel.cs:155`

**When called**: By test harness or user code

```csharp
public virtual async Task RenderAll3DViews(bool clear, Action OnComplete)
{
    foreach (var stageName in GetViewRegistry().Get3DViews())
    {
        var stage = GetArena().EstablishStage<FoStage3D>(stageName);
        
        // Skip if scene not active (canvas unmounted)
        if (stage.GetAssociatedScene() == null)
            continue;
        
        await RenderArena3D(stageName, clear, OnComplete);
    }
}
```

### 6.2 KnModel.RenderArena3D

**File**: `FoundryMentorModeler/Mentor/KnModel.cs:183`

```csharp
public virtual async Task<IStage> RenderArena3D(string view, bool clear, Action OnComplete)
{
    var arena = GetArena();
    var stage = arena.EstablishStage<FoStage3D>(view);
    
    // Create render context
    var ctx = RenderContext3D.Create(stage, view, deep: true);
    
    if (clear)
        await stage.ClearAll();
    
    // 🔑 KEY: Recursively render component tree
    RenderGeometry3D(ctx);
    
    OnComplete?.Invoke();
    return stage;
}
```

### 6.3 KnComponent.RenderGeometry3D (UPDATED - Idempotent)

**File**: `FoundryMentorModeler/Mentor/KnComponent.cs:236`

```csharp
public virtual void RenderGeometry3D(RenderContext3D ctx)
{
    // 1️⃣ Establish geometry parameter
    var (geom, param) = EstablishGeometry3D(ctx.ViewName);
    
    // 2️⃣ Get cached value (already evaluated in Phase 4)
    var result = Geometry3DValueFor(ctx.ViewName);
    
    // 3️⃣ Get shape from result
    var shape = result.IsSuccess() ? result.AsShape3D() : null;
    
    // 4️⃣ 🔑 KEY: Call PostCreation EVERY frame (idempotent)
    if (shape != null)
    {
        ctx.PostCreation(shape, GetKnowId());
        // PostCreation → AddShape → checks if already in stage
    }
    
    // 5️⃣ Recurse into children with correct target context
    if (ctx.Deep)
    {
        var childCtx = shape != null ? ctx.ForChild(shape) : ctx;
        
        foreach (var item in Subcomponents<KnComponent>())
        {
            item.RenderGeometry3D(childCtx);
        }
    }
    
    // 6️⃣ Finalize AFTER children (for grouping/connections)
    FinalizeGeometry3D(ctx.ViewName);
}
```

### 6.4 RenderContext3D.PostCreation

**File**: `FoundryMentorModeler/Mentor/RenderContext.cs:273`

```csharp
public T PostCreation<T>(T shape, string? glyphId = null) where T : FoGlyph3D
{
    if (!string.IsNullOrEmpty(glyphId))
        shape.GlyphId = glyphId;
    
    // 🔑 KEY: Delegates to Target.AddShape (idempotent)
    Target.AddShape(shape);
    
    return shape;
}
```

### 6.5 FoStage3D.AddShape (Idempotent!)

**File**: `FoundryWorldsAndDrawings/Shapes3D/FoStage3D.cs:504`

```csharp
public override T AddShape<T>(T value)
{
    // 🔑 KEY: Check if already in THIS stage
    var existingParent = value.GetParentOfType<FoStage3D>();
    if (existingParent != null)
    {
        if (existingParent == this)
        {
            // ✅ Shape already in stage - EARLY RETURN
            return value;
        }
        // Shape in different stage - remove from there first
        existingParent.RemoveShape(value);
    }
    
    // Check for existing shape with same GlyphId (replacement)
    var existingShape = FindByGlyphId<T>(value.GlyphId);
    if (existingShape != null)
    {
        // Replace old shape with new shape (same GlyphId)
        RemoveShape(existingShape);
        _pendingDeletions.Add(existingShape);
    }
    
    // Add new shape to collection
    AddSlotItem<T>(value);
    
    // Mark stage as stale (needs redraw)
    MarkAsStale();
    
    return value;
}
```

**Result**:
- ✅ NEW shape added to stage
- ✅ Shape marked for rendering
- ✅ Idempotent: Calling again next frame does nothing (early return)

---

## 🔄 PHASE 7: Next Animation Frame (Steady State)

### 7.1 RenderStage (Next Tick)

```csharp
// PASS 1: UpdateForAnimation
// - No callbacks triggered (steady state)
// - No shapes marked for deletion

// PASS 2: CollectChanges
// - NEW shape in stage, NOT marked for deletion
// - Shape NOT stale (no changes since last frame)

// OPTIMIZATION: No changes detected
if (!collector.HasAnyChanges() && !IsStale())
    return;  // ✅ Early exit - nothing to render
```

### 7.2 RenderGeometry3D (Next Tick)

```csharp
// 1. EstablishGeometry3D - finds cached geometry
// 2. Geometry3DValueFor - returns cached shape (already evaluated)
// 3. PostCreation called - AddShape sees shape already in stage
// 4. AddShape returns early (idempotent)
// 5. No changes to stage - IsStale = false
```

**Result**: Minimal overhead, no JavaScript calls

---

## 📊 Complete Lifecycle Summary

### Initial Render (First Time)

```
1. Canvas mounts → scene.SetActive()
2. Animation loop starts
3. PreRender → no parameter changes yet
4. ComputeGeometryEvent → DirtyRegistry empty
5. RenderStage → no shapes yet
6. User calls RenderAll3DViews
7. RenderGeometry3D → Evaluate → Create shape
8. PostCreation → AddShape → Shape added to stage
9. Next RenderStage → NEW shape sent to JavaScript
```

### Parameter Change (Height updated)

```
1. SetHeight(2.0) → parameter.SetValue(2.0)
2. Parameter.Smash() triggered
   → oldShape.SetShouldDelete()
   → DirtyGeometryRegistry.MarkDirty(geometry)
   → parameter marked Unknown
3. ComputeGeometryEvent
   → geometry.GetCurrentValue()
   → ComputeTestShape3D creates NEW shape
   → NEW shape cached in parameter.Value
4. RenderStage PASS 1
   → oldShape.IsShouldDelete() = true
   → Add to deleteThese list
5. RenderStage PASS 2
   → oldShape.CollectChanges → adds to collector.Deletions
6. ProcessCollectedChanges
   → Send deletion to JavaScript
7. Cleanup
   → RemoveShape(oldShape) from C# collection
8. RenderGeometry3D
   → PostCreation(newShape)
   → AddShape(newShape)
   → newShape added to stage
9. Next RenderStage
   → newShape sent to JavaScript
```

### Steady State (No Changes)

```
1. Animation tick arrives
2. PreRender → no parameter changes
3. ComputeGeometryEvent → DirtyRegistry empty
4. RenderStage
   → No shapes marked for deletion
   → No stale shapes
   → collector.HasAnyChanges() = false
   → Early return (no JavaScript calls)
5. RenderGeometry3D (if called)
   → PostCreation → AddShape returns early
   → No stage changes
```

---

## 🎯 Key Architectural Insights

### Three-Tier Optimization

1. **Tier 1: IsActive** (Component-Level)
   - Canvas mounted → scene.IsActive = true
   - Canvas unmounted → scene.IsActive = false
   - Skips entire stage if scene inactive

2. **Tier 2: DirtyGeometryRegistry** (Parameter-Level)
   - Only evaluates geometries that were Smashed
   - O(dirty) instead of O(all geometries)
   - Massive performance win for large models

3. **Tier 3: IsStale** (Shape-Level)
   - Skips JavaScript export if no changes
   - collector.HasAnyChanges() check
   - Avoids expensive interop when nothing changed

### Idempotent PostCreation

**Old Approach** (wasUnknown flag):
```csharp
if (wasUnknown && shape != null)
    ctx.PostCreation(shape);
```
**Problem**: DirtyRegistry evaluates BEFORE RenderGeometry3D, so wasUnknown=false for recreated shapes

**New Approach** (idempotent):
```csharp
if (shape != null)
    ctx.PostCreation(shape);  // Safe to call every frame
```
**Solution**: AddShape checks if shape already in stage, returns early if so

### Correct Order for Connections/Grouping

```csharp
// 1. Evaluate geometry → get shape
var result = Geometry3DValueFor(ctx.ViewName);
var shape = result.AsShape3D();

// 2. Add parent shape to stage FIRST
ctx.PostCreation(shape, GetKnowId());

// 3. Recurse into children (target = parent shape)
foreach (var child in Subcomponents())
    child.RenderGeometry3D(ctx.ForChild(shape));

// 4. Finalize AFTER children (grouping/connections)
FinalizeGeometry3D(ctx.ViewName);
```

**Why**: Children need parent as target context. Finalize may connect children to parent.

---

## 🐛 Bug That Was Fixed

### The Deletion Loop Bug

**Symptoms**:
- Console shows: "🗑️ marked for deletion" every frame
- Shape visible in tree but not rendering
- Infinite loop

**Root Cause**:
1. Height changes → Smash marks oldShape for deletion
2. DirtyRegistry evaluates geometry → creates newShape
3. Parameter no longer Unknown (already evaluated)
4. RenderGeometry3D checks wasUnknown → FALSE
5. PostCreation skipped → newShape NEVER added to stage
6. oldShape deleted from stage
7. Next frame: geometry has reference to deleted shape
8. CollectChanges finds it → marks for deletion again
9. Repeat forever

**Fix**: Make PostCreation idempotent, call every frame
- AddShape checks if already in stage
- If yes: return early (no-op)
- If no: add shape
- Works for both initial creation AND recreation

---

## 📝 Files Modified During Fix

1. **KnGeometryParameter.cs** - Removed _needsPostCreation flag (not needed)
2. **KnComponent.cs** - RenderGeometry3D now calls PostCreation unconditionally
3. **KnComponent.cs** - Moved Finalize AFTER recursion (for grouping)
4. **RenderContext.cs** - PostCreation already idempotent (no changes needed)
5. **FoStage3D.cs** - AddShape already had idempotent check (no changes needed)

---

## 🎓 Lessons Learned

1. **Idempotency is powerful** - Simpler than state tracking
2. **Trust the optimization tiers** - They handle performance
3. **Order matters** - Parent before children, finalize after recursion
4. **Timing is critical** - DirtyRegistry evaluates before RenderGeometry3D
5. **Test at boundaries** - Initial render AND parameter changes AND steady state
