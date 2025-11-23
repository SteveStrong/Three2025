using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Components.Pages;

public partial class MultiCanvas2DTest : IDisposable
{
    [Inject] public required IFoundryService Foundry { get; set; }
    [Inject] public required IWorkspace Workspace { get; set; }

    private IDrawing drawing => Workspace.GetDrawing()!;

    private FoShape2D _rectA = null!;
    private FoShape2D _circleB1 = null!, _circleB2 = null!, _circleB3 = null!;
    private FoShape2D _boxC1 = null!, _boxC2 = null!;
    private FoShape1D _connectorC = null!;

    private double _rotationA = 0;
    private double _timeB = 0;
    private double _positionC = 0;

    protected override async Task OnInitializedAsync()
    {
        "MultiCanvas2DTest: Initializing".WriteInfo();
        "MultiCanvas2DTest: Initialized - pages will be setup after render".WriteSuccess();
    }

    private void SetupPageA()
    {
        "SetupPageA: Starting".WriteInfo();
        if (drawing == null)
        {
            "SetupPageA: Drawing is NULL!".WriteError();
            return;
        }
        
        var page = drawing.EstablishPage<FoPage2D>("PageA");
        page.Color = "LightCoral";  // Subtle background color
        // DON'T call SetCurrentPage - let Canvas2DComponent manage that per canvas

        // Create a rotating rectangle
        _rectA = new FoShape2D(100, 100, "DarkBlue");
        _rectA.MoveTo(400, 300);
        page.AddShape(_rectA);

        $"Page A setup complete - page has {page.Members<FoShape2D>().Count()} shapes".WriteSuccess();
    }

    private void SetupPageB()
    {
        "SetupPageB: Starting".WriteInfo();
        if (drawing == null)
        {
            "SetupPageB: Drawing is NULL!".WriteError();
            return;
        }
        
        var page = drawing.EstablishPage<FoPage2D>("PageB");
        page.Color = "LightSkyBlue";  // Subtle background to verify it's rendering
        // DON'T call SetCurrentPage - let Canvas2DComponent manage that per canvas

        // Create three circles  
        _circleB1 = new FoShape2D(60, 60, "red");
        _circleB1.MoveTo(200, 300);
        page.AddShape(_circleB1);

        _circleB2 = new FoShape2D(60, 60, "green");
        _circleB2.MoveTo(400, 300);
        page.AddShape(_circleB2);

        _circleB3 = new FoShape2D(60, 60, "blue");
        _circleB3.MoveTo(600, 300);
        page.AddShape(_circleB3);

        $"Page B setup complete - page has {page.Members<FoShape2D>().Count()} shapes".WriteSuccess();
    }

    private void SetupPageC()
    {
        "SetupPageC: Starting".WriteInfo();
        if (drawing == null)
        {
            "SetupPageC: Drawing is NULL!".WriteError();
            return;
        }
        
        var page = drawing.EstablishPage<FoPage2D>("PageC");
        page.Color = "LightGreen";  // Subtle background to verify it's rendering
        // DON'T call SetCurrentPage - let Canvas2DComponent manage that per canvas

        // Create two boxes and a connector
        _boxC1 = new FoShape2D(80, 80, "orange");
        _boxC1.MoveTo(200, 300);
        page.AddShape(_boxC1);

        _boxC2 = new FoShape2D(80, 80, "purple");
        _boxC2.MoveTo(600, 300);
        page.AddShape(_boxC2);

        // Create a 1D connector between them
        _connectorC = new FoShape1D("Arrow", "cyan");
        _connectorC.Height = 50;
        _connectorC.GlueStartTo(_boxC1, "RIGHT");
        _connectorC.GlueFinishTo(_boxC2, "LEFT");
        page.AddShape(_connectorC);

        $"Page C setup complete - page has {page.Members<FoGlyph2D>().Count()} shapes".WriteSuccess();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        "MultiCanvas2DTest: First render - delaying page setup to ensure all canvases are ready".WriteInfo();

        // Delay page setup to ensure all Canvas2DComponents have completed their OnAfterRenderAsync
        Task.Run(async () =>
        {
            await Task.Delay(100); // Small delay to let all canvases initialize

            "MultiCanvas2DTest: Setting up pages now".WriteInfo();

            // Setup all three pages
            SetupPageA();
            SetupPageB();
            SetupPageC();

            // Subscribe to PreAnimation for updates
            AnimationFrameBus.SubscribeToPreAnimation(HandleAnimationFrame);

            "MultiCanvas2DTest: All pages setup and connected".WriteSuccess();
        });
    }

    private void HandleAnimationFrame(PreAnimationEvent message)
    {
        if (!message.IsDrawing2D()) return;

        var deltaTime = 1.0 / Math.Max(message.fps, 1);

        // Animate Page A - Rotating rectangle
        if (_rectA != null)
        {
            _rotationA += deltaTime * 45.0; // 45 degrees per second
            _rotationA %= 360.0;
            _rectA.Angle = _rotationA;
        }

        // Animate Page B - Moving circles vertically
        if (_circleB1 != null && _circleB2 != null && _circleB3 != null)
        {
            _timeB += deltaTime;
            
            var baseY = 300;
            var amplitude = 100;
            
            _circleB1.PinY = baseY + (int)(Math.Sin(_timeB * 2) * amplitude);
            _circleB2.PinY = baseY + (int)(Math.Sin(_timeB * 2 + Math.PI * 2/3) * amplitude);
            _circleB3.PinY = baseY + (int)(Math.Sin(_timeB * 2 + Math.PI * 4/3) * amplitude);
        }

        // Animate Page C - Moving boxes horizontally (connector follows)
        if (_boxC1 != null && _boxC2 != null)
        {
            _positionC += deltaTime * 50; // 50 pixels per second
            
            var baseX1 = 200;
            var baseX2 = 600;
            var offset = (int)(Math.Sin(_positionC * 0.02) * 50);
            
            _boxC1.PinX = baseX1 + offset;
            _boxC2.PinX = baseX2 - offset;
            // Connector automatically updates because it's glued to the boxes
        }
    }

    public void Dispose()
    {
        "MultiCanvas2DTest: Disposing".WriteInfo();

        AnimationFrameBus.UnSubscribeFromPreAnimation(HandleAnimationFrame);

        // Don't clear drawing here - let page navigation handle it
        "MultiCanvas2DTest: Disposed".WriteInfo();
    }
}
