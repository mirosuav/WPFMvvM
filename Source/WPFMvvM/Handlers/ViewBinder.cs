using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WPFMvvM.Exceptions;
using WPFMvvM.Messages;
using WPFMvvM.Utils;

namespace WPFMvvM.Handlers;

internal class ViewBinder : IViewBinder
{
    private readonly WPFApplicationHost appHost;
    private readonly IMessenger messenger;
    private EventHandler<EventArgs>? windowActivatedHandler;

    public ViewBinder(IMessenger messenger, WPFApplicationHost appHost)
    {
        this.messenger = messenger;
        this.appHost = appHost;
    }

    public void BindView(BaseViewModel viewModel)
    {
        var vmAttr = viewModel.GetType().GetCustomAttribute<BindViewAttribute>();
        if (vmAttr is null)
            return;

        var view = Activator.CreateInstance(vmAttr.ViewType) as ContentControl;
        Guard.IsNotNull(view, $"Could not create an instance of UI part: {vmAttr.ViewType.FullName}");

        view.DataContext = viewModel;

        if (view is Window window)
        {
            if (viewModel is not BaseWindowModel windowModel)
                throw new UIException($"DataContext of {vmAttr.ViewType.FullName} must derive from {nameof(BaseWindowModel)}.");
            BindWindow(window, windowModel);
            EnsureWindowParent(window);
        }
    }

    private void BindWindow(Window window, BaseWindowModel windowModel)
    {
        messenger.Register<Window, WindowShowRequest, Guid>(window, windowModel.WindowRequestToken, WindowShowRequestHandler);
        messenger.Register<Window, WindowShowDialogRequest, Guid>(window, windowModel.WindowRequestToken, WindowShowDialogRequestHandler);
        messenger.Register<Window, WindowCloseRequest, Guid>(window, windowModel.WindowRequestToken, WindowCloseRequestHandler);

        windowActivatedHandler ??= new EventHandler<EventArgs>(Window_Activated);
        WeakEventManager<Window, EventArgs>.AddHandler(window, nameof(window.Activated), windowActivatedHandler);
    }

    private static async void Window_Activated(object? sender, EventArgs e)
    {
        if (sender is Window window && window.DataContext is BaseWindowModel windowModel)
        {
           await windowModel.ActivateCommand.ExecuteAsync(null);
        }
    }

    private static void WindowCloseRequestHandler(Window window, WindowCloseRequest message)
    {
        window.Close();
        message.Reply(true);
    }

    private static void WindowShowRequestHandler(Window window, WindowShowRequest message)
    {
        window.Show();
        message.Reply(true);
    }

    private static void WindowShowDialogRequestHandler(Window window, WindowShowDialogRequest message)
    {
        var result = window.ShowDialog();
        message.Reply(result ?? false);
    }

    private void EnsureWindowParent(Window window)
    {
        //the owner of all windows is main application widow
        if (window != appHost.HostedApp.MainWindow)
            window.Owner = appHost.HostedApp.MainWindow;
    }
}
