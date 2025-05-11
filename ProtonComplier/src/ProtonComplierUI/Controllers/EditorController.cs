using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProtonComplierUI.Models;
using Microsoft.AspNetCore.SignalR;
using ProtonComplierUI.Hubs;
using Proton.Lexer.Services;
using Proton.Parser.Service;
using Proton.Lexer.Enums;
using Proton.Lexer;
using Proton.ErrorHandler;
using System.Text;
using Proton.Parser.Statements;
using Newtonsoft.Json.Linq;
using Proton.Semantic.Services;
using Proton.CodeGenerator.Services;
using Proton.CodeGenerator;
using Proton.Parser;
using System.Reflection;
using Proton.Semantic;

namespace ProtonComplierUI.Controllers;

public class EditorController : Controller
{
    private readonly IHubContext<CompilerHub> hubContext;
    private readonly LexicalService lexicalService;
    private readonly ParserService parserService;
    private readonly SemanticService semanticService;
    private readonly CodeGeneratorService codeGeneratorService;
    private readonly string path;
    private readonly object _sbLock;

    public EditorController(LexicalService lexicalService, ParserService parserService, SemanticService semanticService, CodeGeneratorService codeGeneratorService, IHubContext<CompilerHub> hubContext)
    {
        this.hubContext = hubContext;
        this.lexicalService = lexicalService;
        this.parserService = parserService;
        this.semanticService = semanticService;
        this.codeGeneratorService = codeGeneratorService;
        path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "ProtonTemplate.txt");
        _sbLock = new();
    }

    public async Task<IActionResult> Index(EditorViewModel model)
    {
        try
        {
            model = await CreateDefaultEditorViewModelAsync();
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "Failed to load the default template file.";
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Compile([FromForm] CompileRequest request)
    {
        await Task.Delay(500);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CompileAndRun([FromForm] CompileRequest request)
    {
        // Insted of: Clients.All.. this now support multiple users simultaneously:
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Compiling ({request.FileName}.prtn) started..");
        await Task.Delay(2000);

        // ===== LEXICAL ANALYSIS =====
        var lexicalresult = await LexicalAnalyze(request);

        if (lexicalresult.isSuccessful)
        {
            // ===== SYNTAX ANALYSIS =====
            var syntaxresult = await SyntaxAnalyze(request, lexicalresult);

            if (syntaxresult.isSuccessful)
            {
                // ===== SEMANTIC ANALYSIS =====
                var semanticalresult = await SemanticAnalyze(request, syntaxresult);

                if (semanticalresult.isSuccessful)
                {
                    // ===== CODE GENERATION =====
                    await CodeGeneration(request, semanticalresult);
                    return Ok();
                }
                return BadRequest();
            }
            return BadRequest();
        }
        return BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> Clear([FromForm] CompileRequest request)
    {
        if (!System.IO.File.Exists(path))
        {
            return BadRequest();
        }

        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ResetEditor", await System.IO.File.ReadAllTextAsync(path), "// Output C# code display here\n", "Main");
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromForm] CompileRequest request)
    {
        if (request.File != null && request.File.Length > 0)
        {
            using var reader = new StreamReader(request.File.OpenReadStream());
            string fileContent = await reader.ReadToEndAsync();

            // Set the file content to the model so it shows up in the editor
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ResetEditor", fileContent, "// Output C# code display here\n", request.FileName);

            return Ok();
        }

        return BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> Download([FromForm] CompileRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest("No input text provided.");
        }

        var fileBytes = System.Text.Encoding.UTF8.GetBytes(request.Code);
        await Task.Delay(500);
        return File(fileBytes, "application/octet-stream");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }


    // -------- Helper Methods --------
    private async Task<EditorViewModel> CreateDefaultEditorViewModelAsync()
    {
        var model = new EditorViewModel();

        if (!System.IO.File.Exists(path))
            throw new FileNotFoundException("Default template file not found.", path);

        model.InputText = await System.IO.File.ReadAllTextAsync(path);
        model.OutputText = "// Output C# code display here";
        model.FileName = "Main";

        return model;
    }

    public async Task<LexicalResult> LexicalAnalyze([FromForm] CompileRequest request)
    {
        var output = new StringBuilder();
        var errors = new StringBuilder();
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var lexicalresult = lexicalService.Complie(request.Code);
        stopwatch.Stop();
        var lexicalelapsedTime = stopwatch.Elapsed;
        var timeMessage = $"[ProtonCompiler]: Total Lexical Analysis time: {lexicalelapsedTime.TotalSeconds:F8} seconds";

        string statusMessage = lexicalresult.isSuccessful
        ? $"[ProtonComplier]: Lexical Analyzing ({request.FileName}.prtn) complete | Status: Succesfull"
        : $"[ProtonComplier]: Lexical Analyzing ({request.FileName}.prtn) complete | Status: Denied";

        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", statusMessage);
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);

        AppendErrorMessage(errors, lexicalresult.errors);
        AppendErrorMessage(errors, lexicalresult.warnings);
        await Task.Delay(500);

        if (lexicalresult.isSuccessful)
        {
            if (request.Lexical)
            {
                lock (_sbLock)
                {
                    output.AppendLine($"|===== LEXICAL ANALYSIS RESULTS =====|");
                }
                MacroType.ExpectedMacros
                    .Where(macro => lexicalresult.sections.ContainsKey(macro.Value))
                    .ToList()
                    .ForEach(macro => AppendMacroSection(output, macro.Value, lexicalresult.sections[macro.Value]));

                await hubContext.Clients.Client(request.ConnectionId).SendAsync("RightEditorOutput", output.ToString());
            }

            if (errors.ToString().Split('\n').Length - 1 > 1)
            {
                // More than one AppendLine() has been called
                await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            }
            return lexicalresult;
        }
        else
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            return lexicalresult;
        }
    }

    public async Task<ParserResult> SyntaxAnalyze([FromForm] CompileRequest request, LexicalResult result)
    {
        var output = new StringBuilder();
        var errors = new StringBuilder();
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var parserresult = parserService.Complie(result.sections);
        stopwatch.Stop();
        var parseelapsedTime = stopwatch.Elapsed;
        var timeMessage = $"[ProtonCompiler]: Total Syntax Analysis time: {parseelapsedTime.TotalSeconds:F8} seconds";

        string statusMessage = parserresult.isSuccessful
        ? $"[ProtonComplier]: Syntax Analyzing ({request.FileName}.prtn) complete | Status: Succesfull"
        : $"[ProtonComplier]: Syntax Analyzing ({request.FileName}.prtn) complete | Status: Denied";

        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", statusMessage);
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);

        AppendErrorMessage(errors, parserresult.errors);
        AppendErrorMessage(errors, parserresult.warnings);
        await Task.Delay(500);

        if (parserresult.isSuccessful)
        {
            if (request.Syntax)
            {
                lock (_sbLock)
                {
                    output.AppendLine($"|===== SYNTAX ANALYSIS RESULTS =====|");
                }
                MacroType.ExpectedMacros
                    .Where(macro => parserresult.sections.ContainsKey(macro.Value))
                    .ToList()
                    .ForEach(macro => AppendMacroSection(output, macro.Value, parserresult.sections[macro.Value]));

                await hubContext.Clients.Client(request.ConnectionId).SendAsync("RightEditorOutput", output.ToString());
            }

            if (errors.ToString().Split('\n').Length - 1 > 1)
            {
                // More than one AppendLine() has been called
                await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            }
            return parserresult;
        }
        else
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            return parserresult;
        }
    }

    public async Task<SemanticResult> SemanticAnalyze([FromForm] CompileRequest request, ParserResult result)
    {
        var output = new StringBuilder();
        var errors = new StringBuilder();
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var semanticresult = semanticService.Complie(result.sections);
        stopwatch.Stop();
        var semanticelapsedTime = stopwatch.Elapsed;
        var timeMessage = $"[ProtonCompiler]: Total Semantic Analysis time: {semanticelapsedTime.TotalSeconds:F8} seconds";

        string statusMessage = semanticresult.isSuccessful
        ? $"[ProtonComplier]: Semantic Analyzing ({request.FileName}.prtn) complete | Status: Succesfull"
        : $"[ProtonComplier]: Semantic Analyzing ({request.FileName}.prtn) complete | Status: Denied";

        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", statusMessage);
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);

        AppendErrorMessage(errors, semanticresult.errors);
        AppendErrorMessage(errors, semanticresult.warnings);
        await Task.Delay(500);

        if (semanticresult.isSuccessful)
        {
            if (request.Semantical)
            {
                lock (_sbLock)
                {
                    output.AppendLine($"|===== SEMANTIC ANALYSIS RESULTS =====|");
                    output.AppendLine(semanticresult.table.DisplaySymbols());
                }

                await hubContext.Clients.Client(request.ConnectionId).SendAsync("RightEditorOutput", output.ToString());
            }

            if (errors.ToString().Split('\n').Length - 1 > 1)
            {
                // More than one AppendLine() has been called
                await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            }
            return semanticresult;
        }
        else
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            return semanticresult;
        }
    }

    public async Task CodeGeneration([FromForm] CompileRequest request, SemanticResult result)
    {
        var output = new StringBuilder();
        var errors = new StringBuilder();
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var codegenresult = await codeGeneratorService.GenerateAndExecute(result.table);
        stopwatch.Stop();
        var codegenelapsedTime = stopwatch.Elapsed;
        var timeMessage = $"[ProtonCompiler]: Total Code Generation time: {codegenelapsedTime.TotalSeconds:F8} seconds";

        string statusMessage = codegenresult.isSuccessful
        ? $"[ProtonComplier]: Code Generation ({request.FileName}.prtn) complete | Status: Succesfull"
        : $"[ProtonComplier]: Code Generation ({request.FileName}.prtn) complete | Status: Denied";

        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", statusMessage);
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
        await Task.Delay(500);
        lock (_sbLock)
        {
            // output.AppendLine($"|===== CODE GENERATION RESULTS =====|");
            output.AppendLine(codegenresult.code);
        }
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("RightEditorCodeOutput", output.ToString());

        if (codegenresult.isSuccessful)
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Code Running result: {codegenresult.result}");
        }
        else
        {
            var text = string.Join(Environment.NewLine, codegenresult.errors.Select(e => $"⚠️ {e}"));
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Code Running errors: {text}");
        }

        if (errors.ToString().Split('\n').Length - 1 > 1)
        {
            // More than one AppendLine() has been called
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
        }
    }

    void AppendErrorMessage(StringBuilder sb, IEnumerable<BaseException> messageOutput)
    {
        lock (_sbLock)
        {
            if (sb.Length == 0)
            {
                sb.AppendLine("|===== ERRORS & WARNINGS =====|");
            }
            messageOutput.ToList().ForEach(msg => sb.AppendLine($"\u26A0 {msg}"));
        }
    }

    void AppendErrorMessage(StringBuilder sb, IEnumerable<string> messageOutput)
    {
        lock (_sbLock)
        {
            if (sb.Length == 0)
            {
                sb.AppendLine("|===== ERRORS & WARNINGS =====|");
            }
            messageOutput.ToList().ForEach(msg => sb.AppendLine($"\u26A0 {msg}"));
        }
    }

    void AppendMacroSection(StringBuilder sb, string macroName, IEnumerable<Token> tokens)
    {
        lock (_sbLock)
        {
            sb.AppendLine($"MACRO SECTION: {macroName}");

            tokens
                .Where(token => token.TokenType is not TokenType.Whitespace and not TokenType.Newline)
                .ToList()
                .ForEach(token => sb.AppendLine(token.DisplayToken(true)));

            sb.AppendLine();
        }
    }

    void AppendMacroSection(StringBuilder sb, string macroName, IEnumerable<Statement> statements)
    {
        lock (_sbLock)
        {
            sb.Append($"MACRO SECTION: {macroName}");

            statements
                .ToList()
                .ForEach(Statement => sb.AppendLine(Statement.ToCode()));

            sb.AppendLine();
        }
    }
}
