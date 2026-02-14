# Blazor Service Testing Architecture - Quick Reference Guide

## 🚀 TL;DR

This is a **copy-paste friendly** quick reference for implementing the Blazor service testing architecture. For full details, see `BLAZOR_SERVICE_TESTING_ARCHITECTURE_IMPLEMENTATION_PLAN.md`.

---

## 📁 File Structure Template

```
YourProject/
├── Helpers/
│   ├── BaseServiceDebugHelper.cs          # Base class (create once)
│   ├── AgentServiceDebugHelper.cs         # Per service
│   ├── DocumentServiceDebugHelper.cs      # Per service
│   └── [YourService]ServiceDebugHelper.cs # Per service
├── Components/
│   └── Controls/
│       ├── JsonResponseDisplay.razor      # JSON viewer (create once)
│       ├── JsonResponseDisplay.razor.cs
│       ├── AgentServiceButtonPanel.razor  # Per service
│       ├── AgentServiceButtonPanel.razor.cs
│       └── [YourService]ButtonPanel.razor # Per service
└── Models/
    └── ServiceDebugResultMessage.cs       # Message model (create once)
```

---

## 🎯 The 4-Line Pattern (Core Method)

Every service method follows this exact pattern:

```csharp
public async Task<ContextWrapper<[ReturnType]>> [MethodName]Async([parameters])
{
    var result = await _[serviceName]Service.[MethodName]Async([parameters]);
    var json = CodingExtensions.DehydrateWrapper<[ReturnType]>(result, true);
    PublishMessage("[ServiceName]Service", "[MethodName]", json);
    return result;
}
```

**Example**:
```csharp
public async Task<ContextWrapper<ReadyAgentDTO>> GetAllAgentsAsync()
{
    var result = await _agentService.GetAllAgentsAsync();
    var json = CodingExtensions.DehydrateWrapper<ReadyAgentDTO>(result, true);
    PublishMessage("AgentService", "GetAllAgentsAsync", json);
    return result;
}
```

---

## 📋 Copy-Paste Templates

### 1. BaseServiceDebugHelper.cs (Create Once)

```csharp
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using ReadyAICore.Services;
using YourProject.Models;
using System.Text.Json;

namespace YourProject.Helpers;

public class BaseServiceDebugHelper
{
    protected ServiceNotificationChannel? _notificationChannel;
    
    public BaseServiceDebugHelper(ServiceNotificationChannel? notificationChannel = null)
    {
        _notificationChannel = notificationChannel;
    }

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    protected void PublishMessage(string serviceName, string methodName, string json)
    {
        $"[Tier 2] Publishing: {serviceName}.{methodName}".WriteNote();
        
        _notificationChannel?.PublishAsync(new ServiceDebugResultMessage
        {
            ServiceName = serviceName,
            MethodName = methodName,
            Json = json,
            Timestamp = DateTime.Now,
        });
    }

    protected void PublishInfoMessage(string message, string context = "System")
    {
        PublishMessage("System", "Info", $"[{context}] {message}");
    }

    protected void PublishSuccessMessage(string message, string context = "System")
    {
        PublishMessage("System", "Success", $"[{context}] {message}");
    }

    protected void PublishWarningMessage(string message, string context = "System")
    {
        PublishMessage("System", "Warning", $"[{context}] {message}");
    }

    protected void PublishErrorMessage(string message, string context = "System", Exception? exception = null)
    {
        var errorText = $"[{context}] {message}";
        if (exception != null)
        {
            errorText += $" | Exception: {exception.Message}";
        }
        PublishMessage("System", "Error", errorText);
    }
}
```

---

### 2. Simple Service Helper (No Context Required)

```csharp
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using ReadyAICore.Models.DTOs;
using ReadyAICore.Services.Interfaces;
using ReadyAICore.Services;
using YourProject.Models;

namespace YourProject.Helpers;

public class AgentServiceHelper : BaseServiceDebugHelper
{
    private readonly IAgentService _agentService;
    
    public AgentServiceHelper(
        IAgentService agentService, 
        ServiceNotificationChannel? notificationChannel) 
        : base(notificationChannel)
    {
        _agentService = agentService;
    }

    public async Task<ContextWrapper<ReadyAgentDTO>> GetAllAgentsAsync()
    {
        var result = await _agentService.GetAllAgentsAsync();
        var json = CodingExtensions.DehydrateWrapper<ReadyAgentDTO>(result, true);
        PublishMessage("AgentService", "GetAllAgentsAsync", json);
        return result;
    }

    public async Task<ContextWrapper<ReadyAgentDTO>> GetAgentByIdAsync(string agentId)
    {
        var result = await _agentService.GetAgentByIdAsync(agentId);
        var json = CodingExtensions.DehydrateWrapper<ReadyAgentDTO>(result, true);
        PublishMessage("AgentService", "GetAgentByIdAsync", json);
        return result;
    }

    // Add more methods following the same 4-line pattern
}
```

