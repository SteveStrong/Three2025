using BlazorComponentBus;
using BlazorThreeJS.Events;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;

using FoundryBlazor.PubSub;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
using System.Reflection.Metadata.Ecma335;

namespace Three2025.Shared;
#nullable enable

public class Canvas3DBase : ComponentBase, IDisposable
{

    [Inject] public IWorkspace? Workspace { get; set; }
    [Inject] private ComponentBus? PubSub { get; set; }

    [Parameter] public string CanvasStyle { get; set; } = "width:max-content; border:1px solid black;cursor:default";
    [Parameter] public int CanvasWidth { get; set; } = 2500;
    [Parameter] public int CanvasHeight { get; set; } = 4000;

    [Parameter] public ViewerSettings? Settings3D { get; set; }
    [Parameter] public Scene3D? Scene3D { get; set; }
    [Parameter,EditorRequired] public string? SceneName { get; set; }


    protected ViewerThreeD? Viewer3DReference;


    public string GetCanvasStyle()
    {
        var style = new StringBuilder(CanvasStyle)
            .Append("; ")
            .Append("width:")
            .Append(CanvasWidth)
            .Append("px; ")
            .Append("height:")
            .Append(CanvasHeight)
            .Append("px; ")
            .ToString();
        return style;
    }

    // https://stackoverflow.com/questions/72488563/blazor-server-side-application-throwing-system-invalidoperationexception-javas

    public async ValueTask DisposeAsync()
    {
        try
        {
            "Canvas3DBase DisposeAsync".WriteInfo();
            //await DoStop();
            //await _jsRuntime!.InvokeVoidAsync("AppBrowser.Finalize");
            await ValueTask.CompletedTask;
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            $"Canvas3DBase DisposeAsync Exception {ex.Message}".WriteError();
        }


    }

    public void Dispose()
    {
        try
        {
            "Canvas3DBase Dispose".WriteInfo();
            PubSub!.UnSubscribeFrom<RefreshUIEvent>(OnRefreshUIEvent);
            GC.SuppressFinalize(this);
        }
        catch (JSDisconnectedException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            $"Canvas3DBase Dispose Exception {ex.Message}".WriteError();
        }
    }

    public (bool, Scene3D) GetActiveScene() 
    {
        if (Viewer3DReference == null) 
            return (false, null!);

        var scene = Viewer3DReference.GetActiveScene();
        var found = scene != null;
        return (found, scene!);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = GetActiveScene();
            if (found)
                Workspace?.GetArena()?.SetScene(scene);

            scene?.SetAfterUpdateAction((s,j)=>
            {
                PubSub!.Publish<RefreshUIEvent>(new RefreshUIEvent("Canvas3DBase"));
            });


            PubSub!.SubscribeTo<RefreshUIEvent>(OnRefreshUIEvent);

        }
        await base.OnAfterRenderAsync(firstRender);
    }


    private void OnRefreshUIEvent(RefreshUIEvent e)
    {
        //InvokeAsync(StateHasChanged);
        $"Canvas3DBase OnRefreshUIEvent StateHasChanged {e.note}".WriteInfo();

        Task.Run(async () =>
        {
            var arena = Workspace?.GetArena();
            if ( arena != null )
                await arena.UpdateArena();
            //$"after ThreeJSView3D.UpdateScene() {e.note}".WriteInfo();
        });
    }




}
