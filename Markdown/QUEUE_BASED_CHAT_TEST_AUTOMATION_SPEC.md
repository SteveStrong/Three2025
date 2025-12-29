# Queue-Based Chat Test Automation Specification

## Status: Phase 1 Complete ✅

**Last Updated**: December 27, 2025

## Overview

This specification describes a unified queue-based architecture for the chat system that treats all message processing through a single queue mechanism. This approach naturally enables automated test sequences while maintaining normal single-message chat functionality.

**Core Principle**: Every message—whether typed by a user or loaded from a test sequence—goes through the same queue and processing pipeline.

## Current State

### ✅ Phase 1 Complete
- Created `Models/Chat/TestSequenceMetadata.cs` with attribute support
- Created `Services/Chat/ChatTestScenarios.cs` with three domain collections:
  - **ChatTestScenarios3D**: 7 test sequences for 3D geometry
  - **ChatTestScenarios2D**: 3 test sequences for 2D shapes
  - **ChatTestScenariosModels**: 3 test sequences for model operations
- Reflection-based discovery working across all domains
- Tests designed to be **accumulative** - operations build on each other

---

## Architecture Diagram

```
User Input (single) ──┐
                      ├──> Message Queue ──> Process Next ──> Agent ──> Response ──> Check Queue
Test Sequence (N)  ──┘                         │                              │
                                               └──────────────────────────────┘
                                                    (Loop until empty)
```

---

## Architecture Components

### ✅ 1. Test Scenarios Classes (COMPLETE)
**Files**: 
- `Models/Chat/TestSequenceMetadata.cs` ✅
- `Services/Chat/ChatTestScenarios.cs` ✅

Three domain-specific scenario collections:
- **ChatTestScenarios3D**: 3D geometry operations (boxes, spheres, cylinders, colors, transforms)
- **ChatTestScenarios2D**: 2D drawing operations (rectangles, circles, triangles, 2D transforms)
- **ChatTestScenariosModels**: Model assembly operations (grouping, cloning, complex assemblies)

### 2. Test Sequence Selector Component (NEXT)
**Files**: 
- `Components/Shared/Chat/TestSequenceSelector.razor`
- `Components/Shared/Chat/TestSequenceSelector.razor.cs`
- `wwwroot/css/chat-components.css` (add styles)

Self-contained Blazor component that:
- Discovers all test sequences via reflection
- Groups by domain/category in dropdown
- Fires EventCallback when sequence selected
- Shows description and prompt count

### 3. Queue Management in Chat Pages (NEXT)
**Files**: 
---

## Implementation Plan - Remaining Phases

### ✅ Phase 1: Create Test Infrastructure (COMPLETE)
**Goal**: Add test scenario infrastructure without touching existing chat code.

**Status**: ✅ COMPLETE
- Add Queue<string> for message queueing
- Refactor SendMessage → ProcessMessageQueue + ProcessSingleMessage
- Add HandleTestSequenceSelected event handler
- Disable UI during queue processing

### 4. Integration & Testing (FINAL)
Validate that:
- Single messages work normally (queue size = 1)
- Test sequences execute all prompts in order
- Visual feedback during queue processing
- Can run multiple tests consecutively

---

## Implementation Phases

### Phase 1: Create Test Infrastructure (Non-Breaking)
**Goal**: Add test scenario infrastructure without touching existing chat code.

#### Tasks:
1. **Create `Models/Chat/TestSequenceMetadata.cs`**
   ```csharp
   public record TestSequenceMetadata
   {
       public string Name { get; init; }
       public string DisplayName { get; init; }
       public string Description { get; init; }
       public string[] Prompts { get; init; }
       public int PromptCount => Prompts?.Length ?? 0;
   }

   [AttributeUsage(AttributeTargets.Property)]
   public class TestSequenceAttribute : Attribute
   {
       public string DisplayName { get; set; }
       public string Description { get; set; }
       public string Category { get; set; } = "General";
   }
   ```

