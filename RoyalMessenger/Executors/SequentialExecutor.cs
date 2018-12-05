using RoyalMessenger.Contracts;
using RoyalMessenger.ExceptionHandlers;
using RoyalMessenger.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoyalMessenger.Executors
{
    /// <summary>An <see cref="IExecutor"/> that executes all message handlers one at a time, sequentially.</summary>
    public sealed class SequentialExecutor : FireAndForgetExecutor
    {
        private static readonly Logger Log = new Logger(typeof(SequentialExecutor));

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
            [Nullable] IExceptionHandler exceptionHandler = null
        ) : base(isFireAndForget)
        {
            ExceptionPolicy = exceptionPolicy;
            ExceptionHandler = exceptionHandler ?? new RethrowExceptionHandler();
        }

        protected override async Task DoExecutionAsync(object message, IReadOnlyCollection<MessageHandler> handlers)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));
            if (handlers is null) throw new ArgumentNullException(nameof(handlers));

            foreach (var handler in handlers)
            {
                try { await handler(message).ConfigureAwait(false); }
                catch (Exception e)
                {
                    Log.Error(e, $"A handler for {message} threw an {e.GetType().FullName}, delegating to {ExceptionHandler}");
                    await ExceptionHandler.HandleAsync(e, message, handler).ConfigureAwait(false);

                    if (ExceptionPolicy == SequentialExceptionPolicy.Stop)
                    {
                        Log.Debug($"Exception policy is set to Stop, aborting execution of handlers");
                        break;
                    }
                    else
                    {
                        Log.Trace($"Exception policy is set to Continue, finishing execution of handlers after an exception");
                    }
                }
            }
        }
    }
}
