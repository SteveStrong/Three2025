#nullable enable
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using FoundryMentorModeler.Model;
using FoundryRulesAndUnits;
using FoundryRulesAndUnits.Models;
using Three2025.Apprentice;

namespace Three2025.Components.Pages;

public partial class ConversationalModeler : ComponentBase
{
    [Inject] private NavigationManager? Nav { get; set; }
    [Inject] private IModelTech? ModelTech { get; set; }
    [Inject] private IMentorServices? MentorServices { get; set; }

    // Panel sizing
    private int ChatPanelWidth { get; set; } = 50;
    private int ModelPanelWidth => 100 - ChatPanelWidth;
    
    // Chat state
    private List<ChatMessage> ChatMessages { get; set; } = new();
    private string UserInput { get; set; } = "";
    
    // Model state
    private PartComponent? CurrentModel { get; set; }
    private PartComponent? SelectedComponent { get; set; }
    private List<ApiCall> ApiCallLog { get; set; } = new();
    
    // Test prompts
    private static readonly Dictionary<string, string> TestPrompts = new()
    {
        ["beam-simple"] = @"I have a simply-supported steel beam, 10 feet long, with a 500 lb point load at the center. The I-beam has a moment of inertia of 10.9 in^4. What's the deflection at the center?",
        ["beam-cantilever"] = @"Calculate the maximum deflection of a cantilever beam. Length is 6 feet, uniformly distributed load of 100 lb/ft, aluminum (E = 10 Mpsi), rectangular cross-section 2in x 4in.",
        ["heat-wall"] = @"A concrete wall is 8 inches thick. Inside temperature is 70°F, outside is 20°F. Concrete has thermal conductivity of 0.8 BTU/(hr·ft·°F). What's the heat flux through the wall?"
    };

