namespace Allegro.Extensions.RateLimiting
{
    /// <summary>
    /// Allows to execute operations with rate limiting and change the rate dynamically.
    /// </summary>
    public interface IRateLimiterWithVariableRate : IRateLimiter
    {
        /// <summary>
        /// Gets the calculated average rate metric value.
        /// </summary>
        double AvgRate { get; }

        /// <summary>
        /// Gets the max rate of the rate limiter.
        /// </summary>
        double MaxRate { get; }

        /// <summary>
        /// Gets the interval over which the rate is limited.
        /// </summary>
        TimeSpan RateInterval { get; }

        /// <summary>
        /// Changes the max rate.
        /// </summary>
        /// <param name="maxRate">New max rate to be set</param>
        void ChangeMaxRate(double maxRate);
    }
}