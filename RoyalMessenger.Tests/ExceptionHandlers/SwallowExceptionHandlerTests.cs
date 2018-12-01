using RoyalMessenger.ExceptionHandlers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests.ExceptionHandlers
{
    public class SwallowExceptionHandlerTests
    {
        private Task DummyHandler(object message) => Task.CompletedTask;

        [Fact]
        public async Task Handle_DoesntThrow()
        {
            var handler = new SwallowExceptionHandler();
            await handler.HandleAsync(new Exception(), new object(), DummyHandler);
        }
    }
}
