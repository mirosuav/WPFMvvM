using System.Reflection;
using System.Windows.Controls;

namespace WPFMvvM.Framework.Services;

/// <summary>
/// Creates and shows Windows for BaseWindowModel
/// </summary>
public class DialogService : IDialogService
{
    private readonly Window _mainAppWindow;
    private readonly IWindowBinder _windowBinder;

    public DialogService(IWPFApplicationHost appHost, IWindowBinder windowBinder)
    {
        _mainAppWindow = appHost.HostedApplication.MainWindow;
        _windowBinder = windowBinder;
    }

    public bool ShowDialog<TWindowModel>(TWindowModel windowModel) where TWindowModel : BaseWindowModel
    {
        return ResolveWindowFor(windowModel).ShowDialog() ?? false;
    }

    public bool Show<TWindowModel>(TWindowModel viewModel) where TWindowModel : BaseWindowModel
    {
        ResolveWindowFor(viewModel).Show();
        return true;
    }


    Window ResolveWindowFor(BaseWindowModel windowModel)
    {
        var view = ResolveAttributedView(windowModel);

        var window = view is Window uiWindow ? uiWindow : WrapWithShellWindow(view);

        EnsureWindowParent(window);

        _windowBinder.BindViewModel(window, windowModel);

        return window;
    }

    static Window WrapWithShellWindow(ContentControl ui)
    {
        return new ShellWindow { Content = ui };
    }

    ContentControl ResolveAttributedView(BaseWindowModel windowModel)
    {
        var vmAttr = windowModel.GetType().GetCustomAttribute<UseWindowAttribute>();
        Guard.IsNotNull(vmAttr, $"No {nameof(UseWindowAttribute)} defined on {windowModel.GetType().FullName}");

        if (!vmAttr.ViewType.IsAssignableTo(typeof(Window)))
            throw new Exception($"Type {vmAttr.ViewType.Name} is not a Window type!");

        var view = Activator.CreateInstance(vmAttr.ViewType) as ContentControl;
        Guard.IsNotNull(view, $"Could not create an instance of UI part: {vmAttr.ViewType.FullName}");

        return view;
    }

    void EnsureWindowParent(Window window)
    {
        //the owner of all windows is main application widow
        if (window != _mainAppWindow)
            window.Owner = _mainAppWindow;
    }
}
