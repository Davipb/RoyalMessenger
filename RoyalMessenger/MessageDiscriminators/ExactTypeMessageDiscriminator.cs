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
        public bool IsCompatible(Type messageType, Type registeredType) => messageType == registeredType;
    }
}
