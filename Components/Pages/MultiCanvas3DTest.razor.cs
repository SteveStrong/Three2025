using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Components.Pages;

public partial class MultiCanvas3DTest : IDisposable
{
    [Inject] public required IFoundryService Foundry { get; set; }

    private IArena arena => Foundry.Arena();

    private FoShape3D _cubeA;
    private FoShape3D _sphereB1, _sphereB2, _sphereB3;
    private FoShape3D _cylinderC, _coneC;

    // Animation state - captured in closures
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

        // Create a rotating cube - add DIRECTLY to this specific stage
        _cubeA = new FoShape3D("RotatingCube", "red").CreateBox("RotatingCube", 2, 2, 2);
        _cubeA.Transform.Position.Y = 0;
        
        // Animation via BeforeAnimationRefresh - invoked automatically during RenderStage
        _cubeA.BeforeAnimationRefresh((shape, tick, fps) => {
            var deltaTime = 1.0 / Math.Max(fps, 1);
            _rotationA += deltaTime * 1.0; // 1 radian per second
            _rotationA %= (2 * Math.PI);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new FoundryWorldsAndDrawings.ThreeD.Maths.Euler(rot.X, _rotationA, rot.Z);
        });
        
        stage.AddShape(_cubeA);

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

        // Create three spheres with animation callbacks
        _sphereB1 = new FoShape3D("SphereRed", "red").CreateSphere("SphereRed", 1, 1, 1);
        _sphereB1.Transform.Position.Set(-3, 0, 0);
        _sphereB1.BeforeAnimationRefresh((shape, tick, fps) => {
            var deltaTime = 1.0 / Math.Max(fps, 1);
            _timeB += deltaTime;
            var pos = shape.Transform.Position;
            shape.Transform.Position = new FoundryWorldsAndDrawings.ThreeD.Maths.Vector3(pos.X, Math.Sin(_timeB * 2) * 2, pos.Z);
        });
        stage.AddShape(_sphereB1);

        _sphereB2 = new FoShape3D("SphereGreen", "green").CreateSphere("SphereGreen", 1, 1, 1);
        _sphereB2.Transform.Position.Set(0, 0, 0);
        _sphereB2.BeforeAnimationRefresh((shape, tick, fps) => {
            var pos = shape.Transform.Position;
            shape.Transform.Position = new FoundryWorldsAndDrawings.ThreeD.Maths.Vector3(pos.X, Math.Sin(_timeB * 2 + Math.PI * 2/3) * 2, pos.Z);
        });
        stage.AddShape(_sphereB2);

        _sphereB3 = new FoShape3D("SphereBlue", "blue").CreateSphere("SphereBlue", 1, 1, 1);
        _sphereB3.Transform.Position.Set(3, 0, 0);
        _sphereB3.BeforeAnimationRefresh((shape, tick, fps) => {
            var pos = shape.Transform.Position;
            shape.Transform.Position = new FoundryWorldsAndDrawings.ThreeD.Maths.Vector3(pos.X, Math.Sin(_timeB * 2 + Math.PI * 4/3) * 2, pos.Z);
        });
        stage.AddShape(_sphereB3);

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

        // Create shapes with animation callbacks
        _cylinderC = new FoShape3D("Cylinder", "orange").CreateCylinder("Cylinder", 1, 3, 1);
        _cylinderC.Transform.Position.Set(-2, 0, 0);
        _cylinderC.BeforeAnimationRefresh((shape, tick, fps) => {
            var deltaTime = 1.0 / Math.Max(fps, 1);
            _timeC += deltaTime * 0.5;
            _timeC %= (2 * Math.PI);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new FoundryWorldsAndDrawings.ThreeD.Maths.Euler(rot.X, _timeC, rot.Z);
        });
        stage.AddShape(_cylinderC);

        _coneC = new FoShape3D("Cone", "purple").CreateCone("Cone", 1.5, 3, 1.5);
        _coneC.Transform.Position.Set(2, 0, 0);
        _coneC.BeforeAnimationRefresh((shape, tick, fps) => {
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new FoundryWorldsAndDrawings.ThreeD.Maths.Euler(rot.X, -_timeC, rot.Z);
        });
        stage.AddShape(_coneC);

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

            // No event subscription needed - animation happens via BeforeAnimationRefresh callbacks
            // which are invoked automatically during stage.RenderStage() → glyph.UpdateForAnimation()

            "MultiCanvas3DTest: All scenes setup and connected".WriteSuccess();
        });
    }

    public void Dispose()
    {
        "MultiCanvas3DTest: Disposing".WriteInfo();
        // No event unsubscription needed - shapes clean up their own callbacks
        "MultiCanvas3DTest: Disposed".WriteInfo();
    }
}
