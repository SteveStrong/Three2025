# Service Debug Helper Pattern Guide

## Overview

This document defines the standardized pattern for all Service Debug Helper classes in the ReadyAI.Blazor project. This pattern eliminates code duplication, provides clean inheritance architecture, and ensures consistent behavior across all service debug operations.

## The Problem We Solved

**Before**: Each helper class contained ~150-200 lines with massive duplication:
- Static classes preventing inheritance
- Complex try/catch blocks duplicated across every method
- Manual JSON serialization logic repeated everywhere
- ComponentBus publishing code duplicated in every method
- Error handling logic duplicated between helpers and button panels

**After**: Clean inheritance-based architecture with ~30-40 lines per helper:
- Instance classes inheriting from `BaseServiceDebugHelper`
- ContextWrapper handles all error states internally
- Common functionality centralized in base class
- Simple 4-line pattern per method

## Architecture Overview

```
BaseServiceDebugHelper (Abstract Base)
├── ComponentBus management
├── JSON serialization options
├── PublishMessage() method
└── Common infrastructure

├── AgentServiceDebugHelper : BaseServiceDebugHelper
├── DocumentServiceDebugHelper : BaseServiceDebugHelper  
├── ModuleServiceDebugHelper : BaseServiceDebugHelper
├── ScenarioServiceDebugHelper : BaseServiceDebugHelper (needs refactoring)
├── SystemServiceDebugHelper : BaseServiceDebugHelper (needs refactoring)
└── UserServiceDebugHelper : BaseServiceDebugHelper (needs refactoring)
```

## BaseServiceDebugHelper Reference

```csharp
public class BaseServiceDebugHelper
{
    protected ComponentBus? _componentBus;
    
    public BaseServiceDebugHelper(ComponentBus? componentBus = null)
    {
        _componentBus = componentBus;
    }

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    protected void PublishMessage(string serviceName, string methodName, string json)
    {
        _componentBus?.Publish(new ServiceDebugResultMessage
        {
            ServiceName = serviceName,
            MethodName = methodName,
            Json = json,
            Timestamp = DateTime.Now,
        });
    }
}
```

## The Golden Pattern Template

### Step 1: Class Declaration
```csharp
public class [ServiceName]ServiceDebugHelper : BaseServiceDebugHelper
{
    private readonly I[ServiceName]Service _[serviceName]Service;
    
    public [ServiceName]ServiceDebugHelper(I[ServiceName]Service [serviceName]Service, ComponentBus? componentBus) : base(componentBus)
    {
        _[serviceName]Service = [serviceName]Service;
    }
```

### Step 2: Method Pattern (4 Lines Each)
```csharp
public async Task<ContextWrapper<[ReturnType]>> [MethodName]Async([parameters])
{
    var result = await _[serviceName]Service.[MethodName]Async([parameters]);
    var json = CodingExtensions.DehydrateWrapper<[ReturnType]>(result, true);
    PublishMessage("[ServiceName]Service", "[MethodName]", json);
    return result;
}
```

## Real-World Examples

### AgentServiceDebugHelper (Perfect Example)
```csharp
public class AgentServiceDebugHelper : BaseServiceDebugHelper
{
    private readonly IAgentService _agentService;
    
    public AgentServiceDebugHelper(IAgentService agentService, ComponentBus? componentBus) : base(componentBus)
    {
        _agentService = agentService;
    }

    public async Task<ContextWrapper<AgentDto>> GetAllAgentsAsync() 
    {
        var result = await _agentService.GetAllAgentsAsync();
        var json = CodingExtensions.DehydrateWrapper<AgentDto>(result, true);
        PublishMessage("AgentService", "GetAllAgentsAsync", json);
        return result;
    }

    public async Task<ContextWrapper<AgentDto>> GetAgentByIdAsync(string agentId)
    {  
        var result = await _agentService.GetAgentByIdAsync(agentId);
        var json = CodingExtensions.DehydrateWrapper<AgentDto>(result, true);
        PublishMessage("AgentService", "GetAgentByIdAsync", json);
        return result;
    }
}
```

### DocumentServiceDebugHelper (Perfect Example)
```csharp
public class DocumentServiceDebugHelper : BaseServiceDebugHelper
{
    private readonly IDocumentService _documentService;
    
    public DocumentServiceDebugHelper(IDocumentService documentService, ComponentBus? componentBus) : base(componentBus)
    {
        _documentService = documentService;
    }

    public async Task<ContextWrapper<DocumentDto>> GetAllDocumentsAsync(string userId)
    {
        var result = await _documentService.GetAllDocumentsAsync(userId);
        var json = CodingExtensions.DehydrateWrapper<DocumentDto>(result, true);
        PublishMessage("DocumentService", "GetAllDocumentsAsync", json);
        return result;
    }
}
```

## Refactoring Checklist

When converting an existing helper class:

### ✅ Class Structure Changes
- [ ] Remove `static` keyword from class declaration
- [ ] Add `: BaseServiceDebugHelper` inheritance
- [ ] Remove `JsonSerializerOptions` field (inherited from base)
- [ ] Add private readonly service field: `private readonly I[Service] _[service]Service;`
- [ ] Add constructor with DI: `public [Helper](I[Service] service, ComponentBus? componentBus) : base(componentBus)`

