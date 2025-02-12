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
using FoundryRulesAndUnits.Units;
using Three2025.Apprentice;




public interface ICageTech : ITechnician
{
    FoShape3D CreateCageForRack(string name);
    (int j, FoShape3D shape) GetSpacialBox(string name, int i, string section);
}

public class CageTech : ICageTech
{
    protected IWorkspace Workspace { get; init; }
    protected IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();



    public CageTech(IWorkspace space, IFoundryService foundry)
    {
        Workspace = space;
        FoundryService = foundry;
    }

    public FoShape3D CreateCageForRack(string name)
    {
        var arena = Workspace.GetArena();
        var cage = new FoShape3D(name, "Blue")
        {
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, 0),
            }
        };
        cage.CreateBox(name, 1, 5, 1);
        arena.AddShapeToStage<FoShape3D>(cage);
        return cage;
    }

    public bool ComputeHitBoundaries(Action OnComplete)
    {
        var arena = FoundryService.Arena();
        var (success, scene) = arena.CurrentScene();

        if (!success) return false;
        scene.UpdateHitBoundaries(OnComplete);
        return true;
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
        var rightlist = new List<Point3D>(innerBox.RightFace) { rightFace };


        DrawPipe(root, "leftedge", "red", innerBox.LeftFace);
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

    public void GenerateCage(double x, double z, Length Width, Length Height, Length Depth, Length Step)
    {

        var w = Width.Value();
        var d = Depth.Value();

        var (_,lf) = GenerateColumn("Left-Front", x, z, Height, Step);
        var (_,lb) = GenerateColumn("Left-Back", x, z+d, Height, Step);
        LinkColumns($"Left-Side", lf, lb);

        var (_, cb) = GenerateColumn("Center-Back", x +w/2, z+d, Height, Step);
        LinkColumns($"Left-Back-Side", lb, cb);

        var (_, rb) = GenerateColumn("Right-Back", x +w, z+d, Height, Step);
        LinkColumns($"Right-Back-Side", cb, rb);

        var (_,rf) = GenerateColumn("Right-Front", x +w, z, Height, Step);
        LinkColumns($"Right-Side", rb, rf);
    }

    
    public FoGroup3D LinkColumns(string name, List<Node3D> start, List<Node3D> finish)
    {
        var root = new FoGroup3D(name);

        for (int i = 0; i < start.Count(); i++)
        {
            var s = start[i];
            var f = finish[i];
            var link = new Link3D($"{name}:{i}", "blue", s, f, 0.01);
            root.AddShape<FoShape3D>(link);
        }
        return root;
    }

    public (FoGroup3D group, List<Node3D> nodes) GenerateColumn(string groupName, double x, double z, Length Height, Length Step)
    {

        var h = Height.Value();
        var s = Step.Value();

        var columns = new List<Node3D>();
        var y = 0.0;

        var root = new FoGroup3D(groupName);

        while (y <= h)
        {
            var name = $"{groupName}:{x:F1}:{y:F1}:{z:F1}";
            var shape = new Node3D(name, "blue")
            {
                Transform = new Transform3()
                {
                    Position = new Vector3(x, y, z),
                }
            };
            shape.CreateBox(name, .05, .03, .05);
            columns.Add(shape);
            root.AddShape<FoShape3D>(shape);
            y += s;
        }

        for (int i = 1; i < columns.Count; i++)
        {
            var start = columns[i - 1];
            var finish = columns[i];
            var link = new Link3D($"Link::{x:F1}:{z:F1}{i}", "blue", start, finish, 0.01);
            root.AddShape<FoShape3D>(link);
        }
        return (root, columns);
    }

    public void GenerateGeometry()
    {
        var width = new Length(1, "m");
        var height = new Length(2.6, "m");
        var depth = new Length(.8, "m");
        var step = new Length(.2, "m");

        var (x, z) = (5.0, 5.0);
        GenerateCage(x, z, width, height, depth, step);
        // x += width.Value() + 0.05;
        // GenerateCage(x, z, width, height, depth, step);
        // x += width.Value() + 0.05;
        // GenerateCage(x, z, width, height, depth, step);
        // x += width.Value() + 0.05;
        // GenerateCage(x, z, width, height, depth, step);
    }



}