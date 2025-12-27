#nullable enable

using Microsoft.AspNetCore.Components;
using Three2025.Models.Chat;

namespace Three2025.Components.Shared.Chat;

public partial class ActivityLogEntry
{
    /// <summary>
    /// The log entry to display
    /// </summary>
    [Parameter, EditorRequired]
    public Models.Chat.ActivityLogEntry Entry { get; set; } = default!;

    private string GetTypeCssClass()
    {
        return Entry.Type.ToLower().Replace(" ", "-");
    }

    private string GetTextColor() => Entry.Type switch
    {
        ActivityLogType.UserInput => "#4ec9b0",
        ActivityLogType.AgentSwitch => "#dcdcaa",
        ActivityLogType.ToolExecution => "#ce9178",
        ActivityLogType.Response => "#9cdcfe",
        ActivityLogType.Routing => "#c586c0",
        ActivityLogType.Error => "#f48771",
        ActivityLogType.System => "#608b4e",
        _ => "#d4d4d4"
    };

    private string GetBorderColor() => Entry.Type switch
    {
        ActivityLogType.UserInput => "#4ec9b0",
        ActivityLogType.AgentSwitch => "#dcdcaa",
        ActivityLogType.ToolExecution => "#ce9178",
        ActivityLogType.Response => "#9cdcfe",
        ActivityLogType.Routing => "#c586c0",
        ActivityLogType.Error => "#f48771",
        ActivityLogType.System => "#608b4e",
        _ => "#3e3e3e"
    };

    private string GetBackgroundColor() => Entry.Type switch
    {
        ActivityLogType.Error => "#3d2422",
        ActivityLogType.ToolExecution => "#2d2a26",
        ActivityLogType.AgentSwitch => "#2d2d2a",
        _ => "#262626"
    };

    private string GetIcon() => Entry.Type switch
    {
        ActivityLogType.UserInput => "💬",
        ActivityLogType.AgentSwitch => "🔀",
        ActivityLogType.ToolExecution => "🔧",
        ActivityLogType.Response => "💡",
        ActivityLogType.Routing => "🧭",
        ActivityLogType.Error => "❌",
        ActivityLogType.System => "⚙️",
        ActivityLogType.ToolDiscovery => "🔍",
        ActivityLogType.GeometryTools => "📐",
        ActivityLogType.ClockTools => "⏰",
        ActivityLogType.OtherTools => "🛠️",
        _ => "📋"
    };
}
