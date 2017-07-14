namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    internal class ProducerElement : ConfigurationElement
    {
        private const string ClusterPropertyName = "cluster";
        private const string BatchDelayPropertyName = "batchDelayTimeMs";

        [ConfigurationProperty(BatchDelayPropertyName, IsRequired = true)]
        public int BatchDelayTimeMs => (int)this[BatchDelayPropertyName];

        /// <summary>
        /// Gets a list of clusters
        /// </summary>
        [ConfigurationProperty(ClusterPropertyName, IsKey = true, IsRequired = true)]
        internal string Cluster => (string)(base[ClusterPropertyName]);
    }
}
