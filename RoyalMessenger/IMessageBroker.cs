using System;
using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>A message broker.</summary>
    public interface IMessageBroker
    {
        /// <summary>
        /// Registers a handler to receive messages of a certain type.
        /// </summary>
        /// <param name="messageType">The type of the messages that the handler will receive.</param>
        /// <param name="messageHandler">The handler to register. It will always be called with a non-null message of type specified in the previous parameter.</param>
        /// <returns>
        /// An object that represents this handler's registration. If this object is disposed or garbage collected,
        /// the handler will be unregistered and will no longer receive messages.
        /// </returns>
        Task<IAsyncDisposable> RegisterAsync(Type messageType, Func<object, Task> messageHandler);

        /// <summary>Sends a message.</summary>
        /// <param name="message">The message to send.</param>
        Task SendAsync(object message);
    }
}
