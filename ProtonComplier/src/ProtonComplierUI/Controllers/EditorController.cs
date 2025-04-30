using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProtonComplierUI.Models;

namespace ProtonComplierUI.Controllers;

public class EditorController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public EditorController(ILogger<HomeController> logger)
    {
        _logger = logger;
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
                model.OutputText = "// Output C# code display here";
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


        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> CompileAndRun(EditorViewModel model)
    {


        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Clear(EditorViewModel model)
    {


        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(EditorViewModel model)
    {


        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Download(EditorViewModel model)
    {


        return View("Index", model);
    }

    [HttpPost]
    public async Task<IActionResult> Copy(EditorViewModel model)
    {


        return View("Index", model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
