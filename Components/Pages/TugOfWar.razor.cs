using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD;

using FoundryWorldsAndDrawings.ThreeD.Viewers;
using BlazorComponentBus;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Components.Pages;

public partial class TugOfWarBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }

    public FoundryWorldsAndDrawings.Shared.Canvas2DComponent Canvas2DReference = null;
    public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;

    [Parameter] public int CanvasWidth { get; set; } = 800;
    [Parameter] public int CanvasHeight { get; set; } = 600;

    protected MockDataGenerator DataGenerator { get; set; } = new();

    // 3D Animation state
    private FoModel3D _box1_3D;
    private FoModel3D _box2_3D;
    private FoModel3D _tube_3D;
    private bool _isAnimating3D = false;
    private double _animationTime = 0;
    private const double ANIMATION_DURATION = 2.0; // seconds

    protected override void OnInitialized()
    {
        base.OnInitialized();
        $"TugOfWar Page OnInitialized".WriteInfo();
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

        // Create two shapes
        var s1 = new FoShape2D(50, 50, "Blue");
        s1.MoveTo(300, 300);
        var s2 = new FoShape2D(50, 50, "Orange");
        s2.MoveTo(500, 300);
        
        var service = Workspace.GetSelectionService();
        service.AddItem(drawing.AddShape(s1));
        service.AddItem(drawing.AddShape(s2));
        
        // Create connecting arrow
        var wire = new FoShape1D("Arrow", "Cyan")
        {
            Height = 50,
            ShapeDraw = async (ctx, obj) => await DrawArrowAsync(ctx, obj.Width, obj.Height, obj.Color)
        };
        wire.GlueStartTo(s1, "RIGHT");
        wire.GlueFinishTo(s2, "LEFT");
        drawing.AddShape(wire);
        
        // Animate both shapes
        FoGlyph2D.Animations.Tween<FoShape2D>(s1, new { PinX = s1.PinX - 150, }, 2, 2.2F);
        FoGlyph2D.Animations.Tween<FoShape2D>(s2, new { PinX = s2.PinX + 150, PinY = s2.PinY + 50, }, 2, 2.4f).OnComplete(() =>
        {
            service.ClearAll();
            $"2D Tug of War animation completed".WriteSuccess();
        });
        
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
        $"Starting 3D Tug of War".WriteInfo();
        
        var arena = Workspace.GetArena();
        if (arena == null)
        {
            $"No arena available".WriteError();
            return;
        }

        // Clear existing objects
        arena.ClearArena();

        // Create Box 1 (Blue) - starting position left
        _box1_3D = new FoModel3D($"Box1-{Guid.NewGuid().ToString().Substring(0, 8)}")
        {
            Url = GetReferenceTo(@"storage/StaticFiles/box.glb"),
            Transform = new Transform3("Box1Transform")
            {
                Position = new Vector3(-2, 0, 0),
                Scale = new Vector3(0.5, 0.5, 0.5),
            },
        };

        // Create Box 2 (Red) - starting position right
        _box2_3D = new FoModel3D($"Box2-{Guid.NewGuid().ToString().Substring(0, 8)}")
        {
            Url = GetReferenceTo(@"storage/StaticFiles/box.glb"),
            Transform = new Transform3("Box2Transform")
            {
                Position = new Vector3(2, 0, 0),
                Scale = new Vector3(0.5, 0.5, 0.5),
            },
        };

        // Create connecting tube (Cyan)
        _tube_3D = new FoModel3D($"Tube-{Guid.NewGuid().ToString().Substring(0, 8)}")
        {
            Url = GetReferenceTo(@"storage/StaticFiles/tube.glb"),
            Transform = new Transform3("TubeTransform")
            {
                Position = new Vector3(0, 0, 0),
                Scale = new Vector3(4, 0.2, 0.2), // Long thin tube
            },
        };

        // Add to arena
        arena.AddShapeToStage<FoModel3D>(_box1_3D);
        arena.AddShapeToStage<FoModel3D>(_box2_3D);
        arena.AddShapeToStage<FoModel3D>(_tube_3D);

        // Reset animation state
        _isAnimating3D = true;
        _animationTime = 0;

        // Setup animation callbacks
        _box1_3D.SetAnimationUpdate((self, tick, fps) =>
        {
            if (!_isAnimating3D) return;

            _animationTime += 1.0 / fps;
            var progress = Math.Min(_animationTime / ANIMATION_DURATION, 1.0);

            // Box 1 moves left
            var newX = -2 - (progress * 3); // Move from -2 to -5
            self.Transform.Position = new Vector3(newX, 0, 0);

            if (progress >= 1.0)
            {
                _isAnimating3D = false;
                $"3D Tug of War animation completed".WriteSuccess();
            }
        });

        _box2_3D.SetAnimationUpdate((self, tick, fps) =>
        {
            if (!_isAnimating3D) return;

            var progress = Math.Min(_animationTime / ANIMATION_DURATION, 1.0);

            // Box 2 moves right and up
            var newX = 2 + (progress * 3); // Move from 2 to 5
            var newY = progress * 1; // Move up by 1 unit
            self.Transform.Position = new Vector3(newX, newY, 0);
        });

        // Update tube to stretch between boxes
        _tube_3D.SetAnimationUpdate((self, tick, fps) =>
        {
            if (_box1_3D == null || _box2_3D == null) return;

            var pos1 = _box1_3D.Transform.Position;
            var pos2 = _box2_3D.Transform.Position;

            // Center between boxes
            var centerX = (pos1.X + pos2.X) / 2;
            var centerY = (pos1.Y + pos2.Y) / 2;
            self.Transform.Position = new Vector3(centerX, centerY, 0);

            // Scale to stretch between boxes
            var distance = Math.Sqrt(Math.Pow(pos2.X - pos1.X, 2) + Math.Pow(pos2.Y - pos1.Y, 2));
            self.Transform.Scale = new Vector3(distance / 2, 0.2, 0.2);
            
            // Rotate to point from box1 to box2
            var angle = Math.Atan2(pos2.Y - pos1.Y, pos2.X - pos1.X);
            self.Transform.Rotation = new Euler(0, 0, angle);
        });

        $"3D Tug of War started".WriteSuccess();
    }

    public void Reset3D()
    {
        _isAnimating3D = false;
        _animationTime = 0;
        
        var arena = Workspace?.GetArena();
        arena?.ClearArena();
        
        $"3D scene reset".WriteInfo();
    }

    public void Dispose()
    {
        _isAnimating3D = false;
        $"TugOfWar Page Disposed".WriteInfo();
    }
}
