namespace RoyalMessenger.Logging
{
    /// <summary>Represents a level of verbosity of logging.</summary>
    public enum LogLevel
    {
        /// <summary>A detailed message that should only be of interest to library developers.</summary>
        Trace,
        /// <summary>Debugging information that can be used by library users to track down application bugs.</summary>
        Debug,
        /// <summary>Generic information that lets library users know about important events in the library.</summary>
        Info,
        /// <summary>An important event that should be looked at by library users.</summary>
        Warn,
        /// <summary>An error that has stopped normal program flow.</summary>
        Error
    }
}
