# Implementation Spec – MVP v0.1

## 1. Estrutura da Solution
Conforme definido na Architecture Spec v0.3.

---

## 2. Endpoints

### GET /articles/search
Retorna lista de artigos disponíveis.

### POST /content/generate
Recebe { articleId }, gera conteúdo e retorna ReviewPackage.

---

## 3. DTOs
- ArticleDto
- GenerateContentRequest
- ReviewPackageDto
  - ArticleId, ArticleTitle, ArticleUrl
  - PtBR: ContentBlockDto
  - EnUS: ContentBlockDto
- ContentBlockDto
  - TechnicalSummary
  - Insights (List\<string\>, 3 itens)
  - CasualExplanation
  - PostSuggestions (List\<string\>, 3 itens, 1.200–1.800 chars cada)
  - ConsolidatedText (gerado on-demand, não persiste)
- AiContentResponse (interno, desserialização da resposta OpenAI)
  - PtBR: AiContentBlock
  - EnUS: AiContentBlock
- AiContentBlock (interno)
  - TechnicalSummary, Insights, CasualExplanation, PostSuggestions

---

## 4. Use Cases

### SearchArticlesUseCase
- Busca artigos via provider
- Persiste dados
- Retorna DTOs

### GenerateContentUseCase
- Carrega artigo
- Extrai texto
- Gera conteúdo via IA
- Persiste ProcessedContent
- Consolida texto
- Retorna ReviewPackage

---

## 5. Interfaces
Definidas em Application Layer:
- IArticleProvider
- IContentExtractor
- IAiContentService
- IContentRepository

---

## 6. Entidades
- ArticleRawContent
- ProcessedContent

---

## 7. Persistence
EF Core + SQLite
- ContentDbContext
- Mapeamentos
- Suporte a JSON em colunas

---

## 8. Providers e Services
- RssArticleProvider
- HtmlContentExtractor
- OpenAiContentService
  - Lê Model, MaxTokens e Temperature de IConfiguration (seção "OpenAI")
  - Usa response_format: json_object
  - Desserializa resposta como AiContentResponse via System.Text.Json
  - Lança AiResponseParseException com conteúdo bruto em caso de falha no parse
- PromptBuilder (implementa IPromptBuilder)
  - Contém o system prompt fixo
  - Contém o user prompt template com placeholders {{articleTitle}}, {{articleUrl}}, {{articleRawText}}

---

## 9. ConsolidatedText

**PT-BR:**
```
[Resumo Técnico]
{technicalSummary}

Principais insights:
• {insight1}
• {insight2}
• {insight3}

Explicação rápida:
{casualExplanation}

Sugestões de post:

1)
{postSuggestion1}

2)
{postSuggestion2}

3)
{postSuggestion3}
```

**EN-US:**
```
[Technical Summary]
{technicalSummary}

Key insights:
• {insight1}
• {insight2}
• {insight3}

Quick explanation:
{casualExplanation}

Post suggestions:

1)
{postSuggestion1}

2)
{postSuggestion2}

3)
{postSuggestion3}
```

---

## 10. Configuração OpenAI

```json
"OpenAI": {
  "ApiKey": "",
  "Model": "gpt-4o",
  "MaxTokens": 4096,
  "Temperature": 0.7
}
```

---

## 11. Roadmap de Implementação
1. Criar solution e projetos
2. Implementar DbContext e migrations
3. Implementar providers
4. Implementar PromptBuilder com system prompt e user prompt template
5. Implementar IA service
6. Implementar use cases
7. Criar endpoints
8. Testar fluxo completo no Swagger