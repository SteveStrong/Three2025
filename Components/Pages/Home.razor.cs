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

public partial class HomeBase : ComponentBase, IDisposable
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

            World3D = FoundryService.WorldManager().CreateWorld<CableWorld>("Cables");

            var arena = Workspace.GetArena();
            arena.EstablishStage<FoStage3D>("Main Stage");
            if (found)
                arena.SetScene(scene);

            CreateMenus(Workspace);
            CreateServices(FoundryService, arena, World3D);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }
    



    
    public void CreateMenus(IWorkspace space)
    {
        var arena = space.GetArena();
        var stage = arena.CurrentStage();

        // arena.AddAction("Update", "btn-primary", () =>
        // {
        // });

        // arena.AddAction("Clear", "btn-primary", () =>
        // {
        // });

        // stage.AddAction("Clear", "btn-primary", () =>
        // {
        //  });

        // stage.AddAction("Render", "btn-primary", () =>
        // {
        // });
    }

    public void CreateServices(IFoundryService manager, IArena arena, FoWorld3D world)
    {

        world.AddAction("Clear", "btn-primary", () => 
        {
            world.ClearAll();
        });

        world.AddAction("Publish", "btn-info", () => 
        {
            world.PublishToStage(arena.CurrentStage());
        });

        world.AddAction("Box", "btn-info", () => 
        {

            var box = AddBox(DataGenerator.GenerateName());
            World3D.AddGlyph3D<FoShape3D>(box);
            arena.AddShape<FoShape3D>(box);
        });



        

    }


    public void OnAddCage()
    {
        var cables = new CableChannels(World3D);
        cables.GenerateGeometry();

        var arena = Workspace.GetArena() as FoArena3D;
        var stage = arena.EstablishStage<FoStage3D>("The Cage");
        World3D.PublishToArena(arena);

        var (found, scene) = GetCurrentScene();
        if (found)
            stage.RefreshScene(scene);
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
            stage.RefreshScene(scene);
    }


    public void DoAddTubeToScene()
    {
        var arena = Workspace.GetArena();
        var (found, scene) = arena.CurrentScene();


        var capsuleRadius = 0.15f;
        var capsulePositions = new List<Vector3>() {
            new Vector3(0, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(4, 4, 0),
            new Vector3(4, 4, -4)
        };

        if (found)
            scene.AddChild(new Mesh3D
            {
                Uuid = Guid.NewGuid().ToString(),
                Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: capsuleRadius, path: capsulePositions),
                Material = new MeshStandardMaterial()
                {
                    Color = "yellow"
                }
            });

    }

    public void DoAddConeToScene()
    {
        var arena = Workspace.GetArena();
        var (found, scene) = arena.CurrentScene();

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var Uuid = Guid.NewGuid().ToString();
        var text = DataGenerator.GenerateText();
        var color = DataGenerator.GenerateColor();


        var mesh = new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = DataGenerator.GenerateWord(),
            Geometry = new ConeGeometry(radius: 0.5f, height: 2, radialSegments: 16),
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
                Rotation = new Euler(0, 0, 0),
                Scale = new Vector3(1, 1, 1)
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
                Transform = new Transform3()
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
    }




    public void DoAddTRexToArena()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var arena = Workspace.GetArena();
        var shape = new FoModel3D("T-Rex " + name)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
            },
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
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
            },
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
            BoundingBox = new Vector3(bx, by, bz),
            Transform = new Transform3()
            {
                 Scale = new Vector3(scale, scale, scale),
            }
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
            stage.RefreshScene(scene);

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
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            }
        };

        arena.AddShape<FoText3D>(shape);

        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        var (found, scene) = GetCurrentScene();
        if (found)
            stage.RefreshScene(scene);
    }


    public void DoAddTRISOCToArena()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        //var arena = Workspace.GetArena();
        var shape = new FoModel3D("TRISOC " + name)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
                Scale = new Vector3(30, 30, 30),
            }
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
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            }
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
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            }
        };


        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        arena.AddShape<FoText3D>(shape);

        var (found, scene) = GetCurrentScene();
        if (found)
            stage.RefreshScene(scene);
    }

    public Node3D AddBox(string name, double x=0, double z=0)
    {
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var height = DataGenerator.GenerateDouble(1, 10);
        var box = new Node3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
                Pivot = new Vector3(0, height/2, 0)
            }

        };
        box.CreateBox(label, .5, height, .5);

        return box;
    }

    public Node3D AddCone(string name, double x=0, double z=0)
    {
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var height = DataGenerator.GenerateDouble(1, 10);
        var box = new Node3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
                Pivot = new Vector3(0, height/2, 0)
            }

        };
        box.CreateCone(label, .75, height, .75);

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

        stage.RefreshScene(scene);
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
    }





   public async Task DoAddAxisToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;


        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };


        await scene.Request3DModel(model, async (uuid) => {

            scene.AddChild(model);
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


        var text3d = new Text3D()
        {
            Uuid = Guid.NewGuid().ToString(),
            Text = DataGenerator.GenerateText(),
            Color = DataGenerator.GenerateColor(),
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            },
        };
        scene.AddChild(text3d);
    }


    public async Task DoRequestAddJetToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var model = new Model3D()
        {
            Name = $"JET:{DataGenerator.GenerateWord()}",
            Uuid = Guid.NewGuid().ToString(),
            Url =  GetReferenceTo(@"storage/StaticFiles/jet.glb"),
            Format = Model3DFormats.Gltf,
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            },
        };


        await scene.Request3DModel(model, async (uuid) => {
            scene.AddChild(model);
            StateHasChanged();
            await Task.CompletedTask;
        });
    }


    public void Dispose()
    {

    }
}

