using System.Diagnostics.CodeAnalysis;
using FoundryMicroCore.Core.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Materials;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Geometires;

namespace Three2025.Apprentice;

public interface ICuckooClockTech : ITechnician
{
    FoShape3D CreateCuckooClockHousing(int timeMultiplier = 1, string tRexUrl = "");
    void StartAutomaticCuckoo(string stageName);
    void TriggerCuckooSequence();
    void SetAnimationRunning(bool running);
}

public class CuckooClockTech : ICuckooClockTech
{
    public IFoundryService FoundryService { get; init; }
    
    private string _stageName = null!;
    private FoShape3D _clockHousing = null!;
    private FoModel3D _tRexModel = null!;
    private bool _animationRunning = true;
    private int _timeMultiplier = 1;
    
    // Clock hand references for animation
    private FoShape3D _minutePost = null!;
    private FoShape3D _hourPost = null!;
    
    // Door references for Phase 4
    private FoShape3D _leftDoor = null!;
    private FoShape3D _rightDoor = null!;
    
    // Time display for debugging
    private FoText3D _timeDisplay = null!;
    
    // Animation state tracking
    private enum CuckooState
    {
        Idle,
        DoorOpening,
        TRexEmerging,
        Cuckooing,
        TRexRetreating,
        DoorClosing
    }
    
    private CuckooState _currentState = CuckooState.Idle;
    private int _sequenceStartTick = 0;
    private int _roarCount = 3; // Number of times to "cuckoo"
    private int _roarsCompleted = 0;
    private int _lastHourTriggered = -1; // Track last hour to avoid multiple triggers

    public CuckooClockTech(IFoundryService foundry)
    {
        FoundryService = foundry;
    }

