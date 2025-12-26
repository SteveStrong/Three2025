using Microsoft.Extensions.AI;
using Three2025.Services.Chat.Agents;

namespace Three2025.Services.Chat;

public class AgentFactory : IAgentFactory
{
    private readonly IMultiProviderChatService _chatService;
    private readonly ILoggerFactory _loggerFactory;
    
    public AgentFactory(
        IMultiProviderChatService chatService,
        ILoggerFactory loggerFactory)
    {
        _chatService = chatService;
        _loggerFactory = loggerFactory;
    }
    
    public ISpecializedAgent Create3DModelingAgent(IEnumerable<AIFunction> tools)
        => new ThreeDModelingAgent(_chatService, tools, _loggerFactory.CreateLogger<ThreeDModelingAgent>());
    
    public ISpecializedAgent CreateAnimationAgent(IEnumerable<AIFunction> tools)
        => new AnimationAgent(_chatService, tools, _loggerFactory.CreateLogger<AnimationAgent>());
    
    public ISpecializedAgent CreateLightingAgent(IEnumerable<AIFunction> tools)
        => new LightingAgent(_chatService, tools, _loggerFactory.CreateLogger<LightingAgent>());
    
    public ISpecializedAgent CreateClockAgent(IEnumerable<AIFunction> tools)
        => new ClockAgent(_chatService, tools, _loggerFactory.CreateLogger<ClockAgent>());
    
    public ISpecializedAgent CreateGeneralAgent(IEnumerable<AIFunction> tools)
        => new GeneralAgent(_chatService, tools, _loggerFactory.CreateLogger<GeneralAgent>());
}
