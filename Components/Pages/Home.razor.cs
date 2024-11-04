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



namespace Three2025.Components.Pages;

public partial class HomeBase : ComponentBase, IDisposable
{

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }

    public Canvas3DComponentBase CanvasReference = null!;

    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;
    [Parameter] public int WorldWidth { get; set; } = 1500;
    [Parameter] public int WorldHeight { get; set; } = 1500;

    private CableWorld World3D { get; set; } = null!;

    public Scene GetCurrentScene()
    {
        return CanvasReference.GetActiveScene();
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
            World3D = FoundryService.WorldManager().CreateWorld<CableWorld>("Cables");

            var arena = Workspace.GetArena();
            arena.SetScene(GetCurrentScene());

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

    public FoShape3D DoLoad3dModel(string url, double bx, double by, double bz)
    {
        var name = url.Split('\\').Last();
        var shape = new FoShape3D(name,"blue")
        {
            Name = name,
            GlyphId = Guid.NewGuid().ToString(),
            Position = new Vector3(0, 0, 0),
            BoundingBox = new Vector3(bx, by, bz),
            //Scale = new Vector3(.1, .1, .1)
        };
        shape.CreateGlb(url);
        World3D.AddGlyph3D<FoShape3D>(shape);
        return shape;
    }

    public void ForceSceneRefresh()
    {
        Task.Run(async () =>
        {
            await GetCurrentScene().UpdateScene();
        });
    }


    public void CreateMenus(IWorkspace space)
    {
        var arena = space.GetArena();
        var stage = arena.CurrentStage();

        arena.AddAction("Update", "btn-primary", () =>
        {
            Task.Run(async () => await arena.UpdateArena());
        });

        arena.AddAction("Clear", "btn-primary", () =>
        {
            Task.Run(async () => await arena.ClearArena());
        });

        stage.AddAction("Render", "btn-primary", () =>
        {
            stage.PreRender(arena);
            Task.Run(async () => await stage.RenderDetailed(arena.CurrentScene(), 0, 0));
        });
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
            var box = AddBox();
            arena.AddShape<FoShape3D>(box);
        });

        world.AddAction("TRex", "btn-primary", () =>
        {
            var url = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb");
            var shape = DoLoad3dModel(url, -2, 6, -2);
            arena.AddShape<FoShape3D>(shape);
        });

        world.AddAction("Porsche", "btn-primary", () =>
        {
            var url = GetReferenceTo(@"storage/StaticFiles/porsche_911.glb");
            var shape = DoLoad3dModel(url, 2, 6, 2);
            arena.AddShape<FoShape3D>(shape);
        });
        
        world.AddAction("Render Tube", "btn-primary", () =>
        {
            var scene = arena.CurrentScene();


            var capsuleRadius = 0.15f;
            var capsulePositions = new List<Vector3>() {
                new Vector3(0, 0, 0),
                new Vector3(4, 0, 0),
                new Vector3(4, 4, 0),
                new Vector3(4, 4, -4)
            };

            scene.Add(new Mesh
            {
                Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: capsuleRadius, path: capsulePositions),
                Position = new Vector3(0, 0, 0),
                Material = new MeshStandardMaterial()
                {
                    Color = "yellow"
                }
            });

            ForceSceneRefresh();
        });

        world.AddAction("Draw Box", "btn-primary", () =>
        {
            var height = 4;

            var piv = new Vector3(-1, -height / 2, -3);
            var pos = new Vector3(0, height, 0);
            var rot = new Euler(0, Math.PI * 45 / 180, 0);

            var scene = arena.CurrentScene();
            scene.Add(new Mesh
            {
                Geometry = new BoxGeometry(width: 2, height: height, depth: 6),
                Position = pos,
                Rotation = rot,
                Pivot = piv,
                Material = new MeshStandardMaterial()
                {
                    Color = "magenta"
                }
            });


            ForceSceneRefresh();
        });
    }





    public async Task OnAddCage()
    {
        var cables = new CableChannels(World3D);
        cables.GenerateGeometry();

        var scene = GetCurrentScene();
        await scene.UpdateScene();
    }

    public Node3D AddBox()
    {
        var box = new Node3D("test","blue")
        {
            GlyphId = Guid.NewGuid().ToString(),
            Position = new Vector3(0, 0, 0),
        };
        box.CreateBox("test", .5, 10, .5);
        
        World3D.AddGlyph3D<FoShape3D>(box);
        return box;
    }

    protected void CreateAndRenderBox(bool render = true)
    {
        var arena = Workspace.GetArena();

        var shape = new FoShape3D()
        {
            Color = "Red"
        };
        shape.CreateBox("Dave",1, 2, 3);

        //this will render
        //var world = new FoWorld3D("Test");
        //world.AddGlyph3D<FoShape3D>(shape);
        //arena.RenderWorld3D(world);

        //this should render
        arena.AddShape<FoShape3D>(shape);
        if ( render )
            ForceSceneRefresh();

    }
    protected void GoDrawing()
    {
        //"Click Go".WriteInfo();

        var drawing = Workspace.GetDrawing();
        var page = drawing?.CurrentPage();
        //$"Current Page {page?.Title}".WriteSuccess();

        var shape = new FoShape2D()
        {
            Name = "Rectangle",
            PinX = 100,
            PinY = 100,
            Width = 100,
            Height = 100,
            Color = "Red"
        };
        page?.AddShape<FoShape2D>(shape);

        var shape2 = new FoShape1D(200,100,800,500,10,"Green");
        page?.AddShape<FoShape1D>(shape2);


        var shape3 = new FoConnector1D(300, 100, 600, 200, "Blue")
        {
            Layout = LineLayoutStyle.HorizontalFirst,
        };
        page?.AddShape<FoConnector1D>(shape3);
    }



    public async Task OnAddTRex()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"),
            Position = new Vector3(3, 0, 3),
        };

        var scene = GetCurrentScene();

        await scene.Request3DModel(model);
        await scene.UpdateScene();
    }

    public async Task OnAddJet()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/jet.glb"),
            Position = new Vector3(0, 0, 0),
        };

        var scene = GetCurrentScene();

        await scene.Request3DModel(model);
        await scene.UpdateScene();
    }

    public List<FoPage2D> AllPages()
    {
        var drawing = Workspace.GetDrawing()!;
        var manager = drawing.Pages();
        return manager.GetAllPages();
    }

    
    public bool GoToPage(FoPage2D page)
    {
        var drawing = Workspace.GetDrawing()!;
        drawing.SetCurrentPage(page);
        return true;
    }

    public void Dispose()
    {
        // if (Navigation != null)
        //     Navigation.LocationChanged -= LocationChanged;
        //GC.SuppressFinalize(this);
    }
}

