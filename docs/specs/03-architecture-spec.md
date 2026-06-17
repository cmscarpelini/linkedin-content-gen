# Architecture Spec – MVP v0.4

## 1. Visão Geral
Arquitetura baseada em Clean Architecture, utilizando:
- Minimal API (.NET 10)
- EF Core + SQLite
- Providers externos (RSS, HTML, IA via Groq/Llama)
- Frontend React + Vite + TypeScript + Tailwind CSS
- Documentação interativa via Scalar (`/scalar`)

Camadas principais:
- API
- Application
- Domain
- Infrastructure

---

## 2. Storage Oficial: SQLite

### Justificativas
- Fácil configuração
- Simples para MVP
- Integração total com EF Core
- Facilita futura migração para Postgres (SaaS)
- Estrutura relacional, ideal para o domínio do sistema

---

## 3. Modelo de Dados

### ArticleRawContent
- Id (Guid)
- Title
- Url
- Source
- PublishedAt

> O HTML do artigo é buscado sob demanda na geração de conteúdo (via `HtmlContentExtractor`) e não é persistido — apenas o `ProcessedContent` resultante é gravado.

### ProcessedContent
- Id (Guid)
- ArticleId (FK → ArticleRawContent)
- TechnicalSummaryPtBR
- InsightsPtBR (JSON: string[])
- CasualExplanationPtBR
- PostSuggestionsPtBR (JSON: string[])
- TechnicalSummaryEnUS
- InsightsEnUS (JSON: string[])
- CasualExplanationEnUS
- PostSuggestionsEnUS (JSON: string[])
- CreatedAt

### PostPublication
- Id (Guid)
- ArticleId (Guid)
- Language (`pt-BR` | `en-US`)
- PostIndex (int, 0-based)
- PublishedAt (DateTime)
- Índice único em (ArticleId, Language, PostIndex). A existência da linha = post publicado; desmarcar remove a linha.

### ReviewPackage
DTO gerado on-demand (não persiste como tabela).
Campos:
- ArticleId, ArticleTitle, ArticleUrl
- PtBR: ContentBlockDto (TechnicalSummary, Insights, CasualExplanation, PostSuggestions, ConsolidatedText)
- EnUS: ContentBlockDto (mesma estrutura)
- `PostSuggestions` é uma lista de `PostDto` (Index, Text, Published) — o flag `Published` reflete as linhas de `PostPublication`

---

## 4. Interfaces
- IArticleProvider
- IContentExtractor
- IAiContentService
- IContentRepository
- IPromptBuilder

---

## 5. Fluxo Arquitetural

### 1) GET /articles/search
- Busca fontes RSS configuradas em `appsettings.json` (`RssSources`, `RssMaxArticles`)
- Persiste novos artigos no banco; reutiliza existentes pela URL
- Retorna lista de `ArticleDto`

### 2) GET /articles
- Retorna todos os artigos salvos no banco
- Inclui flag `hasContent` indicando se já há conteúdo gerado
- Retorna lista de `SavedArticleDto`

### 3) POST /content/generate
- Verifica se conteúdo já existe — se sim, retorna sem chamar a IA (idempotente)
- Extrai HTML do artigo via `HtmlContentExtractor`
- Chama IA duas vezes: PT-BR e EN-US
- Persiste `ProcessedContent`
- Retorna `ReviewPackage`

### 4) GET /content
- Retorna histórico de todo conteúdo gerado
- Retorna lista de `ContentSummaryDto`

### 5) GET /content/{articleId}
- Retorna `ReviewPackage` completo para um artigo específico (com flag `Published` por post)

### 6) PUT /content/{articleId}/posts/{language}/{index}/published
- Marca/desmarca um post específico como publicado (idempotente)
- Valida idioma (`pt-BR`/`en-US`) e índice; `ValidationException` → 400, conteúdo inexistente → 404

### 7) Usuário revisa manualmente via frontend

---

## 6. Tratamento de Erros e Observabilidade

### Respostas de erro (RFC 7807 / ProblemDetails)
Um `GlobalExceptionHandler` (`IExceptionHandler`) centraliza o tratamento de exceções e retorna `application/problem+json`:

| Exceção | Status | Significado |
|---|---|---|
| `ValidationException` | 400 | Entrada inválida (idioma/índice de post) |
| `NotFoundException` | 404 | Artigo inexistente |
| `AiResponseParseException` | 502 | Resposta inválida do provider de IA |
| `HttpRequestException` | 502 | Falha ao buscar artigo/feed externo |
| (demais) | 500 | Erro inesperado — `Detail` genérico, sem vazar internals |

### Logging
Logging estruturado via `ILogger`:
- `GenerateContentUseCase` — distingue cache-hit de geração nova e registra persistência.
- `RssArticleProvider` — registra (warning) feeds indisponíveis e segue com as demais fontes, em vez de engolir a exceção silenciosamente.
- `GlobalExceptionHandler` — `LogError` para 5xx, `LogWarning` para 4xx.

---

## 7. Evolução para SaaS (futuro)
- Troca SQLite → Postgres sem reescrita
- Filas (RabbitMQ / Hangfire)
- Redis caching
- Multi-tenant
- Notificações
- Worker distribuído
- Painel web completo

---

## 8. Estrutura de Pastas
```
src/
  ContentGen.Api/          # Minimal API, endpoints, DI, Scalar
  ContentGen.Application/  # Use cases, interfaces, DTOs
  ContentGen.Domain/       # Entidades
  ContentGen.Infrastructure/ # RSS, HTML, IA, EF Core, SQLite
  ContentGen.Web/          # Frontend React + Vite + TypeScript + Tailwind
tests/
  ContentGen.Tests/
```