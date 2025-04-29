document.addEventListener("DOMContentLoaded", function () {
    const desktopInput = document.getElementById('fileNameInputDesktop');
    const mobileInput = document.getElementById('fileNameInputMobile');

    // When either one changes, update the other
    desktopInput.addEventListener('input', () => {
        mobileInput.value = desktopInput.value;
    });
    mobileInput.addEventListener('input', () => {
        desktopInput.value = mobileInput.value;
    });
});