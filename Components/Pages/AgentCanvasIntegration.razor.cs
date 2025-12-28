using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using Three2025.Services.Chat;
using Three2025.Apprentice;
using Three2025.Models.Chat;
using Microsoft.Extensions.AI;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;
using FoundryMentorModeler.Model;

namespace Three2025.Components.Pages;

#nullable enable

public partial class AgentCanvasIntegration : ComponentBase
{
    [Inject] protected IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] protected ILogger<AgentCanvasIntegration> Logger { get; set; } = default!;
    [Inject] protected IGeometryTech GeometryTech { get; set; } = default!;
    [Inject] protected IFoundryService FoundryService { get; set; } = default!;
    [Inject] protected IMentorServices MentorServices { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public IModelEditor ModelEditor { get; init; } = null!;
    
    protected Canvas3DComponent Canvas3DReference = default!;
    protected Canvas2DComponent Canvas2DReference = default!;
    
    protected ElementReference chatContainer;
    protected ElementReference logContainer;
    protected ElementReference logScrollAnchor;
    
    protected string userInput = "";
    protected List<AIChatMessage> conversationHistory = new();
    protected List<ChatDisplayMessage> displayMessages => ConvertToDisplayMessages();
    protected List<ActivityLog> activityLogs = new();
    protected List<ActivityLogEntry> activityLogEntries => ConvertToActivityLogEntries();
    protected bool isProcessing = false;
    protected bool autoScrollLogs = true;
    protected int selectedTabIndex = 0;
    protected string activeTreeTab = "shape";
    protected int toolCount = 0;
    protected List<string> availableAgents = new();
    protected string currentAgent = "Assistant";
    protected string streamingResponse = "";
    
    // Queue management for test sequences
    private Queue<string> messageQueue = new();
    private bool isProcessingQueue = false;
    
    protected int CanvasWidth3D = 800;
    protected int CanvasHeight3D = 400;
    protected int CanvasWidth2D = 800;
    protected int CanvasHeight2D = 400;
    
    protected AnimatedKnModel? animatedModel;
    
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
        AddLog("System", "💡 Tool execution logs appear in server console", "Use browser dev tools or server logs to see tool calls");
    }

    public string GetReferenceTo(string filename)
    {
        return Path.Combine(Navigation.BaseUri, filename);
    }

    public void DoRequestAxisToScene()
    {
        var (found, scene) = Canvas3DReference?.GetActiveScene() ?? (false, null!);
        if (!found || scene == null)
        {
            AddLog("Warning", "⚠️ Scene not ready for axis");
            return;
        }

        var model = new FoundryWorldsAndDrawings.ThreeD.Objects.Model3D()
        {
            Name = "Axis",
            Url = GetReferenceTo(@"storage/StaticFiles/fiveMeterAxis.glb"),
            Format = FoundryWorldsAndDrawings.ThreeD.Objects.Model3DFormats.Gltf,
        };

        scene.AddChild(model);
        AddLog("System", "✅ Added coordinate axis to scene");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Check if there's a test query parameter
            var uri = new Uri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var testName = query["test"];
            
            // Calculate canvas sizes based on viewport
            CanvasWidth3D = 800;
            CanvasHeight3D = 350;
            CanvasWidth2D = 800;
            CanvasHeight2D = 350;
            
            // CRITICAL: Connect GeometryTech to the canvas stage
            await Task.Delay(100); // Let canvas initialize
            
            if (Canvas3DReference?.Stage != null)
            {
                GeometryTech.SetStage(Canvas3DReference.Stage);
                AddLog("System", $"✅ Connected GeometryTech to stage '{Canvas3DReference.Stage.Name}'");
                
                // Add coordinate axis
                DoRequestAxisToScene();
                
                // Create AnimatedKnModel through MentorServices so model tree view is aware
                animatedModel = MentorServices.EstablishModel<AnimatedKnModel>("AgentCanvasModel");
                animatedModel.SetExpanded(true);
                animatedModel.EnsureAnimationSetup();
                AddLog("System", $"✅ Created AnimatedKnModel with animation callbacks");
            }
            else
            {
                AddLog("Error", "❌ Canvas3DReference.Stage is null - shapes won't render!");
            }
            
            StateHasChanged();
            
            // Auto-execute test if specified in query parameter
            if (!string.IsNullOrEmpty(testName))
            {
                Logger.LogInformation("🧪 Auto-executing test sequence: {TestName}", testName);
                var test = ChatTestScenarios.GetSequence(testName);
                if (test != null)
                {
                    // Small delay to ensure UI is ready
                    await Task.Delay(500);
                    await HandleTestSequenceSelected(test);
                }
                else
                {
                    Logger.LogWarning("⚠️ Test sequence not found: {TestName}", testName);
                    AddLog("Warning", $"Test sequence '{testName}' not found");
                }
            }
        }
        
        await base.OnAfterRenderAsync(firstRender);
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

    private List<ActivityLogEntry> ConvertToActivityLogEntries()
    {
        return activityLogs.Select(log => new ActivityLogEntry
        {
            Type = log.Type,
            Message = log.Message,
            Details = log.Details,
            Timestamp = log.Timestamp
        }).ToList();
    }

    protected async Task SendMessage()
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

        while (messageQueue.Count > 0)
        {
            var message = messageQueue.Dequeue();
            await ProcessSingleMessage(message);
            
            // Brief pause between queued messages for visual feedback
            if (messageQueue.Count > 0)
            {
                await Task.Delay(500);
            }
        }

        isProcessingQueue = false;
    }

    private async Task ProcessSingleMessage(string message)
    {
        // Note: Don't check isProcessing here - it will be set below
        // The queue ensures sequential processing
        
        Console.WriteLine($"🔵 ProcessSingleMessage START: '{message}'");
        Logger.LogInformation($"🔵 ProcessSingleMessage START: '{message}'");
        
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
            var chunkBuffer = "";
            var lastUpdateTime = DateTime.UtcNow;
            
            // Stream response from orchestrator
            await foreach (var chunk in ChatOrchestrator.ProcessMessageStreamingAsync(
                message,
                pageContext,
                conversationHistory,
                onAgentSwitch: async (agentName) => 
                {
                    currentAgent = agentName;
                    AddLog("Agent Switch", $"Routing to {agentName}", $"Specialized agent selected");
                    Console.WriteLine($"🔀 Agent Switch: {agentName}");
                    await InvokeAsync(StateHasChanged);
                }))
            {
                if (!chunk.IsComplete)
                {
                    // Show chunk content for debugging
                    var preview = chunk.Content?.Length > 50 ? chunk.Content.Substring(0, 50) + "..." : chunk.Content;
                    Console.WriteLine($"📝 Chunk: \"{preview}\" | StreamingResponse length: {streamingResponse?.Length ?? 0}");
                    
                    // Accumulate chunks
                    chunkBuffer += chunk.Content;
                    fullResponse += chunk.Content;
                    
                    // Only update UI every 100ms or when buffer reaches ~10 chars
                    var timeSinceLastUpdate = (DateTime.UtcNow - lastUpdateTime).TotalMilliseconds;
                    if (timeSinceLastUpdate >= 100 || chunkBuffer.Length >= 10)
                    {
                        streamingResponse += chunkBuffer;
                        chunkBuffer = "";
                        currentAgent = chunk.AgentName;
                        lastUpdateTime = DateTime.UtcNow;
                        
                        await InvokeAsync(StateHasChanged);
                        await Task.Delay(50); // Give SignalR time to flush
                    }
                }
                else
                {
                    // Final chunk - flush any remaining buffer
                    if (!string.IsNullOrEmpty(chunkBuffer))
                    {
                        streamingResponse += chunkBuffer;
                        await InvokeAsync(StateHasChanged);
                    }
                    
                    currentAgent = chunk.AgentName;
                    streamingResponse = "";
                    AddLog("Response", $"Received from {chunk.AgentName}", $"Length: {fullResponse.Length} chars");
                    Console.WriteLine($"✅ Response complete: Length={fullResponse.Length}");
                    
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
            
            // Log tool calls from conversation history
            var toolCallMessages = conversationHistory
                .Where(m => m.Role == ChatRole.Tool || 
                           (m.Contents != null && m.Contents.Any(c => c is FunctionCallContent || c is FunctionResultContent)))
                .ToList();
            
            if (toolCallMessages.Any())
            {
                AddLog("Tool Execution", $"🔧 Detected {toolCallMessages.Count} tool-related messages in history", "Check server console for details");
                
                // Log each tool call
                foreach (var toolMsg in toolCallMessages.TakeLast(10))
                {
                    if (toolMsg.Contents != null)
                    {
                        foreach (var content in toolMsg.Contents)
                        {
                            if (content is FunctionCallContent funcCall)
                            {
                                AddLog("Tool Call", $"📞 {funcCall.Name}", $"Calling function with args");
                            }
                            else if (content is FunctionResultContent funcResult)
                            {
                                var resultPreview = funcResult.Result?.ToString() ?? "null";
                                if (resultPreview.Length > 100) resultPreview = resultPreview.Substring(0, 100) + "...";
                                AddLog("Tool Result", $"✅ {funcResult.CallId}", resultPreview);
                            }
                        }
                    }
                }
            }
            
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
    
    protected async Task HandleTestSequenceSelected(TestSequenceMetadata sequence)
    {
        if (isProcessingQueue)
        {
            AddLog("System", "Cannot start test - already processing", 
                "Another test sequence is currently running");
            return;
        }

        AddLog("System", $"🧪 Starting test: {sequence.DisplayName}", 
            $"Loading {sequence.PromptCount} prompts into queue");

        // Load all prompts into the queue
        foreach (var prompt in sequence.Prompts)
        {
            messageQueue.Enqueue(prompt);
        }

        // Start processing
        await ProcessMessageQueue();
        
        AddLog("System", $"✅ Test sequence completed: {sequence.DisplayName}", 
            $"Executed all {sequence.PromptCount} prompts");
    }

    private void NavigateToTestSuites()
    {
        Navigation.NavigateTo("/test-suites");
    }
    
    protected void ShowToolDiagnostics()
    {
        AddLog("System", "🔍 Running tool diagnostics...");
        
        var tools = ChatOrchestrator.GetAllTools().ToList();
        var toolNames = tools.Select(t => t.Name).ToList();
        
        AddLog("Tool Discovery", $"Found {toolNames.Count} tools", string.Join(", ", toolNames));
        
        if (toolNames.Count == 0)
        {
            AddLog("Error", "⚠️ NO TOOLS FOUND! Check server logs for TechnicianToolProvider messages");
            AddLog("System", "Expected: GeometryTech methods like AddShape, GetShapes, etc.");
            AddLog("System", "Check: 1) DI registration, 2) [Description] attributes on methods");
        }
        else
        {
            // Categorize tools
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
        
        // Update the tool count display
        toolCount = toolNames.Count;
        StateHasChanged();
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

    protected void ClearScene()
    {
        try
        {
            GeometryTech.ClearShapes();
            AddLog("Manual Action", "🗑️ Cleared all shapes from scene");
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to clear scene: {ex.Message}");
        }
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
