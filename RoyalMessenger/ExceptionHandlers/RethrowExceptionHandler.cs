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
        public Task HandleAsync(Exception exception, object message, MessageHandler handler)
        {
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
