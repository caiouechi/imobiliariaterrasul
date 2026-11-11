using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using TerraSul.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<ICatalogService, CatalogService>();
builder.Services.AddResponseCompression(o =>
{
    o.EnableForHttps = true;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Brazilian formatting everywhere: "R$ 1.800.000", never the server's own "1,800,000".
var ptBR = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = ptBR;
CultureInfo.DefaultThreadCurrentUICulture = ptBR;
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(ptBR),
    SupportedCultures = [ptBR],
    SupportedUICultures = [ptBR],
});

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
            ctx.Context.Response.Headers.CacheControl = "public,max-age=2592000";
    }
});
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute("imovel", "imoveis/{slug}", new { controller = "Imoveis", action = "Detalhes" });
app.MapControllerRoute("empreendimento", "empreendimentos/{slug}", new { controller = "Empreendimentos", action = "Detalhes" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
