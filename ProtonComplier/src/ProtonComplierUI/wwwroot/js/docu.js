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
        text: 'LexerError: during complie Proton project',
        example: 'The error is caused when an empty file or a file containing only comments is compiled by the compiler.\n ' +
            'Please define at least the macros to run the code correctly.',
        editorcode: '?',
    },
    P004: {
        code: 'P004',
        severity: 'Error',
        text: 'LexerError: Invalid macro: {0} at line {1}, column {2}. Expected one of: {3}.',
        example: 'The error is caused when file contains an invalid macro type. You can fix this error if you define one of the following macro type:\n' +
            '<ul><li><b>#StateSpace</b></li><li><b>#Input</b></li><li><b>#Precondition</b></li><li><b>#Postcondition</b></li> </ul >\n' +
            '<h2><b>Example</b></h2><p>The following example generates P004:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\n#NotdefinedMacro\n\n#Precondition\n\n#Postcondition',
    },
    P005: {
        code: 'P005',
        severity: 'Error',
        text: 'LexerError: Missing section: {0}. This section is required for the document to be valid.',
        example: 'The error is caused when file not contains all valid macro type. You can fix this error if you define all of the following macro type:\n' +
            '<ul><li><b>#StateSpace</b></li><li><b>#Input</b></li><li><b>#Precondition</b></li><li><b>#Postcondition</b></li> </ul >\n' +
            '<h2><b>Example</b></h2><p>The following example generates P005:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\n#Precondition\n\n#Postcondition',
    },
    P006: {
        code: 'P006',
        severity: 'Warning',
        text: 'LexerWarning: Multiple {0} macros detected at line {1}, column {2}. The definitions are being added as extra lines.',
        example: 'The warning is caused when file contains one valid macro type defined in multiple times. You can fix this error if you define all of the following macro type just once:\n' +
            '<ul><li><b>#StateSpace</b></li><li><b>#Input</b></li><li><b>#Precondition</b></li><li><b>#Postcondition</b></li> </ul >\n' +
            '<h2><b>Example</b></h2><p>The following example generates P006:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\n#Input\n\n#Precondition\n\n#Postcondition\n\n#StateSpace',
    },
    P008: {
        code: 'P008',
        severity: 'Error',
        text: 'LexerError: {0} at line {1}, column {2} is not valid because the first row does not belong under any macro type.',
        example: 'The error occurs when the file contains lines of code that cannot be associated with any macro type. You can fix this error if you define every code row under one of the following valid and related macro type:\n' +
            '<ul><li><b>#StateSpace</b></li><li><b>#Input</b></li><li><b>#Precondition</b></li><li><b>#Postcondition</b></li> </ul >\n' +
            '<h2><b>Example</b></h2><p>The following example generates P008:</p>',
        editorcode: '// @ProtonComplier\n\nvar:N[];\n\n#StateSpace\n\n#Input\n\n#Precondition\n\n#Postcondition\n\n#StateSpace',
    },
    P011: {
        code: 'P011',
        severity: 'Error',
        text: 'LexerError: The unknown token {0} detected at line {1}, column {2} is not exist in the current context.',
        example: 'The error occurs when the file contains lines of code that cannot be associated with any macro type. You can fix this error if you define every code row under one of the following valid and related macro type:\n' +
            '<ul><li><b>#StateSpace</b></li><li><b>#Input</b></li><li><b>#Precondition</b></li><li><b>#Postcondition</b></li> </ul >\n' +
            '<h2><b>Example</b></h2><p>The following example generates P011:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\n@:N[];\n\n#Input\n\n#Precondition\n\n#Postcondition\n\n#StateSpace',
    },
    P101: {
        code: 'P101',
        severity: 'Error',
        text: 'ParserError: Invalid expression at line {0}, column {1}. Expected expression type: {2}.',
        example: 'The error occurs when the file contains lines of invalid expressions.You can fix this error if you define a fully valid expression.' +
                 '<h2><b>Example</b></h2><p>The following example generates P101:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\nn:$[]f:N[];\n\n#Input\n\n#Precondition\n\n#Postcondition',
    },
    P104: {
        code: 'P104',
        severity: 'Error',
        text: 'ParserError: Unexpected token: {0} at line {1}, column {2}. Expected token: {3}.',
        example: 'The error occurs when the file contains lines of invalid tokens in the expressions.You can fix this error if you define a proper token for the expression.' +
            '<h2><b>Example</b></h2><p>The following example generates P104:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\n#Input\n\nn=1+2(3)\n\n#Precondition\n\n#Postcondition',
    },
    P107: {
        code: 'P107',
        severity: 'Warning',
        text: 'ParserWarning: Multiple consecutive commas detected at line {0}, column {1}.',
        example: 'The warning occurs when the file contains lines where multiple consecutive comma follow each other as separator. You can fix this warning if you define only one separator.' +
            '<h2><b>Example</b></h2><p>The following example generates P107:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\nvar1,,var2:N;\n\n#Input\n\n#Precondition\n\n#Postcondition',
    },
    P108: {
        code: 'P108',
        severity: 'Warning',
        text: 'ParserWarning: Multiple consecutive delimeter detected at line {0}, column {1}.',
        example: 'The warning occurs when the file contains lines where multiple consecutive delimeter follow each other as separator. You can fix this warning if you define only one delimeter as separator.' +
                 '<h2><b>Example</b></h2><p>The following example generates P108:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\nvar1:N;;var2:N;\n\n#Input\n\n#Precondition\n\n#Postcondition',
    },
    P112: {
        code: 'P112',
        severity: 'Error',
        text: 'ParserError: Multiple list specifier detected at line {0}, column {1}.',
        example: 'The warning occurs when the file contains lines where identifier has multiple consecutive list specifier follow each other. You can fix this error if you define only one list specifier.' +
            '<h2><b>Example</b></h2><p>The following example generates P112:</p>',
        editorcode: '// @ProtonComplier\n\n#StateSpace\n\nvar1:N[][];\n\n#Input\n\n#Precondition\n\n#Postcondition',
    },
    P204: {
        code: 'P204',
        severity: 'Error',
        text: 'SemanticalError: Invalid variable name declaration {0} at line {1}, column {2}.',
        example: 'The warning occurs when the file contains lines where variable name definition is invalid. You can fix this error if you define variable name which cant be longer than 511 character and start with and followed by: underscore or the mixture of the following characters: numbers: [0-9]; characters: [a-z;A-Z] or underscore.',
        editorcode: '?',
    },
};

