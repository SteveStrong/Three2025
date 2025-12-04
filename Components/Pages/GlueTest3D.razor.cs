using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.ThreeD;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using BlazorComponentBus;
using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Core;

namespace Three2025.Components.Pages;

public partial class GlueTest3DBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }

    public Canvas3DComponent Canvas3DReference = null;

    [Parameter] public int CanvasWidth { get; set; } = 800;
    [Parameter] public int CanvasHeight { get; set; } = 600;

    // Test objects
    private FoShape3D _baseBox;    // Red box at bottom
    private FoShape3D _middleBox;  // Green box glued to red
    private FoShape3D _topBox;     // Blue box glued to green
    
    // Glue objects - track them separately
    private FoGlue3D _glue1;
    private FoGlue3D _glue2;
    
    // Dynamic pipes that update as objects move
    private FoGluePipe3D _pipe1;
    private FoGluePipe3D _pipe2;

    // Animation state
    protected double _animationTime = 0;
    private const double ANIMATION_DURATION = 10.0; // 10 second circular path
    private const double CIRCLE_RADIUS = 3.0;
    private bool _isAnimating = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        //$"GlueTest3D Page OnInitialized".WriteInfo();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //$"GlueTest3D Page OnAfterRenderAsync".WriteInfo();

            // Wait for Canvas3DComponent to initialize
            await Task.Delay(100);

            // Don't auto-create - let user click button to create boxes
            //$"GlueTest3D: Ready. Click 'Create Boxes' to begin.".WriteInfo();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    // ==================== 3D Glue Test ====================
    
    /// <summary>
    /// Add colored dots to each face to show orientation
    /// Top=Yellow, Bottom=Cyan, Front=Green, Back=Magenta, Left=Blue, Right=Red
    /// </summary>
    private void AddFaceMarkers(FoShape3D box)
    {
        var hw = box.Width / 2;
        var hh = box.Height / 2;
        var hd = box.Depth / 2;
        var markerSize = 0.15;
        
        // Top face - Yellow
        var topMarker = new FoShape3D($"{box.Name}_TopMarker", "yellow");
        topMarker.CreateSphere("TopDot", markerSize, markerSize, markerSize);
        topMarker.Transform = new Transform3($"{box.Name}_TopMarkerTransform")
        {
            Position = new Vector3(0, hh + markerSize, 0)
        };
        box.AddSubGlyph3D(topMarker);
        
        // Bottom face - Cyan  
        var bottomMarker = new FoShape3D($"{box.Name}_BottomMarker", "cyan");
        bottomMarker.CreateSphere("BottomDot", markerSize, markerSize, markerSize);
        bottomMarker.Transform = new Transform3($"{box.Name}_BottomMarkerTransform")
        {
            Position = new Vector3(0, -hh - markerSize, 0)
        };
        box.AddSubGlyph3D(bottomMarker);
        
        // Front face - Green (+Z)
        var frontMarker = new FoShape3D($"{box.Name}_FrontMarker", "lime");
        frontMarker.CreateSphere("FrontDot", markerSize, markerSize, markerSize);
        frontMarker.Transform = new Transform3($"{box.Name}_FrontMarkerTransform")
        {
            Position = new Vector3(0, 0, hd + markerSize)
        };
        box.AddSubGlyph3D(frontMarker);
        
        // Back face - Magenta (-Z)
        var backMarker = new FoShape3D($"{box.Name}_BackMarker", "magenta");
        backMarker.CreateSphere("BackDot", markerSize, markerSize, markerSize);
        backMarker.Transform = new Transform3($"{box.Name}_BackMarkerTransform")
        {
            Position = new Vector3(0, 0, -hd - markerSize)
        };
        box.AddSubGlyph3D(backMarker);
        
        // Left face - Blue (-X)
        var leftMarker = new FoShape3D($"{box.Name}_LeftMarker", "blue");
        leftMarker.CreateSphere("LeftDot", markerSize, markerSize, markerSize);
        leftMarker.Transform = new Transform3($"{box.Name}_LeftMarkerTransform")
        {
            Position = new Vector3(-hw - markerSize, 0, 0)
        };
        box.AddSubGlyph3D(leftMarker);
        
        // Right face - Orange (+X)
        var rightMarker = new FoShape3D($"{box.Name}_RightMarker", "orange");
        rightMarker.CreateSphere("RightDot", markerSize, markerSize, markerSize);
        rightMarker.Transform = new Transform3($"{box.Name}_RightMarkerTransform")
        {
            Position = new Vector3(hw + markerSize, 0, 0)
        };
        box.AddSubGlyph3D(rightMarker);
    }
    
    public void CreateStackedTower()
    {
        if (_baseBox != null)
        {
            $"Boxes already created. Click Reset to start over.".WriteWarning();
            return;
        }
        
        $"Creating boxes on the floor - ready for gluing".WriteInfo();
        
        var arena = Workspace.GetArena();
        if (arena == null)
        {
            $"No arena available".WriteError();
            return;
        }

        var stage = arena.CurrentStage();
        // Clear existing objects and glue
        _animationTime = 0;
        _isAnimating = false;
        _glue1 = null;
        _glue2 = null;

        // Create base box (red) - sits on floor at Y=0
        _baseBox = new FoShape3D($"BaseBox-{Guid.NewGuid().ToString().Substring(0, 8)}", "red")
        {
            Transform = new Transform3("BaseBoxTransform")
            {
                Position = new Vector3(0, 0.5, 0),  // Half-height above floor
            },
        };
        _baseBox.CreateBox("BaseBox", 2.0, 1.0, 2.0);
        arena.AddShapeToStage<FoShape3D>(_baseBox, stage.GetName());
        AddFaceMarkers(_baseBox);

        // Create middle box (green) - also on floor initially
        _middleBox = new FoShape3D($"MiddleBox-{Guid.NewGuid().ToString().Substring(0, 8)}", "green")
        {
            Transform = new Transform3("MiddleBoxTransform")
            {
                Position = new Vector3(-3, 0.5, 0),  // To the left, on floor
            },
        };
        _middleBox.CreateBox("MiddleBox", 1.5, 1.0, 1.5);
        arena.AddShapeToStage<FoShape3D>(_middleBox, stage.GetName());
        AddFaceMarkers(_middleBox);

        // Create top box (blue) - also on floor initially
        _topBox = new FoShape3D($"TopBox-{Guid.NewGuid().ToString().Substring(0, 8)}", "blue")
        {
            Transform = new Transform3("TopBoxTransform")
            {
                Position = new Vector3(3, 0.5, 0),  // To the right, on floor
            },
        };
        _topBox.CreateBox("TopBox", 1.0, 1.0, 1.0);
        arena.AddShapeToStage<FoShape3D>(_topBox, stage.GetName());

        AddFaceMarkers(_topBox);

        $"Three boxes created with face markers:".WriteSuccess();
        $"  Yellow=Top, Cyan=Bottom, Lime=Front, Magenta=Back, Blue=Left, Orange=Right".WriteInfo();
    }
    
    public void GlueGreenToRed()
    {
        if (_baseBox == null || _middleBox == null)
        {
            $"No boxes created - call CreateStackedTower first".WriteError();
            return;
        }
        
        // Toggle: If already glued, unglue it
        if (_glue1 != null)
        {
            $"🔓 Ungluing green from red...".WriteWarning();
            _glue1.UnGlue();
            _glue1 = null;
            $"✅ Green box unglued - now free to move independently".WriteSuccess();
            return;
        }

        // Glue green box to red box's top
        _glue1 = _middleBox.GlueTo(_baseBox, "TopFaceCenter");
        
        $"✅ Green box glued to Red box's top".WriteSuccess();
    }
    
    public void GlueBlueToGreen()
    {
        if (_middleBox == null || _topBox == null)
        {
            $"No boxes created - call CreateStackedTower first".WriteError();
            return;
        }
        
        // Toggle: If already glued, unglue it
        if (_glue2 != null)
        {
            $"🔓 Ungluing blue from green...".WriteWarning();
            _glue2.UnGlue();
            _glue2 = null;
            $"✅ Blue box unglued - now free to move independently".WriteSuccess();
            return;
        }

        // Glue blue box to green box's top
        _glue2 = _topBox.GlueTo(_middleBox, "TopFaceCenter");
        
        $"✅ Blue box glued to Green box's top".WriteSuccess();
    }

    public void StartAnimation()
    {
        if (_baseBox == null)
        {
            $"No tower created - call CreateStackedTower first".WriteError();
            return;
        }

        _animationTime = 0;
        _isAnimating = true;

        // Set up animation callback on base box
        var glyph = _baseBox as FoGlyph3D;
        if (glyph != null)
        {
            glyph.BeforeAnimationRefresh((shape, tick, fps) =>
            {
                if (!_isAnimating) return;

                _animationTime += 1.0 / fps;
                var progress = _animationTime / ANIMATION_DURATION;
                
                if (progress >= 1.0)
                {
                    _isAnimating = false;
                    $"Animation completed".WriteSuccess();
                    return;
                }

                // Move base box in circular path
                var angle = progress * 2 * Math.PI;
                var x = Math.Cos(angle) * CIRCLE_RADIUS;
                var z = Math.Sin(angle) * CIRCLE_RADIUS;
                
                var newPos = new Vector3(x, 0.5, z);
                shape.Transform.Position = newPos;  // Keep Y at 0.5
                
                // Add rotation to the base box as it moves
                // Rotate around Y-axis (vertical) - 2 full rotations during the circular path
                var rotationAngle = progress * 4 * Math.PI;  // 2 full rotations (4π radians)
                shape.Transform.Rotation = new Euler(0, rotationAngle, 0);

                // Glue propagation happens automatically via Transform.OnChange!
                // No manual code needed - the FoGlue3D objects handle it
            });
        }

        $"Animation started - base box moving in circular path".WriteSuccess();
    }

    public void StopAnimation()
    {
        _isAnimating = false;
        var glyph = _baseBox as FoGlyph3D;
        if (glyph != null)
        {
            glyph.ClearAnimationRefresh();
        }
        $"Animation stopped".WriteInfo();
    }

    public void Reset()
    {
        StopAnimation();
        
        var arena = Workspace?.GetArena();
        arena?.ClearArena();
        
        _baseBox = null;
        _middleBox = null;
        _topBox = null;
        _glue1 = null;
        _glue2 = null;
        _animationTime = 0;
        
        $"Scene reset".WriteInfo();
    }
    
    // ==================== Advanced Glue Tests ====================
    
    /// <summary>
    /// Test gluing with floating offset - box hovers above target
    /// </summary>
    public void TestFloatingGlue()
    {
        if (_baseBox == null || _middleBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        // Unglue if already glued
        _glue1?.UnGlue();
        
        // Glue with 0.5 unit offset - green box floats above red
        _glue1 = _middleBox.GlueTo(_baseBox, "TopFaceCenter", offset: 0.5);
        
        $"✅ Green box floating 0.5 units above red".WriteSuccess();
    }
    
    /// <summary>
    /// Test gluing to different connection points
    /// </summary>
    public void TestSideGlue()
    {
        if (_baseBox == null || _middleBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        // Unglue if already glued
        _glue1?.UnGlue();
        
        // Glue to right face center
        _glue1 = _middleBox.GlueTo(_baseBox, "RightFaceCenter");
        
        $"✅ Green box glued to red's right side".WriteSuccess();
    }
    
    /// <summary>
    /// Test gluing to corner vertices
    /// </summary>
    public void TestCornerGlue()
    {
        if (_baseBox == null || _topBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        // Unglue if already glued
        _glue2?.UnGlue();
        
        // Glue to corner vertex
        _glue2 = _topBox.GlueTo(_baseBox, "RightTopFront");
        
        $"✅ Blue box glued to red's front-right-top corner".WriteSuccess();
    }
    
    /// <summary>
    /// Test gluing to edge centers
    /// </summary>
    public void TestEdgeGlue()
    {
        if (_baseBox == null || _topBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        // Unglue if already glued
        _glue2?.UnGlue();
        
        // Glue to top-front edge
        _glue2 = _topBox.GlueTo(_baseBox, "TopFrontEdge", offset: 0.2);
        
        $"✅ Blue box glued to red's top-front edge with offset".WriteSuccess();
    }
    
    // ==================== Rotation Alignment Tests ====================
    
    /// <summary>
    /// Test rotation alignment on top face
    /// </summary>
    public void TestAlignedTopGlue()
    {
        if (_baseBox == null || _middleBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        _glue1?.UnGlue();
        _glue1 = _middleBox.GlueTo(_baseBox, "TopFaceCenter", offset: 0, alignRotation: true);
        
        $"✅ Green aligned to red's top (should face down)".WriteSuccess();
    }
    
    /// <summary>
    /// Test rotation alignment on side face
    /// </summary>
    public void TestAlignedSideGlue()
    {
        if (_baseBox == null || _middleBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        _glue1?.UnGlue();
        _glue1 = _middleBox.GlueTo(_baseBox, "RightFaceCenter", offset: 0.3, alignRotation: true);
        
        $"✅ Green aligned to red's right side (should rotate 90°)".WriteSuccess();
    }
    
    /// <summary>
    /// Test rotation alignment on bottom face (upside down)
    /// </summary>
    public void TestAlignedBottomGlue()
    {
        if (_baseBox == null || _topBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        _glue2?.UnGlue();
        _glue2 = _topBox.GlueTo(_baseBox, "BottomFaceCenter", offset: 0.2, alignRotation: true);
        
        $"✅ Blue aligned to red's bottom (should be upside down)".WriteSuccess();
    }
    
    /// <summary>
    /// Test rotation alignment on corner vertex
    /// </summary>
    public void TestAlignedCornerGlue()
    {
        if (_baseBox == null || _topBox == null)
        {
            $"Create boxes first!".WriteError();
            return;
        }
        
        _glue2?.UnGlue();
        _glue2 = _topBox.GlueTo(_baseBox, "RightTopFront", offset: 0.1, alignRotation: true);
        
        $"✅ Blue aligned to red's corner (diagonal rotation)".WriteSuccess();
    }
    
    // ==================== Dynamic Pipe Tests ====================
    
    /// <summary>
    /// Create a pipe connecting two boxes that updates as they move
    /// </summary>
    public void CreatePipeBetweenBoxes()
    {
        if (_baseBox == null || _middleBox == null)
        {
            CreateStackedTower();
        }

        var arena = Workspace.GetArena();
        if (arena == null)
        {
            $"No arena available".WriteError();
            return;
        }
        var stage = arena.CurrentStage();
        
        // Create pipe connecting base to middle box
        _pipe1 = new FoGluePipe3D($"Pipe1-{Guid.NewGuid().ToString().Substring(0, 8)}", "orange");
        _pipe1.ConnectBetween(
            _baseBox, "TopFaceCenter",
            _middleBox, "BottomFaceCenter",
            radius: 0.08
        );
        arena.AddShapeToStage<FoGluePipe3D>(_pipe1, stage.GetName());
        
        $"Created dynamic pipe between boxes - will update as they move!".WriteSuccess();
        StateHasChanged();
    }
    
    /// <summary>
    /// Add multiple pipes at different connection points
    /// </summary>
    public void CreateMultiplePipes()
    {
        if (_baseBox == null || _middleBox == null || _topBox == null)
        {
            CreateStackedTower();
        }

        var arena = Workspace.GetArena();
        var stage = arena.CurrentStage();
        
        // Pipe 1: Base top to Middle bottom
        _pipe1 = new FoGluePipe3D($"Pipe1-{Guid.NewGuid().ToString().Substring(0, 8)}", "orange");
        _pipe1.ConnectBetween(
            _baseBox, "RightTopFront",
            _middleBox, "LeftBottomBack",
            radius: 0.06
        );
        arena.AddShapeToStage<FoGluePipe3D>(_pipe1, stage.GetName());
        
        // Pipe 2: Middle side to Top side
        _pipe2 = new FoGluePipe3D($"Pipe2-{Guid.NewGuid().ToString().Substring(0, 8)}", "cyan");
        _pipe2.ConnectBetween(
            _middleBox, "FrontFaceCenter",
            _topBox, "BackFaceCenter",
            radius: 0.06
        );
        arena.AddShapeToStage<FoGluePipe3D>(_pipe2, stage.GetName());
        
        $"Created 2 dynamic pipes - watch them update during animation!".WriteSuccess();
        StateHasChanged();
    }
    
    /// <summary>
    /// Test pipe updating during glue operations
    /// </summary>
    public void TestPipeWithGlue()
    {
        if (_baseBox == null || _middleBox == null)
        {
            CreateStackedTower();
        }
        
        // Create pipe first
        CreatePipeBetweenBoxes();
        
        // Now glue the boxes - pipe should update automatically
        _glue1 = _middleBox.GlueTo(_baseBox, "TopFaceCenter", offset: 0.5, alignRotation: true);
        
        $"Glued boxes with pipe connected - pipe auto-updates!".WriteSuccess();
        StateHasChanged();
    }

    public void Dispose()
    {
        StopAnimation();
        //$"GlueTest3D Page Disposed".WriteInfo();
    }
}
