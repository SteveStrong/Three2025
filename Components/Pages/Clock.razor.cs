using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryWorldsAndDrawings.Shape;

using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Objects;



namespace Three2025.Components.Pages;

public partial class ClockBase : ComponentBase, IDisposable
{
    public Canvas3DComponent Canvas3DReference = null;

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public IClockTech Tech { get; init; }


    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;

    protected double _currentFps = 0;
    protected int _currentTick = 0;

    protected MockDataGenerator DataGenerator { get; set; } = new();


    public (bool, Scene3D) GetCurrentScene()
    {
        var arena = Workspace.GetArena();
        return arena.CurrentScene();
    }


    protected override void OnInitialized()
    {
        Workspace.SetBaseUrl(Navigation?.BaseUri ?? "");
        
        // Clear any previous page's objects from the arena
        var arena = Workspace.GetArena();
        arena.ClearArena();
        $"Clock: Cleared arena on initialization".WriteInfo();
        
        // Subscribe directly to AnimationFrameBus for animation events
        $"Clock: Subscribing to AnimationEvent on AnimationFrameBus".WriteSuccess();
        AnimationFrameBus.SubscribeToAnimation(OnAnimationFrame);
        
        base.OnInitialized();
    }

    private void OnAnimationFrame(AnimationEvent animEvent)
    {
        if (animEvent.IsWorld3D())
        {
            _currentFps = animEvent.fps;
            _currentTick = animEvent.tick;
            //$"Clock OnAnimationFrame: Tick {_currentTick}, FPS {_currentFps:F1}".WriteInfo();
            
            // Only update UI every 15 frames to avoid overwhelming Blazor

            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        var arena = Workspace.GetArena();
        arena.ClearArena();
        // Unsubscribe when component is disposed
        AnimationFrameBus.UnSubscribeFromAnimation(OnAnimationFrame);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            $"Clock OnAfterRenderAsync: Canvas3DReference is {(Canvas3DReference == null ? "null" : "not null")}".WriteInfo();
            $"Clock OnAfterRenderAsync: Workspace is {(Workspace == null ? "null" : "available")}".WriteInfo();
            $"Clock OnAfterRenderAsync: FoundryService is {(FoundryService == null ? "null" : "available")}".WriteInfo();
            $"Clock OnAfterRenderAsync: Tech is {(Tech == null ? "null" : "available")}".WriteInfo();
            
            // Wait a bit for the canvas to initialize
            await Task.Delay(500); // Increased delay for better initialization
            
            if (Canvas3DReference != null)
            {
                var (found, scene) = Canvas3DReference.GetActiveScene();
                
                $"Clock OnAfterRenderAsync: GetActiveScene returned found={found}, scene={scene?.Name ?? "null"}".WriteInfo();

                if (found && scene != null)
                {

                    var arena = Workspace.GetArena();
                    arena.SetScene(scene);
                    $"Clock OnAfterRenderAsync: Scene set in arena successfully".WriteSuccess();
                    
                    // Try to add a simple object to test rendering
                    // try
                    // {
                    //     //DoClockFaceOnScene();
                    //     $"Clock OnAfterRenderAsync: Clock face added successfully".WriteSuccess();
                    // }
                    // catch (Exception ex)
                    // {
                    //     $"Clock OnAfterRenderAsync: Error adding clock face: {ex.Message}".WriteError();
                    //     $"Clock OnAfterRenderAsync: Stack trace: {ex.StackTrace}".WriteError();
                    // }
                }
                else
                {
                    $"Clock OnAfterRenderAsync: Canvas3DReference.GetActiveScene() failed to return valid scene".WriteError();
                }
            }
            else
            {
                $"Clock OnAfterRenderAsync: Canvas3DReference is null - component not properly bound".WriteError();
            }
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
            Transform = new Transform3("TRISOCTransform")
            {
                Position = new Vector3(x, 0, z),
                Scale = new Vector3(s, s, s),
            }
        };


        var arena = Workspace.GetArena();
        arena.AddShapeToStage<FoModel3D>(shape);
    }

    public void DoClockFace()
    {
        var clockFace = new FoClockFace3D("ArenaClockFace")
        {
            Radius = 12.0,
            Height = 0.2,
            FontSize = 1.2,
            Transform = new Transform3("ClockTransform")
            {
                Position = new Vector3(0, 0, 0),
            }
        };

        var arena = Workspace.GetArena();
        arena.AddShapeToStage<FoClockFace3D>(clockFace);

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
            FontSize = DataGenerator.GenerateDouble(2.5, 5.0),
            Transform = new Transform3("Text3DTransform")
            {
                Position = new Vector3(x, y, z),
            },
        };

        var label = new FoText3D()
        {
            Text = "Extra Text",
            Color = "White",
            Transform = new Transform3("LabelTransform")
            {
                Position = new Vector3(0, 3, 0),
            },
        };
        text3d.AddSubGlyph3D(label);
        arena.AddShapeToStage<FoText3D>(text3d);

