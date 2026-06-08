# LinkedIn Content Gen

[![CI](https://github.com/cmscarpelini/linkedin-content-gen/actions/workflows/ci.yml/badge.svg)](https://github.com/cmscarpelini/linkedin-content-gen/actions/workflows/ci.yml)

AI-powered LinkedIn content generator for Microsoft ecosystem developers.

Reads RSS feeds from Microsoft tech blogs, extracts the article content, and uses **Groq (Llama 3.3 70B)** to generate bilingual LinkedIn posts — **PT-BR and EN-US** — ready to copy and publish.

---

## Why this project exists

Keeping a consistent LinkedIn presence is hard for developers. Finding relevant articles, reading them, writing posts — all of that takes time that most devs don't have.

This tool automates the pipeline:
1. Fetch the latest articles from Microsoft blogs
2. Pick one
3. Get AI-generated posts in both Portuguese and English
4. Review, copy, and publish

---

## Tech Stack

**Backend**
- .NET 10 / ASP.NET Core Minimal API
- Clean Architecture (Domain → Application → Infrastructure → API)
- EF Core 10 + SQLite
- Groq API (Llama 3.3 70B) via OpenAI-compatible SDK
- HtmlAgilityPack (HTML content extraction)
- System.ServiceModel.Syndication (RSS parsing)
- Scalar (interactive API docs at `/scalar`)

**Frontend**
- React 19 + TypeScript
- Vite 8
- Tailwind CSS v3
- react-router-dom v7

---

## Features

- 🔍 **Fetch articles** from configurable RSS sources (Microsoft DevBlogs, Azure, TechCommunity)
- 🗄️ **Saved articles** — view all articles already in the database, with content status
- ✨ **Generate content** — AI creates for each article:
  - Technical summary
  - 3 key insights
  - Casual explanation
  - 3 LinkedIn post suggestions, each with a **distinct angle** (technical deep-dive / storytelling-opinion / practical takeaway), 1,200–1,800 chars each — length is **enforced** (posts outside the range trigger an automatic corrective regeneration)
- 🌐 **Bilingual** — every generation produces PT-BR and EN-US independently
- 📋 **Copy button** — one click to copy any post, with character count
- ✅ **Mark as published** — flag individual posts as published on LinkedIn (per language + position) to keep track of what's already live
- 📄 **History** — browse all previously generated content
- ⚡ **Idempotent** — generating content for the same article twice returns the cached result without calling the AI again

---

## Project Structure

```
src/
  ContentGen.Api/           # Minimal API, endpoints, DI registration
  ContentGen.Application/   # Use cases, interfaces, DTOs
  ContentGen.Domain/        # Entities
  ContentGen.Infrastructure/ # RSS, HTML, AI, EF Core, SQLite
  ContentGen.Web/           # React frontend
tests/
  ContentGen.Tests/
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- A free [Groq API key](https://console.groq.com/)

### Backend

```bash
# Clone the repo
git clone https://github.com/cmscarpelini/linkedin-content-gen.git
cd linkedin-content-gen

# Create your local settings file from the example
cp src/ContentGen.Api/appsettings.Development.example.json src/ContentGen.Api/appsettings.Development.json
```

Edit `appsettings.Development.json` and replace `YOUR_GROQ_API_KEY_HERE` with your actual key:

```json
{
  "OpenAI": {
    "ApiKey": "your-groq-key-here",
    "Model": "llama-3.3-70b-versatile",
    "MaxTokens": 8192,
    "Temperature": 0.7,
    "BaseUrl": "https://api.groq.com/openai/v1",
    "MaxRegenerationAttempts": 2
  }
}
```

```bash
# Run the API (migrations are applied automatically on startup)
dotnet run --project src/ContentGen.Api --launch-profile http
# API running at http://localhost:5234
# Docs at http://localhost:5234/scalar
```

### Frontend

```bash
cd src/ContentGen.Web

# Create local env file
cp .env.example .env.local

npm install
npm run dev
# App running at http://localhost:5173
```

---

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/articles/search` | Fetch latest articles from RSS feeds |
| `GET` | `/articles` | List all articles saved in the database |
| `POST` | `/content/generate` | Generate bilingual content for an article |
| `GET` | `/content` | List all generated content (history) |
| `GET` | `/content/{articleId}` | Get full content for a specific article |
| `PUT` | `/content/{articleId}/posts/{language}/{index}/published` | Mark/unmark a single post as published on LinkedIn |

Full interactive docs available at `http://localhost:5234/scalar` when running in Development.

**Error responses** follow [RFC 7807 ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807) (`application/problem+json`): `404` for unknown articles, `502` when the AI provider or an external feed fails, `500` for unexpected errors. A global exception handler maps and logs all of them centrally.

---

## Configuration

RSS sources and article limit are configurable in `appsettings.json`:

```json
"RssSources": [
  "https://devblogs.microsoft.com/dotnet/feed/",
  "https://devblogs.microsoft.com/azure/feed/",
  "https://techcommunity.microsoft.com/plugins/custom/microsoft/o365/rss-board-messages?board=AzureArchitectureBlog"
],
"RssMaxArticles": 5
```

Add any RSS feed URL to the array — no recompilation needed.

---

## License

MIT
