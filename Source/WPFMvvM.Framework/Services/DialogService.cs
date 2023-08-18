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

    public DialogService(IWPFAppHost appHost, IWindowBinder windowBinder)
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
        var viewAttribute = ResolveWindowAttribute(windowModel);

        var view = CreateView(viewAttribute);

        var window = view is Window uiWindow ? uiWindow : WrapViewInWindow(view, viewAttribute);

        EnsureWindowParent(window);

        _windowBinder.BindViewModel(window, windowModel);

        return window;
    }

    static ContentControl CreateView(UseWindowAttribute viewAttribute)
    {
        var view = Activator.CreateInstance(viewAttribute.ViewType) as ContentControl;
        Guard.IsNotNull(view, $"Could not create an instance of UI part: {viewAttribute.ViewType.FullName}");
        return view;
    }

    static Window WrapViewInWindow(ContentControl ui, UseWindowAttribute viewAttribute)
    {
        return new Window
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            WindowStyle = WindowStyle.ToolWindow,
            Width = viewAttribute.ViewWidth,
            Height = viewAttribute.ViewHeight,
            Title = viewAttribute.Title,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = ui
        };
    }

    static UseWindowAttribute ResolveWindowAttribute(BaseWindowModel windowModel)
    {
        var vmAttr = windowModel.GetType().GetCustomAttribute<UseWindowAttribute>();
        Guard.IsNotNull(vmAttr, $"No {nameof(UseWindowAttribute)} defined on {windowModel.GetType().FullName}");

        if (!vmAttr.ViewType.IsAssignableTo(typeof(Window)) && !vmAttr.UseGenericDialog)
            throw new Exception($"Type {vmAttr.ViewType.Name} is not a Window type, set UseGenericDialog=true to use generic window!");

        return vmAttr;
    }

    void EnsureWindowParent(Window window)
    {
        //the owner of all windows is main application widow
        if (window != _mainAppWindow)
            window.Owner = _mainAppWindow;
    }
}