2. **Create `Services/Chat/ChatTestScenarios.cs`**
   ```csharp
   public class ChatTestScenarios
   {
       [TestSequence(
           DisplayName = "🔷 Basic Geometry",
           Description = "Tests basic geometry creation and modification",
           Category = "Geometry")]
       public string[] BasicGeometryTest => new[]
       {
           "Create a red box",
           "Change it to yellow", 
           "Move the X location to 4",
           "Convert it to a cylinder"
       };

       [TestSequence(
           DisplayName = "🎨 Color Cycle",
           Description = "Tests color changes on shapes",
           Category = "Appearance")]
       public string[] ColorCycleTest => new[]
       {
           "Create a blue sphere",
           "Change it to green",
           "Change it to red",
           "Make it yellow"
       };

       [TestSequence(
           DisplayName = "📐 Transform Sequence",
           Description = "Tests position and scale transformations",
           Category = "Transform")]
       public string[] TransformSequence => new[]
       {
           "Create a box at position 0,0,0",
           "Move it to X:5",
           "Move it to Y:3",
           "Make it twice as big",
           "Rotate it 45 degrees"
       };

       // Reflection-based discovery
       public static Dictionary<string, TestSequenceMetadata> GetAllSequences()
       {
           var scenarios = new ChatTestScenarios();
           var type = typeof(ChatTestScenarios);
           var sequences = new Dictionary<string, TestSequenceMetadata>();

           foreach (var prop in type.GetProperties())
           {
               if (prop.PropertyType == typeof(string[]))
               {
                   var attr = prop.GetCustomAttribute<TestSequenceAttribute>();
                   var prompts = prop.GetValue(scenarios) as string[];
                   
                   sequences[prop.Name] = new TestSequenceMetadata
                   {
                       Name = prop.Name,
                       DisplayName = attr?.DisplayName ?? prop.Name,
                       Description = attr?.Description ?? string.Empty,
                       Prompts = prompts ?? Array.Empty<string>()
                   };
               }
           }

           return sequences;
       }

       public static TestSequenceMetadata? GetSequence(string name)
       {
           var all = GetAllSequences();
           return all.TryGetValue(name, out var sequence) ? sequence : null;
       }
   }
   ```

