# Shape Rendering Debug Session - December 20, 2025

## Status: JavaScript Receiving Data, THREE.js Not Rendering

---

## What We Changed Today

### C# Changes

1. **FoStage3D.cs** (FoundryWorldsAndDrawings/Shapes3D/)
   - **Line 537**: Changed slot storage from `DynamicSlot(value.GetType())` to `DynamicSlot(typeof(FoGlyph3D))`
   - **Line 587**: Changed RemoveShape to use same slot type
   - **Reason**: Was storing shapes as derived type (FoShape3D), RenderStage was looking for base type (FoGlyph3D)
   - **Result**: ✅ Shapes now found during collection

2. **RenderContext.cs** (FoundryMentorModeler/Mentor/)
   - **Converted to records** with primary constructors
   - **ForChild method**: Changed from mutation to immutable copy using `with` expression
   - **Before**: `Target = parentShape; return this;` (MUTATION BUG)
   - **After**: `return this with { Target = parentShape };` (IMMUTABLE)
   - **Reason**: ForChild was corrupting parent context causing stack overflow
   - **Result**: ✅ Hierarchical rendering works

3. **ShapeLifecycleTest.razor.cs** (Three2025/Components/Pages/)
   - **Line 175**: Changed from `await _canvasRef.RenderFrame()` to `await _stage.RenderStage(tick, fps)`
   - **Reason**: Animations paused, so animation loop doesn't call RenderStage automatically
   - **Result**: ✅ RenderStage executes on manual trigger

4. **Scene3D.cs** (FoundryWorldsAndDrawings/ThreeD/Viewers/)
   - **Added extensive logging** to ProcessCollectedChanges and SendBatchedUpdate
   - **Line 231**: Now logs full JSON payload being sent to JavaScript
   - **Result**: ✅ Can verify exact data sent to JavaScript

5. **FoShape3D.cs** (FoundryWorldsAndDrawings/Shapes3D/)
   - **Added logging** to RecomputeMesh and AsMesh3D methods
   - **Result**: ✅ Can verify geometry creation in C#

### JavaScript Changes

1. **index.ts** (FoundryWorldsAndDrawings/JsLib/src/)
   - **request3DBatchedUpdate**: Added extensive logging
   - **getViewerFromSettings**: Added viewer lookup diagnostics
   - **Result**: ✅ Can verify JavaScript receives data and finds viewer

2. **Viewer3D.ts** (FoundryWorldsAndDrawings/JsLib/src/Viewer/)
   - **processBatchOperations**: Added logging for operation counts
   - **Line 142**: Added logging for viewerId registration
   - **Result**: ✅ Can verify viewer registration and operation routing

3. **Constructors.ts** (FoundryWorldsAndDrawings/JsLib/src/Utils/)
   - **establish3DChildren**: Added logging for children processing
   - **maker dispatch**: Added logging for maker function calls
   - **Result**: ✅ Can verify maker function execution

---

## How The System SHOULD Work

### Architecture Overview

```
C# Side:                                    JavaScript Side:
┌─────────────────────┐                    ┌──────────────────────┐
│ FoShape3D           │                    │ Viewer3D             │
│  - GeomType="Box"   │                    │  - scene: Scene      │
│  - Width=100        │                    │  - viewerId          │
│  - RecomputeMesh()  │                    │  - processBatch..()  │
└──────────┬──────────┘                    └──────────┬───────────┘
           │                                          │
           ▼                                          ▼
┌─────────────────────┐                    ┌──────────────────────┐
│ FoStage3D           │                    │ Constructors         │
│  - AddShape()       │                    │  - makers Map        │
│  - RenderStage()    │                    │  - establish3D..()   │
│  - CollectChanges() │                    │  - MeshBuilder       │
└──────────┬──────────┘                    └──────────┬───────────┘
           │                                          │
           ▼                                          ▼
┌─────────────────────┐                    ┌──────────────────────┐
│ Scene3D             │  ═══ JSON ═══>    │ THREE.Scene          │
│  - ProcessCollected │     over JSInterop │  - add(mesh)         │
│  - SendBatchedUpdate│                    │  - render()          │
└─────────────────────┘                    └──────────────────────┘
```

### Step-by-Step Flow

