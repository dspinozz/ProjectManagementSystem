// Video Background Controller for Login Page
// Handles slow playback, lazy loading, and reduced-motion preference

window.setVideoPlaybackSpeed = function (videoId, speed) {
    const video = document.getElementById(videoId);
    if (!video) return;

    // Check for reduced motion preference
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) {
        video.pause();
        video.style.display = 'none';
        return;
    }

    // Set playback speed (0.1 = ultra slow, cinematic)
    video.playbackRate = speed || 0.1;

    // Fade in when video is ready
    video.addEventListener('canplaythrough', function () {
        video.classList.add('loaded');
        // Fade out the gradient fallback
        const fallback = document.querySelector('.gradient-fallback');
        if (fallback) {
            fallback.style.opacity = '0';
        }
    });

    // Handle video loading errors gracefully
    video.addEventListener('error', function () {
        console.log('Video failed to load, using gradient fallback');
        video.style.display = 'none';
    });

    // If video is already loaded (cached), trigger immediately
    if (video.readyState >= 3) {
        video.classList.add('loaded');
        const fallback = document.querySelector('.gradient-fallback');
        if (fallback) {
            fallback.style.opacity = '0';
        }
    }

    // Ensure video plays (some browsers block autoplay)
    video.play().catch(function (error) {
        console.log('Autoplay prevented, video will show on interaction');
    });
};

// Listen for reduced motion changes
window.matchMedia('(prefers-reduced-motion: reduce)').addEventListener('change', function (e) {
    const video = document.getElementById('bgVideo');
    if (!video) return;

    if (e.matches) {
        video.pause();
        video.style.display = 'none';
        const fallback = document.querySelector('.gradient-fallback');
        if (fallback) fallback.style.opacity = '1';
    } else {
        video.style.display = 'block';
        video.play();
    }
});
