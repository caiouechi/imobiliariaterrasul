using Microsoft.AspNetCore.Mvc;

namespace TerraSul.Web.Models;

public sealed class PropertyFilter
{
    [FromQuery(Name = "q")] public string? Query { get; set; }
    [FromQuery(Name = "tipo")] public PropertyType? Type { get; set; }
    [FromQuery(Name = "finalidade")] public ListingPurpose? Purpose { get; set; }
    [FromQuery(Name = "bairro")] public string? Neighborhood { get; set; }
    [FromQuery(Name = "min")] public decimal? MinPrice { get; set; }
    [FromQuery(Name = "max")] public decimal? MaxPrice { get; set; }
    [FromQuery(Name = "quartos")] public int? MinBedrooms { get; set; }
    [FromQuery(Name = "vagas")] public int? MinGarages { get; set; }
    [FromQuery(Name = "area")] public int? MinArea { get; set; }
    [FromQuery(Name = "ordem")] public string Sort { get; set; } = "destaque";
    [FromQuery(Name = "view")] public string View { get; set; } = "grid";
}

public sealed class PropertyListViewModel
{
    public required PropertyFilter Filter { get; init; }
    public required IReadOnlyList<Property> Items { get; init; }
    public required IReadOnlyList<string> Neighborhoods { get; init; }
    public decimal PriceFloor { get; init; }
    public decimal PriceCeiling { get; init; }
    public int Total { get; init; }
}

public sealed class PropertyDetailsViewModel
{
    public required Property Property { get; init; }
    public required IReadOnlyList<Property> Similar { get; init; }
    public Development? Development { get; init; }
}

public sealed class HomeViewModel
{
    public required IReadOnlyList<Property> Featured { get; init; }
    public required IReadOnlyList<Property> Latest { get; init; }
    public required IReadOnlyList<Development> Developments { get; init; }
    public required IReadOnlyList<Testimonial> Testimonials { get; init; }
    public required IReadOnlyList<InstagramPost> Instagram { get; init; }
    public required IReadOnlyList<string> Neighborhoods { get; init; }
    public decimal PriceFloor { get; init; }
    public decimal PriceCeiling { get; init; }
}

public sealed class ContactForm
{
    public string Nome { get; set; } = "";
    public string Telefone { get; set; } = "";
    public string? Email { get; set; }
    public string Interesse { get; set; } = "Comprar";
    public string? Imovel { get; set; }
    public string Mensagem { get; set; } = "";
}
