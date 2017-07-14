namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    internal class ClusterElement : ConfigurationElement
    {
        private const string NamePropertyName = "name";
        private const string BrokersPropertyName = "brokers";

        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty(NamePropertyName, IsRequired = true)]
        public string Name => (string)base[NamePropertyName];

        /// <summary>
        /// Gets a list of servers
        /// </summary>
        [ConfigurationProperty(BrokersPropertyName, IsDefaultCollection = true)]
        internal BrokersCollection Brokers => (BrokersCollection)(base[BrokersPropertyName]);
    }
}
