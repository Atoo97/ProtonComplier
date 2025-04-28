// Function to apply the correct theme from localStorage
function applySavedTheme() {
    const theme = localStorage.getItem('theme');
    const themeToggle = document.getElementById('themeToggle');
    const header = document.querySelector('header');
    const navbar = document.querySelector('.navbar-proton');
    const logo = document.querySelector('.navbar-brand img');

    if (theme === 'dark') {
        document.body.classList.add('dark-mode');
        header.classList.add('dark-mode');
        navbar.classList.add('dark-mode');
        themeToggle.checked = true;
        if (logo) logo.src = '/images/proton_logo_dark.png';
    } else {
        document.body.classList.remove('dark-mode');
        header.classList.remove('dark-mode');
        navbar.classList.remove('dark-mode');
        themeToggle.checked = false;
        if (logo) logo.src = '/images/proton_logo.png';
    }
}

// Function to toggle theme and save to localStorage
function toggleTheme() {
    const header = document.querySelector('header');
    const navbar = document.querySelector('.navbar-proton');
    const logo = document.querySelector('.navbar-brand img');
    const isDark = document.getElementById('themeToggle').checked;

    if (isDark) {
        document.body.classList.add('dark-mode');
        header.classList.add('dark-mode');
        navbar.classList.add('dark-mode');
        if (logo) logo.src = '/images/proton_logo_dark.png';
        localStorage.setItem('theme', 'dark');
    } else {
        document.body.classList.remove('dark-mode');
        header.classList.remove('dark-mode');
        navbar.classList.remove('dark-mode');
        if (logo) logo.src = '/images/proton_logo.png';
        localStorage.setItem('theme', 'light');
    }
}

window.addEventListener('DOMContentLoaded', () => {
    applySavedTheme(); // Apply theme when page loads
    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        themeToggle.addEventListener('change', toggleTheme); // Listen for toggle changes
    }
});