---

### 3. Context-Aware Service Helper (Requires User/Scenario)

```csharp
using FoundryRulesAndUnits.Extensions;
using FoundryRulesAndUnits.Models;
using ReadyAICore.Models.DTOs;
using ReadyAICore.Services.Interfaces;
using ReadyAICore.Services;
using YourProject.Models;

namespace YourProject.Helpers;

public class DocumentServiceDebugHelper : BaseServiceDebugHelper
{
    private readonly IDocumentService _documentService;
    private readonly IUserContextService _userContextService;
    
    public DocumentServiceDebugHelper(
        IDocumentService documentService, 
        IUserContextService userContextService,
        ServiceNotificationChannel? notificationChannel) 
        : base(notificationChannel)
    {
        _documentService = documentService;
        _userContextService = userContextService;
    }

    public async Task<ContextWrapper<ReadyDocumentDTO>> GetAllDocumentsAsync()
    {
        if (_userContextService.CurrentUser == null)
        {
            PublishErrorMessage("No user context available", "DocumentService");
            return ContextWrapper<ReadyDocumentDTO>.Error("No user context available");
        }

        var result = await _documentService.GetAllDocumentsAsync(
            _userContextService.CurrentUser.Id);
        var json = CodingExtensions.DehydrateWrapper<ReadyDocumentDTO>(result, true);
        PublishMessage("DocumentService", "GetAllDocumentsAsync", json);
        return result;
    }

    // Add more methods following the same pattern
}
```

---

### 4. Simple Button Panel (No Context)

**AgentServiceButtonPanel.razor.cs**:
```csharp
using Microsoft.AspNetCore.Components;
using YourProject.Helpers;

namespace YourProject.Components;

public partial class AgentServiceButtonPanel : ComponentBase
{
    [Inject] private AgentServiceHelper AgentHelper { get; set; } = null!;

    private async Task GetAllAgents()
    {
        await AgentHelper.GetAllAgentsAsync();
    }

    private async Task GetAgentById()
    {
        // Could prompt for ID, or use first agent, etc.
        await AgentHelper.GetAgentByIdAsync("some-agent-id");
    }
}
```

**AgentServiceButtonPanel.razor**:
```razor
@using YourProject.Helpers
@using Radzen
@using Radzen.Blazor

@namespace YourProject.Components

<div class="service-debug-panel agent">
    <h4>
        <RadzenIcon Icon="smart_toy" />
        Agent Service
    </h4>
    <small>Test agent management operations</small>

    <div class="button-grid">
        <RadzenButton Text="Get All Agents" 
                    Icon="smart_toy" 
                    ButtonStyle="ButtonStyle.Secondary"
                    Size="ButtonSize.Small"
                    Click="@GetAllAgents" />

        <RadzenButton Text="Get Agent By ID" 
                    Icon="search" 
                    ButtonStyle="ButtonStyle.Info"
                    Size="ButtonSize.Small"
                    Click="@GetAgentById" />
    </div>

    <div class="service-info">
        <RadzenIcon Icon="info" /> 
        <em>Agent operations work globally without user context requirements.</em>
    </div>
</div>

<style>
    .service-debug-panel {
        background-color: #f8f9fa;
        border: 2px solid #28a745;
        padding: 20px;
        margin-bottom: 20px;
        border-radius: 8px;
    }

    .service-debug-panel h4 {
        color: #28a745;
        margin-bottom: 15px;
    }

    .button-grid {
        display: flex;
        flex-wrap: wrap;
        gap: 10px;
        margin-top: 15px;
    }

    .service-info {
        margin-top: 15px;
        padding: 10px;
        background-color: #e7f3ff;
        border-left: 4px solid #007bff;
        font-size: 0.9em;
    }
</style>
```

---

### 5. Context-Aware Button Panel

