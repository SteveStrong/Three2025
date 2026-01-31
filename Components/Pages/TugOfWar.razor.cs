using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core.Extensions;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using BlazorComponentBus;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using  FoundryWorldsAndDrawings.ThreeD.Core;

namespace Three2025.Components.Pages;

public partial class TugOfWarBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }

    public Canvas2DComponent Canvas2DReference = null;
    public Canvas3DComponent Canvas3DReference = null;

    [Parameter] public int CanvasWidth { get; set; } = 800;
    [Parameter] public int CanvasHeight { get; set; } = 600;

    protected MockDataGenerator DataGenerator { get; set; } = new();

    // 3D Animation state
    private FoShape3D _box1_3D;
    private FoShape3D _box2_3D;
    private FoPipe3D _tube_3D;  // The connecting tube between boxes
    private FoPipe3D _growingPipe;
    private FoText3D _distanceText;
    private double _animationTime = 0;
    
    // Animation constants - debug mode uses much shorter duration for visible stepping
    private bool _debugMode = false;
    private const double ANIMATION_DURATION = 5.0; // seconds (normal mode)
    private const double DEBUG_ANIMATION_DURATION = 0.5; // seconds (10 frames at 60fps - very visible)
    private const double START_HEIGHT = 1.0;
    private const double TARGET_HEIGHT = 5.0;
    private const double BOX_MOVE_DISTANCE = 3.0;

    // FPS and tick tracking
    protected double _currentFps = 0;
    protected int _currentTick = 0;
    private int _frameCount = 0;
    private const int FPS_UPDATE_INTERVAL = 15; // Update display every 15 frames
    
    // Debug control state
    protected string _animationState = "Running";
    protected int _stepCount = 0;
    
    // ✅ Phase 0.5: Per-page stage (matches 2D's ManagedPage pattern)
    private FoStage3D _tugOfWarStage;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
        $"TugOfWar Page OnInitialized - CIRCUIT ACTIVE".WriteInfo();
    }

    private void OnAnimationFrame(AnimationEvent animEvent)
    {
        _frameCount++;
        
        // 🔍 DIAGNOSTIC: Log first 5 frames to confirm we're receiving events
        if (_frameCount <= 5)
        {
            $"🎬 TugOfWar OnAnimationFrame: tick={animEvent.tick}, fps={animEvent.fps:F1}, domain={animEvent.domain}".WriteInfo();
        }
        
        if (_frameCount >= FPS_UPDATE_INTERVAL)
        {
            _currentFps = animEvent.fps;
            _currentTick = animEvent.tick;
            _animationState = AnimationFrameBus.GetAnimationState();
            _frameCount = 0;
            
            // Update UI silently - no need to spam console
            InvokeAsync(StateHasChanged);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            $"TugOfWar Page OnAfterRenderAsync - firstRender=true - INTERACTIVE MODE ACTIVE".WriteSuccess();

            // Wait a moment for Canvas3DComponent to finish its OnAfterRenderAsync
            await Task.Delay(100);

            // Setup 3D Arena-Scene bridge
            var (found3D, scene3D) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
            $"TugOfWar: GetActiveScene found={found3D}, scene={scene3D?.Title ?? "null"}, Canvas3DReference={Canvas3DReference != null}".WriteInfo();
            
            if (found3D)
            {
                // ✅ Stage-centric pattern: Get stage from Canvas
                _tugOfWarStage = Canvas3DReference.Stage;
                
                // Canvas already linked stage ↔ scene - just verify
                var linkedScene = _tugOfWarStage?.GetAssociatedScene();
                $"TugOfWar: Retrieved TugOfWarStage '{_tugOfWarStage?.Name}' linked to scene '{linkedScene?.Title ?? "null"}'".WriteSuccess();
            }
            else
            {
                $"TugOfWar: WARNING - No active scene found! Canvas3D may not be initialized".WriteWarning();
            }

            
            // DON'T AUTO-START - let user click button to start
            // StartTugOfWar3D();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public string GetReferenceTo(string filename)
    {
        return $"{Navigation.BaseUri}{filename}";
    }

    // ==================== 2D Tug of War ====================
    public void StartTugOfWar2D()
    {
        $"Starting 2D Tug of War".WriteInfo();
        
        var drawing = Workspace!.GetDrawing();
        if (drawing == null) 
        {
            $"No drawing available".WriteError();
            return;
        }

        // Clear existing shapes
        drawing.ClearAll();
        
        // Get page using stage-centric pattern (FirstPage or Canvas2DReference.Page)
        var page = Canvas2DReference?.Page ?? drawing.FirstPage();
        if (page != null)
        {
            page.ShowGrid = true;  // Keep grid visible
            page.ShowLineRouter = false;
        }
        drawing.ToggleHitTestRender(); // Turn off if currently on
        if (drawing.ToggleHitTestRender()) // Check state
        {
            drawing.ToggleHitTestRender(); // Turn it off
        }

        // Create two shapes
        var s1 = new FoShape2D(50, 50, "Blue");
        s1.MoveTo(300, 300);
        var s2 = new FoShape2D(50, 50, "Orange");
        s2.MoveTo(500, 300);
        
        // Add shapes to page directly (stage-centric pattern)
        page?.AddShape(s1);
        page?.AddShape(s2);


        // Create connecting arrow
        var wire = new FoShape1D("Arrow", "Cyan")
        {
            Height = 50,
            ShapeDraw = async (ctx, obj) => await DrawArrowAsync(ctx, obj.Width, obj.Height, obj.Color)
        };
        wire.GlueStartTo(s1, "RIGHT");
        wire.GlueFinishTo(s2, "LEFT");
        page?.AddShape(wire);

        var text = new FoText2D(100, 50, "Green")
        {
            Text = "Tug of War!",
        };
        text.MoveTo(400, 400);
        page?.AddShape(text);
        
        
        // Animate both shapes
        $"🎭 2D Animation: Tweening s1 from PinX={s1.PinX} to {s1.PinX - 150}".WriteInfo();
        $"🎭 2D Animation: Tweening s2 from PinX={s2.PinX} to {s2.PinX + 150}, PinY={s2.PinY} to {s2.PinY + 50}".WriteInfo();
        
        FoGlyph2D.Animations.Tween<FoShape2D>(s1, new { PinX = s1.PinX - 150, }, 2, 0);
        FoGlyph2D.Animations.Tween<FoShape2D>(s2, new { PinX = s2.PinX + 150, PinY = s2.PinY + 50, }, 2, 0).OnComplete(() =>
        {
            $"2D Tug of War animation completed".WriteSuccess();
            text.Text = $"dist: {s1.DistanceBetween(s2):F2}";
        });
        
        // Trigger initial render to show shapes
        StateHasChanged();
        
        $"2D Tug of War started".WriteSuccess();
    }

    public void Reset2D()
    {
        var drawing = Workspace?.GetDrawing();
        drawing?.ClearAll();
        $"2D scene reset".WriteInfo();
    }

    private static async Task DrawArrowAsync(Blazor.Extensions.Canvas.Canvas2D.Canvas2DContext ctx, int width, int height, string color)
    {
        var headWidth = 40;
        var bodyHeight = height / 4;
        var bodyWidth = width - headWidth;
        await ctx.SetFillStyleAsync(color);
        var y = (height - bodyHeight) / 2.0;
        await ctx.FillRectAsync(0, y, bodyWidth, bodyHeight);
        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(bodyWidth, 0);
        await ctx.LineToAsync(width, height / 2);
        await ctx.LineToAsync(bodyWidth, height);
        await ctx.LineToAsync(bodyWidth, 0);
        await ctx.ClosePathAsync();
        await ctx.FillAsync();
        await ctx.SetFillStyleAsync("#fff");
        await ctx.FillTextAsync("→", width / 2, height / 2, 20);
    }



    public async void Add3Boxes()
    {
        $"SIMPLE TEST: Adding 3 static boxes".WriteInfo();
        
        if (_tugOfWarStage == null)
        {
            $"ERROR: Stage is null! Canvas3DReference={Canvas3DReference != null}, Canvas3DReference.Stage={Canvas3DReference?.Stage != null}".WriteError();
            // Try to get it again
            _tugOfWarStage = Canvas3DReference?.Stage;
            if (_tugOfWarStage == null)
            {
                $"ERROR: Still null after retry!".WriteError();
                return;
            }
        }

        // Check if stage is linked to scene
        var linkedScene = _tugOfWarStage.GetAssociatedScene();
        var isSceneActive = linkedScene?.IsActive ?? false;
        $"Stage '{_tugOfWarStage.Name}' linked to scene: {linkedScene?.Title ?? "NULL"}, Scene.IsActive={isSceneActive}".WriteInfo();

        // DON'T clear - let boxes accumulate to test coexistence
        var existingCount = _tugOfWarStage.AllBodies().Count + _tugOfWarStage.AllLinks().Count;
        $"Before adding: Stage has {existingCount} shapes ({_tugOfWarStage.AllBodies().Count} bodies, {_tugOfWarStage.AllLinks().Count} links)".WriteInfo();

        // Create 3 boxes - NO animation, just static shapes
        var box1 = new FoShape3D("Box1", "blue");
        box1.Transform.Position = new Vector3(-3, 0.5, 3);
        box1.CreateBox("Box1", 1.0, 1.0, 1.0);

        $"Adding Box1...".WriteInfo();
        _tugOfWarStage.AddShape(box1);
 
        var box2 = new FoShape3D("Box2", "red");
        box2.Transform.Position = new Vector3(0, 0.5, 3);
        box2.CreateBox("Box2", 1.0, 1.0, 1.0);

        _tugOfWarStage.AddShape(box2);
 
        var box3 = new FoShape3D("Box3", "green");
        box3.Transform.Position = new Vector3(3, 0.5, 3);
        box3.CreateBox("Box3", 1.0, 1.0, 1.0);

        _tugOfWarStage.AddShape(box3);
        
        // DIAGNOSTIC: Verify shapes are in the stage collections (not slots!)
        var bodies = _tugOfWarStage.AllBodies();
        var links = _tugOfWarStage.AllLinks();
        $"After adding: Stage has {bodies.Count} bodies, {links.Count} links".WriteInfo();
        
        foreach (var body in bodies)
        {
            $"  Body: {body.Name}, Stale={body.IsStale()}, Type={body.GetType().Name}".WriteInfo();
        }
        foreach (var link in links)
        {
            $"  Link: {link.Name}, Stale={link.IsStale()}, Type={link.GetType().Name}".WriteInfo();
        }
         
        // CRITICAL: Trigger immediate render - don't wait for animation loop
        var arena = Workspace.GetArena();
        await arena.RenderArena(0, 0);

    }

    private int _boxCounter = 0;
    
    public async void AddOneBox()
    {
        if (_tugOfWarStage == null)
        {
            $"ERROR: Stage is null!".WriteError();
            return;
        }

        _boxCounter++;
        
        // Just add a box - don't worry about total count or what's already there
        var box = new FoShape3D($"Box{_boxCounter}", "cyan");
        box.Transform.Position = new Vector3((_boxCounter - 1) * 1.5, 0.5, 5);
        box.CreateBox($"Box{_boxCounter}", 1.0, 1.0, 1.0);
        
        _tugOfWarStage.AddShape(box);
        
        // Push it out to JavaScript immediately
        var arena = Workspace.GetArena();
        await arena.RenderArena(0, 0);
    }


    public async void StartTugOfWar3D(bool startPaused = false)
    {
        $"🚀 StartTugOfWar3D called - startPaused={startPaused}".WriteSuccess();
        
        if (startPaused)
        {
            AnimationFrameBus.PauseAllAnimations();
            _animationState = AnimationFrameBus.GetAnimationState();
            _stepCount = 0;
            _debugMode = true;  // Use fast animation for visible stepping
            $"Starting 3D Tug of War Animation in DEBUG/PAUSED mode (0.5s duration)".WriteInfo();
        }
        else
        {
            _stepCount = 0;
            _debugMode = false;  // Use normal 5s animation
            $"🎬 Starting 3D Tug of War Animation (NORMAL mode, 5s duration)".WriteSuccess();
        }
        
        var arena = Workspace.GetArena();
        if (arena == null)
        {
            $"No arena available".WriteError();
            return;
        }

        // DON'T clear - let objects accumulate to test multiple animations
        var currentBodies = _tugOfWarStage?.AllBodies().Count ?? 0;
        var currentLinks = _tugOfWarStage?.AllLinks().Count ?? 0;
        $"Before animation: Stage has {currentBodies} bodies, {currentLinks} links".WriteInfo();

        // Complex tug-of-war test with two boxes and connecting pipe
        _box1_3D = new FoShape3D($"Box1-{Guid.NewGuid().ToString().Substring(0, 8)}", "blue")
        {
            Transform = new Transform3("Box1Transform")
            {
                Position = new Vector3(-2, 0.5, 0),
            },
        };

        _box1_3D.CreateBox("Box1", 1.0, 1.0, 1.0)
                .SetRecomputeBoundary()  // ← Opt-in IMMEDIATELY, not during animation
                .BeforeAnimationRefresh((shape, tick, fps) =>
                {
                    if (fps <= 0 || tick == 0) return;  // Skip manual renders with invalid FPS
                    
                    // 🔍 DIAGNOSTIC: Log first 10 callbacks
                    if (tick <= 10)
                    {
                        $"📦 Box1 BeforeAnimationRefresh: tick={tick}, fps={fps:F1}, animTime={_animationTime:F2}".WriteSuccess();
                    }
                    
                    _animationTime += 1.0 / fps;
                    var duration = _debugMode ? DEBUG_ANIMATION_DURATION : ANIMATION_DURATION;
                    var progress = Math.Min(_animationTime / duration, 1.0);
                    
                    if (tick <= 10)
                    {
                        $"   Progress: {progress:F3}, Duration: {duration:F1}s".WriteInfo();
                    }
                    
                    if (progress < 1.0)
                    {
                        var x = -2 - (progress * BOX_MOVE_DISTANCE);
                        shape.Transform.Position = new Vector3(x, 0.5, x);
                        shape.SetTransformStale();  // ← CRITICAL: Must mark stale after changing position!
                        _tube_3D?.SetGeometryStale();
                        _distanceText?.SetGeometryStale();
                        
                        if (tick <= 10)
                        {
                            $"   New position: ({x:F2}, 0.5, {x:F2})".WriteInfo();
                        }
                        $"📦 Box1 moved to x={x:F2}, progress={progress:F2}".WriteInfo();
                    }
                    else if (progress >= 1.0)
                    {
                        // Animation complete - stop all animations
                        _box1_3D?.ClearAnimationRefresh();
                        _box2_3D?.ClearAnimationRefresh();
                        _growingPipe?.ClearAnimationRefresh();
                        $"All 3D animations completed".WriteSuccess();
                    }
                });

        _box2_3D = new FoShape3D($"Box2-{Guid.NewGuid().ToString().Substring(0, 8)}", "orange")
        {
            Transform = new Transform3("Box2Transform")
            {
                Position = new Vector3(2, 0.5, 0),
            },
        };

        _box2_3D.CreateBox("Box2", 1.0, 1.0, 1.0)
                .SetRecomputeBoundary()  // ← Opt-in IMMEDIATELY, not during animation
                .BeforeAnimationRefresh((shape, tick, fps) =>
                {
                    if (fps <= 0 || tick == 0) return;  // Skip manual renders with invalid FPS
                    
                    // 🔍 DIAGNOSTIC: Log first 10 callbacks
                    if (tick <= 10)
                    {
                        $"📦 Box2 BeforeAnimationRefresh: tick={tick}, fps={fps:F1}".WriteSuccess();
                    }
                    
                    _animationTime += 1.0 / fps;
                    var duration = _debugMode ? DEBUG_ANIMATION_DURATION : ANIMATION_DURATION;
                    var progress = Math.Min(_animationTime / duration, 1.0);
                    
                    if (progress < 1.0)
                    {
                        var x = 2 + (progress * BOX_MOVE_DISTANCE);
                        shape.Transform.Position = new Vector3(x, x, 2 * x);
                        _tube_3D?.SetGeometryStale();
                        _distanceText?.SetGeometryStale();
                    }
                });

        _tube_3D = new FoPipe3D($"Tube-{Guid.NewGuid().ToString().Substring(0, 8)}", "cyan")
        {
            FromShape3D = _box1_3D,
            ToShape3D = _box2_3D
        }.CreatePipe("ConnectingTube", 0.1);

        // Mark pipe as needing boundary computation AND initial geometry creation
        _tube_3D.SetRecomputeBoundary();
        _tube_3D.SetGeometryStale();  // Force initial geometry computation

        // ADD BOXES AND TUBE TO ARENA
        // Create distance text
        _distanceText = new FoText3D("DistanceText", "white")
        {
            Text = "dist: 0.00",
            FontSize = 0.8,
            Transform = new Transform3("DistanceTextTransform")
            {
                Position = new Vector3(0, 2, 0)
            }
        };
        _distanceText.PreComputeMesh = (shape) =>
        {
            // Update distance text EVERY frame
            var (success1, pos1) = _box1_3D.GetWorldPosition();
            var (success2, pos2) = _box2_3D.GetWorldPosition();
            var (success, distance) = _box1_3D.DistanceBetween(_box2_3D);
            
            $"Box1: success={success1}, pos={pos1.X:F2},{pos1.Y:F2},{pos1.Z:F2}".WriteInfo();
            $"Box2: success={success2}, pos={pos2.X:F2},{pos2.Y:F2},{pos2.Z:F2}".WriteInfo();
            $"Distance: success={success}, dist={distance:F2}".WriteInfo();
            
            shape.Color = success ? "green" : "red";
            _distanceText.Text = $"length: {distance:F2} {success}";
        };

        // ✅ Phase 0.5: Add all shapes to this page's stage
        $"🎯 Adding shapes to TugOfWarStage...".WriteInfo();
        
        _tugOfWarStage.AddShape(_box1_3D);
        $"✅ Added Box1 to TugOfWarStage at {_box1_3D.Transform.Position}".WriteSuccess();
        
        _tugOfWarStage.AddShape(_box2_3D);
        $"✅ Added Box2 to TugOfWarStage at {_box2_3D.Transform.Position}".WriteSuccess();
        
        _tugOfWarStage.AddShape(_tube_3D);
        $"✅ Added connecting tube to TugOfWarStage (Type={_tube_3D.GetType().Name}, IsIBodyLink3D={_tube_3D is IBodyLink3D})".WriteSuccess();
        
        _tugOfWarStage.AddShape(_distanceText);
        $"✅ Added distance text to TugOfWarStage at {_distanceText.Transform.Position}".WriteSuccess();
        
        // DIAGNOSTIC: Verify stage collections after adding
        $"STAGE DIAGNOSTIC: Bodies={_tugOfWarStage.AllBodies().Count()}, Links={_tugOfWarStage.AllLinks().Count()}".WriteInfo();
        foreach (var body in _tugOfWarStage.AllBodies())
        {
            $"  Body: {body.Name} (Type={body.GetType().Name})".WriteInfo();
        }
        foreach (var link in _tugOfWarStage.AllLinks())
        {
            $"  Link: {link.Name} (Type={link.GetType().Name})".WriteInfo();
        }

        // ULTRA SIMPLIFIED TEST: Just a pipe with animated path
        $"Creating pipe with animated path".WriteInfo();
        
        _growingPipe = new FoPipe3D("GrowingPipe", "red");
        
        var initialPath = new List<Vector3>
        {
            new Vector3(10, 0, 10),
            new Vector3(10, START_HEIGHT, 10)
        };
        
        _growingPipe.CreateTube("GrowingPipe", 0.25, initialPath)
                    .BeforeAnimationRefresh((self, tick, fps) =>
                    {
                        if (fps <= 0 || tick == 0) return;  // Skip manual renders with invalid FPS
                        
                        // 🔍 DIAGNOSTIC: Log first 10 callbacks
                        if (tick <= 10)
                        {
                            $"🔴 GrowingPipe BeforeAnimationRefresh: tick={tick}, fps={fps:F1}".WriteSuccess();
                        }
                        
                        _animationTime += 1.0 / fps;
                        var duration = _debugMode ? DEBUG_ANIMATION_DURATION : ANIMATION_DURATION;
                        var progress = Math.Min(_animationTime / duration, 1.0);
                        
                        if (tick <= 10)
                        {
                            $"   Pipe Progress: {progress:F3}".WriteInfo();
                        }
                        
                        if (progress < 1.0)
                        {
                            var newHeight = START_HEIGHT + (progress * (TARGET_HEIGHT - START_HEIGHT));
                            _growingPipe.Path3D = new List<Vector3>
                            {
                                new Vector3(10, 0, 10),
                                new Vector3(10, newHeight, 10)
                            };
                            _growingPipe.SetGeometryStale();  // Mark stale when path changes
                        }
                    });

        // Mark pipe as needing initial geometry creation
        _growingPipe.SetGeometryStale();

        // ✅ Phase 0.5: Add growing pipe to this page's stage
        _tugOfWarStage.AddShape(_growingPipe);
        $"Added growing pipe to TugOfWarStage".WriteSuccess();
        

        // Reset animation state and START animation immediately
        _animationTime = 0;

        // If starting paused, run for 2 frames to create and render geometry, then auto-pause
        if (startPaused)
        {
            $"🔄 Running 2 frames to create and render geometry, then auto-pause".WriteInfo();
            AnimationFrameBus.RunForFrames(2);
            _stepCount = 2; // Will have run 2 frames
            $"✅ Geometry will be created and rendered - will auto-pause after 2 frames".WriteSuccess();
        }
        else
        {
            // CRITICAL: Push all shapes to JavaScript immediately so they appear
            $"📤 Pushing initial geometry to JavaScript...".WriteInfo();
            await arena.RenderArena(0, 0);
            $"🎬 3D Tug of War started - all objects should be animating now!".WriteSuccess();
            $"   Animation state: {AnimationFrameBus.GetAnimationState()}".WriteInfo();
            $"   Is paused? {AnimationFrameBus.IsGloballyPaused()}".WriteInfo();
            $"   Current tick: {AnimationFrameBus.GetCurrentTick()}".WriteInfo();
        }
    }

    public void StartBothAnimations()
    {
        StartTugOfWar2D();
        StartTugOfWar3D();
        $"Both 2D and 3D Tug of War started".WriteSuccess();
    }

    public void ResetBoth()
    {
        Reset2D();
        Reset3D();
        $"Both 2D and 3D reset".WriteSuccess();
    }

    public async void Reset3D()
    {
        // Stop animations first
        if (_growingPipe != null)
        {
            _growingPipe.ClearAnimationRefresh();
        }
        if (_box1_3D != null)
        {
            _box1_3D.ClearAnimationRefresh();
        }
        if (_box2_3D != null)
        {
            _box2_3D.ClearAnimationRefresh();
        }
        
        _animationTime = 0;
        
        // ✅ Stage-centric pattern: Clear only this page's stage
        if (_tugOfWarStage != null)
            await _tugOfWarStage.ClearAll();
        
        // Clear local references
        _growingPipe = null;
        _box1_3D = null;
        _box2_3D = null;
        _tube_3D = null;
        _distanceText = null;
        
        $"3D scene reset - objects deleted".WriteSuccess();
        
        // Force UI update to reflect cleared state
        StateHasChanged();
    }

    public async Task ClearScene3D()
    {
        $"Clearing 3D scene...".WriteInfo();
        
        // ✅ Stage-centric pattern: ClearAll sends deletions immediately
        if (_tugOfWarStage != null)
            await _tugOfWarStage.ClearAll();
        
        var remaining = (_tugOfWarStage?.AllBodies().Count ?? 0) + (_tugOfWarStage?.AllLinks().Count ?? 0);
        $"3D scene cleared - stage now has {remaining} shapes".WriteSuccess();
        StateHasChanged();
    }

    // ========== Debug Control Methods ==========
    
    protected void StartTugOfWarPaused()
    {
        StartTugOfWar3D(startPaused: true);
    }
    
    protected void PauseAnimation()
    {
        AnimationFrameBus.PauseAllAnimations();
        _animationState = AnimationFrameBus.GetAnimationState();
        StateHasChanged();
        "Animation paused by user".WriteInfo();
    }

    protected async Task StepFrame()
    {
        _stepCount++;
        await AnimationFrameBus.TriggerSingleFrame();
        _animationState = AnimationFrameBus.GetAnimationState();
        _currentTick = AnimationFrameBus.IsGloballyPaused() ? _currentTick : _currentTick + 1;
        StateHasChanged();
        $"Step {_stepCount}: Single frame executed (Tick: {_currentTick})".WriteInfo();
    }

    protected void ResumeAnimation()
    {
        AnimationFrameBus.ResumeAllAnimations();
        _animationState = AnimationFrameBus.GetAnimationState();
        _stepCount = 0;
        StateHasChanged();
        "Animation resumed by user".WriteInfo();
    }

    public void Dispose()
    {
        _growingPipe?.ClearAnimationRefresh();
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
        $"TugOfWar Page Disposed".WriteInfo();
    }
}
