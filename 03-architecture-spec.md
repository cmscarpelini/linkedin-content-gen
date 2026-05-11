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
- RawHtml
- PublishedAt

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

### ReviewPackage
DTO gerado on-demand (não persiste como tabela).
Campos:
- ArticleId, ArticleTitle, ArticleUrl
- PtBR: ContentBlockDto (TechnicalSummary, Insights, CasualExplanation, PostSuggestions, ConsolidatedText)
- EnUS: ContentBlockDto (mesma estrutura)

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
- Persiste novos artigos no banco; reutiliza existentes pelo GUID
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
- Retorna `ReviewPackage` completo para um artigo específico

### 6) Usuário revisa manualmente via frontend

---

## 6. Evolução para SaaS (futuro)
- Troca SQLite → Postgres sem reescrita
- Filas (RabbitMQ / Hangfire)
- Redis caching
- Multi-tenant
- Notificações
- Worker distribuído
- Painel web completo

---

## 7. Estrutura de Pastas
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