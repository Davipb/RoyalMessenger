using System;
using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>
    /// Extensions that facilitate the usage of <see cref="IMessageBroker"/>s.
    /// </summary>
    public static class MessageBrokerExtensions
    {
        /// <summary>Registers a handler to receive messages of a certain type.</summary>
        /// <typeparam name="T">The type of messages the handler will receive.</typeparam>
        /// <param name="broker">The <see cref="IMessageBroker"/> where the handler will be registered.</param>
        /// <param name="handler">The handler to register. It will always be called with a non-null message.</param>
        /// <returns>
        /// An object that represents this handler's registration. If this object is disposed or garbage collected,
        /// the handler will be unregistered and will no longer receive messages.
        /// </returns>
        public static Task<IAsyncDisposable> RegisterAsync<T>(this IMessageBroker broker, MessageHandler<T> handler)
        {
            return broker.RegisterAsync(typeof(T), obj =>
            {
                if (!(obj is T message))
                    throw new ArgumentException($"Expected a {typeof(T).Name}, got a {obj?.GetType()?.Name} instead");

                return handler(message);
            });
        }
    }
}
