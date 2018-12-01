using NullGuard;
using RoyalMessenger.ExceptionHandlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RoyalMessenger.Executors
{
    /// <summary>An <see cref="IExecutor"/> that executes message handlers in parallel with each other.</summary>
    public class ParallelExecutor : FireAndForgetExecutor
    {
        /// <summary>The handler used to handle exceptions thrown by message handlers.</summary>
        public IExceptionHandler ExceptionHandler { get; }

        /// <summary>Creates a new <see cref="SequentialExecutor" />.</summary>
        /// <param name="isFireAndForget">Weather this executor will be fire-and-forget or not. Defaults to <see langword="false" />.</param>
        /// <param name="exceptionHandler">The exception handler used by this executor. If <see langword="null"/>, it will be set to <see cref="RethrowExceptionHandler"/>.</param>
        public ParallelExecutor(
            bool isFireAndForget = false,
            [AllowNull] IExceptionHandler exceptionHandler = null
        ) : base(isFireAndForget)
        {
            ExceptionHandler = exceptionHandler ?? new RethrowExceptionHandler();
        }

        protected override Task DoExecutionAsync(object message, IReadOnlyCollection<MessageHandler> handlers)
        {
            var tasks = new List<Task>();
            foreach (var handler in handlers)
            {
                // Use Task.Run instead of calling the handler directly to ensure that "fast-track async" methods
                // that actually run synchronously won't block the parallelization
                tasks.Add(Task.Run(async () =>
                {
                    try { await handler(message).ConfigureAwait(false); }
                    catch (Exception e)
                    {
                        await ExceptionHandler.HandleAsync(e, message, handler).ConfigureAwait(false);
                    }
                }));
            }

            return Task.WhenAll(tasks);
        }
    }
}
