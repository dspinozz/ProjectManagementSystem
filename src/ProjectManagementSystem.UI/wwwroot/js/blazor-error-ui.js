// Blazor error UI handling
window.addEventListener('load', function() {
    const errorUi = document.getElementById('blazor-error-ui');
    if (errorUi) {
        const dismissButton = errorUi.querySelector('.dismiss');
        const reloadButton = errorUi.querySelector('.reload');
        
        if (dismissButton) {
            dismissButton.addEventListener('click', function() {
                errorUi.style.display = 'none';
            });
        }
        
        if (reloadButton) {
            reloadButton.addEventListener('click', function(e) {
                e.preventDefault();
                window.location.reload();
            });
        }
    }
});
