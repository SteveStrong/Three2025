#nullable enable
using System.Net.Http.Headers;
using System.Text.Json;

namespace Three2025.Services.Agents;

/// <summary>
/// Utility to check GitHub API rate limits
/// </summary>
public class RateLimitChecker
{
    private readonly string _token;

    public RateLimitChecker(string token)
    {
        _token = token;
    }

    public async Task<RateLimitInfo> CheckRateLimitAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SimpleChat", "1.0"));

        try
        {
            // Check GitHub Models API rate limits
            var modelsResponse = await httpClient.GetAsync("https://models.github.ai/rate_limit");
            
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== GitHub Models API Rate Limit Check ===");
            Console.WriteLine($"Status Code: {modelsResponse.StatusCode}");
            
            if (modelsResponse.IsSuccessStatusCode)
            {
                var content = await modelsResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Response: {content}");
            }
            else
            {
                Console.WriteLine($"Error: {modelsResponse.ReasonPhrase}");
                var errorContent = await modelsResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Details: {errorContent}");
            }
            
            // Also check standard GitHub API rate limits
            var githubResponse = await httpClient.GetAsync("https://api.github.com/rate_limit");
            
            Console.WriteLine("\n=== GitHub API Rate Limit ===");
            Console.WriteLine($"Status Code: {githubResponse.StatusCode}");
            
            if (githubResponse.IsSuccessStatusCode)
            {
                var content = await githubResponse.Content.ReadAsStringAsync();
                var rateLimitData = JsonSerializer.Deserialize<JsonElement>(content);
                
                if (rateLimitData.TryGetProperty("resources", out var resources))
                {
                    if (resources.TryGetProperty("core", out var core))
                    {
                        var limit = core.GetProperty("limit").GetInt32();
                        var remaining = core.GetProperty("remaining").GetInt32();
                        var reset = DateTimeOffset.FromUnixTimeSeconds(core.GetProperty("reset").GetInt64());
                        
                        Console.WriteLine($"Core API - Limit: {limit}, Remaining: {remaining}");
                        Console.WriteLine($"Reset Time: {reset.ToLocalTime()}");
                        
                        if (remaining == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("⚠️ WARNING: Rate limit exhausted!");
                            Console.WriteLine($"Rate limit will reset at: {reset.ToLocalTime()}");
                        }
                    }
                }
            }
            
            Console.WriteLine("==========================================");
            Console.ResetColor();
            
            return new RateLimitInfo { IsRateLimited = false };
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n=== Error Checking Rate Limits ===");
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine("===================================");
            Console.ResetColor();
            
            return new RateLimitInfo { IsRateLimited = false, Error = ex.Message };
        }
    }

    public async Task TestSimpleCallAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SimpleChat", "1.0"));
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== Testing GitHub Models API Connection ===");
            
            var testPayload = new
            {
                messages = new[]
                {
                    new { role = "user", content = "Say 'Hello'" }
                },
                model = "gpt-4o-mini",
                max_tokens = 10
            };
            
            var jsonContent = JsonSerializer.Serialize(testPayload);
            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            Console.WriteLine("Sending test request to https://models.github.ai/inference/chat/completions...");
            
            var response = await httpClient.PostAsync(
                "https://models.github.ai/inference/chat/completions",
                httpContent);
            
            Console.WriteLine($"Response Status: {response.StatusCode}");
            
            // Check rate limit headers
            if (response.Headers.TryGetValues("x-ratelimit-limit", out var limitValues))
            {
                Console.WriteLine($"Rate Limit: {string.Join(", ", limitValues)}");
            }
            if (response.Headers.TryGetValues("x-ratelimit-remaining", out var remainingValues))
            {
                Console.WriteLine($"Rate Limit Remaining: {string.Join(", ", remainingValues)}");
            }
            if (response.Headers.TryGetValues("x-ratelimit-reset", out var resetValues))
            {
                Console.WriteLine($"Rate Limit Reset: {string.Join(", ", resetValues)}");
            }
            
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ API call successful!");
                Console.WriteLine($"Response (truncated): {(responseContent.Length > 200 ? responseContent.Substring(0, 200) + "..." : responseContent)}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("✗ API call failed!");
                Console.WriteLine($"Response: {responseContent}");
            }
            
            Console.WriteLine("==========================================");
            Console.ResetColor();
        }
        catch (TaskCanceledException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Request timed out: {ex.Message}");
            Console.WriteLine("The API might be slow or unresponsive.");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            Console.WriteLine($"Type: {ex.GetType().Name}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            Console.ResetColor();
        }
    }
}

public class RateLimitInfo
{
    public bool IsRateLimited { get; set; }
    public string? Error { get; set; }
}
