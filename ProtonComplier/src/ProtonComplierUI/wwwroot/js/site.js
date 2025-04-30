// Function to apply the correct theme from localStorage
function applySavedTheme() {
    const theme = localStorage.getItem('theme');
    const themeToggle = document.getElementById('themeToggle');
    const body = document.querySelector('body');
    const header = document.querySelector('header');
    const navbar = document.querySelector('.navbar-proton');
    const logo = document.querySelector('.navbar-brand img');
    const wavePath = document.querySelector('.wave-bottom path');
    const featuresSections = document.querySelectorAll('.features-section'); // <-- changed to querySelectorAll
    const navContainer = document.querySelectorAll('.nav-container');
    const docsHeader = document.querySelectorAll('.docs-header');
    const docuBody = document.querySelectorAll('.DocuBody');
    const docuBodyEditor = document.querySelectorAll('.DocuBodyEditor');
    const docsMain = document.querySelectorAll('.docs-main');
    const docuMainTitle = document.querySelectorAll('.docs-main-title');
    const linkbox = document.querySelectorAll('.link-box');
    const docsSidebar = document.querySelectorAll('.sidebar');

    if (theme === 'dark') {
        body.classList.add('dark-mode');
        header.classList.add('dark-mode');
        navbar.classList.add('dark-mode');
        if (wavePath) wavePath.setAttribute('fill', '#1d1e23');
        featuresSections.forEach(section => section.classList.add('dark-mode')); // <-- loop through all
        navContainer.forEach(section => section.classList.add('dark-mode')); 
        docsHeader.forEach(section => section.classList.add('dark-mode')); 
        docuBody.forEach(section => section.classList.add('dark-mode')); 
        docuBodyEditor.forEach(section => section.classList.add('dark-mode')); 
        docsMain.forEach(section => section.classList.add('dark-mode')); 
        docuMainTitle.forEach(section => section.classList.add('dark-mode')); 
        linkbox.forEach(section => section.classList.add('dark-mode')); 
        docsSidebar.forEach(section => section.classList.add('dark-mode')); 
        themeToggle.checked = true;
        if (logo) logo.src = '/images/proton_logo_darkmode.png';
    } else {
        body.classList.remove('dark-mode');
        header.classList.remove('dark-mode');
        navbar.classList.remove('dark-mode');
        if (wavePath) wavePath.setAttribute('fill', '#ffffff');
        featuresSections.forEach(section => section.classList.remove('dark-mode')); // <-- loop through all
        navContainer.forEach(section => section.classList.remove('dark-mode')); 
        docsHeader.forEach(section => section.classList.remove('dark-mode'));
        docuBody.forEach(section => section.classList.remove('dark-mode')); 
        docuBodyEditor.forEach(section => section.classList.remove('dark-mode')); 
        docsMain.forEach(section => section.classList.remove('dark-mode')); 
        docuMainTitle.forEach(section => section.classList.remove('dark-mode')); 
        linkbox.forEach(section => section.classList.remove('dark-mode')); 
        docsSidebar.forEach(section => section.classList.remove('dark-mode')); 
        themeToggle.checked = false;
        if (logo) logo.src = '/images/proton_logo.png';
    }
}

// Function to toggle theme and save to localStorage
function toggleTheme() {
    const body = document.querySelector('body');
    const header = document.querySelector('header');
    const navbar = document.querySelector('.navbar-proton');
    const logo = document.querySelector('.navbar-brand img');
    const wavePath = document.querySelector('.wave-bottom path');
    const featuresSections = document.querySelectorAll('.features-section'); // <-- changed to querySelectorAll
    const navContainer = document.querySelectorAll('.nav-container');
    const docsHeader = document.querySelectorAll('.docs-header');
    const docuBody = document.querySelectorAll('.DocuBody');
    const docuBodyEditor = document.querySelectorAll('.DocuBodyEditor');
    const docsMain = document.querySelectorAll('.docs-main');
    const docuMainTitle = document.querySelectorAll('.docs-main-title');
    const linkbox = document.querySelectorAll('.link-box');
    const isDark = document.getElementById('themeToggle').checked;
    const docsSidebar = document.querySelectorAll('.sidebar');

    if (isDark) {
        body.classList.add('dark-mode');
        header.classList.add('dark-mode');
        navbar.classList.add('dark-mode');
        if (wavePath) wavePath.setAttribute('fill', '#1d1e23');
        featuresSections.forEach(section => section.classList.add('dark-mode')); // <-- loop through all
        navContainer.forEach(section => section.classList.add('dark-mode')); 
        docsHeader.forEach(section => section.classList.add('dark-mode'));
        docuBody.forEach(section => section.classList.add('dark-mode'));
        docuBodyEditor.forEach(section => section.classList.add('dark-mode')); 
        docsMain.forEach(section => section.classList.add('dark-mode')); 
        docuMainTitle.forEach(section => section.classList.add('dark-mode')); 
        linkbox.forEach(section => section.classList.add('dark-mode')); 
        docsSidebar.forEach(section => section.classList.add('dark-mode')); 
        if (logo) logo.src = '/images/proton_logo_darkmode.png';
        localStorage.setItem('theme', 'dark');
    } else {
        body.classList.remove('dark-mode');
        header.classList.remove('dark-mode');
        navbar.classList.remove('dark-mode');
        if (wavePath) wavePath.setAttribute('fill', '#ffffff');
        featuresSections.forEach(section => section.classList.remove('dark-mode')); // <-- loop through all
        navContainer.forEach(section => section.classList.remove('dark-mode')); 
        docsHeader.forEach(section => section.classList.remove('dark-mode'));
        docuBody.forEach(section => section.classList.remove('dark-mode')); 
        docuBodyEditor.forEach(section => section.classList.remove('dark-mode')); 
        docsMain.forEach(section => section.classList.remove('dark-mode')); 
        docuMainTitle.forEach(section => section.classList.remove('dark-mode')); 
        linkbox.forEach(section => section.classList.remove('dark-mode')); 
        docsSidebar.forEach(section => section.classList.remove('dark-mode')); 
        if (logo) logo.src = '/images/proton_logo.png';
        localStorage.setItem('theme', 'light');
    }
}

