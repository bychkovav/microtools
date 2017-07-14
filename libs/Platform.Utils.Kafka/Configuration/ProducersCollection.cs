namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ConsumerElement), AddItemName = "add")]
    internal class ProducersCollection : ConfigurationElementCollection
    {
        public new ProducerElement this[string key] => BaseGet(key) as ProducerElement;

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProducerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProducerElement)element).Cluster;
        }
    }
}
