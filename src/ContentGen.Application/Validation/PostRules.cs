namespace ContentGen.Application.Validation;

public enum PostLengthProblem
{
    TooShort,
    TooLong
}

/// <summary>A single LinkedIn post that violates the length rule, with its current length.</summary>
public record PostLengthIssue(int Index, int Length, PostLengthProblem Problem);

/// <summary>
/// The business rule for LinkedIn post length (1,200–1,800 characters), in one place so the
/// prompt, the validation and the corrective feedback all agree.
/// </summary>
public static class PostRules
{
    public const int MinLength = 1200;
    public const int MaxLength = 1800;

    public static bool IsWithinRange(string post) => post.Length is >= MinLength and <= MaxLength;

    /// <summary>Returns the posts (by index) that fall outside the allowed length range.</summary>
    public static IReadOnlyList<PostLengthIssue> FindIssues(IReadOnlyList<string> posts)
    {
        var issues = new List<PostLengthIssue>();
        for (var i = 0; i < posts.Count; i++)
        {
            var length = posts[i].Length;
            if (length < MinLength)
                issues.Add(new PostLengthIssue(i, length, PostLengthProblem.TooShort));
            else if (length > MaxLength)
                issues.Add(new PostLengthIssue(i, length, PostLengthProblem.TooLong));
        }
        return issues;
    }
}