**DocumentServiceButtonPanel.razor.cs**:
```csharp
using Microsoft.AspNetCore.Components;
using ReadyAICore.Services.Interfaces;
using ReadyAICore.Services;
using YourProject.Helpers;
using YourProject.Models;

namespace YourProject.Components;

public partial class DocumentServiceButtonPanel : ComponentBase, IDisposable
{
    [Inject] private IUserContextService UserContextService { get; set; } = null!;
    [Inject] private DocumentServiceDebugHelper DocumentHelper { get; set; } = null!;
    [Inject] private NotificationListener NotificationListener { get; set; } = null!;

    private bool hasUser => UserContextService.CurrentUser != null;
    private bool hasScenario => UserContextService.CurrentScenario != null;

    protected override void OnInitialized()
    {
        NotificationListener.Subscribe(
            ServiceType.UserContextService,
            msg => HandleUserContextChange(msg));
    }

    private void HandleUserContextChange(IServiceMessage message)
    {
        InvokeAsync(() => StateHasChanged());
    }

    private async Task GetAllDocuments()
    {
        await DocumentHelper.GetAllDocumentsAsync();
    }

    public void Dispose()
    {
        // NotificationListener handles cleanup automatically
    }
}
```

**DocumentServiceButtonPanel.razor**:
```razor
@using ReadyAICore.Services.Interfaces
@using ReadyAICore.Services
@using YourProject.Helpers
@using Radzen
@using Radzen.Blazor

@namespace YourProject.Components

<div class="service-debug-panel document">
    <h4>
        <RadzenIcon Icon="description" />
        Document Service
    </h4>
    <small>Test document management operations</small>

    @if (!hasUser)
    {
        <div class="context-warning user-required">
            <div class="context-warning-title">
                <RadzenIcon Icon="info" /> Current User Required
            </div>
            <div>Please select a current user to test document operations.</div>
        </div>
    }

    <div class="button-grid">
        <RadzenButton Text="Get All Documents" 
                    Icon="description" 
                    ButtonStyle="ButtonStyle.Secondary"
                    Size="ButtonSize.Small"
                    Click="@GetAllDocuments"
                    Disabled="@(!hasUser)" />
    </div>

    <div class="service-info">
        <RadzenIcon Icon="info" /> 
        <em>Document operations require a selected user.</em>
    </div>
</div>

<style>
    .service-debug-panel {
        background-color: #f8f9fa;
        border: 2px solid #6f42c1;
        padding: 20px;
        margin-bottom: 20px;
        border-radius: 8px;
    }

    .context-warning {
        background-color: #fff3cd;
        border: 1px solid #ffeaa7;
        border-radius: 4px;
        padding: 15px;
        margin: 15px 0;
    }

    .context-warning-title {
        color: #856404;
        font-weight: bold;
        margin-bottom: 5px;
    }

    .button-grid {
        display: flex;
        flex-wrap: wrap;
        gap: 10px;
        margin-top: 15px;
    }
</style>
```

---

### 6. ServiceDebugResultMessage.cs

```csharp
using ReadyAICore.Services;

namespace YourProject.Models;

public class ServiceDebugResultMessage : IServiceMessage
{
    public string ServiceName { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public string Json { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    // IServiceMessage implementation
    string IServiceMessage.ServiceName => ServiceName;
    string IServiceMessage.Operation => MethodName;
    string? IServiceMessage.UserId => null;
    ServiceStatus IServiceMessage.Status => ServiceStatus.Success;
    DateTime IServiceMessage.Timestamp => Timestamp;
    string? IServiceMessage.Message => Json;
    string IServiceMessage.DataType => "DebugResult";
    
    public bool IsFromService(ServiceType serviceType) => 
        ServiceName == serviceType.ToString();
    
    public bool IsOperation(string operation) => 
        MethodName == operation;
}
```

---

### 7. JsonResponseDisplay.razor

