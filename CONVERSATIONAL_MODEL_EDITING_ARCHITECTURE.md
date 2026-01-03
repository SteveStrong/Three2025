# Conversational Design Synthesis Architecture

**Created**: January 2, 2026  
**Vision**: AI agents as intelligent design partners that take structured notes, build calculable models, and synthesize designs through natural conversation  
**Foundation**: SysML 2 modeling paradigms + Object-Oriented programming + Spreadsheet-like calculations  

---

## 🎯 Scope & Focus - Core Knowledge Modeling

**CURRENT IMPLEMENTATION SCOPE**: This architecture focuses on **core knowledge modeling tools** that work directly with the knowledge model layer (KnModel, KnComponent, KnParameter). 

### What We're Building Now ✅
- **ModelTech Tools**: Core function calling API for establish_model(), add_component(), set_parameter() 
- **Knowledge Modeling Agent**: AI specialized for creating structured, calculable models
- **Instance-Focused Workflow**: Models → Component Instances → Parameters → Formulas

### What We're NOT Including Yet ⚠️
- **Extended Mentor 2D Shape Vocabulary**: The broader Mentor 2D system includes many additional shape types, visual modeling features, and advanced diagramming capabilities
- **Visual Shape Operations**: Advanced shape manipulation, layout, visual styling (these have separate APIs)
- **Extended Knowledge Model**: Full knowledge engineering with relationships, contexts, roles, traits (future scope)

**Key Point**: Mentor 2D has a much richer shape vocabulary and visual modeling system. For now, we're deliberately limiting scope to the core knowledge modeling tools that AI agents can use effectively. The extended knowledge model and full visual capabilities will be added in future iterations.

This focused approach ensures we build robust AI-driven model creation first, then expand to the full Mentor 2D capability set.

---

## Core Mission: AI as Design Partner & Memory System

Engineers and domain experts need AI agents that can **listen, learn, and synthesize** during design conversations. The AI must:

1. **Take Structured Notes** → Convert conversational knowledge into calculable parameters and formulas
2. **Build Living Models** → Create SysML 2-inspired object hierarchies with dependencies  
3. **Maintain Design Memory** → Short-term (conversation context) + Long-term (design patterns)
4. **Synthesize Solutions** → Combine conversational knowledge into coherent designs
5. **Enable Calculation** → Spreadsheet-like formula evaluation during conversation

### The Engineer-AI Design Partnership

```
Engineer: "I'm working on a HVAC system for a 3-story office building. 
          Each floor is 10,000 sq ft with 8-foot ceilings."

AI Memory System:
├─ 📝 Takes Notes → Creates BuildingModel with parameters
├─ 🧮 Calculates → Volume = 10,000 × 8 × 3 = 240,000 ft³
├─ 🎯 Synthesizes → Applies HVAC sizing knowledge 
├─ 💾 Remembers → Stores pattern for future office buildings
└─ 🔄 Evolves → Updates design as conversation continues

Result: Living, calculable model that grows with the conversation
```

### Core Philosophy: AI Memory Through Structured Models

```
Human: "The motor needs to handle 50 HP at 1800 RPM with 92% efficiency"
    ↓ AI Note-Taking Process ↓
1. EstablishModel("MotorSpecification") // Create design memory container
2. AddComponent("ElectricalMotor") // Object-oriented structure  
3. SetParameter("Power", "50 HP") // Quantified knowledge
4. SetParameter("Speed", "1800 RPM") // Operating conditions
5. SetParameter("Efficiency", "0.92") // Performance targets
6. SetParameter("Torque", "Power@ * 5252 / Speed@") // SysML 2 formula dependency
    ↓ Result: Living Design Memory ↓
Knowledge Model updates continuously as conversation evolves
AI can reference, calculate, and build upon previous statements
Design synthesis emerges from accumulated conversational knowledge
```

---

## Architecture: Conversational Design Intelligence