#### Phase 1: C# Shape Creation ✅ WORKING
1. User clicks "Create Shape"
2. ModelEditor creates AnimatedParameterTestComponent
3. Component.EstablishGeometry3D creates FoShape3D with GeomType="Box", Width=100, Height=100, Depth=100
4. PartComponent.RenderGeometry3D calls shape.PostCreation(context)
5. PostCreation calls stage.AddShape(shape) → stored in FoGlyph3D slot
6. Shape marked with all stale flags (IsGeometryStale=true, IsStructureStale=true)

**Verification Logs**:
```
✅ Shape created - 
🔷 AddShape: Added '' (GlyphId=...) to stage 'manual-test-stage', IsStale=True
```

#### Phase 2: C# Geometry Computation ✅ WORKING
1. User clicks "Render"
2. Test calls `await _stage.RenderStage(tick, fps)`
3. RenderStage → CollectChanges → shape is STALE → added to collector.Independents
4. ProcessCollectedChanges iterates Independents
5. Calls shape.GetComputedMesh(sceneName)
6. GetComputedMesh → shape.RecomputeMesh()
7. FoShape3D.RecomputeMesh → AsMesh3D()
8. AsMesh3D creates Mesh3D with BoxGeometry(100, 100, 100)
9. Returns complete Mesh3D with material, geometry, transform, uuid, type="Mesh3D"

**Verification Logs**:
```
🔧 FoShape3D.RecomputeMesh: GeomType='Box' IsGeometryStale=True
🔨 Creating NEW mesh for GeomType='Box'
🎨 AsMesh3D: Creating mesh GeomType='Box' W=100 H=100 D=100
📦 ProcessAndAddToOperations: → GeometryUpdates
```

#### Phase 3: C# JSON Export ✅ WORKING
1. ProcessAndAddToOperations adds mesh to operations.GeometryUpdates
2. SendBatchedUpdate serializes BatchedSceneUpdateDTO
3. JSON contains:
   - `sceneOperations["manual-test-stage"].geometryUpdates[0]`
   - `type: "Mesh3D"` ← KEY FOR MAKER LOOKUP
   - `geometry: { type: "BoxGeometry", width: 100, height: 100, depth: 100 }`
   - `material: { type: "MeshStandardMaterial", color: "Green" }`
   - `transform: { position: {x:0, y:0, z:0} }`
   - `uuid: "940f9177-febd-49ae-8367-6016d562a674"`
4. Calls `JsRuntime.InvokeVoidAsync("FoundryWorldsAndDrawings.request3DBatchedUpdate", json)`

**Verification Logs**:
```
📤 SendBatchedUpdate: Scene 'manual-test-stage' sending 3743 chars to JavaScript
📋 FULL JSON: { "sceneOperations": { "manual-test-stage": { "geometryUpdates": [...] } } }
```

#### Phase 4: JavaScript Reception ✅ WORKING
1. `window.FoundryWorldsAndDrawings.request3DBatchedUpdate(batchJson)` called
2. Parses JSON → batch.sceneOperations
3. Iterates sceneOperations dictionary
4. For sceneName="manual-test-stage", finds operations
5. Calls `getViewerFromSettings("manual-test-stage")`
6. Returns viewer from `viewManager.ViewerLookup["manual-test-stage"]`
7. Calls `viewer.processBatchOperations("manual-test-stage", operations)`

**Verification Logs** (expected):
```
📥 request3DBatchedUpdate called with 3743 chars
✅ Batch parsed - sceneOperations: ["manual-test-stage"]
🔄 Processing scene: manual-test-stage
🔍 getViewerFromSettings: Looking for viewer with sceneName='manual-test-stage'
✅ Found viewer for scene 'manual-test-stage'
✅ Found viewer for: manual-test-stage, calling processBatchOperations
```

#### Phase 5: JavaScript Batch Processing ⚠️ NEEDS VERIFICATION
1. `viewer.processBatchOperations(sceneName, operations)`
2. Checks `operations.geometryUpdates.length` → should be 1
3. Creates `geoOptions = { children: operations.geometryUpdates, sceneName }`
4. Calls `Constructors.establish3DChildren(geoOptions, this.scene)`

**Verification Logs** (expected):
```
🔧 processBatchOperations START: { geometryUpdates: 1 }
🎨 Processing geometryUpdates: 1 meshes
📦 First geometry item: {...}
🔨 Calling Constructors.establish3DChildren
```

