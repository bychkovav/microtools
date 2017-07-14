namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    internal class KafkaConfigurationSection : ConfigurationSection
    {
        private const string ProducersPropertyName = "producers";
        private const string ClustersPropertyName = "clusters";
        private const string ConsumersPropertyName = "consumers";

        protected KafkaConfigurationSection()
        {

        }

        /// <summary>
        /// Gets the current kafka config section.
        /// </summary>
        public static KafkaConfigurationSection Current => (KafkaConfigurationSection)ConfigurationManager.GetSection("kafka");

        /// <summary>
        /// Gets a list of consumers
        /// </summary>
        [ConfigurationProperty(ClustersPropertyName, IsDefaultCollection = true)]
        internal ClustersCollection Clusters => (ClustersCollection)(base[ClustersPropertyName]);

        /// <summary>
        /// Gets a list of consumers
        /// </summary>
        [ConfigurationProperty(ConsumersPropertyName, IsDefaultCollection = true)]
        internal ConsumersCollection Consumers => (ConsumersCollection)(base[ConsumersPropertyName]);

        /// <summary>
        /// Gets a list of producers
        /// </summary>
        [ConfigurationProperty(ProducersPropertyName, IsDefaultCollection = true)]
        internal ProducersCollection Producer => (ProducersCollection)(base[ProducersPropertyName]);

    }
}
