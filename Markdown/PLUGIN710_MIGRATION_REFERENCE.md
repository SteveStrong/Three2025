# Plugin710 Migration Reference - Semantic Kernel to Microsoft.Extensions.AI

**Source Project**: Three2025 (edesignstudio)  
**Target Project**: Plugin710  
**Date**: January 8, 2026  
**Purpose**: Copy-paste ready code for migrating from Semantic Kernel to Microsoft.Extensions.AI

---

## 1. NuGet Package Versions (EXACT)

From `Three2025.csproj` (lines 1-52):

```xml
<ItemGroup>
  <!-- Core Microsoft.Extensions.AI Framework -->
  <PackageReference Include="Microsoft.Extensions.AI" Version="10.1.1" />
  
  <!-- Provider-Specific Packages (choose what you need) -->
  
  <!-- For Ollama (local LLM) -->
  <PackageReference Include="OllamaSharp" Version="5.4.12" />
  <PackageReference Include="Microsoft.Extensions.AI.Ollama" Version="9.0.1-preview.1.24570.5" />
  
  <!-- For Azure AI Inference (GitHub Models, Azure OpenAI) -->
  <PackageReference Include="Azure.AI.Inference" Version="1.0.0-beta.5" />
  <PackageReference Include="Microsoft.Extensions.AI.AzureAIInference" Version="10.0.0-preview.1.25559.3" />
  
  <!-- For AWS Bedrock (Claude, etc.) -->
  <PackageReference Include="AWSSDK.BedrockRuntime" Version="4.0.14.3" />
  <PackageReference Include="AWSSDK.Extensions.Bedrock.MEAI" Version="4.0.5.1" />
  
  <!-- For agent-based workflows (optional) -->
  <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.251219.1" />
  
  <!-- Logging (required) -->
  <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
  
  <!-- Configuration (for secrets) -->
  <PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="9.0.0" />
</ItemGroup>
```

**IMPORTANT**: Remove Semantic Kernel:
```xml
<!-- DELETE THIS LINE -->
<!-- <PackageReference Include="Microsoft.SemanticKernel" Version="1.32.0" /> -->
```

---

## 2. OpenAI/GitHub Models Chat Client Implementation

### File: `Services/Chat/GitHubChatClient.cs` (COMPLETE FILE)

**Location**: `c:\Users\admin\workspace\Core\Three2025\Services\Chat\GitHubChatClient.cs`  
**Lines**: 1-167

```csharp
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
        
        // ⭐ KEY CODE: Create Azure AI Inference client and convert to IChatClient
        _innerClient = new ChatCompletionsClient(
            new Uri("https://models.github.ai/inference"),
            new AzureKeyCredential(token),
            new AzureAIInferenceClientOptions())
            .AsIChatClient(modelId);  // ← CRITICAL: .AsIChatClient() extension
        
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

    public void Dispose()
    {
        // No resources to dispose
    }

    public TService? GetService<TService>(object? key = null) where TService : class
    {
        return _innerClient.GetService<TService>(key);
    }
}
```

**Key Points:**
- ✅ Uses `Azure.AI.Inference.ChatCompletionsClient` 
- ✅ Converts to `IChatClient` via `.AsIChatClient(modelId)` extension
- ✅ GitHub Models endpoint: `https://models.github.ai/inference`
- ✅ Works with GitHub PAT token via `AzureKeyCredential`
- ✅ Supports streaming and non-streaming
- ✅ Built-in logging for debugging

---

## 3. Provider Pattern (Multi-Provider Support)

### File: `Services/Chat/IChatProvider.cs`

```csharp
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

/// <summary>
/// Interface for chat provider implementations
/// </summary>
public interface IChatProvider
{
    string ProviderName { get; }
    string ModelName { get; }
    IChatClient GetChatClient();
}
```

### File: `Services/Chat/GitHubModelProvider.cs`

**Location**: `c:\Users\admin\workspace\Core\Three2025\Services\Chat\GitHubModelProvider.cs`

