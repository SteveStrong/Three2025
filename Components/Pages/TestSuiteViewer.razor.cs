#nullable enable

using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;
using Three2025.Services.Chat;

namespace Three2025.Components.Pages;

public partial class TestSuiteViewer
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private Dictionary<string, List<TestSequenceMetadata>> testsByCategory = new();
    private int selectedTabIndex = 0;

    protected override void OnInitialized()
    {
        // Get all test sequences
        var allTests = ChatTestScenarios.GetAllSequences();

        // Group by category
        testsByCategory = allTests.Values
            .GroupBy(t => t.Category)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(t => t.DisplayName).ToList());
    }

    private void RunTest(TestSequenceMetadata test)
    {
        // Navigate to Agent Canvas with test name as query parameter
        // Use forceLoad to ensure page reloads if already on agent-canvas
        Navigation.NavigateTo($"/agent-canvas?test={Uri.EscapeDataString(test.Name)}", forceLoad: true);
    }
}
