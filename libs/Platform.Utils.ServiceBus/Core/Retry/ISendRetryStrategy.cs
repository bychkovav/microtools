namespace Platform.Utils.ServiceBus.Core.Retry
{
    /// <summary>
    /// An interface for reccuring send retries
    /// </summary>
    public interface ISendRetryStrategy
    {
        /// <summary>
        /// Retry method
        /// </summary>
        void RetryMessagePublish();
    }
}
