using RoyalMessenger.Executors;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests.Executors
{
    public class DelegateExecutorTests
    {
        private class Message { }

        [Fact]
        public async Task Delegates()
        {
            var called = 0;
            var executor = new DelegateExecutor(async (_, __) => called++);

            await executor.ExecuteAsync(null, null);

            Assert.Equal(1, called);
        }

        [Fact]
        public async Task KeepsParameters()
        {
            var expectedMessage = new Message();
            var expectedHandlers = new MessageHandler[]
            {
                msg => Task.CompletedTask
            };

            object actualMessage = null;
            IReadOnlyCollection<MessageHandler> actualHandlers = null;

            var executor = new DelegateExecutor(async (msg, h) =>
            {
                actualMessage = msg;
                actualHandlers = h;
            });
            await executor.ExecuteAsync(expectedMessage, expectedHandlers);

            Assert.Same(expectedMessage, actualMessage);
            Assert.Same(expectedHandlers, actualHandlers);
        }
    }
}
