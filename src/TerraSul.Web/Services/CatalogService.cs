using TerraSul.Web.Models;

namespace TerraSul.Web.Services;

/// <summary>
/// In-memory catalog. Swap for EF Core or a CRM feed later; controllers only depend on ICatalogService.
///
/// REAL LISTINGS (2026-11-10). Every property below is a post on @imobiliariaterrasulbls: the
/// photos are frames of those posts (wwwroot/img/real) and every number (land, built area,
/// suites, garage, price) is copied from the post's caption. Nothing is invented: a caption with
/// "valor a consultar" has Price = 0, which the site shows as "Sob consulta", and a fact the post
/// does not give (bathrooms, year) is left out rather than guessed. The Instagram post id sits
/// next to each one so the listing can be checked against the original.
/// </summary>
public sealed class CatalogService : ICatalogService
{
    private const string R = "/img/real/";
    private static PropertyImage Img(string file, string alt) => new(R + file + ".webp", alt);

    public IReadOnlyList<Property> All { get; }
    public IReadOnlyList<Development> Developments { get; }
    public IReadOnlyList<Testimonial> Testimonials { get; }
    public IReadOnlyList<InstagramPost> Instagram { get; }
    public IReadOnlyList<string> Neighborhoods { get; }
    public (decimal floor, decimal ceiling) PriceRange { get; }

    public CatalogService()
    {
        All = Seed();
        Developments = SeedDevelopments();
        // DEMO CONTENT: these quotes are placeholders written for the presentation, not real
        // clients. Replace them with real reviews (Google, WhatsApp) before the site goes live.
        Testimonials =
        [
            new("Fernanda & Rodrigo Almeida", "Compradores · Prime Ville", "Procurávamos uma casa que fosse ao mesmo tempo sofisticada e prática para a família. A Terra Sul entendeu exatamente o que queríamos e nos apresentou apenas opções que valiam a visita."),
            new("Dr. Marcelo Teixeira", "Investidor", "Já fiz quatro negócios com a Terra Sul. Transparência absoluta, documentação impecável e uma leitura de mercado que poucos têm em Balsas."),
            new("Ana Luiza Carvalho", "Proprietária", "Venderam minha casa em menos de 30 dias, por um valor acima do que eu esperava. Profissionalismo do início ao fim."),
            new("Família Sampaio", "Compradores · Prime Rio", "Da primeira visita à entrega das chaves, fomos acompanhados em cada detalhe. Uma experiência de compra à altura do imóvel."),
        ];
        Instagram =
        [
            new(R + "prime-ville-hidro.webp", "Residência no Prime Ville", SiteInfo.Instagram),
            new(R + "casa-pergolado-piscina.webp", "Lazer com pergolado em angelim", SiteInfo.Instagram),
            new(R + "prime-rio-portal.webp", "Prime Rio Residence", SiteInfo.Instagram),
            new(R + "interior-cozinha.webp", "Tour pelo imóvel", SiteInfo.Instagram),
            new(R + "prime-ville-piscina-privativa.webp", "Piscina privativa · Prime Ville", SiteInfo.Instagram),
            new(R + "prime-rio-entregue.webp", "Mais um imóvel entregue", SiteInfo.Instagram),
            new(R + "fazenda-rio-maravilha.webp", "Fazenda às margens do Rio Maravilha", SiteInfo.Instagram),
            new(R + "prime-ville-aerea.webp", "Prime Ville Residence", SiteInfo.Instagram),
        ];
        Neighborhoods = All.Select(p => p.Neighborhood).Distinct().OrderBy(n => n).ToList();
        var priced = All.Where(p => p.Purpose == ListingPurpose.Venda && p.Price > 0).ToList();
        PriceRange = (priced.Min(p => p.Price), priced.Max(p => p.Price));
    }

