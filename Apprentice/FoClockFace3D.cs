using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD.Maths;

namespace Three2025.Apprentice;

public class FoClockFace3D : FoShape3D
{
    public double Radius { get; set; } = 12.0;
    public new double Height { get; set; } = 0.2;
    public double FontSize { get; set; } = 1.2;
    
    private FoText3D _timeText = null!;
    private FoShape3D _centerPost = null!;
    private FoShape3D _secondHand = null!;
    
    public FoClockFace3D(string name = "ClockFace") : base(name, "Blue")
    {
       this.BuildClock(); 
    }
    
    protected FoClockFace3D BuildClock()
    {
        var diameter = 2 * Radius;
        
        // Create base cylinder
        this.CreateCylinder("ClockFaceBase", diameter, Height, diameter);
        
        // Add numbers as children
        for (int i = 1; i <= 12; i++)
        {
            var letter = $"{i}";
            var angle = i * (2 * Math.PI / 12) - Math.PI / 2;
            AddNumberText(angle, letter);
        }
        
        // Add time display text
        _timeText = new FoText3D("TimeText", "white")
        {
            Text = "Current Time",
            FontSize = 5.0,
            Transform = new Transform3("TimeTextTransform")
            {
                Position = new Vector3(0, 2, 0),
            }
        };
        this.AddSubGlyph3D(_timeText);
        
        // Add center post
        _centerPost = new FoShape3D("Post", "red")
            .CreateBox("PostBox", 0.2, 1.0, 0.2);
        this.AddSubGlyph3D(_centerPost);
        
        // Add second hand
        _secondHand = new FoShape3D("Hand", "green")
        {
            Transform = new Transform3("HandTransform")
            {
                Position = new Vector3(0.5 * Radius, 1, 0),
            }
        }.CreateBox("HandBox", 1.2 * Radius, 2.0, 0.1);
        _centerPost.AddSubGlyph3D(_secondHand);
        
        // Set up animation to update every second
        BeforeAnimationRefresh(UpdateClockAnimation);
        
        return this;
    }
    
    private void AddNumberText(double angle, string text)
    {
        var x = (Radius - 1.0) * Math.Cos(angle);
        var y = Height + 1.0;
        var z = (Radius - 1.0) * Math.Sin(angle);
        
        var number = new FoText3D(text, "white")
        {
            Text = text,
            FontSize = FontSize,
            Transform = new Transform3($"Number{text}Transform")
            {
                Position = new Vector3(x, y, z),
            }
        };
        this.AddSubGlyph3D(number);
    }
    
    private void UpdateClockAnimation(FoGlyph3D self, int tick, double fps)
    {
        // Update every 60 frames (approximately once per second at 60fps)
        if (tick % 60 != 0) return;

        $"refresh floor clock".WriteInfo();
        
        var time = DateTime.Now;
        var angle = time.Second * (2 * Math.PI / 60) - Math.PI / 2;
        var radius = 10.0;
        var x = radius * Math.Cos(angle);
        var y = 2;
        var z = radius * Math.Sin(angle);
        
        // Update time text
        if (_timeText != null)
        {
            var currentTime = time.ToString("HH:mm:ss");
            _timeText.Text = currentTime;
            _timeText.Transform.Position = new Vector3(x, y, z);
        }
        
        // Rotate center post (and attached second hand)
        if (_centerPost != null)
        {
            _centerPost.Transform.RotateTo(0, -angle, 0, AngleUnit.Radians);
        }
        
        // Rotate the entire clock face slightly
        var rot = this.Transform.Rotation;
        var deltaX = Math.PI / 2 - rot.X;
        var deltaZ = angle - rot.Z;
        this.Transform.RotateBy(deltaX, 0, deltaZ, AngleUnit.Radians);
    }
}
