namespace WPFMvvM.Framework.Exceptions
{
    internal interface IGlobalExceptionHandler
    {
        void Dispose();
        void Handle(LogLevel logLevel, string message, Exception? ex = null);
    }
}