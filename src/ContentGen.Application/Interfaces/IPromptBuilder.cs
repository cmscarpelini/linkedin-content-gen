using ContentGen.Application.Validation;

namespace ContentGen.Application.Interfaces;

public interface IPromptBuilder
{
    string BuildSystemPrompt();
    string BuildUserPrompt(string articleTitle, string articleUrl, string articleRawText, string languageCode, string languageName);

    /// <summary>Builds a corrective follow-up message instructing the model to fix posts that violate the length rule.</summary>
    string BuildLengthCorrectionPrompt(IReadOnlyList<PostLengthIssue> issues, string languageName);
}
