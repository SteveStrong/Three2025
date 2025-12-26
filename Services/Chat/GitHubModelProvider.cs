using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

/// <summary>
/// Provider implementation for GitHub Models
/// </summary>
public class GitHubModelProvider : IChatProvider
{
    private readonly string _token;
    private readonly string _modelName;
    private IChatClient? _client;

    public GitHubModelProvider(string token, string modelName)
    {
        _token = token;
        _modelName = modelName;
    }

    public string ProviderName => "GitHub Models";
    
    public string ModelName => _modelName;

    public IChatClient GetChatClient()
    {
        return _client ??= new GitHubChatClient(_token, _modelName);
    }
}
