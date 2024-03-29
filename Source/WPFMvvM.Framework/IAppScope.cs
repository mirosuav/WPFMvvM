﻿using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Services;

namespace WPFMvvM.Framework;
public interface IAppScope
{
    IServiceProvider Services { get; }
    IExceptionHandler ExceptionHandler { get; }
    IDialogService DialogService { get; }
    IUIServices UI { get; }
    IMessenger Messenger { get; }

    /// <summary>
    /// Create new application scope.
    /// This creates new AppScope with new ServiceScope
    /// </summary>
    ApplicationHostScope CreateNewScope();
    TViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel;
    BaseViewModel? ResolveViewModel(Type viewModelType);
    ApplicationHostScope ResolveViewModelWithNewScope<TViewModel>(out TViewModel viewModel) where TViewModel : BaseViewModel;

}