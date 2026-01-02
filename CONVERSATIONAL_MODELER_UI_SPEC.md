# Conversational Modeler UI Specification

## Overview

The Conversational Modeler is a split-panel interface that combines natural language interaction with real-time knowledge model construction and evaluation. The chatbot constructs declarative models through conversation, and the model panel displays the resulting calculations with proper units.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Conversational Modeler                       │
├──────────────────────────────┬──────────────────────────────────┤
│   CHAT PANEL (Left)          │   MODEL PANEL (Right)            │
│                              │                                  │
│  [Test Prompt Dropdown ▼]    │   Model Tree:                    │
│  ┌─────────────────────────┐ │   ⊟ beam1 (BeamConcept)         │
│  │ Conversation History    │ │     ├─ Length: 10 ft            │
│  │                         │ │     ├─ Load: 500 lb             │
│  │ User: I have a 10 ft   │ │     ├─ E: 29000000 psi          │
│  │ steel beam...          │ │     ├─ I: 10.9 in⁴             │
│  │                         │ │     └─ deflection: 0.154 in    │
│  │ Copilot: I'll calculate│ │                                  │
│  │ the deflection...      │ │   Parameters:                   │
│  │                         │ │   ┌────────────────────────┐   │
│  │                         │ │   │ Name: deflection       │   │
│  │                         │ │   │ Value: 0.154 in        │   │
│  └─────────────────────────┘ │   │ Formula: (Load@ * ...  │   │
│                              │   └────────────────────────┘   │
│  [🎤] Type message...   [📤] │                                  │
│                              │   API Calls Log:                │
│                              │   • CreateComponent(...)        │
│                              │   • AddCalculation(...)         │
└──────────────────────────────┴──────────────────────────────────┘
                    [Resizable Splitter]
```

## Left Panel: Chat Interface

### Components

1. **Test Prompt Dropdown**
   - Pre-built prompts for testing common scenarios
   - Categories: Beams, Trusses, Thermal, Electrical, etc.
   - Clicking a prompt injects it into the input field
   - Examples:
     - "Simple Beam Deflection"
     - "Cantilever Beam with Distributed Load"
     - "Heat Transfer Through Wall"
     - "Voltage Divider Circuit"

2. **Conversation History**
   - Scrollable message list
   - User messages: right-aligned, blue background
   - Copilot messages: left-aligned, gray background
   - Display timestamps
   - Support markdown rendering (equations, code blocks)
   - Auto-scroll to newest message

3. **Input Controls**
   - Text input field with placeholder: "Describe your engineering problem..."
   - Voice input button (🎤) - uses browser speech recognition
   - Send button (📤)
   - Enter to send, Shift+Enter for newline
   - Input validation (non-empty)

### Features

**Voice Input:**
- Click microphone icon to start recording
- Visual feedback during recording (pulsing animation)
- Speech-to-text using browser Web Speech API
- Automatically populates text input when complete
- Error handling for unsupported browsers

**Test Prompt Injection:**
```razor
<select @onchange="InjectPrompt">
    <option value="">-- Select Test Prompt --</option>
    <optgroup label="Structural">
        <option value="beam-simple">Simple Beam Deflection</option>
        <option value="beam-cantilever">Cantilever Beam</option>
        <option value="truss-basic">Basic Truss Analysis</option>
    </optgroup>
    <optgroup label="Thermal">
        <option value="heat-wall">Heat Transfer Through Wall</option>
        <option value="heat-exchanger">Heat Exchanger Sizing</option>
    </optgroup>
</select>
```

**Prompt Library:**
```csharp
public class TestPrompts
{
    public static Dictionary<string, string> Prompts = new()
    {
        ["beam-simple"] = @"I have a simply-supported steel beam, 10 feet long, 
            with a 500 lb point load at the center. The I-beam has a moment of 
            inertia of 10.9 in^4. What's the deflection at the center?",
            
        ["beam-cantilever"] = @"Calculate the maximum deflection of a cantilever 
            beam. Length is 6 feet, uniformly distributed load of 100 lb/ft, 
            aluminum (E = 10 Mpsi), rectangular cross-section 2in x 4in.",
            
        ["heat-wall"] = @"A concrete wall is 8 inches thick. Inside temperature 
            is 70°F, outside is 20°F. Concrete has thermal conductivity of 
            0.8 BTU/(hr·ft·°F). What's the heat flux through the wall?"
    };
}
```

## Right Panel: Model Visualization

### Components

1. **Model Tree View**
   - Hierarchical display of components
   - Expandable/collapsible nodes (⊞/⊟)
   - Component type displayed with name: `beam1 (BeamConcept)`
   - Child components indented
   - Click to select component (highlights in blue)
   - Shows parameter count badge: `beam1 (5 params)`

2. **Parameter Detail Panel**
   - Displays parameters for selected component
   - Three columns:
     - **Name**: Parameter identifier (e.g., "deflection")
     - **Value**: Computed result with units (e.g., "0.154 in")
     - **Formula**: Source formula or value (e.g., "(Load@ * Length@^3) / ...")
   - Color coding:
     - Input values: black text
     - Calculated values: blue text
     - Error states: red text with tooltip
   - Unit highlighting: units shown in gray italic
   - Click formula to expand/edit (future feature)

3. **API Call Log**
   - Real-time display of API calls made by chatbot
   - Formatted as timestamped entries:
     ```
     10:23:45 CreateComponent("BeamConcept", "beam1")
     10:23:45 AddCalculation("beam1", "Length|ft: 10")
     10:23:45 AddCalculation("beam1", "Load|lb: 500")
     10:23:45 AddCalculation("beam1", "E|psi: 29e6")
     10:23:46 GetParameter("beam1", "deflection") → 0.154 in
     ```
   - Collapsible section (starts expanded)
   - Clear log button
   - Export log to JSON/CSV

### Real-Time Updates

**Model State Synchronization:**
- Model updates trigger UI refresh via Blazor state change
- SignalR hub broadcasts model changes to connected clients
- Incremental updates (only changed components refresh)
- Optimistic UI updates with rollback on error

**Calculation Feedback:**
- Parameters show loading spinner while calculating
- Success: green checkmark appears briefly
- Error: red X with hover tooltip explaining issue
- Dependency chain visualization (show which params depend on which)

## Splitter Component

### Behavior
- Draggable vertical divider between panels
- Default split: 50/50
- Min panel width: 400px (prevents collapse)
- Snap points at 33%, 50%, 67%
- Double-click to reset to 50/50
- Save user preference to localStorage
- Show resize cursor on hover

### Implementation
```razor
<div class="modeler-container">
    <div class="chat-panel" style="width: @ChatPanelWidth%">
        <!-- Chat content -->
    </div>
    
    <div class="splitter" 
         @onmousedown="StartResize"
         @ondblclick="ResetSplit">
    </div>
    
    <div class="model-panel" style="width: @ModelPanelWidth%">
        <!-- Model content -->
    </div>
</div>
```

## Page Component Structure

### File: `Apprentice/ConversationalModeler.razor`

```razor
@page "/conversational-modeler"
@inject IJSRuntime JS
@inject NavigationManager Nav

<PageTitle>Conversational Modeler</PageTitle>

<div class="modeler-page">
    <h2>Conversational Modeler</h2>
    <p>Describe your engineering problem and I'll build a knowledge model to solve it.</p>
    
    <div class="modeler-container">
        <!-- Chat Panel -->
        <ChatPanel @bind-Messages="chatMessages"
                   @bind-CurrentInput="userInput"
                   TestPrompts="TestPrompts.Prompts"
                   OnSendMessage="HandleUserMessage"
                   OnInjectPrompt="InjectTestPrompt" />
        
        <!-- Splitter -->
        <Splitter @bind-LeftWidth="chatPanelWidth" />
        
        <!-- Model Panel -->
        <ModelPanel Model="currentModel"
                    SelectedComponent="selectedComponent"
                    ApiCallLog="apiCallLog"
                    OnComponentSelected="HandleComponentSelection" />
    </div>
</div>

@code {
    private List<ChatMessage> chatMessages = new();
    private string userInput = "";
    private int chatPanelWidth = 50;
    private PartComponent? currentModel;
    private PartComponent? selectedComponent;
    private List<ApiCall> apiCallLog = new();
    
    private async Task HandleUserMessage(string message)
    {
        // Add user message to history
        chatMessages.Add(new ChatMessage 
        { 
            Role = "user", 
            Content = message, 
            Timestamp = DateTime.Now 
        });
        
        // Call LLM with model construction API context
        var response = await CallChatbot(message);
        
        // Add assistant response
        chatMessages.Add(new ChatMessage 
        { 
            Role = "assistant", 
            Content = response, 
            Timestamp = DateTime.Now 
        });
        
        StateHasChanged();
    }
    
    private void InjectTestPrompt(string promptKey)
    {
        if (TestPrompts.Prompts.TryGetValue(promptKey, out var prompt))
        {
            userInput = prompt;
        }
    }
}
```

## Model Construction API

The chatbot uses these APIs to construct models dynamically:

### API Methods

```csharp
public interface IModelConstructionService
{
    /// <summary>
    /// Create a new component of the specified type
    /// </summary>
    string CreateComponent(string typeName, string instanceName);
    
    /// <summary>
    /// Add a calculation (parameter with formula or value) to a component
    /// </summary>
    void AddCalculation(string componentPath, string formula);
    
    /// <summary>
    /// Add a child component to a parent
    /// </summary>
    void AddSubComponent(string parentPath, string childPath);
    
    /// <summary>
    /// Get the computed value of a parameter with units
    /// </summary>
    (double value, string unit) GetParameter(string componentPath, string paramName);
    
    /// <summary>
    /// Get summary of component (parameters, subcomponents)
    /// </summary>
    ComponentSummary GetComponentSummary(string componentPath);
    
    /// <summary>
    /// Clear the current model and start fresh
    /// </summary>
    void ResetModel();
}
```

### API Call Logging

```csharp
public class ApiCall
{
    public DateTime Timestamp { get; set; }
    public string Method { get; set; }
    public object[] Arguments { get; set; }
    public object? Result { get; set; }
    public string? Error { get; set; }
    
    public string FormatLog()
    {
        var args = string.Join(", ", Arguments.Select(a => 
            a is string ? $"\"{a}\"" : a.ToString()));
        var result = Result != null ? $" → {Result}" : "";
        return $"{Timestamp:HH:mm:ss} {Method}({args}){result}";
    }
}
```

## Example Usage Flow

### Scenario: Beam Deflection Problem

**User Action:**
1. User selects "Simple Beam Deflection" from test prompt dropdown
2. Prompt appears in input field
3. User clicks send or presses Enter

**System Response:**

*Chat Panel:*
```
User (10:23:45):
I have a simply-supported steel beam, 10 feet long, with a 500 lb 
point load at the center. The I-beam has a moment of inertia of 
10.9 in^4. What's the deflection at the center?

Copilot (10:23:46):
I'll create a beam model to calculate the deflection. For a 
simply-supported beam with center point load, the deflection 
formula is: δ = (P × L³) / (48 × E × I)

Setting up the model...
[API calls execute]

The beam deflection at the center is **0.154 inches**.
```

*Model Panel:*
```
Model Tree:
⊟ beam1 (BeamConcept) [6 params]
  ├─ Length: 10 ft
  ├─ Load: 500 lb
  ├─ E: 29000000 psi
  ├─ I: 10.9 in⁴
  ├─ L_inches: 120 in (calculated)
  └─ deflection: 0.154 in (calculated)

API Call Log:
10:23:45 CreateComponent("BeamConcept", "beam1")
10:23:45 AddCalculation("beam1", "Length|ft: 10")
10:23:45 AddCalculation("beam1", "Load|lb: 500")
10:23:45 AddCalculation("beam1", "E|psi: 29e6")
10:23:45 AddCalculation("beam1", "I|in4: 10.9")
10:23:45 AddCalculation("beam1", "L_inches|in: Length@")
10:23:46 AddCalculation("beam1", "deflection|in: (Load@ * L_inches@^3) / (48 * E@ * I@)")
10:23:46 GetParameter("beam1", "deflection") → 0.154 in
```

## Styling

### Layout CSS
```css
.modeler-page {
    padding: 20px;
    height: calc(100vh - 100px);
}

.modeler-container {
    display: flex;
    height: calc(100% - 80px);
    gap: 0;
    border: 1px solid #ddd;
    border-radius: 8px;
    overflow: hidden;
}

.chat-panel {
    display: flex;
    flex-direction: column;
    background: #f9f9f9;
    overflow: hidden;
}

.splitter {
    width: 4px;
    background: #ccc;
    cursor: col-resize;
    transition: background 0.2s;
}

.splitter:hover {
    background: #0078d4;
}

.model-panel {
    display: flex;
    flex-direction: column;
    background: white;
    overflow: hidden;
}
```

### Chat Styling
```css
.conversation-history {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
}

.message {
    margin-bottom: 12px;
    padding: 10px 14px;
    border-radius: 8px;
    max-width: 80%;
}

.message.user {
    background: #0078d4;
    color: white;
    margin-left: auto;
}

.message.assistant {
    background: #e8e8e8;
    color: black;
}

.message .timestamp {
    font-size: 0.75rem;
    opacity: 0.7;
    margin-top: 4px;
}
```

### Model Panel Styling
```css
.model-tree {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
    font-family: 'Cascadia Code', monospace;
}

.tree-node {
    padding: 4px 8px;
    cursor: pointer;
    border-radius: 4px;
}

.tree-node:hover {
    background: #f0f0f0;
}

.tree-node.selected {
    background: #e3f2fd;
    border-left: 3px solid #0078d4;
}

.parameter-panel {
    border-top: 1px solid #ddd;
    padding: 16px;
    max-height: 40%;
    overflow-y: auto;
}

.parameter-grid {
    display: grid;
    grid-template-columns: 150px 120px 1fr;
    gap: 8px;
    font-size: 0.9rem;
}

.parameter-value.calculated {
    color: #0078d4;
    font-weight: 500;
}

.parameter-unit {
    color: #666;
    font-style: italic;
}
```

## Implementation Phases

### Phase 1: Basic Layout (Week 1)
- ✅ Create ConversationalModeler.razor page
- ✅ Implement splitter component
- ✅ Basic chat UI (messages, input)
- ✅ Basic model tree display

### Phase 2: Model Construction API (Week 2)
- ✅ Implement IModelConstructionService
- ✅ CreateComponent, AddCalculation methods
- ✅ PartComponent integration with dynamic creation
- ✅ API call logging

### Phase 3: Chatbot Integration (Week 3)
- ✅ Connect to OpenAI/Azure OpenAI
- ✅ System prompt with API documentation
- ✅ Function calling for model construction
- ✅ Response streaming

### Phase 4: Test Prompts & Validation (Week 4)
- ✅ Build prompt library
- ✅ Automated test suite
- ✅ Validation of computed results
- ✅ Error handling and recovery

### Phase 5: Polish & Features (Week 5)
- ⬜ Voice input
- ⬜ Export model to JSON/code
- ⬜ Save/load conversations
- ⬜ Visualization improvements

## Success Criteria

### Functional Requirements
- ✅ User can chat with AI using natural language
- ✅ AI constructs models using API calls
- ✅ Model updates in real-time on right panel
- ✅ All calculations show correct values with units
- ✅ Test prompts work reliably (>90% success rate)

### Performance Requirements
- ⏱️ Model updates render within 100ms
- ⏱️ Chat response latency < 3 seconds
- ⏱️ Support models with 100+ components without lag

### Quality Requirements
- ✅ Formula validation catches syntax errors
- ✅ Unit conversions are automatic and correct
- ✅ Error messages are clear and actionable
- ✅ UI is responsive on tablet/desktop

## Next Steps

1. **Create ConversationalModeler.razor page** with split layout
2. **Implement ModelConstructionService** with basic CRUD operations
3. **Build test prompt library** for beam deflection scenario
4. **Connect chatbot** with system prompt containing API docs
5. **Validate end-to-end flow** with one complete test case

Once working, we can expand to more complex scenarios (trusses, thermal, electrical) and refine the prompt engineering to improve success rates.
