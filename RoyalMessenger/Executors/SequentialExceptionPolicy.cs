namespace RoyalMessenger.Executors
{
    /// <summary>Defines how a sequential executor will handle exceptions</summary>
    public enum SequentialExceptionPolicy
    {
        /// <summary>Stop executing message handlers at the first exception, even if the exception handler completed successfuly.</summary>
        Stop,

        /// <summary>Continue executing message handlers even if one of them throws an exception.</summary>
        Continue,
    }
}