```razor
@using YourProject.Models
@using Radzen
@using Radzen.Blazor

@namespace YourProject.Components

@rendermode InteractiveServer

<div class="debug-left-panel">
    <div class="debug-toolbar">
        <h5 style="color: Black; margin: 0; display: inline-block;">
            <RadzenIcon Icon="code" /> JSON Response
        </h5>
        
        <div style="float: right;">
            <RadzenButton Text="Clear" 
                        Icon="clear" 
                        ButtonStyle="ButtonStyle.Secondary" 
                        Size="ButtonSize.Small"
                        Click="@ClearOutput" />
            <RadzenButton Text="Copy" 
                        Icon="content_copy" 
                        ButtonStyle="ButtonStyle.Info" 
                        Size="ButtonSize.Small"
                        Click="@CopyToClipboard" 
                        style="margin-left: 0.5rem;" />
        </div>
        
        <div style="clear: both;"></div>
    </div>
    
    <pre class="json-output">@((MarkupString)jsonOutput)</pre>
</div>

<style>
    .debug-left-panel {
        background-color: #1a1a1a;
        color: #ffffff;
        height: calc(100vh - 120px);
        overflow-y: auto;
        padding: 1rem;
        font-family: 'Courier New', Consolas, Monaco, monospace;
        font-size: 12px;
        line-height: 1.4;
    }

    .json-output {
        white-space: pre-wrap;
        word-wrap: break-word;
        margin: 0;
        padding: 1rem;
        background-color: #1a1a1a;
        border: 1px solid #333;
        border-radius: 4px;
        min-height: 200px;
    }

    .debug-toolbar {
        margin-bottom: 1rem;
        padding: 0.5rem;
        background-color: #e9ecef;
        border-radius: 4px;
    }
</style>
```

**JsonResponseDisplay.razor.cs**:
```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YourProject.Models;
using BlazorComponentBus;

namespace YourProject.Components;

public partial class JsonResponseDisplay : ComponentBase, IDisposable
{
    [Inject] private ComponentBus ComponentBus { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private string jsonOutput = "";

    protected override void OnInitialized()
    {
        ComponentBus.SubscribeTo<ServiceDebugResultMessage>(HandleServiceDebugResult);
        AppendOutput("🚀 JsonResponseDisplay initialized...\n\n");
    }

    private void HandleServiceDebugResult(ServiceDebugResultMessage message)
    {
        if (message.ServiceName == "System")
        {
            var output = message.MethodName switch
            {
                "Info" => $"\n🔵 [{message.Timestamp:HH:mm:ss}] INFO: {message.Json}\n\n",
                "Success" => $"\n✅ [{message.Timestamp:HH:mm:ss}] SUCCESS: {message.Json}\n\n",
                "Warning" => $"\n⚠️ [{message.Timestamp:HH:mm:ss}] WARNING: {message.Json}\n\n",
                "Error" => $"\n❌ [{message.Timestamp:HH:mm:ss}] ERROR: {message.Json}\n\n",
                _ => ""
            };

            if (message.MethodName == "ClearScreen")
                ClearOutput();
            else
                AppendOutput(output);
        }
        else
        {
            ClearOutput();
            var outputText = 
                $"\n🚀 [{message.Timestamp:HH:mm:ss}] {message.ServiceName}.{message.MethodName}\n" +
                "════════════ JSON RESPONSE ════════════\n" +
                $"{message.Json}\n" +
                "══════════════════════════════════════\n\n";
            
            AppendOutput(outputText);
        }
    }

    private void AppendOutput(string text)
    {
        jsonOutput += text;
        InvokeAsync(() => StateHasChanged());
    }

    private void ClearOutput()
    {
        jsonOutput = "";
        InvokeAsync(() => StateHasChanged());
    }

    private async Task CopyToClipboard()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", jsonOutput);
            AppendOutput("📋 ✅ Copied to clipboard!\n\n");
        }
        catch (Exception ex)
        {
            AppendOutput($"❌ Failed to copy: {ex.Message}\n\n");
        }
    }

    public void Dispose()
    {
        ComponentBus?.UnSubscribeFrom<ServiceDebugResultMessage>(HandleServiceDebugResult);
    }
}
```

---

### 8. DI Registration (Program.cs)

```csharp
// Add to your Program.cs or Startup.cs

// Service Helpers
builder.Services.AddScoped<AgentServiceHelper>();
builder.Services.AddScoped<DocumentServiceDebugHelper>();
// Add more helpers as needed

// Note: ServiceNotificationChannel and NotificationListener 
// are typically registered by your core services package
// If not, register them as:
// builder.Services.AddSingleton<ServiceNotificationChannel>();
// builder.Services.AddScoped<NotificationListener>();
```

---

## 🎨 Visual Design Quick Reference

### Service Colors

```csharp
// Use these hex codes for border colors
Agent      = "#28a745"  // Green
Document   = "#6f42c1"  // Purple
Module     = "#9370db"  // Medium Purple
Scenario   = "#17a2b8"  // Teal
System     = "#ff8c00"  // Orange
User       = "#007bff"  // Blue
```

### Icon Mapping

