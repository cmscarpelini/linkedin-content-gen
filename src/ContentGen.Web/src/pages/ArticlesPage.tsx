import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import type { ArticleSummary } from "../types";

export default function ArticlesPage() {
  const [articles, setArticles] = useState<ArticleSummary[]>([]);
  const [loading, setLoading] = useState(false);
  const [generating, setGenerating] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  async function handleSearch() {
    setLoading(true);
    setError(null);
    try {
      const data = await api.searchArticles();
      setArticles(data);
    } catch (e) {
      setError("Erro ao buscar artigos. Verifique se a API está rodando.");
    } finally {
      setLoading(false);
    }
  }

  async function handleGenerate(article: ArticleSummary) {
    setGenerating(article.id);
    setError(null);
    try {
      const pkg = await api.generateContent(article.id);
      navigate(`/content/${pkg.articleId}`);
    } catch (e) {
      setError("Erro ao gerar conteúdo.");
    } finally {
      setGenerating(null);
    }
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-800">Buscar Artigos</h1>
        <button
          onClick={handleSearch}
          disabled={loading}
          className="px-5 py-2 bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white text-sm font-medium rounded-lg transition-colors"
        >
          {loading ? "Buscando..." : "🔍 Buscar Artigos"}
        </button>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {articles.length === 0 && !loading && (
        <p className="text-gray-500 text-sm">
          Clique em "Buscar Artigos" para carregar os artigos mais recentes das fontes RSS.
        </p>
      )}

      <div className="grid gap-4">
        {articles.map((article) => (
          <div
            key={article.id}
            className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm flex items-start justify-between gap-4"
          >
            <div className="flex-1 min-w-0">
              <span className="inline-block text-xs font-medium px-2 py-0.5 rounded-full bg-blue-50 text-blue-700 mb-1">
                {article.source}
              </span>
              <h2 className="text-sm font-semibold text-gray-800 leading-snug mb-1 line-clamp-2">
                {article.title}
              </h2>
              <a
                href={article.url}
                target="_blank"
                rel="noopener noreferrer"
                className="text-xs text-blue-500 hover:underline truncate block"
              >
                {article.url}
              </a>
              <p className="text-xs text-gray-400 mt-1">
                {new Date(article.publishedAt).toLocaleDateString("pt-BR")}
              </p>
            </div>
            <button
              onClick={() => handleGenerate(article)}
              disabled={generating === article.id}
              className="shrink-0 px-4 py-2 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-xs font-semibold rounded-lg transition-colors"
            >
              {generating === article.id ? "Gerando..." : "✨ Gerar"}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