    /// <summary>
    /// Phase 2: Creates the static clock housing structure
    /// </summary>
    public FoShape3D CreateCuckooClockHousing(int timeMultiplier = 1, string tRexUrl = "")
    {
        $"CuckooClockTech: Creating cuckoo clock housing with {timeMultiplier}x time speed".WriteInfo();
        
        _timeMultiplier = timeMultiplier; // Store for animation function
        
        // Main housing container
        var housing = new FoShape3D("CuckooClockHousing", "#8B4513") // Brown/wood color
        {
            Transform = new Transform3("HousingTransform")
            {
                Position = new Vector3(0, 0, 0), // Position at floor level
                Pivot = new Vector3(0, -4, 0), // Pivot at bottom of housing (housing is 8 tall, so -4 from center)
            }
        }.CreateBoundary("CuckooClock", 5, 8, 3);
        
        // Main housing box (5 wide x 8 tall x 3 deep)
        var mainBox = new FoShape3D("MainBox", "#8B4513")
        {
            Transform = new Transform3("MainBoxTransform")
            {
                Position = new Vector3(0, 0, 0),
            }
        }.CreateBox("HousingBox", 5, 8, 3);
        housing.AddShape(mainBox);
        
        // Peaked roof (two triangular sides forming a peak)
        var roof = new FoShape3D("Roof", "#8B0000") // Dark red
        {
            Transform = new Transform3("RoofTransform")
            {
                Position = new Vector3(0, 4.5, 0), // Top of housing
            }
        }.CreateBox("RoofBox", 6, 1.5, 4); // Slightly larger than housing for overhang
        housing.AddShape(roof);
        
        // Left door (positioned at front, top area) - Phase 4: Store reference
        _leftDoor = new FoShape3D("LeftDoor", "#A0522D") // Sienna/lighter wood
        {
            Transform = new Transform3("LeftDoorTransform")
            {
                Position = new Vector3(-0.8, 2, 1.51), // Slightly in front of housing
                Pivot = new Vector3(-0.75, 0, 0), // Pivot on left edge for hinge
            }
        }.CreateBox("LeftDoorBox", 1.5, 2, 0.1);
        housing.AddShape(_leftDoor);
        
        // Right door - Phase 4: Store reference
        _rightDoor = new FoShape3D("RightDoor", "#A0522D")
        {
            Transform = new Transform3("RightDoorTransform")
            {
                Position = new Vector3(0.8, 2, 1.51),
                Pivot = new Vector3(0.75, 0, 0), // Pivot on right edge for hinge
            }
        }.CreateBox("RightDoorBox", 1.5, 2, 0.1);
        housing.AddShape(_rightDoor);
        
        // Small clock face on front (below doors)
        var clockFaceRadius = 1.2;
        var clockFace = new FoShape3D("ClockFace", "white")
        {
            Transform = new Transform3("ClockFaceTransform")
            {
                Position = new Vector3(0, 0, 1.6), // Below doors, on front face
                Rotation = new Euler(0, 0, 0),
            }
        }.CreateCylinder("ClockFaceCylinder", clockFaceRadius * 2, 0.1, clockFaceRadius * 2);
        housing.AddShape(clockFace);
        
        // Clock numbers (simplified - just 12, 3, 6, 9)
        var numberPositions = new[] { 
            (12, 0.0, 1.0), 
            (3, 1.0, 0.0), 
            (6, 0.0, -1.0), 
            (9, -1.0, 0.0) 
        };
        
        foreach (var (number, xOffset, yOffset) in numberPositions)
        {
            var numberText = new FoText3D($"Number{number}", "black")
            {
                Text = number.ToString(),
                FontSize = 0.3,
                Transform = new Transform3($"Number{number}Transform")
                {
                    Position = new Vector3(xOffset, yOffset, 0.1),
                }
            };
            clockFace.AddShape(numberText);
        }
        
        // Time display for debugging (below center)
        _timeDisplay = new FoText3D("TimeDisplay", "yellow")
        {
            Text = "00:00",
            FontSize = 0.4,
            Transform = new Transform3("TimeDisplayTransform")
            {
                Position = new Vector3(0, -0.8, 0.11),
            }
        };
        clockFace.AddShape(_timeDisplay);
        
        // Center post for minute hand - hands are children so they rotate with the post
        _minutePost = new FoShape3D("MinutePost", "darkgray")
        {
            Transform = new Transform3("MinutePostTransform")
            {
                Position = new Vector3(0, 0, 0.14), // Center of clock face
                Rotation = new Euler(0, 0, 0),
            }
        }.CreateBox("MinutePostBox", 0.08, 0.08, 0.05); // Small center post
        clockFace.AddShape(_minutePost);
        
        // Minute hand extends from center
        var minuteHand = new FoShape3D("MinuteHand", "red")
        {
            Transform = new Transform3("MinuteHandTransform")
            {
                Position = new Vector3(0, 0.5, 0.01), // Half the hand length offset
            }
        }.CreateBox("MinuteHandBox", 0.1, 1.0, 0.05);
        _minutePost.AddShape(minuteHand);
        
        // Center post for hour hand (slightly in front)
        _hourPost = new FoShape3D("HourPost", "darkgray")
        {
            Transform = new Transform3("HourPostTransform")
            {
                Position = new Vector3(0, 0, 0.15),
                Rotation = new Euler(0, 0, 0),
            }
        }.CreateBox("HourPostBox", 0.1, 0.1, 0.05);
        clockFace.AddShape(_hourPost);
        
        // Hour hand extends from center
        var hourHand = new FoShape3D("HourHand", "blue")
        {
            Transform = new Transform3("HourHandTransform")
            {
                Position = new Vector3(0, 0.35, 0.01), // Half the hand length offset
            }
        }.CreateBox("HourHandBox", 0.15, 0.7, 0.05);
        _hourPost.AddShape(hourHand);
        
        // Phase 3: Add T-Rex model inside housing (hidden behind doors) - only if URL provided
        if (!string.IsNullOrEmpty(tRexUrl))
        {
            // Create model - technician creates the shape object but relies on arena/stage for persistence
            _tRexModel = new FoModel3D("TRexCuckoo")
            {
                Url = tRexUrl,
                Transform = new Transform3("TRexTransform")
                {
                    Position = new Vector3(0, -2, -0.5), // Inside housing, behind doors, below door level
                    Rotation = new Euler(0, Math.PI, 0), // Face outward (toward doors)
                    Scale = new Vector3(0.3, 0.3, 0.3), // Scale down to fit inside housing
                }
            };
            housing.AddShape(_tRexModel);
            $"CuckooClockTech: T-Rex model added inside housing (hidden)".WriteInfo();
        }
        
        // Set up animation callback using separate function (easier to swap/disable)
        housing.OnBeforeRender(UpdateClockAnimation);
        
        $"CuckooClockTech: Housing created successfully".WriteSuccess();
        
        _clockHousing = housing;
        return housing;
    }
    
