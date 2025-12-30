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
        
        // Use the AWSSDK.Extensions.Bedrock.MEAI extension to get IChatClient
        _client = bedrockClient.AsIChatClient(_modelId);
        
        return _client;
    }
}
