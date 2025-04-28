// JavaScript to auto - close mobile TOC after clicking a link
document.addEventListener('DOMContentLoaded', function () {
    const tocLinks = document.querySelectorAll('#mobileToc .nav-link-item');
    const mobileToc = document.getElementById('mobileToc');

    tocLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            if (window.innerWidth < 768) { // Only collapse on mobile
                var collapse = bootstrap.Collapse.getInstance(mobileToc);
                if (collapse) {
                    collapse.hide();
                }
            }
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const navLinks = document.querySelectorAll('.nav-link-item');
    const sections = document.querySelectorAll('.main-content');

    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            // Check if the clicked link corresponds to the Home or Index button
            if (link.getAttribute('href') === '/' || link.getAttribute('href') === '/Home/Index') {
                return; // Allow default behavior (no script interference)
            }

            if (link.getAttribute('href') === '/' || link.getAttribute('href') === '/Editor') {
                return; // Allow default behavior (no script interference)
            }

            if (link.getAttribute('href') === '/' || link.getAttribute('href') === '/Documentation/Docu') {
                return; // Allow default behavior (no script interference)
            }

            e.preventDefault(); // Prevent the default link behavior for all other links

            // Hide all sections
            sections.forEach(section => {
                section.style.display = 'none';
            });

            // Show the selected section
            const targetId = link.getAttribute('href').substring(1); // Get the target section id
            const targetSection = document.getElementById(targetId);
            if (targetSection) {
                targetSection.style.display = 'block';
            }
        });
    });
});

document.addEventListener("DOMContentLoaded", function () {
    // Make the first section visible by default
    const firstSection = document.querySelector('.main-content');
    if (firstSection) {
        firstSection.style.display = 'block';
    }
});