        // Animation using MoveBy for proper dirty flag handling
        text3d.BeforeAnimationRefresh((self, tick, fps) =>
        {
            bool move = tick % 10 == 0;
            if (!move) return;

            //shift the characters 1 letter to the left
            // every itteration 
            text3d.Text = text3d.Text.Substring(1) + text3d.Text[0];

            var pos = self.Transform.MoveBy(0, 0, delta);
            if (pos.Z > 10 || pos.Z < -10)
            {
                delta = -delta;
            }
            self.SetTransformStale();
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
            Url = GetReferenceTo(@"storage/staticfiles/BoxAnimated.glb"),
            Transform = new Transform3("BoxTransform")
            {
                Position = new Vector3(x, y, z),
            },
        };

        var label = new FoText3D()
        {
            Text = "This is a Box",
            Color = "White",
            Transform = new Transform3("LabelTransform")
            {
                Position = new Vector3(0, 3, 0),
            },
        };
        model3d.AddSubGlyph3D(label);


        arena.AddShapeToStage<FoModel3D>(model3d);

        model3d.BeforeAnimationRefresh((self, tick, fps) =>
        {
            bool move = tick % 10 == 0;
            if (!move) return;

            var pos = self.Transform.MoveBy(delta, 0, 0);
            if (pos.X > 10 || pos.X < -10)
            {
                delta = -delta;
                if (pos.X > 10) angle = Math.PI;
                else angle = 0.0;
                self.Transform.RotateTo(0, angle, 0, AngleUnit.Radians);
            }

            self.SetTransformStale();
        });

    }

    // public async Task DoRequestAddBoxGLBToScene()
    // {
    //     var (found, scene) = GetCurrentScene();
    //     if (!found) return;

    //     var x = DataGenerator.GenerateDouble(-10, 10);
    //     var y = DataGenerator.GenerateDouble(-10, 10);
    //     var z = DataGenerator.GenerateDouble(-10, 10);

    //     var angle = 0.0;
    //     var delta = 0.5;

    //     var model = new Model3D()
    //     {
    //         Name = "Box Animated",
    //         Uuid = Guid.NewGuid().ToString(),
    //         Url = GetReferenceTo(@"storage/staticfiles/BoxAnimated.glb"),
    //         Format = Model3DFormats.Gltf,
    //         Transform = new Transform3("BoxTransform")
    //         {
    //             Position = new Vector3(x, y, z),
    //         },
    //     };

    //     model.SetAnimationUpdate((self, tick, fps) =>
    //     {
    //         bool move = tick % 10 == 0;
    //         if (!move) return;

    //         var pos = self.Transform.MoveBy(delta, 0, 0);
    //         if (pos.X > 10 || pos.X < -10)
    //         {
    //             delta = -delta;
    //             if (pos.X > 10) angle = Math.PI;
    //             else angle = 0.0;
    //             self.Transform.RotateTo(0, angle, 0, AngleUnit.Radians);
    //         }

    //         self.SetTransformStale();
    //     });


        // await scene.Request3DModel(model, async (uuid) =>
        // {
        //     scene.AddChild(model);
        //     await Task.CompletedTask;
        // });
    //}

    public void DoAddTRexToArena()
    {
        var arena = Workspace.GetArena();

        var range = 20.0;
        
        // Use array to hold mutable state (arrays are reference types)
        var state = new double[] { 0.5 }; // state[0] is delta - MUCH FASTER to see movement

        var uniqueName = $"WalkingSub-{Guid.NewGuid().ToString().Substring(0, 8)}";
        
        var model = new FoModel3D(uniqueName)
        {
            Url = GetReferenceTo(@"storage/staticfiles/T_Rex.glb"), // Use sub instead of T-Rex
            Transform = new Transform3("SubWalkTransform")
            {
                Position = new Vector3(0, 5, 0), // Start at center, raised up
                Scale = new Vector3(1, 1, 1), // Normal size for sub
            },
        };

        model.BeforeAnimationRefresh((self, tick, fps) =>
        {
            // Move every frame to make it obvious
            var delta = state[0];
            var pos = self.Transform.MoveBy(0, 0, delta);
            var loc = pos.Z;
            
            if (tick % 30 == 0) // Log every 30 frames
            {
                $"{uniqueName} at Z={loc:F2}, delta={delta}".WriteInfo();
            }

            if (loc > range)
            {
                state[0] = -Math.Abs(state[0]);
                self.Transform.RotateTo(0, Math.PI, 0, AngleUnit.Radians);
                //Console.WriteLine($"{uniqueName} turned around at Z={loc:F2}, delta now {state[0]}");
            }
            else if (loc < -range)
            {
                state[0] = Math.Abs(state[0]);
                self.Transform.RotateTo(0, 0, 0, AngleUnit.Radians);
                //Console.WriteLine($"{uniqueName} turned around at Z={loc:F2}, delta now {state[0]}");
            }

            self.SetTransformStale();
        });

        arena.AddShapeToStage<FoModel3D>(model);
        $"Added {uniqueName} to arena - submarine walking back and forth".WriteSuccess();
    }



    public void DoRequestAddSubToArena()
    {
        var arena = Workspace.GetArena();
        
        var name = "Sub";
        var state = new double[] { 0.0 }; // Use array for mutable state (reference type)
        var radius = 22.0;
        var y = -3.0;

        var model = new FoModel3D(name)
        {
            Url = GetReferenceTo(@"storage/staticfiles/sub.glb"),
            Transform = new Transform3("SubTransform")
            {
                Position = new Vector3(radius, y, 0),
                Scale = new Vector3(0.1, 0.1, 0.1),
                Rotation = new Euler(0, Math.PI/2, 0, AngleUnit.Radians)
            },
        };

        model.BeforeAnimationRefresh((self, tick, fps) =>
        {
            // Update angle using array reference
            state[0] += Math.PI / 120;
            var angle = state[0];
            
            var x = radius * Math.Cos(angle);
            var z = radius * Math.Sin(angle);
            self.Transform.Position = new Vector3(x, y, z);
            
            var direction = new Vector3(-Math.Sin(angle), 0, Math.Cos(angle));
            var rotationY = Math.Atan2(direction.X, direction.Z);
            rotationY += Math.PI/2;
            self.Transform.Rotation = new Euler(0, rotationY, 0, AngleUnit.Radians);
            self.SetTransformStale();
        });

        arena.AddShapeToStage<FoModel3D>(model);
    }
}
