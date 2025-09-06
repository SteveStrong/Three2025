// Matrix Transformation Test Visualization
// Provides 3D visualization of matrix transformations using Three.js

window.MatrixTestVisualization = {
    scene: null,
    camera: null,
    renderer: null,
    originalPointMesh: null,
    transformedPointMesh: null,
    gridHelper: null,
    axesHelper: null,
    lineMesh: null,
    
    initialize: function(containerId) {
        try {
            const container = document.getElementById(containerId);
            if (!container) {
                console.error('Container not found:', containerId);
                return;
            }
            
            // Check if THREE is available (from BlazorThreeJS)
            if (typeof THREE === 'undefined') {
                console.warn('THREE.js not available, matrix visualization will not work');
                container.innerHTML = '<div style="padding: 20px; text-align: center; color: #666;">THREE.js not available for visualization</div>';
                return;
            }
            
            // Clear any existing content
            container.innerHTML = '';
            
            // Scene setup
            this.scene = new THREE.Scene();
            this.scene.background = new THREE.Color(0xf8f9fa);
            
            // Camera setup
            const aspect = container.clientWidth / container.clientHeight;
            this.camera = new THREE.PerspectiveCamera(75, aspect, 0.1, 1000);
            this.camera.position.set(8, 6, 8);
            this.camera.lookAt(0, 0, 0);
            
            // Renderer setup
            this.renderer = new THREE.WebGLRenderer({ antialias: true });
            this.renderer.setSize(container.clientWidth, container.clientHeight);
            this.renderer.shadowMap.enabled = true;
            this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
            container.appendChild(this.renderer.domElement);
            
            // Lighting
            const ambientLight = new THREE.AmbientLight(0x404040, 0.6);
            this.scene.add(ambientLight);
            
            const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
            directionalLight.position.set(10, 10, 5);
            directionalLight.castShadow = true;
            directionalLight.shadow.mapSize.width = 2048;
            directionalLight.shadow.mapSize.height = 2048;
            this.scene.add(directionalLight);
            
            // Grid helper
            this.gridHelper = new THREE.GridHelper(20, 20, 0x888888, 0xcccccc);
            this.scene.add(this.gridHelper);
            
            // Axes helper
            this.axesHelper = new THREE.AxesHelper(5);
            this.scene.add(this.axesHelper);
            
            // Create point geometries
            this.createPointMeshes();
            
            // Controls
            if (typeof THREE.OrbitControls !== 'undefined') {
                const controls = new THREE.OrbitControls(this.camera, this.renderer.domElement);
                controls.enableDamping = true;
                controls.dampingFactor = 0.05;
                controls.screenSpacePanning = false;
                controls.minDistance = 3;
                controls.maxDistance = 50;
                controls.maxPolarAngle = Math.PI;
                
                // Animation loop with controls
                const animate = () => {
                    requestAnimationFrame(animate);
                    controls.update();
                    this.renderer.render(this.scene, this.camera);
                };
                animate();
            } else {
                // Simple animation loop without controls
                const animate = () => {
                    requestAnimationFrame(animate);
                    this.renderer.render(this.scene, this.camera);
                };
                animate();
            }
            
            // Handle window resize
            window.addEventListener('resize', () => this.onWindowResize(container));
            
            console.log('MatrixTestVisualization initialized successfully');
            
        } catch (error) {
            console.error('Error initializing MatrixTestVisualization:', error);
        }
    },
    
    createPointMeshes: function() {
        // Original point (blue sphere)
        const originalGeometry = new THREE.SphereGeometry(0.15, 16, 16);
        const originalMaterial = new THREE.MeshPhongMaterial({ 
            color: 0x2196F3,
            transparent: true,
            opacity: 0.8
        });
        this.originalPointMesh = new THREE.Mesh(originalGeometry, originalMaterial);
        this.originalPointMesh.castShadow = true;
        this.scene.add(this.originalPointMesh);
        
        // Transformed point (green sphere)
        const transformedGeometry = new THREE.SphereGeometry(0.15, 16, 16);
        const transformedMaterial = new THREE.MeshPhongMaterial({ 
            color: 0x4CAF50,
            transparent: true,
            opacity: 0.8
        });
        this.transformedPointMesh = new THREE.Mesh(transformedGeometry, transformedMaterial);
        this.transformedPointMesh.castShadow = true;
        this.scene.add(this.transformedPointMesh);
        
        // Line connecting the points
        const lineGeometry = new THREE.BufferGeometry();
        const linePositions = new Float32Array(6); // 2 points * 3 coordinates
        lineGeometry.setAttribute('position', new THREE.BufferAttribute(linePositions, 3));
        
        const lineMaterial = new THREE.LineBasicMaterial({ 
            color: 0x666666,
            transparent: true,
            opacity: 0.6
        });
        this.lineMesh = new THREE.Line(lineGeometry, lineMaterial);
        this.scene.add(this.lineMesh);
        
        // Labels
        this.createLabels();
    },
    
    createLabels: function() {
        try {
            // Create text sprites for labels
            const canvas = document.createElement('canvas');
            const context = canvas.getContext('2d');
            canvas.width = 256;
            canvas.height = 64;
            
            // Original point label
            context.clearRect(0, 0, canvas.width, canvas.height);
            context.fillStyle = '#2196F3';
            context.font = 'Bold 24px Arial';
            context.textAlign = 'center';
            context.fillText('Original', canvas.width / 2, canvas.height / 2);
            
            const originalTexture = new THREE.CanvasTexture(canvas);
            const originalSpriteMaterial = new THREE.SpriteMaterial({ map: originalTexture });
            const originalSprite = new THREE.Sprite(originalSpriteMaterial);
            originalSprite.scale.set(2, 0.5, 1);
            originalSprite.position.set(0, 0.5, 0);
            this.originalPointMesh.add(originalSprite);
            
            // Transformed point label
            context.clearRect(0, 0, canvas.width, canvas.height);
            context.fillStyle = '#4CAF50';
            context.font = 'Bold 24px Arial';
            context.textAlign = 'center';
            context.fillText('Transformed', canvas.width / 2, canvas.height / 2);
            
            const transformedTexture = new THREE.CanvasTexture(canvas);
            const transformedSpriteMaterial = new THREE.SpriteMaterial({ map: transformedTexture });
            const transformedSprite = new THREE.Sprite(transformedSpriteMaterial);
            transformedSprite.scale.set(2, 0.5, 1);
            transformedSprite.position.set(0, 0.5, 0);
            this.transformedPointMesh.add(transformedSprite);
            
        } catch (error) {
            console.log('Labels not created (canvas/sprite features may not be available)');
        }
    },
    
    updatePoints: function(originalPoint, transformedPoint) {
        try {
            if (!this.originalPointMesh || !this.transformedPointMesh || !this.lineMesh) {
                console.warn('Meshes not initialized');
                return;
            }
            
            // Update original point position
            this.originalPointMesh.position.set(originalPoint.x, originalPoint.y, originalPoint.z);
            
            // Update transformed point position
            this.transformedPointMesh.position.set(transformedPoint.x, transformedPoint.y, transformedPoint.z);
            
            // Update connecting line
            const linePositions = this.lineMesh.geometry.attributes.position.array;
            linePositions[0] = originalPoint.x;
            linePositions[1] = originalPoint.y;
            linePositions[2] = originalPoint.z;
            linePositions[3] = transformedPoint.x;
            linePositions[4] = transformedPoint.y;
            linePositions[5] = transformedPoint.z;
            this.lineMesh.geometry.attributes.position.needsUpdate = true;
            
            // Optional: animate camera to keep points in view
            this.adjustCameraToFitPoints(originalPoint, transformedPoint);
            
        } catch (error) {
            console.error('Error updating points:', error);
        }
    },
    
    adjustCameraToFitPoints: function(originalPoint, transformedPoint) {
        try {
            // Calculate the center point between the two points
            const centerX = (originalPoint.x + transformedPoint.x) / 2;
            const centerY = (originalPoint.y + transformedPoint.y) / 2;
            const centerZ = (originalPoint.z + transformedPoint.z) / 2;
            
            // Calculate the distance between points
            const dx = transformedPoint.x - originalPoint.x;
            const dy = transformedPoint.y - originalPoint.y;
            const dz = transformedPoint.z - originalPoint.z;
            const distance = Math.sqrt(dx * dx + dy * dy + dz * dz);
            
            // Adjust camera position to maintain a good view
            const minDistance = Math.max(10, distance * 2);
            const currentDistance = this.camera.position.length();
            
            if (currentDistance < minDistance) {
                const factor = minDistance / currentDistance;
                this.camera.position.multiplyScalar(factor);
            }
            
            // Smoothly look at the center point
            const targetLookAt = new THREE.Vector3(centerX, centerY, centerZ);
            
            // Only update if the camera isn't being manually controlled
            // This prevents fighting with OrbitControls
            if (!this.camera.userData.isUserControlled) {
                this.camera.lookAt(targetLookAt);
            }
            
        } catch (error) {
            console.error('Error adjusting camera:', error);
        }
    },
    
    onWindowResize: function(container) {
        try {
            if (!this.camera || !this.renderer) return;
            
            const width = container.clientWidth;
            const height = container.clientHeight;
            
            this.camera.aspect = width / height;
            this.camera.updateProjectionMatrix();
            
            this.renderer.setSize(width, height);
            
        } catch (error) {
            console.error('Error handling window resize:', error);
        }
    }
};

// Debug helper functions
window.MatrixTestVisualization.debug = {
    logScene: function() {
        console.log('Scene children count:', window.MatrixTestVisualization.scene?.children.length);
        window.MatrixTestVisualization.scene?.children.forEach((child, index) => {
            console.log(`Child ${index}:`, child.type, child.position);
        });
    },
    
    setPointPositions: function(ox, oy, oz, tx, ty, tz) {
        window.MatrixTestVisualization.updatePoints(
            { x: ox, y: oy, z: oz },
            { x: tx, y: ty, z: tz }
        );
    }
};
