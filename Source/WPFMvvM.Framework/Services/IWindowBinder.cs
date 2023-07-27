namespace WPFMvvM.Framework.Services;

public interface IWindowBinder
{
    void BindViewModel(Window window, BaseWindowModel windowModel);
}