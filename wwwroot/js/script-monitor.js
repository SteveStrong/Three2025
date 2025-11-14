// Enhanced script loading detection
(function () {
  "use strict";

  // Add script load monitoring
  const originalAppendChild = Node.prototype.appendChild;
  Node.prototype.appendChild = function (child) {
    if (
      child.tagName === "SCRIPT" &&
      child.src &&
      child.src.includes("app-lib.js")
    ) {
      console.log("Detected app-lib.js script being added:", child.src);

      child.addEventListener("load", function () {
        console.log("app-lib.js successfully loaded");
        // Give it a moment to initialize
        setTimeout(() => {
          if (
            typeof window.FoundryWorldsAndDrawings?.Initialize3DViewer ===
              "function" &&
            !window.FoundryWorldsAndDrawings.Initialize3DViewer.toString().includes(
              "stub called"
            )
          ) {
            console.log("Real Initialize3DViewer implementation confirmed");
            // Trigger any pending canvas initializations
            document.dispatchEvent(new CustomEvent("foundryNamespaceReady"));
          }
        }, 100);
      });

      child.addEventListener("error", function () {
        console.error("Failed to load app-lib.js:", child.src);
        console.warn("Canvas3D will continue using stub implementations");
      });
    }
    return originalAppendChild.call(this, child);
  };
})();
