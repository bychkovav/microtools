namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(BrokerElement), AddItemName = "add")]
    internal class BrokersCollection : ConfigurationElementCollection
    {
        public new BrokerElement this[string key] => BaseGet(key) as BrokerElement;

        protected override ConfigurationElement CreateNewElement()
        {
            return new BrokerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((BrokerElement)element).Url;
        }
    }
}
