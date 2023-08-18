namespace WPFMvvM.Framework.Exceptions;

public interface IExceptionHandler 
{
    void Handle(LogLevel logLevel, string message, Exception? exception = null);
}