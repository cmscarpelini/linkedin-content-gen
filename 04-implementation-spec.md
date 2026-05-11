# Implementation Spec – MVP v0.2

## 1. Estrutura da Solution
Conforme definido na Architecture Spec v0.3.

---

## 2. Endpoints

### GET /articles/search
Busca fontes RSS configuradas, persiste artigos novos e retorna lista de `ArticleDto`.

### GET /articles
Retorna todos os artigos já salvos no banco com flag `hasContent`. Retorna lista de `SavedArticleDto`.

### POST /content/generate
Recebe `{ articleId }`. Se conteúdo já existir, retorna sem chamar a IA. Caso contrário, gera e persiste. Retorna `ReviewPackage`.

### GET /content
Retorna histórico de todo conteúdo gerado. Retorna lista de `ContentSummaryDto`.

### GET /content/{articleId}
Retorna `ReviewPackage` completo para o artigo especificado. Retorna 404 se não encontrado.

---

## 3. DTOs
- `ArticleDto` — Id, Title, Url, Source, PublishedAt
- `SavedArticleDto` — Id, Title, Url, Source, PublishedAt, HasContent
- `ContentSummaryDto` — ArticleId, ArticleTitle, ArticleUrl, CreatedAt
- `GenerateContentRequest` — ArticleId
- `ReviewPackageDto` — ArticleId, ArticleTitle, ArticleUrl, PtBR: ContentBlockDto, EnUS: ContentBlockDto
- `ContentBlockDto` — TechnicalSummary, Insights (List\<string\>), CasualExplanation, PostSuggestions (List\<string\>, 1.200–1.800 chars cada), ConsolidatedText (gerado on-demand)
- `AiContentResponse` (interno) — desserialização por idioma: AiContentBlock
- `AiContentBlock` (interno) — TechnicalSummary, Insights, CasualExplanation, PostSuggestions

---

## 4. Use Cases

### SearchArticlesUseCase
- Busca artigos via RSS provider
- Persiste novos; reutiliza existentes pelo GUID
- Retorna lista de `ArticleDto`

### ListArticlesUseCase
- Retorna todos os artigos do banco com flag `hasContent`
- Retorna lista de `SavedArticleDto`

### GenerateContentUseCase
- Verifica se `ProcessedContent` já existe para o artigo — se sim, retorna sem chamar a IA
- Extrai texto via `HtmlContentExtractor`
- Chama IA duas vezes separadas (PT-BR, EN-US) via `OpenAiContentService`
- Persiste `ProcessedContent`
- Métodos `BuildReviewPackage` e `BuildContentBlock` são `internal static` para reuso
- Retorna `ReviewPackage`

### ListContentUseCase
- Retorna histórico de todo conteúdo gerado
- Retorna lista de `ContentSummaryDto`

### GetContentUseCase
- Busca `ProcessedContent` + `ArticleRawContent` por `articleId`
- Retorna `ReviewPackage` ou `null`

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
- `RssArticleProvider` — lê fontes de `RssSources` (array) e limite de `RssMaxArticles` via `IConfiguration`
- `HtmlContentExtractor` — extrai texto via cascade de seletores CSS com HtmlAgilityPack
- `OpenAiContentService`
  - Lê `Model`, `MaxTokens`, `Temperature`, `ApiKey` e `BaseUrl` de `IConfiguration` (seção `OpenAI`)
  - Compativel com qualquer provider OpenAI-compatible via `BaseUrl` (usado com Groq)
  - Faz duas chamadas separadas — uma por idioma — sem JSON mode
  - `ExtractJson` parseia bloco de código markdown ou JSON bruto na resposta
  - `SanitizeJsonStrings` escapa newlines literais dentro de strings JSON antes do parse
  - Lança `AiResponseParseException` com conteúdo bruto em caso de falha
- `PromptBuilder` (implementa `IPromptBuilder`)
  - `BuildSystemPrompt()` — prompt de sistema fixo com instruções de formato e tamanho
  - `BuildUserPrompt(articleTitle, articleUrl, articleRawText, languageCode, languageName)` — gera prompt mono-idioma com schema JSON de saída esperado e requisito explícito de 1.200–1.800 chars por post

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

## 10. Configuração

```json
"OpenAI": {
  "ApiKey": "YOUR_KEY_HERE",
  "Model": "llama-3.3-70b-versatile",
  "MaxTokens": 8192,
  "Temperature": 0.7,
  "BaseUrl": "https://api.groq.com/openai/v1"
},
"RssSources": [
  "https://devblogs.microsoft.com/dotnet/feed/",
  "https://devblogs.microsoft.com/azure/feed/",
  "https://techcommunity.microsoft.com/plugins/custom/microsoft/o365/rss-board-messages?board=AzureArchitectureBlog"
],
"RssMaxArticles": 5
```

A chave real fica em `appsettings.Development.json` (ignorado pelo Git). O arquivo `appsettings.Development.example.json` serve de referência para quem clonar o repositório.

---

## 11. Frontend

Projeto React em `src/ContentGen.Web/`, criado com Vite + TypeScript. Configuração em `.env.local` (ignorado pelo Git; usar `.env.example` como referência).

### Tecnologias
- React 19 + TypeScript
- Vite 8
- Tailwind CSS v3
- react-router-dom v7

### Páginas
| Rota | Componente | Descrição |
|---|---|---|
| `/` | `ArticlesPage` | Busca artigos RSS e exibe lista com botão "✨ Gerar" |
| `/articles` | `SavedArticlesPage` | Lista artigos salvos no banco com status de conteúdo |
| `/content` | `ContentListPage` | Histórico de todo conteúdo gerado |
| `/content/:articleId` | `ContentDetailPage` | Detalhe com abas PT-BR / EN-US, posts com contador de chars e botão copiar |

### Estrutura
```
src/
  api/client.ts       # Funções fetch para todos os endpoints
  types/index.ts      # Interfaces TypeScript
  pages/              # Páginas da aplicação
  App.tsx             # BrowserRouter + sidebar + Routes
  main.tsx            # Entry point
```

## 12. Roadmap de Implementação
1. Criar solution e projetos
2. Implementar DbContext e migrations
3. Implementar providers
4. Implementar PromptBuilder
5. Implementar serviço de IA
6. Implementar use cases
7. Criar endpoints
8. Criar frontend React
9. Testar fluxo completo