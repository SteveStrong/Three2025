# FoundryWorldsAndDrawings JavaScript Integration Guide

## Overview

This document provides a comprehensive guide for future Copilot agents to understand and fix JavaScript integration issues between the Three2025 Blazor Server application and the FoundryWorldsAndDrawings Razor class library. While the JavaScript implementation may be heavily refactored in the future, the core integration principles remain consistent.

## Architecture Understanding

### Core Components
- **Three2025**: Main Blazor Server application
- **FoundryWorldsAndDrawings**: Referenced Razor class library containing Canvas3D components
- **JavaScript Bridge**: Client-side JavaScript that enables C# to JavaScript interop for 3D rendering

### Key Integration Points
1. **Static Web Assets**: JavaScript files served from the referenced library
2. **Namespace Creation**: Global JavaScript namespace for interop functions
3. **Component Lifecycle**: Blazor component initialization timing with JavaScript availability
4. **Interop Calls**: C# IJSRuntime calls to JavaScript functions

## Common Integration Issues and Solutions

### Issue 1: JavaScript Namespace Undefined

**Symptoms:**
```
Error: "FoundryWorldsAndDrawings was undefined"
```

**Root Cause:**
JavaScript namespace not available when C# components attempt interop calls.

**Solution Pattern:**
```javascript
// Create namespace shim that loads before the main library
(function() {
    'use strict';
    
    // Ensure namespace exists
    if (typeof window.FoundryWorldsAndDrawings === 'undefined') {
        window.FoundryWorldsAndDrawings = {};
    }
    
    // Create stub functions for common interop calls
    const commonFunctions = ['Initialize3DViewer', 'createCanvas3D', 'disposeCanvas3D'];
    
    commonFunctions.forEach(function(functionName) {
        if (typeof window.FoundryWorldsAndDrawings[functionName] !== 'function') {
            window.FoundryWorldsAndDrawings[functionName] = function() {
                // Queue calls for replay when real implementation loads
                if (!window.FoundryWorldsAndDrawings._pendingCalls) {
                    window.FoundryWorldsAndDrawings._pendingCalls = [];
                }
                window.FoundryWorldsAndDrawings._pendingCalls.push([functionName, Array.from(arguments)]);
            };
        }
    });
})();
```

### Issue 2: Script Loading Order Problems

**Symptoms:**
```
- Functions called before script loads
- Intermittent "function not defined" errors
- Canvas3D components failing to initialize
```

**Solution Pattern:**
```html
<!-- Critical: Load namespace shim BEFORE main library -->
<script src="js/namespace-shim.js"></script>
<script src="_content/FoundryWorldsAndDrawings/js/app-lib.js"></script>
```

### Issue 3: Component Initialization Timing

**Symptoms:**
```
- Canvas elements created but not initialized
- 3D scenes not rendering
- JavaScript interop calls failing silently
```

**Solution Pattern:**
```javascript
// Event-driven initialization
document.addEventListener('foundryNamespaceReady', function() {
    // Trigger component re-initialization
    document.dispatchEvent(new CustomEvent('canvasReinitialize'));
});
```

### Issue 4: Blazor Callback Promise Rejections

**Symptoms:**
```
- Uncaught (in promise) Error: invoking 'TriggerAnimationFrame'
- Uncaught (in promise) Error: invoking 'LoadedObjectComplete'
- Promise rejections in browser console
```

**Root Cause:**
Blazor SignalR circuit disconnections or component disposal during async JavaScript callbacks.

**Solution Pattern:**
```javascript
// Wrap DotNet.invokeMethodAsync with error handling
const originalInvokeMethodAsync = window.DotNet.invokeMethodAsync;
window.DotNet.invokeMethodAsync = function(assemblyName, methodName, ...args) {
    const promise = originalInvokeMethodAsync.call(this, assemblyName, methodName, ...args);
    return promise.catch(error => {
        console.warn(`Callback failed: ${methodName}`, error);
        return Promise.resolve(null); // Prevent uncaught rejection
    });
};
```

## Implementation Strategy

### 1. Namespace Shim Pattern

**File**: `wwwroot/js/foundry-namespace-shim.js`