#### Phase 6: THREE.js Mesh Creation ❌ SUSPECTED FAILURE POINT
1. `Constructors.establish3DChildren(options, parent)`
2. Iterates `options.children` (the geometryUpdates array)
3. For each element:
   - Gets `element.type` → should be "Mesh3D"
   - Looks up `this.makers.get("Mesh3D")`
   - Maker function should be `this.establish3DGeometry.bind(this)`
4. Calls `establish3DGeometry(element, parent)` where parent is THREE.Scene
5. MeshBuilder.CreateMesh(element):
   - MeshBuilder.ConstructGeometry(element.geometry) → creates THREE.BoxGeometry(100, 100, 100)
   - MeshBuilder.ConstructMaterial(element.material) → creates THREE.MeshStandardMaterial({color: "Green"})
   - Creates THREE.Mesh(geometry, material)
   - Sets mesh.uuid = element.uuid
   - Returns { mesh, geometry, material, entity }
6. ObjectLookup.addPrimitive(uuid, mesh)
7. **CRITICAL**: `parent.add(entity)` → ADDS TO THREE.SCENE
8. MeshBuilder.ApplyMeshTransform(element, entity) → sets position/rotation/scale

**Verification Logs** (expected):
```
🔨 establish3DChildren called with: { childrenCount: 1 }
📦 establish3DChildren [0]: type=Mesh3D, uuid=940f9177-...
✅ Found maker for type='Mesh3D', calling it...
✅ Maker completed for type='Mesh3D'
```

---

## What We've VERIFIED Is Working

### C# Pipeline ✅
- [x] Shape created with correct GeomType="Box"
- [x] Shape added to FoGlyph3D slot in stage
- [x] Shape marked with stale flags
- [x] RenderStage collects shape in Independents
- [x] GetComputedMesh creates BoxGeometry(100,100,100)
- [x] Mesh3D has type="Mesh3D"
- [x] JSON serialization produces correct structure
- [x] JSON sent to JavaScript via JSInterop

### JavaScript Reception ✅
- [x] request3DBatchedUpdate receives 3743 char JSON
- [x] JSON parses successfully
- [x] sceneOperations contains "manual-test-stage" key
- [x] Viewer lookup finds viewer (based on earlier logs showing "Found viewer")

### Remaining Unknowns ❓
- [ ] Does `processBatchOperations` see geometryUpdates array?
- [ ] Does `establish3DChildren` get called with children?
- [ ] Does maker lookup find "Mesh3D" → establish3DGeometry?
- [ ] Does MeshBuilder.CreateMesh execute?
- [ ] Does `parent.add(entity)` add mesh to THREE.Scene?
- [ ] Does THREE.Scene.children contain the mesh after add?
- [ ] Does the animation loop call `renderer.render(scene, camera)`?

---

## The Probable Issue

Based on logs showing:
- ✅ `establish3DChildren completed` (in earlier logs)
- ❌ No mesh visible in viewer

**Hypothesis**: One of these is happening:

1. **Maker Not Found**: 
   - `this.makers.get("Mesh3D")` returns undefined
   - Shape type doesn't match registered maker key
   - Fix: Verify makers Map has "Mesh3D" → establish3DGeometry binding

2. **Parent Is Wrong**:
   - `parent` parameter to establish3DChildren is NOT the THREE.Scene
   - `parent.add(entity)` adds to wrong container
   - Fix: Verify processBatchOperations passes `this.scene` correctly

3. **Mesh Created But Not Added**:
   - MeshBuilder.CreateMesh succeeds
   - But `parent.add(entity)` not called or fails silently
   - Fix: Add logging inside establish3DGeometry after parent.add()

4. **Scene Not Rendering**:
   - Mesh added to scene successfully
   - But viewer not triggering render frame
   - Animation loop paused or not calling RefreshLabelsAndRender()
   - Fix: Verify animation loop runs and calls webGLRenderer.render(scene, camera)

---

## Next Steps (When Fresh)

### 1. Verify Maker Registration
Check `Constructors.ts` constructor:
```typescript
this.makers.set('Mesh3D', this.establish3DGeometry.bind(this));
```

