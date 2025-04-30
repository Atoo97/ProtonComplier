document.addEventListener("DOMContentLoaded", function () {
    const desktopInput = document.getElementById('fileNameInputDesktop');
    const mobileInput = document.getElementById('fileNameInputMobile');
    const desktopUpload = document.getElementById('upLoadFormDesktop');
    const mobileUpload = document.getElementById('upLoadFormMobile');
    const desktopDownload = document.getElementById('downloadFormDesktop');
    const mobileDownload = document.getElementById('downloadFormMobile');
    const desktopCopy = document.getElementById('copyFormDesktop');
    const mobileCopy = document.getElementById('copyFormMobile');
    const desktopToggle = document.getElementById('TogglesDesktop');

    // When either one changes, update the other
    desktopInput.addEventListener('input', () => {
        mobileInput.value = desktopInput.value;
        mobileUpload.value = desktopUpload.value;
        mobileDownload.value = desktopDownload.value;
        mobileCopy.value = desktopCopy.value;
    });
    mobileInput.addEventListener('input', () => {
        desktopInput.value = mobileInput.value;
        desktopUpload.value = mobileUpload.value;
        desktopDownload.value = mobileDownload.value;
        desktopCopy.value = mobileCopy.value;
    });

    // Optional: adjust on window resize
    window.addEventListener('resize', () => {
        if (window.innerWidth < 768) {
            desktopToggle.style.display = 'none';
        } else {
            desktopToggle.style.display = 'flex';
        }
    });

});

/*EDITOR SETTINGS:*/
require.config({ paths: { 'vs': 'https://cdn.jsdelivr.net/npm/monaco-editor@0.38.0/min/vs' } });

let editor;
let rightEditor;

