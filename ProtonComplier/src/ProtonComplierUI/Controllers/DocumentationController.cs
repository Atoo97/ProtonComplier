using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProtonComplierUI.Models;

namespace ProtonComplierUI.Controllers;

public class DocumentationController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public DocumentationController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Docu()
    {
        return View();
    }

    public IActionResult Reference()
    {
        ViewData["ActiveLink"] = "Reference";
        return View();
    }

    public IActionResult ErrorsAndWarnings()
    {
        ViewData["ActiveLink"] = "Errors&Warnings";
        return View();
    }

    public IActionResult Guides()
    {
        ViewData["ActiveLink"] = "Guides";
        return View();
    }

    [HttpGet]
    public IActionResult Editor()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
