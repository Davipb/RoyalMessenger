using RoyalMessenger.Contracts;
using RoyalMessenger.Executors;
using RoyalMessenger.Logging;
using RoyalMessenger.MessageDiscriminators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>An asynchronous message broker that stores its data in-memory.</summary>
    public sealed class StandardMessageBroker : IMessageBroker
    {
        private static readonly Logger Log = new Logger(typeof(StandardMessageBroker));

        /// <summary>Represents a handler registration for this broker.</summary>
        private sealed class Registration : IAsyncDisposable
        {
            private static readonly Logger Log = new Logger(typeof(Registration));

            /// <summary>The broker that created this registration.</summary>
            private readonly StandardMessageBroker broker;

            /// <summary>The type of message this registration represents.</summary>
            public Type MessageType { get; }
            /// <summary>The message handler that this registration represents.</summary>
            public MessageHandler MessageHandler { get; }

            public Registration(StandardMessageBroker broker, Type messageType, MessageHandler messageHandler)
            {
                this.broker = broker ?? throw new ArgumentNullException(nameof(broker));
                MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
                MessageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            }

            /// <summary>Unregisters this registration from the broker that created it.</summary>
            public Task DisposeAsync()
            {
                Log.Trace($"Handler {MessageHandler} was manually unregistered from {MessageType} at {broker}");
                return broker.UnregisterAsync(this);
            }

            public override string ToString() => $"Handler {MessageHandler} for {MessageType} at {broker}";
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
            [Nullable] IExecutor executor = null,
            [Nullable] IMessageDiscriminator discriminator = null)
        {
            this.executor = executor ?? new SequentialExecutor();
            this.discriminator = discriminator ?? new AssignableTypeMessageDiscriminator();
        }

        public async Task<IAsyncDisposable> RegisterAsync(Type messageType, MessageHandler messageHandler)
        {
            if (messageType is null) throw new ArgumentNullException(nameof(messageType));
            if (messageHandler is null) throw new ArgumentNullException(nameof(messageHandler));

            Log.Trace($"Registering a new handler for {messageType}");
            var registration = new Registration(this, messageType, messageHandler);
            await items.AddAsync(messageType, registration).ConfigureAwait(false);
            return registration;
        }

        /// <summary>Unregisters a given registration, ensuring that its handler won't receive messages anymore.</summary>
        /// <param name="registration">The registration to unregister.</param>
        private Task UnregisterAsync(Registration registration)
        {
            if (registration is null) throw new ArgumentNullException(nameof(registration));

            Log.Trace($"Manually unregistering {registration}");
            return items.RemoveAsync(registration.MessageType, registration);
        }

        public async Task SendAsync(object message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            var registrations = await items
                .GetMatchingAsync(k => discriminator.IsCompatible(message.GetType(), k))
                .ConfigureAwait(false);

            // Copy the handlers to make sure additional registrations and unregistrations can occur
            // inside the handlers' execution without affecting the executor
            var handlers = registrations.Select(r => r.MessageHandler).ToList().AsReadOnly();

            if (handlers.Count > 0)
            {
                Log.Info($"Broadcasting {message} to {handlers.Count} handlers using {executor}");
                await executor.ExecuteAsync(message, handlers).ConfigureAwait(false);
            }
            else
            {
                Log.Debug($"{message} has no handlers, ignoring");
            }
        }
    }
}
