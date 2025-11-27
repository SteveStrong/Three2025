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
            var arena = Workspace.GetArena();
            if (found3D) 
            {
                arena.SetScene(scene3D!);
                $"TugOfWar: Arena-Scene bridge established".WriteSuccess();
            }

            // Initialize both scenes but don't start animations yet
            await InitializeScene3D();
            
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

    // ==================== 3D Tug of War ====================
    private async Task InitializeScene3D()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
        if (!found)
        {
            $"3D Scene not found".WriteError();
            return;
        }

        $"Initializing 3D scene". WriteInfo();
        
        // Grid and axis helpers are added by default in InitializeScene
        // No need to add them explicitly here

        $"3D scene initialized".WriteSuccess();
    }

    public async void StartTugOfWar3D()
    {
        $"Starting 3D Tug of War".WriteInfo();
        
        var arena = Workspace.GetArena();
        if (arena == null)
        {
            $"No arena available".WriteError();
            return;
        }

        // Clear existing objects and wait for JavaScript to process deletions
        arena.ClearArena();
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
                .BeforeAnimationRefresh((shape, tick, fps) =>
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
                .BeforeAnimationRefresh((shape, tick, fps) =>
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
        _distanceText.BeforeAnimationRefresh((text, tick, fps) =>
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

        arena.AddShapeToStage<FoShape3D>(_box1_3D);
        $"Added Box1 to stage at {_box1_3D.Transform.Position}".WriteSuccess();
        
        arena.AddShapeToStage<FoShape3D>(_box2_3D);
        $"Added Box2 to stage at {_box2_3D.Transform.Position}".WriteSuccess();
        
        arena.AddShapeToStage<FoPipe3D>(_tube_3D);
        $"Added connecting tube to stage".WriteSuccess();
        
        arena.AddShapeToStage<FoText3D>(_distanceText);
        $"Added distance text to stage at {_distanceText.Transform.Position}".WriteSuccess();

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

        arena.AddShapeToStage<FoPipe3D>(_growingPipe);
        $"Added growing pipe to stage".WriteSuccess();
        
        // Log scene status
        var (found, scene) = arena.CurrentScene();
        if (found && scene != null)
        {
            $"Scene '{scene.Title}' ready - objects should be visible".WriteSuccess();
        }
        else
        {
            $"WARNING: No scene found - objects won't be visible!".WriteError();
        }
        
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
        
        // Clear the arena/stage - this waits for deletion to complete
        var arena = Workspace?.GetArena();
        if (arena != null)
        {
            arena.ClearArena();
        }
        
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
        
        var arena = Workspace?.GetArena();
        if (arena != null)
        {
            arena.ClearArena();
        }
        
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
