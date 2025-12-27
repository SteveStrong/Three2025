using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;
using Three2025.Services.Chat;

namespace Three2025.Components.Shared.Chat;

/// <summary>
/// Component for selecting and running test sequences
/// </summary>
public partial class TestSequenceSelector : ComponentBase
{
    /// <summary>
    /// Event fired when a test sequence is selected and run button clicked
    /// </summary>
    [Parameter] 
    public EventCallback<TestSequenceMetadata> OnSequenceSelected { get; set; }
    
    /// <summary>
    /// Whether the selector is disabled (e.g., during queue processing)
    /// </summary>
    [Parameter] 
    public bool IsDisabled { get; set; }
    
    /// <summary>
    /// Whether to show the description below the selector
    /// </summary>
    [Parameter] 
    public bool ShowDescription { get; set; } = true;

    private string selectedSequenceKey = string.Empty;
    private Dictionary<string, List<TestSequenceMetadata>> sequencesByCategory = new();

    protected override void OnInitialized()
    {
        // Load all test sequences grouped by category
        sequencesByCategory = ChatTestScenarios.GetSequencesByCategory();
    }

    private async Task OnRunClicked()
    {
        if (string.IsNullOrEmpty(selectedSequenceKey))
            return;

        var sequence = ChatTestScenarios.GetSequence(selectedSequenceKey);
        if (sequence != null)
        {
            await OnSequenceSelected.InvokeAsync(sequence);
            selectedSequenceKey = string.Empty; // Reset after running
        }
    }

    private string GetSelectedDescription()
    {
        if (string.IsNullOrEmpty(selectedSequenceKey))
            return string.Empty;

        var sequence = ChatTestScenarios.GetSequence(selectedSequenceKey);
        return sequence?.Description ?? string.Empty;
    }
}
