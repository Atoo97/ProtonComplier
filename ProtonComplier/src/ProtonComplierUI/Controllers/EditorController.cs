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
            ViewBag.DefaultFileName = "Main";
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
        await Task.Delay(1000);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CompileAndRun([FromForm] CompileRequest request)
    {
        var output = new StringBuilder();
        var errors = new StringBuilder();
        var stopwatch = new Stopwatch();
        var analyzetype = "Lexical";

        // Insted of: Clients.All.. this now support multiple users simultaneously:
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Compiling ({request.FileName}.prtn)");
        await Task.Delay(2000);

        // ===== LEXICAL ANALYSIS =====
        stopwatch.Start();
        var lexicalresult = lexicalService.Complie(request.Code);
        stopwatch.Stop();
        var lexicalelapsedTime = stopwatch.Elapsed;
        var timeMessage = $"[ProtonComplier]: Total {analyzetype} Analysis time: {lexicalelapsedTime.TotalSeconds:F8} seconds";

        string statusMessage = lexicalresult.isSuccessful
        ? $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Succesfull"
        : $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Denied";

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

                await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", output.ToString());
            }

            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
        }
        else
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            return BadRequest();
        }

        // ===== SYNTAX ANALYSIS =====
        analyzetype = "Syntax";
        stopwatch.Start();
        var parserresult = parserService.Complie(lexicalresult.sections);
        stopwatch.Stop();
        var parserelapsedTime = stopwatch.Elapsed;
        parserelapsedTime -= lexicalelapsedTime;
        timeMessage = $"[ProtonComplier]: Total {analyzetype} Analysis time: {parserelapsedTime.TotalSeconds:F8} seconds";

        statusMessage = parserresult.isSuccessful
           ? $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Succesfull"
           : $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Denied";

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

                await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", output.ToString());
            }

            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
        }
        else
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            return BadRequest();
        }

        // ===== SEMANTIC ANALYSIS =====
        analyzetype = "Semantic";
        stopwatch.Start();
        var semanticresult = semanticService.Complie(parserresult.sections);
        stopwatch.Stop();
        var semanticelapsedTime = stopwatch.Elapsed;
        semanticelapsedTime -= (lexicalelapsedTime + parserelapsedTime);
        timeMessage = $"[ProtonComplier]: Total {analyzetype} Analysis time: {semanticelapsedTime.TotalSeconds:F8} seconds";

        statusMessage = semanticresult.isSuccessful
           ? $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Succesfull"
           : $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Denied";

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

                await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", output.ToString());
            }

            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
        }
        else
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
            return BadRequest();
        }

        // ===== CODE GENERATION =====
        analyzetype = "Code Generation";
        stopwatch.Start();
        var codegenresult = await codeGeneratorService.GenerateAndExecute(semanticresult.table);
        stopwatch.Stop();
        var codegenelapsedTime = stopwatch.Elapsed;
        codegenelapsedTime -= (lexicalelapsedTime + parserelapsedTime + semanticelapsedTime);
        timeMessage = $"[ProtonComplier]: Total {analyzetype} Analysis time: {codegenelapsedTime.TotalSeconds:F8} seconds";

        statusMessage = codegenresult.isSuccessful
           ? $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Succesfull"
           : $"[ProtonComplier]: {analyzetype} Analyzing ({request.FileName}.prtn) complete | Status: Denied";

        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", statusMessage);
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
        await Task.Delay(500);
        lock (_sbLock)
        {
            output.AppendLine($"|===== CODE GENERATION RESULTS =====|");
            output.AppendLine(codegenresult.code);
        }
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", output.ToString());

        if (codegenresult.isSuccessful)
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Code Running result: {codegenresult.result}");
        }
        else
        {
            var text = string.Join(Environment.NewLine, codegenresult.errors.Select(e => $"⚠️ {e}"));
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Code Running errors: {text}");
            return BadRequest();
        }

        if (errors.Length > 1)
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ErrorsAndWarningsOutput", errors.ToString());
        }
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Clear([FromForm] CompileRequest request)
    {
        if (!System.IO.File.Exists(path))
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", "❌ Failed to reset: Default template not found.");
            return BadRequest();
        }

        // Send both default input and output
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ResetEditor", await System.IO.File.ReadAllTextAsync(path), "// Output C# code display here\n");
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", "🧹 Editor has been reset to default template.");

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(EditorViewModel model)
    {
        await Task.Delay(1000);
        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Download(EditorViewModel model)
    {
        await Task.Delay(1000);
        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Copy(EditorViewModel model)
    {
        await Task.Delay(1000);
        return View("Index", model);
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

        if (TempData["LexicalViewModel"] is not null)
        {
            model.InputText = TempData["LexicalViewModel"]!.ToString();
        }
        else
        {
            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException("Default template file not found.", path);

            model.InputText = await System.IO.File.ReadAllTextAsync(path);
            model.OutputText = "// Output C# code display here";
        }

        return model;
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
