namespace Platform.Utils.Owin.Configuration
{
    using System.Configuration;

    public class ServiceContainerSection : ConfigurationSection
    {
        protected ServiceContainerSection()
        {
        }

        /// <summary>
        /// Gets the current.
        /// </summary>
        public static ServiceContainerSection Current
        {
            get
            {
                return (ServiceContainerSection)ConfigurationManager.GetSection("serviceContainer");
            }
        }

        [ConfigurationProperty("serviceName", IsRequired = true)]
        public string ServiceName
        {
            get
            {
                return (string)this["serviceName"];
            }
        }

        [ConfigurationProperty("description", IsRequired = true)]
        public string Description
        {
            get
            {
                return (string)this["description"];
            }
        }

        [ConfigurationProperty("displayName", IsRequired = false)]
        public string DisplayName
        {
            get
            {
                return (string)this["displayName"];
            }
        }

        [ConfigurationProperty("instanceName", IsRequired = false)]
        public string InstanceName
        {
            get
            {
                return (string)this["instanceName"];
            }
        }
    }
}
