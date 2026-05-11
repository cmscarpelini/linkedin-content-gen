using System.ClientModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContentGen.Application.DTOs;
using ContentGen.Application.Exceptions;
using ContentGen.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace ContentGen.Infrastructure.Services;

public class OpenAiContentService : IAiContentService
{
    private readonly IPromptBuilder _promptBuilder;
    private readonly ChatClient _chatClient;
    private readonly int _maxTokens;
    private readonly float _temperature;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public OpenAiContentService(IPromptBuilder promptBuilder, IConfiguration configuration)
    {
        _promptBuilder = promptBuilder;

        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
        var model = configuration["OpenAI:Model"] ?? "gpt-4o";

        _maxTokens = int.TryParse(configuration["OpenAI:MaxTokens"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var mt) ? mt : 4096;
        _temperature = float.TryParse(configuration["OpenAI:Temperature"], NumberStyles.Float, CultureInfo.InvariantCulture, out var temp) ? temp : 0.7f;

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

        var completion = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
        var rawContent = completion.Value.Content[0].Text;

        // Extract JSON block from the response (handles markdown code fences and plain JSON)
        var jsonContent = ExtractJson(rawContent);
        // Fix literal newlines inside JSON strings (model outputs real \n instead of \\n)
        jsonContent = SanitizeJsonStrings(jsonContent);

        try
        {
            return JsonSerializer.Deserialize<AiContentBlock>(jsonContent, JsonOptions)
                ?? throw new JsonException("Deserialized result was null.");
        }
        catch (JsonException ex)
        {
            throw new AiResponseParseException(rawContent, ex);
        }
    }

    private static string ExtractJson(string text)
    {
        // Try to extract from ```json ... ``` code fence
        var fenceStart = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (fenceStart >= 0)
        {
            var contentStart = text.IndexOf('\n', fenceStart) + 1;
            var fenceEnd = text.IndexOf("```", contentStart, StringComparison.OrdinalIgnoreCase);
            if (fenceEnd > contentStart)
                return text[contentStart..fenceEnd].Trim();
        }

        // Fall back: extract from first { to last }
        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        if (jsonStart >= 0 && jsonEnd > jsonStart)
            return text[jsonStart..(jsonEnd + 1)];

        return text;
    }

    /// <summary>Escapes literal CR/LF inside JSON string values so the JSON is parseable.</summary>
    private static string SanitizeJsonStrings(string json)
    {
        var sb = new System.Text.StringBuilder(json.Length);
        bool inString = false;
        bool escaped = false;

        foreach (char c in json)
        {
            if (escaped) { sb.Append(c); escaped = false; continue; }
            if (c == '\\' && inString) { escaped = true; sb.Append(c); continue; }
            if (c == '"') { inString = !inString; sb.Append(c); continue; }
            if (inString && c == '\n') { sb.Append("\\n"); continue; }
            if (inString && c == '\r') { sb.Append("\\r"); continue; }
            sb.Append(c);
        }

        return sb.ToString();
    }
}
