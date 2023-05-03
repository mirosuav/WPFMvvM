namespace WPFMvvM.Common
{
    public interface IAppScope : IDisposable
    {
        /// <summary>
        /// Create new application scope.
        /// This essentially creates new ServiceScope
        /// </summary>
        IAppScope CreateNewScope();

        /// <summary>
        /// Register given object as receipient for all implemented messages
        /// </summary>
        void RegisterMessageRecipient(object recipient);

        /// <summary>
        /// Send message request
        /// </summary>
        TMessage SendMessage<TMessage>(TMessage message) where TMessage : class;

        /// <summary>
        /// Send a message request using specific channel token
        /// </summary>
        public TMessage SendMessage<TMessage, TToken>(TMessage message, TToken token)
            where TMessage : class
            where TToken : IEquatable<TToken>;

        /// <summary>
        /// Unregister given recipient from 
        /// </summary>
        /// <param name="recipient"></param>
        void UnregisterMessageRecipient(object recipient);

    }
}