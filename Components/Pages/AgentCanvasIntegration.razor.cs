using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FoundryWorldsAndDrawings.Shared;
using Three2025.Services.Chat;
using Microsoft.Extensions.AI;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Three2025.Components.Pages;

public partial class AgentCanvasIntegration : ComponentBase
{
    [Inject] protected IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] protected ILogger<AgentCanvasIntegration> Logger { get; set; } = default!;
    
    protected Canvas3DComponent? Canvas3DReference;
    protected Canvas2DComponent? Canvas2DReference;
    
    protected ElementReference chatContainer;
    protected ElementReference logContainer;
    protected ElementReference logScrollAnchor;
    
    protected string userInput = "";
    protected List<AIChatMessage> conversationHistory = new();
    protected List<ActivityLog> activityLogs = new();
    protected bool isProcessing = false;
    protected bool autoScrollLogs = true;
    protected int selectedTabIndex = 0;
    protected string activeTreeTab = "shape";
    protected int toolCount = 0;
    protected List<string> availableAgents = new();
    protected string currentAgent = "Assistant";
    protected string streamingResponse = "";
    
    protected int CanvasWidth3D = 800;
    protected int CanvasHeight3D = 400;
    protected int CanvasWidth2D = 800;
    protected int CanvasHeight2D = 400;
    
    private PageContext pageContext = new()
    {
        PageName = "AgentCanvasIntegration",
        PageRoute = "/agent-canvas",
        DomainFocus = "3D and 2D shape creation and manipulation with AI agents",
        AvailableAgents = new List<string>()
    };

    protected override void OnInitialized()
    {
        toolCount = ChatOrchestrator.GetToolCount();
        availableAgents = ChatOrchestrator.GetAvailableAgents(pageContext);
        
        AddLog("System", $"Initialized with {toolCount} tools and {availableAgents.Count} agents", string.Join(", ", availableAgents));
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Calculate canvas sizes based on viewport
            CanvasWidth3D = 800;
            CanvasHeight3D = 350;
            CanvasWidth2D = 800;
            CanvasHeight2D = 350;
            StateHasChanged();
        }
    }

    protected async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(userInput) || isProcessing)
            return;

        var message = userInput.Trim();
        userInput = "";
        isProcessing = true;
        streamingResponse = "";
        currentAgent = "Assistant";

        AddLog("User Input", message);

        try
        {
            conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
            await InvokeAsync(StateHasChanged);

            AddLog("Routing", "Analyzing intent and selecting agent...");

            var fullResponse = "";
            
            // Stream response from orchestrator
            await foreach (var chunk in ChatOrchestrator.ProcessMessageStreamingAsync(
                message,
                pageContext,
                conversationHistory,
                onAgentSwitch: async (agentName) => 
                {
                    currentAgent = agentName;
                    AddLog("Agent Switch", $"Routing to {agentName}", $"Specialized agent selected");
                    await InvokeAsync(StateHasChanged);
                }))
            {
                if (!chunk.IsComplete)
                {
                    // Update streaming response
                    streamingResponse += chunk.Content;
                    fullResponse += chunk.Content;
                    currentAgent = chunk.AgentName;
                    await InvokeAsync(StateHasChanged);
                }
                else
                {
                    // Final chunk
                    currentAgent = chunk.AgentName;
                    streamingResponse = "";
                    AddLog("Response", $"Received from {chunk.AgentName}", $"Length: {fullResponse.Length} chars");
                    
                    // Check if response indicates shape/tool operations
                    if (fullResponse.Contains("box", StringComparison.OrdinalIgnoreCase) || 
                        fullResponse.Contains("shape", StringComparison.OrdinalIgnoreCase) ||
                        fullResponse.Contains("light", StringComparison.OrdinalIgnoreCase))
                    {
                        AddLog("Canvas Update", "Agent may have modified canvas", "Check 3D/2D views for changes");
                    }
                }
            }

            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, fullResponse));
            
            Logger.LogInformation($"Response from {currentAgent}: {fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
            AddLog("Error", $"Failed to process message: {ex.Message}");
            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, $"❌ Error: {ex.Message}"));
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

    protected async Task QuickTest(string message)
    {
        userInput = message;
        await SendMessage();
    }

    protected void ClearChat()
    {
        conversationHistory.Clear();
        currentAgent = "Assistant";
        AddLog("System", "Chat cleared");
        StateHasChanged();
    }

    protected void ClearLogs()
    {
        activityLogs.Clear();
        AddLog("System", "Logs cleared");
    }

    protected void ToggleAutoScroll()
    {
        autoScrollLogs = !autoScrollLogs;
        StateHasChanged();
    }

    protected void AddLog(string type, string message, string? details = null)
    {
        activityLogs.Add(new ActivityLog
        {
            Type = type,
            Message = message,
            Details = details ?? "",
            Timestamp = DateTime.Now
        });
        
        if (activityLogs.Count > 100)
        {
            activityLogs.RemoveAt(0);
        }
        
        InvokeAsync(async () => 
        {
            StateHasChanged();
            if (autoScrollLogs)
            {
                await Task.Delay(50);
            }
        });
    }

    // Test methods for manual shape addition
    protected void AddTestBox()
    {
        AddLog("Manual Action", "Add Test Box clicked", "3D Canvas");
        // Canvas will be populated by agents via tools
    }

    protected void AddTestCircle()
    {
        AddLog("Manual Action", "Add Test Circle clicked", "2D Canvas");
        // Canvas will be populated by agents via tools
    }

    protected void Reset3D()
    {
        AddLog("Manual Action", "Reset 3D Canvas", "Clearing 3D scene");
        // Implementation would clear 3D scene
    }

    protected void Reset2D()
    {
        AddLog("Manual Action", "Reset 2D Canvas", "Clearing 2D drawing");
        // Implementation would clear 2D drawing
    }

    protected string GetLogColor(string type) => type switch
    {
        "User Input" => "#4ec9b0",
        "Agent Switch" => "#dcdcaa",
        "Tool Execution" => "#ce9178",
        "Canvas Update" => "#4fc1ff",
        "Response" => "#9cdcfe",
        "Routing" => "#c586c0",
        "Manual Action" => "#b5cea8",
        "Error" => "#f48771",
        "System" => "#608b4e",
        _ => "#d4d4d4"
    };

    protected string GetLogBorderColor(string type) => type switch
    {
        "User Input" => "#4ec9b0",
        "Agent Switch" => "#dcdcaa",
        "Tool Execution" => "#ce9178",
        "Canvas Update" => "#4fc1ff",
        "Response" => "#9cdcfe",
        "Routing" => "#c586c0",
        "Manual Action" => "#b5cea8",
        "Error" => "#f48771",
        "System" => "#608b4e",
        _ => "#3e3e3e"
    };

    protected string GetLogBackgroundColor(string type) => type switch
    {
        "Error" => "#3d2422",
        "Tool Execution" => "#2d2a26",
        "Agent Switch" => "#2d2d2a",
        "Canvas Update" => "#1a2d3d",
        _ => "#262626"
    };

    protected string GetLogIcon(string type) => type switch
    {
        "User Input" => "💬",
        "Agent Switch" => "🔀",
        "Tool Execution" => "🔧",
        "Canvas Update" => "🎨",
        "Response" => "💡",
        "Routing" => "🧭",
        "Manual Action" => "👆",
        "Error" => "❌",
        "System" => "⚙️",
        _ => "📋"
    };

    protected class ActivityLog
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public string Details { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
