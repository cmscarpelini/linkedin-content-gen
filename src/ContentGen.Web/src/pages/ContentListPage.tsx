import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { api } from "../api/client";
import type { ContentSummary } from "../types";

export default function ContentListPage() {
  const [items, setItems] = useState<ContentSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .listContent()
      .then(setItems)
      .catch(() => setError("Erro ao carregar conteúdo."))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-6">Conteúdo Gerado</h1>

      {loading && <p className="text-gray-500 text-sm">Carregando...</p>}

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      {!loading && items.length === 0 && !error && (
        <p className="text-gray-500 text-sm">
          Nenhum conteúdo gerado ainda. Vá para "Buscar Artigos" e gere o primeiro.
        </p>
      )}

      <div className="grid gap-3">
        {items.map((item) => (
          <Link
            key={item.articleId}
            to={`/content/${item.articleId}`}
            className="bg-white border border-gray-200 rounded-xl p-4 shadow-sm hover:border-blue-300 hover:shadow-md transition-all block"
          >
            <div className="flex items-start justify-between gap-4">
              <div className="flex-1 min-w-0">
                <h2 className="text-sm font-semibold text-gray-800 leading-snug mb-1 line-clamp-2">
                  {item.articleTitle}
                </h2>
                <a
                  href={item.articleUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  onClick={(e) => e.stopPropagation()}
                  className="text-xs text-blue-500 hover:underline truncate block"
                >
                  {item.articleUrl}
                </a>
              </div>
              <div className="text-right shrink-0">
                <p className="text-xs text-gray-400">
                  {new Date(item.createdAt).toLocaleDateString("pt-BR")}
                </p>
                <span className="text-xs font-medium text-blue-600 mt-1 inline-block">
                  Ver posts →
                </span>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
