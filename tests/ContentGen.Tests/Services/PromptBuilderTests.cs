using ContentGen.Application.Validation;
using ContentGen.Infrastructure.Services;

namespace ContentGen.Tests.Services;

public class PromptBuilderTests
{
    private readonly PromptBuilder _builder = new();

    [Fact]
    public void BuildLengthCorrectionPrompt_DescribesShortPost_WithOneBasedIndexAndExpandInstruction()
    {
        var issues = new List<PostLengthIssue> { new(0, 950, PostLengthProblem.TooShort) };

        var prompt = _builder.BuildLengthCorrectionPrompt(issues, "Brazilian Portuguese");

        Assert.Contains("Post 1", prompt);           // 1-based, not 0
        Assert.Contains("950 characters", prompt);
        Assert.Contains("TOO SHORT", prompt);
        Assert.Contains("Expand", prompt);
        Assert.Contains("Brazilian Portuguese", prompt);
        Assert.Contains(PostRules.MinLength.ToString(), prompt);
        Assert.Contains(PostRules.MaxLength.ToString(), prompt);
    }

    [Fact]
    public void BuildLengthCorrectionPrompt_DescribesLongPost_WithTightenInstruction()
    {
        var issues = new List<PostLengthIssue> { new(2, 2100, PostLengthProblem.TooLong) };

        var prompt = _builder.BuildLengthCorrectionPrompt(issues, "American English");

        Assert.Contains("Post 3", prompt);
        Assert.Contains("2100 characters", prompt);
        Assert.Contains("TOO LONG", prompt);
        Assert.Contains("Tighten", prompt);
    }

    [Fact]
    public void BuildLengthCorrectionPrompt_ListsEveryIssue()
    {
        var issues = new List<PostLengthIssue>
        {
            new(0, 800, PostLengthProblem.TooShort),
            new(1, 2200, PostLengthProblem.TooLong)
        };

        var prompt = _builder.BuildLengthCorrectionPrompt(issues, "American English");

        Assert.Contains("Post 1", prompt);
        Assert.Contains("Post 2", prompt);
        // Asks for a JSON-only response so the parser can consume it again.
        Assert.Contains("JSON", prompt);
    }

    [Fact]
    public void BuildLengthCorrectionPrompt_RemindsToPreserveAngles()
    {
        var issues = new List<PostLengthIssue> { new(0, 800, PostLengthProblem.TooShort) };

        var prompt = _builder.BuildLengthCorrectionPrompt(issues, "American English");

        Assert.Contains("angle", prompt, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildUserPrompt_AssignsThreeDistinctAnglesToThePosts()
    {
        var prompt = _builder.BuildUserPrompt("Title", "https://example.com", "body", "EN-US", "American English");

        Assert.Contains("POST ANGLES", prompt);
        Assert.Contains("TECHNICAL DEEP-DIVE", prompt);
        Assert.Contains("STORYTELLING", prompt);
        Assert.Contains("PRACTICAL TAKEAWAY", prompt);
    }

    [Fact]
    public void BuildUserPrompt_CarriesLanguageAndLengthRule()
    {
        var prompt = _builder.BuildUserPrompt("Title", "https://example.com", "body", "PT-BR", "Brazilian Portuguese");

        Assert.Contains("Brazilian Portuguese", prompt);
        Assert.Contains("1,200", prompt);
        Assert.Contains("1,800", prompt);
    }
}
