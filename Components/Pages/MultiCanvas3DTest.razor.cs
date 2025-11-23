using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Components.Pages;

public partial class MultiCanvas3DTest : IDisposable
{
    [Inject] public required IFoundryService Foundry { get; set; }

    private IArena arena => Foundry.Arena();

    private FoShape3D? _cubeA;
    private FoShape3D? _sphereB1, _sphereB2, _sphereB3;
    private FoShape3D? _cylinderC, _coneC;

    private double _rotationA = 0;
    private double _timeB = 0;
    private double _timeC = 0;

    protected override async Task OnInitializedAsync()
    {
        "MultiCanvas3DTest: Initializing".WriteInfo();
        "MultiCanvas3DTest: Initialized - scenes will be setup after render".WriteSuccess();
    }

    private void SetupSceneA()
    {
        "SetupSceneA: Starting".WriteInfo();
        if (arena == null)
        {
            "SetupSceneA: Arena is NULL!".WriteError();
            return;
        }
        
        var stage = arena.EstablishStage<FoStage3D>("SceneA");
        arena.SetCurrentStage(stage);

        // Create a rotating cube - just add to stage, scene sync happens automatically
        _cubeA = new FoShape3D("RotatingCube", "red").CreateBox("RotatingCube", 2, 2, 2);
        _cubeA.Transform.Position.Y = 0;
        stage.AddShape(_cubeA);

        $"Scene A setup complete - stage has {stage.Members<FoShape3D>().Count()} shapes".WriteSuccess();
    }

    private void SetupSceneB()
    {
        "SetupSceneB: Starting".WriteInfo();
        if (arena == null)
        {
            "SetupSceneB: Arena is NULL!".WriteError();
            return;
        }
        
        var stage = arena.EstablishStage<FoStage3D>("SceneB");
        arena.SetCurrentStage(stage);

        // Create three spheres - just add to stage, scene sync happens automatically
        _sphereB1 = new FoShape3D("SphereRed", "red").CreateSphere("SphereRed", 1, 1, 1);
        _sphereB1.Transform.Position.Set(-3, 0, 0);
        stage.AddShape(_sphereB1);

        _sphereB2 = new FoShape3D("SphereGreen", "green").CreateSphere("SphereGreen", 1, 1, 1);
        _sphereB2.Transform.Position.Set(0, 0, 0);
        stage.AddShape(_sphereB2);

        _sphereB3 = new FoShape3D("SphereBlue", "blue").CreateSphere("SphereBlue", 1, 1, 1);
        _sphereB3.Transform.Position.Set(3, 0, 0);
        stage.AddShape(_sphereB3);

        $"Scene B setup complete - stage has {stage.Members<FoShape3D>().Count()} shapes".WriteSuccess();
    }

    private void SetupSceneC()
    {
        "SetupSceneC: Starting".WriteInfo();
        if (arena == null)
        {
            "SetupSceneC: Arena is NULL!".WriteError();
            return;
        }
        
        var stage = arena.EstablishStage<FoStage3D>("SceneC");
        arena.SetCurrentStage(stage);

        // Create shapes - just add to stage, scene sync happens automatically
        _cylinderC = new FoShape3D("Cylinder", "orange").CreateCylinder("Cylinder", 1, 3, 1);
        _cylinderC.Transform.Position.Set(-2, 0, 0);
        stage.AddShape(_cylinderC);

        _coneC = new FoShape3D("Cone", "purple").CreateCone("Cone", 1.5, 3, 1.5);
        _coneC.Transform.Position.Set(2, 0, 0);
        stage.AddShape(_coneC);

        $"Scene C setup complete - stage has {stage.Members<FoShape3D>().Count()} shapes".WriteSuccess();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;

        "MultiCanvas3DTest: First render - delaying scene setup to ensure all canvases are ready".WriteInfo();

        // Delay scene setup to ensure all Canvas3DComponents have completed their OnAfterRenderAsync
        // This prevents the race condition where shapes are added before scenes exist
        Task.Run(async () =>
        {
            await Task.Delay(100); // Small delay to let all canvases initialize

            "MultiCanvas3DTest: Setting up scenes now".WriteInfo();

            // Setup all three scenes - Canvas3DComponent already created matching stages and linked them
            SetupSceneA();
            SetupSceneB();
            SetupSceneC();

            // Subscribe to PreAnimation for updates
            AnimationFrameBus.SubscribeToPreAnimation(HandleAnimationFrame);

            "MultiCanvas3DTest: All scenes setup and connected".WriteSuccess();
        });
    }

    private void HandleAnimationFrame(PreAnimationEvent message)
    {
        if (!message.IsWorld3D()) return;

        var deltaTime = 1.0 / Math.Max(message.fps, 1);

        // Animate Scene A - Rotating cube
        if (_cubeA != null)
        {
            _rotationA += deltaTime * 1.0; // 1 radian per second
            _rotationA %= (2 * Math.PI); // Keep within 0 to 2π
            
            // Use object replacement pattern to trigger dirty flag
            var rot = _cubeA.Transform.Rotation;
            _cubeA.Transform.Rotation = new FoundryWorldsAndDrawings.ThreeD.Maths.Euler(rot.X, _rotationA, rot.Z);
        }

        // Animate Scene B - Bouncing spheres
        if (_sphereB1 != null && _sphereB2 != null && _sphereB3 != null)
        {
            _timeB += deltaTime;
            
            // Use object replacement pattern for positions
            var pos1 = _sphereB1.Transform.Position;
            _sphereB1.Transform.Position = new FoundryWorldsAndDrawings.ThreeD.Maths.Vector3(pos1.X, Math.Sin(_timeB * 2) * 2, pos1.Z);
            
            var pos2 = _sphereB2.Transform.Position;
            _sphereB2.Transform.Position = new FoundryWorldsAndDrawings.ThreeD.Maths.Vector3(pos2.X, Math.Sin(_timeB * 2 + Math.PI * 2/3) * 2, pos2.Z);
            
            var pos3 = _sphereB3.Transform.Position;
            _sphereB3.Transform.Position = new FoundryWorldsAndDrawings.ThreeD.Maths.Vector3(pos3.X, Math.Sin(_timeB * 2 + Math.PI * 4/3) * 2, pos3.Z);
        }

        // Animate Scene C - Rotating shapes
        if (_cylinderC != null && _coneC != null)
        {
            _timeC += deltaTime * 0.5;
            _timeC %= (2 * Math.PI); // Keep within 0 to 2π
            
            // Use object replacement pattern for rotations
            var rotC = _cylinderC.Transform.Rotation;
            _cylinderC.Transform.Rotation = new FoundryWorldsAndDrawings.ThreeD.Maths.Euler(rotC.X, _timeC, rotC.Z);
            
            var rotCone = _coneC.Transform.Rotation;
            _coneC.Transform.Rotation = new FoundryWorldsAndDrawings.ThreeD.Maths.Euler(rotCone.X, -_timeC, rotCone.Z);
        }
    }

    public void Dispose()
    {
        "MultiCanvas3DTest: Disposing".WriteInfo();

        AnimationFrameBus.UnSubscribeFromPreAnimation(HandleAnimationFrame);

        // Don't clear arena here - let page navigation handle it
        "MultiCanvas3DTest: Disposed".WriteInfo();
    }
}
