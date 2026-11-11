# Imobiliária Terra Sul · site institucional

Site de alto padrão para a Imobiliária Terra Sul (Balsas · MA, CRECI J-230), em ASP.NET Core 8 MVC.

## Rodar

```bash
dotnet run --project src/TerraSul.Web/TerraSul.Web.csproj --urls http://localhost:5210
```

## Estrutura

- `src/TerraSul.Web/Services/CatalogService.cs`: catálogo em memória (imóveis, empreendimentos, depoimentos). Troque por EF Core ou feed do CRM mantendo `ICatalogService`.
- `src/TerraSul.Web/Models/Development.cs`: `SiteInfo` concentra WhatsApp, Instagram, CRECI, horário e e-mail.
- `src/TerraSul.Web/Controllers/ContatoController.cs`: o formulário hoje redireciona para o WhatsApp com a mensagem preenchida. Pode ser trocado por envio de e-mail/CRM.
- `wwwroot/img/real`: fotos REAIS dos posts do @imobiliariaterrasulbls (quadros dos Reels, 720 px de largura, verticais). Todos os imóveis usam só essas fotos.
- `wwwroot/img/mood`: 4 imagens de clima geradas no Higgsfield (arquitetura clara, interior, chave, textura de travertino) usadas apenas em fundos e faixas, nunca como imóvel.
- `wwwroot/img/gen`: imagens geradas da primeira versão. Não são mais usadas em nenhuma página; podem ser apagadas.
- `wwwroot/css/site.css` e `wwwroot/js/site.js`: design system claro (Cormorant Garamond + Jost, marfim e branco / ardósia-teal do logo / dourado champanhe; rodapé e faixas com foto continuam escuros) e comportamentos (Swiper, GLightbox, AOS, noUiSlider).

## Páginas

| Rota | Conteúdo |
| --- | --- |
| `/` | Hero slider com parallax, busca, contadores, carrossel de destaques, abas de empreendimentos, mosaico lifestyle com lightbox, depoimentos, feed Instagram, CTA |
| `/Imoveis` | Filtros AJAX (finalidade, tipo, bairro, faixa de preço com slider duplo, quartos, vagas, área), ordenação, grade/lista, favoritos |
| `/imoveis/{slug}` | Galeria com miniaturas e tela cheia, ficha técnica, diferenciais, simulador de financiamento, mapa, card de contato fixo, semelhantes |
| `/Empreendimentos`, `/empreendimentos/{slug}` | Prime Rio, Prime Ville, Villa da Serra |
| `/Sobre` | História, pilares, linha do tempo, depoimentos |
| `/Contato` | Formulário → WhatsApp, informações, mapa |

## Observações

Desde 10/11/2026 os **imóveis e empreendimentos são reais**: cada um corresponde a um post do Instagram (o id do post está ao lado de cada um em `CatalogService.cs`) e todos os números vêm da legenda; "valor a consultar" aparece como "Sob consulta". Continuam sendo **conteúdo de demonstração**: os depoimentos, o e-mail e a linha do tempo da página Sobre. Os dados reais confirmados são: bio e nome, CRECI J-230, WhatsApp +55 (99) 98446-8625, Instagram e os nomes dos empreendimentos (Prime Rio, Prime Ville, Villa da Serra).
