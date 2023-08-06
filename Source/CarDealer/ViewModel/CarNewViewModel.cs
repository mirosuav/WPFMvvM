using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CarDealer.Extensions;
using WPFMvvM.Framework.Exceptions;
using CarDealer.Messages;
using CarDealer.Model;
using CarDealer.Services;

namespace CarDealer.ViewModel;

public partial class CarNewViewModel : CarDealerBaseViewModel
{
    [ObservableProperty]
    CarModel model;

    public CarNewViewModel(CarDealerAppScope scope) : base(scope)
    {
        Model = new CarModel(Scope.Data, new Domain.Car());
    }


    [RelayCommand]
    async Task Save(CancellationToken token)
    {
        try
        {
            
            if (Model.Validate())
            {
                Model.Save();
                await Scope.Data.SaveChangesAsync(token).FreeContext();
                Scope.Messenger.Send(new PromptMessage("Car saved", $"New car '{Model.ModelName}' was created."));
                Scope.Messenger.Send(new CarListNavigation());
            }
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(SaveCommand)}", ex);
        }
    }

    [RelayCommand]
    void Cancel()
    {
        try
        {
            Scope.Messenger.Send(new CarListNavigation());
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(CancelCommand)}", ex);
        }
    }

}
