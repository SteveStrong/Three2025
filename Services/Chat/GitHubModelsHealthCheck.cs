using Microsoft.Extensions.AI;

namespace Three2025.Services.Chat;

public static class GitHubModelsHealthCheck
{
    public static async Task<(bool IsHealthy, string Message)> CheckHealthAsync(
        IConfiguration configuration,
        ILogger logger)
    {
        try
        {
            var githubToken = configuration["GitHubPatToken"] ?? 
                             Environment.GetEnvironmentVariable("GitHubPatToken", EnvironmentVariableTarget.User);
            
            if (string.IsNullOrEmpty(githubToken))
            {
                return (false, "❌ GitHubPatToken not configured in environment");
            }

            logger.LogInformation("🔍 Testing GitHub Models connectivity...");

            // Use the same client creation as GitHubChatClient
            var client = new GitHubChatClient(githubToken, "gpt-4o-mini");

            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, "test")
            };

            // Quick test with minimal tokens
            var options = new ChatOptions
            {
                Temperature = 0,
                MaxOutputTokens = 10
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            var response = await client.GetResponseAsync(messages, options, cts.Token);

            if (response != null && response.FinishReason != null)
            {
                logger.LogInformation("✅ GitHub Models health check PASSED");
                return (true, "✅ GitHub Models is accessible");
            }

            return (false, "⚠️ GitHub Models returned empty response");
        }
        catch (TaskCanceledException)
        {
            return (false, "⏱️ GitHub Models request timed out (5s)");
        }
        catch (Exception ex) when (ex.Message.Contains("429") || ex.Message.Contains("Too Many Requests"))
        {
            return (false, "🚫 GitHub Models RATE LIMITED - Try again later or switch providers");
        }
        catch (Exception ex) when (ex.Message.Contains("401") || ex.Message.Contains("Unauthorized"))
        {
            return (false, "🔐 GitHub Models authentication failed - Check GitHubPatToken");
        }
        catch (Exception ex)
        {
            return (false, $"❌ GitHub Models error: {ex.Message}");
        }
    }
}
