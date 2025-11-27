// Blazor JavaScript Interop Handler
// This file provides bridge functions for Blazor-JavaScript communication

window.blazorInterop = {
    // Log function for debugging
    log: function (message) {
        console.log('Blazor Interop:', message);
    },

    // Get element by ID
    getElementById: function (id) {
        return document.getElementById(id);
    },

    // Set element text content
    setTextContent: function (elementId, text) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = text;
            return true;
        }
        return false;
    },

    // Focus element
    focusElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
            return true;
        }
        return false;
    },

    // Get window dimensions
    getWindowDimensions: function () {
        return {
            width: window.innerWidth,
            height: window.innerHeight
        };
    },

    // Add CSS class
    addClass: function (elementId, className) {
        const element = document.getElementById(elementId);
        if (element) {
            element.classList.add(className);
            return true;
        }
        return false;
    },

    // Remove CSS class
    removeClass: function (elementId, className) {
        const element = document.getElementById(elementId);
        if (element) {
            element.classList.remove(className);
            return true;
        }
        return false;
    },

    // Download file
    downloadFileFromStream: async function (fileName, contentStreamReference) {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        const anchorElement = document.createElement('a');
        anchorElement.href = url;
        anchorElement.download = fileName ?? '';
        anchorElement.click();
        anchorElement.remove();
        URL.revokeObjectURL(url);
    }
};

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('?? Blazor Interop Handler loaded and ready');
});