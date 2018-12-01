using RoyalMessenger.Executors;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>An asynchronous message broker that stores its data in-memory.</summary>
    public sealed class BasicMessageBroker : IMessageBroker
    {
        /// <summary>Represents a handler registration for this broker.</summary>
        private sealed class Registration : IAsyncDisposable
        {
            /// <summary>The broker that created this registration.</summary>
            private readonly BasicMessageBroker broker;

            /// <summary>The type of message this registration represents.</summary>
            public Type MessageType { get; }
            /// <summary>The message handler that this registration represents.</summary>
            public Func<object, Task> MessageHandler { get; }

            public Registration(BasicMessageBroker broker, Type messageType, Func<object, Task> messageHandler)
            {
                this.broker = broker;
                MessageType = messageType;
                MessageHandler = messageHandler;
            }

            /// <summary>Unregisters this registration from the broker that created it.</summary>
            public Task DisposeAsync() => broker.UnregisterAsync(this);
        }

        private readonly WeakMultiValueDictionary<Type, Registration> items = new WeakMultiValueDictionary<Type, Registration>();
        private readonly IExecutor executor;

        /// <summary>Creates a new <see cref="BasicMessageBroker"/> with the specified <see cref="IExecutor"/>.</summary>
        /// <param name="executor">The executor that will be used to execute message handlers in this broker.</param>
        public BasicMessageBroker(IExecutor executor)
        {
            this.executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public async Task<IAsyncDisposable> RegisterAsync(Type messageType, Func<object, Task> messageHandler)
        {
            var registration = new Registration(this, messageType, messageHandler);
            await items.AddAsync(messageType, registration).ConfigureAwait(false);
            return registration;
        }

        /// <summary>Unregisters a given registration, ensuring that its handler won't receive messages anymore.</summary>
        /// <param name="registration">The registration to unregister.</param>
        private Task UnregisterAsync(Registration registration) => items.RemoveAsync(registration.MessageType, registration);

        public async Task SendAsync(object message)
        {
            var registrations = await items.GetAsync(message.GetType()).ConfigureAwait(false);

            // Copy the handlers to make sure additional registrations and unregistrations can occur
            // inside the handlers' execution without affecting the executor
            var handlers = registrations.Select(r => r.MessageHandler).ToList().AsReadOnly();

            await executor.ExecuteAsync(message, handlers).ConfigureAwait(false);
        }
    }
}
