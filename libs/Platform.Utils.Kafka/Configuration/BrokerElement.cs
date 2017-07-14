namespace Platform.Utils.Kafka.Configuration
{
    using System.Configuration;

    internal class BrokerElement : ConfigurationElement
    {
        private const string UrlPropertyName = "url";

        /// <summary>
        /// Gets the name.
        /// </summary>
        [ConfigurationProperty(UrlPropertyName, IsRequired = true)]
        public string Url => (string)base[UrlPropertyName];
    }
}
