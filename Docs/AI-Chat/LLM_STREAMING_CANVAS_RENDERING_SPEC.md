# LLM Streaming Canvas Rendering Specification

## Problem Statement

Current implementation streams LLM responses through Blazor component hierarchy:
- LLM chunks → `AgentCanvasIntegration.razor.cs` → `streamingResponse` property
- Updates trigger `StateHasChanged()` → Blazor render cycle
- **Issue**: Blazor batches UI updates for performance, causing choppy/delayed rendering
- Users don't see smooth word-by-word streaming despite chunks arriving quickly

## Proposed Solution: Pub/Sub + Direct Canvas Rendering

Bypass Blazor's render cycle entirely by:
1. Publishing LLM chunks via `PubSub` mechanism (already in FoundryWorldsAndDrawings)
2. Canvas components subscribe and render text directly using WebGL/Canvas 2D
3. Zero HTML DOM involvement = instant, smooth rendering

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│ LLM Provider (GitHub/Bedrock/Ollama)                        │
│   └─> IChatClient.GetStreamingResponseAsync()              │
└────────────────────┬────────────────────────────────────────┘
                     │ chunks
                     ▼
┌─────────────────────────────────────────────────────────────┐
│ MultiProviderChatService                                    │
│   └─> PubSub.Publish("llm-stream-chunk", chunk)           │
└────────┬──────────────────────────────────┬─────────────────┘
         │                                   │
         ▼                                   ▼
┌─────────────────────┐          ┌─────────────────────────┐
│ 3D Canvas Component │          │ 2D Canvas Component     │
│ Subscribe + Render  │          │ Subscribe + Render      │
└─────────────────────┘          └─────────────────────────┘
         │                                   │
         ▼                                   ▼
   Three.js Rendering              Canvas 2D API Rendering
   (CSS3DRenderer or              (fillText + manual layout)
    TextSprite)
```

---

## Implementation Options

### Option 1: CSS3DRenderer Overlay (RECOMMENDED)

**Description**: Use Three.js CSS3DRenderer to position HTML elements in 3D space

**How It Works**:
```typescript
// Subscribe to streaming chunks
PubSub.Subscribe("llm-stream-chunk", (chunk: StreamChunk) => {
    // Update HTML element
    textElement.textContent += chunk.content;
    
    // Position in 3D space
    const css3dObject = new CSS3DObject(textElement);
    css3dObject.position.set(0, 5, 0); // Above scene
    scene.add(css3dObject);
});
```

**Advantages**:
- ✅ Real HTML - keeps markdown, links, formatting
- ✅ Text selectable and copyable
- ✅ Accessible (screen readers work)
- ✅ Smooth streaming (direct DOM manipulation)
- ✅ Can use CSS animations, transitions
- ✅ Positioned in 3D space using Three.js matrices

**Challenges**:
- CSS3D requires separate renderer alongside WebGLRenderer
- Need to manage HTML element lifecycle
- Performance with large text blocks

**Visual Concept**:
- Floating text panel above/beside the 3D scene
- Follows camera or fixed in world space
- Could attach to active shape being discussed
- Fade out when complete, transition to chat history

---

### Option 2: Three.js TextSprite

**Description**: Render text as billboard sprites (always face camera)

**How It Works**:
```typescript
// Create canvas texture for text
const canvas = document.createElement('canvas');
const ctx = canvas.getContext('2d');
ctx.font = '32px Arial';
ctx.fillText(streamingText, 0, 32);

// Create sprite with canvas texture
const texture = new THREE.CanvasTexture(canvas);
const spriteMaterial = new THREE.SpriteMaterial({ map: texture });
const sprite = new THREE.Sprite(spriteMaterial);
scene.add(sprite);

// Update on each chunk
PubSub.Subscribe("llm-stream-chunk", (chunk) => {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillText(streamingText + chunk.content, 0, 32);
    texture.needsUpdate = true;
});
```

**Advantages**:
- ✅ True 3D object in scene
- ✅ Can place near shapes being created
- ✅ Billboard effect (always readable)
- ✅ Fast rendering

**Challenges**:
- ❌ Manual text wrapping, layout, line breaks
- ❌ Not selectable/copyable
- ❌ Canvas text rendering can be blurry
- ❌ No markdown support
- ❌ Accessibility issues

---

### Option 3: 2D Canvas Subtitle Overlay

**Description**: Traditional 2D canvas overlay at bottom of screen (like video captions)

**How It Works**:
```typescript
// Get 2D canvas context
const canvas2D = document.getElementById('subtitleCanvas') as HTMLCanvasElement;
const ctx = canvas2D.getContext('2d');

