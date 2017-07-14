namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    internal class ConsumerElement : ConfigurationElement
    {
        private const string ClusterPropertyName = "cluster";
        private const string TopicPropertyName = "topic";

        /// <summary>
        /// Gets the topic of consumer
        /// </summary>
        [ConfigurationProperty(TopicPropertyName, IsKey = true, IsRequired = true)]
        public string Topic => (string)base[TopicPropertyName];

        /// <summary>
        /// Gets a list of clusters
        /// </summary>
        [ConfigurationProperty(ClusterPropertyName, IsDefaultCollection = true)]
        internal string Cluster => (string)(base[ClusterPropertyName]);
    }
}
