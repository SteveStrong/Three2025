using FoundryWorldsAndDrawings.Shared;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryWorldsAndDrawings.Shape;
using Three2025.Model;

using FoundryRulesAndUnits.Extensions;

using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;

using Plugin710.Apprentice;

using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Materials;
using FoundryWorldsAndDrawings.ThreeD.Geometires;



namespace Three2025.Components.Pages;

public partial class DrawingBase : ComponentBase, IDisposable
{

    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }

    public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;
    private FoStage3D _drawingStage; // Stage-centric pattern

    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;


    protected MockDataGenerator DataGenerator { get; set; } = new();

    public (bool, Scene3D) GetCurrentScene()
    {
        return Canvas3DReference?.GetActiveScene() ?? (false, null!);
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
            // Wait for Canvas to initialize (Canvas handles stage-scene linkage)
            await Task.Delay(100);
            
            // Stage-centric pattern: Get stage from canvas
            _drawingStage = Canvas3DReference?.Stage;
            
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
        if (_drawingStage == null) _drawingStage = Canvas3DReference?.Stage;
        _drawingStage?.AddShape(shape);
        return shape;
    }



    
    public void CreateMenus(IWorkspace space)
    {
        // Stage-centric pattern: menus don't need stage reference at creation time
        // Actions will use _drawingStage when invoked
    }

    public void CreateServices(IFoundryService manager, IArena arena, FoWorld3D world)
    {
        // Stage-centric pattern: actions use _drawingStage from page context
        world.AddAction("Clear", "btn-primary", () => 
        {
            world.ClearAll();
        });

        world.AddAction("Publish", "btn-info", () => 
        {
            // TODO: PublishToStage API removed - needs review
            if (_drawingStage != null)
                world.PublishToStage(_drawingStage);
        });

        world.AddAction("Box", "btn-info", () => 
        {
            var box = AddBox(DataGenerator.RandomFullName());
            if (_drawingStage == null) _drawingStage = Canvas3DReference?.Stage;
            _drawingStage?.AddShape(box);
        });

        world.AddAction("TRex", "btn-primary", () =>
        {
            var url = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb");
            DoLoad3dModel(url, -2, 6, -2); // DoLoad3dModel already adds to stage
        });

        world.AddAction("Porsche", "btn-primary", () =>
        {
            var url = GetReferenceTo(@"storage/StaticFiles/porsche_911.glb");
            DoLoad3dModel(url, 2, 6, 2); // DoLoad3dModel already adds to stage
        });
        
        world.AddAction("Render Tube", "btn-primary", () =>
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
            if ( !found ) return;


            var capsuleRadius = 0.15f;
            var capsulePositions = new List<Vector3>() {
                new Vector3(0, 0, 0),
                new Vector3(4, 0, 0),
                new Vector3(4, 4, 0),
                new Vector3(4, 4, -4)
            };


            scene.AddChild(new Mesh3D
            {                Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: capsuleRadius, path: capsulePositions),
              
                Material = new MeshStandardMaterial("yellow", 1.0)
            });

        });

    }




    public void OnAddTRex()
    {
        var name = DataGenerator.RandomWord();
        var x = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);

        var shape = new FoModel3D("T-Rex " + name)
        {
            Url = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"),
            Transform = new Transform3("TRexTransform")
            {
                Position = new Vector3(x, 0, z),
            }
        };

        if (_drawingStage == null) _drawingStage = Canvas3DReference?.Stage;
        _drawingStage?.AddShape(shape);
    }

    
    public void OnAddGeom()
    {
        var name = DataGenerator.RandomFullName();
        var color = DataGenerator.RandomColor();
        var label = $"{name} {color}";

        var x = DataGenerator.RandomDouble(-10, 10);
        var y = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);

        var shape = new FoShape3D(name,color)
        {
            Transform = new Transform3("GeomTransform")
            {
                Position = new Vector3(x, y, z),
            }
        };

        var w = DataGenerator.RandomDouble(1, 10);
        var h = DataGenerator.RandomDouble(1, 10);
        var d = DataGenerator.RandomDouble(1, 10);
        var index = DataGenerator.RandomInt(0, 10);

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
 
        if (_drawingStage == null) _drawingStage = Canvas3DReference?.Stage;
        _drawingStage?.AddShape(shape);
    }

    public void OnAddText()
    {
        var name = DataGenerator.RandomWord();
        var x = DataGenerator.RandomDouble(-10, 10);
        var y = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);
        var color = DataGenerator.RandomColor();

        var shape = new FoText3D(name,color)
        {
            Text = DataGenerator.RandomSentence(),
            Transform = new Transform3("TextTransform")
            {
                Position = new Vector3(x, y, z),
            }
        };


        if (_drawingStage == null) _drawingStage = Canvas3DReference?.Stage;
        _drawingStage?.AddShape(shape);
    }

    public void OnAddPorsche()
    {
        var url = GetReferenceTo(@"storage/StaticFiles/porsche_911.glb");
        DoLoad3dModel(url, 2, 6, 2); // DoLoad3dModel already adds to stage
    }

    public void OnRenderTube()
    {
        var (found, scene) = GetCurrentScene();
        if (!found) return;

        var capsuleRadius = 0.15f;
        var capsulePositions = new List<Vector3>() {
            new Vector3(0, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(4, 4, 0),
            new Vector3(4, 4, -4)
        };

        scene.AddChild(new Mesh3D
        {            Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: capsuleRadius, path: capsulePositions),
            Material = new MeshStandardMaterial("yellow", 1.0)
        });
    }

    public async void OnClearScene()
    {
        // Clear the 3D scene if available
        var (found, scene) = GetCurrentScene();
        if (found && scene != null)
        {
            await scene.ClearAll();
        }
    }

    public FoShape3D AddBox(string name, double x=0, double z=0)
    {
        var color = DataGenerator.RandomColor();
        var label = $"{name} {color}";

        var height = DataGenerator.RandomDouble(1, 10);
        var box = new FoShape3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3("BoxTransform")
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
        var color = DataGenerator.RandomColor();
        var label = $"{name} {color}";

        var height = DataGenerator.RandomDouble(1, 10);
        var box = new Node3D(label,color)
        {
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3("ConeTransform")
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
        var name = DataGenerator.RandomFullName();
        var x = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);

        if (_drawingStage == null) _drawingStage = Canvas3DReference?.Stage;

        var box = AddBox(name,x,z) as Plugin710.Apprentice.Node3D;
        _drawingStage?.AddShape(box);
    }


    public void AddConeToArena()
    {
        var name = DataGenerator.RandomFullName();
        var x = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);

        var box = AddCone(name,x,z);
        if (_drawingStage == null) _drawingStage = Canvas3DReference?.Stage;
        _drawingStage?.AddShape(box);
    }


    protected void GoDrawing()
    {
        //"Click Go".WriteInfo();

        var drawing = Workspace.GetDrawing();
        var page = drawing?.FirstPage();
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
            Name = $"Axis:{DataGenerator.RandomWord()}",            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
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
            Name = $"JET:{DataGenerator.RandomWord()}",            Url =  GetReferenceTo(@"storage/StaticFiles/jet.glb"),
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
        // SetCurrentPage is removed - pages are managed by canvas in stage-centric pattern
        // Navigation to specific pages should be handled via page routing or canvas switching
        $"GoToPage: {page?.Title} - navigation pattern needs review".WriteWarning();
        return true;
    }

    public void Dispose()
    {

    }
}

