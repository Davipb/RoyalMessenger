using RoyalMessenger.ExceptionHandlers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests.ExceptionHandlers
{
    public class DelegateExceptionHandlerTests
    {
        private class MyBeautifulException : Exception { }
        private class Message { }

        [Fact]
        public async Task Delegates()
        {
            var called = 0;
            var handler = new DelegateExceptionHandler(async (_, __, ___) => called++);

            await handler.HandleAsync(null, null, null);

            Assert.Equal(1, called);
        }

        [Fact]
        public async Task KeepsParameters()
        {
            var expectedException = new MyBeautifulException();
            var expectedMessage = new Message();
            MessageHandler expectedHandler = msg => Task.CompletedTask;

            Exception actualException = null;
            object actualMessage = null;
            MessageHandler actualHandler = null;

            var handler = new DelegateExceptionHandler(async (ex, msg, h) =>
            {
                actualException = ex;
                actualMessage = msg;
                actualHandler = h;
            });
            await handler.HandleAsync(expectedException, expectedMessage, expectedHandler);

            Assert.Same(expectedException, actualException);
            Assert.Same(expectedMessage, actualMessage);
            Assert.Same(expectedHandler, actualHandler);
        }
    }
}
