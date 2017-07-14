namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ConsumerElement), AddItemName = "add")]
    internal class ConsumersCollection :  ConfigurationElementCollection
    {
        public new ConsumerElement this[string key] => BaseGet(key) as ConsumerElement;

        protected override ConfigurationElement CreateNewElement()
        {
            return new ConsumerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ConsumerElement)element).Topic;
        }
    }
}
