

using BlazorThreeJS.Core;
using BlazorThreeJS.Enums;
using BlazorThreeJS.Objects;
using BlazorThreeJS.Materials;
using BlazorThreeJS.Maths;
using BlazorThreeJS.Viewers;
using BlazorThreeJS.Settings;
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using BlazorThreeJS.Geometires;


using FoundryRulesAndUnits.Models;




        


namespace Three2025.Components.Pages;

public class IndexBase : ComponentBase, IDisposable
{
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] protected IJSRuntime JsRuntime { get; set; }
    [Parameter] public int CanvasWidth { get; set; } = 1000;
    [Parameter] public int CanvasHeight { get; set; } = 800;

    protected ViewerThreeD View3D;
    
    protected Text3D TestText { get; set; }
    protected TextPanel3D TextPanel1 { get; set; }
    protected MockDataGenerator DataGenerator { get; set; } = new();
    public FoWorkbook Workbook { get; set; }



    public Scene3D GetCurrentScene()
    {
        return View3D.ActiveScene;
    }


    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //for now this call is too early,  scene is not setup
            //DoRenderingTest();

        }
        return base.OnAfterRenderAsync(firstRender);
    }

    public void DoAxisTest()
    {
        var scene = GetCurrentScene();
        AddAxisToScene(scene);

    }
    public void AddAxisToScene(Scene3D scene)
    {
        var pos = new Vector3(0, 0, 0);
        var rot = new Euler(0, 0, 0);
        var piv = new Vector3(0, 0, 0);


        var Uuid = Guid.NewGuid().ToString();

        var model = new ImportSettings
        {
            Uuid = Uuid,
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Position = pos,
            Rotation = rot,
            Pivot = piv,
            OnComplete = () =>
            {
                var group = new Group3D()
                {
                    Name = "Axis",
                    Uuid = Uuid,
                };
                scene.AddChild(group);
                $"Axis added to scene in callback".WriteSuccess();
                StateHasChanged();
            }

        };

        Task.Run(async () => await scene.Request3DModel(model));
    }





    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        path.WriteSuccess();
        return path;
    }

    public async Task OnAddTRex()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid().ToString(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/T_Rex.glb"),
            Position = new Vector3(2, 0, 2),
        };

        var scene = GetCurrentScene();
        await scene.Request3DModel(model);
        await scene.UpdateScene();
    }

    public async Task OnAddJet()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid().ToString(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/jet.glb"),
            Position = new Vector3(0, 0, 0),
        };


        var scene = GetCurrentScene();
        await scene.Request3DModel(model);
        await scene.UpdateScene();;
    }

    public async Task OnAddCar()
    {
        var model = new ImportSettings
        {
            Uuid = Guid.NewGuid().ToString(),
            Format = Import3DFormats.Gltf,
            FileURL = GetReferenceTo(@"storage/StaticFiles/mustang_1965.glb"),
            Position = new Vector3(0, 0, 0),
        };


        var scene = GetCurrentScene();
        await scene.Request3DModel(model);
        await scene.UpdateScene();
    }

    public async Task OnAddText()
    {
        TestText = new Text3D("My First Text") 
        { 
            Position = new Vector3(3, 2, 3), 
            Color = DataGenerator.GenerateColor(),  //"#33333a" 
        };

        var scene = GetCurrentScene();
        scene.AddChild(TestText);

        await scene.UpdateScene();
    }

    public async Task OnUpdateText()
    {
        var newText = DateTime.Now.ToLongTimeString();

        //$"newText={newText}".WriteInfo();

        if (TestText != null) 
        {
            TestText.Text = newText;
            TestText.Color = DataGenerator.GenerateColor();
        }

        var scene = GetCurrentScene();
        await scene.UpdateScene();
    }

    private TextPanel3D BuildTextPanel()
    {
        var textLines = new List<string>()
        {
            "Lorem ipsum dolor sit amet.", "Lorem ipsum dolor sit amet.", "Lorem ipsum dolor sit amet.", "Lorem ipsum dolor sit amet.", "Lorem ipsum dolor sit amet.", "Lorem ipsum dolor sit amet.", "Lorem ipsum dolor sit amet.", "Lorem ipsum dolor sit amet."
        };
        var panelPos = new Vector3(-1, 2, -2);
        var panelRot = new Euler(-1 * Math.PI * 30 / 180, 0, 0);

        var textPanel = new TextPanel3D
        {
            Name = "TEXTPANEL1",
            // Width = 1,
            // Height = 1,
            TextLines = textLines,
            Position = panelPos,
            Rotation = panelRot
        };
        return textPanel;
    }

    private void ClickButton1(Button3D self)
    {
        $"self.Name={self.Name}".WriteInfo();

        self.Text = self.Text == "OFF" ? "ON" : "OFF";

        var scene = GetCurrentScene();
        if (self.Text == "ON")
        {
            TextPanel1 = BuildTextPanel();
            scene.AddChild(TextPanel1);
        }
        else
        {
            scene.RemoveChild(TextPanel1);
        }


        scene.ForceSceneRefresh();
    }

    public async Task OnAddMenu()
    {
        
        var scene = GetCurrentScene();

        var buttons = new List<Button3D>() {
            new Button3D("BTN1", "OFF") {
                OnClick = ClickButton1
            },
            new Button3D("BTN2","Button 2"),
            new Button3D("BTN3","Button 3")
        };
        var menuPos = new Vector3(-4, 3, -2);
        var menuRot = new Euler(-1 * Math.PI * 30 / 180, 0, 0);

        var panel = new PanelMenu3D
        {
            Name = "MENU1",
            Uuid = Guid.NewGuid().ToString(),
            Width = 1.0,
            Height = 3.0,
            Position = menuPos,
            Rotation = menuRot
        };

        panel.Buttons.AddRange(buttons);
        scene.AddChild(panel);

        // scene.Add(BuildTextPanel());
        //scene.Add(BuildPanelGroup());


        await scene.UpdateScene();
    }

    private TextPanel3D BuildChildPanel(string text)
    {
        var textLines = new List<string>() { text };
        // var panelPos = new Vector3(-1, 2, -2);
        // var panelRot = new Euler(-1 * Math.PI * 30 / 180, 0, 0);

        var panel = new TextPanel3D
        {
            Name = "TEXTPANEL1",
            Color = "red",
            // Width = 1,
            // Height = 1,
            TextLines = textLines,
            // Position = panelPos,
            // Rotation = panelRot
        };
        return panel;
    }

    private Mesh3D BuildTube()
    {
        var path = new List<Vector3>() {
            new Vector3(-2, 0, 0),
            new Vector3(4, 0, 0),
            new Vector3(4, 4, 0),
            new Vector3(4, 4, -4)
        };
        var radius = 0.1;
        var mesh = new Mesh3D()
        {
            Geometry = new TubeGeometry(tubularSegments: 10, radialSegments: 8, radius: radius, path: path),
            Position = new Vector3(0, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "yellow"
            }
        };
        return mesh;
    }

    private Mesh3D BuildSomething()
    {
        var mesh = new Mesh3D
        {
            Geometry = new DodecahedronGeometry(radius: 0.8f),
            Position = new Vector3(-2, 6, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "darkviolet",
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        };
        return mesh;
    }

    private PanelGroup3D BuildPanelGroup()
    {
        var textLines = new List<string>()
        {
            "Lorem ipsum dolor sit amet."
        };
        var panelPos = new Vector3(2, 2, -6);
        var panelRot = new Euler(-1 * Math.PI * 45 / 180, 0, Math.PI * 30 / 180);
        var panelW = 5;
        var panelH = 5;

        var childPanelW = 1;
        var childPanelH = 0.5;
        var childPadding = 0;

        var colors = new List<string>() { "red", "orange", "green", "purple", "blue" };
        var childPanels = new List<TextPanel3D>();

        for (int i = 0; i < 5; i++)
        {
            var text = $"Child Panel {i}";
            var childPanel = BuildChildPanel(text);
            childPanel.Color = colors[i];
            childPanel.Position = new Vector3(-panelW / 2 + i * childPanelW + childPadding, -panelH / 2 + i, 0.1);
            childPanel.TextLines = new List<string>() { text };
            childPanel.Width = childPanelW;
            childPanel.Height = childPanelH;
            childPanels.Add(childPanel);
        }

        var group = new PanelGroup3D
        {
            Name = "PANELGROUP1",
            Width = panelW,
            Height = panelH,
            TextLines = textLines,
            Position = panelPos,
            Rotation = panelRot,
            TextPanels = childPanels,
            Meshes = new List<Object3D>() 
            { 
                BuildTube(), 
                BuildSomething() 
            }
        };
        return group;
    }

    public async Task OnAddGroup1()
    {
        var scene = GetCurrentScene();
        var group = new Group3D()
        {
            Name = "Group1",
            Uuid = Guid.NewGuid().ToString(),
        };

        group.AddChild(new Mesh3D
        {
            Name = "Box1",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(-5, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "magenta"
            }
        });

        group.AddChild(new Mesh3D
        {
            Name = "Box2",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(5, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "green"
            }
        });

        scene.AddChild(group);

        await scene.UpdateScene();
    }

    public async Task OnAddGroup2()
    {
        var scene = GetCurrentScene();

        var group = new Mesh3D
        {
            Name = "Group2",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(0, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "red"
            }
        };

        group.AddChild(new Mesh3D
        {
            Name = "Box3",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(-5, 0, -5),
            Material = new MeshStandardMaterial()
            {
                Color = "magenta"
            }
        });

        group.AddChild(new Mesh3D
        {
            Name = "Box4",
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(5, 0, 5),
            Material = new MeshStandardMaterial()
            {
                Color = "green"
            }
        });
        scene.AddChild(group);
        await scene.UpdateScene();
    }

    public async Task DoMeshTest()
    {
        var scene = GetCurrentScene();


        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new BoxGeometry(width: 1.2f, height: 0.5f),
            Position = new Vector3(-2, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "magenta"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new CircleGeometry(radius: 0.75f, segments: 12),
            Position = new Vector3(2, 0, 0),
            Scale = new Vector3(1, 0.75f, 1),
            Material = new MeshStandardMaterial()
            {
                Color = "#98AFC7"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new CapsuleGeometry(radius: 0.5f, length: 2),
            Position = new Vector3(-4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new ConeGeometry(radius: 0.5f, height: 2, radialSegments: 16),
            Position = new Vector3(4, 0, 0),
            Material = new MeshStandardMaterial()
            {
                Color = "green",
                FlatShading = true,
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new CylinderGeometry(radiusTop: 0.5f, height: 1.2f, radialSegments: 16),
            Position = new Vector3(0, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "red",
                Wireframe = true
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new DodecahedronGeometry(radius: 0.8f),
            Position = new Vector3(-2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "darkviolet",
                Metalness = 0.5f,
                Roughness = 0.5f
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new IcosahedronGeometry(radius: 0.8f),
            Position = new Vector3(-4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "violet"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new OctahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "aqua"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new PlaneGeometry(width: 0.5f, height: 2),
            Position = new Vector3(4, 0, -2),
            Material = new MeshStandardMaterial()
            {
                Color = "purple"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new RingGeometry(innerRadius: 0.6f, outerRadius: 0.7f),
            Position = new Vector3(0, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "DodgerBlue"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new SphereGeometry(radius: 0.6f),
            Position = new Vector3(-2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "darkgreen"
            },
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new TetrahedronGeometry(radius: 0.75f),
            Position = new Vector3(2, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightblue"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new TorusGeometry(radius: 0.6f, tube: 0.4f, radialSegments: 12, tubularSegments: 12),
            Position = new Vector3(4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "lightgreen"
            }
        });

        scene.AddChild(new Mesh3D
        {
            Uuid = Guid.NewGuid().ToString(),
            Geometry = new TorusKnotGeometry(radius: 0.6f, tube: 0.1f),
            Position = new Vector3(-4, 0, -4),
            Material = new MeshStandardMaterial()
            {
                Color = "RosyBrown"
            }
        });

        await scene.UpdateScene();
    }


    public void Dispose()
    {

    }
}