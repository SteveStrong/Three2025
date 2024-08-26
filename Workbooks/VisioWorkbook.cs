using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;

using FoundryBlazor.Shared;


using FoundryRulesAndUnits.Units;



namespace Three2025.Model;


public class VisioWorkbook : FoWorkbook
{

    //private ModelConstructTool Tool { get; set; }
    private IDrawing Drawing { get; set; }
    private IArena Arena { get; set; }


    public VisioWorkbook(IWorkspace space, IFoundryService foundry) :
        base(space, foundry)
    {

        Drawing = space.GetDrawing()!;
        Drawing.ToggleHitTestDisplay();

        //Tool = Drawing.AddToolType<ModelConstructTool> (110, "crosshair");
        Drawing.AddKeyHooks(ProcessKeyDown,null,null);

        Arena = space.GetArena();

        //var page = Drawing.CurrentPage();
        
        var drawing = new Length(10.0, "cm");
        var world = new Length(1.0, "m");
        EstablishCurrentPage<VisioPage>("Front View", "#BBE9FF").ResetScale(drawing,world).SetPageSize(100, 60, "cm");
    

        //Drawing.Pages().RemovePage(page);
        var page = Drawing.CurrentPage();
        $"Current Page {page.GetName()}".WriteSuccess();

        var pages = Drawing.Pages().GetAllPages();
        pages.ForEach(p => $"PageList {p.GetName()}".WriteSuccess());
    }




    
    public bool ProcessKeyDown(CanvasKeyboardEventArgs args)
    {

        //$"ProcessKeyDown ShiftKey?: {args.ShiftKey}, AltKey?: {args.AltKey}, CtrlKey?: {args.CtrlKey}, Key={args.Key} Code={args.Code}".WriteWarning();
        object success = (args.Code, args.AltKey, args.CtrlKey, args.ShiftKey) switch
        {
            // ("KeyC", true, false, false) => DoClear(),
            // ("KeyP", false, false, false) => DoPrint(),
            // ("KeyS", false, false, false) => DoSaveModel(),
            // ("KeyR", false, false, false) => DoRestoreModel(),
            // ("KeyS", false, false, true) => DoSaveDrawing(),
            // ("KeyR", false, false, true) => DoRestoreDrawing(),

            ("Delete", false, false, false) => DoDeleteSelections(),
            ("Delete", false, true, false) => DoDeleteSelections(true),
            // ("KeyD", false, true, false) => DoDuplicateSelections(),
            // ("Insert", false, false, false) => DoDuplicateSelections(),
            _ => false
        };
        return success != null;
    }

    public bool DoDeleteSelections(bool withAnimations = false)
    {

        Drawing.DeleteSelections(withAnimations);
        return true;
    }



    private void CreateMenu2D(FoMenu2D menu, int x, int y)
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null || menu == null) return;

        menu.ToggleLayout();
        drawing.AddShape<FoMenu2D>(menu).AnimatedMoveTo(x, y);
    }

    public override void CreateMenus(IWorkspace space, IJSRuntime js, NavigationManager nav)
    {
        //"InstanceWorkbook CreateMenus TargetManager".WriteWarning();
        var tmenu = new Dictionary<string, Action>()
        {
            { "Clear", () => DoClear() },
            { "Save Model", () => DoNothing() },
            { "Restore Model", () => DoNothing() },
            { "Save Drawing", () => DoNothing() },
            { "Restore Drawing", () => DoNothing() },
        };


        var targetMenu = space.EstablishMenu2D<FoMenu2D, FoButton2D>("Models", tmenu, true);
        targetMenu!.AddButton("Menu", () =>CreateMenu2D(targetMenu, 900, 100));
        CreateMenu2D(targetMenu, 900, 100);

        var cMenu = new Dictionary<string, Action>()
        {
            { "Equipment Shapes", () => DoNothing() },
            { "Scaled Equipment", () => DoNothing() },
            { "BackPanel Test", () => DoNothing() },
        };

        var creationMenu = space.EstablishMenu2D<FoMenu2D, FoButton2D>("Equipment", cMenu, true);
        creationMenu!.AddButton("Menu", () =>CreateMenu2D(creationMenu, 100, 100));
        CreateMenu2D(creationMenu, 100, 100);
    }

    private void DoNothing() {}


    private void DoClear()
    {
        var drawing = Workspace.GetDrawing();
        if (drawing == null) return;
        drawing.ClearAll();
        //CreateMenu2D();
    }


}
