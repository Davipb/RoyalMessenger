using NullGuard;
using RoyalMessenger.Executors;
using RoyalMessenger.MessageDiscriminators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>An asynchronous message broker that stores its data in-memory.</summary>
    public sealed class StandardMessageBroker : IMessageBroker
    {
        /// <summary>Represents a handler registration for this broker.</summary>
        private sealed class Registration : IAsyncDisposable
        {
            /// <summary>The broker that created this registration.</summary>
            private readonly StandardMessageBroker broker;

            /// <summary>The type of message this registration represents.</summary>
            public Type MessageType { get; }
            /// <summary>The message handler that this registration represents.</summary>
            public MessageHandler MessageHandler { get; }

            public Registration(StandardMessageBroker broker, Type messageType, MessageHandler messageHandler)
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
        private readonly IMessageDiscriminator discriminator;

        /// <summary>Creates a new <see cref="StandardMessageBroker"/> with the specified <see cref="IExecutor"/>.</summary>
        /// <param name="executor">
        /// The executor that will be used to execute message handlers in this broker.
        /// If left <see langword="null"/>, it defaults to <see cref="SequentialExecutor"/>.
        /// </param>
        /// <param name="discriminator">
        /// The discriminator used to decide which messages should be delivered to which handlers.
        /// If left <see langword="null"/>, it defaults to <see cref="AssignableTypeMessageDiscriminator"/>.
        /// </param>
        public StandardMessageBroker(
            [AllowNull] IExecutor executor = null,
            [AllowNull] IMessageDiscriminator discriminator = null)
        {
            this.executor = executor ?? new SequentialExecutor();
            this.discriminator = discriminator ?? new AssignableTypeMessageDiscriminator();
        }

        public async Task<IAsyncDisposable> RegisterAsync(Type messageType, MessageHandler messageHandler)
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
            var registrations = await items
                .GetMatchingAsync(k => discriminator.IsCompatible(message.GetType(), k))
                .ConfigureAwait(false);

            // Copy the handlers to make sure additional registrations and unregistrations can occur
            // inside the handlers' execution without affecting the executor
            var handlers = registrations.Select(r => r.MessageHandler).ToList().AsReadOnly();

            if (handlers.Count > 0)
                await executor.ExecuteAsync(message, handlers).ConfigureAwait(false);
        }
    }
}
