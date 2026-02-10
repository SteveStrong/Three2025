using Microsoft.AspNetCore.Components;
using FoundryMicroCore.Core;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryRulesAndUnits.Extensions;

namespace Three2025.Components.Pages;

/// <summary>
/// Demonstrates three independent Canvas3DComponents on a single page,
/// each with its own scene/stage, plus a unified tree view of the arena hierarchy.
/// </summary>
public partial class MultiCanvas3DTest : ComponentBase, IDisposable
{
    [Inject] public required IWorkspace Workspace { get; set; }

    // Canvas references — each creates its own Stage/Scene pair via SceneName
    private Canvas3DComponent _canvasA = null!;
    private Canvas3DComponent _canvasB = null!;
    private Canvas3DComponent _canvasC = null!;

    // Arena tree root for the UnifiedTreeView
    private ITreeNode? _arenaNode;

    // UI counters
    private int _sceneCount;
    private int _shapeCount;
    private bool _shapesAdded;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;

        // Give all three Canvas3DComponents time to initialise their scenes/stages
        await Task.Delay(500);

        var arena = Workspace.GetArena();
        _arenaNode = arena as ITreeNode;

        // Populate each scene with starter shapes
        SetupSceneA();
        SetupSceneB();
        SetupSceneC();
        _shapesAdded = true;

        UpdateCounts();
        await InvokeAsync(StateHasChanged);
    }

    // ── Scene A — a single rotating red cube ──────────────────────────────
    private void SetupSceneA()
    {
        var stage = _canvasA?.Stage;
        if (stage == null) { "MultiCanvas3D: Stage A not ready".WriteWarning(); return; }

        var cube = new FoShape3D("RotatingCube", "red")
            .CreateBox("RotatingCube", 2, 2, 2);

        cube.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            var newY = rot.Y + dt * 1.0;        // 1 rad/s
            shape.Transform.Rotation = new Euler(rot.X, newY, rot.Z);
        });

        stage.AddShape(cube);
        $"MultiCanvas3D: Scene A — added RotatingCube to stage '{stage.Name}'".WriteSuccess();
    }

    // ── Scene B — three bouncing spheres ──────────────────────────────────
    private void SetupSceneB()
    {
        var stage = _canvasB?.Stage;
        if (stage == null) { "MultiCanvas3D: Stage B not ready".WriteWarning(); return; }

        var colors = new[] { "red", "green", "blue" };
        var offsets = new[] { -3.0, 0.0, 3.0 };
        var phaseOffset = new[] { 0.0, Math.PI * 2.0 / 3.0, Math.PI * 4.0 / 3.0 };

        for (int i = 0; i < 3; i++)
        {
            var idx = i;                                       // capture for closure
            var name = $"Sphere{colors[idx]}";
            var sphere = new FoShape3D(name, colors[idx])
                .CreateSphere(name, 1, 1, 1);
            sphere.Transform.Position = new Vector3(offsets[idx], 0, 0);

            sphere.OnBeforeRender((shape, tick, fps) =>
            {
                var t = tick / Math.Max(fps, 1);              // elapsed seconds
                var y = Math.Sin(t * 2.0 + phaseOffset[idx]) * 2.0;
                var pos = shape.Transform.Position;
                shape.Transform.Position = new Vector3(pos.X, y, pos.Z);
            });

            stage.AddShape(sphere);
        }

        $"MultiCanvas3D: Scene B — added 3 spheres to stage '{stage.Name}'".WriteSuccess();
    }

    // ── Scene C — assorted shapes (cylinder, cone, torus, dodecahedron) ───
    private void SetupSceneC()
    {
        var stage = _canvasC?.Stage;
        if (stage == null) { "MultiCanvas3D: Stage C not ready".WriteWarning(); return; }

        var cylinder = new FoShape3D("Cylinder", "orange")
            .CreateCylinder("Cylinder", 1, 3, 1);
        cylinder.Transform.Position = new Vector3(-4, 0, 0);
        cylinder.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X, rot.Y + dt * 0.5, rot.Z);
        });
        stage.AddShape(cylinder);

        var cone = new FoShape3D("Cone", "purple")
            .CreateCone("Cone", 1.5, 3, 1.5);
        cone.Transform.Position = new Vector3(0, 0, 0);
        cone.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X, rot.Y - dt * 0.7, rot.Z);
        });
        stage.AddShape(cone);

        var torus = new FoShape3D("Torus", "cyan")
            .CreateTorus("Torus", 1.2, 0.4, 1.2);
        torus.Transform.Position = new Vector3(4, 1, 0);
        torus.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X + dt * 0.8, rot.Y + dt * 0.3, rot.Z);
        });
        stage.AddShape(torus);

        var dodeca = new FoShape3D("Dodecahedron", "gold")
            .CreateDodecahedron("Dodecahedron", 1.5, 1.5, 1.5);
        dodeca.Transform.Position = new Vector3(8, 0, 0);
        dodeca.OnBeforeRender((shape, tick, fps) =>
        {
            var dt = 1.0 / Math.Max(fps, 1);
            var rot = shape.Transform.Rotation;
            shape.Transform.Rotation = new Euler(rot.X + dt * 0.4, rot.Y + dt * 0.6, rot.Z + dt * 0.2);
        });
        stage.AddShape(dodeca);

        $"MultiCanvas3D: Scene C — added 4 shapes to stage '{stage.Name}'".WriteSuccess();
    }

    // ── Button handlers ───────────────────────────────────────────────────
    private void DoAddShapes()
    {
        if (_shapesAdded) return;
        SetupSceneA();
        SetupSceneB();
        SetupSceneC();
        _shapesAdded = true;
        UpdateCounts();
    }

    private async Task DoClearAll()
    {
        var stageA = _canvasA?.Stage;
        var stageB = _canvasB?.Stage;
        var stageC = _canvasC?.Stage;

        if (stageA != null) await stageA.ClearAll();
        if (stageB != null) await stageB.ClearAll();
        if (stageC != null) await stageC.ClearAll();

        _shapesAdded = false;
        UpdateCounts();
        $"MultiCanvas3D: All stages cleared".WriteInfo();
    }

    private void DoRefreshTree()
    {
        UpdateCounts();
        StateHasChanged();
    }

    private void UpdateCounts()
    {
        var arena = Workspace.GetArena();
        if (arena == null) return;
        _sceneCount = arena.GetAllStages().Count;
        _shapeCount = arena.GetAllStages().Sum(s => s.AllBodies().Count() + s.AllLinks().Count());
    }

    public void Dispose()
    {
        "MultiCanvas3DTest: Disposing".WriteInfo();
    }
}
