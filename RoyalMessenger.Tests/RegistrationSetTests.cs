using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests
{
    public class RegistrationSetTests
    {
        private readonly MockRepository Mocks = TestHelper.CreateRepository();

        private class Registration : IAsyncDisposable
        {
            public bool Disposed { get; private set; } = false;
            public async Task DisposeAsync() => Disposed = true;
        }

        private class Handler
        {
            public MessageHandler Delegate { get; }

            public Handler() => Delegate = msg => Task.CompletedTask;
        }

        private class Message1 { }
        private class Message2 { }
        private class Message3 { }

        private MessageHandler CreateHandler() => o => Task.CompletedTask;

        [Fact]
        public async Task Register_RegistersAll()
        {
            var handler1 = CreateHandler();
            var handler2 = CreateHandler();
            var handler3 = CreateHandler();

            var broker = Mocks.Create<IMessageBroker>();
            broker
                .Setup(b => b.RegisterAsync(It.IsAny<Type>(), It.IsAny<MessageHandler>()))
                .ReturnsAsync(() => new Registration());

            var set = new RegistrationSet(broker.Object)
                .Add(typeof(Message1), handler1)
                .Add(typeof(Message2), handler2)
                .Add(typeof(Message3), handler3);

            await set.RegisterAsync();

            broker.Verify(b => b.RegisterAsync(typeof(Message1), handler1), Times.Once());
            broker.Verify(b => b.RegisterAsync(typeof(Message2), handler2), Times.Once());
            broker.Verify(b => b.RegisterAsync(typeof(Message3), handler3), Times.Once());
            broker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Register_MultipleCalls_DoesntDuplicate()
        {
            var handler1 = CreateHandler();
            var handler2 = CreateHandler();
            var handler3 = CreateHandler();

            var broker = Mocks.Create<IMessageBroker>();
            broker
                .Setup(b => b.RegisterAsync(It.IsAny<Type>(), It.IsAny<MessageHandler>()))
                .ReturnsAsync(() => new Registration());

            var set = new RegistrationSet(broker.Object)
                .Add(typeof(Message1), handler1)
                .Add(typeof(Message2), handler2)
                .Add(typeof(Message3), handler3);

            await set.RegisterAsync();
            await set.RegisterAsync();
            await set.RegisterAsync();

            broker.Verify(b => b.RegisterAsync(typeof(Message1), handler1), Times.Once());
            broker.Verify(b => b.RegisterAsync(typeof(Message2), handler2), Times.Once());
            broker.Verify(b => b.RegisterAsync(typeof(Message3), handler3), Times.Once());
            broker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Add_DoesntRegister()
        {
            var handler1 = CreateHandler();
            var handler2 = CreateHandler();
            var handler3 = CreateHandler();

            var broker = Mocks.Create<IMessageBroker>();
            broker
                .Setup(b => b.RegisterAsync(It.IsAny<Type>(), It.IsAny<MessageHandler>()))
                .ReturnsAsync(() => new Registration());

            var set = new RegistrationSet(broker.Object)
                .Add(typeof(Message1), handler1);

            await set.RegisterAsync();

            set
                .Add(typeof(Message2), handler2)
                .Add(typeof(Message3), handler3);

            broker.Verify(b => b.RegisterAsync(typeof(Message1), handler1), Times.Once());
            broker.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Unregister_UnregistersAll()
        {
            var handler1 = CreateHandler();
            var handler2 = CreateHandler();
            var handler3 = CreateHandler();

            var registration1 = new Registration();
            var registration2 = new Registration();
            var registration3 = new Registration();

            var broker = Mocks.Create<IMessageBroker>();
            broker.Setup(b => b.RegisterAsync(typeof(Message1), handler1)).ReturnsAsync(registration1);
            broker.Setup(b => b.RegisterAsync(typeof(Message2), handler2)).ReturnsAsync(registration2);
            broker.Setup(b => b.RegisterAsync(typeof(Message3), handler3)).ReturnsAsync(registration3);

            var set = new RegistrationSet(broker.Object)
                .Add(typeof(Message1), handler1)
                .Add(typeof(Message2), handler2)
                .Add(typeof(Message3), handler3);

            await set.RegisterAsync();
            await set.UnregisterAsync();

            Assert.True(registration1.Disposed);
            Assert.True(registration2.Disposed);
            Assert.True(registration3.Disposed);
        }
    }
}
