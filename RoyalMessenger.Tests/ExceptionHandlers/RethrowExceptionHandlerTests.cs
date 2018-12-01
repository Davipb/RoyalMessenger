using RoyalMessenger.ExceptionHandlers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests.ExceptionHandlers
{
    public class RethrowExceptionHandlerTests
    {
        private class MyBeautifulException : Exception { }

        private Task DummyHandler(object message) => Task.CompletedTask;

        [Fact]
        public async Task Handle_KeepsType()
        {
            var exception = new MyBeautifulException();
            var exceptionHandler = new RethrowExceptionHandler();

            await Assert.ThrowsAsync<MyBeautifulException>(() => exceptionHandler.HandleAsync(exception, new object(), DummyHandler));
        }

        [Fact]
        public async Task Handle_KeepsInstance()
        {
            var exception = new MyBeautifulException();
            var exceptionHandler = new RethrowExceptionHandler();

            var caught = false;
            try
            {
                await exceptionHandler.HandleAsync(exception, new object(), DummyHandler);
            }
            catch (MyBeautifulException e)
            {
                caught = true;
                Assert.Same(exception, e);
            }

            Assert.True(caught);
        }
    }
}
