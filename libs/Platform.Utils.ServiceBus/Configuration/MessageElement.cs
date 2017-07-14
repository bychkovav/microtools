namespace Platform.Utils.ServiceBus.Configuration
{
    using System.Configuration;

    /// <summary>
    /// The message element.
    /// </summary>
    internal class MessageElement : ConfigurationElement
    {
        #region Public Properties

        /// <summary>
        /// Gets the key.
        /// </summary>
        [ConfigurationProperty("route-key", IsKey = true, IsRequired = true)]
        public string Key
        {
            get
            {
                return (string)base["route-key"];
            }
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }


        #endregion
    }
}
