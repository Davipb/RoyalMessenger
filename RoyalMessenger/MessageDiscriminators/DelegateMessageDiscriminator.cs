using System;

namespace RoyalMessenger.MessageDiscriminators
{
    /// <summary>A <see cref="IMessageDiscriminator"/> that simply delegates its functions to a Delegate.</summary>
    public class DelegateMessageDiscriminator : IMessageDiscriminator
    {
        private readonly Func<Type, Type, bool> Delegate;

        /// <summary>Creates a new <see cref="DelegateMessageDiscriminator"/>.</summary>
        /// <param name="func">The delegate that will receive all calls to this discriminator.</param>
        public DelegateMessageDiscriminator(Func<Type, Type, bool> func) => Delegate = func;

        /// <summary>Checks if a certain message should be received by a handler.</summary>
        /// <param name="messageType">The type of the message that was received.</param>
        /// <param name="registeredType">The type of message the handler was registered with.</param>
        /// <returns>Whether the handler should receive the specified message.</returns>
        public bool IsCompatible(Type messageType, Type registeredType) => Delegate(messageType, registeredType);
    }
}
