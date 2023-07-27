namespace WPFMvvM.Framework.Services;

public interface IDialogService
{
    bool Show<TWindowModel>(TWindowModel windowModel) where TWindowModel : BaseWindowModel;
    bool ShowDialog<TWindowModel>(TWindowModel windowModel) where TWindowModel : BaseWindowModel;
}