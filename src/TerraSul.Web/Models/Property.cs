namespace TerraSul.Web.Models;

public enum PropertyType { Casa, Apartamento, Cobertura, Terreno, Fazenda, Comercial }
public enum ListingPurpose { Venda, Aluguel }

public sealed record PropertyImage(string Url, string Alt);

public sealed class Property
{
    public int Id { get; init; }
    public required string Reference { get; init; }
    public required string Slug { get; init; }
    public required string Title { get; init; }
    public required string Tagline { get; init; }
    public PropertyType Type { get; init; }
    public ListingPurpose Purpose { get; init; } = ListingPurpose.Venda;
    public decimal Price { get; init; }
    public required string Neighborhood { get; init; }
    public string City { get; init; } = "Balsas";
    public string State { get; init; } = "MA";
    public int Area { get; init; }
    public int? LandArea { get; init; }
    public int Bedrooms { get; init; }
    public int Suites { get; init; }
    public int Bathrooms { get; init; }
    public int Garages { get; init; }
    public required string Description { get; init; }
    public IReadOnlyList<string> Features { get; init; } = [];
    public IReadOnlyList<PropertyImage> Images { get; init; } = [];
    public bool Featured { get; init; }
    public bool Exclusive { get; init; }
    public string? Development { get; init; }
    public int YearBuilt { get; init; }

    public PropertyImage Cover => Images[0];
    public string TypeLabel => Type switch
    {
        PropertyType.Casa => "Casa",
        PropertyType.Apartamento => "Apartamento",
        PropertyType.Cobertura => "Cobertura",
        PropertyType.Terreno => "Terreno",
        PropertyType.Fazenda => "Fazenda",
        PropertyType.Comercial => "Comercial",
        _ => Type.ToString()
    };
    /// <summary>Price 0 means the agency publishes it as "valor a consultar".</summary>
    public bool PriceOnRequest => Price <= 0;
    public string PriceLabel => PriceOnRequest
        ? "Sob consulta"
        : Purpose == ListingPurpose.Aluguel ? $"R$ {Price:N0}/mês" : $"R$ {Price:N0}";
    public string PricePerSqm => Area > 0 && !PriceOnRequest ? $"R$ {Price / Area:N0}/m²" : "";
    /// <summary>Built area for a house, land in hectares for a farm (which has no built area to quote).</summary>
    public string SizeLabel => Area > 0
        ? $"{Area:N0} m²"
        : LandArea is { } la ? (la >= 10_000 ? $"{la / 10_000m:N0} ha" : $"{la:N0} m²") : "";
}
