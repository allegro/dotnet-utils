namespace Allegro.Extensions.RateLimiting.Events
{
    /// <summary>
    /// Event handler delegate for the event of max rate of rate limiter being exceeded.
    /// </summary>
#pragma warning disable CA1711
    public delegate void MaxRateExceededEventHandler(object sender, MaxRateExceededEventArgs e);
#pragma warning restore CA1711

    /// <summary>
    /// Event arguments for the event of max rate of rate limiter being exceeded.
    /// </summary>
    public class MaxRateExceededEventArgs : EventArgs
    {
        internal MaxRateExceededEventArgs(
            double maxRate,
            double accumulatedOps,
            double attemptedOpWeight,
            double delayMilliseconds)
        {
            MaxRate = maxRate;
            AccumulatedOps = accumulatedOps;
            AttemptedOpWeight = attemptedOpWeight;
            DelayMilliseconds = delayMilliseconds;
        }

        /// <summary>
        /// Gets the maximum rate of the rate limiter.
        /// </summary>
        public double MaxRate { get; }

        /// <summary>
        /// Gets the total weight of operations executed so far.
        /// </summary>
        public double AccumulatedOps { get; }

        /// <summary>
        /// Gets the weight of the attempted operation, which exceeded the max rate.
        /// </summary>
        public double AttemptedOpWeight { get; }

        /// <summary>
        /// Gets the delay in milliseconds that the rate limiter will wait before executing next operations.
        /// </summary>
        public double DelayMilliseconds { get; }
    }
}