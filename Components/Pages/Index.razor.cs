

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
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Objects;

namespace Three2025.Components.Pages;

public class IndexBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;

    protected ViewerThreeD View3D;
    
    protected ViewerThreeD ViewSteve3D;
    public FoWorkbook Workbook { get; set; }



    public Scene GetCurrentScene()
    {
        return View3D.ActiveScene;
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

    // public Scene GetCurrentScene()
    // {
    //     if (_currentScene != null )
    //         return _currentScene;

    //     var title = "Custom Scene";
    //     if (Scene.FindBestScene(title, out Scene result))
    //     {
    //         _currentScene = result;
    //     }
    //     else
    //     {
    //         _currentScene = new Scene(title, JsRuntime);
    //         _currentScene.Add(new AmbientLight() { Name = "IndexBase" });
    //         _currentScene.Add(new PointLight()
    //         {
    //             Name = "IndexBase",
    //             Position = new Vector3(1, 3, 0)
    //         });
    //     };

    //     return _currentScene;
    // }





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

    public async Task DoMeshTest()
    {
        var scene = GetCurrentScene();


        scene.Add(new Mesh
        {
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(-2, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "magenta"
            }
        });

        // scene.Add(new Mesh
        // {
        //     Geometry = new CircleGeometry(radius: 0.75f, segments: 12),
        //     Position = new Vector3(2, 0, 0),
        //     Scale = new Vector3(1, 0.75f, 1),
        //     Material = new MeshStandardMaterial()
        //     {
        //         Color = "#98AFC7"
        //     }
        // });

        scene.Add(new Mesh
        {
            Geometry = new CapsuleGeometry(radius: 0.5f, length: 2),
            Position = new Vector3(-4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new ConeGeometry(radius: 0.5f, height: 2, radialSegments: 16),
            Position = new Vector3(4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "green",
                FlatShading = true,
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new CylinderGeometry(radiusTop: 0.5f, height: 1.2f, radialSegments: 16),
            Position = new Vector3(0, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "red",
                Wireframe = true
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new DodecahedronGeometry(radius: 0.8f),
            Position = new Vector3(-2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "darkviolet",
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new IcosahedronGeometry(radius: 0.8f),
            Position = new Vector3(-4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "violet"
            }
        });

        scene.Add(new Mesh
        {

            Geometry = new OctahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "aqua"
            }
        });

        scene.Add(new Mesh
        {
            Geometry = new PlaneGeometry(width: 0.5f, height: 2),
            Position = new Vector3(4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "purple"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new RingGeometry(innerRadius: 0.6f, outerRadius: 0.7f),
            Position = new Vector3(0, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "DodgerBlue"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new SphereGeometry(radius: 0.6f),
            Position = new Vector3(-2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            },
        });
        scene.Add(new Mesh
        {
            Geometry = new TetrahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightblue"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new TorusGeometry(radius: 0.6f, tube: 0.4f, radialSegments: 12, tubularSegments: 12),
            Position = new Vector3(4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightgreen"
            }
        });
        scene.Add(new Mesh
        {
            Geometry = new TorusKnotGeometry(radius: 0.6f, tube: 0.1f),
            Position = new Vector3(-4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "RosyBrown"
            }
        });

        await GetCurrentScene().UpdateScene();
    }


    public void Dispose()
    {

    }
}