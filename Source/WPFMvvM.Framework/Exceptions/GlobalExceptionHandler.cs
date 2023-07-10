namespace WPFMvvM.Framework.Exceptions;

internal class GlobalExceptionHandler : IDisposable, IGlobalExceptionHandler
{
    private Application? application;
    private ILogger? logger;
    private bool disposedValue;
    public ExceptionHandler Handle { get; }

    private GlobalExceptionHandler(Application application, ILogger logger, ExceptionHandler? globalExceptionHanlder)
    {
        this.application = application;
        this.logger = logger;
        this.Handle = globalExceptionHanlder ?? LogAndShowCriticalException;
    }

    public static GlobalExceptionHandler Create(Application app, ILogger logger, ExceptionHandler? globalExceptionHanlder)
    {
        Guard.IsNotNull(app, nameof(app));
        Guard.IsNotNull(logger, nameof(logger));
        var handler = new GlobalExceptionHandler(app, logger, globalExceptionHanlder);
        handler.Configure();
        return handler;
    }


    void Configure()
    {
        //close application on explicit request
        application!.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        application!.DispatcherUnhandledException += HandleUiExceptions;
        AppDomain.CurrentDomain.UnhandledException += HandleDomainExceptions;
        TaskScheduler.UnobservedTaskException += HandleUnobservedTaskSchedulerException;
    }

    void HandleDomainExceptions(object sender, UnhandledExceptionEventArgs e)
        => Handle(LogLevel.Critical, "Domain exception occured. Application will terminate. See logs for details.", e.ExceptionObject as Exception);

    void HandleUiExceptions(object sender, DispatcherUnhandledExceptionEventArgs e)
        => Handle(LogLevel.Critical, "Global exception occured.", e.Exception);

    void HandleUnobservedTaskSchedulerException(object? sender, UnobservedTaskExceptionEventArgs e)
    => Handle(LogLevel.Critical, "Application asynchronous exception occured", e.Exception);

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
