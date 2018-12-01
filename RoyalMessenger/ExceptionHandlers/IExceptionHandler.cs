using System;
using System.Threading.Tasks;

namespace RoyalMessenger.ExceptionHandlers
{
    /// <summary>Represents a service capable of handling exceptions thrown by message handlers.</summary>
    public interface IExceptionHandler
    {
        /// <summary>Handles an exception in a message handler.</summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="message">The message that was sent to the handler.</param>
        /// <param name="handler">The handler that threw the exception.</param>
        Task HandleAsync(Exception exception, object message, MessageHandler handler);
    }
}
