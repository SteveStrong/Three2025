using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryBlazor.Shape;
using Three2025.Model;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Enums;
using FoundryRulesAndUnits.Extensions;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using FoundryBlazor.PubSub;
using FoundryRulesAndUnits.Models;
using BlazorThreeJS.Core;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;



namespace Three2025.Components.Pages;

public partial class ClockBase : ComponentBase, IDisposable
{
    public Canvas3DComponentBase Canvas3DReference = null;

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }


    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;


    protected MockDataGenerator DataGenerator { get; set; } = new();
    private CableWorld World3D { get; set; } = null!;

    private Timer _timer = null!;
    private Text3D GlobalText = null!;

    private Mesh3D CenterPost = null!;

    public (bool, Scene3D) GetCurrentScene()
    {
        var arena = Workspace.GetArena();
        return arena.CurrentScene();
    }
 

    protected override void OnInitialized()
    {
        Workspace.SetBaseUrl(Navigation?.BaseUri ?? "");
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false,null!);

            scene?.SetAfterUpdateAction((s,j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            World3D = FoundryService.WorldManager().CreateWorld<CableWorld>("Clocks");

            var arena = Workspace.GetArena();
            arena.EstablishStage<FoStage3D>("Main Stage");
            if (found)
                arena.SetScene(scene!);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public string GetReferenceTo(string filename)
    {
        var path = string.Intern(Path.Combine(Navigation.BaseUri, filename));
        path.WriteSuccess();
        return path;
    }
    
    


    private void UpdateTextWithCurrentTime(object state)
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var time = DateTime.Now;
        var angle = time.Second * (2 * Math.PI / 60) - Math.PI / 2; // Convert seconds to radians
        var radius = 10.0;
        var x = radius * Math.Cos(angle);
        var y = 2;
        var z = radius * Math.Sin(angle);

        var currentTime = time.ToString("HH:mm:ss");
        //var x = DataGenerator.GenerateDouble(-1, 1);
        //var z = DataGenerator.GenerateDouble(-1, 1);

        if (GlobalText != null)
        {
            GlobalText.Text = currentTime;
            GlobalText.Transform.Position = new Vector3(x, y, z);
            GlobalText.SetDirty(true);

            //SecondHand.Transform.Rotation = new Euler(0, -angle, 0);
            //SecondHand.SetDirty(true);
            CenterPost.Transform.Rotation = new Euler(0, -angle, 0);
            CenterPost.SetDirty(true);
        }
        else 
        {
            GlobalText = new Text3D()
            {
                Uuid = Guid.NewGuid().ToString(),
                Text = currentTime,
                Color = DataGenerator.GenerateColor(),
                FontSize = 3.0,
                Transform = new Transform3D()
                {
                    Position = new Vector3(x, y, z),
                },
            };
            CenterPost = new Mesh3D
            {
                Uuid = Guid.NewGuid().ToString(),
                Name = "CenterPost",
                Geometry = new BoxGeometry(width: 0.5, depth: 0.5, height: 2.5),
                Transform = new Transform3D()
                {
                    Position = new Vector3(0, 0, 0),
                    Rotation = new Euler(0, -angle, 0),
                },
                Material = new MeshStandardMaterial("red")
            };
            var secondHand = new Mesh3D
            {
                Uuid = Guid.NewGuid().ToString(),
                Name = "Second Hand",
                Geometry = new BoxGeometry(width: 1.2 * radius, depth: 0.1, height: 2),
                Transform = new Transform3D()
                {
                    Position = new Vector3(0.5 * radius, 1, 0),
                    Rotation = new Euler(0, 0, 0),
                },
                Material = new MeshStandardMaterial("green")
            };
            scene.AddChild(GlobalText);
            scene.AddChild(CenterPost);
            CenterPost.AddChild(secondHand);
        }

    }

    public void PlaceTextAtPosition(Mesh3D parent, double angle, double radius, double height, double size,  string text)
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = radius * Math.Cos(angle);
        var y = height;
        var z = radius * Math.Sin(angle);

        var letter = new Text3D()
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = text,
            Text = text,
            Color = "white",
            FontSize = size,
            Transform = new Transform3D()
            {
                Position = new Vector3(x, y, z),
            },
        };

        scene.AddChild(letter);     

    }
 

    public void DoLabelAutoRefresh()
    {
        if (_timer == null)
        {
            _timer = new Timer(UpdateTextWithCurrentTime, null, 0, 1000);
        }
        else
        {
            _timer?.Dispose();
            _timer = null;
        }
    }


    public void DoClockFace()
    {
        var arena = Workspace.GetArena();
        var (found, scene) = arena.CurrentScene();
        if ( !found) return;

        var radius = 10.0f;
        var height = 0.1f;

        var mesh = new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = "Clock Face",
            Geometry = new CylinderGeometry(radiusTop: radius-1.0, radiusBottom: radius, height: height,  radialSegments: 36),
            Transform = new Transform3D()
            {
                Position = new Vector3(0, 0, 0),
            },
            Material = new MeshStandardMaterial("blue")
        };

        scene.AddChild(mesh);

        for (int i = 1; i <= 12; i++)
        {
            var letter = $"{i}";
            var angle = i * (2 * Math.PI / 12) - Math.PI / 2;

            PlaceTextAtPosition(mesh, angle, radius-1.0, height + 1.0, 1.2, letter);
        }

    }




    public async Task DoRequestAxisToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var Uuid = Guid.NewGuid().ToString();

        var spec = new ImportSettings
        {
            Uuid = Uuid,
            Format = Model3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
        };

        await scene.Request3DModel(spec, async (uuid) => {
            var group = new Group3D()
            {
                Name = "Axis",
                Uuid = uuid,
            };
            scene.AddChild(group);
            $"Axis added to scene in callback".WriteSuccess();
            StateHasChanged();
            await Task.CompletedTask;
        });
    }




    public void DoRequestAddTextToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var Uuid = Guid.NewGuid().ToString();
        var text = DataGenerator.GenerateText();
        var color = DataGenerator.GenerateColor();

        var text3d = new Text3D()
        {
            Uuid = Uuid,
            Text = text,
            Color = color,
            FontSize = 1.0,
            Transform = new Transform3D()
            {
                Position = new Vector3(x, y, z),
            },
        };
        scene.AddChild(text3d);

        // var spec = new ImportSettings
        // {
        //     Uuid = Uuid,
        //     Format = Model3DFormats.Text,
        // };
        // spec.AddChild(text3d);
        // Task.Run(async () => await scene.Request3DLabel(spec, async (uuid) => {
        //     StateHasChanged();
        //     await Task.CompletedTask;
        // }));
    }

    public void DoRequestAddBoxGLBToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var url = GetReferenceTo(@"storage/staticfiles/BoxAnimated.glb");

        var spec = new ImportSettings
        {
            Uuid = Guid.NewGuid().ToString(),
            Format = Model3DFormats.Gltf,
            FileURL = url,
            Transform = new Transform3D()
            {
                Position = new Vector3(x, y, z),
            },

        };
        Task.Run(async () => await scene.Request3DModel(spec, async (uuid) => {
            var group = new Group3D()
            {
                Name = $"BOX:{DataGenerator.GenerateWord()}",
                Uuid = uuid,
            };
            scene.AddChild(group);
            StateHasChanged();
            await Task.CompletedTask;
        }));
    }

   public void DoRequestConeToScene()
    {
        var arena = Workspace.GetArena();
        var (found, scene) = arena.CurrentScene();

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var rx = DataGenerator.GenerateDouble(0, 2 * Math.PI);
        var ry = DataGenerator.GenerateDouble(0, 2 * Math.PI);
        var rz = DataGenerator.GenerateDouble(0, 2 * Math.PI);

        var s = DataGenerator.GenerateDouble(0.1, 5);

        //var Uuid = Guid.NewGuid().ToString();
        //var text = DataGenerator.GenerateText();
        var color = DataGenerator.GenerateColor();


        var mesh = new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = DataGenerator.GenerateWord(),
            Geometry = new ConeGeometry(radius: 0.5f, height: 2, radialSegments: 16),
            Transform = new Transform3D()
            {
                Position = new Vector3(x, y, z),
                Rotation = new Euler(rx, ry, rz),
                Scale = new Vector3(s, s, s)
            },
            Material = new MeshStandardMaterial()
            {
                Color = color,
                FlatShading = true,
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        };

        scene.AddChild(mesh);

        // var spec = new ImportSettings
        // {
        //     Uuid = Uuid,
        //     Format = Model3DFormats.Mesh,
        // };

        // Task.Run(async () => await scene.Request3DGeometry(spec, async (uuid) => {

        //     scene.AddChild(mesh);
        //     StateHasChanged();
        //     await Task.CompletedTask;
        // }));
    }

    public void DoRequestAddJetToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var Uuid = Guid.NewGuid().ToString();
        var url = GetReferenceTo(@"storage/StaticFiles/jet.glb");

        var spec = new ImportSettings
        {
            Uuid = Uuid,
            Format = Model3DFormats.Gltf,
            FileURL = url,
            Transform = new Transform3D()
            {
                Position = new Vector3(x, y, z),
            },
        };
        Task.Run(async () => await scene.Request3DModel(spec, async (uuid) => {
            var group = new Group3D()
            {
                Name = $"JET:{DataGenerator.GenerateWord()}",
                Uuid = uuid,
            };
            scene.AddChild(group);
            StateHasChanged();
            await Task.CompletedTask;
        }));
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _timer = null;
    }
}


