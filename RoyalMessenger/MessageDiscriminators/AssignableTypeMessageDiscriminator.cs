using System;
using System.Reflection;

namespace RoyalMessenger.MessageDiscriminators
{
    /// <summary>
    /// An  <see cref="IMessageDiscriminator"/> that uses <see cref="TypeInfo.IsAssignableFrom(TypeInfo)"/>
    /// to check if messages are compatible.
    /// This means that handlers can be registered with interfaces or parent class types, and implementing or
    /// inheriting classes will automatically be received by it.
    /// </summary>
    public class AssignableTypeMessageDiscriminator : IMessageDiscriminator
    {
        /// <summary>Checks if a certain message should be received by a handler.</summary>
        /// <param name="messageType">The type of the message that was received.</param>
        /// <param name="registeredType">The type of message the handler was registered with.</param>
        /// <returns>Whether the handler should receive the specified message.</returns>
        public bool IsCompatible(Type messageType, Type registeredType)
        {
            if (messageType is null) throw new ArgumentNullException(nameof(messageType));
            if (registeredType is null) throw new ArgumentNullException(nameof(registeredType));

            return registeredType.GetTypeInfo().IsAssignableFrom(messageType.GetTypeInfo());
        }
    }
}
