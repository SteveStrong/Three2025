using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;
using FoundryWorldsAndDrawings.Shape;
using FoundryRulesAndUnits.Extensions;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.ThreeD.Geometires;
using FoundryWorldsAndDrawings.ThreeD.Materials;
using FoundryWorldsAndDrawings.ThreeD.Core;
using FoundryWorldsAndDrawings.Shared;
using Blazor.Extensions;

namespace Three2025.Components.Pages;

public partial class DebugCanvasBase : ComponentBase
{
    public Canvas3DComponent Canvas3DReference;
    public BECanvasComponent BECanvasReference;
    
    [Inject] public NavigationManager Navigation { get; set; }
    [Inject] public IWorkspace Workspace { get; set; }
    
    protected string DebugInfo = "Waiting for canvas...";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            "🔍 DebugCanvas OnAfterRenderAsync - FIRST RENDER".WriteInfo();
            await Task.Delay(500); // Give ViewerThreeD time to initialize
            
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
            DebugInfo = found ? $"✅ Scene found: {scene?.Title}" : "❌ Scene NOT found!";
            
            $"🔍 DebugCanvas: Scene found={found}, Title={scene?.Title}".WriteInfo();
            

            var context = await BECanvasReference.CreateCanvas2DAsync();
            await context.SetFillStyleAsync("green");

            await context.FillRectAsync(10, 100, 100, 100);

            await context.SetFontAsync("48px serif");
            await context.StrokeTextAsync("Hello BECanvasReference!!!", 10, 100);

            StateHasChanged();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public void AddSimpleBox()
    {
        try
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
            if (!found)
            {
                DebugInfo = "❌ Scene not found - cannot add box";
                StateHasChanged();
                return;
            }

            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                DebugInfo = "❌ Arena not found";
                StateHasChanged();
                return;
            }

            var pipe = new FoPipe3D("TestPipe", "#FF0000")
            {
                Transform = new Transform3("PipeTransform")
                {
                    Position = new Vector3(0, 0, 0)
                }
            };
            
            pipe.CreateTube("TestPipe", 0.2, new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(5, 0, 0),
                new Vector3(5, 5, 0)
            });

            var stage = arena.CurrentStage();
            arena.AddShapeToStage(pipe, stage.GetName());
            DebugInfo = $"✅ Added pipe to scene '{scene.Title}'";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            DebugInfo = $"❌ Error: {ex.Message}\n{ex.StackTrace}";
            StateHasChanged();
        }
    }

    public void AddColoredSphere()
    {
        try
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
            if (!found)
            {
                DebugInfo = "❌ Scene not found";
                StateHasChanged();
                return;
            }

            var arena = Workspace?.GetArena();
            if (arena == null)
            {
                DebugInfo = "❌ Arena not found";
                StateHasChanged();
                return;
            }

            var pipe2 = new FoPipe3D("TestPipe2", "#00FF00")
            {
                Transform = new Transform3("Pipe2Transform")
                {
                    Position = new Vector3(0, 3, 0)
                }
            };
            
            pipe2.CreateTube("TestPipe2", 0.2, new List<Vector3>()
            {
                new Vector3(0, 0, 0),
                new Vector3(-5, 0, 0),
                new Vector3(-5, -5, 0)
            });

            var stage = arena.CurrentStage();
            arena.AddShapeToStage(pipe2, stage.GetName());
            DebugInfo = $"✅ Added pipe2 to scene '{scene.Title}'";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            DebugInfo = $"❌ Error: {ex.Message}\n{ex.StackTrace}";
            StateHasChanged();
        }
    }

    public async void ClearScene()
    {
        try
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null);
            if (!found)
            {
                DebugInfo = "❌ Scene not found";
                StateHasChanged();
                return;
            }

            await scene.ClearAll();
            DebugInfo = $"✅ Cleared scene '{scene.Title}'";
            StateHasChanged();
        }
        catch (Exception ex)
        {
            DebugInfo = $"❌ Error: {ex.Message}\n{ex.StackTrace}";
            StateHasChanged();
        }
    }
}

