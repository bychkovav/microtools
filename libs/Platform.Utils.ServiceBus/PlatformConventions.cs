namespace Platform.Utils.ServiceBus
{
    using System.Configuration;
    using EasyNetQ;

    public class PlatformConventions : Conventions
    {
        public PlatformConventions(ITypeNameSerializer typeNameSerializer)
            : base(typeNameSerializer)
        {
            var identity = ConfigurationManager.AppSettings["Identity"];
            if (!string.IsNullOrEmpty(identity))
            {
                ConsumerTagConvention = () => identity;
            }

            this.ErrorQueueNamingConvention = () => $"{identity}_ErrorQueue";

            this.ErrorExchangeNamingConvention = info => $"{identity}_ErrorExchange";
        }
    }
}
