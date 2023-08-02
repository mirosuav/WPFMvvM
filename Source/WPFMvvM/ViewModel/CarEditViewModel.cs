using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Extensions;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Messages;
using WPFMvvM.Model;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class CarEditViewModel : WPFMvvMBaseViewModel
{
    [ObservableProperty]
    CarModel? model;

    public CarEditViewModel(WPFMvvMAppScope scope) : base(scope)
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
        try
        {
            if (!Model.HasErrors)
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
    async Task Cancel(CancellationToken token)
    {
        try
        {
            Model?.Reload();
            Scope.Messenger.Send(new CarListNavigation());
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError($"Error in {nameof(CancelCommand)}", ex);
        }
    }
}
