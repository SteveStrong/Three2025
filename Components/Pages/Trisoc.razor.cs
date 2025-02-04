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
using Three2025.Apprentice;


namespace Three2025.Components.Pages;

public partial class TrisocBase : ComponentBase
{
    public Canvas3DComponentBase Canvas3DReference = null;

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public ITrisocTech Tech { get; init; }
    [Inject] public ILightingTech LightTech { get; init; }


    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;


    protected MockDataGenerator DataGenerator { get; set; } = new();
 



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

            var arena = Workspace.GetArena();
            if (found)
            {
                arena.SetScene(scene!);
                DoRequestAxisToScene(scene!);
            }
                
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public void DoRequestAxisToScene(Scene3D scene)
    {


        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        scene.AddChild(model);
    }


    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        //path.WriteSuccess();
        return path;
    }
    

    
    public void DoAddTRISOCToArena()
    {

        var url = GetReferenceTo(@"storage/StaticFiles/TRISOC.glb");

        Tech.GetTrisocModel(url);

    }

    public void DoAddBoxArena()
    {

        var (c, center) = Tech.GetSpacialBox("Center", 0, "C");


        var (t,top) = Tech.GetSpacialBox("Top",c, "T");
        top.Transform.Position = center.Transform.Position.CreatePlus(0, -10, 0);


        var (f,front) = Tech.GetSpacialBox("Front",t, "F");
        front.Transform.Position = center.Transform.Position.CreatePlus(0, 0, 10);


        var arena = Workspace.GetArena();
        arena.AddShapeToStage(center);
        arena.AddShapeToStage(top);        
        arena.AddShapeToStage(front);       


    }

    public void DoStartStopTimer()
    {
        Tech.StartStopTimer();
    }


    public void DoReposition()
    {
        var list = LightTech.GetLights();
        for(int i=0; i<list.Count-1; i++)
        {
            var pos1 = list[i].Transform.Position;
            var light = list[i+1].GetName();
            LightTech.RepositionLight(light, pos1.X +5, pos1.Y +5, 0);
        }

    }


}


