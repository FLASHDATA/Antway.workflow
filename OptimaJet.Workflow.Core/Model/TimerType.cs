namespace OptimaJet.Workflow.Core.Model
{
    /// <summary>
    /// Type of a timer
    /// </summary>
    public enum TimerType
    {
        /// <summary>
        /// Timer time is set by interval in milliseconds
        /// </summary>
        Interval,

        /// <summary>
        /// Timer time is set to specific time of today or tomorrow
        /// </summary>
        Time,

        /// <summary>
        /// Timer time is set to specific date at 00:00:00
        /// </summary>
        Date,

        /// <summary>
        /// Timer time is set to specific date and time
        /// </summary>
        DateAndTime
    }
}