```csharp
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Three2025.Services.Chat;

#nullable enable

/// <summary>
/// Provider implementation for GitHub Models
/// </summary>
public class GitHubModelProvider : IChatProvider
{
    private readonly string _token;
    private readonly string _modelName;
    private readonly ILogger<GitHubChatClient> _logger;
    private IChatClient? _client;

    public GitHubModelProvider(string token, string modelName, ILogger<GitHubChatClient> logger)
    {
        _token = token;
        _modelName = modelName;
        _logger = logger;
    }

    public string ProviderName => "GitHub Models";
    
    public string ModelName => _modelName;

    public IChatClient GetChatClient()
    {
        return _client ??= new GitHubChatClient(_token, _modelName, _logger);
    }
}
```

### File: `Services/Chat/OllamaProvider.cs` (For Local LLMs)

**Location**: `c:\Users\admin\workspace\Core\Three2025\Services\Chat\OllamaProvider.cs`

```csharp
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace Three2025.Services.Chat;

#nullable enable

/// <summary>
/// Provider implementation for Ollama (local LLM)
/// </summary>
public class OllamaProvider : IChatProvider
{
    private readonly string _endpoint;
    private readonly string _modelName;
    private IChatClient? _client;

    public OllamaProvider(string endpoint, string modelName)
    {
        _endpoint = endpoint;
        _modelName = modelName;
    }

    public string ProviderName => "Ollama (Local)";
    
    public string ModelName => _modelName;

    public IChatClient GetChatClient()
    {
        if (_client != null) return _client;

        // ⭐ OllamaApiClient from OllamaSharp already implements IChatClient
        _client = new OllamaApiClient(_endpoint, _modelName);
        
        return _client;
    }
}
```

### File: `Services/Chat/BedrockProvider.cs` (For AWS Claude)

**Location**: `c:\Users\admin\workspace\Core\Three2025\Services\Chat\BedrockProvider.cs`

```csharp
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

#nullable enable

/// <summary>
/// Provider implementation for AWS Bedrock
/// </summary>
public class BedrockProvider : IChatProvider
{
    private readonly string _accessKeyId;
    private readonly string _secretAccessKey;
    private readonly string _region;
    private readonly string _modelId;
    private IChatClient? _client;

    public BedrockProvider(string accessKeyId, string secretAccessKey, string region, string modelId)
    {
        _accessKeyId = accessKeyId;
        _secretAccessKey = secretAccessKey;
        _region = region;
        _modelId = modelId;
    }

    public string ProviderName => "AWS Bedrock";
    
    public string ModelName => _modelId;

    public IChatClient GetChatClient()
    {
        if (_client != null) return _client;

        // Create AWS credentials
        var credentials = new BasicAWSCredentials(_accessKeyId, _secretAccessKey);
        
        // Create Bedrock Runtime client
        var bedrockClient = new AmazonBedrockRuntimeClient(
            credentials,
            RegionEndpoint.GetBySystemName(_region));
        
        // ⭐ Use AWSSDK extension to get IChatClient
        _client = bedrockClient.AsIChatClient(_modelId);
        
        return _client;
    }
}
```

---

## 4. Chat Service with Tool/Function Calling

### File: `Services/Chat/MultiProviderChatService.cs` (KEY EXCERPTS)

**Location**: `c:\Users\admin\workspace\Core\Three2025\Services\Chat\MultiProviderChatService.cs`  
**Lines**: 1-548

#### A. Using Statements (Lines 1-6)

```csharp
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;  // For ChatClientAgent
using System.ComponentModel;
using OllamaSharp;
using FoundryMentorModeler.Evaluator;  // For OPResult (optional)
```

#### B. Service Interface (Lines 9-46)

```csharp
public interface IMultiProviderChatService
{
    IReadOnlyList<string> AvailableProviders { get; }
    string CurrentProvider { get; }
    bool SetProvider(string providerName);
    
    // ⭐ KEY: Tools parameter for function calling
    IAsyncEnumerable<string> SendMessageStreamingAsync(
        string userMessage, 
        List<ChatMessage> conversationHistory,
        IEnumerable<AIFunction>? tools = null,  // ← Function calling
        CancellationToken cancellationToken = default);
    
    Task<string> SendMessageAsync(
        string userMessage, 
        List<ChatMessage> conversationHistory,
        IEnumerable<AIFunction>? tools = null,  // ← Function calling
        CancellationToken cancellationToken = default);
}
```

#### C. Initialization (Lines 60-176)

