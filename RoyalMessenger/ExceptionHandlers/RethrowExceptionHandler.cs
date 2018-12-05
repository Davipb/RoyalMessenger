using RoyalMessenger.Logging;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace RoyalMessenger.ExceptionHandlers
{
    /// <summary>
    /// An <see cref="IExceptionHandler"/> that will re-throw its exceptions, maintaining the stack trace.
    /// </summary>
    public sealed class RethrowExceptionHandler : IExceptionHandler
    {
        private static readonly Logger Log = new Logger(typeof(RethrowExceptionHandler));

        public Task HandleAsync(Exception exception, object message, MessageHandler handler)
        {
            if (exception is null) throw new ArgumentNullException(nameof(exception));
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (handler is null) throw new ArgumentNullException(nameof(handler));

            Log.Debug($"Re-throwing {exception}");

            // Just using "throw exception" would cause the exception to lose all stack trace information
            // ExceptionDispatchInfo.Throw() will re-throw the exception while keeping the old stack trace
            ExceptionDispatchInfo.Capture(exception).Throw();

            // ExceptionDispatchInfo.Throw() always throws an exception, so our code will never reach this point.
            // The compiler doesn't know that, so we need to trick it into realizing that this method
            // will never return properly.
            throw exception;
        }
    }
}
