using System.Text;
using Microsoft.AspNetCore.Components;
using FoundryBlazor;

namespace Three2025.Components.Layout;

public class NavMenuBase : ComponentBase
{
    protected string VersionDisplay()
    {
        var stats = new FoundryBlazor.CodeStatus();
        return stats.Version();
        // var version = GetType().Assembly.GetName().Version.ToString();
        // var text = new StringBuilder("Version: ").Append(version);
        // return text.ToString();
    }
}