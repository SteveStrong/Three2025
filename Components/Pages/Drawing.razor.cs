﻿using FoundryBlazor.Shared;
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
using Three2025.Apprentice;



namespace Three2025.Components.Pages;

public partial class DrawingBase : ComponentBase, IDisposable
{

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }

    public Canvas2DComponentBase Canvas2DReference = null;

    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;


    protected MockDataGenerator DataGenerator { get; set; } = new();

    public (bool, Scene3D) GetCurrentScene()
    {
        var arena = Workspace.GetArena();
        return arena.CurrentScene();
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
            CreateMenus(Workspace);
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
        var shape = new FoModel3D(name,"blue")
        {
            Name = name,
            Url = url,
            GlyphId = Guid.NewGuid().ToString(),
            //BoundingBox = new Vector3(bx, by, bz),
        };
        var arena = Workspace.GetArena();
        arena.AddShapeToStage<FoShape3D>(shape);
        return shape;
    }



    
    public void CreateMenus(IWorkspace space)
    {
        var arena = space.GetArena();
        var stage = arena.CurrentStage();

        // arena.AddAction("Update", "btn-primary", () =>
        // {
        // });

        // arena.AddAction("Clear", "btn-primary", () =>
        // {
        // });

        // stage.AddAction("Clear", "btn-primary", () =>
        // {
        //  });

        // stage.AddAction("Render", "btn-primary", () =>
        // {
        // });
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

            var box = AddBox(DataGenerator.GenerateName());
            arena.AddShapeToStage<FoShape3D>(box);
        });

        world.AddAction("TRex", "btn-primary", () =>
        {
            var url = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb");
            var shape = DoLoad3dModel(url, -2, 6, -2);
            arena.AddShapeToStage<FoShape3D>(shape);
        });

        world.AddAction("Porsche", "btn-primary", () =>
        {
            var url = GetReferenceTo(@"storage/StaticFiles/porsche_911.glb");
            var shape = DoLoad3dModel(url, 2, 6, 2);
            arena.AddShapeToStage<FoShape3D>(shape);
        });
        
        world.AddAction("Render Tube", "btn-primary", () =>
        {
            var (found, scene) = arena.CurrentScene();
            if ( !found ) return;


            var capsuleRadius = 0.15f;
            var capsulePositions = new List<Vector3>() {
                new Vector3(0, 0, 0),
                new Vector3(4, 0, 0),
                new Vector3(4, 4, 0),
                new Vector3(4, 4, -4)
            };


            scene.AddChild(new Mesh3D
            {
                Uuid = Guid.NewGuid().ToString(),
                Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: capsuleRadius, path: capsulePositions),
              
                Material = new MeshStandardMaterial("yellow", 1.0)
            });

        });

    }




    public void OnAddTRex()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var arena = Workspace.GetArena();
        var shape = new FoModel3D("T-Rex " + name)
        {
            Url = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"),
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
            }
        };


        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        arena.AddShapeToStage<FoShape3D>(shape);
        //stage.PreRender(arena);

        // var (found, scene) = GetCurrentScene();
        // if (found)
        //     stage.RefreshScene(scene);
    }

    
    public void OnAddGeom()
    {
        var name = DataGenerator.GenerateName();
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var arena = Workspace.GetArena();
        var shape = new FoShape3D(name,color)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            }
        };

        var w = DataGenerator.GenerateDouble(1, 10);
        var h = DataGenerator.GenerateDouble(1, 10);
        var d = DataGenerator.GenerateDouble(1, 10);
        var index = DataGenerator.GenerateInt(0, 10);

        shape = index switch
        {
            0 => shape.CreateBox(label, w, h, d),
            1 => shape.CreateCone(label, w, h, d),
            2 => shape.CreateCylinder(label, w, h, d),
            3 => shape.CreateDodecahedron(label, w, h, d),
            4 => shape.CreateIcosahedron(label, w, h, d),
            5 => shape.CreateOctahedron(label, w, h, d),
            6 => shape.CreateSphere(label, w, h, d),
            7 => shape.CreateTetrahedron(label, w, h, d),
            8 => shape.CreateTorusKnot(label, w, h, d),
            9 => shape.CreateTorus(label, w, h, d),
            _ => shape.CreateBox(label, w, h, d),
        };
 



        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        arena.AddShapeToStage<FoShape3D>(shape);

        // var (found, scene) = GetCurrentScene();
        // if (found)
        //     stage.RefreshScene(scene);
    }

    public void OnAddText()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);
        var color = DataGenerator.GenerateColor();
        var arena = Workspace.GetArena();

        var shape = new FoText3D(name,color)
        {
            Text = DataGenerator.GenerateText(),
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            }
        };


        var stage = arena.EstablishStage<FoStage3D>("Main Stage");
        arena.AddShapeToStage<FoText3D>(shape);

        // var (found, scene) = GetCurrentScene();
        // if (found)
        //     stage.RefreshScene(scene);
    }

    public Node3D AddBox(string name, double x=0, double z=0)
    {
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var height = DataGenerator.GenerateDouble(1, 10);
        var box = new Node3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
                Pivot = new Vector3(0, height/2, 0)
            }

        };
        box.CreateBox(label, .5, height, .5);
        return box;
    }

    public Node3D AddCone(string name, double x=0, double z=0)
    {
        var color = DataGenerator.GenerateColor();
        var label = $"{name} {color}";

        var height = DataGenerator.GenerateDouble(1, 10);
        var box = new Node3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
                Pivot = new Vector3(0, height/2, 0)
            }

        };
        box.CreateCone(label, .75, height, .75);
        return box;
    }

    public void AddBoxToStage()
    {
        var name = DataGenerator.GenerateName();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);


        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();

        var box = AddBox(name,x,z);
        stage.AddShape<Node3D>(box);
        
        // var (found, scene) = GetCurrentScene();
        // if ( found )
        //     stage.RefreshScene(scene);
        //await scene.SetCameraPosition(new Vector3(9f, 9f, 9f),box.Position);
    }


    public void AddConeToArena()
    {
        var name = DataGenerator.GenerateName();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var box = AddCone(name,x,z);
        var arena = Workspace.GetArena();

        arena.EstablishStage<FoStage3D>("Main Stage");
        arena.AddShapeToStage<Node3D>(box);
        arena.UpdateArena();
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


   public async Task  DoAxisTest()
    {

        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var model = new Model3D()
        {
            Name = $"Axis:{DataGenerator.GenerateWord()}",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };


        await scene.Request3DModel(model, async (uuid) => {
            scene.AddChild(model);
            $"Axis added to scene in callback".WriteSuccess();
            StateHasChanged();
            await Task.CompletedTask;
        });
    }




    public async Task OnAddJet()
    {
        var model = new Model3D()
        {
            Name = $"JET:{DataGenerator.GenerateWord()}",
            Uuid = Guid.NewGuid().ToString(),
            Url =  GetReferenceTo(@"storage/StaticFiles/jet.glb"),
            Format = Model3DFormats.Gltf,
        };

        var (found, scene) = GetCurrentScene();
        if (!found) return;

        await scene.Request3DModel(model, async (uuid) => {
            scene.AddChild(model);
            StateHasChanged();
            await Task.CompletedTask;
        });
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

    }
}

