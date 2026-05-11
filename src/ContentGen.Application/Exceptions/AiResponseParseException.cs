namespace ContentGen.Application.Exceptions;

public class AiResponseParseException : Exception
{
    public string RawContent { get; }

    public AiResponseParseException(string rawContent, Exception inner)
        : base("Failed to parse AI response as AiContentResponse.", inner)
    {
        RawContent = rawContent;
    }
}