**Purpose**: Ensures JavaScript namespace exists before any components attempt to use it.

**Key Features:**
- Creates namespace immediately on load
- Provides stub functions that queue calls
- Replays queued calls when real implementation loads
- Provides diagnostic functions for troubleshooting

```javascript
// Template for namespace shim
(function() {
    'use strict';
    
    // Namespace creation
    window.FoundryWorldsAndDrawings = window.FoundryWorldsAndDrawings || {};
    
    // Function stubbing with call queuing
    // Real implementation replacement mechanism
    // Diagnostic and monitoring capabilities
})();
```

### 2. Script Loading Monitoring

**File**: `wwwroot/js/script-monitor.js`

**Purpose**: Monitors when the main JavaScript library loads and notifies components.

**Key Features:**
- Intercepts script loading to detect library availability
- Dispatches events when real implementation is ready
- Provides error handling for failed script loads
- Offers debugging information about script status

```javascript
// Template for script monitoring
(function() {
    'use strict';
    
    // Monitor script additions to DOM
    // Detect when main library loads
    // Dispatch readiness events
    // Handle load failures gracefully
})();
```

### 3. Enhanced Debugging

**File**: `wwwroot/js/canvas-debug.js`

**Purpose**: Provides comprehensive debugging for Canvas3D integration issues.

**Key Features:**
- Checks namespace availability and function status
- Validates Canvas element creation and WebGL contexts
- Monitors Three.js library loading
- Provides manual diagnostic functions

```javascript
// Template for debugging
window.debugCanvas = function() {
    // Check JavaScript library status
    // Validate Canvas elements and WebGL
    // Report namespace and function availability
    // Provide actionable troubleshooting information
};
```

## Critical File Locations

### Application Files (Three2025)
- `Components/App.razor` - Script loading order configuration
- `Components/_Imports.razor` - Component namespace imports
- `Components/Pages/*.razor.cs` - Component lifecycle and Canvas3D usage
- `Program.cs` - Service configuration and static file serving
- `Three2025.csproj` - Project references to FoundryWorldsAndDrawings

### JavaScript Files (Three2025/wwwroot/js/)
- `blazor-interop-handler.js` - Blazor callback error handling
- `foundry-namespace-shim.js` - Namespace availability guarantee
- `script-monitor.js` - Library loading detection
- `canvas-debug.js` - Integration diagnostics

### Library Files (FoundryWorldsAndDrawings - External)
- `JsLib/src/index.ts` - Main JavaScript entry point
- `wwwroot/js/app-lib.js` - Compiled JavaScript library
- `FoundryWorldsAndDrawings.csproj` - Static web asset configuration
- `Shared/Canvas3DComponent.razor.cs` - Main 3D component implementation
- `ThreeD/Viewers/ViewerThreeD.cs` - JavaScript interop calls

## Diagnostic Commands

### Browser Console Commands
```javascript
// Check namespace status
window.checkFoundryNamespace();

// Run canvas diagnostics
window.debugCanvas();

// Monitor for readiness events
document.addEventListener('foundryNamespaceReady', () => console.log('Ready!'));

// Check script loading status
document.querySelectorAll('script[src*="app-lib.js"]').forEach(s => console.log(s.src, s.readyState));
```

### PowerShell Commands
```powershell
# Build JavaScript library (if source available)
cd FoundryWorldsAndDrawings\JsLib
npm run build

# Start application
cd Three2025
dotnet run

# Test endpoint
# Navigate to: http://localhost:5228/Home
```

## Static Web Asset Configuration

### Library Configuration (FoundryWorldsAndDrawings.csproj)
```xml
<PropertyGroup>
  <StaticWebAssetBasePath>_content/FoundryWorldsAndDrawings</StaticWebAssetBasePath>
  <PackageId>FoundryWorldsAndDrawings</PackageId>
</PropertyGroup>
```

### Application Script References (App.razor)
```html
<!-- Must match StaticWebAssetBasePath -->
<script src="_content/FoundryWorldsAndDrawings/js/app-lib.js"></script>
```

## Event-Driven Integration Pattern

