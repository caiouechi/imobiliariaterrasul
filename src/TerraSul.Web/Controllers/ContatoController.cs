using Microsoft.AspNetCore.Mvc;
using TerraSul.Web.Models;

namespace TerraSul.Web.Controllers;

public class ContatoController : Controller
{
    public IActionResult Index(string? imovel) => View(new ContactForm { Imovel = imovel });

    /// <summary>
    /// Hands the lead straight to WhatsApp with a pre-filled message. Replace with an e-mail/CRM
    /// integration when the client has one; the view model is already shaped for it.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Enviar(ContactForm form)
    {
        if (string.IsNullOrWhiteSpace(form.Nome) || string.IsNullOrWhiteSpace(form.Telefone))
        {
            ModelState.AddModelError("", "Informe seu nome e telefone.");
            return View("Index", form);
        }
        var msg = $"Olá, Terra Sul! Sou {form.Nome} ({form.Telefone}). Interesse: {form.Interesse}."
                + (string.IsNullOrWhiteSpace(form.Imovel) ? "" : $" Imóvel: {form.Imovel}.")
                + (string.IsNullOrWhiteSpace(form.Mensagem) ? "" : $" {form.Mensagem}");
        return Redirect(SiteInfo.WhatsAppLink(msg));
    }
}
