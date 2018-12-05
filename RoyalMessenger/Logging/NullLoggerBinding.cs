using System;

namespace RoyalMessenger.Logging
{
    /// <summary>The default implementation of <see cref="ILoggerBinding"/>, which simply discards all logs.</summary>
    internal class NullLoggerBinding : ILoggerBinding
    {
        public void Log(Type type, LogLevel level, FormattableString message, Exception exception = null) { }
    }
}
