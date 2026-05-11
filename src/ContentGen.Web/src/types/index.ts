export interface ArticleSummary {
  id: string;
  title: string;
  url: string;
  source: string;
  publishedAt: string;
}

export interface SavedArticle {
  id: string;
  title: string;
  url: string;
  source: string;
  publishedAt: string;
  hasContent: boolean;
}

export interface ContentSummary {
  articleId: string;
  articleTitle: string;
  articleUrl: string;
  createdAt: string;
}

export interface ContentBlock {
  technicalSummary: string;
  insights: string[];
  casualExplanation: string;
  postSuggestions: string[];
  consolidatedText: string;
}

export interface ReviewPackage {
  articleId: string;
  articleTitle: string;
  articleUrl: string;
  ptBR: ContentBlock;
  enUS: ContentBlock;
}
