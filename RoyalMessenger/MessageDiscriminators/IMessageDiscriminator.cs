using System;

namespace RoyalMessenger.MessageDiscriminators
{
    /// <summary>A class capable of determining if a handler should receive a message.</summary>
    public interface IMessageDiscriminator
    {
        /// <summary>Checks if a certain message should be received by a handler.</summary>
        /// <param name="messageType">The type of the message that was received.</param>
        /// <param name="registeredType">The type of message the handler was registered with.</param>
        /// <returns>Whether the handler should receive the specified message.</returns>
        bool IsCompatible(Type messageType, Type registeredType);
    }
}
