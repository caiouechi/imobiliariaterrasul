using TerraSul.Web.Models;

namespace TerraSul.Web.Services;

public interface ICatalogService
{
    IReadOnlyList<Property> All { get; }
    IReadOnlyList<Development> Developments { get; }
    IReadOnlyList<Testimonial> Testimonials { get; }
    IReadOnlyList<InstagramPost> Instagram { get; }
    IReadOnlyList<string> Neighborhoods { get; }
    (decimal floor, decimal ceiling) PriceRange { get; }
    Property? BySlug(string slug);
    Development? DevelopmentBySlug(string slug);
    IReadOnlyList<Property> Search(PropertyFilter filter);
    IReadOnlyList<Property> Similar(Property p, int take = 3);
}
