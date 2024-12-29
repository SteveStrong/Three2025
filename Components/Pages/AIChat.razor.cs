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
            await SendMessage();
        }
    }
    protected async Task<string> GetAIResponse(string userMessage)
    {


        var result = await AI.GetAIResponse(userMessage);
        return result.Content ?? "I'm sorry, I don't understand that.";
    }

    public void Dispose()
    {
        
    }
}