PubSub.Subscribe("llm-stream-chunk", (chunk: StreamChunk) => {
    streamingText += chunk.content;
    
    // Clear and redraw
    ctx.clearRect(0, 0, canvas2D.width, canvas2D.height);
    ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
    ctx.fillRect(0, canvas2D.height - 100, canvas2D.width, 100);
    
    ctx.fillStyle = 'white';
    ctx.font = '18px Arial';
    ctx.fillText(streamingText, 20, canvas2D.height - 50);
});
```

**Advantages**:
- ✅ Very fast rendering
- ✅ Simple implementation
- ✅ Familiar UX (like video subtitles)
- ✅ No 3D complexity

**Challenges**:
- ❌ Not selectable/copyable
- ❌ Manual text wrapping needed
- ❌ No markdown support
- ❌ Accessibility issues
- ❌ Doesn't feel integrated with 3D scene

---

### Option 4: Hybrid Approach (Best of Both Worlds)

**Active streaming in canvas, completed messages in Blazor chat panel**

**Flow**:
1. **While streaming**: Render in 3D/canvas overlay (fast, smooth)
2. **On complete**: Fade out canvas text, add to traditional chat panel
3. **Result**: Smooth streaming + full featured history

```typescript
PubSub.Subscribe("llm-stream-chunk", (chunk: StreamChunk) => {
    if (chunk.isComplete) {
        // Fade out canvas text
        fadeOutCanvasText();
        
        // Signal Blazor to add to chat history
        PubSub.Publish("llm-complete", { fullText: streamingText, agent: chunk.agent });
    } else {
        // Continue rendering in canvas
        updateCanvasText(chunk.content);
    }
});
```

**Advantages**:
- ✅ Smooth streaming where it matters
- ✅ Full HTML/CSS/markdown for history
- ✅ Text remains copyable in history
- ✅ Cool visual transition effect

---

## Recommended Implementation: CSS3DRenderer Overlay

### Why This Option?
1. **Readability**: Real HTML with CSS styling
2. **Accessibility**: Screen readers, keyboard navigation
3. **Functionality**: Copy/paste, markdown rendering, links
4. **Performance**: Direct DOM updates (no Blazor)
5. **Visual Impact**: 3D positioning feels futuristic
6. **Context**: Can position near shapes being created

### Technical Requirements

**Dependencies**:
- Three.js CSS3DRenderer (already have Three.js)
- PubSub mechanism (already in FoundryWorldsAndDrawings)
- Markdown renderer (already using Markdig)

**New Components**:
1. `StreamingTextOverlay3D.ts` - TypeScript component
2. `LLMStreamPublisher.cs` - C# pub/sub wrapper
3. Modified `MultiProviderChatService.cs` - publish chunks

---

## Implementation Steps

### Phase 1: Pub/Sub Infrastructure (30 min)

**1. Create Stream Publishing Service**
```csharp
// Services/Chat/LLMStreamPublisher.cs
public class LLMStreamPublisher
{
    private readonly IPubSub _pubSub;
    
    public void PublishChunk(string content, string agentName, bool isComplete)
    {
        _pubSub.Publish("llm-stream-chunk", new {
            content,
            agentName,
            isComplete,
            timestamp = DateTime.UtcNow
        });
    }
}
```

**2. Modify MultiProviderChatService**
```csharp
// After collecting chunk
if (!string.IsNullOrEmpty(update.Text))
{
    chunks.Add(update.Text);
    
    // NEW: Publish to PubSub
    _streamPublisher.PublishChunk(update.Text, agentName: "General", isComplete: false);
}
```

### Phase 2: 3D Canvas Integration (1 hour)

**1. Create TypeScript Component**
```typescript
// JsLib/StreamingTextOverlay3D.ts
import * as THREE from 'three';
import { CSS3DRenderer, CSS3DObject } from 'three/examples/jsm/renderers/CSS3DRenderer';

export class StreamingTextOverlay3D {
    private css3dRenderer: CSS3DRenderer;
    private textElement: HTMLDivElement;
    private css3dObject: CSS3DObject;
    private streamingText: string = "";
    
    constructor(private scene: THREE.Scene, private camera: THREE.Camera) {
        this.initCSS3DRenderer();
        this.createTextElement();
        this.subscribeToPubSub();
    }
    
    private initCSS3DRenderer() {
        this.css3dRenderer = new CSS3DRenderer();
        this.css3dRenderer.setSize(window.innerWidth, window.innerHeight);
        this.css3dRenderer.domElement.style.position = 'absolute';
        this.css3dRenderer.domElement.style.top = '0';
        this.css3dRenderer.domElement.style.pointerEvents = 'none';
        document.body.appendChild(this.css3dRenderer.domElement);
    }
    
    private createTextElement() {
        this.textElement = document.createElement('div');
        this.textElement.style.width = '600px';
        this.textElement.style.padding = '20px';
        this.textElement.style.background = 'rgba(0, 0, 0, 0.8)';
        this.textElement.style.color = 'white';
        this.textElement.style.fontFamily = 'monospace';
        this.textElement.style.fontSize = '16px';
        this.textElement.style.borderRadius = '8px';
        this.textElement.style.pointerEvents = 'auto';
        
        this.css3dObject = new CSS3DObject(this.textElement);
        this.css3dObject.position.set(0, 5, 0); // Above origin
        this.scene.add(this.css3dObject);
    }
    
