using System;

namespace RoyalMessenger.Logging
{
    /// <summary>
    /// The class responsible for doing all logging for the Royal Messenger system.
    /// </summary>
    public class Logger
    {
        /// <summary>The binding to where all logs are sent.</summary>
        public static ILoggerBinding Binding { get; set; } = new NullLoggerBinding();

        /// <summary>The type that owns this logger.</summary>
        private Type Type { get; }

        /// <summary>Logs a new message with a level of <see cref="LogLevel.Trace"/>.</summary>
        /// <param name="message">The message to log.</param>
        internal void Trace(FormattableString message) => Binding.Log(Type, LogLevel.Trace, message);
        /// <summary>Logs a new message with a level of <see cref="LogLevel.Debug"/>.</summary>
        /// <param name="message">The message to log.</param>
        internal void Debug(FormattableString message) => Binding.Log(Type, LogLevel.Debug, message);
        /// <summary>Logs a new message with a level of <see cref="LogLevel.Info"/>.</summary>
        /// <param name="message">The message to log.</param>
        internal void Info(FormattableString message) => Binding.Log(Type, LogLevel.Info, message);
        /// <summary>Logs a new message with a level of <see cref="LogLevel.Warn"/>.</summary>
        /// <param name="message">The message to log.</param>
        internal void Warn(FormattableString message) => Binding.Log(Type, LogLevel.Warn, message);
        /// <summary>Logs a new message with a level of <see cref="LogLevel.Error"/>.</summary>
        /// <param name="message">The message to log.</param>
        internal void Error(FormattableString message) => Binding.Log(Type, LogLevel.Error, message);
        /// <summary>
        /// Logs a new message with a level of <see cref="LogLevel.Error"/>, reporting an exception along with
        /// the message.
        /// </summary>
        /// <param name="e">The exception that caused this error.</param>
        /// <param name="message">The message to log.</param>
        internal void Error(Exception e, FormattableString message) => Binding.Log(Type, LogLevel.Error, message, e);

        public Logger(Type type) => Type = type;
    }
}
