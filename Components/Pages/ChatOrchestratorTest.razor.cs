using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.AI;
using Three2025.Services.Chat;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Three2025.Components.Pages;

public partial class ChatOrchestratorTest
{
    [Inject] private IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] private ILogger<ChatOrchestratorTest> Logger { get; set; } = default!;

    private ElementReference chatContainer;
    private ElementReference logContainer;
    private ElementReference logScrollAnchor;
    private string userInput = "";
    private List<AIChatMessage> conversationHistory = new();
    private List<ActivityLog> activityLogs = new();
    private string streamingResponse = "";
    private bool isProcessing = false;
    private bool autoScrollLogs = true;
    private int toolCount = 0;
    private List<string> availableAgents = new();
    private string currentAgent = "Assistant";

    private PageContext pageContext = new()
    {
        PageName = "ChatOrchestratorTest",
        PageRoute = "/chat-test",
        DomainFocus = "General testing with lighting tools",
        AvailableAgents = new List<string>()
    };

    protected override void OnInitialized()
    {
        toolCount = ChatOrchestrator.GetToolCount();
        availableAgents = ChatOrchestrator.GetAvailableAgents(pageContext);

        AddLog("System", $"Initialized with {toolCount} tools and {availableAgents.Count} agents", string.Join(", ", availableAgents));
        Logger.LogInformation($"ChatOrchestrator Test initialized: {toolCount} tools, {availableAgents.Count} agents");
    }

    private async Task SendMessage()
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
            // Add user message
            conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
            await ScrollToBottom();

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
                    AddLog("Agent Switch", $"Routing to {agentName}", $"Specialized agent selected based on intent analysis");
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
                    // Final chunk - complete the response
                    currentAgent = chunk.AgentName;
                    AddLog("Response", $"Received from {chunk.AgentName}", $"Length: {fullResponse.Length} characters");

                    // Check if tools were likely used (simplified heuristic)
                    if (fullResponse.Contains("light") || fullResponse.Contains("position") || fullResponse.Contains("color"))
                    {
                        AddLog("Tool Execution", "LLM may have used lighting tools", "Tool usage detected in response context");
                    }
                }
            }

            // Add assistant response to conversation history
            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, fullResponse));
            streamingResponse = "";

            await ScrollToBottom();

            Logger.LogInformation($"Response from {currentAgent}: {fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
            AddLog("Error", $"Failed to process message: {ex.Message}");
            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, $"❌ Error: {ex.Message}"));
            streamingResponse = "";
        }
        finally
        {
            isProcessing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ScrollToBottom()
    {
        try
        {
            await InvokeAsync(StateHasChanged);
            // Small delay to allow DOM to update
            await Task.Delay(50);
        }
        catch { /* Ignore scroll errors */ }
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    private async Task QuickTest(string message)
    {
        userInput = message;
        await SendMessage();
    }

    private void ClearChat()
    {
        conversationHistory.Clear();
        currentAgent = "Assistant";
        AddLog("System", "Chat cleared");
        StateHasChanged();
    }

    private void ClearLogs()
    {
        activityLogs.Clear();
        AddLog("System", "Logs cleared");
    }

    private void ShowToolList()
    {
        AddLog("System", $"Discovering all available tools...");

        var tools = ChatOrchestrator.GetAllTools();
        var toolNames = tools.Select(t => t.Name).ToList();

        AddLog("Tool Discovery", $"Found {toolNames.Count} tools", string.Join(", ", toolNames));

        // Show by category
        var geometryTools = toolNames.Where(t => t.Contains("Shape") || t.Contains("Geometry")).ToList();
        var clockTools = toolNames.Where(t => t.Contains("Clock") || t.Contains("Time")).ToList();
        var otherTools = toolNames.Except(geometryTools).Except(clockTools).ToList();

        if (geometryTools.Any())
            AddLog("Geometry Tools", $"{geometryTools.Count} tools", string.Join(", ", geometryTools));

        if (clockTools.Any())
            AddLog("Clock Tools", $"{clockTools.Count} tools", string.Join(", ", clockTools));

        if (otherTools.Any())
            AddLog("Other Tools", $"{otherTools.Count} tools", string.Join(", ", otherTools));
    }

    private void AddLog(string type, string message, string? details = null)
    {
        activityLogs.Add(new ActivityLog
        {
            Type = type,
            Message = message,
            Details = details ?? "",
            Timestamp = DateTime.Now
        });

        // Keep only last 100 logs to avoid memory issues
        if (activityLogs.Count > 100)
        {
            activityLogs.RemoveAt(0);
        }

        InvokeAsync(async () =>
        {
            StateHasChanged();
            if (autoScrollLogs)
            {
                await Task.Delay(50); // Allow DOM to update
                await ScrollLogsToBottom();
            }
        });
    }

    private async Task ScrollLogsToBottom()
    {
        try
        {
            // Scroll the log container to bottom using the anchor element
            await Task.CompletedTask; // Placeholder for JS interop if needed
        }
        catch { /* Ignore scroll errors */ }
    }

    private void ToggleAutoScroll()
    {
        autoScrollLogs = !autoScrollLogs;
        StateHasChanged();
    }

    private string GetLogColor(string type) => type switch
    {
        "User Input" => "#4ec9b0",
        "Agent Switch" => "#dcdcaa",
        "Tool Execution" => "#ce9178",
        "Response" => "#9cdcfe",
        "Routing" => "#c586c0",
        "Error" => "#f48771",
        "System" => "#608b4e",
        _ => "#d4d4d4"
    };

    private string GetLogBorderColor(string type) => type switch
    {
        "User Input" => "#4ec9b0",
        "Agent Switch" => "#dcdcaa",
        "Tool Execution" => "#ce9178",
        "Response" => "#9cdcfe",
        "Routing" => "#c586c0",
        "Error" => "#f48771",
        "System" => "#608b4e",
        _ => "#3e3e3e"
    };

    private string GetLogBackgroundColor(string type) => type switch
    {
        "Error" => "#3d2422",
        "Tool Execution" => "#2d2a26",
        "Agent Switch" => "#2d2d2a",
        _ => "#262626"
    };

    private string GetLogIcon(string type) => type switch
    {
        "User Input" => "💬",
        "Agent Switch" => "🔀",
        "Tool Execution" => "🔧",
        "Response" => "💡",
        "Routing" => "🧭",
        "Error" => "❌",
        "System" => "⚙️",
        _ => "📋"
    };

    private class ActivityLog
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public string Details { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
