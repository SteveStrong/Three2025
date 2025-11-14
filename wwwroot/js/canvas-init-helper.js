// Canvas3D Initialization Helper
// Ensures consistent canvas rendering across all pages

window.ensureCanvas3DReady = async function (maxAttempts = 20, delayMs = 100) {
  console.log("Ensuring Canvas3D is ready...");

  for (let i = 0; i < maxAttempts; i++) {
    // Check if namespace is available
    if (
      window.FoundryWorldsAndDrawings &&
      typeof window.FoundryWorldsAndDrawings.Initialize3DViewer === "function"
    ) {
      // Check if it's not a stub
      const isReal =
        !window.FoundryWorldsAndDrawings.Initialize3DViewer.toString().includes(
          "stub called"
        );

      if (isReal) {
        console.log("Canvas3D is ready!");

        // Give it one more moment for Three.js to initialize
        await new Promise((resolve) => setTimeout(resolve, 200));

        // Dispatch ready event
        document.dispatchEvent(new CustomEvent("canvas3DReady"));
        return true;
      }
    }

    // Wait before next attempt
    await new Promise((resolve) => setTimeout(resolve, delayMs));
  }

  console.warn("Canvas3D initialization timeout - using stubs");
  return false;
};

// Helper to verify canvas is rendering
window.verifyCanvasRendering = function () {
  const canvases = document.querySelectorAll("canvas");
  console.log(`Found ${canvases.length} canvas elements`);

  canvases.forEach((canvas, index) => {
    const hasContext = !!(
      canvas.getContext("webgl") || canvas.getContext("webgl2")
    );
    const hasContent = canvas.width > 0 && canvas.height > 0;

    console.log(`Canvas ${index}:`, {
      id: canvas.id || "no-id",
      size: `${canvas.width}x${canvas.height}`,
      hasWebGL: hasContext,
      hasContent: hasContent,
      style: canvas.style.cssText || "no-style",
    });
  });
};

// Auto-verify on page load
document.addEventListener("DOMContentLoaded", async function () {
  console.log("Page loaded, ensuring Canvas3D...");
  await window.ensureCanvas3DReady();

  // Wait a bit for Blazor components to render
  setTimeout(() => {
    window.verifyCanvasRendering();
  }, 1000);
});

console.log("Canvas3D initialization helper loaded");
