using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProtonComplierUI.Models;
using Microsoft.AspNetCore.SignalR;
using ProtonComplierUI.Hubs;

namespace ProtonComplierUI.Controllers;

public class EditorController : Controller
{
    private readonly IHubContext<CompilerHub> _hubContext;

    public EditorController(IHubContext<CompilerHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task<IActionResult> Index(EditorViewModel model)
    {
        try
        {
            // Set Editor Input and Output text
            if (TempData["LexicalViewModel"] is not null)
            {
                model.InputText = TempData["LexicalViewModel"]!.ToString();
            }
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "ProtonTemplate.txt");

                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException("Default template file not found.", path);

                model.InputText = await System.IO.File.ReadAllTextAsync(path);
                model.OutputText = "// Output C# code display here\n";
            }
        }
        catch (Exception)
        {
            // Log error if needed: _logger.LogError(ex, "Failed to load default template.");
            TempData["ErrorMessage"] = "Failed to load the default template file.";
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Compile(EditorViewModel model)
    {
        await Task.Delay(1000);
        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> CompileAndRun([FromForm] string code, [FromForm] string connectionId)
    {
        // Insted of: Clients.All.. this now support multiple users simultaneously:
        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveOutput", $"Code from complier {code}");
        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveOutput", "🔧 Compilation started...");
        await Task.Delay(1000);
        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveOutput", "📄 Lexical Analyzing syntax...");
        await Task.Delay(1000);
        await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveOutput", "✅ Compilation successful!");

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Clear(EditorViewModel model)
    {
        await Task.Delay(1000);
        return View("Index", model);
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
}
