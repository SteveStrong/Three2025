# MxObject Architecture Levels

## Overview
The specifications are organized by dependency complexity and capability requirements. Each level builds upon the previous one.

## Level 1: MxObject Foundation
**Dependencies**: FoundryMicroCore.Library only
**Capabilities**: Basic MxObject patterns, logging, lifecycle management
**Use Cases**: Simple components, utilities, non-3D interfaces

### What You Get:
- Clean MxObject inheritance patterns
- Modern logging system (WriteSuccess/Info/Warning/Error)
- Proper disposal and resource management  
- Component lifecycle management
- Basic Blazor component integration

### Examples:
- Simple form components
- Data display components  
- Configuration panels
- Utility classes

## Level 2: 3D Visualization
**Dependencies**: FoundryMicroCore.Library + FoundryWorldsAndDrawings
**Capabilities**: 3D scenes, shapes, models, animation, Canvas3D integration
**Use Cases**: 3D visualization, interactive scenes, model viewers

### What You Get (Level 1 +):
- Canvas3DComponent integration
- FoShape3D, FoText3D, FoModel3D creation
- Stage management and scene integration
- Transform3 positioning and animation
- 3D asset loading (GLB models)
- Animation frame synchronization

### Examples:
- **ClockDemo** (interactive 3D clock with shapes)
- 3D model viewers
- Interactive 3D scenes
- Visualization dashboards

## Level 3: Knowledge Modeling
**Dependencies**: FoundryMicroCore.Library + FoundryWorldsAndDrawings + FoundryMentorModeler  
**Capabilities**: KN models, parametric components, rule evaluation, mentor integration
**Use Cases**: Parametric 3D modeling, knowledge-driven design, mentor-guided workflows

### What You Get (Level 2 +):
- KN model integration (AnimatedKnModel, etc.)
- Parametric component creation
- Rule evaluation and mentor services
- Model editing capabilities
- Knowledge-driven 3D generation
- Component parameter management

### Examples:
- Parametric shape generators
- Knowledge-driven assemblies
- Mentor-guided modeling workflows
- Rule-based component creation

## Choosing Your Level

### Start with Level 1 if:
- Building simple UI components
- No 3D visualization needed
- Learning MxObject patterns
- Minimal dependencies preferred

### Use Level 2 if:
- Need 3D visualization ✅ **ClockDemo fits here**
- Interactive 3D scenes required
- 3D model loading needed
- Animation and transforms needed

### Use Level 3 if:  
- Parametric modeling required
- Knowledge-based generation needed
- Mentor integration required
- Rule evaluation needed

## Migration Path

```
Level 1: MxObject Foundation
    ↓ (Add FoundryWorldsAndDrawings)
Level 2: 3D Visualization  
    ↓ (Add FoundryMentorModeler)
Level 3: Knowledge Modeling
```

Each level is designed to be:
- ✅ **Self-contained** - Complete working examples at each level
- ✅ **Incremental** - Build upon previous capabilities  
- ✅ **Focused** - Clear dependency boundaries
- ✅ **Testable** - Isolated validation at each level

## Folder Structure

```
Levels/
├── Level1-MxObject/           # FoundryMicroCore.Library only
│   ├── Examples/
│   ├── Skills/
│   └── Templates/
├── Level2-3D/                 # + FoundryWorldsAndDrawings  
│   ├── Examples/
│   │   └── ClockDemo/         # ClockDemo belongs here
│   ├── Skills/
│   └── Assets/
└── Level3-KnModel/            # + FoundryMentorModeler
    ├── Examples/
    ├── Skills/
    └── Templates/
```

This organization ensures:
- **Clear dependency requirements** for each project type
- **Appropriate complexity level** for specific use cases  
- **Clean separation** of concerns and capabilities
- **Easier adoption** - start simple, add complexity as needed