```csharp
public class MultiProviderChatService : IMultiProviderChatService
{
    private readonly Dictionary<string, IChatProvider> _providers = new();
    private IChatProvider? _currentProvider;
    private ChatClientAgent? _currentAgent;
    private List<AIFunction> _currentTools = new();
    private readonly ILogger<MultiProviderChatService> _logger;

    public MultiProviderChatService(
        IConfiguration configuration, 
        ILogger<MultiProviderChatService> logger,
        ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        
        // Initialize providers from configuration
        InitializeProviders(configuration);
    }

    private void InitializeProviders(IConfiguration configuration)
    {
        // GitHub Models
        var githubToken = configuration["GitHubPatToken"] ?? 
                         Environment.GetEnvironmentVariable("GitHubPatToken");
        
        if (!string.IsNullOrEmpty(githubToken))
        {
            var githubLogger = _loggerFactory.CreateLogger<GitHubChatClient>();
            _providers["GitHub Models"] = new GitHubModelProvider(
                githubToken, 
                "gpt-4o-mini",  // or "gpt-4o"
                githubLogger);
        }

        // AWS Bedrock
        var awsAccessKey = configuration["AWS_ACCESS_KEY_ID"] ?? 
                          Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
        var awsSecretKey = configuration["AWS_SECRET_ACCESS_KEY"] ?? 
                          Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
        var awsRegion = configuration["AWS_REGION"] ?? "us-east-1";

        if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
        {
            var awsModel = "anthropic.claude-3-5-sonnet-20241022-v2:0";
            _providers["AWS Bedrock"] = new BedrockProvider(
                awsAccessKey, 
                awsSecretKey, 
                awsRegion, 
                awsModel);
        }

        // Ollama (local)
        var ollamaEndpoint = configuration["OLLAMA_ENDPOINT"] ?? "http://localhost:11434";
        var ollamaModel = configuration["OLLAMA_MODEL"] ?? "llama3.2";
        
        _providers["Ollama (Local)"] = new OllamaProvider(ollamaEndpoint, ollamaModel);

        // Set default provider
        if (_providers.ContainsKey("AWS Bedrock"))
            _currentProvider = _providers["AWS Bedrock"];
        else if (_providers.ContainsKey("GitHub Models"))
            _currentProvider = _providers["GitHub Models"];
        else
            _currentProvider = _providers["Ollama (Local)"];
        
        InitializeAgent();
    }
```

#### D. Agent Initialization with Tools (Lines 177-220)

```csharp
    private void InitializeAgent(IEnumerable<AIFunction>? tools = null)
    {
        if (_currentProvider == null) return;

        var chatClient = _currentProvider.GetChatClient();

        // ⭐ Combine built-in tools with provided tools
        var allTools = new List<AIFunction>();
        
        // Built-in tool example
        var dateTimeTool = AIFunctionFactory.Create(GetCurrentDateTime);
        allTools.Add(dateTimeTool);
        
        if (tools != null)
        {
            allTools.AddRange(tools);
            _currentTools = tools.ToList();
            _logger.LogInformation($"Agent initialized with {allTools.Count} tools");
        }
        
        // ⭐ KEY: Create agent with tools using extension method
        if (chatClient is OllamaApiClient)
        {
            // Ollama: Use ChatClientAgent directly
            _currentAgent = new ChatClientAgent(
                chatClient,
                name: "Assistant",
                instructions: "You are a helpful assistant.");
        }
        else
        {
            // Other providers: Use CreateAIAgent extension
            _currentAgent = chatClient.CreateAIAgent(
                instructions: "You are a helpful assistant. Use the available tools to help users.",
                name: "Assistant",
                tools: allTools.ToArray());  // ← Tools passed here
        }
    }

    [Description("Get the current date and time")]
    private string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
```

#### E. Streaming Chat with Automatic Tool Execution (Lines 234-430)

