using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

/// <summary>
/// Factory for creating specialized agents with tool support
/// </summary>
public interface IAgentFactory
{
    ISpecializedAgent Create3DModelingAgent(IEnumerable<AIFunction> tools);
    ISpecializedAgent CreateAnimationAgent(IEnumerable<AIFunction> tools);
    ISpecializedAgent CreateLightingAgent(IEnumerable<AIFunction> tools);
    ISpecializedAgent CreateClockAgent(IEnumerable<AIFunction> tools);
    ISpecializedAgent CreateGeneralAgent(IEnumerable<AIFunction> tools);
}
