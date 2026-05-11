import { BrowserRouter, Routes, Route, NavLink } from "react-router-dom";
import ArticlesPage from "./pages/ArticlesPage";
import SavedArticlesPage from "./pages/SavedArticlesPage";
import ContentListPage from "./pages/ContentListPage";
import ContentDetailPage from "./pages/ContentDetailPage";

export default function App() {
  return (
    <BrowserRouter>
      <div className="min-h-screen bg-gray-50 flex">
        <nav className="w-56 bg-gray-900 text-gray-100 flex flex-col p-4 gap-2 shrink-0">
          <div className="mb-6">
            <span className="text-lg font-bold text-white">ContentGen</span>
            <p className="text-xs text-gray-500 mt-0.5">LinkedIn AI Content</p>
          </div>
          <NavLink
            to="/"
            end
            className={({ isActive }) =>
              `px-3 py-2 rounded-lg text-sm font-medium transition-colors flex items-center gap-2 ${
                isActive ? "bg-blue-600 text-white" : "text-gray-400 hover:bg-gray-800 hover:text-white"
              }`
            }
          >
            <span>🔍</span> Buscar Artigos
          </NavLink>
          <NavLink
            to="/articles"
            className={({ isActive }) =>
              `px-3 py-2 rounded-lg text-sm font-medium transition-colors flex items-center gap-2 ${
                isActive ? "bg-blue-600 text-white" : "text-gray-400 hover:bg-gray-800 hover:text-white"
              }`
            }
          >
            <span>🗄️</span> Artigos Salvos
          </NavLink>
          <NavLink
            to="/content"
            className={({ isActive }) =>
              `px-3 py-2 rounded-lg text-sm font-medium transition-colors flex items-center gap-2 ${
                isActive ? "bg-blue-600 text-white" : "text-gray-400 hover:bg-gray-800 hover:text-white"
              }`
            }
          >
            <span>📄</span> Conteúdo Gerado
          </NavLink>
        </nav>
        <main className="flex-1 p-8 overflow-auto">
          <Routes>
            <Route path="/" element={<ArticlesPage />} />
            <Route path="/articles" element={<SavedArticlesPage />} />
            <Route path="/content" element={<ContentListPage />} />
            <Route path="/content/:articleId" element={<ContentDetailPage />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}
