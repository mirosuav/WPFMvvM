
using System.Reflection;
using System.Windows.Controls;
using WPFMvvM.Messages;

namespace WPFMvvM.Handlers;

/// <summary>
/// This class is responsible for resolving and binding up view models 
/// and their corresponding views based on BindViewAttribute
/// </summary>
public class ViewModelRequestHandler : IRecipient<ViewModelRequest>
{
    readonly IServiceProvider services;
    readonly IMessenger messenger;

    public ViewModelRequestHandler(IServiceProvider services, IMessenger messenger)
    {
        this.services = services;
        this.messenger = messenger;
        messenger.RegisterAll(this);
    }

    public (BaseViewModel ViewModel, object View) GetViewModel(Type viewModelType)
    {
        var vmAttr = viewModelType.GetCustomAttribute<BindViewAttribute>();
        Guard.IsNotNull(vmAttr, $"No View attribute defined for: {viewModelType.Name}!");

        var view = services.GetRequiredService(vmAttr.ViewType) as ContentControl;
        Guard.IsNotNull(view, $"Could not resolve view: {vmAttr.ViewType.FullName}. Make sure the view is registered in DI.");

        var viewModel = services.GetRequiredService(viewModelType) as BaseViewModel;
        Guard.IsNotNull(viewModel, $"Could not resolve view model: {viewModelType.Name}. Make sure the view is registered in DI.");

        viewModel.View = view;
        view.DataContext = viewModel;
        return (viewModel, view);

    }


    public void Receive(ViewModelRequest message)
    {
        message.Result = GetViewModel(message.ViewModelType).ViewModel;
    }
}
