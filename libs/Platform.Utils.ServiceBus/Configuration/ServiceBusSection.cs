namespace Platform.Utils.ServiceBus.Configuration
{
    using System.Configuration;

    /// <summary>
    /// The SB section.
    /// </summary>
    public class ServiceBusSection : ConfigurationSection
    {
        private const int DefaultRetryThreshold = 2;

        protected ServiceBusSection()
        {
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        public static ServiceBusSection Current => (ServiceBusSection)ConfigurationManager.GetSection("serviceBus");

        /// <summary>
        /// Gets the endpoints.
        /// </summary>
        [ConfigurationProperty("exchanges", IsDefaultCollection = true)]
        internal ExchangeCollection Exchanges => (ExchangeCollection)(base["exchanges"]);

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get
            {
                return (string)this["connectionString"];
            }

            set
            {
                this["connectionString"] = value;
            }
        }

        [ConfigurationProperty("enableSendRetry", IsRequired = false, DefaultValue = false)]
        public bool SendRetryEnabled
        {
            get
            {
                var propVal = (bool?)this["enableSendRetry"];
                return propVal ?? false;
            }

            set
            {
                this["enableSendRetry"] = value;
            }
        }

        [ConfigurationProperty("maxSendRetries", IsRequired = false, DefaultValue = DefaultRetryThreshold)]
        public int MaxSendRetries
        {
            get
            {
                var propVal = (int?)this["maxSendRetries"];
                return propVal ?? DefaultRetryThreshold;
            }

            set
            {
                this["maxSendRetries"] = value;
            }
        }

        [ConfigurationProperty("enableConsumeRetry", IsRequired = false, DefaultValue = false)]
        public bool ConsumerRetriesEnabled
        {
            get
            {
                var propVal = (bool?)this["enableConsumeRetry"];
                return propVal ?? false;
            }

            set
            {
                this["enableConsumeRetry"] = value;
            }
        }

        [ConfigurationProperty("maxConsumeRetry", IsRequired = false, DefaultValue = DefaultRetryThreshold)]
        public int MaxConsumeRetry
        {
            get
            {
                var propVal = (int?)this["maxConsumeRetry"];
                return propVal ?? DefaultRetryThreshold;
            }

            set
            {
                this["maxConsumeRetry"] = value;
            }
        }

    }
}
