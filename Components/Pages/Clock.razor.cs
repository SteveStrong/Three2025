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
                arena.SetScene(scene);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }
    
    
    public void DoAddTriSocGeometry()
    {

        var shape = DoLoad3dModelToWorld(GetReferenceTo(@"storage/StaticFiles/TRISOC.glb"), 0, 0, 0, 30);

        var trisoc = new TriSocGeometry(World3D);
        //trisoc.GenerateLabels();
        //trisoc.GenerateMarkers();

        var arena = Workspace.GetArena() as FoArena3D;
        var stage = arena.EstablishStage<FoStage3D>("TriSoc");
        World3D.PublishToArena(arena);

        var (found, scene) = GetCurrentScene();
        if (found)
            stage.RenderToScene(scene);
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

        var Uuid = Guid.NewGuid().ToString();
        var text = DataGenerator.GenerateText();
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

        var spec = new ImportSettings
        {
            Uuid = Uuid,
            Format = Import3DFormats.Mesh,
            OnComplete = () =>
            {
                scene.AddChild(mesh);
                StateHasChanged();
            }
        };
        spec.AddChild(mesh);

        Task.Run(async () => await scene.Request3DGeometry(spec));
    }

    public void DoAddBoxToScene()
    {
        var arena = Workspace.GetArena();
        var (found, scene) = arena.CurrentScene();
        var height = 4;

        var piv = new Vector3(-1, -height / 2, -3);
        var pos = new Vector3(0, height, 0);
        var rot = new Euler(0, Math.PI * 45 / 180, 0);


        if ( found)
            scene.AddChild(new Mesh3D
            {
                Uuid = Guid.NewGuid().ToString(),
                Geometry = new BoxGeometry(width: 2, height: height, depth: 6),
                Transform = new Transform3D()
                {
                    Position = pos,
                    Rotation = rot,
                    Pivot = piv,
                },
                Material = new MeshStandardMaterial()
                {
                    Color = "magenta"
                }
            });

        scene?.ForceSceneRefresh();
    }




    public void DoAddTRexToArena()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var arena = Workspace.GetArena();
        var shape = new FoModel3D("T-Rex " + name)
        {
            Position = new Vector3(x, 0, z),
        };
        shape.CreateGlb(GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"));

        LoadIntoArena(shape);
    }

    public void DoAddPorscheToArena()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);


        var shape = new FoModel3D("Porsche " + name)
        {
            Position = new Vector3(x, 0, z),
        };
        shape.CreateGlb(GetReferenceTo(@"storage/StaticFiles/Porsche_911.glb"));

        LoadIntoArena(shape);
    }

    // world.AddAction("Porsche", "btn-primary", () =>
    // {
    //     var url = GetReferenceTo(@"storage/StaticFiles/porsche_911.glb");
    //     var shape = DoLoad3dModel(url, 2, 6, 2);
    //     arena.AddShape<FoShape3D>(shape);
    // });
        
    public FoShape3D DoLoad3dModelToWorld(string url, double bx, double by, double bz, double scale = 1)
    {
        var name = url.Split('\\').Last();
        var shape = new FoModel3D(name, "blue")
        {
            Name = name,
            GlyphId = Guid.NewGuid().ToString(),
            Position = new Vector3(0, 0, 0),
            BoundingBox = new Vector3(bx, by, bz),
            Scale = new Vector3(scale, scale, scale),
        };
        shape.CreateGlb(url);
        World3D.AddGlyph3D<FoShape3D>(shape);

        return LoadIntoArena(shape);
    }

    private FoShape3D LoadIntoArena(FoShape3D shape)
    {
        var arena = Workspace.GetArena();
        arena.AddShape<FoShape3D>(shape);  //this is what the world publish is doing

        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        stage.PreRender(arena);

        var (found, scene) = GetCurrentScene();
        if (found)
            stage.RenderToScene(scene);

        return shape;
    }


    public void DoAddTextArena()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);
        var color = DataGenerator.GenerateColor();
        var arena = Workspace.GetArena();

        var shape = new FoText3D(name,color)
        {
            Text = DataGenerator.GenerateText(),
            Position = new Vector3(x, y, z),
        };

        arena.AddShape<FoText3D>(shape);

        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        var (found, scene) = GetCurrentScene();
        if (found)
            stage.RenderToScene(scene);
    }


    public void DoAddTRISOCToArena()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        //var arena = Workspace.GetArena();
        var shape = new FoModel3D("TRISOC " + name)
        {
            Position = new Vector3(x, 0, z),
            Scale = new Vector3(30, 30, 30),
        };
        shape.CreateGlb(GetReferenceTo(@"storage/StaticFiles/TRISOC.glb"));

        LoadIntoArena(shape);
    }
    
    public void DoAddGeomToStage()
    {
        var name = DataGenerator.GenerateName();
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var shape = new FoShape3D(name,color)
        {
            Position = new Vector3(x, y, z),
        };

        var w = DataGenerator.GenerateDouble(1, 10);
        var h = DataGenerator.GenerateDouble(1, 10);
        var d = DataGenerator.GenerateDouble(1, 10);
        var index = DataGenerator.GenerateInt(0, 10);

        shape = index switch
        {
            0 => shape.CreateBox(label, w, h, d),
            1 => shape.CreateCone(label, w, h, d),
            2 => shape.CreateCylinder(label, w, h, d),
            3 => shape.CreateDodecahedron(label, w, h, d),
            4 => shape.CreateIcosahedron(label, w, h, d),
            5 => shape.CreateOctahedron(label, w, h, d),
            6 => shape.CreateSphere(label, w, h, d),
            7 => shape.CreateTetrahedron(label, w, h, d),
            8 => shape.CreateTorusKnot(label, w, h, d),
            9 => shape.CreateTorus(label, w, h, d),
            _ => shape.CreateBox(label, w, h, d),
        };
 
        LoadIntoArena(shape);
    }

    public void OnAddText()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);
        var color = DataGenerator.GenerateColor();
        var arena = Workspace.GetArena();

        var shape = new FoText3D(name,color)
        {
            Text = DataGenerator.GenerateText(),
            Position = new Vector3(x, y, z),
        };


        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        arena.AddShape<FoText3D>(shape);

        var (found, scene) = GetCurrentScene();
        if (found)
            stage.RenderToScene(scene);
    }

    public Node3D AddBox(string name, double x=0, double z=0)
    {
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var box = new Node3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Position = new Vector3(x, 0, z),

        };
        var height = DataGenerator.GenerateDouble(1, 10);
        box.CreateBox(label, .5, height, .5);
        box.Pivot = new Vector3(0, height/2, 0);
        return box;
    }

    public Node3D AddCone(string name, double x=0, double z=0)
    {
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var box = new Node3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Position = new Vector3(x, 0, z),

        };
        var height = DataGenerator.GenerateDouble(1, 10);
        box.CreateCone(label, .75, height, .75);
        box.Pivot = new Vector3(0, height/2, 0);
        return box;
    }

    public void AddBoxToStage()
    {
        var name = DataGenerator.GenerateName();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

        var box = AddBox(name,x,z);
        stage.AddShape<Node3D>(box);
        
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        stage.RenderToScene(scene);
        // await scene.SetCameraPosition(new Vector3(15f, 15f, 15f),box.Position);
        // await scene.UpdateScene();
    }

    public void AddConeToArena()
    {
        var name = DataGenerator.GenerateName();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var box = AddCone(name,x,z);
        var arena = Workspace.GetArena();

        arena.EstablishStage<FoStage3D>("Main Stage");
        arena.AddShape<Node3D>(box);
        
        arena.UpdateArena();
    }





    public async Task DoRequestAxisToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var Uuid = Guid.NewGuid().ToString();

        var settings = new ImportSettings
        {
            Uuid = Uuid,
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            OnComplete = () =>
            {
                var group = new Group3D()
                {
                    Name = "Axis",
                    Uuid = Uuid,
                };
                scene.AddChild(group);
                $"DoRequestAxisToScene: OnComplete {group.Uuid}".WriteInfo();

                StateHasChanged();
            }
        };
        await scene.Request3DModel(settings);
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

        var spec = new ImportSettings
        {
            Uuid = Uuid,
            Format = Import3DFormats.Text,
            OnComplete = () =>
            {
                scene.AddChild(text3d);
                StateHasChanged();
            }
        };
        spec.AddChild(text3d);
        Task.Run(async () => await scene.Request3DLabel(spec));
        //scene?.ForceSceneRefresh();
    }

    public void DoRequestAddBoxGLBToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var Uuid = Guid.NewGuid().ToString();
        var url = GetReferenceTo(@"storage/staticfiles/BoxAnimated.glb");

        var spec = new ImportSettings
        {
            Uuid = Uuid,
            Format = Import3DFormats.Gltf,
            FileURL = url,
            Transform = new Transform3D()
            {
                Position = new Vector3(x, y, z),
            },
            OnComplete = () =>
            {
                var group = new Group3D()
                {
                    Name = $"BOX:{DataGenerator.GenerateWord()}",
                    Uuid = Uuid,
                };
                scene.AddChild(group);
                StateHasChanged();
            }

        };
        Task.Run(async () => await scene.Request3DModel(spec));
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
            Format = Import3DFormats.Gltf,
            FileURL = url,
            Transform = new Transform3D()
            {
                Position = new Vector3(x, y, z),
            },
            OnComplete = () =>
            {
                var group = new Group3D()
                {
                    Name = $"JET:{DataGenerator.GenerateWord()}",
                    Uuid = Uuid,
                };
                scene.AddChild(group);
                StateHasChanged();
            }

        };
        Task.Run(async () => await scene.Request3DModel(spec));
    }


    public void Dispose()
    {

    }
}

