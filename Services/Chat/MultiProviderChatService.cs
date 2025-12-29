using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using System.ComponentModel;
using OllamaSharp;

namespace Three2025.Services.Chat;

#nullable enable

/// <summary>
/// Service for managing multi-provider AI chat in Blazor
/// </summary>
public interface IMultiProviderChatService
{
    /// <summary>
    /// Available chat providers
    /// </summary>
    IReadOnlyList<string> AvailableProviders { get; }
    
    /// <summary>
    /// Currently selected provider name
    /// </summary>
    string CurrentProvider { get; }
    
    /// <summary>
    /// Switch to a different provider
    /// </summary>
    bool SetProvider(string providerName);
    
    /// <summary>
    /// Send a message and get streaming response
    /// </summary>
    IAsyncEnumerable<string> SendMessageStreamingAsync(
        string userMessage, 
        List<ChatMessage> conversationHistory,
        IEnumerable<AIFunction>? tools = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a message and get complete response with automatic tool execution
    /// </summary>
    Task<string> SendMessageAsync(
        string userMessage, 
        List<ChatMessage> conversationHistory,
        IEnumerable<AIFunction>? tools = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event for logging/debugging information
    /// </summary>
    event Action<string>? OnLog;
}

public class MultiProviderChatService : IMultiProviderChatService
{
    private readonly Dictionary<string, IChatProvider> _providers = new();
    private IChatProvider? _currentProvider;
    private ChatClientAgent? _currentAgent;
    private List<AIFunction> _currentTools = new();
    private readonly ILogger<MultiProviderChatService> _logger;
    
    public event Action<string>? OnLog;

    public MultiProviderChatService(IConfiguration configuration, ILogger<MultiProviderChatService> logger)
    {
        _logger = logger;
        _logger.LogInformation("🚀 MultiProviderChatService constructor starting...");
        // Initialize providers based on available configuration
        InitializeProviders(configuration);
        _logger.LogInformation("✅ MultiProviderChatService initialized");
    }

    private void InitializeProviders(IConfiguration configuration)
    {
        // Try to add GitHub Models provider
        var githubToken = configuration["GitHubPatToken"] ?? 
                         Environment.GetEnvironmentVariable("GitHubPatToken", EnvironmentVariableTarget.User);
        
        if (!string.IsNullOrEmpty(githubToken))
        {
            _providers["GitHub Models"] = new GitHubModelProvider(githubToken, "gpt-4o-mini");
            LogMessage("✓ GitHub Models provider available");
        }
        else
        {
            LogMessage("✗ GitHub Models provider unavailable (no token)");
        }

        // Try to add AWS Bedrock provider
        var awsAccessKey = configuration["AWS_ACCESS_KEY_ID"] ?? 
                          Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID", EnvironmentVariableTarget.User);
        var awsSecretKey = configuration["AWS_SECRET_ACCESS_KEY"] ?? 
                          Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", EnvironmentVariableTarget.User);
        var awsRegion = configuration["AWS_REGION"] ?? 
                       Environment.GetEnvironmentVariable("AWS_REGION", EnvironmentVariableTarget.User) ?? 
                       "us-east-1";

        if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
        {
            // GovCloud uses different model IDs - Claude 3.5 Sonnet v2
            var defaultModel = awsRegion.Contains("gov") 
                ? "us.anthropic.claude-3-5-sonnet-20241022-v2:0"  // Full GovCloud format
                : "anthropic.claude-3-5-sonnet-20241022-v2:0";
            
            var awsModel = configuration["AWS_BEDROCK_MODEL"] ??
                          Environment.GetEnvironmentVariable("AWS_BEDROCK_MODEL") ??
                          defaultModel;
            
            try
            {
                _providers["AWS Bedrock"] = new BedrockProvider(
                    awsAccessKey, 
                    awsSecretKey, 
                    awsRegion, 
                    awsModel);
                LogMessage($"✓ AWS Bedrock provider available (region: {awsRegion}, model: {awsModel})");
            }
            catch (Exception ex)
            {
                LogMessage($"✗ AWS Bedrock provider failed: {ex.Message}");
            }
        }
        else
        {
            LogMessage("✗ AWS Bedrock provider unavailable (no credentials)");
        }

        // Try to add Ollama provider (local)
        var ollamaEndpoint = configuration["OLLAMA_ENDPOINT"] ?? 
                            Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT") ?? 
                            "http://localhost:11434";
        var ollamaModel = configuration["OLLAMA_MODEL"] ?? 
                         Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? 
                         "llama3.2";

        try
        {
            _providers["Ollama (Local)"] = new OllamaProvider(ollamaEndpoint, ollamaModel);
            LogMessage($"✓ Ollama provider available ({ollamaEndpoint}, model: {ollamaModel})");
        }
        catch (Exception ex)
        {
            LogMessage($"✗ Ollama provider unavailable: {ex.Message}");
        }

        // Set default provider - PREFER BEDROCK > GITHUB > OLLAMA
        if (_providers.Any())
        {
            // Try Bedrock first (unlimited, paid), then GitHub, then Ollama
            if (_providers.ContainsKey("AWS Bedrock"))
            {
                _currentProvider = _providers["AWS Bedrock"];
                LogMessage("🎯 Default provider: AWS Bedrock");
            }
            else if (_providers.ContainsKey("GitHub Models"))
            {
                _currentProvider = _providers["GitHub Models"];
                LogMessage("🎯 Default provider: GitHub Models (gpt-4o-mini) - WARNING: May be rate limited");
            }
            else if (_providers.ContainsKey("Ollama (Local)"))
            {
                _currentProvider = _providers["Ollama (Local)"];
                LogMessage("🎯 Default provider: Ollama (Local)");
            }
            else
            {
                _currentProvider = _providers.First().Value;
                LogMessage($"🎯 Default provider: {_currentProvider.ProviderName}");
            }
            
            InitializeAgent();
        }
    }

    private void InitializeAgent(IEnumerable<AIFunction>? tools = null)
    {
        if (_currentProvider == null) return;

        var chatClient = _currentProvider.GetChatClient();
        
        // Wire up logging if it's a GitHubChatClient
        if (chatClient is GitHubChatClient githubClient)
        {
            githubClient.OnLog += LogMessage;
        }

        // Combine built-in tools with provided tools
        var allTools = new List<AIFunction>();
        var dateTimeTool = AIFunctionFactory.Create(GetCurrentDateTime);
        allTools.Add(dateTimeTool);
        
        if (tools != null)
        {
            allTools.AddRange(tools);
            _currentTools = tools.ToList();
            LogMessage($"Agent initialized with {allTools.Count} tools ({_currentTools.Count} from technicians)");
        }
        
        // For Ollama, use ChatClientAgent directly (as per Microsoft sample)
        // Note: ChatClientAgent doesn't support tools via constructor, but will use them via options
        if (chatClient is OllamaApiClient)
        {
            LogMessage("🦙 Creating ChatClientAgent directly for Ollama");
            _currentAgent = new ChatClientAgent(
                chatClient,
                name: "3D Assistant",
                instructions: "You are a helpful 3D modeling and visualization assistant working with Three.js and Blazor.");
            LogMessage("✅ Ollama ChatClientAgent created successfully");
        }
        else
        {
            LogMessage($"🔧 Creating agent via CreateAIAgent for {_currentProvider.ProviderName}");
            // Create the agent with all tools using extension method
            _currentAgent = chatClient.CreateAIAgent(
                instructions: "You are a helpful 3D modeling and visualization assistant working with Three.js and Blazor. Use the available tools to help users create and manipulate 3D geometry.",
                name: "3D Assistant",
                tools: allTools.ToArray());
        }
    }

    public IReadOnlyList<string> AvailableProviders => _providers.Keys.ToList();

    public string CurrentProvider => _currentProvider?.ProviderName ?? "None";

    public bool SetProvider(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var provider))
        {
            _currentProvider = provider;
            InitializeAgent();
            LogMessage($"Switched to provider: {providerName} ({provider.ModelName})");
            return true;
        }
        return false;
    }

