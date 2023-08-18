namespace WPFMvvM.Framework.Exceptions;

internal class AppExceptionHandler : IDisposable, IExceptionHandler
{
    private Application? application;
    private ILogger? logger;
    private IExceptionHandler exceptionHandler;
    private bool disposedValue;

    private AppExceptionHandler(Application application, ILogger logger, IExceptionHandler? externalExceptionHanlder)
    {
        this.application = application;
        this.logger = logger;
        this.exceptionHandler = externalExceptionHanlder ?? this;
    }

    public static AppExceptionHandler Create(Application app, ILogger logger, IExceptionHandler? externalExceptionHanlder)
    {
        Guard.IsNotNull(app, nameof(app));
        Guard.IsNotNull(logger, nameof(logger));
        var handler = new AppExceptionHandler(app, logger, externalExceptionHanlder);
        handler.ConfigureApplicationExceptionsHandlers();
        return handler;
    }

    public void Handle(LogLevel logLevel, string message, Exception? ex = null)
        => exceptionHandler.Handle(logLevel, message, ex);

    void ConfigureApplicationExceptionsHandlers()
    {
        application!.DispatcherUnhandledException += HandleUiExceptions;
        AppDomain.CurrentDomain.UnhandledException += HandleDomainExceptions;
        TaskScheduler.UnobservedTaskException += HandleUnobservedTaskSchedulerException;
    }

    void HandleDomainExceptions(object sender, UnhandledExceptionEventArgs e)
        => exceptionHandler.Handle(LogLevel.Critical, "Domain exception occured. Application will terminate. See logs for details.", e.ExceptionObject as Exception);

    void HandleUiExceptions(object sender, DispatcherUnhandledExceptionEventArgs e)
        => exceptionHandler.Handle(LogLevel.Critical, "Global exception occured.", e.Exception);

    void HandleUnobservedTaskSchedulerException(object? sender, UnobservedTaskExceptionEventArgs e)
    => exceptionHandler.Handle(LogLevel.Critical, "Application asynchronous exception occured", e.Exception);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "This is global exception handler.")]
    void LogAndShowCriticalException(LogLevel logLevel, string message, Exception? ex = null)
    {
        if (ex is null)
        {
            logger!.Log(logLevel, message);
            MessageBox.Show(message, "Error");
        }
        else
        {
            logger!.Log(logLevel, ex, message);
            MessageBox.Show($"{message}{Environment.NewLine}{ex?.Message}", "Error");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                application!.DispatcherUnhandledException -= HandleUiExceptions;
                AppDomain.CurrentDomain.UnhandledException -= HandleDomainExceptions;
                TaskScheduler.UnobservedTaskException -= HandleUnobservedTaskSchedulerException;
                application = null;
                logger = null;
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
