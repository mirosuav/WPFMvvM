namespace WPFMvvM.Framework.UI;

/// <summary>
/// Binds Window to WindowModel using events
/// Events bindings are disposed when this service is disposed
/// </summary>
public class WindowBinder : IWindowBinder
{
    private readonly IMessenger _messenger;

    public WindowBinder(IMessenger messenger)
    {
        _messenger = messenger;
    }

    public void BindEvents(Window window, BaseWindowModel windowModel)
    {
        window.Activated += Window_Activated;
        window.Closing += Window_Closing;
        window.Closed += Window_Closed;
        _messenger.Register<Window, WindowCloseRequests, Guid>(window, windowModel.WindowRequestToken, WindowCloseRequestHandler);
    }

    void WindowCloseRequestHandler(Window window, WindowCloseRequests message)
    {
        window.Close();
        message.Reply(true);
    }

    async void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (sender is Window window && window.DataContext is BaseWindowModel windowModel)
        {
            if (windowModel.IsClosed)
            {
                e.Cancel = false;
            }
            else
            {
                var param = new ClosingCommandParam { IsTriggeredFromUI = true };
                //WindowModel will call Close 
                await windowModel.CloseCommand.ExecuteAsync(param);
                e.Cancel = !param.CanClose;
            }
        }
    }

    async void Window_Activated(object? sender, EventArgs e)
    {
        if (sender is Window window && window.DataContext is BaseWindowModel windowModel)
        {
            await windowModel.ActivateCommand.ExecuteAsync(null);
        }
    }

    void Window_Closed(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            window.Activated -= Window_Activated;
            window.Closing -= Window_Closing;
            window.Closed -= Window_Closed;
            _messenger.UnregisterAll(window);
        }
    }
}
