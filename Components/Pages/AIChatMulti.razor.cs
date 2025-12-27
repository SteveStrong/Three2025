using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.AI;
using Microsoft.JSInterop;
using Three2025.Services.Chat;
using AIMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Three2025.Components.Pages;

public class ChatDisplayMessage
{
    public bool IsUser { get; set; }
    public string Text { get; set; } = string.Empty;
}

public partial class AIChatMultiBase : ComponentBase, IDisposable
{
    [Inject] private IMultiProviderChatService ChatService { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    protected string newMessage = string.Empty;
    protected List<ChatDisplayMessage> messages = new();
    protected List<AIMessage> conversationHistory = new();
    protected bool isProcessing = false;
    
    protected string selectedProvider = string.Empty;
    protected List<string> AvailableProviders = new();
    protected string CurrentProvider = "None";
    
    protected string logContent = string.Empty;
    protected ElementReference messagesContainer;
    protected ElementReference logContainer;

    protected override void OnInitialized()
    {
        // Get available providers
        AvailableProviders = ChatService.AvailableProviders.ToList();
        
        if (AvailableProviders.Any())
        {
            selectedProvider = AvailableProviders.First();
            CurrentProvider = ChatService.CurrentProvider;
            
            AddLogEntry($"Initialized with provider: {CurrentProvider}");
        }
        else
        {
            AddLogEntry("⚠ No providers available. Configure API keys:");
            AddLogEntry("  - GitHub: Set 'GitHubPatToken' environment variable or user secret");
            AddLogEntry("  - AWS: Set 'AWS_ACCESS_KEY_ID' and 'AWS_SECRET_ACCESS_KEY' environment variables");
        }

        // Subscribe to log events
        ChatService.OnLog += HandleLogMessage;
    }

    protected void OnProviderChanged()
    {
        if (ChatService.SetProvider(selectedProvider))
        {
            CurrentProvider = ChatService.CurrentProvider;
            AddLogEntry($"✓ Switched to: {CurrentProvider}");
            StateHasChanged();
        }
    }

    protected async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(newMessage) || isProcessing)
            return;

        var userMessage = newMessage;
        newMessage = string.Empty;
        
        // Add user message to display
        messages.Add(new ChatDisplayMessage 
        { 
            IsUser = true, 
            Text = userMessage 
        });
        
        isProcessing = true;
        StateHasChanged();
        
        await ScrollToBottom(messagesContainer);

        try
        {
            // Collect the full response
            var fullResponse = new System.Text.StringBuilder();
            
            await foreach (var chunk in ChatService.SendMessageStreamingAsync(
                userMessage, 
                conversationHistory,
                tools: null,
                CancellationToken.None))
            {
                fullResponse.Append(chunk);
            }

            var responseText = fullResponse.ToString();
            
            // Add assistant response to display
            messages.Add(new ChatDisplayMessage 
            { 
                IsUser = false, 
                Text = responseText 
            });
            
            // Add to conversation history
            conversationHistory.Add(new AIMessage(
                Microsoft.Extensions.AI.ChatRole.Assistant, 
                responseText));
            
            AddLogEntry($"✓ Response complete ({responseText.Length} chars)");
        }
        catch (Exception ex)
        {
            AddLogEntry($"❌ Error: {ex.Message}");
            messages.Add(new ChatDisplayMessage 
            { 
                IsUser = false, 
                Text = $"[Error: {ex.Message}]" 
            });
        }
        finally
        {
            isProcessing = false;
            StateHasChanged();
            await ScrollToBottom(messagesContainer);
        }
    }

    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    protected void ClearConversation()
    {
        messages.Clear();
        conversationHistory.Clear();
        logContent = string.Empty;
        AddLogEntry("🗑 Conversation cleared");
        StateHasChanged();
    }

    private void HandleLogMessage(string message)
    {
        _ = InvokeAsync(async () =>
        {
            AddLogEntry(message);
            StateHasChanged();
            await ScrollToBottom(logContainer);
        });
    }

    private void AddLogEntry(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var escapedMessage = System.Net.WebUtility.HtmlEncode(message);
        logContent += $"<div>[{timestamp}] {escapedMessage}</div>";
        
        // Keep only last 500 lines
        var lines = logContent.Split("<div>");
        if (lines.Length > 500)
        {
            logContent = string.Join("<div>", lines.Skip(lines.Length - 500));
        }
    }

    private async Task ScrollToBottom(ElementReference element)
    {
        try
        {
            await JS.InvokeVoidAsync("eval", 
                $"document.querySelector('[_bl_{element.Id}]').scrollTop = " +
                $"document.querySelector('[_bl_{element.Id}]').scrollHeight");
        }
        catch
        {
            // Ignore JS errors
        }
    }

    public void Dispose()
    {
        if (ChatService != null)
        {
            ChatService.OnLog -= HandleLogMessage;
        }
    }
}
