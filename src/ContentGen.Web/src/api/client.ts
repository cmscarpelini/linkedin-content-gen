import type { ArticleSummary, SavedArticle, ContentSummary, ReviewPackage } from "../types";

const BASE = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5234";

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...init,
  });
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`);
  return res.json() as Promise<T>;
}

export const api = {
  searchArticles: () => request<ArticleSummary[]>("/articles/search"),

  listArticles: () => request<SavedArticle[]>("/articles"),

  generateContent: (articleId: string) =>
    request<ReviewPackage>("/content/generate", {
      method: "POST",
      body: JSON.stringify({ articleId }),
    }),

  listContent: () => request<ContentSummary[]>("/content"),

  getContent: (articleId: string) =>
    request<ReviewPackage>(`/content/${articleId}`),
};