3. **Create unit tests** (optional but recommended)
   ```csharp
   // Tests/Services/Chat/ChatTestScenariosTests.cs
   public class ChatTestScenariosTests
   {
       [Fact]
       public void GetAllSequences_ShouldDiscoverAllTestSequences()
       {
           var sequences = ChatTestScenarios.GetAllSequences();
           Assert.NotEmpty(sequences);
           Assert.Contains("BasicGeometryTest", sequences.Keys);
  Completed**:
- ✅ TestSequenceMetadata record and TestSequenceAttribute
- ✅ ChatTestScenarios3D with 7 test sequences
- ✅ ChatTestScenarios2D with 3 test sequences
- ✅ ChatTestScenariosModels with 3 test sequences
- ✅ Reflection-baself-contained, reusable UI component for test selection.

**Where**: Will be placed in ChatOrchestratorTest header alongside existing Tools/Agents/Provider info.

#### Tasks (Estimated: 1-2 hours)
---

### Phase 2: Create Test Selector Component (IN PROGRESSks.

---

### Phase 2: Create Test Selector Component (Non-Breaking)
**Goal**: Create reusable UI component for test selection.

#### Tasks:
1. **Create `Components/Shared/Chat/TestSequenceSelector.razor`**
   ```razor
   @namespace Three2025.Components.Shared.Chat
   @using Three2025.Models.Chat
   @using Three2025.Services.Chat

   <div class="test-sequence-selector">
       <select @bind="selectedSequenceKey" 
               class="test-sequence-dropdown"
               disabled="@IsDisabled">
           <option value="">-- Select Test Sequence --</option>
           @foreach (var kvp in availableSequences.OrderBy(x => x.Value.DisplayName))
           {
               <option value="@kvp.Key">
                   @kvp.Value.DisplayName (@kvp.Value.PromptCount prompts)
               </option>
           }
       </select>
       
       <button @onclick="OnRunClicked" 
               disabled="@(IsDisabled || string.IsNullOrEmpty(selectedSequenceKey))"
               class="test-sequence-run-button"
               title="@GetSelectedDescription()">
           ▶️ Run Test
       </button>
       
       @if (!string.IsNullOrEmpty(selectedSequenceKey) && showDescription)
       {
           <div class="test-sequence-description">
               @GetSelectedDescription()
           </div>
       }
   </div>
   ```

2. **Create `Components/Shared/Chat/TestSequenceSelector.razor.cs`**
   ```csharp
   namespace Three2025.Components.Shared.Chat;

   public partial class TestSequenceSelector
   {
       [Parameter] public EventCallback<TestSequenceMetadata> OnSequenceSelected { get; set; }
       [Parameter] public bool IsDisabled { get; set; }
       [Parameter] public bool ShowDescription { get; set; } = true;

       private string selectedSequenceKey = string.Empty;
       private Dictionary<string, TestSequenceMetadata> availableSequences = new();

       protected override void OnInitialized()
       {
           availableSequences = ChatTestScenarios.GetAllSequences();
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
   ```

3. **Add CSS to `wwwroot/css/chat-components.css`**
   ```css
   /* Test Sequence Selector */
   .test-sequence-selector {
       display: flex;
       gap: 0.5rem;
       align-items: center;
       flex-wrap: wrap;
   }

   .test-sequence-dropdown {
       padding: 0.5rem 0.75rem;
       border-radius: 6px;
       border: 1px solid var(--chat-border-color);
       background: white;
       font-size: 0.9rem;
       min-width: 250px;
       cursor: pointer;
   }

   .test-sequence-dropdown:disabled {
       opacity: 0.5;
       cursor: not-allowed;
   }

   .test-sequence-run-button {
       padding: 0.5rem 1rem;
       border-radius: 6px;
       border: none;
       background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
       color: white;
       font-weight: 600;
       cursor: pointer;
       transition: all 0.2s ease;
   }

   .test-sequence-run-button:hover:not(:disabled) {
       transform: translateY(-2px);
       box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
   }

   .test-sequence-run-button:disabled {
       opacity: 0.5;
       cursor: not-allowed;
       transform: none;
   }

   .test-sequence-description {
       width: 100%;
       padding: 0.5rem;
       background: rgba(102, 126, 234, 0.1);
  Deliverables**:
- Dropdown showing all tests grouped by domain (3D Geometry, 2D Geometry, Models)
- Run button that fires EventCallback with selected test metadata
- Visual styling that fits existing header design
- Disabled state during queue processing
ChatOrchestratorTest without breaking existing functionality.

**Key Insight**: Queue lives at page level, not service level. Services remain unchanged
**Validation**: 
- Component renders in header
- Dropdown populates with all 13 test sequences
- Events fire correctly when test selected
- Styling matches existing UI

---

### Phase 3: Implement Queue Management (Estimated: 2-3 hours
   }
   ```

**Validation**: Component renders correctly, dropdown populates, events fire.

---

### Phase 3: Implement Queue Management (Modify Existing)
**Goal**: Add queue-based processing to chat pages without breaking existing functionality.

#### Tasks:
1. **Modify `Components/Pages/ChatOrchestratorTest.razor.cs`**

   Add queue fields:
   ```csharp
   private Queue<string> messageQueue = new();
   private bool isProcessingQueue = false;
   ```

   Refactor `SendMessage()` to enqueue:
   ```csharp
   private async Task SendMessage()
   {
       if (string.IsNullOrWhiteSpace(userInput))
           return;

       var message = userInput.Trim();
       userInput = "";
       
       // Enqueue the message
       messageQueue.Enqueue(message);
       
       // Start processing if not already running
       if (!isProcessingQueue)
       {
           await ProcessMessageQueue();
       }
   }
   ```

   Create new `ProcessMessageQueue()` method:
   ```csharp
   private async Task ProcessMessageQueue()
   {
       if (isProcessingQueue || messageQueue.Count == 0)
           return;

       isProcessingQueue = true;

       while (messageQueue.Count > 0)
       {
           var message = messageQueue.Dequeue();
           await ProcessSingleMessage(message);
           
           // Brief pause between queued messages for visual feedback
           if (messageQueue.Count > 0)
           {
               await Task.Delay(500);
           }
       }

       isProcessingQueue = false;
   }
   ```

   Extract existing logic into `ProcessSingleMessage()`:
   ```csharp
   private async Task ProcessSingleMessage(string message)
   {
       if (isProcessing)
           return;

       isProcessing = true;
       streamingResponse = "";
       currentAgent = "Assistant";

       AddLog(ActivityLogType.UserInput, message);

       try
       {
           // Add user message
           conversationHistory.Add(new AIChatMessage(ChatRole.User, message));
           displayMessages.Add(new Models.Chat.ChatDisplayMessage 
           { 
               IsUser = true, 
               Text = message 
           });
           await ScrollToBottom();

           AddLog(ActivityLogType.Routing, "Analyzing intent and selecting agent...");

           var fullResponse = "";

           // Stream response from orchestrator
           await foreach (var chunk in ChatOrchestrator.ProcessMessageStreamingAsync(
               message,
               pageContext,
               conversationHistory,
               onAgentSwitch: async (agentName) =>
               {
                   currentAgent = agentName;
                   AddLog(ActivityLogType.AgentSwitch, $"Routing to {agentName}", 
                       $"Specialized agent selected based on intent analysis");
                   await InvokeAsync(StateHasChanged);
               }))
           {
               if (!chunk.IsComplete)
               {
                   streamingResponse += chunk.Content;
                   fullResponse += chunk.Content;
                   currentAgent = chunk.AgentName;
                   await InvokeAsync(StateHasChanged);
               }
               else
               {
                   // Final chunk - complete the response
                   currentAgent = chunk.AgentName;
                   AddLog(ActivityLogType.Response, $"Received from {chunk.AgentName}", 
                       $"Length: {fullResponse.Length} characters");

                   // Check if tools were likely used
                   if (fullResponse.Contains("light") || fullResponse.Contains("position") || 
                       fullResponse.Contains("color"))
                   {
                       AddLog(ActivityLogType.ToolExecution, "LLM may have used lighting tools", 
                           "Tool usage detected in response context");
                   }
               }
           }

           // Add assistant response to conversation history
           conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, fullResponse));
           displayMessages.Add(new Models.Chat.ChatDisplayMessage 
           { 
               IsUser = false, 
               Text = fullResponse,
               AgentName = currentAgent
           });
           streamingResponse = "";

           await ScrollToBottom();

           Logger.LogInformation($"Response from {currentAgent}: " +
               $"{fullResponse.Substring(0, Math.Min(100, fullResponse.Length))}...");
       }
       catch (Exception ex)
       {
           Logger.LogError(ex, "Error processing message");
           AddLog(ActivityLogType.Error, $"Failed to process message: {ex.Message}");
           conversationHistory.Add(new AIChatMessage(ChatRole.Assistant, $"❌ Error: {ex.Message}"));
           displayMessages.Add(new Models.Chat.ChatDisplayMessage 
           { 
               IsUser = false, 
               Text = $"❌ Error: {ex.Message}"
           });
           streamingResponse = "";
       }
       finally
       {
           isProcessing = false;
           await InvokeAsync(StateHasChanged);
       }
   }
   ```

   Add test sequence handler:
   ```csharp
   private async Task HandleTestSequenceSelected(TestSequenceMetadata sequence)
   {
       if (isProcessingQueue)
       {
           AddLog(ActivityLogType.System, "Cannot start test", 
               "Another test sequence is already running");
           return;
       }

       AddLog(ActivityLogType.System, $"Starting test: {sequence.DisplayName}", 
           $"Loading {sequence.PromptCount} prompts into queue");

       // Load all prompts into the queue
       foreach (var prompt in sequence.Prompts)
       {
           messageQueue.Enqueue(prompt);
       }

       // Start processing
       await ProcessMessageQueue();

       AddLog(ActivityLogType.System, "Test sequence completed", 
           $"Finished executing {sequence.DisplayName}");
   }
   ```

2. **Modify `Components/Pages/ChatOrchestratorTest.razor`**

   Add the TestSequenceSelector to FooterActions:
   ```razor
   <FooterActions>
       <!-- Test Sequence Selector -->
       <TestSequenceSelector OnSequenceSelected="HandleTestSequenceSelected"
                            IsDisabled="isProcessingQueue" />
       
       <!-- Existing buttons -->
       <button @onclick='() => QuickTest("Create a red Box width:1, Height:2, Depth:3, at position X:4, y:2, Z:5")' 
               class="chat-quick-action-button chat-quick-action-warning"
               disabled="@isProcessingQueue">
           💡 Test geometry Tool
       </button>
       <button @onclick="ShowToolList" 
               class="chat-quick-action-button chat-quick-action-primary">
  Deliverables**:
