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
