namespace ContentGen.Application.Interfaces;

public interface IPromptBuilder
{
    string BuildSystemPrompt();
    string BuildUserPrompt(string articleTitle, string articleUrl, string articleRawText, string languageCode, string languageName);
}
