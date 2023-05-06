namespace WPFMvvM.Framework.Infrastructure
{
    public interface IWindowBinder
    {
        void BindEvents(Window window, BaseWindowModel windowModel);
    }
}