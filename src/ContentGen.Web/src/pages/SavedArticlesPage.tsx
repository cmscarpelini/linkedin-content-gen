import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../api/client";
import type { SavedArticle } from "../types";

export default function SavedArticlesPage() {
  const [articles, setArticles] = useState<SavedArticle[]>([]);
  const [loading, setLoading] = useState(true);
  const [generating, setGenerating] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    api
      .listArticles()
      .then(setArticles)
      .catch(() => setError("Erro ao carregar artigos. Verifique se a API está rodando."))
      .finally(() => setLoading(false));
  }, []);

  async function handleGenerate(article: SavedArticle) {
    setGenerating(article.id);
    setError(null);
    try {
      const pkg = await api.generateContent(article.id);
      navigate(`/content/${pkg.articleId}`);
    } catch {
      setError("Erro ao gerar conteúdo.");
      setGenerating(null);
    }
  }

  function handleView(articleId: string) {
    navigate(`/content/${articleId}`);
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">Artigos Salvos</h1>
          {!loading && (
            <p className="text-sm text-gray-500 mt-0.5">
              {articles.length} artigo{articles.length !== 1 ? "s" : ""} na base ·{" "}
              {articles.filter((a) => a.hasContent).length} com conteúdo gerado
            </p>
          )}
        </div>
      </div>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {loading && <p className="text-gray-500 text-sm">Carregando...</p>}

      {!loading && articles.length === 0 && !error && (
        <p className="text-gray-500 text-sm">
          Nenhum artigo salvo ainda. Vá para "Buscar Artigos" para importar do RSS.
        </p>
      )}

      <div className="grid gap-3">
        {articles.map((article) => (
          <div
            key={article.id}
            className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm flex items-start justify-between gap-4"
          >
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <span className="inline-block text-xs font-medium px-2 py-0.5 rounded-full bg-blue-50 text-blue-700">
                  {article.source}
                </span>
                {article.hasContent && (
                  <span className="inline-block text-xs font-medium px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-700">
                    ✓ Conteúdo gerado
                  </span>
                )}
              </div>
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

            <div className="shrink-0 flex flex-col gap-2">
              {article.hasContent ? (
                <button
                  onClick={() => handleView(article.id)}
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-xs font-semibold rounded-lg transition-colors"
                >
                  Ver Posts
                </button>
              ) : (
                <button
                  onClick={() => handleGenerate(article)}
                  disabled={generating === article.id}
                  className="px-4 py-2 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white text-xs font-semibold rounded-lg transition-colors"
                >
                  {generating === article.id ? "Gerando..." : "✨ Gerar"}
                </button>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
