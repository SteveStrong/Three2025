

using BlazorThreeJS.Core;
using BlazorThreeJS.Geometires;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Objects;
using FoundryBlazor.Shape;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

// public class FoRack : FoShape3D
// {
//     public FoRack(string name) : base(name)
//     {
//     }

//     public FoRack(string name, string color) : base(name, color)
//     {
//     }
// }

public interface IClockTech : ITechnician
{
    void UpdateTextWithCurrentTime(object state);
    Mesh3D CreateClockFaceMesh();
}

public class ClockTech : IClockTech
{
    public IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();


    private Text3D GlobalText = null!;

    private Mesh3D CenterPost = null!;

    public ClockTech(IFoundryService foundry)
    {
        FoundryService = foundry;
    }

    public bool ComputeHitBoundaries(Action OnComplete)
    {
        var arena = FoundryService.Arena();
        var (success, scene) = arena.CurrentScene();

        if (!success) return false;
        scene.UpdateHitBoundaries(OnComplete);
        return true;
    } 


   public void UpdateTextWithCurrentTime(object state)
    {
        var arena = FoundryService.Arena();
        var (found, scene) = arena.CurrentScene();
        if (!found) return;

        var time = DateTime.Now;
        var angle = time.Second * (2 * Math.PI / 60) - Math.PI / 2; // Convert seconds to radians
        var radius = 10.0;
        var x = radius * Math.Cos(angle);
        var y = 2;
        var z = radius * Math.Sin(angle);

        var currentTime = time.ToString("HH:mm:ss");
        //var x = DataGenerator.GenerateDouble(-1, 1);
        //var z = DataGenerator.GenerateDouble(-1, 1);

        if (GlobalText != null)
        {
            GlobalText.Text = currentTime;
            GlobalText.Transform.Position = new Vector3(x, y, z);
            GlobalText.SetDirty(true);

            CenterPost.Transform.Rotation = new Euler(0, -angle, 0);
            CenterPost.SetDirty(true);
        }
        else 
        {
            GlobalText = new Text3D()
            {
                Uuid = Guid.NewGuid().ToString(),
                Text = currentTime,
                Color = DataGenerator.GenerateColor(),
                FontSize = 3.0,
                Transform = new Transform3()
                {
                    Position = new Vector3(x, y, z),
                },
            };
            CenterPost = new Mesh3D
            {
                Uuid = Guid.NewGuid().ToString(),
                Name = "CenterPost",
                Geometry = new BoxGeometry(width: 0.5, depth: 0.5, height: 2.5),
                Transform = new Transform3()
                {
                    Position = new Vector3(0, 0, 0),
                    Rotation = new Euler(0, -angle, 0),
                },
                Material = new MeshStandardMaterial("red")
            };
            var secondHand = new Mesh3D
            {
                Uuid = Guid.NewGuid().ToString(),
                Name = "Second Hand",
                Geometry = new BoxGeometry(width: 1.2 * radius, depth: 0.1, height: 2),
                Transform = new Transform3()
                {
                    Position = new Vector3(0.5 * radius, 1, 0),
                    Rotation = new Euler(0, 0, 0),
                },
                Material = new MeshStandardMaterial("green")
            };
            CenterPost.AddChild(secondHand);
            
            scene.AddChild(GlobalText);
            scene.AddChild(CenterPost);
        }

    }

    public void PlaceTextAtPosition(Object3D parent, double angle, double radius, double height, double size,  string text)
    {
        var arena = FoundryService.Arena();
        var (found, scene) = arena.CurrentScene();
        if (!found) return;

        var x = radius * Math.Cos(angle);
        var y = height;
        var z = radius * Math.Sin(angle);

        var letter = new Text3D()
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = text,
            Text = text,
            Color = "white",
            FontSize = size,
            Transform = new Transform3()
            {
                Position = new Vector3(x, y, z),
            },
        };

        parent.AddChild(letter);     

    }

    public Mesh3D CreateClockFaceMesh()
    {


        var radius = 10.0f;
        var height = 0.1f;

        var mesh = new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Name = "Clock Face",
            Geometry = new CylinderGeometry(radiusTop: radius-1.0, radiusBottom: radius, height: height,  radialSegments: 36),
            Transform = new Transform3()
            {
                Position = new Vector3(0, 0, 0),
            },
            Material = new MeshStandardMaterial("blue")
        };



        for (int i = 1; i <= 12; i++)
        {
            var letter = $"{i}";
            var angle = i * (2 * Math.PI / 12) - Math.PI / 2;

            PlaceTextAtPosition(mesh, angle, radius-1.0, height + 1.0, 1.2, letter);
        }

        return mesh;

    }



}