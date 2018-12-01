using Moq;
using RoyalMessenger.Tests.MoqIntegration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests
{
    public static class TestHelper
    {
        /// <summary>Creates a new standard <see cref="MockRepository"/>.</summary>
        public static MockRepository CreateRepository() => new MockRepository(MockBehavior.Loose)
        {
            DefaultValueProvider = new AsyncFriendlyDefaultValueProvider()
        };

        /// <summary>
        /// Forces the .NET Runtime to garbage collect all objects, invalidating
        /// <see cref="WeakReference{T}"/>s to unreachable objects.
        /// </summary>
        public static async Task ForceGarbageCollectionAsync()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            await Task.Delay(10);

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Asserts that two collections are equivalent, that is, that they contain the exact same items,
        /// regardless of order.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collections.</typeparam>
        /// <param name="expected">The expected collection.</param>
        /// <param name="actual">The actual collection.</param>
        public static void AssertEquivalent<T>(IReadOnlyCollection<T> expected, IReadOnlyCollection<T> actual)
        {
            Assert.Equal(expected.Count, actual.Count);

            foreach (var item in expected)
                Assert.Contains(item, actual);
        }
    }
}
