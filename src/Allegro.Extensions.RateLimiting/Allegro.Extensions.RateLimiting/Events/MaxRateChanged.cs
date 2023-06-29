namespace Allegro.Extensions.RateLimiting.Events
{
    /// <summary>
    /// Event handler delegate for the event of max rate of rate limiter being changed.
    /// </summary>
#pragma warning disable CA1711
    public delegate void MaxRateChangedEventHandler(object sender, MaxRateChangedEventArgs e);
#pragma warning restore CA1711

    /// <summary>
    /// Event arguments for the event of max rate of rate limiter being changed.
    /// </summary>
    public class MaxRateChangedEventArgs : EventArgs
    {
        internal MaxRateChangedEventArgs(
            double newMaxRate,
            double previousMaxRate)
        {
            NewMaxRate = newMaxRate;
            PreviousMaxRate = previousMaxRate;
        }

        /// <summary>
        /// Gets the new max rate that has been set.
        /// </summary>
        public double NewMaxRate { get; }

        /// <summary>
        /// Gets the previous max rate of the rate limiter.
        /// </summary>
        public double PreviousMaxRate { get; }
    }
}