- `Queue<string> messageQueue` field added
- `isProcessingQueue` flag added
- `SendMessage()` refactored to enqueue and start processing
- New `ProcessMessageQueue()` method iterates through queue
- New `ProcessSingleMessage()` contains existing logic
- New `HandleTestSequenceSelected()` loads prompts and starts queue
- TestSequenceSelector added to HeaderContent in .razor file
- 500ms delay between queued messages for visual feedback
 (AgentCanvasIntegration, Trisoc).

**Decision**: Defer until ChatOrchestratorTest proves the pattern works.

#### Tasks (Future) messages work normally (immediate, no queue overhead visible)
- ✅ Test sequences execute all prompts in order
- ✅ UI disables appropriately during execution (buttons, selector)
- ✅ Queue processes sequentially with visible delays
- ✅ Activity log shows queue operations
- ✅ Can run multiple test sequences consecutively

---

### Phase 4: Apply to Other Chat Pages (OPTIONAL - Futur
   Add namespace import at top:
   ```razor
   @using Three2025.Models.Chat
   ```

**Validation**: 
- Single messages still work normally
- Test sequences execute all prompts in order
- UI disables appropriately during execution
- Queue processes sequentially with delays

---

### Phase 4: Apply to Other Chat Pages (Expand Coverage)
**Goal**: Apply same queue mechanism to other chat implementations.
---

