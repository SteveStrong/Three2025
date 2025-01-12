

using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorThreeJS.Geometires;


using FoundryRulesAndUnits.Models;

using Three2025.Shared;


namespace Three2025.Components.Pages;

public class Test3DPageBase : ComponentBase, IDisposable
{

    [Inject] public NavigationManager Navigation { get; set; }

    protected Canvas3D Canvas3DReference;
    protected Text3D TestText;

    protected MockDataGenerator DataGenerator { get; set; } = new();
    

    public (bool, Scene3D) GetCurrentScene()
    {
        var (success, scene) = Canvas3DReference.GetActiveScene();
        return (success, scene);
    }

    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //for now this call is too early,  scene is not setup
            //DoRenderingTest();

        }
        return base.OnAfterRenderAsync(firstRender);
    }

    public void DoAxisTest()
    {
        var (found, scene) = GetCurrentScene();
        if ( !found ) return;
        AddAxisToScene(scene);

    }



    public void AddAxisToScene(Scene3D scene)
    {
        var pos = new Vector3(0, 0, 0);
        var rot = new Euler(0, 0, 0);
        var piv = new Vector3(0, 0, 0);



        var Uuid = Guid.NewGuid().ToString();

        var model = new ImportSettings
        {
            Uuid = Uuid,
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Transform = new Transform3D()
            {
                Position = pos,
                Rotation = rot,
                Pivot = piv
            },
            OnComplete = () =>
            {
                var group = new Group3D()
                {
                    Name = "Axis",
                    Uuid = Uuid,
                };
                scene.AddChild(group);
                StateHasChanged();
            }

        };

        Task.Run(async () => await scene.Request3DModel(model));
    }



    public async Task OnAddTRex()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid().ToString(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"),
            Transform = new Transform3D()
            {
                Position = new Vector3(2, 0, 2)
            },
        };

        var (success, scene) = GetCurrentScene();
        if (!success) return;

        await scene.Request3DModel(model);
        await scene.UpdateScene();
    }

    public async Task OnAddJet()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid().ToString(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/jet.glb"),
        };


        var (success, scene) = GetCurrentScene();
        if (!success) return;
        await scene.Request3DModel(model);
        await scene.UpdateScene();;
    }

    public async Task OnAddCar()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid().ToString(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/mustang_1965.glb"),
        };


        var (success, scene) = GetCurrentScene();
        if (!success) return;
        await scene.Request3DModel(model);
        await scene.UpdateScene();
    }

     public async Task OnAddText()
    {
        var x = DataGenerator.GenerateInt(-5, 5);
        var y = DataGenerator.GenerateInt(-5, 5);
        var z = DataGenerator.GenerateInt(-5, 5);

        TestText = new Text3D("My First Text") 
        { 
            Transform = new Transform3D() 
            { 
                Position = new Vector3(x, y, z), 
                Pivot = new Vector3(0, 0, 0), 
                Rotation = new Euler(0, 0, 0), 
            },
            Color = "#33333a",
            Uuid = Guid.NewGuid().ToString(),
        };

        var (success, scene) = GetCurrentScene();
        if (!success) return;

        scene.AddChild(TestText);

        await scene.UpdateScene();
    }

    public async Task OnUpdateText()
    {
        var newText = DateTime.Now.ToLongTimeString();

        //$"newText={newText}".WriteInfo();

        if (TestText != null) 
        {
            TestText.Text = newText;
            TestText.Color = DataGenerator.GenerateColor();
        }

        var (success, scene) = GetCurrentScene();
        if (!success) return;

        await scene.UpdateScene();
    }

    public void Dispose()
    {

    }
}