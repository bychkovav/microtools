namespace Platform.Utils.Grpc.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ServiceClientElement), AddItemName = "add")]
    internal class ServiceClientCollection : ConfigurationElementCollection
    {

        public new ServiceClientElement this[string key] => BaseGet(key) as ServiceClientElement;

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceClientElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServiceClientElement)element).Name;
        }
    }
}