## Execution Plan Summary

### Immediate Next Steps (Phase 2 & 3)

**Phase 2: TestSequenceSelector Component**
1. Create `TestSequenceSelector.razor` with dropdown UI
2. Create `TestSequenceSelector.razor.cs` with reflection logic
3. Add CSS styling to `chat-components.css`
4. Test component in isolation

**Phase 3: Queue Integration** 
1. Add queue fields to `ChatOrchestratorTest.razor.cs`
2. Refactor `SendMessage()` → `ProcessMessageQueue()` + `ProcessSingleMessage()`
3. Add `HandleTestSequenceSelected()` event handler
4. Add TestSequenceSelector to ChatOrchestratorTest header
5. Test single messages still work
6. Test sequence execution

**Estimated Time**: 3-5 hours total
**Risk**: Low - all changes are additive, no service modifications

---

### Phase 5: Enhanced Features (DEFERRED - 
   - Implement queue management

2. **Apply to `Components/Pages/Trisoc.razor`**
   - Same refactoring pattern
   - Add test selector if applicable

3. **Consider creating a base class** (optional optimization):
   ```csharp
   // Components/Pages/QueuedChatPageBase.cs
   public abstract class QueuedChatPageBase : ComponentBase
   {
       protected Queue<string> messageQueue = new();
       protected bool isProcessingQueue = false;
       
       protected async Task ProcessMessageQueue() { /* shared logic */ }
       protected abstract Task ProcessSingleMessage(string message);
       protected async Task HandleTestSequenceSelected(TestSequenceMetadata sequence) 
       { /* shared logic */ }
   }
   ```

