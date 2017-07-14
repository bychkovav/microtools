namespace Platform.Utils.ServiceBus.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ExchangeElement), AddItemName = "exchange")]
    internal class ExchangeCollection : ConfigurationElementCollection
    {

        public new ExchangeElement this[string key]
        {
            get
            {
                return BaseGet(key) as ExchangeElement;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ExchangeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExchangeElement)element).Name;
        }
    }
}
