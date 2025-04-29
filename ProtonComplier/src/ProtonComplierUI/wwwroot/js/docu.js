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

// Define error messages in an object
const messages = {
    P001: {
        code: 'P001',
        severity: 'Error',
        text: 'Internal compiler error',
        example: '.....',
    },
    P002: {
        code: 'P002',
        severity: 'Error',
        text: 'Deprecated token usage',
        example: '.....',
    },
    P004: {
        code: 'P004',
        severity: 'Error',
        text: 'Invalid macro definition',
        example: '.....',
    },
    P006: {
        code: 'P006',
        severity: 'Warning',
        text: 'Multiple macro definition',
        example: '.....',
    },
    P007: {
        code: 'P007',
        severity: 'Error',
        text: 'Unknown error',
        example: '.....',
    }
};

// Function to display error details in the content section
function showMessage(code) {
    const msg = messages[code];
    if (!msg) {
        console.error(`Unknown error code: ${code}`);
        return;
    } else { console.error(`Error code: ${code}`); }

    // Dynamically update the content area with error details
    const content = document.getElementById('content');
    content.innerHTML = `
                        <h2>Error ${msg.code}</h2>
                        <table>
                            <tr><th>Code</th><td><b>${msg.code}</b></td></tr>
                            <tr><th>Severity</th><td>${msg.severity}</td></tr>
                            <tr><th>Message</th><td>${msg.text}</td></tr>
                        </table>
                        <p>Example <br>
                           The following example generates ${msg.code}: <br>
                           ${msg.example}
                        </p>
                        <hr />
                        <div class="d-flex justify-content-between">
                                            <a class="nav-link-item" href="#P002" onclick="showMessage('P002'); return false;"> ← Previous: “${msg.code}” </a>
                                            <a class="nav-link-item" href="#P004" onclick="showMessage('P004'); return false;"> Next Up: “${msg.code}” →</a>
                        </div>
                    `;
}

// Handle initial load for hash-based error display
window.addEventListener('load', () => {
    const hash = window.location.hash;
    if (hash && hash.startsWith("#P")) {
        const code = hash.substring(1); // "P001"
        showMessage(code);
    }
});

// Optional: Add dynamic toggling for the sidebar (responsive behavior)
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('active');
}