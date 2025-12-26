using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Three2025.Services.Chat;

/// <summary>
/// Custom IChatClient implementation for GitHub Models that provides logging and event tracking
/// </summary>
public class GitHubChatClient : IChatClient
{
    private readonly IChatClient _innerClient;
    private readonly string _modelId;

    // Event for logging conversation updates to UI
    public event Action<string>? OnLog;

    public GitHubChatClient(string token, string modelId)
    {
        _modelId = modelId;
        
        // Create the underlying client using Azure.AI.Inference
        _innerClient = new ChatCompletionsClient(
            new Uri("https://models.github.ai/inference"),
            new AzureKeyCredential(token),
            new AzureAIInferenceClientOptions())
            .AsIChatClient(modelId);
        
        // Set metadata
        Metadata = new ChatClientMetadata();
    }

    public ChatClientMetadata Metadata { get; }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Convert to list for logging
        var messageList = chatMessages.ToList();
        
        // Log all messages before sending
        LogConversation("=== GetResponseAsync Called ===", messageList);
        
        // Call the underlying client
        var completion = await _innerClient.GetResponseAsync(messageList, options, cancellationToken);
        
        // Log the response
        LogCompletion(completion);
        
        return completion;
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Convert to list for logging
        var messageList = chatMessages.ToList();
        
        // Log all messages before sending
        LogConversation("=== GetStreamingResponseAsync Called ===", messageList);
        
        // Stream the response
        await foreach (var update in _innerClient.GetStreamingResponseAsync(messageList, options, cancellationToken))
        {
            yield return update;
        }
    }

    private void LogConversation(string header, IList<ChatMessage> messages)
    {
        var logBuilder = new System.Text.StringBuilder();
        logBuilder.AppendLine();
        logBuilder.AppendLine(header);
        logBuilder.AppendLine($"Total Messages: {messages.Count}");
        
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            logBuilder.AppendLine($"\n[Message {i + 1}] Role: {message.Role}");
            
            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case TextContent text:
                        logBuilder.AppendLine($"  Type: Text");
                        logBuilder.AppendLine($"  Content: {TruncateForLog(text.Text)}");
                        break;
                        
                    case FunctionCallContent toolCall:
                        logBuilder.AppendLine($"  Type: Function Call");
                        logBuilder.AppendLine($"  Function: {toolCall.Name}");
                        logBuilder.AppendLine($"  CallId: {toolCall.CallId}");
                        logBuilder.AppendLine($"  Arguments: {SerializeArguments(toolCall.Arguments)}");
                        break;
                        
                    case FunctionResultContent toolResult:
                        logBuilder.AppendLine($"  Type: Function Result");
                        logBuilder.AppendLine($"  CallId: {toolResult.CallId}");
                        logBuilder.AppendLine($"  Result: {TruncateForLog(toolResult.Result?.ToString())}");
                        break;
                        
                    default:
                        logBuilder.AppendLine($"  Type: {content.GetType().Name}");
                        break;
                }
            }
        }
        
        logBuilder.AppendLine();
        logBuilder.AppendLine("================");
        
        OnLog?.Invoke(logBuilder.ToString());
    }

    private void LogCompletion(ChatResponse completion)
    {
        var logBuilder = new System.Text.StringBuilder();
        logBuilder.AppendLine("\n=== Response Received ===");
        logBuilder.AppendLine($"Finish Reason: {completion.FinishReason}");
        logBuilder.AppendLine($"Model: {completion.ModelId}");
        
        if (completion.Usage != null)
        {
            logBuilder.AppendLine($"Tokens - Input: {completion.Usage.InputTokenCount}, Output: {completion.Usage.OutputTokenCount}, Total: {completion.Usage.TotalTokenCount}");
        }
        
        logBuilder.AppendLine("================");
        
        OnLog?.Invoke(logBuilder.ToString());
    }

    private static string? TruncateForLog(string? text, int maxLength = 100)
    {
        if (text == null) return null;
        return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
    }

    private static string SerializeArguments(object? arguments)
    {
        if (arguments == null) return "null";
        
        try
        {
            return JsonSerializer.Serialize(arguments, new JsonSerializerOptions 
            { 
                WriteIndented = false 
            });
        }
        catch
        {
            return arguments.ToString() ?? "null";
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return _innerClient.GetService(serviceType, serviceKey);
    }

    public void Dispose()
    {
        (_innerClient as IDisposable)?.Dispose();
    }
}
