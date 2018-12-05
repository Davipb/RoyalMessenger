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
        public bool IsCompatible(Type messageType, Type registeredType)
        {
            if (messageType is null) throw new ArgumentNullException(nameof(messageType));
            if (registeredType is null) throw new ArgumentNullException(nameof(registeredType));

            return registeredType.GetTypeInfo().IsAssignableFrom(messageType.GetTypeInfo());
        }
    }
}
