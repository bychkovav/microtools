namespace Platform.Utils.ServiceBus.Receiving
{
    using System;
    using System.Threading.Tasks;
    using EasyNetQ;

    /// <summary>
    /// Consume additional information.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConsumerContext<T> where T : class
    {
        /// <summary>
        /// Configuration
        /// </summary>
        IEndpointInfo EndpointInfo { get; }

        /// <summary>
        /// Message instance
        /// </summary>
        IMessage<T> Message { get; }

        Type ConsumerType { get; set; }

        /// <summary>
        /// Reply fir current consumed message
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        Task Reply<TResponse>(TResponse result)
            where TResponse : class;
    }
}
