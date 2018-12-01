using Moq;
using RoyalMessenger.ExceptionHandlers;
using RoyalMessenger.Executors;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RoyalMessenger.Tests.Executors
{
    public class SequentialExecutorTests
    {
        private class MyBeautifulException : Exception { }

        private readonly MockRepository Mocks = TestHelper.CreateRepository();

        [Fact]
        public void Execute_FireAndForget_ReturnsImmediately()
        {
            var blockingTask = new TaskCompletionSource<bool>();
            Task BlockingHandler(object message) => blockingTask.Task;

            var executor = new SequentialExecutor(isFireAndForget: true);

            var resultTask = executor.ExecuteAsync(new object(), new MessageHandler[] { BlockingHandler });

            Assert.True(resultTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task Execute_NotFireAndForget_WaitsForCompletion()
        {
            var blockingTask = new TaskCompletionSource<bool>();
            Task BlockingHandler(object message) => blockingTask.Task;

            var executor = new SequentialExecutor(isFireAndForget: false);

            var resultTask = executor.ExecuteAsync(new object(), new MessageHandler[] { BlockingHandler });

            Assert.False(resultTask.IsCompleted);

            blockingTask.SetResult(true);

            await resultTask;
            Assert.True(resultTask.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task Execute_OnException_CallsExceptionHandler()
        {
            var exceptionHandler = Mocks.Create<IExceptionHandler>();
            exceptionHandler
                .Setup(h => h.HandleAsync(It.IsAny<Exception>(), It.IsAny<object>(), It.IsAny<MessageHandler>()))
                .Returns(Task.CompletedTask);

            var exception = new MyBeautifulException();
            var message = 45;

            MessageHandler handler = _ => throw exception;

            var executor = new SequentialExecutor(exceptionHandler: exceptionHandler.Object);
            await executor.ExecuteAsync(message, new MessageHandler[] { handler });

            exceptionHandler.Verify(h => h.HandleAsync(exception, message, handler), Times.Once());
        }

        [Fact]
        public async Task Execute_StopPolicy_OnException_StopsExecution()
        {
            Task Handler1(object _) => throw new MyBeautifulException();

            var called2 = 0;
            async Task Handler2(object _) => called2++;

            var executor = new SequentialExecutor(exceptionPolicy: SequentialExceptionPolicy.Stop, exceptionHandler: Mocks.Create<IExceptionHandler>().Object);
            await executor.ExecuteAsync(new object(), new MessageHandler[] { Handler1, Handler2 });

            Assert.Equal(0, called2);
        }

        [Fact]
        public async Task Execute_ContinuePolicy_OnException_ContinuesExecution()
        {
            Task Handler1(object _) => throw new MyBeautifulException();

            var called2 = 0;
            async Task Handler2(object _) => called2++;

            var executor = new SequentialExecutor(exceptionPolicy: SequentialExceptionPolicy.Continue, exceptionHandler: Mocks.Create<IExceptionHandler>().Object);
            await executor.ExecuteAsync(new object(), new MessageHandler[] { Handler1, Handler2 });

            Assert.Equal(1, called2);
        }

        [Fact]
        public async Task Execute_DoesntOverlapHandlers()
        {
            var called1 = 0;
            var blocker1 = new TaskCompletionSource<bool>();
            Task Handler1(object _) { called1++; return blocker1.Task; }

            var called2 = 0;
            var blocker2 = new TaskCompletionSource<bool>();
            Task Handler2(object _) { called2++; return blocker2.Task; }

            var called3 = 0;
            var blocker3 = new TaskCompletionSource<bool>();
            Task Handler3(object _) { called3++; return blocker3.Task; }

            var executor = new SequentialExecutor();
            var task = executor.ExecuteAsync(new object(), new MessageHandler[] { Handler1, Handler2, Handler3 });

            Assert.Equal(1, called1);
            Assert.Equal(0, called2);
            Assert.Equal(0, called3);
            Assert.False(task.IsCompleted);

            blocker1.SetResult(true);
            await Task.Delay(10);

            Assert.Equal(1, called1);
            Assert.Equal(1, called2);
            Assert.Equal(0, called3);
            Assert.False(task.IsCompleted);

            blocker2.SetResult(true);
            await Task.Delay(10);

            Assert.Equal(1, called1);
            Assert.Equal(1, called2);
            Assert.Equal(1, called3);
            Assert.False(task.IsCompleted);

            blocker3.SetResult(true);
            await Task.Delay(10);

            Assert.Equal(1, called1);
            Assert.Equal(1, called2);
            Assert.Equal(1, called3);
            Assert.True(task.IsCompletedSuccessfully);
        }
    }
}
