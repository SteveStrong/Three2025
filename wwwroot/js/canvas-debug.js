// Canvas debugging script
window.debugCanvas = function () {
  console.log("=== Canvas Debug Info ===");

  // Check if Three.js is loaded
  if (typeof THREE !== "undefined") {
    console.log("Three.js is loaded, version:", THREE.REVISION);
  } else {
    console.log("Three.js is NOT loaded");
  }

  // Check for canvas elements
  const canvasElements = document.querySelectorAll("canvas");
  console.log("Canvas elements found:", canvasElements.length);

  canvasElements.forEach((canvas, index) => {
    console.log(`Canvas ${index}:`, {
      width: canvas.width,
      height: canvas.height,
      style: canvas.style.cssText,
      id: canvas.id,
      className: canvas.className,
    });

    // Check WebGL context
    try {
      const gl =
        canvas.getContext("webgl") || canvas.getContext("experimental-webgl");
      if (gl) {
        console.log(`Canvas ${index} WebGL context: OK`);
      } else {
        console.log(`Canvas ${index} WebGL context: FAILED`);
      }
    } catch (e) {
      console.log(`Canvas ${index} WebGL context error:`, e);
    }
  });

  // Check for app-lib functions
  if (window.FoundryWorldsAndDrawings) {
    console.log("FoundryWorldsAndDrawings namespace is available");

    if (window.FoundryWorldsAndDrawings.Initialize3DViewer) {
      const isStub =
        window.FoundryWorldsAndDrawings.Initialize3DViewer.toString().includes(
          "stub called"
        );
      console.log(
        "Initialize3DViewer function is available:",
        isStub ? "STUB" : "REAL"
      );
    } else {
      console.log("Initialize3DViewer function is NOT available");
    }

    if (window.FoundryWorldsAndDrawings.createCanvas3D) {
      const isStub = window.FoundryWorldsAndDrawings.createCanvas3D
        .toString()
        .includes("stub called");
      console.log(
        "FoundryWorldsAndDrawings.createCanvas3D is available:",
        isStub ? "STUB" : "REAL"
      );
    }
  } else {
    console.log("FoundryWorldsAndDrawings namespace is NOT available");
  }

  if (window.createCanvas3D) {
    const isStub = window.createCanvas3D.toString().includes("stub called");
    console.log(
      "Global createCanvas3D function is available:",
      isStub ? "STUB" : "REAL"
    );
  } else {
    console.log("Global createCanvas3D function is NOT available");
  }

  console.log("=== End Canvas Debug ===");

  // Also run the namespace checker if available
  if (typeof window.checkFoundryNamespace === "function") {
    window.checkFoundryNamespace();
  }
};

// Auto-run debug after page load
window.addEventListener("load", function () {
  setTimeout(window.debugCanvas, 1000);
});

// Listen for when the real namespace becomes ready
document.addEventListener("foundryNamespaceReady", function () {
  console.log("FoundryWorldsAndDrawings namespace is now ready!");
  // Re-run debug to show updated status
  setTimeout(window.debugCanvas, 500);
});