### 2. Add Critical Logging
Inside `establish3DGeometry` after mesh creation:
```typescript
console.log('🎨 MeshBuilder.CreateMesh returned:', result);
console.log('🎨 Adding entity to parent:', parent.type, parent.uuid);
parent.add(entity);
console.log('✅ Entity added to parent. Parent children count:', parent.children.length);
console.log('🔍 Parent children:', parent.children.map(c => c.type + ':' + c.uuid));
```

### 3. Verify Scene Contents
After processBatchOperations completes:
```typescript
console.log('🔍 Scene children after batch:', this.scene.children.length);
console.log('🔍 Scene children types:', this.scene.children.map(c => c.type));
```

### 4. Force Render
After batch processing:
```typescript
this.webGLRenderer.render(this.scene, this.camera);
console.log('🎨 Forced render called');
```

### 5. Check Scene Graph
Use browser console:
```javascript
viewer = window['manual-test-stage']
viewer.scene.children  // Should show AmbientLight, PointLight, GridHelper, AND Mesh
```

---

## Critical Code Locations

### C#
- **Shape Creation**: `PartComponent.RenderGeometry3D()` line 320-351
- **Slot Storage**: `FoStage3D.AddShape()` line 537
- **Collection**: `FoStage3D.RenderStage()` line 217-248
- **Geometry Build**: `FoShape3D.AsMesh3D()` line 271-293
- **JSON Export**: `Scene3D.SendBatchedUpdate()` line 226-234

### JavaScript
- **Entry Point**: `index.ts` request3DBatchedUpdate line 192-220
- **Viewer Lookup**: `index.ts` getViewerFromSettings line 143-159
- **Batch Routing**: `Viewer3D.ts` processBatchOperations line 440-498
- **Children Processor**: `Constructors.ts` establish3DChildren line 282-313
- **Mesh Creator**: `Constructors.ts` establish3DGeometry line 53-111
- **Geometry Builder**: `MeshBuilder.ts` ConstructGeometry
- **Material Builder**: `MeshBuilder.ts` ConstructMaterial

---

## Working State (Earlier Today)

The system was rendering shapes successfully. Changes made today:
1. Slot type fix (necessary - was causing collection failure)
2. RenderContext immutability (necessary - was causing stack overflow)
3. Direct RenderStage call (necessary - animations paused)
4. Extensive logging (diagnostic only)

**None of these should break rendering** - they fix collection and call issues.

**Most Likely Culprit**: Something subtle in JavaScript changed or wasn't committed. The minified `app-lib.js` may not match the TypeScript source. Verify:
- Latest TypeScript compiled to app-lib.js
- Browser loaded latest app-lib.js (hard refresh required)
- No JavaScript errors in console
- Viewer registered with correct containerId

---

## What Was Definitely Working Before

From conversation context: "All that was working just 5 hrs ago"

The rendering pipeline was functional. Shapes created in C# were appearing in THREE.js viewer. This suggests:
- Maker registration was correct
- parent.add(entity) was working  
- Scene rendering was working
- Viewer lookup was working

**Something changed in the last 5 hours** - likely related to:
- The slot type changes (but those are necessary fixes)
- The RenderContext record conversion (but that fixes stack overflow)
- Build/deploy cycle (JavaScript not updated?)
- Browser cache (old JavaScript running?)

---

## Recovery Strategy

1. **Verify Build**: Ensure `npm run build` completed successfully
2. **Verify Deploy**: Ensure app-lib.js copied to wwwroot/js/
3. **Hard Refresh**: Ctrl+F5 to clear browser cache
4. **Check Console**: Look for JavaScript errors during batch processing
5. **Inspect Scene**: Use browser console to check scene.children
6. **Force Render**: Manually call renderer.render() to rule out animation loop issue
7. **Compare Git**: If still broken, git diff to see what actually changed in last 5 hours

---

## Summary

**C# Side**: ✅ Perfect - shape created, geometry computed, JSON exported
**JavaScript Side**: ✅ Receiving data correctly
**THREE.js Side**: ❌ Unknown - need to verify mesh added to scene and rendered

The issue is in Phase 6 (THREE.js mesh creation/addition) or animation loop not triggering render. Fresh eyes needed to trace JavaScript execution from `processBatchOperations` through `establish3DGeometry` to `parent.add(entity)` and verify scene contents.
