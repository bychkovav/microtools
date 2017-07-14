namespace Platform.Utils.ServiceBus.Configuration
{
    using System;
    using System.Configuration;

    internal class OutgoingElement : MessageElement
    {
        /// <summary>
        /// Gets a value indicating whether persist.
        /// </summary>
        [ConfigurationProperty("persist")]
        public bool Persist
        {
            get
            {
                return (bool)base["persist"];
            }
        }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        [ConfigurationProperty("timeout", DefaultValue = "00:00:10")]
        public TimeSpan? Timeout
        {
            get
            {
                return (TimeSpan?)base["timeout"];
            }
        }
    }
}
