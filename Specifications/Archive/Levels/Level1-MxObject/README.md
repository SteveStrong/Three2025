# Level 1: MxObject Foundation Components

## Overview
Basic MxObject patterns using **FoundryMicroCore.Library only**. No 3D visualization or knowledge modeling dependencies.

## Dependencies Required
```xml
<PackageReference Include="FoundryMicroCore.Library" Version="1.0.0" />
```

## Capabilities
- ✅ Clean MxObject inheritance (MxComponent base classes)
- ✅ Modern logging system (WriteSuccess/Info/Warning/Error)
- ✅ Proper disposal and resource management
- ✅ Component lifecycle management
- ✅ Blazor component integration patterns

## Use Cases
- Simple form components and data entry
- Configuration panels and settings UI
- Data display and formatting components
- Utility classes and helper components
- Learning MxObject patterns without complexity

## Examples Coming Soon

### BasicComponent
- Minimal MxObject Blazor component
- Demonstrates core lifecycle patterns
- Template for new simple components

### DataDisplay  
- Shows MxObject data binding patterns
- Demonstrates logging integration
- Clean disposal patterns

### ConfigPanel
- Configuration management with MxObject
- Settings persistence patterns
- Validation and error handling

## Skills Available
- **Basic_Component_Template.md** - Minimal MxObject component pattern
- **Logging_Integration.md** - Modern logging patterns
- **Lifecycle_Management.md** - Proper disposal and cleanup
- **Data_Binding_Patterns.md** - MxObject property patterns

## Templates
- **MxComponent_Template.cs** - Basic MxComponent class
- **MxBlazorComponent_Template.razor** - Basic Blazor + MxObject
- **MxBlazorComponent_Template.razor.cs** - Code-behind patterns

## Success Criteria
- ✅ Zero compilation errors with FoundryMicroCore.Library only
- ✅ Clean MxComponent inheritance demonstrated  
- ✅ Modern logging throughout (.WriteSuccess(), etc.)
- ✅ Proper disposal in all components
- ✅ No external dependencies beyond FoundryMicroCore.Library

## Migration from Legacy
Perfect starting point for:
- Converting simple UI components
- Learning MxObject patterns safely
- Building confidence before adding complexity
- Creating utility components and helpers

Use Level 1 components as building blocks before moving to 3D (Level 2) or Knowledge Modeling (Level 3).