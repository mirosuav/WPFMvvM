using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFMvvM.GlobalHandlers;
using CommunityToolkit.Mvvm.Messaging;

namespace WPFMvvM.Common;

/// <summary>
/// Application scope serves as central communication hub for all application sections
/// </summary>
internal class AppScope : IDisposable, IAppScope
{
    private readonly IServiceProvider serviceProvider;
    private readonly IMessenger messenger;
    //keep global recipient references so their're not garbage collected when using WeakReferenceMessenger
    private List<IGlobalHandler>? globalHandlers;
    private bool disposedValue;

    public AppScope(IServiceProvider scope, IMessenger messenger)
    {
        Guard.IsNotNull(scope);
        Guard.IsNotNull(messenger);
        this.serviceProvider = scope;
        this.messenger = messenger;
        RegisterGlobalHandlers();
    }

    private void RegisterGlobalHandlers()
    {
        globalHandlers = serviceProvider.GetServices<IGlobalHandler>().ToList();
        globalHandlers.ForEach(recipient => this.messenger.RegisterAll(recipient));
    }

    public IAppScope CreateNewScope()
    {
        var newScope = serviceProvider.CreateScope();
        return newScope.ServiceProvider.GetRequiredService<IAppScope>();
    }

    public void RegisterMessageRecipient(object recipient)
    {
        messenger.RegisterAll(recipient);
    }

    public TMessage SendMessage<TMessage>(TMessage message)
        where TMessage : class
    {
        return messenger.Send(message);
    }

    public TMessage SendMessage<TMessage, TToken>(TMessage message, TToken token)
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        return messenger.Send<TMessage, TToken>(message, token);
    }

    public void UnregisterMessageRecipient(object recipient)
    {
        messenger.RegisterAll(recipient);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                messenger.Reset();
            }
            disposedValue = true;
        }
    }
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
