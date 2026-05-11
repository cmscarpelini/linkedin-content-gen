# Architecture Spec – MVP v0.3

## 1. Visão Geral
Arquitetura baseada em Clean Architecture, utilizando:
- Minimal API
- EF Core + SQLite
- Providers externos (RSS, HTML, IA)

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
- Busca artigos
- Salva no banco
- Retorna lista

### 2) POST /content/generate
- Busca artigo
- Extrai texto
- IA gera conteúdo
- Consolida
- Retorna ReviewPackage

### 3) Usuário revisa manualmente

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
src/
ContentGen.Api/
ContentGen.Application/
ContentGen.Domain/
ContentGen.Infrastructure/
tests/
ContentGen.Tests/