```
┌─────────────────────────────────────────────────────────────────┐
│                 ConversationalModeler                           │
│            AI Design Partner & Memory System                    │
├──────────────────────────────┬──────────────────────────────────┤
│   CONVERSATION (50%)         │   DESIGN MEMORY (50%)            │
│                              │                                  │
│  [Engineering Test Cases ▼]  │   📊 Note-Taking Activity Log    │
│  ┌─────────────────────────┐ │   ┌────────────────────────────┐ │
│  │ 👨‍💼 Engineer Expertise   │ │   │ ✅ Captured: Motor 50HP    │ │
│  │                         │ │   │ 🧮 Calculated: Torque      │ │
│  │ Human: "HVAC system     │ │   │ 📝 Noted: Efficiency req   │ │
│  │ for 50,000 sq ft"       │ │   │ 🎯 Pattern: Industrial     │ │
│  │                         │ │   │ 💾 Stored: Design memory   │ │
│  │ 🤖 AI Design Partner:   │ │   └────────────────────────────┘ │
│  │ "I'll create a model    │ │                                  │
│  │ with zones, load calc,  │ │   📁 SysML 2 Object Model       │
│  │ and equipment sizing"   │ │   ⊞ HVACSystem                  │
│  │                         │ │     ⊞ Zone1 (Component)        │
│  │ ✅ Created HVACSystem   │ │       • Area|ft²: 10000        │ │
│  │ ✅ Added cooling zones  │ │       • LoadPerSqFt|BTU: 25    │ │
│  │ 🧮 Calculated loads     │ │       • CoolingLoad|BTU:       │ │
│  │ 📝 Applied design rules │ │         Area@ * LoadPerSqFt@   │ │
│  └─────────────────────────┘ │     ⊞ Equipment (PartInstance) │ │
│                              │       • Capacity|Tons: ...     │ │
│  ┌─────────────────────────┐ │                                  │
│  │ 💬 "What about the      │ │   🧠 AI Memory Systems           │
│  │ electrical load?"       │ │   • Short-term: Conversation    │
│  │                    [📤] │ │   • Long-term: Design patterns  │
│  └─────────────────────────┘ │   • Synthesis: Active learning  │
└──────────────────────────────┴──────────────────────────────────┘
                    [Resizable RadzenSplitter]
```

---

## Architecture: Two-Panel Conversational Interface

```
┌─────────────────────────────────────────────────────────────────┐
│                    ConversationalModeler                        │
├──────────────────────────────┬──────────────────────────────────┤
│   CHAT PANEL (50%)           │   MODEL EXPLORER (50%)           │
│                              │                                  │
│  [Test Sequence Dropdown ▼]  │   📊 Activity Log               │
│  ┌─────────────────────────┐ │   ┌────────────────────────────┐ │
│  │ Conversation History    │ │   │ ✅ Model 'Beam1' created   │ │
│  │                         │ │   │ ℹ️ Component 'beam' added  │ │
│  │ User: Create a beam     │ │   │ ✅ Parameter L = 10 ft     │ │
│  │ with 500 lb load        │ │   │ ⚠️ Computing deflection... │ │
│  │                         │ │   │ ✅ δ = 0.154 in           │ │
│  │ AI: I'll create a       │ │   └────────────────────────────┘ │
│  │ structural model...     │ │                                  │
│  │                         │ │   📁 Model Tree                 │
│  │ ✅ Created BeamModel    │ │   ⊞ BeamModel                   │
│  │ ✅ Added SteelBeam      │ │     ⊞ SteelBeam                 │
│  │ ✅ Set Length: 10 ft    │ │       • Length: 10 ft           │
│  │ ✅ Set Load: 500 lb     │ │       • Load: 500 lb            │
│  │ ✅ Deflection: 0.154 in │ │       • E: 29000000 psi         │
│  └─────────────────────────┘ │       • I: 10.9 in⁴            │
│                              │       • deflection: 0.154 in   │
│  [💬] Type your message...  │                                  │
│                         [📤] │   🔧 Parameters                 │
│                              │   • deflection|in: (Load@ *... │
│                              │                                 │
└──────────────────────────────┴──────────────────────────────────┘
                    [Resizable RadzenSplitter]
```

---

## Implementation Stack: SysML 2 + OOP + Calculations

### 1. ConversationalModeler (Conversation Interface)
**Location**: `Three2025/Components/Pages/ConversationalModeler.razor`

The AI design partner interface for engineering conversations:

```csharp
@inject IChatOrchestrator ChatOrchestrator  // Multi-agent conversation system
@inject IModelTech ModelTech                // SysML 2 modeling API
@inject IMentorServices MentorServices      // Knowledge model services

// Engineer-AI conversation with real-time design synthesis
<RadzenSplitter Orientation="Horizontal" style="height: 100vh;">
    <RadzenSplitterPane Size="50%" Min="25%" Max="75%">
        <ChatPanel @bind-Messages="ChatMessages" 
                   OnSendMessage="ProcessEngineerInput"
                   HeaderContent="@GetEngineeringTestCases()" />
    </RadzenSplitterPane>
    
    <RadzenSplitterPane Size="50%" Min="25%" Max="75%">
        <!-- Living Design Memory with SysML 2 hierarchy -->
        <MentorTreeView Models="@MentorServices.GetAllModels()" 
                        ShowCalculatedParameters="true" />
    </RadzenSplitterPane>
</RadzenSplitter>
```

