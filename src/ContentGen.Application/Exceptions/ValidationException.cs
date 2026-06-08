namespace ContentGen.Application.Exceptions;

/// <summary>Thrown when a request carries invalid input. Mapped to HTTP 400 at the API boundary.</summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