    public async IAsyncEnumerable<string> SendMessageStreamingAsync(
        string userMessage, 
        List<ChatMessage> conversationHistory,
        IEnumerable<AIFunction>? tools = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_currentProvider == null)
        {
            LogMessage("❌ ERROR: No AI provider configured");
            yield return "Error: No AI provider configured.";
            yield break;
        }

        // Use IChatClient directly with proper error handling
        var chatClient = _currentProvider.GetChatClient();
        
        // Create chat options with tools if provided
        ChatOptions? chatOptions = null;
        var toolsList = tools?.ToList() ?? new List<AIFunction>();
        if (toolsList.Any())
        {
            chatOptions = new ChatOptions
            {
                Tools = toolsList.Select(t => (AITool)t).ToList()
            };
            LogMessage($"🔧 Passing {toolsList.Count} tools to LLM");
        }
        
        // Create timeout cancellation token (30 seconds)
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        
        int chunkCount = 0;
        string? errorMessage = null;
        
        // Collect chunks (can't yield inside try-catch)
        var chunks = new List<string>();
        
        // Track max turns to prevent infinite loops
        int maxTurns = 5; // Prevent infinite loops
        int currentTurn = 0;
        
        try
        {
            // Multi-turn loop to handle tool execution
            while (currentTurn < maxTurns)
            {
                currentTurn++;
                var toolCallsInThisTurn = new List<FunctionCallContent>();
                var textInThisTurn = new System.Text.StringBuilder();
                
                LogMessage($"🔄 Turn {currentTurn}: Calling LLM...");
                
                await foreach (var update in chatClient.GetStreamingResponseAsync(conversationHistory, options: chatOptions, linkedCts.Token))
                {
                    chunkCount++;
                    
                    // Collect tool calls
                    if (update.Contents != null)
                    {
                        foreach (var content in update.Contents)
                        {
                            if (content is FunctionCallContent toolCall)
                            {
                                toolCallsInThisTurn.Add(toolCall);
                                LogMessage($"🔧 Tool call detected: {toolCall.Name}");
                            }
                        }
                    }
                    
                    // Collect text
                    if (!string.IsNullOrEmpty(update.Text))
                    {
                        textInThisTurn.Append(update.Text);
                        chunks.Add(update.Text);
                    }
                }
                
                // Add assistant's response to conversation (includes tool calls)
                var assistantMessage = textInThisTurn.ToString();
                if (!string.IsNullOrEmpty(assistantMessage) || toolCallsInThisTurn.Any())
                {
                    // Build the assistant message with both text and tool calls
                    var assistantContents = new List<AIContent>();
                    if (!string.IsNullOrEmpty(assistantMessage))
                    {
                        assistantContents.Add(new TextContent(assistantMessage));
                    }
                    assistantContents.AddRange(toolCallsInThisTurn);
                    
                    conversationHistory.Add(new ChatMessage(ChatRole.Assistant, assistantContents));
                    LogMessage($"📝 Assistant response: {assistantMessage.Length} chars, {toolCallsInThisTurn.Count} tool calls");
                }
                
                // Execute tools if any were called
                if (toolCallsInThisTurn.Any())
                {
                    LogMessage($"⚙️ Executing {toolCallsInThisTurn.Count} tool calls...");
                    
                    foreach (var toolCall in toolCallsInThisTurn)
                    {
                        var tool = toolsList.FirstOrDefault(t => t.Name == toolCall.Name);
                        if (tool != null)
                        {
                            try
                            {
                                LogMessage($"▶️ Executing tool: {toolCall.Name}");
                                
                                // Execute the tool
                                var args = toolCall.Arguments != null 
                                    ? new AIFunctionArguments(toolCall.Arguments) 
                                    : new AIFunctionArguments();
                                
                                // Special logging for ChangeColor
                                if (toolCall.Name == "ChangeColor")
                                {
                                    var argDict = toolCall.Arguments as IDictionary<string, object?>;
                                    var shapeName = argDict?.ContainsKey("name") == true ? argDict["name"]?.ToString() : "unknown";
                                    var color = argDict?.ContainsKey("color") == true ? argDict["color"]?.ToString() : "unknown";
                                    LogMessage($"🎨 ChangeColor called with: name='{shapeName}', color='{color}'");
                                }
                                
                                var result = await tool.InvokeAsync(args, cancellationToken);
                                var resultStr = result?.ToString() ?? "null";
                                
                                LogMessage($"✅ Tool '{toolCall.Name}' executed successfully: {resultStr.Substring(0, Math.Min(100, resultStr.Length))}");
                                
                                // Add tool result to conversation
                                var resultContent = new FunctionResultContent(toolCall.CallId, result);
                                conversationHistory.Add(new ChatMessage(ChatRole.Tool, [resultContent]));
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"❌ Tool '{toolCall.Name}' execution failed: {ex.Message}");
                                var errorContent = new FunctionResultContent(toolCall.CallId, $"Error: {ex.Message}");
                                conversationHistory.Add(new ChatMessage(ChatRole.Tool, [errorContent]));
                            }
                        }
                        else
                        {
                            LogMessage($"⚠️ Tool '{toolCall.Name}' not found in available tools");
                            var errorContent = new FunctionResultContent(toolCall.CallId, $"Error: Tool '{toolCall.Name}' not found");
                            conversationHistory.Add(new ChatMessage(ChatRole.Tool, [errorContent]));
                        }
                    }
                    
                    // Continue conversation to let LLM process tool results
                    LogMessage($"🔄 Continuing conversation with tool results...");
                }
                else
                {
                    // No tool calls - conversation is complete
                    break;
                }
            }
            
