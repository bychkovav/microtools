namespace Platform.Utils.ServiceBus.Core.Retry
{
    public interface IConsumeRetryStrategy
    {
        /// <summary>
        /// Retry method
        /// </summary>
        void RetryConsumerMessage();
    }
}
