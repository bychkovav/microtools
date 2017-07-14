namespace Platform.Utils.ServiceBus
{
    using System;
    using EasyNetQ;
    using EasyNetQ.Consumer;
    using EasyNetQ.Topology;

    /// <summary>
    /// Implementation of IEndpointInfo
    /// </summary>
    public class EndpointInfo : IEndpointInfo
    {
        /// <summary>
        /// Current bus instance
        /// </summary>
        public IAdvancedBus Bus { get; set; }

        /// <summary>
        /// Current exchange instance 
        /// </summary>
        public IExchange Exchange { get; set; }

        /// <summary>
        /// Message properties
        /// </summary>
        public MessageProperties Properties { get; set; }

        /// <summary>
        /// Action to set qos
        /// </summary>
        public Action<IConsumerConfiguration> SetQoS { get; set; }


        /// <summary>
        /// Recievet/Consumer name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Routing key of current queue binding
        /// </summary>
        public string RoutingKey { get; set; }

        /// <summary>
        /// Timeout value
        /// </summary>
        public int Expires { get; set; }
    }
}
