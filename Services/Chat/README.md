# AI Chat Multi-Provider Setup

## Overview

The AI Chat Multi-Provider feature has been successfully migrated from SimpleChat (.NET 10 console) to Three2025 (.NET 9.0 Blazor Server). This implementation provides a flexible, multi-provider AI chat interface with real-time streaming, tool calling, and comprehensive debugging capabilities.

## Features

✅ **Multi-Provider Support**: Switch between different AI providers dynamically
- GitHub Models (Azure AI Inference)
- AWS Bedrock (Claude models)

✅ **Real-Time Streaming**: Responses stream in real-time as they're generated

✅ **Tool Calling**: Built-in tools (e.g., GetCurrentDateTime) with automatic execution

✅ **Debug Logging**: Side-by-side debug panel showing:
- Conversation history
- Tool calls with arguments
- Tool results
- Token usage statistics

✅ **Provider Selection**: Dropdown to switch providers on-the-fly

✅ **Blazor Integration**: Full server-side Blazor component with state management

## Architecture

```
Three2025/Services/Chat/
├── IChatProvider.cs              # Provider abstraction interface
├── GitHubModelProvider.cs        # GitHub Models implementation
├── GitHubChatClient.cs          # Custom IChatClient with logging
├── BedrockProvider.cs           # AWS Bedrock implementation
└── MultiProviderChatService.cs  # Blazor service wrapper

Three2025/Components/Pages/
├── AIChatMulti.razor            # UI component
└── AIChatMulti.razor.cs         # Code-behind with logic
```

## Setup Instructions

### 1. GitHub Models Provider

Get a GitHub Personal Access Token with Models access:
1. Visit https://github.com/marketplace?type=models
2. Choose a model (e.g., gpt-4o-mini)
3. Follow instructions to get a PAT token

Set the token as an environment variable:
```bash
# Windows (PowerShell)
[Environment]::SetEnvironmentVariable("GitHubPatToken", "your-token-here", "User")

# Windows (Command Prompt)
setx GitHubPatToken "your-token-here"

# Or use .NET User Secrets:
dotnet user-secrets set "GitHubPatToken" "your-token-here"
```

### 2. AWS Bedrock Provider

Set AWS credentials as environment variables:
```bash
# Windows (PowerShell)
[Environment]::SetEnvironmentVariable("AWS_ACCESS_KEY_ID", "your-access-key", "User")
[Environment]::SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", "your-secret-key", "User")
[Environment]::SetEnvironmentVariable("AWS_REGION", "us-east-1", "User")

# Or use .NET User Secrets:
dotnet user-secrets set "AWS_ACCESS_KEY_ID" "your-access-key"
dotnet user-secrets set "AWS_SECRET_ACCESS_KEY" "your-secret-key"
dotnet user-secrets set "AWS_REGION" "us-east-1"
```

### 3. Run the Application

```bash
cd Three2025
dotnet run
```

Navigate to: http://localhost:5000/aichatmulti

## Usage

1. **Select Provider**: Use the dropdown to choose between GitHub Models and AWS Bedrock
2. **Type Message**: Enter your message in the input field
3. **Send**: Press Enter or click the Send button
4. **View Debug**: Check the right panel for detailed execution logs
5. **Clear**: Click "Clear Chat" to start a new conversation

## Key Components

### IChatProvider Interface
Abstraction that allows easy addition of new AI providers:
```csharp
public interface IChatProvider
{
    IChatClient GetChatClient();
    string ProviderName { get; }
    string ModelName { get; }
}
```

### GitHubChatClient
Custom wrapper around Azure.AI.Inference that provides:
- Conversation logging
- Tool call visibility
- Token usage tracking
- Event-based logging for UI updates

### MultiProviderChatService
Blazor-specific service that:
- Manages provider lifecycle
- Handles conversation history
- Provides streaming responses
- Integrates with dependency injection
- Supports tool calling with automatic registration

## Adding New Providers

To add a new AI provider:

1. Create a class implementing `IChatProvider`
2. Implement `GetChatClient()` to return an `IChatClient`
3. Add provider initialization in `MultiProviderChatService` constructor
4. Configure credentials via environment variables or user secrets

Example:
```csharp
public class MyNewProvider : IChatProvider
{
    public string ProviderName => "My Provider";
    public string ModelName => "my-model-v1";
    
    public IChatClient GetChatClient()
    {
        // Return your IChatClient implementation
    }
}
```

## Differences from SimpleChat

| Feature | SimpleChat | Three2025 |
|---------|-----------|-----------|
| Framework | .NET 10 Console | .NET 9.0 Blazor Server |
| UI | Console-based | Web-based with Radzen |
| State | Synchronous | Async with component state |
| Logging | Console.WriteLine | Event-based UI updates |
| Streaming | Direct console | StringBuilder accumulation |
| Multi-Agent | SysML workflows | (Not yet implemented) |

## Future Enhancements

- [ ] Add multi-agent workflow support from SimpleChat
- [ ] Implement conversation persistence
- [ ] Add more AI providers (OpenAI, Anthropic Direct, etc.)
- [ ] Add custom tool registration UI
- [ ] Export conversation to file
- [ ] Conversation search and filtering
- [ ] Provider-specific settings UI

## Troubleshooting

### "No AI providers configured"
- Ensure environment variables are set correctly
- Restart the application after setting environment variables
- Check that tokens/credentials are valid

### Build Warnings
- Nullable warnings are expected (project has nullable disabled)
- No errors should appear - only warnings

### Package Compatibility
All preview packages work correctly with .NET 9.0:
- Azure.AI.Inference (beta)
- Microsoft.Extensions.AI (preview)
- Microsoft.Agents.AI (preview)

## Navigation

Access the chat interface via:
- Navigation Menu: Click "💬 AI Chat"
- Direct URL: `/aichatmulti`

## Dependencies Added

```xml
<PackageReference Include="Azure.AI.Inference" Version="1.0.0-beta.5" />
<PackageReference Include="Microsoft.Extensions.AI.AzureAIInference" Version="10.0.0-preview.1.25559.3" />
<PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.251219.1" />
<PackageReference Include="AWSSDK.BedrockRuntime" Version="4.0.14.3" />
<PackageReference Include="AWSSDK.Extensions.Bedrock.MEAI" Version="4.0.5.1" />
<PackageReference Include="Microsoft.Extensions.Configuration.UserSecrets" Version="9.0.0" />
```

## Git Branch

This implementation was developed on the `add-agents` branch.

---

**Migration Date**: December 25, 2025  
**Status**: ✅ Complete and Building Successfully
