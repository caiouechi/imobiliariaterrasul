using Microsoft.AspNetCore.Mvc;
using TerraSul.Web.Services;

namespace TerraSul.Web.Controllers;

public class EmpreendimentosController(ICatalogService catalog) : Controller
{
    public IActionResult Index() => View(catalog.Developments);

    public IActionResult Detalhes(string slug)
    {
        var d = catalog.DevelopmentBySlug(slug);
        if (d is null) return NotFound();
        ViewBag.Properties = catalog.All.Where(p => p.Development == d.Name).ToList();
        return View(d);
    }
}
