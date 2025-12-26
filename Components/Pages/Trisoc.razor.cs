using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FoundryRulesAndUnits.Extensions; // ✅ Phase 0.5: For WriteSuccess extension


using FoundryWorldsAndDrawings.PubSub;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;
using FoundryWorldsAndDrawings.ThreeD.Viewers;
using FoundryWorldsAndDrawings.ThreeD.Objects;
using FoundryWorldsAndDrawings.Shape; // ✅ Phase 0.5: For FoStage3D
using Three2025.Services.Chat;
using Microsoft.Extensions.AI;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;


namespace Three2025.Components.Pages;

public partial class TrisocBase : ComponentBase
{
    public FoundryWorldsAndDrawings.Shared.Canvas3DComponent Canvas3DReference = null;
    private FoStage3D _trisocStage; // ✅ Phase 0.5: Track this page's stage

    [Inject] public NavigationManager Navigation { get; set; }

    [Inject] public IWorkspace Workspace { get; init; }
    [Inject] public IFoundryService FoundryService { get; init; }
    [Inject] public ITrisocTech Tech { get; init; }
    [Inject] public ILightingTech LightTech { get; init; }
    [Inject] public IChatOrchestrator ChatOrchestrator { get; set; }
    [Inject] public ILogger<TrisocBase> Logger { get; set; }


    [Parameter] public int CanvasWidth { get; set; } = 1200;
    [Parameter] public int CanvasHeight { get; set; } = 1000;


    protected MockDataGenerator DataGenerator { get; set; } = new();

    // Chat fields
    protected ElementReference chatContainer;
    protected string userInput = "";
    protected List<AIChatMessage> conversationHistory = new();
    protected bool isProcessing = false;
    protected string currentAgent = "Assistant";
    protected string streamingResponse = "";
    
    private PageContext pageContext = new()
    {
        PageName = "Trisoc",
        PageRoute = "/trisoc",
        DomainFocus = "3D shapes and trisoc construction"
    };




    protected override void OnInitialized()
    {
        Workspace.SetBaseUrl(Navigation?.BaseUri ?? "");
        base.OnInitialized();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);

            var arena = Workspace.GetArena();
            if (found)
            {
                // ✅ Phase 0.5: Get this page's stage (Canvas already linked it to scene)
                _trisocStage = arena.EstablishStage<FoStage3D>(Canvas3DReference.SceneName);
                $"Trisoc: Retrieved stage '{_trisocStage?.Name}' from Canvas".WriteSuccess();
                DoRequestAxisToScene(scene!);
            }

        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public void DoRequestAxisToScene(Scene3D scene)
    {
        var model = new Model3D()
        {
            Name = "Axis",
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = Model3DFormats.Gltf,
        };

        scene.AddChild(model);
    }


    public string GetReferenceTo(string filename)
    {
        var path = Path.Combine(Navigation.BaseUri, filename);
        //path.WriteSuccess();
        return path;
    }



    public void DoAddTRISOCToArena()
    {
        var url = GetReferenceTo(@"storage/StaticFiles/TRISOC.glb");
        Tech.GetTrisocModel(url);
    }

    public void DoAddRackArena()
    {
        var url = GetReferenceTo(@"storage/StaticFiles/8625799.glb");
        Tech.CreateModel("8625799", url);
    }

    public void DoAddBoxArena()
    {

        var (c, center) = Tech.GetSpacialBox("Center", 0, "C");
        center.Transform.MoveBy(0, 4.2, 0);

        var (t, top) = Tech.GetSpacialBox("Top", c, "T");
        top.Transform.Position = center.Transform.Position;
        top.Transform.MoveBy(0, -10, 0);

        var (f, front) = Tech.GetSpacialBox("Front", t, "F");
        front.Transform.Position = center.Transform.Position;
        front.Transform.MoveBy(0, 0, 10);


        // ✅ Phase 0.5: Add shapes to this page's stage
        _trisocStage?.AddShape(center);
        _trisocStage?.AddShape(top);
        _trisocStage?.AddShape(front);


    }

    public void DoStartStopTimer()
    {
        Tech.StartStopTimer();
    }


    public void DoReposition()
    {
        var list = LightTech.GetLights();
        for (int i = 0; i < list.Count - 1; i++)
        {
            var pos1 = list[i].Transform.Position;
            var light = list[i + 1].GetName();
            LightTech.RepositionLight(light, pos1.X + 5, pos1.Y + 5, 0);
        }

    }

    // Chat methods
    protected async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(userInput) || isProcessing)
            return;

        var message = userInput.Trim();
        userInput = "";
        isProcessing = true;
        streamingResponse = "";
        currentAgent = "Assistant";

        try
        {
            conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
            await InvokeAsync(StateHasChanged);

            var fullResponse = "";
            
            await foreach (var chunk in ChatOrchestrator.ProcessMessageStreamingAsync(
                message,
                pageContext,
                conversationHistory,
                onAgentSwitch: async (agentName) => 
                {
                    currentAgent = agentName;
                    await InvokeAsync(StateHasChanged);
                }))
            {
                if (!chunk.IsComplete)
                {
                    streamingResponse += chunk.Content;
                    fullResponse += chunk.Content;
                    currentAgent = chunk.AgentName;
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    currentAgent = chunk.AgentName;
                }
            }

            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, fullResponse));
            streamingResponse = "";
            
            Logger.LogInformation($"Response from {currentAgent}: {fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, $"❌ Error: {ex.Message}"));
            streamingResponse = "";
        }
        finally
        {
            isProcessing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }
}

