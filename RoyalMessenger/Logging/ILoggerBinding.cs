using System;

namespace RoyalMessenger.Logging
{
    /// <summary>
    /// Represents the final destination of all log messages from the Royal Messenger library,
    /// used to connect the library to an external logging system.
    /// </summary>
    public interface ILoggerBinding
    {
        /// <summary>Writes a log message.</summary>
        /// <param name="type">The type from where the log originates.</param>
        /// <param name="level">The level of this message.</param>
        /// <param name="message">The message itself, unformatted.</param>
        /// <param name="exception">A possible exception accompanying the message. Can be null.</param>
        void Log(Type type, LogLevel level, FormattableString message, Exception exception = null);
    }
}
