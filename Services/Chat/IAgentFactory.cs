using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

/// <summary>
/// Factory for creating specialized agents with tool support
/// Currently simplified to focus on 3D modeling with Shape3DTech tools
/// </summary>
public interface IAgentFactory
{
    /// <summary>
    /// Create the primary 3D modeling agent with Shape3DTech tools
    /// </summary>
    ISpecializedAgent Create3DModelingAgent(IEnumerable<AIFunction> tools);
    
    // FUTURE AGENTS - Commented out for simplicity
    // ISpecializedAgent CreateAnimationAgent(IEnumerable<AIFunction> tools);
    // ISpecializedAgent CreateGeometryAgent(IEnumerable<AIFunction> tools);
    // ISpecializedAgent CreateClockAgent(IEnumerable<AIFunction> tools);
    // ISpecializedAgent CreateGeneralAgent(IEnumerable<AIFunction> tools);
}
