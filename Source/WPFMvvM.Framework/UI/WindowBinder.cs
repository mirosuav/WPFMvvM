using WPFMvvM.Framework.Exceptions;

namespace WPFMvvM.Framework.UI;

/// <summary>
/// Binds Window to WindowModel using events
/// Events bindings are disposed when this service is disposed
/// </summary>
public class WindowBinder : IWindowBinder
{
    private readonly IMessenger messenger;
    private readonly IGlobalExceptionHandler exceptionHandler;

    public WindowBinder(IMessenger messenger, IGlobalExceptionHandler exceptionHandler)
    {
        Guard.IsNotNull(messenger);
        Guard.IsNotNull(exceptionHandler);
        this.messenger = messenger;
        this.exceptionHandler = exceptionHandler;
    }

    public void BindViewModel(Window window, BaseWindowModel windowModel)
    {
        window.DataContext = windowModel;
        window.Activated += Window_Activated;
        window.Closing += Window_Closing;
        window.Closed += Window_Closed;
        messenger.Register<Window, SelfWindowCloseRequest, Guid>(window, windowModel.WindowRequestToken, WindowCloseRequestHandler);
    }

    void WindowCloseRequestHandler(Window window, SelfWindowCloseRequest message)
    {
        //prevent cycle calling window model
        window.Closing -= Window_Closing;
        window.Close();
        message.Reply(true);
    }

    async void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            if (sender is Window window && window.DataContext is BaseWindowModel windowModel)
            {
                var request = new WindowClosingRequest { UITriggered = true };
                await windowModel.CloseCommand.ExecuteAsync(request);
                e.Cancel = !request.Response;
            }
        }
        catch (Exception ex)
        {
            exceptionHandler.Handle(LogLevel.Error, "Error in window closing event handler", ex);
        }
    }

    async void Window_Activated(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Window window && window.DataContext is BaseWindowModel windowModel)
            {
                await windowModel.ActivateCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            exceptionHandler.Handle(LogLevel.Error, "Error in window activated event handler", ex);
        }
    }

    void Window_Closed(object? sender, EventArgs e)
    {
        try
        {
            if (sender is Window window)
            {
                window.Activated -= Window_Activated;
                window.Closing -= Window_Closing;
                window.Closed -= Window_Closed;
                messenger.UnregisterAll(window);
                window.DataContext = null;
            }
        }
        catch (Exception ex)
        {
            exceptionHandler.Handle(LogLevel.Error, "Error in window closed event handler", ex);
        }
    }
}
