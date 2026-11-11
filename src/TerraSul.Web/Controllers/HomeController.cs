using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TerraSul.Web.Models;
using TerraSul.Web.Services;

namespace TerraSul.Web.Controllers;

public class HomeController(ICatalogService catalog) : Controller
{
    public IActionResult Index()
    {
        var vm = new HomeViewModel
        {
            Featured = catalog.All.Where(p => p.Featured).Take(6).ToList(),
            Latest = catalog.All.OrderByDescending(p => p.Id).Take(8).ToList(),
            Developments = catalog.Developments,
            Testimonials = catalog.Testimonials,
            Instagram = catalog.Instagram,
            Neighborhoods = catalog.Neighborhoods,
            PriceFloor = catalog.PriceRange.floor,
            PriceCeiling = catalog.PriceRange.ceiling
        };
        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