            if (currentTurn >= maxTurns)
            {
                LogMessage($"⚠️ Reached maximum turns ({maxTurns}), stopping conversation loop");
            }
            
            LogMessage($"✅ Streaming complete: {chunks.Count} text chunks, {chunkCount} total updates");
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            LogMessage("⏱️ ERROR: Request timed out after 30 seconds");
            errorMessage = "\n\n❌ **Error**: Request timed out. The LLM provider may be rate limiting or unavailable.";
        }
        catch (HttpRequestException ex)
        {
            LogMessage($"🌐 ERROR: Network error - {ex.Message}");
            errorMessage = $"\n\n❌ **Error**: Network issue - {ex.Message}";
        }
        catch (Exception ex)
        {
            LogMessage($"❌ ERROR: {ex.GetType().Name} - {ex.Message}");
            
            // Check for rate limit errors
            if (ex.Message.Contains("Too many requests", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "\n\n❌ **Rate Limit Exceeded**: You've hit the rate limit for this provider. Please wait a few minutes or switch to AWS Bedrock.";
            }
            else
            {
                errorMessage = $"\n\n❌ **Error**: {ex.Message}";
            }
        }
        
        // Now yield the results
        if (errorMessage != null)
        {
            yield return errorMessage;
        }
        else
        {
            foreach (var chunk in chunks)
            {
                yield return chunk;
            }
        }
    }

    public async Task<string> SendMessageAsync(
        string userMessage,
        List<ChatMessage> conversationHistory,
        IEnumerable<AIFunction>? tools = null,
        CancellationToken cancellationToken = default)
    {
        if (_currentProvider == null)
        {
            LogMessage("❌ No provider available");
            return "Error: No AI provider configured";
        }

        var chatClient = _currentProvider.GetChatClient();
        LogMessage($"💬 SendMessageAsync: {userMessage.Substring(0, Math.Min(50, userMessage.Length))}... with {tools?.Count() ?? 0} tools");

        ChatOptions? chatOptions = null;
        if (tools != null && tools.Any())
        {
            chatOptions = new ChatOptions
            {
                Tools = tools.Select(t => (AITool)t).ToList()
            };
        }

        var response = new System.Text.StringBuilder();
        
        try
        {
            // Get streaming response and collect chunks
            await foreach (var update in chatClient.GetStreamingResponseAsync(conversationHistory, options: chatOptions, cancellationToken))
            {
                // Collect text response
                if (!string.IsNullOrEmpty(update.Text))
                {
                    response.Append(update.Text);
                }
                
                // Handle tool calls - EXECUTE THEM!
                if (update.Contents != null)
                {
                    foreach (var content in update.Contents)
                    {
                        if (content is FunctionCallContent toolCall)
                        {
                            LogMessage($"🔧 Executing tool: {toolCall.Name}");
                            
                            // Find and execute the tool
                            var tool = tools?.FirstOrDefault(t => t.Name == toolCall.Name);
                            if (tool != null)
                            {
                                try
                                {
                                    // Execute the tool - convert dictionary to AIFunctionArguments
                                    var args = toolCall.Arguments != null 
                                        ? new AIFunctionArguments(toolCall.Arguments) 
                                        : new AIFunctionArguments();
                                    
                                    var result = await tool.InvokeAsync(args, cancellationToken);
                                    var resultStr = result?.ToString() ?? "null";
                                    LogMessage($"✅ Tool '{toolCall.Name}' result: {resultStr.Substring(0, Math.Min(100, resultStr.Length))}");
                                    
                                    // Add tool result back to conversation for LLM (pass as content list)
                                    var resultContent = new FunctionResultContent(toolCall.CallId, result);
                                    conversationHistory.Add(new ChatMessage(ChatRole.Tool, [resultContent]));
                                }
                                catch (Exception ex)
                                {
                                    LogMessage($"❌ Tool '{toolCall.Name}' failed: {ex.Message}");
                                    var errorContent = new FunctionResultContent(toolCall.CallId, $"Error: {ex.Message}");
                                    conversationHistory.Add(new ChatMessage(ChatRole.Tool, [errorContent]));
                                }
                            }
                        }
                    }
                }
            }
            
            LogMessage($"✅ Response complete: {response.Length} chars");
            return response.ToString();
        }
        catch (Exception ex)
        {
            LogMessage($"❌ Error: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
    
    private void LogMessage(string message)
    {
        _logger.LogInformation(message);
        OnLog?.Invoke(message);
    }

    // Tool: Get current date and time
    [Description("Gets the current date and time")]
    private static string GetCurrentDateTime()
    {
        var now = DateTime.Now;
        return $"Current date and time: {now:yyyy-MM-dd HH:mm:ss} (Day: {now.DayOfWeek})";
    }
}
