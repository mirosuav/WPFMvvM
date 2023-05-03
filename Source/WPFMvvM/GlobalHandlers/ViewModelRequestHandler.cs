using WPFMvvM.Handlers;
using WPFMvvM.Messages;

namespace WPFMvvM.GlobalHandlers;

internal class ViewModelRequestHandler : IGlobalHandler, IRecipient<ViewModelRequest>
{
    private readonly IViewBinder viewBinder;
    private readonly IServiceProvider services;

    public ViewModelRequestHandler(IViewBinder viewBinder, IServiceProvider services)
    {
        this.viewBinder = viewBinder;
        this.services = services;
    }

    private BaseViewModel GetViewModel(Type viewModelType)
    {
        var viewModel = services.GetRequiredService(viewModelType) as BaseViewModel;
        Guard.IsNotNull(viewModel, $"Could not resolve view model: {viewModelType.Name}. Make sure the view is registered in DI.");
        viewBinder.BindView(viewModel);
        return viewModel;
    }

    public void Receive(ViewModelRequest message)
        => message.Reply(GetViewModel(message.ViewModelType));

}
