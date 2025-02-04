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

public partial class ClockBase : ComponentBase
{
    public Canvas3DComponentBase Canvas3DReference = null;

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IClockTech Tech { get; init; }


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
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false,null!);

            scene?.SetAfterUpdateAction((s,j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = Workspace.GetArena();
            if (found)
                arena.SetScene(scene!);
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        //path.WriteSuccess();
        return path;
    }
    

    
    public void DoAddTRISOCToArena()
    {
        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);
        var s = 50.0;


        var shape = new FoModel3D("TRISOC " + name)
        {
            Url = GetReferenceTo(@"storage/StaticFiles/TRISOC.glb"),
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
                Scale = new Vector3(s, s, s),
            }
        };


        var arena = Workspace.GetArena();
        arena.AddShapeToStage<FoModel3D>(shape);
    }


    public void DoClockFaceOnScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var mesh = Tech.CreateClockFaceMesh();
        scene.AddChild(mesh);
    }

    public void DoRunClock()
    {
        Tech.RunClock();
    }



    public void DoRequestAxisToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        scene.AddChild(model);
    }




    public void DoAddTextToArena()
    {
        var arena = Workspace.GetArena();
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var delta = 0.5;
        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);


        var text3d = new FoText3D()
        {
            Text = DataGenerator.GenerateText(),
            Color = DataGenerator.GenerateColor(),
            FontSize =  DataGenerator.GenerateDouble(.5, 5.0),
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            },
        };

        var label = new FoText3D()
        {
            Text = "Extra Text",
            Color = "White",
            Transform = new Transform3()
            {
                Position = new Vector3(0, 3, 0),
            },
        };
        text3d.AddSubGlyph3D(label);
        arena.AddShapeToStage<FoText3D>(text3d);

        //can we do some animation here?
        text3d.SetAnimationUpdate((self, tick, fps) =>
        {
            bool move = tick % 10 == 0;
            if (!move) return;

            var loc = self.Transform.Position.Z;
            loc += delta;
            if ( loc > 10 || loc < -10)
            {
                delta = -delta;
            }
            self.Transform.Position.Z = loc;
            self.SetDirty(true);
        });
    }

    
    public void DoAddBoxGLBToArena()
    {
        var arena = Workspace.GetArena();
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var angle = 0.0;
        var delta = 0.5;

        var model3d = new FoModel3D()
        {
            Name = "Box Animated",
            Url =  GetReferenceTo(@"storage/staticfiles/BoxAnimated.glb"),
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            },
        };

        var label = new FoText3D()
        {
            Text = "This is a Box",
            Color = "White",
            Transform = new Transform3()
            {
                Position = new Vector3(0, 3, 0),
            },
        };
        model3d.AddSubGlyph3D(label);


        arena.AddShapeToStage<FoModel3D>(model3d);

        model3d.SetAnimationUpdate((self, tick, fps) =>
        {
            bool move = tick % 10 == 0;
            if (!move) return;

            var loc = self.Transform.Position.X;
            loc += delta;
            if ( loc > 10 || loc < -10)
            {
                delta = -delta;
                if (loc > 10) angle = Math.PI;
                else angle = 0.0;
            }


            self.Transform.Position.X = loc;
            self.Transform.Rotation.Y = angle;
            self.SetDirty(true);
        });

    }

    public async Task DoRequestAddBoxGLBToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var x = DataGenerator.GenerateDouble(-10, 10);
        var y = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);

        var angle = 0.0;
        var delta = 0.5;

        var model = new Model3D()
        {
            Name = "Box Animated",
            Uuid = Guid.NewGuid().ToString(),
            Url =  GetReferenceTo(@"storage/staticfiles/BoxAnimated.glb"),
            Format = Model3DFormats.Gltf,
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            },
        };

        model.SetAnimationUpdate((self, tick, fps) =>
        {
            bool move = tick % 10 == 0;
            if (!move) return;

            var loc = self.Transform.Position.X;
            loc += delta;
            if ( loc > 10 || loc < -10)
            {
                delta = -delta;
                if (loc > 10) angle = Math.PI;
                else angle = 0.0;
            }


            self.Transform.Position.X = loc;
            self.Transform.Rotation.Y = angle;
            self.SetDirty(true);
        });


        await scene.Request3DModel(model, async (uuid) =>
        {
            scene.AddChild(model);
            await Task.CompletedTask;
        });
    }

    public async Task DoAddTRexToScene()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var name = DataGenerator.GenerateWord();
        var x = DataGenerator.GenerateDouble(-10, 10);
        var z = DataGenerator.GenerateDouble(-10, 10);
        var delta = 0.5;
        var angle = 0.0;

        var model = new Model3D()
        {
            Name = "TRex", // $"TRex:{DataGenerator.GenerateWord()}",
            Uuid = Guid.NewGuid().ToString(),
            Url =  GetReferenceTo(@"storage/staticfiles/T_Rex.glb"),
            Format = Model3DFormats.Gltf,
            Transform = new Transform3()
            {
                Position = new Vector3(x, 0, z),
            },
        };

        model.SetAnimationUpdate((self, tick, fps) =>
        {
            bool move = tick % 10 == 0;
            if (!move) return;

            var loc = self.Transform.Position.Z;
            loc += delta;
            if ( loc > 10 || loc < -10)
            {
                delta = -delta;
                if (loc > 10) angle = Math.PI;
                else angle = 0.0;
            }


            self.Transform.Position.Z = loc;
            self.Transform.Rotation.Y = angle;
            self.SetDirty(true);

            //FoGlyph2D.Animations.Tween<FoShape2D>(s1, new { PinX = s1.PinX - 150, }, 2, 2.2F);
            // FoGlyph2D.Animations.Tween<FoShape2D>(s2, new { PinX = s2.PinX + 150, PinY = s2.PinY + 50, }, 2, 2.4f).OnComplete(() =>
            // {
            //     service.ClearAll();
            // });
        });

        await scene.Request3DModel(model, async (uuid) =>
        {
            scene.AddChild(model);
            await Task.CompletedTask;
        });
    }


}


