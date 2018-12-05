using System;

namespace RoyalMessenger.MessageDiscriminators
{
    /// <summary>
    /// A <see cref="IMessageDiscriminator"/> that only marks as compatible messages with the exact same type,
    /// regardless of their inheritance relation.
    /// For example, for a type <code>ParentMessage</code> and a type <code>ChildMessage : ParentMessage</code>,
    /// a handler registered for <code>ParentMessage</code> will not receive messages of type <code>ChildMessage</code>,
    /// and vice-versa.
    /// </summary>
    public class ExactTypeMessageDiscriminator : IMessageDiscriminator
    {
        /// <summary>Checks if a certain message should be received by a handler.</summary>
        /// <param name="messageType">The type of the message that was received.</param>
        /// <param name="registeredType">The type of message the handler was registered with.</param>
        /// <returns>Whether the handler should receive the specified message.</returns>
        public bool IsCompatible(Type messageType, Type registeredType)
        {
            if (messageType is null) throw new ArgumentNullException(nameof(messageType));
            if (registeredType is null) throw new ArgumentNullException(nameof(registeredType));

            return messageType == registeredType;
        }
    }
}
