namespace WPFMvvM.Exceptions;

public class UIException : Exception
{
    public UIException(string? message) : base(message)
    {
    }

    public UIException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
