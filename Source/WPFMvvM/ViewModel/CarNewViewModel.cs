using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Extensions;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Messages;
using WPFMvvM.Model;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class CarNewViewModel : WPFMvvMBaseViewModel
{
    [ObservableProperty]
    CarModel model;

    public CarNewViewModel(WPFMvvMAppScope scope) : base(scope)
    {
        Model = new CarModel(Scope.Data, new Domain.Car());
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
    async Task Cancel(CancellationToken token)
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
