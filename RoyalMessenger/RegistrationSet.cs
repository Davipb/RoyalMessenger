using NullGuard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>
    /// Groups a series of registrations to an <see cref="IMessageBroker"/>, allowing
    /// them to be managed a single object, for convenience.
    /// This class is not thread-safe.
    /// </summary>
    public class RegistrationSet
    {
        /// <summary>Represents an entry in this set.</summary>
        private class Entry
        {
            public Type Type { get; }
            public MessageHandler Handler { get; }
            [AllowNull] public IAsyncDisposable Registration { get; set; }

            public Entry(Type type, MessageHandler handler)
            {
                Type = type;
                Handler = handler;
            }
        }

        private readonly IMessageBroker Broker;
        private readonly List<Entry> Entries = new List<Entry>();

        /// <summary>Creates a new Registration set.</summary>
        /// <param name="broker">The <see cref="IMessageBroker"/> that this set wraps.</param>
        public RegistrationSet(IMessageBroker broker) => Broker = broker;


        /// <summary>Adds a new handler to this set. The handler won't be registered until <see cref="RegisterAsync"/> is called.</summary>
        /// <typeparam name="T">The type of message this handler receives.</typeparam>
        /// <param name="handler">The handler to be added.</param>
        /// <returns>This instance, for chaining.</returns>
        public RegistrationSet Add<T>(MessageHandler<T> handler)
            => Add(typeof(T), obj =>
                {
                    if (obj is T t)
                        return handler(t);

                    throw new ArgumentException($"Expected a {typeof(T).Name}, got a {obj.GetType().Name} instead");
                });


        /// <summary>Adds a new handler to this set. The handler won't be registered until <see cref="RegisterAsync"/> is called.</summary>
        /// <param name="type">The type of message this handler receives.</param>
        /// <param name="handler">The handler to be added.</param>
        /// <returns>This instance, for chaining.</returns>
        public RegistrationSet Add(Type type, MessageHandler handler)
        {
            Entries.Add(new Entry(type, handler));
            return this;
        }

        /// <summary>
        /// Registers all handlers added to this set.
        /// Handlers that have already been registered will be ignored.
        /// </summary>
        public async Task RegisterAsync()
        {
            foreach (var entry in Entries.Where(e => e.Registration is null))
                entry.Registration = await Broker.RegisterAsync(entry.Type, entry.Handler).ConfigureAwait(false);
        }

        /// <summary>
        /// Unregisters all registered handlers in this set.
        /// Handlers that haven't been registered will be ignored.
        /// </summary>
        public async Task UnregisterAsync()
        {
            foreach (var entry in Entries.Where(e => e.Registration != null))
            {
                await entry.Registration.DisposeAsync().ConfigureAwait(false);
                entry.Registration = null;
            }
        }
    }
}
