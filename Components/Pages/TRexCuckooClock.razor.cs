using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Objects;

namespace Three2025.Components.Pages;

public partial class TRexCuckooClockBase : ComponentBase, IDisposable
{
    public Canvas3DComponent Canvas3DReference = null;

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public ICuckooClockTech CuckooTech { get; init; }

    [Parameter] public int CanvasWidth { get; set; } = 1400;
    [Parameter] public int CanvasHeight { get; set; } = 1200;

    protected double _currentFps = 0;
    protected int _currentTick = 0;
    protected string _cuckooState = "Idle";
    protected bool _clockCreated = false;
    protected int _timeMultiplier = 60; // 60x speed = 1 real second = 1 minute
    protected bool _animationRunning = true; // Animation starts when clock is created

    private FoStage3D _cuckooStage;

    public (bool, Scene3D) GetCurrentScene()
    {
        return Canvas3DReference?.GetActiveScene() ?? (false, null!);
    }

    protected override async Task OnInitializedAsync()
    {
        Workspace.SetBaseUrl(Navigation?.BaseUri ?? "");
        
        // Subscribe to animation events for FPS/Tick display
        $"TRexCuckooClock: Subscribing to AnimationEvent on AnimationFrameBus".WriteSuccess();
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
        
        await base.OnInitializedAsync();
    }

    private void OnAnimationFrame(AnimationEvent animEvent)
    {
        if (animEvent.IsWorld3D())
        {
            _currentFps = animEvent.fps;
            _currentTick = animEvent.tick;
            
            // Update UI every frame for smooth display
            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        _ = _cuckooStage?.ClearAll();
        $"TRexCuckooClock: Cleared stage on dispose".WriteInfo();
        
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Wait for canvas to initialize
            await Task.Delay(500);
            
            if (Canvas3DReference != null)
            {
                var (found, scene) = Canvas3DReference.GetActiveScene();
                
                if (found && scene != null)
                {
                    // Get stage from Canvas (already created and linked by Canvas3DComponent)
                    _cuckooStage = Canvas3DReference.Stage;
                    
                    $"TRexCuckooClock: Retrieved stage '{_cuckooStage?.Key}' from Canvas".WriteSuccess();
                    
                    // Auto-create the clock on startup
                    DoCreateCuckooClock();
                }
                else
                {
                    $"TRexCuckooClock: Failed to get active scene from Canvas".WriteError();
                }
            }
            else
            {
                $"TRexCuckooClock: Canvas3DReference is null".WriteError();
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private string GetReferenceTo(string path)
    {
        return Navigation?.ToAbsoluteUri(path).ToString() ?? path;
    }

    // Phase 1: Button handlers (stubbed out for now)
    public void DoCreateCuckooClock()
    {
        if (_cuckooStage == null)
        {
            $"TRexCuckooClock: Stage not initialized yet".WriteError();
            return;
        }

        // Phase 2/3: Create and add housing to stage with time multiplier and T-Rex
        $"TRexCuckooClock: Creating cuckoo clock housing with {_timeMultiplier}x time acceleration".WriteInfo();
        var tRexUrl = GetReferenceTo(@"storage/staticfiles/T_Rex.glb");
        var housing = CuckooTech.CreateCuckooClockHousing(_timeMultiplier, tRexUrl);
        _cuckooStage.AddShape(housing);
        
        _clockCreated = true;
        $"TRexCuckooClock: Clock housing added to stage".WriteSuccess();
    }

    public void DoSetTimeSpeed(int multiplier)
    {
        _timeMultiplier = multiplier;
        $"TRexCuckooClock: Time speed set to {_timeMultiplier}x".WriteInfo();
    }

    public void DoToggleAnimation()
    {
        _animationRunning = !_animationRunning;
        CuckooTech.SetAnimationRunning(_animationRunning);
        $"TRexCuckooClock: Animation {(_animationRunning ? "started" : "stopped")}".WriteInfo();
    }

    public void DoTriggerCuckoo()
    {
        if (!_clockCreated)
        {
            $"TRexCuckooClock: Create the clock first!".WriteWarning();
            return;
        }

        // TODO: Phase 7 - Trigger cuckoo sequence
        $"TRexCuckooClock: Trigger cuckoo button clicked (implementation pending)".WriteInfo();
    }

    public void DoTestDoors()
    {
        if (!_clockCreated)
        {
            $"TRexCuckooClock: Create the clock first!".WriteWarning();
            return;
        }

        // TODO: Phase 4 - Test door animation
        $"TRexCuckooClock: Test doors button clicked (implementation pending)".WriteInfo();
    }

    public void DoClearStage()
    {
        _ = _cuckooStage?.ClearAll();
        _clockCreated = false;
        $"TRexCuckooClock: Cleared stage".WriteInfo();
    }
}
