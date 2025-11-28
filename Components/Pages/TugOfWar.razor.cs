using Microsoft.AspNetCore.Components;
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
    private const double ANIMATION_DURATION = 5.0; // seconds
    private const double START_HEIGHT = 1.0;
    private const double TARGET_HEIGHT = 5.0;
    private const double BOX_MOVE_DISTANCE = 3.0;

    // FPS and tick tracking
    protected double _currentFps = 0;
    protected int _currentTick = 0;
    private int _frameCount = 0;
    private const int FPS_UPDATE_INTERVAL = 15; // Update display every 15 frames
    
    // ✅ Phase 0.5: Per-page stage (matches 2D's ManagedPage pattern)
    private FoStage3D _tugOfWarStage;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
        $"TugOfWar Page OnInitialized".WriteInfo();
    }

    private void OnAnimationFrame(AnimationEvent animEvent)
    {
        _frameCount++;
        if (_frameCount >= FPS_UPDATE_INTERVAL)
        {
            _currentFps = animEvent.fps;
            _currentTick = animEvent.tick;
            _frameCount = 0;
            InvokeAsync(StateHasChanged);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            $"TugOfWar Page OnAfterRenderAsync".WriteInfo();

            // Wait a moment for Canvas3DComponent to finish its OnAfterRenderAsync
            await Task.Delay(100);

            // Setup 3D Arena-Scene bridge
            var (found3D, scene3D) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
            if (found3D)
            {
                // ✅ Phase 0.5: Get stage created by Canvas (matches 2D pattern)
                var arena = Workspace.GetArena();
                _tugOfWarStage = arena.EstablishStage<FoStage3D>(Canvas3DReference.SceneName);
                
                // Canvas already linked stage ↔ scene - just verify
                var linkedScene = _tugOfWarStage.GetAssociatedScene();
                $"TugOfWar: Retrieved TugOfWarStage '{_tugOfWarStage.Key}' linked to scene '{linkedScene?.Title ?? "null"}'".WriteSuccess();
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
        
        // Disable line router and hit test display for clean display (keep grid visible)
        var page = drawing.CurrentPage();
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
        
        // Add shapes to drawing (without selection to avoid debug lines)
        drawing.AddShape(s1);
        drawing.AddShape(s2);


        // Create connecting arrow
        var wire = new FoShape1D("Arrow", "Cyan")
        {
            Height = 50,
            ShapeDraw = async (ctx, obj) => await DrawArrowAsync(ctx, obj.Width, obj.Height, obj.Color)
        };
        wire.GlueStartTo(s1, "RIGHT");
        wire.GlueFinishTo(s2, "LEFT");
        drawing.AddShape(wire);

        var text = new FoText2D(100, 50, "Green")
        {
            Text = "Tug of War!",
        };
        text.MoveTo(400, 400);
        drawing.AddShape(text);
        
        
        // Animate both shapes
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



    public async void StartTugOfWar3D()
    {
        $"SIMPLE TEST: Adding 3 static boxes".WriteInfo();
        
        if (_tugOfWarStage == null)
        {
            $"ERROR: Stage is null!".WriteError();
            return;
        }

        _tugOfWarStage.ClearStage();

        // Create 3 boxes - NO animation, just static shapes
        var box1 = new FoShape3D("Box1", "blue");
        box1.Transform.Position = new Vector3(-3, 0.5, 0);
        box1.CreateBox("Box1", 1.0, 1.0, 1.0);

        var box2 = new FoShape3D("Box2", "red");
        box2.Transform.Position = new Vector3(0, 0.5, 0);
        box2.CreateBox("Box2", 1.0, 1.0, 1.0);

        var box3 = new FoShape3D("Box3", "green");
        box3.Transform.Position = new Vector3(3, 0.5, 0);
        box3.CreateBox("Box3", 1.0, 1.0, 1.0);

        _tugOfWarStage.AddShape(box1);
        _tugOfWarStage.AddShape(box2);
        _tugOfWarStage.AddShape(box3);

        var linkedScene = _tugOfWarStage.GetAssociatedScene();
        $"✅ Added 3 boxes. Stage has scene: {linkedScene != null}, Scene name: {linkedScene?.Title ?? "NULL"}".WriteSuccess();
        
        // CRITICAL: Trigger immediate render - don't wait for animation loop
        var arena = Workspace.GetArena();
        await arena.RenderArena(0, 0);

    }

    public async void StartTugOfWar3D_OLD()
    {
        $"Starting 3D Tug of War".WriteInfo();
        
        var arena = Workspace.GetArena();
        if (arena == null)
        {
            $"No arena available".WriteError();
            return;
        }

        // ✅ Phase 0.5: Clear only this page's stage
        _tugOfWarStage?.ClearStage();
        $"3D scene cleared - ready for new objects".WriteSuccess();

        // Complex tug-of-war test with two boxes and connecting pipe
        _box1_3D = new FoShape3D($"Box1-{Guid.NewGuid().ToString().Substring(0, 8)}", "blue")
        {
            Transform = new Transform3("Box1Transform")
            {
                Position = new Vector3(-2, 0.5, 0),
            },
        };

        _box1_3D.CreateBox("Box1", 1.0, 1.0, 1.0)
                .BeforeAnimationRefreshNOOP((shape, tick, fps) =>
                {
                    _animationTime += 1.0 / fps;
                    var progress = Math.Min(_animationTime / ANIMATION_DURATION, 1.0);
                    
                    if (progress < 1.0)
                    {
                        var x = -2 - (progress * BOX_MOVE_DISTANCE);
                        shape.Transform.Position = new Vector3(x, 0.5, x);
                        _tube_3D.SetGeometryStale();

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
                .BeforeAnimationRefreshNOOP((shape, tick, fps) =>
                {
                    _animationTime += 1.0 / fps;
                    var progress = Math.Min(_animationTime / ANIMATION_DURATION, 1.0);
                    
                    if (progress < 1.0)
                    {
                        var x = 2 + (progress * BOX_MOVE_DISTANCE);
                        shape.Transform.Position = new Vector3(x, x, 2 * x);
                        _tube_3D.SetGeometryStale();
                    }
                });

        _tube_3D = new FoPipe3D($"Tube-{Guid.NewGuid().ToString().Substring(0, 8)}", "cyan")
        {
            FromShape3D = _box1_3D,
            ToShape3D = _box2_3D
        }.CreatePipe("ConnectingTube", 0.1);

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
        _distanceText.BeforeAnimationRefreshNOOP((text, tick, fps) =>
        {
            // Update distance text EVERY frame
            var distance = _box1_3D.DistanceBetween(_box2_3D);
            
            // Only log occasionally to avoid spam
            if (tick % 30 == 0)
            {
                $"Distance updated at tick {tick}: {distance:F2}".WriteInfo(1);
            }
            
            _distanceText.Text = $"length: {distance:F2}";
            
            // DON'T clear animation - we want this to run every frame!
            // _distanceText.ClearAnimationRefresh(); // ❌ REMOVED - was stopping updates after first frame
        });

        // ✅ Phase 0.5: Add all shapes to this page's stage
        _tugOfWarStage.AddShape(_box1_3D);
        $"Added Box1 to TugOfWarStage at {_box1_3D.Transform.Position}".WriteSuccess();
        
        _tugOfWarStage.AddShape(_box2_3D);
        $"Added Box2 to TugOfWarStage at {_box2_3D.Transform.Position}".WriteSuccess();
        
        _tugOfWarStage.AddShape(_tube_3D);
        $"Added connecting tube to TugOfWarStage".WriteSuccess();
        
        _tugOfWarStage.AddShape(_distanceText);
        $"Added distance text to TugOfWarStage at {_distanceText.Transform.Position}".WriteSuccess();

        // ULTRA SIMPLIFIED TEST: Just a pipe with animated path
        $"Creating pipe with animated path".WriteInfo();
        
        _growingPipe = new FoPipe3D("GrowingPipe", "red");
        
        var initialPath = new List<Vector3>
        {
            new Vector3(10, 0, 10),
            new Vector3(10, START_HEIGHT, 10)
        };
        
        _growingPipe.CreateTube("GrowingPipe", 0.25, initialPath)
                    .BeforeAnimationRefreshNOOP((self, tick, fps) =>
                    {
                        _animationTime += 1.0 / fps;
                        var progress = Math.Min(_animationTime / ANIMATION_DURATION, 1.0);
                        
                        if (progress < 1.0)
                        {
                            var newHeight = START_HEIGHT + (progress * (TARGET_HEIGHT - START_HEIGHT));
                            _growingPipe.Path3D = new List<Vector3>
                            {
                                new Vector3(10, 0, 10),
                                new Vector3(10, newHeight, 10)
                            };
                        }
                    });

        // ✅ Phase 0.5: Add growing pipe to this page's stage
        _tugOfWarStage.AddShape(_growingPipe);
        $"Added growing pipe to TugOfWarStage".WriteSuccess();
        

        // Reset animation state and START animation immediately
        _animationTime = 0;

        $"3D Tug of War started - all objects animating".WriteSuccess();
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
        
        // ✅ Phase 0.5: Clear only this page's stage
        _tugOfWarStage?.ClearStage();
        $"3D scene reset complete".WriteSuccess();
        
        // Clear local references
        _growingPipe = null;
        _box1_3D = null;
        _box2_3D = null;
        _tube_3D = null;
        _distanceText = null;
        
        // Force UI update to reflect cleared state
        StateHasChanged();
        
        $"3D scene reset complete".WriteSuccess();
    }

    public async Task ClearScene3D()
    {
        $"Clearing 3D scene...".WriteInfo();
        
        // ✅ Phase 0.5: Clear only this page's stage
        _tugOfWarStage?.ClearStage();
        $"3D scene cleared".WriteSuccess();
        StateHasChanged();
    }

    public void Dispose()
    {
        _growingPipe?.ClearAnimationRefresh();
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
        $"TugOfWar Page Disposed".WriteInfo();
    }
}
