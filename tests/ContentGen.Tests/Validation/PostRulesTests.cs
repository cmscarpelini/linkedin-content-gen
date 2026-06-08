using ContentGen.Application.Validation;

namespace ContentGen.Tests.Validation;

public class PostRulesTests
{
    [Theory]
    [InlineData(1200, true)]   // lower bound inclusive
    [InlineData(1800, true)]   // upper bound inclusive
    [InlineData(1500, true)]
    [InlineData(1199, false)]  // one under
    [InlineData(1801, false)]  // one over
    [InlineData(0, false)]
    public void IsWithinRange_ChecksInclusiveBounds(int length, bool expected)
    {
        var post = new string('x', length);

        Assert.Equal(expected, PostRules.IsWithinRange(post));
    }

    [Fact]
    public void FindIssues_WhenAllPostsValid_ReturnsNone()
    {
        var posts = new[] { new string('a', 1200), new string('b', 1500), new string('c', 1800) };

        Assert.Empty(PostRules.FindIssues(posts));
    }

    [Fact]
    public void FindIssues_FlagsShortAndLongPostsWithIndexAndLength()
    {
        var posts = new[]
        {
            new string('a', 800),   // too short
            new string('b', 1500),  // ok
            new string('c', 2000)   // too long
        };

        var issues = PostRules.FindIssues(posts);

        Assert.Equal(2, issues.Count);

        var shortIssue = issues.Single(i => i.Problem == PostLengthProblem.TooShort);
        Assert.Equal(0, shortIssue.Index);
        Assert.Equal(800, shortIssue.Length);

        var longIssue = issues.Single(i => i.Problem == PostLengthProblem.TooLong);
        Assert.Equal(2, longIssue.Index);
        Assert.Equal(2000, longIssue.Length);
    }
}