### 2. ModelTech (SysML 2 Note-Taking API)
**Location**: `Three2025/Apprentice/ModelTech.cs`

AI-driven structured note-taking with SysML 2 paradigms:

```csharp
[Description("SysML 2 modeling tools for AI note-taking and design synthesis")]
public class ModelTech : IModelTech
{
    [AgentTool("establish_model")]
    [Description("Create design memory container for conversational knowledge")]
    public KnModel EstablishModel(string designContext, string? modelType = null)
    // ↳ Creates SysML 2-style model as conversation memory

    [AgentTool("add_component")]  
    [Description("Add system component with object-oriented structure")]
    public KnComponent AddComponent(string componentName, string? parentPath = null)
    // ↳ Builds hierarchical object model from conversation

    [AgentTool("set_parameter")]
    [Description("Capture quantified knowledge with spreadsheet-like formulas")]
    public KnParameter SetParameter(string paramName, string valueOrFormula, string? componentPath = null)
    // ↳ Enables "Area@ * LoadPerSqFt@" dependency calculations
    
    [AgentTool("create_engineering_system")]
    [Description("Apply domain knowledge patterns to synthesize system designs")]
    public string CreateEngineeringSystem(string systemType, string[] keyParameters)
    // ↳ Leverages AI's engineering knowledge for design synthesis
}
```

### 3. PartComponent Calculations (Spreadsheet Engine)
**Location**: FoundryMentorModeler library

Declarative calculations syntax for design formulas:

```csharp
var hvacZone = new PartComponent("CoolingZone") 
{
    Calculations = """
        Area|ft²: 10000
        LoadPerSqFt|BTU/hr/ft²: 25  
        OccupancyLoad|BTU/hr: 500
        LightingLoad|BTU/hr: Area@ * 3
        EquipmentLoad|BTU/hr: Area@ * 2
        TotalLoad|BTU/hr: LoadPerSqFt@ * Area@ + OccupancyLoad@ + LightingLoad@ + EquipmentLoad@
        RequiredCapacity|Tons: TotalLoad@ / 12000
        """
};
// AI can build these through SetParameter() calls
// Dependencies auto-update as conversation evolves
```
```

### 4. ChatOrchestrator (AI Memory & Synthesis Engine)
**Location**: `Three2025/Services/IChatOrchestrator.cs`

Multi-agent conversation system with design pattern memory:

```csharp
// AI agents work together as design partners
var designAgents = new[]
{
    new StructuralAgent(),      // Knows beam sizing, structural analysis
    new HVACAgent(),           // Knows load calculations, equipment sizing  
    new ElectricalAgent(),     // Knows power calculations, motor specs
    new GeneralAgent()         // Synthesizes cross-domain knowledge
};

// Function calling tools automatically generated from ModelTech [AgentTool] methods
var conversationalTools = new[]
{
    new Tool("establish_model", "Create design memory container"),
    new Tool("add_component", "Add system component with OOP structure"),
    new Tool("set_parameter", "Capture quantified knowledge with formulas"),
    new Tool("create_engineering_system", "Apply domain patterns for synthesis")
};

// AI memory systems
public class AIMemorySystem
{
    public ConversationMemory ShortTerm;    // Current design conversation
    public DesignPatternMemory LongTerm;    // Accumulated engineering knowledge  
    public SynthesisEngine ActiveLearning;  // Combines patterns for new designs
}
```

---

## SysML 2 Knowledge Application

The AI applies SysML 2 modeling principles to structure conversational knowledge:

### Object-Oriented Hierarchies
```
SystemModel (Package)
├─ Subsystem1 (Component)
│  ├─ SubComponent1A (PartInstance) 
│  │  ├─ Parameter: Value|Unit
│  │  └─ Parameter: Formula|Unit
│  └─ SubComponent1B (PartInstance)
└─ Subsystem2 (Component)
   └─ Interface connections...
```

### Dependency Modeling
AI creates formula dependencies using SysML 2 constraint blocks:
```
CoolingLoad|BTU: Area@ * LoadPerSqFt@ + LatentLoad@
RequiredCapacity|Tons: CoolingLoad@ / 12000  
CompressorPower|kW: RequiredCapacity@ * EER@
ElectricalLoad|Amps: CompressorPower@ * 1000 / (Voltage@ * PowerFactor@)
```

### Parametric Analysis
Real-time "what-if" analysis as conversation evolves:
```
Engineer: "What if we increase the building area to 15,000 sq ft?"
AI: Updates Area@ parameter → All dependent calculations auto-update
    → Cooling load increases → Equipment sizing changes → Cost impact shown
