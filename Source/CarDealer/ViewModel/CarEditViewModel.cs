using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CarDealer.Extensions;
using WPFMvvM.Framework.Exceptions;
using CarDealer.Messages;
using CarDealer.Model;
using CarDealer.Services;

namespace CarDealer.ViewModel;

public partial class CarEditViewModel : CarDealerBaseViewModel
{
    [ObservableProperty]
    CarModel? model;

    public CarEditViewModel(CarDealerAppScope scope) : base(scope)
    {
    }

    protected override async ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        await base.InitializeInternal(cancelltoken, parameters);
        var carId = parameters.GetFirstAs<int>();
        if (carId <= 0)
            throw new ApplicationException("No Car id provided for edit.");

        Model = await CarModel.Load(Scope.Data, carId);
    }


    [RelayCommand]
    async Task Save(CancellationToken token)
    {
        if (Model is null)
            return;
        try
        {
            if (Model.Validate())
            {
                Model.Save();
                await Scope.Data.SaveChangesAsync(token).FreeContext();
                Scope.Messenger.Send(new CarListNavigation());
            }
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(SaveCommand)}", ex);
        }
    }

    [RelayCommand]
    Task Cancel(CancellationToken token)
    {
        if (Model is null)
            return Task.CompletedTask;
        try
        {
            Model?.Reload();
            Scope.Messenger.Send(new CarListNavigation());
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(CancelCommand)}", ex);
        }
        return Task.CompletedTask;
    }
}
