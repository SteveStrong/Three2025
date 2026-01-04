using Azure;
using Azure.AI.Inference;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Three2025.Services.Chat;

#nullable enable

/// <summary>
/// Custom IChatClient implementation for GitHub Models that provides logging via standard ILogger
/// </summary>
public class GitHubChatClient : IChatClient
{
    private readonly IChatClient _innerClient;
    private readonly ILogger<GitHubChatClient> _logger;
    private readonly string _modelId;

    public GitHubChatClient(string token, string modelId, ILogger<GitHubChatClient> logger)
    {
        _logger = logger;
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
        _logger.LogInformation("📡 Calling GitHub API...");
        int chunkCount = 0;
        await foreach (var update in _innerClient.GetStreamingResponseAsync(messageList, options, cancellationToken))
        {
            chunkCount++;
            if (chunkCount == 1)
            {
                _logger.LogDebug("✅ Received first chunk from GitHub API");
            }
            yield return update;
        }
        _logger.LogDebug("✅ GitHub API streaming complete. Total chunks: {ChunkCount}", chunkCount);
    }

    private void LogConversation(string header, IList<ChatMessage> messages)
    {
        _logger.LogDebug("{Header} Total Messages: {MessageCount}", header, messages.Count);
        
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            
            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case TextContent text:
                        _logger.LogDebug("[Message {Index}] {Role}: {Text}", i + 1, message.Role, TruncateForLog(text.Text));
                        break;
                        
                    case FunctionCallContent toolCall:
                        _logger.LogDebug("[Message {Index}] {Role} → Tool: {ToolName}({Arguments})", 
                            i + 1, message.Role, toolCall.Name, TruncateForLog(SerializeArguments(toolCall.Arguments), 50));
                        break;
                        
                    case FunctionResultContent toolResult:
                        _logger.LogDebug("[Message {Index}] {Role} ← Result: {Result}", 
                            i + 1, message.Role, TruncateForLog(toolResult.Result?.ToString(), 80));
                        break;
                        
                    default:
                        _logger.LogDebug("[Message {Index}] {Role}: {ContentType}", i + 1, message.Role, content.GetType().Name);
                        break;
                }
            }
        }
    }

    private void LogCompletion(ChatResponse completion)
    {
        if (completion.Usage != null)
        {
            _logger.LogInformation("✅ Response: {FinishReason} Tokens: {InputTokens}in/{OutputTokens}out", 
                completion.FinishReason, 
                completion.Usage.InputTokenCount, 
                completion.Usage.OutputTokenCount);
        }
        else
        {
            _logger.LogInformation("✅ Response: {FinishReason}", completion.FinishReason);
        }
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
