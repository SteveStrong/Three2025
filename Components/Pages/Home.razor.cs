using BlazorComponentBus;
using FoundryBlazor.Shared;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;
using FoundryBlazor.Shape;
using Three2025.Model;


namespace Three2025.Components.Pages;

public partial class HomeBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] private IToast Toast { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }

    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;
    [Parameter] public int WorldWidth { get; set; } = 1500;
    [Parameter] public int WorldHeight { get; set; } = 1500;
    [Parameter] public string LoadWorkbook { get; set; } = "knowledge";

    public FoWorkbook Workbook { get; set; }


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

        var page = Workbook.CurrentPage();
        $"new current page {page.Title}".WriteInfo();

        var url = ""; //RestAPI?.GetServerUrl() ?? "";
        Workspace.CreateCommands(Workspace, JsRuntime, Navigation, url);

        base.OnInitialized();
    }


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
            Workspace.GetDrawing();
            Workspace.GetArena();

            // await Task.Run(async () =>
            // {
            //     await Task.Delay(5000);
            //     Go();
            // });
        }

        await base.OnAfterRenderAsync(firstRender);
    }


    protected void Go()
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

