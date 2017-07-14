namespace Platform.Utils.ServiceBus.Configuration
{
    using System.Configuration;

    internal class ExchangeElement : ConfigurationElement
    {
        [ConfigurationProperty("prefetchCount", IsRequired = false)]
        public ushort PrefetchCount
        {
            get
            {
                return (ushort)(base["prefetchCount"]);
            }
        }


        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty("type", DefaultValue = "topic", IsKey = true, IsRequired = false)]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
        }

        /// <summary>
        /// Gets the incoming.
        /// </summary>
        [ConfigurationProperty("in")]
        public IncomingCollection In
        {
            get
            {
                return (IncomingCollection)base["in"];
            }
        }

        /// <summary>
        /// Gets the outgoing.
        /// </summary>
        [ConfigurationProperty("out")]
        public OutgoingCollection Out
        {
            get
            {
                return (OutgoingCollection)base["out"];
            }
        }
    }
}
