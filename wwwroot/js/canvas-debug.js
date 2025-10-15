// Canvas debugging script
window.debugCanvas = function() {
    console.log('=== Canvas Debug Info ===');
    
    // Check if Three.js is loaded
    if (typeof THREE !== 'undefined') {
        console.log('Three.js is loaded, version:', THREE.REVISION);
    } else {
        console.log('Three.js is NOT loaded');
    }
    
    // Check for canvas elements
    const canvasElements = document.querySelectorAll('canvas');
    console.log('Canvas elements found:', canvasElements.length);
    
    canvasElements.forEach((canvas, index) => {
        console.log(`Canvas ${index}:`, {
            width: canvas.width,
            height: canvas.height,
            style: canvas.style.cssText,
            id: canvas.id,
            className: canvas.className
        });
        
        // Check WebGL context
        try {
            const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
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
    if (window.createCanvas3D) {
        console.log('createCanvas3D function is available');
    } else {
        console.log('createCanvas3D function is NOT available');
    }
    
    console.log('=== End Canvas Debug ===');
};

// Auto-run debug after page load
window.addEventListener('load', function() {
    setTimeout(window.debugCanvas, 1000);
});