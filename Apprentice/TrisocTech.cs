using System.Drawing;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorThreeJS.Core;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Solutions;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

public class Label3D : FoText3D
{


    public Label3D(string name, string color, Text3DAlign align) : base(name, color)
    {
        TextAlign = align;
        AnchorY = Text3DAnchor.Top;
        AnchorX = align == Text3DAlign.Right ? Text3DAnchor.Left : Text3DAnchor.Right;

        Transform = new Transform3()
        {
            Position = align == Text3DAlign.Left ? new Vector3(-5, 0, 0) : new Vector3(3, 0, 0),
        };
    }
}


public interface ITrisocTech : ITechnician
{
    FoModel3D GetTrisocModel(string url);
    (int j, FoShape3D shape) GetSpacialBox(string name, int i, string section);

    void StartStopTimer();
}

public class TrisocTech : ITrisocTech
{

    protected IFoundryService FoundryService { get; init; }
    protected IThreeDService Render3dService { get; set; }
    protected MockDataGenerator DataGenerator { get; set; } = new();

    private Timer _timer = null!;
    private Label3D GlobalText = null!;
    private FoPipe3D GlobalPipe = null!;

    private FoModel3D CurrentModel { get; set; } = null!;
    private Dictionary<string, (FoShape3D,Label3D)> Tags = new();

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
        if ( GlobalText == null )
        {
            GlobalText = new Label3D("Clock", "black", Text3DAlign.Left)
            {
                Text = DateTime.Now.ToString("HH:mm:ss"),
                FontSize = 1.5,
                AnchorX = Text3DAnchor.Center,
                AnchorY = Text3DAnchor.Middle,
                TextAlign = Text3DAlign.Center,
                Transform = new Transform3()
                {
                    Position = new Vector3(0, 0, 0),
                },
            };

            var arena = FoundryService.Arena();
            arena.AddShapeToStage<Label3D>(GlobalText);
        }
        else
        {
            GlobalText.Text = DateTime.Now.ToString("HH:mm:ss");
            UpdateTemperatures();

            if ( GlobalPipe != null )
            
                GlobalPipe.Color = DataGenerator.GenerateColor();
        }
    }

    private static Color GetColorForTemperature(int temperature)
    {
        // Example gradient from blue (-20°C) to red (100°C)
        int r = (int)((temperature + 20) * 255 / 120);
        int g = 0;
        int b = 255 - r;
        return Color.FromArgb(r, g, b);
    }

    private void UpdateTemperatures()
    {
        foreach (var item in Tags)
        {
            var (shape, label) = item.Value;

            var temp = DataGenerator.GenerateInt(-20, 100);
            label.Text = $"{temp} C";


            var color = GetColorForTemperature(temp);
            shape.Color = DataGenerator.GenerateColor();

            //$"Updating {label.Text} {shape.Color} {color}".WriteInfo();
        }
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

    public Label3D CreateTextLabel3D(FoShape3D parent, string name, string text, Text3DAlign align, double size=1.0)
    {

        var letter = new Label3D(name, "black", align)
        {
            Text = text,
            FontSize = size,
        };

        parent.AddSubGlyph3D(letter);
        return letter;
    }


    public FoModel3D GetTrisocModel(string url)
    {
        if ( CurrentModel != null)
            return CurrentModel;

        var s = 100.0;
        var y = 0.0;


        CurrentModel = new FoModel3D("TRISOC")
        {
            Url = url,
            Transform = new Transform3()
            {
                Position = new Vector3(0, y, 0),
                Rotation = new Euler(Math.PI/2, Math.PI, Math.PI/2),
                Scale = new Vector3(s, s, s),
            }
        };

        CreateTextLabel3D(CurrentModel, "xxx", "Test", Text3DAlign.Left, 1.5);

        var arena = FoundryService.Arena();
        arena.AddShapeToStage<FoModel3D>(CurrentModel);
        return CurrentModel;
    }

    public (int j, FoShape3D shape) GetSpacialBox(string name, int i, string section)
    {

        var root = new FoShape3D(name)
        {
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, 0),
            }
        }.CreateBoundary(name, 10, 10, 10);


        var outerBox = new SpacialBox3D(10, 10, 10, "cm");
        var innerBox = new SpacialBox3D(10.5, 8, 8, "cm");

        var leftFace = innerBox.FaceCenters.FirstOrDefault(p => p.Name.Matches("left"));
        var rightFace = innerBox.FaceCenters.FirstOrDefault(p => p.Name.Matches("right"));


        var leftList = new List<Point3D>(innerBox.LeftFace) { leftFace };
        i = TagVertex(root, i, Text3DAlign.Left, leftList, (p,i) => $"{section}L{i}");

        var rightlist = new List<Point3D>(innerBox.RightFace) { rightFace };
        i = TagVertex(root, i, Text3DAlign.Right, rightlist, (p,i) => $"{section}R{i}");

        GlobalPipe = DrawPipe(root, "leftedge", "red", innerBox.LeftFace);
        DrawPipe(root, "rightedge", "green", innerBox.RightFace);

        DrawFace(root, "Left", outerBox.LeftFaceMesh(0.1, "blue"));
        DrawFace(root, "Right", outerBox.RightFaceMesh(0.1, "yellow"));

        return (i,root);
    }

    private static FoGlyph3D DrawFace(FoShape3D root, string name, Mesh3D face)
    {
        var shape = new FoGlyph3D(name);
        shape.SetValue3D(face);
        root.AddSubGlyph3D(shape);
        return shape;
    }

    private static FoPipe3D DrawPipe(FoShape3D root, string name,  string color, List<Point3D> points)
    {
        var path = points.Select(p => p.AsVector3()).ToList();
        var pipe = new FoPipe3D(name)
        {
            Color = color,
            Closed = true,
        };
        pipe.CreateTube(name, 0.1, path);

        root.AddSubGlyph3D(pipe);
        return pipe;
    }

    private int TagVertex(FoShape3D root, int i, Text3DAlign align, List<Point3D> list, Func<Point3D, int, string> tag)
    {
        int start = i;
        foreach (var item in list)
        {
            var name = tag(item, start++);
            var sphere = new FoShape3D("Thermocouple")
            {
                Transform = new Transform3()
                {
                    Position = item.AsVector3(),
                }
            }.CreateBox(name, 1.5, .5, .5);

            var label = CreateTextLabel3D(sphere, "tag",  name, align, 1.5);
            Tags.Add(name, (sphere, label));

            root.AddSubGlyph3D(sphere);
        }

        return start;
    }
}