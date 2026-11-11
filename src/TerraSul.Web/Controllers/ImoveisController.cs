using Microsoft.AspNetCore.Mvc;
using TerraSul.Web.Models;
using TerraSul.Web.Services;

namespace TerraSul.Web.Controllers;

public class ImoveisController(ICatalogService catalog) : Controller
{
    public IActionResult Index(PropertyFilter filter) => View(Build(filter));

    /// <summary>Partial grid used by the AJAX filter on the listing page.</summary>
    public IActionResult Grid(PropertyFilter filter) => PartialView("_PropertyGrid", Build(filter));

    public IActionResult Detalhes(string slug)
    {
        var p = catalog.BySlug(slug);
        if (p is null) return NotFound();
        return View(new PropertyDetailsViewModel
        {
            Property = p,
            Similar = catalog.Similar(p),
            Development = p.Development is null ? null : catalog.Developments.FirstOrDefault(d => d.Name == p.Development)
        });
    }

    private PropertyListViewModel Build(PropertyFilter filter)
    {
        var items = catalog.Search(filter);
        return new PropertyListViewModel
        {
            Filter = filter,
            Items = items,
            Neighborhoods = catalog.Neighborhoods,
            PriceFloor = catalog.PriceRange.floor,
            PriceCeiling = catalog.PriceRange.ceiling,
            Total = items.Count
        };
    }
}
