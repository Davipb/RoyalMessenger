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
            Log.Info($"Swallowed {exception}");
            return Task.CompletedTask;
        }
    }
}