**Validation**: All chat pages support queue-based test automation.

---

### Phase 5: Enhanced Features (Optional Extensions)

**These are valuable future enhancements once the core queue system is validated.**

#### 5.0 Test Suite Viewer Page (HIGH PRIORITY)
**New page to visualize and manage all test scenarios**

Create `Components/Pages/TestSuiteViewer.razor`:
- Display all test domains (3D, 2D, Models) in expandable sections
- Show each test sequence with its prompts listed
- Metadata display (name, description, prompt count)
- Quick run button for each sequence
- Edit/Add new sequences capability
- Export test definitions to JSON

**Benefits**:
- Documentation of available tests
- Easy way to review what each test does
- Central management of test scenarios
- Onboarding tool for new team members

**UI Mockup**:
```
Test Suite Viewer
├─ 3D Geometry (7 sequences)
│  ├─ 🔷 Basic Geometry (4 prompts) [Run]
│  │  1. Create a red box
│  │  2. Change it to yellow
│  │  3. Move the X location to 4
│  │  4. Convert it to a cylinder
│  ├─ 🎨 Color Cycle (4 prompts) [Run]
│  └─ ...
├─ 2D Geometry (3 sequences)
└─ Models (3 sequences)
```

#### 5.1 Structured Output Verification (HIGH PRIORITY)
**Add assertion capabilities to validate test results**

Extend `TestSequenceMetadata` with expected outcomes:
```csharp
public record TestSequenceStep
{
    public string Prompt { get; init; }
    public Dictionary<string, object>? ExpectedState { get; init; }
}

// Example usage:
new TestSequenceStep
{
    Prompt = "Create a red box",
    ExpectedState = new()
    {
        ["shapeType"] = "box",
        ["color"] = "red",
        ["height"] = 25,
        ["exists"] = true
    }
}
```

**Verification Flow**:
1. Send prompt → get response
2. Query scene state via tool/API (e.g., GetShapeProperties)
3. Compare actual vs expected values
4. Log pass/fail with details
5. Generate test report

**Benefits**:
- Automated regression testing
- Confidence that geometry operations actually work
- Catch bugs in tool implementations
- Performance benchmarking
- "Yeah, we did change that shape to red" validation
- "Yeah, that height is 25" verification

**Implementation Considerations**:
- Need scene query API (get object properties)
- Flexible assertion matching (exact, contains, range)
- Test result persistence
- Visual diff reporting

#### 5.2 Queue Status Indicator
Add visual feedback for queue state:
```razor
@if (messageQueue.Count > 0)
{
    <div class="queue-status">
        📋 Queue: @messageQueue.Count remaining
    </div>
}
```

#### 5.3 Pause/Resume Queue
Add controls to pause test execution:
```csharp
private bool isQueuePaused = false;

private void PauseQueue() => isQueuePaused = true;
private void ResumeQueue() 
{
    isQueuePaused = false;
    _ = ProcessMessageQueue(); // Resume processing
}
```

#### 5.4 Queue Clear Button
Allow clearing the queue mid-execution:
```csharp
private void ClearQueue()
{
    messageQueue.Clear();
    AddLog(ActivityLogType.System, "Queue cleared");
}
```

