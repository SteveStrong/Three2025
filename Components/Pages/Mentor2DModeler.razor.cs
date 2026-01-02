using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using FoundryMentorModeler.Model;
using FoundryMentorModeler.Shared;
using FoundryWorldsAndDrawings.Shape;
using FoundryWorldsAndDrawings.Shared;
using Three2025.Components.Shared.Chat;
using Three2025.Models.Chat;
using Three2025.Services.Chat;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;
using FoundryWorldsAndDrawings.Solutions;
using Three2025.Apprentice; // For IModelTech

#nullable enable

namespace Three2025.Components.Pages;

public partial class Mentor2DModeler : ComponentBase
{
    [Inject] private IMentorServices MentorServices { get; set; } = default!;
    [Inject] private IWorkspace Workspace { get; set; } = default!;
    [Inject] private IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] private ILogger<Mentor2DModeler> Logger { get; set; } = default!;
    [Inject] protected IModelTech ModelTech { get; set; } = default!;
    [Inject] protected IShape2DTech Shape2DTech { get; set; } = default!;

    private ElementReference canvasContainer;
    private Canvas2DComponent? Canvas2DReference;
    private IMentorStudio? Playground;
    private IDrawing? Drawing;
    private int nextX = 100;
    private int nextY = 100;
    private string _activeTreeTab = "model";
    private string chatInputValue = "";
    
    // Chat-related fields
    private List<ChatDisplayMessage> chatMessages = new();
    private List<AIChatMessage> conversationHistory = new();
    private bool isProcessingChat = false;
    private string currentAgent = "General AI Assistant";
    private Queue<string> messageQueue = new();
    private bool isProcessingQueue = false;
    private PageContext pageContext = new PageContext
    {
        PageName = "Mentor 2D Modeler",
        PageRoute = "/mentor2d-modeler",
        DomainFocus = "General AI Assistant with access to knowledge modeling tools. Can discuss engineering concepts, synthesize models, and create visual diagrams with concepts, properties, roles, and relationships. Has access to cloud-based engineering knowledge and can provide strategic guidance."
    };

    private int ShapeCount => Drawing?.FirstPage()?.AllShapes2D().OfType<MentorShape2D>().Count() ?? 0;
    private List<TreeItemData> ModelTreeItems = new();
    private List<LogEntry> ActivityLog = new();
    
    // Activity logging infrastructure
    private bool autoScrollLogs = true;
    private ElementReference logContainer;
    private ElementReference logScrollAnchor;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        LogInfo("Mentor 2D Visual Modeler initializing...");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Wait for canvas to initialize
            await Task.Delay(100);
            
            if (Canvas2DReference?.Page == null)
            {
                LogError("❌ Canvas2DReference.Page is null - shapes won't render!");
                return;
            }
            
            // Get the drawing and verify it's connected to the canvas page
            Drawing = Workspace.GetDrawing();
            var canvasPage = Canvas2DReference.Page;
            
            LogInfo($"✅ Canvas page available: {canvasPage.Name}");
            
            // CRITICAL: Connect technicians to canvas so AI can create shapes
            Shape2DTech.SetPage(canvasPage);
            LogSuccess($"✅ Connected Shape2DTech to page '{canvasPage.Name}'");
            LogInfo($"🔧 Initialized with {ChatOrchestrator.GetToolCount()} tools available for AI");
            
            // Create MentorStudio - it will register MentorConstructTool automatically
            Playground = new MentorStudio(Workspace, MentorServices.PubSub, MentorServices);
            
            // CRITICAL: Connect ModelTech to MentorStudio so AI can create MentorShape2D
            ModelTech.SetPageContext(canvasPage, Playground);
            LogSuccess($"✅ Connected ModelTech to MentorStudio for visual shape creation");
            
            // Verify the studio is using the correct page
            var studioPage = Drawing?.FirstPage();
            if (studioPage != null && studioPage == canvasPage)
            {
                LogSuccess($"✅ MentorStudio connected to canvas page '{canvasPage.Name}'");
            }
            else
            {
                LogWarning($"⚠️ Page mismatch - Studio: {studioPage?.Name}, Canvas: {canvasPage.Name}");
            }
            
            StateHasChanged();
        }
        
        await base.OnAfterRenderAsync(firstRender);
    }

    // Simple button handlers - CreateShape will capture and pass the page
    private void CreateConcept() => CreateShape(() => Playground!.CreateShape<KnConcept>("", Canvas2DReference!.Page), "Concept");
    private void CreateProperty() => CreateShape(() => Playground!.CreateShape<KnProperty>("", Canvas2DReference!.Page), "Property");
    private void CreateRole() => CreateShape(() => Playground!.CreateShape<KnRole>("", Canvas2DReference!.Page), "Role");
    private void CreateContext() => CreateShape(() => Playground!.CreateShape<KnContext>("", Canvas2DReference!.Page), "Context");
    private void CreateComponent() => CreateShape(() => Playground!.CreateShape<KnComponent>("", Canvas2DReference!.Page), "Component");
    private void CreateFormula() => CreateShape(() => Playground!.CreateShape<KnFormula>("", Canvas2DReference!.Page), "Formula");
    private void CreateFeature() => CreateShape(() => Playground!.CreateShape<KnFeature>("", Canvas2DReference!.Page), "Feature");
    private void CreateRelation() => CreateShape(() => Playground!.CreateShape<KnRelation>("", Canvas2DReference!.Page), "Relation");
    private void CreateVariable() => CreateShape(() => Playground!.CreateShape<KnVariable>("", Canvas2DReference!.Page), "Variable");
    private void CreateValidValues() => CreateShape(() => Playground!.CreateShape<KnValidValues>("", Canvas2DReference!.Page), "ValidValues");

    // Common implementation
    private void CreateShape(Func<MentorShape2D?> factory, string typeName)
    {
        var page = Canvas2DReference?.Page;
        Logger.LogInformation("🔍 CreateShape: Canvas2DReference={Canvas}, Page={Page}, PageKey={PageKey}, PageName={PageName}",
            Canvas2DReference != null ? "EXISTS" : "NULL",
            page != null ? "EXISTS" : "NULL",
            page?.GetName() ?? "NULL",
            page?.Name ?? "NULL");
        
        if (Playground == null || page == null)
        {
            LogError($"❌ Not ready to create {typeName} - Playground:{Playground != null}, Page:{page != null}");
            return;
        }

        try
        {
            Logger.LogInformation($"🏭 Creating {typeName} on page '{page.GetName()}'...");
            var shape = factory();
            
            if (shape != null)
            {
                // Shape already added to page by MentorStudio.CreateNodeShape(page)
                shape.MoveTo(nextX, nextY);
                
                nextX += 150;
                if (nextX > 800)
                {
                    nextX = 100;
                    nextY += 150;
                }
                if (nextY > 600)
                {
                    nextY = 100;
                }

                LogSuccess($"✅ Created {typeName}: {shape.Text} at ({shape.PinX},{shape.PinY})");
                RefreshTree();
                StateHasChanged();
            }
            else
            {
                LogError($"❌ Factory returned null for {typeName}");
            }
        }
        catch (Exception ex)
        {
            LogError($"❌ Error creating {typeName}: {ex.Message}");
            Logger.LogError(ex, "Exception in CreateShape");
        }
    }

    // ============================================================
    // MANUAL TEST METHODS - Verify basic canvas connectivity
    // ============================================================
    
    protected void AddTestCircle()
    {
        try
        {
            var page = Canvas2DReference?.Page;
            if (page == null)
            {
                LogError("❌ Canvas2D page not available for test circle");
                return;
            }

            var random = new Random();
            var testName = $"TestCircle_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var radius = random.Next(30, 60); // radius 30-60
            var diameter = radius * 2;
            var colors = new[] { "red", "blue", "green", "yellow", "purple", "orange", "cyan", "magenta" };
            var color = colors[random.Next(colors.Length)];
            
            var shape = new FoShape2D(diameter, diameter, color)
            {
                Name = testName
            };
            shape.ShapeDraw = shape.DrawCircle;
            
            // Random position within canvas bounds
            var x = random.Next(radius + 50, 800 - radius - 50);
            var y = random.Next(radius + 50, 600 - radius - 50);
            shape.MoveTo(x, y);
            
            page.AddShape(shape);
            LogSuccess($"⭕ Added {color} test circle radius={radius} at ({x},{y})");
            StateHasChanged();
        }
        catch (Exception ex)
        {
            LogError($"Failed to add test circle: {ex.Message}");
        }
    }

    private void AddTestConcept()
    {
        try
        {
            if (Playground == null)
            {
                LogError("Playground not initialized");
                return;
            }

            var random = new Random();
            var testName = $"Test_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var concepts = new[] { "Beam", "Column", "Foundation", "Load", "Stress", "Strain", "Moment", "Shear" };
            var conceptText = concepts[random.Next(concepts.Length)];
            
            var shape = Playground.CreateShape<KnConcept>();
            if (shape != null)
            {
                shape.Name = testName;
                shape.Text = conceptText;  // Set the display text
                
                // Random position
                var x = random.Next(50, 800);
                var y = random.Next(50, 500);
                shape.MoveTo(x, y);
                
                LogSuccess($"✅ Added test concept '{conceptText}' at ({x},{y})");
                RefreshTree();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            LogError($"Failed to add test concept: {ex.Message}");
        }
    }

    private void CreateBeamExample()
    {
        LogInfo("Creating Beam Example...");
        
        // Create Beam Concept (already added to page by Playground)
        var beam = Playground?.CreateShape<KnConcept>("Beam");
        if (beam != null)
        {
            beam.MoveTo(200, 200);
            LogSuccess("Created Beam Concept");

            // Create Properties and attach them
            var length = Playground?.CreateShape<KnProperty>("Length");
            if (length != null)
            {
                Playground?.Attach(length, beam);
                LogSuccess("Added Length Property");
            }

            var width = Playground?.CreateShape<KnProperty>("Width");
            if (width != null)
            {
                Playground?.Attach(width, beam);
                LogSuccess("Added Width Property");
            }

            var height = Playground?.CreateShape<KnProperty>("Height");
            if (height != null)
            {
                Playground?.Attach(height, beam);
                LogSuccess("Added Height Property");
            }

            LogSuccess("Beam example created!");
            RefreshTree();
            StateHasChanged();
        }
    }

    private void CreateStrategicPlanExample()
    {
        LogInfo("Creating Strategic Plan Example...");
        
        if (Playground == null)
        {
            LogError("Playground not initialized");
            return;
        }

        // Main Context
        var context = CreateShapeWithType(KnowledgeType.Context, "Strategic Plan Pro");
        context.MoveTo(400, 100);

        // Question 1: Profit vs Non-profit
        var a1 = CreateShapeWithType(KnowledgeType.Property, "Answer1");
        var vv1 = CreateShapeWithType(KnowledgeType.ValidValues, "profit; non-profit");
        Playground.Attach(vv1, a1);

        var q1 = CreateShapeWithType(KnowledgeType.Context, "Is this for a profit or non-profit?");
        Playground.Attach(q1, context);
        Playground.Attach(a1, q1);

        // Question 2: New vs On-going
        var a2 = CreateShapeWithType(KnowledgeType.Property, "Answer2");
        var vv2 = CreateShapeWithType(KnowledgeType.ValidValues, "new; on-going");
        Playground.Attach(vv2, a2);

        var q2 = CreateShapeWithType(KnowledgeType.Context, "Is this for a new organization or on-going concern?");
        Playground.Attach(q2, context);
        Playground.Attach(a2, q2);

        // Question 3: Detailed plan
        var a3 = CreateShapeWithType(KnowledgeType.Property, "Answer3");
        var vv3 = CreateShapeWithType(KnowledgeType.ValidValues, "yes; no");
        Playground.Attach(vv3, a3);

        var q3 = CreateShapeWithType(KnowledgeType.Context, "Do you want a detailed plan for a product or service?");
        Playground.Attach(q3, context);
        Playground.Attach(a3, q3);

        // Roles hierarchy
        var role = CreateShapeWithType(KnowledgeType.Role, "Strategic Plan Pro");
        role.MoveTo(400, 500);

        var r1 = CreateShapeWithType(KnowledgeType.Role, "General Plan");
        Playground.Attach(r1, role);

        // Plan variants
        var p1 = CreateShapeWithType(KnowledgeType.Role, "New Non-profit Plan");
        Playground.Attach(p1, r1);
        var e1 = CreateShapeWithType(KnowledgeType.Formula, "Answer1@ == 'non-profit' && Answer2@ == 'new'");
        Playground.Attach(e1, p1);
        Playground.Attach(CreateShapeWithType(KnowledgeType.Concept, "New Non-profit TEMPLATE"), p1);

        var p2 = CreateShapeWithType(KnowledgeType.Role, "On-going Non-profit Plan");
        Playground.Attach(p2, r1);
        var e2 = CreateShapeWithType(KnowledgeType.Formula, "Answer1@ == 'non-profit' && Answer2@ == 'on-going'");
        Playground.Attach(e2, p2);
        Playground.Attach(CreateShapeWithType(KnowledgeType.Concept, "On-going Non-profit TEMPLATE"), p2);

        LogSuccess("Strategic Plan created successfully!");
        RefreshTree();
        StateHasChanged();
    }

    private MentorShape2D CreateShapeWithType(KnowledgeType type, string title)
    {
        var page = Canvas2DReference?.Page;
        return type switch
        {
            KnowledgeType.Concept => Playground!.CreateShape<KnConcept>(title, page),
            KnowledgeType.Property => Playground!.CreateShape<KnProperty>(title, page),
            KnowledgeType.Role => Playground!.CreateShape<KnRole>(title, page),
            KnowledgeType.Context => Playground!.CreateShape<KnContext>(title, page),
            KnowledgeType.Component => Playground!.CreateShape<KnComponent>(title, page),
            KnowledgeType.Formula => Playground!.CreateShape<KnFormula>(title, page),
            KnowledgeType.ValidValues => Playground!.CreateShape<KnValidValues>(title, page),
            _ => Playground!.CreateShape<KnConcept>(title, page)
        };
    }

    private void ClearCanvas()
    {
        Drawing?.ClearAll();
        nextX = 100;
        nextY = 100;
        ModelTreeItems.Clear();
        LogWarning("Canvas cleared");
        StateHasChanged();
    }

    private void SaveModel()
    {
        MentorServices?.MentorModel?.SaveModel<FoundryMentorModeler.Persistence.KnowledgePersist>();
        LogSuccess("Model saved");
    }

    private void LoadModel()
    {
        MentorServices?.MentorModel?.RestoreModel<FoundryMentorModeler.Persistence.KnowledgePersist>();
        RefreshTree();
        LogSuccess("Model loaded");
        StateHasChanged();
    }

    private void RefreshTree()
    {
        ModelTreeItems.Clear();
        
        // Get all shapes from current page
        var page = Drawing?.FirstPage();
        if (page == null) return;
        
        var shapes = page.AllShapes2D().OfType<MentorShape2D>() ?? Enumerable.Empty<MentorShape2D>();
        
        foreach (var shape in shapes.Where(s => s.ParentShape == null))
        {
            ModelTreeItems.Add(new TreeItemData
            {
                Name = shape.Text,
                Type = shape.GetKnowledgeType(),
                ShapeId = shape.GetGlyphId(),
                Children = GetChildrenRecursive(shape)
            });
        }

        StateHasChanged();
    }

    private List<TreeItemData> GetChildrenRecursive(MentorShape2D parent)
    {
        var children = new List<TreeItemData>();
        
        foreach (var child in parent.GetSubshapes<MentorShape2D>() ?? Enumerable.Empty<MentorShape2D>())
        {
            children.Add(new TreeItemData
            {
                Name = child.Text,
                Type = child.GetKnowledgeType(),
                ShapeId = child.GetGlyphId(),
                Children = GetChildrenRecursive(child)
            });
        }
        
        return children;
    }

    private void SelectShape(TreeItemData item)
    {
        LogInfo($"Selected: {item.Name}");
    }

    private string GetKnowledgeTypeIcon(KnowledgeType type)
    {
        return type switch
        {
            KnowledgeType.Concept => "📘",
            KnowledgeType.Property => "📋",
            KnowledgeType.Role => "👤",
            KnowledgeType.Context => "📦",
            KnowledgeType.Component => "🔧",
            KnowledgeType.Formula => "ƒ",
            KnowledgeType.Feature => "⚙️",
            KnowledgeType.Relation => "🔗",
            KnowledgeType.Variable => "🔢",
            KnowledgeType.ValidValues => "✓",
            _ => "❓"
        };
    }

    private void ClearLog()
    {
        ActivityLog.Clear();
        StateHasChanged();
    }

    private void LogInfo(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "info", Message = message, Timestamp = DateTime.Now });
        ScrollToBottomIfNeeded();
    }

    private void LogSuccess(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "success", Message = message, Timestamp = DateTime.Now });
        ScrollToBottomIfNeeded();
    }

    private void LogWarning(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "warning", Message = message, Timestamp = DateTime.Now });
        ScrollToBottomIfNeeded();
    }

    private void LogError(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "error", Message = message, Timestamp = DateTime.Now });
        ScrollToBottomIfNeeded();
    }
    
    private void AddActivityLog(string level, string message)
    {
        ActivityLog.Add(new LogEntry { Level = level, Message = message, Timestamp = DateTime.Now });
        ScrollToBottomIfNeeded();
        StateHasChanged();
    }
    
    private void ClearActivityLogs()
    {
        ActivityLog.Clear();
        StateHasChanged();
    }
    
    private void ToggleAutoScroll()
    {
        autoScrollLogs = !autoScrollLogs;
        StateHasChanged();
    }
    
    private async Task ScrollToBottomIfNeeded()
    {
        if (autoScrollLogs && _activeTreeTab == "activity")
        {
            await Task.Delay(50); // Let DOM update
            await InvokeAsync(async () =>
            {
                try
                {
                    await logScrollAnchor.FocusAsync();
                }
                catch
                {
                    // Ignore focus errors
                }
            });
        }
    }
    
    private string GetLogBackgroundColor(string level) => level.ToLower() switch
    {
        "error" => "#3c1e1e",
        "warning" => "#3c3c1e", 
        "success" => "#1e3c1e",
        "info" => "#1e2a3c",
        _ => "#2e2e2e"
    };
    
    private string GetLogBorderColor(string level) => level.ToLower() switch
    {
        "error" => "#dc3545",
        "warning" => "#ffc107",
        "success" => "#28a745", 
        "info" => "#17a2b8",
        _ => "#6c757d"
    };
    
    private string GetLogColor(string level) => level.ToLower() switch
    {
        "error" => "#ff6b6b",
        "warning" => "#ffd93d",
        "success" => "#51cf66",
        "info" => "#74c0fc", 
        _ => "#d4d4d4"
    };
    
    private string GetLogIcon(string level) => level.ToLower() switch
    {
        "error" => "❌",
        "warning" => "⚠️",
        "success" => "✅",
        "info" => "ℹ️",
        _ => "📝"
    };
    
    private async Task QuickTest(string testPrompt)
    {
        Logger.LogInformation($"🧪 Quick Test: {testPrompt}");
        chatInputValue = testPrompt;
        await SendChatMessage();
    }
    
    protected async Task HandleTestSequenceSelected(TestSequenceMetadata sequence)
    {
        if (isProcessingQueue)
        {
            LogWarning("Cannot start test - already processing another sequence");
            return;
        }

        LogInfo($"🧪 Starting test: {sequence.DisplayName} ({sequence.PromptCount} prompts)");

        // Load all prompts into the queue
        messageQueue.Clear();
        foreach (var prompt in sequence.Prompts)
        {
            messageQueue.Enqueue(prompt);
        }

        // Start processing
        await ProcessMessageQueue();
        
        LogSuccess($"✅ Test sequence completed: {sequence.DisplayName}");
    }
    
    private async Task ProcessMessageQueue()
    {
        if (isProcessingQueue || messageQueue.Count == 0) return;
        
        isProcessingQueue = true;
        
        try
        {
            while (messageQueue.Count > 0)
            {
                var message = messageQueue.Dequeue();
                LogInfo($"🚀 Auto-executing: {message}");
                
                chatInputValue = message;
                await SendChatMessage();
                
                // Wait between messages to avoid overwhelming
                if (messageQueue.Count > 0)
                {
                    await Task.Delay(2000);
                }
            }
        }
        finally
        {
            isProcessingQueue = false;
        }
    }
    
    private async Task SendChatMessage()
    {
        if (isProcessingChat || string.IsNullOrWhiteSpace(chatInputValue)) return;
        
        var userMessage = chatInputValue;
        chatInputValue = ""; // Clear input
        
        // Add user message to display
        chatMessages.Add(new ChatDisplayMessage 
        { 
            IsUser = true,
            Text = userMessage,
            Timestamp = DateTime.Now
        });
        await InvokeAsync(StateHasChanged);
        
        isProcessingChat = true;
        try
        {
            Logger.LogInformation($"🔵 Processing message: '{userMessage}'");
            
            // Add to conversation history
            conversationHistory.Add(new AIChatMessage(ChatRole.User, userMessage));
            
            // Call the LLM through ChatOrchestrator
            var response = await ChatOrchestrator.ProcessMessageAsync(
                userMessage,
                pageContext,
                conversationHistory,
                onAgentSwitch: async (agentName) => 
                {
                    currentAgent = agentName;
                    Logger.LogInformation($"🔀 Agent Switch: {agentName}");
                    await InvokeAsync(StateHasChanged);
                });
            
            var fullResponse = response.Content;
            currentAgent = response.AgentName;
            
            Logger.LogInformation($"✅ Response received: Length={fullResponse.Length}");
            
            // Add AI response to chat history
            conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, fullResponse));
            
            // Add to display messages
            chatMessages.Add(new ChatDisplayMessage 
            { 
                IsUser = false,
                Text = fullResponse,
                AgentName = currentAgent,
                Timestamp = DateTime.Now
            });
            
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing chat message");
            
            chatMessages.Add(new ChatDisplayMessage 
            { 
                IsUser = false,
                Text = $"❌ Error: {ex.Message}",
                AgentName = "System",
                Timestamp = DateTime.Now
            });
            
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            isProcessingChat = false;
        }
    }

    private class TreeItemData
    {
        public string Name { get; set; } = "";
        public KnowledgeType Type { get; set; }
        public string ShapeId { get; set; } = "";
        public List<TreeItemData> Children { get; set; } = new();
    }

    private class LogEntry
    {
        public string Level { get; set; } = "info";
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
