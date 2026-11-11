namespace TerraSul.Web.Models;

public sealed class Development
{
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string Tagline { get; init; }
    public required string Description { get; init; }
    public required string Cover { get; init; }
    public IReadOnlyList<PropertyImage> Gallery { get; init; } = [];
    public IReadOnlyList<string> Highlights { get; init; } = [];
    public required string Location { get; init; }
    /// <summary>Payment conditions exactly as the agency publishes them (entrada, parcelas).</summary>
    public string? Conditions { get; init; }
    public string Status { get; init; } = "Lançamento";
}

public sealed record Testimonial(string Name, string Role, string Quote);

public sealed record InstagramPost(string Image, string Caption, string Url);

public static class SiteInfo
{
    public const string Name = "Imobiliária Terra Sul";
    public const string Creci = "CRECI J-230";
    public const string City = "Balsas · Maranhão";
    public const string WhatsAppDigits = "5599984468625";
    public const string WhatsAppDisplay = "+55 (99) 98446-8625";
    public const string Instagram = "https://www.instagram.com/imobiliariaterrasulbls/";
    public const string InstagramHandle = "@imobiliariaterrasulbls";
    public const string Email = "contato@imobiliariaterrasul.com.br";
    public const string Hours = "Segunda a Sexta · 8h às 18h  |  Sábado · 8h às 12h";

    public static string WhatsAppLink(string? message = null)
        => "https://wa.me/" + WhatsAppDigits + (string.IsNullOrWhiteSpace(message) ? "" : "?text=" + Uri.EscapeDataString(message));
}
