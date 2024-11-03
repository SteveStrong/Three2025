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



namespace Three2025.Components.Pages;

public partial class HomeBase : ComponentBase, IDisposable
{

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] private IToast Toast { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }


    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;
    [Parameter] public int WorldWidth { get; set; } = 1500;
    [Parameter] public int WorldHeight { get; set; } = 1500;
    [Parameter] public string LoadWorkbook { get; set; } = "knowledge";

    public FoWorkbook Workbook { get; set; }
    public Scene scene  { get; set; }


    public ViewerSettings settings = new ViewerSettings()
    {
        containerId = "example1",
        CanSelect = true,// default is false
        SelectedColor = "#808080",
        Width = 900,
        Height = 600,
        WebGLRendererSettings = new WebGLRendererSettings
        {
            Antialias = false // if you need poor quality for some reasons
        }
    };

 


    protected override void OnInitialized()
    {
        Workspace.SetBaseUrl(Navigation?.BaseUri ?? "");

        //create all worksbooks for reuse later, but only show the one we want
        Workbook = Workspace.EstablishWorkbook<VisioWorkbook>("visio");
        //Workbook.SetMentorService(MentorServices!, MentorPlayground!);
        //book.SetVaultService(VaultService!);

        Workspace.SetCurrentWorkbook(Workbook);
        // Workspace.CreateMenus(Workspace, JsRuntime!, Navigation!);

        //Diagram = MentorServices!.EstablishDiagram();
        //RefreshWorkbookMenus();



        var url = ""; //RestAPI?.GetServerUrl() ?? "";
        Workspace.CreateCommands(Workspace, JsRuntime, Navigation, url);
        var arena = Workspace.GetArena();
        scene = arena.CurrentScene();

        base.OnInitialized();
    }

//    protected override Task OnInitializedAsync()
//     {        
//         objGuid = Guid.NewGuid();
//         scene = new(jsRuntime!);
//         scene.Add(new AmbientLight());
//         scene.Add(new PointLight()
//         {
//             Position = new Vector3(1, 3, 0)
//         });
//          return base.OnInitializedAsync();
//     }

    protected override async Task OnInitializedAsync()
    {
        if (Workspace is not null)
        {
            var defaultHubURI = Navigation!.ToAbsoluteUri("/DrawingSyncHub").ToString();
            await Workspace.InitializedAsync(defaultHubURI!);

            var text = LoadWorkbook == null ? "No Workbook" : $"LoadWorkbook={LoadWorkbook}";
            Toast.Info(text);
        }

        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //PubSub!.SubscribeTo<ViewStyle>(OnViewStyleChanged);
            //Toast!.Success($"Drawing Page Loaded!");
            //Workspace.GetDrawing();
            //Workspace.GetArena();


        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected void Go()
    {
        //"Click Go".WriteInfo();
        //GoDrawing();
        GoCables();
        //CreateAndRenderBox();
    }
    
    protected void GoCables()
    {
        var cables = new CableChannels(Workspace,FoundryService);
        cables.GenerateGeometry();

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
            Task.Run(async () =>
            {
                await arena.UpdateArena();
            });

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
            //FileURL = "https://localhost:7101/storage/StaticFiles/porsche_911.glb",
            FileURL = "https://localhost:7101/storage/StaticFiles/T_Rex.glb",
            Position = new Vector3(2, 0, 2),
        };



        await scene.Request3DModel(model);
        await scene.UpdateScene();
    }

    public async Task OnAddJET()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = Import3DFormats.Gltf,
            FileURL = @"https://localhost:7101/jet.glb",
            Position = new Vector3(0, 0, 0),
        };


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
        GC.SuppressFinalize(this);
    }
}