// Get an ordered list of error codes
const errorCodes = Object.keys(messages);

// Function to display error details in the content section
function showMessage(code) {

    //Update URL hash without page reload
    window.location.hash = code;

    const content = document.getElementById('content');
    const content2 = document.getElementById('content2');
    const editorContainer = document.getElementById('editor-container');

    // Handle special "Main" page case
    if (code === 'P000') {
        content.innerHTML = `
            <h1>Proton Error Documentation</h1>
            <p>Welcome to the Proton Error Documentation section. <br />
                Here, you can explore detailed information on various errors and warnings that may occur during compilation and execution. <br />
                Each entry includes an explanation of the error or warning, along with common causes and practical examples of how to resolve them. Use the navigation on the left to select an error or warning code and view its full details.</p>
                <button class="btn btn-primary" onclick="showMessage('P001')">
                    Next Up: “P001” →
                </button>
        `;
        content2.innerHTML = "";
        editorContainer.style.display = 'none';
        return;
    }

    const msg = messages[code];
    if (!msg) {
        console.error(`Unknown error code: ${code}`);
        return;
    }

    // Dynamically update the content area with error details
    content.innerHTML = `
        <h2>Error ${msg.code}</h2>
        <table id="errortable"> 
            <tr><th>Code</th><td><b>${msg.code}</b></td></tr>
            <tr><th>Severity</th><td>${msg.severity}</td></tr>
            <tr><th>Message</th><td>${msg.text}</td></tr>
        </table>
        <br>
        <h2>${msg.severity} description:</h2>
           ${msg.example}
        </p>
    `;

    // Event listener to trigger setting the editor value
    if (msg.editorcode !== '?') {
        setEditorValue(msg.editorcode);
        document.getElementById('editor-container').style.display = 'block';
    } else {
        document.getElementById('editor-container').style.display = 'none';
    }

    content2.innerHTML = `
        <hr />
        <div class="d-flex justify-content-between">
            ${getPrevLink(code)}
            ${getNextLink(code)}
        </div>
    `;
}

// Function to get the previous error link
function getPrevLink(code) {
    const index = errorCodes.indexOf(code);
    if (index > 0) {
        const prevCode = errorCodes[index - 1];
        return `<button class="btn btn-secondary" onclick="showMessage('${prevCode}')">
                  ← Previous: “${prevCode}”
                </button>`;
    } else {
        return ''; // No previous error
    }
}

// Function to get the next error link
function getNextLink(code) {
    const index = errorCodes.indexOf(code);
    if (index < errorCodes.length - 1) {
        const nextCode = errorCodes[index + 1];
        return `<button class="btn btn-primary" onclick="showMessage('${nextCode}')">
                  Next Up: “${nextCode}” →
                </button>`;
    } else {
        return ''; // No next error
    }
}

// Optional: Add dynamic toggling for the sidebar (responsive behavior)
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    sidebar.classList.toggle('active');
}

// Handle initial load for hash-based error display
window.addEventListener('load', () => {
    const hash = window.location.hash;
    if (hash && hash.startsWith("#P")) {
        const code = hash.substring(1); // "P001"
        showMessage(code);
    }
});

window.addEventListener('hashchange', () => {
    const hash = window.location.hash;
    if (hash && hash.startsWith("#P")) {
        const code = hash.substring(1); // "P001"
        showMessage(code);
    }
});