#### 5.5 Test Result Capture
Track test outcomes:
```csharp
public record TestResult
{
    public string Prompt { get; init; }
    public bool Success { get; init; }
    public string Response { get; init; }
    public TimeSpan Duration { get; init; }
}

private List<TestResult> testResults = new();
```

#### 5.6 Configurable Delays
Allow users to adjust delay between prompts:
```razor
<input type="number" @bind="delayBetweenPrompts" min="0" max="5000" step="100" />
<span>ms delay between prompts</span>
```

#### 5.7 Export Test Results
Generate markdown or JSON report of test execution:
```csharp
private string GenerateTestReport()
{
    var sb = new StringBuilder();
    sb.AppendLine("# Test Execution Report");
    sb.AppendLine($"Date: {DateTime.Now}");
    // ... format results
    return sb.ToString();
}
```

---

## Future Vision (Post Phase 5)

### High-Value Future Enhancements

**1. Test Suite Viewer Page** 📊
- Visual catalog showing all 13+ test scenarios
- Expandable sections by domain (3D, 2D, Models)
- Each sequence shows its full prompt list
- "Hey buddy, this is my test suite" - complete visibility
- Quick run buttons, management UI
- Central documentation of capabilities

**2. Structured Output Verification** ✅
- Assert expected outcomes after each prompt
- "Yeah, we did change that shape to red" validation
- "Yeah, that height is 25" verification
- Query scene state, compare actual vs expected
- Pass/fail reporting
- **Enables true automated regression testing**

Example:
```csharp
await SendPrompt("Create a red box with height 25");
Assert.That(scene.GetShape().Color, Is.EqualTo("red"));
Assert.That(scene.GetShape().Height, Is.EqualTo(25));
```

**Benefits**:
- Confidence that operations work correctly
- Catch regressions automatically
- Validate tool implementations
- Performance benchmarking
- Production-ready test automation

---

## Testing Strategy

### Unit Tests
- `ChatTestScenarios.GetAllSequences()` returns all sequences
- `ChatTestScenarios.GetSequence(name)` returns correct sequence
- Metadata attributes are correctly extracted

### Integration Tests
- Queue processes messages in order
- Single message bypasses queue complexity
- Test sequences load correctly
- UI disables during processing

### Manual Testing Checklist
- [ ] Single message sends and receives response
- [ ] Test sequence loads all prompts
- [ ] Queue processes prompts in order
- [ ] Delay between prompts is visible
- [ ] UI disables during test execution
- [ ] Can clear chat during/after test
- [ ] Activity log shows queue operations
- [ ] Multiple test sequences can run consecutively
- [ ] Error in middle of sequence doesn't block queue

---

## Migration Considerations

### Backward Compatibility
- Existing chat functionality must work unchanged
- Queue is transparent for single messages
- No breaking changes to existing APIs

### Performance
- Queue processing is async and non-blocking
- Delays are configurable
- Large queues don't freeze UI

### Error Handling
- Errors in one prompt don't stop queue
- Option to stop on error vs. continue
- Cl✅ Created Files (Phase 1)
```
✅ Models/Chat/TestSequenceMetadata.cs
✅ Services/Chat/ChatTestScenarios.cs
```

### To Be Created (Phase 2)
```
⏳ Components/Shared/Chat/TestSequenceSelector.razor
⏳ Components/Shared/Chat/TestSequenceSelector.razor.cs
```

### To Be Modified (Phase 2 & 3)
```
⏳ Components/Pages/ChatOrchestratorTest.razor (add selector to header)
⏳ Components/Pages/ChatOrchestratorTest.razor.cs (add queue logic)
⏳ wwwroot/css/chat-components.css (add selector styles)
```

