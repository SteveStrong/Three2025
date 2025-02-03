using FoundryRulesAndUnits.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Three2025.Apprentice;

namespace Three2025.Components.Pages;

    public class ChatMessage
    {
        public string User { get; set; }
        public string Text { get; set; }
    }

    public class AIResponse
    {
        public string Text { get; set; }
    }


public partial class AIChatBase: ComponentBase, IDisposable
{

    [Inject] private IApprenticeAI AI { get; set; }

    protected string newMessage;
    protected List<ChatMessage> messages = new List<ChatMessage>();
    protected List<string> messageBuffer = new List<string>();
    protected int bufferIndex = -1;

    protected override void OnInitialized()
    {
        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT",EnvironmentVariableTarget.User);
        if ( string.IsNullOrEmpty(endpoint))
             messages.Add(new ChatMessage { User = "Chat", Text = "OPENAI_ENDPOINT is not an EnvironmentVariable" });
    }

    protected async Task SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(newMessage))
        {
            messages.Add(new ChatMessage { User = "You", Text = newMessage });
            var response = await GetAIResponse(newMessage);
            messages.Add(new ChatMessage { User = "AI", Text = response });
            newMessage = string.Empty;
        }
    }
    protected async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            if (!string.IsNullOrWhiteSpace(newMessage))
            {
                messageBuffer.Add(newMessage);
                bufferIndex = messageBuffer.Count;
            }
            await SendMessage();
        }
        else if (e.Key == "ArrowUp")
        {
            if (bufferIndex > 0)
            {
                bufferIndex--;
                newMessage = messageBuffer[bufferIndex];
            }
        }
        else if (e.Key == "ArrowDown")
        {
            if (bufferIndex < messageBuffer.Count - 1)
            {
                bufferIndex++;
                newMessage = messageBuffer[bufferIndex];
            }
            else
            {
                bufferIndex = messageBuffer.Count;
                newMessage = string.Empty;
            }
        }
        else
        {
            $"{e.Key}".WriteInfo();
        }
    }
    protected async Task<string> GetAIResponse(string userMessage)
    {
        var result = await AI.GetAIResponse(userMessage);
        return result.Content ?? "I'm sorry, I don't understand that.";
    }

    protected async Task ExecuteBufferCommand(int index)
    {
        if (index >= 0 && index < messageBuffer.Count)
        {
            newMessage = messageBuffer[index];
            await SendMessage();
        }
    }

    public void Dispose()
    {
        
    }
}

