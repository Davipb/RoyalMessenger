using System;
using System.Threading.Tasks;

namespace RoyalMessenger.ExceptionHandlers
{
    /// <summary>An <see cref="IExceptionHandler"/> that simply delegates its work to a Delegate.</summary>
    public class DelegateExceptionHandler : IExceptionHandler
    {
        private readonly Func<Exception, object, MessageHandler, Task> Delegate;

        /// <summary>Creates a new <see cref="DelegateExceptionHandler"/> for a specified delegate.</summary>
        /// <param name="func">The delegate that will receive all data from this handler.</param>
        public DelegateExceptionHandler(Func<Exception, object, MessageHandler, Task> func) => Delegate = func ?? throw new ArgumentNullException(nameof(func));

        /// <summary>Handles an exception in a message handler.</summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="message">The message that was sent to the handler.</param>
        /// <param name="handler">The handler that threw the exception.</param>
        public Task HandleAsync(Exception exception, object message, MessageHandler handler) => Delegate(exception, message, handler);

    }
}
