import { useState, useEffect } from "react";
import { useParams, Link } from "react-router-dom";
import { api } from "../api/client";
import type { ReviewPackage, ContentBlock, PostSuggestion, LanguageCode } from "../types";

function PostCard({
  post,
  articleId,
  language,
  onToggle,
}: {
  post: PostSuggestion;
  articleId: string;
  language: LanguageCode;
  onToggle: (index: number, published: boolean) => void;
}) {
  const [copied, setCopied] = useState(false);
  const [saving, setSaving] = useState(false);

  const withinRange = post.text.length >= 1200 && post.text.length <= 1800;

  async function handleCopy() {
    await navigator.clipboard.writeText(post.text);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  }

  async function togglePublished() {
    const next = !post.published;
    setSaving(true);
    try {
      await api.setPostPublished(articleId, language, post.index, next);
      onToggle(post.index, next);
    } catch {
      // keep current state on failure
    } finally {
      setSaving(false);
    }
  }

  return (
    <div
      className={`bg-white border rounded-xl p-4 shadow-sm ${
        post.published ? "border-emerald-300 ring-1 ring-emerald-200" : "border-gray-200"
      }`}
    >
      <div className="flex items-center justify-between mb-2">
        <div className="flex items-center gap-2">
          <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">
            Post #{post.index + 1}
          </span>
          {post.published && (
            <span className="text-xs font-medium px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-700">
              ✓ Publicado
            </span>
          )}
        </div>
        <div className="flex items-center gap-3">
          <span
            className={`text-xs font-medium px-2 py-0.5 rounded-full ${
              withinRange ? "bg-emerald-50 text-emerald-700" : "bg-amber-50 text-amber-700"
            }`}
          >
            {post.text.length} chars
          </span>
          <button
            onClick={togglePublished}
            disabled={saving}
            className={`text-xs px-3 py-1 rounded-lg font-medium transition-colors disabled:opacity-50 ${
              post.published
                ? "bg-emerald-50 text-emerald-700 hover:bg-emerald-100"
                : "bg-gray-100 text-gray-600 hover:bg-gray-200"
            }`}
          >
            {post.published ? "Desmarcar" : "Marcar publicado"}
          </button>
          <button
            onClick={handleCopy}
            className="text-xs px-3 py-1 rounded-lg bg-blue-50 text-blue-600 hover:bg-blue-100 font-medium transition-colors"
          >
            {copied ? "✓ Copiado!" : "Copiar"}
          </button>
        </div>
      </div>
      <p className="text-sm text-gray-700 whitespace-pre-wrap leading-relaxed">{post.text}</p>
    </div>
  );
}

function ContentBlockView({
  block,
  articleId,
  language,
  onTogglePost,
}: {
  block: ContentBlock;
  articleId: string;
  language: LanguageCode;
  onTogglePost: (index: number, published: boolean) => void;
}) {
  return (
    <div className="space-y-6">
      {/* Technical Summary */}
      <div>
        <h3 className="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-2">
          Resumo Técnico
        </h3>
        <p className="text-sm text-gray-700 leading-relaxed bg-gray-50 rounded-lg p-3">
          {block.technicalSummary}
        </p>
      </div>

      {/* Insights */}
      {block.insights?.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-2">
            Insights
          </h3>
          <ul className="space-y-1">
            {block.insights.map((insight, i) => (
              <li key={i} className="text-sm text-gray-700 flex gap-2">
                <span className="text-blue-500 mt-0.5 shrink-0">•</span>
                {insight}
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* Casual Explanation */}
      <div>
        <h3 className="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-2">
          Explicação Casual
        </h3>
        <p className="text-sm text-gray-700 leading-relaxed bg-gray-50 rounded-lg p-3">
          {block.casualExplanation}
        </p>
      </div>

      {/* Posts */}
      {block.postSuggestions?.length > 0 && (
        <div>
          <h3 className="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-3">
            Sugestões de Post LinkedIn
          </h3>
          <div className="space-y-4">
            {block.postSuggestions.map((post) => (
              <PostCard
                key={post.index}
                post={post}
                articleId={articleId}
                language={language}
                onToggle={onTogglePost}
              />
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

export default function ContentDetailPage() {
  const { articleId } = useParams<{ articleId: string }>();
  const [pkg, setPkg] = useState<ReviewPackage | null>(null);
  const [tab, setTab] = useState<"ptBR" | "enUS">("ptBR");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!articleId) return;
    api
      .getContent(articleId)
      .then(setPkg)
      .catch(() => setError("Erro ao carregar conteúdo."))
      .finally(() => setLoading(false));
  }, [articleId]);

  if (loading) return <p className="text-gray-500 text-sm">Carregando...</p>;
  if (error)
    return (
      <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
        {error}
      </div>
    );
  if (!pkg) return null;

  const blockKey: "ptBR" | "enUS" = tab;
  const languageCode: LanguageCode = tab === "ptBR" ? "pt-BR" : "en-US";
  const activeBlock: ContentBlock = pkg[blockKey];

  function handleTogglePost(index: number, published: boolean) {
    setPkg((prev) => {
      if (!prev) return prev;
      const block = prev[blockKey];
      const postSuggestions = block.postSuggestions.map((p) =>
        p.index === index ? { ...p, published } : p
      );
      return { ...prev, [blockKey]: { ...block, postSuggestions } };
    });
  }

  return (
    <div>
      {/* Header */}
      <div className="mb-6">
        <Link to="/content" className="text-sm text-blue-500 hover:underline mb-2 inline-block">
          ← Voltar
        </Link>
        <h1 className="text-xl font-bold text-gray-800 leading-snug">{pkg.articleTitle}</h1>
        <a
          href={pkg.articleUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="text-xs text-blue-500 hover:underline mt-1 inline-block"
        >
          {pkg.articleUrl}
        </a>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 mb-6 bg-gray-100 p-1 rounded-lg w-fit">
        <button
          onClick={() => setTab("ptBR")}
          className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${
            tab === "ptBR" ? "bg-white text-gray-800 shadow-sm" : "text-gray-500 hover:text-gray-700"
          }`}
        >
          🇧🇷 PT-BR
        </button>
        <button
          onClick={() => setTab("enUS")}
          className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${
            tab === "enUS" ? "bg-white text-gray-800 shadow-sm" : "text-gray-500 hover:text-gray-700"
          }`}
        >
          🇺🇸 EN-US
        </button>
      </div>

      <ContentBlockView
        block={activeBlock}
        articleId={pkg.articleId}
        language={languageCode}
        onTogglePost={handleTogglePost}
      />
    </div>
  );
}
