namespace Platform.Utils.ServiceBus.Objects
{
    using System.Collections.Generic;
    using EasyNetQ.Topology;

    public class ExchangeItem
    {
        public ushort PrefetchCount { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IList<InMessage> In { get; set; }
        public IList<OutMessage> Out { get; set; }
        public IExchange RabbitExchange { get; set; }
    }
}
