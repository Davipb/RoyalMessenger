using RoyalMessenger.Logging;
using System;
using System.Threading.Tasks;

namespace RoyalMessenger.ExceptionHandlers
{
    /// <summary>An <see cref="IExceptionHandler"/> that swallows exceptions without doing anything.</summary>
    public sealed class SwallowExceptionHandler : IExceptionHandler
    {
        private static readonly Logger Log = new Logger(typeof(SwallowExceptionHandler));

        public Task HandleAsync(Exception exception, object message, MessageHandler handler)
        {
            if (exception is null) throw new ArgumentNullException(nameof(exception));
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            Log.Info($"Swallowed {exception}");
            return Task.CompletedTask;
        }
    }
}
