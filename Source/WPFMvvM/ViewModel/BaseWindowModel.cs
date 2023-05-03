using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFMvvM.Exceptions;
using WPFMvvM.Messages;

namespace WPFMvvM.ViewModel;

/// <summary>
///  WindowModel             Window
///  ActivateCommand  <-     Activated
///  CloseCommand     <-     Closed
///  Show             ->     Show
///  ShowModal        ->     ShowDialog
///  Close            ->     Close
/// </summary>

public abstract partial class BaseWindowModel : BaseViewModel
{
    internal readonly Guid WindowRequestToken = Guid.NewGuid();

    [ObservableProperty]
    string? title;

    protected BaseWindowModel(IAppScope scope) : base(scope)
    {
    }

    /// <summary>
    /// Handle operations on window activated
    /// </summary>
    [RelayCommand]
    Task Activate(CancellationToken token) => OnActivating(token);

    /// <summary>
    /// Request window close
    /// </summary>
    [RelayCommand]
    public async Task Close(CancellationToken token)
    {
        if (await CanClose(token))
        {
            await Scope.SendMessage(new WindowCloseRequest(), WindowRequestToken);
            await AfterClose(token);
        }
    }

    protected virtual Task OnActivating(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    protected virtual ValueTask<bool> CanClose(CancellationToken token)
    {
        return CheckUnsavedChanges();
    }

    protected virtual ValueTask AfterClose(CancellationToken token)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask<bool> ShowView(CancellationToken token)
    {
        if (Disposed) 
            throw new UIException("Invalid or disposed screen!");
        return await Scope.SendMessage(new WindowShowRequest(), WindowRequestToken).Response;
    }

    public async ValueTask<bool> ShowViewDialog(CancellationToken token)
    {
        if (Disposed) 
            throw new UIException("Invalid or disposed screen!");
        return await Scope.SendMessage(new WindowShowDialogRequest(), WindowRequestToken).Response;
    }

}
