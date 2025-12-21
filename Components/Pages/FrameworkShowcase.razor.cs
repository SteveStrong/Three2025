using Microsoft.AspNetCore.Components;

namespace Three2025.Components.Pages
{
    public partial class FrameworkShowcase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; } = default!;

        private int selectedTabIndex = 0;

        private void Nav(string url) => NavigationManager.NavigateTo(url);
    }
}
