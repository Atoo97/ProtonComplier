using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProtonComplierUI.Models;
using Microsoft.AspNetCore.SignalR;
using ProtonComplierUI.Hubs;
using Proton.Lexer.Services;
using Proton.Lexer.Enums;
using Proton.Lexer;
using Proton.ErrorHandler;
using System.Text;

namespace ProtonComplierUI.Controllers;

public class EditorController : Controller
{
    private readonly IHubContext<CompilerHub> hubContext;
    private readonly LexicalService lexicalService;
    private readonly string path;

    public EditorController(LexicalService lexicalService, IHubContext<CompilerHub> hubContext)
    {
        this.hubContext = hubContext;
        this.lexicalService = lexicalService;
        path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "ProtonTemplate.txt");
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
        var result = lexicalService.Compile(request.Code);
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
        await hubContext.Clients.Client(request.ConnectionId).SendAsync("ConsoleOutput", $"[ProtonComplier]: Compiling ({request.FileName}.ptrn)");
        await Task.Delay(1000);
        stopwatch.Start();

        // ===== LEXICAL ANALYSIS =====
        var result = lexicalService.Compile(request.Code);
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

        sb.AppendLine(isError ? "|===== ERRORS =====|" : "|===== WARNINGS =====|");
        messageOutput.ToList().ForEach(msg => sb.AppendLine($"\u26A0 {msg}"));
        sb.AppendLine();
    }


    void AppendMacroSection(StringBuilder sb, string macroName, IEnumerable<Token> tokens)
    {
        sb.AppendLine($"|~~~~~ MACRO SECTION: {macroName} ~~~~~|");

        tokens
            .Where(token => token.TokenType is not TokenType.Whitespace and not TokenType.Newline)
            .ToList()
            .ForEach(token => sb.AppendLine(token.DisplayToken(true)));

        sb.AppendLine();
    }
}
