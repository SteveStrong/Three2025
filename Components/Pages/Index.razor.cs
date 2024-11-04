

using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Lights;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Three2025.Components.Pages;

public class IndexBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;


    public FoWorkbook Workbook { get; set; }

    private Scene _currentScene { get; set; } = null;
    private ViewerSettings _settings { get; set; } = null;



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
        var pos = new Vector3(0, 0, 0);
        var rot = new Euler(0, 0, 0);
        var piv = new Vector3(0, 0, 0);

        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Position = pos,
            Rotation = rot,
            Pivot = piv,
            OnComplete = (Scene scene, Object3D object3D) =>
            {
                if (object3D != null)
                {
                    $"OnComplete callback object3d.Uuid={object3D.Uuid}".WriteSuccess();
                    StateHasChanged();
                }
                else
                {
                    "object3D is null".WriteError();
                }
            }
        };

        Task.Run(async () => await GetCurrentScene().Request3DModel(model));
        //Task.Run(async () => await scene.UpdateScene());
    }

    public Scene GetCurrentScene()
    {
        if (_currentScene != null )
            return _currentScene;

        var title = "Custom Scene";
        if (Scene.FindBestScene(title, out Scene result))
        {
            _currentScene = result;
        }
        else
        {
            _currentScene = new Scene(title, JsRuntime);
            _currentScene.Add(new AmbientLight() { Name = "IndexBase" });
            _currentScene.Add(new PointLight()
            {
                Name = "IndexBase",
                Position = new Vector3(1, 3, 0)
            });
        };

        return _currentScene;
    }

    public ViewerSettings GetSettings()
    {
        _settings ??= new()
        {
            CanSelect = true,// default is false
            SelectedColor = "black",
            Width = CanvasWidth,
            Height = CanvasHeight,
            WebGLRendererSettings = new WebGLRendererSettings
            {
                Antialias = false // if you need poor quality for some reasons
            }
        };

        return _settings;
    }



    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }

    public async Task OnAddTRex()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"),
            Position = new Vector3(2, 0, 2),
        };



        await GetCurrentScene().Request3DModel(model);
        await GetCurrentScene().UpdateScene();
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


        await GetCurrentScene().Request3DModel(model);
        await GetCurrentScene().UpdateScene();
    }

    public async Task OnAddCar()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/mustang_1965.glb"),
            Position = new Vector3(0, 0, 0),
        };


        await GetCurrentScene().Request3DModel(model);
        await GetCurrentScene().UpdateScene();
    }

    public void Dispose()
    {
        // if ( _currentScene != null )
        //     Scene.RemoveScene(_currentScene);
    }
}