### Custom Events for Coordination
```javascript
// Namespace ready
document.dispatchEvent(new CustomEvent('foundryNamespaceReady'));

// Canvas reinitialization
document.dispatchEvent(new CustomEvent('canvasReinitialize'));

// Library load failure
document.dispatchEvent(new CustomEvent('foundryLibraryFailure', { detail: error }));
```

### Component Response Pattern
```csharp
// C# component listening for JavaScript readiness
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Wait for JavaScript namespace to be ready before making interop calls
        await JSRuntime.InvokeVoidAsync("waitForFoundryNamespace");
    }
}
```

## Troubleshooting Checklist

### When Canvas3D Components Don't Render
1. **Check Browser Console** - Look for JavaScript errors and namespace availability
2. **Verify Script Loading** - Ensure app-lib.js loads successfully
3. **Validate Paths** - Confirm static web asset paths match library configuration
4. **Test Interop** - Use browser console to manually test JavaScript functions
5. **Check Component Lifecycle** - Verify OnAfterRenderAsync timing
6. **Examine Network Tab** - Look for 404 errors on JavaScript files

### When JavaScript Namespace is Undefined
1. **Script Order** - Ensure namespace shim loads before main library
2. **Static Assets** - Verify FoundryWorldsAndDrawings project builds and serves files
3. **Project References** - Confirm Three2025.csproj references FoundryWorldsAndDrawings
4. **Service Registration** - Check Program.cs for AddFoundryWorldsAndDrawingsServices call
5. **Build Process** - Ensure JavaScript compilation completed successfully

## Future Refactoring Considerations

### JavaScript Library Changes
- **Namespace Structure**: May change but shim pattern remains applicable
- **Function Names**: Update shim to include new function names
- **Module Loading**: Consider ES6 modules vs global namespace approach
- **Build Process**: Webpack/TypeScript compilation may change output structure

### Blazor Component Evolution
- **Component Hierarchy**: Canvas3DComponent inheritance may change
- **Lifecycle Methods**: OnAfterRenderAsync timing may need adjustment
- **Interop Patterns**: IJSRuntime usage patterns may evolve
- **Service Dependencies**: DI container registration may change

### Integration Patterns That Should Persist
- **Namespace Availability**: Always ensure JavaScript namespace exists before use
- **Error Handling**: Graceful degradation when JavaScript fails to load
- **Event Coordination**: Event-driven architecture for timing coordination
- **Diagnostic Capabilities**: Rich debugging and troubleshooting information
- **Static Asset Management**: Proper configuration of library asset serving

## Success Criteria

After implementing fixes, verify:
1. **No Console Errors**: No JavaScript namespace undefined errors
2. **Canvas Rendering**: 3D scenes display properly on /Home endpoint
3. **Component Functionality**: All Canvas3D component methods work
4. **Graceful Degradation**: System handles JavaScript load failures
5. **Diagnostic Information**: Debug tools provide actionable information

## Key Learnings from Current Implementation

### What Worked Well
- **Namespace Shim Pattern**: Prevents "undefined" errors effectively
- **Call Queuing**: Allows components to make calls before library loads
- **Event-Driven Architecture**: Provides clean coordination between C# and JavaScript
- **Enhanced Debugging**: Rich diagnostic information speeds troubleshooting

### Common Pitfalls to Avoid
- **Hard Dependencies**: Don't assume JavaScript libraries will always load
- **Timing Assumptions**: Components must handle async JavaScript loading
- **Error Masking**: Stubs should provide clear warnings about missing functionality
- **Static Path Mismatches**: Script src paths must align with library configuration

### Best Practices for Future Implementation
- **Defensive Programming**: Always check for JavaScript availability before use
- **Progressive Enhancement**: Start with stubs, enhance with real functionality
- **Comprehensive Logging**: Track every step of the integration process
- **Event Coordination**: Use custom events to coordinate timing across components
- **Graceful Failure**: Provide meaningful fallbacks when JavaScript fails

This guide provides a foundation for understanding and fixing FoundryWorldsAndDrawings JavaScript integration issues, regardless of future refactoring efforts. The principles of defensive namespace management, event-driven coordination, and comprehensive diagnostics should remain applicable even as the underlying implementation evolves.