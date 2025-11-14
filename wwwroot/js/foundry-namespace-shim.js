// FoundryWorldsAndDrawings Namespace Shim
// This ensures the namespace exists before any components try to use it

(function () {
  "use strict";

  console.log("Loading FoundryWorldsAndDrawings namespace shim...");

  // Create the namespace if it doesn't exist
  if (typeof window.FoundryWorldsAndDrawings === "undefined") {
    window.FoundryWorldsAndDrawings = {};
    console.log("Created FoundryWorldsAndDrawings namespace");
  }

  // List of common functions that might be called
  const commonFunctions = [
    "Initialize3DViewer",
    "createCanvas3D",
    "disposeCanvas3D",
    "updateCanvas3D",
    "resizeCanvas3D",
  ];

  // Add stubs for common functions if they don't exist
  commonFunctions.forEach(function (functionName) {
    if (typeof window.FoundryWorldsAndDrawings[functionName] !== "function") {
      window.FoundryWorldsAndDrawings[functionName] = function () {
        console.log(functionName + " stub called with:", arguments);
        console.warn(
          functionName +
            " is using a stub - waiting for actual implementation to load"
        );

        // Try to queue this call for when the real function is available
        if (!window.FoundryWorldsAndDrawings._pendingCalls) {
          window.FoundryWorldsAndDrawings._pendingCalls = [];
        }
        window.FoundryWorldsAndDrawings._pendingCalls.push([
          functionName,
          Array.from(arguments),
        ]);
      };
      console.log("Created " + functionName + " stub");
    }

    // Also create global stubs for common patterns
    if (typeof window[functionName] !== "function") {
      window[functionName] = window.FoundryWorldsAndDrawings[functionName];
    }
  });

  // Function to replace stub with real implementation
  window.FoundryWorldsAndDrawings._replaceStub = function (
    functionName,
    realFunction
  ) {
    console.log("Replacing stub for", functionName);
    window.FoundryWorldsAndDrawings[functionName] = realFunction;

    // Process any pending calls
    if (window.FoundryWorldsAndDrawings._pendingCalls) {
      const pendingCalls = window.FoundryWorldsAndDrawings._pendingCalls.filter(
        (call) => call[0] === functionName
      );
      pendingCalls.forEach((call) => {
        console.log("Executing pending call for", functionName);
        realFunction.apply(this, call[1]);
      });

      // Remove processed calls
      window.FoundryWorldsAndDrawings._pendingCalls =
        window.FoundryWorldsAndDrawings._pendingCalls.filter(
          (call) => call[0] !== functionName
        );
    }
  };

  console.log("FoundryWorldsAndDrawings namespace shim loaded successfully");

  // Prevent WebGL context conflicts
  const originalGetContext = HTMLCanvasElement.prototype.getContext;
  HTMLCanvasElement.prototype.getContext = function (
    contextType,
    contextAttributes
  ) {
    // If canvas already has a context, return it if compatible
    if (this.__webglContext) {
      const existingType = this.__webglContextType;
      if (
        existingType === contextType ||
        (existingType === "webgl" && contextType === "experimental-webgl") ||
        (existingType === "experimental-webgl" && contextType === "webgl")
      ) {
        console.log(
          "Reusing existing WebGL context for canvas:",
          this.id || "unnamed"
        );
        return this.__webglContext;
      }
    }

    const context = originalGetContext.call(
      this,
      contextType,
      contextAttributes
    );

    // Cache WebGL contexts
    if (
      contextType === "webgl" ||
      contextType === "experimental-webgl" ||
      contextType === "webgl2"
    ) {
      this.__webglContext = context;
      this.__webglContextType = contextType;
    }

    return context;
  };
})();

// Monitor for when the real app-lib.js loads
document.addEventListener("DOMContentLoaded", function () {
  console.log("DOM loaded, checking for app-lib.js...");

  // Check every 100ms for up to 10 seconds if the real implementation has loaded
  let attempts = 0;
  const maxAttempts = 100;

  const checkInterval = setInterval(function () {
    attempts++;

    // Look for signs that the real implementation has loaded
    const scripts = document.querySelectorAll('script[src*="app-lib.js"]');
    if (scripts.length > 0) {
      console.log("Found app-lib.js script tag");

      // Check if the script has actually loaded and contains real functions
      if (
        typeof window.FoundryWorldsAndDrawings.Initialize3DViewer ===
          "function" &&
        window.FoundryWorldsAndDrawings.Initialize3DViewer.toString().indexOf(
          "stub called"
        ) === -1
      ) {
        console.log("Real FoundryWorldsAndDrawings implementation detected!");
        document.dispatchEvent(new CustomEvent("foundryNamespaceReady"));
        clearInterval(checkInterval);
        return;
      }
    }

    // Check if app-lib.js failed to load
    scripts.forEach(function (script) {
      script.addEventListener("error", function () {
        console.error("Failed to load app-lib.js from:", script.src);
        console.warn("Canvas3D functionality will use stubs only");
      });

      script.addEventListener("load", function () {
        console.log("app-lib.js script loaded from:", script.src);
      });
    });

    // Stop checking after max attempts
    if (attempts >= maxAttempts) {
      console.warn(
        "Stopped checking for real FoundryWorldsAndDrawings implementation after",
        maxAttempts * 100,
        "ms"
      );
      console.warn(
        "If Canvas3D components are not working, check that app-lib.js is being served correctly"
      );
      clearInterval(checkInterval);
    }
  }, 100);
});

// Add a global function to manually check namespace status
window.checkFoundryNamespace = function () {
  console.log("=== FoundryWorldsAndDrawings Namespace Status ===");
  console.log(
    "Namespace exists:",
    typeof window.FoundryWorldsAndDrawings !== "undefined"
  );

  if (window.FoundryWorldsAndDrawings) {
    console.log(
      "Available functions:",
      Object.keys(window.FoundryWorldsAndDrawings)
    );
    console.log(
      "Pending calls:",
      window.FoundryWorldsAndDrawings._pendingCalls
        ? window.FoundryWorldsAndDrawings._pendingCalls.length
        : 0
    );

    // Check if functions are stubs or real
    if (window.FoundryWorldsAndDrawings.Initialize3DViewer) {
      const isStub =
        window.FoundryWorldsAndDrawings.Initialize3DViewer.toString().includes(
          "stub called"
        );
      console.log("Initialize3DViewer is:", isStub ? "STUB" : "REAL");
    }
  }

  // Check script elements
  const scripts = document.querySelectorAll('script[src*="app-lib.js"]');
  console.log("app-lib.js script elements found:", scripts.length);
  scripts.forEach((script, index) => {
    console.log(`Script ${index}:`, {
      src: script.src,
      loaded: script.readyState || "unknown",
    });
  });

  // Check global functions
  console.log("Global createCanvas3D:", typeof window.createCanvas3D);
  console.log(
    "Namespace createCanvas3D:",
    typeof window.FoundryWorldsAndDrawings?.createCanvas3D
  );

  console.log("=== End Namespace Status ===");
};
