using Microsoft.AspNetCore.Mvc;
using TerraSul.Web.Services;

namespace TerraSul.Web.Controllers;

public class SobreController(ICatalogService catalog) : Controller
{
    public IActionResult Index() => View(catalog.Testimonials);
}
