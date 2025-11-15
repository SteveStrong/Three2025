using FoundryWorldsAndDrawings.Solutions;
using Microsoft.AspNetCore.Components;

namespace Three2025.Components.Pages;

public partial class AnimationControl : ComponentBase
{
    [Inject]
    public IFoundryService FoundryService { get; set; } = default!;

    private bool isRunning = true; // Animation auto-starts on page load
    private string message = "";
    private string alertClass = "alert-info";

    protected override void OnInitialized()
    {
        message = "Animation is running automatically. Use Stop to pause it.";
        alertClass = "alert-info";
    }

    private async Task StartAnimation()
    {
        try
        {
            await FoundryService.StartGlobalAnimation();
            isRunning = true;
            message = "Animation started successfully! Check console for TriggerAnimationFrame() logs.";
            alertClass = "alert-success";
        }
        catch (Exception ex)
        {
            message = $"Error starting animation: {ex.Message}";
            alertClass = "alert-danger";
        }
    }

    private async Task StopAnimation()
    {
        try
        {
            await FoundryService.StopGlobalAnimation();
            isRunning = false;
            message = "Animation stopped successfully.";
            alertClass = "alert-warning";
        }
        catch (Exception ex)
        {
            message = $"Error stopping animation: {ex.Message}";
            alertClass = "alert-danger";
        }
    }
}