```csharp
    public async IAsyncEnumerable<string> SendMessageStreamingAsync(
        string userMessage, 
        List<ChatMessage> conversationHistory,
        IEnumerable<AIFunction>? tools = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] 
        CancellationToken cancellationToken = default)
    {
        if (_currentProvider == null)
        {
            yield return "Error: No AI provider configured.";
            yield break;
        }

        var chatClient = _currentProvider.GetChatClient();
        
        // ⭐ Create chat options with tools
        ChatOptions? chatOptions = null;
        var toolsList = tools?.ToList() ?? new List<AIFunction>();
        if (toolsList.Any())
        {
            chatOptions = new ChatOptions
            {
                Tools = toolsList.Select(t => (AITool)t).ToList()  // ← Pass tools
            };
            _logger.LogInformation($"🔧 Passing {toolsList.Count} tools to LLM");
        }
        
        // Timeout handling
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, 
            timeoutCts.Token);
        
        var chunks = new List<string>();
        
        // ⭐ Multi-turn loop for automatic tool execution
        int maxTurns = 5;
        int currentTurn = 0;
        
        try
        {
            while (currentTurn < maxTurns)
            {
                currentTurn++;
                var toolCallsInThisTurn = new List<FunctionCallContent>();
                var textInThisTurn = new System.Text.StringBuilder();
                
                _logger.LogInformation($"🔄 Turn {currentTurn}: Calling LLM...");
                
                // ⭐ Stream response from LLM
                await foreach (var update in chatClient.GetStreamingResponseAsync(
                    conversationHistory, 
                    options: chatOptions, 
                    linkedCts.Token))
                {
                    // Collect tool calls
                    if (update.Contents != null)
                    {
                        foreach (var content in update.Contents)
                        {
                            if (content is FunctionCallContent toolCall)
                            {
                                toolCallsInThisTurn.Add(toolCall);
                                _logger.LogInformation($"🔧 Tool call: {toolCall.Name}");
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
                
                // Add assistant's response to history
                var assistantMessage = textInThisTurn.ToString();
                if (!string.IsNullOrEmpty(assistantMessage) || toolCallsInThisTurn.Any())
                {
                    var assistantContents = new List<AIContent>();
                    if (!string.IsNullOrEmpty(assistantMessage))
                    {
                        assistantContents.Add(new TextContent(assistantMessage));
                    }
                    assistantContents.AddRange(toolCallsInThisTurn);
                    
                    conversationHistory.Add(new ChatMessage(ChatRole.Assistant, assistantContents));
                }
                
                // ⭐ Execute tools if any were called
                if (toolCallsInThisTurn.Any())
                {
                    _logger.LogInformation($"⚙️ Executing {toolCallsInThisTurn.Count} tool calls...");
                    
                    foreach (var toolCall in toolCallsInThisTurn)
                    {
                        var tool = toolsList.FirstOrDefault(t => t.Name == toolCall.Name);
                        if (tool != null)
                        {
                            try
                            {
                                _logger.LogInformation($"▶️ Executing: {toolCall.Name}");
                                
                                // ⭐ Execute the tool
                                var args = toolCall.Arguments != null 
                                    ? new AIFunctionArguments(toolCall.Arguments) 
                                    : new AIFunctionArguments();
                                
                                var result = await tool.InvokeAsync(args, cancellationToken);
                                
                                _logger.LogInformation($"✅ Tool result: {result}");
                                
                                // ⭐ Add tool result to conversation
                                var resultContent = new FunctionResultContent(
                                    toolCall.CallId, 
                                    result);
                                conversationHistory.Add(
                                    new ChatMessage(ChatRole.Tool, [resultContent]));
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"❌ Tool failed: {ex.Message}");
                                var errorContent = new FunctionResultContent(
                                    toolCall.CallId, 
                                    $"Error: {ex.Message}");
                                conversationHistory.Add(
                                    new ChatMessage(ChatRole.Tool, [errorContent]));
                            }
                        }
                    }
                    
                    // Continue conversation to process tool results
                    _logger.LogInformation($"🔄 Continuing with tool results...");
                }
                else
                {
                    // No tool calls - done
                    break;
                }
            }
            
            _logger.LogInformation($"✅ Complete: {chunks.Count} chunks");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Error: {ex.Message}");
            yield return $"\n\n❌ **Error**: {ex.Message}";
            yield break;
        }
        
        // Yield all collected chunks
        foreach (var chunk in chunks)
        {
            yield return chunk;
        }
    }
```

---

## 5. Creating Tools from Methods

### File: `Services/Agents/TechnicianToolProvider.cs` (EXCERPT)

**How to convert C# methods to AIFunction:**