require(['vs/editor/editor.main'], function () {
    if (!monaco.languages.getLanguages().some(lang => lang.id === 'proton')) {
        monaco.languages.register({ id: 'proton' });
        monaco.languages.setMonarchTokensProvider('proton', {
            tokenizer: {
                root: [
                    // === Error & Warning ===
                    [/^\u26A0 ?\[ERROR\] \(\d+\)/, "error"],  // Handling warning symbol ⚠️ as unicode
                    [/^\u26A0 ?\[WARNING\] \(\d+\)/, "warning"],  // Same here for the warning

                    // === Comment ===
                    [/\/\/.*/, "comment"],

                    // === Identifiers ===
                    [/\bN(?![a-zA-Z_0-9])/, "identifier"],
                    [/\bZ(?![a-zA-Z_0-9])/, "identifier"],
                    [/\bR(?![a-zA-Z_0-9])/, "identifier"],
                    [/\bC(?![a-zA-Z_0-9])/, "identifier"],
                    [/\$(?![a-zA-Z_0-9])/, "identifier"],
                    [/\bB(?![a-zA-Z_0-9])/, "identifier"],

                    // === Literals ===
                    [/\b0|[1-9][0-9]*\b/, "number"],
                    [/\b-?(?!-)(0|[1-9][0-9]*)\b/, "number"],
                    [/\b-?(?!-)(0|[1-9][0-9]*)\.[0-9]+\b/, "number"],

                    [/\b[a-z_][\p{L}0-9_]*\b/u, "identifier-main"], // Identifier

                    [/'[^']'/, "char"],
                    [/".*?"/, "string"],
                    [/\b(True|False)\b/i, "boolean"],

                    // === Keywords ===
                    [/\b(Opt|Min|Max|Length)\b/, "custom-keyword"],

                    // === Operators ===
                    [/\+\+|--/, "operator"],
                    [/\+/, "operator"],
                    [/-/, "operator"],
                    [/\*/, "operator"],
                    [/\//, "operator"],
                    [/%/, "operator"],
                    [/=/, "operator"],
                    [/==/, "operator"],
                    [/≠/, "operator"],
                    [/>/, "operator"],
                    [/</, "operator"],
                    [/≥/, "operator"],
                    [/≤/, "operator"],
                    [/∧/, "operator"],
                    [/∨/, "operator"],
                    [/┐/, "operator"],
                    [/→/, "operator"],
                    [/∀/, "operator"],
                    [/∃/, "operator"],
                    [/∏/, "operator"],
                    [/∑/, "operator"],

                    // === Punctuators ===
                    [/;/, "delimiter"],
                    [/:/, "delimiter"],
                    [/,/, "delimiter"],
                    [/\(/, "delimiter"],
                    [/\)/, "delimiter"],
                    [/\{/, "delimiter"],
                    [/\}/, "delimiter"],
                    [/\[\]/, "delimiter"],
                    [/\[/, "delimiter"],
                    [/\]/, "delimiter"],

                    // === Specials ===
                    [/#([^\r\n]*)/, "macro"],
                    [/\?\?/, "special"],
                    [/\./, "delimiter"],
                ],
            }
        });
    }

    monaco.languages.registerCompletionItemProvider('proton', {
        triggerCharacters: ['&'],
        provideCompletionItems: (model, position) => {
            const suggestions = [
                // Logic Symbols
                {
                    label: '∧',
                    kind: monaco.languages.CompletionItemKind.Operator,
                    insertText: '∧',
                    detail: 'Logical AND',
                    documentation: 'Unicode: U+2227'
                },
                {
                    label: '∨',
                    kind: monaco.languages.CompletionItemKind.Operator,
                    insertText: '∨',
                    detail: 'Logical OR',
                    documentation: 'Unicode: U+2228'
                },
                {
                    label: '┐',
                    kind: monaco.languages.CompletionItemKind.Operator,
                    insertText: '┐',
                    detail: 'Logical NOT',
                    documentation: 'Unicode: U+2510'
                },
                {
                    label: '→',
                    kind: monaco.languages.CompletionItemKind.Operator,
                    insertText: '→',
                    detail: 'Implies',
                    documentation: 'Unicode: U+2192'
                },

                // Quantifiers
                {
                    label: '∀',
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: '∀',
                    detail: 'Universal quantifier',
                    documentation: 'Unicode: U+2200'
                },
                {
                    label: '∃',
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: '∃',
                    detail: 'Existential quantifier',
                    documentation: 'Unicode: U+2203'
                },

                // Math Symbols
                {
                    label: '≥',
                    kind: monaco.languages.CompletionItemKind.Operator,
                    insertText: '≥',
                    detail: 'Greater than or equal',
                    documentation: 'Unicode: U+2265'
                },
                {
                    label: '≤',
                    kind: monaco.languages.CompletionItemKind.Operator,
                    insertText: '≤',
                    detail: 'Less than or equal',
                    documentation: 'Unicode: U+2264'
                },
                {
                    label: '≠',
                    kind: monaco.languages.CompletionItemKind.Operator,
                    insertText: '≠',
                    detail: 'Not equal',
                    documentation: 'Unicode: U+2260'
                },
                {
                    label: '∑',
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: '∑',
                    detail: 'Summation',
                    documentation: 'Unicode: U+2211'
                },
                {
                    label: '∏',
                    kind: monaco.languages.CompletionItemKind.Function,
                    insertText: '∏',
                    detail: 'Product',
                    documentation: 'Unicode: U+220F'
                },

                // Special Symbols
                {
                    label: '[]',
                    kind: monaco.languages.CompletionItemKind.Struct,
                    insertText: '[]',
                    detail: 'List Specifier',
                    documentation: 'Array or list-like notation'
                }
            ];

            return { suggestions };
        }
    });

    monaco.editor.defineTheme('proton-light', {
        base: 'vs',
        inherit: true,
        rules: [
            // === Error & Warning ===
            { token: 'error', foreground: 'FF0000', fontStyle: 'bold' },   // Red for errors
            { token: 'warning', foreground: 'FFA500', fontStyle: 'bold' }, // Orange for warnings

            // === Comments ===
            { token: 'comment', foreground: '008000', fontStyle: 'italic' },

            // === Identifiers ===
            { token: 'identifier', foreground: '005FCC' },
            { token: 'identifier-main', foreground: '800080' },

            // === Literals ===
            { token: 'number', foreground: '098658' },
            { token: 'char', foreground: 'A31515' },
            { token: 'string', foreground: 'B20000' },
            { token: 'boolean', foreground: '0451A5', fontStyle: 'bold' },

            // === Keywords ===
            { token: 'custom-keyword', foreground: '0000FF', fontStyle: 'bold' },

            // === Operators ===
            { token: 'operator', foreground: 'AF00DB' },

            // === Punctuators / Delimiters ===
            { token: 'delimiter', foreground: '333333' },

            // === Specials ===
            { token: 'macro', foreground: '007878', fontStyle: 'bold' },
            { token: 'special', foreground: 'FF0000' },
        ],
        colors: {
            'editor.foreground': '#000000',
            'editor.background': '#FFFFFF',
            'editor.lineHighlightBackground': '#F3F3F3',
            'editorCursor.foreground': '#333333',
            'editorLineNumber.foreground': '#237893',
            'editor.selectionBackground': '#ADD6FF',
            'editor.inactiveSelectionBackground': '#E5EBF1',
        }
    });

    monaco.editor.defineTheme('proton-dark', {
        base: 'vs-dark',
        inherit: true,
        rules: [
            // === Error & Warning ===
            { token: 'error', foreground: 'FF0000', fontStyle: 'bold' },   // Red for errors
            { token: 'warning', foreground: 'FFA500', fontStyle: 'bold' }, // Orange for warnings

            // === Comments ===
            { token: 'comment', foreground: '2DB500', fontStyle: 'italic' },

            // === Identifiers ===
            { token: 'identifier', foreground: '5C47E0' },
            // { token: 'identifier-main', foreground: '3FC6F0' },

            // === Literals ===
            { token: 'number', foreground: 'B5CEA8' },   // greenish tone for numbers
            { token: 'char', foreground: 'CE9178' },     // reddish-brown for chars
            { token: 'string', foreground: 'D69D85' },   // lighter orange for strings
            { token: 'boolean', foreground: '4EC9B0', fontStyle: 'bold' }, // cyan for booleans

            // === Keywords ===
            { token: 'custom-keyword', foreground: '569CD6', fontStyle: 'bold' }, // cool blue for keywords

            // === Operators ===
            { token: 'operator', foreground: 'FF8A00' },

            // === Punctuators / Delimiters ===
            { token: 'delimiter', foreground: '808080' }, // muted gray for symbols like ; : {}

            // === Specials ===
            { token: 'macro', foreground: '328f81', fontStyle: 'bold' },
            { token: 'special', foreground: 'C88AFA' }, // red for ??

        ],
        colors: {
            'editor.foreground': '#FFFFFF',
            'editor.background': '#1E1E1E',
            'editor.lineHighlightBackground': '#2a2d2e',
            'editorCursor.foreground': '#A7A7A7',
            'editorLineNumber.foreground': '#858585',
            'editor.selectionBackground': '#264F78',
            'editor.inactiveSelectionBackground': '#3A3D41',
        }
    });

    // Display Editors:
    const container = document.getElementById('editor-inner');
    const rightContainer = document.getElementById('right-editor-inner');

    editor = monaco.editor.create(container, {
        value: window.editorInputText,
        language: 'proton',
        theme: 'proton-light',
        automaticLayout: true
    });

    rightEditor = monaco.editor.create(rightContainer, {
        value: window.outputText,
        language: 'proton',
        theme: 'proton-light',
        readOnly: true,
        automaticLayout: true
    });

    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle.checked) {
        monaco.editor.setTheme('proton-dark');
    } else {
        monaco.editor.setTheme('proton-light'); // Switch Monaco to light theme
    }
});


/*ADDITIONAL COMPLIE LOGIC*/
let outputString = "";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/compilerhub")
    .build();

connection.on("ConsoleOutput", function (message) {
    const output = document.getElementById("compilerOutput");
    const blinkingArrow = document.getElementById("blinkingArrow");

    if (output && blinkingArrow) {
        const formattedMessage = document.createElement("span");
        formattedMessage.textContent = message;

        output.insertBefore(formattedMessage, blinkingArrow);
        output.insertBefore(document.createElement("br"), blinkingArrow);

        // Scroll to bottom of the console panel
        output.parentElement.scrollTop = output.parentElement.scrollHeight;
    }
});

connection.on("EditorOutput", function (message) {
    window.outputText += message + "\n";

    if (typeof rightEditor !== 'undefined') {
        rightEditor.setValue(window.outputText);
    }
});

connection.on("ResetEditor", function (defaultCode, defaultOutput) {
    if (typeof editor !== 'undefined') {
        editor.setValue(defaultCode);
    }

    if (typeof rightEditor !== 'undefined') {
        rightEditor.setValue(defaultOutput);
    }

    // Also clear the console visually
    const output = document.getElementById("compilerOutput");
    if (output) {
        output.innerHTML = '<span id="blinkingArrow">ProtonCompiler &gt;&gt; </span>';
    }
});

let connectionId = null;

connection.start()
    .then(() => connection.invoke("GetConnectionId"))
    .then(id => {
        connectionId = id;
        console.log("Connected with ID:", connectionId);
    })
    .catch(err => console.error(err.toString()));


document.getElementById("compileButton").addEventListener("click", function () {
    const editorContent = editor.getValue();
    document.getElementById("InputText").value = editorContent;

    const lexicalEnabled = document.getElementById("toggleLexicalAnalyzer").checked;

    // Reset outputText and right editor
    window.outputText = "";
    if (typeof rightEditor !== 'undefined') {
        rightEditor.setValue("");
    }

    // Reset HTML console output
    const output = document.getElementById("compilerOutput");
    if (output) {
        output.innerHTML = '<span id="blinkingArrow">ProtonCompiler &gt;&gt; </span>';
    }

    // Trigger backend compile action
    fetch("/Editor/Compile", {
        method: "POST",
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({
            code: editorContent,
            connectionId: connectionId,
            lexical: lexicalEnabled
        })
    });
});

document.getElementById("compileAndRunButton").addEventListener("click", async function () {
    const editorContent = editor.getValue();
    const fileName = document.getElementById("fileNameInputDesktop").value;
    const lexicalEnabled = document.getElementById("toggleLexicalAnalyzer").checked;

    // Sync editor content to hidden input if needed
    document.getElementById("InputText").value = editorContent;

    // Clear right editor
    if (typeof rightEditor !== 'undefined') {
        rightEditor.setValue("");
    }

    // Reset console output
    const output = document.getElementById("compilerOutput");
    if (output) {
        output.innerHTML = '<span id="blinkingArrow">ProtonCompiler &gt;&gt; </span>';
    }

    // Prepare form data
    const formData = new FormData();
    formData.append("Code", editorContent);
    formData.append("ConnectionId", connectionId);
    formData.append("FileName", fileName);
    formData.append("Lexical", lexicalEnabled);

    // Post to backend
    const response = await fetch("/Editor/CompileAndRun", {
        method: "POST",
        body: formData, // do NOT set Content-Type manually
    });
});

document.getElementById("clearButton").addEventListener("click", function () {
        const confirmed = confirm("Are you sure you want to clear both the editor and console output?");
    if (!confirmed) return;

    // Trigger backend clear action
    fetch("/Editor/Clear", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: new URLSearchParams({ connectionId: connectionId })
    });
});