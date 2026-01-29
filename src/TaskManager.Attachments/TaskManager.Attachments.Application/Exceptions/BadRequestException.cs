namespace TaskManager.Attachments.Application.Exceptions;

/// <summary>
/// Exception thrown when a request is invalid (e.g., business rule violation).
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
