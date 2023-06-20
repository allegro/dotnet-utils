namespace Allegro.Extensions.RateLimiting
{
    /// <summary>
    /// Allows to execute operations with rate limiting
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>
        /// Executes operation with rate limiting.
        /// This method is equivalent of ExecuteWeighted with weight = 1.
        /// </summary>
        Task<T> ExecuteAsync<T>(Func<Task<T>> operation) => ExecuteWeightedAsync(operation, 1);

        /// <summary>
        /// Executes operation with rate limiting.
        /// This method is equivalent of ExecuteWeighted with weight = 1.
        /// </summary>
        Task ExecuteAsync(Func<Task> operation) => ExecuteWeightedAsync(operation, 1);

        /// <summary>
        /// Executes operation with rate limiting, given the operation weight.
        /// Consumes "weight" from the defined limit.
        /// </summary>
        Task<T> ExecuteWeightedAsync<T>(Func<Task<T>> operation, double weight);

        /// <summary>
        /// Executes operation with rate limiting, given the operation weight.
        /// Consumes "weight" from the defined limit.
        /// </summary>
        Task ExecuteWeightedAsync(Func<Task> operation, double weight);

        /// <summary>
        /// Executes operation with rate limiting, estimating its weight based on previous executions of similar operation.
        /// </summary>
        /// <param name="operationName">Name of the operation (it will be used to estimate weight based on previous executions)</param>
        /// <param name="operation">Operation to execute</param>
        /// <param name="weightCalculator">Callback function to calculate (post-factum) the weight of the executed operation.</param>
        Task<T> ExecuteWithEstimatedWeightAsync<T>(
            string operationName,
            Func<Task<T>> operation,
            Func<T, double?> weightCalculator);
    }
}