```csharp
using System.ComponentModel;
using System.Reflection;
using Microsoft.Extensions.AI;

public class TechnicianToolProvider
{
    private readonly IServiceProvider _serviceProvider;
    
    public IEnumerable<AIFunction> DiscoverAllTools()
    {
        var tools = new List<AIFunction>();
        
        // Get your service from DI
        var myService = _serviceProvider.GetService<IMyService>();
        if (myService == null) return tools;
        
        var implementationType = myService.GetType();
        
        // ⭐ Find methods with [Description] attribute
        var methods = implementationType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null)
            .ToList();
        
        foreach (var method in methods)
        {
            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
            
            // ⭐ Create AIFunction using AIFunctionFactory
            var func = AIFunctionFactory.Create(
                method,
                target: myService,  // The instance to invoke on
                name: method.Name,
                description: descAttr!.Description);
            
            tools.Add(func);
        }
        
        return tools;
    }
}
```

### Example Tool Definition

```csharp
using System.ComponentModel;

public class MyService
{
    // ⭐ This method becomes a tool automatically
    [Description("Create a 3D box shape with specified dimensions")]
    public string CreateBox(
        [Description("Name of the box")] string name,
        [Description("Width in meters")] double width,
        [Description("Height in meters")] double height,
        [Description("Depth in meters")] double depth)
    {
        // Your implementation
        return $"Created box '{name}' with size {width}x{height}x{depth}";
    }
    
    // ⭐ Private methods or methods without [Description] are NOT exposed
    private void InternalHelper()
    {
        // Not a tool
    }
}
```

---

## 6. Configuration & Secrets

### appsettings.json

```json
{
  "GitHubPatToken": "",  // Leave empty, use User Secrets
  "AWS_ACCESS_KEY_ID": "",
  "AWS_SECRET_ACCESS_KEY": "",
  "AWS_REGION": "us-east-1",
  "AWS_BEDROCK_MODEL": "anthropic.claude-3-5-sonnet-20241022-v2:0",
  "OLLAMA_ENDPOINT": "http://localhost:11434",
  "OLLAMA_MODEL": "llama3.2"
}
```

### User Secrets (recommended for tokens)

```bash
dotnet user-secrets init
dotnet user-secrets set "GitHubPatToken" "your-github-pat-here"
dotnet user-secrets set "AWS_ACCESS_KEY_ID" "your-aws-key"
dotnet user-secrets set "AWS_SECRET_ACCESS_KEY" "your-aws-secret"
```

---

## 7. Key Differences from Semantic Kernel

### OLD (Semantic Kernel)

```csharp
using Microsoft.SemanticKernel;

// Create kernel
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4", apiKey)
    .Build();

// Add functions
kernel.Plugins.AddFromType<MyPlugin>();

// Invoke
var result = await kernel.InvokePromptAsync("Hello");
```

### NEW (Microsoft.Extensions.AI)

```csharp
using Microsoft.Extensions.AI;
using Azure;
using Azure.AI.Inference;

// Create chat client
var chatClient = new ChatCompletionsClient(
    new Uri("https://models.github.ai/inference"),
    new AzureKeyCredential(githubToken),
    new AzureAIInferenceClientOptions())
    .AsIChatClient("gpt-4o-mini");

// Create tools
var tools = new List<AIFunction>();
var tool = AIFunctionFactory.Create(MyMethod);
tools.Add(tool);

// Create chat options with tools
var options = new ChatOptions
{
    Tools = tools.Select(t => (AITool)t).ToList()
};

// Invoke
var messages = new List<ChatMessage>
{
    new(ChatRole.User, "Hello")
};

await foreach (var chunk in chatClient.GetStreamingResponseAsync(messages, options))
{
    Console.Write(chunk.Text);
}
```

---

## 8. Complete Minimal Example

### Minimal Working Chat with Tools

