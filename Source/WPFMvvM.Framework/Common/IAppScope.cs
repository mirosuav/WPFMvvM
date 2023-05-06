﻿namespace WPFMvvM.Framework.Common;

public interface IAppScope : IDisposable
{
    IDialogService WindowService { get; }

    IMessenger Messenger { get; }

    CancellationToken CancellToken { get; }

    /// <summary>
    /// Create new application scope.
    /// This creates new AppScope with new ServiceScope
    /// </summary>
    IAppScope CreateNewScope();

    TViewModel ResolveViewModel<TViewModel>() where TViewModel : BaseViewModel;

}