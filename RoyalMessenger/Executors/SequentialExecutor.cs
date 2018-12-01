using NullGuard;
using RoyalMessenger.ExceptionHandlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoyalMessenger.Executors
{
    /// <summary>An <see cref="IExecutor"/> that executes all message handlers one at a time, sequentially.</summary>
    public sealed class SequentialExecutor : FireAndForgetExecutor
    {
        /// <summary>The policy that thise executor will follow when handling exceptions.</summary>
        public SequentialExceptionPolicy ExceptionPolicy { get; }
        /// <summary>The handler used to handle exceptions thrown by message handlers.</summary>
        public IExceptionHandler ExceptionHandler { get; }

        /// <summary>Creates a new <see cref="SequentialExecutor" />.</summary>
        /// <param name="isFireAndForget">Weather this executor will be fire-and-forget or not. Defaults to <see langword="false" />.</param>
        /// <param name="exceptionPolicy">The exception policy that will be used by this executor. Defaults to <see cref="SequentialExceptionPolicy.Continue" />.</param>
        /// <param name="exceptionHandler">The exception handler used by this executor. If <see langword="null"/>, it will be set to <see cref="RethrowExceptionHandler"/>.</param>
        public SequentialExecutor(
            bool isFireAndForget = false,
            SequentialExceptionPolicy exceptionPolicy = SequentialExceptionPolicy.Continue,
            [AllowNull] IExceptionHandler exceptionHandler = null
        ) : base(isFireAndForget)
        {
            ExceptionPolicy = exceptionPolicy;
            ExceptionHandler = exceptionHandler ?? new RethrowExceptionHandler();
        }

        protected override async Task DoExecutionAsync(object message, IReadOnlyCollection<MessageHandler> handlers)
        {
            foreach (var handler in handlers)
            {
                try { await handler(message).ConfigureAwait(false); }
                catch (Exception messageException)
                {
                    await ExceptionHandler.HandleAsync(messageException, message, handler).ConfigureAwait(false);

                    if (ExceptionPolicy == SequentialExceptionPolicy.Stop)
                        break;
                }
            }
        }
    }
}
