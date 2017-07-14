namespace Platform.Utils.Grpc.Configuration
{
    using System.Configuration;

    internal class ServiceSection : ConfigurationSection
    {
        private const string ServiceHostPropertyName = "serviceHost";
        private const string ClientsCollectionName = "clients";

        protected ServiceSection()
        {
        }

        /// <summary>
        /// Gets the current ServiceSection.
        /// </summary>
        public static ServiceSection Current => (ServiceSection)ConfigurationManager.GetSection("grpcService");


        /// <summary>
        /// Gets the endpoints.
        /// </summary>
        [ConfigurationProperty(ClientsCollectionName, IsDefaultCollection = true)]
        internal ServiceClientCollection Clients => (ServiceClientCollection)(base[ClientsCollectionName]);

        [ConfigurationProperty(ServiceHostPropertyName)]
        public ServiceHostElement ServiceHost
        {
            get
            {
                return (ServiceHostElement)this[ServiceHostPropertyName];
            }
            set
            {
                this[ServiceHostPropertyName] = value;
            }
        }
    }
}
