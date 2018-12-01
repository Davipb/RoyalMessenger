using System.Threading.Tasks;

namespace RoyalMessenger
{
    /// <summary>Represents a method capable of handling a message received by a <see cref="IMessageBroker"/>.</summary>
    /// <param name="message">The message that was received.</param>
    public delegate Task MessageHandler(object message);

    /// <summary>Represents a method capable of handling a strongly-typed message received by a <see cref="IMessageBroker"/>.</summary>
    /// <param name="message">The message that was received.</param>
    public delegate Task MessageHandler<in T>(T message);
}
