namespace Platform.Utils.ServiceBus
{
    using System;
    using EasyNetQ;
    using EasyNetQ.Topology;

    internal class EndpointInfoFactory
    {
        public static IEndpointInfo Create(IAdvancedBus bus, IExchange exchange, string name, string key, bool? persist = null, TimeSpan? timeout = null, ushort? prefetchCount = null)
        {
            var conf = new EndpointInfo() { Bus = bus, Exchange = exchange, RoutingKey = key, Name = name, Properties = new MessageProperties() { ContentType = "application/json" } };
            if (prefetchCount.HasValue)
            {
                conf.SetQoS = configuration => configuration.WithPrefetchCount(prefetchCount.Value);
            }
            else
            {
                conf.SetQoS = configuration => { };
            }
            if (persist.HasValue)
            {
                conf.Properties.DeliveryMode = 2;
            }
            if (timeout.HasValue)
            {
                conf.Expires = (int)timeout.Value.TotalMilliseconds;
            }

            return conf;
        }
    }
}