    private subscribeToPubSub() {
        // Use your existing PubSub mechanism
        (window as any).foundryPubSub?.subscribe('llm-stream-chunk', (data: any) => {
            this.onChunkReceived(data);
        });
    }
    
    private onChunkReceived(chunk: { content: string, agentName: string, isComplete: boolean }) {
        if (chunk.isComplete) {
            this.fadeOutAndClear();
        } else {
            this.streamingText += chunk.content;
            this.textElement.textContent = this.streamingText;
        }
    }
    
    private fadeOutAndClear() {
        // Fade out animation
        this.textElement.style.transition = 'opacity 0.5s';
        this.textElement.style.opacity = '0';
        
        setTimeout(() => {
            this.streamingText = "";
            this.textElement.textContent = "";
            this.textElement.style.opacity = '1';
        }, 500);
    }
    
    public render() {
        this.css3dRenderer.render(this.scene, this.camera);
    }
}
```

**2. Integrate in Animation Loop**
```typescript
// Modify your existing animation loop
const streamingOverlay = new StreamingTextOverlay3D(scene, camera);

function animate() {
    requestAnimationFrame(animate);
    
    // Regular WebGL rendering
    renderer.render(scene, camera);
    
    // CSS3D rendering (for streaming text)
    streamingOverlay.render();
}
```

### Phase 3: Agent-Specific Positioning (Optional, 1 hour)

**Idea**: Different agents = different text positions/colors

```typescript
private getPositionForAgent(agentName: string): THREE.Vector3 {
    switch(agentName) {
        case "3D Modeling Agent":
            return new THREE.Vector3(5, 3, 0); // Right side
        case "Geometry Agent":
            return new THREE.Vector3(-5, 3, 0); // Left side
        case "General Agent":
        default:
            return new THREE.Vector3(0, 5, 0); // Center top
    }
}

private getColorForAgent(agentName: string): string {
    const colors = {
        "3D Modeling Agent": "#667eea",
        "Geometry Agent": "#f093fb",
        "General Agent": "#4facfe"
    };
    return colors[agentName] || "#ffffff";
}
```

---

## Visual Design Concepts

### Concept A: Floating Speech Bubble
- Text appears in 3D space above the shape being created
- Arrow pointing to relevant geometry
- Agent icon/avatar floating nearby
- Fade in word-by-word with typewriter sound effect

### Concept B: Holographic Terminal
- Fixed position in corner of 3D view
- Scanline/CRT shader effect
- Monospace font, green-on-black (retro)
- Agent name in header bar

### Concept C: Contextual Captions
- Text appears near mouse cursor / active object
- Follows camera but maintains readable angle
- Multi-line with word wrap
- Auto-hide after 3 seconds if no new chunks

---

## Edge Cases & Considerations

### Long Responses
- **Problem**: 1000-word response in 3D space?
- **Solution**: Paginate, show "..." for truncated, or auto-scroll in overlay

### Multiple Simultaneous Agents
- **Problem**: Two agents responding at once?
- **Solution**: Stack multiple overlays, or queue/interleave

### User Camera Movement
- **Problem**: Text flies off screen when camera rotates
- **Solution**: Billboard mode (always face camera) or lock to screen space

### Markdown Rendering
- **Problem**: Canvas can't render markdown
- **Solution**: CSS3DRenderer uses HTML, so use `marked.js` or similar

### Error Messages
- **Problem**: "Rate limit exceeded" should look different
- **Solution**: Red overlay, different position, pulsing animation

---

## Fallback Strategy

If CSS3D has performance issues or browser compatibility problems:
1. Fall back to traditional Blazor chat panel
2. Keep pub/sub infrastructure for future use
3. Log telemetry to understand failure modes

---

## Success Metrics

- **Streaming smoothness**: Users see word-by-word updates (< 50ms latency)
- **Readability**: Text clearly readable from any camera angle
- **Performance**: No FPS drop during streaming (maintain 60fps)
- **User engagement**: Time spent reading responses increases

---

## Future Enhancements

1. **Voice Synthesis**: Stream audio alongside text (text-to-speech)
2. **Gesture Control**: User points at shape, text appears there
3. **AR Mode**: Text overlays in AR/VR headsets
4. **Collaborative**: Multiple users see same streaming text in shared 3D space
5. **Code Highlighting**: If agent returns code, syntax highlight in 3D

---

## Next Steps

1. ✅ Review this spec
2. ⬜ Choose target view for implementation (DebugCanvas? New prototype page?)
3. ⬜ Implement Phase 1 (pub/sub infrastructure)
4. ⬜ Prototype CSS3DRenderer overlay
5. ⬜ Test streaming performance
6. ⬜ Iterate on visual design
7. ⬜ Roll out to AgentCanvasIntegration page

---

*This is a bold idea, and it's going to be awesome! 🚀*
