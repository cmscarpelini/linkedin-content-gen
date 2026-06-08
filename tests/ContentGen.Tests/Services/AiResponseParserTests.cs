using System.Text.Json;
using ContentGen.Application.Exceptions;
using ContentGen.Infrastructure.Services;

namespace ContentGen.Tests.Services;

public class AiResponseParserTests
{
    private const string ValidJson =
        """{ "technicalSummary": "A summary", "insights": ["one", "two", "three"], "casualExplanation": "Casual take", "postSuggestions": ["p1", "p2", "p3"] }""";

    // ---- Parse: happy paths ----

    [Fact]
    public void Parse_PlainJson_DeserializesAllFields()
    {
        var block = AiResponseParser.Parse(ValidJson);

        Assert.Equal("A summary", block.TechnicalSummary);
        Assert.Equal(["one", "two", "three"], block.Insights);
        Assert.Equal("Casual take", block.CasualExplanation);
        Assert.Equal(["p1", "p2", "p3"], block.PostSuggestions);
    }

    [Fact]
    public void Parse_JsonWrappedInMarkdownFence_Deserializes()
    {
        var raw = $"Here is your content:\n```json\n{ValidJson}\n```\nHope it helps!";

        var block = AiResponseParser.Parse(raw);

        Assert.Equal("A summary", block.TechnicalSummary);
        Assert.Equal(3, block.PostSuggestions.Count);
    }

    [Fact]
    public void Parse_IsCaseInsensitiveOnPropertyNames()
    {
        var raw = """{ "TechnicalSummary": "S", "Insights": [], "CasualExplanation": "C", "PostSuggestions": [] }""";

        var block = AiResponseParser.Parse(raw);

        Assert.Equal("S", block.TechnicalSummary);
        Assert.Equal("C", block.CasualExplanation);
    }

    [Fact]
    public void Parse_PostWithLiteralNewlines_DeserializesAndPreservesLineBreaks()
    {
        // LLMs frequently emit real line breaks inside string values, which is invalid JSON.
        var raw = "{ \"technicalSummary\": \"line one\nline two\", \"insights\": [], \"casualExplanation\": \"\", \"postSuggestions\": [] }";

        var block = AiResponseParser.Parse(raw);

        Assert.Equal("line one\nline two", block.TechnicalSummary);
    }

    // ---- Parse: failure path ----

    [Fact]
    public void Parse_UnrecoverableContent_ThrowsWithRawContentAttached()
    {
        var raw = "I'm sorry, I cannot help with that request.";

        var ex = Assert.Throws<AiResponseParseException>(() => AiResponseParser.Parse(raw));
        Assert.Equal(raw, ex.RawContent);
        Assert.IsType<JsonException>(ex.InnerException);
    }

    // ---- ExtractJson ----

    [Fact]
    public void ExtractJson_FromFence_ReturnsInnerJsonTrimmed()
    {
        var raw = "prose\n```json\n{ \"a\": 1 }\n```\nmore prose";

        Assert.Equal("{ \"a\": 1 }", AiResponseParser.ExtractJson(raw));
    }

    [Fact]
    public void ExtractJson_WithSurroundingProse_ReturnsFirstBraceToLastBrace()
    {
        var raw = "Sure! { \"a\": 1 } that's it.";

        Assert.Equal("{ \"a\": 1 }", AiResponseParser.ExtractJson(raw));
    }

    [Fact]
    public void ExtractJson_WithNoJson_ReturnsOriginalText()
    {
        var raw = "no json here";

        Assert.Equal(raw, AiResponseParser.ExtractJson(raw));
    }

    // ---- SanitizeJsonStrings ----

    [Fact]
    public void SanitizeJsonStrings_EscapesNewlinesOnlyInsideStrings()
    {
        var input = "{\n  \"k\": \"a\nb\"\n}";

        var result = AiResponseParser.SanitizeJsonStrings(input);

        // The newline inside the value is escaped...
        Assert.Contains("\"a\\nb\"", result);
        // ...but structural newlines between tokens are left untouched.
        Assert.Contains("{\n", result);
    }

    [Fact]
    public void SanitizeJsonStrings_LeavesAlreadyEscapedSequencesUntouched()
    {
        var input = "\"a\\nb\"";

        Assert.Equal("\"a\\nb\"", AiResponseParser.SanitizeJsonStrings(input));
    }

    [Fact]
    public void SanitizeJsonStrings_EscapesCarriageReturnsInsideStrings()
    {
        var input = "\"a\rb\"";

        Assert.Equal("\"a\\rb\"", AiResponseParser.SanitizeJsonStrings(input));
    }
}
