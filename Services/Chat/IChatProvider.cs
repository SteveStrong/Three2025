using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

/// <summary>
/// Abstraction for chat providers (GitHub Models, AWS Bedrock, etc.)
/// </summary>
public interface IChatProvider
{
    /// <summary>
    /// Gets the chat client for this provider
    /// </summary>
    IChatClient GetChatClient();
    
    /// <summary>
    /// Name of the provider (e.g., "GitHub Models", "AWS Bedrock")
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// The model being used (e.g., "gpt-4o-mini", "anthropic.claude-3-sonnet")
    /// </summary>
    string ModelName { get; }
}
