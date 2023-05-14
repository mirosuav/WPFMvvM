namespace WPFMvvM.Framework.Exceptions
{
    public interface IGlobalExceptionHandler
    {
        void Dispose();
        void Handle(LogLevel logLevel, string message, Exception? ex = null);
    }
}