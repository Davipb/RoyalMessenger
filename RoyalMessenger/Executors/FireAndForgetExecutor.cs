using RoyalMessenger.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoyalMessenger.Executors
{
    /// <summary>
    /// An abstract base for <see cref="IExecutor"/>s that simplifies the implementation of "fire-and-forget" mechanics.
    /// </summary>
    public abstract class FireAndForgetExecutor : IExecutor
    {
        private static readonly Logger Log = new Logger(typeof(FireAndForgetExecutor));

        /// <summary>
        /// Weather this executor is a fire-and-forget runner or not.
        /// Fire-and-Forget runners start executing their handlers and immediately return control to the calling code
        /// while the handlers run in a separate Task.
        /// Runners that are not fire-and-forget will wait for all handlers to finish executing before returning control
        /// to the calling code.
        /// </summary>
        public bool IsFireAndForget { get; }

        /// <summary>Creates a new <see cref="FireAndForgetExecutor"/>.</summary>
        /// <param name="isFireAndForget">Weather this executor is fire-and-forget or not.</param>
        protected FireAndForgetExecutor(bool isFireAndForget) => IsFireAndForget = isFireAndForget;

        public Task ExecuteAsync(object message, IReadOnlyCollection<MessageHandler> handlers)
        {
            Log.Trace($"Received {handlers.Count} handlers for a message {message}");

            if (IsFireAndForget)
            {
                Log.Debug($"Fire and Forget mode is set, running {handlers.Count} handlers in a background task");
                Task.Run(() => DoExecutionAsync(message, handlers));
                return Task.CompletedTask;
            }

            return DoExecutionAsync(message, handlers);
        }

        /// <summary>
        /// Does the actual execution of the handlers.
        /// If the executor is fire-and-forget, this method will be called in a separate task from the calling code.
        /// Otherwise, it will be called as normal.
        /// </summary>
        /// <param name="message">The message that was sent.</param>
        /// <param name="handlers">The handlers that must receive the message.</param>
        protected abstract Task DoExecutionAsync(object message, IReadOnlyCollection<MessageHandler> handlers);
    }
}
