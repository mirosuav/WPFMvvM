namespace WPFMvvM.Framework.Exceptions;

public delegate void ExceptionHandler(LogLevel logLevel, string message, Exception? exception = null);
public interface IGlobalExceptionHandler : IDisposable
{
    ExceptionHandler Handle { get; }
}