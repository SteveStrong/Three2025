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

        // Create Ollama client using OllamaSharp (which implements IChatClient)
        _client = new OllamaApiClient(_endpoint, _modelName);
        
        return _client;
    }
}
