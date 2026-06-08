using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ContentGen.Application.DTOs;
using ContentGen.Application.Exceptions;

namespace ContentGen.Infrastructure.Services;

/// <summary>
/// Turns a raw LLM completion into a strongly-typed <see cref="AiContentBlock"/>.
/// Language models are unreliable about output shape, so this parser tolerates the two most
/// common deviations: JSON wrapped in markdown code fences and literal CR/LF characters inside
/// JSON string values. On unrecoverable input it throws <see cref="AiResponseParseException"/>
/// carrying the original text for diagnostics.
/// </summary>
public static class AiResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static AiContentBlock Parse(string rawContent)
    {
        var json = ExtractJson(rawContent);
        json = SanitizeJsonStrings(json);

        try
        {
            return JsonSerializer.Deserialize<AiContentBlock>(json, JsonOptions)
                ?? throw new JsonException("Deserialized result was null.");
        }
        catch (JsonException ex)
        {
            throw new AiResponseParseException(rawContent, ex);
        }
    }

    /// <summary>Pulls the JSON object out of a completion, handling markdown fences and surrounding prose.</summary>
    public static string ExtractJson(string text)
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
    public static string SanitizeJsonStrings(string json)
    {
        var sb = new StringBuilder(json.Length);
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
