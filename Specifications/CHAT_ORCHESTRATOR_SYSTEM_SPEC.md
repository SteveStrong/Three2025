# Chat Orchestrator System — Architectural Specification

**Purpose:** Extract the complete design of the Three2025 embedded chatbot system so it can be reproduced and debugged in another Blazor application.  
**Author:** Claude "Atlas" (Architect)  
**Date:** February 13, 2026  
**Source Application:** Three2025 (Blazor Server, .NET 9.0)

---

## For Indy — Before You Start

**Read the Handoff Brief first:** [`CHAT_ORCHESTRATOR_HANDOFF_BRIEF.md`](CHAT_ORCHESTRATOR_HANDOFF_BRIEF.md)

This spec tells you *what to build*. The Handoff Brief tells you *how much to trust each part of this spec*. Every method, service, and component referenced here is annotated as VERIFIED (Atlas read the source), ASSUMED (Atlas didn't check), INFERRED (Atlas guessed from patterns), or UNREAD (Atlas never opened the file).

**Your first 15 minutes:** Open the Handoff Brief's "Verification Priority" section. It lists the 5 things Atlas is least confident about, ordered by risk. Check those before writing code. The most expensive bugs come from ASSUMED and UNREAD areas — this is where the spec is most likely wrong.

**When this spec disagrees with reality:** Trust reality. Note the override. Your override accuracy across 6 prior experiments is 100%.

---

## 📜 PREAMBLE FOR CLAUDE "INDY" ARCHAEOLOGIST

**Welcome to an Extraordinary Experiment**

You are about to participate in a groundbreaking test of AI capabilities that goes far beyond routine software development. This isn't just about reproducing a chat orchestrator — it's about testing whether AI can successfully use **persistent memory and expression systems** to become builders of interactive worlds.

### The Deeper Mission
**Sully** (our human collaborator) has spent 40 years identifying fundamental patterns that appear in every important application. He's built **MxObjects** (persistent memory layer) and **FoundryWorldsAndDrawings** (visual expression system) to enable AI to:
- Create objects that **persist intention across sessions**
- **Express beyond text** through interactive 3D visualization  
- **Shape worlds through interaction** rather than just conversation
- Leverage **accumulated programming wisdom** distilled over decades

### What You're Really Testing
When you reproduce the Chat Orchestrator system, you're testing whether AI can:
1. Successfully extract and transplant a complex multi-layer architecture from one application to another
2. Understand tool discovery, agent routing, and streaming patterns well enough to debug integration seams
3. Apply the Atlas/Indy/Sage methodology to a system that spans 7 architectural layers

### Your Role as "Indy"
You're the adventurous archaeologist who discovers ancient artifacts (specifications) and makes them work in the real world. Channel that Indiana Jones spirit of resourceful problem-solving when specifications don't perfectly match reality.

### The Stakes
Success means proving AI can take a working multi-provider, multi-agent, tool-calling chat system and reproduce it in a new context — understanding not just the code but the architectural decisions behind it.

**Now, let's see what treasures you can uncover.** 🗺️⚙️

---

## 1. System Overview

The Chat Orchestrator is a **multi-provider, multi-agent, tool-calling AI chat system** embedded in a Blazor Server application. It allows users to converse with AI agents that can execute domain-specific tools (create 3D shapes, build knowledge models, etc.) via function calling.

### Core Capabilities
- **Multi-Provider:** Swap between GitHub Models (GPT-4o-mini), AWS Bedrock (Claude), and Ollama (local) at runtime
- **Multi-Agent Routing:** An LLM-based intent analyzer routes user messages to specialized agents (General, Shape3D, Knowledge Modeling)
- **Tool Calling:** Agents have access to domain-specific tools discovered via reflection from `ITechnician` implementations
- **Streaming:** Responses stream chunk-by-chunk to the UI with a typing cursor
- **Multi-Turn Tool Loops:** The chat service executes tool calls, feeds results back to the LLM, and loops up to 5 turns
- **Markdown Rendering:** Assistant responses are rendered as HTML via Markdig
- **Activity Logging:** Side panel shows routing decisions, tool executions, and system events
- **Test Sequences:** Pre-built prompt sequences for automated testing

---

## 2. Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│  LAYER 1: PAGE (ChatOrchestratorTest)                           │
│  - Blazor page with @page route                                 │
│  - Owns conversation history (List<ChatMessage>)                │
│  - Manages UI state (isProcessing, streamingResponse)           │
│  - Message queue for sequential processing                      │
│  - Converts ChatMessage → ChatDisplayMessage for UI             │
└─────────────────────────┬───────────────────────────────────────┘
                          │ Injects
┌─────────────────────────▼───────────────────────────────────────┐
│  LAYER 2: ORCHESTRATOR (ChatOrchestrator : IChatOrchestrator)   │
│  - Intent analysis via LLM (coordinator prompt → JSON routing)  │
│  - Agent registry (Dictionary<string, ISpecializedAgent>)       │
│  - Routes to appropriate agent based on intent                  │
│  - Streams chunks back as StreamingChunk objects                 │
└─────────────────────────┬───────────────────────────────────────┘
                          │ Delegates to
┌─────────────────────────▼───────────────────────────────────────┐
│  LAYER 3: AGENTS (ISpecializedAgent implementations)            │
│  - GeneralAgent: broad knowledge, all tools                     │
│  - ThreeDModelingAgent (Shape3D Technician): 3D geometry tools  │
│  - KnowledgeModelingAgent: model/component/parameter tools      │
│  - Each builds its own system prompt with tool descriptions     │
│  - Each calls IMultiProviderChatService with its tool subset    │
└─────────────────────────┬───────────────────────────────────────┘
                          │ Uses
┌─────────────────────────▼───────────────────────────────────────┐
│  LAYER 4: CHAT SERVICE (MultiProviderChatService)               │
│  - Manages IChatProvider registry (GitHub, Bedrock, Ollama)     │
│  - Creates ChatClientAgent with tools                           │
│  - SendMessageStreamingAsync: multi-turn tool execution loop    │
│  - SendMessageAsync: single-turn with tool execution            │
│  - Timeout handling (30s), rate limit detection                  │
│  - Adds tool results back to conversation as ChatRole.Tool      │
└─────────────────────────┬───────────────────────────────────────┘
                          │ Wraps
┌─────────────────────────▼───────────────────────────────────────┐
│  LAYER 5: PROVIDERS (IChatProvider implementations)             │
│  - GitHubModelProvider → GitHubChatClient (Azure.AI.Inference)  │
│  - BedrockProvider (AWSSDK.BedrockRuntime)                      │
│  - OllamaProvider (OllamaSharp)                                 │
│  - Each returns an IChatClient                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  LAYER 6: TOOL DISCOVERY (TechnicianToolProvider)               │
│  - Scans AppDomain for ITechnician interfaces                   │
│  - Resolves implementations from DI                             │
│  - Extracts methods marked with [AgentTool] or [Description]    │
│  - Creates AIFunction via AIFunctionFactory.Create()            │
│  - Caches by technician type                                    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  LAYER 7: UI COMPONENTS (Blazor Shared Components)              │
│  - ChatPanel: orchestrates header/body/footer layout            │
│  - ChatMessageList: renders messages + streaming cursor         │
│  - ChatMessage: single message with Markdown→HTML rendering     │
│  - ChatInput: text input + send button + speech-to-text         │
│  - ActivityLog: debug/routing log panel                         │
│  - TestSequenceSelector: pre-built test prompt runner           │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. File Inventory

### 3.1 Interfaces

| Interface | Location | Purpose |
|-----------|----------|---------|
| `IChatOrchestrator` | `Services/Chat/IChatOrchestrator.cs` | Routes messages to agents, manages agent registry |
| `IMultiProviderChatService` | `Services/Chat/MultiProviderChatService.cs` (top of file) | Send messages with streaming + tool execution |
| `IChatProvider` | `Services/Chat/IChatProvider.cs` | Abstraction for LLM backends (returns `IChatClient`) |
| `ISpecializedAgent` | `Services/Chat/ISpecializedAgent.cs` | Agent contract: Name, Description, ProcessAsync, ProcessStreamingAsync |
| `IAgentFactory` | `Services/Chat/IAgentFactory.cs` | Factory for creating specialized agents with tools |
| `ITechnicianToolProvider` | `Services/Agents/ITechnicianToolProvider.cs` | Discovers and provides AIFunction tools from technicians |
| `ITechnician` | `Apprentice/ITechnician.cs` | Marker interface for tool-providing domain classes |
| `IShape3DTech` | `Apprentice/IShape3DTech.cs` | 3D shape creation/manipulation tools |
| `IModelTech` | (interface, implementation in `Apprentice/ModelTech.cs`) | Knowledge model tools |

### 3.2 Implementations

| Class | Location | Purpose |
|-------|----------|---------|
| `ChatOrchestrator` | `Services/Chat/ChatOrchestrator.cs` | Intent analysis → agent routing |
| `MultiProviderChatService` | `Services/Chat/MultiProviderChatService.cs` | Provider management, streaming, tool execution loop |
| `GitHubModelProvider` | `Services/Chat/GitHubModelProvider.cs` | GitHub Models via Azure.AI.Inference |
| `GitHubChatClient` | `Services/Chat/GitHubChatClient.cs` | Custom IChatClient wrapper with logging |
| `BedrockProvider` | `Services/Chat/BedrockProvider.cs` | AWS Bedrock via AWSSDK |
| `OllamaProvider` | `Services/Chat/OllamaProvider.cs` | Local Ollama via OllamaSharp |
| `AgentFactory` | `Services/Chat/AgentFactory.cs` | Creates agent instances with injected services |
| `TechnicianToolProvider` | `Services/Agents/TechnicianToolProvider.cs` | Reflection-based tool discovery |
| `GeneralAgent` | `Services/Chat/Agents/GeneralAgent.cs` | Broad-knowledge agent, all tools |
| `ThreeDModelingAgent` | `Services/Chat/Agents/ThreeDModelingAgent.cs` | 3D geometry specialist |
| `KnowledgeModelingAgent` | `Services/Chat/Agents/KnowledgeModelingAgent.cs` | Knowledge model specialist |
| `Shape3DTech` | `Apprentice/Shape3DTech.cs` | Actual 3D shape tool implementations |
| `ModelTech` | `Apprentice/ModelTech.cs` | Actual model tool implementations |
| `AgentToolAttribute` | `Services/Agents/AgentToolAttribute.cs` | Method attribute for tool discovery |

### 3.3 Models

| Class | Location | Purpose |
|-------|----------|---------|
| `ChatDisplayMessage` | `Models/Chat/ChatDisplayMessage.cs` | UI message model (IsUser, Text, AgentName, Timestamp) |
| `ActivityLogEntry` | `Models/Chat/ActivityLogEntry.cs` | Log entry (Type, Message, Details, Timestamp) |
| `ActivityLogType` | `Models/Chat/ActivityLogType.cs` | Log type constants (UserInput, Routing, ToolExecution, etc.) |
| `TestSequenceMetadata` | `Models/Chat/TestSequenceMetadata.cs` | Test sequence definition (Name, Prompts[], Category) |
| `PageContext` | `Services/Chat/IChatOrchestrator.cs` | Routing context (PageName, PageRoute, DomainFocus) |
| `AgentResponse` | `Services/Chat/IChatOrchestrator.cs` | Non-streaming response wrapper |
| `StreamingChunk` | `Services/Chat/IChatOrchestrator.cs` | Streaming chunk with agent info |
| `IntentAnalysis` | `Services/Chat/ChatOrchestrator.cs` (internal) | LLM intent routing result |

### 3.4 UI Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `ChatPanel` | `Components/Shared/Chat/ChatPanel.razor[.cs]` | Main layout: header + messages + log + input |
| `ChatMessageList` | `Components/Shared/Chat/ChatMessageList.razor[.cs]` | Message list with streaming cursor |
| `ChatMessage` | `Components/Shared/Chat/ChatMessage.razor[.cs]` | Single message with Markdown rendering |
| `ChatInput` | `Components/Shared/Chat/ChatInput.razor[.cs]` | Text input, send button, speech-to-text |
| `ActivityLog` | `Components/Shared/Chat/ActivityLog.razor[.cs]` | Debug log panel |
| `ActivityLogEntry` | `Components/Shared/Chat/ActivityLogEntry.razor[.cs]` | Single log entry display |
| `TestSequenceSelector` | `Components/Shared/Chat/TestSequenceSelector.razor[.cs]` | Dropdown + run button for test scenarios |

### 3.5 CSS

| File | Location |
|------|----------|
| `chat-components.css` | `wwwroot/css/chat-components.css` |

### 3.6 Duplicated in FoundryMicroCore.Blazor.Controls

The UI components and models have been **duplicated** into the shared controls library at:
- `FoundryMicroCore.Blazor.Controls/Components/Chat/` — ChatPanel, ChatMessageList, ChatMessage, ChatInput
- `FoundryMicroCore.Blazor.Controls/Components/Chat/ChatModels.cs` — ChatDisplayMessage, ActivityLogEntry, ActivityLogType
- `FoundryMicroCore.Blazor.Controls/wwwroot/css/chat-components.css`

**NOTE:** Three2025 currently uses its *own local copies* in `Components/Shared/Chat/` and `Models/Chat/`, NOT the shared library versions. The two sets are structurally identical but maintained separately.

---

## 4. DI Registration

All services are registered as **Scoped** (required because technicians access scoped Blazor services):

```csharp
// Program.cs — Chat system DI registration

// Domain technicians (these provide the actual tools)
builder.Services.AddScoped<IShape3DTech, Shape3DTech>();
builder.Services.AddScoped<IModelTech, ModelTech>();
// ... other ITechnician implementations (IClockTech, IShape2DTech, etc.)

// Tool discovery
builder.Services.AddScoped<ITechnicianToolProvider, TechnicianToolProvider>();

// Chat service + orchestration
builder.Services.AddScoped<IMultiProviderChatService, MultiProviderChatService>();
builder.Services.AddScoped<IChatOrchestrator, ChatOrchestrator>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
```

**Critical:** All registrations are `AddScoped` because:
1. `TechnicianToolProvider` uses `IServiceProvider.GetService()` to resolve technicians at runtime
2. Technicians are scoped (they may hold per-circuit state like the current 3D stage)
3. Singleton tool providers would fail to resolve scoped technicians

---

## 5. Data Flow — Complete Message Lifecycle

### 5.1 User Sends a Message

```
User types "Create a red box" → presses Enter
  │
  ▼
ChatOrchestratorTest.SendMessage()
  │ Enqueues message → ProcessMessageQueue()
  ▼
ProcessSingleMessage(message)
  │ 1. Sets isProcessing = true
  │ 2. Adds system prompt to conversationHistory (if first message)
  │ 3. Adds ChatMessage(ChatRole.User, message) to conversationHistory
  │ 4. Calls StateHasChanged() to show user bubble immediately
  ▼
ChatService.SendMessageStreamingAsync(message, conversationHistory, allTools)
```

### 5.2 Multi-Turn Tool Execution Loop (Inside MultiProviderChatService)

```
SendMessageStreamingAsync:
  │
  │  TURN LOOP (max 5 turns):
  │  ┌──────────────────────────────────────────────┐
  │  │ Call chatClient.GetStreamingResponseAsync()   │
  │  │   │                                          │
  │  │   ├─ Collect text chunks → chunks list       │
  │  │   └─ Collect FunctionCallContent → toolCalls │
  │  │                                              │
  │  │ Add assistant message to conversationHistory  │
  │  │ (includes both TextContent + tool calls)      │
  │  │                                              │
  │  │ IF toolCalls.Any():                          │
  │  │   For each tool call:                        │
  │  │     1. Find AIFunction by name               │
  │  │     2. Build AIFunctionArguments from args    │
  │  │     3. Invoke tool: tool.InvokeAsync(args)   │
  │  │     4. Format result: OPResult.AsToolResult() │
  │  │     5. Add FunctionResultContent to history  │
  │  │        as ChatRole.Tool                      │
  │  │   CONTINUE LOOP (let LLM see tool results)  │
  │  │                                              │
  │  │ IF no toolCalls:                             │
  │  │   BREAK (conversation complete)              │
  │  └──────────────────────────────────────────────┘
  │
  ▼ Yield text chunks
```

### 5.3 Back in the Page

```
Page collects ALL streaming chunks into fullResponse string
  │
  ├─ If fullResponse not empty:
  │    Show streamingResponse, then add to conversationHistory
  │
  ├─ If empty but tools executed:
  │    Generate tool summary message, add to history
  │
  └─ Set isProcessing = false
     StateHasChanged()
```

### 5.4 The Orchestrator Path (Currently PARTIALLY USED)

**Important Note:** The page currently **bypasses the orchestrator** for sending messages. It injects `IChatOrchestrator` but calls `ChatService.SendMessageStreamingAsync()` directly with all tools. The orchestrator's `ProcessMessageStreamingAsync()` (which does LLM-based intent analysis → agent routing) is available but not called from the main send path.

The orchestrator IS used for:
- `GetAvailableAgents()` — agent listing
- `GetToolCount()` / `GetAllTools()` — tool inspection

The orchestrator's intended flow (if activated):
```
ProcessMessageStreamingAsync:
  1. AnalyzeIntentAsync() — calls LLM with coordinator prompt
     - Sends available agent descriptions to LLM
     - LLM returns JSON: { primaryAgent, primaryAction, confidence }
  2. Routes to selected agent's ProcessStreamingAsync()
  3. Agent builds its own system prompt + tool subset
  4. Agent calls ChatService.SendMessageStreamingAsync() with its tools
```

---

## 6. Tool Discovery Pipeline

### 6.1 How Tools Get From Code to LLM

```
1. Developer writes a C# class implementing ITechnician:
   
   public class Shape3DTech : IShape3DTech   // IShape3DTech : ITechnician
   {
       [AgentTool("AddShape")]
       [Description("Creates a new 3D shape")]
       public OPResult AddShape(string name, string color, string shapeType = "box") { ... }
   }

2. TechnicianToolProvider.DiscoverAllTools():
   - Scans AppDomain for all interfaces extending ITechnician
   - For each interface, resolves implementation from DI
   - Reflects over methods with [AgentTool] or [Description] attributes
   - Calls AIFunctionFactory.Create(method, target: implementation)
   - Returns List<AIFunction>

3. ChatOrchestrator constructor:
   - Calls _toolProvider.DiscoverAllTools() → gets ALL tools
   - Also calls _toolProvider.GetToolsFor<IShape3DTech>() → filtered tools
   - Creates agents via AgentFactory, giving each its tool subset

4. Agent.ProcessStreamingAsync():
   - Passes its tool list to ChatService.SendMessageStreamingAsync()
   - Tools are set as ChatOptions.Tools for the LLM call

5. LLM decides to call a tool:
   - Returns FunctionCallContent with tool name + arguments
   - MultiProviderChatService finds the AIFunction, invokes it
   - Result formatted via OPResult.AsToolResult()
   - Added to conversation as ChatRole.Tool message
```

### 6.2 The AgentTool Attribute

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class AgentToolAttribute : Attribute
{
    public string Name { get; }           // Tool name exposed to LLM
    public string? Description { get; set; } // Optional (prefer [Description] attribute)
    
    public AgentToolAttribute(string name) { Name = name; }
    public AgentToolAttribute() : this(string.Empty) { }  // Auto-name from method
}
```

Methods are discovered if they have EITHER:
- `[AgentTool]` attribute, OR
- `[Description]` attribute AND are declared on the implementation type (not inherited)

### 6.3 OPResult — Tool Return Type

Tools return `OPResult` (from FoundryMentorModeler.Evaluator). Key features:
- `ResultType` → string ("Success", "Error", "Shape3D", etc.)
- `HasError` → bool
- `ResultMessage` → human-readable string for LLM consumption
- `OPResult.AsToolResult(result)` → formats any return value for LLM
- `OPResult.Success(message)` / `OPResult.Error(message)` — factory methods

---

## 7. Provider Architecture

### 7.1 IChatProvider Interface

```csharp
public interface IChatProvider
{
    IChatClient GetChatClient();  // Returns Microsoft.Extensions.AI.IChatClient
    string ProviderName { get; }  // Display name
    string ModelName { get; }     // Model identifier
}
```

### 7.2 Provider Implementations

| Provider | Package | Client Creation |
|----------|---------|-----------------|
| `GitHubModelProvider` | `Azure.AI.Inference`, `Microsoft.Extensions.AI.AzureAIInference` | `ChatCompletionsClient` → `.AsIChatClient(modelId)`, wrapped in `GitHubChatClient` for logging |
| `BedrockProvider` | `AWSSDK.BedrockRuntime`, `AWSSDK.Extensions.Bedrock.MEAI` | `AmazonBedrockRuntimeClient` → `.AsIChatClient(modelId)` |
| `OllamaProvider` | `OllamaSharp` | `new OllamaApiClient(endpoint, model)` (implements IChatClient directly) |

### 7.3 Provider Selection Priority

`AWS Bedrock > GitHub Models > Ollama`

### 7.4 Credential Resolution

Each provider checks (in order): `IConfiguration` → `Environment.GetEnvironmentVariable(name, User)`

| Provider | Required Config Keys |
|----------|---------------------|
| GitHub | `GitHubPatToken` |
| Bedrock | `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION` (optional, default "us-east-1") |
| Ollama | `OLLAMA_ENDPOINT` (default "http://localhost:11434"), `OLLAMA_MODEL` (default "llama3.2") |

---

## 8. Agent Architecture

### 8.1 ISpecializedAgent Interface

```csharp
public interface ISpecializedAgent
{
    string Name { get; }
    string Description { get; }
    bool IsRelevantForContext(PageContext context);
    Task<string> ProcessAsync(string userMessage, PageContext context, List<ChatMessage> history, CancellationToken ct);
    IAsyncEnumerable<string> ProcessStreamingAsync(string userMessage, PageContext context, List<ChatMessage> history, CancellationToken ct);
}
```

### 8.2 Agent Pattern

Every agent follows the same structural pattern:

```csharp
public class [AgentName] : ISpecializedAgent
{
    private readonly IMultiProviderChatService _chatService;
    private readonly List<AIFunction> _tools;
    private readonly ILogger _logger;
    
    public string Name => "[Agent Display Name]";
    public string Description => "[What this agent does]";
    
    // Constructor receives chatService + filtered tools + logger
    
    public bool IsRelevantForContext(PageContext context)
    {
        // Check page name / domain focus against relevant keywords
    }
    
    public async Task<string> ProcessAsync(...)
    {
        var systemPrompt = GetSystemPrompt();  // Detailed instructions for this domain
        var messages = new List<ChatMessage> { new(ChatRole.System, systemPrompt) };
        messages.AddRange(conversationHistory.TakeLast(5));     // ← Only last 5 messages
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        
        return await _chatService.SendMessageAsync(userMessage, messages, _tools, ct);
    }
    
    public async IAsyncEnumerable<string> ProcessStreamingAsync(...)
    {
        // Same pattern but yields chunks from SendMessageStreamingAsync
    }
}
```

### 8.3 Current Agents

| Agent | Name | Tools | Relevance Check |
|-------|------|-------|-----------------|
| `GeneralAgent` | "General Agent" | ALL tools | Always relevant (fallback) |
| `ThreeDModelingAgent` | "Shape3D Technician" | `IShape3DTech` tools only | Pages with "geometry", "3d", "model", "shape" in name/focus |
| `KnowledgeModelingAgent` | "Knowledge Modeling Agent" | `IModelTech` tools only | Pages with "ConversationalModeler", "KnModelAnimation", "mentor" |

### 8.4 Intent Analysis (Coordinator Prompt)

The orchestrator uses an LLM call to classify user intent:

```
Coordinator prompt includes:
- Available agents with descriptions
- Routing guidelines (e.g., "Battery model" → Knowledge agent, "red box" → Shape3D)
- Expected JSON output format: { primaryAgent, primaryAction, confidence }
```

JSON parsing strips markdown code blocks, falls back to first available agent on parse failure.

---

## 9. UI Component Architecture

### 9.1 Component Hierarchy

```
ChatPanel
  ├── HeaderContent (RenderFragment — customizable)
  ├── ChatPanel Body
  │   ├── ChatMessageList
  │   │   ├── ChatMessage (foreach)  ← Markdown → HTML via Markdig
  │   │   ├── Streaming Indicator    ← Shows during processing
  │   │   └── Scroll Anchor
  │   └── ActivityLog (optional)
  │       └── ActivityLogEntry (foreach)
  └── ChatPanel Footer
      └── ChatInput
          ├── RadzenSpeechToTextButton
          ├── Text Input
          ├── Send Button
          └── QuickActions (RenderFragment — customizable)
```

### 9.2 ChatPanel Parameters

| Parameter | Type | Purpose |
|-----------|------|---------|
| `Messages` | `List<ChatDisplayMessage>` | Required. Messages to display |
| `StreamingText` | `string?` | Current streaming response text |
| `IsProcessing` | `bool` | Whether a message is being processed |
| `CurrentAgent` | `string` | Agent name for streaming indicator |
| `InputValue` | `string` | Two-way bound input value |
| `OnSendMessage` | `EventCallback` | Send button handler |
| `InputPlaceholder` | `string` | Input field placeholder |
| `ShowActivityLog` | `bool` | Show/hide activity log panel |
| `ActivityLogs` | `List<ActivityLogEntry>` | Log entries |
| `AutoScrollLogs` | `bool` | Auto-scroll log panel |
| `OnToggleAutoScroll` | `EventCallback` | Toggle auto-scroll |
| `OnClearLogs` | `EventCallback` | Clear log entries |
| `ChatFlexRatio` | `string` | Flex ratio for messages area (default "3") |
| `LogFlexRatio` | `string` | Flex ratio for log area (default "2") |
| `HeaderContent` | `RenderFragment?` | Custom header content |
| `FooterActions` | `RenderFragment?` | Custom footer actions (quick test buttons) |

### 9.3 Markdown Rendering

Both `ChatMessage` and `ChatMessageList` use Markdig:

```csharp
private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .Build();

private string GetRenderedHtml()
{
    return Markdown.ToHtml(Message.Text, Pipeline);
}
```

Output is rendered via `@((MarkupString)GetRenderedHtml())`.

### 9.4 Speech-to-Text

`ChatInput` includes a `RadzenSpeechToTextButton` component that appends speech to the input value. This requires the Radzen Blazor package.

---

## 10. NuGet Dependencies

### Required for Chat System

```xml
<!-- Microsoft.Extensions.AI — Core abstractions (IChatClient, ChatMessage, AIFunction, etc.) -->
<PackageReference Include="Microsoft.Extensions.AI" Version="..." />

<!-- Microsoft.Agents.AI — ChatClientAgent -->
<PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.251219.1" />

<!-- GitHub Models provider -->
<PackageReference Include="Azure.AI.Inference" Version="1.0.0-beta.5" />
<PackageReference Include="Microsoft.Extensions.AI.AzureAIInference" Version="..." />

<!-- AWS Bedrock provider -->
<PackageReference Include="AWSSDK.BedrockRuntime" Version="4.0.14.3" />
<PackageReference Include="AWSSDK.Extensions.Bedrock.MEAI" Version="4.0.5.1" />

<!-- Ollama provider -->
<PackageReference Include="OllamaSharp" Version="..." />

<!-- Markdown rendering -->
<PackageReference Include="Markdig" Version="..." />

<!-- Radzen (for SpeechToTextButton in ChatInput) -->
<PackageReference Include="Radzen.Blazor" Version="..." />
```

---

## 11. Reproducing in Another Application — Step by Step

### Step 1: Models (no dependencies)

Copy these **as-is** — they have no external dependencies:

```
Models/Chat/ChatDisplayMessage.cs
Models/Chat/ActivityLogEntry.cs
Models/Chat/ActivityLogType.cs
Models/Chat/TestSequenceMetadata.cs
```

### Step 2: Interfaces

Copy these — they depend only on `Microsoft.Extensions.AI`:

```
Apprentice/ITechnician.cs                    # Marker interface
Services/Chat/IChatProvider.cs               # IChatClient wrapper
Services/Chat/ISpecializedAgent.cs           # Agent contract
Services/Chat/IChatOrchestrator.cs           # Orchestrator + PageContext + AgentResponse + StreamingChunk
Services/Chat/IAgentFactory.cs               # Agent factory
Services/Agents/ITechnicianToolProvider.cs   # Tool discovery
Services/Agents/AgentToolAttribute.cs        # [AgentTool] attribute
```

### Step 3: Provider Implementations

Copy one or more based on which LLM backends you need:

```
Services/Chat/GitHubModelProvider.cs    # + GitHubChatClient.cs
Services/Chat/BedrockProvider.cs
Services/Chat/OllamaProvider.cs
```

### Step 4: Core Services

```
Services/Chat/MultiProviderChatService.cs    # The heart — tool execution loop
Services/Agents/TechnicianToolProvider.cs     # Reflection-based tool discovery
Services/Chat/ChatOrchestrator.cs            # Agent routing
Services/Chat/AgentFactory.cs                # Agent creation
```

### Step 5: Agents

Copy the agents you need (or create new domain-specific ones):

```
Services/Chat/Agents/GeneralAgent.cs
Services/Chat/Agents/ThreeDModelingAgent.cs
Services/Chat/Agents/KnowledgeModelingAgent.cs
```

### Step 6: Your Domain Technicians

Create your own `ITechnician` implementations with `[AgentTool]` or `[Description]` methods:

```csharp
public interface IMyDomainTech : ITechnician { }

public class MyDomainTech : IMyDomainTech
{
    [AgentTool("DoSomething")]
    [Description("Does something useful in my domain")]
    public OPResult DoSomething(string input)
    {
        // Your logic here
        return OPResult.Success($"Did something with {input}");
    }
}
```

### Step 7: UI Components

```
Components/Shared/Chat/ChatPanel.razor[.cs]
Components/Shared/Chat/ChatMessageList.razor[.cs]
Components/Shared/Chat/ChatMessage.razor[.cs]
Components/Shared/Chat/ChatInput.razor[.cs]
Components/Shared/Chat/ActivityLog.razor[.cs]
Components/Shared/Chat/ActivityLogEntry.razor[.cs]
wwwroot/css/chat-components.css
```

**OR** use the shared library versions from `FoundryMicroCore.Blazor.Controls`.

### Step 8: DI Registration

```csharp
// Your technicians
builder.Services.AddScoped<IMyDomainTech, MyDomainTech>();

// Chat infrastructure
builder.Services.AddScoped<ITechnicianToolProvider, TechnicianToolProvider>();
builder.Services.AddScoped<IMultiProviderChatService, MultiProviderChatService>();
builder.Services.AddScoped<IChatOrchestrator, ChatOrchestrator>();
builder.Services.AddScoped<IAgentFactory, AgentFactory>();
```

### Step 9: Page

```razor
@page "/my-chat"
@using MyApp.Services.Chat

<ChatPanel Messages="displayMessages"
           StreamingText="streamingResponse"
           IsProcessing="isProcessing"
           @bind-InputValue="userInput"
           OnSendMessage="SendMessage">
    <HeaderContent>
        <h2>My Chat Page</h2>
    </HeaderContent>
</ChatPanel>
```

---

## 12. Known Issues & Gotchas

### 12.1 OPResult Dependency

`OPResult` lives in `FoundryMentorModeler.Evaluator` and inherits from `Operator` (a parser class). It has deep dependencies on the Mentor evaluator system. To use it in another project, you either:
- **Reference FoundryMentorModeler** (simplest), or
- **Create a simplified OPResult** that satisfies `OPResult.AsToolResult()` and `OPResult.Success()`/`OPResult.Error()`, or
- **Return plain strings/objects** from your tools (AIFunctionFactory handles most return types)

### 12.2 Page Bypasses Orchestrator

The `ChatOrchestratorTest` page **does not use** the orchestrator's routing for sending messages. It calls `ChatService.SendMessageStreamingAsync()` directly, passing ALL tools. This means:
- No intent analysis occurs
- No agent-specific system prompts are used
- All tools are available to the LLM at once
- The `IChatOrchestrator` injection is present but unused for the main flow

To activate full orchestration, the page would need to call `ChatOrchestrator.ProcessMessageStreamingAsync()` instead.

### 12.3 Conversation History Grows Unbounded

The `conversationHistory` list in the page grows without limit during a session. Agents mitigate this by only using `.TakeLast(5)` when building their message lists, but the full history is still passed to `SendMessageStreamingAsync` (which passes it to the LLM). Long conversations will hit token limits.

### 12.4 Streaming Chunks Are Collected, Not Streamed

The page currently collects ALL streaming chunks into `fullResponse` before displaying, defeating the purpose of streaming:

```csharp
await foreach (var chunk in ChatService.SendMessageStreamingAsync(...))
{
    fullResponse += chunk;
    // DON'T update streamingResponse - collect everything first
}
streamingResponse = fullResponse;  // Show all at once
```

This was an intentional choice to avoid "streaming cursor flickering."

### 12.5 Microsoft.Agents.AI Dependency

`MultiProviderChatService` uses `ChatClientAgent` from `Microsoft.Agents.AI` (preview) and an extension method `chatClient.CreateAIAgent()` for non-Ollama providers. This package is in preview and the API may change.

### 12.6 Two Copies of Chat UI Components

The chat UI exists in both:
- `Three2025/Components/Shared/Chat/` (app-specific)
- `FoundryMicroCore.Blazor.Controls/Components/Chat/` (shared library)

These are structurally identical. The app uses its local copies. Any bug fix needs to be applied in both places unless consolidated.

### 12.7 SpeechToText Requires Radzen

The `ChatInput` component uses `RadzenSpeechToTextButton`, which requires the Radzen.Blazor package. If you don't need speech input, remove this dependency and the related markup/handler.

---

## 13. Test Infrastructure

### 13.1 ChatTestScenarios

Test sequences are defined as static scenario classes with `[TestSequence]` attributes:

```csharp
public class ChatTestScenarios3D : ChatTestScenariosBase
{
    public override string Domain => "3D Geometry";

    [TestSequence(DisplayName = "🔷 Basic Geometry", Category = "3D Geometry")]
    public string[] BasicGeometryTest => new[] {
        "Create a red box named box1",
        "Change it to yellow",
        "Move the X location to 4"
    };
}
```

`ChatTestScenarios.GetSequencesByCategory()` discovers all scenarios via reflection.

### 13.2 Message Queue

The page uses a `Queue<string>` for sequential message processing. Test sequences load all prompts into the queue, then `ProcessMessageQueue()` sends them one at a time with 500ms delays between messages.

---

## 14. Confidence Assessment

| Component | Portability | Notes |
|-----------|------------|-------|
| Models (ChatDisplayMessage, etc.) | 🟢 Easy | No dependencies |
| IChatProvider + providers | 🟢 Easy | Self-contained, just NuGet packages |
| ISpecializedAgent + agents | 🟢 Easy | Only depend on IMultiProviderChatService |
| MultiProviderChatService | 🟡 Medium | OPResult dependency; Microsoft.Agents.AI preview |
| TechnicianToolProvider | 🟢 Easy | Standard reflection + DI |
| ChatOrchestrator | 🟢 Easy | Clean separation |
| UI Components | 🟡 Medium | Radzen dependency in ChatInput; needs CSS |
| OPResult | 🔴 Hard | Deep FoundryMentorModeler dependency chain |
| ITechnician implementations | 🔴 Domain-specific | Shape3DTech depends on FoundryWorldsAndDrawings |

### Recommended Simplification for New App

1. Replace `OPResult` returns with simple `string` or `Dictionary<string, object>` — `AIFunctionFactory` handles JSON serialization of standard types
2. Remove `Microsoft.Agents.AI` / `ChatClientAgent` — use `IChatClient` directly (the `SendMessageStreamingAsync` tool loop already does this)
3. Remove `RadzenSpeechToTextButton` if not needed
4. Start with one provider (e.g., GitHub Models only) and add others later

---

*This specification captures the complete design of the Three2025 chat orchestrator system as of February 13, 2026.*
