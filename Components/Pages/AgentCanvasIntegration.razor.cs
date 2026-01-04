using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FoundryWorldsAndDrawings.Shared;
using FoundryWorldsAndDrawings.Solutions;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.PubSub;
using FoundryWorldsAndDrawings.ThreeD.Maths;
using Three2025.Services.Chat;
using Three2025.Apprentice;
using Three2025.Apprentice.RackEquipment;
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
    [Inject] protected IShape3DTech Shape3DTech { get; set; } = default!;
    [Inject] protected IShape2DTech Shape2DTech { get; set; } = default!;
    [Inject] protected IMentor2DTech Mentor2DTech { get; set; } = default!;
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
    protected string activeChatTab = "chat"; // For Chat vs Manual Test tab
    protected int toolCount = 0;
    protected List<string> availableAgents = new();
    protected string currentAgent = "Assistant";
    
    // Rack Equipment State
    protected bool rackIsLoading = false;
    protected string rackStatusMessage = "";
    protected bool rackStatusIsError = false;
    protected int rackCabinetCount = 0;
    protected int rackEquipmentCount = 0;
    protected int rackTotalRUUsed = 0;
    protected int rackAvailableRU = 0;
    protected List<RackCabinetSummary> rackCabinetSummaries = new();
    protected string rackKnowledgeModelSummary = "";
    
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
            
            // CRITICAL: Connect Shape3DTech to the canvas stage
            await Task.Delay(100); // Let canvas initialize
            
            if (Canvas3DReference?.Stage != null)
            {
                Shape3DTech.SetStage(Canvas3DReference.Stage);
                AddLog("System", $"✅ Connected Shape3DTech to stage '{Canvas3DReference.Stage.Name}'");
                
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
            
            // CRITICAL: Connect Shape2DTech to the 2D canvas page
            if (Canvas2DReference?.Page != null)
            {
                Shape2DTech.SetPage(Canvas2DReference.Page);
                AddLog("System", $"✅ Connected Shape2DTech to page '{Canvas2DReference.Page.Name}'");
            }
            else
            {
                AddLog("Error", "❌ Canvas2DReference.Page is null - 2D shapes won't render!");
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
        Console.WriteLine($"🔵 ProcessSingleMessage START: '{message}'");
        Logger.LogInformation($"🔵 ProcessSingleMessage START: '{message}'");
        
        isProcessing = true;
        currentAgent = "Assistant";

        AddLog("User Input", message);

        try
        {
            // Handle special introspection commands
            if (message.Contains("list tools", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("what tools", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("available tools", StringComparison.OrdinalIgnoreCase))
            {
                var toolsDescription = ChatOrchestrator.GetToolsDescription();
                conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
                conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, toolsDescription));
                AddLog("Tool Introspection", $"Listed {ChatOrchestrator.GetToolCount()} available tools");
                await InvokeAsync(StateHasChanged);
                return;
            }
            
            conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
            await InvokeAsync(StateHasChanged);

            AddLog("Routing", "Analyzing intent and selecting agent...");
            Logger.LogInformation("🔍 Calling ProcessMessageAsync (NON-STREAMING mode)");
            
            // Use NON-STREAMING mode for reliable tool execution
            var response = await ChatOrchestrator.ProcessMessageAsync(
                message,
                pageContext,
                conversationHistory,
                onAgentSwitch: async (agentName) => 
                {
                    currentAgent = agentName;
                    AddLog("Agent Switch", $"Routing to {agentName}", $"Specialized agent selected");
                    Console.WriteLine($"🔀 Agent Switch: {agentName}");
                    Logger.LogInformation($"🔀 Agent Switch: {agentName}");
                    await InvokeAsync(StateHasChanged);
                });
            
            var fullResponse = response.Content;
            currentAgent = response.AgentName;
            
            AddLog("Response", $"Received from {response.AgentName}", $"Length: {fullResponse.Length} chars");
            Console.WriteLine($"✅ Response complete: Length={fullResponse.Length}");
            Logger.LogInformation($"✅ Response complete from {response.AgentName}: {fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
            
            // Check if response indicates shape/tool operations
            if (fullResponse.Contains("box", StringComparison.OrdinalIgnoreCase) || 
                fullResponse.Contains("shape", StringComparison.OrdinalIgnoreCase) ||
                fullResponse.Contains("sphere", StringComparison.OrdinalIgnoreCase) ||
                fullResponse.Contains("circle", StringComparison.OrdinalIgnoreCase) ||
                fullResponse.Contains("light", StringComparison.OrdinalIgnoreCase))
            {
                AddLog("Canvas Update", "Agent may have modified canvas", "Check 3D/2D views for changes");
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
            AddLog("System", "Expected: Shape3DTech methods like AddShape, GetShapes, etc.");
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
        try
        {
            var stage = Canvas3DReference?.Stage;
            if (stage == null)
            {
                AddLog("Error", "Canvas3D stage not available");
                return;
            }

            var random = new Random();
            var testName = $"TestBox_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var size = random.NextDouble() * 1.5 + 0.5; // 0.5 to 2.0
            var colors = new[] { "red", "blue", "green", "yellow", "purple", "orange", "cyan", "magenta" };
            var color = colors[random.Next(colors.Length)];
            
            // Create shape using factory methods directly instead of GeometryShape
            var factoryShape = new FoShape3D("factory");
            var shape = factoryShape.CreateBox(testName, size, size, size);
            shape.Color = color;
            
            // Set spatial formatter like GeometryShape did
            shape.ComputeTreeNodeTitle = TreeNodeFormatters.Spatial;
            
            // Add text tag like GeometryShape did
            var tag = new FoText3D("tag")
            {
                Text = testName,
                FontSize = 0.5,
                Transform = new Transform3("TagTransform")
                {
                    Position = new Vector3(3, 0, 0),
                },
                Color = "black"
            };
            shape.AddShape(tag);
            
            // Random position: X: -5 to 5, Y: 0 to 5, Z: -5 to 5
            var x = (float)(random.NextDouble() * 10 - 5);
            var y = (float)(random.NextDouble() * 5);
            var z = (float)(random.NextDouble() * 10 - 5);
            shape.Transform.Position = new Vector3(x, y, z);
            
            stage.AddShape(shape);
            AddLog("Manual Action", $"✅ Added {color} box '{testName}' size={size:F2} at ({x:F1},{y:F1},{z:F1})", "3D Canvas API");
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to add test box: {ex.Message}");
        }
    }

    protected void ClearScene()
    {
        try
        {
            Shape3DTech.ClearShapes();
            AddLog("Manual Action", "🗑️ Cleared all shapes from scene");
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to clear scene: {ex.Message}");
        }
    }

    protected void AddTestCircle()
    {
        try
        {
            var page = Canvas2DReference?.Page;
            if (page == null)
            {
                AddLog("Error", "Canvas2D page not available");
                return;
            }

            var random = new Random();
            var testName = $"TestCircle_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var radius = random.Next(20, 80); // radius 20-80
            var diameter = radius * 2;
            var colors = new[] { "red", "blue", "green", "yellow", "purple", "orange", "cyan", "magenta" };
            var color = colors[random.Next(colors.Length)];
            
            var shape = new FoShape2D(diameter, diameter, color)
            {
                Name = testName
            };
            shape.ShapeDraw = shape.DrawCircle;
            
            // Random position within canvas bounds (assuming 1800x1200 canvas)
            var x = random.Next(radius + 50, 1800 - radius - 50);
            var y = random.Next(radius + 50, 1200 - radius - 50);
            shape.MoveTo(x, y);
            
            page.AddShape(shape);
            AddLog("Manual Action", $"✅ Added {color} circle '{testName}' radius={radius} at ({x},{y})", "2D Canvas API");
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to add test circle: {ex.Message}");
        }
    }

    protected async Task Reset3D()
    {
        try
        {
            var stage = Canvas3DReference?.Stage;
            if (stage != null)
            {
                await stage.ClearAll();
                AddLog("Manual Action", "🗑️ Reset 3D Canvas - cleared stage via canvas API");
            }
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to reset 3D: {ex.Message}");
        }
    }

    protected void ForceRefresh3D()
    {
        try
        {
            FoundryService.PubSub().Publish<RefreshRenderMessage>(RefreshRenderMessage.ClearAllSelected());
            AddLog("Manual Action", "🔄 Forced 3D canvas refresh");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to refresh 3D: {ex.Message}");
        }
    }

    protected void Reset2D()
    {
        try
        {
            var page = Canvas2DReference?.Page;
            if (page != null)
            {
                var drawing = FoundryService.Drawing();
                drawing?.ClearAll();
                AddLog("Manual Action", "🗑️ Reset 2D Canvas - cleared drawing via canvas API");
            }
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to reset 2D: {ex.Message}");
        }
    }

    protected void ForceRefresh2D()
    {
        try
        {
            FoundryService.PubSub().Publish<RefreshUIEvent>(RefreshUIEvent.External("AgentCanvas2D"));
            AddLog("Manual Action", "🔄 Forced 2D canvas refresh");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            AddLog("Error", $"Failed to refresh 2D: {ex.Message}");
        }
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
    
    // ================================================================
    // RACK EQUIPMENT METHODS
    // ================================================================
    
    protected async Task RackEquip_CreateAllCabinets()
    {
        await RackEquip_ExecuteWithLoading(async () =>
        {
            var stage = Canvas3DReference?.Stage;
            if (stage == null)
            {
                RackEquip_SetStatus("Stage not initialized", isError: true);
                return;
            }

            RackEquip_ClearCabinets();

            var cabinets = MFCabinetFactory.CreateAllMFCabinets(spacing: 25.0);
            foreach (var cabinet in cabinets)
            {
                stage.AddShape(cabinet);
            }

            RackEquip_UpdateStatistics();
            RackEquip_SetStatus($"Successfully created {rackCabinetCount} cabinets with {rackEquipmentCount} devices", isError: false);

            await Task.Delay(100);
            StateHasChanged();
        });
    }
    
    protected async Task RackEquip_CreateFromKnowledgeModel()
    {
        await RackEquip_ExecuteWithLoading(async () =>
        {
            var stage = Canvas3DReference?.Stage;
            if (stage == null)
            {
                RackEquip_SetStatus("Stage not initialized", isError: true);
                return;
            }

            RackEquip_ClearCabinets();

            // Create knowledge model
            var dataCenterModel = new DataCenterRackModel("MF_DataCenter");
            
            // Get summary from knowledge model
            rackKnowledgeModelSummary = RackKnowledgeToFoFactory.GetKnowledgeModelSummary(dataCenterModel);
            
            // Generate FO objects directly into stage (technician operates on stage, doesn't create it)
            RackKnowledgeToFoFactory.GenerateDataCenter(dataCenterModel, stage);

            RackEquip_UpdateStatistics();
            RackEquip_SetStatus($"✅ Generated from Knowledge Model: {rackCabinetCount} cabinets with {rackEquipmentCount} devices", isError: false);

            await Task.Delay(100);
            StateHasChanged();
        });
    }

    protected async Task RackEquip_CreateCabinet1() => await RackEquip_CreateSingleCabinet(() => MFCabinetFactory.CreateMFCabinet1(), "MF Cabinet 1");
    protected async Task RackEquip_CreateCabinet2() => await RackEquip_CreateSingleCabinet(() => MFCabinetFactory.CreateMFCabinet2(), "MF Cabinet 2");
    protected async Task RackEquip_CreateCabinet3() => await RackEquip_CreateSingleCabinet(() => MFCabinetFactory.CreateMFCabinet3(), "MF Cabinet 3");
    protected async Task RackEquip_CreateCabinet4() => await RackEquip_CreateSingleCabinet(() => MFCabinetFactory.CreateMFCabinet4(), "MF Cabinet 4");

    private async Task RackEquip_CreateSingleCabinet(Func<RackCabinetShape> factory, string name)
    {
        await RackEquip_ExecuteWithLoading(async () =>
        {
            var stage = Canvas3DReference?.Stage;
            if (stage == null)
            {
                RackEquip_SetStatus("Stage not initialized", isError: true);
                return;
            }

            var (found, existing) = stage.FindMember<RackCabinetShape>(name.Replace(" ", "_"));
            if (found && existing != null)
            {
                stage.RemoveShape(existing);
            }

            var cabinet = factory();
            stage.AddShape(cabinet);

            RackEquip_UpdateStatistics();

            var equipCount = cabinet.GetEquipment().Count;
            var availRU = cabinet.GetAvailableRU();
            RackEquip_SetStatus($"Created {name}: {equipCount} devices, {availRU} RU available", isError: false);

            await Task.Delay(50);
            StateHasChanged();
        });
    }

    protected void RackEquip_ClearCabinets()
    {
        var stage = Canvas3DReference?.Stage;
        if (stage == null) return;

        var cabinets = stage.GetMembers<RackCabinetShape>()?.ToList();
        if (cabinets == null) return;
        
        foreach (var cabinet in cabinets)
        {
            stage.RemoveShape(cabinet);
        }

        rackKnowledgeModelSummary = "";  // Clear knowledge model summary
        RackEquip_UpdateStatistics();
        RackEquip_SetStatus("Cabinets cleared", isError: false);
        StateHasChanged();
    }

    private void RackEquip_UpdateStatistics()
    {
        var stage = Canvas3DReference?.Stage;
        if (stage == null)
        {
            rackCabinetCount = 0;
            rackEquipmentCount = 0;
            rackTotalRUUsed = 0;
            rackAvailableRU = 0;
            rackCabinetSummaries.Clear();
            return;
        }

        var cabinets = stage.GetMembers<RackCabinetShape>()?.ToList() ?? new List<RackCabinetShape>();
        rackCabinetCount = cabinets.Count;
        rackEquipmentCount = 0;
        rackTotalRUUsed = 0;
        rackAvailableRU = 0;
        rackCabinetSummaries.Clear();

        foreach (var cabinet in cabinets)
        {
            var equipment = cabinet.GetEquipment();
            var usedRU = equipment.Sum(e => (int)Math.Ceiling(e.HeightInRU));
            var availRU = cabinet.GetAvailableRU();

            rackEquipmentCount += equipment.Count;
            rackTotalRUUsed += usedRU;
            rackAvailableRU += availRU;

            rackCabinetSummaries.Add(new RackCabinetSummary
            {
                Name = cabinet.Key,
                DeviceCount = equipment.Count,
                UsedRU = usedRU,
                TotalRU = RackCabinetShape.TOTAL_RACK_UNITS,
                HasPDU = cabinet.HasPDU,
                BorderColor = RackEquip_GetCabinetColor(cabinet.Key)
            });
        }
    }

    private string RackEquip_GetCabinetColor(string cabinetName) => cabinetName switch
    {
        "MF_Cabinet_1" => "#8e44ad",
        "MF_Cabinet_2" => "#9b59b6",
        "MF_Cabinet_3" => "#a569bd",
        "MF_Cabinet_4" => "#bb8fce",
        _ => "#6c757d"
    };

    private void RackEquip_SetStatus(string message, bool isError)
    {
        rackStatusMessage = message;
        rackStatusIsError = isError;

        if (isError)
        {
            FoundryService.Toast().Error(message);
        }
        else
        {
            FoundryService.Toast().Success(message);
        }
    }

    private async Task RackEquip_ExecuteWithLoading(Func<Task> action)
    {
        try
        {
            rackIsLoading = true;
            StateHasChanged();
            await action();
        }
        catch (Exception ex)
        {
            RackEquip_SetStatus($"Error: {ex.Message}", isError: true);
        }
        finally
        {
            rackIsLoading = false;
            StateHasChanged();
        }
    }

    protected class RackCabinetSummary
    {
        public string Name { get; set; } = "";
        public int DeviceCount { get; set; }
        public int UsedRU { get; set; }
        public int TotalRU { get; set; }
        public bool HasPDU { get; set; }
        public string BorderColor { get; set; } = "";
    }
}
