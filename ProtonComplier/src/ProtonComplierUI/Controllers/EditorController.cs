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
        _sbLock = new ();
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
        var sb = new StringBuilder();
        var stopwatch = new Stopwatch();

        // Insted of: Clients.All.. this now support multiple users simultaneously:
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Compiling ({request.FileName}.ptrn)");
        await Task.Delay(1000);
        stopwatch.Start();

        // ===== LEXICAL ANALYSIS =====
        var result = lexicalService.Complie(request.Code);
        var elapsedTime = stopwatch.Elapsed;
        var timeMessage = $"🕒 Total Lexical Analysis time: {elapsedTime.TotalSeconds:F8} seconds";

        if (result.isSuccessful)
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", "[ProtonComplier]: LexicalAnalyzing (Main.prtn) complete | Status: SuccessFull");
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
            AppendErrorMessage(sb, result.warnings, false);

            if (request.Lexical)
            {
                sb.AppendLine($"|===== LEXICAL ANALYSIS =====|");  // Append sections for each expected macro
                MacroType.ExpectedMacros
                    .Where(macro => result.sections.ContainsKey(macro.Value))
                    .ToList()
                    .ForEach(macro => AppendMacroSection(sb, macro.Value, result.sections[macro.Value]));
            }

            // ===== SYNTAX ANALYSIS (Add your logic if syntax == true) =====
            // ===== SEMANTICAL ANALYSIS (Add your logic if semantical == true) =====

            await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", sb.ToString());
        }
        else
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", "[ProtonComplier]: LexicalAnalyzing (Main.prtn) complete | Status: Denied");
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
            AppendErrorMessage(sb, result.errors, true);
            AppendErrorMessage(sb, result.warnings, false);
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", sb.ToString());
        }

        stopwatch.Stop();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CompileAndRun([FromForm] CompileRequest request)
    {
        var sb = new StringBuilder();
        var stopwatch = new Stopwatch();

        // Insted of: Clients.All.. this now support multiple users simultaneously:
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Compiling ({request.FileName}.ptrn) started...");
        await Task.Delay(2000);
        stopwatch.Start();

        // ===== LEXICAL ANALYSIS =====
        var lexicalresult = lexicalService.Complie(request.Code);
        stopwatch.Stop();
        var elapsedTimeLexical = stopwatch.Elapsed;
        var timeMessage = $"[ProtonComplier]: Total Lexical Analysis time: {elapsedTimeLexical.TotalSeconds:F8} seconds";

        if (lexicalresult.isSuccessful)
        {
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: LexicalAnalyzing ({request.FileName}.ptrn) complete | Status: SuccessFull");
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
            AppendErrorMessage(sb, lexicalresult.warnings, false);

            if (request.Lexical) 
            {
                lock (_sbLock)
                {
                    sb.AppendLine($"|===== LEXICAL ANALYSIS =====|");  // Append sections for each expected macro
                }
                MacroType.ExpectedMacros
                    .Where(macro => lexicalresult.sections.ContainsKey(macro.Value))
                    .ToList()
                    .ForEach(macro => AppendMacroSection(sb, macro.Value, lexicalresult.sections[macro.Value]));

                await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", sb.ToString());
            }

            // ===== SYNTAX ANALYSIS =====
            await Task.Delay(1000);
            stopwatch.Start();
            var parserresult = parserService.Complie(lexicalresult.sections);
            stopwatch.Stop();
            var elapsedTimeParser = stopwatch.Elapsed;
            elapsedTimeParser -= elapsedTimeLexical;
            timeMessage = $"[ProtonComplier]: Total Syntax Analysis time: {elapsedTimeParser.TotalSeconds:F8} seconds";

            if (parserresult.isSuccessful)
            {
                await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: SyntaxAnalyzing ({request.FileName}.ptrn) complete | Status: SuccessFull");
                await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
                AppendErrorMessage(sb, parserresult.warnings, false);

                if (request.Syntax)
                {
                    lock (_sbLock)
                    {
                        sb.AppendLine($"|===== SYNTAX ANALYSIS =====|"); // Append sections for each expected macro
                    }
                    MacroType.ExpectedMacros
                        .Where(macro => parserresult.sections.ContainsKey(macro.Value))
                        .ToList()
                        .ForEach(macro => AppendMacroSection(sb, macro.Value, parserresult.sections[macro.Value]));

                    await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", sb.ToString());
                }

                // ===== SEMANTICAL ANALYSIS =====
                await Task.Delay(1000);
                stopwatch.Start();
                var semanticalresult = semanticService.Complie(parserresult.sections);
                stopwatch.Stop();
                var elapsedTimeSemantic = stopwatch.Elapsed;
                elapsedTimeSemantic -= (elapsedTimeLexical+elapsedTimeParser);
                timeMessage = $"[ProtonComplier]: Total Semantical Analysis time: {elapsedTimeSemantic.TotalSeconds:F8} seconds";

                if (semanticalresult.isSuccessful)
                {
                    await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: SemanticAnalyzing ({request.FileName}.ptrn) complete | Status: SuccessFull");
                    await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
                    AppendErrorMessage(sb, semanticalresult.warnings, false);

                    if (request.Semantical)
                    {
                        lock (_sbLock)
                        {
                            sb.AppendLine($"|===== SEMANTIC ANALYSIS =====|");
                        }
                        MacroType.ExpectedMacros
                            .Where(macro => semanticalresult.sections.ContainsKey(macro.Value))
                            .ToList()
                            .ForEach(macro => AppendMacroSection(sb, macro.Value, semanticalresult.sections[macro.Value]));     //TODO: to midfy display logic here

                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", sb.ToString());
                    }

                    // ===== CODE GENERATOR =====
                    await Task.Delay(1000);
                    stopwatch.Start();
                    var codegeneratorresult = await codeGeneratorService.GenerateAndExecute(semanticalresult.table);
                    stopwatch.Stop();
                    var elapsedTimeCodeGen = stopwatch.Elapsed;
                    elapsedTimeCodeGen -= (elapsedTimeLexical + elapsedTimeParser + elapsedTimeSemantic);
                    timeMessage = $"[ProtonComplier]: Total Code Generation time: {elapsedTimeCodeGen.TotalSeconds:F8} seconds";

                    if (codegeneratorresult.errors.Count == 0)
                    {
                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: CodeGeneeation ({request.FileName}.ptrn) complete | Status: SuccessFull");
                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);

                        lock (_sbLock)
                        {
                            sb.AppendLine($"|===== GENERATED C# CODE =====|");
                            sb.AppendLine(codegeneratorresult.code);
                        }
                        
                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", sb.ToString());
                        var text = $"[ProtonComplier]: Executed code result: {codegeneratorresult.result}";
                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", text);
                    }
                    else
                    {
                        await Task.Delay(500);
                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: CodeGeneration ({request.FileName}.ptrn) complete | Status: Denied");
                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", timeMessage);
                        lock (_sbLock)
                        {
                            sb.AppendLine($"|===== GENERATED C# CODE =====|");
                            sb.AppendLine(codegeneratorresult.code);
                        }

                        var text = $"[ProtonComplier]: Executed code errors:\n{string.Join("\n", codegeneratorresult.errors)}";
                        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput",text);
                    }
                }
                else
                {
                    await Task.Delay(500);
                    await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: SemanticAnalyzing ({request.FileName}.ptrn) complete | Status: Denied");
                    await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", elapsedTimeSemantic);
                    AppendErrorMessage(sb, semanticalresult.errors, true);
                    AppendErrorMessage(sb, semanticalresult.warnings, false);
                }
            }
            else
            {
                await Task.Delay(500);
                await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: SyntaxAnalyzing ({request.FileName}.ptrn) complete | Status: Denied");
                await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", elapsedTimeParser);
                AppendErrorMessage(sb, parserresult.errors, true);
                AppendErrorMessage(sb, parserresult.warnings, false);
            }
        }
        else
        {
            await Task.Delay(500);
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: LexicalAnalyzing ({request.FileName}.ptrn) complete | Status: Denied");
            await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", elapsedTimeLexical);
            AppendErrorMessage(sb, lexicalresult.errors, true);
            AppendErrorMessage(sb, lexicalresult.warnings, false);
        }

        await hubContext.Clients.Client(request.ConnectionId).SendAsync("EditorOutput", sb.ToString());
        stopwatch.Stop();
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


    void AppendErrorMessage(StringBuilder sb, IEnumerable<BaseException> messageOutput, bool isError)
    {
        if (!messageOutput.Any()) return;

        lock (_sbLock)
        {
            sb.AppendLine(isError ? "|===== ERRORS =====|" : "|===== WARNINGS =====|");
            messageOutput.ToList().ForEach(msg => sb.AppendLine($"\u26A0 {msg}"));
            sb.AppendLine();
        }
    }

    void AppendMacroSection(StringBuilder sb, string macroName, IEnumerable<Token> tokens)
    {
        lock (_sbLock)
        {
            sb.AppendLine($"~~~~~ MACRO SECTION: {macroName}");

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
            sb.AppendLine($"~~~~~ MACRO SECTION: {macroName}");

            statements
                .ToList()
                .ForEach(Statement => sb.AppendLine(Statement.ToCode()));

            sb.AppendLine();
        }
    }
}
