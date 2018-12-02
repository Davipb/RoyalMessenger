using RoyalMessenger.MessageDiscriminators;
using Xunit;

namespace RoyalMessenger.Tests.MessageDiscriminators
{
    public class AssignableTypeMessageDiscriminatorTests
    {
        private interface IIndirectInterface { }
        private interface IInterface { }
        private class Parent : IIndirectInterface { }
        private class Message : Parent, IInterface { }
        private class Child : Message { }
        private class Sibling : Parent { }
        private class Unrelated { }

        private static void AssertCompatible<A, B>()
        {
            var discriminator = new AssignableTypeMessageDiscriminator();
            Assert.True(discriminator.IsCompatible(typeof(A), typeof(B)));
        }

        private static void AssertIncompatible<A, B>()
        {
            var discriminator = new AssignableTypeMessageDiscriminator();
            Assert.False(discriminator.IsCompatible(typeof(A), typeof(B)));
        }

        [Fact] public void SameType_IsCompatible() => AssertCompatible<Message, Message>();
        [Fact] public void Child_IsCompatible() => AssertCompatible<Child, Message>();

        [Fact] public void Parent_IsIncompatible() => AssertIncompatible<Parent, Message>();
        [Fact] public void Sibling_IsIncompatible() => AssertIncompatible<Sibling, Message>();
        [Fact] public void Unrelated_IsIncompatible() => AssertIncompatible<Unrelated, Message>();

        [Fact] public void Interface_IsCompatible() => AssertCompatible<Message, IInterface>();
        [Fact] public void IndirectInterface_IsCompatible() => AssertCompatible<Message, IIndirectInterface>();
    }
}
