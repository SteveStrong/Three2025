using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using System.ComponentModel;

namespace Three2025.Services.Chat;

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
    
    public event Action<string>? OnLog;

    public MultiProviderChatService(IConfiguration configuration)
    {
        // Initialize providers based on available configuration
        InitializeProviders(configuration);
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
            _providers["AWS Bedrock"] = new BedrockProvider(
                awsAccessKey, 
                awsSecretKey, 
                awsRegion, 
                "anthropic.claude-3-5-sonnet-20241022-v2:0");
            LogMessage("✓ AWS Bedrock provider available");
        }
        else
        {
            LogMessage("✗ AWS Bedrock provider unavailable (no credentials)");
        }

        // Set default provider
        if (_providers.Any())
        {
            _currentProvider = _providers.First().Value;
            InitializeAgent();
        }
    }

    private void InitializeAgent()
    {
        if (_currentProvider == null) return;

        var chatClient = _currentProvider.GetChatClient();
        
        // Wire up logging if it's a GitHubChatClient
        if (chatClient is GitHubChatClient githubClient)
        {
            githubClient.OnLog += LogMessage;
        }

        // Create tools for the agent
        var dateTimeTool = AIFunctionFactory.Create(GetCurrentDateTime);
        
        // Create the agent
        _currentAgent = chatClient.CreateAIAgent(
            instructions: "You are a helpful 3D modeling and visualization assistant working with Three.js and Blazor.",
            name: "3D Assistant",
            tools: [dateTimeTool]);
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
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_currentAgent == null)
        {
            yield return "Error: No AI provider configured. Please set up GitHub PAT token or AWS credentials.";
            yield break;
        }

        // Add user message to history
        conversationHistory.Add(new ChatMessage(ChatRole.User, userMessage));

        // Stream the response without try-catch to avoid yield restrictions
        var updateEnumerator = _currentAgent.RunStreamingAsync(conversationHistory).GetAsyncEnumerator(cancellationToken);
        
        while (true)
        {
            Microsoft.Agents.AI.AgentRunResponseUpdate? update = null;
            Exception? error = null;
            
            try
            {
                if (!await updateEnumerator.MoveNextAsync())
                    break;
                update = updateEnumerator.Current;
            }
            catch (Exception ex)
            {
                error = ex;
                LogMessage($"Error: {ex.Message}");
            }
            
            if (error != null)
            {
                yield return $"\n\n[Error: {error.Message}]";
                break;
            }
            
            if (update != null)
            {
                var text = update.ToString();
                if (!string.IsNullOrEmpty(text))
                {
                    yield return text;
                }
            }
        }
        
        await updateEnumerator.DisposeAsync();
    }

    private void LogMessage(string message)
    {
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
