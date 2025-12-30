#nullable enable
using Microsoft.Extensions.AI;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Azure.AI.Inference;

namespace Three2025.Services.Chat;

public static class AIProviderHealthCheck
{
    public static async Task<(bool IsHealthy, string Message, string Provider)> CheckHealthAsync(
        IConfiguration configuration,
        ILogger logger)
    {
        // Determine which provider is configured (same logic as MultiProviderChatService)
        // Check Environment variables directly since EnvConfig sets them there
        var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? 
                          configuration["AWS_ACCESS_KEY_ID"];
        var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? 
                          configuration["AWS_SECRET_ACCESS_KEY"];
        
        var githubToken = Environment.GetEnvironmentVariable("GitHubPatToken") ?? 
                         configuration["GitHubPatToken"];
        
        logger.LogInformation($"🔍 Checking credentials - AWS: {!string.IsNullOrEmpty(awsAccessKey)}, GitHub: {!string.IsNullOrEmpty(githubToken)}");
        
        // Check AWS Bedrock first (preferred provider)
        if (!string.IsNullOrEmpty(awsAccessKey) && !string.IsNullOrEmpty(awsSecretKey))
        {
            return await CheckBedrockHealthAsync(configuration, logger, awsAccessKey, awsSecretKey);
        }
        
        // Fallback to GitHub Models
        if (!string.IsNullOrEmpty(githubToken))
        {
            return await CheckGitHubHealthAsync(configuration, logger, githubToken);
        }
        
        return (false, "❌ No AI provider configured (no AWS or GitHub credentials)", "None");
    }
    
    private static async Task<(bool IsHealthy, string Message, string Provider)> CheckBedrockHealthAsync(
        IConfiguration configuration,
        ILogger logger,
        string awsAccessKey,
        string awsSecretKey)
    {
        try
        {
            logger.LogInformation("🔍 Testing AWS Bedrock connectivity...");
            
            var awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? 
                           configuration["AWS_REGION"] ?? 
                           "us-east-1";
            
            // Use GovCloud model prefix if in gov region
            var modelId = awsRegion.Contains("gov") 
                ? "us.anthropic.claude-3-5-sonnet-20241022-v2:0"
                : "anthropic.claude-3-5-sonnet-20241022-v2:0";
            
            modelId = Environment.GetEnvironmentVariable("AWS_BEDROCK_MODEL") ??
                     configuration["AWS_BEDROCK_MODEL"] ??
                     modelId;
            
            logger.LogInformation($"   Region: {awsRegion}, Model: {modelId}");
            
            // Create AWS credentials (same as BedrockProvider)
            var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
            
            // Create Bedrock Runtime client
            var bedrockClient = new AmazonBedrockRuntimeClient(
                credentials,
                RegionEndpoint.GetBySystemName(awsRegion));
            
            // Use the AWSSDK.Extensions.Bedrock.MEAI extension to get IChatClient
            var client = bedrockClient.AsIChatClient(modelId);
            
            var messages = new List<Microsoft.Extensions.AI.ChatMessage>
            {
                new(Microsoft.Extensions.AI.ChatRole.User, "test")
            };
            
            var options = new Microsoft.Extensions.AI.ChatOptions
            {
                Temperature = 0,
                MaxOutputTokens = 10
            };
            
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            
            var response = await client.GetResponseAsync(messages, options, cts.Token);
            
            if (response != null && response.FinishReason != null)
            {
                logger.LogInformation("✅ AWS Bedrock health check PASSED");
                return (true, "✅ AWS Bedrock is accessible", "AWS Bedrock");
            }
            
            return (false, "⚠️ AWS Bedrock returned empty response", "AWS Bedrock");
        }
        catch (TaskCanceledException)
        {
            return (false, "⏱️ AWS Bedrock request timed out (10s)", "AWS Bedrock");
        }
        catch (Exception ex) when (ex.Message.Contains("ThrottlingException") || ex.Message.Contains("TooManyRequests"))
        {
            var rateLimitInfo = ExtractRateLimitInfo(ex);
            var message = "🚫 AWS Bedrock RATE LIMITED - Try again later";
            if (!string.IsNullOrEmpty(rateLimitInfo))
            {
                message += $"\n   📊 Rate Limit Details: {rateLimitInfo}";
            }
            return (false, message, "AWS Bedrock");
        }
        catch (Exception ex) when (ex.Message.Contains("UnrecognizedClientException") || ex.Message.Contains("InvalidSignature"))
        {
            return (false, "🔐 AWS Bedrock authentication failed - Check AWS credentials", "AWS Bedrock");
        }
        catch (Exception ex)
        {
            return (false, $"❌ AWS Bedrock error: {ex.Message}", "AWS Bedrock");
        }
    }
    
