
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

    private string _mainBoxGuid = Guid.NewGuid().ToString();
    private Scene3D _scene;
    private IArena Arena => FoundryService?.Arena();

    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.5;
    protected double BoxDepth { get; set; } = 1.0;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = string.Empty;

    private bool IsReady => Arena != null && _scene != null;



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
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
            if (!found || scene == null) return base.OnAfterRenderAsync(firstRender);

            _scene = scene;
            _scene.SetAfterUpdateAction((s, j) => 
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree")));

            Arena.SetScene(_scene);
            
            // 🔥 CRITICAL: Clear arena to remove artifacts from previous tests
            // Scene is global singleton - must explicitly clean on page load
            Arena.ClearArena();
            
            AddAxisToScene();
            CreateSpacialBox();
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    private void AddAxisToScene()
    {
        var axis = new Model3D
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf
        };
        _scene.AddChild(axis);
    }

    public void CreateSpacialBox()
    {
        if (!EnsureReady()) return;

        try
        {
            if (CurrentBox?.Shape != null)
            {
                UpdateExistingBox();
            }
            else
            {
                CreateNewBox();
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}");
        }
    }

    private void UpdateExistingBox()
    {
        CurrentBox.Shape.Width = BoxWidth;
        CurrentBox.Shape.Height = BoxHeight;
        CurrentBox.Shape.Depth = BoxDepth;
        CurrentBox.Shape.Transform.Pivot = new Vector3(0, -BoxHeight / 2, 0);
        
        CurrentBox = new SpacialBox3D(CurrentBox.Shape, "m");
        SetStatus($"Updated: {BoxWidth}×{BoxHeight}×{BoxDepth}m");
    }

    private void CreateNewBox()
    {
        var boxShape = new FoShape3D
        {
            Name = "SpacialBoxMain",
            GlyphId = _mainBoxGuid,
            Color = "#4CAF50",
            Opacity = 0.8,
            Transform = new Transform3("BoxTransform")
        }.CreateBox("SpacialBoxMain", BoxWidth, BoxHeight, BoxDepth);

        boxShape.Transform.Position = new Vector3(0, 0, 0);
        boxShape.Transform.Rotation = Euler.FromDegrees(0, 0, 0);
        boxShape.Transform.Scale = new Vector3(1, 1, 1);
        boxShape.Transform.Pivot = new Vector3(0, -BoxHeight / 2, 0);

        Arena.AddShapeToStage<FoShape3D>(boxShape);
        CurrentBox = new SpacialBox3D(boxShape, "m");
        
        SetStatus($"Created: {BoxWidth}×{BoxHeight}×{BoxDepth}m");
    }


    public void ClearAll()
    {
        if (!EnsureReady()) return;
        Arena.ClearArena();
        StateHasChanged();
    }

    public void Dispose()
    {
        // Component cleanup - scene lifecycle managed by Canvas3DComponent
        CurrentBox = null;
        _scene = null;
    }

    // === PRESET SHAPES ===
    protected void CreateCube() => SetDimensionsAndCreate(2.0, 2.0, 2.0);
    protected void CreateLongBox() => SetDimensionsAndCreate(4.0, 1.0, 1.0);
    protected void CreateTallBox() => SetDimensionsAndCreate(1.0, 4.0, 1.0);
    protected void CreateWideBox() => SetDimensionsAndCreate(1.0, 1.0, 4.0);
    protected void CreateTinyBox() => SetDimensionsAndCreate(0.5, 0.5, 0.5);

    private void SetDimensionsAndCreate(double width, double height, double depth)
    {
        BoxWidth = width;
        BoxHeight = height;
        BoxDepth = depth;
        CreateSpacialBox();
    }



    public void ShowQuadrants()
    {
        if (!EnsureBoxCreated()) return;

        var center = CurrentBox.Center;
        var offset = 0.2;
        var colors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FFA500", "#800080" };
        
        var quadrants = new[]
        {
            new Point3D(center.X + offset, center.Y + offset, center.Z + offset),
            new Point3D(center.X - offset, center.Y + offset, center.Z + offset),
            new Point3D(center.X + offset, center.Y - offset, center.Z + offset),
            new Point3D(center.X - offset, center.Y - offset, center.Z + offset),
            new Point3D(center.X + offset, center.Y + offset, center.Z - offset),
            new Point3D(center.X - offset, center.Y + offset, center.Z - offset),
            new Point3D(center.X + offset, center.Y - offset, center.Z - offset),
            new Point3D(center.X - offset, center.Y - offset, center.Z - offset)
        };

        for (int i = 0; i < quadrants.Length; i++)
            VisualizationService.CreateMarkerSphere(Arena, $"Quadrant{i}", quadrants[i], colors[i], 0.04);

        SetStatus("Showing 8 quadrants around center");
    }

    public void ShowSubModel()
    {
        if (!EnsureReady()) return;

        var url = GetReferenceTo(@"storage/StaticFiles/sub.glb");
        var model = new FoModel3D { Name = "Submarine", Url = url }
            .CreateModel("sub", url, 12.0, 4.5, 4.5);

        Arena.AddShapeToStage<FoModel3D>(model);

        var frame = new SpacialFrame3D(model, "m");
        VisualizationService.ShowLabeledVertices(Arena, frame.GetVertices());
        VisualizationService.ShowLabeledEdges(Arena, frame.GetEdges());
        VisualizationService.ShowLabeledFaces(Arena, frame.GetFaces());
        VisualizationService.ShowLabeledNormals(Arena, frame.GetFaces());

        SetStatus("Submarine model added");
    }
 




    // === VISUALIZATION METHODS ===
    public void ShowVertices()
    {
        if (!EnsureBoxCreated()) return;
        var vertices = CurrentBox.GetLocalVertices();
        VisualizationService.ShowLabeledVertices(Arena, vertices);
        SetStatus($"Showing {vertices.Count} vertices");
    }

    public void ShowEdges()
    {
        if (!EnsureBoxCreated()) return;
        var edges = CurrentBox.GetLocalEdges();
        VisualizationService.ShowLabeledEdges(Arena, edges);
        SetStatus($"Showing {edges.Count} edges");
    }

    public void ShowFaces()
    {
        if (!EnsureBoxCreated()) return;
        var faces = CurrentBox.GetLocalFaces();
        VisualizationService.ShowLabeledFaces(Arena, faces);
        SetStatus($"Showing {faces.Count} faces");
    }

    public void ShowNormals()
    {
        if (!EnsureBoxCreated()) return;
        var faces = CurrentBox.GetLocalFaces();
        VisualizationService.ShowLabeledNormals(Arena, faces);
        SetStatus($"Showing {faces.Count} normals");
    }

    // === ANIMATION METHODS ===
    protected void AnimateDoorHinge()
    {
        if (!EnsureBoxCreated()) return;
        
        SetPivotAndReset(new Vector3(-BoxWidth/2, -BoxHeight/2, 0), "Door Hinge");
        
        var tweener = new Tweener();
        tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { Y = Math.PI/2 }, 2.0f)
            .Ease(Ease.BackOut)
            .OnComplete(() => SetStatus("Door swung open"));
        
        SetStatus("Door hinge animation started");
    }

    protected void AnimateCornerBalance()
    {
        if (!EnsureBoxCreated()) return;
        
        SetPivotAndReset(new Vector3(-BoxWidth/2, -BoxHeight/2, -BoxDepth/2), "Corner Balance");
        
        var tweener = new Tweener();
        tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { X = Math.PI/8 }, 1.0f)
            .Ease(Ease.SineInOut).Repeat().Reflect();
        tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { Z = Math.PI/12 }, 1.3f, 0.2f)
            .Ease(Ease.SineInOut).Repeat().Reflect();

        SetStatus("Box balancing on corner");
    }

    protected void AnimateCenterSpin()
    {
        if (!EnsureBoxCreated()) return;
        
        SetPivotAndReset(new Vector3(0, 0, 0), "Center Spin");
        
        var tweener = new Tweener();
        tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { Y = Math.PI * 4 }, 4.0f)
            .Rotation().Repeat();
        tweener.Tween(CurrentBox.Shape.Transform.Rotation, new { X = Math.PI/4 }, 3.0f, 0.5f)
            .Ease(Ease.SineInOut).Repeat().Reflect();

        SetStatus("Box spinning around center");
    }

    protected void AnimateResetToFloor()
    {
        if (!EnsureBoxCreated()) return;

        var transform = CurrentBox.Shape.Transform;
        var tweener = new Tweener();
        
        var floorPivot = new Vector3(0, -BoxHeight/2, 0);
        tweener.Tween(transform.Pivot, new { X = floorPivot.X, Y = floorPivot.Y, Z = floorPivot.Z }, 1.5f)
            .Ease(Ease.BackOut);
        tweener.Tween(transform.Rotation, new { X = 0.0, Y = 0.0, Z = 0.0 }, 1.5f)
            .Ease(Ease.BackOut);
        tweener.Tween(transform.Position, new { X = 0.0, Y = 0.0, Z = 0.0 }, 1.5f)
            .Ease(Ease.BackOut)
            .OnComplete(() => SetStatus("Reset to floor"));
    }

    private void SetPivotAndReset(Vector3 newPivot, string testName)
    {
        if (!EnsureBoxCreated()) return;

        var transform = CurrentBox.Shape.Transform;
        transform.Pivot = newPivot;
        transform.Position = new Vector3(0, 0, 0);
        transform.Rotation = Euler.FromDegrees(0, 0, 0);

        SetStatus($"{testName} pivot set to ({newPivot.X:F2}, {newPivot.Y:F2}, {newPivot.Z:F2})");
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

    // === HELPER METHODS ===
    private bool EnsureReady()
    {
        if (IsReady) return true;
        SetStatus("Scene not ready yet");
        return false;
    }

    private bool EnsureBoxCreated()
    {
        if (CurrentBox != null) return true;
        SetStatus("No box created yet. Please create a box first");
        return false;
    }

    private void SetStatus(string message)
    {
        StatusMessage = message;
        StateHasChanged();
    }

    }