```

---

## Design Synthesis Goals

The ultimate vision is AI agents that can:

1. **Listen & Learn**: Extract structured knowledge from engineering conversations
2. **Calculate & Validate**: Apply engineering formulas and check feasibility  
3. **Remember & Apply**: Use design patterns from previous conversations
4. **Synthesize & Propose**: Generate complete designs from conversational inputs
5. **Evolve & Improve**: Learn better design patterns through accumulated experience

### Example Synthesis Workflow:
```
Engineer: "Design a pump system for a 20-story building"

AI Synthesis Process:
├─ 📚 Recalls: Previous high-rise HVAC projects
├─ 🧮 Calculates: Static head = 20 floors × 12 ft = 240 ft
├─ 🔍 Applies: Pump sizing rules from knowledge base
├─ 🏗️ Structures: Creates hierarchical system model
├─ 📊 Validates: Checks against code requirements
├─ 💡 Proposes: "I recommend variable speed pumps with..."
└─ 📝 Documents: Updates design pattern memory

Result: Complete pump system design with specifications,
        calculations, and justifications - all from conversation
```

---

## Success Pattern: Mentor 2D Editor → Model Editor

We successfully implemented this pattern with the **Mentor 2D Editor**:

1. **Chat Interface** → User types "Create a pump system" 
2. **AI Function Calling** → Calls `Mentor2DTech.CreateConceptShape("Pump")`
3. **Visual Canvas** → Shape appears on 2D canvas
4. **Event System** → `DrawingEditChanged` events trigger UI refresh
5. **User Feedback** → Tree view updates, activity log shows progress

Now we're applying the same pattern to **direct model manipulation**:

1. **Chat Interface** → User types "Create a beam with 10 ft span"
2. **AI Function Calling** → Calls `ModelTech.AddComponent("Beam")` + `ModelTech.SetParameter("Length", "10 ft")`
3. **Knowledge Model** → Component created with parameter calculations
4. **Event System** → `ModelEditChanged` events trigger UI refresh  
5. **User Feedback** → Tree view updates, activity log shows progress, calculations appear

---

## PartComponent Calculations Syntax

For engineering models, we use declarative **Calculations** syntax:

```csharp
var beam = new PartComponent("SteelBeam")
{
    Calculations = """
        Length|ft: 10
        Load|lb: 500  
        E|psi: 29000000
        I|in^4: 10.9
        deflection|in: (Load@ * Length@^3) / (3 * E@ * I@)
        """
};
```

The AI can create these through `ModelTech.SetParameter()` calls:
- `ModelTech.SetParameter("Length", "10 ft")`
- `ModelTech.SetParameter("deflection", "(Load@ * Length@^3) / (3 * E@ * I@)")`

---

## Event-Driven UI Refresh

The ModelEditor publishes events that automatically refresh the UI:

```csharp
// ModelEditor publishes events
ModelChanged(ModelEditChanged.ChildAdded(parent, component));      // Tree view refreshes
ModelChanged(ModelEditChanged.ParameterChanged(component, param)); // Parameter panel updates

// MentorModelManager subscribes and triggers UI refresh
private void OnModelChanged(ModelEditChanged message)
{
    PubSub.Publish<RefreshRenderMessage>(RefreshRenderMessage.Refresh(message.Selections));
}
```

This creates the **"watch your model build itself"** experience where:
- AI makes function calls through conversation
- Knowledge models update in real-time  
- Tree views refresh automatically
- Activity logs show AI progress
- Engineering calculations appear live

---

## Development Status

✅ **Complete**: 
- ConversationalModeler UI rebuilt (2-panel layout)
- ModelTech interface with comprehensive Description attributes
- ModelEditor service with event publishing
- Dependency injection registration
- Activity logging with color-coded entries

🔄 **In Progress**:
- ChatOrchestrator integration with ModelTech function calling
- Test sequence selector with engineering prompts
- PartComponent Calculations integration

🎯 **Next Steps**:
- Define function calling tools from ModelTech methods
- Create engineering test prompts for beam/truss/thermal analysis  
- Integrate visual shape creation with knowledge model construction
- Add undo/redo support for conversational edits

---

## Conclusion

This architecture enables **natural language knowledge model construction** by combining:
- **Rich conversational interface** with engineering test prompts
- **AI function calling** through well-documented ModelTech API
- **Event-driven model updates** with automatic UI refresh
- **Real-time calculations** using PartComponent syntax
- **Visual feedback** through activity logs and model trees

The proven success with Mentor 2D Editor gives us confidence that this direct model editing approach will provide an even more powerful conversational modeling experience.