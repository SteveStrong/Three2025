using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using FoundryBlazor;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;

using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryRulesAndUnits.Units;


namespace Three2025.Model;

public class CableChannels : FoComponent
{
    protected IWorkspace _space;
    protected CableWorld _world;

    protected IFoundryService _manager;

    public CableChannels()
    {
    }


    public CableChannels(IWorkspace space, IFoundryService manager)
    {
        _space = space;
        _manager = manager;
        _world = _manager.WorldManager().CreateWorld<CableWorld>("Cables");



        _world.AddAction("Clear", "btn-primary", () => 
        {
            _world.ClearAll();

        });

        _world.AddAction("Render", "btn-info", () => 
        {
            var count = _world.Members<FoShape3D>().Count();
            $"Rendering {count} shapes".WriteNote();
            var arena = _space.GetArena();
            arena.RenderWorld3D(_world);
        });

        _world.AddAction("TRex", "btn-primary", () =>
        {
            $"Loading T-Rex".WriteNote();

            var space = _space;
            var baseURL = $"{space.GetBaseUrl()}storage";
            var url = Path.Join(baseURL, "StaticFiles","T_Rex.glb");

            $"Loading {url}".WriteNote();

            DoLoad3dModel(url, 2, 6, 2);
        });

        _world.AddAction("Render Tube", "btn-primary", () =>
        {

            var arena = GetArena();
            var scene = arena.CurrentScene();


            var capsuleRadius = 0.15f;
            var capsulePositions = new List<Vector3>() {
                new Vector3(0, 0, 0),
                new Vector3(4, 0, 0),
                new Vector3(4, 4, 0),
                new Vector3(4, 4, -4)
            };

            scene.Add(new Mesh
            {
                Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: capsuleRadius, path: capsulePositions),
                Position = new Vector3(0, 0, 0),
                Material = new MeshStandardMaterial()
                {
                    Color = "yellow"
                }
            });

            Task.Run(async () =>
            {
                var window = arena.CurrentViewer();
                await window.UpdateScene();
            });
        });

        _world.AddAction("Draw it", "btn-primary", () =>
        {
            var arena = GetArena()!;
            var scene = arena.CurrentScene();

            var height = 4;

            var piv = new Vector3(-1, -height / 2, -3);
            var pos = new Vector3(0, height, 0);
            var rot = new Euler(0, Math.PI * 45 / 180, 0);
            scene.Add(new Mesh
            {
                Geometry = new BoxGeometry(width: 2, height: height, depth: 6),
                Position = pos,
                Rotation = rot,
                Pivot = piv,
                Material = new MeshStandardMaterial()
                {
                    Color = "magenta"
                }
            });

            Task.Run(async () =>
            {
                var window = arena.CurrentViewer();
                await window.UpdateScene();
            });
        });
    }

    public CableWorld GetWorld()
    {
        return _world;
    }

    public IArena GetArena()
    {
        return _space.GetArena();
    }




  public void DoLoad3dModel(string url, double bx, double by, double bz)
    {
        var name = url.Split('\\').Last();
        var shape = new FoShape3D(name,"blue")
        {
            Name = name,
            GlyphId = Guid.NewGuid().ToString(),
            Position = new Vector3(0, 0, 0),
            BoundingBox = new Vector3(bx, by, bz),
            //Scale = new Vector3(.1, .1, .1)
        };
        shape.CreateGlb(url);
        GetWorld().AddGlyph3D<FoShape3D>(shape);
    }

    public void AddBox()
    {
        var box = new Node3D("test","blue")
        {
            GlyphId = Guid.NewGuid().ToString(),
            Position = new Vector3(0, 0, 0),
        };
        box.CreateBox("test", .5, 10, .5);
        
        GetWorld().AddGlyph3D<FoShape3D>(box);
    }

    // public IWorld3D AddShapeToArena(FoShape3D spec)
    // {
    //     var world = GetWorld();
    //     world.AddGlyph3D<FoShape3D>(spec);
    //    // GetArena().RenderWorld3D(world);
    //     return world;
    // }

    protected void GenerateGeometry()
    {
        var width = new Length(1, "m");
        var height = new Length(2.6, "m");
        var depth = new Length(.8, "m");
        var step = new Length(.2, "m");

        var (x, z) = (0.0, 0.0);
        GenerateCage(x, z, width, height, depth, step);
        x += width.Value() + 0.05;
        GenerateCage(x, z, width, height, depth, step);
        x += width.Value() + 0.05;
        GenerateCage(x, z, width, height, depth, step);
        x += width.Value() + 0.05;
        GenerateCage(x, z, width, height, depth, step);
    }



    public void BuildChannels()
    {
        GenerateGeometry();


        var world = GetWorld();
        var arena = GetArena();
        arena.RenderWorld3D(world);

    }






    public List<Node3D> GenerateColumn(double x, double z, Length Height, Length Step)
    {
        var world = GetWorld();
        var h = Height.Value();
        var s = Step.Value();

        var columns = new List<Node3D>();
        var y = 0.0;

        while (y <= h)
        {
            var name = $"Node:{x:F1}:{y:F1}:{z:F1}";
            var shape = new Node3D(name, "blue")
            {
                Position = new Vector3(x, y, z),
            };
            shape.CreateBox(name, .05, .03, .05);
            columns.Add(shape);
            world.AddGlyph3D<FoShape3D>(shape);
            y += s;
        }

        for (int i = 1; i < columns.Count; i++)
        {
            var start = columns[i - 1];
            var finish = columns[i];
            var link = new Link3D($"Link::{x:F1}:{z:F1}{i}", "blue", start, finish, 0.01);
            world.AddGlyph3D<FoShape3D>(link);
        }
        return columns;
    }

    //create a method to link together 2 columns of List<Node3D>
    public void LinkColumns(string name, List<Node3D> start, List<Node3D> finish)
    {
        var world = GetWorld();

        for (int i = 0; i < start.Count(); i++)
        {
            var s = start[i];
            var f = finish[i];
            var link = new Link3D($"{name}:{i}", "blue", s, f, 0.01);
            world.AddGlyph3D<FoShape3D>(link);
        }
    }


    public void GenerateCage(double x, double z, Length Width, Length Height, Length Depth, Length Step)
    {

        var w = Width.Value();
        var d = Depth.Value();

        var lf = GenerateColumn(x, z, Height, Step);
        var lb = GenerateColumn(x, z+d, Height, Step);
        var cb = GenerateColumn(x+w/2, z+d, Height, Step);
        var rb = GenerateColumn(x+w, z+d, Height, Step);
        var rf = GenerateColumn(x+w, z, Height, Step);

        LinkColumns($"left {x:F1}", lf, lb);
        LinkColumns($"left-back {x:F1}", lb, cb);
        LinkColumns($"right-back {x:F1}", cb, rb);
        LinkColumns($"right {x:F1}", rb, rf);


    }

}
