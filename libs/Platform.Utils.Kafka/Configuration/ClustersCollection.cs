namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ClusterElement), AddItemName = "add")]
    internal class ClustersCollection : ConfigurationElementCollection
    {
        public new ClusterElement this[string key] => BaseGet(key) as ClusterElement;

        protected override ConfigurationElement CreateNewElement()
        {
            return new ClusterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClusterElement)element).Name;
        }
    }
}
