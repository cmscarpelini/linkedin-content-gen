# ContentGen — Web

React + TypeScript frontend for **ContentGen**, the AI-powered LinkedIn content
generator. It talks to the [ContentGen API](../ContentGen.Api) to search articles,
trigger bilingual content generation, and review/copy the generated posts.

> For the full project overview, architecture, and Docker setup, see the
> [root README](../../README.md).

## Stack

- **React 19** + **TypeScript**
- **Vite 8** (dev server + build)
- **Tailwind CSS v3**
- **react-router-dom v7**

## Getting started

```bash
# from src/ContentGen.Web
cp .env.example .env.local      # set VITE_API_BASE_URL (default: http://localhost:5234)
npm install
npm run dev                     # http://localhost:5173
```

The API must be running for the app to do anything useful — start it with
`dotnet run --project ../ContentGen.Api --launch-profile http`, or bring up the whole
stack with `docker compose up --build` from the repo root.

## Scripts

| Command | Description |
|---|---|
| `npm run dev` | Start the Vite dev server with HMR |
| `npm run build` | Type-check (`tsc -b`) and build for production |
| `npm run lint` | Run ESLint (CI fails on errors) |
| `npm run preview` | Serve the production build locally |

## Structure

```
src/
  api/client.ts     # fetch wrappers for every API endpoint
  types/index.ts    # shared TypeScript interfaces
  pages/            # ArticlesPage, SavedArticlesPage, ContentListPage, ContentDetailPage
  App.tsx           # BrowserRouter + sidebar + routes
  main.tsx          # entry point
```

## Pages

| Route | Component | Description |
|---|---|---|
| `/` | `ArticlesPage` | Search RSS articles and trigger generation |
| `/articles` | `SavedArticlesPage` | Articles saved in the DB, with content status |
| `/content` | `ContentListPage` | History of all generated content |
| `/content/:articleId` | `ContentDetailPage` | PT-BR / EN-US tabs, per-post char count, copy button, and "mark as published" toggle |

## Configuration

`VITE_API_BASE_URL` points the client at the API. In dev it lives in `.env.local`
(gitignored — use `.env.example` as the template). In the Docker image it's a **build
arg** baked into the bundle at build time (see [`Dockerfile`](Dockerfile) and the root
`docker-compose.yml`).