    private static async Task<(bool IsHealthy, string Message, string Provider)> CheckGitHubHealthAsync(
        IConfiguration configuration,
        ILogger logger,
        string githubToken)
    {
        Azure.Response<ChatCompletions>? rawResponse = null;
        
        try
        {
            logger.LogInformation("🔍 Testing GitHub Models connectivity...");

            // Use raw Azure client to get headers
            var client = new Azure.AI.Inference.ChatCompletionsClient(
                new Uri("https://models.github.ai/inference"),
                new Azure.AzureKeyCredential(githubToken),
                new Azure.AI.Inference.AzureAIInferenceClientOptions());

            var chatMessages = new[]
            {
                new ChatRequestUserMessage("test")
            };

            var completionsOptions = new ChatCompletionsOptions(chatMessages)
            {
                Temperature = 0,
                MaxTokens = 10,
                Model = "gpt-4o-mini"
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            rawResponse = await client.CompleteAsync(completionsOptions, cts.Token);

            if (rawResponse?.Value != null)
            {
                var rateLimitInfo = ExtractGitHubRateLimitHeaders(rawResponse);
                
                logger.LogInformation("✅ GitHub Models health check PASSED");
                
                var message = "✅ GitHub Models is accessible";
                if (!string.IsNullOrEmpty(rateLimitInfo))
                {
                    message += $"\n   📊 Rate Limits: {rateLimitInfo}";
                }
                
                return (true, message, "GitHub Models");
            }

            return (false, "⚠️ GitHub Models returned empty response", "GitHub Models");
        }
        catch (TaskCanceledException)
        {
            return (false, "⏱️ GitHub Models request timed out (5s) - Possible rate limiting or network issue", "GitHub Models");
        }
        catch (Azure.RequestFailedException ex)
        {
            var rateLimitInfo = ex.Status == 429 ? ExtractExceptionRateLimitInfo(ex) : string.Empty;
            
            if (ex.Status == 429)
            {
                var message = "🚫 GitHub Models RATE LIMITED";
                if (!string.IsNullOrEmpty(rateLimitInfo))
                {
                    message += $"\n   📊 Rate Limit Details: {rateLimitInfo}";
                }
                return (false, message, "GitHub Models");
            }
            else if (ex.Status == 401)
            {
                return (false, "🔐 GitHub Models authentication failed - Check GitHubPatToken", "GitHub Models");
            }
            else
            {
                return (false, $"❌ GitHub Models error (Status {ex.Status}): {ex.Message}", "GitHub Models");
            }
        }
        catch (Exception ex)
        {
            return (false, $"❌ GitHub Models error: {ex.Message}", "GitHub Models");
        }
    }
    
    private static string ExtractGitHubRateLimitHeaders(Azure.Response<ChatCompletions> response)
    {
        var info = new List<string>();
        
        try
        {
            var headers = response.GetRawResponse().Headers;
            
            if (headers.TryGetValue("x-ratelimit-limit", out var limit))
                info.Add($"Limit: {limit}");
            
            if (headers.TryGetValue("x-ratelimit-remaining", out var remaining))
                info.Add($"Remaining: {remaining}");
            
            if (headers.TryGetValue("x-ratelimit-reset", out var reset))
            {
                if (long.TryParse(reset, out var resetTimestamp))
                {
                    var resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimestamp).ToLocalTime();
                    info.Add($"Reset: {resetTime:HH:mm:ss}");
                }
                else
                {
                    info.Add($"Reset: {reset}");
                }
            }
            
            if (headers.TryGetValue("retry-after", out var retryAfter))
                info.Add($"Retry-After: {retryAfter}s");
        }
        catch
        {
            // Ignore header parsing errors
        }
        
        return info.Any() ? string.Join(", ", info) : string.Empty;
    }
    
    private static string ExtractExceptionRateLimitInfo(Azure.RequestFailedException ex)
    {
        var info = new List<string>();
        
        try
        {
            // Try to get headers from the exception
            var response = ex.GetRawResponse();
            if (response != null)
            {
                var headers = response.Headers;
                
                if (headers.TryGetValue("x-ratelimit-limit", out var limit))
                    info.Add($"Limit: {limit}");
                
                if (headers.TryGetValue("x-ratelimit-remaining", out var remaining))
                    info.Add($"Remaining: {remaining}");
                
                if (headers.TryGetValue("x-ratelimit-reset", out var reset))
                {
                    if (long.TryParse(reset, out var resetTimestamp))
                    {
                        var resetTime = DateTimeOffset.FromUnixTimeSeconds(resetTimestamp).ToLocalTime();
                        info.Add($"Reset: {resetTime:HH:mm:ss}");
                    }
                    else
                    {
                        info.Add($"Reset: {reset}");
                    }
                }
                
                if (headers.TryGetValue("retry-after", out var retryAfter))
                    info.Add($"Retry-After: {retryAfter}s");
            }
        }
        catch
        {
            // Ignore header parsing errors
        }
        
        return info.Any() ? string.Join(", ", info) : string.Empty;
    }
    
    private static string ExtractRateLimitInfo(Exception ex)
    {
        var info = new List<string>();
        
        // Try to extract rate limit information from exception message
        var message = ex.ToString();
        
        // Common rate limit headers/info to look for
        var patterns = new[]
        {
            @"X-RateLimit-Limit[:\s]+(\d+)",
            @"X-RateLimit-Remaining[:\s]+(\d+)",
            @"X-RateLimit-Reset[:\s]+(\d+)",
            @"Retry-After[:\s]+(\d+)",
            @"requests per (\w+)",
            @"limit[:\s]+(\d+)",
            @"remaining[:\s]+(\d+)"
        };
        
        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(message, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                info.Add(match.Value);
            }
        }
        
        return info.Any() ? string.Join(", ", info) : string.Empty;
    }
}
