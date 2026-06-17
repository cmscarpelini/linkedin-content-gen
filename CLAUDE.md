# CLAUDE.md

Guidance for AI agents (and humans) working in this repository.

## What this is

**ContentGen** — an AI-powered LinkedIn content generator for Microsoft-ecosystem
developers. It reads RSS feeds from Microsoft tech blogs, extracts the article text,
and uses **Groq (Llama 3.3 70B)** to produce bilingual (PT-BR + EN-US) LinkedIn posts
ready to copy and publish.

Backend is .NET 10 (ASP.NET Core Minimal API) following **Clean Architecture**;
frontend is React 19 + Vite + TypeScript + Tailwind.

## Commands

Backend (run from repo root):

```bash
dotnet build ContentGen.slnx -c Release
dotnet test  ContentGen.slnx                      # ~65 tests (xUnit)
dotnet run --project src/ContentGen.Api --launch-profile http   # http://localhost:5234
```

Frontend (run from `src/ContentGen.Web`):

```bash
npm install
npm run dev      # http://localhost:5173
npm run lint     # eslint — CI fails on lint errors
npm run build    # tsc -b && vite build
```

Full stack via Docker (from repo root):

```bash
cp .env.example .env     # set OPENAI_API_KEY to your Groq key
docker compose up --build
```

Migrations are applied automatically on API startup (`db.Database.Migrate()` in
`Program.cs`) — no manual `dotnet ef database update` needed to run the app.

## Architecture & conventions

Clean Architecture, dependencies point **inward**:

```
ContentGen.Api  ──►  ContentGen.Application  ──►  ContentGen.Domain
ContentGen.Infrastructure ──► Application + Domain (implements the ports)
```

- **Domain** — entities only (`ArticleRawContent`, `ProcessedContent`, `PostPublication`).
  No dependencies.
- **Application** — use cases, DTOs, exceptions, validation (`PostRules`), and the
  **interfaces/ports** (`IArticleProvider`, `IContentExtractor`, `IAiContentService`,
  `IContentRepository`, `IPromptBuilder`). Depends only on Domain.
- **Infrastructure** — adapters that implement the ports: RSS, HTML extraction, the AI
  service, EF Core/SQLite. Depends on Application + Domain.
- **Api** — Minimal API endpoints + DI wiring in `Program.cs`, `GlobalExceptionHandler`,
  Scalar docs.

When adding behavior, put the rule in the layer that owns it: business logic in a use
case (Application), an external concern behind a new port + Infrastructure adapter, an
endpoint thin (delegates to a use case). New use cases and adapters are registered in
`Program.cs`.

Error handling: throw the domain-meaningful exception
(`ValidationException`→400, `NotFoundException`→404,
`AiResponseParseException`/`HttpRequestException`→502, others→500). The
`GlobalExceptionHandler` maps them to RFC 7807 ProblemDetails — don't build error
responses by hand in endpoints.

## Things that are easy to get wrong

- **Groq vs OpenAI / Production config.** The AI service (`OpenAiContentService`) is an
  OpenAI-compatible client pointed at Groq via `OpenAI:BaseUrl`. `appsettings.json`
  ships with OpenAI defaults; the Groq settings live in `appsettings.Development.json`
  (gitignored). **Containers run in `Production`**, so they ignore
  `appsettings.Development.json` — the Groq `BaseUrl`/`Model`/`MaxTokens` are injected as
  env vars in `docker-compose.yml`. If generation returns `401 invalid_api_key`, the
  provider config (not just the key) is wrong for the running environment.
- **Post length is enforced.** Posts must be **1,200–1,800 chars** (`PostRules`). After
  each AI response, posts out of range trigger a corrective regeneration loop
  (`BuildLengthCorrectionPrompt`, up to `OpenAI:MaxRegenerationAttempts`, default 2). On
  non-convergence it returns the best result and logs a warning. `PostRules` is the
  single source of truth for the range — change it there.
- **Generation is idempotent.** `POST /content/generate` takes `{ "articleId": "<GUID>" }`
  (a GUID, not a URL). If `ProcessedContent` already exists for the article it returns the
  cached result without calling the AI.
- **Articles are de-duped by URL**, not by GUID, in `SearchArticlesUseCase`.
- **Two AI calls per generation** — one per language (PT-BR, EN-US), independently; the 3
  posts per language each use a distinct angle (technical deep-dive / storytelling-opinion
  / practical takeaway).
- **Don't commit secrets.** Real keys go in `appsettings.Development.json` and `.env`,
  both gitignored. Use the `*.example` files as templates.

## Testing

xUnit tests in `tests/ContentGen.Tests/`: use cases, AI-response parsing
(`AiResponseParser`), `PostRules`, and integration tests via `WebApplicationFactory`
(`ContentGenApiFactory`) with in-memory/fake adapters in `TestDoubles/`. Add tests
alongside the layer you change; integration tests should not hit the real Groq API.

## Project layout

```
src/
  ContentGen.Api/            # Minimal API, endpoints, DI, Scalar, GlobalExceptionHandler
  ContentGen.Application/    # Use cases, ports, DTOs, PostRules, exceptions
  ContentGen.Domain/         # Entities
  ContentGen.Infrastructure/ # RSS, HTML, AI (Groq), EF Core + SQLite, migrations
  ContentGen.Web/            # React + Vite frontend
tests/ContentGen.Tests/      # xUnit
docs/specs/                  # Product/functional/architecture/implementation specs
```

## Docs

`README.md` is the public-facing overview (this is a portfolio project — keep it polished).
Design intent lives in `docs/specs/` (`01`–`04`). Interactive API docs at `/scalar` in
Development.
