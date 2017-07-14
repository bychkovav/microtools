namespace Platform.Utils.Grpc.Configuration
{
    using System.Configuration;

    internal class ServiceHostElement : ConfigurationElement
    {
        private const string HostPropertyName = "host";
        private const int DefaultPortValue = 51052;
        private const string PortPropertyName = "port";

        [ConfigurationProperty(HostPropertyName, IsRequired = true)]
        public string Host
        {
            get
            {
                return (string)this[HostPropertyName];
            }

            set
            {
                this[HostPropertyName] = value;
            }
        }

        [ConfigurationProperty(PortPropertyName, IsRequired = false, DefaultValue = DefaultPortValue)]
        public int Port
        {
            get
            {
                var propVal = (int?)this[PortPropertyName];
                return propVal ?? DefaultPortValue;
            }

            set
            {
                this[PortPropertyName] = value;
            }
        }
    }
}