    public Property? BySlug(string slug) => All.FirstOrDefault(p => p.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
    public Development? DevelopmentBySlug(string slug) => Developments.FirstOrDefault(d => d.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Property> Search(PropertyFilter f)
    {
        IEnumerable<Property> q = All;
        if (!string.IsNullOrWhiteSpace(f.Query))
        {
            var s = f.Query.Trim();
            q = q.Where(p => p.Title.Contains(s, StringComparison.OrdinalIgnoreCase)
                          || p.Neighborhood.Contains(s, StringComparison.OrdinalIgnoreCase)
                          || p.Reference.Contains(s, StringComparison.OrdinalIgnoreCase)
                          || (p.Development?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (f.Type is { } t) q = q.Where(p => p.Type == t);
        if (f.Purpose is { } pu) q = q.Where(p => p.Purpose == pu);
        if (!string.IsNullOrWhiteSpace(f.Neighborhood)) q = q.Where(p => p.Neighborhood == f.Neighborhood);
        // "Sob consulta" (Price 0) has no number to compare, so a price filter keeps it rather
        // than hiding a real listing behind a range it may well fall inside.
        if (f.MinPrice is { } min) q = q.Where(p => p.Price == 0 || p.Price >= min);
        if (f.MaxPrice is { } max) q = q.Where(p => p.Price == 0 || p.Price <= max);
        if (f.MinBedrooms is { } b) q = q.Where(p => p.Bedrooms >= b);
        if (f.MinGarages is { } g) q = q.Where(p => p.Garages >= g);
        if (f.MinArea is { } a) q = q.Where(p => p.Area >= a);

        q = f.Sort switch
        {
            "menor-preco" => q.OrderBy(p => p.Price == 0 ? decimal.MaxValue : p.Price),
            "maior-preco" => q.OrderByDescending(p => p.Price),
            "maior-area" => q.OrderByDescending(p => p.Area),
            "recentes" => q.OrderByDescending(p => p.Id),
            _ => q.OrderByDescending(p => p.Featured).ThenByDescending(p => p.Price)
        };
        return q.ToList();
    }

    public IReadOnlyList<Property> Similar(Property p, int take = 3) =>
        All.Where(x => x.Id != p.Id && x.Purpose == p.Purpose)
           .OrderBy(x => x.Type == p.Type ? 0 : 1)
           .ThenBy(x => x.Development == p.Development ? 0 : 1)
           .ThenBy(x => Math.Abs(x.Price - p.Price))
           .Take(take).ToList();

    // Context pictures for a listing that has one photo of its own: the condominium it is in.
    private static readonly PropertyImage PrimeVilleAerial = Img("prime-ville-aerea", "Condomínio Prime Ville Residence");
    private static readonly PropertyImage PrimeRioPortal = Img("prime-rio-portal", "Portal do Prime Rio Residence");

    private static List<Property> Seed() =>
    [
        // IG 17975031234188160
        new()
        {
            Id = 1, Reference = "TS-101", Slug = "residencia-prime-rio-tres-suites-mobiliada",
            Title = "Residência Prime Rio", Tagline = "3 suítes, piscina e móveis planejados inclusos na venda",
            Type = PropertyType.Casa, Price = 1_800_000, Neighborhood = "Prime Rio", Development = "Prime Rio",
            Area = 180, LandArea = 300, Bedrooms = 3, Suites = 3,
            Featured = true,
            Description = "Residência de alto padrão no Prime Rio Residence, um dos condomínios mais desejados de Balsas. Terreno de 300 m² e 180 m² de área construída, com três suítes (uma com closet), sala integrada à cozinha e à área de lazer. Móveis planejados em todos os ambientes, inclusos na venda, ar-condicionado em todos os ambientes e sistema de energia solar. Imóvel financiável.",
            Features = ["Móveis planejados inclusos", "Ar-condicionado em todos os ambientes", "Energia solar", "Sala integrada à área de lazer", "Suíte com closet", "Financiável"],
            Images = [Img("prime-rio-piscina", "Piscina e área de lazer"), PrimeRioPortal]
        },
        // IG 18093642824555720
        new()
        {
            Id = 2, Reference = "TS-102", Slug = "residencia-prime-ville-piscina-privativa",
            Title = "Residência Prime Ville", Tagline = "300 m² construídos, piscina privativa e escritório",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Prime Ville", Development = "Prime Ville",
            Area = 300, LandArea = 450, Bedrooms = 3, Suites = 3,
            Featured = true,
            Description = "Uma casa pensada para quem busca sofisticação, conforto, privacidade e integração. São 450 m² de terreno e 300 m² de área construída no Condomínio Prime Ville, com três suítes (uma com closet), piscina privativa, escritório privativo e cozinha integrada à área de lazer.",
            Features = ["Piscina privativa", "Escritório privativo", "Suíte com closet", "Cozinha integrada à área de lazer", "Condomínio fechado"],
            Images = [Img("prime-ville-piscina-privativa", "Fachada"), PrimeVilleAerial]
        },
        // IG 18189835234378202
        new()
        {
            Id = 3, Reference = "TS-103", Slug = "casa-prime-ville-piscina-hidro",
            Title = "Casa Prime Ville Vidro", Tagline = "Piscina com hidro e móveis planejados inclusos",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Prime Ville", Development = "Prime Ville",
            Area = 240, LandArea = 450, Bedrooms = 3, Suites = 3, Garages = 2,
            Featured = true,
            Description = "Casa no Condomínio Prime Ville Residence em terreno de 15 x 30 m (450 m²) e 240 m² de área construída. Três suítes, sendo uma suíte master com closet, sala, copa e cozinha, área de lazer com churrasqueira e piscina com hidro. Garagem para dois veículos cobertos e móveis planejados inclusos.",
            Features = ["Piscina com hidro", "Churrasqueira", "Suíte master com closet", "Móveis planejados inclusos", "2 vagas cobertas"],
            Images = [Img("prime-ville-hidro", "Fachada"), PrimeVilleAerial]
        },
        // IG 17995976130032614
        new()
        {
            Id = 4, Reference = "TS-104", Slug = "casa-piscina-pergolado-angelim",
            Title = "Casa do Pergolado", Tagline = "400 m² construídos e lazer para 50 pessoas",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Balsas",
            Area = 400, LandArea = 560, Bedrooms = 3, Suites = 3, Garages = 4,
            Featured = true,
            Description = "Uma residência ampla e sofisticada, ideal para quem busca conforto, espaço e uma excelente área de lazer. Terreno de 560 m² e 400 m² construídos, com três suítes (a master com closet), cozinha ampla, lavanderia e despensa internas. Garagem para quatro carros com madeira de lei aparente, churrasqueira grande, pergolado em angelim, piscina grande e área de lazer para aproximadamente 50 pessoas, com banheiro.",
            Features = ["Piscina grande", "Pergolado em angelim", "Churrasqueira grande", "Lazer para cerca de 50 pessoas", "Garagem para 4 carros", "Lavanderia e despensa internas"],
            Images = [Img("casa-pergolado-piscina", "Piscina e pergolado")]
        },
        // IG 18126295378887898
        new()
        {
            Id = 5, Reference = "TS-105", Slug = "residencia-mobiliada-prime-ville",
            Title = "Residência Mobiliada Prime Ville", Tagline = "Toda mobiliada, com sacadas para a piscina",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Prime Ville", Development = "Prime Ville",
            Area = 257, LandArea = 450, Bedrooms = 3, Suites = 3, Garages = 2,
            Description = "Residência toda mobiliada no Condomínio Prime Ville, com 257 m² construídos em terreno de 450 m² (15 x 30). Sala de visita ampla, cozinha americana, escritório, lavabo e despensa, cozinha com churrasqueira e mesa de jantar voltada para a piscina. Uma suíte ampla com closet e sacada para a piscina, mais duas suítes completas e uma sacada ampla para o condomínio. Área de lazer com piscina e banheiro, lavanderia, alarme e placa solar.",
            Features = ["Toda mobiliada", "Escritório", "Sacada com vista para a piscina", "Alarme", "Placa solar", "2 vagas cobertas"],
            Images = [Img("prime-ville-mobiliada", "Fachada"), PrimeVilleAerial]
        },
        // IG 17935617528328380
        new()
        {
            Id = 6, Reference = "TS-106", Slug = "residencia-prime-ville-jacuzzi",
            Title = "Residência Prime Ville Jacuzzi", Tagline = "450 m² de terreno, 236 m² construídos",
            Type = PropertyType.Casa, Price = 2_100_000, Neighborhood = "Prime Ville", Development = "Prime Ville",
            Area = 236, LandArea = 450, Bedrooms = 3, Suites = 1,
            Featured = true,
            Description = "Uma residência pensada para quem busca conforto, espaço e exclusividade em um dos condomínios mais desejados de Balsas. 450 m² de terreno e 236 m² construídos, sala, copa e cozinha integradas, três quartos (um suíte com closet), banheiro social externo e jacuzzi.",
            Features = ["Jacuzzi", "Sala, copa e cozinha integradas", "Suíte com closet", "Banheiro social externo"],
            Images = [Img("prime-ville-jacuzzi", "Fachada"), PrimeVilleAerial]
        },
        // IG 18069619760412648
        new()
        {
            Id = 7, Reference = "TS-107", Slug = "residencia-prime-rio-198",
            Title = "Residência Prime Rio Jardim", Tagline = "Projeto luminotécnico e paisagismo, 3 suítes",
            Type = PropertyType.Casa, Price = 1_800_000, Neighborhood = "Prime Rio", Development = "Prime Rio",
            Area = 198, LandArea = 300, Bedrooms = 3, Suites = 3, Garages = 2,
            Description = "Residência no Condomínio Prime Rio Residence em terreno de 10 x 30 m (300 m²) e 198 m² construídos. Três suítes (a master com closet), sala, copa, cozinha, área de lazer com churrasqueira, piscina, lavabo externo, área de serviço e depósito externo. Garagem para dois veículos cobertos, projeto luminotécnico e paisagismo. Financiável.",
            Features = ["Piscina", "Churrasqueira", "Projeto luminotécnico", "Paisagismo", "2 vagas cobertas", "Financiável"],
            Images = [Img("prime-rio-198", "Fachada"), PrimeRioPortal]
        },
        // IG 18375507142240442
        new()
        {
            Id = 8, Reference = "TS-108", Slug = "casa-prime-ville-tres-suites-jacuzzi",
            Title = "Casa Prime Ville Três Suítes", Tagline = "Churrasqueira e jacuzzi na área de lazer",
            Type = PropertyType.Casa, Price = 1_700_000, Neighborhood = "Prime Ville", Development = "Prime Ville",
            Area = 190, LandArea = 360, Bedrooms = 3, Suites = 3, Garages = 2,
            Description = "Casa no Condomínio Prime Ville Residence em terreno de 12 x 30 m (360 m²) e 190 m² construídos. Três quartos, todos suítes, sala, copa e cozinha, área de lazer com churrasqueira e jacuzzi e garagem para dois veículos cobertos.",
            Features = ["Jacuzzi", "Churrasqueira", "Três suítes", "2 vagas cobertas"],
            Images = [Img("prime-ville-360-tres-suites", "Fachada"), PrimeVilleAerial]
        },
        // IG 18124401067857846
        new()
        {
            Id = 9, Reference = "TS-109", Slug = "casa-prime-ville-quatro-suites",
            Title = "Casa Prime Ville Quatro Suítes", Tagline = "4 suítes, churrasqueira e jacuzzi",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Prime Ville", Development = "Prime Ville",
            Area = 190, LandArea = 360, Bedrooms = 4, Suites = 4, Garages = 2,
            Description = "Casa no Condomínio Prime Ville Residence em terreno de 12 x 30 m (360 m²) e 190 m² construídos. Quatro suítes, sendo uma suíte master, sala, copa e cozinha, área de lazer com churrasqueira e jacuzzi e garagem para dois veículos cobertos.",
            Features = ["4 suítes", "Jacuzzi", "Churrasqueira", "2 vagas cobertas"],
            Images = [Img("prime-ville-quatro-suites", "Fachada"), PrimeVilleAerial]
        },
        // IG 18031331537841994
        new()
        {
            Id = 10, Reference = "TS-110", Slug = "residencia-prime-ville-377",
            Title = "Residência Prime Ville Piscina", Tagline = "377 m² de terreno, 3 suítes e piscina",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Prime Ville", Development = "Prime Ville",
            Area = 187, LandArea = 377, Bedrooms = 3, Suites = 3, Garages = 2,
            Description = "Uma excelente oportunidade para quem busca conforto, sofisticação e qualidade de vida no Prime Ville Residence. 377 m² de terreno e 187 m² construídos, três suítes (uma master com closet), sala integrada à cozinha, piscina e garagem para dois carros.",
            Features = ["Piscina", "Sala integrada à cozinha", "Suíte master com closet", "Garagem para 2 carros"],
            Images = [Img("prime-ville-377", "Fachada"), PrimeVilleAerial]
        },
        // IG 18111964910471461
        new()
        {
            Id = 11, Reference = "TS-111", Slug = "casa-proximo-ao-ctg-terreno-1350",
            Title = "Casa CTG", Tagline = "1.350 m² de terreno e lote lateral de 675 m²",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Próximo ao CTG",
            Area = 330, LandArea = 1_350, Bedrooms = 3, Suites = 3,
            Description = "Uma excelente oportunidade para quem busca espaço, conforto e uma área de lazer completa em localização privilegiada. 1.350 m² de terreno e 330 m² construídos, três suítes (duas com closet), sala ampla, copa, cozinha e área de lazer com piscina e churrasqueira. Acompanha terreno lateral adicional de 675 m²: espaço para morar, receber a família e investir.",
            Features = ["Terreno lateral adicional de 675 m²", "Piscina", "Churrasqueira", "Duas suítes com closet", "Sala ampla"],
            Images = [Img("casa-ctg", "Fachada")]
        },
        // IG 18135511198593479
        new()
        {
            Id = 12, Reference = "TS-112", Slug = "casa-alto-padrao-escritorio-jacuzzi",
            Title = "Casa Escritório", Tagline = "Home office, jacuzzi e energia solar",
            Type = PropertyType.Casa, Price = 0, Neighborhood = "Balsas",
            Area = 191, LandArea = 313, Bedrooms = 3, Suites = 1,
            Description = "Casa de alto padrão em terreno de 312,50 m² com 191 m² construídos. Imóvel moderno, com três quartos (uma ampla suíte master), escritório privativo para home office, sala de estar e jantar integradas, cozinha planejada, área de serviço e área gourmet integrada à área de lazer, com banheiro exclusivo. Energia solar, jacuzzi e financiável.",
            Features = ["Escritório privativo", "Jacuzzi", "Energia solar", "Cozinha planejada", "Área gourmet integrada", "Financiável"],
            Images = [Img("casa-piscina-jacuzzi", "Área de lazer")]
        },
        // IG 18114635969282235
        new()
        {
            Id = 13, Reference = "TS-113", Slug = "casa-jardim-europa",
            Title = "Casa Jardim Europa", Tagline = "167,5 m² construídos, cozinha integrada ao lazer",
            Type = PropertyType.Casa, Price = 920_000, Neighborhood = "Jardim Europa",
            Area = 168, LandArea = 300, Bedrooms = 3, Suites = 1,
            Description = "Uma excelente oportunidade para quem busca conforto, modernidade e ótima localização no Jardim Europa. Terreno de 12 x 25 m e 167,5 m² construídos, três quartos (um suíte com closet), sala confortável e cozinha integrada à área de lazer. Imóvel financiável.",
            Features = ["Cozinha integrada à área de lazer", "Suíte com closet", "Financiável"],
            Images = [Img("casa-jardim-europa", "Fachada")]
        },
        // IG 18171242101436710
        new()
        {
            Id = 14, Reference = "TS-114", Slug = "casa-bairro-sao-luis",
            Title = "Casa São Luís", Tagline = "Cozinha com ilha, piscina e porcelanato em toda a casa",
            Type = PropertyType.Casa, Price = 900_000, Neighborhood = "São Luís",
            Area = 157, Bedrooms = 3, Suites = 1,
            Description = "Imóvel de alto padrão, com acabamento impecável e construção de excelente qualidade. Três quartos, sendo uma suíte master com closet amplo, sala ampla, cozinha moderna com ilha, banheiro social, lavabo e área de lazer com piscina. Estrutura totalmente em ferro, piso 100% em porcelanato, cobertura toda em manta e infraestrutura pronta para ar-condicionado em todos os ambientes. 146,98 m² de casa e 10 m² de piscina, 156,98 m² construídos no total. Financiável.",
            Features = ["Cozinha com ilha", "Piscina", "Closet amplo", "Porcelanato em toda a casa", "Estrutura em ferro", "Financiável"],
            Images = [Img("casa-sao-luis", "Fachada")]
        },
        // IG 18393981637088311
        new()
        {
            Id = 15, Reference = "TS-115", Slug = "residencia-jardim-iracema",
            Title = "Residência Jardim Iracema", Tagline = "Terreno de 10 x 40 m e garagem para até 6 carros",
            Type = PropertyType.Casa, Price = 670_000, Neighborhood = "Jardim Iracema",
            Area = 157, LandArea = 400, Bedrooms = 3, Suites = 1, Garages = 6,
            Description = "Um verdadeiro sonho pronto para morar. Terreno amplo de 10 x 40 m e 156,74 m² construídos, com três quartos (suíte master com closet), sala de estar e de jantar, cozinha espaçosa, banheiro social, lavabo, área de serviço e despensa. Área gourmet, espaço pronto para piscina, paisagismo moderno, cerca elétrica, portão com motor e garagem para até seis carros (dois cobertos).",
            Features = ["Garagem para até 6 carros", "Área gourmet", "Espaço pronto para piscina", "Paisagismo moderno", "Cerca elétrica"],
            Images = [Img("casa-jardim-iracema", "Fachada e jardim")]
        },
        // IG 18076680902207479
        new()
        {
            Id = 16, Reference = "TS-116", Slug = "casa-bairro-sao-felix",
            Title = "Casa São Félix", Tagline = "3 quartos e garagem coberta para dois carros",
            Type = PropertyType.Casa, Price = 580_000, Neighborhood = "São Félix",
            Area = 124, LandArea = 300, Bedrooms = 3, Suites = 1, Garages = 2,
            Description = "Casa no bairro São Félix em terreno de 10 x 30 m (300 m²) e 123,5 m² construídos. Três quartos, sendo uma suíte com closet, sala, cozinha americana, banheiro social, área de serviço, área na frente e no fundo e garagem para dois carros cobertos. Financiável.",
            Features = ["Cozinha americana", "Suíte com closet", "2 vagas cobertas", "Financiável"],
            Images = [Img("casa-sao-felix", "Fachada")]
        },
        // IG 18152225137470078
        new()
        {
            Id = 17, Reference = "TS-117", Slug = "casa-jardim-das-oliveiras",
            Title = "Casa Jardim das Oliveiras", Tagline = "Pronta para morar, com paisagismo moderno",
            Type = PropertyType.Casa, Price = 420_000, Neighborhood = "Jardim das Oliveiras",
            Area = 92, LandArea = 186, Bedrooms = 2, Suites = 1, Garages = 2,
            Description = "Um verdadeiro sonho pronto para morar. Terreno de 186 m² e 91,74 m² construídos, com dois quartos (suíte master com closet), sala de estar e de jantar, cozinha espaçosa, banheiro social, área de serviço, box nos banheiros, paisagismo moderno e garagem para dois carros.",
            Features = ["Pronta para morar", "Suíte master com closet", "Paisagismo moderno", "Garagem para 2 carros"],
            Images = [Img("casa-jardim-oliveiras", "Fachada")]
        },
        // IG 18123665833655889 / 18083334428158887
        new()
        {
            Id = 18, Reference = "TS-118", Slug = "fazenda-522-hectares-rio-maravilha",
            Title = "Fazenda Rio Maravilha", Tagline = "522 hectares, 7 açudes e 1.200 m de margem de rio",
            Type = PropertyType.Fazenda, Price = 0, Neighborhood = "Zona Rural",
            Area = 0, LandArea = 5_220_000, Bedrooms = 3, Suites = 3,
            Featured = true,
            Description = "Propriedade rural ideal para pecuária, integração agropecuária e expansão produtiva. 522 hectares no total, cerca de 100 hectares de pastagem formada e 300 hectares já abertos, divididos em 19 piquetes para manejo rotacionado. Abundância de água: 7 açudes e 1.200 metros de margem do Rio Maravilha. Curral completo com extensão para confinamento, três galpões para insumos, máquinas e equipamentos, e sede com três suítes e cozinha ampla.",
            Features = ["522 hectares", "1.200 m de margem do Rio Maravilha", "7 açudes", "19 piquetes", "Curral com extensão para confinamento", "3 galpões", "Sede com 3 suítes"],
            Images = [Img("fazenda-rio-maravilha", "Entrada da fazenda")]
        },
        // IG 18126601592311757
        new()
        {
            Id = 19, Reference = "TS-119", Slug = "chacara-16-hectares-pesqueiro",
            Title = "Chácara 16 Hectares", Tagline = "Pastagem formada, pesqueiro e rampa para barcos",
            Type = PropertyType.Fazenda, Price = 1_500_000, Neighborhood = "Zona Rural",
            Area = 0, LandArea = 160_000, Bedrooms = 2, Suites = 2,
            Description = "Uma propriedade diferenciada, com excelente estrutura, espaço para criação, lazer e momentos em família. 16 hectares, sendo 8 de pastagem formada. Sede com duas suítes, banheiro social, área de lazer com churrasqueira, depósito e despensa, galpão para equipamentos, triturador, canis, energia elétrica e câmeras de segurança. Rampa para embarcações, pesqueiro e pomar com diversas frutas.",
            Features = ["16 hectares", "8 hectares de pastagem formada", "Rampa para embarcações", "Pesqueiro", "Pomar", "Câmeras de segurança"],
            Images = [Img("fazenda-16ha", "Acesso à rampa de embarcações")]
        },
        // IG 17966060864961052
        new()
        {
            Id = 20, Reference = "TS-120", Slug = "apartamento-locacao-centro",
            Title = "Apartamento Centro", Tagline = "Locação · semimobiliado, 2 suítes, sem condomínio",
            Type = PropertyType.Apartamento, Purpose = ListingPurpose.Aluguel, Price = 3_700, Neighborhood = "Centro",
            Area = 80, Bedrooms = 2, Suites = 2, Garages = 2,
            Description = "Apartamento para locação no centro de Balsas, com 80 m² de área privativa. Semimobiliado, duas suítes, sala, copa e cozinha, área de serviço e garagem para dois veículos. Sem condomínio.",
            Features = ["Semimobiliado", "2 suítes", "Sem condomínio", "2 vagas"],
            Images = [Img("apartamento-centro", "Sala")]
        },
        // IG 18097392518610098
        new()
        {
            Id = 21, Reference = "TS-121", Slug = "sala-comercial-locacao-centro",
            Title = "Sala Comercial Centro", Tagline = "Locação · ideal para consultório ou escritório",
            Type = PropertyType.Comercial, Purpose = ListingPurpose.Aluguel, Price = 5_000, Neighborhood = "Centro",
            Area = 70,
            Description = "Sala comercial para locação no centro de Balsas, ideal para consultório ou escritório. 70 m² de área privativa, dois banheiros, recepção, copa, porta automática e sala multiuso.",
            Features = ["Recepção", "2 banheiros", "Copa", "Porta automática", "Sala multiuso"],
            Images = [Img("sala-comercial-centro", "Fachada")]
        },
    ];

    private static List<Development> SeedDevelopments() =>
    [
        // IG 17944593549248867 (lots), 18019886501891540 (delivered house)
        new()
        {
            Slug = "prime-rio", Name = "Prime Rio", Tagline = "Prime Rio Residence, um dos condomínios mais desejados de Balsas",
            Location = "Balsas · MA", Status = "Terrenos disponíveis",
            Conditions = "Entrada a partir de R$ 13.200 · parcelas a partir de R$ 1.320 em até 180 meses",
            Description = "Condomínio fechado de alto padrão em Balsas, com terrenos à venda e casas prontas. Entrada a partir de 5% e financiamento em até 180 parcelas. É também onde a Construtora Terra Sul entregou uma de suas obras mais recentes: 346 m² construídos em terreno de 360 m², com piscina e projeto contemporâneo, em 18 meses.",
            Cover = R + "prime-rio-portal.webp",
            Gallery = [Img("prime-rio-portal", "Portal do Prime Rio Residence"), Img("prime-rio-entregue", "Residência entregue pela Construtora Terra Sul"), Img("prime-rio-piscina", "Residência no Prime Rio"), Img("prime-rio-198", "Residência no Prime Rio")],
            Highlights = ["Condomínio fechado", "Entrada a partir de 5%", "Até 180 parcelas", "Terrenos e casas prontas", "Obra da Construtora Terra Sul"]
        },
        // IG 18099776062929681 (lots) and the Prime Ville listings
        new()
        {
            Slug = "prime-ville", Name = "Prime Ville", Tagline = "Prime Ville Residence, casas contemporâneas em condomínio fechado",
            Location = "Balsas · MA", Status = "Terrenos e casas",
            Conditions = "Terrenos com entrada a partir de 5% e até 180 parcelas",
            Description = "O condomínio com o maior número de residências da Terra Sul à venda: casas contemporâneas de três e quatro suítes, muitas com piscina, jacuzzi e móveis planejados. Terrenos com entrada a partir de 5% e financiamento em até 180 parcelas.",
            Cover = R + "prime-ville-aerea.webp",
            Gallery = [Img("prime-ville-aerea", "Vista aérea do Prime Ville"), Img("prime-ville-hidro", "Residência no Prime Ville"), Img("prime-ville-piscina-privativa", "Residência no Prime Ville"), Img("prime-ville-mobiliada", "Residência no Prime Ville"), Img("prime-ville-quatro-suites", "Residência no Prime Ville")],
            Highlights = ["Condomínio fechado", "Casas de 3 e 4 suítes", "Entrada a partir de 5%", "Até 180 parcelas"]
        },
        // IG 18186024517400321
        new()
        {
            Slug = "garden-prime", Name = "Garden Prime", Tagline = "Chegou a hora de conquistar o seu terreno",
            Location = "Balsas · MA", Status = "Terrenos disponíveis",
            Conditions = "Entrada a partir de 5% · financiamento em até 180 parcelas",
            Description = "Loteamento para quem quer sair do aluguel ou investir com segurança: entrada a partir de 5% e financiamento em até 180 parcelas, em uma região de Balsas em crescimento.",
            Cover = R + "garden-prime-aerea.webp",
            Gallery = [Img("garden-prime-aerea", "Vista aérea dos terrenos"), Img("condominio-aerea", "Balsas vista do alto")],
            Highlights = ["Entrada a partir de 5%", "Até 180 parcelas", "Para morar ou investir"]
        },
    ];
}