```csharp
using Microsoft.Extensions.AI;
using Azure;
using Azure.AI.Inference;
using System.ComponentModel;

class Program
{
    static async Task Main()
    {
        // 1. Create chat client
        var chatClient = new ChatCompletionsClient(
            new Uri("https://models.github.ai/inference"),
            new AzureKeyCredential("your-github-pat"),
            new AzureAIInferenceClientOptions())
            .AsIChatClient("gpt-4o-mini");

        // 2. Create a tool
        var weatherTool = AIFunctionFactory.Create(GetWeather);
        
        // 3. Create options with tools
        var options = new ChatOptions
        {
            Tools = new List<AITool> { weatherTool }
        };

        // 4. Chat with conversation history
        var history = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant."),
            new(ChatRole.User, "What's the weather in Seattle?")
        };

        // 5. Call LLM (streaming)
        await foreach (var update in chatClient.GetStreamingResponseAsync(history, options))
        {
            Console.Write(update.Text);
            
            // Handle tool calls
            if (update.Contents != null)
            {
                foreach (var content in update.Contents)
                {
                    if (content is FunctionCallContent toolCall)
                    {
                        Console.WriteLine($"\nTool called: {toolCall.Name}");
                        
                        // Execute tool
                        var args = new AIFunctionArguments(toolCall.Arguments);
                        var result = await weatherTool.InvokeAsync(args);
                        
                        Console.WriteLine($"Tool result: {result}");
                        
                        // Add result to history
                        history.Add(new ChatMessage(ChatRole.Assistant, [toolCall]));
                        history.Add(new ChatMessage(ChatRole.Tool, 
                            [new FunctionResultContent(toolCall.CallId, result)]));
                        
                        // Continue conversation
                        await foreach (var chunk in chatClient.GetStreamingResponseAsync(history, options))
                        {
                            Console.Write(chunk.Text);
                        }
                    }
                }
            }
        }
    }

    [Description("Get the current weather for a city")]
    static string GetWeather([Description("The city name")] string city)
    {
        return $"The weather in {city} is sunny, 72°F";
    }
}
```

---

## 9. File Paths Reference

**Key Implementation Files in Three2025:**

| File | Path | Purpose |
|------|------|---------|
| GitHubChatClient.cs | `Services/Chat/GitHubChatClient.cs` | OpenAI/GitHub client wrapper |
| GitHubModelProvider.cs | `Services/Chat/GitHubModelProvider.cs` | Provider factory |
| OllamaProvider.cs | `Services/Chat/OllamaProvider.cs` | Local LLM provider |
| BedrockProvider.cs | `Services/Chat/BedrockProvider.cs` | AWS Claude provider |
| MultiProviderChatService.cs | `Services/Chat/MultiProviderChatService.cs` | Main chat service with tools |
| TechnicianToolProvider.cs | `Services/Agents/TechnicianToolProvider.cs` | Tool discovery |
| ThreeDModelingAgent.cs | `Services/Chat/Agents/ThreeDModelingAgent.cs` | Specialized agent |
| ChatOrchestrator.cs | `Services/Chat/ChatOrchestrator.cs` | Agent coordination |
| Three2025.csproj | Root | Package references |

---

## 10. Migration Checklist

- [ ] Install NuGet packages (see section 1)
- [ ] Remove `Microsoft.SemanticKernel` package
- [ ] Create `IChatProvider` interface
- [ ] Implement provider(s) - GitHub/Ollama/Bedrock
- [ ] Create chat service with tool support
- [ ] Replace `[KernelFunction]` with `[Description]`
- [ ] Update tool creation to use `AIFunctionFactory.Create()`
- [ ] Replace `Kernel.InvokePromptAsync()` with `chatClient.GetStreamingResponseAsync()`
- [ ] Update conversation history to use `List<ChatMessage>`
- [ ] Implement tool execution loop
- [ ] Update DI registration
- [ ] Test streaming responses
- [ ] Test tool calling
- [ ] Remove all `using Microsoft.SemanticKernel` statements

---

## Summary

**Three Key Changes:**

1. **Client Creation**: 
   ```csharp
   // OLD: Kernel.CreateBuilder().AddOpenAIChatCompletion()
   // NEW: new ChatCompletionsClient(...).AsIChatClient()
   ```

2. **Tool Definition**:
   ```csharp
   // OLD: [KernelFunction("name")]
   // NEW: [Description("description")] + AIFunctionFactory.Create()
   ```

3. **Execution**:
   ```csharp
   // OLD: kernel.InvokePromptAsync()
   // NEW: chatClient.GetStreamingResponseAsync(messages, options)
   ```

**The provider pattern makes multi-LLM support trivial** - just implement `IChatProvider` and return an `IChatClient`.

Good luck with Plugin710 migration!

