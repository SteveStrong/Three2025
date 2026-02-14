# Level 3: Knowledge Modeling Components

## Overview  
Full-capability MxObject components using **FoundryMicroCore.Library + FoundryWorldsAndDrawings + FoundryMentorModeler**. Adds parametric knowledge modeling to 3D visualization.

## Dependencies Required
```xml
<PackageReference Include="FoundryMicroCore.Library" Version="1.0.0" />
<PackageReference Include="FoundryWorldsAndDrawings" Version="1.0.0" />
<PackageReference Include="FoundryMentorModeler" Version="1.0.0" />
```

## Capabilities (Level 2 +)
- ✅ KN model integration (AnimatedKnModel, etc.)
- ✅ Parametric component creation (PartComponent)
- ✅ Rule evaluation and mentor services
- ✅ Model editing capabilities (IModelEditor)
- ✅ Knowledge-driven 3D generation
- ✅ Component parameter management
- ✅ Workspace integration patterns

## Example Coming Soon: ParametricClock

### Planned: ParametricClockDemo
**Purpose**: Demonstrates knowledge-driven parametric 3D modeling

**Features to Demonstrate**:
- KN model parameter binding
- Rule-based clock hand sizing
- Parametric clock face generation  
- Mentor-guided configuration
- Model evaluation and updates
- Knowledge persistence

## Key Patterns for Level 3

### KN Model Integration
```csharp
public class ParametricComponent : PartComponent
{
    // KN parameters drive 3D geometry
    private AnimatedKnModel _knModel;
    
    protected override void EstablishGeometry3D()
    {
        // Generate 3D based on KN parameters
    }
}
```

### Mentor Services  
```csharp
[Inject] public IMentorServices MentorServices { get; set; }
[Inject] public IModelEditor ModelEditor { get; set; }

// Use mentor for guided workflows
```

### Parameter Evaluation
```csharp
private void OnParameterChanged(string paramName, object value)
{
    // Re-evaluate dependent parameters
    // Update 3D geometry based on new values
    ComputeShape3D();
}
```

## Use Cases
- Parametric 3D design tools
- Knowledge-driven assemblies
- Mentor-guided modeling workflows  
- Rule-based component generators
- Educational parametric modeling
- Expert system integration

## Skills Available (Planned)
- **KN_Model_Integration.md** - AnimatedKnModel + MxObject patterns
- **Parametric_Component.md** - PartComponent inheritance  
- **Rule_Evaluation.md** - Mentor services integration
- **Parameter_Binding.md** - KN ↔ 3D synchronization
- **Knowledge_Persistence.md** - Model saving and loading

## Migration Requirements
Must have working Level 2 (3D) components first:
- ✅ Canvas3D integration working
- ✅ Stage management established  
- ✅ 3D shapes rendering correctly
- ✅ Animation system functional

Then add FoundryMentorModeler:
- Add KN model parameters
- Implement rule evaluation
- Bind parameters to 3D geometry
- Add mentor service integration

## Success Criteria
- ✅ All Level 2 criteria met (3D visualization)
- ✅ KN models create and evaluate successfully
- ✅ Parameters drive 3D geometry updates
- ✅ Mentor services integrate cleanly
- ✅ Rule evaluation affects visual output
- ✅ Knowledge persistence works correctly
- ✅ Performance remains acceptable with KN overhead

## When to Use Level 3
Choose Level 3 when you need:
- **Parametric modeling** - User-adjustable parameters that drive geometry
- **Rule-based design** - Expert knowledge embedded in components  
- **Guided workflows** - Mentor system helps users make decisions
- **Complex assemblies** - Multiple interrelated parametric components
- **Educational tools** - Teaching parametric design concepts

## When NOT to Use Level 3
Stick with Level 2 if you only need:
- Static 3D visualization
- Simple interactive 3D scenes
- Performance-critical applications
- Minimal dependency requirements
- Fixed geometry without parameters

**Level 3 is the most powerful but also most complex option.**