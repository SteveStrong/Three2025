
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.PubSub;
using Three2025.Services.Visualization;

using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using Unglide;


namespace Three2025.Components.Pages;

public partial class SpacialBoxTest : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IGeometryVisualizationService VisualizationService { get; set; }

    public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;
    protected SpacialBox3D CurrentBox;

    // Box properties for UI binding
    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.5;
    protected double BoxDepth { get; set; } = 1.0;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = string.Empty;



    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false,null!);

            scene?.SetAfterUpdateAction((s,j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = FoundryService.Arena();
            if (found)
            {
                arena.SetScene(scene!);
                DoRequestAxisToScene(scene!);
                CreateSpacialBox();
            }
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    public void DoRequestAxisToScene(Scene3D scene)
    {
        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        scene.AddChild(model);
    }

    public void CreateSpacialBox()
    {
        try
        {
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            arena.ClearArena();

            //ok you need to remember that for spacialbox it is in a local coord system with 0,0,0 being the 
            // left , bottom, back corner
            //we should test by drawing the axis and then drawing the box
            //then we can see where the box is in relation to the axis

            // 🎯 PIVOT SYSTEM: The key to our 3D positioning
            // Pivot = Shifted Center of Gravity - where the object's center should be
            // JavaScript creates a group and OFFSETS the geometry so pivot becomes the new center
            // This allows natural floor contact, door hinges, corner balancing, etc.

            var boxShape = new FoShape3D()
            {
                Name = "SpacialBoxMain",
                GlyphId = Guid.NewGuid().ToString(),
                Color = "#4CAF50",
                Opacity = 0.8,
                Transform = new Transform3("BoxTransform")  // ⚠️ Transform assigned in object initializer
            }.CreateBox("SpacialBoxMain", BoxWidth, BoxHeight, BoxDepth);

            // 🚨 CRITICAL INITIALIZATION FIX: Properties set AFTER transform is assigned to shape
            // This ensures the parent-child relationship exists for dirty flag propagation
            boxShape.Transform.Position = new Vector3(0, 0, 0);
            boxShape.Transform.Rotation = Euler.FromDegrees(0, 0, 0);
            boxShape.Transform.Scale = new Vector3(1, 1, 1);
            boxShape.Transform.Pivot = new Vector3(0, -BoxHeight/2, 0);  // 🏠 FLOOR CONTACT

            $"� Transform properties set via property setters (should trigger dirty flags)".WriteInfo();

            // Debug: Log the transform values to verify floor positioning
            $"📦 Box Created - Position: {boxShape.Transform.Position}, Pivot: {boxShape.Transform.Pivot}".WriteInfo();
            $"📦 Pivot shifts geometry so bottom face sits at group center (floor contact at Y=0)".WriteInfo();

            // 🚨 INITIALIZATION FIX: Force transform matrix calculation and ensure dirty flag is set
            var matrix = boxShape.Transform.ToMatrix3();  // Force matrix calculation
            boxShape.SetDirty(true);  // Ensure shape is marked dirty for initial render
            $"🔧 Initial transform matrix calculated and shape marked dirty for rendering".WriteInfo();

             arena.AddShapeToStage<FoShape3D>(boxShape);

            CurrentBox = new SpacialBox3D(boxShape, "m");

            // 🔗 Setup transform change monitoring
            SetupTransformMonitoring(boxShape);

            StatusMessage = $"Created SpacialBox3D: {BoxWidth}×{BoxHeight}×{BoxDepth}m with bottom face on floor (Y=0)";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating box: {ex.Message}";
            StateHasChanged();
        }
    }

    /// <summary>
    /// 🔗 Setup transform change monitoring to verify dirty flag propagation
    /// </summary>
    private void SetupTransformMonitoring(FoShape3D shape)
    {
        if (shape?.Transform != null)
        {
            // Monitor transform changes
            shape.Transform.OnChange = (isDirty) => {
                $"🔔 Transform.OnChange fired: isDirty={isDirty} for {shape.Transform.OwnerName}".WriteInfo();
                if (isDirty)
                {
                    $"   ✅ Transform marked dirty - cache invalidated".WriteInfo();
                }
                else
                {
                    $"   ✅ Transform marked clean - matrix calculated".WriteInfo();
                }
            };

            // Monitor matrix computation completion
            shape.Transform.OnComputed = (matrix) => {
                $"🎯 Transform.OnComputed fired for {shape.Transform.OwnerName}".WriteInfo();
                $"   Matrix computed and cached - transform is now stable".WriteInfo();
            };

            $"🔗 Transform monitoring setup for {shape.Transform.OwnerName}".WriteInfo();
        }
    }


    public void ClearAll()
    {
        var arena = FoundryService.Arena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }

        arena.ClearArena();
        StateHasChanged();
    }





    public void Dispose()
    {
        // Cleanup resources if needed
    }

    // === PRESET SHAPES ===
    protected void CreateCube()
    {
        BoxWidth = BoxHeight = BoxDepth = 2.0;
        CreateSpacialBox();
    }

    protected void CreateLongBox()
    {
        BoxWidth = 4.0; BoxHeight = 1.0; BoxDepth = 1.0;
        CreateSpacialBox();
    }

    protected void CreateTallBox()
    {
        BoxWidth = 1.0; BoxHeight = 4.0; BoxDepth = 1.0;
        CreateSpacialBox();
    }

    protected void CreateWideBox()
    {
        BoxWidth = 1.0; BoxHeight = 1.0; BoxDepth = 4.0;
        CreateSpacialBox();
    }

    protected void CreateTinyBox()
    {
        BoxWidth = BoxHeight = BoxDepth = 0.5;
        CreateSpacialBox();
    }



    public void ShowQuadrants()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var arena = FoundryService.Arena();
        if (arena == null) return;

        var center = CurrentBox.Center;
        var quadrantSize = 0.2;
        var colors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FFA500", "#800080" };

        // Create 8 quadrant markers (for a 3D box)
        var quadrants = new[]
        {
            new Point3D(center.X + quadrantSize, center.Y + quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y + quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y - quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y - quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y + quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y + quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y - quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y - quadrantSize, center.Z - quadrantSize)
        };

        for (int i = 0; i < quadrants.Length; i++)
        {
            VisualizationService.CreateMarkerSphere(arena, $"Quadrant{i}", quadrants[i], colors[i], 0.04);
        }

        StatusMessage = "Showing 8 3D quadrants around center";
        StateHasChanged();
    }

        public void ShowSubModel()
        {
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            var model = new FoModel3D()
            {
                Name = "Submarine",
                Url = GetReferenceTo(@"storage/StaticFiles/sub.glb"),

            }.CreateModel("sub", GetReferenceTo(@"storage/StaticFiles/sub.glb"), 12.0, 4.5, 4.5);

            arena.AddShapeToStage<FoModel3D>(model);

            var box = new SpacialFrame3D(model, "m");

            var vertices = box.GetVertices();
            VisualizationService.ShowLabeledVertices(arena, vertices);

            var edges = box.GetEdges();
            VisualizationService.ShowLabeledEdges(arena, edges);

            var faces = box.GetFaces();
            VisualizationService.ShowLabeledFaces(arena, faces);
            VisualizationService.ShowLabeledNormals(arena, faces);

            StatusMessage = "Submarine model added to scene.";
            StateHasChanged();
        }
 




        // === VISUALIZATION TEST METHODS ===
        public void ShowVertices()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var vertices = CurrentBox.GetLocalVertices();
            VisualizationService.ShowLabeledVertices(arena, vertices);
            StatusMessage = $"Showing {vertices.Count} vertices as labeled spheres.";
            StateHasChanged();
        }

        public void ShowEdges()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
           var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            var edges = CurrentBox.GetLocalEdges();
            VisualizationService.ShowLabeledEdges(arena, edges);
            StatusMessage = $"Showing {edges.Count} edges as labeled tubes.";
            StateHasChanged();
        }

        public void ShowFaces()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var faces = CurrentBox.GetLocalFaces();
            VisualizationService.ShowLabeledFaces(arena, faces);
            StatusMessage = $"Showing {faces.Count} faces as wireframe outlines with labels.";
            StateHasChanged();
        }

        public void ShowNormals()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = FoundryService.Arena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            
            var faces = CurrentBox.GetLocalFaces();
            VisualizationService.ShowLabeledNormals(arena, faces);
            StatusMessage = $"Showing {faces.Count} face normals as red cylinders, aligned with normals.";
            StateHasChanged();
        }

        // === ANIMATED PIVOT TESTS ===
        
        /// <summary>
        /// 🚪 Door Hinge Animation - Pivot on left edge and swing open like a door
        /// 
        /// 🎯 PIVOT CONCEPT: Sets pivot to left-bottom edge, making that the new center of gravity.
        /// JavaScript shifts the box geometry so the edge becomes the rotation center.
        /// When we rotate around Y-axis, box swings like a real door on hinges.
        /// </summary>
        protected void AnimateDoorHinge()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }

            // Set pivot to left edge bottom (door hinge position)
            // 🚪 SHIFTED GRAVITY: Left edge becomes the new center - box rotates around this point
            var pivotPosition = new Vector3(-BoxWidth/2, -BoxHeight/2, 0);
            SetPivotAndReset(pivotPosition, "Door Hinge");

            // Animate Y-rotation to swing the door open (90 degrees over 2 seconds)
            if (CurrentBox.Shape?.Transform != null)
        {
                var transform = CurrentBox.Shape.Transform;
                var tweener = new Tweener();
                tweener.Tween(transform.Rotation, new { Y = Math.PI/2 }, 2.0f)
                    .Ease(Ease.BackOut)
                    .OnComplete(() => {
                        CurrentBox.Shape.SetDirty(true); // Mark shape dirty after animation
                        StatusMessage = "🚪 Door swung open! Pivot at left edge.";
                        StateHasChanged();
                    });
                
                StatusMessage = "🚪 Door hinge animation started...";
                StateHasChanged();
            }
        }

        /// <summary>
        /// ⚖️ Corner Balance Animation - Pivot on bottom corner and wobble
        /// 
        /// 🎯 PIVOT CONCEPT: Sets pivot to corner, making that the new center of gravity.
        /// JavaScript shifts the box so the corner becomes the balance point.
        /// Perfect for simulating balancing on a single point like real physics.
        /// </summary>
        protected void AnimateCornerBalance()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }

            // Set pivot to bottom corner (dramatic balancing point)
            // ⚖️ CORNER BALANCE: Extreme shifted gravity - box balances on single corner
            var pivotPosition = new Vector3(-BoxWidth/2, -BoxHeight/2, -BoxDepth/2);
            SetPivotAndReset(pivotPosition, "Corner Balance");

            // Animate a wobbling effect around X and Z axes
            if (CurrentBox.Shape?.Transform != null)
            {
                var tweener = new Tweener();
                
                // Wobble around X axis
                tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { X = Math.PI/8 }, 1.0f)
                    .Ease(Ease.SineInOut)
                    .Repeat()
                    .Reflect();

                // Wobble around Z axis (slightly offset timing)
                tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { Z = Math.PI/12 }, 1.3f, 0.2f)
                    .Ease(Ease.SineInOut)
                    .Repeat()
                    .Reflect();

                StatusMessage = "⚖️ Box balancing on corner! Watch it wobble!";
                StateHasChanged();
            }
        }

        /// <summary>
        /// 🌀 Center Spin Animation - Pivot at center and rotate continuously
        /// </summary>
        protected void AnimateCenterSpin()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }

            // Set pivot to center (geometric center)
            var pivotPosition = new Vector3(0, 0, 0);
            SetPivotAndReset(pivotPosition, "Center Spin");

            // Animate continuous rotation around all axes
            if (CurrentBox.Shape?.Transform != null)
            {
                var tweener = new Tweener();
                
                // Rotate around Y axis (main spin)
                tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { Y = Math.PI * 4 }, 4.0f)
                    .Rotation()
                    .Repeat();

                // Slight tumble around X axis
                tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { X = Math.PI/4 }, 3.0f, 0.5f)
                    .Ease(Ease.SineInOut)
                    .Repeat()
                    .Reflect();

                StatusMessage = "🌀 Box spinning around its center!";
                StateHasChanged();
            }
        }

        /// <summary>
        /// 🏠 Reset to Floor Animation - Smooth return to floor position
        /// </summary>
        protected void AnimateResetToFloor()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }

            if (CurrentBox.Shape?.Transform != null)
            {
                var tweener = new Tweener();
                
                // Animate back to floor position (bottom pivot)
                var floorPivot = new Vector3(0, -BoxHeight/2, 0);
                var floorRotation = Euler.FromDegrees(0, 0, 0);
                var floorPosition = new Vector3(0, 0, 0);

                // Animate pivot change
                tweener.Tween(CurrentBox.Shape.Transform.Pivot, new { 
                        X = floorPivot.X, 
                        Y = floorPivot.Y, 
                        Z = floorPivot.Z 
                    }, 1.5f)
                    .Ease(Ease.BackOut);

                // Animate rotation reset
                tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { 
                        X = floorRotation.X, 
                        Y = floorRotation.Y, 
                        Z = floorRotation.Z 
                    }, 1.5f)
                    .Ease(Ease.BackOut);

                // Animate position reset
                tweener.Tween(CurrentBox.Shape.Transform.Position, new { 
                        X = floorPosition.X, 
                        Y = floorPosition.Y, 
                        Z = floorPosition.Z 
                    }, 1.5f)
                    .Ease(Ease.BackOut)
                    .OnComplete(() => {
                        StatusMessage = "🏠 Box reset to floor position with bottom pivot.";
                        StateHasChanged();
                    });
            }
        }

        /// <summary>
        /// Helper method to set pivot and reset position/rotation
        /// </summary>
        private void SetPivotAndReset(Vector3 newPivot, string testName)
        {
            if (CurrentBox?.Shape?.Transform != null)
            {
                // 🧪 Test dirty flag system
                var transform = CurrentBox.Shape.Transform;
                var shapeBeforeDirty = CurrentBox.Shape.IsDirty;
                var transformBeforeDirty = transform.IsDirty;
                
                $"🧪 BEFORE {testName} - Shape.IsDirty: {shapeBeforeDirty}, Transform.IsDirty: {transformBeforeDirty}".WriteInfo();

                // Set new pivot
                transform.Pivot = newPivot;
                $"🔄 After Pivot Set - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                transform.Position = new Vector3(0, 0, 0);
                $"🔄 After Position Set - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                transform.Rotation = Euler.FromDegrees(0, 0, 0);
                $"🔄 After Rotation Set - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();

                // 🔄 Force transform matrix calculation and refresh
                var matrix = transform.ToMatrix3();
                
                // 🎯 Refresh the shape to the scene
                var arena = FoundryService.Arena();
                if (arena != null)
                {
                    var (found, scene) = arena.CurrentScene();
                    if (found)
                    {
                        CurrentBox.Shape.RefreshToScene(scene);
                    }
                }

                StatusMessage = $"🎯 {testName} pivot set to ({newPivot.X:F2}, {newPivot.Y:F2}, {newPivot.Z:F2}) - Dirty flags verified";
                StateHasChanged();
            }
            else
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
            }
        }

        /// <summary>
        /// 🧪 Verify that dirty flags are working correctly
        /// </summary>
        protected void VerifyDirtyFlags()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }

            var transform = CurrentBox.Shape?.Transform;
            if (transform != null)
            {
                var shapeDirty = CurrentBox.Shape.IsDirty;
                var transformDirty = transform.IsDirty;
                
                $"🧪 DIRTY FLAG STATUS:".WriteInfo();
                $"   Shape.IsDirty: {shapeDirty}".WriteInfo();
                $"   Transform.IsDirty: {transformDirty}".WriteInfo();
                $"   Transform.OwnerName: {transform.OwnerName}".WriteInfo();
                
                StatusMessage = $"🧪 Dirty Flag Status:\n" +
                              $"Shape.IsDirty: {shapeDirty}\n" +
                              $"Transform.IsDirty: {transformDirty}\n" +
                              $"Transform Owner: {transform.OwnerName}";
                StateHasChanged();
            }
        }

        /// <summary>
        /// 🔄 Test dirty flag propagation by making a small change
        /// </summary>
        protected void TestDirtyFlagPropagation()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }

            var transform = CurrentBox.Shape?.Transform;
            if (transform != null)
            {
                $"🧪 TESTING DIRTY FLAG PROPAGATION:".WriteInfo();
                $"   BEFORE - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                // Make a tiny change to position
                var currentPos = transform.Position;
                transform.Position = new Vector3(currentPos.X + 0.001, currentPos.Y, currentPos.Z);
                
                $"   AFTER Position Change - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                // Reset back
                transform.Position = currentPos;
                
                $"   AFTER Position Reset - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                StatusMessage = $"🧪 Dirty flag propagation test completed - check console for details";
                StateHasChanged();
            }
        }

        /// <summary>
        /// 🎬 Test that tweener animations properly trigger dirty flags
        /// 
        /// 🔥 CRITICAL UNDERSTANDING: Property mutation vs object replacement
        /// ❌ transform.Rotation.Y += 0.1  - Mutates existing object, NO dirty flag
        /// ✅ transform.Rotation = new Euler(...) - Creates new object, triggers dirty flag
        /// ✅ shape.SetDirty(true) - Manual dirty flag after mutations
        /// </summary>
        protected void TestTweenerDirtyFlags()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }

            var transform = CurrentBox.Shape?.Transform;
            if (transform != null)
            {
                $"🎬 TESTING TWEENER DIRTY FLAG TRIGGERING:".WriteInfo();
                $"   BEFORE Tweener - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                // 🧪 DEMONSTRATION: Show both working patterns
                $"   🔍 Testing OBJECT REPLACEMENT pattern (recommended)".WriteInfo();
                
                // Count how many times OnChange is triggered during animation
                int changeCount = 0;
                var originalOnChange = transform.OnChange;
                transform.OnChange = (isDirty) => {
                    changeCount++;
                    $"   🔔 Tweener triggered OnChange #{changeCount}: isDirty={isDirty}".WriteInfo();
                    originalOnChange?.Invoke(isDirty);
                };

                // ✅ CORRECT: Object replacement pattern - creates new Euler objects
                var startRotation = transform.Rotation;
                var tweener = new Tweener();
                
                // Simple rotation animation that will trigger object replacement
                tweener.Tween(transform.Rotation, new { Y = startRotation.Y + 0.5 }, 1.0f)
                    .OnUpdate((progress) => {
                        // ✅ PATTERN 1: Object replacement happens automatically via property setter
                        var current = transform.Rotation;
                        $"   📝 Animation progress: {progress:F2}, Rotation.Y = {current.Y:F3}".WriteInfo();
                    })
                    .OnComplete(() => {
                        $"   🎯 Animation completed. Total OnChange calls: {changeCount}".WriteInfo();
                        
                        // 🧪 Now test manual dirty flag pattern
                        $"   🔍 Testing MANUAL DIRTY FLAG pattern (alternative)".WriteInfo();
                        
                        // ❌ This would NOT trigger dirty flag (property mutation)
                        // transform.Rotation.Y += 0.1;  
                        
                        // ✅ PATTERN 2: Manual dirty after mutations
                        var currentRot = transform.Rotation;
                        // Simulate direct property access (which doesn't exist, but if it did...)
                        transform.Rotation = new Euler(currentRot.X, currentRot.Y + 0.1, currentRot.Z);
                        // OR: CurrentBox.Shape.SetDirty(true);  // Manual trigger
                        
                        transform.OnChange = originalOnChange; // Restore original handler
                        StatusMessage = $"🎬 Tweener dirty flag test completed - {changeCount} change events fired";
                        StateHasChanged();
                    });
                
                StatusMessage = $"🎬 Testing tweener dirty flag triggering (object replacement pattern)...";
                StateHasChanged();
            }
        }

        /// <summary>
        /// 🧪 Test initialization dirty flag behavior
        /// </summary>
        protected void TestInitializationDirtyFlags()
        {
            if (CurrentBox?.Shape?.Transform != null)
            {
                var transform = CurrentBox.Shape.Transform;
                
                $"🔬 TESTING INITIALIZATION DIRTY FLAG BEHAVIOR:".WriteInfo();
                $"   Current - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                // Test if the transform was properly initialized as dirty
                var hasMatrix = transform.ToMatrix3() != null;
                $"   Matrix exists: {hasMatrix}".WriteInfo();
                
                // Check if forcing a property re-assignment triggers proper updates
                var currentPivot = transform.Pivot;
                transform.Pivot = new Vector3(currentPivot.X, currentPivot.Y, currentPivot.Z);  // Same values, new object
                $"   After Pivot Reassignment - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                StatusMessage = "🔬 Initialization dirty flag test completed - check console";
                StateHasChanged();
            }
        }

        /// <summary>
        /// 🧪 Test the parent-child relationship timing during initialization
        /// </summary>
        protected void TestInitializationTiming()
        {
            $"🔬 TESTING INITIALIZATION TIMING ISSUE:".WriteInfo();
            
            // 🚨 PROBLEMATIC PATTERN: Create transform, set properties, THEN assign to shape
            $"--- Testing PROBLEMATIC pattern (properties before assignment) ---".WriteInfo();
            var transform1 = new Transform3("TestTransform1");
            transform1.Position = new Vector3(1, 2, 3);  // No parent to notify yet!
            transform1.Pivot = new Vector3(0, -0.5f, 0);  // Dirty flag set but no propagation
            $"   Transform1 after properties set - IsDirty: {transform1.IsDirty}".WriteInfo();
            
            var testShape1 = new FoShape3D("test1");
            testShape1.Transform = transform1;  // NOW the parent-child relationship is established
            $"   TestShape1 after transform assignment - Shape.IsDirty: {testShape1.IsDirty}".WriteInfo();
            
            // 🏗️ CORRECT PATTERN: Assign transform to shape FIRST, then set properties
            $"--- Testing CORRECT pattern (assignment before properties) ---".WriteInfo();
            var transform2 = new Transform3("TestTransform2");
            var testShape2 = new FoShape3D("test2");
            testShape2.Transform = transform2;  // Establish parent-child relationship FIRST
            $"   TestShape2 after transform assignment - Shape.IsDirty: {testShape2.IsDirty}".WriteInfo();
            
            testShape2.Transform.Position = new Vector3(1, 2, 3);  // NOW parent can be notified
            testShape2.Transform.Pivot = new Vector3(0, -0.5f, 0);  // Dirty flag propagates to shape
            $"   TestShape2 after properties set - Shape.IsDirty: {testShape2.IsDirty}".WriteInfo();
            
            StatusMessage = "🔬 Initialization timing test completed - check console for results";
            StateHasChanged();
        }
        protected void TestObjectReplacementPattern()
        {
            if (CurrentBox?.Shape?.Transform != null)
            {
                var transform = CurrentBox.Shape.Transform;
                
                $"🧪 DEMONSTRATING OBJECT REPLACEMENT vs PROPERTY MUTATION:".WriteInfo();
                $"   Initial - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                // ✅ PATTERN 1: Object Replacement (WORKS)
                var currentRot = transform.Rotation;
                transform.Rotation = new Euler(currentRot.X, currentRot.Y + 0.1, currentRot.Z);
                $"   After Object Replacement - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                // Reset dirty flags by getting matrix
                var matrix = transform.ToMatrix3();
                $"   After Matrix Calculation - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                // ❌ PATTERN 2: Property Mutation (Would NOT work if possible)
                // transform.Rotation.Y += 0.1;  // This would NOT trigger dirty flag
                // Instead, show manual dirty flag pattern:
                $"   Simulating property mutation + manual dirty flag...".WriteInfo();
                CurrentBox.Shape.SetDirty(true);  // Manual dirty flag
                $"   After Manual SetDirty - Shape.IsDirty: {CurrentBox.Shape.IsDirty}, Transform.IsDirty: {transform.IsDirty}".WriteInfo();
                
                StatusMessage = "🧪 Object replacement pattern demonstrated - check console";
                StateHasChanged();
            }
        }

    }