using BlazorThreeJS.Maths;
using BlazorThreeJS.Solutions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;



public interface ITrisocTech : ITechnician
{
    FoModel3D GetTrisocModel(string url);
    FoShape3D GetSpacialBox();

    void StartStopTimer();
}

public class TrisocTech : ITrisocTech
{

    protected IFoundryService FoundryService { get; init; }
    protected IThreeDService Render3dService { get; set; }
    protected MockDataGenerator DataGenerator { get; set; } = new();

    private Timer _timer = null!;

    private FoModel3D CurrentModel { get; set; } = null!;

    public TrisocTech(IFoundryService foundry, IThreeDService render3d)
    {

        FoundryService = foundry;
        Render3dService = render3d;
    }



    public bool ComputeHitBoundaries(Action OnComplete)
    {
        var arena = FoundryService.Arena();
        var (success, scene) = arena.CurrentScene();

        if (!success) return false;
        scene.UpdateHitBoundaries(OnComplete);
        return true;
    } 

    private void UpdateClock(object state)
    {
        //UpdateSceneClock(state);
    }

    public void StartStopTimer()
    {
        if (_timer == null)
        {
            _timer = new Timer(UpdateClock, null, 0, 1000);
        }
        else
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    public FoText3D LetterText3D(FoShape3D parent, string text, Vector3 pos, double size=1.0)
    {

        var letter = new FoText3D()
        {
            Name = text,
            Text = text,
            Color = "white",
            FontSize = size,
            Transform = new Transform3()
            {
                Position = pos,
            },
        };

        parent.AddSubGlyph3D(letter);
        return letter;
    }


    public FoModel3D GetTrisocModel(string url)
    {
        if ( CurrentModel != null)
            return CurrentModel;

        var s = 100.0;
        var y = 10.0;


        CurrentModel = new FoModel3D("TRISOC")
        {
            Url = url,
            Transform = new Transform3()
            {
                Position = new Vector3(0, y, 0),
                Rotation = new Euler(Math.PI/2, Math.PI/2, 0),
                Scale = new Vector3(s, s, s),
            }
        };

        LetterText3D(CurrentModel, "Test", new Vector3(0, 0, 0), .5);

        var arena = FoundryService.Arena();
        arena.AddShapeToStage<FoModel3D>(CurrentModel);
        return CurrentModel;
    }

    public FoShape3D GetSpacialBox()
    {

        var root = new FoShape3D("Root")
        {
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, 0),
                Pivot = new Vector3(5, 5, 5),
            }
        }.CreateBoundary("Root", 10, 10, 10);


        var box = new SpacialBox3D(10, 10, 10, "cm");

        var i = 0;
        var list = new List<Point3D>();
        list.AddRange(box.Vertices);
        list.AddRange(box.FaceCenters);
        list.AddRange(box.EdgeCenters);

        foreach (var item in list)
        {
            var sphere = new FoShape3D("Sphere")
            {
                Transform = new Transform3()
                {
                    Position = item.AsVector3(),
                }
            }.CreateSphere($"S{i++}", .15, .15, .15);

            LetterText3D(sphere, sphere.GetName(), new Vector3(1, 1, 1), .5);

            root.AddSubGlyph3D(sphere);
        }

        var path1 = box.Vertices.Select(p => p.AsVector3()).ToList();
        var path2 = box.FaceCenters.Select(p => p.AsVector3()).ToList();
        var path3 = box.EdgeCenters.Select(p => p.AsVector3()).ToList();

        var pipe1 = new FoPipe3D("Pipe1")
        {
            Color = "red",
            Closed = true,
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, 0),
            }
        }.CreateTube("Pipe1", 0.1, path1);


        root.AddSubGlyph3D(pipe1);

        return root;
    }




}