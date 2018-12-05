using Moq;
using RoyalMessenger.Logging;
using System;
using Xunit;

namespace RoyalMessenger.Tests.Logging
{
    /*
     * The Logger binding is static to allow for ease of use and configuration.
     * 
     * Having to send some kind of ILogger<MyClass> to every call of new MyClass()
     * gets annoying fast, and so does having to remember to specify it every time
     * a new Messenger is created.
     *
     * However, static code is hard to test.
     * Other unit tests may be running in parallel, which means that these unit tests
     * both affect and are affected by every other unit test currently running.
     *
     * As such, a lot of care is taken to ensure that there is no interference
     * between unit tests in this particular test suite.
     */

    public class LoggerTests : IDisposable
    {
        private readonly MockRepository Mocks = TestHelper.CreateRepository();

        public LoggerTests()
        {
            /*
             * A new class is instantiated for every unit test, so this constructor runs before every unit test
             * Reset the logger to the default null (no-op) logger
             * Since the Logger is static, an old mocked logger may still be in place when tests begin,
             * which can throw off some intuitive assumptions.
             */
            Logger.Binding = new NullLoggerBinding();
        }

        /// <summary>
        /// The type used to test logging.
        /// Because tests may be run in parallel and the Logger binding is static, other log
        /// requests may be issued to the binding being tested, from unit tests not in this suite (LoggerTests).
        /// Using this class to filter log requests ensures that those requests came from LoggerTests.
        /// </summary>
        private class TestType { }

        private class MyBeautifulException : Exception { }

        [Fact]
        public void NewLogger_ObeysBinding()
        {
            var binding = Mocks.Create<ILoggerBinding>();
            binding.Setup(b => b.Log(typeof(TestType), It.IsAny<LogLevel>(), It.IsAny<FormattableString>(), It.IsAny<Exception>()));

            FormattableString message = $"I am running inside of {GetType().Name}";

            Logger.Binding = binding.Object;
            var logger = new Logger(typeof(TestType));

            logger.Info(message);

            binding.Verify(b => b.Log(typeof(TestType), LogLevel.Info, message, null), Times.Once());
        }

        [Fact]
        public void ExistingLogger_ObeysBindingChange()
        {
            var binding1 = Mocks.Create<ILoggerBinding>();
            binding1.Setup(b => b.Log(typeof(TestType), It.IsAny<LogLevel>(), It.IsAny<FormattableString>(), It.IsAny<Exception>()));

            var binding2 = Mocks.Create<ILoggerBinding>();
            binding2.Setup(b => b.Log(typeof(TestType), It.IsAny<LogLevel>(), It.IsAny<FormattableString>(), It.IsAny<Exception>()));

            FormattableString message1 = $"I should go nowhere, here's a number: {25.4442m}";
            FormattableString message2 = $"I should go to binding1, here's a number: {1337.88342f}";
            FormattableString message3 = $"I should go to binding2, here's a number: {42}";
            FormattableString message4 = $"I should go nowhere, here's a number: {0x3F}";
            var logger = new Logger(typeof(TestType));

            logger.Info(message1);

            Logger.Binding = binding1.Object;
            logger.Info(message2);

            Logger.Binding = binding2.Object;
            logger.Info(message3);

            Logger.Binding = new NullLoggerBinding();
            logger.Info(message4);


            binding1.Verify(b => b.Log(typeof(TestType), LogLevel.Info, message2, null), Times.Once());
            binding2.Verify(b => b.Log(typeof(TestType), LogLevel.Info, message3, null), Times.Once());
        }

        [Fact]
        public void KeepsException()
        {
            var binding = Mocks.Create<ILoggerBinding>();
            binding.Setup(b => b.Log(typeof(TestType), It.IsAny<LogLevel>(), It.IsAny<FormattableString>(), It.IsAny<Exception>()));

            var exception = new MyBeautifulException();
            FormattableString message = $"Oh no, something went wrong!";

            Logger.Binding = binding.Object;
            var logger = new Logger(typeof(TestType));

            logger.Error(exception, message);

            binding.Verify(b => b.Log(typeof(TestType), LogLevel.Error, message, exception), Times.Once());
        }

        public void Dispose()
        {
            // Restore the original null logger to avoid leaking a possible mock
            Logger.Binding = new NullLoggerBinding();
        }
    }
}
