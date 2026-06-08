using System.ClientModel;
using System.Globalization;
using ContentGen.Application.DTOs;
using ContentGen.Application.Interfaces;
using ContentGen.Application.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;

namespace ContentGen.Infrastructure.Services;

public class OpenAiContentService : IAiContentService
{
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILogger<OpenAiContentService> _logger;
    private readonly ChatClient _chatClient;
    private readonly int _maxTokens;
    private readonly float _temperature;
    private readonly int _maxRegenerationAttempts;

    public OpenAiContentService(IPromptBuilder promptBuilder, IConfiguration configuration, ILogger<OpenAiContentService> logger)
    {
        _promptBuilder = promptBuilder;
        _logger = logger;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        var model = configuration["OpenAI:Model"] ?? "gpt-4o";

        _maxTokens = int.TryParse(configuration["OpenAI:MaxTokens"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var mt) ? mt : 4096;
        _temperature = float.TryParse(configuration["OpenAI:Temperature"], NumberStyles.Float, CultureInfo.InvariantCulture, out var temp) ? temp : 0.7f;
        _maxRegenerationAttempts = int.TryParse(configuration["OpenAI:MaxRegenerationAttempts"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var mr) ? mr : 2;

        var baseUrl = configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
        var clientOptions = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
        _chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions).GetChatClient(model);
    }

    public async Task<AiContentResponse> GenerateContentAsync(
        string articleTitle,
        string articleUrl,
        string articleRawText,
        CancellationToken cancellationToken = default)
    {
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _maxTokens,
            Temperature = _temperature
        };

        var ptBR = await GenerateBlockAsync("PT-BR", "Brazilian Portuguese", articleTitle, articleUrl, articleRawText, options, cancellationToken);
        var enUS = await GenerateBlockAsync("EN-US", "American English", articleTitle, articleUrl, articleRawText, options, cancellationToken);

        return new AiContentResponse(ptBR, enUS);
    }

    private async Task<AiContentBlock> GenerateBlockAsync(
        string languageCode,
        string languageName,
        string articleTitle,
        string articleUrl,
        string articleRawText,
        ChatCompletionOptions options,
        CancellationToken cancellationToken)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(_promptBuilder.BuildSystemPrompt()),
            new UserChatMessage(_promptBuilder.BuildUserPrompt(articleTitle, articleUrl, articleRawText, languageCode, languageName))
        };

        AiContentBlock? best = null;
        var bestInRange = -1;

        for (var attempt = 0; ; attempt++)
        {
            var completion = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var rawContent = completion.Value.Content[0].Text;
            var block = AiResponseParser.Parse(rawContent);

            var issues = PostRules.FindIssues(block.PostSuggestions);
            if (issues.Count == 0)
                return block;

            // Keep the attempt with the most posts inside the allowed range as a fallback.
            var inRange = block.PostSuggestions.Count - issues.Count;
            if (inRange > bestInRange)
            {
                bestInRange = inRange;
                best = block;
            }

            if (attempt >= _maxRegenerationAttempts)
            {
                _logger.LogWarning(
                    "{Language} content still has {IssueCount} post(s) outside {Min}-{Max} chars after {Attempts} attempt(s); returning best effort",
                    languageCode, issues.Count, PostRules.MinLength, PostRules.MaxLength, attempt + 1);
                return best!;
            }

            _logger.LogInformation(
                "Regenerating {Language} posts (attempt {Attempt}): {IssueCount} post(s) outside the {Min}-{Max} char range",
                languageCode, attempt + 2, issues.Count, PostRules.MinLength, PostRules.MaxLength);

            // Continue the same conversation: echo the model's output, then ask it to fix the offenders.
            messages.Add(new AssistantChatMessage(rawContent));
            messages.Add(new UserChatMessage(_promptBuilder.BuildLengthCorrectionPrompt(issues, languageName)));
        }
    }
}
