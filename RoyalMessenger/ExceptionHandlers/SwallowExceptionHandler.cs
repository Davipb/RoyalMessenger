using System;
using System.Threading.Tasks;

namespace RoyalMessenger.ExceptionHandlers
{
    /// <summary>An <see cref="IExceptionHandler"/> that swallows exceptions without doing anything.</summary>
    public sealed class SwallowExceptionHandler : IExceptionHandler
    {
        public Task HandleAsync(Exception exception, object message, MessageHandler handler) => Task.CompletedTask;
    }
}