```csharp
// Common service icons
Agent      -> "smart_toy"
Document   -> "description"
Module     -> "apps"
Scenario   -> "assignment"
System     -> "settings"
User       -> "people"

// Common action icons
Get/List   -> "list", "search"
Create     -> "add", "add_circle"
Update     -> "edit", "edit_note"
Delete     -> "delete", "delete_outline"
Info       -> "info", "info_outline"
Refresh    -> "refresh", "sync"
```

---

## ✅ Pre-Flight Checklist

Before starting implementation:

- [ ] Do you have `FoundryRulesAndUnits.Extensions`?
- [ ] Do you have `FoundryRulesAndUnits.Models.ContextWrapper<T>`?
- [ ] Do you have a `ServiceNotificationChannel` or similar pub/sub system?
- [ ] Do you have Radzen Blazor components installed?
- [ ] Do you understand dependency injection in your project?

---

## 🚫 Common Mistakes

### ❌ Don't Do This:
```csharp
// Manual error handling
try {
    var result = await _service.GetAll();
} catch (Exception ex) {
    // Handle error
}

// Manual JSON serialization
var json = JsonSerializer.Serialize(result);

// Direct ComponentBus calls
ComponentBus.Publish(new Message { ... });

// Complex button panel logic
if (isExecuting) return;
isExecuting = true;
```

### ✅ Do This Instead:
```csharp
// Trust ContextWrapper for errors
var result = await _service.GetAll();
var json = CodingExtensions.DehydrateWrapper<T>(result, true);
PublishMessage("ServiceName", "MethodName", json);
return result;

// Simple button panel delegation
private async Task GetAll()
{
    await ServiceHelper.GetAllAsync();
}
```

---

## 📊 Quality Metrics

Your implementation should meet these standards:

| Metric | Target |
|--------|--------|
| Helper method lines | 4-10 |
| Button panel code-behind lines | 15-30 |
| Manual try/catch blocks | 0 |
| Manual JSON serialization | 0 |
| Manual ComponentBus calls | 0 |
| Code duplication | None |

---

## 🎯 Implementation Order

1. ✅ Create `BaseServiceDebugHelper.cs`
2. ✅ Create `ServiceDebugResultMessage.cs`
3. ✅ Create `JsonResponseDisplay` component
4. ✅ Test JsonResponseDisplay with sample data
5. ✅ Create first simple service helper (e.g., Agent)
6. ✅ Create first button panel
7. ✅ Test end-to-end
8. ✅ Create context-aware service helper (e.g., Document)
9. ✅ Create context-aware button panel
10. ✅ Replicate for remaining services

---

## 🔍 Troubleshooting

### "Results don't appear in JSON display"
- Check if `ComponentBus.SubscribeTo<ServiceDebugResultMessage>` is called
- Verify `PublishMessage()` is being called in helper
- Check browser console for JavaScript errors
- Ensure `StateHasChanged()` is called after updates

### "Buttons always disabled"
- Check if `UserContextService.CurrentUser` is actually set
- Verify `hasUser` property is returning correct value
- Check if `Disabled="@(!hasUser)"` syntax is correct
- Ensure component is re-rendering when context changes

### "Helper methods throw errors"
- Verify service is properly injected via DI
- Check if `ContextWrapper<T>` is being used correctly
- Ensure service interface matches expected method signatures
- Check if required parameters are being passed

---

## 📚 Additional Resources

- **Full Implementation Plan**: `BLAZOR_SERVICE_TESTING_ARCHITECTURE_IMPLEMENTATION_PLAN.md`
- **Communication Architecture**: `TWO_TIER_COMMUNICATION_IMPLEMENTATION_COMPLETE.md`
- **Button Panel Guide**: `BUTTON_PANEL_DEVELOPMENT_GUIDE.md`
- **Service Helper Pattern**: `SERVICE_DEBUG_HELPER_PATTERN_GUIDE.md`

---

## 💡 Pro Tips

1. **Start Simple**: Begin with a service that has no context dependencies
2. **Test Frequently**: After each component, test end-to-end before moving on
3. **Copy-Paste First**: Use these templates exactly, then customize
4. **Follow the Pattern**: Don't deviate until you understand why the pattern exists
5. **Ask Questions**: If something doesn't make sense, review the full implementation plan

---

**Quick Reference Version**: 1.0  
**Last Updated**: December 29, 2025  
**Companion Document**: BLAZOR_SERVICE_TESTING_ARCHITECTURE_IMPLEMENTATION_PLAN.md
