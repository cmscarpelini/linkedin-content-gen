using ContentGen.Application.Interfaces;
using ContentGen.Application.Validation;

namespace ContentGen.Infrastructure.Services;

public class PromptBuilder : IPromptBuilder
{
    // The three posts each take a distinct angle so the user gets varied content, not three rewrites
    // of the same idea. Defined once and referenced by both the initial and the corrective prompts.
    private const string PostAnglesBlock =
        """
        POST ANGLES (the three posts MUST each take a DIFFERENT angle — never repeat the same one):
        - Post 1 — TECHNICAL DEEP-DIVE: precise and detailed, demonstrating hands-on expertise (architecture, trade-offs, gotchas, code-level reasoning).
        - Post 2 — STORYTELLING / OPINION: a personal narrative or a strong, well-reasoned point of view that sparks discussion and feels human.
        - Post 3 — PRACTICAL TAKEAWAY: what the reader can apply today, with concrete steps, a checklist or a quick win.
        """;

    private const string PostAnglesReminder =
        "Preserve each post's distinct angle: Post 1 = technical deep-dive, Post 2 = storytelling/opinion, Post 3 = practical takeaway.";

    public string BuildSystemPrompt() =>
        """
        You are a technical content specialist for LinkedIn with deep expertise in the Microsoft ecosystem (.NET, Azure, C#).

        Your writing style is MIXED: technically precise in summaries and insights, casual and approachable in explanations and post suggestions — as if explaining to a fellow developer over coffee.

        Output must be valid JSON matching the schema provided in the user message. Do not add any text outside the JSON block.
        """;

    public string BuildUserPrompt(string articleTitle, string articleUrl, string articleRawText, string languageCode, string languageName) =>
        $$"""
        Analyze the following article and generate structured LinkedIn content in {{languageName}} ({{languageCode}}) ONLY.

        ARTICLE TITLE: {{articleTitle}}
        ARTICLE URL: {{articleUrl}}
        ARTICLE CONTENT:
        {{articleRawText}}

        Generate the following content in {{languageName}} only:

        1. TECHNICAL SUMMARY (2-3 paragraphs): precise and informative, written for a developer audience.
        2. INSIGHTS (exactly 3): short, punchy, actionable technical takeaways.
        3. CASUAL EXPLANATION (2-3 sentences): explain the core idea as if talking to a dev friend. No jargon overload.

        4. POST SUGGESTIONS (exactly 3 LinkedIn posts).

        {{PostAnglesBlock}}

        MANDATORY CHARACTER REQUIREMENT: Each post MUST contain at least 1,200 characters and no more than 1,800 characters.
        Count every character: letters, spaces, punctuation, line breaks, hashtags.
        If a post is shorter than 1,200 characters, it is INVALID. Expand with more context, examples, and developer scenarios until it reaches 1,200+ characters.

        POST STRUCTURE (follow exactly):
        - Line 1: bold hook using ** (direct, creates curiosity or surprise)
        - [blank line]
        - 4 to 6 body paragraphs, each separated by a blank line, developing the topic with technical depth and real-world developer context
        - [blank line]
        - Closing question or call-to-action
        - [blank line]
        - 3 to 5 relevant hashtags

        Return ONLY the following JSON structure, no markdown, no explanation:

        {
          "technicalSummary": "string",
          "insights": ["string", "string", "string"],
          "casualExplanation": "string",
          "postSuggestions": ["string with 1200-1800 chars", "string with 1200-1800 chars", "string with 1200-1800 chars"]
        }
        """;

    public string BuildLengthCorrectionPrompt(IReadOnlyList<PostLengthIssue> issues, string languageName)
    {
        var lines = issues.Select(issue => issue.Problem == PostLengthProblem.TooShort
            ? $"- Post {issue.Index + 1} has {issue.Length} characters — TOO SHORT. Expand it to between {PostRules.MinLength} and {PostRules.MaxLength} characters with more technical depth, concrete examples and real-world developer context."
            : $"- Post {issue.Index + 1} has {issue.Length} characters — TOO LONG. Tighten it to between {PostRules.MinLength} and {PostRules.MaxLength} characters without losing the core message.");

        return $"""
        Your previous response did not meet the MANDATORY length requirement of {PostRules.MinLength}–{PostRules.MaxLength} characters per post:

        {string.Join("\n", lines)}

        Regenerate ALL THREE posts in {languageName}, fixing the issues above. Every post must land between {PostRules.MinLength} and {PostRules.MaxLength} characters (counting letters, spaces, punctuation, line breaks and hashtags).
        {PostAnglesReminder}
        Keep the exact same JSON schema as before. Return ONLY the JSON, no markdown, no explanation.
        """;
    }
}
