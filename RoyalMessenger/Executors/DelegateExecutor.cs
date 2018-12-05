using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoyalMessenger.Executors
{
    /// <summary>An <see cref="IExecutor"/> that simply delegates its functions to a Delegate.</summary>
    public class DelegateExecutor : IExecutor
    {
        private readonly Func<object, IReadOnlyCollection<MessageHandler>, Task> Delegate;

        /// <summary>Creates a new <see cref="DelegateExecutor"/>.</summary>
        /// <param name="func">The delegate that will receive every call to this executor.</param>
        public DelegateExecutor(Func<object, IReadOnlyCollection<MessageHandler>, Task> func) => Delegate = func;

        /// <summary>Executes a set of handlers with a specified message.</summary>
        /// <param name="message">The message that was received.</param>
        /// <param name="handlers">The handlers that should be executed with the received message.</param>
        public Task ExecuteAsync(object message, IReadOnlyCollection<MessageHandler> handlers) => Delegate(message, handlers);
    }
}
