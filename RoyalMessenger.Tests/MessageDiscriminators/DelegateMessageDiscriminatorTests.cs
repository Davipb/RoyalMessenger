using RoyalMessenger.MessageDiscriminators;
using System;
using Xunit;

namespace RoyalMessenger.Tests.MessageDiscriminators
{
    public class DelegateMessageDiscriminatorTests
    {
        [Fact]
        public void Delegates()
        {
            var called = 0;
            var discriminator = new DelegateMessageDiscriminator((_, __) => called++ > 0);
            discriminator.IsCompatible(null, null);

            Assert.Equal(1, called);
        }

        [Fact]
        public void KeepsParameters()
        {
            var expectedType1 = typeof(string);
            var expectedType2 = typeof(int);
            var expectedReturn = true;

            Type actualType1 = null;
            Type actualType2 = null;
            bool? actualReturn = null;

            var discriminator = new DelegateMessageDiscriminator((t1, t2) =>
            {
                actualType1 = t1;
                actualType2 = t2;
                return expectedReturn;
            });
            actualReturn = discriminator.IsCompatible(expectedType1, expectedType2);

            Assert.Same(expectedType1, actualType1);
            Assert.Same(expectedType2, actualType2);
            Assert.Equal(expectedReturn, actualReturn);
        }
    }
}
