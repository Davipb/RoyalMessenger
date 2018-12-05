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

        public Task HandleAsync(Exception exception, object message, MessageHandler handler) => Delegate(exception, message, handler);

    }
}
