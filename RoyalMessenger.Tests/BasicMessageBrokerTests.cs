using Moq;
using RoyalMessenger.Executors;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests
{
    public class BasicMessageBrokerTests
    {
        /// <summary>Represents a request received by an <see cref="IExecutor"/>.</summary>
        private class ExecutionRequest
        {
            /// <summary>The message that triggered the execution.</summary>
            public object Message { get; }
            /// <summary>The handlers that were to be executed.</summary>
            public IReadOnlyCollection<MessageHandler> Handlers { get; }

            public ExecutionRequest(object message, IReadOnlyCollection<MessageHandler> handlers)
            {
                Message = message;
                Handlers = handlers;
            }
        }

        /// <summary>An <see cref="IExecutor"/> that simply logs all requests made to it.</summary>
        private class Executor : IExecutor
        {
            /// <summary>The requests made to this executor.</summary>
            public List<ExecutionRequest> Requests { get; } = new List<ExecutionRequest>();
            /// <summary>The latest request made to this executor.</summary>
            public ExecutionRequest Latest => Requests.Last();

            public Task ExecuteAsync(object message, IReadOnlyCollection<MessageHandler> handlers)
            {
                Requests.Add(new ExecutionRequest(message, handlers));
                return Task.CompletedTask;
            }
        }

        private class Message1 { }
        private class Message2 { }

        /// <summary>Creates a message handler that does nothing.</summary>
        private static MessageHandler CreateDummyHandler() => _ => Task.CompletedTask;
        /// <summary>Creates multiple dummy handlers.</summary>
        /// <param name="count">The number of handlers to generate.</param>
        private static List<MessageHandler> CreateDummyHandlers(int count)
        {
            var result = new List<MessageHandler>();
            for (var i = 0; i < count; i++)
                result.Add(CreateDummyHandler());

            return result;
        }

        private readonly MockRepository Mocks = TestHelper.CreateRepository();

        [Fact]
        public async Task Send_SingleHandler_CorrectMessage_ExecutesHandler()
        {
            var handler = CreateDummyHandler();
            var executor = new Executor();
            var broker = new BasicMessageBroker(executor);

            var registration = await broker.RegisterAsync(typeof(Message1), handler);
            await broker.SendAsync(new Message1());

            Assert.Single(executor.Requests);
            Assert.Single(executor.Latest.Handlers, handler);
        }

        [Fact]
        public async Task Send_SingleHandler_WrongMessage_DoesntExecuteHandler()
        {
            var handler = CreateDummyHandler();
            var executor = new Executor();
            var broker = new BasicMessageBroker(executor);

            var registration = await broker.RegisterAsync(typeof(Message1), handler);
            await broker.SendAsync(new Message2());

            Assert.Empty(executor.Requests);
        }

        [Fact]
        public async Task Send_MultipleHandlers_CorrectMessage_ExecutesHandlers()
        {
            var handlers = CreateDummyHandlers(3);

            var executor = new Executor();
            var broker = new BasicMessageBroker(executor);
            var registrations = new List<IAsyncDisposable>();
            foreach (var handler in handlers)
                registrations.Add(await broker.RegisterAsync(typeof(Message1), handler));

            await broker.SendAsync(new Message1());

            Assert.Single(executor.Requests);
            TestHelper.AssertEquivalent(handlers, executor.Latest.Handlers);
        }

        [Fact]
        public async Task Send_MultipleHandlers_FiltersCorrectHandlers()
        {
            var expected = CreateDummyHandlers(3);
            var ignored = CreateDummyHandlers(3);

            var executor = new Executor();
            var broker = new BasicMessageBroker(executor);
            var registrations = new List<IAsyncDisposable>();

            foreach (var handler in expected)
                registrations.Add(await broker.RegisterAsync(typeof(Message1), handler));

            foreach (var handler in ignored)
                registrations.Add(await broker.RegisterAsync(typeof(Message2), handler));

            await broker.SendAsync(new Message1());

            Assert.Single(executor.Requests);
            TestHelper.AssertEquivalent(expected, executor.Latest.Handlers);
        }

        [Fact]
        public async Task RegistrationDisposed_UnregistersHandler()
        {
            var handler = CreateDummyHandler();
            var executor = new Executor();
            var broker = new BasicMessageBroker(executor);

            var registration = await broker.RegisterAsync(typeof(Message1), handler);
            await registration.DisposeAsync();
            await broker.SendAsync(new Message1());

            Assert.Empty(executor.Requests);
        }

        [Fact]
        public async Task RegistrationGarbageCollected_UnregistersHandler()
        {
            var handler = CreateDummyHandler();
            var executor = new Executor();
            var broker = new BasicMessageBroker(executor);

            await broker.RegisterAsync(typeof(Message1), handler);
            await TestHelper.ForceGarbageCollectionAsync();
            await broker.SendAsync(new Message1());

            Assert.Empty(executor.Requests);
        }

    }
}