    /// <summary>
    /// Animation function for clock hands and cuckoo sequence - separate for easier management
    /// </summary>
    private void UpdateClockAnimation(FoGlyph3D self, int tick, double fps)
    {
        // Check if animation is running
        if (!_animationRunning) return;
        
        // Only update once per second
        if (tick % (int)fps != 0) return;
        
        // Use accelerated time
        var baseTime = DateTime.Now;
        var totalMinutes = (baseTime.Hour * 60 + baseTime.Minute) * _timeMultiplier;
        var acceleratedHour = (totalMinutes / 60) % 12;
        var acceleratedMinute = totalMinutes % 60;
        
        // Update time display
        _timeDisplay.Text = $"{acceleratedHour:00}:{acceleratedMinute:00}";
        
        // Rotate minute post (hand rotates with it)
        var minuteAngle = acceleratedMinute * (Math.PI / 30.0);
        _minutePost.Transform.RotateTo(0, 0, -minuteAngle, AngleUnit.Radians);
        
        // Rotate hour post (hand rotates with it)
        var hourAngle = acceleratedHour * (Math.PI / 6.0) + acceleratedMinute * (Math.PI / 360.0);
        _hourPost.Transform.RotateTo(0, 0, -hourAngle, AngleUnit.Radians);
        
        // Phase 7: Auto-trigger cuckoo sequence on the hour
        if (_currentState == CuckooState.Idle && acceleratedMinute == 0 && acceleratedHour != _lastHourTriggered)
        {
            _lastHourTriggered = acceleratedHour;
            _roarCount = acceleratedHour == 0 ? 12 : acceleratedHour; // 12 roars at midnight/noon
            StartCuckooSequence(tick);
        }
        
        // Phase 7: State machine for cuckoo sequence
        if (_currentState != CuckooState.Idle)
        {
            UpdateCuckooSequence(tick, fps);
        }
    }
    
