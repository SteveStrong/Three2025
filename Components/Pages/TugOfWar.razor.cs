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
    private FoText3D _distanceText_3D;
    private List<(FoShape3D ground, FoShape3D top, FoPipe3D pole)> _flagPoles = new();
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
            
            // AUTO-START 3D animation for testing
            StartTugOfWar3D();
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
        FoGlyph2D.Animations.Tween<FoShape2D>(s1, new { PinX = s1.PinX - 150, }, 2, 2.2F);
        FoGlyph2D.Animations.Tween<FoShape2D>(s2, new { PinX = s2.PinX + 150, PinY = s2.PinY + 50, }, 2, 2.4f).OnComplete(() =>
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

        $"Initializing 3D scene".WriteInfo();
        
        // Add grid and axes helpers
        // await scene.DoAddGridHelper(20, 20);
        // await scene.DoAddAxisHelper(5);

        $"3D scene initialized".WriteSuccess();
    }

    public void StartTugOfWar3D()
    {
        $"Starting 3D Growing Flag Pole Test".WriteInfo();
        
        var arena = Workspace.GetArena();
        if (arena == null)
        {
            $"No arena available".WriteError();
            return;
        }

        // Clear existing objects
        arena.ClearArena();

        // Complex tug-of-war test with two boxes and connecting pipe
        _box1_3D = new FoShape3D($"Box1-{Guid.NewGuid().ToString().Substring(0, 8)}", "blue")
        {
            Transform = new Transform3("Box1Transform")
            {
                Position = new Vector3(-2, 0, 0),
            },
        }.CreateBox("Box1", 1.0, 1.0, 1.0);

        _box2_3D = new FoShape3D($"Box2-{Guid.NewGuid().ToString().Substring(0, 8)}", "orange")
        {
            Transform = new Transform3("Box2Transform")
            {
                Position = new Vector3(2, 0, 0),
            },
        }.CreateBox("Box2", 1.0, 1.0, 1.0);

        _tube_3D = new FoPipe3D($"Tube-{Guid.NewGuid().ToString().Substring(0, 8)}", "cyan")
        {
            FromShape3D = _box1_3D,
            ToShape3D = _box2_3D
        }.CreatePipe("ConnectingTube", 0.1);

        // Static test pipes (triangle formation)
        var staticBox1 = new FoShape3D("StaticBox1", "yellow")
        {
            Transform = new Transform3("StaticBox1Transform") { Position = new Vector3(-4, 2, 0) }
        }.CreateBox("StaticBox1", 0.5, 0.5, 0.5);
        
        var staticBox2 = new FoShape3D("StaticBox2", "green")
        {
            Transform = new Transform3("StaticBox2Transform") { Position = new Vector3(4, 2, 0) }
        }.CreateBox("StaticBox2", 0.5, 0.5, 0.5);
        
        var staticBox3 = new FoShape3D("StaticBox3", "purple")
        {
            Transform = new Transform3("StaticBox3Transform") { Position = new Vector3(0, 3, 3) }
        }.CreateBox("StaticBox3", 0.5, 0.5, 0.5);
        
        var staticPipe1 = new FoPipe3D("StaticPipe1", "yellow")
        {
            FromShape3D = staticBox1,
            ToShape3D = staticBox2
        }.CreatePipe("StaticPipe1", 0.05);
        
        var staticPipe2 = new FoPipe3D("StaticPipe2", "magenta")
        {
            FromShape3D = staticBox1,
            ToShape3D = staticBox3
        }.CreatePipe("StaticPipe2", 0.05);
        
        var staticPipe3 = new FoPipe3D("StaticPipe3", "lime")
        {
            FromShape3D = staticBox2,
            ToShape3D = staticBox3
        }.CreatePipe("StaticPipe3", 0.05);

        // Multiple flag poles test
        _flagPoles = new List<(FoShape3D ground, FoShape3D top, FoPipe3D pole)>();
        for (int i = 0; i < 5; i++)
        {
            var xPos = -6 + (i * 3);
            var ground = new FoShape3D($"FlagPoleBase{i}", "brown")
            {
                Transform = new Transform3($"FlagPoleBase{i}Transform") { Position = new Vector3(xPos, 0, -5) }
            }.CreateBox($"FlagPoleBase{i}", 0.3, 0.1, 0.3);
            
            var top = new FoShape3D($"FlagPoleTop{i}", "red")
            {
                Transform = new Transform3($"FlagPoleTop{i}Transform") { Position = new Vector3(xPos, START_HEIGHT, -5) }
            }.CreateBox($"FlagPoleTop{i}", 0.4, 0.1, 0.4);
            
            var pole = new FoPipe3D($"FlagPole{i}", "gray")
            {
                FromShape3D = ground,
                ToShape3D = top
            }.CreatePipe($"FlagPole{i}", 0.03);
            
            _flagPoles.Add((ground, top, pole));
        }

        // ADD ALL OBJECTS TO ARENA
        arena.AddShapeToStage<FoShape3D>(_box1_3D);
        arena.AddShapeToStage<FoShape3D>(_box2_3D);
        arena.AddShapeToStage<FoPipe3D>(_tube_3D);
        
        arena.AddShapeToStage<FoShape3D>(staticBox1);
        arena.AddShapeToStage<FoShape3D>(staticBox2);
        arena.AddShapeToStage<FoShape3D>(staticBox3);
        arena.AddShapeToStage<FoPipe3D>(staticPipe1);
        arena.AddShapeToStage<FoPipe3D>(staticPipe2);
        arena.AddShapeToStage<FoPipe3D>(staticPipe3);
        
        foreach (var (ground, top, pole) in _flagPoles)
        {
            arena.AddShapeToStage<FoShape3D>(ground);
            arena.AddShapeToStage<FoShape3D>(top);
            arena.AddShapeToStage<FoPipe3D>(pole);
        }

        // ULTRA SIMPLIFIED TEST: Just a pipe with animated path
        $"Creating pipe with animated path".WriteInfo();
        
        // Create pipe directly with initial path
        _growingPipe = new FoPipe3D("GrowingPipe", "red");
        
        // Initial path (straight line from origin upward)
        var initialPath = new List<Vector3>
        {
            new Vector3(10, 0, 10),      // Start point (static)
            new Vector3(10, START_HEIGHT, 10)  // End point (will animate)
        };
        
        _growingPipe.CreateTube("GrowingPipe", 0.25, initialPath);
        
        $"Pipe created with initial path: (0,0,0) -> (0,{START_HEIGHT},0)".WriteSuccess();

        // Add just the pipe to arena
        arena.AddShapeToStage<FoPipe3D>(_growingPipe);
        
        // Reset animation state - DON'T start animation yet
        _animationTime = 0;

        $"All 3D objects created - ready for animation".WriteSuccess();
        $"Use 'Start Animation' button to begin".WriteInfo();
    }

    // Unified animation function for all 3D objects
    private void Animate3D(Object3D self, int tick, double fps)
    {
        _animationTime += 1.0 / fps;
        var progress = Math.Min(_animationTime / ANIMATION_DURATION, 1.0);
        
        if (progress >= 1.0) 
        {
            StopAnimation3D();
            $"All 3D animations completed".WriteSuccess();
            return;
        }
        
        // 1. Animate tug-of-war boxes (move apart)
        if (_box1_3D != null && _box2_3D != null)
        {
            var box1NewX = -2 - (progress * BOX_MOVE_DISTANCE);
            var box2NewX = 2 + (progress * BOX_MOVE_DISTANCE);
            
            _box1_3D.Transform.Position = new Vector3(box1NewX, 0, 0);
            _box2_3D.Transform.Position = new Vector3(box2NewX, 0, 0);
            
            // Manually smash the connecting tube since we don't have 3D Glue yet
            _tube_3D?.SetGeometryStale();
        }
        
        // 2. Animate growing pipe (vertical growth)
        if (_growingPipe != null)
        {
            var newHeight = START_HEIGHT + (progress * (TARGET_HEIGHT - START_HEIGHT));
            _growingPipe.Path3D = new List<Vector3>
            {
                new Vector3(10, 0, 10),
                new Vector3(10, newHeight, 10)
            };
        }
        
        // 3. Animate flag poles (grow upward)
        if (_flagPoles != null && _flagPoles.Count > 0)
        {
            var flagHeight = START_HEIGHT + (progress * (TARGET_HEIGHT - START_HEIGHT));
            foreach (var (ground, top, pole) in _flagPoles)
            {
                var currentPos = top.Transform.Position;
                top.Transform.Position = new Vector3(currentPos.X, flagHeight, currentPos.Z);
                
                // Manually smash the pole since we don't have 3D Glue yet
                pole?.SetGeometryStale();
            }
        }
    }

    public void StartAnimation3D()
    {
        if (_growingPipe == null && _box1_3D == null && _flagPoles.Count == 0)
        {
            $"No objects available - call StartTugOfWar3D first".WriteError();
            return;
        }
        
        _animationTime = 0;
        
        // Register animation callback on growing pipe (will drive all animations)
        if (_growingPipe != null)
        {
            _growingPipe.SetAnimationUpdate(Animate3D);
            $"3D animation started - animating pipe, boxes, and flag poles".WriteSuccess();
        }
        else
        {
            $"No growing pipe to attach animation to".WriteError();
        }
    }

    public void StopAnimation3D()
    {
        if (_growingPipe != null)
        {
            _growingPipe.ClearAnimationUpdate();
        }
        
        $"3D animation stopped".WriteInfo();
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

    public void Reset3D()
    {
        if (_growingPipe != null)
        {
            _growingPipe.ClearAnimationUpdate();
        }
        
        _animationTime = 0;
        
        var arena = Workspace?.GetArena();
        arena?.ClearArena();
        
        _growingPipe = null;
        _box1_3D = null;
        _box2_3D = null;
        _tube_3D = null;
        _flagPoles.Clear();
        
        $"3D scene reset".WriteInfo();
    }

    public void Dispose()
    {
        _growingPipe?.ClearAnimationUpdate();
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
        $"TugOfWar Page Disposed".WriteInfo();
    }
}
