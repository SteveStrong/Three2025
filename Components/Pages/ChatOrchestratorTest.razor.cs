#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.AI;
using Three2025.Services.Chat;
using Three2025.Models.Chat;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Three2025.Components.Pages;

public partial class ChatOrchestratorTest
{
    [Inject] private IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] private IMultiProviderChatService ChatService { get; set; } = default!;
    [Inject] private ILogger<ChatOrchestratorTest> Logger { get; set; } = default!;


    private string userInput = "";
    private List<AIChatMessage> conversationHistory = new();
    private List<Models.Chat.ChatDisplayMessage> displayMessages = new();
    private List<Models.Chat.ActivityLogEntry> activityLogs = new();
    private string streamingResponse = "";
    private bool isProcessing = false;
    private bool autoScrollLogs = true;
    private int toolCount = 0;
    private List<string> availableAgents = new();
    private string currentAgent = "Assistant";
    private string selectedProvider = string.Empty;
    private List<string> AvailableProviders = new();
    private string CurrentProvider = "None";

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

        // Initialize providers with GitHub as default
        AvailableProviders = ChatService.AvailableProviders.ToList();
        if (AvailableProviders.Contains("GitHub"))
        {
            selectedProvider = "GitHub";
            ChatService.SetProvider("GitHub");
        }
        else if (AvailableProviders.Any())
        {
            selectedProvider = AvailableProviders.First();
            ChatService.SetProvider(selectedProvider);
        }
        CurrentProvider = ChatService.CurrentProvider;

        AddLog(ActivityLogType.System, $"Initialized with {toolCount} tools and {availableAgents.Count} agents", string.Join(", ", availableAgents));
        AddLog(ActivityLogType.System, $"Active provider: {CurrentProvider}", "");
        Logger.LogInformation($"ChatOrchestrator Test initialized: {toolCount} tools, {availableAgents.Count} agents, Provider: {CurrentProvider}");
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

        AddLog(ActivityLogType.UserInput, message);

        try
        {
            // Add user message
            conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
            displayMessages.Add(new Models.Chat.ChatDisplayMessage 
            { 
                IsUser = true, 
                Text = message 
            });
            await ScrollToBottom();

            AddLog(ActivityLogType.Routing, "Analyzing intent and selecting agent...");

            var fullResponse = "";

            // Stream response from orchestrator
            await foreach (var chunk in ChatOrchestrator.ProcessMessageStreamingAsync(
                message,
                pageContext,
                conversationHistory,
                onAgentSwitch: async (agentName) =>
                {
                    currentAgent = agentName;
                    AddLog(ActivityLogType.AgentSwitch, $"Routing to {agentName}", $"Specialized agent selected based on intent analysis");
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
                    AddLog(ActivityLogType.Response, $"Received from {chunk.AgentName}", $"Length: {fullResponse.Length} characters");

                    // Check if tools were likely used (simplified heuristic)
                    if (fullResponse.Contains("light") || fullResponse.Contains("position") || fullResponse.Contains("color"))
                    {
                        AddLog(ActivityLogType.ToolExecution, "LLM may have used lighting tools", "Tool usage detected in response context");
                    }
                }
            }

            // Add assistant response to conversation history
            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, fullResponse));
            displayMessages.Add(new Models.Chat.ChatDisplayMessage 
            { 
                IsUser = false, 
                Text = fullResponse,
                AgentName = currentAgent
            });
            streamingResponse = "";

            await ScrollToBottom();

            Logger.LogInformation($"Response from {currentAgent}: {fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
            AddLog(ActivityLogType.Error, $"Failed to process message: {ex.Message}");
            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, $"❌ Error: {ex.Message}"));
            displayMessages.Add(new Models.Chat.ChatDisplayMessage 
            { 
                IsUser = false, 
                Text = $"❌ Error: {ex.Message}"
            });
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
        displayMessages.Clear();
        currentAgent = "Assistant";
        AddLog(ActivityLogType.System, "Chat cleared");
        StateHasChanged();
    }

    private void ClearLogs()
    {
        activityLogs.Clear();
        AddLog(ActivityLogType.System, "Logs cleared");
    }

    private void ShowToolList()
    {
        AddLog(ActivityLogType.System, $"Discovering all available tools...");

        var tools = ChatOrchestrator.GetAllTools();
        var toolNames = tools.Select(t => t.Name).ToList();

        AddLog(ActivityLogType.ToolDiscovery, $"Found {toolNames.Count} tools", string.Join(", ", toolNames));

        // Show by category
        var geometryTools = toolNames.Where(t => t.Contains("Shape") || t.Contains("Geometry")).ToList();
        var clockTools = toolNames.Where(t => t.Contains("Clock") || t.Contains("Time")).ToList();
        var otherTools = toolNames.Except(geometryTools).Except(clockTools).ToList();

        if (geometryTools.Any())
            AddLog(ActivityLogType.GeometryTools, $"{geometryTools.Count} tools", string.Join(", ", geometryTools));

        if (clockTools.Any())
            AddLog(ActivityLogType.ClockTools, $"{clockTools.Count} tools", string.Join(", ", clockTools));

        if (otherTools.Any())
            AddLog(ActivityLogType.OtherTools, $"{otherTools.Count} tools", string.Join(", ", otherTools));
    }

    private void OnProviderChanged()
    {
        if (ChatService.SetProvider(selectedProvider))
        {
            CurrentProvider = ChatService.CurrentProvider;
            AddLog(ActivityLogType.System, $"✓ Switched provider to: {CurrentProvider}", "");
            StateHasChanged();
        }
    }

    private void AddLog(string type, string message, string? details = null)
    {
        activityLogs.Add(new Models.Chat.ActivityLogEntry
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
}
