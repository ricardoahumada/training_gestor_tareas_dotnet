namespace TaskManager.Attachments.Application.Exceptions;

/// <summary>
/// Exception thrown when a user is not authorized to perform an action.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }

    public ForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
