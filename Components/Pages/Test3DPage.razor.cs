﻿

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
        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        Task.Run(async () => await scene.Request3DModel(model, async (uuid) => {
            var group = new Group3D()
            {
                Name = "Axis",
                Uuid = uuid,
            };
            scene.AddChild(group);
            $"Axis added to scene in callback".WriteSuccess();
            StateHasChanged();
            await Task.CompletedTask;
        }));
    }



    public async Task OnAddTRex()
    {
        var (success, scene) = GetCurrentScene();
        if (!success) return;

        var model = new Model3D()
        {
            Name = "TRex",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"),
            Format = Model3DFormats.Gltf,
            Transform = new Transform3()
            {
                Position = new Vector3(2, 0, 2)
            },
        };

        await scene.Request3DModel(model);

    }

    public async Task OnAddJet()
    {
        var (success, scene) = GetCurrentScene();
        if (!success) return;

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

    public async Task OnAddCar()
    {
        var (success, scene) = GetCurrentScene();
        if (!success) return;

        var model = new Model3D()
        {
            Name = "Car",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/mustang_1965.glb"),
            Format = Model3DFormats.Gltf,
        };

        await scene.Request3DModel(model);
        await Task.CompletedTask;
    }

     public async Task OnAddText()
    {
        var x = DataGenerator.GenerateInt(-5, 5);
        var y = DataGenerator.GenerateInt(-5, 5);
        var z = DataGenerator.GenerateInt(-5, 5);

        TestText = new Text3D("My First Text") 
        { 
            Transform = new Transform3() 
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

       await Task.CompletedTask;
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

        await Task.CompletedTask;
    }

    public void Dispose()
    {

    }
}