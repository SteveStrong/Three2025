using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Geometires;
using FoundryBlazor.Shape;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Three2025.Shared;
using FoundryBlazor.Solutions;
using BlazorThreeJS.Core;
using FoundryBlazor.Shared;
using FoundryBlazor.PubSub;

namespace Three2025.Components.Pages;

public class SpacialBoxTestBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    [Inject] public IFoundryService FoundryService { get; init; }

    public Canvas3DComponentBase Canvas3DReference = null;
    protected SpacialBox3D CurrentBox;

    // Box properties for UI binding
    protected double BoxWidth { get; set; } = 2.0;
    protected double BoxHeight { get; set; } = 1.5;
    protected double BoxDepth { get; set; } = 1.0;

    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;

    protected string StatusMessage { get; set; } = string.Empty;



    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false,null!);

            scene?.SetAfterUpdateAction((s,j) =>
            {
                FoundryService.PubSub().Publish<RefreshUIEvent>(new RefreshUIEvent("ShapeTree"));
            });

            var arena = Workspace.GetArena();
            if (found)
            {
                arena.SetScene(scene!);
                CreateSpacialBox();
            }
        }
        return base.OnAfterRenderAsync(firstRender);
    }

    public void DoRequestAxisToScene(Scene3D scene)
    {
        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        scene.AddChild(model);
    }

    public void CreateSpacialBox()
    {
        try
        {
            CurrentBox = new SpacialBox3D(BoxWidth, BoxHeight, BoxDepth, "m");

            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }

            arena.ClearArena();

            var boxShape = new FoShape3D()
                .CreateBox("SpacialBoxMain", BoxWidth, BoxHeight, BoxDepth);

            boxShape.Color = "#4CAF50";
            boxShape.Opacity = 0.8;
            boxShape.Transform.Position = new BlazorThreeJS.Maths.Vector3(CurrentBox.Center.X - BoxWidth / 2, CurrentBox.Center.Y - BoxHeight / 2, CurrentBox.Center.Z - BoxDepth / 2);

            arena.AddShapeToStage<FoShape3D>(boxShape);

            StatusMessage = $"Created SpacialBox3D (FoShape3D): {BoxWidth}×{BoxHeight}×{BoxDepth}m";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating box: {ex.Message}";
            StateHasChanged();
        }
    }

    // Removed: CreateBoxMesh. All box creation now uses FoShape3D and Arena.

    public void ShowBoxInfo()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var info = $"Box Info:\n" +
                  $"Volume: {CurrentBox.Volume:F2} {CurrentBox.Units}³\n" +
                  $"Surface Area: {CurrentBox.SurfaceArea:F2} {CurrentBox.Units}²\n" +
                  $"Center: ({CurrentBox.Center.X:F2}, {CurrentBox.Center.Y:F2}, {CurrentBox.Center.Z:F2})";

        StatusMessage = info;
        StateHasChanged();
    }






    public void ClearAll()
    {
        var arena = Workspace?.GetArena();
        if (arena == null)
        {
            StatusMessage = "Arena not ready yet. Try again in a moment.";
            StateHasChanged();
            return;
        }


        arena.ClearArena();
        StateHasChanged();
    }



    public void AddAxisToScene(Scene3D scene)
    {
        var model = new Model3D()
        {
            Name = "Axis",
            Uuid = Guid.NewGuid().ToString(),
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        Task.Run(async () => await scene.Request3DModel(model, async (uuid) =>
        {
            var group = new Group3D()
            {
                Name = "Axis",
                Uuid = uuid,
            };
            scene.AddChild(group);
            StatusMessage = "Axis added to scene";
            StateHasChanged();
            await Task.CompletedTask;
        }));
    }

    public void Dispose()
    {
        // Cleanup resources if needed
    }

    // === PRESET SHAPES ===
    protected void CreateCube()
    {
        BoxWidth = BoxHeight = BoxDepth = 2.0;
        CreateSpacialBox();
    }

    protected void CreateLongBox()
    {
        BoxWidth = 4.0; BoxHeight = 1.0; BoxDepth = 1.0;
        CreateSpacialBox();
    }

    protected void CreateTallBox()
    {
        BoxWidth = 1.0; BoxHeight = 4.0; BoxDepth = 1.0;
        CreateSpacialBox();
    }

    protected void CreateWideBox()
    {
        BoxWidth = 1.0; BoxHeight = 1.0; BoxDepth = 4.0;
        CreateSpacialBox();
    }

    protected void CreateTinyBox()
    {
        BoxWidth = BoxHeight = BoxDepth = 0.5;
        CreateSpacialBox();
    }





    public void ShowDiagonals()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        // Show main diagonals connecting opposite corners
        var diagonalPairs = new[]
        {
            (CurrentBox.LeftTopFront, CurrentBox.RightBottomBack),
            (CurrentBox.RightTopFront, CurrentBox.LeftBottomBack),
            (CurrentBox.LeftBottomFront, CurrentBox.RightTopBack),
            (CurrentBox.RightBottomFront, CurrentBox.LeftTopBack)
        };

        int index = 0;
        foreach (var (start, end) in diagonalPairs)
        {
            // Create small spheres at diagonal endpoints
            CreateMarkerSphere($"Diagonal{index}Start", start, "#E91E63", 0.03);
            CreateMarkerSphere($"Diagonal{index}End", end, "#E91E63", 0.03);
            index++;
        }

        StatusMessage = $"Showing {diagonalPairs.Length} main diagonals";
        StateHasChanged();
    }

    public void ShowCenterCross()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }



        var center = CurrentBox.Center;
        var halfW = CurrentBox.Width / 2;
        var halfH = CurrentBox.Height / 2;
        var halfD = CurrentBox.Depth / 2;

        // Create cross points extending from center
        var crossPoints = new[]
        {
            new Point3D(center.X + halfW, center.Y, center.Z), // Right
            new Point3D(center.X - halfW, center.Y, center.Z), // Left
            new Point3D(center.X, center.Y + halfH, center.Z), // Top
            new Point3D(center.X, center.Y - halfH, center.Z), // Bottom
            new Point3D(center.X, center.Y, center.Z + halfD), // Front
            new Point3D(center.X, center.Y, center.Z - halfD)  // Back
        };

        CreateMarkerSphere("CenterPoint", center, "#FF0000", 0.08);

        for (int i = 0; i < crossPoints.Length; i++)
        {
            CreateMarkerSphere($"CrossPoint{i}", crossPoints[i], "#FF5722", 0.05);
        }

        StatusMessage = "Showing center cross with 6 directional points";
        StateHasChanged();
    }

    public void ShowQuadrants()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }



        var center = CurrentBox.Center;
        var quadrantSize = 0.2;
        var colors = new[] { "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FFA500", "#800080" };

        // Create 8 quadrant markers (for a 3D box)
        var quadrants = new[]
        {
            new Point3D(center.X + quadrantSize, center.Y + quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y + quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y - quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y - quadrantSize, center.Z + quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y + quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y + quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X + quadrantSize, center.Y - quadrantSize, center.Z - quadrantSize),
            new Point3D(center.X - quadrantSize, center.Y - quadrantSize, center.Z - quadrantSize)
        };

        for (int i = 0; i < quadrants.Length; i++)
        {
            CreateMarkerSphere($"Quadrant{i}", quadrants[i], colors[i], 0.04);
        }

        StatusMessage = "Showing 8 3D quadrants around center";
        StateHasChanged();
    }

 


    // === MEASUREMENT TOOLS ===
    public void MeasureDistances()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var diagonal = Math.Sqrt(Math.Pow(CurrentBox.Width, 2) + Math.Pow(CurrentBox.Height, 2) + Math.Pow(CurrentBox.Depth, 2));
        var faceDiagonal1 = Math.Sqrt(Math.Pow(CurrentBox.Width, 2) + Math.Pow(CurrentBox.Height, 2));
        var faceDiagonal2 = Math.Sqrt(Math.Pow(CurrentBox.Height, 2) + Math.Pow(CurrentBox.Depth, 2));
        var faceDiagonal3 = Math.Sqrt(Math.Pow(CurrentBox.Width, 2) + Math.Pow(CurrentBox.Depth, 2));

        StatusMessage = $"Distances - Main diagonal: {diagonal:F2}, Face diagonals: {faceDiagonal1:F2}, {faceDiagonal2:F2}, {faceDiagonal3:F2}";
        StateHasChanged();
    }

        // === VISUALIZATION TEST METHODS ===
        public void ShowVertices()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            arena.ClearArena();
            int i = 0;
            foreach (var v in CurrentBox.Vertices)
            {
                CreateMarkerSphere($"Vertex{i}", v, "#2196F3", 0.05);
                i++;
            }
            StatusMessage = $"Showing {CurrentBox.Vertices.Count} vertices as spheres.";
            StateHasChanged();
        }

        public void ShowEdges()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            arena.ClearArena();
            // Box edges: 12 unique pairs of vertices
            var verts = CurrentBox.Vertices;
            var edgePairs = new (int, int)[] {
                (0,1),(1,3),(3,2),(2,0), // top face
                (4,5),(5,7),(7,6),(6,4), // bottom face
                (0,4),(1,5),(2,6),(3,7)  // verticals
            };
            int i = 0;
            foreach (var (a, b) in edgePairs)
            {
                var start = verts[a];
                var end = verts[b];
                var mid = new Point3D((start.X+end.X)/2, (start.Y+end.Y)/2, (start.Z+end.Z)/2);
                var length = Math.Sqrt(Math.Pow(end.X-start.X,2)+Math.Pow(end.Y-start.Y,2)+Math.Pow(end.Z-start.Z,2));
                var edge = new FoShape3D()
                    .CreateCylinder($"Edge{i}", 0.03, length, 0.03);
                edge.Color = "#333";
                edge.Transform.Position = new Vector3(mid.X, mid.Y, mid.Z);
                arena.AddShapeToStage<FoShape3D>(edge);
                i++;
            }
            StatusMessage = $"Showing {edgePairs.Length} edges as cylinders.";
            StateHasChanged();
        }

        public void ShowFaces()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            arena.ClearArena();
            // Faces: 6, each as a thin box
            var faces = new (List<Point3D> verts, string name)[] {
                (CurrentBox.FrontFace, "Front"),
                (CurrentBox.BackFace, "Back"),
                (CurrentBox.LeftFace, "Left"),
                (CurrentBox.RightFace, "Right"),
                (CurrentBox.TopFace, "Top"),
                (CurrentBox.BottomFace, "Bottom")
            };
            int i = 0;
            foreach (var (verts, fname) in faces)
            {
                // Center of face
                var cx = verts.Average(v=>v.X);
                var cy = verts.Average(v=>v.Y);
                var cz = verts.Average(v=>v.Z);
                // Face dimensions
                var w = Math.Sqrt(Math.Pow(verts[0].X-verts[1].X,2)+Math.Pow(verts[0].Y-verts[1].Y,2)+Math.Pow(verts[0].Z-verts[1].Z,2));
                var h = Math.Sqrt(Math.Pow(verts[1].X-verts[2].X,2)+Math.Pow(verts[1].Y-verts[2].Y,2)+Math.Pow(verts[1].Z-verts[2].Z,2));
                var faceShape = new FoShape3D()
                    .CreateBox($"Face{fname}", w, h, 0.02);
                faceShape.Color = "#4CAF50";
                faceShape.Opacity = 0.4;
                faceShape.Transform.Position = new Vector3(cx, cy, cz);
                arena.AddShapeToStage<FoShape3D>(faceShape);
                i++;
            }
            StatusMessage = $"Showing 6 faces as thin boxes.";
            StateHasChanged();
        }

        public void ShowNormals()
        {
            if (CurrentBox == null)
            {
                StatusMessage = "No box created yet. Please create a box first.";
                StateHasChanged();
                return;
            }
            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                StatusMessage = "Arena not ready yet. Try again in a moment.";
                StateHasChanged();
                return;
            }
            arena.ClearArena();
            // Normals: for each face, draw a cylinder from face center in normal direction
            var faceData = new (List<Point3D> verts, Vector3 normal, string name)[] {
                (CurrentBox.FrontFace, new Vector3(0,0,1), "Front"),
                (CurrentBox.BackFace, new Vector3(0,0,-1), "Back"),
                (CurrentBox.LeftFace, new Vector3(-1,0,0), "Left"),
                (CurrentBox.RightFace, new Vector3(1,0,0), "Right"),
                (CurrentBox.TopFace, new Vector3(0,1,0), "Top"),
                (CurrentBox.BottomFace, new Vector3(0,-1,0), "Bottom")
            };
            int i = 0;
            foreach (var (verts, normal, fname) in faceData)
            {
                var cx = verts.Average(v=>v.X);
                var cy = verts.Average(v=>v.Y);
                var cz = verts.Average(v=>v.Z);
                var start = new Point3D(cx, cy, cz);
                var end = new Point3D(cx+normal.X*0.4, cy+normal.Y*0.4, cz+normal.Z*0.4);
                var mid = new Point3D((start.X+end.X)/2, (start.Y+end.Y)/2, (start.Z+end.Z)/2);
                var length = 0.4;
                var normalShape = new FoShape3D()
                    .CreateCylinder($"Normal{fname}", 0.015, length, 0.015);
                normalShape.Color = "#F00";
                normalShape.Transform.Position = new Vector3(mid.X, mid.Y, mid.Z);
                arena.AddShapeToStage<FoShape3D>(normalShape);
                i++;
            }
            StatusMessage = $"Showing 6 face normals as red cylinders.";
            StateHasChanged();
        }

    public void ShowDimensions()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }


        // Create dimension indicators (simplified - would need line drawing capability)
        var center = CurrentBox.Center;
        CreateMarkerSphere("DimCenter", center, "#FFFF00", 0.05);

        // Show dimension endpoints
        CreateMarkerSphere("WidthEnd1", new Point3D(center.X - CurrentBox.Width / 2, center.Y, center.Z), "#FF0000", 0.03);
        CreateMarkerSphere("WidthEnd2", new Point3D(center.X + CurrentBox.Width / 2, center.Y, center.Z), "#FF0000", 0.03);

        StatusMessage = $"Dimensions: W={CurrentBox.Width:F2}, H={CurrentBox.Height:F2}, D={CurrentBox.Depth:F2}";
        StateHasChanged();
    }

    public void CalculateAngles()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        // Calculate angles between diagonals and faces
        var widthAngle = Math.Atan(CurrentBox.Height / CurrentBox.Width) * 180 / Math.PI;
        var heightAngle = Math.Atan(CurrentBox.Width / CurrentBox.Height) * 180 / Math.PI;
        var depthAngle = Math.Atan(CurrentBox.Depth / CurrentBox.Width) * 180 / Math.PI;

        StatusMessage = $"Angles - Width/Height: {widthAngle:F1}°, Height/Width: {heightAngle:F1}°, Depth/Width: {depthAngle:F1}°";
        StateHasChanged();
    }

    public void ShowVolume3D()
    {
        if (CurrentBox == null)
        {
            StatusMessage = "No box created yet. Please create a box first.";
            StateHasChanged();
            return;
        }

        var volume = CurrentBox.Volume;
        var surfaceArea = CurrentBox.SurfaceArea;
        var ratio = surfaceArea / volume;

        StatusMessage = $"3D Metrics - Volume: {volume:F2} {CurrentBox.Units}³, Surface Area: {surfaceArea:F2} {CurrentBox.Units}², SA/V Ratio: {ratio:F2}";
        StateHasChanged();
    }

    // === UTILITY METHODS ===
    public void ResetView()
    {
        StatusMessage = "View reset (would require camera controls)";
        StateHasChanged();
    }

    private void CreateMarkerSphere(string name, Point3D position, string color, double radius)
    {
        var shape = new FoShape3D()
        {
            Name = name,
            Color = color,
            GlyphId = Guid.NewGuid().ToString(),
            Transform = new Transform3() {
                Position = new Vector3(position.X, position.Y, position.Z)
            }
        }.CreateSphere(name, radius, radius, radius);


        var arena = Workspace?.GetArena();
        arena?.AddShapeToStage<FoShape3D>(shape);
    }
}
