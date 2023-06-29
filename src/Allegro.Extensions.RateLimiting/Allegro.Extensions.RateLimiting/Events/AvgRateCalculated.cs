namespace Allegro.Extensions.RateLimiting.Events
{
    /// <summary>
    /// Event handler delegate for the event of periodic average rate of rate limiter being calculated.
    /// </summary>
#pragma warning disable CA1711
    public delegate void AvgRateCalculatedEventHandler(object sender, AvgRateCalculatedEventArgs e);
#pragma warning restore CA1711

    /// <summary>
    /// Event arguments for the event of periodic average rate of rate limiter being calculated.
    /// </summary>
    public class AvgRateCalculatedEventArgs : EventArgs
    {
        internal AvgRateCalculatedEventArgs(
            double avgRate,
            TimeSpan period)
        {
            AvgRate = avgRate;
            Period = period;
        }

        /// <summary>
        /// Gets the average rate calculated over given Period.
        /// </summary>
        public double AvgRate { get; }

        /// <summary>
        /// Gets the period during which the average rate was calculated.
        /// </summary>
        public TimeSpan Period { get; }
    }
}