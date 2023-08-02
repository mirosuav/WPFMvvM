using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Messages;
using WPFMvvM.Model;
using WPFMvvM.Services;

namespace WPFMvvM.ViewModel;

public partial class CarListViewModel : WPFMvvMBaseViewModel
{
    [ObservableProperty]
    CarCollectionModel? cars;

    public CarListViewModel(WPFMvvMAppScope scope) : base(scope)
    {
    }

    protected override async ValueTask InitializeInternal(CancellationToken cancelltoken, params object[] parameters)
    {
        await base.InitializeInternal(cancelltoken, parameters);
        Cars = await CarCollectionModel.Load(Scope.Data);
    }

    [RelayCommand]
    private void Edit(CarModel model)
    {
        try
        {
            var entityId = model.EntityId ?? throw new ArgumentNullException("Car is not persisted yet!");
            Scope.Messenger.Send(new CarEditNavigation(entityId));
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError("Error initializing car list editor", ex);
        }
    }

    [RelayCommand]
    private async Task Delete(CarModel model, CancellationToken token)
    {
        try
        {
            var deleteQuestion = new QuestionMessage("Deleting car", $"Are you sure to delete car {model.ModelName}?");
            Scope.Messenger.Send(deleteQuestion);
            if (deleteQuestion.Response != WindowResult.OkYes)
                return;

            Cars!.Remove(model);
            if (model.Delete())
                await Scope.Data.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            Scope.ExceptionHandler.HandleError("Error initializing car list editor", ex);
        }
    }

}
