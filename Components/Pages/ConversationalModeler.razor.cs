#nullable enable
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
using Three2025.Apprentice;
using Three2025.Models.Apprentice;

namespace Three2025.Components.Pages;

public partial class ConversationalModeler : ComponentBase
{
    [Inject] private IMentorServices MentorServices { get; set; } = default!;
    [Inject] private IWorkspace Workspace { get; set; } = default!;
    [Inject] private IChatOrchestrator ChatOrchestrator { get; set; } = default!;
    [Inject] private ILogger<ConversationalModeler> Logger { get; set; } = default!;
    [Inject] protected IModelTech ModelTech { get; set; } = default!;

    private string _activeTreeTab = "model";
    private string chatInputValue = "";
    
    // Chat-related fields
    private List<ChatDisplayMessage> chatMessages = new();
    private List<AIChatMessage> conversationHistory = new();
    private bool isProcessingChat = false;
    private string currentAgent = "Conversational Modeler AI";
    private Queue<string> messageQueue = new();
    private bool isProcessingQueue = false;
    private PageContext pageContext = new PageContext
    {
        PageName = "Conversational Modeler",
        PageRoute = "/conversational-modeler",
        DomainFocus = "Conversational AI Assistant specialized in creating knowledge models through natural language. Expert in engineering analysis, calculations, and model construction. Can build structural beam models, thermal analysis, electrical circuits, and other engineering systems through conversation."
    };

    // Model-related properties  
    private KnModel? CurrentModel => ModelTech?.CurrentModel; // Get current model from ModelTech
    private KnComponent? SelectedComponent => ModelTech?.CurrentComponent; // Get current component from ModelTech
    private List<TreeItemData> ModelTreeItems = new();
    private List<LogEntry> ActivityLog = new();
    
    // Activity logging infrastructure
    private bool autoScrollLogs = true;
    private ElementReference logContainer;
    private ElementReference logScrollAnchor;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        _ = LogInfo("Conversational Modeler initializing...");
        
        // Welcome message
        chatMessages.Add(new ChatDisplayMessage
        {
            IsUser = false,
            Text = "Hello! I'm the Conversational Modeler. Describe an engineering problem and I'll build a knowledge model to solve it through natural conversation.",
            AgentName = currentAgent,
            Timestamp = DateTime.Now
        });
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _ = LogInfo($"🔧 Initialized with {ChatOrchestrator.GetToolCount()} tools available for AI");
            _ = LogSuccess($"✅ Conversational Modeler ready for knowledge model construction");
            
            await InvokeAsync(StateHasChanged);
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
            _ = LogInfo($"🔵 User: {userMessage}");
            
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
            _ = LogSuccess($"✅ {currentAgent}: {fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
            
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
            _ = LogError($"❌ Error: {ex.Message}");
            
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

    private async Task QuickTest(string testPrompt)
    {
        Logger.LogInformation($"🧪 Quick Test: {testPrompt}");
        _ = LogInfo($"🧪 Quick Test: {testPrompt}");
        chatInputValue = testPrompt;
        await SendChatMessage();
    }
    
    protected async Task HandleTestSequenceSelected(TestSequenceMetadata sequence)
    {
        if (isProcessingQueue)
        {
            _ = LogWarning("Cannot start test - already processing another sequence");
            return;
        }

        _ = LogInfo($"🧪 Starting test: {sequence.DisplayName} ({sequence.PromptCount} prompts)");

        // Load all prompts into the queue
        messageQueue.Clear();
        foreach (var prompt in sequence.Prompts)
        {
            messageQueue.Enqueue(prompt);
        }

        // Start processing
        await ProcessMessageQueue();
        
        _ = LogSuccess($"✅ Test sequence completed: {sequence.DisplayName}");
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
                _ = LogInfo($"🚀 Auto-executing: {message}");
                
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

    private void ClearModel()
    {
        _ = LogInfo("🗑️ Clearing conversation and model...");
        conversationHistory.Clear();
        chatMessages.Clear();
        
        // Add welcome message back
        chatMessages.Add(new ChatDisplayMessage
        {
            IsUser = false,
            Text = "Model cleared! Ready to start fresh. Describe an engineering problem and I'll build a knowledge model to solve it.",
            AgentName = currentAgent,
            Timestamp = DateTime.Now
        });
        
        StateHasChanged();
        _ = LogSuccess("✅ Model and conversation cleared");
    }

    private void HandleTreeSelection(object selectedItem)
    {
        _ = LogInfo($"🔍 Selected tree item: {selectedItem}");
        // TODO: Implement component selection handling
    }

    private IEnumerable<ParameterInfo> GetParameters(object? component)
    {
        if (component is not KnComponent knComponent)
            return new List<ParameterInfo>();
            
        return knComponent.Members<KnParameter>().Select(p => new ParameterInfo
        {
            Name = p.Name ?? "",
            Value = p.GetValue()?.ToString() ?? "",
            Unit = null, // TODO: Extract unit from parameter if available
            IsFormula = !string.IsNullOrEmpty(p.Expression)
        });
    }

    // Activity logging methods
    private async Task LogInfo(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "info", Message = message, Timestamp = DateTime.Now });
        await ScrollToBottomIfNeeded();
    }

    private async Task LogSuccess(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "success", Message = message, Timestamp = DateTime.Now });
        await ScrollToBottomIfNeeded();
    }

    private async Task LogWarning(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "warning", Message = message, Timestamp = DateTime.Now });
        await ScrollToBottomIfNeeded();
    }

    private async Task LogError(string message)
    {
        ActivityLog.Add(new LogEntry { Level = "error", Message = message, Timestamp = DateTime.Now });
        await ScrollToBottomIfNeeded();
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