    protected override void OnInitialized()
    {
        // Welcome message
        ChatMessages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = "Hello! I'm the Conversational Modeler. Describe an engineering problem and I'll build a knowledge model to solve it. You can also select a test prompt from the dropdown above.",
            Timestamp = DateTime.Now
        });
    }

    private void OnTestPromptSelected(ChangeEventArgs e)
    {
        var key = e.Value?.ToString();
        if (!string.IsNullOrEmpty(key) && TestPrompts.TryGetValue(key, out var prompt))
        {
            UserInput = prompt;
            StateHasChanged();
        }
    }

    private async Task OnInputKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey && !string.IsNullOrWhiteSpace(UserInput))
        {
            await SendMessage();
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserInput)) return;

        var message = UserInput.Trim();
        UserInput = "";

        // Add user message
        ChatMessages.Add(new ChatMessage
        {
            Role = "user",
            Content = message,
            Timestamp = DateTime.Now
        });

        StateHasChanged();

        // Process message (for now, echo back with model construction intent)
        await ProcessUserMessage(message);
    }

    private async Task ProcessUserMessage(string message)
    {
        // For now, demonstrate the API with a simple response
        // In Phase 3, this will call the actual LLM with function calling
        
        await Task.Delay(500); // Simulate processing

        var response = "I understand you want to solve an engineering problem. ";
        
        // Simple pattern matching for demo
        if (message.Contains("beam", StringComparison.OrdinalIgnoreCase))
        {
            response += "I'll create a beam model with the parameters you provided.\n\n";
            response += "API calls that will be made:\n";
            response += "• CreateComponent(\"BeamConcept\", \"beam1\")\n";
            response += "• AddCalculation(\"beam1\", \"Length|ft: 10\")\n";
            response += "• AddCalculation(\"beam1\", \"Load|lb: 500\")\n";
            response += "• AddCalculation(\"beam1\", \"E|psi: 29e6\")\n";
            response += "• AddCalculation(\"beam1\", \"I|in4: 10.9\")\n";
            response += "• AddCalculation(\"beam1\", \"deflection|in: (Load@ * Length@^3) / (48 * E@ * I@)\")\n";
            response += "• GetParameter(\"beam1\", \"deflection\")\n\n";
            response += "📝 Note: Full model construction API integration coming in Phase 3!";
            
            // Demonstrate creating a simple model
            CreateDemoBeamModel();
        }
        else
        {
            response += "Right now I'm in demo mode. Try asking about a beam deflection problem to see the model construction workflow!";
        }

        ChatMessages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = response,
            Timestamp = DateTime.Now
        });

        StateHasChanged();
    }

    private void CreateDemoBeamModel()
    {
        if (ModelTech == null || MentorServices == null)
        {
            ChatMessages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = "❌ Error: ModelTech service not available",
                Timestamp = DateTime.Now
            });
            return;
        }

        try
        {
            // Use ModelTech API to create model dynamically
            var modelInfo = ModelTech.EstablishModel("BeamModel", "AnimatedKnModel");
            LogApiCall("EstablishModel", ["BeamModel", "AnimatedKnModel"], modelInfo.Name);
            
            // Add a PartComponent for the beam
            var component = ModelTech.AddComponent("beam1", "BeamModel");
            LogApiCall("AddComponent", ["beam1", "BeamModel"], component.Name ?? "");
            
            // Set parameters using ModelTech
            ModelTech.SetParameter("Length", "units(10, 'ft')", "beam1");
            LogApiCall("SetParameter", ["Length", "units(10, 'ft')", "beam1"], null);
            
            ModelTech.SetParameter("Load", "units(500, 'lb')", "beam1");
            LogApiCall("SetParameter", ["Load", "units(500, 'lb')", "beam1"], null);
            
            ModelTech.SetParameter("E", "units(29e6, 'psi')", "beam1");
            LogApiCall("SetParameter", ["E", "units(29e6, 'psi')", "beam1"], null);
            
            ModelTech.SetParameter("I", "units(10.9, 'in4')", "beam1");
            LogApiCall("SetParameter", ["I", "units(10.9, 'in4')", "beam1"], null);
            
            // Set formula for deflection
            ModelTech.SetParameter("deflection", "(Load@ * Length@^3) / (48 * E@ * I@)", "beam1");
            LogApiCall("SetParameter", ["deflection", "(Load@ * Length@^3) / (48 * E@ * I@)", "beam1"], null);
            
            // Get the calculated result
            var deflectionParam = ModelTech.GetParameter("deflection", "beam1");
            var deflectionValue = deflectionParam?.GetValue()?.ToString() ?? "N/A";
            LogApiCall("GetParameter", ["deflection", "beam1"], deflectionValue);
            
            // Retrieve the actual model to display
            var model = MentorServices.FindModel<KnModel>("BeamModel");
            if (model != null)
            {
                CurrentModel = model.Members<PartComponent>().FirstOrDefault();
                SelectedComponent = CurrentModel;
            }
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            ChatMessages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = $"❌ Error creating model: {ex.Message}",
                Timestamp = DateTime.Now
            });
            StateHasChanged();
        }
    }

    private void StartVoiceInput()
    {
        // Placeholder for voice input - will use JSInterop with Web Speech API
        ChatMessages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = "🎤 Voice input coming in Phase 5! For now, please type your message.",
            Timestamp = DateTime.Now
        });
        StateHasChanged();
    }

    private void HandleTreeSelection(FoundryRulesAndUnits.Models.ITreeNode node)
    {
        if (node is PartComponent component)
        {
            SelectedComponent = component;
        }
        StateHasChanged();
    }

    private void ClearModel()
    {
        CurrentModel = null;
        SelectedComponent = null;
        ApiCallLog.Clear();
        
        ChatMessages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = "Model cleared. Ready to build a new model!",
            Timestamp = DateTime.Now
        });
        
        StateHasChanged();
    }

    private void ClearLog()
    {
        ApiCallLog.Clear();
        StateHasChanged();
    }

    // Splitter functionality
    private void StartResize(MouseEventArgs e)
    {
        // TODO: Add mouse move/up event handlers via JSInterop for smooth dragging
    }

    private void ResetSplit()
    {
        ChatPanelWidth = 50;
        StateHasChanged();
    }

    // Helper methods
    private int GetParameterCount(PartComponent component)
    {
        return component.Members<KnParameter>().Count();
    }

    private List<PartComponent> GetSubComponents(PartComponent component)
    {
        return component.ModelComponents<PartComponent>().ToList();
    }

    private List<DisplayParameterInfo> GetParameters(PartComponent component)
    {
        var result = new List<DisplayParameterInfo>();
        
        foreach (var param in component.Members<KnParameter>())
        {
            var opResult = param.GetValue();
            var displayValue = "N/A";
            
            if (opResult.IsNumberWithUnits())
            {
                var mv = opResult.AsMeasuredValue();
                displayValue = $"{mv.Value} {mv.Units}";
            }
            else if (opResult.IsNumber())
            {
                displayValue = opResult.AsNumber().ToString("F3");
            }
            else if (opResult.IsString())
            {
                displayValue = opResult.AsString();
            }
            
            var formula = param.Expression;
            
            result.Add(new DisplayParameterInfo
            {
                Name = param.Name ?? "",
                DisplayValue = displayValue,
                Formula = formula ?? "",
                IsCalculated = !string.IsNullOrEmpty(formula)
            });
        }
        
        return result;
    }

    private void LogApiCall(string method, object[] args, object? result)
    {
        ApiCallLog.Add(new ApiCall
        {
            Timestamp = DateTime.Now,
            Method = method,
            Arguments = args,
            Result = result
        });
    }
    
    // Simple concrete beam class for demo
    private class BeamConcept : PartComponent
    {
        public BeamConcept(string name) : base(name)
        {
            Calculations([
                "Length|ft: 10",
                "Load|lb: 500",
                "E|psi: 29e6",
                "I|in4: 10.9",
                "L_inches|in: Length@",
                "deflection|in: (Load@ * L_inches@^3) / (48 * E@ * I@)"
            ]);
        }
    }
}

// Supporting classes
public class ChatMessage
{
    public required string Role { get; set; } // "user" or "assistant"
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; }
}

public class DisplayParameterInfo
{
    public required string Name { get; set; }
    public required string DisplayValue { get; set; }
    public required string Formula { get; set; }
    public bool IsCalculated { get; set; }
}

public class ApiCall
{
    public DateTime Timestamp { get; set; }
    public required string Method { get; set; }
    public object[] Arguments { get; set; } = Array.Empty<object>();
    public object? Result { get; set; }
    
    public string FormatLog()
    {
        var args = string.Join(", ", Arguments.Select(a =>
            a is string s ? $"\"{s}\"" : a?.ToString() ?? "null"));
        var result = Result != null ? $" → {Result}" : "";
        return $"{Timestamp:HH:mm:ss} {Method}({args}){result}";
    }
}
