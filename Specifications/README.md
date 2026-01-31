# MxObject Migration Pattern Specifications

## Overview
Specifications organized by **dependency complexity levels** for generating clean MxObject components. **Designed for LLM code generation - NOT for copying/fixing existing files.**

## Architecture Levels

### [Level 1: MxObject Foundation](Levels/Level1-MxObject/)
**Dependencies**: FoundryMicroCore.Library only  
**Use Cases**: Simple components, utilities, non-3D interfaces

### [Level 2: 3D Visualization](Levels/Level2-3D/) ⭐  
**Dependencies**: FoundryMicroCore.Library + FoundryWorldsAndDrawings  
**Use Cases**: 3D scenes, interactive visualization (**ClockDemo fits here**)

### [Level 3: Knowledge Modeling](Levels/Level3-KnModel/)
**Dependencies**: FoundryMicroCore.Library + FoundryWorldsAndDrawings + FoundryMentorModeler  
**Use Cases**: Parametric modeling, rule-based design, mentor integration

## 🎯 **Intended Workflow:**
1. **Choose appropriate level** based on dependency requirements
2. **Copy relevant level folder** to target Blazor app
3. **Use LLM to generate fresh code** from level specifications  
4. **Never copy existing broken files** - generate clean implementations
5. **Validate against success criteria** in level documentation
6. **Iterate specs and regenerate** as needed

## Available Specifications

### Core Documentation
- **[Levels/README.md](Levels/README.md)** - 📚 **Architecture levels overview**
- **[CLOCKDEMO_COMPONENT_SPECIFICATION.md](CLOCKDEMO_COMPONENT_SPECIFICATION.md)** - 📋 **Comprehensive ClockDemo specification**  
- **[LLM_Generation_Instructions.md](LLM_Generation_Instructions.md)** - 🚀 **Optimized for LLM code generation**
- **[Skills/](Skills/)** - Individual reusable patterns and templates

## Usage Guidelines

### For LLM Code Generation (Recommended)
1. **Choose your level**: Level1 (basic), Level2 (3D), or Level3 (KN modeling)
2. **Copy appropriate level folder** to target project
3. **Use level README and examples** as LLM input
4. **Generate fresh code** - never copy existing broken files
5. **Validate output** against level success criteria
6. **Iterate and regenerate** until requirements met

### Level Selection Guide
- **Level 1**: Simple UI, no 3D, learning MxObject patterns
- **Level 2**: 3D visualization needed ✅ **Most projects start here**  
- **Level 3**: Parametric modeling, rule-based design needed

### For Migration
1. Choose components similar to existing skills
2. Extract component to standalone project first
3. Apply MxObject patterns incrementally
4. Test thoroughly before integration

### For Sharing
1. Skills are designed to be self-contained
2. Copy individual .md files for sharing  
3. Include Prerequisites section for context
4. Reference FoundryMicroCore documentation as needed

## Skill Template Format

Each skill follows this structure:

```markdown
# Skill Title

## Skill Overview
**Purpose**: What you'll accomplish
**Difficulty**: Beginner/Intermediate/Advanced  
**Prerequisites**: Required knowledge/components
**Output**: What you'll have when done

## What You'll Learn
- Key patterns and concepts

## Step-by-Step Implementation
Detailed instructions with code examples

## Key Patterns Demonstrated  
Important patterns with explanations

## Testing Checklist
Validation steps

## Common Issues & Solutions
Troubleshooting guide

## Next Steps
How to extend and improve
```

## Contributing New Skills

When creating new skills:

1. **Follow the template format** for consistency
2. **Include complete code examples** that can be copied
3. **Add testing checklist** for validation
4. **Document common issues** you encountered
5. **Keep skills focused** on one specific pattern or component

## Folder Structure

```
Specifications/
├── CLOCKDEMO_COMPONENT_SPECIFICATION.md  # Comprehensive detailed specification
├── LLM_Generation_Instructions.md        # Optimized for LLM code generation  
├── Skills/                              # Individual reusable patterns
│   ├── Basic_Component_Template.md
│   └── ...
├── Levels/                              # Organized by dependency complexity
│   ├── README.md                        # Architecture levels overview
│   ├── Level1-MxObject/                 # FoundryMicroCore.Library only
│   │   └── README.md
│   ├── Level2-3D/                       # + FoundryWorldsAndDrawings
│   │   ├── README.md  
│   │   ├── Examples/
│   │   │   └── ClockDemo/               # ⭐ ClockDemo reference
│   │   │       ├── ClockDemo.razor.txt
│   │   │       ├── ClockDemo.razor.cs.txt
│   │   │       └── README.md
│   │   └── Assets/
│   │       └── models/                  # 3D model placeholders
│   └── Level3-KnModel/                  # + FoundryMentorModeler
│       └── README.md
└── README.md                            # This file
```

This approach makes the specifications:
- ✅ **LLM-Optimized** - Clear generation instructions and constraints
- ✅ **Shareable** - Individual .md files can be copied/sent
- ✅ **Focused** - Each skill teaches specific patterns  
- ✅ **Reusable** - Apply to different components
- ✅ **Testable** - Clear validation criteria
- ✅ **Educational** - Step-by-step learning progression
- ✅ **Clean** - No technical debt from broken legacy code