

using System.Diagnostics.CodeAnalysis;

using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;

using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Materials;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Geometires;

namespace Three2025.Apprentice;

public interface IClockTech : ITechnician
{
    FoShape3D CreateClockOnArena();
    void RunClock();
}

public class ClockTech : IClockTech
{
    public IFoundryService FoundryService { get; init; }
    protected MockDataGenerator DataGenerator { get; set; } = new();

    private Timer _timer = null!;

    private Text3D GlobalText = null!;

    private Mesh3D CenterPost = null!;

    private FoShape3D Clock = null!;

    public ClockTech(IFoundryService foundry)
    {
        FoundryService = foundry;
    }



    public void UpdateClock(object state)
    {
        UpdateArenaClock(state);
        //UpdateClock(state);
    }



    public void RunClock()
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



    public FoText3D LetterText3D(FoShape3D parent, double angle, double radius, double height, double size,  string text)
    {
        var x = radius * Math.Cos(angle);
        var y = height;
        var z = radius * Math.Sin(angle);

        var letter = new FoText3D()
        {
            Name = text,
            Text = text,
            Color = "white",
            FontSize = size,
            Transform = new Transform3("Letter"+text)
            {
                Position = new Vector3(x, y, z),
            },
        };

        parent.AddSubGlyph3D(letter);
        return letter;
    }

    public FoShape3D CreateClockOnArena()
    {
        var radius = 12.0f;
        var height = 0.6;
        var fontSize = 1.2;
        var diameter = 2 * radius;


        var clock = new FoShape3D("ArenaClock", "Red")
        {
            Transform = new Transform3("ClockTransform")
            {
                Position = new Vector3(0, 1.2 * radius, 0),
                Rotation = new Euler(Math.PI / 2, 0, 0),
            }
        };
        clock.CreateCylinder("Clock", diameter, height, diameter);



        //now add all the numbers as children
        for (int i = 1; i <= 12; i++)
        {
            var letter = $"{i}";
            var angle = i * (2 * Math.PI / 12) - Math.PI / 2;
            LetterText3D(clock, angle, radius - 1.0, height, fontSize, letter);
        }

        //now lets add the center post
        var centerPost = new FoShape3D("Post", "red")
        {
            // Transform = new Transform3("PostTransform")
            // {
            //     Position = new Vector3(0, 0, 0),
            //     Rotation = new Euler(0, 0, 0),
            // }
        }.CreateBox("Post", 0.2, 1.0, .2);

        clock.AddSubGlyph3D(centerPost);

        //now lets add the secondHand
        var secondHand = new FoShape3D("Hand", "green")
        {
            Transform = new Transform3("HandTransform")
            {
                Position = new Vector3(0.6 * radius, 1, 0),
            }
        }.CreateBox("Hand", 1.2 * radius, 2.0, .1);

        centerPost.AddSubGlyph3D(secondHand);

        //now lets add the time text at the end of the hand
        var timeText = new FoText3D("TimeText", "white")
        {
            Text = "Ready",
            FontSize = 1.5,
            Transform = new Transform3("TimeTextTransform")
            {
                Position = new Vector3(0.6 * radius, 0, 0),
            }
        };
        secondHand.AddSubGlyph3D(timeText);

        return clock;
    }

    public void UpdateArenaClock(object state)
    {

        var time = DateTime.Now;
        var angle = time.Second * (2 * Math.PI / 60) - Math.PI / 2; // Convert seconds to radians
        var radius = 10.0;
        var x = radius * Math.Cos(angle);
        var y = 2;
        var z = radius * Math.Sin(angle);

  
        if (Clock != null)
        {
            //RunClockOnArena(); //stop the clock for debugging

            var post = Clock.FindSubGlyph3D<FoShape3D>("Post");

            if (post != null)
            {
                //$"Rotating Post {post.Name} by {angle} radians".WriteInfo();
                var xxx = post.Transform.RotateTo(0, -angle, 0, AngleUnit.Radians);
                //$"Post {post.Name} {angle} new rotation is {xxx.X}, {xxx.Y}, {xxx.Z}".WriteInfo();

            }

            // Find the time text (now a child of the hand)
            var hand = post.FindSubGlyph3D<FoShape3D>("Hand");
            if (hand != null)
            {
                var timeText = hand.FindSubGlyph3D<FoText3D>("TimeText");
                if (timeText != null)
                {
                    var currentTime = time.ToString("HH:mm:ss");
                    timeText.Text = currentTime;
                }
            }

            // Rotate the entire clock face around global Y to keep numbers facing the hand
            Clock.Transform.RotateTo(Math.PI / 2, 0, angle, AngleUnit.Radians);

        }
        else
        {
            Clock = CreateClockOnArena();
            var arena = FoundryService.Arena();
            var stage = arena.CurrentStage();
            arena.AddShapeToStage<FoShape3D>(Clock, stage.GetName());
        }

    }


   public void UpdateSceneClock(object state)
    {
        // Get scene from the Clock shape's stage
        var scene = Clock?.ParentStage?.GetAssociatedScene();
        if (scene == null) return;

        var time = DateTime.Now;
        var angle = time.Second * (2 * Math.PI / 60) - Math.PI / 2; // Convert seconds to radians
        var radius = 18.0;
        var x = radius * Math.Cos(angle);
        var y = 2;
        var z = radius * Math.Sin(angle);

        var currentTime = time.ToString("HH:mm:ss");


        if (GlobalText != null)
        {
            GlobalText.Text = currentTime;
            var dx = x - GlobalText.Transform.Position.X;
            var dy = y - GlobalText.Transform.Position.Y;
            var dz = z - GlobalText.Transform.Position.Z;
            GlobalText.Transform.MoveBy(dx, dy, dz);

            var deltaY = -angle - CenterPost.Transform.Rotation.Y;
            CenterPost.Transform.RotateBy(0, deltaY, 0, AngleUnit.Radians);
        }
        else 
        {
            GlobalText = new Text3D()
            {
                Text = currentTime,
                Color = DataGenerator.GenerateColor(),
                FontSize = 3.0,
                Transform = new Transform3("GlobalTextTransform")
                {
                    Position = new Vector3(x, y, z),
                },
            };
            CenterPost = new Mesh3D
            {
                Name = "CenterPost",
                Geometry = new BoxGeometry(width: 0.5, depth: 0.5, height: 2.5),
                Transform = new Transform3("CenterPostTransform")
                {
                    Position = new Vector3(0, 0, 0),
                    Rotation = new Euler(0, -angle, 0),
                },
                Material = new MeshStandardMaterial("red", .5)
            };
            var secondHand = new Mesh3D
            {
                Name = "Second Hand",
                Geometry = new BoxGeometry(width: 1.2 * radius, depth: 0.1, height: 2),
                Transform = new Transform3("SecondHandTransform")
                {
                    Position = new Vector3(0.5 * radius, 1, 0),
                    Rotation = new Euler(0, 0, 0),
                },
                Material = new MeshStandardMaterial("green", .5)
            };
            CenterPost.AddChild(secondHand);
            
            scene.AddChild(GlobalText);
            scene.AddChild(CenterPost);
        }
    }


}