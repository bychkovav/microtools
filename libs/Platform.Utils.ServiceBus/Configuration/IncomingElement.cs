namespace Platform.Utils.ServiceBus.Configuration
{
    using System.Configuration;

    internal class IncomingElement : MessageElement
    {
        [ConfigurationProperty("react", IsRequired = true)]
        public string React
        {
            get { return (string)base["react"]; }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        [ConfigurationProperty("type")]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
        }
    }
}
