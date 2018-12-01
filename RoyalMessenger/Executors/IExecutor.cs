using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoyalMessenger.Executors
{
    /// <summary>A service capable of executing many message handlers.</summary>
    public interface IExecutor
    {
        /// <summary>Executes a set of handlers with a specified message.</summary>
        /// <param name="message">The message that was received.</param>
        /// <param name="handlers">The handlers that should be executed with the received message.</param>
        Task ExecuteAsync(object message, IReadOnlyCollection<MessageHandler> handlers);
    }
}
