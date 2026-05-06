namespace TaskManager.Api.Exceptions;

public class InvalidJsonException : Exception
{
    public InvalidJsonException(string message) : base(message) { }
}
