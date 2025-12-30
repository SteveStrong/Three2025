#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.AI;
using Three2025.Services.Chat;
using Three2025.Models.Chat;
using Three2025.Services.Agents;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Three2025.Components.Pages;

public partial class ChatOrchestratorTest
{
    [Inject] private IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] private IMultiProviderChatService ChatService { get; set; } = default!;
    [Inject] private ITechnicianToolProvider ToolProvider { get; set; } = default!;
    [Inject] private ILogger<ChatOrchestratorTest> Logger { get; set; } = default!;

    private List<AIFunction> allTools = new();


    private string userInput = "";
    private List<AIChatMessage> conversationHistory = new();
    private List<Models.Chat.ChatDisplayMessage> displayMessages => ConvertToDisplayMessages();
    private List<Models.Chat.ActivityLogEntry> activityLogs = new();
    private string streamingResponse = "";
    private bool isProcessing = false;
    private Queue<string> messageQueue = new();
    private bool isProcessingQueue = false;
    private bool autoScrollLogs = true;
    private int toolCount = 0;
    private List<string> availableAgents = new();
    private string currentAgent = "Assistant";
    private string selectedProvider = string.Empty;
    private List<string> AvailableProviders = new();
    private string CurrentProvider = "None";
    private List<string> toolsExecutedInCurrentTurn = new();

    private PageContext pageContext = new()
    {
        PageName = "ChatOrchestratorTest",
        PageRoute = "/chat-test",
        DomainFocus = "General testing with lighting tools",
        AvailableAgents = new List<string>()
    };

    protected override void OnInitialized()
    {
        // Get all tools directly
        allTools = ToolProvider.DiscoverAllTools().ToList();
        toolCount = allTools.Count;
        availableAgents = new List<string> { "3D Modeling Assistant" };

        // Wire up chat service logging to activity log
        ChatService.OnLog += (message) =>
        {
            // Track tool executions
            if (message.Contains("▶️ Executing tool:"))
            {
                var toolName = message.Replace("▶️ Executing tool:", "").Trim();
                toolsExecutedInCurrentTurn.Add(toolName);
            }
            
            // Parse log messages and add them to activity log
            if (message.Contains("Tool call detected:") || message.Contains("Executing tool:"))
            {
                AddLog(ActivityLogType.ToolExecution, message);
            }
            else if (message.Contains("executed successfully"))
            {
                AddLog(ActivityLogType.Response, message);
            }
            else if (message.Contains("ERROR") || message.Contains("failed"))
            {
                AddLog(ActivityLogType.Error, message);
            }
            else
            {
                AddLog(ActivityLogType.System, message);
            }
        };

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
        if (string.IsNullOrWhiteSpace(userInput))
            return;

        var message = userInput.Trim();
        userInput = "";
        
        // Enqueue the message
        messageQueue.Enqueue(message);
        
        // Start processing if not already running
        if (!isProcessingQueue)
        {
            await ProcessMessageQueue();
        }
    }

    private async Task ProcessMessageQueue()
    {
        if (isProcessingQueue || messageQueue.Count == 0)
            return;

        isProcessingQueue = true;
        StateHasChanged();

        while (messageQueue.Count > 0)
        {
            var message = messageQueue.Dequeue();
            await ProcessSingleMessage(message);
            
            // Delay between messages for visual feedback
            if (messageQueue.Count > 0)
            {
                await Task.Delay(500);
            }
        }

        isProcessingQueue = false;
        StateHasChanged();
    }

    private async Task ProcessSingleMessage(string message)
    {
        // Note: Don't check isProcessing here - it will be set below
        // The queue ensures sequential processing
        
        isProcessing = true;
        streamingResponse = "";
        currentAgent = "3D Modeling Assistant";

        AddLog(ActivityLogType.UserInput, message);

        try
        {
            // Add system prompt for 3D modeling
            if (conversationHistory.Count == 0 || conversationHistory.First().Role != ChatRole.System)
            {
                var systemPrompt = $$$"""
                    You are a 3D Modeling Expert Assistant specializing in creating and manipulating 3D geometry.
                    
                    Your expertise includes:
                    - Creating 3D shapes (boxes, spheres, cylinders, cones, etc.)
                    - Positioning and transforming objects
                    - Managing materials and colors
                    - Scene composition and lighting
                    
                    You have access to {{{allTools.Count}}} tools for direct 3D manipulation.
                    
                    When the user asks you to create or modify 3D objects, USE THE TOOLS to perform the actions.
                    After using tools, explain what you did in a friendly, conversational way.
                    """;
                conversationHistory.Insert(0, new AIChatMessage(ChatRole.System, systemPrompt));
            }

            // Add user message
            conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
            await InvokeAsync(StateHasChanged); // Force UI update to show user message
            await ScrollToBottom();

            AddLog(ActivityLogType.Routing, $"Processing with {allTools.Count} tools available...");

            var fullResponse = "";
            toolsExecutedInCurrentTurn.Clear(); // Reset tool tracking

            // Collect ALL streaming chunks before displaying
            // (Don't show the streaming cursor flickering)
            await foreach (var chunk in ChatService.SendMessageStreamingAsync(
                message,
                conversationHistory,
                allTools))
            {
                fullResponse += chunk;
                // DON'T update streamingResponse - collect everything first
            }

            // NOW display the complete response (or handle empty response from tool-only execution)
            if (!string.IsNullOrEmpty(fullResponse))
            {
                streamingResponse = fullResponse;
                await InvokeAsync(StateHasChanged);
                
                // Brief delay to show complete message
                await Task.Delay(300);
                
                // Add to conversation history and clear streaming display
                conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, fullResponse));
                streamingResponse = "";
                await InvokeAsync(StateHasChanged);
            }
            else if (toolsExecutedInCurrentTurn.Any())
            {
                // Tool-only execution - generate summary message
                var toolSummary = $"✅ Executed {toolsExecutedInCurrentTurn.Count} tool(s): {string.Join(", ", toolsExecutedInCurrentTurn)}";
                
                // Show the summary briefly
                streamingResponse = toolSummary;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(500);
                
                // Add to conversation history
                conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, toolSummary));
                streamingResponse = "";
                await InvokeAsync(StateHasChanged);
                
                AddLog(ActivityLogType.Response, $"Tool execution complete: {string.Join(", ", toolsExecutedInCurrentTurn)}");
            }
            else
            {
                // No tools, no text - just add empty message
                conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, ""));
                AddLog(ActivityLogType.Response, "Response complete (no content)");
            }

            AddLog(ActivityLogType.Response, $"Response complete", $"Length: {fullResponse.Length} characters");

            await ScrollToBottom();

            Logger.LogInformation($"Response from {currentAgent}: {fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing message");
            AddLog(ActivityLogType.Error, $"Failed to process message: {ex.Message}");
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

    private List<ChatDisplayMessage> ConvertToDisplayMessages()
    {
        return conversationHistory.Select(msg => new ChatDisplayMessage
        {
            IsUser = msg.Role == ChatRole.User,
            Text = msg.Text ?? "",
            AgentName = msg.Role == ChatRole.Assistant ? currentAgent : null,
            Timestamp = DateTime.Now
        }).ToList();
    }

    private void ShowToolList()
    {
        AddLog(ActivityLogType.System, $"Discovering all available tools...");

        var toolNames = allTools.Select(t => t.Name).ToList();

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

    private async Task HandleTestSequenceSelected(TestSequenceMetadata sequence)
    {
        Logger.LogInformation($"🧪 Starting test sequence: {sequence.Name}");
        AddLog(ActivityLogType.System, $"🧪 Test Sequence: {sequence.DisplayName}", $"Running {sequence.PromptCount} prompts");
        
        // Load all prompts into queue
        foreach (var prompt in sequence.Prompts)
        {
            messageQueue.Enqueue(prompt);
        }
        
        // Process the queue
        await ProcessMessageQueue();
        
        Logger.LogInformation($"✅ Test sequence completed: {sequence.Name}");
        AddLog(ActivityLogType.System, $"✅ Test sequence completed", $"{sequence.DisplayName}");
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