    /// <summary>
    /// Phase 7: Update the cuckoo sequence state machine
    /// </summary>
    private void UpdateCuckooSequence(int tick, double fps)
    {
        var elapsed = tick - _sequenceStartTick;
        var elapsedSeconds = elapsed / fps;
        
        switch (_currentState)
        {
            case CuckooState.DoorOpening:
                // Phase 4: Animate doors opening (0-2 seconds)
                if (elapsedSeconds < 2)
                {
                    var progress = elapsedSeconds / 2.0;
                    var angle = progress * Math.PI / 2; // 90 degrees
                    _leftDoor.Transform.RotateTo(0, -angle, 0, AngleUnit.Radians);
                    _rightDoor.Transform.RotateTo(0, angle, 0, AngleUnit.Radians);
                }
                else
                {
                    _currentState = CuckooState.TRexEmerging;
                    _sequenceStartTick = tick;
                }
                break;
                
            case CuckooState.TRexEmerging:
                // Phase 5: T-Rex moves forward (0-1.5 seconds)
                if (_tRexModel != null)
                {
                    if (elapsedSeconds < 1.5)
                    {
                        var progress = elapsedSeconds / 1.5;
                        var zPos = -0.5 + progress * 2.5; // Move from -0.5 to 2.0
                        _tRexModel.Transform.Position = new Vector3(0, -2, zPos);
                    }
                    else
                    {
                        _currentState = CuckooState.Cuckooing;
                        _sequenceStartTick = tick;
                        _roarsCompleted = 0;
                    }
                }
                else
                {
                    // Skip to next state if no T-Rex
                    _currentState = CuckooState.TRexRetreating;
                    _sequenceStartTick = tick;
                }
                break;
                
            case CuckooState.Cuckooing:
                // Phase 6: T-Rex roars (0.5 seconds per roar)
                var roarProgress = elapsedSeconds % 0.5; // Each roar is 0.5 seconds
                var currentRoar = (int)(elapsedSeconds / 0.5);
                
                if (_tRexModel != null && currentRoar < _roarCount)
                {
                    // Pulse scale for roar effect
                    var scalePulse = 1.0 + Math.Sin(roarProgress * Math.PI * 4) * 0.1;
                    _tRexModel.Transform.Scale = new Vector3(0.3 * scalePulse, 0.3 * scalePulse, 0.3 * scalePulse);
                    _roarsCompleted = currentRoar + 1;
                }
                else if (currentRoar >= _roarCount)
                {
                    // Reset scale and move to retreat
                    if (_tRexModel != null)
                    {
                        _tRexModel.Transform.Scale = new Vector3(0.3, 0.3, 0.3);
                    }
                    _currentState = CuckooState.TRexRetreating;
                    _sequenceStartTick = tick;
                }
                break;
                
            case CuckooState.TRexRetreating:
                // Phase 5: T-Rex moves back (0-1.5 seconds)
                if (_tRexModel != null)
                {
                    if (elapsedSeconds < 1.5)
                    {
                        var progress = elapsedSeconds / 1.5;
                        var zPos = 2.0 - progress * 2.5; // Move from 2.0 back to -0.5
                        _tRexModel.Transform.Position = new Vector3(0, -2, zPos);
                    }
                    else
                    {
                        _currentState = CuckooState.DoorClosing;
                        _sequenceStartTick = tick;
                    }
                }
                else
                {
                    _currentState = CuckooState.DoorClosing;
                    _sequenceStartTick = tick;
                }
                break;
                
            case CuckooState.DoorClosing:
                // Phase 4: Animate doors closing (0-2 seconds)
                if (elapsedSeconds < 2)
                {
                    var progress = elapsedSeconds / 2.0;
                    var angle = (1 - progress) * Math.PI / 2; // 90 to 0 degrees
                    _leftDoor.Transform.RotateTo(0, -angle, 0, AngleUnit.Radians);
                    _rightDoor.Transform.RotateTo(0, angle, 0, AngleUnit.Radians);
                }
                else
                {
                    // Sequence complete
                    _currentState = CuckooState.Idle;
                    $"CuckooClockTech: Cuckoo sequence complete ({_roarCount} roars)".WriteSuccess();
                }
                break;
        }
    }
    
    /// <summary>
    /// Phase 7: Internal method to start the cuckoo sequence
    /// </summary>
    private void StartCuckooSequence(int tick)
    {
        _currentState = CuckooState.DoorOpening;
        _sequenceStartTick = tick;
        $"CuckooClockTech: Starting cuckoo sequence with {_roarCount} roars".WriteInfo();
    }

    /// <summary>
    /// Phase 7: Automatic cuckoo is now integrated into UpdateClockAnimation
    /// This method kept for interface compatibility but does nothing
    /// </summary>
    public void StartAutomaticCuckoo(string stageName)
    {
        _stageName = stageName;
        $"CuckooClockTech: Automatic cuckoo is always active when animation is running".WriteInfo();
    }

    /// <summary>
    /// Manual trigger for testing - uses current hour for roar count
    /// </summary>
    public void TriggerCuckooSequence()
    {
        if (_currentState != CuckooState.Idle)
        {
            $"CuckooClockTech: Sequence already in progress, ignoring trigger".WriteWarning();
            return;
        }
        
        // Use current accelerated hour for roar count
        var baseTime = DateTime.Now;
        var totalMinutes = (baseTime.Hour * 60 + baseTime.Minute) * _timeMultiplier;
        var acceleratedHour = (totalMinutes / 60) % 12;
        _roarCount = acceleratedHour == 0 ? 12 : acceleratedHour;
        
        // Start from tick 0 (will use elapsed time from this point)
        StartCuckooSequence(0);
    }

    /// <summary>
    /// Control animation on/off
    /// </summary>
    public void SetAnimationRunning(bool running)
    {
        _animationRunning = running;
    }
}
