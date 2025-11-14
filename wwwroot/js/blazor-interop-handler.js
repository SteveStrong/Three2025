// Minimal Blazor Error Suppression
// Only handles specific known non-critical errors without wrapping Blazor internals

(function () {
  "use strict";

  console.log("Loading minimal error handler...");

  // Only handle unhandled promise rejections for specific non-critical cases
  window.addEventListener("unhandledrejection", function (event) {
    if (event.reason && event.reason.message) {
      const message = event.reason.message;

      // List of known non-critical errors that can be safely suppressed
      const nonCriticalErrors = [
        "TriggerAnimationFrame",
        "LoadedObjectComplete",
      ];

      // Check if this is a known non-critical error
      const isNonCritical = nonCriticalErrors.some((error) =>
        message.includes(error)
      );

      if (isNonCritical) {
        console.debug("Suppressed non-critical error:", message);
        event.preventDefault();
      }
    }
  });

  console.log("Minimal error handler loaded");
})();
