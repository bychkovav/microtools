namespace Platform.Utils.ServiceBus
{
    using System;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using EasyNetQ.Topology;

    /// <summary>
    /// Interface to store sender/consumer configuration
    /// </summary>
    public interface IEndpointInfo
    {
        /// <summary>
        /// Current bus instance
        /// </summary>
        IAdvancedBus Bus { get; set; }

        /// <summary>
        /// Current exchange instance 
        /// </summary>
        IExchange Exchange { get; set; }

        /// <summary>
        /// Message properties
        /// </summary>
        MessageProperties Properties { get; set; }

        /// <summary>
        /// Action to set qos
        /// </summary>
        Action<IConsumerConfiguration> SetQoS { get; set; }

        /// <summary>
        /// Recievet/Consumer name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Routing key of current queue binding
        /// </summary>
        string RoutingKey { get; set; }

        /// <summary>
        /// Timeout value
        /// </summary>
        int Expires { get; set; }
    }
}