### ✅ Method Refactoring (Per Method)
- [ ] Remove `static` keyword
- [ ] Remove service parameter (use injected field instead)
- [ ] Remove `componentBus` parameter (use inherited field instead)
- [ ] Remove entire `try/catch` block
- [ ] Remove all logging statements (`WriteInfo`, `WriteSuccess`, `WriteError`)
- [ ] Remove manual `JsonSerializer.Serialize` calls
- [ ] Remove manual `componentBus?.Publish` calls
- [ ] Replace with 4-line pattern:
  1. `var result = await _service.MethodAsync(params);`
  2. `var json = CodingExtensions.DehydrateWrapper<T>(result, true);`
  3. `PublishMessage("ServiceName", "MethodName", json);`
  4. `return result;`

### ✅ Common Mistakes to Avoid
- ❌ Don't wrap `IEnumerable<T>` in ContextWrapper - use `ContextWrapper<T>` where T is the item type
- ❌ Don't use `JsonSerializer.Serialize` - use `CodingExtensions.DehydrateWrapper`
- ❌ Don't manually publish to ComponentBus - use `PublishMessage`
- ❌ Don't add try/catch blocks - ContextWrapper handles all error states
- ❌ Don't add logging - keep methods clean and simple

## Classes That Need Refactoring

### 🔧 ScenarioServiceDebugHelper
- **Current State**: Static class with duplicated error handling
- **Estimated Lines Saved**: ~150-180 lines
- **Pattern**: Follow DocumentServiceDebugHelper example

### 🔧 SystemServiceDebugHelper  
- **Current State**: Static class with duplicated error handling
- **Estimated Lines Saved**: ~120-150 lines
- **Pattern**: Follow AgentServiceDebugHelper example

### 🔧 UserServiceDebugHelper
- **Current State**: Static class with duplicated error handling
- **Estimated Lines Saved**: ~150-180 lines
- **Pattern**: Follow DocumentServiceDebugHelper example

## Creating New Helper Classes

When creating a new service debug helper:

1. **Copy the template** from AgentServiceDebugHelper
2. **Replace placeholders**:
   - `Agent` → Your service name
   - `IAgentService` → Your service interface
   - `AgentDto` → Your return type
3. **Add methods** following the 4-line pattern
4. **Register in DI** (see Dependency Injection section)

## Dependency Injection Registration

All helper classes must be registered in `Program.cs`:

```csharp
// Service Debug Helpers (Instance-based with inheritance)
builder.Services.AddScoped<AgentServiceDebugHelper>();
builder.Services.AddScoped<DocumentServiceDebugHelper>();
builder.Services.AddScoped<ModuleServiceDebugHelper>();
builder.Services.AddScoped<ScenarioServiceDebugHelper>(); // After refactoring
builder.Services.AddScoped<SystemServiceDebugHelper>();   // After refactoring
builder.Services.AddScoped<UserServiceDebugHelper>();     // After refactoring
```

## Button Panel Integration

Button panels should inject the helper and call methods directly:

```csharp
@inject AgentServiceDebugHelper AgentHelper

private async Task HandleGetAllAgents()
{
    var result = await AgentHelper.GetAllAgentsAsync();
    // No error handling needed - ContextWrapper and helper handle everything
}
```

## Benefits of This Pattern

### ✅ Code Reduction
- **Before**: ~900+ lines across 6 helpers
- **After**: ~200 lines across 6 helpers  
- **Savings**: ~700 lines eliminated (77% reduction)

### ✅ Maintainability
- Single source of truth for ComponentBus logic
- Single source of truth for JSON serialization
- Error handling centralized in ContextWrapper
- Easy to add new service methods
- Consistent behavior across all services

### ✅ Testability
- Helper classes can be easily mocked
- Dependency injection enables proper unit testing
- Clear separation of concerns

### ✅ Scalability
- Adding new service debug helpers takes ~10 minutes
- Pattern is well-established and documented
- New developers can follow template exactly

## Testing Pattern

Each helper should have corresponding tests:

```csharp
[Test]
public async Task GetAllAgentsAsync_ShouldReturnResult_AndPublishMessage()
{
    // Arrange
    var mockService = new Mock<IAgentService>();
    var mockBus = new Mock<ComponentBus>();
    var helper = new AgentServiceDebugHelper(mockService.Object, mockBus.Object);
    
    // Act
    var result = await helper.GetAllAgentsAsync();
    
    // Assert
    mockService.Verify(s => s.GetAllAgentsAsync(), Times.Once);
    mockBus.Verify(b => b.Publish(It.IsAny<ServiceDebugResultMessage>()), Times.Once);
}
```

## Key Principles

1. **ContextWrapper is King**: Trust it to handle all error states
2. **Keep Methods Simple**: 4 lines maximum per method
3. **No Error Handling**: Let ContextWrapper and ComponentBus handle it
4. **Consistent Naming**: Service name + "ServiceDebugHelper"
5. **Single Responsibility**: Each helper handles one service only
6. **Inheritance Over Duplication**: Always inherit from BaseServiceDebugHelper

---

## Next Steps

1. Refactor `ScenarioServiceDebugHelper` following this guide
2. Refactor `SystemServiceDebugHelper` following this guide  
3. Refactor `UserServiceDebugHelper` following this guide
4. Update button panels to use dependency injection
5. Add unit tests for all helper classes
6. Remove any remaining static helper references

This pattern ensures consistency, maintainability, and eliminates hundreds of lines of duplicated code while providing a clear path forward for future development.