window.capturePhotoFromCamera = async function() {
    try {
        // Get camera stream
        const stream = await navigator.mediaDevices.getUserMedia({ 
            video: { facingMode: 'environment' } // Use back camera on mobile
        });
        
        // Create simple modal with video and buttons
        const modal = document.createElement('div');
        modal.style.cssText = 'position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.9); z-index: 9999; display: flex; flex-direction: column; align-items: center; justify-content: center;';
        
        const video = document.createElement('video');
        video.id = 'camera';
        video.autoplay = true;
        video.playsinline = true;
        video.style.cssText = 'max-width: 100%; max-height: 70vh;';
        video.srcObject = stream;
        
        const canvas = document.createElement('canvas');
        canvas.id = 'canvas';
        canvas.style.display = 'none';
        
        const buttonContainer = document.createElement('div');
        buttonContainer.style.cssText = 'margin-top: 20px; display: flex; gap: 10px;';
        
        const snapBtn = document.createElement('button');
        snapBtn.id = 'snap';
        snapBtn.textContent = 'Chụp ảnh';
        snapBtn.className = 'btn btn-primary';
        snapBtn.style.cssText = 'padding: 10px 20px; font-size: 16px;';
        
        const cancelBtn = document.createElement('button');
        cancelBtn.textContent = 'Hủy';
        cancelBtn.className = 'btn btn-secondary';
        cancelBtn.style.cssText = 'padding: 10px 20px; font-size: 16px;';
        
        buttonContainer.appendChild(snapBtn);
        buttonContainer.appendChild(cancelBtn);
        modal.appendChild(video);
        modal.appendChild(canvas);
        modal.appendChild(buttonContainer);
        document.body.appendChild(modal);
        
        return new Promise((resolve, reject) => {
            let isResolved = false;
            
            const cleanup = () => {
                stream.getTracks().forEach(track => track.stop());
                if (modal.parentNode) {
                    document.body.removeChild(modal);
                }
            };
            
            // Wait for video to be ready
            video.addEventListener('loadedmetadata', () => {
                // Video is ready, enable capture button
            }, { once: true });
            
            snapBtn.onclick = () => {
                if (isResolved) return;
                
                // Wait a bit to ensure video is fully loaded
                if (video.videoWidth === 0 || video.videoHeight === 0) {
                    alert('Camera chưa sẵn sàng. Vui lòng đợi một chút.');
                    return;
                }
                
                try {
                    canvas.width = video.videoWidth;
                    canvas.height = video.videoHeight;
                    const ctx = canvas.getContext('2d');
                    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                    
                    const dataUrl = canvas.toDataURL('image/jpeg', 0.9);
                    isResolved = true;
                    cleanup();
                    resolve(dataUrl);
                } catch (error) {
                    cleanup();
                    reject(error);
                }
            };
            
            cancelBtn.onclick = () => {
                if (isResolved) return;
                isResolved = true;
                cleanup();
                reject(new Error('Cancelled'));
            };
            
            // Handle modal click outside to close
            modal.onclick = (e) => {
                if (e.target === modal && !isResolved) {
                    isResolved = true;
                    cleanup();
                    reject(new Error('Cancelled'));
                }
            };
        });
    } catch (error) {
        alert('Không thể truy cập camera: ' + error.message);
        throw error;
    }
};