window.addEventListener('DOMContentLoaded', () => {
    applySavedTheme();
    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        themeToggle.addEventListener('change', toggleTheme);
    }
});


// Listen for changes to the dark mode toggle checkbox to modify ProtonComplier theme
document.getElementById('themeToggle').addEventListener('change', function () {
    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle.checked) {
        monaco.editor.setTheme('proton-dark');
    } else {
        monaco.editor.setTheme('proton-light'); // Switch Monaco to light theme
    }
});

// For error messgae display
setTimeout(() => {
    const msg = document.getElementById("tempErrorMessage");
    if (msg) {
        msg.style.transition = "opacity 0.5s ease-out";
        msg.style.opacity = 0;
        setTimeout(() => msg.remove(), 300); // remove from DOM
    }
}, 3000);

// DragHandler
const handler = document.querySelector('.handler');
const wrapper = handler.closest('.wrapper');
const boxA = wrapper.querySelectorAll('.box')[0];
const boxB = wrapper.querySelectorAll('.box')[1];
let isHandlerDragging = false;
const originalWidthA = boxA.offsetWidth;
const originalWidthB = boxB.offsetWidth;
const minWidthPercentage = 0.3;

document.addEventListener('mousedown', e => {
    if (e.target === handler) isHandlerDragging = true;
});

document.addEventListener('mousemove', e => {
    if (!isHandlerDragging) return;

    const containerOffsetLeft = wrapper.offsetLeft;
    const pointerRelativeXpos = e.clientX - containerOffsetLeft;
    const minWidth = wrapper.offsetWidth * minWidthPercentage;

    const boxALeftWidth = Math.max(minWidth, pointerRelativeXpos - 8);
    const boxBRightWidth = Math.max(minWidth, wrapper.offsetWidth - boxALeftWidth - handler.offsetWidth);

    boxA.style.width = boxALeftWidth + 'px';
    boxB.style.width = boxBRightWidth + 'px';

    boxA.style.flexGrow = 0;
    boxB.style.flexGrow = 0;
});

document.addEventListener('mouseup', () => {
    isHandlerDragging = false;

    // Save current widths to localStorage
    localStorage.setItem('boxAWidth', boxA.style.width);
    localStorage.setItem('boxBWidth', boxB.style.width);
});


window.addEventListener('resize', () => {
    if (window.innerWidth <= 768) {
        // Reset widths for mobile layout
        boxA.style.width = '';
        boxB.style.width = '';

        // Optional: Clear stored widths
        localStorage.removeItem('boxAWidth');
        localStorage.removeItem('boxBWidth');
    } else {
        // Restore saved widths only if none are currently set
        if (boxA.style.width === '' && boxB.style.width === '') {
            const savedWidthA = localStorage.getItem('boxAWidth');
            const savedWidthB = localStorage.getItem('boxBWidth');

            if (savedWidthA && savedWidthB) {
                boxA.style.width = savedWidthA;
                boxB.style.width = savedWidthB;
            } else {
                boxA.style.width = originalWidthA + 'px';
                boxB.style.width = originalWidthB + 'px';
            }

            boxA.style.flexGrow = 0;
            boxB.style.flexGrow = 0;
        }
    }
});

window.addEventListener('DOMContentLoaded', () => {
    const savedWidthA = localStorage.getItem('boxAWidth');
    const savedWidthB = localStorage.getItem('boxBWidth');

    if (savedWidthA && savedWidthB && window.innerWidth > 768) {
        boxA.style.width = savedWidthA;
        boxB.style.width = savedWidthB;
        boxA.style.flexGrow = 0;
        boxB.style.flexGrow = 0;
    }

    // Also apply theme
    applySavedTheme();
});