### No Changes Required (Services Stay Clean)
```
✅ Services/Chat/ChatOrchestrator.cs (unchanged)
✅ Services/Chat/IMultiProviderChatService.cs (unchanged)
✅ All agent implementations (unchanged)
✅ All other components (unchanged)

### Phase 2
- ✅ Component renders and 

| Phase | Status | Time Estimate | Risk Level |
|-------|--------|---------------|------------|
| Phase 1: Test Infrastructure | ✅ COMPLETE | ~1 hour | Low |
| Phase 2: Selector Component | ⏳ NEXT | 1-2 hours | Low |
| Phase 3: Queue Integration | ⏳ PENDING | 2-3 hours | Low |
| Phase 4: Other Pages | 🔮 DEFERRED | 3-4 hours | Low |
| Phase 5: Enhanced Features | 🔮 OPTIONAL | 4-8 hours | Low |
| **Core Implementation** | | **3-5 hours** | |

**Current Progress**: Phase 1 complete (✅), ready to proceed with Phase 2 & 3
### Phase 4
- ✅ All chat pages support test automation
- ✅ Consistent behavior across pages

### Phase 5
- ✅ Enhanced features working as designed
- ✅ User documentation complete

---

## File Summary
---

## Decision Points for Phase 2 & 3

### Confirmed Decisions
✅ **Queue location**: Page-level, not service-level  
✅ **Delay between prompts**: 500ms default  
✅ **Component placement**: ChatOrchestratorTest header  
✅ **Test organization**: Three domains (3D, 2D, Models)  
✅ **Tests are accumulative**: Operations build on each other  

### Pending Decisions
❓ **Error handling**: Stop on error vs. continue through sequence?  
❓ **Queue cancellation**: Should user be able to cancel mid-execution?  
❓ **Visual feedback**: Just disable UI or show "X of N prompts remaining"?  

**Recommended Defaults**:
- Continue on error (log it, keep going)
- No cancellation in initial version (keep it simple)
- Simple disable UI (can enhance later)

### Future Capabilities (Phase 5+)
🔮 **Test Suite Viewer**: Visual catalog of all test scenarios with inline prompts  
🔮 **Verification System**: Assert expected outcomes (color=red, height=25, etc.)  
🔮 **This enables true automated regression testing** ✅

---

## Approval Required

**Ready to proceed with Phase 2 & 3**:
1. ✅ Phase 1 complete - test infrastructure ready
2. Create TestSequenceSelector component (1-2 hours)
3. Integrate queue management into ChatOrchestratorTest (2-3 hours)
4. Validate single messages and test sequences both work

**Future enhancements identified**:
- Test Suite Viewer page (visualize all tests)
- Structured output verification (validate results)

**Total estimated time**: 3-5 hours  
**Risk level**: Low (all additive changes, no service modifications)

**Awaiting approval to begin Phase 2 implementation.**
Components/Pages/ChatOrchestratorTest.razor.cs
Components/Pages/AgentCanvasIntegration.razor (Phase 4)
Components/Pages/AgentCanvasIntegration.razor.cs (Phase 4)
wwwroot/css/chat-components.css
```

### No Changes Required
```
Services/Chat/ChatOrchestrator.cs
Services/Chat/IMultiProviderChatService.cs
All agent implementations
```

---

## Implementation Timeline Estimate

| Phase | Estimated Time | Risk Level |
|-------|---------------|------------|
| Phase 1 | 2-3 hours | Low |
| Phase 2 | 2-3 hours | Low |
| Phase 3 | 4-6 hours | Medium |
| Phase 4 | 3-4 hours | Low |
| Phase 5 | 4-8 hours | Low |
| **Total** | **15-24 hours** | |

---

## Questions for Implementation

1. Should queue processing be cancellable mid-execution?
2. What should the default delay between prompts be? (suggested: 500ms)
3. Should errors in a sequence stop execution or continue?
4. Do we want test results captured and exportable?
5. Should test sequences be saveable/loadable from files?

---

## Conclusion

This specification provides a phased, low-risk approach to implementing queue-based test automation in the chat system. The core innovation is treating all message processing through a unified queue, making test automation a natural extension rather than a separate system.
