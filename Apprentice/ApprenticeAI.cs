

// https://learn.microsoft.com/en-us/semantic-kernel/get-started/quick-start-guide?pivots=programming-language-csharp
// https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Concepts
//https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/models?tabs=global-standard%2Cstandard-chat-completions#o1-preview-and-o1-mini-models-limited-access

//https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel?pivots=programming-language-csharp

// Import packages
using FoundryBlazor.Solutions;
using FoundryRulesAndUnits.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;


namespace Three2025.Apprentice;


public interface IApprenticeAI
{
    Task<ChatMessageContent> GetAIResponse(string userMessage);
}



public class ApprenticeAI : IApprenticeAI
{
    public string User { get; set; }
    public string Text { get; set; }

    public ChatHistory chatHistory = new();

    private Kernel kernel;
    private IChatCompletionService chatCompletionService;
    private OpenAIPromptExecutionSettings openAIPromptExecutionSettings;

    public ApprenticeAI(IWorkspace workspace, ILightingTech lighting)
    {
        //export OPENAI_KEY="your_openai_api_key"
        //export OPENAI_ENDPOINT="https://your-openai-endpoint.com"

        var modelId = "gpt-4o-mini"; //"GPT-3.5";

        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT",EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(endpoint))
            return;

        $"Endpoint {endpoint}".WriteSuccess();

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_KEY", EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(apiKey))
            return;

        $"Building Kernel for {modelId}...".WriteSuccess();
        "........................................".WriteSuccess();
        // Create a kernel with Azure OpenAI chat completion
        var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);


        // Add enterprise components
        //builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(LogLevel.Trace));

        kernel = builder.Build();
        chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        //var plug3D = new ThreeDPlugin(foundry);
        //kernel.Plugins.AddFromObject(plug3D,"3DBuilder");
        //var lights = new LightingShapes(workspace);
        kernel.Plugins.AddFromObject(lighting,"Lighting");
        //kernel.Plugins.AddFromType<LightsPlugin>("Lights");
        

        openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings() 
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),

        }; 
    }



    public async Task<ChatMessageContent> GetAIResponse(string userMessage)
    {
        if ( kernel == null ) 
            return new ChatMessageContent(AuthorRole.System, "Apprentice Not Found!");

        chatHistory.AddUserMessage(userMessage);

        var result = new ChatMessageContent(AuthorRole.Assistant, "I'm sorry, I don't understand that.");
        try
        {
            result = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel);

            chatHistory.AddMessage(result.Role, result.Content ?? string.Empty);
        }
        catch (System.Exception ex)
        {
            result = new ChatMessageContent(AuthorRole.Assistant, ex.Message);
            $"Error: {ex.Message}".WriteError();

        }
        return result;
    }

    public void InitHistory()
    {
        // Add system message
        chatHistory.Add(
            new() {
                Role = AuthorRole.System,
                Content = "You are a helpful assistant"
            }
        );

        // Add user message with an image
        chatHistory.Add(
            new() {
                Role = AuthorRole.User,
                Items = [
                    new TextContent { Text = "What available on this menu" },
                    new ImageContent { Uri = new Uri("https://example.com/menu.jpg") }
                ]
            }
        );

        // Add assistant message
        chatHistory.Add(
            new() {
                Role = AuthorRole.Assistant,
                Content = "We have pizza, pasta, and salad available to order. What would you like to order?"
            }
        );

        // Add additional message from a different user
        chatHistory.Add(
            new() {
                Role = AuthorRole.User,
                Content = "I'd like to have the first option, please."
            }
        );
    }
}