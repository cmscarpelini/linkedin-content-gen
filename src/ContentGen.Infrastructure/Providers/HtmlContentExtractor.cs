using ContentGen.Application.Interfaces;
using HtmlAgilityPack;

namespace ContentGen.Infrastructure.Providers;

public class HtmlContentExtractor : IContentExtractor
{
    private readonly HttpClient _httpClient;

    public HtmlContentExtractor(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<string> ExtractTextAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Remove ruído: scripts, estilos, nav, header, footer, sidebar, comments
        var noiseSelectors = "//script|//style|//nav|//header|//footer|//aside|//form|//noscript|//*[@class='sidebar']|//*[@class='comments']|//*[@id='comments']";
        foreach (var node in doc.DocumentNode.SelectNodes(noiseSelectors) ?? Enumerable.Empty<HtmlNode>())
            node.Remove();

        // Tenta extrair o conteúdo principal por seletores específicos do DevBlogs e blogs similares
        var candidates = new[]
        {
            "//div[contains(@class,'entry-content')]",
            "//div[contains(@class,'post-content')]",
            "//div[contains(@class,'article-content')]",
            "//div[contains(@class,'blog-content')]",
            "//article",
            "//main",
            "//div[@role='main']",
            "//body"
        };

        HtmlNode? mainNode = null;
        foreach (var selector in candidates)
        {
            mainNode = doc.DocumentNode.SelectSingleNode(selector);
            if (mainNode != null) break;
        }

        // Extrai parágrafos e headings diretamente para preservar estrutura
        var contentNodes = mainNode?.SelectNodes(".//p|.//h1|.//h2|.//h3|.//h4|.//li")
            ?? doc.DocumentNode.SelectNodes("//p|//h2|//h3");

        string text;
        if (contentNodes != null && contentNodes.Count > 0)
        {
            text = string.Join("\n\n", contentNodes
                .Select(n => HtmlEntity.DeEntitize(n.InnerText).Trim())
                .Where(t => t.Length > 30)); // ignora textos muito curtos (menus, labels)
        }
        else
        {
            text = HtmlEntity.DeEntitize(mainNode?.InnerText ?? string.Empty);
        }

        // Normaliza espaços excessivos
        text = string.Join("\n", text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0));

        // Limita a 12.000 caracteres para não estourar o contexto do modelo
        return text.Length > 12000 ? text[..12000] : text;
    }
}
