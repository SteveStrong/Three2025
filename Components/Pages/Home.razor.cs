using FoundryWorldsAndDrawings.Solutions;
using FoundryMicroCore.Core.Extensions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Geometires;
using FoundryWorldsAndDrawings.ThreeD.Materials;
using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryWorldsAndDrawings.Shared;


namespace Three2025.Components.Pages;



public partial class HomeBase : ComponentBase, IDisposable
{
    public Canvas3DComponent Canvas3DReference = null;
    public Canvas2DComponent Canvas2DReference = null;
    private FoStage3D _homeStage; // Stage-centric pattern

    [Inject] public NavigationManager Navigation { get; set; }

    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    // RETIRED: Plugin710 dependencies - will restore in future implementation
    // [Inject] public IRackTech RackTech { get; init; }
    // [Inject] public ICageTech CageTech { get; init; }

    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;


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
            // Wait for Canvas to initialize (Canvas handles stage-scene linkage)
            await Task.Delay(100);
            
            // Stage-centric pattern: Get stage from canvas
            _homeStage = Canvas3DReference?.Stage;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }
    





    public void DoAddTubeToScene()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        if ( !found ) return;

        var x = DataGenerator.RandomDouble(-10, 10);
        var y = DataGenerator.RandomDouble(10, 20);
        var z = DataGenerator.RandomDouble(-10, 10);

        var ax = DataGenerator.RandomDouble(-Math.PI, Math.PI);
        var ay = DataGenerator.RandomDouble(-Math.PI, Math.PI);
        var az = DataGenerator.RandomDouble(-Math.PI, Math.PI);


        //var text = DataGenerator.RandomSentence();
        var color = DataGenerator.RandomColor();

        var shape = new FoPipe3D("Tube", color)
        {
            Transform = new Transform3("TubeTransform")
            {
                Position = new Vector3(x, y, z),
                Rotation = new Euler(ax, ay, az),
            }
        };
                
        var path = new List<Vector3>() {
            new Vector3(0, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(4, 4, 0),
            new Vector3(4, 4, -4)
        };

        shape.CreateTube("TheTube", 0.15f, path);

    }

    public void DoAddConeToScene()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        if ( !found ) return;

        var x = DataGenerator.RandomDouble(-10, 10);
        var y = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);

        //var Uuid = Guid.NewGuid().ToString();
        //var text = DataGenerator.RandomSentence();
        var color = DataGenerator.RandomColor();


        var mesh = new Mesh3D
        {            Name = DataGenerator.RandomWord(),
            Geometry = new ConeGeometry(radius: 0.5f, height: 2, radialSegments: 16),
            Transform = new Transform3("ConeTransform")
            {
                Position = new Vector3(x, y, z),
            },
            Material = new MeshStandardMaterial()
            {
                Color = color,
                FlatShading = true,
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        };
        scene.AddChild(mesh);
    }




    public void OnAddCageToRacks()
    {
        //RackTech.ComputeHitBoundaries(() => {
            // TODO: CageTech.CreateRoutingCage() no longer exists
            // CageTech.CreateRoutingCage();
        //});
    }

    public void DoAddRacksArena()
    {
        // TODO: This method uses old RackTech.CreateRack which doesn't exist.
        // Use RackTech.AddRack() from Plugin710 instead
        // var height = 10;
        // var r1 = RackTech.AddRack("rack1");
        // var r2 = RackTech.AddRack("rack2");
    }

    public string GeneratePath()
    {
        var rack = $"rack{DataGenerator.RandomInt(0, 7)}";
        var box = $"box{DataGenerator.RandomInt(0, 7)}";
        var cn = $"cn{DataGenerator.RandomInt(0, 7)}";
        return $"{rack}.{box}.{cn}";
    }

    public void DoAddRoutesArena()
    {
        //RackTech.ComputeHitBoundaries(() => {
            for (int i = 0; i < 20; i++)
            {
                TryAddRoutesArena();
            }
        //});

    }

 

    public void TryAddRoutesArena()
    {
        // TODO: This method uses old RackTech.TryCreatePipe which doesn't exist in IRackTech interface.
        // if (_homeStage == null) _homeStage = Canvas3DReference?.Stage;
        // if (_homeStage == null) return;
        // var (success, pipe) = RackTech.TryCreatePipe(GeneratePath(), GeneratePath());
        // if ( success ) 
        //     _homeStage.AddShape(pipe);

    }

    public void DoAddPipeToArena()
    {
        var v1 = new Vector3 (0, 0, 0);
        var v2 = new Vector3(3, 5, 7);

        var path = new List<Vector3>()
        {
            v1,
            new(v1.X, v1.Y, v2.Z),
            new(v2.X, v1.Y, v2.Z),
            v2
        };
        var shape = new FoPipe3D("test", "Red")
        {

        };
        shape.CreateTube("hello", 0.25, path);

        if (_homeStage == null) _homeStage = Canvas3DReference?.Stage;
        _homeStage?.AddShape(shape);  

    }
    public void DoAddWiresArena()
    {
        for (int i = 0; i < 20; i++)
        {
            TryAddWiresArena();
        }
    }



    public void TryAddWiresArena()
    {
        // TODO: This method uses old RackTech.TryFindHitPosition which doesn't exist in IRackTech interface.
        // var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        // if ( !found ) return;
        // if (_homeStage == null) _homeStage = Canvas3DReference?.Stage;
        // var (s1, cn1, v1) = RackTech.TryFindHitPosition<FoGlyph3D>(GeneratePath());
        // var (s2, cn2, v2) = RackTech.TryFindHitPosition<FoGlyph3D>(GeneratePath());
        // if (!s1 || !s2) return;
        // var capsuleRadius = 0.15f;
        // var capsulePositions = new List<Vector3>() { v1, v2 };
        // var mesh = new Mesh3D
        // {  Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: capsuleRadius, path: capsulePositions),
        //     Material = new MeshStandardMaterial("yellow", 1.0)
        // };
        // scene.AddChild(mesh);
    }

    public void DoAddEquipmentArena()
    {
        // TODO: This method uses old RackTech.DoAddEquipmentArena which doesn't exist.
        // Use RackTech.AddEquipmentToRack() from Plugin710 instead
        // RackTech.AddEquipmentToRack("equipmentName", "rackName");
    }


    
    public void DoAddGeomToArena()
    {
        var name = DataGenerator.RandomFullName();
        var color = DataGenerator.RandomColor();
        var label = $"{name} {color}";

        var x = DataGenerator.RandomDouble(-10, 10);
        var y = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);

        var shape = new FoShape3D(name,color)
        {
            Transform = new Transform3("ShapeTransform")
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
 
        if (_homeStage == null) _homeStage = Canvas3DReference?.Stage;
        _homeStage?.AddShape(shape);
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


        if (_homeStage == null) _homeStage = Canvas3DReference?.Stage;
        _homeStage?.AddShape(shape);
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

    public FoShape3D AddCone(string name, double x=0, double z=0)
    {
        var color = DataGenerator.RandomColor();
        var label = $"{name} {color}";

        var height = DataGenerator.RandomDouble(1, 10);
        var box = new FoShape3D(label,color)
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

        if (_homeStage == null) _homeStage = Canvas3DReference?.Stage;

        var box = AddBox(name,x,z);
        _homeStage?.AddShape(box);
        
    }



   public async Task DoAddAxisToScene()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        if (!found) return;

        var model = new Model3D()
        {
            Name = "Axis",            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };


        await scene.Request3DModel(model, async (uuid) => {

            scene.AddChild(model);
            await Task.CompletedTask;
        });
    }



    public void DoNavigateToCuckooClock()
    {
        Navigation.NavigateTo("/trex-cuckoo-clock");
    }
    public void DoRequestAddTextToScene()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        if (!found) return;

        var x = DataGenerator.RandomDouble(-10, 10);
        var y = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);


        var text3d = new Text3D()
        {            Text = DataGenerator.RandomSentence(),
            Color = DataGenerator.RandomColor(),
            Transform = new Transform3("Text3DTransform")
            {
                Position = new Vector3(x, y, z),
            },
        };
        scene.AddChild(text3d);
    }


    public async Task DoRequestAddJetToScene()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
        if (!found) return;

        var x = DataGenerator.RandomDouble(-10, 10);
        var y = DataGenerator.RandomDouble(-10, 10);
        var z = DataGenerator.RandomDouble(-10, 10);

        var model = new Model3D()
        {
            Name = $"JET:{DataGenerator.RandomWord()}",            Url =  GetReferenceTo(@"storage/StaticFiles/jet.glb"),
            Format = Model3DFormats.Gltf,
            Transform = new Transform3("JetTransform")
            {
                Position = new Vector3(x, y, z),
                Scale = new Vector3(0.1, 0.1, 0.1)
            },
        };


        await scene.Request3DModel(model, async (uuid) => {
            scene.AddChild(model);
            //StateHasChanged();
            await Task.CompletedTask;
        });
    }


    public void Dispose()
    {

    }
}

